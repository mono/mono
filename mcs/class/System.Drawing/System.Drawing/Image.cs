//
// System.Drawing.Image.cs
//
// Copyright (C) 2002 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004 Novell, Inc.  http://www.novell.com
//
// Author: 	Christian Meyer (Christian.Meyer@cs.tum.edu)
// 		Alexandre Pigolkine (pigolkine@gmx.de)
//		Jordi Mas i Hernandez (jordi@ximian.com)
//		Sanjay Gupta (gsanjay@novell.com)
//		Ravindra (rkumar@novell.com)
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Runtime.Remoting;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;

namespace System.Drawing
{
[Serializable]
[ComVisible (true)]
[Editor ("System.Drawing.Design.ImageEditor, " + Consts.AssemblySystem_Drawing_Design, typeof (System.Drawing.Design.UITypeEditor))]
[TypeConverter (typeof(ImageConverter))]
[ImmutableObject (true)]
public abstract class Image : MarshalByRefObject, IDisposable , ICloneable, ISerializable 
{
	public delegate bool GetThumbnailImageAbort();
	
	internal IntPtr nativeObject = IntPtr.Zero;	
	ColorPalette colorPalette;
	
	
	// constructor
	internal  Image()
	{		
		colorPalette = new ColorPalette();
	}
	
	private Image (SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry serEnum in info) {
			if (String.Compare(serEnum.Name, "Data", true) == 0) {
				byte[] bytes = (byte[]) serEnum.Value;

				if (bytes != null) {
					InitFromStream(new MemoryStream(bytes));
				}
			}
		}
	}
	
