//
// System.Drawing.Imaging.ColorChannelFlag.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Dennis Hayes (dennish@raytek.com)
//
using System;
namespace System.Drawing.Imaging
{
	[Serializable]
	public enum ColorChannelFlag {
		ColorChannelC = 0,
		ColorChannelK = 3,
		ColorChannelLast = 4,
		ColorChannelM = 1,
		ColorChannelY = 2
	}
}
