// Copyright 2015 Xamarin Inc. All rights reserved.
#if SECURITY_DEP

#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
using MSI = MonoSecurity::Mono.Security.Interface;
#else
using MSI = Mono.Security.Interface;
#endif

namespace Mono.Net.Security
{
	static partial class MonoTlsProviderFactory
	{
		static MSI.MonoTlsProvider CreateDefaultProviderImpl ()
		{
			MSI.MonoTlsProvider provider = null;
			if (MSI.MonoTlsProviderFactory._PrivateFactoryDelegate != null)
				provider = MSI.MonoTlsProviderFactory._PrivateFactoryDelegate ();
			return provider;
		}
	}
}
#endif
