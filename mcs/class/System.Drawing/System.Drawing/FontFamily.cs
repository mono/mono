//
// System.Drawing.FontFamily.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//   Alexandre Pigolkine (pigolkine@gmx.de)
// (C) 2002/2004 Ximian, Inc
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
				
		internal FontFamily(IntPtr fntfamily)
		{
			nativeFontFamily = fntfamily;		
			refreshName();			
		}
		
		internal void refreshName()
		{
			int language = 0;			
			StringBuilder sBuilder = new StringBuilder (GDIPlus.FACESIZE * UnicodeEncoding.CharSize);	
	    		Status status = GDIPlus.GdipGetFamilyName (nativeFontFamily, sBuilder, language);
    			name = sBuilder.ToString();    		    		
		}
		
		//Need to come back here, is Arial the right thing to do
		internal FontFamily() : this ("Arial", null)
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

		public FontFamily(GenericFontFamilies genericFamily) 
		{
			Status status;
			switch (genericFamily) 
			{
				case GenericFontFamilies.Monospace:
					status = GDIPlus.GdipGetGenericFontFamilyMonospace (out nativeFontFamily);
					if ( status != Status.Ok ) 
						throw new Exception ( "Error calling GDIPlus.GdipGetGenericFontFamilyMonospace: " + status );
					
					refreshName();
					break;
				case GenericFontFamilies.SansSerif:
					status = GDIPlus.GdipGetGenericFontFamilySansSerif (out nativeFontFamily);
					if ( status != Status.Ok ) 
						throw new Exception ( "Error calling GDIPlus.GdipGetGenericFontFamilySansSerif: " + status );
					
					refreshName();
					break;
				case GenericFontFamilies.Serif:
					status = GDIPlus.GdipGetGenericFontFamilySerif (out nativeFontFamily);
					if ( status != Status.Ok ) 
						throw new Exception ( "Error calling GDIPlus.GdipGetGenericFontFamilySerif: " + status );
					
					refreshName();
					break;
				default:	// Undocumented default 
					status = GDIPlus.GdipGetGenericFontFamilyMonospace (out nativeFontFamily);
					if ( status != Status.Ok ) 
						throw new Exception ( "Error calling GDIPlus.GdipGetGenericFontFamilyMonospace: " + status );
					
					refreshName();
					break;
			}
		}
		
		public FontFamily(string familyName) : this (familyName, null)
		{			
		}
		
		public FontFamily (string familyName, FontCollection collection) 
		{
			Status status;
			if ( collection != null )
				status = GDIPlus.GdipCreateFontFamilyFromName (familyName, collection.nativeFontCollection, out nativeFontFamily);
			else
				status = GDIPlus.GdipCreateFontFamilyFromName (familyName, IntPtr.Zero, out nativeFontFamily);
						
			if (status != Status.Ok)
			{
				nativeFontFamily = IntPtr.Zero;
				throw new Exception ( "Error calling GDIPlus.GdipCreateFontFamilyFromName: " + status );
			}
			
			refreshName();
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
				if (genericMonospace == null) 
				{
					IntPtr generic = IntPtr.Zero;
					Status status = GDIPlus.GdipGetGenericFontFamilyMonospace (out generic);
					if ( status != Status.Ok ) 
					{
						generic = IntPtr.Zero;
						throw new Exception ( "Error calling GDIPlus.GdipGetGenericFontFamilyMonospace: " + status );
					}
					genericMonospace = new FontFamily (generic);
					genericMonospace.refreshName();
				}
				return genericMonospace;
			}
		}
		
		public static FontFamily GenericSansSerif 
		{
			get 
			{
				if (genericSansSerif == null) 
				{
					IntPtr generic = IntPtr.Zero;
					Status status = GDIPlus.GdipGetGenericFontFamilySansSerif (out generic);
					if ( status != Status.Ok ) 
					{
						generic = IntPtr.Zero;
						throw new Exception ( "Error calling GDIPlus.GdipGetGenericFontFamilySansSerif: " + status );
					}
					genericSansSerif = new FontFamily ( generic );
					genericSansSerif.refreshName();
				}
				return genericSansSerif;
			}
		}
		
		public static FontFamily GenericSerif 
		{
			get 
			{
				if (genericSerif == null) 
				{
					IntPtr generic = IntPtr.Zero;
					Status status = GDIPlus.GdipGetGenericFontFamilySerif (out generic);
					if ( status != Status.Ok ) 
					{
						generic = IntPtr.Zero;
						throw new Exception ( "Error calling GDIPlus.GdipGetGenericFontFamilySerif: " + status );
					}
					genericSerif = new FontFamily (generic);
					genericSerif.refreshName();
				}
				return genericSerif;
			}
		}
		
		//[MONO TODO]
		//Need to check how to get the Flags attribute to read 
		//bitwise value of the enumeration
		internal int GetStyleCheck(FontStyle style)
		{
			int styleCheck = 0 ;
			switch ( style) {
				case FontStyle.Bold:
					styleCheck = 1;
					break;
				case FontStyle.Italic:
					styleCheck = 2;
					break;
				case FontStyle.Regular:
					styleCheck = 0;
					break;
				case FontStyle.Strikeout:
					styleCheck = 8;
					break;
				case FontStyle.Underline:
					styleCheck = 4;
					break;
			}
			return styleCheck;
		}

		public int GetCellAscent(FontStyle style) 
		{
			Status status;
			uint outProperty;
			int styleCheck = GetStyleCheck (style);				
			status = GDIPlus.GdipGetCellAscent (nativeFontFamily, styleCheck, out outProperty);
			if ( status != Status.Ok )
				throw new Exception ("Error calling GDIPlus.GdipGetCellAscent: " + status);
				
			return (int)outProperty;
		}
		
		public int GetCellDescent(FontStyle style) 
		{
			Status status;
			uint outProperty;
			int styleCheck = GetStyleCheck (style);				
			status = GDIPlus.GdipGetCellDescent (nativeFontFamily, styleCheck, out outProperty);
			if (status != Status.Ok)
				throw new Exception ("Error calling GDIPlus.GdipGetCellAscent: " + status);
				
			return (int)outProperty;
		}
		
		public int GetEmHeight(FontStyle style ) 
		{
			Status status;
			uint outProperty;
			int styleCheck = GetStyleCheck (style);				
			status = GDIPlus.GdipGetEmHeight (nativeFontFamily, styleCheck, out outProperty);
			if (status != Status.Ok)
				throw new Exception ("Error calling GDIPlus.GdipGetCellAscent: " + status );
				
			return (int)outProperty;
		}
		
		public int GetLineSpacing(FontStyle style) 
		{
			Status status;
			uint outProperty;
			int styleCheck = GetStyleCheck (style);				
			status = GDIPlus.GdipGetLineSpacing (nativeFontFamily, styleCheck, out outProperty);
			if (status != Status.Ok)
				throw new Exception ( "Error calling GDIPlus.GdipGetCellAscent: " + status);
				
			return (int)outProperty;
		}
		
		public bool IsStyleAvailable(FontStyle style)
		{
			Status status;
			bool outProperty;
			int styleCheck = GetStyleCheck (style);				
			status = GDIPlus.GdipIsStyleAvailable (nativeFontFamily, styleCheck, out outProperty);
			if (status != Status.Ok)
				throw new Exception ("Error calling GDIPlus.GdipGetCellAscent: " + status);
				
			return outProperty;
		}
		
		public void Dispose() 
		{
			Status status;
			if ( genericSerif != null ) {
				status = GDIPlus.GdipDeleteFontFamily (genericSerif.nativeFontFamily);
				if ( status != Status.Ok ) 
					genericSerif.nativeFontFamily = IntPtr.Zero;					
			}

			if ( genericSansSerif != null ) 
			{
				status = GDIPlus.GdipDeleteFontFamily (genericSansSerif.nativeFontFamily);
				if ( status != Status.Ok ) 
					genericSansSerif.nativeFontFamily = IntPtr.Zero;					
			}

			if ( genericMonospace != null ) 
			{
				status = GDIPlus.GdipDeleteFontFamily (genericMonospace.nativeFontFamily);
				if ( status != Status.Ok ) 
					genericMonospace.nativeFontFamily = IntPtr.Zero;					
			}

			status = GDIPlus.GdipDeleteFontFamily (nativeFontFamily);
			if ( status != Status.Ok ) 
				nativeFontFamily = IntPtr.Zero;					
			
		}
		
		
		public override bool Equals(object obj)
		{
			if (!(obj is FontFamily))
				return false;

			return (this == (FontFamily) obj);			
		}
		
		public override int GetHashCode()
		{
			return name.GetHashCode();			
		}
			
			
		public FontFamily[] Families
		{
			get {
				
				return GetFamilies (null);
			}
		}		
		
		public static FontFamily[] GetFamilies(Graphics graphics)
		{
			InstalledFontCollection fntcol = new InstalledFontCollection ();
			return fntcol.Families;			
		}
		
		[MonoTODO ("We only support the default system language")]
		public string GetName(int language)
		{
			return name;
		}
		
		public override string ToString()
		{
			return "FontFamily :" + name;
		}

	}
}

