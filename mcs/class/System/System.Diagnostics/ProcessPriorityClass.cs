//
// System.Diagnostics.ProcessPriorityClass.cs
//
// Authors:
//	Dick Porter (dick@ximian.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

namespace System.Diagnostics {
	[Serializable]
	public enum ProcessPriorityClass {
		AboveNormal=0x08000,
		BelowNormal=0x04000,
		High=0x00080,
		Idle=0x00040,
		Normal=0x00020,
		RealTime=0x00100,
	}
}
