
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.ComponentModel;
using awt = java.awt;
using TextAttribute = java.awt.font.TextAttribute;

namespace System.Drawing {
	[Serializable]
	public sealed class Font: MarshalByRefObject, ISerializable, ICloneable, IDisposable {

		#region variables

		private GraphicsUnit _gUnit = GraphicsUnit.Point;
		private FontFamily _fontFamily;
		awt.Font _jFont;

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
		}

		public Font(FontFamily family, float emSize)
			: this(family, emSize, FontStyle.Regular, GraphicsUnit.Point, (byte)0, false) {
		}

		public Font(FontFamily family, float emSize, FontStyle style)
			: this(family, emSize, style, GraphicsUnit.Point, (byte)0, false) {
		}
		public Font(FontFamily family, float emSize, GraphicsUnit unit)
			: this(family, emSize, FontStyle.Regular, unit, (byte)0, false) {
		}

		public Font(FontFamily family, float emSize, FontStyle style, GraphicsUnit unit)
			: this(family, emSize, style, unit, (byte)0, false) {
		}

		public Font(FontFamily family, float emSize, FontStyle style, GraphicsUnit unit, byte charSet)
			: this(family, emSize, style, unit, charSet, false) {
		}
		
		public Font(FontFamily family, float emSize, FontStyle style, GraphicsUnit unit, byte charSet, bool isVertical) {
			//TODO: charset management
			_gUnit = unit;
			_fontFamily = family;

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
			:this(new FontFamily(familyName), emSize, style, unit, charSet, isVertical) {
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
		
#if INTPTR_SUPPORT
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
				return 1; //DEFAULT_CHARSET
			}
		}
		
		public bool GdiVerticalFont {
			get {
				return Name.StartsWith("@");
			}
		}
		
		public int Height {
			get {
				awt.Container c = new awt.Container();
				return c.getFontMetrics(NativeObject).getHeight();
			}
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
				return _jFont.getSize2D() / Graphics.UnitConversion[ (int)_gUnit ];
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

		#endregion
		
		public override System.String ToString() {
			return ("[Font: Name="+ Name +", Size="+ Size +", Style="+ Style +", Units="+ Unit +"]");			
		}

		static internal java.util.Map DeriveStyle(java.util.Map attribs, FontStyle style, bool createNew) {
			java.util.Map newAttribs;
			if (createNew) {
				newAttribs = new java.util.Hashtable( attribs.size() );
				object [] keys = attribs.keySet().toArray();
				for (int i=0; i < keys.Length; i++)
					newAttribs.put( keys[i], attribs.get( keys[i] ) );
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
