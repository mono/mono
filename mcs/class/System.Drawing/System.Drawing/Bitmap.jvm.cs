using System;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.Serialization;
using Mainsoft.Drawing.Imaging;

using io = java.io;
using imageio = javax.imageio;
using stream = javax.imageio.stream;
using spi = javax.imageio.spi;
using BufferedImage = java.awt.image.BufferedImage;
using JavaImage = java.awt.Image;
using awt = java.awt;
using image = java.awt.image;

namespace System.Drawing 
{
	public sealed class Bitmap : Image {

		# region Static fields

		static readonly image.ColorModel _jpegColorModel = new image.DirectColorModel(24, 0x00ff0000, 0x0000ff00, 0x000000ff, 0x0);

		#endregion

		#region constructors

		Bitmap (PlainImage orig) {
			base.Initialize( orig, false );
		}

		[MonoTODO]
		private Bitmap (SerializationInfo info, StreamingContext context) {
			throw new NotImplementedException ();
		}

		public Bitmap (int width, int height, Graphics g) 
			:this (width, height, PixelFormat.Format32bppArgb) {
			CurrentImage.HorizontalResolution = g.DpiX;
			CurrentImage.VerticalResolution = g.DpiY;
		}

		public Bitmap (Image original) 
			:this (original, original.Size) {}

		public Bitmap (Image orig, Size newSize)
			:this (orig, newSize.Width, newSize.Height) {}

		public Bitmap (Image orig, int width, int height)
			:base (CreateScaledImage (orig, width, height), ImageFormat.MemoryBmp) {}

		internal Bitmap (java.awt.Image nativeObject, ImageFormat format)
			:base (nativeObject, format) {}

		[MonoTODO]
		private Bitmap (java.awt.Image nativeObject, ImageFormat format, PixelFormat pixFormat)
			:this (nativeObject, format) {
			if (pixFormat != this.PixelFormat)
				throw new NotImplementedException ("Converting PixelFormat is not implemented yet.");
		}

		public Bitmap (int width, int height) 
			:this (width, height, PixelFormat.Format32bppArgb) {}

		public Bitmap (int width, int height, PixelFormat format)
			:base (
			new java.awt.image.BufferedImage (width, height,
			ToBufferedImageFormat (format)),
			ImageFormat.Bmp) {
		}

		public Bitmap (Stream stream)
			:this (stream, false) {}

		public Bitmap (string filename) 
			:this (filename, false) {}

		[MonoTODO]
		public Bitmap (Stream stream, bool useIcm)
			:this (stream, useIcm, null) {}

		[MonoTODO]
		public Bitmap (string filename, bool useIcm)
			:this (filename, useIcm, null) {}

		internal Bitmap (Stream stream, bool useIcm, ImageFormat format) {
			// TBD: useIcm param
			io.InputStream jis = vmw.common.IOUtils.ToInputStream (stream);
            Initialize (new stream.MemoryCacheImageInputStream (jis), format);
		}

		internal Bitmap (string filename, bool useIcm, ImageFormat format) {
			using(FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read)) {
				// TBD: useIcm param
				io.InputStream jis = vmw.common.IOUtils.ToInputStream (stream);
				Initialize (new stream.MemoryCacheImageInputStream (jis), format);
			}
		}

		public Bitmap (Type type, string resource) {
			using (Stream s = type.Assembly.GetManifestResourceStream (resource)) {
				if (s == null)
					throw new ArgumentException("Resource '" + resource + "' could not be found in class '" + type.ToString() + "'");

				io.InputStream jis = vmw.common.IOUtils.ToInputStream (s);
				Initialize (new stream.MemoryCacheImageInputStream (jis), null);
			}
		}
#if INTPTR_SUPPORT
		[MonoTODO]
		public Bitmap (int width, int height, int stride, PixelFormat format, IntPtr scan0)
		{						
			throw new NotImplementedException();			
		}
#endif
		#endregion

		#region Internal Initialization

		private void Initialize (stream.ImageInputStream input, ImageFormat format) {
			ImageCodec ic = null;

			if (format == null)
				ic = ImageCodec.CreateReader(input);
			else
				ic = ImageCodec.CreateReader(format);

			if (ic == null)
				throw new ArgumentException ("Parameter is not valid.");

			try {
				ic.NativeStream = input;
				PlainImage pi = ic.ReadPlainImage();
				base.Initialize( pi, false );

				pi = ic.ReadNextPlainImage();
				while ( pi != null) {
					base.Initialize( pi, true );
					pi = ic.ReadNextPlainImage();
				}

				_flags |= (int)(ImageFlags.ImageFlagsReadOnly | ImageFlags.ImageFlagsHasRealPixelSize);
			}
			catch (IOException ex) {
				throw ex;
			}			
			finally {
				ic.Dispose();
			}
		}

		#endregion

