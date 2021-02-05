// Copyright (C) 2019 Topsoft (https://topsoft.by)

using System;
using Microsoft.Extensions.Logging;
using Serilog.Debugging;

namespace T2.CLS.LoggerExtensions.Serilog
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

				SelfLog.WriteLine(exception != null ? $"{formatter(state, exception)}{Environment.NewLine}Exception: {exception}" : formatter(state, null));
			}

			public bool IsEnabled(LogLevel logLevel)
			{
				return true;
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