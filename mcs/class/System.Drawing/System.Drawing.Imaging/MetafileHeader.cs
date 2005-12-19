//
// System.Drawing.Imaging.MetafileHeader.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Everaldo Canuto
// eMail: everaldo.canuto@bol.com.br
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
using System.Runtime.InteropServices;

namespace System.Drawing.Imaging {

	[MonoTODO]
#if !TARGET_JVM
	[StructLayout(LayoutKind.Sequential)]
#endif
	public sealed class MetafileHeader 
	{
		
		//constructor
		internal MetafileHeader()
		{
			//Nothing to be done here
		}

		// methods
		[MonoTODO]
		public bool IsDisplay() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsEmf() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsEmfOrEmfPlus() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsEmfPlus() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsEmfPlusDual() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsEmfPlusOnly() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsWmf() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsWmfPlaceable() {
			throw new NotImplementedException ();
		}

		// properties
		[MonoTODO]
		public Rectangle Bounds {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public float DpiX {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public float DpiY {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public int EmfPlusHeaderSize {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public int LogicalDpiX {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public int LogicalDpiY {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public int MetafileSize {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public MetafileType Type {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public int Version {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public MetaHeader WmfHeader {
			get { throw new NotImplementedException (); }
		}

	}

}
