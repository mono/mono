//
// System.Drawing.Fonts.cs
//
// Authors:
//	Alexandre Pigolkine (pigolkine@gmx.de)
//	Miguel de Icaza (miguel@ximian.com)
//	Todd Berman (tberman@sevenl.com)
//	Jordi Mas i Hernandez (jordi@ximian.com)
//	Ravindra (rkumar@novell.com)
//
// Copyright (C) 2004 Ximian, Inc. (http://www.ximian.com)
// Copyright (C) 2004, 2006 Novell, Inc (http://www.novell.com)
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

using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace System.Drawing
{
	[Serializable]
	[ComVisible (true)]
	[Editor ("System.Drawing.Design.FontEditor, " + Consts.AssemblySystem_Drawing_Design, typeof (System.Drawing.Design.UITypeEditor))]
	[TypeConverter (typeof (FontConverter))]
	public sealed class Font : MarshalByRefObject, ISerializable, ICloneable, IDisposable
	{
		private IntPtr	fontObject = IntPtr.Zero;
		private string  systemFontName;
		private float _size;

		private void CreateFont(string familyName, float emSize, FontStyle style, GraphicsUnit unit, byte charSet, bool isVertical) {
                        Status		status;                  
                        FontFamily	family;                      

			// NOTE: If family name is null, empty or invalid,
			// MS creates Microsoft Sans Serif font.
			try {
				family = new FontFamily (familyName);
			}
			catch (Exception){
				family = FontFamily.GenericSansSerif;
			}

			setProperties (family, emSize, style, unit, charSet, isVertical);           
			status = GDIPlus.GdipCreateFont (family.NativeObject, emSize,  style, unit, out fontObject);
			GDIPlus.CheckStatus (status);
		}

       		private Font (SerializationInfo info, StreamingContext context)
		{
			string		name;
			float		size;
			FontStyle	style;
			GraphicsUnit	unit;

			name = (string)info.GetValue("Name", typeof(string));
			size = (float)info.GetValue("Size", typeof(float));
			style = (FontStyle)info.GetValue("Style", typeof(FontStyle));
			unit = (GraphicsUnit)info.GetValue("Unit", typeof(GraphicsUnit));
 
			CreateFont(name, size, style, unit, (byte)0, false);
		}

		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Name", Name);
			info.AddValue("Size", Size);
			info.AddValue("Style", Style);
			info.AddValue("Unit", Unit);
		}

		~Font()
		{
			Dispose ();
		}

		public void Dispose ()
		{
			if (fontObject != IntPtr.Zero) {
				GDIPlus.CheckStatus (GDIPlus.GdipDeleteFont (fontObject));
				fontObject = IntPtr.Zero;
				GC.SuppressFinalize (this);
			}
		}

		internal void unitConversion (GraphicsUnit fromUnit, GraphicsUnit toUnit, float nSrc, out float nTrg)
		{
			float inchs = 0;
			nTrg = 0;
			
			switch (fromUnit) {
			case GraphicsUnit.Display:
				inchs = nSrc / 75f;
				break;
			case GraphicsUnit.Document:
				inchs = nSrc / 300f;
				break;
			case GraphicsUnit.Inch:
				inchs = nSrc;
				break;
			case GraphicsUnit.Millimeter:
				inchs = nSrc / 25.4f;
				break;
			case GraphicsUnit.Pixel:
			case GraphicsUnit.World:
				inchs = nSrc / Graphics.systemDpiX;
				break;
			case GraphicsUnit.Point:
				inchs = nSrc / 72f;
				break;
			default:
				throw new ArgumentException("Invalid GraphicsUnit");
			}

			switch (toUnit) {
			case GraphicsUnit.Display:
				nTrg = inchs * 75;
				break;
			case GraphicsUnit.Document:
				nTrg = inchs * 300;
				break;
			case GraphicsUnit.Inch:
				nTrg = inchs;
				break;
			case GraphicsUnit.Millimeter:
				nTrg = inchs * 25.4f;
				break;
			case GraphicsUnit.Pixel:
			case GraphicsUnit.World:
				nTrg = inchs * Graphics.systemDpiX;
				break;
			case GraphicsUnit.Point:
				nTrg = inchs * 72;
				break;
			default:
				throw new ArgumentException("Invalid GraphicsUnit");
			}
		}

		internal void setProperties (FontFamily family, float emSize, FontStyle style, GraphicsUnit unit, byte charSet, bool isVertical)
		{
			_name = family.Name;
			_fontFamily = family;			
			_size = emSize;

			// MS throws ArgumentException, if unit is set to GraphicsUnit.Display
			_unit = unit;
			_style = style;
			_gdiCharSet = charSet;
			_gdiVerticalFont = isVertical;
			
			unitConversion (unit, GraphicsUnit.Point, emSize, out  _sizeInPoints);
						
			_bold = _italic = _strikeout = _underline = false;

                        if ((style & FontStyle.Bold) == FontStyle.Bold)
                                _bold = true;
				
                        if ((style & FontStyle.Italic) == FontStyle.Italic)
                               _italic = true;

                        if ((style & FontStyle.Strikeout) == FontStyle.Strikeout)
                                _strikeout = true;

                        if ((style & FontStyle.Underline) == FontStyle.Underline)
                                _underline = true;                  
		}

		public static Font FromHfont (IntPtr Hfont)
		{
			OperatingSystem	osInfo = Environment.OSVersion;
			IntPtr			newObject;
			IntPtr			hdc;			
			FontStyle		newStyle = FontStyle.Regular;
			float			newSize;
			LOGFONT			lf = new LOGFONT ();

			// Sanity. Should we throw an exception?
			if (Hfont == IntPtr.Zero) {
				Font result = new Font ("Arial", (float)10.0, FontStyle.Regular);
				return(result);
			}

			if ((int) osInfo.Platform == 128 || (int) osInfo.Platform == 4) {
				// If we're on Unix we use our private gdiplus API to avoid Wine 
				// dependencies in S.D
				Status s = GDIPlus.GdipCreateFontFromHfont (Hfont, out newObject, ref lf);
				GDIPlus.CheckStatus (s);
			} else {

				// This needs testing
				// GetDC, SelectObject, ReleaseDC GetTextMetric and
				// GetFontFace are not really GDIPlus, see gdipFunctions.cs

				newStyle = FontStyle.Regular;

				hdc = GDIPlus.GetDC (IntPtr.Zero);
				Font f = FromLogFont (lf, hdc);
				GDIPlus.ReleaseDC (hdc);
				return f;				
			}
			
			if (lf.lfItalic != 0) {
				newStyle |= FontStyle.Italic;
			}

			if (lf.lfUnderline != 0) {
				newStyle |= FontStyle.Underline;
			}

			if (lf.lfStrikeOut != 0) {
				newStyle |= FontStyle.Strikeout;
			}

			if (lf.lfWeight > 400) {
				newStyle |= FontStyle.Bold;
			}

			if (lf.lfHeight < 0) {
				newSize = lf.lfHeight * -1;
			} else {
				newSize = lf.lfHeight;
			}

			return (new Font (newObject, lf.lfFaceName, newStyle, newSize));
		}

		public IntPtr ToHfont ()
		{
			IntPtr Hfont;
			OperatingSystem	osInfo = Environment.OSVersion;

			// Sanity. Should we throw an exception?
			if (fontObject == IntPtr.Zero) {
				return IntPtr.Zero;
			}

			if ((int) osInfo.Platform == 128 || (int) osInfo.Platform == 4) {
				return fontObject;
			} else {
				LOGFONT lf = new LOGFONT ();
				ToLogFont(lf);
				Hfont = GDIPlus.CreateFontIndirect (ref lf);
			}
			return Hfont;
		}

		internal Font (IntPtr newFontObject, string familyName, FontStyle style, float size)
		{
			FontFamily fontFamily;			
			
			try {
				fontFamily = new FontFamily (familyName);
			}
			catch (Exception){
				fontFamily = FontFamily.GenericSansSerif;
			}
			
			setProperties (fontFamily, size, style, GraphicsUnit.Pixel, 0, false);
			fontObject = newFontObject;
		}

		public Font (Font original, FontStyle style)
		{
			Status status;
			setProperties (original.FontFamily, original.Size, style, original.Unit, original.GdiCharSet, original.GdiVerticalFont);
				
			status = GDIPlus.GdipCreateFont (_fontFamily.NativeObject,	Size,  Style,   Unit,  out fontObject);
			GDIPlus.CheckStatus (status);			
		}

		public Font (FontFamily family, float emSize,  GraphicsUnit unit)
			: this (family, emSize, FontStyle.Regular, unit, (byte)0, false)
		{
		}

		public Font (string familyName, float emSize,  GraphicsUnit unit)
			: this (new FontFamily (familyName), emSize, FontStyle.Regular, unit, (byte)0, false)
		{
		}

		public Font (FontFamily family, float emSize)
			: this (family, emSize, FontStyle.Regular, GraphicsUnit.Point, (byte)0, false)
		{
		}

		public Font (FontFamily family, float emSize, FontStyle style)
			: this (family, emSize, style, GraphicsUnit.Point, (byte)0, false)
		{
		}

		public Font (FontFamily family, float emSize, FontStyle style, GraphicsUnit unit)
			: this (family, emSize, style, unit, (byte)0, false)
		{
		}

		public Font (FontFamily family, float emSize, FontStyle style, GraphicsUnit unit, byte charSet)
			: this (family, emSize, style, unit, charSet, false)
		{
		}

		public Font (FontFamily family, float emSize, FontStyle style,
				GraphicsUnit unit, byte charSet, bool isVertical)
		{
			// MS does not accept null family
			Status status;
			setProperties (family, emSize, style, unit, charSet, isVertical);		
			status = GDIPlus.GdipCreateFont (family.NativeObject, emSize,  style,   unit,  out fontObject);
			GDIPlus.CheckStatus (status);
		}

		public Font (string familyName, float emSize)
			: this (familyName, emSize, FontStyle.Regular, GraphicsUnit.Point, (byte)0, false)
		{
		}

		public Font (string familyName, float emSize, FontStyle style)
			: this (familyName, emSize, style, GraphicsUnit.Point, (byte)0, false)
		{
		}

		public Font (string familyName, float emSize, FontStyle style, GraphicsUnit unit)
			: this (familyName, emSize, style, unit, (byte)0, false)
		{
		}

		public Font (string familyName, float emSize, FontStyle style, GraphicsUnit unit, byte charSet)
			: this (familyName, emSize, style, unit, charSet, false)
		{
		}

		public Font (string familyName, float emSize, FontStyle style,
				GraphicsUnit unit, byte charSet, bool isVertical)
		{
			CreateFont(familyName, emSize, style, unit, charSet, isVertical);
		}

		public object Clone ()
		{
			return new Font (this, Style);
		}

		internal IntPtr NativeObject {            
			get {
					return fontObject;
			}
			set {
					fontObject = value;
			}
		}

#if NET_2_0
		internal string SysFontName {
			set {
				systemFontName = value;
			}
		}
#endif

		private bool _bold;

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public bool Bold {
			get {
				return _bold;
			}
		}

		private FontFamily _fontFamily;

		[Browsable (false)]
		public FontFamily FontFamily {
			get {
				return _fontFamily;
			}
		}

		private byte _gdiCharSet;

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public byte GdiCharSet {
			get {
				return _gdiCharSet;
			}
		}

		private bool _gdiVerticalFont;

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public bool GdiVerticalFont {
			get {
				return _gdiVerticalFont;
			}
		}

		[Browsable (false)]
		public int Height {
			get {
				return (int) Math.Ceiling (GetHeight ());
			}
		}

#if NET_2_0
		[Browsable(false)]
		public bool IsSystemFont {
			get {
				if (systemFontName == null)
					return false;

				return StringComparer.InvariantCulture.Compare (systemFontName, string.Empty) != 0;
			}
		}
#endif

		private bool _italic;

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public bool Italic {
			get {
				return _italic;
			}
		}

		private string _name;

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Editor ("System.Drawing.Design.FontNameEditor, " + Consts.AssemblySystem_Drawing_Design, typeof (System.Drawing.Design.UITypeEditor))]
		[TypeConverter (typeof (FontConverter.FontNameConverter))]
		public string Name {
			get {
				return _name;
			}
		}
		
		public float Size {
			get {
				return _size;			
			}
		}

		private float _sizeInPoints;

		[Browsable (false)]
		public float SizeInPoints {
			get {
				return _sizeInPoints;
			}
		}

		private bool _strikeout;

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public bool Strikeout {
			get {
				return _strikeout;
			}
		}
		
		private FontStyle _style;

		[Browsable (false)]
		public FontStyle Style {
			get {
				return _style;
			}
		}

