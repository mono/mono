//
// System.Drawing.Imaging.ImageCodecFlags.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Dennis Hayes (dennish@raytek.com)
//
using System;
namespace System.Drawing.Imaging
{
	[Flags]
	[Serializable]
	public enum ImageCodecFlags {
		BlockingDecode = 32,
		Builtin = 65536,
		Decoder = 2,
		Encoder = 1,
		SeekableEncode = 16,
		SupportBitmap = 4,
		SupportVector = 8,
		System = 131072,
		User = 262144
	}
}
