// Copyright (C) 2019 Topsoft (https://topsoft.by)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;
using T2.CLS.LoggerExtensions.Core.Extensions;

namespace T2.CLS.LoggerExtensions.Core.Buffer
{
	public abstract partial class GenericFileBuffer<TItem>
	{
		#region Nested Types

		private sealed class FileBufferWorker
		{
			#region Ctors

			public FileBufferWorker(GenericFileBuffer<TItem> buffer,
				ILogger logger,
				string path,
				int workerIndex,
				Action<IReadOnlyList<string>> handleRead,
				int readLimit,
				int? memoryBufferLimit = null,
				int? fileBufferLimit = null,
				double? flushTimeout = null,
				double? resentTimeout = null,
				double[] resendIntervals = null)
			{
				_buffer = buffer;
				_path = path;
				_workerIndex = workerIndex;
				_handleRead = handleRead;
				_readLimit = readLimit;
				_logger = logger;
				_memoryBufferLimit = Math.Max(MinMemoryBufferLimit, memoryBufferLimit ?? DefaultMemoryBufferLimit);
				_fileBufferLimit = Math.Max(MinFileBufferLimit, fileBufferLimit ?? DefaultFileBufferLimit);

                _logger.LEC00004_Trace_FileBufferWorker_workerIndex_workerIndex(workerIndex);

				if (resendIntervals != null)
					_resendIntervals = resendIntervals.Select(TimeSpan.FromMilliseconds).Prepend(TimeSpan.Zero).ToArray();
				else
					_resendIntervals = Enumerable.Range(0, 13).Select(i => TimeSpan.FromSeconds(15 * Math.Pow(2, i)))
						.Prepend(TimeSpan.Zero).ToArray();

				if (Directory.Exists(path) == false)
					Directory.CreateDirectory(path);

				_errorMarkLock = new FileLock(Path.Combine(path, $"error.buffer.{_workerIndex}.mark"));
				_markLock = new FileLock(Path.Combine(path, $"buffer.{_workerIndex}.mark"));

				var flushInterval = Math.Max(MinFlushTimeout, flushTimeout ?? DefaultFlushTimeout);

				_flushTimeout = TimeSpan.FromMilliseconds(flushInterval);
				_flushTimer = new Timer {Interval = flushInterval};
				_flushTimer.Elapsed += OnFlushTimer;
				_flushTimer.Start();

				_handleErrorBufferTimer = new Timer {Interval = resentTimeout ?? DefaultResendTimeout};
				_handleErrorBufferTimer.Elapsed += OnHandleErrorBufferTimer;
				_handleErrorBufferTimer.Start();
			}

			#endregion

			public void GetInfo(out long written, out long read)
			{
				written = 0;
				read = 0;

				try
				{
					_markLock.Lock();

					var markInfo = ReadMark(_markLock);

					written = markInfo.Written;
					read = markInfo.Read;
				}
				finally
				{
					_markLock.Unlock();
				}
			}

			#region Static Fields and Constants

			private const int OpenReadStreamTimeout = 50;
			private const int DefaultMemoryBufferLimit = 64;
			private const int DefaultFileBufferLimit = 1024;
			private const double DefaultFlushTimeout = 5000;
			private const int MinMemoryBufferLimit = 64;
			private const int MinFileBufferLimit = 1024;
			private const int MinFlushTimeout = 1000;
			private const int MainBufferFailedSendLimit = 6;
			private const int DefaultResendTimeout = 60000;

			// ReSharper disable once StaticMemberInGenericType
			private static readonly char[] Separator = {'|'};

			#endregion

			#region Fields

			private readonly GenericFileBuffer<TItem> _buffer;
			private readonly FileLock _errorMarkLock;
			private readonly int _fileBufferLimit;

			// ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
			private readonly Timer _flushTimer;
			private readonly Timer _handleErrorBufferTimer;
			private readonly Action<IReadOnlyList<string>> _handleRead;
			private readonly ILogger _logger;
			private readonly FileLock _markLock;
			private readonly int _memoryBufferLimit;
			private readonly Stack<List<TItem>> _memoryBufferPool = new Stack<List<TItem>>();
			private readonly string _path;
			private readonly int _readLimit;
			private readonly TimeSpan[] _resendIntervals;
			private readonly AutoResetEvent _syncErrorRead = new AutoResetEvent(true);
			private readonly object _syncFlush = new object();
			private readonly AutoResetEvent _syncRead = new AutoResetEvent(true);
			private readonly int _workerIndex;
			private bool _disposed;
			private readonly TimeSpan _flushTimeout;
			private StreamWriter _streamWriter;
			private string _writeBuffer;
			private List<TItem> _memoryBuffer = new List<TItem>();
			private FileLock _writerLock;

			#endregion

			#region Methods

			private void CleanStalledBuffers(FileLock markLock)
			{
				try
				{
					markLock.Lock();

					var oldBuffers = ReadMark(markLock).Buffers;
					var newBuffers = new List<BufferInfo>();

					foreach (var bufferInfo in oldBuffers)
					{
						// TODO Investigate case when read is greater than written
						if (bufferInfo.WriteActive == false && bufferInfo.Read >= bufferInfo.Written)
						{
							var bufferFilePath = GetBufferFilePath(bufferInfo);

							if (_buffer.StreamService.IsStreamExist(bufferFilePath))
							{
								try
								{
									DeleteBuffer(bufferFilePath);
								}
								catch (IOException)
								{
									newBuffers.Add(bufferInfo);
								}
							}
						}
						else
						{
							newBuffers.Add(bufferInfo);
						}
					}

					if (newBuffers.Count != oldBuffers.Count)
						WriteMark(markLock, newBuffers);
				}
				finally
				{
					markLock.Unlock();
				}
			}

			public void Dispose(bool disposing)
			{
				if (_disposed)
					return;

				if (disposing == false)
					return;

				_disposed = true;
				_flushTimer.Dispose();
				_handleErrorBufferTimer.Dispose();

				Flush(true, false);

				if (_syncRead.WaitOne(TimeSpan.FromMilliseconds(200)))
					_syncRead.Set();

				_syncRead.Dispose();
				_streamWriter?.Close();
			}

			private static void WriteStartLine(StreamWriter streamWriter)
			{
				LineHelper.WriteStartLine(streamWriter);
			}

			private static void WriteEndLine(StreamWriter streamWriter)
			{
				LineHelper.WriteEndLine(streamWriter);
			}

			private void Flush(bool force, bool push = true)
			{
				try
				{
					List<TItem> flushBuffer;

					lock (_memoryBuffer)
					{
						flushBuffer = _memoryBuffer;

						var bufferCount = flushBuffer.Count;

						if (bufferCount == 0)
							return;

						if (force == false && bufferCount < _memoryBufferLimit)
							return;

						_memoryBuffer = RentMemoryBuffer();
					}

					lock (_syncFlush)
					{
						try
						{
							if (_streamWriter == null)
								_streamWriter = OpenWriteBufferStream(out _writeBuffer, out _writerLock);

							var count = 0;

							try
							{
								_writerLock.Lock();

								_streamWriter.BaseStream.Seek(0, SeekOrigin.End);

								foreach (var logEvent in flushBuffer)
								{
									WriteStartLine(_streamWriter);

									_buffer.FormatItem(logEvent, _streamWriter);

									WriteEndLine(_streamWriter);

									count++;
								}

								_streamWriter.Flush();
							}
							finally
							{
								_writerLock.Unlock();
							}

							ReleaseMemoryBuffer(flushBuffer);

							if (MarkWrite(_markLock, _writeBuffer, count).WriteActive == false)
							{
								_streamWriter.Close();
								_streamWriter = null;
								_writerLock = null;
							}
						}
						catch (Exception e)
						{
							_logger.LogError(e, "Flush.");
						}
					}
				}
				finally
				{
					if (push)
						PushBlock();
				}
			}

