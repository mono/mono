// Copyright 2015 Xamarin Inc. All rights reserved.
#if SECURITY_DEP
using System;
using MSI = Mono.Security.Interface;

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
