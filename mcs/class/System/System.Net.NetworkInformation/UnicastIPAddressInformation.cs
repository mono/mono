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
using System;
using System.Runtime.InteropServices;

namespace System.Net.NetworkInformation {
	public abstract class UnicastIPAddressInformation : IPAddressInformation {
		protected UnicastIPAddressInformation ()
		{
		}
		
		public abstract long AddressPreferredLifetime { get; }
		public abstract long AddressValidLifetime { get; }
		public abstract long DhcpLeaseLifetime { get; }
		public abstract DuplicateAddressDetectionState DuplicateAddressDetectionState { get; }
		public abstract IPAddress IPv4Mask { get; }
		public abstract PrefixOrigin PrefixOrigin { get; }
		public abstract SuffixOrigin SuffixOrigin { get; }
	}

	class Win32UnicastIPAddressInformation : UnicastIPAddressInformation 
	{
		int if_index;
		Win32_IP_ADAPTER_UNICAST_ADDRESS info;

		public Win32UnicastIPAddressInformation (int ifIndex, Win32_IP_ADAPTER_UNICAST_ADDRESS info)
		{
			this.if_index = ifIndex;
			this.info = info;
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

		public override IPAddress IPv4Mask {
			get {
				Win32_IP_ADAPTER_INFO ai = Win32NetworkInterface2.GetAdapterInfoByIndex (if_index);
				if (ai == null)
					throw new Exception ("huh? " + if_index);
				if (this.Address == null)
					return null;
				string expected = this.Address.ToString ();
				unsafe {
					Win32_IP_ADDR_STRING p = ai.IpAddressList;
					while (true) {
						if (p.IpAddress == expected)
							return IPAddress.Parse (p.IpMask);
						if (p.Next == IntPtr.Zero)
							break;
						p = (Win32_IP_ADDR_STRING) Marshal.PtrToStructure (p.Next, typeof (Win32_IP_ADDR_STRING));
					}

					// Or whatever it should be...
					return null;
				}
			}
		}

		public override PrefixOrigin PrefixOrigin {
			get { return info.PrefixOrigin; }
		}

		public override SuffixOrigin SuffixOrigin {
			get { return info.SuffixOrigin; }
		}
	}

	class LinuxUnicastIPAddressInformation : UnicastIPAddressInformation
	{
		IPAddress address;

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
			get { throw new NotImplementedException (); }
		}

		public override PrefixOrigin PrefixOrigin {
			get { throw new NotImplementedException (); }
		}

		public override SuffixOrigin SuffixOrigin {
			get { throw new NotImplementedException (); }
		}
	}
}

