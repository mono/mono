// Copyright 2015 Xamarin Inc. All rights reserved.

using System;
using MSI = Mono.Security.Interface;

namespace Mono.Net.Security
{
	static partial class MonoTlsProviderFactory
	{
		static IMonoTlsProvider CreateDefaultProvider ()
		{
			#if SECURITY_DEP
			MSI.MonoTlsProvider provider = null;
			if (MSI.MonoTlsProviderFactory._PrivateFactoryDelegate != null)
				provider = MSI.MonoTlsProviderFactory._PrivateFactoryDelegate ();
			if (provider != null)
				return new Private.MonoTlsProviderWrapper (provider);
			#endif
			return null;
		}
	}
}
