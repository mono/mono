//
// System.Net.NetworkInformation.NetworkChange
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//  Aaron Bockover (abock@xamarin.com)
//
// Copyright (c) 2006,2011 Novell, Inc. (http://www.novell.com)
// Copyright (c) 2013 Xamarin, Inc. (http://www.xamarin.com)
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

using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Net.NetworkInformation {
	internal interface INetworkChange {
		event NetworkAddressChangedEventHandler NetworkAddressChanged;
		event NetworkAvailabilityChangedEventHandler NetworkAvailabilityChanged;
	}

	public sealed class NetworkChange {
		static INetworkChange networkChange;

		static NetworkChange ()
		{
			if (MacNetworkChange.IsEnabled) {
				networkChange = new MacNetworkChange ();
			} else {
				networkChange = new LinuxNetworkChange ();
			}
		}

		public static event NetworkAddressChangedEventHandler NetworkAddressChanged {
			add { networkChange.NetworkAddressChanged += value; }
			remove { networkChange.NetworkAddressChanged -= value; }
		}

		public static event NetworkAvailabilityChangedEventHandler NetworkAvailabilityChanged {
			add { networkChange.NetworkAvailabilityChanged += value; }
			remove { networkChange.NetworkAvailabilityChanged -= value; }
		}
	}

	internal sealed class MacNetworkChange : INetworkChange {
		public static bool IsEnabled {
			get { return mono_sc_reachability_enabled () != 0; }
		}

		event NetworkAddressChangedEventHandler networkAddressChanged;
		event NetworkAvailabilityChangedEventHandler networkAvailabilityChanged;

		public event NetworkAddressChangedEventHandler NetworkAddressChanged {
			add {
				if (value != null) {
					MaybeInitialize ();
					networkAddressChanged += value;
					value (null, EventArgs.Empty);
				}
			}

			remove {
				networkAddressChanged -= value;
				MaybeDispose ();
			}
		}

		public event NetworkAvailabilityChangedEventHandler NetworkAvailabilityChanged {
			add {
				if (value != null) {
					MaybeInitialize ();
					networkAvailabilityChanged += value;
					var available = handle != IntPtr.Zero && mono_sc_reachability_is_available (handle) != 0;
					value (null, new NetworkAvailabilityEventArgs (available));
				}
			}

			remove {
				networkAvailabilityChanged -= value;
				MaybeDispose ();
			}
		}

		IntPtr handle;
		MonoSCReachabilityCallback callback;

		void Callback (int available)
		{
			var addressChanged = networkAddressChanged;
			if (addressChanged != null) {
				addressChanged (null, EventArgs.Empty);
			}

			var availabilityChanged = networkAvailabilityChanged;
			if (availabilityChanged != null) {
				availabilityChanged (null, new NetworkAvailabilityEventArgs (available != 0));
			}
		}

		void MaybeInitialize ()
		{
			lock (this) {
				if (handle == IntPtr.Zero) {
					callback = new MonoSCReachabilityCallback (Callback);
					handle = mono_sc_reachability_new (callback);
				}
			}
		}

		void MaybeDispose ()
		{
			lock (this) {
				var addressChanged = networkAddressChanged;
				var availabilityChanged = networkAvailabilityChanged;
				if (handle != IntPtr.Zero && addressChanged == null && availabilityChanged == null) {
					mono_sc_reachability_free (handle);
					handle = IntPtr.Zero;
				}
			}
		}

#if MONOTOUCH || MONODROID
		const string LIBNAME = "__Internal";
#else
		const string LIBNAME = "MonoPosixHelper";
#endif

		delegate void MonoSCReachabilityCallback (int available);

		[DllImport (LIBNAME)]
		static extern int mono_sc_reachability_enabled ();

		[DllImport (LIBNAME)]
		static extern IntPtr mono_sc_reachability_new (MonoSCReachabilityCallback callback);

		[DllImport (LIBNAME)]
		static extern void mono_sc_reachability_free (IntPtr handle);

		[DllImport (LIBNAME)]
		static extern int mono_sc_reachability_is_available (IntPtr handle);
	}

	internal sealed class LinuxNetworkChange : INetworkChange {
		[Flags]
		enum EventType {
			Availability = 1 << 0,
			Address = 1 << 1,
		}

		object _lock = new object ();
		Socket nl_sock;
		SocketAsyncEventArgs nl_args;
		EventType pending_events;
		Timer timer;

		NetworkAddressChangedEventHandler AddressChanged;
		NetworkAvailabilityChangedEventHandler AvailabilityChanged;

		public event NetworkAddressChangedEventHandler NetworkAddressChanged {
			add { Register (value); }
			remove { Unregister (value); }
		}

		public event NetworkAvailabilityChangedEventHandler NetworkAvailabilityChanged {
			add { Register (value); }
			remove { Unregister (value); }
		}

		//internal Socket (AddressFamily family, SocketType type, ProtocolType proto, IntPtr sock)

		bool EnsureSocket ()
		{
			lock (_lock) {
				if (nl_sock != null)
					return true;
				IntPtr fd = CreateNLSocket ();
				if (fd.ToInt64 () == -1)
					return false;

				nl_sock = new Socket (0, SocketType.Raw, ProtocolType.Udp, fd);
				nl_args = new SocketAsyncEventArgs ();
				nl_args.SetBuffer (new byte [8192], 0, 8192);
				nl_args.Completed += OnDataAvailable;
				nl_sock.ReceiveAsync (nl_args);
			}
			return true;
		}

		// _lock is held by the caller
		void MaybeCloseSocket ()
		{
			if (nl_sock == null || AvailabilityChanged != null || AddressChanged != null)
				return;

			CloseNLSocket (nl_sock.Handle);
			GC.SuppressFinalize (nl_sock);
			nl_sock = null;
			nl_args = null;
		}

		bool GetAvailability ()
		{
			NetworkInterface [] adapters = NetworkInterface.GetAllNetworkInterfaces ();
			foreach (NetworkInterface n in adapters) {
				// TODO: also check for a default route present?
				if (n.NetworkInterfaceType == NetworkInterfaceType.Loopback)
					continue;
				if (n.OperationalStatus == OperationalStatus.Up)
					return true;
			}
			return false;
		}

		void OnAvailabilityChanged (object unused)
		{
			NetworkAvailabilityChangedEventHandler d = AvailabilityChanged;
			d (null, new NetworkAvailabilityEventArgs (GetAvailability ()));
		}

		void OnAddressChanged (object unused)
		{
			NetworkAddressChangedEventHandler d = AddressChanged;
			d (null, EventArgs.Empty);
		}

		void OnEventDue (object unused)
		{
			EventType evts;
			lock (_lock) {
				evts = pending_events;
				pending_events = 0;
				timer.Change (-1, -1);
			}
			if ((evts & EventType.Availability) != 0)
				ThreadPool.QueueUserWorkItem (OnAvailabilityChanged);
			if ((evts & EventType.Address) != 0)
				ThreadPool.QueueUserWorkItem (OnAddressChanged);
		}

		void QueueEvent (EventType type)
		{
			lock (_lock) {
				if (timer == null)
					timer = new Timer (OnEventDue);
				if (pending_events == 0)
					timer.Change (150, -1);
				pending_events |= type;
			}
		}

		unsafe void OnDataAvailable (object sender, SocketAsyncEventArgs args)
		{
			EventType type;
			fixed (byte *ptr = args.Buffer) {	
				type = ReadEvents (nl_sock.Handle, new IntPtr (ptr), args.BytesTransferred, 8192);
			}
			nl_sock.ReceiveAsync (nl_args);
			if (type != 0)
				QueueEvent (type);
		}

		void Register (NetworkAddressChangedEventHandler d)
		{
			EnsureSocket ();
			AddressChanged += d;
		}

		void Register (NetworkAvailabilityChangedEventHandler d)
		{
			EnsureSocket ();
			AvailabilityChanged += d;
		}

		void Unregister (NetworkAddressChangedEventHandler d)
		{
			lock (_lock) {
				AddressChanged -= d;
				MaybeCloseSocket ();
			}
		}

		void Unregister (NetworkAvailabilityChangedEventHandler d)
		{
			lock (_lock) {
				AvailabilityChanged -= d;
				MaybeCloseSocket ();
			}
		}

#if MONOTOUCH || MONODROID
		const string LIBNAME = "__Internal";
#else
		const string LIBNAME = "MonoPosixHelper";
#endif

		[DllImport (LIBNAME, CallingConvention=CallingConvention.Cdecl)]
		static extern IntPtr CreateNLSocket ();

		[DllImport (LIBNAME, CallingConvention=CallingConvention.Cdecl)]
		static extern EventType ReadEvents (IntPtr sock, IntPtr buffer, int count, int size);

		[DllImport (LIBNAME, CallingConvention=CallingConvention.Cdecl)]
		static extern IntPtr CloseNLSocket (IntPtr sock);
	}
}

