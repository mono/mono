//
// Mono.Posix.UnixEndPoint: EndPoint derived class for AF_UNIX family sockets.
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Mono.Posix
{
	[Serializable]
	public class UnixEndPoint : EndPoint
	{
		string filename;
		
		public UnixEndPoint (string filename)
		{
			this.filename = filename;
		}
		
		public string Filename {
			get {
				return(filename);
			}
			set {
				filename=value;
			}
		}

		public override AddressFamily AddressFamily {
			get { return AddressFamily.Unix; }
		}

		public override EndPoint Create (SocketAddress socketAddress)
		{
			int size = socketAddress.Size;
			byte [] bytes = new byte [size];
			for (int i = 0; i < size; i++) {
				bytes [i] = socketAddress [i];
			}

			string name = Encoding.Default.GetString (bytes);
			return new UnixEndPoint (name);
		}

		public override SocketAddress Serialize ()
		{
			byte [] bytes = Encoding.Default.GetBytes (filename);
			SocketAddress sa = new SocketAddress (AddressFamily, bytes.Length + 2);
			// sa [0] -> family low byte, sa [1] -> family high byte
			for (int i = 0; i < bytes.Length; i++)
				sa [i + 2] = bytes [i];

			return sa;
		}

		public override string ToString() {
			return(filename);
		}
	}
}

