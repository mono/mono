namespace System.Drawing {

	public sealed class Font : MarshalByRefObject, IDisposable
	{
		internal IFont implementation_;
		internal static IFontFactory factory_ = Factories.GetFontFactory();

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

	}
}
