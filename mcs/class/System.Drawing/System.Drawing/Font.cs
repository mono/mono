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

       		private Font (SerializationInfo info, StreamingContext context)
		{
		}

		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
		}

		~Font()
		{
			Dispose ();
		}

		public void Dispose ()
		{
			if (fontObject != IntPtr.Zero) {
				GDIPlus.CheckStatus (GDIPlus.GdipDeleteFont (fontObject));
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
			IntPtr			oldFont;
			FontStyle		newStyle = FontStyle.Regular;
			float			newSize;
			LOGFONTA		lf = new LOGFONTA ();

			// Sanity. Should we throw an exception?
			if (Hfont == IntPtr.Zero) {
				Font result = new Font ("Arial", (float)10.0, FontStyle.Regular);
				return(result);
			}

			if ((int) osInfo.Platform == 128) {
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

			if ((int) osInfo.Platform == 128) {
				// If we're on Unix we use our private gdiplus API
				GDIPlus.CheckStatus (GDIPlus.GdipGetHfont (fontObject, out Hfont));
			} else {
				LOGFONTA lf = new LOGFONTA ();
				ToLogFont(lf);
				Hfont = GDIPlus.CreateFontIndirectA (ref lf);
			}
			return Hfont;
		}

		internal Font (IntPtr newFontObject, string familyName, FontStyle style, float size)
		{
			FontFamily fontFamily = new FontFamily (familyName);
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
			// NOTE: If family name is null, empty or invalid,
			// MS creates Microsoft Sans Serif font.
			Status status;
			FontFamily family = new FontFamily (familyName);
			setProperties (family, emSize, style, unit, charSet, isVertical);				

			status = GDIPlus.GdipCreateFont (family.NativeObject, emSize,  style, unit, out fontObject);
			GDIPlus.CheckStatus (status);			
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

		private float _size;
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
			
			if (fnt.FontFamily == FontFamily && fnt.Size == Size &&
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
			LOGFONTA o = (LOGFONTA)lf;
			int l = GDIPlus.GdipCreateFontFromLogfontA (hdc, ref o, out newObject);
			return new Font (newObject, "Microsoft Sans Serif", FontStyle.Regular, 10);
		}

		public float GetHeight ()
		{
			return GetHeight (Graphics.systemDpiY);
		}

		[MonoTODO]
		public static Font FromLogFont (object lf)
		{
			throw new NotImplementedException ();
		}

		public void ToLogFont (object logFont)
		{
			using (Graphics g = Graphics.FromHdc (GDIPlus.GetDC (IntPtr.Zero))) {
				ToLogFont (logFont, g);
			}
		}

		public void ToLogFont (object logFont, Graphics graphics)
		{
			if (graphics == null) {
				throw new ArgumentNullException ("graphics");
			}

			// TODO: Does it make a sense to deal with LOGFONTW ?
			LOGFONTA o = (LOGFONTA)logFont;
			GDIPlus.CheckStatus (GDIPlus.GdipGetLogFontA(NativeObject, graphics.NativeObject, ref o));
		}

		public float GetHeight (Graphics graphics)
		{
			float height = GetHeight (graphics.DpiY);

			switch (graphics.PageUnit) {
				case GraphicsUnit.Document:
					height *= (300f / graphics.DpiY);
					break;
				case GraphicsUnit.Display:
					height *= (75f / graphics.DpiY);
					break;
				case GraphicsUnit.Inch:
					height /=  graphics.DpiY;
					break;
				case GraphicsUnit.Millimeter:
					height *= (25.4f / graphics.DpiY);
					break;
				case GraphicsUnit.Point:
					height *= (72f / graphics.DpiY);
					break;

				case GraphicsUnit.Pixel:
				case GraphicsUnit.World:
				default:
					break;
			}

			return height;
		}

		public float GetHeight (float dpi)
		{
			float height;
			int emHeight = _fontFamily.GetEmHeight (_style);
			int lineSpacing = _fontFamily.GetLineSpacing (_style);

			height = lineSpacing * (_size / emHeight);

			switch (_unit) {
				case GraphicsUnit.Document:
					height *= (dpi / 300f);
					break;
				case GraphicsUnit.Display:
					height *= (dpi / 75f);
					break;
				case GraphicsUnit.Inch:
					height *= dpi;
					break;
				case GraphicsUnit.Millimeter:
					height *= (dpi / 25.4f);
					break;
				case GraphicsUnit.Point:
					height *= (dpi / 72f);
					break;

				case GraphicsUnit.Pixel:
				case GraphicsUnit.World:
				default:
					break;
			}

			return height;
		}

		public override String ToString ()
		{
			return String.Format ("[Font: Name={0}, Size={1}, Style={2}, Units={3}, GdiCharSet={4}, GdiVerticalFont={5}]", _name, _size, _style, _unit, _gdiCharSet, _gdiVerticalFont);
		}
	}
}
