//
// System.Drawing.StringTrimming.cs
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
// Author: Dennis Hayes (dennish@raytek.com)
//


using System;
namespace System.Drawing 
{
	public enum  StringTrimming {
		None              = 0,
		Character         = 1,
		Word              = 2,
		EllipsisCharacter = 3,
		EllipsisWord      = 4,
		EllipsisPath      = 5
	}
}
