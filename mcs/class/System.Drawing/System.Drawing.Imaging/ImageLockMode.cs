//
// System.Drawing.Imaging.ImageLockMode.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Dennis Hayes (dennish@raytek.com)
//
using System;
namespace System.Drawing.Imaging 
{
	[Serializable]
	public enum ImageLockMode {
		ReadOnly = 1,
		ReadWrite = 3,
		UserInputBuffer = 4,
		WriteOnly = 2
	}
}
