//
// System.Drawing.Bitmap.cs
//
// Copyright (C) 2002 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004 Novell, Inc.  http://www.novell.com
//
// Authors: 
//	Alexandre Pigolkine (pigolkine@gmx.de)
//	Christian Meyer (Christian.Meyer@cs.tum.edu)
//	Miguel de Icaza (miguel@ximian.com)
//	Jordi Mas i Hernandez (jmas@softcatala.org)
//	Ravindra (rkumar@novell.com)
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
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace System.Drawing
{
	[Serializable]
	[ComVisible (true)]
	[Editor ("System.Drawing.Design.BitmapEditor, " + Consts.AssemblySystem_Drawing_Design, typeof (System.Drawing.Design.UITypeEditor))]
	public sealed class Bitmap : Image
	{
		#region constructors
		// constructors
		internal Bitmap (IntPtr ptr)
		{
			nativeObject = ptr;
		}

		public Bitmap (int width, int height) : this (width, height, PixelFormat.Format32bppArgb)
		{
			
		}

		public Bitmap (int width, int height, Graphics g)
		{		
			IntPtr bmp;
			Status s = GDIPlus.GdipCreateBitmapFromGraphics (width, height, g.nativeObject, out bmp);
			GDIPlus.CheckStatus (s);
			nativeObject = (IntPtr)bmp;						
		}

		public Bitmap (int width, int height, PixelFormat format)
		{	
			IntPtr bmp;
			Status s = GDIPlus.GdipCreateBitmapFromScan0 (width, height, 0, format, IntPtr.Zero, out bmp);
			GDIPlus.CheckStatus (s);
			nativeObject = (IntPtr) bmp;
			
		}

		public Bitmap (Image original) : this (original.Width, original.Height, PixelFormat.Format32bppArgb)
		{
			BitmapFromImage(original, original.Size);
		}

		public Bitmap (Stream stream)  : this (stream, false) {} 

		public Bitmap (string filename) : this (filename, false) {}

		public Bitmap (Image original, Size newSize)  : this (newSize.Width, newSize.Height, PixelFormat.Format32bppArgb)
		{			
			Status          status;
			Graphics        g;

			g=Graphics.FromImage(this);

			status = GDIPlus.GdipDrawImageRectRectI(g.nativeObject, original.nativeObject,
				0, 0, newSize.Width, newSize.Height,
				0, 0, original.Width, original.Height,
				GraphicsUnit.Pixel, IntPtr.Zero, null, IntPtr.Zero);
			GDIPlus.CheckStatus (status);

			g.Dispose();
		}
		
		internal Bitmap (int width, int height, PixelFormat pixel, IntPtr bmp)
		{			
			nativeObject = (IntPtr)bmp;						
		}
		
		internal Bitmap (float width, float height, PixelFormat pixel, IntPtr bmp)
		{			
			nativeObject = (IntPtr)bmp;			
			
		}
		
		internal void BitmapFromImage(Image original, Size newSize){
			
			if (original is Bitmap) {
				
				if (nativeObject!=IntPtr.Zero) 
					Dispose();
				
				Bitmap bmpOriginal = (Bitmap) original;
					
				IntPtr bmp;
				Status s = GDIPlus.GdipCloneBitmapAreaI (0, 0, newSize.Width, newSize.Height, bmpOriginal.PixelFormat, bmpOriginal.nativeObject, out bmp);
				GDIPlus.CheckStatus (s);
				nativeObject = (IntPtr) bmp;
				
			}
			else {
				throw new NotImplementedException ();
			}
		}

		void InitFromFile (string filename)
		{		
			IntPtr imagePtr;
			Status st = GDIPlus.GdipLoadImageFromFile (filename, out imagePtr);
			GDIPlus.CheckStatus (st);
			nativeObject = imagePtr;			
		}

		public Bitmap (Stream stream, bool useIcm)
		{
			InitFromStream (stream);
		}

		public Bitmap (string filename, bool useIcm)
		{
			InitFromFile (filename);
		}

		public Bitmap (Type type, string resource)
		{
			using (Stream s = type.Assembly.GetManifestResourceStream (resource)){
				if (s == null)
					throw new FileNotFoundException ("Resource name was not found: `" + resource + "'");

				InitFromStream (s);
			}
		}

		public Bitmap (Image original, int width, int heigth) 
		{
			Size newSize = new Size();
			newSize.Height=heigth;
			newSize.Width=width;
			
			BitmapFromImage(original,newSize);
		}

		public Bitmap (int width, int height, int stride, PixelFormat format, IntPtr scan0)
		{		
			IntPtr bmp;
				
			Status status = GDIPlus.GdipCreateBitmapFromScan0 (width, height, stride, format, scan0, out bmp);
			GDIPlus.CheckStatus (status);	
			nativeObject = (IntPtr) bmp;						 								
		}

		//The below function is not required. Call should resolve to base
		//Moreover there is a problem with the declaration. Base class function
		//is not declared as protected to access in descendent class
		/*private Bitmap (SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}*/

		#endregion
		// methods
		public Color GetPixel (int x, int y) {
			
			int argb;				
			
			Status s = GDIPlus.GdipBitmapGetPixel(nativeObject, x, y, out argb);
			GDIPlus.CheckStatus (s);

			return Color.FromArgb(argb);		
		}

		public void SetPixel (int x, int y, Color color)
		{									
			Status s = GDIPlus.GdipBitmapSetPixel(nativeObject, x, y, color.ToArgb());
			GDIPlus.CheckStatus (s);
		}

		public Bitmap Clone (Rectangle rect,PixelFormat format)
		{				
			IntPtr bmp;			
   			Status status = GDIPlus.GdipCloneBitmapAreaI(rect.X, rect.Top, rect.Width, rect.Height,
                               PixelFormat, nativeObject,  out bmp);
                               
			GDIPlus.CheckStatus (status);

			Bitmap bmpnew = new Bitmap (rect.Width, rect.Height,  PixelFormat, (IntPtr) bmp);
       			return bmpnew;
       		}
		
		public Bitmap Clone (RectangleF rect, PixelFormat format)
		{
			IntPtr bmp;			
   			Status status = GDIPlus.GdipCloneBitmapArea (rect.X, rect.Top, rect.Width, rect.Height,
                               PixelFormat, nativeObject,  out bmp);
			GDIPlus.CheckStatus (status);

			Bitmap bmpnew = new Bitmap (rect.Width, rect.Height,  PixelFormat, (IntPtr) bmp);
	       		return bmpnew;
		}

		public static Bitmap FromHicon (IntPtr hicon)	//TODO: Untested
		{	
			IntPtr bitmap;	
				
			Status status = GDIPlus.GdipCreateBitmapFromHICON (hicon, out bitmap);
			GDIPlus.CheckStatus (status);

			return new Bitmap (0,0, PixelFormat.Format32bppArgb, bitmap);	// FIXME
		}

		public static Bitmap FromResource (IntPtr hinstance, string bitmapName)	//TODO: Untested
		{
			IntPtr bitmap;	
				
			Status status = GDIPlus.GdipCreateBitmapFromResource (hinstance, bitmapName, out bitmap);
			GDIPlus.CheckStatus (status);

			return new Bitmap (0,0, PixelFormat.Format32bppArgb, bitmap); // FIXME
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public IntPtr GetHbitmap ()
		{
			return GetHbitmap(Color.Gray);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public IntPtr GetHbitmap (Color background)
		{
			IntPtr HandleBmp;
			
			Status status = GDIPlus.GdipCreateHBITMAPFromBitmap (nativeObject, out HandleBmp, background.ToArgb ());
			GDIPlus.CheckStatus (status);

			return  HandleBmp;
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public IntPtr GetHicon ()
		{
			IntPtr HandleIcon;
			
			Status status = GDIPlus.GdipCreateHICONFromBitmap (nativeObject, out HandleIcon);
			GDIPlus.CheckStatus (status);

			return  HandleIcon;			
		}

		public BitmapData LockBits (Rectangle rect, ImageLockMode flags, PixelFormat format)
		{
			BitmapData result = new BitmapData();

			if (nativeObject == (IntPtr) 0)
				throw new Exception ("nativeObject is null");			
			
			IntPtr lfBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(result));
     			Marshal.StructureToPtr(result, lfBuffer, false);						
     		
			Status status = GDIPlus.GdipBitmapLockBits (nativeObject, ref rect, flags, format,  lfBuffer);
			
			result = (BitmapData) Marshal.PtrToStructure(lfBuffer,  typeof(BitmapData));											
			Marshal.FreeHGlobal (lfBuffer);			
			//NOTE: scan0 points to piece of memory allocated in the unmanaged space
			GDIPlus.CheckStatus (status);

			return  result;
		}

		public void MakeTransparent ()
		{
			Color clr = GetPixel(0,0);			
			MakeTransparent (clr);
		}

		public void MakeTransparent (Color transparentColor)
		{							
			// We have to draw always over a 32-bitmap surface that supports alpha channel
			Bitmap	bmp = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
			Graphics gr = Graphics.FromImage(bmp);
			Rectangle destRect = new Rectangle(0,0, Width, Height);
			ImageAttributes imageAttr = new ImageAttributes();
			
			imageAttr.SetColorKey(transparentColor,	transparentColor);

			gr.DrawImage (this, destRect, 0, 0, Width, Height, 	GraphicsUnit.Pixel, imageAttr);					
			
			Size newSize = new Size();
			newSize.Height=Height;
			newSize.Width=Width;			
			BitmapFromImage(bmp,newSize);			
			
			gr.Dispose();
			bmp.Dispose();
		}

		public void SetResolution (float xDpi, float yDpi)
		{
			Status status = GDIPlus.GdipBitmapSetResolution (nativeObject, xDpi, yDpi);
			GDIPlus.CheckStatus (status);
		}

		public void UnlockBits (BitmapData bitmap_data)
		{
			Status status = GDIPlus.GdipBitmapUnlockBits (nativeObject, bitmap_data);
			GDIPlus.CheckStatus (status);
		}
	}
}
