//
// System.Drawing.Text.PrivateFontCollection.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Everaldo Canuto everaldo.canuto@bol.com.br
//		Sanjay Gupta (gsanjay@novell.com)
//
using System;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Drawing.Text {

	[ComVisible(false)]
	public sealed class PrivateFontCollection : FontCollection {

		// constructors
		internal PrivateFontCollection ( IntPtr ptr ): base ( ptr )
		{}

		public PrivateFontCollection ()
		{
			Status status = GDIPlus.GdipNewPrivateFontCollection( out nativeFontCollection );
			GDIPlus.CheckStatus ( status );
		}
		
		// methods
		[ComVisible(false)]
		public void AddFontFile( string filename ) 
		{
			if ( filename == null )
				throw new Exception ( "Value cannot be null, Parameter name : filename" );
			bool exists = File.Exists( filename );
			if ( !exists )
				throw new Exception ( "The path is not of a legal form" );

			Status status = GDIPlus.GdipPrivateAddFontFile ( nativeFontCollection, filename );
			GDIPlus.CheckStatus ( status );			
		}

		[ComVisible(false)]
		public void AddMemoryFont ( IntPtr memory, int length ) 
		{
			Status status = GDIPlus.GdipPrivateAddMemoryFont ( nativeFontCollection, memory, length );
			GDIPlus.CheckStatus ( status );						
		}
	}
}