#if NET_2_0
		[Browsable(false)]
		public string SystemFontName {
			get {
				return systemFontName;
			}
		}
#endif
		private bool _underline;

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public bool Underline {
			get {
				return _underline;
			}
		}

		private GraphicsUnit _unit;

		[TypeConverter (typeof (FontConverter.FontUnitConverter))]
		public GraphicsUnit Unit {
			get {
				return _unit;
			}
		}

		public override bool Equals (object obj)
		{
			if (! (obj is Font))
				return false;
				
			Font fnt = (Font) obj;			
			
			if (fnt.FontFamily.Equals (FontFamily) && fnt.Size == Size &&
			    fnt.Style == Style && fnt.Unit == Unit &&
			    fnt.GdiCharSet == GdiCharSet && 
			    fnt.GdiVerticalFont == GdiVerticalFont)
				return true;
			else
				return false;
		}

		public override int GetHashCode ()
		{
			return _name.GetHashCode ();
		}

		[MonoTODO]
		public static Font FromHdc (IntPtr hdc)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("This is temporary implementation")]
		public static Font FromLogFont (object lf,  IntPtr hdc)
		{
			IntPtr newObject;
			LOGFONT o = (LOGFONT)lf;
			GDIPlus.GdipCreateFontFromLogfont (hdc, ref o, out newObject);
			return new Font (newObject, "Microsoft Sans Serif", FontStyle.Regular, 10);
		}

		public float GetHeight ()
		{
			return GetHeight (Graphics.systemDpiY);
		}

		public static Font FromLogFont (object lf)
		{
			if ((int) Environment.OSVersion.Platform == 128 || (int) Environment.OSVersion.Platform == 4) {
				return FromLogFont(lf, IntPtr.Zero);
			} else {
				IntPtr	hDC;

				hDC = IntPtr.Zero;

				try {
					hDC = GDIPlus.GetDC(IntPtr.Zero);
					return FromLogFont (lf, hDC);
				}

				finally {
					GDIPlus.ReleaseDC(hDC);
				}
			}

		}

		public void ToLogFont (object logFont)
		{
			Graphics g;

			g = null;

			if ((int) Environment.OSVersion.Platform == 128 || (int) Environment.OSVersion.Platform == 4) {
				// Unix
				Bitmap	img;

				img = null;

				try {
					// We don't have a window we could associate the DC with
					// so we use an image instead
					img = new Bitmap(1, 1, Imaging.PixelFormat.Format32bppArgb);
					g = Graphics.FromImage(img);
					ToLogFont(logFont, g);
				}

				finally {
					if (g != null) {
						g.Dispose();
					}

					if (img != null) {
						img.Dispose();
					}
				}
			} else {
				// Windows
				IntPtr	hDC;

				hDC = IntPtr.Zero;

				try {

					hDC = GDIPlus.GetDC(IntPtr.Zero);
					g = Graphics.FromHdc(hDC);

					ToLogFont (logFont, g);
				}

				finally {
					if (g != null) {
						g.Dispose();
					}

					GDIPlus.ReleaseDC(hDC);
				}
			}
		}

		public void ToLogFont (object logFont, Graphics graphics)
		{
			if (graphics == null) {
				throw new ArgumentNullException ("graphics");
			}

			if (Marshal.SizeOf(logFont) >= Marshal.SizeOf(typeof(LOGFONT))) {
				GDIPlus.CheckStatus (GDIPlus.GdipGetLogFont(NativeObject, graphics.NativeObject, logFont));
			}
		}

		public float GetHeight (Graphics graphics)
		{
			float size;
			
			GDIPlus.GdipGetFontHeight (fontObject, graphics.NativeObject, out size);
			return size;
		}

		public float GetHeight (float dpi)
		{
			float size;
			GDIPlus.GdipGetFontHeightGivenDPI (fontObject, dpi, out size);
			return size;
		}

		public override String ToString ()
		{
			return String.Format ("[Font: Name={0}, Size={1}, Units={2}, GdiCharSet={3}, GdiVerticalFont={4}]", _name, Size, (int)_unit, _gdiCharSet, _gdiVerticalFont);
		}
	}
}
