// Copyright (C) 2019 Topsoft (https://topsoft.by)

using System;
using System.Collections.Generic;
using System.IO;

namespace T2.CLS.LoggerExtensions.Core.Formatters
{
	internal sealed class ArrayBatchFormatter : BatchFormatter
	{
		#region Ctors

		/// <summary>
		///   Initializes a new instance of the <see cref="ArrayBatchFormatter" /> class.
		/// </summary>
		/// <param name="eventBodyLimitBytes">
		///   The maximum size, in bytes, that the JSON representation of an event may take before it
		///   is dropped rather than being sent to the server. Specify null for no limit. Default
		///   value is 1024 KB.
		/// </param>
		public ArrayBatchFormatter(long? eventBodyLimitBytes = 1024 * 1024)
			: base(eventBodyLimitBytes)
		{
		}

		#endregion

		#region  Methods

		public override void Format(IEnumerable<string> logEvents, TextWriter output)
		{
			if (logEvents == null) throw new ArgumentNullException(nameof(logEvents));
			if (output == null) throw new ArgumentNullException(nameof(output));

			var delimiterStart = "[";

			foreach (var logEvent in logEvents)
			{
				if (string.IsNullOrWhiteSpace(logEvent))
					continue;


				output.Write(delimiterStart);
				output.Write(logEvent);
				delimiterStart = ",";
			}

			if (delimiterStart == ",")
				output.Write("]");
		}

		#endregion
	}
}