//
// System.Drawing.Text.PrivateFontCollection.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Everaldo Canuto everaldo.canuto@bol.com.br
//         Sanjay Gupta (gsanjay@novell.com)
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

		public PrivateFontCollection ()
		{
			Status status = GDIPlus.GdipNewPrivateFontCollection(out nativeFontCollection);
						
			if (status != Status.Ok)
			{
				nativeFontCollection = IntPtr.Zero;
				throw new Exception ("Error calling GDIPlus.GdipNewPrivateFontCollection: " +status);
			}
		}
		
		// methods
		[ComVisible(false)]
		public void AddFontFile(string filename) 
		{
			if (filename == null)
				throw new Exception("Value cannot be null, Parameter name : filename" );
			bool exists = File.Exists(filename);
			if (!exists)
				throw new Exception("The path is not of a legal form");

			Status status = GDIPlus.GdipPrivateAddFontFile(nativeFontCollection, filename);
						
			if (status != Status.Ok)
			{
				throw new Exception ("Error calling GDIPlus.GdipPrivateAddFontFile: " +status);
			}

			Console.WriteLine("Font file added to collection");

		}

		[ComVisible(false)]
		public void AddMemoryFont(IntPtr memory, int length) 
		{
		
		}

	}

}

