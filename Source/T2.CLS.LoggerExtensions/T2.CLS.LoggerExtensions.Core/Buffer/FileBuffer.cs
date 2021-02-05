// Copyright (C) 2019 Topsoft (https://topsoft.by)

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using T2.CLS.LoggerExtensions.Core.Interface;

namespace T2.CLS.LoggerExtensions.Core.Buffer
{
	internal sealed class FileBuffer : Buffer
	{
		#region Fields

		private readonly FileBufferImpl _bufferImpl;

		#endregion

		#region Ctors

		public FileBuffer(string path,
			ITextFormatter textFormatter,
			Action<IReadOnlyList<string>> handleRead,
			int readLimit,
			int? memoryBufferLimit = null,
			int? fileBufferLimit = null,
			double? flushTimeout = null,
			int workerCount = 1,
			Encoding encoding = null,
			ILoggerFactory loggerFactory = null) : base(textFormatter)
		{
			_bufferImpl = new FileBufferImpl(path, textFormatter, handleRead, readLimit, memoryBufferLimit, fileBufferLimit, flushTimeout, workerCount, encoding, loggerFactory);
		}

		#endregion

		#region  Methods

		protected override void DisposeCore(bool disposing)
		{
			_bufferImpl.Dispose();
		}

		public override void Write(LogEvent logEvent)
		{
			_bufferImpl.Write(logEvent);
		}

		#endregion

		#region  Nested Types

		private sealed class FileBufferImpl : GenericFileBuffer<LogEvent>
		{
			#region Fields

			private readonly Action<IReadOnlyList<string>> _handleRead;

			private readonly ITextFormatter _textFormatter;

			#endregion

			#region Ctors

			public FileBufferImpl(string path,
				ITextFormatter textFormatter,
				Action<IReadOnlyList<string>> handleRead,
				int readLimit,
				int? memoryBufferLimit = null,
				int? fileBufferLimit = null,
				double? flushTimeout = null,
				int workerCount = 1,
				Encoding encoding = null,
				ILoggerFactory loggerFactory = null) : base(path, readLimit, memoryBufferLimit, fileBufferLimit, flushTimeout, null, null, workerCount, encoding, loggerFactory)
			{
				_textFormatter = textFormatter;
				_handleRead = handleRead;
			}

			#endregion

			#region  Methods

			protected override void FormatItem(LogEvent item, StreamWriter streamWriter)
			{
				_textFormatter.Format(item, streamWriter);
			}

			protected override void HandleRead(IReadOnlyList<string> buffer)
			{
				_handleRead(buffer);
			}

			#endregion
		}

		#endregion
	}
}