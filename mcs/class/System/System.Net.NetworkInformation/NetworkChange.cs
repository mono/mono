//
// System.Net.NetworkInformation.NetworkChange
//
// Authors:
//   Gonzalo Paniagua Javier (LinuxNetworkChange) (gonzalo@novell.com)
//   Aaron Bockover (MacNetworkChange) (abock@xamarin.com)
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

using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

#if NETWORK_CHANGE_STANDALONE
namespace NetworkInformation {

	public class NetworkAvailabilityEventArgs : EventArgs
	{
		public bool IsAvailable { get; set; }

		public NetworkAvailabilityEventArgs (bool available)
		{
			IsAvailable = available;
		}
	}

	public delegate void NetworkAddressChangedEventHandler (object sender, EventArgs args);
	public delegate void NetworkAvailabilityChangedEventHandler (object sender, NetworkAvailabilityEventArgs args);
#else
namespace System.Net.NetworkInformation {
#endif

	internal interface INetworkChange : IDisposable {
		event NetworkAddressChangedEventHandler NetworkAddressChanged;
		event NetworkAvailabilityChangedEventHandler NetworkAvailabilityChanged;
		bool HasRegisteredEvents { get; }
	}

	public sealed class NetworkChange {
		static INetworkChange networkChange;

		public static event NetworkAddressChangedEventHandler NetworkAddressChanged {
			add {
				lock (typeof (INetworkChange)) {
					MaybeCreate ();
					if (networkChange != null)
						networkChange.NetworkAddressChanged += value;
				}
			}

			remove {
				lock (typeof (INetworkChange)) {
					if (networkChange != null) {
						networkChange.NetworkAddressChanged -= value;
						MaybeDispose ();
					}
				}
			}
		}

		public static event NetworkAvailabilityChangedEventHandler NetworkAvailabilityChanged {
			add {
				lock (typeof (INetworkChange)) {
					MaybeCreate ();
					if (networkChange != null)
						networkChange.NetworkAvailabilityChanged += value;
				}
			}

			remove {
				lock (typeof (INetworkChange)) {
					if (networkChange != null) {
						networkChange.NetworkAvailabilityChanged -= value;
						MaybeDispose ();
					}
				}
			}
		}

		static void MaybeCreate ()
		{
#if MONOTOUCH_WATCH || ORBIS
			throw new PlatformNotSupportedException ("NetworkInformation.NetworkChange is not supported on the current platform.");
#else
			if (networkChange != null)
				return;

			try {
				networkChange = new MacNetworkChange ();
			} catch {
#if !NETWORK_CHANGE_STANDALONE && !MONOTOUCH
				networkChange = new LinuxNetworkChange ();
#endif
			}
#endif // MONOTOUCH_WATCH
		}

		static void MaybeDispose ()
		{
			if (networkChange != null && networkChange.HasRegisteredEvents) {
				networkChange.Dispose ();
				networkChange = null;
			}
		}
	}

#if !MONOTOUCH_WATCH && !ORBIS
	internal sealed class MacNetworkChange : INetworkChange
	{
		const string DL_LIB = "/usr/lib/libSystem.dylib";
		const string CORE_SERVICES_LIB = "/System/Library/Frameworks/SystemConfiguration.framework/SystemConfiguration";
		const string CORE_FOUNDATION_LIB = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

		[UnmanagedFunctionPointerAttribute (CallingConvention.Cdecl)]
		delegate void SCNetworkReachabilityCallback (IntPtr target, NetworkReachabilityFlags flags, IntPtr info);

		[DllImport (DL_LIB)]
		static extern IntPtr dlopen (string path, int mode);

		[DllImport (DL_LIB)]
		static extern IntPtr dlsym (IntPtr handle, string symbol);

		[DllImport (DL_LIB)]
		static extern int dlclose (IntPtr handle);

		[DllImport (CORE_FOUNDATION_LIB)]
		static extern void CFRelease (IntPtr handle);

