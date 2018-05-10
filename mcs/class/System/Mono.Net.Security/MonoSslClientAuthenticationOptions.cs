//
// MonoSslClientAuthenticationOptions.cs
//
// Author:
//       Martin Baulig <mabaul@microsoft.com>
//
// Copyright (c) 2018 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

#if SECURITY_DEP

#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
#endif

#if MONO_SECURITY_ALIAS
using MonoSecurity::Mono.Security.Interface;
#else
using Mono.Security.Interface;
#endif
#endif

using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;

namespace Mono.Net.Security
{
	sealed class MonoSslClientAuthenticationOptions : MonoSslAuthenticationOptions, IMonoSslClientAuthenticationOptions
	{
		public SslClientAuthenticationOptions Options {
			get;
		}

		public override bool ServerMode => false;

		public MonoSslClientAuthenticationOptions (SslClientAuthenticationOptions options)
		{
			Options = options;
		}

		public MonoSslClientAuthenticationOptions ()
		{
			Options = new SslClientAuthenticationOptions ();
		}

		public override bool AllowRenegotiation {
			get => Options.AllowRenegotiation;
			set => Options.AllowRenegotiation = value;
		}

		public override RemoteCertificateValidationCallback RemoteCertificateValidationCallback {
			get => Options.RemoteCertificateValidationCallback;
			set => Options.RemoteCertificateValidationCallback = value;
		}


		public override X509RevocationMode CertificateRevocationCheckMode {
			get => Options.CertificateRevocationCheckMode;
			set => Options.CertificateRevocationCheckMode = value;
		}

		public override EncryptionPolicy EncryptionPolicy {
			get => Options.EncryptionPolicy;
			set => Options.EncryptionPolicy = value;
		}

		public override SslProtocols EnabledSslProtocols {
			get => Options.EnabledSslProtocols;
			set => Options.EnabledSslProtocols = value;
		}

		public LocalCertificateSelectionCallback LocalCertificateSelectionCallback {
			get => Options.LocalCertificateSelectionCallback;
			set => Options.LocalCertificateSelectionCallback = value;
		}

		public override string TargetHost {
			get => Options.TargetHost;
			set => Options.TargetHost = value;
		}

		public override bool ClientCertificateRequired {
			get => throw new NotSupportedException ();
			set => throw new NotSupportedException ();
		}

		public override X509CertificateCollection ClientCertificates {
			get => Options.ClientCertificates;
			set => Options.ClientCertificates = value;
		}

		public override X509Certificate ServerCertificate {
			get => throw new NotSupportedException ();
			set => throw new NotSupportedException ();
		}
	}
}
