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
								MonoTlsSettings settings, MonoTlsProvider provider)
			: base (innerStream, leaveInnerStreamOpen, owner, settings, provider)
		{
		}

		protected override MNS.MobileTlsContext CreateContext (
			bool serverMode, string targetHost, SslProtocols enabledProtocols,
			X509Certificate serverCertificate, X509CertificateCollection clientCertificates,
			bool askForClientCert)
		{
			return new UnityTlsContext (
				this, serverMode, targetHost,
				enabledProtocols, serverCertificate,
				clientCertificates, askForClientCert);
		}
	}
}
#endif