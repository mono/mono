//
// System.Web.ProcessStatus.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
//

namespace System.Web {
	public enum ProcessStatus {
		Alive = 0x1,
		ShuttingDown,
		ShutDown,
		Terminated
	}
}
