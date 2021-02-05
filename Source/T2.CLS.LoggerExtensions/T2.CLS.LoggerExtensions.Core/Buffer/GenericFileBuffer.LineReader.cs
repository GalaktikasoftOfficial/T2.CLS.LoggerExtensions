// Copyright (C) 2019 Topsoft (https://topsoft.by)

using System;
using System.IO;
using System.Text;

namespace T2.CLS.LoggerExtensions.Core.Buffer
{
	public abstract partial class GenericFileBuffer<TItem>
	{
		#region Nested Types

		private sealed class LineReader
		{
			private readonly char[] _buffer = new char[4 * 4096];
			private readonly TextReader _reader;
			private readonly StringBuilder _stringBuilder = new StringBuilder();
			private int _bufferCount;
			private int _bufferPointer;
			private Status _status = Status.Default;

			public LineReader(TextReader reader)
			{
				_reader = reader;
			}

			private string BuildLine()
			{
				var line = _stringBuilder.ToString();

				_stringBuilder.Clear();

				return line;
			}

			public bool ReadFinished => _status == Status.Finished;

			public bool ReadLine(out string line)
			{
				line = null;

				if (_status == Status.Finished)
					return false;

				if (_bufferPointer == _bufferCount)
				{
					_bufferCount = _reader.ReadBlock(_buffer, 0, _buffer.Length);
					_bufferPointer = 0;

					if (_bufferCount == 0)
					{
						_status = Status.Finished;

						return false;
					}
				}

				var lineStart = _bufferPointer;

				if (_status == Status.Default)
				{
					_status = _buffer[_bufferPointer++] == LineHelper.StartLine ? Status.StartMatched : Status.StartMissing;

					if (_status == Status.StartMatched)
						lineStart++;
				}

				while (true)
				{
					var c = (char) 0;
					var span = new ReadOnlySpan<char>(_buffer, _bufferPointer, _bufferCount - _bufferPointer);

					foreach (var ch in span)
					{
						if (ch == LineHelper.EndLine || ch == LineHelper.StartLine)
						{
							c = ch;

							break;
						}

						_bufferPointer++;
					}

					if (c != 0)
					{
						var blockLength = _bufferPointer - lineStart;

						if (blockLength > 0)
						{
							if (_stringBuilder.Length > 0)
							{
								_stringBuilder.Append(_buffer, lineStart, blockLength);

								line = BuildLine();
							}
							else
								line = new string(_buffer, lineStart, blockLength);
						}
						else if (_stringBuilder.Length > 0)
							line = BuildLine();

						_bufferPointer++;

						if (c == LineHelper.EndLine)
						{
							var result = _status == Status.StartMatched;

							_status = Status.Default;

							return result;
						}

						_status = Status.StartMatched;

						return false;
					}

					if (_bufferCount - lineStart > 0)
						_stringBuilder.Append(_buffer, lineStart, _bufferCount - lineStart);

					_bufferCount = _reader.ReadBlock(_buffer, 0, _buffer.Length);
					_bufferPointer = 0;

					lineStart = 0;

					if (_bufferCount == 0)
						break;
				}

				_status = Status.Finished;

				line = _stringBuilder.Length > 0 ? BuildLine() : null;

				return false;
			}

			private enum Status
			{
				Default,
				Finished,
				StartMissing,
				StartMatched
			}

			public void Close()
			{
				_reader.Close();
			}
		}

		#endregion
	}
}