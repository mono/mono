//
// System.Drawing.Bitmap.cs
//
// (C) 2002/3 Ximian, Inc.  http://www.ximian.com
// Author: Christian Meyer <Christian.Meyer@cs.tum.edu>
//			Jason Perkins <jason@379.com>
//			Dennis Hayes <dennish@raytek.com>
//          Alexandre Pigolkine <pigolkine@gmx.de>
//
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace System.Drawing {
	namespace XrImpl {

		internal class BitmapFactory : IBitmapFactory {
			public IBitmap Bitmap(int width, int height) {
				return new Bitmap(width, height);
			}

			public IBitmap Bitmap(int width, int height, System.Drawing.Graphics g) {
				return new Bitmap(width, height, (Graphics)g.implementation_);
			}

			public IBitmap Bitmap(int width, int height, System.Drawing.Imaging.PixelFormat format) {
				return new Bitmap(width, height, format);
			}

			public IBitmap Bitmap(System.Drawing.Image original, Size newSize){
				return new Bitmap((System.Drawing.XrImpl.Image)original.implementation_, newSize);
			}

			public IBitmap Bitmap(Stream stream, bool useIcm){
				return new Bitmap(stream);
			}

			public IBitmap Bitmap(string filename, bool useIcm){
				return new Bitmap(filename, useIcm);
			}

			public IBitmap Bitmap(Type type, string resource){
				return new Bitmap(type, resource);
			}

			public IBitmap Bitmap(int width, int height, int stride, System.Drawing.Imaging.PixelFormat format, IntPtr scan0){
				return new Bitmap(width, height, stride, format, scan0);
			}
		}

		internal sealed class Bitmap : Image, IBitmap {

			internal void CommonInit (int width, int height) {
				size = new Size(width, height);
				switch( format) {
				case PixelFormat.Format32bppArgb:
					xrFormat_ = Xr.XrFormat.XrFormatARGB32;
					nativeObject_ = GDK.gdk_pixbuf_new(0, true, 8, width, height);
					break;
				case PixelFormat.Format24bppRgb:
					xrFormat_ = Xr.XrFormat.XrFormatRGB24;
					nativeObject_ = GDK.gdk_pixbuf_new(0, false, 8, width, height);
					break;
				default:
					throw new NotImplementedException ();
				}
			}

			#region constructors
			/// <summary>
			/// Constructors
			/// </summary>

			public Bitmap (int width, int height) {
				format = PixelFormat.Format32bppArgb;
				CommonInit (width, height);
			}

			public Bitmap (int width, int height, Graphics g) {
				// TODO: Get pixelFormat from g
				CommonInit (width,height);
				//TODO: use graphics to set vertial and horzontal resolution.
				//TODO: that is all the spec requires or desires
			}

			[MonoTODO]
			public Bitmap(int width, int height, PixelFormat format) {
				this.format = format;
				CommonInit (width, height);
			}

			public Bitmap (Image origial) {
				throw new NotImplementedException ();
				//this.original = original;
			}
			//FIXME: This uses GTK
			public Bitmap(Stream stream) {
				throw new NotImplementedException ();
			}


			public Bitmap (string filename) {
				throw new NotImplementedException ();
				//this.filename = filename;
			}

			public Bitmap (Image original, Size newSize) {
				throw new NotImplementedException ();
				//this.original = original;
				//this.newSize = newSize;
			}

			public Bitmap (Stream stream, bool useIcm) {
				throw new NotImplementedException ();
				//this.stream = stream;
				//this.useIcm = useIcm;
			}

			public Bitmap (string filename, bool useIcm) {
				throw new NotImplementedException ();
				//this.filename = filename;
				//this.useIcm = useIcm;
			}

			public Bitmap (Type type, string resource) {
				throw new NotImplementedException ();
				//this.type = type;
				//this.resource = resource;
			}

			public Bitmap (Image original, int width, int heigth) {
				throw new NotImplementedException ();
				//this.original = original;
				//this.width = width;
				//this.heigth = heigth;
			}


			public Bitmap (int width, int height, int stride,
				PixelFormat format, IntPtr scan0) {
				throw new NotImplementedException ();
				//this.width = width;
				//this.heigth = heigth;
				//this.stride = stride;
				//this.format = format;
				//this.scan0 = scan0;
			}
			#endregion

			// methods
			public Color GetPixel (int x, int y) {
				throw new NotImplementedException ();
			}

			public void SetPixel (int x, int y, Color color) {
				throw new NotImplementedException ();
			}
			public IBitmap Clone (Rectangle rect,PixelFormat format) {
				throw new NotImplementedException ();
			}
		
			public IBitmap Clone (RectangleF rect, PixelFormat format) {
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public static Bitmap FromHicon (IntPtr hicon) {
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public static Bitmap FromResource (IntPtr hinstance,
				string bitmapName) {
				throw new NotImplementedException ();
			}

			public IntPtr GetHbitmap () {
				throw new NotImplementedException ();
			}

			public IntPtr GetHbitmap (Color background) {
				throw new NotImplementedException ();
			}

			public IntPtr GetHicon () {
				throw new NotImplementedException ();
			}


			[MonoTODO]
			public BitmapData LockBits(Rectangle rect, ImageLockMode flags, PixelFormat format) {
				throw new NotImplementedException ();
			}

			public void MakeTransparent () {
				throw new NotImplementedException ();
			}

			public void MakeTransparent (Color transparentColor) {
				throw new NotImplementedException ();
			}

			public void SetResolution (float xDpi, float yDpi) {
				throw new NotImplementedException ();
			}
		
			//Fixme: gtk should be inherited from Image
			//public override PixelFormat PixelFormat {
			public new PixelFormat PixelFormat {
				get {
					return format;
				}
			}

			public void UnlockBits(BitmapData bitmapdata) {
				// nothing to do!
			}
		
			// properties
			// needs to be done ###FIXME###


			/// <summary>
			/// Methods 
			/// </summary>
			protected override void Dispose(bool disposing) {
				if(selectedIntoGraphics_ == null) {
					GDK.gdk_pixbuf_finalize(nativeObject_);			
				}
				base.Dispose(disposing);
			}
			//FIXME: included with gtk code. should be inherited from Image.
			//		[MonoTODO]
			//		public override void RotateFlip(RotateFlipType rotateFlipType) {
			//			throw new NotImplementedException();
			//		}
		
		}
	}
}
