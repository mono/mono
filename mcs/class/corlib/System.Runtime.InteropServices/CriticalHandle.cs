//
// System.Runtime.InteropServices.CriticalHandle
//
// Author:
//   Kazuki Oikawa  (kazuki@panicode.com)
//

#if NET_2_0

using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Serialization;

namespace System.Runtime.InteropServices
{
	public abstract class CriticalHandle : CriticalFinalizerObject, IDisposable
	{
		protected IntPtr handle;
		bool _disposed = false;

		protected CriticalHandle (IntPtr invalidHandleValue)
		{
			handle = invalidHandleValue;
		}

		~CriticalHandle ()
		{
			Dispose ();
		}

		public void Close ()
		{
			Dispose ();
		}

		public void Dispose ()
		{
			if (_disposed)
				return;

			_disposed = true;
			if (IsInvalid)
				return;

			if (ReleaseHandle ()) {
				GC.SuppressFinalize (this);
			} else {
				// Failed in release...
			}
		}

		protected abstract bool ReleaseHandle ();

		protected void SetHandle (IntPtr handle)
		{
			this.handle = handle;
		}

		public void SetHandleAsInvalid()
		{
			_disposed = true;
		}

		public bool IsClosed {
			get { return _disposed; }
		}

		public abstract bool IsInvalid {get;}
	}
}
#endif