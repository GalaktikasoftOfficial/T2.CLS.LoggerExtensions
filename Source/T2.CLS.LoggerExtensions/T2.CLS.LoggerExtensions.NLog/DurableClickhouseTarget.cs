// Copyright (C) 2019 Topsoft (https://topsoft.by)

// Copyright (C) 2019 Topsoft (https://topsoft.by)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading;
using NLog;
using NLog.Config;
using NLog.Targets;
using T2.CLS.LoggerExtensions.Core;
using T2.CLS.LoggerExtensions.Core.Senders;

namespace T2.CLS.LoggerExtensions.NLog
{
	[Target("DurableClickhouse")]
	public class DurableClickhouseTarget : TargetWithLayout
	{
		#region Properties

		[ArrayParameter(typeof(FluentTargetAttribute), "attribute")]
		public IList<FluentTargetAttribute> Attributes { get; private set; } = new List<FluentTargetAttribute>();

		[DefaultParameter]
		public string BufferPath { get; set; }

		[DefaultParameter]
		public string DiagnosticLogPath { get; set; }

		[DefaultParameter]
		public int FileBufferLimit { get; set; } = 4096;

		[DefaultParameter]
		public int SenderBufferLimit { get; set; } = 512;

		[DefaultParameter]
		public double FlushTimeout { get; set; } = 5000;

		[DefaultParameter]
		public HttpClient HttpClient { get; set; }

		[DefaultParameter]
		public int MemoryBufferLimit { get; set; } = 64;

		[RequiredParameter]
		public List<string> RequestUri { get; set; }

		private DurableHttpSender Transport { get; set; }

		[DefaultParameter]
		public int WorkerCount { get; set; } = 1;

		[DefaultParameter]
		public Encoding Encoding { get; set; } = null;

		#endregion

		#region  Methods

		protected override void CloseTarget()
		{
			base.CloseTarget();

			Transport.Dispose();
		}

		protected virtual DurableHttpSender CreateTransport()
		{
			return new DurableHttpSender(RequestUri.ToArray(), HttpClient, BufferPath, MemoryBufferLimit, FileBufferLimit, SenderBufferLimit, FlushTimeout, WorkerCount, Encoding, InternalLoggerFactory.Instance);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
				Transport.Dispose();
		}

		protected override void InitializeTarget()
		{
			base.InitializeTarget();

			Transport ??= CreateTransport();
		}

		protected override void Write(LogEventInfo logEvent)
		{
			if (logEvent == null)
				throw new ArgumentNullException(nameof(logEvent));

			var timestamp = logEvent.TimeStamp;
			var level = NLogUtils.ConvertLevel(logEvent.Level);
			var message = logEvent.FormattedMessage;
			var exception = logEvent.Exception;
			var processId = Process.GetCurrentProcess().Id;
			var threadId = Thread.CurrentThread.ManagedThreadId;
			var userName = LogEvent.CurrentUserName;
			var machineName = Environment.MachineName;

			Dictionary<string, object> properties = null;

			if (logEvent.HasProperties)
			{
				properties = new Dictionary<string, object>();

				foreach (var logEventProperty in logEvent.Properties)
				{
					var propertyName = logEventProperty.Key.ToString();

					properties[propertyName] = logEventProperty.Value;
				}
			}

			if (Attributes.Count > 0)
			{
				properties ??= new Dictionary<string, object>();

				foreach (var attribute in Attributes)
				{
					var propertyName = attribute.Name;

					properties[propertyName] = attribute.Layout.Render(logEvent);
				}
			}

			var coreLogEvent = new LogEvent(timestamp, level, message, exception, processId, threadId, userName, machineName, properties);

			Transport.Send(coreLogEvent);
		}

		#endregion
	}
}