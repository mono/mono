
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.ComponentModel;
using awt = java.awt;
using TextAttribute = java.awt.font.TextAttribute;

namespace System.Drawing {
	[Serializable]
	public sealed class Font: MarshalByRefObject, ISerializable, ICloneable, IDisposable {

		#region variables

		const byte DEFAULT_CHARSET = 1;

		private readonly GraphicsUnit _gUnit = GraphicsUnit.Point;
		private readonly FontFamily _fontFamily;
		private readonly awt.Font _jFont;
		private readonly byte _charset;

		static readonly float [] _screenResolutionConverter = {
													   1,								// World
													   1,								// Display
													   1,								// Pixel
													   Graphics.DefaultScreenResolution,	// Point
													   Graphics.DefaultScreenResolution,	// Inch
													   Graphics.DefaultScreenResolution,	// Document
													   Graphics.DefaultScreenResolution		// Millimeter
												   };

#if NET_2_0
		private readonly string _systemFontName;
#endif

		#endregion

		internal awt.Font NativeObject {
			get {
				return _jFont;
			}
		}

		#region ISerializable

		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
			info.AddValue("Name", Name);
			info.AddValue("Size", Size);
			info.AddValue("Style", Style, typeof(FontStyle));
			info.AddValue("Unit", Unit, typeof(GraphicsUnit));
		}

		#endregion

		#region ctors

		private Font (SerializationInfo info, StreamingContext context)
			: this(
			info.GetString("Name"), 
			info.GetSingle("Size"), 
			(FontStyle)info.GetValue("Style", typeof(FontStyle)), 
			(GraphicsUnit)info.GetValue("Unit", typeof(GraphicsUnit)) ) {
		}

		public Font(Font original, FontStyle style) {
			_jFont = original.NativeObject.deriveFont( DeriveStyle(original.NativeObject.getAttributes(), style, true) );
			_gUnit = original._gUnit;
			_fontFamily = original._fontFamily;
			_charset = original._charset;
		}

		public Font(FontFamily family, float emSize)
			: this(family, emSize, FontStyle.Regular, GraphicsUnit.Point, DEFAULT_CHARSET, false) {
		}

		public Font(FontFamily family, float emSize, FontStyle style)
			: this(family, emSize, style, GraphicsUnit.Point, DEFAULT_CHARSET, false) {
		}
		public Font(FontFamily family, float emSize, GraphicsUnit unit)
			: this(family, emSize, FontStyle.Regular, unit, DEFAULT_CHARSET, false) {
		}

		public Font(FontFamily family, float emSize, FontStyle style, GraphicsUnit unit)
			: this(family, emSize, style, unit, DEFAULT_CHARSET, false) {
		}

		public Font(FontFamily family, float emSize, FontStyle style, GraphicsUnit unit, byte charSet)
			: this(family, emSize, style, unit, charSet, false) {
		}
		
		public Font(FontFamily family, float emSize, FontStyle style, GraphicsUnit unit, byte charSet, bool isVertical) {
			if (family == null)
				throw new ArgumentNullException("family");

			_gUnit = unit;
			_fontFamily = family;
			_charset = charSet;

			java.util.Hashtable attribs = new java.util.Hashtable();
			attribs.put(TextAttribute.FAMILY, family.Name/*TODO: family doungrade possibility*/);
			//init defaults
			attribs.put(TextAttribute.POSTURE, TextAttribute.POSTURE_REGULAR);

			float newSize = emSize * Graphics.UnitConversion[ (int)_gUnit ];
			attribs.put(TextAttribute.SIZE, new java.lang.Float(newSize));

			DeriveStyle(attribs, style, false);

			_jFont = family.FamilyFont.deriveFont(attribs);
		}

		public Font(string familyName, float emSize)
			: this(familyName, emSize, FontStyle.Regular, GraphicsUnit.Point, (byte)0, false) {
		}

		public Font(string familyName, float emSize, FontStyle style)
			: this(familyName, emSize, style, GraphicsUnit.Point, (byte)0, false) {
		}

		public Font(string familyName, float emSize, GraphicsUnit unit)
			: this(familyName, emSize, FontStyle.Regular, unit, (byte)0, false) {
		}
		
		public Font(string familyName, float emSize, FontStyle style, GraphicsUnit unit)
			: this(familyName, emSize, style, unit, (byte)0, false) {
		}
		
		public Font(string familyName, float emSize, FontStyle style, GraphicsUnit unit, byte charSet)
			: this(familyName, emSize, style, unit, charSet, false) {
		}
		
		public Font(string familyName, float emSize, FontStyle style, GraphicsUnit unit, byte charSet, bool isVertical)
			: this (GetFontFamily (familyName), emSize, style, unit, charSet, isVertical) {
		}

#if NET_2_0
		internal Font (string familyName, float emSize, string systemName)
			: this (familyName, emSize) {
			_systemFontName = systemName;
		}
#endif

		static FontFamily GetFontFamily (string familyName) {
#if ONLY_1_1
			if (familyName == null)
				throw new ArgumentNullException ("familyName");
#endif
			// NOTE: If family name is null, empty or invalid,
			// MS creates Microsoft Sans Serif font.
			try {
				return new FontFamily (familyName);
			}
			catch {
				return FontFamily.GenericSansSerif;
			}
		}

