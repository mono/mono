//
// System.Drawing.EncoderValue.cs
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
// Author: Dennis Hayes (dennish@raytek.com)
//
using System;
namespace System.Drawing.Imaging 
{
	public enum EncoderValue {
		ColorTypeCMYK,
		ColorTypeYCCK,
		CompressionCCITT3,
		CompressionCCITT4,
		CompressionLZW,
		CompressionNone,
		CompressionRle,
		Flush,
		FrameDimensionPage,
		FrameDimensionResolution,
		FrameDimensionTime,
		LastFrame,
		MultiFrame,
		RenderNonProgressive,
		RenderProgressive,
		ScanMethodInterlaced,
		ScanMethodNonInterlaced,
		TransformFlipHorizontal,
		TransformFlipVertical,
		TransformRotate180,
		TransformRotate270,
		TransformRotate90,
		VersionGif87,
		VersionGif89
	}
}
