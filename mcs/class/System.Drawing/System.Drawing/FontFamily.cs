//
// System.Drawing.FontFamily.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//   Alexandre Pigolkine (pigolkine@gmx.de)
// (C) 2002/2004 Ximian, Inc
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
using System.Drawing.Text;
using System.Text;
using System.Runtime.InteropServices;

namespace System.Drawing {

	public sealed class FontFamily : MarshalByRefObject, IDisposable 
	{
		
		static private FontFamily genericMonospace = null;
		static private FontFamily genericSansSerif = null;
		static private FontFamily genericSerif = null;
		private string name;
		internal IntPtr nativeFontFamily = IntPtr.Zero;
				
		internal FontFamily(IntPtr fntfamily)
		{
			nativeFontFamily = fntfamily;		
			refreshName();			
		}
		
		internal void refreshName()
		{
			if (nativeFontFamily != IntPtr.Zero) {
				int language = 0;			
				StringBuilder sBuilder = new StringBuilder (GDIPlus.FACESIZE * UnicodeEncoding.CharSize);	
				Status status = GDIPlus.GdipGetFamilyName (nativeFontFamily, sBuilder, language);
				GDIPlus.CheckStatus (status);
				name = sBuilder.ToString();    		    		
			}
		}
		
		//Need to come back here, is Arial the right thing to do
		internal FontFamily() : this ("Arial", null)
		{
										
		}
		
		
		~FontFamily()
		{	
			Dispose ();
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
					GDIPlus.CheckStatus (status);
					refreshName ();
					break;
				case GenericFontFamilies.SansSerif:
					status = GDIPlus.GdipGetGenericFontFamilySansSerif (out nativeFontFamily);
					GDIPlus.CheckStatus (status);
					refreshName ();
					break;
				case GenericFontFamilies.Serif:
					status = GDIPlus.GdipGetGenericFontFamilySerif (out nativeFontFamily);
					GDIPlus.CheckStatus (status);
					refreshName ();
					break;
				default:	// Undocumented default 
					status = GDIPlus.GdipGetGenericFontFamilyMonospace (out nativeFontFamily);
					GDIPlus.CheckStatus (status);
					refreshName ();
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
			GDIPlus.CheckStatus (status);
			
			refreshName ();
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
					lock (typeof (FontFamily))
					{
						IntPtr generic = IntPtr.Zero;
						Status status = GDIPlus.GdipGetGenericFontFamilyMonospace (out generic);
						GDIPlus.CheckStatus (status);
						genericMonospace = new FontFamily (generic);
						genericMonospace.refreshName ();
					}
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
					lock (typeof (FontFamily))
					{
						IntPtr generic = IntPtr.Zero;
						Status status = GDIPlus.GdipGetGenericFontFamilySansSerif (out generic);
						GDIPlus.CheckStatus (status);
						genericSansSerif = new FontFamily (generic);
						genericSansSerif.refreshName ();
					}
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
					lock (typeof (FontFamily))
					{
						IntPtr generic = IntPtr.Zero;
						Status status = GDIPlus.GdipGetGenericFontFamilySerif (out generic);
						GDIPlus.CheckStatus (status);
						genericSerif = new FontFamily (generic);
						genericSerif.refreshName ();
					}
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

		public int GetCellAscent (FontStyle style) 
		{
			Status status;
			uint outProperty;
			int styleCheck = GetStyleCheck (style);				
			status = GDIPlus.GdipGetCellAscent (nativeFontFamily, styleCheck, out outProperty);
			GDIPlus.CheckStatus (status);

			return (int) outProperty;
		}
		
		public int GetCellDescent (FontStyle style) 
		{
			Status status;
			uint outProperty;
			int styleCheck = GetStyleCheck (style);				
			status = GDIPlus.GdipGetCellDescent (nativeFontFamily, styleCheck, out outProperty);
			GDIPlus.CheckStatus (status);

			return (int) outProperty;
		}
		
		public int GetEmHeight (FontStyle style) 
		{
			Status status;
			uint outProperty;
			int styleCheck = GetStyleCheck (style);				
			status = GDIPlus.GdipGetEmHeight (nativeFontFamily, styleCheck, out outProperty);
			GDIPlus.CheckStatus (status);

			return (int) outProperty;
		}
		
		public int GetLineSpacing (FontStyle style)
		{
			Status status;
			uint outProperty;
			int styleCheck = GetStyleCheck (style);				
			status = GDIPlus.GdipGetLineSpacing (nativeFontFamily, styleCheck, out outProperty);
			GDIPlus.CheckStatus (status);

			return (int) outProperty;
		}
		
		public bool IsStyleAvailable (FontStyle style)
		{
			Status status;
			bool outProperty;
			int styleCheck = GetStyleCheck (style);				
			status = GDIPlus.GdipIsStyleAvailable (nativeFontFamily, styleCheck, out outProperty);
			GDIPlus.CheckStatus (status);

			return outProperty;
		}
		
		public void Dispose ()
		{	
			lock (this)
			{
				Status status = GDIPlus.GdipDeleteFontFamily (nativeFontFamily);
				if ( status == Status.Ok ) 
					nativeFontFamily = IntPtr.Zero;							
			}
		}
		
		
		public override bool Equals(object obj)
		{
			if (!(obj is FontFamily))
				return false;

			return (this == (FontFamily) obj);			
		}
		
		public override int GetHashCode ()
		{
			return name.GetHashCode ();			
		}
			
			
		public static FontFamily[] Families
		{
			get {
				
				return GetFamilies (null);
			}
		}		
		
		public static FontFamily[] GetFamilies (Graphics graphics)
		{
			InstalledFontCollection fntcol = new InstalledFontCollection ();
			return fntcol.Families;			
		}
		
		[MonoTODO ("We only support the default system language")]
		public string GetName (int language)
		{
			return name;
		}
		
		public override string ToString ()
		{
			return "FontFamily :" + name;
		}

	}
}

