//
// System.Drawing.FontFamily.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//   Alexandre Pigolkine (pigolkine@gmx.de)
//
// (C) 2002/2003 Ximian, Inc
//
using System;
using System.Drawing.Text;
using System.Text;
using System.Runtime.InteropServices;

namespace System.Drawing {

	public sealed class FontFamily : MarshalByRefObject, IDisposable 
	{
		
		static FontFamily genericMonospace;
		static FontFamily genericSansSerif;
		static FontFamily genericSerif;

		string name;

		internal IntPtr nativeFontFamily = IntPtr.Zero;
				
		internal FontFamily ( IntPtr fntfamily )
		{
			nativeFontFamily = fntfamily;		
			int language = 0;
			
    		// Temporal until we fix bug 53700
			IntPtr dest = Marshal.AllocHGlobal (GDIPlus.FACESIZE * UnicodeEncoding.CharSize);            
			Status status = GDIPlus.GdipGetFamilyName (fntfamily, dest, language);
			byte[] marshalled = new byte[GDIPlus.FACESIZE* UnicodeEncoding.CharSize];    		
			Marshal.Copy (dest, marshalled, 0, GDIPlus.FACESIZE* UnicodeEncoding.CharSize);     		
			UnicodeEncoding enc = new UnicodeEncoding (false, true);           
			name = enc.GetString (marshalled);    		
		}
		
		//Need to come back here, is Arial the right thing to do
		internal FontFamily () : this ( "Arial", null )
		{
										
		}

		internal IntPtr NativeObject
		{            
			get	
			{
				return nativeFontFamily;
			}
			set	
			{
				nativeFontFamily = value;
			}
		}

		public FontFamily ( GenericFontFamilies genericFamily ) 
		{
			IntPtr generic = IntPtr.Zero;
			Status status;
			switch ( genericFamily ) 
			{
				case GenericFontFamilies.Monospace :
					status = GDIPlus.GdipGetGenericFontFamilyMonospace ( out generic );
					if ( status != Status.Ok ) 
					{
						generic = IntPtr.Zero;
						throw new Exception ( "Error calling GDIPlus.GdipGetGenericFontFamilyMonospace: " + status );
					}
					nativeFontFamily = generic ;
					name = "Courier New";
					break;
				case GenericFontFamilies.SansSerif :
					status = GDIPlus.GdipGetGenericFontFamilySansSerif ( out generic );
					if ( status != Status.Ok ) 
					{
						generic = IntPtr.Zero;
						throw new Exception ( "Error calling GDIPlus.GdipGetGenericFontFamilySansSerif: " + status );
					}
					nativeFontFamily = generic ;
					name = "Sans Serif";
					break;
				case GenericFontFamilies.Serif :
					status = GDIPlus.GdipGetGenericFontFamilySerif ( out generic );
					if ( status != Status.Ok ) 
					{
						generic = IntPtr.Zero;
						throw new Exception ( "Error calling GDIPlus.GdipGetGenericFontFamilySerif: " + status );
					}
					nativeFontFamily = generic ;
					name = "Times New Roman";
					break;
			}
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
						
			if ( status != Status.Ok )
			{
				nativeFontFamily = IntPtr.Zero;
				throw new Exception ( "Error calling GDIPlus.GdipCreateFontFamilyFromName: " + status );
			}
			name = familyName;
		}
		
		public string Name 
		{
			get 
			{
				return name;
			}
		}
		
		public static FontFamily GenericMonospace 
		{
			get 
			{
				if ( genericMonospace == null ) 
				{
					IntPtr generic = IntPtr.Zero;
					Status status = GDIPlus.GdipGetGenericFontFamilyMonospace ( out generic );
					if ( status != Status.Ok ) 
					{
						generic = IntPtr.Zero;
						throw new Exception ( "Error calling GDIPlus.GdipGetGenericFontFamilyMonospace: " + status );
					}
					genericMonospace = new FontFamily ( generic );
					genericMonospace.name = "Courier New";					
				}
				return genericMonospace;
			}
		}
		
		public static FontFamily GenericSansSerif 
		{
			get 
			{
				if ( genericSansSerif == null ) 
				{
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
		
		public static FontFamily GenericSerif 
		{
			get 
			{
				if ( genericSerif == null ) 
				{
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
		
		//[MONO TODO]
		//Need to check how to get the Flags attribute to read 
		//bitwise value of the enumeration
		internal int GetStyleCheck ( FontStyle style )
		{
			int styleCheck = 0 ;
			switch ( style) {
				case FontStyle.Bold :
					styleCheck = 1;
					break;
				case FontStyle.Italic :
					styleCheck = 2;
					break;
				case FontStyle.Regular :
					styleCheck = 0;
					break;
				case FontStyle.Strikeout :
					styleCheck = 8;
					break;
				case FontStyle.Underline :
					styleCheck = 4;
					break;
			}
			return styleCheck;
		}

		public int GetCellAscent ( FontStyle style ) 
		{
			Status status;
			uint outProperty;
			int styleCheck = GetStyleCheck ( style ) ;				
			status = GDIPlus.GdipGetCellAscent ( nativeFontFamily, styleCheck, out outProperty );
			if ( status != Status.Ok )
				throw new Exception ( "Error calling GDIPlus.GdipGetCellAscent: " + status );
			return (int)outProperty;
		}
		
		public int GetCellDescent ( FontStyle style ) 
		{
			Status status;
			uint outProperty;
			int styleCheck = GetStyleCheck ( style ) ;				
			status = GDIPlus.GdipGetCellDescent ( nativeFontFamily, styleCheck, out outProperty );
			if ( status != Status.Ok )
				throw new Exception ( "Error calling GDIPlus.GdipGetCellAscent: " + status );
			return (int)outProperty;
		}
		
		public int GetEmHeight ( FontStyle style ) 
		{
			Status status;
			uint outProperty;
			int styleCheck = GetStyleCheck ( style ) ;				
			status = GDIPlus.GdipGetEmHeight ( nativeFontFamily, styleCheck, out outProperty );
			if ( status != Status.Ok )
				throw new Exception ( "Error calling GDIPlus.GdipGetCellAscent: " + status );
			return (int)outProperty;
		}
		
		public int GetLineSpacing ( FontStyle style ) 
		{
			Status status;
			uint outProperty;
			int styleCheck = GetStyleCheck ( style ) ;				
			status = GDIPlus.GdipGetLineSpacing ( nativeFontFamily, styleCheck, out outProperty );
			if ( status != Status.Ok )
				throw new Exception ( "Error calling GDIPlus.GdipGetCellAscent: " + status );
			return (int)outProperty;
		}
		
		public bool IsStyleAvailable ( FontStyle style )
		{
			Status status;
			bool outProperty;
			int styleCheck = GetStyleCheck ( style ) ;				
			status = GDIPlus.GdipIsStyleAvailable ( nativeFontFamily, styleCheck, out outProperty );
			if ( status != Status.Ok )
				throw new Exception ( "Error calling GDIPlus.GdipGetCellAscent: " + status );
			return outProperty;
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

