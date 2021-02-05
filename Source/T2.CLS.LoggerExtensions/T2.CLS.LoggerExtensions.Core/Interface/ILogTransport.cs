// Copyright (C) 2019 Topsoft (https://topsoft.by)

using System;

namespace T2.CLS.LoggerExtensions.Core.Interface
{
	public interface ILogTransport : IDisposable
	{
		#region  Methods

		void Send(LogEvent logEvent);

		#endregion
	}
}