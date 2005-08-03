using System;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.Serialization;
using java.io;
using javax.imageio;
using javax.imageio.stream;
using javax.imageio.spi;

using BufferedImage = java.awt.image.BufferedImage;
using JavaImage = java.awt.Image;
using awt = java.awt;
using image = java.awt.image;

namespace System.Drawing 
{
	public sealed class Bitmap : Image {

		#region constructors

		Bitmap (Bitmap orig):base (orig) {}

		private Bitmap (SerializationInfo info, StreamingContext context) {
			throw new NotImplementedException ();
		}

		public Bitmap (int width, int height, Graphics g) {
			throw new NotImplementedException();			
		}

		public Bitmap (Image orig, Size newSize)
			:this (orig, newSize.Width, newSize.Height) {}

		public Bitmap (Image orig, int width, int height)
			:base (CreateScaledImage (orig, width, height), ImageFormat.Bmp) {}

		public Bitmap (int width, int height) 
			:this (width, height, PixelFormat.Format32bppArgb) {}

		public Bitmap (Image original) 
			:this (original, original.Size) {}

		public Bitmap (Stream stream)
			:this (stream, false) {}

		public Bitmap (string filename) 
			:this (filename, false) {}

		internal Bitmap (java.awt.Image nativeObject, ImageFormat format)
			:base (nativeObject, format) {}

		private Bitmap (java.awt.Image nativeObject, ImageFormat format, PixelFormat pixFormat)
			:this (nativeObject, format) {
			if (pixFormat != this.PixelFormat)
				throw new NotImplementedException ("Converting PixelFormat is not implemented yet.");
		}

		public Bitmap (int width, int height, PixelFormat format)
			:base (
				new java.awt.image.BufferedImage (width, height,
					ToBufferedImageFormat (format)),
				ImageFormat.Bmp)
		{
			//TBD: why the following 3 lines are necessary?
//			java.awt.Graphics2D graphics2d = NativeObject.createGraphics();
//			graphics2d.drawImage(NativeObject, 0, 0, null);
//			graphics2d.dispose();
		}

		public Bitmap (Stream stream, bool useIcm)
			:this (stream, null) {}

		public Bitmap (string filename, bool useIcm)
			//FIXME: useIcm param
			:this (filename, null) {}

		internal Bitmap (Stream stream, ImageFormat format) {
			//FIXME: useIcm param
			//FIXME: use direct ImageInputStream wrapper for NET Stream
			InputStream jis = vmw.common.IOUtils.ToInputStream (stream);
            Initialize (new MemoryCacheImageInputStream (jis), format);
		}

		internal Bitmap (string filename, ImageFormat format) {
			java.io.File file = vmw.common.IOUtils.getJavaFile (filename);
			if (!file.exists ())
				//TBD: check what exception throws NET
				throw new System.IO.IOException ("File not found: "+filename);
			Initialize (new FileImageInputStream (file), format);
		}

		public Bitmap (Type type, string resource) {
			using (Stream s = type.Assembly.GetManifestResourceStream (resource)) {
				if (s == null)
					//TBD: check what type is thrown in MS
					throw new Exception("Resource name was not found: `" + resource + "'");
				InputStream jis = vmw.common.IOUtils.ToInputStream (s);
				Initialize (new MemoryCacheImageInputStream (jis), null);
			}
		}

		private void Initialize (ImageInputStream input, ImageFormat format) {
			if (format != null) {
				ImageReader r = format.ImageReaderSpi.createReaderInstance ();
				r.setInput (input);
				Initialize (r, format);
			}
			else {
				java.util.Iterator iter = ImageIO.getImageReaders (input);
				if (!iter.hasNext ())
					throw new ArgumentException ("Format not found"); //TBD: make same text as MS

				//following vars are initialized in the try block
				string mimeType;
				javax.imageio.spi.ImageReaderSpi readerSpi;
				ImageReader r;
				try {
					r = (ImageReader) iter.next ();
					r.setInput (input);
					readerSpi = r.getOriginatingProvider();
					mimeType = readerSpi.getMIMETypes() [0];
				}
				catch (Exception e) {
					//TBD: make same text as MS
					throw new ArgumentException ("Error reading", e);
				}
				format = new ImageFormat (mimeType, readerSpi, null);
				Initialize (r, format);
			}
		}

