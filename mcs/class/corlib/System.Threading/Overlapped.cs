//
// System.Threading.Overlapped.cs
//
// Authors:
// 	Dick Porter (dick@ximian.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com);
//
// (C) Ximian, Inc.  http://www.ximian.com
// (C) 2004 Novell, Inc. (http://www.novell.com)
//

using System.Runtime.InteropServices;

namespace System.Threading
{
	public class Overlapped
	{
		IAsyncResult ares;
		int offsetL;
		int offsetH;
		int evt;

		public Overlapped ()
		{
		}

		public Overlapped(int offsetLo, int offsetHi, int hEvent, IAsyncResult ar)
		{
			offsetL = offsetLo;
			offsetH = offsetHi;
			evt = hEvent;
			ares = ar;
		}

		[CLSCompliant(false)]
		unsafe public static void Free (NativeOverlapped *nativeOverlappedPtr)
		{
			if ((IntPtr) nativeOverlappedPtr == IntPtr.Zero)
				throw new ArgumentNullException ("nativeOverlappedPtr");

			Marshal.FreeHGlobal ((IntPtr) nativeOverlappedPtr);
		}

		[CLSCompliant(false)]
		unsafe public static Overlapped Unpack (NativeOverlapped *nativeOverlappedPtr)
		{
			if ((IntPtr) nativeOverlappedPtr == IntPtr.Zero)
				throw new ArgumentNullException ("nativeOverlappedPtr");

			Overlapped result = new Overlapped ();
			result.offsetL = nativeOverlappedPtr->OffsetLow;
			result.offsetH = nativeOverlappedPtr->OffsetHigh;
			result.evt = nativeOverlappedPtr->EventHandle;
			return result;
		}

		[CLSCompliant(false)]
		unsafe public NativeOverlapped *Pack (IOCompletionCallback iocb)
		{
			NativeOverlapped *result = (NativeOverlapped *) Marshal.AllocHGlobal (Marshal.SizeOf (typeof (NativeOverlapped)));
			result->OffsetLow = offsetL;
			result->OffsetHigh = offsetH;
			result->EventHandle = evt;
			return result;
		}
		
		[CLSCompliant(false)]
		unsafe public NativeOverlapped *UnsafePack (IOCompletionCallback iocb)
		{
			return Pack (iocb);
		}

		public IAsyncResult AsyncResult {
			get { return ares; }
			set { ares = value; }
		}

		public int EventHandle {
			get { return evt; }
			set { evt = value; }
		}

		public int OffsetHigh {
			get { return offsetH; }
			set { offsetH = value; }
		}

		public int OffsetLow {
			get { return offsetL; }
			set { offsetL = value; }
		}
	}
}

