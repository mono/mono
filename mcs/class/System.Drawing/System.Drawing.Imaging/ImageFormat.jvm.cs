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
using javax.imageio;
using javax.imageio.spi;

namespace System.Drawing.Imaging {


	public sealed class ImageFormat {
		#region Statics
		static ImageFormat () {
			//remember mime according to the beginning of the string
			//workaround for com.sun.media.imageioimpl.plugins.bmp.BMPImageReader
			//which has mimeType="image/bmp, image/x-bmp, image/x-windows-bmp"

			String [] mimeReaders = javax.imageio.ImageIO.getReaderMIMETypes ();
			String [] mimeWriters = javax.imageio.ImageIO.getWriterMIMETypes ();
			String [] mimeTypes = new string [mimeReaders.Length+mimeWriters.Length];
			mimeReaders.CopyTo (mimeTypes, 0);
			mimeWriters.CopyTo (mimeTypes, mimeReaders.Length);

			for (int i = 0; i < mimeTypes.Length; i++) {
				if (mimeTypes [i].StartsWith ("image/bmp"))
					mimeBmp = mimeTypes [i];
				else if (mimeTypes [i].StartsWith ("image/gif"))
					mimeGif = mimeTypes [i];
				else if (mimeTypes [i].StartsWith ("image/tiff"))
					mimeTiff = mimeTypes [i];
				else if (mimeTypes [i].StartsWith ("image/png"))
					mimePng = mimeTypes [i];
				else if (mimeTypes [i].StartsWith ("image/x-ico"))
					mimeIcon = mimeTypes [i];
				else if (mimeTypes [i].StartsWith ("image/jpeg"))
					mimeJpeg = mimeTypes [i];
			}

			BmpImageFormat = new ImageFormat (mimeBmp, guidBmp, null, null);
			EmfImageFormat = new ImageFormat (null, guidEmf, null, null);
			ExifImageFormat = new ImageFormat (null, guidExif, null, null);
			GifImageFormat = new ImageFormat (mimeGif, guidGif, null, null);
			TiffImageFormat = new ImageFormat (mimeTiff, guidTiff, null, null);
			PngImageFormat = new ImageFormat (mimePng, guidPng, null, null);
			MemoryBmpImageFormat = new ImageFormat (null, guidMemoryBmp, null, null);
			IconImageFormat = new ImageFormat (mimeIcon, guidIcon, null, null);
			JpegImageFormat = new ImageFormat(mimeJpeg, guidJpeg, null, null);
			WmfImageFormat = new ImageFormat (null, guidWmf, null, null);
		}

		static Guid guidBmp = new Guid ("b96b3cab-0728-11d3-9d7b-0000f81ef32e");
		static Guid guidEmf = new Guid ("b96b3cac-0728-11d3-9d7b-0000f81ef32e");
		static Guid guidExif = new Guid ("b96b3cb2-0728-11d3-9d7b-0000f81ef32e");
		static Guid guidGif = new Guid ("b96b3cb0-0728-11d3-9d7b-0000f81ef32e");
		static Guid guidTiff = new Guid ("b96b3cb1-0728-11d3-9d7b-0000f81ef32e");
		static Guid guidPng = new Guid("b96b3caf-0728-11d3-9d7b-0000f81ef32e");
		static Guid guidMemoryBmp = new Guid ("b96b3caa-0728-11d3-9d7b-0000f81ef32e");
		static Guid guidIcon = new Guid ("b96b3cb5-0728-11d3-9d7b-0000f81ef32e");
		static Guid guidJpeg = new Guid("b96b3cae-0728-11d3-9d7b-0000f81ef32e");
		static Guid guidWmf = new Guid ("b96b3cad-0728-11d3-9d7b-0000f81ef32e");

		static string mimeBmp, mimeGif, mimeTiff, mimePng, mimeIcon, mimeJpeg;

		static ImageFormat BmpImageFormat;
		static ImageFormat EmfImageFormat;
		static ImageFormat ExifImageFormat;
		static ImageFormat GifImageFormat;
		static ImageFormat TiffImageFormat;
		static ImageFormat PngImageFormat;
		static ImageFormat MemoryBmpImageFormat;
		static ImageFormat IconImageFormat;
		static ImageFormat JpegImageFormat;
		static ImageFormat WmfImageFormat;

