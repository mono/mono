#if SECURITY_DEP
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
#endif

using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;

#if MONO_SECURITY_ALIAS
using MonoSecurity::Mono.Security.Interface;
#else
using Mono.Security.Interface;
#endif

using MNS = Mono.Net.Security;

namespace Mono.Unity
{
	class UnityTlsStream : MNS.MobileAuthenticatedStream
	{
		public UnityTlsStream (Stream innerStream, bool leaveInnerStreamOpen, SslStream owner,
								MonoTlsSettings settings, MNS.MobileTlsProvider provider)
			: base (innerStream, leaveInnerStreamOpen, owner, settings, provider)
		{
		}

		protected override MNS.MobileTlsContext CreateContext (MNS.MonoSslAuthenticationOptions options)
		{
			return new UnityTlsContext (this, options);
		}
	}
}
#endif