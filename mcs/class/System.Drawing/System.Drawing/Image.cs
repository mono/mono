//
// System.Drawing.Image.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Christian Meyer
// eMail: Christian.Meyer@cs.tum.edu
//
// Alexandre Pigolkine (pigolkine@gmx.de)
// Sanjay Gupta (gsanjay@novell.com)
//
namespace System.Drawing {

using System;
using System.Runtime.Remoting;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;

[Serializable]
[ComVisible (true)]
[Editor ("System.Drawing.Design.ImageEditor, " + Consts.AssemblySystem_Drawing_Design, typeof (System.Drawing.Design.UITypeEditor))]
[TypeConverter (typeof(ImageConverter))]
[ImmutableObject (true)]
public abstract class Image : MarshalByRefObject, IDisposable , ICloneable, ISerializable 
{
	public delegate bool GetThumbnailImageAbort ();
	
	internal IntPtr nativeObject = IntPtr.Zero;
	
	protected Size image_size;
	protected PixelFormat pixel_format;
	protected ColorPalette colorPalette;

	protected ImageFormat raw_format;
	
	// constructor
	public Image ()
	{
		pixel_format = PixelFormat.Format32bppArgb;
		colorPalette = new ColorPalette();
	}

	internal Image ( IntPtr nativeObj)
	{
		nativeObject = nativeObj;
	}

	private Image (SerializationInfo info, StreamingContext context)
	{
	}		

	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
	}
    
	// public methods
	// static
	public static Image FromFile ( string filename ) 
	{
		if ( filename == null )
			throw new Exception ( "Value cannot be null, Parameter name : filename" );
		bool exists = File.Exists( filename );
		if ( !exists )
			throw new Exception ( "The path is not of a legal form" );
		
		IntPtr ptr;
		Status status = GDIPlus.GdipLoadImageFromFile ( filename, out ptr );
		GDIPlus.CheckStatus ( status );
		Bitmap bmp = new Bitmap ( ptr );
		return bmp;
	}
	
	public static Image FromFile (string filename, bool useEmbeddedColorManagement)
	{
		if ( filename == null )
			throw new Exception ( "Value cannot be null, Parameter name : filename" );
		bool exists = File.Exists( filename );
		if ( !exists )
			throw new Exception ( "The path is not of a legal form" );
		
		IntPtr ptr;
		Status status;
		if ( useEmbeddedColorManagement )
			status = GDIPlus.GdipLoadImageFromFileICM ( filename, out ptr );
		else
			status = GDIPlus.GdipLoadImageFromFile ( filename, out ptr );
		GDIPlus.CheckStatus ( status );
		Bitmap bmp = new Bitmap ( ptr );
		return bmp;
	}
	
	public static Bitmap FromHbitmap (IntPtr hbitmap)
	{
		IntPtr ptr;
		Status status = GDIPlus.GdipCreateBitmapFromHBITMAP ( hbitmap, IntPtr.Zero, out ptr );
		GDIPlus.CheckStatus ( status );
		Bitmap bmp = new Bitmap ( ptr );
		return bmp;
	}

