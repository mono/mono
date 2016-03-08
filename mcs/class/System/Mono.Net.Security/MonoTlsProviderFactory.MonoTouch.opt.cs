#if MONOTOUCH || XAMMAC

// this file is a shim to enable compiling monotouch profiles without mono-extensions
namespace Mono.Net.Security
{
	static partial class MonoTlsProviderFactory
	{
		static IMonoTlsProvider CreateDefaultProvider ()
		{
			throw new System.NotSupportedException ();
		}
	}
}

#endif
