//
// System.Drawing.Image.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Christian Meyer
// eMail: Christian.Meyer@cs.tum.edu
//
// Many methods are still commented. I'll care about them when all necessary
// classes are implemented.
// 
// Alexandre Pigolkine (pigolkine@gmx.de)
// 
//
namespace System.Drawing {

using System;
using System.Runtime.Remoting;
using System.Runtime.Serialization;
using System.Drawing.Imaging;
using System.IO;

internal class InternalImageInfo {
	Size			imageSize;
	PixelFormat		format;
	int				stride;
	ColorPalette	palette;
	byte[]			image;
	ImageFormat		rawFormat;

	internal InternalImageInfo() {
		palette = new ColorPalette();
		imageSize = new Size(0,0);
		format = PixelFormat.Format32bppArgb;
		image = new byte[0];
		stride = 0;
	}

	internal Size Size {
		get { return imageSize; }
		set { imageSize = value; }
	}

	internal PixelFormat Format {
		get { return format; }
		set { format = value; }
	}

	internal ColorPalette Palette {
		get { return palette; }
		set { palette = value; }
	}

	internal byte[] RawImageBytes {
		get { return image; }
		set { image = value; }
	}

	internal int Stride {
		get { return stride; }
		set { stride = value; }
	}

	internal ImageFormat RawFormat {
		get { return rawFormat; }
		set { rawFormat = value; }
	}
}

[Serializable]
//[ComVisible(true)]

public abstract class Image : MarshalByRefObject, IDisposable , ICloneable, ISerializable {

	internal IImage	implementation_ = null;
	protected Size imageSize_;
	
	// constructor
	public Image () {}

       	private Image (SerializationInfo info, StreamingContext context)
	{
	}

	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
	}
    
	// public methods
	// static
	public static Image FromFile (string filename)
	{
		// Fixme: implement me
		throw new NotImplementedException ();
	}
	
	public static Image FromFile (string filename, bool useEmbeddedColorManagement)
	{
		// Fixme: implement me
		throw new NotImplementedException ();
	}
	
	public static Bitmap FromHbitmap (IntPtr hbitmap)
	{
		// Fixme: implement me
		throw new NotImplementedException ();
	}

	public static Bitmap FromHbitmap (IntPtr hbitmap, IntPtr hpalette)
	{
		// Fixme: implement me
		throw new NotImplementedException ();
	}

	internal static InternalImageInfo Decode( Stream streamIn) {
		Stream stream = streamIn;
		InternalImageInfo	result = new InternalImageInfo();
		if (!stream.CanSeek) {
			// FIXME: if stream.CanSeek == false, copy to a MemoryStream and read nicely 
		}
		ImageCodecInfo[] availableDecoders = ImageCodecInfo.GetImageDecoders();
		long pos = stream.Position;
		ImageCodecInfo codecToUse = null;
		foreach( ImageCodecInfo info in availableDecoders) {
			for (int i = 0; i < info.SignaturePatterns.Length; i++) {
				stream.Seek(pos, SeekOrigin.Begin);
				bool codecFound = true;
				for (int iPattern = 0; iPattern < info.SignaturePatterns[i].Length; iPattern++) {
					byte pattern = (byte)stream.ReadByte();
					pattern &= info.SignatureMasks[i][iPattern];
					if( pattern != info.SignaturePatterns[i][iPattern]) {
						codecFound = false;
						break;
					}
				}
				if (codecFound) {
					codecToUse = info;
					break;
				}
			}
		}
		stream.Seek (pos, SeekOrigin.Begin);
		if (codecToUse != null && codecToUse.decode != null) {
			codecToUse.decode( stream, result);
		}
		return result;
	}
	
