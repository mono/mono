#if SECURITY_DEP && MONO_FEATURE_APPLETLS
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
	class AppleTlsProvider : MonoTlsProvider
	{
		static readonly Guid id = new Guid ("981af8af-a3a3-419a-9f01-a518e3a17c1c");

		public override string Name {
			get { return "apple-tls"; }
		}

		public override Guid ID {
			get { return id; }
		}

		public override IMonoSslStream CreateSslStream (
			Stream innerStream, bool leaveInnerStreamOpen,
			MonoTlsSettings settings = null)
		{
			return new AppleTlsStream (innerStream, leaveInnerStreamOpen, settings, this);
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

		public override SslProtocols SupportedProtocols {
			get { return SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls; }
		}

		internal override bool ValidateCertificate (
			ICertificateValidator2 validator, string targetHost, bool serverMode,
			X509CertificateCollection certificates, bool wantsChain, ref X509Chain chain,
			ref MonoSslPolicyErrors errors, ref int status11)
		{
			if (wantsChain)
				chain = MNS.SystemCertificateValidator.CreateX509Chain (certificates);
			return AppleCertificateHelper.InvokeSystemCertificateValidator (validator, targetHost, serverMode, certificates, ref errors, ref status11);
		}
	}
}
#endif
