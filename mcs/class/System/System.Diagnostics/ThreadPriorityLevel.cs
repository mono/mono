//
// System.Diagnostics.ThreadPriorityLevel.cs
//
// Authors:
//	Dick Porter (dick@ximian.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

namespace System.Diagnostics {
	[Serializable]
	public enum ThreadPriorityLevel {
		AboveNormal=1,
		BelowNormal=-1,
		Highest=2,
		Idle=-15,
		Lowest=-2,
		Normal=0,
		TimeCritical=15,
	}
}
