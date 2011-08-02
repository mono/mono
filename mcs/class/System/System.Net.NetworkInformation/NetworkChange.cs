//
// System.Net.NetworkInformation.NetworkChange
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// Copyright (c) 2006,2011 Novell, Inc. (http://www.novell.com)
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
	public sealed class NetworkChange {
		[Flags]
		enum EventType {
			Availability = 1 << 0,
			Address = 1 << 1,
		}

		static object _lock = new object ();
		static Socket nl_sock;
		static SocketAsyncEventArgs nl_args;
		static EventType pending_events;
		static Timer timer;

		static NetworkAddressChangedEventHandler AddressChanged;
		static NetworkAvailabilityChangedEventHandler AvailabilityChanged;

		private NetworkChange ()
		{
		}

		public static event NetworkAddressChangedEventHandler NetworkAddressChanged {
			add { Register (value); }
			remove { Unregister (value); }
		}

		public static event NetworkAvailabilityChangedEventHandler NetworkAvailabilityChanged {
			add { Register (value); }
			remove { Unregister (value); }
		}

		//internal Socket (AddressFamily family, SocketType type, ProtocolType proto, IntPtr sock)

		static bool EnsureSocket ()
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
		static void MaybeCloseSocket ()
		{
			if (nl_sock == null || AvailabilityChanged != null || AddressChanged != null)
				return;

			CloseNLSocket (nl_sock.Handle);
			GC.SuppressFinalize (nl_sock);
			nl_sock = null;
			nl_args = null;
		}

		static bool GetAvailability ()
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

		static void OnAvailabilityChanged (object unused)
		{
			NetworkAvailabilityChangedEventHandler d = AvailabilityChanged;
			d (null, new NetworkAvailabilityEventArgs (GetAvailability ()));
		}

		static void OnAddressChanged (object unused)
		{
			NetworkAddressChangedEventHandler d = AddressChanged;
			d (null, EventArgs.Empty);
		}

		static void OnEventDue (object unused)
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

		static void QueueEvent (EventType type)
		{
			lock (_lock) {
				if (timer == null)
					timer = new Timer (OnEventDue);
				if (pending_events == 0)
					timer.Change (150, -1);
				pending_events |= type;
			}
		}

		unsafe static void OnDataAvailable (object sender, SocketAsyncEventArgs args)
		{
			EventType type;
			fixed (byte *ptr = args.Buffer) {	
				type = ReadEvents (nl_sock.Handle, new IntPtr (ptr), args.BytesTransferred, 8192);
			}
			nl_sock.ReceiveAsync (nl_args);
			if (type != 0)
				QueueEvent (type);
		}

		static void Register (NetworkAddressChangedEventHandler d)
		{
			EnsureSocket ();
			AddressChanged += d;
		}

		static void Register (NetworkAvailabilityChangedEventHandler d)
		{
			EnsureSocket ();
			AvailabilityChanged += d;
		}

		static void Unregister (NetworkAddressChangedEventHandler d)
		{
			lock (_lock) {
				AddressChanged -= d;
				MaybeCloseSocket ();
			}
		}

		static void Unregister (NetworkAvailabilityChangedEventHandler d)
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

