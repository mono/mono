
namespace System.Drawing {

	public sealed class Font : MarshalByRefObject, IDisposable
	{
		public void Dispose ()
		{
		}

		public static Font FromHfont(IntPtr font)
		{
			return null;
		}	

		public IntPtr ToHfont () { return (IntPtr) 0; }
		
		public Font(FontFamily family, float size, FontStyle style)
		{
		}

		public Font(string familyName, float size, FontStyle style)
		{
		}
	}
}
