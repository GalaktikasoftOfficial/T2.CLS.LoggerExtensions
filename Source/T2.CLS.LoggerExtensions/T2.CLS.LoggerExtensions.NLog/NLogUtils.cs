// Copyright (C) 2019 Topsoft (https://topsoft.by)

// Copyright (C) 2019 Galaktikasoft (www.Galaktikasoft.com)

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace T2.CLS.LoggerExtensions.NLog
{
	internal static class NLogUtils
	{
		#region Static Fields and Constants

		private static readonly LogLevel[] NLogLevelMap;

		#endregion

		#region Ctors

		static NLogUtils()
		{
			var levelTuples = new List<(int, LogLevel)>
			{
				(global::NLog.LogLevel.Debug.Ordinal, LogLevel.Debug),
				(global::NLog.LogLevel.Debug.Ordinal, LogLevel.Debug),
				(global::NLog.LogLevel.Error.Ordinal, LogLevel.Error),
				(global::NLog.LogLevel.Fatal.Ordinal, LogLevel.Critical),
				(global::NLog.LogLevel.Info.Ordinal, LogLevel.Information),
				(global::NLog.LogLevel.Trace.Ordinal, LogLevel.Trace),
				(global::NLog.LogLevel.Warn.Ordinal, LogLevel.Warning),
				(global::NLog.LogLevel.Off.Ordinal, LogLevel.None)
			};

			var maxOrdinal = levelTuples.Max(t => t.Item1);

			NLogLevelMap = new LogLevel[maxOrdinal + 1];

			foreach (var levelTuple in levelTuples)
				NLogLevelMap[levelTuple.Item1] = levelTuple.Item2;
		}

		#endregion

		#region  Methods

		public static LogLevel ConvertLevel(global::NLog.LogLevel logLevel)
		{
			return NLogLevelMap[logLevel.Ordinal];
		}

		public static global::NLog.LogLevel ConvertLevel(LogLevel logLevel)
		{
			switch (logLevel)
			{
				case LogLevel.Trace:
					return global::NLog.LogLevel.Trace;
				case LogLevel.Debug:
					return global::NLog.LogLevel.Debug;
				case LogLevel.Information:
					return global::NLog.LogLevel.Info;
				case LogLevel.Warning:
					return global::NLog.LogLevel.Warn;
				case LogLevel.Error:
					return global::NLog.LogLevel.Error;
				case LogLevel.Critical:
					return global::NLog.LogLevel.Fatal;
				case LogLevel.None:
					return global::NLog.LogLevel.Off;
				default:
					return global::NLog.LogLevel.Debug;
			}
		}

		#endregion
	}
}