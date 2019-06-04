//
// System.Net.NetworkInformation.IcmpV4Statistics
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
	class MibIcmpV4Statistics : IcmpV4Statistics
	{
		StringDictionary dic;

		public MibIcmpV4Statistics (StringDictionary dic)
		{
			this.dic = dic;
		}

		long Get (string name)
		{
			return dic [name] != null ? long.Parse (dic [name], NumberFormatInfo.InvariantInfo) : 0;
		}

		public override long AddressMaskRepliesReceived {
			get { return Get ("InAddrMaskReps"); }
		}
		public override long AddressMaskRepliesSent {
			get { return Get ("OutAddrMaskReps"); }
		}
		public override long AddressMaskRequestsReceived {
			get { return Get ("InAddrMasks"); }
		}
		public override long AddressMaskRequestsSent {
			get { return Get ("OutAddrMasks"); }
		}
		public override long DestinationUnreachableMessagesReceived {
			get { return Get ("InDestUnreachs"); }
		}
		public override long DestinationUnreachableMessagesSent {
			get { return Get ("OutDestUnreachs"); }
		}
		public override long EchoRepliesReceived {
			get { return Get ("InEchoReps"); }
		}
		public override long EchoRepliesSent {
			get { return Get ("OutEchoReps"); }
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
		public override long MessagesReceived {
			get { return Get ("InMsgs"); }
		}
		public override long MessagesSent {
			get { return Get ("OutMsgs"); }
		}
		public override long ParameterProblemsReceived {
			get { return Get ("InParmProbs"); }
		}
		public override long ParameterProblemsSent {
			get { return Get ("OutParmProbs"); }
		}
		public override long RedirectsReceived {
			get { return Get ("InRedirects"); }
		}
		public override long RedirectsSent {
			get { return Get ("OutRedirects"); }
		}
		public override long SourceQuenchesReceived {
			get { return Get ("InSrcQuenchs"); }
		}
		public override long SourceQuenchesSent {
			get { return Get ("OutSrcQuenchs"); }
		}
		public override long TimeExceededMessagesReceived {
			get { return Get ("InTimeExcds"); }
		}
		public override long TimeExceededMessagesSent {
			get { return Get ("OutTimeExcds"); }
		}
		public override long TimestampRepliesReceived {
			get { return Get ("InTimestampReps"); }
		}
		public override long TimestampRepliesSent {
			get { return Get ("OutTimestampReps"); }
		}
		public override long TimestampRequestsReceived {
			get { return Get ("InTimestamps"); }
		}
		public override long TimestampRequestsSent {
			get { return Get ("OutTimestamps"); }
		}
	}
}
