//
// System.Drawing.Imaging.ImageFormat.cs
//
// Authors:
//   Everaldo Canuto (everaldo.canuto@bol.com.br)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.ComponentModel;

namespace System.Drawing.Imaging {

	[TypeConverter (typeof (ImageFormatConverter))]
	public sealed class ImageFormat {

		Guid	guid;

		// constructors
		public ImageFormat(Guid guid) {
			this.guid = guid;
		}
		
		// methods
		[MonoTODO]
		public override bool Equals(object o) {
			return base.Equals(o);
		}

		[MonoTODO]
		public override int GetHashCode() {
			return base.GetHashCode();
		}
		
		public override string ToString() {
			if (this.Guid.Equals(ImageFormat.Bmp.Guid))
				return "Bmp";
			if (this.Guid.Equals(ImageFormat.Emf.Guid))
				return "Emf";
			if (this.Guid.Equals(ImageFormat.Exif.Guid))
				return "Exif";
			if (this.Guid.Equals(ImageFormat.Gif.Guid))
				return "Gif";
			if (this.Guid.Equals(ImageFormat.Icon.Guid))
				return "Icon";
			if (this.Guid.Equals(ImageFormat.Jpeg.Guid))
				return "Jpeg";
			if (this.Guid.Equals(ImageFormat.MemoryBmp.Guid))
				return "MemoryBMP";
			if (this.Guid.Equals(ImageFormat.Png.Guid))
				return "Png";
			if (this.Guid.Equals(ImageFormat.Tiff.Guid))
				return "Tiff";
			return "Wmf";		
		}

		// properties
		public Guid Guid {
			get { return guid; }
		}

		static ImageFormat BmpImageFormat = new ImageFormat (new Guid ("b96b3cab-0728-11d3-9d7b-0000f81ef32e"));
		public static ImageFormat Bmp {
			get { return BmpImageFormat; }
		}
		
		static ImageFormat EmfImageFormat = new ImageFormat (new Guid ("b96b3cac-0728-11d3-9d7b-0000f81ef32e"));
		public static ImageFormat Emf {
			get { return EmfImageFormat; }
		}
		
		static ImageFormat ExifImageFormat = new ImageFormat (new Guid ("b96b3cb2-0728-11d3-9d7b-0000f81ef32e"));
		public static ImageFormat Exif {
			get { return ExifImageFormat; }
		}
		
		static ImageFormat GifImageFormat = new ImageFormat (new Guid ("b96b3cb0-0728-11d3-9d7b-0000f81ef32e"));
		public static ImageFormat Gif {
			get { return GifImageFormat; }
		}
		
		static ImageFormat IconImageFormat = new ImageFormat (new Guid ("b96b3cb5-0728-11d3-9d7b-0000f81ef32e"));
		public static ImageFormat Icon {
			get { return IconImageFormat; }
		}
		
		static ImageFormat JpegImageFormat = new ImageFormat(new Guid("b96b3cae-0728-11d3-9d7b-0000f81ef32e"));
		public static ImageFormat Jpeg {
			get { return JpegImageFormat; }
		}
		
		static ImageFormat MemoryBmpImageFormat = new ImageFormat (new Guid ("b96b3caa-0728-11d3-9d7b-0000f81ef32e"));
		public static ImageFormat MemoryBmp {
			get { return MemoryBmpImageFormat; }
		}
		
		static ImageFormat PngImageFormat = new ImageFormat(new Guid("b96b3caf-0728-11d3-9d7b-0000f81ef32e"));
		public static ImageFormat Png {
			get { return PngImageFormat; }
		}
		
		static ImageFormat TiffImageFormat = new ImageFormat (new Guid ("b96b3cb1-0728-11d3-9d7b-0000f81ef32e"));
		public static ImageFormat Tiff {
			get { return TiffImageFormat; }
		}
		
		static ImageFormat WmfImageFormat = new ImageFormat (new Guid ("b96b3cad-0728-11d3-9d7b-0000f81ef32e"));
		public static ImageFormat Wmf {
			get { return WmfImageFormat; }
		}
		
	}

}
