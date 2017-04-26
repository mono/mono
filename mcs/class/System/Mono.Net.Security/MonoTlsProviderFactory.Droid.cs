// Copyright 2015 Xamarin Inc. All rights reserved.
#if SECURITY_DEP

#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
using MSI = MonoSecurity::Mono.Security.Interface;
#else
using MSI = Mono.Security.Interface;
#endif

#if MONO_FEATURE_BTLS
using Mono.Btls;
#endif

using System;

namespace Mono.Net.Security
{
	static partial class MonoTlsProviderFactory
	{
		static MSI.MonoTlsProvider CreateDefaultProviderImpl ()
		{
			MSI.MonoTlsProvider provider = null;
			var type = Environment.GetEnvironmentVariable ("XA_TLS_PROVIDER");
			switch (type) {
			case null:
			case "default":
			case "legacy":
				return new LegacyTlsProvider ();
#if MONO_FEATURE_BTLS
			case "btls":
				if (!IsBtlsSupported ())
					throw new NotSupportedException ("BTLS in not supported!");
				return new MonoBtlsProvider ();
#endif
			default:
				throw new NotSupportedException (string.Format ("Invalid TLS Provider: `{0}'.", provider));
			}
		}
	}
}
#endif
