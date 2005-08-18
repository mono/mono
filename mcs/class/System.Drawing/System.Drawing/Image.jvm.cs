//
// System.Drawing.Image.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: 	Christian Meyer (Christian.Meyer@cs.tum.edu)
// 		Alexandre Pigolkine (pigolkine@gmx.de)
//		Jordi Mas i Hernandez (jordi@ximian.com)
//
namespace System.Drawing {

using System;
using System.Runtime.Remoting;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using BufferedImage = java.awt.image.BufferedImage;
using java.io;
using javax.imageio;
using javax.imageio.stream;
using vmw.common;
using awt = java.awt;
using image = java.awt.image;

public abstract class Image : MarshalByRefObject, IDisposable , ICloneable
{	
	#region Vars	
	java.awt.Image [] _nativeObjects;

	//consider using Image[] to support many thumbnails per Image
	java.awt.Image [] _thumbnails;
	int _currentObject;
	Guid _dimension;
	ImageFormat _format;
	#endregion

	#region Constructor
	public void Dispose ()
	{
	}

	protected virtual void DisposeResources ()
	{
	}
	
	protected virtual void Dispose (bool disposing)
	{
	}

	// Derived classes must call Initialize () when they use this constructor
	protected Image () {
	}
 
	protected Image (Image orig) {
		_nativeObjects = CloneNativeObjects(orig._nativeObjects);
		_thumbnails = CloneNativeObjects(orig._thumbnails);
		_format = orig._format;
		_currentObject = orig._currentObject;
		_dimension = orig._dimension;
	}

	protected Image (java.awt.Image nativeObject, ImageFormat format)
		:this (new java.awt.Image [] {nativeObject}, format) {}

	protected Image (java.awt.Image [] nativeObjects, ImageFormat format)
        :this (nativeObjects, format, FrameDimension.Page.Guid)	{}

	protected Image (java.awt.Image [] nativeObjects, ImageFormat format, Guid dimension) {
		Initialize (nativeObjects, null, format, dimension);
	}

	protected void Initialize (java.awt.Image [] nativeObjects, java.awt.Image [] thumbnails, ImageFormat format, Guid dimension) {
		_format = format;
		_nativeObjects = nativeObjects;
		_thumbnails = thumbnails;
		_currentObject = 0;
		_dimension = dimension;
	}

	#endregion
	
	#region Internals

	internal java.awt.Image NativeObject {
		get {
		  return _nativeObjects [_currentObject];
		}
	}

	protected java.awt.Image this [int i] {
		get {
			return _nativeObjects [i];
		}
	}

	protected int NativeObjectsCount {
		get {
			return _nativeObjects.Length;
		}
	}

	#endregion
    
	#region FromFile
	public static Image FromFile(string filename)
	{
		//FIXME: check if it's not a metafile
		return new Bitmap (filename);
	}
	
	public static Image FromFile(string filename, bool useIcm)
	{
		//FIXME: check if it's not a metafile
		return new Bitmap (filename, useIcm);
	}
	#endregion

	#region GetThumbnailImageAbort
	[Serializable]
	public delegate bool GetThumbnailImageAbort();
	#endregion

	#region Clone
	public virtual object Clone() {
		throw new NotImplementedException ("Must be implemented in child class");
	}
	#endregion

	// static
	#region FromStream
	public static Image FromStream (Stream stream)
	{
		//FIXME: check if it's not a metafile
		return new Bitmap (stream);
	}
	
	public static Image FromStream (Stream stream, bool useIcm)
	{
		//FIXME: check if it's not a metafile
		return new Bitmap (stream, useIcm);
	}
	#endregion

	#region GetPixelFormatSize
	public static int GetPixelFormatSize(PixelFormat pixfmt)
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
	#endregion

	#region IsAlphaPixelFormat
	public static bool IsAlphaPixelFormat(PixelFormat pixfmt)
	{
		return (pixfmt & PixelFormat.Alpha) != PixelFormat.Undefined;
	}
	#endregion
	
	#region GetCanonicalPixelFormat [TODO]
	public static bool IsCanonicalPixelFormat (PixelFormat pixfmt)
	{
		return (pixfmt & PixelFormat.Canonical) != PixelFormat.Undefined;
	}
	#endregion
	
	#region IsextendedPixelFormat [TODO]
	public static bool IsExtendedPixelFormat (PixelFormat pixfmt)
	{
		return (pixfmt & PixelFormat.Extended) != PixelFormat.Undefined;
	}
	#endregion

	// non-static
	#region GetBounds
	public RectangleF GetBounds (ref GraphicsUnit pageUnit)
	{	
		int w = NativeObject.getWidth(null);
		int h = NativeObject.getHeight(null);

		pageUnit = GraphicsUnit.Pixel; //java.awt.Image always returns pixels

		return new RectangleF((float)0,(float)0,(float)w,(float)h);
	}
	#endregion
	
	#region GetEncoderParameterList [TODO]
	public EncoderParameters GetEncoderParameterList(Guid encoder)
	{
		throw new NotImplementedException ();
	}
	#endregion
	
	#region GetFrameCount
	public int GetFrameCount(FrameDimension dimension)
	{
		return _nativeObjects.Length;
	}
	#endregion
	
	#region GetPropertyItem [TODO]
	public PropertyItem GetPropertyItem(int propid)
	{
		throw new NotImplementedException ();
	}
	#endregion

	#region RemovePropertyItem [TODO]
	public void RemovePropertyItem (int propid)
	{		
		throw new NotImplementedException ();
	}
	#endregion
	
