//
// System.Drawing.PrintingPermissionLevel.cs
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
// Author: Dennis Hayes (dennish@raytek.com)
//
using System;
namespace System.Drawing.Printing 
{
	public enum PrintingPermissionLevel {
		AllPrinting,
		DefaultPrinting,
		NoPrinting,
		SafePrinting
	}
}
