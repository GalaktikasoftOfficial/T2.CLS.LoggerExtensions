// Copyright (C) 2019 Topsoft (https://topsoft.by)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using T2.CLS.LoggerExtensions.Core.Interface;
using T2.CLS.LoggerExtensions.Core.Extensions;

namespace T2.CLS.LoggerExtensions.Core.Buffer
{
	public abstract partial class GenericFileBuffer<TItem> : IDisposable
	{
		
		#region Ctors

		protected GenericFileBuffer(string path,
			int readLimit,
			int? memoryBufferLimit = null,
			int? fileBufferLimit = null,
			double? flushTimeout = null,
			double? resendTimeout = null,
			double[] resendIntervals = null,

			int workerCount = 1,
			Encoding encoding = null,
			ILoggerFactory loggerFactory = null)
		{
			_logger = loggerFactory?.CreateLogger(typeof(GenericFileBuffer<TItem>))?? NullLogger.Instance;
			
			_logger.LEC00005_Debug_GenericFileBuffer();
			_logger.LEC00006_Trace_InputParameter_path_path(path);
            _logger.LEC00007_Trace_InputParameter_readLimit_readLimit(readLimit);
            _logger.LEC00008_Trace_InputParameter_memoryBufferLimit_memoryBufferLimit(memoryBufferLimit);
            _logger.LEC00009_Trace_InputParameter_fileBufferLimit_fileBufferLimit(fileBufferLimit);
            _logger.LEC00010_Trace_InputParameter_flushTimeout_flushTimeout(flushTimeout);
            _logger.LEC00011_Trace_InputParameter_resendTimeout_resendTimeout(resendTimeout);
            _logger.LEC00012_Trace_InputParameter_workerCount_workerCount(workerCount);
			_workerCount = workerCount < 1 ? 1 : workerCount > 8 ? 8 : workerCount;
			_workers = new FileBufferWorker[_workerCount];
			_encoding = encoding;

			_logger.LEC00002_Debug_Checked_WorkerCount_value_workerCount(workerCount);

			var workerLogger = loggerFactory?.CreateLogger<FileBufferWorker>() ?? NullLogger<FileBufferWorker>.Instance;

			if (Path.IsPathRooted(path) == false)
				path = Path.Combine(AppContext.BaseDirectory, path);

            _logger.LEC00003_Debug_Checked_path_value_value(path);

			for (var i = 0; i < _workerCount; i++)
				_workers[i] = new FileBufferWorker(this, workerLogger, path, i, HandleRead, readLimit, memoryBufferLimit,
					fileBufferLimit, flushTimeout, resendTimeout, resendIntervals);
		}

		#endregion

		#region Interface Implementations

		#region IDisposable

		public void Dispose()
		{
			DisposeCore(true);
		}

		#endregion

		#endregion

		#region Fields

		private readonly int _workerCount;
		private readonly FileBufferWorker[] _workers;
		private long _worker;
		private IStreamService _streamService;
		private readonly Encoding _encoding;
		private ILogger _logger;

		#endregion

		#region Methods

		protected long GetPendingLineCount()
		{
			long written = 0;
			long read = 0;

			foreach (var worker in _workers)
			{
				worker.GetInfo(out var workerWritten, out var workerRead);

				written += workerWritten;
				read += workerRead;
			}

			return written - read;
		}

		protected double GetSendRatePerSecond()
		{
			return _workers.Sum(w => w.SendRatePerSecond) / _workerCount;
		}

		protected virtual void DisposeCore(bool disposing)
		{
			foreach (var worker in _workers)
				worker.Dispose(disposing);
		}

		protected abstract void FormatItem(TItem item, StreamWriter streamWriter);

		protected abstract void HandleRead(IReadOnlyList<string> buffer);

		public void Write(TItem logEvent)
		{
			var workerIndex = Interlocked.Increment(ref _worker) % _workerCount;

			_workers[workerIndex].Write(logEvent);
		}

		#endregion
	}
}

