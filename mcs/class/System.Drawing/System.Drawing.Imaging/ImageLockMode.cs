//
// System.Drawing.ImageLockMode.cs
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
// Author: Dennis Hayes (dennish@raytek.com)
//
using System;
namespace System.Drawing.Imaging 
{
	public enum ImageLockMode {
		ReadOnly,
		ReadWrite,
		UserInputBuffer,
		WriteOnly
	}
}