	public static Bitmap FromHbitmap (IntPtr hbitmap, IntPtr hpalette)
	{
		IntPtr ptr;
		Status status = GDIPlus.GdipCreateBitmapFromHBITMAP ( hbitmap, hpalette, out ptr );
		GDIPlus.CheckStatus ( status );
		Bitmap bmp = new Bitmap ( ptr );
		return bmp;
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
	
	public static bool IsCanonicalPixelFormat (PixelFormat pixfmt)
	{
		bool result = false;
		switch (pixfmt) 
		{
			case PixelFormat.Canonical:
				result = true;
				break;
			default:
				result = false;
				break;
		}
		return result;
	}
	
	public static bool IsExtendedPixelFormat (PixelFormat pixfmt)
	{
		bool result = false;
		switch (pixfmt) 
		{
			case PixelFormat.Extended:
				result = true;
				break;
			default:
				result = false;
				break;
		}
		return result;
	}

	// non-static
	public RectangleF GetBounds (ref GraphicsUnit pageUnit)
	{
		RectangleF rectF;
		Status status = GDIPlus.GdipGetImageBounds ( nativeObject, out rectF, ref pageUnit );
		GDIPlus.CheckStatus ( status );
		return rectF;
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
		Status status = GDIPlus.GdipRemovePropertyItem ( nativeObject, propid );
		GDIPlus.CheckStatus ( status );
	}
	
	public void RotateFlip (RotateFlipType rotateFlipType)
	{
		Status status = GDIPlus.GdipImageRotateFlip ( nativeObject, rotateFlipType );
		GDIPlus.CheckStatus ( status );
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
			encoder.encode(this, stream);
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
	
	public int SelectActiveFrame(FrameDimension dimension, int frameIndex)
	{
		Status status = GDIPlus.GdipImageSelectActiveFrame ( nativeObject, dimension.Guid, (uint) frameIndex );
		GDIPlus.CheckStatus ( status );
		return frameIndex;
	}

	public void SetPropertyItem(PropertyItem propItem)
	{
		/*Status status = GDIPlus.GdipSetPropertyItem ( nativeObject, propItem );
		GDIPlus.CheckStatus ( status );*/
	}

	// properties
	public int Flags {
		get {
			uint flag;
			Status status = GDIPlus.GdipGetImageFlags ( nativeObject , out flag );
			GDIPlus.CheckStatus ( status );
			return ( int ) flag;
		}
	}
	
	public Guid[] FrameDimensionsList {
		get {
			throw new NotImplementedException ();
		}
	}
	
	public int Height {
		get {
			uint height;
			Status status = GDIPlus.GdipGetImageHeight ( nativeObject , out height );
			GDIPlus.CheckStatus ( status );
			return ( int ) height;
		}
	}
	
	public float HorizontalResolution {
		get {
			float resolution;
			Status status = GDIPlus.GdipGetImageHorizontalResolution ( nativeObject , out resolution );
			GDIPlus.CheckStatus ( status );
			return resolution;
		}
	}
	
	public ColorPalette Palette {
		get {
			int size;
			Status status = GDIPlus.GdipGetImagePaletteSize ( nativeObject , out size );
			GDIPlus.CheckStatus ( status );
			ColorPalette palette;
			status = GDIPlus.GdipGetImagePalette ( nativeObject , out palette, size );
			GDIPlus.CheckStatus ( status );
			return palette;
		}
		set {
			Status status = GDIPlus.GdipSetImagePalette ( nativeObject , value );
			GDIPlus.CheckStatus ( status );
		}
	}
	
	public SizeF PhysicalDimension {
		get {
			float height, width ;
			Status status = GDIPlus.GdipGetImageDimension ( nativeObject , out width, out height);
			GDIPlus.CheckStatus ( status );
			SizeF size = new SizeF ( width, height );
			return size;
		}
	}
	
	public PixelFormat PixelFormat {
		get {
			Status status = GDIPlus.GdipGetImagePixelFormat ( nativeObject, out pixel_format );
			GDIPlus.CheckStatus ( status );
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
			Guid guid;
			Status status = GDIPlus.GdipGetImageRawFormat ( nativeObject, out guid );
			GDIPlus.CheckStatus ( status );
			raw_format = new ImageFormat ( guid );
			return raw_format;
		}
	}

	internal void SetRawFormat (ImageFormat format)
	{
		raw_format = format;
	}

	public Size Size {
		get {
			int height = this.Height;
			int width = this.Width;
			Size size = new Size ( width, height );
			return size;
		}
	}
	
	public float VerticalResolution {
		get {
			float resolution;
			Status status = GDIPlus.GdipGetImageVerticalResolution ( nativeObject , out resolution );
			GDIPlus.CheckStatus ( status );
			return resolution;
		}
	}
	
	public int Width {
		get {
			uint width ;
			Status status = GDIPlus.GdipGetImageWidth ( nativeObject , out width);
			GDIPlus.CheckStatus ( status );
			return (int) width;
			//return image_size.Width;
		}
	}
	
	internal IntPtr NativeObject{
		get{
			return nativeObject;
		}
		set	{
			nativeObject = value;
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
	
	protected virtual void Dispose (bool disposing)
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