		[DllImport (CORE_FOUNDATION_LIB)]
		static extern IntPtr CFRunLoopGetMain ();

		[DllImport (CORE_SERVICES_LIB)]
		static extern IntPtr SCNetworkReachabilityCreateWithAddress (IntPtr allocator, ref sockaddr_in sockaddr);

		[DllImport (CORE_SERVICES_LIB)]
		static extern bool SCNetworkReachabilityGetFlags (IntPtr reachability, out NetworkReachabilityFlags flags);

		[DllImport (CORE_SERVICES_LIB)]
		static extern bool SCNetworkReachabilitySetCallback (IntPtr reachability, SCNetworkReachabilityCallback callback, ref SCNetworkReachabilityContext context);

		[DllImport (CORE_SERVICES_LIB)]
		static extern bool SCNetworkReachabilityScheduleWithRunLoop (IntPtr reachability, IntPtr runLoop, IntPtr runLoopMode);

		[DllImport (CORE_SERVICES_LIB)]
		static extern bool SCNetworkReachabilityUnscheduleFromRunLoop (IntPtr reachability, IntPtr runLoop, IntPtr runLoopMode);

		[StructLayout (LayoutKind.Explicit, Size = 28)]
		struct sockaddr_in {
			[FieldOffset (0)] public byte sin_len;
			[FieldOffset (1)] public byte sin_family;

			public static sockaddr_in Create ()
			{
				return new sockaddr_in {
					sin_len = 28,
					sin_family = 2 // AF_INET
				};
			}
		}

		[StructLayout (LayoutKind.Sequential)]
		struct SCNetworkReachabilityContext {
			public IntPtr version;
			public IntPtr info;
			public IntPtr retain;
			public IntPtr release;
			public IntPtr copyDescription;
		}

		[Flags]
		enum NetworkReachabilityFlags {
			None = 0,
			TransientConnection = 1 << 0,
			Reachable = 1 << 1,
			ConnectionRequired = 1 << 2,
			ConnectionOnTraffic = 1 << 3,
			InterventionRequired = 1 << 4,
			ConnectionOnDemand = 1 << 5,
			IsLocalAddress = 1 << 16,
			IsDirect = 1 << 17,
			IsWWAN = 1 << 18,
			ConnectionAutomatic = ConnectionOnTraffic
		}

		IntPtr handle;
		IntPtr runLoopMode;
		SCNetworkReachabilityCallback callback;
		bool scheduledWithRunLoop;
		NetworkReachabilityFlags flags;

		event NetworkAddressChangedEventHandler networkAddressChanged;
		event NetworkAvailabilityChangedEventHandler networkAvailabilityChanged;

		public event NetworkAddressChangedEventHandler NetworkAddressChanged {
			add {
				value (null, EventArgs.Empty);
				networkAddressChanged += value;
			}

			remove { networkAddressChanged -= value; }
		}

		public event NetworkAvailabilityChangedEventHandler NetworkAvailabilityChanged {
			add {
				value (null, new NetworkAvailabilityEventArgs (IsAvailable));
				networkAvailabilityChanged += value;
			}

			remove { networkAvailabilityChanged -= value; }
		}

		bool IsAvailable {
			get {
				return (flags & NetworkReachabilityFlags.Reachable) != 0 &&
					(flags & NetworkReachabilityFlags.ConnectionRequired) == 0;
			}
		}

		public bool HasRegisteredEvents {
			get { return networkAddressChanged != null || networkAvailabilityChanged != null; }
		}

		public MacNetworkChange ()
		{
			var sockaddr = sockaddr_in.Create ();
			handle = SCNetworkReachabilityCreateWithAddress (IntPtr.Zero, ref sockaddr);
			if (handle == IntPtr.Zero)
				throw new Exception ("SCNetworkReachabilityCreateWithAddress returned NULL");

			callback = new SCNetworkReachabilityCallback (HandleCallback);
			var info = new SCNetworkReachabilityContext {
				info = GCHandle.ToIntPtr (GCHandle.Alloc (this))
			};

			SCNetworkReachabilitySetCallback (handle, callback, ref info);

			scheduledWithRunLoop =
			LoadRunLoopMode () &&
				SCNetworkReachabilityScheduleWithRunLoop (handle, CFRunLoopGetMain (), runLoopMode);

			SCNetworkReachabilityGetFlags (handle, out flags);
		}

