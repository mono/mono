//
// System.Drawing.Image.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Christian Meyer
// eMail: Christian.Meyer@cs.tum.edu
// Alexandre Pigolkine(pigolkine@gmx.de)
//
namespace System.Drawing {

using System;
using System.Runtime.Remoting;
using System.Runtime.Serialization;
using System.Drawing.Imaging;
using System.IO;

//[Serializable]
//[ComVisible(true)]

internal interface IImage : IDisposable {

	
//	static int GetPixelFormatSize (PixelFormat pixfmt)
//	{
//		// Fixme: implement me
//		throw new NotImplementedException ();
//	}
//
//	static bool IsAlphaPixelFormat (PixelFormat pixfmt)
//	{
//		// Fixme: implement me
//		throw new NotImplementedException ();
//	}
//	
//	static bool IsCanonicalPixelFormat (PixelFormat pixfmt)
//	{
//		// Fixme: implement me
//		throw new NotImplementedException ();
//	}
//	
//	static bool IsExtendedPixelFormat (PixelFormat pixfmt)
//    	{
//		// Fixme: implement me
//		throw new NotImplementedException ();
//	}

	// non-static
	RectangleF GetBounds (ref GraphicsUnit pageUnit);
	
	//EncoderParameters GetEncoderParameterList(Guid encoder);
	//int GetFrameCount(FrameDimension dimension);
	//PropertyItem GetPropertyItem(int propid);
	/*
	  Image GetThumbnailImage(int thumbWidth, int thumbHeight,
	  Image.GetThumbnailImageAbort callback,
	  IntPtr callbackData);
	*/
	
	void RemovePropertyItem (int propid);
	
	void RotateFlip (RotateFlipType rotateFlipType);

	InternalImageInfo ConvertToInternalImageInfo();

	void Save (string filename);

	void Save(Stream stream, ImageFormat format);
	void Save(string filename, ImageFormat format);
	//void Save(Stream stream, ImageCodecInfo encoder,
	//                 EncoderParameters encoderParams);
	//void Save(string filename, ImageCodecInfo encoder,
	//                 EncoderParameters encoderParams);
	//void SaveAdd(EncoderParameters_ encoderParams);
	//void SaveAdd(Image image, EncoderParameters_ encoderParams);
	//int SelectActiveFrame(FrameDimension dimension, int frameIndex);
	//void SetPropertyItem(PropertyItem propitem);

	// properties
	int Flags {
		get ;
	}
	
	Guid[] FrameDimensionsList {
		get ;
	}
	
	int Height {
		get ;
	}
	
	float HorizontalResolution {
		get ;
	}
	
//	ColorPalette Palette {
//		get {
//			throw new NotImplementedException ();
//		}
//		set {
//			throw new NotImplementedException ();
//		}
//	}
	
	SizeF PhysicalDimension {
		get ;
	}
	
	PixelFormat PixelFormat {
		get ;
	}
	
	int[] PropertyIdList {
		get ;
	}
	
	PropertyItem[] PropertyItems {get;}
	ImageFormat RawFormat {get;}

	Size Size {
		get ;
	}
	
	float VerticalResolution {
		get ;
	}
	
	int Width {
		get ;
	}

}

}