	#region RotateFlip [TODO]
	public void RotateFlip (RotateFlipType rotateFlipType)
	{		
		throw new NotImplementedException ();
	}
	#endregion

	#region Save
	protected abstract void InternalSave (ImageOutputStream output, Guid clsid);

	public void Save (Stream stream, ImageCodecInfo encoder, EncoderParameters encoderParams)
	{
		//FIXME: ignoring encoderParams
		java.io.OutputStream jos = vmw.common.IOUtils.ToOutputStream (stream);
		MemoryCacheImageOutputStream output = new MemoryCacheImageOutputStream(jos);
		
		InternalSave (output, encoder.Clsid);
	}
	
	public void Save(string filename, ImageCodecInfo encoder, EncoderParameters encoderParams)
	{
		//FIXME: ignoring encoderParams
		java.io.File jf = vmw.common.IOUtils.getJavaFile (filename);
		FileImageOutputStream output = new FileImageOutputStream (jf);
		InternalSave (output, encoder.Clsid);
	}

	public void Save (string filename)
	{
		Save (filename, ImageFormat.Png);
	}

	public void Save (Stream stream, ImageFormat format)
	{
		Save (stream, ImageCodecInfo.FindEncoder (
			ImageCodecInfo.ImageFormatToClsid (format)), null);
	}

	public void Save(string filename, ImageFormat format) 
	{
		Save (filename, ImageCodecInfo.FindEncoder (
			ImageCodecInfo.ImageFormatToClsid (format)), null);
	}
	#endregion

	#region SaveAdd [TODO]
	public void SaveAdd(EncoderParameters encoderParams)
	{
		throw new NotImplementedException ();
	}
	
	public void SaveAdd(Image image, EncoderParameters encoderParams)
	{
		throw new NotImplementedException ();
	}
	#endregion
	
	#region SelectActiveFrame
	public int SelectActiveFrame(FrameDimension dimension, int frameIndex)
	{
		if (dimension.Guid != _dimension) //TBD check if this is the correct exception and message
            throw new ArgumentException (dimension + " dimension is not supported");
		_currentObject = frameIndex;
		//TBD check what net makes when index out of bounds
		return frameIndex;
	}
	#endregion
	
	#region SetPropertyItem [TODO]
	public void SetPropertyItem(PropertyItem propitem)
	{
		throw new NotImplementedException ();
	}
	#endregion

	// properties
	#region Flags[TODO]
	public int Flags 
	{
		get {
			throw new NotImplementedException ();	
		}
	}
	#endregion

	#region FrameDimensionsList
	public Guid[] FrameDimensionsList {
		get {
			return new Guid [] {_dimension};
		}
	}
	#endregion

	#region Height
	public int Height 
	{
		get {
			return NativeObject.getHeight(null);
		}
	}
	#endregion
	
	#region HorisontalDimention [TODO]
	public float HorizontalResolution 
	{
		get {
			throw new NotImplementedException ();
		}
	}
	#endregion
	
	#region ColorPalette [TODO]
	public ColorPalette Palette 
	{
		get {							
			
			throw new NotImplementedException ();
		}
		set {
			throw new NotImplementedException ();
		}
	}
	#endregion
		
	#region PhysicalDimension [TODO]
	public SizeF PhysicalDimension 
	{
		get {
			throw new NotImplementedException ();
		}
	}
	#endregion
	
	#region PixelFormat
	abstract protected PixelFormat InternalPixelFormat {get;}

	public PixelFormat PixelFormat {
		get {
			return InternalPixelFormat;
		}
	}
	#endregion
		
	#region PropertiIdList [TODO]
	public int[] PropertyIdList 
	{
		get {
			throw new NotImplementedException ();
		}
	}
	#endregion
		
	#region PropertItems [TODO]
	public PropertyItem[] PropertyItems 
	{
		get {
			throw new NotImplementedException ();
		}
	}
	#endregion

	#region RawFormat
	public ImageFormat RawFormat 
	{
		get {
			return _format;
		}
	}
	#endregion

	#region Size
	public Size Size 
	{
		get {
			return new Size(Width, Height);
		}
	}
	#endregion
	
	#region VerticalResolution [TODO]
	public float VerticalResolution 
	{
		get {
			throw new NotImplementedException();
		}
	}
	#endregion
	
	#region Width
	public int Width 
	{
		get {
			return NativeObject.getWidth(null);
		}
	}
	#endregion	

	public Image GetThumbnailImage(int thumbWidth, int thumbHeight, Image.GetThumbnailImageAbort callback, IntPtr callbackData)
	{
		awt.Image img = NativeObject;			
		if (_thumbnails != null && _currentObject < _thumbnails.Length) {
			if (_thumbnails[_currentObject] != null)
				img = _thumbnails[_currentObject];
		}

		if (img.getHeight(null) != thumbHeight || img.getWidth(null) != thumbWidth)
			img = img.getScaledInstance(thumbWidth, thumbHeight, awt.Image.SCALE_DEFAULT);

		return ImageFromNativeImage(img, RawFormat);
	}
#if INTPTR_SUPPORT
	public static Bitmap FromHbitmap(IntPtr hbitmap)
	{		
		throw new NotImplementedException ();
	}	

	public static Bitmap FromHbitmap(IntPtr hbitmap, IntPtr hpalette)
	{		
		throw new NotImplementedException ();
	}
#endif

	internal static Image ImageFromNativeImage(awt.Image nativeImage, ImageFormat format) {
		if (nativeImage is BufferedImage)
			return new Bitmap(nativeImage, format);

		throw new ArgumentException("Invalid image type");
	}

	protected abstract awt.Image [] CloneNativeObjects(awt.Image [] src);
}

}