		bool LoadRunLoopMode ()
		{
			var cfLibHandle = dlopen (CORE_FOUNDATION_LIB, 0);
			if (cfLibHandle == IntPtr.Zero)
				return false;

			try {
				runLoopMode = dlsym (cfLibHandle, "kCFRunLoopDefaultMode");
				if (runLoopMode != IntPtr.Zero) {
					runLoopMode = Marshal.ReadIntPtr (runLoopMode);
					return runLoopMode != IntPtr.Zero;
				}
			} finally {
				dlclose (cfLibHandle);
			}

			return false;
		}

		public void Dispose ()
		{
			lock (this) {
				if (handle == IntPtr.Zero)
					return;

				if (scheduledWithRunLoop)
					SCNetworkReachabilityUnscheduleFromRunLoop (handle, CFRunLoopGetMain (), runLoopMode);

				CFRelease (handle);
				handle = IntPtr.Zero;
				callback = null;
				flags = NetworkReachabilityFlags.None;
				scheduledWithRunLoop = false;
			}
		}

		[Mono.Util.MonoPInvokeCallback (typeof (SCNetworkReachabilityCallback))]
		static void HandleCallback (IntPtr reachability, NetworkReachabilityFlags flags, IntPtr info)
		{
			if (info == IntPtr.Zero)
				return;

			var instance = GCHandle.FromIntPtr (info).Target as MacNetworkChange;
			if (instance == null || instance.flags == flags)
				return;

			instance.flags = flags;

			var addressChanged = instance.networkAddressChanged;
			if (addressChanged != null)
				addressChanged (null, EventArgs.Empty);

			var availabilityChanged = instance.networkAvailabilityChanged;
			if (availabilityChanged != null)
				availabilityChanged (null, new NetworkAvailabilityEventArgs (instance.IsAvailable));
		}
	}
#endif // !MONOTOUCH_WATCH

#if !NETWORK_CHANGE_STANDALONE && !MONOTOUCH && !ORBIS

	internal sealed class LinuxNetworkChange : INetworkChange {
		[Flags]
		enum EventType : int {
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

		public bool HasRegisteredEvents {
			get { return AddressChanged != null || AvailabilityChanged != null; }
		}

		public void Dispose ()
		{
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

				var safeHandle = new SafeSocketHandle (fd, true);

				nl_sock = new Socket (0, SocketType.Raw, ProtocolType.Udp, safeHandle);
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
			if (d != null)
				d (null, new NetworkAvailabilityEventArgs (GetAvailability ()));
		}

		void OnAddressChanged (object unused)
		{
			NetworkAddressChangedEventHandler d = AddressChanged;
			if (d != null)
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
			if (nl_sock == null) // Recent changes in Mono cause MaybeCloseSocket to be called before OnDataAvailable
				return;
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

#if MONODROID
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern IntPtr CreateNLSocket ();

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern EventType ReadEvents (IntPtr sock, IntPtr buffer, int count, int size);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern IntPtr CloseNLSocket (IntPtr sock);
#else
		[DllImport ("MonoPosixHelper", CallingConvention=CallingConvention.Cdecl)]
		static extern IntPtr CreateNLSocket ();

		[DllImport ("MonoPosixHelper", CallingConvention=CallingConvention.Cdecl)]
		static extern EventType ReadEvents (IntPtr sock, IntPtr buffer, int count, int size);

		[DllImport ("MonoPosixHelper", CallingConvention=CallingConvention.Cdecl)]
		static extern IntPtr CloseNLSocket (IntPtr sock);
#endif
	}

#endif

}
