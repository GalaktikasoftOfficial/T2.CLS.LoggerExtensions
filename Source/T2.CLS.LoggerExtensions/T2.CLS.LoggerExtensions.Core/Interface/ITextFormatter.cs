// Copyright (C) 2019 Topsoft (https://topsoft.by)

using System.IO;

namespace T2.CLS.LoggerExtensions.Core.Interface
{
	public interface ITextFormatter
	{
		#region  Methods

		void Format(LogEvent logEvent, TextWriter output);

		#endregion
	}
}