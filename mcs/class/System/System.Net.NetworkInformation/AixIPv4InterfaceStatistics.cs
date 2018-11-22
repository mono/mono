//
// System.Net.NetworkInformation.IPv4InterfaceStatistics
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//	Miguel de Icaza (miguel@ximian.com)
//
// Copyright (c) 2006-2008 Novell, Inc. (http://www.novell.com)
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
namespace System.Net.NetworkInformation {
	// dummy class
	class AixIPv4InterfaceStatistics : IPv4InterfaceStatistics
	{
		//AixNetworkInterface aix;

		public AixIPv4InterfaceStatistics (AixNetworkInterface parent)
		{
			//aix = parent;
		}

		public override long BytesReceived {
			get { return 0; }
		}

		public override long BytesSent {
			get { return 0; }
		}

		public override long IncomingPacketsDiscarded {
			get { return 0; }
		}

		public override long IncomingPacketsWithErrors {
			get { return 0; }
		}

		public override long IncomingUnknownProtocolPackets {
			get { return 0; }
		}

		public override long NonUnicastPacketsReceived {
			get { return 0; }
		}

		public override long NonUnicastPacketsSent {
			get { return 0; }
		}

		public override long OutgoingPacketsDiscarded {
			get { return 0; }
		}

		public override long OutgoingPacketsWithErrors {
			get { return 0; }
		}

		public override long OutputQueueLength {
			get { return 0; }
		}

		public override long UnicastPacketsReceived {
			get { return 0; }
		}

		public override long UnicastPacketsSent {
			get { return 0; }
		}
	}
}

