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
#if WIN_PLATFORM
using System.Runtime.InteropServices;

namespace System.Net.NetworkInformation {
	class IcmpV6MessageTypes {
		public const int DestinationUnreachable = 1;
		public const int PacketTooBig = 2;
		public const int TimeExceeded = 3;
		public const int ParameterProblem = 4;
		public const int EchoRequest = 128;
		public const int EchoReply = 129;
		public const int GroupMembershipQuery = 130;
		public const int GroupMembershipReport = 131;
		public const int GroupMembershipReduction = 132;
		public const int RouterSolicitation = 133;
		public const int RouterAdvertisement = 134;
		public const int NeighborSolicitation = 135;
		public const int NeighborAdvertisement = 136;
		public const int Redirect = 137;
		public const int RouterRenumbering = 138;
	}

	class Win32IcmpV6Statistics : IcmpV6Statistics
	{
		Win32_MIBICMPSTATS_EX iin, iout;

		public Win32IcmpV6Statistics (Win32_MIB_ICMP_EX info)
		{
			iin = info.InStats;
			iout = info.OutStats;
		}

		public override long DestinationUnreachableMessagesReceived {
			get { return iin.Counts [IcmpV6MessageTypes.DestinationUnreachable]; }
		}
		public override long DestinationUnreachableMessagesSent {
			get { return iout.Counts [IcmpV6MessageTypes.DestinationUnreachable]; }
		}
		public override long EchoRepliesReceived {
			get { return iin.Counts [IcmpV6MessageTypes.EchoReply]; }
		}
		public override long EchoRepliesSent {
			get { return iout.Counts [IcmpV6MessageTypes.EchoReply]; }
		}
		public override long EchoRequestsReceived {
			get { return iin.Counts [IcmpV6MessageTypes.EchoRequest]; }
		}
		public override long EchoRequestsSent {
			get { return iout.Counts [IcmpV6MessageTypes.EchoRequest]; }
		}
		public override long ErrorsReceived {
			get { return iin.Errors; }
		}
		public override long ErrorsSent {
			get { return iout.Errors; }
		}
		public override long MembershipQueriesReceived {
			get { return iin.Counts [IcmpV6MessageTypes.GroupMembershipQuery]; }
		}
		public override long MembershipQueriesSent {
			get { return iout.Counts [IcmpV6MessageTypes.GroupMembershipQuery]; }
		}
		public override long MembershipReductionsReceived {
			get { return iin.Counts [IcmpV6MessageTypes.GroupMembershipReduction]; }
		}
		public override long MembershipReductionsSent {
			get { return iout.Counts [IcmpV6MessageTypes.GroupMembershipReduction]; }
		}
		public override long MembershipReportsReceived {
			get { return iin.Counts [IcmpV6MessageTypes.GroupMembershipReport]; }
		}
		public override long MembershipReportsSent {
			get { return iout.Counts [IcmpV6MessageTypes.GroupMembershipReport]; }
		}
		public override long MessagesReceived {
			get { return iin.Msgs; }
		}
		public override long MessagesSent {
			get { return iout.Msgs; }
		}
		public override long NeighborAdvertisementsReceived {
			get { return iin.Counts [IcmpV6MessageTypes.NeighborAdvertisement]; }
		}
		public override long NeighborAdvertisementsSent {
			get { return iout.Counts [IcmpV6MessageTypes.NeighborAdvertisement]; }
		}
		public override long NeighborSolicitsReceived {
			get { return iin.Counts [IcmpV6MessageTypes.NeighborSolicitation]; }
		}
		public override long NeighborSolicitsSent {
			get { return iout.Counts [IcmpV6MessageTypes.NeighborSolicitation]; }
		}
		public override long PacketTooBigMessagesReceived {
			get { return iin.Counts [IcmpV6MessageTypes.PacketTooBig]; }
		}
		public override long PacketTooBigMessagesSent {
			get { return iout.Counts [IcmpV6MessageTypes.PacketTooBig]; }
		}
		public override long ParameterProblemsReceived {
			get { return iin.Counts [IcmpV6MessageTypes.ParameterProblem]; }
		}
		public override long ParameterProblemsSent {
			get { return iout.Counts [IcmpV6MessageTypes.ParameterProblem]; }
		}
		public override long RedirectsReceived {
			get { return iin.Counts [IcmpV6MessageTypes.Redirect]; }
		}
		public override long RedirectsSent {
			get { return iout.Counts [IcmpV6MessageTypes.Redirect]; }
		}
		public override long RouterAdvertisementsReceived {
			get { return iin.Counts [IcmpV6MessageTypes.RouterAdvertisement]; }
		}
		public override long RouterAdvertisementsSent {
			get { return iout.Counts [IcmpV6MessageTypes.RouterAdvertisement]; }
		}
		public override long RouterSolicitsReceived {
			get { return iin.Counts [IcmpV6MessageTypes.RouterSolicitation]; }
		}
		public override long RouterSolicitsSent {
			get { return iout.Counts [IcmpV6MessageTypes.RouterSolicitation]; }
		}
		public override long TimeExceededMessagesReceived {
			get { return iin.Counts [IcmpV6MessageTypes.TimeExceeded]; }
		}
		public override long TimeExceededMessagesSent {
			get { return iout.Counts [IcmpV6MessageTypes.TimeExceeded]; }
		}
	}

	struct Win32_MIB_ICMP_EX
	{
		public Win32_MIBICMPSTATS_EX InStats;
		public Win32_MIBICMPSTATS_EX OutStats;
	}

	struct Win32_MIBICMPSTATS_EX
	{
		public uint Msgs;
		public uint Errors;
		[MarshalAs (UnmanagedType.ByValArray, SizeConst = 256)]
		public uint [] Counts;
	}
}
#endif
