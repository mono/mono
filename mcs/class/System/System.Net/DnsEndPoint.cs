//
// System.Net.DnsEndPoint
//
// Authors:
//	Stephane Delcroix  <stephane@delcroix.org>
//
// (c) 2007, 2009 Novell, Inc. (http://www.novell.com)
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

#if NET_2_1 || NET_4_0

using System.Net.Sockets;

namespace System.Net { 

	public sealed class DnsEndPoint : EndPoint {
		string host;
		int port;
		AddressFamily addressFamily = AddressFamily.Unspecified;

		public DnsEndPoint (string host, int port)
		{
			if (host == null)
				throw new ArgumentNullException ("host");
			if (host == String.Empty)
				throw new ArgumentException ("host parameter contains an empty string");
			if (port < 0 || port > 0xffff)
				throw new ArgumentOutOfRangeException ("port is less than 0 or greater than 0xffff");

			this.host = host;
			this.port = port;
		}

		public DnsEndPoint (string host, int port, AddressFamily addressFamily) : this (host, port)
		{
			switch (addressFamily) {
			case AddressFamily.InterNetwork:
			case AddressFamily.InterNetworkV6:
			case AddressFamily.Unspecified:
				this.addressFamily = addressFamily;
				break;
			default:
				// throw for Unknown or any invalid value
				throw new ArgumentException ("addressFamily");
			}
		}

		public override bool Equals (object other)
		{
			if (other is DnsEndPoint)
				return Equals (other as DnsEndPoint);
			return false;
		}

		private bool Equals (DnsEndPoint other)
		{
			if (port != other.port || addressFamily != other.addressFamily|| host != other.host)
				return false;
			return true;
		}

		public override int GetHashCode ()
		{
			return port ^ (int)addressFamily ^ host.GetHashCode ();
		}

		public override string ToString ()
		{
			return String.Format ("{0}/{1}:{2}", addressFamily, host, port);
		}

		public override AddressFamily AddressFamily {
			get { return addressFamily; }
		}

		public string Host {
			get { return host; }
		}

		public int Port {
			get { return port; }
		}
	}
}

#endif
