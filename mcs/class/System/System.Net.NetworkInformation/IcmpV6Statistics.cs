//
// System.Net.NetworkInformation.IcmpV6Statistics
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
	public abstract class IcmpV6Statistics {
		protected IcmpV6Statistics ()
		{
		}

		public abstract long DestinationUnreachableMessagesReceived { get; }
		public abstract long DestinationUnreachableMessagesSent { get; }
		public abstract long EchoRepliesReceived { get; }
		public abstract long EchoRepliesSent { get; }
		public abstract long EchoRequestsReceived { get; }
		public abstract long EchoRequestsSent { get; }
		public abstract long ErrorsReceived { get; }
		public abstract long ErrorsSent { get; }
		public abstract long MembershipQueriesReceived { get; }
		public abstract long MembershipQueriesSent { get; }
		public abstract long MembershipReductionsReceived { get; }
		public abstract long MembershipReductionsSent { get; }
		public abstract long MembershipReportsReceived { get; }
		public abstract long MembershipReportsSent { get; }
		public abstract long MessagesReceived { get; }
		public abstract long MessagesSent { get; }
		public abstract long NeighborAdvertisementsReceived { get; }
		public abstract long NeighborAdvertisementsSent { get; }
		public abstract long NeighborSolicitsReceived { get; }
		public abstract long NeighborSolicitsSent { get; }
		public abstract long PacketTooBigMessagesReceived { get; }
		public abstract long PacketTooBigMessagesSent { get; }
		public abstract long ParameterProblemsReceived { get; }
		public abstract long ParameterProblemsSent { get; }
		public abstract long RedirectsReceived { get; }
		public abstract long RedirectsSent { get; }
		public abstract long RouterAdvertisementsReceived { get; }
		public abstract long RouterAdvertisementsSent { get; }
		public abstract long RouterSolicitsReceived { get; }
		public abstract long RouterSolicitsSent { get; }
		public abstract long TimeExceededMessagesReceived { get; }
		public abstract long TimeExceededMessagesSent { get; }
	}
}
#endif

