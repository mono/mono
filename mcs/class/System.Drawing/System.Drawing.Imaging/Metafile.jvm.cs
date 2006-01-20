//
// System.Drawing.Imaging.Metafile.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Christian Meyer
// eMail: Christian.Meyer@cs.tum.edu
// Dennis Hayes (dennish@raytek.com)
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
using System.Reflection;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Drawing.Imaging {

	[Serializable]
	[ComVisible (false)]
#if SYSTEM_DRAWING_DESIGN_SUPPORT
	[Editor ("System.Drawing.Design.MetafileEditor, " + Consts.AssemblySystem_Drawing_Design, typeof (System.Drawing.Design.UITypeEditor))]
#endif
	[MonoTODO]
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

#if INTPTR_SUPPORT
		
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
		
		[MonoTODO]
		public Metafile (string fileName, IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit, EmfType type,
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
#endif

		[MonoTODO]
		public MetafileHeader GetMetafileHeader()
		{
			throw new NotFiniteNumberException();
		}

#if INTPTR_SUPPORT
		[MonoTODO]
		public static MetafileHeader GetMetafileHeader(IntPtr henhmetafile)
		{
			throw new NotImplementedException ();
		}
#endif
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

#if INTPTR_SUPPORT
		[MonoTODO]
		public static MetafileHeader GetMetafileHeader(IntPtr henhmetafile, WmfPlaceableFileHeader wmfHeader)
		{
			throw new NotImplementedException ();
		}
#endif
		[MonoTODO]
		public void PlayRecord(EmfPlusRecordType recordType, int flags, int dataSize, byte[] datawmfHeader)
		{
			throw new NotImplementedException ();
		}
		// properties

		[MonoTODO]
		protected override void InternalSave (javax.imageio.stream.ImageOutputStream output, Guid clsid) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override PixelFormat InternalPixelFormat {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		protected override java.awt.Image[] CloneNativeObjects(java.awt.Image[] src) {
			throw new NotImplementedException ();
		}

		#region Clone
		[MonoTODO]
		public override object Clone() {
			throw new NotImplementedException ();
		}
		#endregion

	}

}
