//
// AppleTlsStream.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2016 Xamarin, Inc.
//

#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
#endif

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

using MNS = Mono.Net.Security;
#if MONO_SECURITY_ALIAS
using MonoSecurity::Mono.Security.Interface;
#else
using Mono.Security.Interface;
#endif

namespace Mono.AppleTls
{
	class AppleTlsStream : MNS.MobileAuthenticatedStream
	{
		public AppleTlsStream (Stream innerStream, bool leaveInnerStreamOpen, SslStream owner,
		                       MonoTlsSettings settings, MNS.MobileTlsProvider provider)
			: base (innerStream, leaveInnerStreamOpen, owner, settings, provider)
		{
		}

		protected override MNS.MobileTlsContext CreateContext (MNS.MonoSslAuthenticationOptions options)
		{
			return new AppleTlsContext (this, options);
		}
	}
}
