//
// System.Diagnostics.ThreadState.cs
//
// Authors:
//	Dick Porter (dick@ximian.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

namespace System.Diagnostics {
	[Serializable]
	public enum ThreadState {
		Initialized=0,
		Ready=1,
		Running=2,
		Standby=3,
		Terminated=4,
		Transition=6,
		Unknown=7,
		Wait=5,
	}
}
