using System.Runtime.Serialization;

namespace System.Drawing {

	[Serializable]
	public sealed class Font : MarshalByRefObject, ISerializable, ICloneable, IDisposable
	{
       	private Font (SerializationInfo info, StreamingContext context)
		{
		}

		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
		}

		public void Dispose ()
		{
		}

		public static Font FromHfont(IntPtr font)
		{
			// FIXME: 
			Font result = new Font("Arial", (float)12.0, FontStyle.Regular);
			return result;
		}

		public IntPtr ToHfont () { 	/*throw new NotImplementedException ();*/ return (IntPtr)100; }

		public Font(Font original, FontStyle style)
		{
			throw new NotImplementedException();
		}

		public Font(FontFamily family, float emSize)
			: this(family, emSize, FontStyle.Regular, GraphicsUnit.Point, (byte)0, false)
		{
		}

		public Font(FontFamily family, float emSize, FontStyle style)
			: this(family, emSize, style, GraphicsUnit.Point, (byte)0, false)
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
			//throw new NotImplementedException ();
		}
		
		object ICloneable.Clone()
		{
			throw new NotImplementedException ();
		}
		
		public bool Bold {
			get {
				throw new NotImplementedException ();
			}
		}
		
		public FontFamily FontFamily {
			get {
				throw new NotImplementedException ();
			}
		}
		
		public byte GdiCharSet {
			get {
				throw new NotImplementedException ();
			}
		}
		
		public bool GdiVerticalFont {
			get {
				throw new NotImplementedException ();
			}
		}
		
		public int Height {
			get {
				throw new NotImplementedException ();
			}
		}

		public bool Italic {
			get {
				throw new NotImplementedException ();
			}
		}

		public string Name {
			get {
				throw new NotImplementedException ();
			}
		}

		public float Size {
			get {
				throw new NotImplementedException ();
			}
		}

		public float SizeInPoints {
			get {
				throw new NotImplementedException ();
			}
		}

		public bool Strikeout {
			get {
				throw new NotImplementedException ();
			}
		}
		
		public FontStyle Style {
			get {
				throw new NotImplementedException ();
			}
		}

		public bool Underline {
			get {
				throw new NotImplementedException ();
			}
		}

		public GraphicsUnit Unit {
			get {
				throw new NotImplementedException ();
			}
		}
	}
}
