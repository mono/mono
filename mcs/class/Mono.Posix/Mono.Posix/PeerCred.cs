//
// Mono.Posix.PeerCred: Peer credentials class for AF_UNIX sockets
//
// Authors:
//	Dick Porter (dick@ximian.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
//

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

namespace Mono.Posix
{
	internal struct PeerCredData {
		public int pid;
		public int uid;
		public int gid;
	}

	[Obsolete ("Use Mono.Unix.PeerCred")]
	public class PeerCred
	{
		/* Make sure this doesn't clash with anything in
		 * SocketOptionName, and keep it synchronised with the
		 * runtime
		 */
		private const int so_peercred=10001;
		private PeerCredData data;
		
		public PeerCred (Socket sock) {
			if (sock.AddressFamily != AddressFamily.Unix) {
				throw new ArgumentException ("Only Unix sockets are supported", "sock");
			}

			data = (PeerCredData)sock.GetSocketOption (SocketOptionLevel.Socket, (SocketOptionName)so_peercred);
		}
		
		public int ProcessID {
			get {
				return(data.pid);
			}
		}

		public int UserID {
			get {
				return(data.uid);
			}
		}

		public int GroupID {
			get {
				return(data.gid);
			}
		}
	}
}

