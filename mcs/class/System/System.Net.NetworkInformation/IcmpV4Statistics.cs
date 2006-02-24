//
// System.Net.NetworkInformation.IcmpV4Statistics
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
	public abstract class IcmpV4Statistics {
		protected IcmpV4Statistics ()
		{
		}

		public abstract long AddressMaskRepliesReceived { get; }
		public abstract long AddressMaskRepliesSent { get; }
		public abstract long AddressMaskRequestsReceived { get; }
		public abstract long AddressMaskRequestsSent { get; }
		public abstract long DestinationUnreachableMessagesReceived { get; }
		public abstract long DestinationUnreachableMessagesSent { get; }
		public abstract long EchoRepliesReceived { get; }
		public abstract long EchoRepliesSent { get; }
		public abstract long EchoRequestsReceived { get; }
		public abstract long EchoRequestsSent { get; }
		public abstract long ErrorsReceived { get; }
		public abstract long ErrorsSent { get; }
		public abstract long MessagesReceived { get; }
		public abstract long MessagesSent { get; }
		public abstract long ParameterProblemsReceived { get; }
		public abstract long ParameterProblemsSent { get; }
		public abstract long RedirectsReceived { get; }
		public abstract long RedirectsSent { get; }
		public abstract long SourceQuenchesReceived { get; }
		public abstract long SourceQuenchesSent { get; }
		public abstract long TimeExceededMessagesReceived { get; }
		public abstract long TimeExceededMessagesSent { get; }
		public abstract long TimestampRepliesReceived { get; }
		public abstract long TimestampRepliesSent { get; }
		public abstract long TimestampRequestsReceived { get; }
		public abstract long TimestampRequestsSent { get; }
	}
}
#endif

