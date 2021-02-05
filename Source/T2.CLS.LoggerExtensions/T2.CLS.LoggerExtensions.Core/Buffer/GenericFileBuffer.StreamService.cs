// Copyright (C) 2019 Topsoft (https://topsoft.by)

using T2.CLS.LoggerExtensions.Core.Interface;

namespace T2.CLS.LoggerExtensions.Core.Buffer
{
	public abstract partial class GenericFileBuffer<TItem>
	{
		#region Properties

		public virtual IStreamService StreamService
		{
			get { return _streamService ??= new FileStreamService(_encoding); }
		}

		#endregion
	}
}