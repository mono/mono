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

internal interface IImage : IDisposable
{
	RectangleF GetBounds (ref GraphicsUnit pageUnit);
		
	void RemovePropertyItem (int propid);

	void SetPropertyItem (PropertyItem item);
	
	PropertyItem GetPropertyItem (int propid);
	
	int GetFrameCount (FrameDimension dimension);
	
	int SelectActiveFrame (FrameDimension dimension, int frameIndex);
	
	void RotateFlip (RotateFlipType rotateFlipType);

	// Possible MCS bug
//	   Image GetThumbnailImage (
//		   int thumbWidth, int thumbHeight,
//		   System.Drawing.Image.GetThumbnailImageAbort callback,
//		   IntPtr callbackData);

	InternalImageInfo ConvertToInternalImageInfo ();

	void Save (string filename);

	void Save (Stream stream, ImageFormat format);
	void Save (string filename, ImageFormat format);

	// properties
	int Flags { get; }
	
	Guid [] FrameDimensionsList { get; }
	
	int Height { get; }
	
	float HorizontalResolution { get; }
		
	SizeF PhysicalDimension { get; }
	
	PixelFormat PixelFormat { get; }
	
	int [] PropertyIdList { get; }
	
	PropertyItem [] PropertyItems { get; }

	ImageFormat RawFormat { get; }

	Size Size { get; }
	
	float VerticalResolution { get; }
	
	int Width { get; }
	
	ColorPalette Palette { get; set; }
}
}
