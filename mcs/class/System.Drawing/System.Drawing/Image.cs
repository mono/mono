//
// System.Drawing.Image.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Christian Meyer
// eMail: Christian.Meyer@cs.tum.edu
//
// Alexandre Pigolkine (pigolkine@gmx.de)
// 
//
namespace System.Drawing {

using System;
using System.Runtime.Remoting;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.IO;

[Serializable]
public abstract class Image : MarshalByRefObject, IDisposable , ICloneable, ISerializable 
{
	public delegate bool GetThumbnailImageAbort ();
	
	internal IntPtr nativeObject = IntPtr.Zero;
	protected Size image_size;
	protected PixelFormat pixel_format;
	protected ColorPalette palette;

	ImageFormat raw_format;
	
	// constructor
	public Image ()
	{
		pixel_format = PixelFormat.Format32bppArgb;
	}

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
		return new Bitmap (filename);
	}
	
	public static Image FromFile (string filename, bool useEmbeddedColorManagement)
	{
		return new Bitmap (filename, useEmbeddedColorManagement);
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

	internal BitmapData Decode (Stream streamIn) 
	{
		Stream stream = streamIn;
		BitmapData result = new BitmapData ();
		if (!stream.CanSeek) {
			// FIXME: if stream.CanSeek == false, copy to a MemoryStream and read nicely 
		}
		ImageCodecInfo[] availableDecoders = ImageCodecInfo.GetImageDecoders();
		long pos = stream.Position;
		ImageCodecInfo codecToUse = null;
		foreach (ImageCodecInfo info in availableDecoders) {
			for (int i = 0; i < info.SignaturePatterns.Length; i++) {
				stream.Seek(pos, SeekOrigin.Begin);
				bool codecFound = true;
				for (int iPattern = 0; iPattern < info.SignaturePatterns[i].Length; iPattern++) {
					byte pattern = (byte)stream.ReadByte();
					pattern &= info.SignatureMasks[i][iPattern];
					if (pattern != info.SignaturePatterns[i][iPattern]) {
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
			codecToUse.decode (this, stream, result);
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

	public static bool IsAlphaPixelFormat (PixelFormat pixfmt)
	{
		bool result = false;
		switch (pixfmt) {
			case PixelFormat.Format16bppArgb1555:
			case PixelFormat.Format32bppArgb:
			case PixelFormat.Format32bppPArgb:
			case PixelFormat.Format64bppArgb:
			case PixelFormat.Format64bppPArgb:
				result = true;
				break;
			case PixelFormat.Format16bppGrayScale:
			case PixelFormat.Format16bppRgb555:
			case PixelFormat.Format16bppRgb565:
			case PixelFormat.Format1bppIndexed:
			case PixelFormat.Format24bppRgb:
			case PixelFormat.Format32bppRgb:
			case PixelFormat.Format48bppRgb:
			case PixelFormat.Format4bppIndexed:
			case PixelFormat.Format8bppIndexed:
				result = false;
				break;
		}
		return result;
	}
	
//	public static bool IsCanonicalPixelFormat (PixelFormat pixfmt)
//	{
//		// Fixme: implement me
//		throw new NotImplementedException ();
//	}
//	
//	public static bool IsExtendedPixelFormat (PixelFormat pixfmt)
//	{
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
		Save (filename, RawFormat);
	}

	public void Save (Stream stream, ImageFormat format)
	{
		ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();

		foreach (ImageCodecInfo encoder in encoders) {
			if (encoder.FormatID != format.Guid)
				continue;
			
			if (encoder.encode == null)
				continue;
			if (!(this is Bitmap))
				continue;
			
			BitmapData bitmap_data = ((Bitmap)this).LockBits (new Rectangle (new Point (0,0), Size),
									  ImageLockMode.ReadOnly, PixelFormat);
			encoder.encode(this, stream, bitmap_data);
			((Bitmap)this).UnlockBits (bitmap_data);
			break;
		}
	}

	public void Save(string filename, ImageFormat format) 
	{
		FileStream fs = new FileStream (filename, FileMode.Create);
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
			return image_size.Height;
		}
	}
	
	public float HorizontalResolution {
		get {
			throw new NotImplementedException ();
		}
	}
	
	public ColorPalette Palette {
		get {
			return palette;
		}
		set {
			palette = value;
		}
	}
	
	public SizeF PhysicalDimension {
		get {
			throw new NotImplementedException ();
		}
	}
	
	public PixelFormat PixelFormat {
		get {
			return pixel_format;
		}
	}
	
	public int[] PropertyIdList {
		get {
			throw new NotImplementedException ();
		}
	}
	
	public PropertyItem[] PropertyItems {
		get {
			throw new NotImplementedException ();
		}
	}

	public ImageFormat RawFormat {
		get {
			return raw_format;
		}
	}

	internal void SetRawFormat (ImageFormat format)
	{
		raw_format = format;
	}

	public Size Size {
		get {
			return image_size;
		}
	}
	
	public float VerticalResolution {
		get {
			throw new NotImplementedException ();
		}
	}
	
	public int Width {
		get {
			return image_size.Width;
		}
	}
	
	public void Dispose ()
	{
		Dispose (true);
	}

	~Image ()
	{
		Dispose (false);
	}

	protected virtual void DisposeResources ()
	{
		GDIPlus.GdipDisposeImage (nativeObject);
	}
	
	void Dispose (bool disposing)
	{
		if (nativeObject != (IntPtr) 0){
			DisposeResources ();
		}
	}
	
	[MonoTODO]
	object ICloneable.Clone()
	{
		throw new NotImplementedException ();
	}

}

}