			private string GetBufferFilePath(BufferInfo bufferInfo)
			{
				return GetBufferFilePath(bufferInfo.Name);
			}

			private string GetBufferFilePath(string bufferName)
			{
				return Path.Combine(_path, bufferName);
			}

			private static BufferInfo GetBufferIndex(string bufferName, IEnumerable<BufferInfo> buffers, out int index)
			{
				index = 0;

				foreach (var bufferInfo in buffers)
				{
					if (bufferInfo.Name == bufferName)
						return bufferInfo;

					index++;
				}

				throw new InvalidOperationException();
			}

			private string GetNextBufferName()
			{
				return $"{DateTime.Now.Ticks}.{_workerIndex}.buffer";
			}

			private void HandleFailedSend(FileLock markLock, string bufferName, ref LineReader streamReader,
				FileLock readerLock, List<string> memoryBuffer, int failLimit, Action<List<string>> alternativeWrite)
			{
				try
				{
					markLock.Lock();

					var buffers = ReadMark(markLock).Buffers;
					var buffer = GetBufferIndex(bufferName, buffers, out var index);

					if (buffer.FailedSendCount >= failLimit)
					{
						alternativeWrite(memoryBuffer);
						HandleSuccessSend(markLock, bufferName, ref streamReader, readerLock, memoryBuffer);
					}
					else
					{
						var modifiedBuffer = new BufferInfo(buffer.Name, buffer.Written, buffer.Read, buffer.WriteActive,
							buffer.LastFlushTime, buffer.FailedSendCount + 1, DateTime.Now);

						buffers[index] = modifiedBuffer;

						WriteMark(markLock, buffers);

						streamReader.Close();
						streamReader = null;
					}
				}
				finally
				{
					markLock.Unlock();
				}
			}

			private void DeleteBuffer(string path)
			{
				_buffer.StreamService.Delete(path);
			}

			private void HandleSuccessSend(FileLock markLock, string bufferName, ref LineReader streamReader, FileLock readerLock, List<string> memoryBuffer)
			{
				try
				{
					var buffer = MarkRead(markLock, bufferName, memoryBuffer.Count);

					if (buffer.WriteActive == false && buffer.Read >= buffer.Written)
					{
						streamReader.Close();
						streamReader = null;

						DeleteBuffer(readerLock.Path);
					}
				}
				catch (IOException e)
				{
					_logger.LogError(e, "Mark read failed.");
				}
				finally
				{
					memoryBuffer.Clear();
				}
			}

			private bool HasPauseTimeoutExpired(BufferInfo bufferInfo)
			{
				if (bufferInfo.FailedSendCount >= _resendIntervals.Length)
					return true;

				return DateTime.Now >= bufferInfo.LastSendTime + _resendIntervals[bufferInfo.FailedSendCount];
			}

			private bool IsReadBufferEmpty(FileLock markLock)
			{
				try
				{
					markLock.Lock();

					var mark = ReadMark(markLock);

					return mark.Read >= mark.Written;
				}
				catch (IOException e)
				{
					_logger.LogError(e, "IsReadBufferEmpty");

					return true;
				}
				finally
				{
					markLock.Unlock();
				}
			}

			private bool IsReadBufferEmpty()
			{
				return IsReadBufferEmpty(_markLock);
			}

			private BufferInfo MarkRead(FileLock markLock, string bufferName, int read)
			{
				return ModifyBuffer(markLock, bufferName,
					buffer => new BufferInfo(buffer.Name, buffer.Written, buffer.Read + read, buffer.WriteActive,
						buffer.LastFlushTime, 0, DateTime.Now));
			}

			private BufferInfo MarkWrite(FileLock markLock, string bufferName, int written)
			{
				return ModifyBuffer(markLock, bufferName,
					buffer => new BufferInfo(buffer.Name, buffer.Written + written, buffer.Read,
						buffer.Written + written < _fileBufferLimit, DateTime.Now, buffer.FailedSendCount, buffer.LastSendTime));
			}

