//
// System.Drawing.Imaging.ImageFormat.cs
//
// Authors:
//   Everaldo Canuto (everaldo.canuto@bol.com.br)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Dennis Hayes (dennish@raytek.com)
//   Jordi Mas i Hernandez (jordi@ximian.com)
//
// (C) 2002-4 Ximian, Inc.  http://www.ximian.com
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
using System.ComponentModel;

namespace System.Drawing.Imaging {

	[TypeConverter (typeof (ImageFormatConverter))]
	public sealed class ImageFormat 
	{
		private Guid guid;
		private static ImageFormat BmpImageFormat = new ImageFormat (new Guid ("b96b3cab-0728-11d3-9d7b-0000f81ef32e"));
		private static ImageFormat EmfImageFormat = new ImageFormat (new Guid ("b96b3cac-0728-11d3-9d7b-0000f81ef32e"));
		private static ImageFormat ExifImageFormat = new ImageFormat (new Guid ("b96b3cb2-0728-11d3-9d7b-0000f81ef32e"));
		private static ImageFormat GifImageFormat = new ImageFormat (new Guid ("b96b3cb0-0728-11d3-9d7b-0000f81ef32e"));
		private static ImageFormat TiffImageFormat = new ImageFormat (new Guid ("b96b3cb1-0728-11d3-9d7b-0000f81ef32e"));
		private static ImageFormat PngImageFormat = new ImageFormat(new Guid("b96b3caf-0728-11d3-9d7b-0000f81ef32e"));
		private static ImageFormat MemoryBmpImageFormat = new ImageFormat (new Guid ("b96b3caa-0728-11d3-9d7b-0000f81ef32e"));
		private static ImageFormat IconImageFormat = new ImageFormat (new Guid ("b96b3cb5-0728-11d3-9d7b-0000f81ef32e"));
		private static ImageFormat JpegImageFormat = new ImageFormat(new Guid("b96b3cae-0728-11d3-9d7b-0000f81ef32e"));
		private static ImageFormat WmfImageFormat = new ImageFormat (new Guid ("b96b3cad-0728-11d3-9d7b-0000f81ef32e"));
		

		// constructors
		public ImageFormat (Guid guid) 
		{
			this.guid = guid;
		}
		

		// methods
		public override bool Equals(object o) {
			
			if (o is ImageFormat)
				if ( ((ImageFormat)o).Guid.Equals(this.Guid))
					return true;
			return false;

		}

		
		public override int GetHashCode() 
		{
			return guid.GetHashCode();
		}
		
		
		public override string ToString() 
		{			
			if (this.Equals (Bmp))
				return "Bmp";			
				
			if (this.Equals (Emf))
				return "Emf";			
				
			if (this.Equals (Exif))
				return "Exif";			
				
			if (this.Equals (Gif))
				return "Gif";			
				
			if (this.Equals (Tiff))
				return "Tiff";			
				
			if (this.Equals (Png))
				return "Png";			
				
			if (this.Equals (MemoryBmp))
				return "MemoryBmp";			
				
			if (this.Equals (Icon))
				return "Icon";			
				
			if (this.Equals (Jpeg))
				return "Jpeg";							
				
			if (this.Equals (Wmf))
				return "Wmf";												
			
			// Default			
			return ("[ImageFormat: " + Guid.ToString () + "]");


		}

		// properties
		public Guid Guid 
		{
			get { return guid; }
		}

		
		public static ImageFormat Bmp 
		{
			get { return BmpImageFormat; }
		}
		
		
		public static ImageFormat Emf 
		{
			get { return EmfImageFormat; }
		}
		
		
		public static ImageFormat Exif 
		{
			get { return ExifImageFormat; }
		}
		

		public static ImageFormat Gif 
		{
			get { return GifImageFormat; }
		}
		
		
		public static ImageFormat Icon 
		{
			get { return IconImageFormat; }
		}
		
		
		public static ImageFormat Jpeg 
		{
			get { return JpegImageFormat; }
		}
		
		
		public static ImageFormat MemoryBmp 
		{
			get { return MemoryBmpImageFormat; }
		}
		
		
		public static ImageFormat Png 
		{
			get { return PngImageFormat; }
		}
		
		
		public static ImageFormat Tiff 
		{
			get { return TiffImageFormat; }
		}
		
		
		public static ImageFormat Wmf 
		{
			get { return WmfImageFormat; }
		}
		
	}

}
