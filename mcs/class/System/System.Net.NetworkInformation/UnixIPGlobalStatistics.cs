//
// System.Net.NetworkInformation.IPGlobalProperties
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
using System.Collections.Specialized;
using System.Globalization;

namespace System.Net.NetworkInformation {
	class MibIPGlobalStatistics : IPGlobalStatistics
	{
		StringDictionary dic;

		public MibIPGlobalStatistics (StringDictionary dic)
		{
			this.dic = dic;
		}

		long Get (string name)
		{
			return dic [name] != null ? long.Parse (dic [name], NumberFormatInfo.InvariantInfo) : 0;
		}

		public override int DefaultTtl {
			get { return (int) Get ("DefaultTTL"); }
		}
		public override bool ForwardingEnabled {
			get { return Get ("Forwarding") != 0; }
		}
		public override int NumberOfInterfaces {
			get { return (int) Get ("NumIf"); }
		}
		public override int NumberOfIPAddresses {
			get { return (int) Get ("NumAddr"); }
		}
		public override int NumberOfRoutes {
			get { return (int) Get ("NumRoutes"); }
		}
		public override long OutputPacketRequests {
			get { return Get ("OutRequests"); }
		}
		public override long OutputPacketRoutingDiscards {
			get { return Get ("RoutingDiscards"); }
		}
		public override long OutputPacketsDiscarded {
			get { return Get ("OutDiscards"); }
		}
		public override long OutputPacketsWithNoRoute {
			get { return Get ("OutNoRoutes"); }
		}
		public override long PacketFragmentFailures {
			get { return Get ("FragFails"); }
		}
		public override long PacketReassembliesRequired {
			get { return Get ("ReasmReqds"); }
		}
		public override long PacketReassemblyFailures {
			get { return Get ("ReasmFails"); }
		}
		public override long PacketReassemblyTimeout {
			get { return Get ("ReasmTimeout"); }
		}
		public override long PacketsFragmented {
			get { return Get ("FragOks"); }
		}
		public override long PacketsReassembled {
			get { return Get ("ReasmOks"); }
		}
		public override long ReceivedPackets {
			get { return Get ("InReceives"); }
		}
		public override long ReceivedPacketsDelivered {
			get { return Get ("InDelivers"); }
		}
		public override long ReceivedPacketsDiscarded {
			get { return Get ("InDiscards"); }
		}
		public override long ReceivedPacketsForwarded {
			get { return Get ("ForwDatagrams"); }
		}
		public override long ReceivedPacketsWithAddressErrors {
			get { return Get ("InAddrErrors"); }
		}
		public override long ReceivedPacketsWithHeadersErrors {
			get { return Get ("InHdrErrors"); }
		}
		public override long ReceivedPacketsWithUnknownProtocol {
			get { return Get ("InUnknownProtos"); }
		}
	}
}
