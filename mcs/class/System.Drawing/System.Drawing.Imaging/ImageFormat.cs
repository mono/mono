//
// System.Drawing.Imaging.ImageFormat.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Everaldo Canuto
// eMail: everaldo.canuto@bol.com.br
// Dennis Hayes (dennish@raytek.com)
//
using System;

namespace System.Drawing.Imaging {

	public sealed class ImageFormat {

		Guid	guid;

		// constructors
		[MonoTODO]
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
		
		[MonoTODO]
		public override string ToString() {
			return String.Format("ImageFormat.Guid {0}", guid);
		}

		// properties
		static ImageFormat BmpImageFormat = new ImageFormat (new Guid ("DFB9AC7D-498D-4bd8-9D42-E23E541964B1"));

		[MonoTODO]
		public static ImageFormat Bmp {
			get { return BmpImageFormat; }
		}
		
		[MonoTODO]
		public static ImageFormat Emf {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public static ImageFormat Exif {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public static ImageFormat Gif {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public Guid Guid {
			get { return guid; }
		}
		
		[MonoTODO]
		public static ImageFormat Icon {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO] 
		static ImageFormat JpegImageFormat = new ImageFormat(new Guid("83BFFDF8-398F-407f-BA33-A7993D11B2DA"));
		public static ImageFormat Jpeg {
			get { return JpegImageFormat; }
		}
		
		[MonoTODO]
		public static ImageFormat MemoryBmp {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public static ImageFormat Png {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public static ImageFormat Tiff {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public static ImageFormat Wmf {
			get { throw new NotImplementedException (); }
		}
		
	}

}
