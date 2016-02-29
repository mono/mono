#if MONOTOUCH && FULL_AOT_RUNTIME

// this file is a shim to enable compiling monotouch profiles without mono-extensions
namespace System
{
	partial struct Guid
	{
		public static Guid NewGuid ()
		{
			throw new NotSupportedException ();
		}
	}
}

#endif
