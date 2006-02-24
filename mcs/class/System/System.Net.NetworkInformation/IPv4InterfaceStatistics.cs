//
// System.Net.NetworkInformation.IPv4InterfaceStatistics
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
	public abstract class IPv4InterfaceStatistics {
		protected IPv4InterfaceStatistics ()
		{
		}
		
		public abstract long BytesReceived { get; }
		public abstract long BytesSent { get; }
		public abstract long IncomingPacketsDiscarded { get; }
		public abstract long IncomingPacketsWithErrors { get; }
		public abstract long IncomingUnknownProtocolPackets { get; }
		public abstract long NonUnicastPacketsReceived { get; }
		public abstract long NonUnicastPacketsSent { get; }
		public abstract long OutgoingPacketsDiscarded { get; }
		public abstract long OutgoingPacketsWithErrors { get; }
		public abstract long OutputQueueLength { get; }
		public abstract long UnicastPacketsReceived { get; }
		public abstract long UnicastPacketsSent { get; }
	}
}
#endif

