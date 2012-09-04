//
// System.Net.NetworkInformation.MulticastIPAddressInformation
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
using System.Runtime.InteropServices;

namespace System.Net.NetworkInformation {
	public abstract class MulticastIPAddressInformation : IPAddressInformation {
		protected MulticastIPAddressInformation ()
		{
		}

		public abstract long AddressPreferredLifetime { get; }
		public abstract long AddressValidLifetime { get; }
		public abstract long DhcpLeaseLifetime { get; }
		public abstract DuplicateAddressDetectionState DuplicateAddressDetectionState { get; }
		public abstract PrefixOrigin PrefixOrigin { get; }
		public abstract SuffixOrigin SuffixOrigin { get; }
	}

	// So, why are there many pointless members in the base class?
	class MulticastIPAddressInformationImpl : MulticastIPAddressInformation
	{
		IPAddress address;
		bool is_dns_eligible, is_transient;

		public MulticastIPAddressInformationImpl (IPAddress address, bool isDnsEligible, bool isTransient)
		{
			this.address = address;
			this.is_dns_eligible = isDnsEligible;
			this.is_transient = isTransient;
		}

		public override IPAddress Address {
			get { return address; }
		}

		public override bool IsDnsEligible {
			get { return is_dns_eligible; }
		}

		public override bool IsTransient {
			get { return is_transient; }
		}

		public override long AddressPreferredLifetime {
			get { return 0; }
		}

		public override long AddressValidLifetime {
			get { return 0; }
		}

		public override long DhcpLeaseLifetime {
			get { return 0; }
		}

		public override DuplicateAddressDetectionState DuplicateAddressDetectionState {
			get { return DuplicateAddressDetectionState.Invalid; }
		}

		public override PrefixOrigin PrefixOrigin {
			get { return PrefixOrigin.Other; }
		}

		public override SuffixOrigin SuffixOrigin {
			get { return SuffixOrigin.Other; }
		}

	}
}
