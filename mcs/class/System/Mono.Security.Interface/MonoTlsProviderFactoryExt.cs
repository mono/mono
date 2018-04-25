namespace Mono.Security.Interface
{
	public delegate MonoTlsProvider MonoTlsProviderFactoryDelegate ();

	static partial class MonoTlsProviderFactory
	{
		public static MonoTlsProviderFactoryDelegate _PrivateFactoryDelegate;
	}
}
