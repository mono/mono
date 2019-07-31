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
#if WIN_PLATFORM
namespace System.Net.NetworkInformation {
	class Win32IcmpV4Statistics : IcmpV4Statistics
	{
		Win32_MIBICMPSTATS iin, iout;

		public Win32IcmpV4Statistics (Win32_MIBICMPINFO info)
		{
			iin = info.InStats;
			iout = info.OutStats;
		}

		public override long AddressMaskRepliesReceived {
			get { return iin.AddrMaskReps; }
		}
		public override long AddressMaskRepliesSent {
			get { return iout.AddrMaskReps; }
		}
		public override long AddressMaskRequestsReceived {
			get { return iin.AddrMasks; }
		}
		public override long AddressMaskRequestsSent {
			get { return iout.AddrMasks; }
		}
		public override long DestinationUnreachableMessagesReceived {
			get { return iin.DestUnreachs; }
		}
		public override long DestinationUnreachableMessagesSent {
			get { return iout.DestUnreachs; }
		}
		public override long EchoRepliesReceived {
			get { return iin.EchoReps; }
		}
		public override long EchoRepliesSent {
			get { return iout.EchoReps; }
		}
		public override long EchoRequestsReceived {
			get { return iin.Echos; }
		}
		public override long EchoRequestsSent {
			get { return iout.Echos; }
		}
		public override long ErrorsReceived {
			get { return iin.Errors; }
		}
		public override long ErrorsSent {
			get { return iout.Errors; }
		}
		public override long MessagesReceived {
			get { return iin.Msgs; }
		}
		public override long MessagesSent {
			get { return iout.Msgs; }
		}
		public override long ParameterProblemsReceived {
			get { return iin.ParmProbs; }
		}
		public override long ParameterProblemsSent {
			get { return iout.ParmProbs; }
		}
		public override long RedirectsReceived {
			get { return iin.Redirects; }
		}
		public override long RedirectsSent {
			get { return iout.Redirects; }
		}
		public override long SourceQuenchesReceived {
			get { return iin.SrcQuenchs; }
		}
		public override long SourceQuenchesSent {
			get { return iout.SrcQuenchs; }
		}
		public override long TimeExceededMessagesReceived {
			get { return iin.TimeExcds; }
		}
		public override long TimeExceededMessagesSent {
			get { return iout.TimeExcds; }
		}
		public override long TimestampRepliesReceived {
			get { return iin.TimestampReps; }
		}
		public override long TimestampRepliesSent {
			get { return iout.TimestampReps; }
		}
		public override long TimestampRequestsReceived {
			get { return iin.Timestamps; }
		}
		public override long TimestampRequestsSent {
			get { return iout.Timestamps; }
		}
	}

	struct Win32_MIBICMPINFO
	{
		public Win32_MIBICMPSTATS InStats;
		public Win32_MIBICMPSTATS OutStats;
	}

	struct Win32_MIBICMPSTATS
	{
		public uint Msgs;
		public uint Errors;
		public uint DestUnreachs;
		public uint TimeExcds;
		public uint ParmProbs;
		public uint SrcQuenchs;
		public uint Redirects;
		public uint Echos;
		public uint EchoReps;
		public uint Timestamps;
		public uint TimestampReps;
		public uint AddrMasks;
		public uint AddrMaskReps;
	}
}
#endif