			private BufferInfo ModifyBuffer(FileLock markLock, string bufferName, Func<BufferInfo, BufferInfo> mutator)
			{
				try
				{
					markLock.Lock();

					var buffers = ReadMark(markLock).Buffers;
					var buffer = GetBufferIndex(bufferName, buffers, out var index);
					var modifiedBuffer = mutator(buffer);

					buffers[index] = modifiedBuffer;

					WriteMark(markLock, buffers);

					return modifiedBuffer;
				}
				finally
				{
					markLock.Unlock();
				}
			}

			private void OnFlushTimer(object sender, ElapsedEventArgs e)
			{
				Flush(true);
				CleanStalledBuffers(_markLock);
			}

			private void OnHandleErrorBufferTimer(object sender, ElapsedEventArgs e)
			{
				PushErrorBlock();
			}

			private LineReader OpenReadBufferStream(FileLock markLock, out string bufferName, out FileLock streamLock)
			{
				try
				{
					markLock.Lock();

					bufferName = null;

					var buffers = ReadMark(markLock).Buffers;

					foreach (var buffer in buffers)
					{
						if (HasPauseTimeoutExpired(buffer) == false)
							continue;

						if (buffer.WriteActive && buffer.Written - buffer.Read < _readLimit && buffer.LastFlushTime + _flushTimeout > DateTime.Now)
							continue;

						var lineReader = OpenReadBufferStream(buffer, out streamLock);

						if (lineReader == null)
							continue;

						if (lineReader.ReadFinished)
						{
							try
							{
								if (buffer.WriteActive == false)
								{
									lineReader.Close();

									DeleteBuffer(streamLock.Path);
								}
							}
							catch (Exception e)
							{
								_logger.LogError(e, $"Delete buffer '{streamLock.Path}' error.");
							}

							continue;
						}

						bufferName = buffer.Name;

						return lineReader;
					}

					streamLock = null;

					return null;
				}
				finally
				{
					markLock.Unlock();
				}
			}

			private LineReader OpenReadBufferStream(BufferInfo bufferInfo, out FileLock streamLock)
			{
				streamLock = null;

				if (bufferInfo.Read >= bufferInfo.Written)
					return null;

				var bufferPath = GetBufferFilePath(bufferInfo);

				if (_buffer.StreamService.IsStreamExist(bufferPath) == false)
					return null;

				streamLock = new FileLock(bufferPath);

				if (streamLock.Lock(TimeSpan.FromMilliseconds(OpenReadStreamTimeout)) == false)
				{
					streamLock = null;

					return null;
				}

				try
				{
					var streamReader = _buffer.StreamService.OpenReadStream(streamLock.Path, FileMode.Open, FileShare.Write);
					var lineReader = new LineReader(streamReader);

					for (var i = 0; i < bufferInfo.Read; i++)
					{
						if (lineReader.ReadLine(out var line) == false)
						{
							if (line != null)
							{
							}
							else
								break;
						}
					}

					return lineReader;
				}
				catch (IOException e)
				{
					_logger.LogError(e, "OpenReadBufferStream.");
				}
				finally
				{
					streamLock.Unlock();
				}

				streamLock = null;

				return null;
			}

			private StreamWriter OpenWriteBufferStream(out string bufferName, out FileLock streamLock)
			{
				bufferName = null;

				while (true)
					try
					{
						_markLock.Lock();

						var buffers = ReadMark(_markLock).Buffers;

						if (buffers.Count == 0)
							buffers.Add(new BufferInfo(GetNextBufferName(), 0, 0, true, DateTime.MinValue, 0, DateTime.MinValue));

						var bufferInfo = buffers[buffers.Count - 1];

						if (bufferInfo.WriteActive == false || bufferInfo.Written >= _fileBufferLimit)
						{
							bufferInfo = new BufferInfo(GetNextBufferName(), 0, 0, true, DateTime.MinValue, 0, DateTime.MinValue);
							buffers.Add(bufferInfo);
						}

						bufferName = bufferInfo.Name;
						streamLock = new FileLock(GetBufferFilePath(bufferInfo));

						var streamWriter =
							_buffer.StreamService.OpenWriteStream(streamLock.Path, FileMode.Append, FileAccess.Write, FileShare.Read);

						streamWriter.AutoFlush = false;

						WriteMark(_markLock, buffers);

						return streamWriter;
					}
					catch (IOException e)
					{
						_logger.LogWarning(e, $"OpenWriteBufferStream failed to open buffer '{bufferName}'.");
					}
					finally
					{
						_markLock.Unlock();
					}
			}

