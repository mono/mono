//
// System.Drawing.StringFormat.cs
//
// Authors:
//   Dennis Hayes (dennish@Raytek.com)
//   Miguel de Icaza (miguel@ximian.com)
//   Jordi Mas i Hernandez (jordi@ximian.com)
//
// (C) 2002 Ximian, Inc
// (C) 2003 Novell, Inc.
//
using System;
using System.Drawing.Text;

namespace System.Drawing
{
	/// <summary>
	/// Summary description for StringFormat.
	/// </summary>
	public sealed class StringFormat : MarshalByRefObject, IDisposable, ICloneable
	{
		private static StringFormat genericDefault;
		private IntPtr nativeStrFmt = IntPtr.Zero;
                private int language = GDIPlus.LANG_NEUTRAL;
		internal CharacterRange [] CharRanges;
		
		public StringFormat() : this (0, GDIPlus.LANG_NEUTRAL)
		{					   
			
		}		
		
		public StringFormat(StringFormatFlags options, int lang)
		{
			Status status = GDIPlus.GdipCreateStringFormat (options, lang, out nativeStrFmt);        			
			GDIPlus.CheckStatus (status);

			LineAlignment =  StringAlignment.Near;
			Alignment =  StringAlignment.Near;			
			language = lang;
		}
		
		internal StringFormat(IntPtr native)
		{
			nativeStrFmt = native;
		}
		
		~StringFormat()
		{	
			Dispose ();
		}
		
		public void Dispose()
		{	
			Dispose (true);
			System.GC.SuppressFinalize (this);
		}

		void Dispose (bool disposing)
		{
			if (disposing) {
				Status status = GDIPlus.GdipDeleteStringFormat (nativeStrFmt);
				GDIPlus.CheckStatus (status);
			}
		}

		public StringFormat (StringFormat source)
		{			
			Status status = GDIPlus.GdipCloneStringFormat (source.NativeObject, out nativeStrFmt);
			GDIPlus.CheckStatus (status);
		}

		public StringFormat (StringFormatFlags flags)
		{
			Status status = GDIPlus.GdipCreateStringFormat (flags, GDIPlus.LANG_NEUTRAL, out nativeStrFmt);
			GDIPlus.CheckStatus (status);
		}
		
		public StringAlignment Alignment {
			get {
                                StringAlignment align;
				Status status = GDIPlus.GdipGetStringFormatAlign (nativeStrFmt, out align);
				GDIPlus.CheckStatus (status);

        			return align;
			}

			set {					
				Status status = GDIPlus.GdipSetStringFormatAlign (nativeStrFmt, value);
				GDIPlus.CheckStatus (status);
			}
		}

		public StringAlignment LineAlignment {
			get {
				StringAlignment align;
				Status status = GDIPlus.GdipGetStringFormatLineAlign (nativeStrFmt, out align);
				GDIPlus.CheckStatus (status);

                                return align;
			}

			set {				
				Status status = GDIPlus.GdipSetStringFormatLineAlign (nativeStrFmt, value);
				GDIPlus.CheckStatus (status);
        		}
		}

		public StringFormatFlags FormatFlags {
			get {				
				StringFormatFlags flags;
				Status status = GDIPlus.GdipGetStringFormatFlags (nativeStrFmt, out flags);
				GDIPlus.CheckStatus (status);

        			return flags;			
			}

			set {
				Status status = GDIPlus.GdipSetStringFormatFlags (nativeStrFmt, value);
				GDIPlus.CheckStatus (status);
			}
		}

		public HotkeyPrefix HotkeyPrefix {
			get {				
				HotkeyPrefix hotkeyPrefix;
				Status status = GDIPlus.GdipGetStringFormatHotkeyPrefix (nativeStrFmt, out hotkeyPrefix);
				GDIPlus.CheckStatus (status);

               			return hotkeyPrefix;
			}

			set {
				Status status = GDIPlus.GdipSetStringFormatHotkeyPrefix (nativeStrFmt, value);
				GDIPlus.CheckStatus (status);
			}
		}


		public StringTrimming Trimming {
			get {
				StringTrimming trimming;
				Status status = GDIPlus.GdipGetStringFormatTrimming (nativeStrFmt, out trimming);
				GDIPlus.CheckStatus (status);
        			return trimming;
			}

			set {
				Status status = GDIPlus.GdipSetStringFormatTrimming (nativeStrFmt, value);
				GDIPlus.CheckStatus (status);
			}
		}

		public static StringFormat GenericDefault {
			get {
				IntPtr ptr;
				
				Status status = GDIPlus.GdipStringFormatGetGenericDefault (out ptr);
				GDIPlus.CheckStatus (status);

				return new StringFormat (ptr);
			}
		}
		
		
		public int DigitSubstitutionLanguage {
			get{
				return language;
			}
		}

		
		public static StringFormat GenericTypographic {
			get {
				IntPtr ptr;
					
				Status status = GDIPlus.GdipStringFormatGetGenericTypographic (out ptr);
				GDIPlus.CheckStatus (status);

				return new StringFormat (ptr);				
			}
		}

                public StringDigitSubstitute  DigitSubstitutionMethod  {
			get {
                                StringDigitSubstitute substitute;
                                
                                Status status = GDIPlus.GdipGetStringFormatDigitSubstitution(nativeStrFmt, language, out substitute);
				GDIPlus.CheckStatus (status);

                                return substitute;     
			}
		}


      		public void SetMeasurableCharacterRanges (CharacterRange [] range)
		{
			CharRanges=(CharacterRange [])range.Clone();
		}

		internal CharacterRange [] GetCharRanges
		{
			get {
				return(CharRanges);
			}
		}
	
		public object Clone()
		{
			IntPtr native;
			
			Status status = GDIPlus.GdipCloneStringFormat (nativeStrFmt, out native);
			GDIPlus.CheckStatus (status);

	        	return new StringFormat (native);
		}

		public override string ToString()
		{
			return "[StringFormat, FormatFlags=" + this.FormatFlags.ToString() + "]";
		}
		
		internal IntPtr NativeObject
                {            
			get{
				return nativeStrFmt;
			}
			set	{
				nativeStrFmt = value;
			}
		}

                public void SetTabStops(float firstTabOffset, float[] tabStops)
                {
			Status status = GDIPlus.GdipSetStringFormatTabStops(nativeStrFmt, firstTabOffset, tabStops.Length, tabStops);
			GDIPlus.CheckStatus (status);
                }

                public void SetDigitSubstitution(int language,  StringDigitSubstitute substitute)
                {
			Status status = GDIPlus.GdipSetStringFormatDigitSubstitution(nativeStrFmt, this.language, substitute);
			GDIPlus.CheckStatus (status);
                }

                public float[] GetTabStops(out float firstTabOffset)
                {
                        int count = 0;
                        firstTabOffset = 0;
                        
                        Status status = GDIPlus.GdipGetStringFormatTabStopCount(nativeStrFmt, out count);
			GDIPlus.CheckStatus (status);

                        float[] tabStops = new float[count];                        
                        
                        if (count != 0) {                        
                        	status = GDIPlus.GdipGetStringFormatTabStops(nativeStrFmt, count, out firstTabOffset, tabStops);
				GDIPlus.CheckStatus (status);
			}
                        	
                        return tabStops;                        
                }

	}
}
