#if SECURITY_DEP
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
#endif

using System.IO;
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
		public UnityTlsStream (Stream innerStream, bool leaveInnerStreamOpen,
			MonoTlsSettings settings, MonoTlsProvider provider)
			: base (innerStream, leaveInnerStreamOpen, settings, provider)
		{
		}

		protected override MNS.MobileTlsContext CreateContext (
			MNS.MobileAuthenticatedStream parent, bool serverMode, string targetHost,
			SslProtocols enabledProtocols, X509Certificate serverCertificate,
			X509CertificateCollection clientCertificates, bool askForClientCert)
		{
			return new UnityTlsContext (parent, serverMode, targetHost,
				enabledProtocols, serverCertificate, clientCertificates,
				askForClientCert);
		}
	}
}
#endif