		static string GuidToMime (Guid guid) {
			if (guid.Equals (guidBmp))
				return mimeBmp;
			if (guid.Equals (guidGif))
				return mimeGif;
			if (guid.Equals (guidJpeg))
				return mimeJpeg;
			if (guid.Equals (guidPng))
				return mimePng;
			if (guid.Equals (guidTiff))
				return mimeTiff;
			if (guid.Equals (guidIcon))
				return mimeIcon;
			// Default			
			return null;
		}

		static Guid MimeToGuid (string mime) {
			if (mime == mimeBmp)
				return guidBmp;
			if (mime == mimeGif)
				return guidGif;
			if (mime == mimeJpeg)
				return guidJpeg;
			if (mime == mimePng)
				return guidPng;
			if (mime == mimeTiff)
				return guidTiff;
			// Default			
			return Guid.Empty;
		}

		#endregion

		#region Variables
		Guid _guid;
		string _mimeType;
		ImageReaderSpi _readerSpi;
		ImageWriterSpi _writerSpi;
		#endregion

		#region Constructors
		public ImageFormat (Guid guid)
			:this (GuidToMime (guid), guid, null, null) {}

		internal ImageFormat (string mimeType, ImageReaderSpi readerSpi, ImageWriterSpi writerSpi)
			:this (mimeType, MimeToGuid (mimeType), readerSpi, writerSpi) {}

		private ImageFormat (string mimeType, Guid guid,
			ImageReaderSpi readerSpi /*can be null*/,
			ImageWriterSpi writerSpi /*can be null*/)
		{
			_mimeType = mimeType;
			_guid = guid;

			if (writerSpi == null && mimeType != null) {
				java.util.Iterator iter = ImageIO.getImageWritersByMIMEType (mimeType);
				if (iter.hasNext ()) {
					ImageWriter writer = (ImageWriter) iter.next ();
					_writerSpi = writer.getOriginatingProvider ();
				} else
					_writerSpi = null;
			} else
				_writerSpi = writerSpi;

			if (readerSpi == null && mimeType != null) {
				java.util.Iterator iter = ImageIO.getImageReadersByMIMEType (mimeType);
				if (iter.hasNext ()) {
					ImageReader reader = (ImageReader) iter.next ();
					_readerSpi = reader.getOriginatingProvider ();
				} else
					_readerSpi = null;
			} else
				_readerSpi = readerSpi;
		}

		#endregion

		#region Methods
		public override bool Equals(object o) {
			ImageFormat f = o as ImageFormat;
			if (f != null) {
				if (f._guid == _guid)
					return true;
				else if (f._mimeType == _mimeType)
					return true;
				else if (f._readerSpi == _readerSpi && f._writerSpi == _writerSpi)
					return true;
			}
			return false;
		}

		public override int GetHashCode() {
			if (_guid != Guid.Empty)
				return _guid.GetHashCode ();
			else if (_mimeType != null)
				return _mimeType.GetHashCode ();
			else if (_readerSpi != null)
				return _readerSpi.GetHashCode();
			else if (_writerSpi != null)
				return _writerSpi.GetHashCode ();
			else
				return 0;
		}

		public override string ToString() {			
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
			return String.Concat("[ImageFormat: ", _guid, "]");
		}
		#endregion

		#region Properties
		public Guid Guid {
			get { return _guid; }
		}

		internal ImageReaderSpi ImageReaderSpi {
			get {
				return _readerSpi; 
			}
		}

		internal ImageWriterSpi ImageWriterSpi {
			get { 

				return _writerSpi; 
			}
		}

		#endregion

		#region Static properties
		public static ImageFormat Bmp {
			get { return BmpImageFormat; }
		}
		
		public static ImageFormat Emf {
			get { return EmfImageFormat; }
		}
		
		public static ImageFormat Exif {
			get { return ExifImageFormat; }
		}

		public static ImageFormat Gif {
			get { return GifImageFormat; }
		}
		
		public static ImageFormat Icon {
			get { return IconImageFormat; }
		}
		
		public static ImageFormat Jpeg {
			get { return JpegImageFormat; }
		}
		
		public static ImageFormat MemoryBmp {
			get { return MemoryBmpImageFormat; }
		}
		
		public static ImageFormat Png {
			get { return PngImageFormat; }
		}
		
		public static ImageFormat Tiff {
			get { return TiffImageFormat; }
		}
		
		public static ImageFormat Wmf {
			get { return WmfImageFormat; }
		}
		#endregion
	}
}
