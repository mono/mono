//
// System.Diagnostics.ThreadWaitReason.cs
//
// Authors:
//	Dick Porter (dick@ximian.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

namespace System.Diagnostics {
	[Serializable]
	public enum ThreadWaitReason {
		EventPairHigh=7,
		EventPairLow=8,
		ExecutionDelay=4,
		Executive=0,
		FreePage=1,
		LpcReceive=9,
		LpcReply=10,
		PageIn=2,
		PageOut=12,
		Suspended=5,
		SystemAllocation=3,
		Unknown=13,
		UserRequest=6,
		VirtualMemory=11,
	}
}
