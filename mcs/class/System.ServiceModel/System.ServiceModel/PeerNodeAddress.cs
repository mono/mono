//
// PeerNodeAddress.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Net;

namespace System.ServiceModel
{
	[DataContract (Name = "PeerNodeAddress", Namespace = "http://schemas.microsoft.com/net/2006/05/peer")]
	[KnownType (typeof (IPAddress []))]
	public sealed class PeerNodeAddress
	{
		EndpointAddress endpoint;
		ReadOnlyCollection<IPAddress> peer_addresses;

		private PeerNodeAddress ()
		{
			// It is for DataContract deserialization.
		}

		public PeerNodeAddress (EndpointAddress endpointAddress,
			ReadOnlyCollection<IPAddress> ipAddresses)
		{
			if (endpointAddress == null)
				throw new ArgumentNullException ("endpointAddress");
			if (ipAddresses == null)
				throw new ArgumentNullException ("ipAddresses");
			this.endpoint = endpointAddress;
			peer_addresses = ipAddresses;
		}

		public EndpointAddress EndpointAddress {
			get { return endpoint; }
		}

		public ReadOnlyCollection<IPAddress> IPAddresses {
			get { return peer_addresses; }
		}

		[DataMember (Name = "EndpointAddress")]
		EndpointAddress10 SerializedEndpoint {
			get { return EndpointAddress10.FromEndpointAddress (endpoint); }
			set { endpoint = value.ToEndpointAddress (); }
		}

		[DataMember (Name = "IPAddresses")]
		IPAddress [] SerializedIPAddresses {
			get {
				IPAddress [] arr = new IPAddress [peer_addresses.Count];
				peer_addresses.CopyTo (arr, 0);
				return arr;
			}
			set {
				peer_addresses = new ReadOnlyCollection<IPAddress> (value);
			}
		}
	}
}
