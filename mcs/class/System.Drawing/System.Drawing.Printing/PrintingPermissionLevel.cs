//
// System.Drawing.PrintingPermissionLevel.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Dennis Hayes (dennish@raytek.com)
//
using System;
namespace System.Drawing.Printing 
{
	public enum PrintingPermissionLevel {
		AllPrinting = 3,
		DefaultPrinting = 2,
		NoPrinting = 0,
		SafePrinting = 1
	}
}