	[MonoTODO]	
	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		throw new NotImplementedException();
	}
    
	// public methods
	// static
	public static Image FromFile(string filename)
	{
		return new Bitmap (filename);
	}
	
	public static Image FromFile(string filename, bool useEmbeddedColorManagement)
	{
		return new Bitmap (filename, useEmbeddedColorManagement);
	}

	[MonoTODO]	
	public static Bitmap FromHbitmap(IntPtr hbitmap)
	{		
		throw new NotImplementedException ();
	}

	[MonoTODO]	
	public static Bitmap FromHbitmap(IntPtr hbitmap, IntPtr hpalette)
	{		
		throw new NotImplementedException ();
	}

	public static Image FromStream (Stream stream)
	{
		return new Bitmap (stream);
	}
	
	public static Image FromStream (Stream stream, bool useECM)
	{
		return new Bitmap (stream, useECM);
	}

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

	public static bool IsAlphaPixelFormat(PixelFormat pixfmt)
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
		return ((pixfmt & PixelFormat.Canonical) != 0);
	}
	
	public static bool IsExtendedPixelFormat (PixelFormat pixfmt)
	{
		return ((pixfmt & PixelFormat.Extended) != 0);
	}

	internal void InitFromStream (Stream stream)
	{
		if (Environment.OSVersion.Platform == (PlatformID) 128) {
			// Unix, with libgdiplus
			// We use a custom API for this, because there's no easy way
			// to get the Stream down to libgdiplus.  So, we wrap the stream
			// with a set of delegates.
			GDIPlus.GdiPlusStreamHelper sh = new GDIPlus.GdiPlusStreamHelper (stream);
			IntPtr imagePtr;

			Status st = GDIPlus.GdipLoadImageFromDelegate_linux (sh.GetBytesDelegate, sh.PutBytesDelegate,
									sh.SeekDelegate, sh.CloseDelegate, sh.SizeDelegate,
										     out imagePtr);
			GDIPlus.CheckStatus (st);
			nativeObject = imagePtr;
		} else {
			// this is MS-land
			// FIXME
			// We can't call the native gdip functions here, because they expect
			// a COM IStream interface.  So, a hack is to create a tmp file, read
			// the stream, and then load from the tmp file.
			// This is an ugly hack.
			throw new NotImplementedException ("Bitmap.InitFromStream (win32)");
		}
	}

	// non-static	
	public RectangleF GetBounds (ref GraphicsUnit pageUnit)
	{	
		RectangleF source;			
		
		Status status = GDIPlus.GdipGetImageBounds (nativeObject, out source, ref pageUnit);
		GDIPlus.CheckStatus (status);		
		
		return source;
	}
	
	public EncoderParameters GetEncoderParameterList(Guid format)
	{
		Status status;
		uint sz;

		status = GDIPlus.GdipGetEncoderParameterListSize (nativeObject, ref format, out sz);
		GDIPlus.CheckStatus (status);

		IntPtr rawEPList = Marshal.AllocHGlobal ((int) sz);
		EncoderParameters eps;

		try {
			status = GDIPlus.GdipGetEncoderParameterList (nativeObject, ref format, sz, rawEPList);
			eps = EncoderParameters.FromNativePtr (rawEPList);
			GDIPlus.CheckStatus (status);
		} catch {
			Marshal.FreeHGlobal (rawEPList);
			throw;
		}

		Marshal.FreeHGlobal (rawEPList);

		return eps;
	}
	
	public int GetFrameCount(FrameDimension dimension)
	{
		int count;
		Guid guid = dimension.Guid;
		Status status = GDIPlus.GdipImageGetFrameCount (nativeObject, ref guid, out  count); 

		GDIPlus.CheckStatus (status);		
		
		return count;
		
	}
	
	public PropertyItem GetPropertyItem(int propid)
	{
		int propSize;
		IntPtr property;
		PropertyItem item = new PropertyItem ();
		GdipPropertyItem gdipProperty = new GdipPropertyItem ();
		Status status;
			
		status = GDIPlus.GdipGetPropertyItemSize (nativeObject, propid, 
									out propSize);
		GDIPlus.CheckStatus (status);

		/* Get PropertyItem */
		property = Marshal.AllocHGlobal (propSize);
		status = GDIPlus.GdipGetPropertyItem (nativeObject, propid, propSize,  
										property);
		GDIPlus.CheckStatus (status);
		gdipProperty = (GdipPropertyItem) Marshal.PtrToStructure ((IntPtr)property, 
								typeof (GdipPropertyItem));						
		GdipPropertyItem.MarshalTo (gdipProperty, item);								
		
		Marshal.FreeHGlobal (property);
		return item;
	}
	
	public Image GetThumbnailImage(int thumbWidth, int thumbHeight, Image.GetThumbnailImageAbort callback, IntPtr callbackData)
	{
		Status		status;
		Image		ThumbNail;
		Graphics	g;

		ThumbNail=new Bitmap(thumbWidth, thumbHeight);
		g=Graphics.FromImage(ThumbNail);
		
		status = GDIPlus.GdipDrawImageRectRectI(g.nativeObject, nativeObject,
					0, 0, thumbWidth, thumbHeight,
					0, 0, this.Width, this.Height,
					GraphicsUnit.Pixel, IntPtr.Zero, null, IntPtr.Zero);
                GDIPlus.CheckStatus (status);
		g.Dispose();

		return(ThumbNail);
	}
	
	
	public void RemovePropertyItem (int propid)
	{		
		Status status = GDIPlus.GdipRemovePropertyItem (nativeObject, propid);
		GDIPlus.CheckStatus (status);					
	}	
	
	public void RotateFlip (RotateFlipType rotateFlipType)
	{			
		Status status = GDIPlus.GdipImageRotateFlip (nativeObject, rotateFlipType);
		GDIPlus.CheckStatus (status);				
	}

	internal ImageCodecInfo findEncoderForFormat (ImageFormat format)
	{
		ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();			
		ImageCodecInfo encoder = null;
		
		if (format.Guid.Equals (ImageFormat.MemoryBmp.Guid))
			format = ImageFormat.Bmp;
	
		/* Look for the right encoder for our format*/
		for (int i = 0; i < encoders.Length; i++) {
			if (encoders[i].FormatID.Equals (format.Guid)) {
				encoder = encoders[i];
				break;
			}			
		}

		return encoder;
	}

	public void Save (string filename)
	{
		Save (filename, RawFormat);
	}

	public void Save(string filename, ImageFormat format) 
	{
		ImageCodecInfo encoder = findEncoderForFormat (format);

		if (encoder == null)
			throw new ArgumentException ("No codec available for format:" + format.Guid);

		Save (filename, encoder, null);
	}

	public void Save(string filename, ImageCodecInfo encoder, EncoderParameters encoderParams)
	{
		Status st;
		Guid guid = encoder.Clsid;

		if (encoderParams == null) {
			st = GDIPlus.GdipSaveImageToFile (nativeObject, filename, ref guid, IntPtr.Zero);
		} else {
			IntPtr nativeEncoderParams = encoderParams.ToNativePtr ();
			st = GDIPlus.GdipSaveImageToFile (nativeObject, filename, ref guid, nativeEncoderParams);
			Marshal.FreeHGlobal (nativeEncoderParams);
		}

		GDIPlus.CheckStatus (st);
	}

	public void Save (Stream stream, ImageFormat format)
	{
		ImageCodecInfo encoder = findEncoderForFormat (format);

		if (encoder == null)
			throw new ArgumentException ("No codec available for format:" + format.Guid);

		Save (stream, encoder, null);
	}

	public void Save(Stream stream, ImageCodecInfo encoder, EncoderParameters encoderParams)
	{
		Status st;
		Guid guid = encoder.Clsid;

		if (Environment.OSVersion.Platform == (PlatformID) 128) {
			GDIPlus.GdiPlusStreamHelper sh = new GDIPlus.GdiPlusStreamHelper (stream);
			if (encoderParams == null) {
				st = GDIPlus.GdipSaveImageToDelegate_linux (nativeObject, sh.GetBytesDelegate, sh.PutBytesDelegate,
						sh.SeekDelegate, sh.CloseDelegate, sh.SizeDelegate, ref guid, IntPtr.Zero);
			} else {
				IntPtr nativeEncoderParams = encoderParams.ToNativePtr ();
				st = GDIPlus.GdipSaveImageToDelegate_linux (nativeObject, sh.GetBytesDelegate, sh.PutBytesDelegate,
						sh.SeekDelegate, sh.CloseDelegate, sh.SizeDelegate, ref guid, nativeEncoderParams);
				Marshal.FreeHGlobal (nativeEncoderParams);
			}
		} else {
			throw new NotImplementedException ("Image.Save(Stream) (win32)");
		}
		GDIPlus.CheckStatus (st);
	}
	
	public void SaveAdd (EncoderParameters encoderParams)
	{
		Status st;
		
		IntPtr nativeEncoderParams = encoderParams.ToNativePtr ();
		st = GDIPlus.GdipSaveAdd (nativeObject, nativeEncoderParams);
		Marshal.FreeHGlobal (nativeEncoderParams);
		GDIPlus.CheckStatus (st);
	}
		
	public void SaveAdd (Image image, EncoderParameters encoderParams)
	{
		Status st;
		
		IntPtr nativeEncoderParams = encoderParams.ToNativePtr ();
		st = GDIPlus.GdipSaveAddImage (nativeObject, image.NativeObject, nativeEncoderParams);
		Marshal.FreeHGlobal (nativeEncoderParams);
		GDIPlus.CheckStatus (st);
	}
		
	public int SelectActiveFrame(FrameDimension dimension, int frameIndex)
	{
		Guid guid = dimension.Guid;		
		Status st = GDIPlus.GdipImageSelectActiveFrame (nativeObject, ref guid, frameIndex);
		
		GDIPlus.CheckStatus (st);			
		
		return frameIndex;		
	}
	
	public void SetPropertyItem(PropertyItem propitem)
	{
		IntPtr property;
		int size = Marshal.SizeOf (typeof(GdipPropertyItem));
		property = Marshal.AllocHGlobal (size);

		Marshal.StructureToPtr (propitem, property, true);
		Status status = GDIPlus.GdipSetPropertyItem (nativeObject, property);
		GDIPlus.CheckStatus (status);
	}

	// properties	
	[Browsable (false)]
	public int Flags {
		get {
			int flags;
			
			Status status = GDIPlus.GdipGetImageFlags (nativeObject, out flags);			
			GDIPlus.CheckStatus (status);						
			return flags;			
		}
	}
	
	[Browsable (false)]
	public Guid[] FrameDimensionsList {
		get {
			uint found;
			Status status = GDIPlus.GdipImageGetFrameDimensionsCount (nativeObject, out found);
			GDIPlus.CheckStatus (status);
			Guid [] guid = new Guid [found];
			status = GDIPlus.GdipImageGetFrameDimensionsList (nativeObject, guid, found);
			GDIPlus.CheckStatus (status);  
			return guid;
		}
	}

	[DefaultValue (false)]
	[Browsable (false)]
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	public int Height {
		get {
			int height;			
			Status status = GDIPlus.GdipGetImageHeight (nativeObject, out height);		
			GDIPlus.CheckStatus (status);			
			
			return height;
		}
	}
	
	public float HorizontalResolution {
		get {
			float resolution;
			
			Status status = GDIPlus.GdipGetImageHorizontalResolution (nativeObject, out resolution);			
			GDIPlus.CheckStatus (status);			
			
			return resolution;
		}
	}
	
	[Browsable (false)]
	public ColorPalette Palette {
		get {							
			
			return colorPalette;
		}
		set {
			colorPalette = value;					
		}
	}
		
	public SizeF PhysicalDimension {
		get {
			float width,  height;
			Status status = GDIPlus.GdipGetImageDimension (nativeObject, out width, out height);		
			GDIPlus.CheckStatus (status);			
			
			return new SizeF (width, height);
		}
	}
	
	public PixelFormat PixelFormat {
		get {			
			PixelFormat pixFormat;				
			Status status = GDIPlus.GdipGetImagePixelFormat (nativeObject, out pixFormat);		
			GDIPlus.CheckStatus (status);			
			
			return pixFormat;
		}		
	}
	
	[Browsable (false)]
	public int[] PropertyIdList {
		get {
			uint propNumbers;
			
			Status status = GDIPlus.GdipGetPropertyCount (nativeObject, 
									out propNumbers);			
			GDIPlus.CheckStatus (status);
			
			int [] idList = new int [propNumbers];
			status = GDIPlus.GdipGetPropertyIdList (nativeObject, 
								propNumbers, idList);
			GDIPlus.CheckStatus (status);
			
			return idList;
		}
	}
	
	[Browsable (false)]
	public PropertyItem[] PropertyItems {
		get {
			int propNums, propsSize, propPtr, propSize;
			IntPtr properties;
			PropertyItem[] items;
			GdipPropertyItem gdipProperty = new GdipPropertyItem ();
			Status status;
			
			status = GDIPlus.GdipGetPropertySize (nativeObject, out propsSize, out propNums);
			GDIPlus.CheckStatus (status);

			items =  new PropertyItem [propNums];
			
			if (propNums == 0)
				return items;			
					
			/* Get PropertyItem list*/
			properties = Marshal.AllocHGlobal (propsSize);
			status = GDIPlus.GdipGetAllPropertyItems (nativeObject, propsSize, 
								propNums, properties);
			GDIPlus.CheckStatus (status);

			propSize = Marshal.SizeOf (gdipProperty);			
			propPtr = properties.ToInt32();
			
			for (int i = 0; i < propNums; i++, propPtr += propSize)
			{
				gdipProperty = (GdipPropertyItem) Marshal.PtrToStructure 
						((IntPtr)propPtr, typeof (GdipPropertyItem));						
				items [i] = new PropertyItem ();
				GdipPropertyItem.MarshalTo (gdipProperty, items [i]);								
			}
			
			Marshal.FreeHGlobal (properties);
			return items;
		}
	}

	public ImageFormat RawFormat {
		get {
			Guid guid;
			Status st = GDIPlus.GdipGetImageRawFormat (nativeObject, out guid);
			
			GDIPlus.CheckStatus (st);
			return new ImageFormat (guid);			
		}
	}
	
	public Size Size {
		get {
			return new Size(Width, Height);
		}
	}
	
	public float VerticalResolution {
		get {
			float resolution;
			
			Status status = GDIPlus.GdipGetImageVerticalResolution (nativeObject, out resolution);
			GDIPlus.CheckStatus (status);

			return resolution;
		}
	}

	[DefaultValue (false)]
	[Browsable (false)]
	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	public int Width {
		get {
			int width;			
			Status status = GDIPlus.GdipGetImageWidth (nativeObject, out width);		
			GDIPlus.CheckStatus (status);			
			
			return width;
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
		System.GC.SuppressFinalize (this);
	}

	~Image ()
	{
		Dispose (false);
	}

	private void DisposeResources ()
	{
		lock (this)
		{
			Status status = GDIPlus.GdipDisposeImage (nativeObject);
			GDIPlus.CheckStatus (status);
		}
	}
	
	protected virtual void Dispose (bool disposing)
	{
		if (nativeObject != IntPtr.Zero){
			DisposeResources ();
			nativeObject = IntPtr.Zero;
		}
	}
	
	public virtual object Clone()
	{				
		lock (this)
		{
			IntPtr newimage = IntPtr.Zero;
			
			if (!(this is Bitmap)) 
				throw new NotImplementedException (); 
			
			Status status = GDIPlus.GdipCloneImage (NativeObject, out newimage);			
			GDIPlus.CheckStatus (status);			

			if (this is Bitmap){
				Bitmap b = new Bitmap (newimage);

				if (colorPalette != null)
					b.colorPalette = colorPalette.Clone ();

				return b;
			}
			
			throw new NotImplementedException (); 
		}
	}

}

}
