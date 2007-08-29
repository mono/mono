//
// System.Net.NetworkInformation.UnicastIPAddressInformation
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//	Atsushi Enomoto (atsushi@ximian.com)
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
#if NET_2_0
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
		Win32_IP_ADAPTER_UNICAST_ADDRESS info;

		public Win32UnicastIPAddressInformation (Win32_IP_ADAPTER_UNICAST_ADDRESS info)
		{
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

		// FIXME: where to get this info?
		public override IPAddress IPv4Mask {
			get { throw new NotImplementedException (); }
		}

		public override PrefixOrigin PrefixOrigin {
			get { return info.PrefixOrigin; }
		}

		public override SuffixOrigin SuffixOrigin {
			get { return info.SuffixOrigin; }
		}

	}
}
#endif

