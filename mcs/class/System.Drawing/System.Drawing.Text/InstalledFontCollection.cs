//
// System.Drawing.Text.InstalledFontCollection.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Everaldo Canuto everaldo.canuto@bol.com.br
//         Alexandre Pigolkine ( pigolkine@gmx.de)
//			Sanjay Gupta (gsanjay@novell.com)
//
using System;
using System.Drawing;

namespace System.Drawing.Text {

	public sealed class InstalledFontCollection : FontCollection {
		
		internal InstalledFontCollection ( IntPtr ptr ): base ( ptr )
		{}

		public InstalledFontCollection ()
		{
			Status status = GDIPlus.GdipNewInstalledFontCollection( out nativeFontCollection );
						
			if ( status != Status.Ok ) {
				nativeFontCollection = IntPtr.Zero;
				throw new Exception ( "Error calling GDIPlus.GdipNewInstalledFontCollection: " + status );
			}			
		}
	}
}

