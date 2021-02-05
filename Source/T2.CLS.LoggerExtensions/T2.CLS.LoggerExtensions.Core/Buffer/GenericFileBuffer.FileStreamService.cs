// Copyright (C) 2019 Topsoft (https://topsoft.by)

using System.IO;
using System.Text;
using T2.CLS.LoggerExtensions.Core.Interface;

namespace T2.CLS.LoggerExtensions.Core.Buffer
{
	public abstract partial class GenericFileBuffer<TItem>
	{
		#region Nested Types

		private sealed class FileStreamService : IStreamService
		{
			#region Static Fields and Constants

			//public static readonly IStreamService Instance = new FileStreamService(Encoding.UTF8);
			private readonly Encoding _encoding;

			#endregion

			#region Ctors

			public FileStreamService(Encoding encoding)
			{
				_encoding = encoding;
			}

			#endregion

			#region Interface Implementations

			#region IStreamService

			public bool IsStreamExist(string path)
			{
				return File.Exists(path);
			}

			public StreamReader OpenReadStream(string path, FileMode mode, FileShare share)
			{
				return _encoding == null
					? new StreamReader(File.Open(path, mode, FileAccess.Read, share))
					: new StreamReader(File.Open(path, mode, FileAccess.Read, share), _encoding);
			}

			public StreamWriter OpenWriteStream(string path, FileMode mode, FileAccess access, FileShare share)
			{
				return _encoding == null
					? new StreamWriter(File.Open(path, mode, access, share))
					: new StreamWriter(File.Open(path, mode, access, share), _encoding);
			}

			public void Delete(string path)
			{
				File.Delete(path);
			}

			#endregion

			#endregion
		}

		#endregion
	}
}