//
// System.Net.NetworkInformation.UnicastIPAddressInformation
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//	Eric Butler (eric@extremeboredom.net)
//
// Copyright (c) 2006-2007 Novell, Inc. (http://www.novell.com)
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

namespace System.Net.NetworkInformation {
	class LinuxUnicastIPAddressInformation : UnicastIPAddressInformation
	{
		IPAddress address;
		IPAddress ipv4Mask;

		public LinuxUnicastIPAddressInformation (IPAddress address)
		{
			this.address = address;
		}

		public override IPAddress Address {
			get { return address; }
		}

		public override bool IsDnsEligible {
			get {
				byte[] addressBytes = address.GetAddressBytes ();
				return !(addressBytes[0] == 169 && addressBytes[1] == 254);
			}
		}

		[MonoTODO("Always returns false")]
		public override bool IsTransient {
			get { return false; }
		}

		// UnicastIPAddressInformation members

		public override long AddressPreferredLifetime {
			get { throw new NotImplementedException (); }
		}

		public override long AddressValidLifetime {
			get { throw new NotImplementedException (); }
		}

		public override long DhcpLeaseLifetime {
			get { throw new NotImplementedException (); }
		}

		public override DuplicateAddressDetectionState DuplicateAddressDetectionState {
			get { throw new NotImplementedException (); }
		}

		public override IPAddress IPv4Mask {
			get {
				// The IPv6 equivilant (for .net compatibility)
				if (Address.AddressFamily != AddressFamily.InterNetwork)
					return IPAddress.Any;

				if (ipv4Mask == null)
					ipv4Mask = SystemNetworkInterface.GetNetMask (address);

				return ipv4Mask;
			}
		}

		public override PrefixOrigin PrefixOrigin {
			get { throw new NotImplementedException (); }
		}

		public override SuffixOrigin SuffixOrigin {
			get { throw new NotImplementedException (); }
		}
	}
}