		#endregion
		
		#region IDisposable members

		public void Dispose() {
		}

		#endregion

		#region ICloneable

		public object Clone() {
			return (Font)MemberwiseClone();
		}

		#endregion

		public override bool Equals (object obj)
		{
			Font other = obj as Font;
			if (other == null) {
				return false;
			}

			return NativeObject.Equals (other.NativeObject);
		}

		public override int GetHashCode ()
		{
			return NativeObject.GetHashCode ();
		}

#if INTPTR_SUPPORT
		[MonoTODO]
		public IntPtr ToHfont ()
		{
			throw new NotImplementedException();
		}
#endif
		
		#region public properties

		public bool Bold {
			get {
				return _jFont.isBold();
			}
		}
		
		public FontFamily FontFamily {
			get {				
				return _fontFamily;
			}
		}
		
		public byte GdiCharSet {
			get {
				return _charset;
			}
		}
		
		public bool GdiVerticalFont {
			get {
				return Name.StartsWith("@");
			}
		}
		
		public int Height {
			get {
				return FontFamily.Container.getFontMetrics(NativeObject).getHeight();
			}
		}

		public float GetHeight () {
			return GetHeight (Graphics.DefaultScreenResolution);
		}

		public float GetHeight (float dpi) {
			return (FontFamily.GetLineSpacing (Style) / FontFamily.GetEmHeight (Style))
				* (SizeInPoints / _screenResolutionConverter [(int) Unit])
				* dpi;
		}

		public float GetHeight (Graphics graphics) {
			if (graphics == null)
				throw new ArgumentNullException ("graphics");
			return GetHeight (graphics.DpiY);
		}

		public bool Italic {
			get {
				return _jFont.isItalic();
			}
		}

		public string Name {
			get {
				return _jFont.getName();
			}
		}

		public float Size {
			get {
				return SizeInPoints / Graphics.UnitConversion[ (int)_gUnit ];
			}
		}
		
		public float SizeInPoints {
			get {
				return _jFont.getSize2D();
			}
		}
		
		public bool Strikeout {
			get {
				try {
					if((java.lang.Boolean)_jFont.getAttributes().get(TextAttribute.STRIKETHROUGH) 
						== TextAttribute.STRIKETHROUGH_ON )
						return true;
				}
				catch {
				}
				return false;
			}
		}
		
		public FontStyle Style {
			get {
				FontStyle style = FontStyle.Regular;
				if (Bold)
					style |= FontStyle.Bold;
				if (Italic)
					style |= FontStyle.Italic;
				if (Underline)
					style |= FontStyle.Underline;
				if (Strikeout)
					style |= FontStyle.Strikeout;

				return style;
			}
		}
		
		public bool Underline {
			get {
				try {
					if((java.lang.Integer)_jFont.getAttributes().get(TextAttribute.UNDERLINE) 
						== TextAttribute.UNDERLINE_ON )
						return true;
				}
				catch {
				}
				return false;
			}
		}

		[TypeConverter(typeof(FontConverter.FontUnitConverter))]
		public GraphicsUnit Unit {
			get {
				return _gUnit;
			}
		}

#if NET_2_0
		[Browsable (false)]
		public bool IsSystemFont {
			get {
				return !string.IsNullOrEmpty (_systemFontName);
			}
		}

		[Browsable (false)]
		public string SystemFontName {
			get {
				return _systemFontName;
			}
		}
#endif

		#endregion
		
		public override System.String ToString() {
			return ("[Font: Name="+ Name +", Size="+ Size +", Style="+ Style +", Units="+ Unit +"]");			
		}

		static internal java.util.Map DeriveStyle(java.util.Map attribs, FontStyle style, bool createNew) {
			java.util.Map newAttribs;
			if (createNew) {
				newAttribs = new java.util.Hashtable( attribs.size() );
				java.util.Iterator it = attribs.keySet().iterator();
				while (it.hasNext ()) {
					object key = it.next ();
					object value = attribs.get (key);
					if (value != null)
						newAttribs.put (key, value);
				}
			}
			else
				newAttribs = attribs;

			//Bold
			if((style & FontStyle.Bold) == FontStyle.Bold)
				newAttribs.put(TextAttribute.WEIGHT, TextAttribute.WEIGHT_BOLD);
			else
				newAttribs.remove(TextAttribute.WEIGHT);

			//Italic
			if((style & FontStyle.Italic) == FontStyle.Italic)
				newAttribs.put(TextAttribute.POSTURE, TextAttribute.POSTURE_OBLIQUE);
			else
				newAttribs.put(TextAttribute.POSTURE, TextAttribute.POSTURE_REGULAR);

			//Underline
			if((style & FontStyle.Underline) == FontStyle.Underline)
				newAttribs.put(TextAttribute.UNDERLINE, TextAttribute.UNDERLINE_ON);
			else
				newAttribs.remove(TextAttribute.UNDERLINE);

			//Strikeout
			if((style & FontStyle.Strikeout) == FontStyle.Strikeout)
				newAttribs.put(TextAttribute.STRIKETHROUGH, TextAttribute.STRIKETHROUGH_ON);
			else
				newAttribs.remove(TextAttribute.STRIKETHROUGH);

			return newAttribs;
		}
	}
}
