// Copyright (C) 2019 Topsoft (https://topsoft.by)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using T2.CLS.LoggerExtensions.Core.Interface;
using SelfLog = System.Console;

namespace T2.CLS.LoggerExtensions.Core.Formatters
{
	internal abstract class BatchFormatter : IBatchFormatter
	{
		#region Fields

		private readonly long? _eventBodyLimitBytes;

		#endregion

		#region Ctors

		protected BatchFormatter(long? eventBodyLimitBytes)
		{
			_eventBodyLimitBytes = eventBodyLimitBytes;
		}

		#endregion

		#region  Methods

		protected bool CheckEventBodySize(string json)
		{
			if (_eventBodyLimitBytes.HasValue &&
			    Encoding.UTF8.GetByteCount(json) > _eventBodyLimitBytes.Value)
			{
				SelfLog.WriteLine(
					"Event JSON representation exceeds the byte size limit of {0} set for this sink and will be dropped; data: {1}",
					_eventBodyLimitBytes,
					json);

				return false;
			}

			return true;
		}

		#endregion

		#region Interface Implementations

		#region IBatchFormatter

		public void Format(IEnumerable<LogEvent> logEvents, ITextFormatter formatter, TextWriter output)
		{
			if (logEvents == null) throw new ArgumentNullException(nameof(logEvents));
			if (formatter == null) throw new ArgumentNullException(nameof(formatter));

			var formattedLogEvents = logEvents.Select(logEvent =>
			{
				var writer = new StringWriter();

				formatter.Format(logEvent, writer);

				return writer.ToString();
			});

			Format(formattedLogEvents, output);
		}

		public abstract void Format(IEnumerable<string> logEvents, TextWriter output);

		#endregion

		#endregion
	}
}