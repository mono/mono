//
// System.Drawing.PrinterResolutionKind.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Dennis Hayes (dennish@raytek.com)
//
using System;
namespace System.Drawing.Printing 
{
	public enum PrinterResolutionKind {
		Custom = 0,
		Draft = -1,
		High = -4,
		Low = -2,
		Medium = -3
	}
}
