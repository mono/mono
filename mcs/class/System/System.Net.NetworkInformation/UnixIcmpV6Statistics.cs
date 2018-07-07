//
// System.Net.NetworkInformation.IcmpV6Statistics
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
	class MibIcmpV6Statistics : IcmpV6Statistics
	{
		StringDictionary dic;

		public MibIcmpV6Statistics (StringDictionary dic)
		{
			this.dic = dic;
		}

		long Get (string name)
		{
			return dic [name] != null ? long.Parse (dic [name], NumberFormatInfo.InvariantInfo) : 0;
		}

		public override long DestinationUnreachableMessagesReceived {
			get { return Get ("InDestUnreachs"); }
		}
		public override long DestinationUnreachableMessagesSent {
			get { return Get ("OutDestUnreachs"); }
		}
		public override long EchoRepliesReceived {
			get { return Get ("InEchoReplies"); }
		}
		public override long EchoRepliesSent {
			get { return Get ("OutEchoReplies"); }
		}
		public override long EchoRequestsReceived {
			get { return Get ("InEchos"); }
		}
		public override long EchoRequestsSent {
			get { return Get ("OutEchos"); }
		}
		public override long ErrorsReceived {
			get { return Get ("InErrors"); }
		}
		public override long ErrorsSent {
			get { return Get ("OutErrors"); }
		}
		public override long MembershipQueriesReceived {
			get { return Get ("InGroupMembQueries"); }
		}
		public override long MembershipQueriesSent {
			get { return Get ("OutGroupMembQueries"); }
		}
		public override long MembershipReductionsReceived {
			get { return Get ("InGroupMembReductiions"); }
		}
		public override long MembershipReductionsSent {
			get { return Get ("OutGroupMembReductiions"); }
		}
		public override long MembershipReportsReceived {
			get { return Get ("InGroupMembRespons"); }
		}
		public override long MembershipReportsSent {
			get { return Get ("OutGroupMembRespons"); }
		}
		public override long MessagesReceived {
			get { return Get ("InMsgs"); }
		}
		public override long MessagesSent {
			get { return Get ("OutMsgs"); }
		}
		public override long NeighborAdvertisementsReceived {
			get { return Get ("InNeighborAdvertisements"); }
		}
		public override long NeighborAdvertisementsSent {
			get { return Get ("OutNeighborAdvertisements"); }
		}
		public override long NeighborSolicitsReceived {
			get { return Get ("InNeighborSolicits"); }
		}
		public override long NeighborSolicitsSent {
			get { return Get ("OutNeighborSolicits"); }
		}
		public override long PacketTooBigMessagesReceived {
			get { return Get ("InPktTooBigs"); }
		}
		public override long PacketTooBigMessagesSent {
			get { return Get ("OutPktTooBigs"); }
		}
		public override long ParameterProblemsReceived {
			get { return Get ("InParmProblems"); }
		}
		public override long ParameterProblemsSent {
			get { return Get ("OutParmProblems"); }
		}
		public override long RedirectsReceived {
			get { return Get ("InRedirects"); }
		}
		public override long RedirectsSent {
			get { return Get ("OutRedirects"); }
		}
		public override long RouterAdvertisementsReceived {
			get { return Get ("InRouterAdvertisements"); }
		}
		public override long RouterAdvertisementsSent {
			get { return Get ("OutRouterAdvertisements"); }
		}
		public override long RouterSolicitsReceived {
			get { return Get ("InRouterSolicits"); }
		}
		public override long RouterSolicitsSent {
			get { return Get ("OutRouterSolicits"); }
		}
		public override long TimeExceededMessagesReceived {
			get { return Get ("InTimeExcds"); }
		}
		public override long TimeExceededMessagesSent {
			get { return Get ("OutTimeExcds"); }
		}
	}
}