		private void Initialize (ImageReader r, ImageFormat format) {
			java.awt.Image [] nativeObjects;
			java.awt.Image [] thumbnails = null;
			try {
				nativeObjects = new BufferedImage [r.getNumImages (false)];
				for (int i = 0; i < nativeObjects.Length; i++) {
					if (r.hasThumbnails(i)) {
						if (thumbnails == null)
							thumbnails = new BufferedImage[nativeObjects.Length];

						thumbnails[i] = r.readThumbnail(i, 0);
					}
					nativeObjects [i] = r.read (i);
				}

			}
			catch (Exception e) {
				//TDB: make exception same as in MS
				throw new ArgumentException ("Error reading", e);
			}
			base.Initialize (nativeObjects, thumbnails, format, FrameDimension.Page.Guid);
		}

#if INTPTR_SUPPORT
		public Bitmap (int width, int height, int stride, PixelFormat format, IntPtr scan0)
		{						
			throw new NotImplementedException();			
		}
#endif
		#endregion

		#region InternalSave
		protected override void InternalSave (ImageOutputStream output, ImageFormat format) {
			ImageWriterSpi spi = format.ImageWriterSpi;
			if (spi == null)
				spi = ImageFormat.Png.ImageWriterSpi;

			ImageWriter writer = spi.createWriterInstance ();
			writer.setOutput (output);
			if (NativeObjectsCount == 1)
				writer.write (NativeObject);
			else if (writer.canWriteSequence ())
				SaveSequence ();
			else
				throw new NotImplementedException ();
		}

		void SaveSequence () {
			//FIXME: does not supports metadata and thumbnails for now
			ImageWriter writer = RawFormat.ImageWriterSpi.createWriterInstance ();

			writer.prepareWriteSequence (null);

			for (int i = 0; i < NativeObjectsCount; i++) {
				IIOImage iio = new IIOImage ((BufferedImage)this[i], null, null);
				writer.writeToSequence (iio, null);
			}

			writer.endWriteSequence ();
		}

		#endregion

		#region private statics: ToBufferedImageFormat, CreateScaledImage

		private static int ToBufferedImageFormat (PixelFormat format) {
			switch(format) {
				case PixelFormat.Format16bppGrayScale:
					return BufferedImage.TYPE_USHORT_GRAY;
				case PixelFormat.Format1bppIndexed:
					return BufferedImage.TYPE_BYTE_GRAY;
				case PixelFormat.Format32bppArgb:
					return BufferedImage.TYPE_INT_ARGB;
				case PixelFormat.Format32bppRgb:
					return BufferedImage.TYPE_INT_RGB;
				case PixelFormat.Format32bppPArgb:
					return BufferedImage.TYPE_INT_ARGB_PRE;
				case PixelFormat.Format16bppRgb555:
					return BufferedImage.TYPE_USHORT_555_RGB;
				case PixelFormat.Format16bppRgb565:
					return BufferedImage.TYPE_USHORT_565_RGB;
				case PixelFormat.Indexed:
					return BufferedImage.TYPE_BYTE_INDEXED;
				default:
					return 0;
			}			
		}

		private static java.awt.Image CreateScaledImage(Image original, int width, int height) {
			JavaImage oldscaled = original.NativeObject.getScaledInstance(width, height,
				JavaImage.SCALE_DEFAULT);
			BufferedImage newimage = new BufferedImage(oldscaled.getWidth(null), 
				oldscaled.getHeight(null),
				BufferedImage.TYPE_INT_ARGB);
			java.awt.Graphics2D graphics2d = newimage.createGraphics();
			graphics2d.drawImage(oldscaled, 0, 0, null);
			graphics2d.dispose();
			return newimage;				
		}
		#endregion

		#region Get-SetPixel
		public Color GetPixel (int x, int y) 
		{

			int argb = NativeObject.getRGB(x,y);				
			return Color.FromArgb(argb); 
		}

		public void SetPixel (int x, int y, Color color)
		{				
			int rgb = color.ToArgb();
			NativeObject.setRGB(x,y,rgb);
		}
		#endregion

		#region Clone
		public override object Clone () {
			return new Bitmap (this);
		}

		public Bitmap Clone (Rectangle rect, PixelFormat format)
		{
			BufferedImage sub = NativeObject.getSubimage(rect.X,rect.Y,rect.Width,rect.Height);
			return new Bitmap(sub, RawFormat, format);
       	}
		
		public Bitmap Clone (RectangleF rect, PixelFormat format)
		{
			//TODO: check if there is more precise API
			BufferedImage sub = NativeObject.getSubimage((int)rect.X,(int)rect.Y,(int)rect.Width,(int)rect.Height);
			return new Bitmap(sub, RawFormat, format);
		}
		#endregion

