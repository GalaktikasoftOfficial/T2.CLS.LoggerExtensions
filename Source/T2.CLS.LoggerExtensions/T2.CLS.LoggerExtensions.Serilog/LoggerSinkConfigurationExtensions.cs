// Copyright (C) 2019 Topsoft (https://topsoft.by)

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using T2.CLS.LoggerExtensions.Core;
using T2.CLS.LoggerExtensions.Core.Senders;

namespace T2.CLS.LoggerExtensions.Serilog
{
	public static class LoggerSinkConfigurationExtensions
	{
		#region  Methods

		[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
		public static LoggerConfiguration DurableFluentd(
			this LoggerSinkConfiguration sinkConfiguration,
			string[] requestUri,
			LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
			HttpClient httpClient = null,
			string bufferPath = null,
			int? memoryBufferLimit = null,
			int? fileBufferLimit = null,
			int? fluentBufferLimit = null,
			double? flushTimeout = null,
			int workerCount = 1,
			Encoding encoding = null
			)
		{
			if (sinkConfiguration == null) throw new ArgumentNullException(nameof(sinkConfiguration));

			var transport = new DurableFluentdHttpSender(requestUri, httpClient, bufferPath, memoryBufferLimit,
				fileBufferLimit, fluentBufferLimit, flushTimeout, workerCount, encoding, InternalLoggerFactory.Instance);

			return sinkConfiguration.Sink(new LogTransportSink(transport), restrictedToMinimumLevel);
		}

		#endregion
	}
}