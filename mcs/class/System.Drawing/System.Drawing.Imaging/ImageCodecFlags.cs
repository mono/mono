//
// System.Drawing.ImageCodecFlags.cs
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
// Author: Dennis Hayes (dennish@raytek.com)
//
using System;
namespace System.Drawing 
{
	public enum ImageCodecFlags {
		BlockingDecode,
		Builtin,
		Decoder,
		Encoder,
		SeekableEncode,
		SupportBitmap,
		SupportVector,
		System,
		User
	}
}