			private void PushBlock()
			{
				if (_disposed)
					return;

				if (_syncRead.WaitOne(0) == false)
					return;

				if (IsReadBufferEmpty())
				{
					_syncRead.Set();

					return;
				}

				Task.Run(PushBlockRoutine).ContinueWith(t => _syncRead.Set(), TaskScheduler.Default);
			}

			private long PushBlockImpl(FileLock markLock, int failSendLimit, Action<List<string>> alternativeWrite)
			{
				long totalSend = 0;

				if (_disposed)
					return totalSend;

				var memoryBuffer = new List<string>();

				while (true)
				{
					if (_disposed)
						break;

					var lineReader = OpenReadBufferStream(markLock, out var bufferName, out var readerLock);

					if (lineReader == null)
						break;

					try
					{
						while (lineReader != null)
						{
							memoryBuffer.Clear();

							try
							{
								readerLock.Lock();

								for (var iLine = memoryBuffer.Count; iLine < _readLimit; iLine++)
								{
									if (lineReader.ReadLine(out var line) == false)
									{
										if (line != null)
										{
											_logger.LogError($"BrokenLine: {line}");

											continue;
										}
									}

									if (line == null)
										break;

									memoryBuffer.Add(line);
								}
							}
							catch (Exception e)
							{
								memoryBuffer.Clear();

								_logger.LogError(e, "Read failed.");
							}
							finally
							{
								readerLock.Unlock();
							}

							if (memoryBuffer.Count == 0)
							{
								lineReader.Close();
								lineReader = null;

								break;
							}

							try
							{
								_handleRead(memoryBuffer);

#if DEBUG
								Console.WriteLine($"Push\t{DateTime.Now:HH:mm:ss}\t{memoryBuffer.Count}");
#endif

								totalSend += memoryBuffer.Count;

								HandleSuccessSend(markLock, bufferName, ref lineReader, readerLock, memoryBuffer);
							}
							catch (Exception e)
							{
								_logger.LogError(e, "Send block failed.");

								HandleFailedSend(markLock, bufferName, ref lineReader, readerLock, memoryBuffer, failSendLimit,
									alternativeWrite);

								break;
							}
						}
					}
					finally
					{
						lineReader?.Close();
					}
				}

				return totalSend;
			}

			private long _totalSend;
			private double _sendSpendMilliseconds;

			private void PushBlockRoutine()
			{
				var stopWatch = Stopwatch.StartNew();

				_totalSend += PushBlockImpl(_markLock, MainBufferFailedSendLimit, WriteErrorBuffer);

				stopWatch.Stop();

				_sendSpendMilliseconds += stopWatch.ElapsedMilliseconds;

				SendRatePerSecond = _totalSend / _sendSpendMilliseconds * 1000;
			}

			public double SendRatePerSecond { get; private set; } = 10000 * Environment.ProcessorCount;

			private void PushErrorBlock()
			{
				if (_syncErrorRead.WaitOne(0) == false)
					return;

				if (IsReadBufferEmpty(_errorMarkLock))
				{
					_syncErrorRead.Set();

					return;
				}

				Task.Run(PushErrorBlockRoutine).ContinueWith(t => _syncErrorRead.Set(), TaskScheduler.Default);
			}