		#region LockBits [TODO]
		public BitmapData LockBits (Rectangle rect, ImageLockMode flags, PixelFormat format) {
			throw new NotImplementedException();
		}
		#endregion

		#region MakeTransparent
		public void MakeTransparent ()
		{
			Color clr = GetPixel(0,0);			
			MakeTransparent (clr);
		}

		public void MakeTransparent (Color transparentColor)
		{
			byte A = transparentColor.A;
			image.WritableRaster raster = NativeObject.getRaster();
			int numBands  = raster.getNumBands();
			int maxWidth  = raster.getWidth() + raster.getMinX();
			int maxHeight = raster.getHeight() + raster.getMinY();
			int[] srcPix  = new int[numBands];

			for (int y = raster.getMinY(); y < maxHeight; y++) {
				for (int x = raster.getMinX(); x < maxWidth; x++) {
					/*srcPix =*/ raster.getPixel(x, y, srcPix);
					for (int z = 0; z < numBands; z++) {
						int argb = srcPix[z];
						if ((uint)argb >> 24 == A) {
							argb &= 0x00FFFFFF;
							srcPix[z] = argb;
						}
					}
				}
			}
		}
		#endregion

		#region SetResolution [TODO]
		public void SetResolution (float xDpi, float yDpi)
		{
			throw new NotImplementedException();
		}
		#endregion 

		#region UnlockBits [TODO]
		public void UnlockBits (BitmapData bitmap_data)
		{
			throw new NotImplementedException();
		}
		#endregion 

		#region NativeObject
		internal new BufferedImage NativeObject {
			get {
				return (BufferedImage)base.NativeObject;
			}
		}

		protected override java.awt.Image[] CloneNativeObjects(java.awt.Image[] src) {
			if (src == null)
				return null;

			awt.Image[] dst = new awt.Image[src.Length];
			for (int i = 0; i < dst.Length; i++) {
				BufferedImage image = src[i] as BufferedImage;
				if (image == null)
					throw new ArgumentException(String.Format("Unsupported image type '{0}'", src[i].ToString()), "src");

				dst[i] = new BufferedImage(image.getColorModel(), image.copyData(null), image.isAlphaPremultiplied(), null);
			}

			return dst;
		}

		#endregion

		#region InternalPixelFormat
		protected override PixelFormat InternalPixelFormat {
			get {
				int t = NativeObject.getType();
				switch(t) {
					case 11://JavaImage.TYPE_USHORT_GRAY:
						return PixelFormat.Format16bppGrayScale;
					case 10://JavaImage.TYPE_BYTE_GRAY:
						return PixelFormat.Format1bppIndexed;				
					case 1:	//JavaImage.TYPE_INT_RGB
						return PixelFormat.Format32bppRgb;
					case 2: //JavaImage.TYPE_INT_ARGB:			
						return PixelFormat.Format32bppArgb;
					case 3://JavaImage.TYPE_INT_ARGB_PRE:
						return PixelFormat.Format32bppPArgb;
					case 9://JavaImage.TYPE_USHORT_555_RGB:
						return PixelFormat.Format16bppRgb555;
					case 8://JavaImage.TYPE_USHORT_565_RGB:
						return PixelFormat.Format16bppRgb565;
					case 13://JavaImage.TYPE_BYTE_INDEXED:
						return PixelFormat.Indexed;
						//TODO: support this
					case 12://JavaImage.TYPE_BYTE_BINARY:
					case 0://JavaImage.TYPE_CUSTOM:
					case 4://JavaImage.TYPE_INT_BGR:
					case 5://JavaImage.TYPE_3BYTE_BGR:					
					case 6://JavaImage.TYPE_4BYTE_ABGR:
					case 7://JavaImage.TYPE_4BYTE_ABGR_PRE:
					default:
						return PixelFormat.Undefined;
				}			
			}		
		}
		#endregion

#if INTPTR_SUPPORT
		public static Bitmap FromHicon (IntPtr hicon)
		{	
			throw new NotImplementedException();
		}

		public static Bitmap FromResource (IntPtr hinstance, string bitmapName)	//TODO: Untested
		{
			throw new NotImplementedException();
		}

		public IntPtr GetHbitmap ()
		{
			throw new NotImplementedException();
		}

		public IntPtr GetHbitmap (Color background)
		{
			throw new NotImplementedException();
		}

		public IntPtr GetHicon ()
		{
			throw new NotImplementedException();
		}
#endif

	}
}
