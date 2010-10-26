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

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Runtime.InteropServices;
#if !DISABLE_SECURITY
using System.Security.Permissions;
#endif

namespace System.Threading
{
#if NET_2_0
	[ComVisible (true)]
#endif
	public class Overlapped
	{
		IAsyncResult ares;
		int offsetL;
		int offsetH;
		int evt;

#if NET_2_0
		IntPtr evt_ptr;
#endif

		public Overlapped ()
		{
		}

#if NET_2_0
		[Obsolete ("Not 64bit compatible.  Please use the constructor that takes IntPtr for the event handle", false)]
#endif
		public Overlapped(int offsetLo, int offsetHi, int hEvent, IAsyncResult ar)
		{
			offsetL = offsetLo;
			offsetH = offsetHi;
			evt = hEvent;
			ares = ar;
		}

#if NET_2_0
		public Overlapped (int offsetLo, int offsetHi, IntPtr hEvent,
				   IAsyncResult ar)
		{
			offsetL = offsetLo;
			offsetH = offsetHi;
			evt_ptr = hEvent;
			ares = ar;
		}
#endif

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
#if NET_2_0
			result.evt = (int)nativeOverlappedPtr->EventHandle;
#else
			result.evt = nativeOverlappedPtr->EventHandle;
#endif
			return result;
		}

		[CLSCompliant(false)]
#if NET_2_0
		[Obsolete ("Use Pack(iocb, userData) instead", false)]
#endif
		[MonoTODO ("Security - we need to propagate the call stack")]
		unsafe public NativeOverlapped *Pack (IOCompletionCallback iocb)
		{
			NativeOverlapped *result = (NativeOverlapped *) Marshal.AllocHGlobal (Marshal.SizeOf (typeof (NativeOverlapped)));
			result->OffsetLow = offsetL;
			result->OffsetHigh = offsetH;
#if NET_2_0
			result->EventHandle = (IntPtr)evt;
#else
			result->EventHandle = evt;
#endif
			return result;
		}

#if NET_2_0
		[CLSCompliant (false)]
		[ComVisible (false)]
		[MonoTODO ("handle userData")]
		unsafe public NativeOverlapped *Pack (IOCompletionCallback iocb, object userData)
		{
			NativeOverlapped *result = (NativeOverlapped *) Marshal.AllocHGlobal (Marshal.SizeOf(typeof(NativeOverlapped)));
			result->OffsetLow = offsetL;
			result->OffsetHigh = offsetH;
			result->EventHandle = evt_ptr;
			return(result);
		}
#endif
		
		[CLSCompliant(false)]
#if NET_2_0
		[Obsolete ("Use UnsafePack(iocb, userData) instead", false)]
#endif
#if !DISABLE_SECURITY
		[SecurityPermission (SecurityAction.Demand, ControlEvidence=true, ControlPolicy=true)]
#endif
		unsafe public NativeOverlapped *UnsafePack (IOCompletionCallback iocb)
		{
			// no need to propagate the call stack in the unsafe version
			return Pack (iocb);
		}

#if NET_2_0
		[ComVisible (false)]
		[CLSCompliant (false)]
		unsafe public NativeOverlapped *UnsafePack (IOCompletionCallback iocb, object userData)
		{
			return Pack (iocb, userData);
		}
#endif

		public IAsyncResult AsyncResult {
			get { return ares; }
			set { ares = value; }
		}

#if NET_2_0
		[Obsolete ("Not 64bit compatible.  Use EventHandleIntPtr instead.", false)]
#endif
		public int EventHandle {
			get { return evt; }
			set { evt = value; }
		}

#if NET_2_0
		[ComVisible (false)]
		public IntPtr EventHandleIntPtr 
		{
			get{
				return(evt_ptr);
			}
			set{
				evt_ptr = value;
			}
		}
#endif

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

