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
		IntPtr nativeStrFmt = IntPtr.Zero;
		
		public StringFormat()
		{						   
			Status status = GDIPlus.GdipCreateStringFormat (0, GDIPlus.LANG_NEUTRAL, out nativeStrFmt);        			
			
			if (status != Status.Ok)
				throw new ArgumentException ("Could not allocate string format: " + status);
				
			LineAlignment =  StringAlignment.Near;
			Alignment =  StringAlignment.Near;			
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
			if (disposing)
				GDIPlus.GdipDeleteStringFormat (nativeStrFmt);			
		}

		public StringFormat (StringFormat source)
		{			
			Status status = GDIPlus.GdipCloneStringFormat (source.NativeObject, out nativeStrFmt);
			
			if (status != Status.Ok)
				throw new ArgumentException ("Could not allocate string format: " + status);
		}

		public StringFormat (StringFormatFlags flags)
		{
			Status status = GDIPlus.GdipCreateStringFormat (flags, GDIPlus.LANG_NEUTRAL, out nativeStrFmt);        
			
			if (status != Status.Ok)
				throw new ArgumentException ("Could not allocate string format: " + status);
		}
		
		public StringAlignment Alignment {
			get {
					StringAlignment align;
					GDIPlus.GdipGetStringFormatAlign (nativeStrFmt, out align);
        			return align;
			}

			set {					
					GDIPlus.GdipSetStringFormatAlign (nativeStrFmt, value);				
			}
		}

		public StringAlignment LineAlignment {
			get {
					StringAlignment align;
					GDIPlus.GdipGetStringFormatLineAlign (nativeStrFmt, out align);        
					return align;
			}

			set {				
					GDIPlus.GdipSetStringFormatLineAlign (nativeStrFmt, value);
			}
		}

		public StringFormatFlags FormatFlags {
			get {				
					StringFormatFlags flags;
					
					GDIPlus.GdipGetStringFormatFlags (nativeStrFmt, out flags);
        			return flags;			
			}

			set {
					GDIPlus.GdipSetStringFormatFlags (nativeStrFmt, value);					
			}
		}

		public HotkeyPrefix HotkeyPrefix {
			get {				
					HotkeyPrefix hotkeyPrefix;
					GDIPlus.GdipGetStringFormatHotkeyPrefix (nativeStrFmt, out hotkeyPrefix);           
        			return hotkeyPrefix;
			}

			set {
								
					GDIPlus.GdipSetStringFormatHotkeyPrefix (nativeStrFmt, value);
			}
		}

		public void SetMeasurableCharacterRanges (CharacterRange [] range)
		{
				
		}

		public StringTrimming Trimming {
			get {
					StringTrimming trimming;
					GDIPlus.GdipGetStringFormatTrimming (nativeStrFmt, out trimming);
        			return trimming;
			}

			set {
					GDIPlus.GdipSetStringFormatTrimming (nativeStrFmt, value);        
			}
		}

		public static StringFormat GenericDefault {
			get {
					IntPtr ptr;
					
					Status status = GDIPlus.GdipStringFormatGetGenericDefault (out ptr);        
					
					if (status != Status.Ok)
						throw new ArgumentException ("Could not allocate string format: " + status);
						
					return new StringFormat (ptr);
			}
		}

		
		public static StringFormat GenericTypographic {
			get {
					IntPtr ptr;
					
					Status status = GDIPlus.GdipStringFormatGetGenericTypographic (out ptr);        
					
					if (status != Status.Ok)
						throw new ArgumentException ("Could not allocate string format: " + status);
						
					return new StringFormat (ptr);				
			}
		}

	
		public object Clone()
		{
			IntPtr native;
			
			Status status = GDIPlus.GdipCloneStringFormat (nativeStrFmt, out native);        			
					
			if (status != Status.Ok)
				throw new ArgumentException ("Could not allocate string format: " + status);
			
			return new StringFormat (native);
		}

		public override string ToString ()
		{
			return "[StringFormat, FormatFlags=" + this.FormatFlags.ToString() + "]";
		}
		
		internal IntPtr NativeObject{            
			get{
					return nativeStrFmt;
			}
			set	{
					nativeStrFmt = value;
			}
		}
		
	}
}
