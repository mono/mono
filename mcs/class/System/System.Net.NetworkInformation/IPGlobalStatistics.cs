//
// System.Net.NetworkInformation.IPGlobalProperties
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// Copyright (c) 2006 Novell, Inc. (http://www.novell.com)
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
namespace System.Net.NetworkInformation {
	public abstract class IPGlobalStatistics {
		protected IPGlobalStatistics ()
		{
		}

		public abstract int DefaultTtl { get; }
		public abstract bool ForwardingEnabled { get; }
		public abstract int NumberOfInterfaces { get; }
		public abstract int NumberOfIPAddresses { get; }
		public abstract int NumberOfRoutes { get; }
		public abstract long OutputPacketRequests { get; }
		public abstract long OutputPacketRoutingDiscards { get; }
		public abstract long OutputPacketsDiscarded { get; }
		public abstract long OutputPacketsWithNoRoute { get; }
		public abstract long PacketFragmentFailures { get; }
		public abstract long PacketReassembliesRequired { get; }
		public abstract long PacketReassemblyFailures { get; }
		public abstract long PacketReassemblyTimeout { get; }
		public abstract long PacketsFragmented { get; }
		public abstract long PacketsReassembled { get; }
		public abstract long ReceivedPackets { get; }
		public abstract long ReceivedPacketsDelivered { get; }
		public abstract long ReceivedPacketsDiscarded { get; }
		public abstract long ReceivedPacketsForwarded { get; }
		public abstract long ReceivedPacketsWithAddressErrors { get; }
		public abstract long ReceivedPacketsWithHeadersErrors { get; }
		public abstract long ReceivedPacketsWithUnknownProtocol { get; }
	}
}
#endif

