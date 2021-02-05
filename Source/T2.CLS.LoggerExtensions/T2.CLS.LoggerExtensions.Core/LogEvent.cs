// Copyright (C) 2019 Topsoft (https://topsoft.by)

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace T2.CLS.LoggerExtensions.Core
{
	public sealed class LogEvent
	{
		#region Static Fields and Constants

		private static string _environmentUserNameCache;

		#endregion

		#region Fields

		private readonly Dictionary<string, object> _properties;

		#endregion

		#region Ctors

		public LogEvent(DateTimeOffset timestamp, LogLevel level, string message, Exception exception, int processId, int threadId, string environmentUserName, string machineName, Dictionary<string, object> properties)
		{
			_properties = properties;
			Message = message;
			Timestamp = timestamp;
			Level = level;
			Exception = exception;
			ProcessId = processId;
			ThreadId = threadId;
			EnvironmentUserName = environmentUserName;
			MachineName = machineName;
		}

		#endregion

		#region Properties

		public static string CurrentUserName => EnvironmentUserNameCache;

		public string EnvironmentUserName { get; }

		private static string EnvironmentUserNameCache => _environmentUserNameCache ??= $"{Environment.UserDomainName}\\{Environment.UserName}";

		public long EventTimeTics => Timestamp.ToUnixTimeMilliseconds();

		public Exception Exception { get; }

		public LogLevel Level { get; }

		public string MachineName { get; }

		public string Message { get; }

		public int ProcessId { get; }

		public IReadOnlyDictionary<string, object> Properties => _properties;

		public int ThreadId { get; }

		public DateTimeOffset Timestamp { get; }

		public string TimeZone => Timestamp.ToString("zzz", DateTimeFormatInfo.InvariantInfo);

		#endregion

		#region  Methods

		public static bool IsSystemProperty(string property)
		{
			return string.Equals(nameof(ProcessId), property, StringComparison.OrdinalIgnoreCase) ||
			       string.Equals(nameof(ThreadId), property, StringComparison.OrdinalIgnoreCase) ||
			       string.Equals(nameof(EnvironmentUserName), property, StringComparison.OrdinalIgnoreCase) ||
			       string.Equals(nameof(MachineName), property, StringComparison.OrdinalIgnoreCase) ||
			       string.Equals(nameof(EventTimeTics), property, StringComparison.OrdinalIgnoreCase) ||
			       string.Equals(nameof(TimeZone), property, StringComparison.OrdinalIgnoreCase);
		}

		#endregion
	}
}