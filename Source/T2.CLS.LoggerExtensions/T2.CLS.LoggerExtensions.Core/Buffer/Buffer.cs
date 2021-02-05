// Copyright (C) 2019 Topsoft (https://topsoft.by)

using System;
using T2.CLS.LoggerExtensions.Core.Interface;

namespace T2.CLS.LoggerExtensions.Core.Buffer
{
	public abstract class Buffer : IDisposable
	{
		#region Ctors

		protected Buffer(ITextFormatter textFormatter)
		{
			TextFormatter = textFormatter;
		}

		#endregion

		#region Properties

		public ITextFormatter TextFormatter { get; }

		#endregion

		#region  Methods

		protected abstract void DisposeCore(bool disposing);

		public abstract void Write(LogEvent logEvent);

		#endregion

		#region Interface Implementations

		#region IDisposable

		public void Dispose()
		{
			DisposeCore(true);
		}

		#endregion

		#endregion
	}
}