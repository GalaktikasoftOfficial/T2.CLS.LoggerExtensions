// Copyright (C) 2019 Topsoft (https://topsoft.by)

using System;
using System.Globalization;
using System.IO;
using T2.CLS.LoggerExtensions.Core.Interface;

namespace T2.CLS.LoggerExtensions.Core.Formatters
{
	public sealed class JsonFormatter : ITextFormatter
	{
		#region Static Fields and Constants

		public static readonly ITextFormatter Instance = new JsonFormatter();

		#endregion

		#region Ctors

		private JsonFormatter()
		{
		}

		#endregion

		#region  Methods

		private static void FormatBooleanValue(bool value, TextWriter output)
		{
			output.Write(value ? "true" : "false");
		}

		private static void FormatDateTimeValue(IFormattable value, TextWriter output)
		{
			output.Write('"');
			output.Write(value.ToString("O", CultureInfo.InvariantCulture));
			output.Write('"');
		}

		private static void FormatDoubleValue(double value, TextWriter output)
		{
			if (double.IsNaN(value) || double.IsInfinity(value))
				FormatStringValue(value.ToString(CultureInfo.InvariantCulture), output);
			else
				output.Write(value.ToString("R", CultureInfo.InvariantCulture));
		}

		private static void FormatExactNumericValue(IFormattable value, TextWriter output)
		{
			output.Write(value.ToString(null, CultureInfo.InvariantCulture));
		}

		private static void FormatFloatValue(float value, TextWriter output)
		{
			if (float.IsNaN(value) || float.IsInfinity(value))
				FormatStringValue(value.ToString(CultureInfo.InvariantCulture), output);
			else
				output.Write(value.ToString("R", CultureInfo.InvariantCulture));
		}

		private static void FormatLiteralObjectValue(object value, TextWriter output)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			FormatStringValue(value.ToString(), output);
		}

		private static void FormatNullValue(TextWriter output)
		{
			output.Write("null");
		}

		private static void FormatStringValue(string str, TextWriter output)
		{
			WriteQuotedJsonString(str, output);
		}

		private static void FormatTimeSpanValue(TimeSpan value, TextWriter output)
		{
			output.Write('"');
			output.Write(value.ToString("YYYY-MM-dd HH:mm:ss.fff  zzz"));
			output.Write('"');
		}

		private static void FormatValue(object value, TextWriter output)
		{
			if (value == null)
			{
				FormatNullValue(output);
			}
			else
			{
				if (value is string str)
				{
					FormatStringValue(str, output);
				}
				else
				{
					if (value is ValueType)
					{
						if (value is int || value is uint || (value is long || value is ulong) || (value is decimal || value is byte || (value is sbyte || value is short)) || value is ushort)
						{
							FormatExactNumericValue((IFormattable) value, output);

							return;
						}

						if (value is double d)
						{
							FormatDoubleValue(d, output);

							return;
						}

						if (value is float f)
						{
							FormatFloatValue(f, output);

							return;
						}

						if (value is bool b)
						{
							FormatBooleanValue(b, output);

							return;
						}

						if (value is char)
						{
							FormatStringValue(value.ToString(), output);

							return;
						}

						if (value is DateTime || value is DateTimeOffset)
						{
							FormatDateTimeValue((IFormattable) value, output);

							return;
						}

						if (value is TimeSpan span)
						{
							FormatTimeSpanValue(span, output);

							return;
						}
					}

					FormatLiteralObjectValue(value, output);
				}
			}
		}

		private static void WriteQuotedJsonString(string str, TextWriter output)
		{
			output.Write('"');

			var startIndex = 0;
			var flag = false;

			for (var index = 0; index < str.Length; ++index)
			{
				var ch = str[index];

				if (ch < ' ' || ch == '\\' || ch == '"')
				{
					flag = true;
					output.Write(str.Substring(startIndex, index - startIndex));
					startIndex = index + 1;

					switch (ch)
					{
						case '\t':
							output.Write("\\t");

							continue;
						case '\n':
							output.Write("\\n");

							continue;
						case '\f':
							output.Write("\\f");

							continue;
						case '\r':
							output.Write("\\r");

							continue;
						case '"':
							output.Write("\\\"");

							continue;
						case '\\':
							output.Write("\\\\");

							continue;
						default:
							output.Write("\\u");
							output.Write(((int) ch).ToString("X4"));

							continue;
					}
				}
			}

			if (flag)
			{
				if (startIndex != str.Length)
					output.Write(str.Substring(startIndex));
			}
			else
				output.Write(str);

			output.Write('"');
		}

		#endregion

		#region Interface Implementations

		#region ITextFormatter

		public void Format(LogEvent logEvent, TextWriter output)
		{
			// LogTime
			output.Write("{\"EventTime\":\"");
			output.Write(logEvent.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));

			// EventTimeTics
			output.Write("\",\"EventTimeTics\":");
			FormatValue(logEvent.EventTimeTics, output);

			// TimeZone
			output.Write(",\"TimeZone\":");
			WriteQuotedJsonString(logEvent.TimeZone, output);

			// Level
			output.Write(",\"Level\":");
			FormatValue(((int) logEvent.Level), output);

			// Message
			output.Write(",\"Message\":");
			WriteQuotedJsonString(logEvent.Message, output);

			// ProcessId
			output.Write(",\"ProcessId\":");
			FormatValue(logEvent.ProcessId, output);

			// ThreadId
			output.Write(",\"ThreadId\":");
			FormatValue(logEvent.ThreadId, output);

			// UserName
			output.Write(",\"EnvironmentUserName\":");
			WriteQuotedJsonString(logEvent.EnvironmentUserName, output);

			// MachineName
			output.Write(",\"MachineName\":");
			WriteQuotedJsonString(logEvent.MachineName, output);

			// Exception
			if (logEvent.Exception != null)
			{
				output.Write(",\"Exception\":");
				WriteQuotedJsonString(logEvent.Exception.ToString(), output);
			}

			if (logEvent.Properties?.Count > 0)
			{
				foreach (var property in logEvent.Properties)
				{
					if (LogEvent.IsSystemProperty(property.Key))
						continue;

					output.Write(",");

					WriteQuotedJsonString(property.Key, output);
					output.Write(':');
					FormatValue(property.Value, output);
				}
			}

			output.Write('}');
		}

		#endregion

		#endregion
	}
}