//
// System.Web.ProcessShutdownReason.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
//

namespace System.Web {
	[Serializable]
	public enum ProcessShutdownReason {
		None,
		Unexpected,
		RequestsLimit,
		RequestQueueLimit,
		Timeout,
		IdleTimeout,
		MemoryLimitExceeded,
		PingFailed,
		DeadlockSuspected
   }
}
