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
#if WIN_PLATFORM
using System.Net.Sockets;
using System.Diagnostics.Contracts;

namespace System.Net.NetworkInformation {
	class Win32UnicastIPAddressInformation : UnicastIPAddressInformation
	{
		Win32_IP_ADAPTER_UNICAST_ADDRESS info;
		IPAddress ipv4Mask;

		public Win32UnicastIPAddressInformation (Win32_IP_ADAPTER_UNICAST_ADDRESS info)
		{
			this.info = info;
			IPAddress ipAddress = info.Address.GetIPAddress ();
			// IPv6 returns 0.0.0.0 for consistancy with XP
			if (ipAddress.AddressFamily == AddressFamily.InterNetwork) {
				ipv4Mask = PrefixLengthToSubnetMask (info.OnLinkPrefixLength, ipAddress.AddressFamily);
			}
		}

		public override IPAddress Address {
			get { return info.Address.GetIPAddress (); }
		}

		public override bool IsDnsEligible {
			get { return info.LengthFlags.IsDnsEligible; }
		}

		public override bool IsTransient {
			get { return info.LengthFlags.IsTransient; }
		}

		// UnicastIPAddressInformation members

		public override long AddressPreferredLifetime {
			get { return info.PreferredLifetime; }
		}

		public override long AddressValidLifetime {
			get { return info.ValidLifetime; }
		}

		public override long DhcpLeaseLifetime {
			get { return info.LeaseLifetime; }
		}

		public override DuplicateAddressDetectionState DuplicateAddressDetectionState {
			get { return info.DadState; }
		}

		public override IPAddress IPv4Mask{
			get {
				// The IPv6 equivilant was never available on XP, and we've kept this behavior for legacy reasons.
				// For IPv6 use PrefixLength instead.
				if (Address.AddressFamily != AddressFamily.InterNetwork) {
					return IPAddress.Any;
				}

				return ipv4Mask;
			}
		}

		public override PrefixOrigin PrefixOrigin {
			get { return info.PrefixOrigin; }
		}

		public override SuffixOrigin SuffixOrigin {
			get { return info.SuffixOrigin; }
		}

		// Convert a CIDR prefix length to a subnet mask "255.255.255.0" format
		private static IPAddress PrefixLengthToSubnetMask (byte prefixLength, AddressFamily family) {
			Contract.Requires ((0 <= prefixLength) && (prefixLength <= 126));
			Contract.Requires ((family == AddressFamily.InterNetwork) || (family == AddressFamily.InterNetworkV6));

			byte[] addressBytes;
			if (family == AddressFamily.InterNetwork) {
				addressBytes = new byte [4];
			} else { // v6
				addressBytes = new byte [16];
			}

			Contract.Assert (prefixLength < (addressBytes.Length * 8));

			// Enable bits one at a time from left/high to right/low
			for (int bit = 0; bit < prefixLength; bit++) {
				addressBytes [bit / 8] |= (byte) (0x80 >> (bit % 8));
			}

			return new IPAddress (addressBytes);
		}
	}
}
#endif
