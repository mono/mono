//
// System.Drawing.Imaging.MetafileHeader.cs
//
// Author: Everaldo Canuto
// eMail: everaldo.canuto@bol.com.br
// Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004, 2006 Novell, Inc (http://www.novell.com)
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

using System.Runtime.InteropServices;

namespace System.Drawing.Imaging {

	[MonoTODO ("Metafiles, both WMF and EMF formats, aren't supported.")]
#if !TARGET_JVM
	[StructLayout(LayoutKind.Sequential)]
#endif
	public sealed class MetafileHeader {
		
		//constructor
		internal MetafileHeader()
		{
			//Nothing to be done here
		}

		// methods

		public bool IsDisplay ()
		{
			throw new NotImplementedException ();
		}

		public bool IsEmf ()
		{
			throw new NotImplementedException ();
		}

		public bool IsEmfOrEmfPlus ()
		{
			throw new NotImplementedException ();
		}

		public bool IsEmfPlus ()
		{
			throw new NotImplementedException ();
		}

		public bool IsEmfPlusDual ()
		{
			throw new NotImplementedException ();
		}

		public bool IsEmfPlusOnly ()
		{
			throw new NotImplementedException ();
		}

		public bool IsWmf ()
		{
			throw new NotImplementedException ();
		}

		public bool IsWmfPlaceable ()
		{
			throw new NotImplementedException ();
		}

		// properties

		public Rectangle Bounds {
			get { throw new NotImplementedException (); }
		}

		public float DpiX {
			get { throw new NotImplementedException (); }
		}
		
		public float DpiY {
			get { throw new NotImplementedException (); }
		}
		
		public int EmfPlusHeaderSize {
			get { throw new NotImplementedException (); }
		}

		public int LogicalDpiX {
			get { throw new NotImplementedException (); }
		}
		
		public int LogicalDpiY {
			get { throw new NotImplementedException (); }
		}
		
		public int MetafileSize {
			get { throw new NotImplementedException (); }
		}

		public MetafileType Type {
			get { throw new NotImplementedException (); }
		}

		public int Version {
			get { throw new NotImplementedException (); }
		}
		
		public MetaHeader WmfHeader {
			get { throw new NotImplementedException (); }
		}
	}
}