			private void PushErrorBlockRoutine()
			{
				PushBlockImpl(_errorMarkLock, _resendIntervals.Length, WriteFatalBuffer);
				CleanStalledBuffers(_errorMarkLock);
			}

			private MarkInfo ReadMark(FileLock markLock)
			{
				var reader = _buffer.StreamService.OpenReadStream(markLock.Path, FileMode.OpenOrCreate, FileShare.None);
				var bufferInfos = new List<BufferInfo>();
				var written = 0;
				var read = 0;
				string bufferLine;

				while ((bufferLine = reader.ReadLine()) != null)
				{
					var values = bufferLine.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

					if (values.Length < 7)
						continue;

					var bufferName = values[0];
					var bufferWritten = int.Parse(values[1], CultureInfo.InvariantCulture);
					var bufferRead = int.Parse(values[2], CultureInfo.InvariantCulture);
					var bufferWriteActive = int.Parse(values[3], CultureInfo.InvariantCulture);
					var lastFlushTime = DateTime.Parse(values[4], CultureInfo.InvariantCulture);

					written += bufferWritten;
					read += bufferRead;

					var failedSendCount = int.Parse(values[5], CultureInfo.InvariantCulture);
					var lastSendTime = DateTime.Parse(values[6], CultureInfo.InvariantCulture);

					bufferInfos.Add(new BufferInfo(bufferName, bufferWritten, bufferRead, bufferWriteActive == 1, lastFlushTime,
						failedSendCount, lastSendTime));
				}

				reader.Close();

				return new MarkInfo(written, read, bufferInfos);
			}

			private void ReleaseMemoryBuffer(List<TItem> flushBuffer)
			{
				flushBuffer.Clear();

				lock (_memoryBufferPool)
				{
					_memoryBufferPool.Push(flushBuffer);
				}
			}

			private List<TItem> RentMemoryBuffer()
			{
				lock (_memoryBufferPool)
				{
					return _memoryBufferPool.Count > 0 ? _memoryBufferPool.Pop() : new List<TItem>();
				}
			}

			public void Write(TItem logEvent)
			{
				if (_disposed)
					throw new InvalidOperationException("Disposed");

				lock (_memoryBuffer)
				{
					_memoryBuffer.Add(logEvent);
				}

				Flush(false);
			}

			private void WriteErrorBuffer(IReadOnlyList<string> content)
			{
				while (true)
				{
					var nextErrorBufferName = $"{DateTime.Now.Ticks}.{_workerIndex}.error.buffer";
					var nextErrorBufferPath = GetBufferFilePath(nextErrorBufferName);
					var bufferLock = new FileLock(nextErrorBufferPath);

					try
					{
						bufferLock.Lock();

						if (_buffer.StreamService.IsStreamExist(nextErrorBufferPath))
							continue;

						var writer = _buffer.StreamService.OpenWriteStream(nextErrorBufferPath, FileMode.Create,
							FileAccess.ReadWrite, FileShare.None);

						foreach (var line in content)
						{
							WriteStartLine(writer);
							writer.Write(line);
							WriteEndLine(writer);
						}

						writer.Close();

						_errorMarkLock.DoLocked(() =>
						{
							var errorMark = ReadMark(_errorMarkLock);

							errorMark.Buffers.Add(new BufferInfo(nextErrorBufferName, content.Count, 0, false, DateTime.Now,
								MainBufferFailedSendLimit, DateTime.Now));

							WriteMark(_errorMarkLock, errorMark.Buffers);
						});

						return;
					}
					catch (IOException e)
					{
						_logger.LogError(e, "WriteErrorBuffer");
					}
					finally
					{
						bufferLock.Unlock();
					}
				}
			}

