//
// System.Drawing.Imaging.EmfType.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Dennis Hayes (dennish@raytek.com)
//
using System;
namespace System.Drawing.Imaging 
{
	[Serializable]
	public enum EmfType {
		EmfOnly = 3,
		EmfPlusDual = 5,
		EmfPlusOnly = 4
	}
}
