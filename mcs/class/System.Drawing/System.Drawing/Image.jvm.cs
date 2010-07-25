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
	using System.Drawing;
	using System.Drawing.Imaging;
	using System.IO;
	using System.Xml;
	using Mainsoft.Drawing.Imaging;

	using BufferedImage = java.awt.image.BufferedImage;
	using java.io;
	using javax.imageio;
	using javax.imageio.stream;
	using vmw.common;
	using awt = java.awt;
	using image = java.awt.image;

	public abstract class Image : MarshalByRefObject, IDisposable , ICloneable {	
		#region Vars	
		PlainImageCollection _nativeObject = new PlainImageCollection();
		protected int _flags = 0;

		//consider using Image[] to support many thumbnails per Image
		#endregion

		#region flags enum
		[Flags]
		protected enum ImageFlags {
			ImageFlagsNone = 0,
			ImageFlagsScalable = 0x0001,
			ImageFlagsHasAlpha = 0x0002,
			ImageFlagsHasTranslucent = 0x0004,
			ImageFlagsPartiallyScalable = 0x0008,
			ImageFlagsColorSpaceRGB = 0x0010,
			ImageFlagsColorSpaceCMYK = 0x0020,
			ImageFlagsColorSpaceGRAY = 0x0040,
			ImageFlagsColorSpaceYCBCR = 0x0080,
			ImageFlagsColorSpaceYCCK = 0x0100,
			ImageFlagsHasRealDPI = 0x1000,
			ImageFlagsHasRealPixelSize = 0x2000,
			ImageFlagsReadOnly = 0x00010000,
			ImageFlagsCaching = 0x00020000
		}
		#endregion

		#region Constructor
		public void Dispose () {
		}

		protected virtual void DisposeResources () {
		}
	
		protected virtual void Dispose (bool disposing) {
		}

		// Derived classes must call Initialize () when they use this constructor
		protected Image () {
		}
 
		protected Image (java.awt.Image nativeObject) : this(nativeObject, ImageFormat.MemoryBmp) {
		}

		protected Image (java.awt.Image nativeObject, ImageFormat format) {
			PlainImage pi = new PlainImage( nativeObject, null, format, 0, 0, FrameDimension.Page );
			Initialize( pi, false );
		}

		protected void Initialize (PlainImage pi, bool addToCollection) {
			if (!addToCollection)
				NativeObject.Clear();
				
			NativeObject.Add( pi );
		}

		#endregion
	
		#region Internals

		internal PlainImageCollection NativeObject {
			get {
				return _nativeObject;
			}
		}

		internal PlainImage CurrentImage {
			get {
				return NativeObject.CurrentImage;
			}
		}
		
		#endregion
    
		#region FromFile
		[MonoTODO]
		public static Image FromFile(string filename) {
			//FIXME: check if it's not a metafile, throw NotImplementedException
			return new Bitmap (filename);
		}
	
		[MonoTODO]
		public static Image FromFile(string filename, bool useIcm) {
			//FIXME: check if it's not a metafile, throw NotImplementedException
			return new Bitmap (filename, useIcm);
		}
		#endregion

		#region GetThumbnailImageAbort
		[Serializable]
			public delegate bool GetThumbnailImageAbort();
		#endregion

		#region Clone
		public abstract object Clone();
		#endregion

		// static
		#region FromStream
		[MonoTODO]
		public static Image FromStream (Stream stream) {
			//FIXME: check if it's not a metafile, throw NotImplementedException
			return new Bitmap (stream);
		}
	
		[MonoTODO]
		public static Image FromStream (Stream stream, bool useIcm) {
			//FIXME: check if it's not a metafile, throw NotImplementedException
			return new Bitmap (stream, useIcm);
		}
		#endregion

		#region GetPixelFormatSize
		public static int GetPixelFormatSize(PixelFormat pixfmt) {

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
		public static bool IsAlphaPixelFormat(PixelFormat pixfmt) {
			return (pixfmt & PixelFormat.Alpha) != PixelFormat.Undefined;
		}
		#endregion
	
		#region IsCanonicalPixelFormat
		// TBD: implement this
		public static bool IsCanonicalPixelFormat (PixelFormat pixfmt) {
			return (pixfmt & PixelFormat.Canonical) != PixelFormat.Undefined;
		}
		#endregion
	
		#region IsExtendedPixelFormat
		// TBD: implement this
		public static bool IsExtendedPixelFormat (PixelFormat pixfmt) {
			return (pixfmt & PixelFormat.Extended) != PixelFormat.Undefined;
		}
		#endregion

		// non-static
		#region GetBounds
		public RectangleF GetBounds (ref GraphicsUnit pageUnit) {	
			pageUnit = GraphicsUnit.Pixel; //java.awt.Image always returns pixels
			return new RectangleF((float)0,(float)0,(float)Width,(float)Height);
		}
		#endregion
	
		#region GetEncoderParameterList
		[MonoTODO]
		public EncoderParameters GetEncoderParameterList(Guid encoder) {
			throw new NotImplementedException ();
		}
		#endregion
	
		#region GetFrameCount
		public int GetFrameCount(FrameDimension dimension) {
			// FALLBACK: now, only one dimension assigned for all frames
			if (dimension.Guid != CurrentImage.Dimension.Guid) 
				throw new ArgumentException ("dimension");

			return NativeObject.Count;
		}
		#endregion
	
		#region GetPropertyItem
		[MonoTODO]
		public PropertyItem GetPropertyItem(int propid) {
			throw new NotImplementedException ();
		}
		#endregion

		#region RemovePropertyItem
		[MonoTODO]
		public void RemovePropertyItem (int propid) {		
			throw new NotImplementedException ();
		}
		#endregion
	
		#region RotateFlip
		public void RotateFlip (RotateFlipType rotateFlipType) {
			awt.geom.AffineTransform tx;

			if ( !(CurrentImage.NativeImage is image.BufferedImage) )
				// TBD: This implementation is for raster formats only
				throw new NotImplementedException("Only raster formats are supported");

			switch (rotateFlipType) {
				case RotateFlipType.RotateNoneFlipNone :
					return;
				
				case RotateFlipType.Rotate90FlipNone :
					tx = awt.geom.AffineTransform.getRotateInstance(Math.PI / 2);
					tx.translate( 0, -Height );
					break;

				case RotateFlipType.Rotate180FlipNone :
					tx = awt.geom.AffineTransform.getScaleInstance(-1, -1);
					tx.translate( -Width, -Height );
					break;

				case RotateFlipType.Rotate270FlipNone :
					tx = awt.geom.AffineTransform.getRotateInstance(-Math.PI / 2);
					tx.translate( -Width, 0 );
					break;

				case RotateFlipType.RotateNoneFlipX :
					tx = awt.geom.AffineTransform.getScaleInstance(-1, 1);
					tx.translate( -Width, 0 );
					break;

				case RotateFlipType.Rotate90FlipX :
					tx = awt.geom.AffineTransform.getRotateInstance(Math.PI / 2);
					tx.scale(1, -1);
					break;

				case RotateFlipType.Rotate180FlipX :
					tx = awt.geom.AffineTransform.getScaleInstance(1, -1);
					tx.translate( 0, -Height );
					break;

				case RotateFlipType.Rotate270FlipX :
					tx = awt.geom.AffineTransform.getRotateInstance(Math.PI / 2);
					tx.scale(-1, 1);
					tx.translate( -Width, -Height );
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
			image.AffineTransformOp op = new image.AffineTransformOp(tx, image.AffineTransformOp.TYPE_NEAREST_NEIGHBOR);
			CurrentImage.NativeImage = op.filter((BufferedImage)CurrentImage.NativeImage, null);
		}
		#endregion

		#region Save
		protected abstract void InternalSave (ImageOutputStream output, Guid clsid);

		[MonoTODO]
		public void Save (Stream stream, ImageCodecInfo encoder, EncoderParameters encoderParams) {
			//TBD: implement encoderParams
			if (encoder == null)
				throw new ArgumentNullException("Value cannot be null.");

			try {
				java.io.OutputStream jos = vmw.common.IOUtils.ToOutputStream (stream);
				MemoryCacheImageOutputStream output = new MemoryCacheImageOutputStream(jos);
				InternalSave (output, encoder.Clsid);
				output.flush();
			}
			catch (java.io.IOException ex) {
				throw new System.IO.IOException(ex.Message, ex);
			}
		}
	
		public void Save(string filename, ImageCodecInfo encoder, EncoderParameters encoderParams) {
			using (Stream outputStream = new FileStream(filename, FileMode.Create))
				Save(outputStream, encoder, encoderParams);
		}

		public void Save (string filename) {
			Save (filename, ImageFormat.Png);
		}

		public void Save (Stream stream, ImageFormat format) {
			ImageCodecInfo encoder = ImageCodec.FindEncoder ( ImageCodec.ImageFormatToClsid (format) );
			Save (stream, encoder, null);
		}

		public void Save(string filename, ImageFormat format) {
			using (Stream outputStream = new FileStream(filename, FileMode.Create))
				Save(outputStream, format);
		}
		#endregion

		#region SaveAdd
		[MonoTODO]
		public void SaveAdd(EncoderParameters encoderParams) {
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		public void SaveAdd(Image image, EncoderParameters encoderParams) {
			throw new NotImplementedException ();
		}
		#endregion
	
		#region SelectActiveFrame

		// TBD: .Net does not load all frames at the initialization. New frames loaded by request.
		[MonoTODO]
		public int SelectActiveFrame(FrameDimension dimension, int frameIndex) {
			// FALLBACK: now, only one dimension assigned for all frames
			if (dimension.Guid != CurrentImage.Dimension.Guid) 
				throw new ArgumentException ("dimension");

			if (frameIndex < NativeObject.Count)
				NativeObject.CurrentImageIndex = frameIndex;

			return frameIndex;
		}
		#endregion
	
		#region SetPropertyItem
		[MonoTODO]
		public void SetPropertyItem(PropertyItem propitem) {
			throw new NotImplementedException ();
		}
		#endregion

		// properties
		#region Flags
		public int Flags {
			// TDB: ImageFlagsScalable, ImageFlagsHasTranslucent, ImageFlagsPartiallyScalable, ImageFlagsCaching
			[MonoTODO]
			get {
				image.ColorModel colorModel = ((BufferedImage)CurrentImage.NativeImage).getColorModel();
				int t = colorModel.getColorSpace().getType();
				
				if (t == awt.color.ColorSpace.TYPE_RGB)
					_flags |= (int)ImageFlags.ImageFlagsColorSpaceRGB;
				else if (t == awt.color.ColorSpace.TYPE_CMYK)
					_flags |= (int)ImageFlags.ImageFlagsColorSpaceCMYK;
				else if (t == awt.color.ColorSpace.TYPE_GRAY)
					_flags |= (int)ImageFlags.ImageFlagsColorSpaceGRAY;
				else if (t == awt.color.ColorSpace.TYPE_YCbCr)
					_flags |= (int)ImageFlags.ImageFlagsColorSpaceYCBCR;

				if (colorModel.hasAlpha())
					_flags |= (int)ImageFlags.ImageFlagsHasAlpha;

				if ((CurrentImage.HorizontalResolution > 0) || (CurrentImage.VerticalResolution > 0))
					_flags |= (int)ImageFlags.ImageFlagsHasRealDPI;

				return _flags;
			}
		}
		#endregion

		#region FrameDimensionsList
		[MonoTODO]
		public Guid[] FrameDimensionsList {
			// TBD: look over all frames and build array of dimensions
			// FALLBACK: now, only one dimension assigned for all frames
			get {
				Guid [] dimList = new Guid[]{CurrentImage.Dimension.Guid};
				return dimList;
			}
		}
		#endregion

		#region Height
		public int Height {
			get {
				return CurrentImage.NativeImage.getHeight(null);
			}
		}
		#endregion
	
		#region HorizontalResolution
		public float HorizontalResolution {
			get {
				if (CurrentImage.HorizontalResolution <= 1)
					return Graphics.DefaultScreenResolution;

				return CurrentImage.HorizontalResolution;
			}
		}
		#endregion
	
		#region ColorPalette
		[MonoTODO]
		public ColorPalette Palette {
			get {
				if (!(CurrentImage.NativeImage is BufferedImage))
					// TBD: This implementation is for raster formats only
					throw new NotImplementedException("Only raster formats are supported");

				image.ColorModel colorModel = ((BufferedImage)CurrentImage.NativeImage).getColorModel();
				if (colorModel is image.IndexColorModel) {

					Color [] colors = new Color[ ((image.IndexColorModel)colorModel).getMapSize() ];
					for (int i=0; i<colors.Length; i++) {
						colors[i] = Color.FromArgb( ((image.IndexColorModel)colorModel).getRGB(i) );
					}
					ColorPalette palette = new ColorPalette(0, colors);
					return palette;
				}
				return new ColorPalette();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		#endregion
		
		#region PhysicalDimension
		public SizeF PhysicalDimension {
			get {
				return new Size(Width, Height);
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
		
		#region PropertiIdList
		[MonoTODO]
		public int[] PropertyIdList {
			get {
				throw new NotImplementedException ();
			}
		}
		#endregion
		
		#region PropertItems
		[MonoTODO]
		public PropertyItem[] PropertyItems {
			get {
				throw new NotImplementedException ();
			}
		}
		#endregion

		#region RawFormat
		public ImageFormat RawFormat {
			get {
				return CurrentImage.ImageFormat;
			}
		}
		#endregion

		#region Size
		public Size Size {
			get {
				return new Size(Width, Height);
			}
		}
		#endregion
	
		#region VerticalResolution
		public float VerticalResolution {
			get {
				if (CurrentImage.VerticalResolution <= 1)
					return Graphics.DefaultScreenResolution;

				return CurrentImage.VerticalResolution;
			}
		}
		#endregion
	
		#region Width
		public int Width {
			get {
				return CurrentImage.NativeImage.getWidth(null);
			}
		}
		#endregion	

		public Image GetThumbnailImage(int thumbWidth, int thumbHeight, Image.GetThumbnailImageAbort callback, IntPtr callbackData) {
			awt.Image img;

#if THUMBNAIL_SUPPORTED
			if (CurrentImage.Thumbnails != null) {
				for (int i=0; i < CurrentImage.Thumbnails.Length; i++)
					if (CurrentImage.Thumbnails[i] != null) {
						img = CurrentImage.Thumbnails[i];
						if (img.getHeight(null) == thumbHeight && img.getWidth(null) == thumbWidth)
							return ImageFromNativeImage(img, RawFormat);
					}
			}
#endif
			img = CurrentImage.NativeImage.getScaledInstance(thumbWidth, thumbHeight, awt.Image.SCALE_DEFAULT);

			return ImageFromNativeImage(img, RawFormat);
		}
#if INTPTR_SUPPORT
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
#endif

		internal static Image ImageFromNativeImage(awt.Image nativeImage, ImageFormat format) {
			if (nativeImage is BufferedImage)
				return new Bitmap(nativeImage, format);

			throw new ArgumentException("Invalid image type");
		}

		protected abstract awt.Image [] CloneNativeObjects(awt.Image [] src);
	}

}
