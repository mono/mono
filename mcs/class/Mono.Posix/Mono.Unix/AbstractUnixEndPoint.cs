//
// Mono.Unix.AbstractUnixEndPoint: EndPoint derived class for AF_UNIX family sockets.
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Alp Toker  (alp@atoker.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
// (C) 2006 Alp Toker
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
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Mono.Unix
{
	[Serializable]
	public class AbstractUnixEndPoint : EndPoint
	{
		string path;

		public AbstractUnixEndPoint (string path)
		{
			if (path == null)
				throw new ArgumentNullException ("path");

			if (path == "")
				throw new ArgumentException ("Cannot be empty.", "path");
			this.path = path;
		}

		public string Path {
			get {
				return(path);
			}
			set {
				path=value;
			}
		}

		public override AddressFamily AddressFamily {
			get { return AddressFamily.Unix; }
		}

		public override EndPoint Create (SocketAddress socketAddress)
		{
			/*
			 * Should also check this
			 *
			int addr = (int) AddressFamily.Unix;
			if (socketAddress [0] != (addr & 0xFF))
				throw new ArgumentException ("socketAddress is not a unix socket address.");

			if (socketAddress [1] != ((addr & 0xFF00) >> 8))
				throw new ArgumentException ("socketAddress is not a unix socket address.");
			 */

			byte [] bytes = new byte [socketAddress.Size - 2 - 1];
			for (int i = 0; i < bytes.Length; i++) {
				bytes [i] = socketAddress [2 + 1 + i];
			}

			string name = Encoding.Default.GetString (bytes);
			return new AbstractUnixEndPoint (name);
		}

		public override SocketAddress Serialize ()
		{
			byte [] bytes = Encoding.Default.GetBytes (path);
			SocketAddress sa = new SocketAddress (AddressFamily, 2 + 1 + bytes.Length);
			//NULL prefix denotes the abstract namespace, see unix(7)
			//in this case, there is no NULL suffix
			sa [2] = 0;

			// sa [0] -> family low byte, sa [1] -> family high byte
			for (int i = 0; i < bytes.Length; i++)
				sa [i + 2 + 1] = bytes [i];

			return sa;
		}

		public override string ToString() {
			return(path);
		}

		public override int GetHashCode ()
		{
			return path.GetHashCode ();
		}

		public override bool Equals (object o)
		{
			AbstractUnixEndPoint other = o as AbstractUnixEndPoint;
			if (other == null)
				return false;

			return (other.path == path);
		}
	}
}

