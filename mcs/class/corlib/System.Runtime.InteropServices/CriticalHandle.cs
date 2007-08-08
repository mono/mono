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

		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.MayFail)]
		protected CriticalHandle (IntPtr invalidHandleValue)
		{
			handle = invalidHandleValue;
		}

		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		~CriticalHandle ()
		{
			Dispose (false);
		}

		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public void Close ()
		{
			Dispose (true);
		}

		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public void Dispose ()
		{
			Dispose (true);
		}

		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		protected virtual void Dispose (bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;
			if (IsInvalid)
				return;

			if (disposing == true && !IsInvalid){
				if (!ReleaseHandle ()) {
					GC.SuppressFinalize (this);
				} else {
					// Failed in release...
				}
			}
		}

		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		protected abstract bool ReleaseHandle ();

		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		protected void SetHandle (IntPtr handle)
		{
			this.handle = handle;
		}

		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public void SetHandleAsInvalid()
		{
			_disposed = true;
		}

		public bool IsClosed {
			[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
			get { return _disposed; }
		}

		public abstract bool IsInvalid {
			[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
			get;
		}
	}
}
#endif
