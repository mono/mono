//
// System.Net.NetworkInformation.TcpStatistics
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
	public abstract class TcpStatistics {
		protected TcpStatistics ()
		{
		}
		
		public abstract long ConnectionsAccepted { get; }
		public abstract long ConnectionsInitiated { get; }
		public abstract long CumulativeConnections { get; }
		public abstract long CurrentConnections { get; }
		public abstract long ErrorsReceived { get; }
		public abstract long FailedConnectionAttempts { get; }
		public abstract long MaximumConnections { get; }
		public abstract long MaximumTransmissionTimeout { get; }
		public abstract long MinimumTransmissionTimeout { get; }
		public abstract long ResetConnections { get; }
		public abstract long ResetsSent { get; }
		public abstract long SegmentsReceived { get; }
		public abstract long SegmentsResent { get; }
		public abstract long SegmentsSent { get; }
	}
}
#endif

