//
// System.Drawing.Imaging.EncoderParameterValueType.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Dennis Hayes (dennish@raytek.com)
//
using System;
namespace System.Drawing.Imaging 
{
	[Serializable]
	public enum EncoderParameterValueType {
		ValueTypeAscii = 2,
		ValueTypeByte = 1,
		ValueTypeLong = 4,
		ValueTypeLongRange = 6,
		ValueTypeRational = 5,
		ValueTypeRationalRange = 8,
		ValueTypeShort = 3,
		ValueTypeUndefined = 7
	}
}
