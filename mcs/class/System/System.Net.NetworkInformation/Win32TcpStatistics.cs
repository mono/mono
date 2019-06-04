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
#if WIN_PLATFORM
namespace System.Net.NetworkInformation {
	class Win32TcpStatistics : TcpStatistics
	{
		Win32_MIB_TCPSTATS info;

		public Win32TcpStatistics (Win32_MIB_TCPSTATS info)
		{
			this.info = info;
		}

		public override long ConnectionsAccepted {
			get { return info.PassiveOpens; }
		}

		public override long ConnectionsInitiated {
			get { return info.ActiveOpens; }
		}

		public override long CumulativeConnections {
			get { return info.NumConns; }
		}

		public override long CurrentConnections {
			get { return info.CurrEstab; }
		}

		public override long ErrorsReceived {
			get { return info.InErrs; }
		}

		public override long FailedConnectionAttempts {
			get { return info.AttemptFails; }
		}

		public override long MaximumConnections {
			get { return info.MaxConn; }
		}

		public override long MaximumTransmissionTimeout {
			get { return info.RtoMax; }
		}

		public override long MinimumTransmissionTimeout {
			get { return info.RtoMin; }
		}

		public override long ResetConnections {
			get { return info.EstabResets; }
		}

		public override long ResetsSent {
			get { return info.OutRsts; }
		}

		public override long SegmentsReceived {
			get { return info.InSegs; }
		}

		public override long SegmentsResent {
			get { return info.RetransSegs; }
		}

		public override long SegmentsSent {
			get { return info.OutSegs; }
		}
	}

	struct Win32_MIB_TCPSTATS
	{
		public uint RtoAlgorithm;
		public uint RtoMin;
		public uint RtoMax;
		public uint MaxConn;
		public uint ActiveOpens;
		public uint PassiveOpens;
		public uint AttemptFails;
		public uint EstabResets;
		public uint CurrEstab;
		public uint InSegs;
		public uint OutSegs;
		public uint RetransSegs;
		public uint InErrs;
		public uint OutRsts;
		public uint NumConns;
	}
}
#endif
