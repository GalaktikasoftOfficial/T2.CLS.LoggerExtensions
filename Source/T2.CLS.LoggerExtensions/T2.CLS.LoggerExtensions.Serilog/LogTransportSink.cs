// Copyright (C) 2019 Topsoft (https://topsoft.by)

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using Serilog.Events;
using T2.CLS.LoggerExtensions.Core;
using T2.CLS.LoggerExtensions.Core.Interface;
using LogEvent = Serilog.Events.LogEvent;

namespace T2.CLS.LoggerExtensions.Serilog
{
	public sealed class LogTransportSink : ILogEventSink, IDisposable
	{
		#region Fields

		private readonly ILogTransport _logTransport;

		#endregion

		#region Ctors

		public LogTransportSink(ILogTransport logTransport)
		{
			_logTransport = logTransport;
		}

		#endregion

		#region  Methods

		private static LogLevel ConvertLogLevel(LogEventLevel serilogLogLevel)
		{
			switch (serilogLogLevel)
			{
				case LogEventLevel.Verbose:
					return LogLevel.Trace;
				case LogEventLevel.Debug:
					return LogLevel.Debug;
				case LogEventLevel.Information:
					return LogLevel.Information;
				case LogEventLevel.Warning:
					return LogLevel.Warning;
				case LogEventLevel.Error:
					return LogLevel.Error;
				case LogEventLevel.Fatal:
					return LogLevel.Critical;
				default:
					throw new ArgumentOutOfRangeException(nameof(serilogLogLevel), serilogLogLevel, null);
			}
		}

		private static object GetPropertyValue(LogEventPropertyValue propertyValue)
		{
			return propertyValue switch
			{
				SequenceValue sequenceValue => sequenceValue.Elements.Select(RenderSequenceValue).ToArray(),
				ScalarValue scalarValue => scalarValue.Value,
				_ => propertyValue.ToString()
			};
		}

		private static object RenderSequenceValue(LogEventPropertyValue x) => (x as ScalarValue)?.Value ?? x.ToString();

		#endregion

		#region Interface Implementations

		#region IDisposable

		public void Dispose()
		{
			_logTransport?.Dispose();
		}

		#endregion

		#region ILogEventSink

		public void Emit(LogEvent logEvent)
		{
			var level = ConvertLogLevel(logEvent.Level);
			var timestamp = logEvent.Timestamp;
			var message = logEvent.RenderMessage();
			var exception = logEvent.Exception;
			var processId = Process.GetCurrentProcess().Id;
			var threadId = Thread.CurrentThread.ManagedThreadId;
			var userName = Core.LogEvent.CurrentUserName;
			var machineName = Environment.MachineName;

			_logTransport.Send(new Core.LogEvent(timestamp, level, message, exception, processId, threadId, userName, machineName, logEvent.Properties.ToDictionary(kv => kv.Key, kv => GetPropertyValue(kv.Value))));
		}

		#endregion

		#endregion
	}
}