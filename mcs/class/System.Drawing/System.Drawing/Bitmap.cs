//
// System.Drawing.Bitmap.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//
// Authors: 
//   Alexandre Pigolkine (pigolkine@gmx.de)
//   Christian Meyer (Christian.Meyer@cs.tum.edu)
//   Miguel de Icaza (miguel@ximian.com)
//
using System;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace System.Drawing {

	[Serializable]
	[ComVisible (true)]
	[Editor ("System.Drawing.Design.BitmapEditor, " + Consts.AssemblySystem_Drawing_Design, typeof (System.Drawing.Design.UITypeEditor))]
	public sealed class Bitmap : Image {
				
		#region constructors
		// constructors
		public Bitmap (int width, int height) : this (width, height, PixelFormat.Format32bppArgb)
		{
		}

		public Bitmap (int width, int height, Graphics g)
		{
			image_size = new Size(width, height);
			int bmp = 0;
			Status s = GDIPlus.GdipCreateBitmapFromGraphics (width, height, g.nativeObject, out bmp);
			if (s != Status.Ok)
				throw new Exception ("Could not create Bitmap from Graphics: " + s);
			nativeObject = (IntPtr)bmp;
			pixel_format = PixelFormat.Format32bppArgb;
		}

		public Bitmap (int width, int height, PixelFormat format)
		{
			image_size = new Size(width, height);
			pixel_format = format;
			int bpp = 32;
			int stride = ((bpp * width) / 8);
			stride = (stride + 3) & ~3;
			int bmp_size = stride * height;			
			
			int bmp = 0;
			Status s = GDIPlus.GdipCreateBitmapFromScan0 (width, height, stride, PixelFormat.Format32bppArgb, IntPtr.Zero, 
				out bmp);
				
			if (s != Status.Ok)
				throw new ArgumentException ("Could not allocate the GdiPlus image: " + s);
				
			nativeObject = (IntPtr)bmp;
		}

		public Bitmap (Image original) : this (original, original.Size)
		{
		}

		public Bitmap (Stream stream)  : this (stream, false) {} 

		public Bitmap (string filename) : this (filename, false) {}

		public Bitmap (Image original, Size newSize)
		{
			if (original is Bitmap) {
				Bitmap bmpOriginal = (Bitmap) original;
				image_size = bmpOriginal.Size;
				pixel_format = bmpOriginal.pixel_format;
				int bmp = 0;
				Status s = GDIPlus.GdipCloneBitmapAreaI (0, 0, newSize.Width, newSize.Height, bmpOriginal.PixelFormat, bmpOriginal.nativeObject, out bmp);
				if (s != Status.Ok)
					throw new ArgumentException ("Could not allocate the GdiPlus image: " + s);
				nativeObject = (IntPtr)bmp;
			}
			else {
				throw new NotImplementedException ();
			}
		}

		void InitFromStream (Stream stream)
		{
			BitmapData bd = Decode (stream);
			if (bd == null)
				throw new ArgumentException ("Stream could not be decoded");
			
			image_size = new Size (bd.Width, bd.Height);
			pixel_format = bd.PixelFormat;

			int bmp = 0;
			//buffer = bd.Scan0;
			Console.WriteLine ("Stride: {0} ", bd.Stride);
			Console.WriteLine ("Scan0: {0:x}" , (long) bd.Scan0);
			Status s = GDIPlus.GdipCreateBitmapFromScan0 (bd.Width, bd.Height, bd.Stride, bd.PixelFormat, bd.Scan0, out bmp);
			if (s != Status.Ok)
				throw new ArgumentException ("Could not allocate the GdiPlus image: " + s);
			Console.WriteLine ("Image is {0}", bmp);
			nativeObject = (IntPtr)bmp;
		}
		
		public Bitmap (Stream stream, bool useIcm)
		{
			InitFromStream (stream);
		}

		public Bitmap (string filename, bool useIcm)
		{
			using (FileStream file = new FileStream(filename, FileMode.Open)){
				InitFromStream (file);
			}
		}

		public Bitmap (Type type, string resource)
		{
			throw new NotImplementedException ();
		}

		public Bitmap (Image original, int width, int heigth)
		{
			throw new NotImplementedException ();
		}

		public Bitmap (int width, int height, int stride, PixelFormat format, IntPtr scan0)
		{
			throw new NotImplementedException ();
		}

		private Bitmap (SerializationInfo info, StreamingContext context)
		{
		}

		#endregion
		// methods
		public Color GetPixel (int x, int y) {
			
			int argb;				
			
			Status s = GDIPlus.GdipBitmapGetPixel(nativeObject, x, y, out argb);
								
			if (s != Status.Ok)
				throw new Exception ("Unable to GetPixel: " + x +":" +y + ";status: " + s);					
			
			return Color.FromArgb(argb);		
		}

		public void SetPixel (int x, int y, Color color)
		{									
			Status s = GDIPlus.GdipBitmapSetPixel(nativeObject, x, y, color.ToArgb());
								
			if (s != Status.Ok)
				throw new Exception ("Unable to SetPixel: " + x +":" +y + ";status: " + s);									
		}

		public Bitmap Clone (Rectangle rect,PixelFormat format)
		{
			throw new NotImplementedException ();
		}
		
		public Bitmap Clone (RectangleF rect, PixelFormat format)
		{
			throw new NotImplementedException ();
		}

		public static Bitmap FromHicon (IntPtr hicon)
		{
			throw new NotImplementedException ();
		}

		public static Bitmap FromResource (IntPtr hinstance, string bitmapName)
		{
			throw new NotImplementedException ();
		}

		public IntPtr GetHbitmap ()
		{
			throw new NotImplementedException ();
		}

		public IntPtr GetHbitmap (Color background)
		{
			throw new NotImplementedException ();
		}

		public IntPtr GetHicon ()
		{
			throw new NotImplementedException ();
		}

		public BitmapData LockBits (Rectangle rect, ImageLockMode flags, PixelFormat format)
		{
			Console.WriteLine("Bitmap.LockBits");
			
			BitmapData result = new BitmapData();

			if (nativeObject == (IntPtr) 0)
				throw new Exception ("nativeObject is null");			
			
			IntPtr lfBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(result));
     		Marshal.StructureToPtr(result, lfBuffer, false);						
     		
			Status status = GDIPlus.GdipBitmapLockBits (nativeObject, ref rect, flags, format,  lfBuffer);
			
			result = (BitmapData) Marshal.PtrToStructure(lfBuffer,  typeof(BitmapData));											
			Marshal.FreeHGlobal (lfBuffer);
			
			if (status != Status.Ok)
				throw new Exception ("Could not lock bits: " + status);
							
			Console.WriteLine("Bitmap.LockBits->height "+ result.height+ " scan"+ result.address);
							
			return  result;
		}

		public void MakeTransparent ()
		{
			throw new NotImplementedException ();
		}

		public void MakeTransparent (Color transparentColor)
		{
			throw new NotImplementedException ();
		}

		public void SetResolution (float xDpi, float yDpi)
		{
			throw new NotImplementedException ();
		}

		public void UnlockBits (BitmapData bitmap_data)
		{
			Status s = GDIPlus.GdipBitmapUnlockBits (nativeObject, bitmap_data);

			if (s != Status.Ok)
				throw new Exception ("Could not unlock bits: " + s);
		}

		// properties
		// needs to be done ###FIXME###

		protected override void DisposeResources ()
		{
			base.DisposeResources ();
			
		}
	}
}
