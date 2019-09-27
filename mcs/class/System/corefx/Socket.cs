namespace System.Net.Sockets
{
	partial class Socket
	{
#if MONO_WEB_DEBUG
		static int nextId;
		internal readonly int ID = ++nextId;
#else
		internal readonly int ID;
#endif

		// called by System.Net/Dns.cs
		internal static int FamilyHint {
			get {
				// Returns one of
				//	MONO_HINT_UNSPECIFIED		= 0,
				//	MONO_HINT_IPV4				= 1,
				//	MONO_HINT_IPV6				= 2,

				int hint = 0;
				if (OSSupportsIPv4) {
					hint = 1;
				}

				if (OSSupportsIPv6) {
					hint = hint == 0 ? 2 : 0;
				}

				return hint;
			}
		}

		// private constructor used by System.Net.NetworkInformation/NetworkChange.cs
		internal static Socket CreateFromFileDescriptor (IntPtr fd, AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
		{
			var safeHandle = SafeCloseSocket.CreateSocket (fd);
			return new Socket (addressFamily, socketType, protocolType, safeHandle);
		}

		Socket (AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, SafeCloseSocket safeHandle)
		{
			InitializeSockets ();

			_handle = safeHandle;
			_addressFamily = addressFamily;
			_socketType = socketType;
			_protocolType = protocolType;
			_isConnected = true;
		}
	}
}
