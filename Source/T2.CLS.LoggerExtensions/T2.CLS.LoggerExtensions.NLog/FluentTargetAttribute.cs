// Copyright (C) 2019 Topsoft (https://topsoft.by)

using NLog.Config;
using NLog.Layouts;

namespace T2.CLS.LoggerExtensions.NLog
{
	[NLogConfigurationItem]
	public class FluentTargetAttribute
	{
		#region Properties

		[RequiredParameter]
		public Layout Layout { get; set; }

		[RequiredParameter]
		public string Name { get; set; }

		#endregion
	}
}