#if SECURITY_DEP
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
#endif

#if MONO_SECURITY_ALIAS
using MonoSecurity::Mono.Security.Interface;
#else
using Mono.Security.Interface;
#endif

using System;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Mono.Net.Security
{
	abstract class MobileTlsProvider : MonoTlsProvider
	{
		public sealed override IMonoSslStream CreateSslStream (
			Stream innerStream, bool leaveInnerStreamOpen,
			MonoTlsSettings settings = null)
		{
			return SslStream.CreateMonoSslStream (innerStream, leaveInnerStreamOpen, this, settings);
		}

		internal abstract MobileAuthenticatedStream CreateSslStream (
			SslStream sslStream, Stream innerStream, bool leaveInnerStreamOpen,
			MonoTlsSettings settings);

		/*
		 * If @serverMode is true, then we're a server and want to validate a certificate
		 * that we received from a client.
		 *
		 * On OS X and Mobile, the @chain will be initialized with the @certificates, but not actually built.
		 *
		 * Returns `true` if certificate validation has been performed and `false` to invoke the
		 * default system validator.
		 */
		internal abstract bool ValidateCertificate (
			ChainValidationHelper validator, string targetHost, bool serverMode,
			X509CertificateCollection certificates, bool wantsChain, ref X509Chain chain,
			ref SslPolicyErrors errors, ref int status11);
	}
}

#endif
