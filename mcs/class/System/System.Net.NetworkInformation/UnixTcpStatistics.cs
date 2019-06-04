//
// System.Net.NetworkInformation.TcpStatistics
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
	class MibTcpStatistics : TcpStatistics
	{
		StringDictionary dic;

		public MibTcpStatistics (StringDictionary dic)
		{
			this.dic = dic;
		}

		long Get (string name)
		{
			return dic [name] != null ? long.Parse (dic [name], NumberFormatInfo.InvariantInfo) : 0;
		}

		public override long ConnectionsAccepted {
			get { return Get ("PassiveOpens"); }
		}
		public override long ConnectionsInitiated {
			get { return Get ("ActiveOpens"); }
		}
		public override long CumulativeConnections {
			get { return Get ("NumConns"); }
		}
		public override long CurrentConnections {
			get { return Get ("CurrEstab"); }
		}
		public override long ErrorsReceived {
			get { return Get ("InErrs"); }
		}
		public override long FailedConnectionAttempts {
			get { return Get ("AttemptFails"); }
		}
		public override long MaximumConnections {
			get { return Get ("MaxConn"); }
		}
		public override long MaximumTransmissionTimeout {
			get { return Get ("RtoMax"); }
		}
		public override long MinimumTransmissionTimeout {
			get { return Get ("RtoMin"); }
		}
		public override long ResetConnections {
			get { return Get ("EstabResets"); }
		}
		public override long ResetsSent {
			get { return Get ("OutRsts"); }
		}
		public override long SegmentsReceived {
			get { return Get ("InSegs"); }
		}
		public override long SegmentsResent {
			get { return Get ("RetransSegs"); }
		}
		public override long SegmentsSent {
			get { return Get ("OutSegs"); }
		}
	}
}
