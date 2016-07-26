namespace Mono.Security.Interface
{
	delegate MonoTlsProvider MonoTlsProviderFactoryDelegate ();

	static partial class MonoTlsProviderFactory
	{
		internal static MonoTlsProviderFactoryDelegate _PrivateFactoryDelegate;
	}
}