			private void WriteFatalBuffer(IReadOnlyList<string> content)
			{
				var writeAttempt = 0;

				while (true)
				{
					var nextErrorBufferName = $"{DateTime.Now.Ticks}.fatal.buffer";
					var nextErrorBufferPath = GetBufferFilePath(nextErrorBufferName);
					var bufferLock = new FileLock(nextErrorBufferPath);

					try
					{
						bufferLock.Lock();

						if (_buffer.StreamService.IsStreamExist(nextErrorBufferPath))
							continue;

						var writer = _buffer.StreamService.OpenWriteStream(nextErrorBufferPath, FileMode.Create,
							FileAccess.ReadWrite, FileShare.None);

						foreach (var line in content)
						{
							WriteStartLine(writer);
							writer.Write(line);
							WriteEndLine(writer);
						}

						writer.Close();

						break;
					}
					catch (Exception e)
					{
						_logger.LogError(e, "WriteFatalBuffer failed.");

						// Try to write 3 times. 
						if (++writeAttempt >= 3)
							break;
					}
					finally
					{
						bufferLock.Unlock();
					}
				}
			}

			private void WriteMark(FileLock markLock, IEnumerable<BufferInfo> bufferInfos)
			{
				using var writer =
					_buffer.StreamService.OpenWriteStream(markLock.Path, FileMode.Create, FileAccess.ReadWrite, FileShare.None);

				foreach (var bufferInfo in bufferInfos)
				{
					var bufferFilePath = GetBufferFilePath(bufferInfo);

					if (_buffer.StreamService.IsStreamExist(bufferFilePath) == false)
						continue;

					writer.Write(bufferInfo.Name);
					writer.Write('|');
					writer.Write(bufferInfo.Written);
					writer.Write('|');
					writer.Write(bufferInfo.Read);
					writer.Write('|');
					writer.Write(bufferInfo.WriteActive ? '1' : '0');
					writer.Write('|');
					writer.Write(bufferInfo.LastFlushTime.ToString(CultureInfo.InvariantCulture));
					writer.Write('|');
					writer.Write(bufferInfo.FailedSendCount);
					writer.Write('|');
					writer.Write(bufferInfo.LastSendTime.ToString(CultureInfo.InvariantCulture));
					writer.WriteLine();
				}

				writer.Close();
			}

			#endregion

			#region Nested Types

			private struct MarkInfo
			{
				public MarkInfo(int written, int read, List<BufferInfo> buffers)
				{
					Written = written;
					Read = read;
					Buffers = buffers;
				}

				public readonly int Written;
				public readonly int Read;
				public readonly List<BufferInfo> Buffers;
			}

			private struct BufferInfo
			{
				public BufferInfo(string name, int written, int read, bool writeActive, DateTime lastFlushTime,
					int failedSendCount, DateTime lastSendTime)
				{
					Name = name;
					Written = written;
					Read = read;
					WriteActive = writeActive;
					LastFlushTime = lastFlushTime;
					FailedSendCount = failedSendCount;
					LastSendTime = lastSendTime;
				}

				public readonly string Name;
				public readonly int Written;
				public readonly int Read;
				public readonly bool WriteActive;
				public readonly int FailedSendCount;
				public readonly DateTime LastSendTime;
				public readonly DateTime LastFlushTime;
			}

			private class FileLock
			{
				#region Fields

				private readonly Mutex _mutex;

				#endregion

				#region Properties

				public string Path { get; }

				#endregion

				#region Ctors

				private FileLock(Mutex mutex, string path)
				{
					_mutex = mutex;
					Path = path;
				}

				public FileLock(string filePath) : this(
					new Mutex(false, filePath.Replace(":", "").Replace("/", "").Replace("\\", "")), filePath)
				{
				}

				#endregion

				#region Methods

				public void DoLocked(Action action)
				{
					try
					{
						Lock();
						action();
					}
					finally
					{
						Unlock();
					}
				}

				public void Lock()
				{
					_mutex?.WaitOne();
				}

				public bool Lock(TimeSpan timeout)
				{
					return _mutex?.WaitOne(timeout) ?? false;
				}

				public void Unlock()
				{
					_mutex?.ReleaseMutex();
				}

				#endregion
			}

			#endregion
		}

		#endregion
	}
}