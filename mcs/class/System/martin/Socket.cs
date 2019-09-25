using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Sockets
{
	partial class Socket
	{
#if MARTIN_FIXME
		// _rightEndPoint is null if the socket has not been bound.  Otherwise, it is any EndPoint of the
		// correct type (IPEndPoint, etc).
		internal EndPoint _rightEndPoint;
		internal EndPoint _remoteEndPoint;

		internal void SetToConnected ()
		{
		}

		internal void SetToDisconnected ()
		{
		}

		internal SocketError ReplaceHandle ()
		{
			throw new NotImplementedException ();
		}

		// CreateAcceptSocket - pulls unmanaged results and assembles them into a new Socket object.
		internal Socket CreateAcceptSocket (SafeCloseSocket fd, EndPoint remoteEP)
		{
			throw new NotImplementedException ();
		}

		internal Socket UpdateAcceptSocket (Socket socket, EndPoint remoteEP)
		{
			throw new NotImplementedException ();
		}

		internal static void GetIPProtocolInformation (AddressFamily addressFamily, Internals.SocketAddress socketAddress, out bool isIPv4, out bool isIPv6)
		{
			bool isIPv4MappedToIPv6 = socketAddress.Family == AddressFamily.InterNetworkV6 && socketAddress.GetIPAddress ().IsIPv4MappedToIPv6;
			isIPv4 = addressFamily == AddressFamily.InterNetwork || isIPv4MappedToIPv6; // DualMode
			isIPv6 = addressFamily == AddressFamily.InterNetworkV6;
		}
#else
#if MONO_WEB_DEBUG
		static int nextId;
		internal readonly int ID = ++nextId;
#else
		internal readonly int ID;
#endif

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

		internal int ReceiveFrom (byte [] buffer, int offset, int size, SocketFlags socketFlags, ref EndPoint remoteEP, out SocketError errorCode)
		{
			throw new NotImplementedException ();
		}

#endif
	}
}
