//
// System.Drawing.FontFamily.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//   Alexandre Pigolkine (pigolkine@gmx.de)
//   Sanjay Gupta (gsanjay@novell.com)
//
// (C) 2002/2003 Ximian, Inc
//
using System;
using System.Drawing.Text;

namespace System.Drawing {

	public sealed class FontFamily : MarshalByRefObject, IDisposable {
		
		static FontFamily genericMonospace;
		static FontFamily genericSansSerif;
		static FontFamily genericSerif;

		string name;

		internal IntPtr nativeFontFamily = IntPtr.Zero;
				
		internal FontFamily ( IntPtr ptr )
		{
			nativeFontFamily = ptr;
		}
		
		//Need to come back here, is Arial the right thing to do
		internal FontFamily () : this ( "Arial", null )
		{
			//FIXME								
		}
		
		internal IntPtr NativeObject{            
			get{
					return nativeFontFamily;
			}
			set	{
					nativeFontFamily = value;
			}
		}

		public FontFamily ( GenericFontFamilies genericFamily ) 
		{
		}
		
		public FontFamily ( string familyName ) : this( familyName, null )
		{			
		}
		
		public FontFamily ( string familyName, FontCollection collection ) 
		{
			Status status;
			if ( collection != null )
				status = GDIPlus.GdipCreateFontFamilyFromName( familyName, collection.nativeFontCollection, out nativeFontFamily );
			else
				status = GDIPlus.GdipCreateFontFamilyFromName( familyName, IntPtr.Zero, out nativeFontFamily );
						
			if ( status != Status.Ok ){
				nativeFontFamily = IntPtr.Zero;
				throw new Exception ( "Error calling GDIPlus.GdipCreateFontFamilyFromName: " + status );
			}
			name = familyName;
		}
		
		public string Name {
			get {
				return name;
			}
		}
		
		public static FontFamily GenericMonospace {
			get {
				if ( genericMonospace == null ) {
					IntPtr generic = IntPtr.Zero;
					Status status = GDIPlus.GdipGetGenericFontFamilyMonospace ( out generic );
					if ( status != Status.Ok ) {
						generic = IntPtr.Zero;
						throw new Exception ( "Error calling GDIPlus.GdipGetGenericFontFamilyMonospace: " + status );
					}
					genericMonospace = new FontFamily ( generic );
					genericMonospace.name = "Courier New";					
				}
				return genericMonospace;
			}
		}
		
		public static FontFamily GenericSansSerif {
			get {
				if ( genericSansSerif == null ) {
					IntPtr generic = IntPtr.Zero;
					Status status = GDIPlus.GdipGetGenericFontFamilySansSerif ( out generic );
					if ( status != Status.Ok ) 
					{
						generic = IntPtr.Zero;
						throw new Exception ( "Error calling GDIPlus.GdipGetGenericFontFamilySansSerif: " + status );
					}
					genericSansSerif = new FontFamily ( generic );
					genericSansSerif.name = "Sans Serif";					
				}
				return genericSansSerif;
			}
		}
		
		public static FontFamily GenericSerif {
			get {
				if ( genericSerif == null ) {
					IntPtr generic = IntPtr.Zero;
					Status status = GDIPlus.GdipGetGenericFontFamilySerif ( out generic );
					if ( status != Status.Ok ) 
					{
						generic = IntPtr.Zero;
						throw new Exception ( "Error calling GDIPlus.GdipGetGenericFontFamilySerif: " + status );
					}
					genericSerif = new FontFamily ( generic );
					genericSerif.name = "Times New Roman";					
				}
				return genericSerif;
			}
		}
		
		public int GetCellAscent ( FontStyle style ) 
		{
			throw new NotImplementedException ();
		}
		
		public int GetCellDescent ( FontStyle style ) 
		{
			throw new NotImplementedException ();
		}
		
		public int GetEmHeight ( FontStyle style ) 
		{
			throw new NotImplementedException ();
		}
		
		public int GetLineSpacing ( FontStyle style ) 
		{
			throw new NotImplementedException ();
		}
		
		public bool IsStyleAvailable ( FontStyle style )
		{
			throw new NotImplementedException ();
		}
		
		public void Dispose() 
		{
			Status status;
			if ( genericSerif != null ) {
				status = GDIPlus.GdipDeleteFontFamily ( genericSerif.nativeFontFamily );
				if ( status != Status.Ok ) 
					genericSerif.nativeFontFamily = IntPtr.Zero;					
			}

			if ( genericSansSerif != null ) 
			{
				status = GDIPlus.GdipDeleteFontFamily ( genericSansSerif.nativeFontFamily );
				if ( status != Status.Ok ) 
					genericSansSerif.nativeFontFamily = IntPtr.Zero;					
			}

			if ( genericMonospace != null ) 
			{
				status = GDIPlus.GdipDeleteFontFamily ( genericMonospace.nativeFontFamily );
				if ( status != Status.Ok ) 
					genericMonospace.nativeFontFamily = IntPtr.Zero;					
			}

			status = GDIPlus.GdipDeleteFontFamily ( nativeFontFamily );
			if ( status != Status.Ok ) 
				nativeFontFamily = IntPtr.Zero;					
			
		}
	}
}

