
// System.Runtime.InteropServices/ComMemberType.cs
//
// Paolo Molaro (lupus@ximian.com)
//
// (C) 2002 Ximian, Inc.

using System;

namespace System.Runtime.InteropServices
{
	[Serializable]
	public enum ComMemberType {
		Method  = 0,
		PropGet = 1,
		PropSet = 2
	}
}

