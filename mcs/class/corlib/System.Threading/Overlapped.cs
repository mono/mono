//
// System.Threading.Overlapped.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//


namespace System.Threading
{
	public class Overlapped
	{
		[CLSCompliant(false)][MonoTODO]
		unsafe public static void Free(NativeOverlapped *nativeOverlappedPtr) {
			// FIXME
		}

		[CLSCompliant(false)][MonoTODO]
		unsafe public static Overlapped Unpack(NativeOverlapped *nativeOverlappedPtr) {
			// FIXME
			return(new Overlapped());
		}

		[MonoTODO]
		public Overlapped() {
			// FIXME
		}

		[MonoTODO]
		public Overlapped(int offsetLo, int offsetHi, int hEvent, IAsyncResult ar) {
			// FIXME
		}

		[MonoTODO]
		public IAsyncResult AsyncResult {
			get {
				// FIXME
				return(null);
			}
			
			set {
			}
		}

		[MonoTODO]
		public int EventHandle {
			get {
				// FIXME
				return(0);
			}
			
			set {
			}
		}

		[MonoTODO]
		public int OffsetHigh {
			get {
				// FIXME
				return(0);
			}
			
			set {
			}
		}

		[MonoTODO]
		public int OffsetLow {
			get {
				// FIXME
				return(0);
			}
			
			set {
			}
		}

		[CLSCompliant(false)][MonoTODO]
		unsafe public NativeOverlapped *Pack(IOCompletionCallback iocb) {
			// FIXME
			return(null);
		}
		
		[CLSCompliant(false)][MonoTODO]
		unsafe public NativeOverlapped *UnsafePack(IOCompletionCallback iocb) {
			// FIXME
			return(null);
		}
	}
}
