//
// System.Drawing.Bitmap.cs
//
// (C) 2002/3 Ximian, Inc.  http://www.ximian.com
// Author: Christian Meyer <Christian.Meyer@cs.tum.edu>
//			Jason Perkins <jason@379.com>
//			Dennis Hayes <dennish@raytek.com>
//	    Alexandre Pigolkine <pigolkine@gmx.de>
//
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace System.Drawing {
	namespace XrImpl {

		internal class BitmapFactory : IBitmapFactory
		{
			public IBitmap Bitmap (int width, int height)
			{
				return new Bitmap (width, height);
			}

			public IBitmap Bitmap (int width, int height, System.Drawing.Graphics g)
			{
				return new Bitmap (width, height, (Graphics)g.implementation);
			}

			public IBitmap Bitmap (int width, int height, System.Drawing.Imaging.PixelFormat format)
			{
				return new Bitmap (width, height, format);
			}

			public IBitmap Bitmap (System.Drawing.Image original, Size newSize)
			{
				return new Bitmap ((System.Drawing.XrImpl.Image)original.implementation, newSize);
			}

			public IBitmap Bitmap (Stream stream, bool useIcm)
			{
				return new Bitmap (stream);
			}

			public IBitmap Bitmap (string filename, bool useIcm)
			{
				return new Bitmap (filename, useIcm);
			}

			public IBitmap Bitmap (Type type, string resource)
			{
				return new Bitmap (type, resource);
			}

			public IBitmap Bitmap (int width, int height, int stride,
					System.Drawing.Imaging.PixelFormat format, IntPtr scan0)
			{
				return new Bitmap (width, height, stride, format, scan0);
			}
		}

		internal sealed class Bitmap : Image, IBitmap {

			float horizontalResolution;
			float verticalResolution;
			
			internal void CommonInit (int width, int height)
			{
				CommonInit (width, height, true);
			}

			internal void CommonInit (int width, int height, bool allocNativeObject)
			{
				size = new Size (width, height);
				imageFormat = ImageFormat.Bmp;
				switch( pixelFormat) {
				case PixelFormat.Format32bppArgb:
					xr_format = Xr.Format.ARGB32;
					if (allocNativeObject) native_object = GDK.gdk_pixbuf_new(0, true, 8, width, height);
					break;
				case PixelFormat.Format24bppRgb:
					xr_format = Xr.Format.RGB24;
					if (allocNativeObject) native_object = GDK.gdk_pixbuf_new(0, true, 8, width, height);
					break;
				default:
					throw new NotImplementedException ();
				}

				horizontalResolution = verticalResolution = 96.0F;
			}
			
			#region constructors
			/// <summary>
			/// Constructors
			/// </summary>

			internal Bitmap (int width, int height)
			{
				pixelFormat = PixelFormat.Format32bppArgb;
				CommonInit (width, height);
			}

			internal Bitmap (int width, int height, Graphics g)
			{
				// TODO: Get pixelFormat from g
				CommonInit (width,height);
				//TODO: use graphics to set vertial and horzontal resolution.
				//TODO: that is all the spec requires or desires
			}

			[MonoTODO]
			internal Bitmap (int width, int height, PixelFormat format)
			{
				this.pixelFormat = format;
				CommonInit (width, height);
			}

			internal Bitmap (Image origial)
			{
				throw new NotImplementedException ();
				//this.original = original;
			}
			
			unsafe void InitFromStream (Stream stream)
			{
				InternalImageInfo info = System.Drawing.Image.Decode (stream);
				if (info != null) {
					imageFormat = info.RawFormat;
					pixelFormat = info.PixelFormat;
					CommonInit (info.Size.Width, info.Size.Height, false);
					if (pixelFormat == PixelFormat.Format32bppArgb) {
						native_object = GDK.gdk_pixbuf_new_from_data (
							info.UnmanagedImagePtr, GDK.GdkColorspace.GDK_COLORSPACE_RGB, true, 8,
							((IImage) this).Size.Width, ((IImage) this).Size.Height,
							info.Stride, IntPtr.Zero, IntPtr.Zero);

					} else if (pixelFormat == PixelFormat.Format24bppRgb) {
						info.ChangePixelFormat (PixelFormat.Format32bppArgb);
						native_object = GDK.gdk_pixbuf_new_from_data (
							info.UnmanagedImagePtr, GDK.GdkColorspace.GDK_COLORSPACE_RGB, true, 8,
							((IImage) this).Size.Width, ((IImage) this).Size.Height,
							info.Stride, IntPtr.Zero, IntPtr.Zero);

					} else {
						throw new NotImplementedException();
					}
					// FIXME: It is not safe to dispose the info here
					// Memory leak!!!
					//info.Dispose();
				}
			}
			
			internal Bitmap (Stream stream)
				: this (stream, false)
			{
			}


			internal Bitmap (string filename)
				: this (filename, false)
			{
			}

			internal Bitmap (Image original, Size newSize)
			{
				throw new NotImplementedException ();
				//this.original = original;
				//this.newSize = newSize;
			}

			internal Bitmap (Stream stream, bool useIcm)
			{
				InitFromStream(stream);
			}

			internal Bitmap (string filename, bool useIcm)
			{
				FileStream file = new FileStream(filename, FileMode.Open);
				InitFromStream(file);
				file.Close();
			}

			internal Bitmap (Type type, string resource)
			{
				throw new NotImplementedException ();
				//this.type = type;
				//this.resource = resource;
			}

			internal Bitmap (Image original, int width, int heigth)
			{
				throw new NotImplementedException ();
				//this.original = original;
				//this.width = width;
				//this.heigth = heigth;
			}


			internal Bitmap (
				int width, int height, int stride,
				PixelFormat format, IntPtr scan0)
			{
				throw new NotImplementedException ();
				//this.width = width;
				//this.heigth = heigth;
				//this.stride = stride;
				//this.format = format;
				//this.scan0 = scan0;
			}
			#endregion

			// methods
			Color IBitmap.GetPixel (int x, int y)
			{
				throw new NotImplementedException ();
			}

			void IBitmap.SetPixel (int x, int y, Color color)
			{
				throw new NotImplementedException ();
			}
			IBitmap IBitmap.Clone (Rectangle rect,PixelFormat format)
			{
				throw new NotImplementedException ();
			}
		
			IBitmap IBitmap.Clone (RectangleF rect, PixelFormat format)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public static Bitmap FromHicon (IntPtr hicon) {
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public static Bitmap FromResource (IntPtr hinstance, string bitmapName)
			{
				throw new NotImplementedException ();
			}

			IntPtr IBitmap.GetHbitmap ()
			{
				throw new NotImplementedException ();
			}

			IntPtr IBitmap.GetHbitmap (Color background)
			{
				throw new NotImplementedException ();
			}

			IntPtr IBitmap.GetHicon ()
			{
				throw new NotImplementedException ();
			}


			[MonoTODO]
			BitmapData IBitmap.LockBits (Rectangle rect, ImageLockMode flags, PixelFormat format)
			{
				throw new NotImplementedException ();
			}

			void IBitmap.MakeTransparent ()
			{
				throw new NotImplementedException ();
			}

			void IBitmap.MakeTransparent (Color transparentColor)
			{
				throw new NotImplementedException ();
			}

			void IBitmap.SetResolution (float xDpi, float yDpi)
                        {
				horizontalResolution = xDpi;
				verticalResolution = yDpi;
			}
		
			//Fixme: gtk should be inherited from Image
			PixelFormat IImage.PixelFormat {
				get {
					return pixelFormat;
				}
			}

			void IBitmap.UnlockBits (BitmapData bitmapdata)
			{
				// nothing to do!
			}
		
			// properties
			// needs to be done ###FIXME###


			/// <summary>
			/// Methods 
			/// </summary>
			protected override void Dispose (bool disposing)
			{
				if (selected_into_graphics == null) {
					GDK.gdk_pixbuf_finalize (native_object);			
				}
				base.Dispose(disposing);
			}
			
			//FIXME: included with gtk code. should be inherited from Image.
			//		[MonoTODO]
			//		public override void RotateFlip (RotateFlipType rotateFlipType) {
			//			throw new NotImplementedException();
			//		}
		
		}
	}
}
