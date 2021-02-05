// Copyright (C) 2019 Topsoft (https://topsoft.by)

using T2.CLS.LoggerExtensions.Core.Interface;

namespace T2.CLS.LoggerExtensions.Core
{
	public abstract class TransportBase : ILogTransport
	{
		#region Fields

		private bool _disposed;

		#endregion

		#region  Methods

		protected virtual void DisposeCore(bool disposing)
		{
		}

		protected abstract void SendCore(LogEvent logEvent);

		#endregion

		#region Interface Implementations

		#region IDisposable

		public void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;

			DisposeCore(true);
		}

		#endregion

		#region ILogTransport

		public void Send(LogEvent logEvent)
		{
			SendCore(logEvent);
		}

		#endregion

		#endregion
	}
}