	public static int GetPixelFormatSize (PixelFormat pixfmt)
	{
		int result = 0;
		switch (pixfmt) {
			case PixelFormat.Format16bppArgb1555:
			case PixelFormat.Format16bppGrayScale:
			case PixelFormat.Format16bppRgb555:
			case PixelFormat.Format16bppRgb565:
				result = 16;
				break;
			case PixelFormat.Format1bppIndexed:
				result = 1;
				break;
			case PixelFormat.Format24bppRgb:
				result = 24;
				break;
			case PixelFormat.Format32bppArgb:
			case PixelFormat.Format32bppPArgb:
			case PixelFormat.Format32bppRgb:
				result = 32;
				break;
			case PixelFormat.Format48bppRgb:
				result = 48;
				break;
			case PixelFormat.Format4bppIndexed:
				result = 4;
				break;
			case PixelFormat.Format64bppArgb:
			case PixelFormat.Format64bppPArgb:
				result = 64;
				break;
			case PixelFormat.Format8bppIndexed:
				result = 8;
				break;
		}
		return result;
	}

//	public static bool IsAlphaPixelFormat (PixelFormat pixfmt)
//	{
//		// Fixme: implement me
//		throw new NotImplementedException ();
//	}
//	
//	public static bool IsCanonicalPixelFormat (PixelFormat pixfmt)
//	{
//		// Fixme: implement me
//		throw new NotImplementedException ();
//	}
//	
//	public static bool IsExtendedPixelFormat (PixelFormat pixfmt)
//    	{
//		// Fixme: implement me
//		throw new NotImplementedException ();
//	}

	// non-static
	public RectangleF GetBounds (ref GraphicsUnit pageUnit)
	{
		// Fixme: implement me
		throw new NotImplementedException ();
	}
	
	//public EncoderParameters GetEncoderParameterList(Guid encoder);
	//public int GetFrameCount(FrameDimension dimension);
	//public PropertyItem GetPropertyItem(int propid);
	/*
	  public Image GetThumbnailImage(int thumbWidth, int thumbHeight,
	  Image.GetThumbnailImageAbort callback,
	  IntPtr callbackData);
	*/
	
	public void RemovePropertyItem (int propid)
	{
		// Fixme: implement me
		throw new NotImplementedException ();
	}
	
	public void RotateFlip (RotateFlipType rotateFlipType)
	{
		// Fixme: implement me
		throw new NotImplementedException ();
	}

	public void Save (string filename)
	{
		Save( filename, RawFormat);
	}

	internal virtual InternalImageInfo ConvertToInternalImageInfo() {
		return implementation_.ConvertToInternalImageInfo();
	}

	public void Save(Stream stream, ImageFormat format) {
		ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
		foreach( ImageCodecInfo encoder in encoders) {
			if (encoder.FormatID == format.Guid) {
				if(encoder.encode != null) {
					InternalImageInfo imageInfo = ConvertToInternalImageInfo();
					encoder.encode(stream, imageInfo);
				}
				break;
			}
		}
	}

	public void Save(string filename, ImageFormat format) {
		FileStream fs = new FileStream( filename, FileMode.Create);
		Save(fs, format);
		fs.Flush();
		fs.Close();
	}

	//public void Save(Stream stream, ImageCodecInfo encoder, EncoderParameters encoderParams);
	//public void Save(string filename, ImageCodecInfo encoder, EncoderParameters encoderParams);
	//public void SaveAdd(EncoderParameters_ encoderParams);
	//public void SaveAdd(Image image, EncoderParameters_ encoderParams);
	//public int SelectActiveFrame(FrameDimension dimension, int frameIndex);
	//public void SetPropertyItem(PropertyItem propitem);

	// destructor
	~Image() {}

	// properties
	public int Flags {
		get {
			throw new NotImplementedException ();
		}
	}
	
	public Guid[] FrameDimensionsList {
		get {
			throw new NotImplementedException ();
		}
	}
	
	public int Height {
		get {
			return imageSize_.Height;
		}
	}
	
	public float HorizontalResolution {
		get {
			throw new NotImplementedException ();
		}
	}
	
//	public ColorPalette Palette {
//		get {
//			throw new NotImplementedException ();
//		}
//		set {
//			throw new NotImplementedException ();
//		}
//	}
	
	public SizeF PhysicalDimension {
		get {
			throw new NotImplementedException ();
		}
	}
	
	public PixelFormat PixelFormat {
		get {
			return implementation_.PixelFormat;
		}
	}
	
	public int[] PropertyIdList {
		get {
			throw new NotImplementedException ();
		}
	}
	
	public PropertyItem[] PropertyItems {
		get {
			return implementation_.PropertyItems;
		}
	}

	public ImageFormat RawFormat {
		get {
			return implementation_.RawFormat;
		}
	}

	public Size Size {
		get {
			return imageSize_;
		}
	}
	
	public float VerticalResolution {
		get {
			throw new NotImplementedException ();
		}
	}
	
	public int Width {
		get {
			return imageSize_.Width;
		}
	}
	[MonoTODO]
	public void Dispose ()
	{
		implementation_.Dispose();
	}

	[MonoTODO]
	object ICloneable.Clone()
	{
		throw new NotImplementedException ();
	}

}

}
