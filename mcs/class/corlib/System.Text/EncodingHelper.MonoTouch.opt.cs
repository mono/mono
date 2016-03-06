#if MONOTOUCH

// this file is a shim to enable compiling monotouch profiles without mono-extensions
namespace System.Text
{
	internal static partial class EncodingHelper
	{
		internal static Encoding GetDefaultEncoding ()
		{
			throw new NotSupportedException ();
		}
	}
}

#endif
