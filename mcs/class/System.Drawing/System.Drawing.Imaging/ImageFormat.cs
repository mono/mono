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
			if (Guid.Equals (Bmp.Guid))
				return "Bmp";			
				
			if (Guid.Equals (Emf.Guid))
				return "Emf";			
				
			if (Guid.Equals (Gif.Guid))
				return "Gif";			
				
			if (Guid.Equals (Jpeg.Guid))
				return "Jpeg";							
				
			if (Guid.Equals (MemoryBmp.Guid))
				return "MemoryBmp";			
				
			if (Guid.Equals (Png.Guid))
				return "Png";			
				
			if (Guid.Equals (Tiff.Guid))
				return "Tiff";			
				
			if (Guid.Equals (Wmf.Guid))
				return "Wmf";												
			
			// Default			
			return guid.ToString ();

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
