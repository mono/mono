
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.ComponentModel;
using awt = java.awt;
using TextAttribute = java.awt.font.TextAttribute;

namespace System.Drawing {

	[Serializable]
	public sealed class Font: MarshalByRefObject, ISerializable, ICloneable, IDisposable
	{
		const int DPI = 72; 
		private GraphicsUnit gUnit = GraphicsUnit.Point;
		awt.Font _jFont;

		internal awt.Font NativeObject {
			get {
				return _jFont;
			}
		}

		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
			info.AddValue("Name", Name);
			info.AddValue("Size", Size);
			info.AddValue("Style", Style, typeof(FontStyle));
			info.AddValue("Unit", Unit, typeof(GraphicsUnit));
		}

		public void Dispose()
		{
		}

       	private Font (SerializationInfo info, StreamingContext context)
			: this(
			info.GetString("Name"), 
			info.GetSingle("Size"), 
			(FontStyle)info.GetValue("Style", typeof(FontStyle)), 
			(GraphicsUnit)info.GetValue("Unit", typeof(GraphicsUnit)) )
		{
			
		}
		

		// FIXME: add this method when/if there will be resources needed to be disposed
//		~Font()
//		{	
//			
//		}

		internal float unitConversion(GraphicsUnit fromUnit, GraphicsUnit toUnit, float nSrc)
		{
			double inchs = 0;
			double nTrg = 0;		
			
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
				inchs = nSrc / Graphics.DefaultScreenResolution;
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
				nTrg = inchs * Graphics.DefaultScreenResolution;
				break;
			case GraphicsUnit.Point:
				nTrg = inchs * 72;
				break;
			default:	
				throw new ArgumentException("Invalid GraphicsUnit");				
			}
			return (float)nTrg;	
		}

#if INTPTR_SUPPORT
		public IntPtr ToHfont ()
		{
			throw new NotImplementedException();
		}
