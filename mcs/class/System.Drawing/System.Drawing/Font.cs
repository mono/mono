using System.Runtime.Serialization;

namespace System.Drawing {

	[Serializable]
        public sealed class Font : MarshalByRefObject, ISerializable, ICloneable, IDisposable
	{
		internal IFont implementation_;
		internal static IFontFactory factory_ = Factories.GetFontFactory();

        	private Font (SerializationInfo info, StreamingContext context)
		{
		}

		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
		}

		public void Dispose ()
		{
			implementation_.Dispose();
		}

		public static Font FromHfont(IntPtr font)
		{
			// FIXME: 
			Font result = new Font("Arial", (float)12.0, FontStyle.Regular);
			result.implementation_ = factory_.FromHfont(font);
			return result;
		}

		public IntPtr ToHfont () { return implementation_.ToHfont(); }

		public Font(FontFamily family, float size, FontStyle style)
		{
			implementation_ = factory_.Font(family, size, style);
		}

		public Font(string familyName, float size, FontStyle style)
		{
			implementation_ = factory_.Font(familyName, size, style);
		}

		object ICloneable.Clone()
		{
			throw new NotImplementedException ();
		}
	}
}