		#region InternalSave
		protected override void InternalSave (stream.ImageOutputStream output, Guid clsid) {

			ImageCodec ic = ImageCodec.CreateWriter( clsid );
			using (ic) {

				PlainImage plainImage = CurrentImage;
				plainImage.NativeImage.flush();

				if ( ImageCodec.ClsidToImageFormat( clsid ).Equals( ImageFormat.Jpeg ) ) {
					image.ColorModel cm = ((image.BufferedImage)CurrentImage.NativeImage).getColorModel();
					if (cm.hasAlpha()) {
						if (cm is image.DirectColorModel) {
							image.Raster raster = ((image.BufferedImage)CurrentImage.NativeImage).getRaster();
							image.DataBuffer db = raster.getDataBuffer();
							image.DirectColorModel dcm = (image.DirectColorModel)cm;
							image.SinglePixelPackedSampleModel jpegSampleModel = new image.SinglePixelPackedSampleModel( 
								db.getDataType(), Width, Height, 
								new int[] {dcm.getRedMask(), dcm.getGreenMask(), dcm.getBlueMask()}	);
		
							image.BufferedImage tb = new image.BufferedImage( 
								_jpegColorModel, 
								image.Raster.createWritableRaster( jpegSampleModel, db, null ),
								false, null );

							plainImage = new PlainImage( tb, plainImage.Thumbnails, ImageFormat.Jpeg, plainImage.HorizontalResolution, plainImage.VerticalResolution, plainImage.Dimension );
							plainImage.NativeMetadata = plainImage.NativeMetadata;
						}
					}
				}

				ic.NativeStream = output;
				ic.WritePlainImage( plainImage );
			}
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
					return BufferedImage.TYPE_INT_ARGB;
			}			
		}

		private static java.awt.Image CreateScaledImage(Image original, int width, int height) {
			JavaImage oldscaled = original.CurrentImage.NativeImage.getScaledInstance(width, height,
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
			return new Bitmap ( (PlainImage)CurrentImage.Clone() );
		}

		public Bitmap Clone (Rectangle rect, PixelFormat pixFormat)
		{
			return Clone(new RectangleF( rect.X, rect.Y, rect.Width, rect.Height ), pixFormat);
       	}
		
		public Bitmap Clone (RectangleF rect, PixelFormat pixFormat)
		{
			PlainImage plainImage = CurrentImage.Clone(false);
			BufferedImage clone = new BufferedImage( (int)rect.Width, (int)rect.Height, ToBufferedImageFormat( pixFormat ) );
			awt.Graphics2D g = clone.createGraphics();
			try {
				g.drawImage( NativeObject, -(int)rect.X, -(int)rect.Y, null );
			}
			finally {
				g.dispose();
			}

			plainImage.NativeImage = clone;
			return new Bitmap(plainImage);
		}
		#endregion

		#region LockBits
		[MonoTODO]
		public BitmapData LockBits (Rectangle rect, ImageLockMode flags, PixelFormat format) {
			throw new NotImplementedException();
		}

#if NET_2_0
		public
#endif
		BitmapData LockBits (Rectangle rect, ImageLockMode flags, PixelFormat format, BitmapData bitmapData) {
			throw new NotImplementedException();
		}
		#endregion

		#region MakeTransparent
		public void MakeTransparent ()
		{
			Color clr = Color.FromArgb(0,0,0);			
			MakeTransparent (clr);
		}

		public void MakeTransparent (Color transparentColor)
		{
			image.WritableRaster raster = NativeObject.getRaster();
			int numBands  = raster.getNumBands();
			if (numBands != 4)
				return;

			int maxWidth  = raster.getWidth() + raster.getMinX();
			int maxHeight = raster.getHeight() + raster.getMinY();
			int[] srcPix  = new int[numBands];

			for (int y = raster.getMinY(); y < maxHeight; y++) {
				for (int x = raster.getMinX(); x < maxWidth; x++) {
					/*srcPix =*/ raster.getPixel(x, y, srcPix);
					if (srcPix[0] == transparentColor.R &&
						srcPix[1] == transparentColor.G &&
						srcPix[2] == transparentColor.B) {
						srcPix[3] = 0;
						raster.setPixel(x, y, srcPix);
					}
				}
			}
		}
		#endregion

		#region SetResolution
		public void SetResolution (float xDpi, float yDpi)
		{
			CurrentImage.HorizontalResolution = xDpi;
			CurrentImage.VerticalResolution = yDpi;
		}
		#endregion 

		#region UnlockBits
		[MonoTODO]
		public void UnlockBits (BitmapData bitmap_data)
		{
			throw new NotImplementedException();
		}
		#endregion 

		#region NativeObject
		internal new BufferedImage NativeObject {
			get {
				return (BufferedImage)base.NativeObject.CurrentImage.NativeImage;
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
						return PixelFormat.Format8bppIndexed;				
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
						//TBD: support this
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
		[MonoTODO]
		public static Bitmap FromHicon (IntPtr hicon)
		{	
			throw new NotImplementedException();
		}

		[MonoTODO]
		public static Bitmap FromResource (IntPtr hinstance, string bitmapName)	//TBD: Untested
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public IntPtr GetHbitmap ()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public IntPtr GetHbitmap (Color background)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public IntPtr GetHicon ()
		{
			throw new NotImplementedException();
		}
#endif

	}
}
