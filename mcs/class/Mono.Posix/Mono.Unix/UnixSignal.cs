//
// Mono.Unix/UnixSignal.cs
//
// Authors:
//   Jonathan Pryor (jpryor@novell.com)
//   Tim Jenks (tim.jenks@realtimeworlds.com)
//
// (C) 2008 Novell, Inc.
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

using System;
using System.Runtime.InteropServices;
using System.Threading;

using Mono.Unix.Native;

namespace Mono.Unix {

	public class UnixSignal : WaitHandle {
		private int signum;
		private IntPtr signal_info;

		public UnixSignal (Signum signum)
		{
			this.signum = NativeConvert.FromSignum (signum);
			this.signal_info = install (this.signum);
			if (this.signal_info == IntPtr.Zero) {
				throw new ArgumentException ("Unable to handle signal", "signum");
			}
		}

		public UnixSignal (Mono.Unix.Native.RealTimeSignum rtsig)
		{
			signum = NativeConvert.FromRealTimeSignum (rtsig);
			this.signal_info = install (this.signum);
			Native.Errno err = Native.Stdlib.GetLastError ();
			if (this.signal_info == IntPtr.Zero) {
				if (err == Native.Errno.EADDRINUSE)
					throw new ArgumentException ("Signal registered outside of Mono.Posix", "signum");
				throw new ArgumentException ("Unable to handle signal", "signum");
			}
		}

		public Signum Signum {
			get {
				if (IsRealTimeSignal)
					throw new InvalidOperationException ("This signal is a RealTimeSignum");
				return NativeConvert.ToSignum (signum); 
			}
		}

		public RealTimeSignum RealTimeSignum {
			get {
				if (!IsRealTimeSignal)
					throw new InvalidOperationException ("This signal is not a RealTimeSignum");
				return NativeConvert.ToRealTimeSignum (signum-GetSIGRTMIN ());
			}
		}

		public bool IsRealTimeSignal {
			get {
				AssertValid ();
				int sigrtmin = GetSIGRTMIN ();
				if (sigrtmin == -1)
					return false;
				return signum >= sigrtmin;
			}
		}

		[DllImport (Stdlib.MPH, CallingConvention=CallingConvention.Cdecl,
				EntryPoint="Mono_Unix_UnixSignal_install", SetLastError=true)]
		private static extern IntPtr install (int signum);

		[DllImport (Stdlib.MPH, CallingConvention=CallingConvention.Cdecl,
				EntryPoint="Mono_Unix_UnixSignal_uninstall")]
		private static extern int uninstall (IntPtr info);

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate int Mono_Posix_RuntimeIsShuttingDown ();

		[DllImport (Stdlib.MPH, CallingConvention=CallingConvention.Cdecl,
				EntryPoint="Mono_Unix_UnixSignal_WaitAny")]
		private static extern int WaitAny (IntPtr[] infos, int count, int timeout, Mono_Posix_RuntimeIsShuttingDown shutting_down);

		[DllImport (Stdlib.MPH, CallingConvention=CallingConvention.Cdecl,
                                EntryPoint="Mono_Posix_SIGRTMIN")]
		internal static extern int GetSIGRTMIN ();

		[DllImport (Stdlib.MPH, CallingConvention=CallingConvention.Cdecl,
                                EntryPoint="Mono_Posix_SIGRTMAX")]
		internal static extern int GetSIGRTMAX ();

		private void AssertValid ()
		{
			if (signal_info == IntPtr.Zero)
				throw new ObjectDisposedException (GetType().FullName);
		}

		private unsafe SignalInfo* Info {
			get {
				AssertValid ();
				return (SignalInfo*) signal_info;
			}
		}

		public bool IsSet {
			get {
				return Count > 0;
			}
		}

		public unsafe bool Reset ()
		{
			int n = Interlocked.Exchange (ref Info->count, 0);
			return n != 0;
		}

		public unsafe int Count {
			get {return Info->count;}
			set {Interlocked.Exchange (ref Info->count, value);}
		}

		[Map]
		struct SignalInfo {
			public int signum, count, read_fd, write_fd, have_handler, pipecnt;
			public IntPtr handler;
		}

		#region WaitHandle overrides
		protected unsafe override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			if (signal_info == IntPtr.Zero)
				return;
			uninstall (signal_info);
			signal_info = IntPtr.Zero;
		}

		public override bool WaitOne ()
		{
			return WaitOne (-1, false);
		}

		public override bool WaitOne (TimeSpan timeout, bool exitContext)
		{
			long ms = (long) timeout.TotalMilliseconds;
			if (ms < -1 || ms > Int32.MaxValue)
				throw new ArgumentOutOfRangeException ("timeout");
			return WaitOne ((int) ms, exitContext);
		}

		public override bool WaitOne (int millisecondsTimeout, bool exitContext)
		{
			AssertValid ();
			if (exitContext)
				throw new InvalidOperationException ("exitContext is not supported");
			return WaitAny (new UnixSignal[]{this}, millisecondsTimeout) == 0;
		}
		#endregion

		public static int WaitAny (UnixSignal[] signals)
		{
			return WaitAny (signals, -1);
		}

		public static int WaitAny (UnixSignal[] signals, TimeSpan timeout)
		{
			long ms = (long) timeout.TotalMilliseconds;
			if (ms < -1 || ms > Int32.MaxValue)
				throw new ArgumentOutOfRangeException ("timeout");
			return WaitAny (signals, (int) ms);
		}

		public static unsafe int WaitAny (UnixSignal[] signals, int millisecondsTimeout)
		{
			if (signals == null)
				throw new ArgumentNullException ("signals");
			if (millisecondsTimeout < -1)
				throw new ArgumentOutOfRangeException ("millisecondsTimeout");
			IntPtr[] infos = new IntPtr [signals.Length];
			for (int i = 0; i < signals.Length; ++i) {
				infos [i] = signals [i].signal_info;
				if (infos [i] == IntPtr.Zero)
					throw new InvalidOperationException ("Disposed UnixSignal");
			}
			return WaitAny (infos, infos.Length, millisecondsTimeout, () => Environment.HasShutdownStarted ? 1 : 0);
		}
	}
}

