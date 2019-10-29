//
// AppleTlsProvider.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2015 Xamarin, Inc.
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
	class AppleTlsProvider : MNS.MobileTlsProvider
	{
		public override string Name {
			get { return "apple-tls"; }
		}

		public override Guid ID {
			get { return MNS.MonoTlsProviderFactory.AppleTlsId; }
		}

		internal override MNS.MobileAuthenticatedStream CreateSslStream (
			SslStream sslStream, Stream innerStream, bool leaveInnerStreamOpen,
			MonoTlsSettings settings)
		{
			return new AppleTlsStream (innerStream, leaveInnerStreamOpen, sslStream, settings, this);
		}

		public override bool SupportsSslStream {
			get { return true; }
		}

		public override bool SupportsMonoExtensions {
			get { return true; }
		}

		public override bool SupportsConnectionInfo {
			get { return true; }
		}

		internal override bool SupportsCleanShutdown {
			get { return false; }
		}

		public override SslProtocols SupportedProtocols {
			get { return SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls; }
		}

		internal override bool ValidateCertificate (
			MNS.ChainValidationHelper validator, string targetHost, bool serverMode,
			X509CertificateCollection certificates, bool wantsChain, ref X509Chain chain,
			ref SslPolicyErrors errors, ref int status11)
		{
			if (wantsChain)
				chain = MNS.SystemCertificateValidator.CreateX509Chain (certificates);
			return AppleCertificateHelper.InvokeSystemCertificateValidator (validator, targetHost, serverMode, certificates, ref errors, ref status11);
		}
	}
}
