//
// System.Drawing.Imaging.EncoderValue.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Dennis Hayes (dennish@raytek.com)
//
using System;
namespace System.Drawing.Imaging 
{
	[Serializable]
	public enum EncoderValue {
		ColorTypeCMYK = 0,
		ColorTypeYCCK = 1,
		CompressionCCITT3 = 3,
		CompressionCCITT4 = 4,
		CompressionLZW = 2,
		CompressionNone = 6,
		CompressionRle = 5,
		Flush = 20,
		FrameDimensionPage = 23,
		FrameDimensionResolution = 22,
		FrameDimensionTime = 21,
		LastFrame = 19,
		MultiFrame = 18,
		RenderNonProgressive = 12,
		RenderProgressive = 11,
		ScanMethodInterlaced = 7,
		ScanMethodNonInterlaced = 8,
		TransformFlipHorizontal = 16,
		TransformFlipVertical = 17,
		TransformRotate180 = 14,
		TransformRotate270 = 15,
		TransformRotate90 = 13,
		VersionGif87 = 9,
		VersionGif89 = 10
	}
}
