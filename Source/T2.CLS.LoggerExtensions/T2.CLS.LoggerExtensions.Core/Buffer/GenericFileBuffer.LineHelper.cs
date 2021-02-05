// Copyright (C) 2019 Topsoft (https://topsoft.by)

using System.IO;

namespace T2.CLS.LoggerExtensions.Core.Buffer
{
	public abstract partial class GenericFileBuffer<TItem>
	{
		#region Nested Types

		private static class LineHelper
		{
			// UTF-8/UTF-16 do not use these bytes to represent actual chars
			public const byte StartLine = 0xFE;
			public const byte EndLine = 0xFF;

			public static void WriteStartLine(TextWriter writer)
			{
				writer.Write((char) StartLine);
			}

			public static void Write(TextWriter writer, string str)
			{
				writer.Write(str);
			}

			public static void WriteEndLine(TextWriter writer)
			{
				writer.Write((char) EndLine);
			}

			public static void WriteLine(TextWriter writer, string line)
			{
				WriteStartLine(writer);

				writer.Write(line);

				WriteEndLine(writer);
			}
		}

		#endregion
	}
}