//
// System.Drawing.Text.PrivateFontCollection.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Everaldo Canuto everaldo.canuto@bol.com.br
//		Sanjay Gupta (gsanjay@novell.com)
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
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Drawing.Text {

	[ComVisible(false)]
	public sealed class PrivateFontCollection : FontCollection {

		// constructors
		internal PrivateFontCollection (IntPtr ptr): base (ptr)
		{}

		public PrivateFontCollection()
		{
			Status status = GDIPlus.GdipNewPrivateFontCollection (out nativeFontCollection);
			GDIPlus.CheckStatus (status);
		}
		
		// methods
		public void AddFontFile(string filename) 
		{
			if ( filename == null )
				throw new Exception ("Value cannot be null, Parameter name : filename");
			bool exists = File.Exists(filename);
			if (!exists)
				throw new Exception ("The path is not of a legal form");

			Status status = GDIPlus.GdipPrivateAddFontFile (nativeFontCollection, filename);
			GDIPlus.CheckStatus (status);			
		}

		public void AddMemoryFont(IntPtr memory, int length) 
		{
			Status status = GDIPlus.GdipPrivateAddMemoryFont (nativeFontCollection, memory, length);
			GDIPlus.CheckStatus (status);						
		}
		
		// methods	
		protected override void Dispose(bool disposing)
		{
			if (nativeFontCollection!=IntPtr.Zero){				
				GDIPlus.GdipDeletePrivateFontCollection (nativeFontCollection);							
				nativeFontCollection = IntPtr.Zero;
			}
			
			base.Dispose (true);
		}		
		

	}
}