#endif

		public Font(Font original, FontStyle style)
		{
			_jFont = original.NativeObject.deriveFont((int)style);
		}

		public Font(FontFamily family, float emSize)
			: this(family, emSize, FontStyle.Regular, GraphicsUnit.Point, (byte)0, false)
		{
		}

		public Font(FontFamily family, float emSize, FontStyle style)
			: this(family, emSize, style, GraphicsUnit.Point, (byte)0, false)
		{
		}
		public Font(FontFamily family, float emSize, GraphicsUnit unit)
			: this(family, emSize, FontStyle.Regular, unit, (byte)0, false)
		{
		}

		public Font(FontFamily family, float emSize, FontStyle style, GraphicsUnit unit)
			: this(family, emSize, style, unit, (byte)0, false)
		{
		}

		public Font(FontFamily family, float emSize, FontStyle style, GraphicsUnit unit, byte charSet)
			: this(family, emSize, style, unit, charSet, false)
		{
		}
		
		public Font(FontFamily family, float emSize, FontStyle style, GraphicsUnit unit, byte charSet, bool isVertical)
			:this(family.Name,emSize,style,unit,charSet,isVertical)
		{
		}

		public Font(string familyName, float emSize)
			: this(familyName, emSize, FontStyle.Regular, GraphicsUnit.Point, (byte)0, false)
		{
		}

		public Font(string familyName, float emSize, FontStyle style)
			: this(familyName, emSize, style, GraphicsUnit.Point, (byte)0, false)
		{
		}

		public Font(string familyName, float emSize, GraphicsUnit unit)
			: this(familyName, emSize, FontStyle.Regular, unit, (byte)0, false)
		{
		}
		
		public Font(string familyName, float emSize, FontStyle style, GraphicsUnit unit)
			: this(familyName, emSize, style, unit, (byte)0, false)
		{
		}
		
		public Font(string familyName, float emSize, FontStyle style, GraphicsUnit unit, byte charSet)
			: this(familyName, emSize, style, unit, charSet, false)
		{
		}
		
		public Font(string familyName, float emSize, FontStyle style, GraphicsUnit unit, byte charSet, bool isVertical)			 
		{
			//TODO: charset management
			gUnit = unit;
			java.util.Hashtable attribs = new java.util.Hashtable();
			attribs.put(TextAttribute.FAMILY, familyName/*TODO: family doungrade possibility*/);
			//init defaults
			attribs.put(TextAttribute.POSTURE, TextAttribute.POSTURE_REGULAR);

			if((style & FontStyle.Bold) != FontStyle.Regular)
				attribs.put(TextAttribute.WEIGHT, TextAttribute.WEIGHT_BOLD);
			if((style & FontStyle.Italic) != FontStyle.Regular)
				attribs.put(TextAttribute.POSTURE, TextAttribute.POSTURE_OBLIQUE);
			if((style & FontStyle.Underline) != FontStyle.Regular)
				attribs.put(TextAttribute.UNDERLINE, TextAttribute.UNDERLINE_ON);
			if((style & FontStyle.Strikeout) != FontStyle.Regular)
				attribs.put(TextAttribute.STRIKETHROUGH, TextAttribute.STRIKETHROUGH_ON);

			float newSize = unitConversion(gUnit,GraphicsUnit.World,emSize);
			attribs.put(TextAttribute.SIZE,new java.lang.Float(newSize));

			#region OldStyleSwitch
			//			switch(style)
			//			{
			//				case 0: // '\0'
			//					attribs.put(TextAttribute.POSTURE, TextAttribute.POSTURE_REGULAR);
			//					break;
			//
			//				case 1: // '\001'
			//					attribs.put(TextAttribute.WEIGattribs, TextAttribute.WEIGattribs_BOLD);
			//					break;
			//
			//				case 2: // '\002'
			//					attribs.put(TextAttribute.POSTURE, TextAttribute.POSTURE_OBLIQUE);
			//					break;
			//
			//				case 3: // '\003'
			//					attribs.put(TextAttribute.WEIGattribs, TextAttribute.WEIGattribs_BOLD);
			//					attribs.put(TextAttribute.POSTURE, TextAttribute.POSTURE_OBLIQUE);
			//					break;
			//
			//				case 4: // '\004'
			//					attribs.put(TextAttribute.UNDERLINE, TextAttribute.UNDERLINE_ON);
			//					break;
			//
			//				case 5: // '\005'
			//					attribs.put(TextAttribute.WEIGattribs, TextAttribute.WEIGattribs_BOLD);
			//					attribs.put(TextAttribute.UNDERLINE, TextAttribute.UNDERLINE_ON);
			//					break;
			//
			//				case 6: // '\006'
			//					attribs.put(TextAttribute.POSTURE, TextAttribute.POSTURE_OBLIQUE);
			//					attribs.put(TextAttribute.UNDERLINE, TextAttribute.UNDERLINE_ON);
			//					break;
			//
			//				case 7: // '\007'
			//					attribs.put(TextAttribute.WEIGattribs, TextAttribute.WEIGattribs_BOLD);
			//					attribs.put(TextAttribute.POSTURE, TextAttribute.POSTURE_OBLIQUE);
			//					attribs.put(TextAttribute.UNDERLINE, TextAttribute.UNDERLINE_ON);
			//					break;
			//
			//				case 8: // '\b'
			//					attribs.put(TextAttribute.STRIKETHROUGH, TextAttribute.STRIKETHROUGH_ON);
			//					break;
			//
			//				case 9: // '\t'
			//					attribs.put(TextAttribute.WEIGattribs, TextAttribute.WEIGattribs_BOLD);
			//					attribs.put(TextAttribute.STRIKETHROUGH, TextAttribute.STRIKETHROUGH_ON);
			//					break;
			//
			//				case 10: // '\n'
			//					attribs.put(TextAttribute.POSTURE, TextAttribute.POSTURE_OBLIQUE);
			//					attribs.put(TextAttribute.STRIKETHROUGH, TextAttribute.STRIKETHROUGH_ON);
			//					break;
			//
			//				case 11: // '\013'
			//					attribs.put(TextAttribute.WEIGattribs, TextAttribute.WEIGattribs_BOLD);
			//					attribs.put(TextAttribute.POSTURE, TextAttribute.POSTURE_OBLIQUE);
			//					attribs.put(TextAttribute.STRIKETHROUGH, TextAttribute.STRIKETHROUGH_ON);
			//					break;
			//
			//				case 12: // '\f'
			//					attribs.put(TextAttribute.UNDERLINE, TextAttribute.UNDERLINE_ON);
			//					attribs.put(TextAttribute.STRIKETHROUGH, TextAttribute.STRIKETHROUGH_ON);
			//					break;
			//
			//				case 13: // '\r'
			//					attribs.put(TextAttribute.WEIGattribs, TextAttribute.WEIGattribs_BOLD);
			//					attribs.put(TextAttribute.UNDERLINE, TextAttribute.UNDERLINE_ON);
			//					attribs.put(TextAttribute.STRIKETHROUGH, TextAttribute.STRIKETHROUGH_ON);
			//					break;
			//
			//				case 14: // '\016'
			//					attribs.put(TextAttribute.POSTURE, TextAttribute.POSTURE_OBLIQUE);
			//					attribs.put(TextAttribute.UNDERLINE, TextAttribute.UNDERLINE_ON);
			//					attribs.put(TextAttribute.STRIKETHROUGH, TextAttribute.STRIKETHROUGH_ON);
			//					break;
			//
			//				case 15: // '\017'
			//					attribs.put(TextAttribute.WEIGattribs, TextAttribute.WEIGattribs_BOLD);
			//					attribs.put(TextAttribute.POSTURE, TextAttribute.POSTURE_OBLIQUE);
			//					attribs.put(TextAttribute.UNDERLINE, TextAttribute.UNDERLINE_ON);
			//					attribs.put(TextAttribute.STRIKETHROUGH, TextAttribute.STRIKETHROUGH_ON);
			//					break;
			//
			//				default:
			//					attribs.put(TextAttribute.POSTURE, TextAttribute.POSTURE_REGULAR);
			//					break;
			//			}
			#endregion
			//TODO: units conversion
			try
			{
				_jFont = new awt.Font(attribs);
			}
			catch (Exception e)
			{
#if DEBUG
				string mess = e.ToString();
				Console.WriteLine(mess);
#endif
				throw e;
			}
		}		
		
		public object Clone()
		{
			return new Font(this, Style);
		}
		
		
		
		public bool Bold {
			get {
				return _jFont.isBold();
			}
		}
		
		
		public FontFamily FontFamily {
			get {				
				return new FontFamily(_jFont.getFamily());
			}
		}
		
		public byte GdiCharSet {
			get {
				return 1; //DEFAULT_CHARSET
			}
		}
		
		public bool GdiVerticalFont {
			get {
				return false; //Name.StartsWith("@");
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
				return unitConversion(GraphicsUnit.World,gUnit,_jFont.getSize2D());
			}
		}

		
		public float SizeInPoints {
			get {
				return unitConversion(GraphicsUnit.World,GraphicsUnit.Point,_jFont.getSize2D());
			}
		}

		
		public bool Strikeout {
			get {
				try
				{
					if((java.lang.Boolean)_jFont.getAttributes().get(TextAttribute.STRIKETHROUGH) 
						== TextAttribute.STRIKETHROUGH_ON )
						return true;
				}
				catch
				{
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
				try
				{
					if((java.lang.Integer)_jFont.getAttributes().get(TextAttribute.UNDERLINE) 
						== TextAttribute.UNDERLINE_ON )
						return true;
				}
				catch
				{
				}
				return false;
			}
		}

		[TypeConverter(typeof(FontConverter.FontUnitConverter))]
		public GraphicsUnit Unit {
			get {
				return gUnit;
			}
		}
		
		public override System.String ToString()
		{
			return ("[Font: Name="+ Name +", Size="+ Size+", Style="+ Style  +", Units="+ Unit + "]");			
		}
	}
}
