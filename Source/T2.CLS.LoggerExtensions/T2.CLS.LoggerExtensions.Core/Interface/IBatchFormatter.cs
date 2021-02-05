// Copyright (C) 2019 Topsoft (https://topsoft.by)

using System.Collections.Generic;
using System.IO;

namespace T2.CLS.LoggerExtensions.Core.Interface
{
	public interface IBatchFormatter
	{
		#region  Methods

		void Format(IEnumerable<LogEvent> logEvents, ITextFormatter formatter, TextWriter output);

		void Format(IEnumerable<string> logEvents, TextWriter output);

		#endregion
	}
}