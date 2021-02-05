// Copyright (C) 2019 Topsoft (https://topsoft.by)

using System;
using Microsoft.Extensions.Logging;
using NLog.Common;

namespace T2.CLS.LoggerExtensions.NLog
{
	internal sealed class InternalLoggerFactory : ILoggerFactory
	{
		#region Static Fields and Constants

		public static readonly ILoggerFactory Instance = new InternalLoggerFactory();

		#endregion

		#region Ctors

		private InternalLoggerFactory()
		{
		}

		#endregion

		#region Interface Implementations

		#region IDisposable

		public void Dispose()
		{
		}

		#endregion

		#region ILoggerFactory

		public ILogger CreateLogger(string categoryName)
		{
			return InternalLoggerImpl.LoggerInstance;
		}

		public void AddProvider(ILoggerProvider provider)
		{
		}

		#endregion

		#endregion

		#region  Nested Types

		private sealed class InternalLoggerImpl : ILogger
		{
			#region Static Fields and Constants

			public static readonly ILogger LoggerInstance = new InternalLoggerImpl();

			#endregion

			#region Ctors

			private InternalLoggerImpl()
			{
			}

			#endregion

			#region Interface Implementations

			#region ILogger

			public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
			{
				if (formatter == null)
					throw new ArgumentNullException(nameof(formatter));

				var nlogLevel = NLogUtils.ConvertLevel(logLevel);
				var message = formatter(state, exception);

				if (exception != null)
					InternalLogger.Log(exception, nlogLevel, message);
				else
					InternalLogger.Log(nlogLevel, message);
			}

			public bool IsEnabled(LogLevel logLevel)
			{
				switch (logLevel)
				{
					case LogLevel.Trace:
						return InternalLogger.IsTraceEnabled;
					case LogLevel.Debug:
						return InternalLogger.IsDebugEnabled;
					case LogLevel.Information:
						return InternalLogger.IsInfoEnabled;
					case LogLevel.Warning:
						return InternalLogger.IsWarnEnabled;
					case LogLevel.Error:
						return InternalLogger.IsErrorEnabled;
					case LogLevel.Critical:
						return InternalLogger.IsFatalEnabled;
					default:
						return false;
				}
			}

			public IDisposable BeginScope<TState>(TState state)
			{
				return NullDisposable.NullDisposableInstance;
			}

			#endregion

			#endregion

			#region  Nested Types

			private class NullDisposable : IDisposable
			{
				#region Static Fields and Constants

				public static readonly NullDisposable NullDisposableInstance = new NullDisposable();

				#endregion

				#region Interface Implementations

				#region IDisposable

				public void Dispose()
				{
				}

				#endregion

				#endregion
			}

			#endregion
		}

		#endregion
	}
}