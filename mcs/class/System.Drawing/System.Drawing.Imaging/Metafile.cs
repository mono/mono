//
// System.Drawing.Imaging.Metafile.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Christian Meyer
// eMail: Christian.Meyer@cs.tum.edu
// Dennis Hayes (dennish@raytek.com)
//
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Drawing.Imaging {

	[Serializable]
	[ComVisible (false)]
	public sealed class Metafile : Image {

		// constructors
		[MonoTODO]
		public Metafile (Stream stream) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Metafile (string filename) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Metafile (IntPtr henhmetafile, bool deleteEmf) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Metafile (IntPtr referenceHtc, EmfType emfType) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Metafile (IntPtr referenceHtc, Rectangle frameRect) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Metafile (IntPtr referenceHtc, RectangleF frameRect) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Metafile (IntPtr hmetafile, WmfPlaceableFileHeader wmfHeader) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Metafile (Stream stream, IntPtr referenceHtc) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Metafile (String fileName, IntPtr referenceHtc) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Metafile (IntPtr referenceHdc, EmfType emfType, string description) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Metafile (IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Metafile (IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Metafile (IntPtr hmetafile, WmfPlaceableFileHeader wmfHeader, bool deleteWmf) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Metafile (Stream stream, IntPtr referenceHdc, EmfType type) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Metafile (Stream stream, IntPtr referenceHdc, Rectangle frameRect) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Metafile (Stream stream, IntPtr referenceHdc, RectangleF frameRect) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Metafile (string fileName, IntPtr referenceHdc, EmfType type) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Metafile (string fileName, IntPtr referenceHdc, Rectangle frameRect) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public Metafile (string fileName, IntPtr referenceHdc, RectangleF frameRect) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Metafile (IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit, EmfType type) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Metafile (IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit, EmfType type) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Metafile (Stream stream, IntPtr referenceHtc, EmfType type, string description) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Metafile (Stream stream, IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Metafile (Stream stream, IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Metafile (string fileName, IntPtr referenceHdc, EmfType type, string description)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Metafile (string fileName, IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public Metafile (string fileName, IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Metafile (IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit, EmfType type, string description) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Metafile (IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit, EmfType type, string description) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Metafile (Stream stream, IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit, EmfType type) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Metafile (Stream stream, IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit, EmfType type) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Metafile (string fileName, IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit, EmfType type) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public Metafile (string fileName, IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit, string description) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Metafile (string fileName, IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit, EmfType type) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public Metafile (string fileName, IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit, string description) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public Metafile (Stream stream, IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit, EmfType type,
															string description) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Metafile (Stream stream, IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit, EmfType type,
															string description) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Metafile (string fileName, IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit, EmfType type,
															string description) 
		{
			throw new NotImplementedException ();
		}
		
		// methods
		[MonoTODO]
		public IntPtr GetHenhmetafile()
		{
			throw new NotImplementedException ();
		}

		public MetafileHeader GetMetafileHeader()
		{
			return GetMetafileHeader (GetHenhmetafile () );
		}

		[MonoTODO]
		public static MetafileHeader GetMetafileHeader(IntPtr henhmetafile)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MetafileHeader GetMetafileHeader(Stream stream)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MetafileHeader GetMetafileHeader(string fileName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MetafileHeader GetMetafileHeader(IntPtr henhmetafile, WmfPlaceableFileHeader wmfHeader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void PlayRecord(EmfPlusRecordType recordType, int flags, int dataSize, byte[] datawmfHeader)
		{
			throw new NotImplementedException ();
		}
		// properties
	}

}
