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

		[ReliabilityContract (Consistency.WillNotCorruptState, CER.Success)]
		public void Close ()
		{
			Dispose ();
		}

		[ReliabilityContract (Consistency.WillNotCorruptState, CER.Success)]
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

		[ReliabilityContract (Consistency.WillNotCorruptState, CER.Success)]
		protected abstract bool ReleaseHandle ();

		[ReliabilityContract (Consistency.WillNotCorruptState, CER.Success)]
		protected void SetHandle (IntPtr handle)
		{
			this.handle = handle;
		}

		[ReliabilityContract (Consistency.WillNotCorruptState, CER.Success)]
		public void SetHandleAsInvalid()
		{
			_disposed = true;
		}

		public bool IsClosed {
			[ReliabilityContract (Consistency.WillNotCorruptState, CER.Success)]
			get { return _disposed; }
		}

		public abstract bool IsInvalid {
			[ReliabilityContract (Consistency.WillNotCorruptState, CER.Success)]
			get;
		}
	}
}
#endif