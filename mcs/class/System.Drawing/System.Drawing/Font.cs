
namespace System.Drawing {

	public sealed class Font : MarshalByRefObject, IDisposable
	{
		public void Dispose ()
		{
		}

		public static Font FromHfont(IntPtr font)
		{
			return new Font("Arial", (float)12.0, FontStyle.Regular);
		}

		public IntPtr ToHfont () { return IntPtr.Zero; }

		public Font(FontFamily family, float size, FontStyle style)
		{
		}

		public Font(string familyName, float size, FontStyle style)
		{
		}

	}
}
