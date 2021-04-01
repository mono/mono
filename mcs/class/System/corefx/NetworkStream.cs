using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Sockets
{
	partial class NetworkStream
	{
		internal Socket InternalSocket {
			get {
				Socket chkSocket = _streamSocket;
				if (_cleanedUp || chkSocket == null)
					throw new ObjectDisposedException (GetType ().FullName);

				return chkSocket;
			}
		}
	}
}
