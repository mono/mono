//
// System.Drawing.StringUnit.cs
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
// Author: Dennis Hayes (dennish@raytek.com)
//


using System;
namespace System.Drawing 
{
	public enum  StringUnit{
		World      = 0x00,
		Display    = 0x01,
		Pixel      = 0x02,
		Point      = 0x03,
		Inch       = 0x04,
		Document   = 0x05,
		Millimeter = 0x06,
		Em         = 0x20
	}
}
