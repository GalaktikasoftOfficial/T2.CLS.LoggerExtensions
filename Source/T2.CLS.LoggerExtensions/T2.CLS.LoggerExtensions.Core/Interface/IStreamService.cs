// Copyright (C) 2019 Topsoft (https://topsoft.by)

using System.IO;

namespace T2.CLS.LoggerExtensions.Core.Interface
{
	public interface IStreamService
	{
		#region Methods

		void Delete(string path);

		bool IsStreamExist(string path);

		StreamReader OpenReadStream(string path, FileMode mode, FileShare share);

		StreamWriter OpenWriteStream(string path, FileMode mode, FileAccess access, FileShare share);

		#endregion
	}
}