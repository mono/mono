//
// MonoTlsProvider.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2015 Xamarin, Inc.
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

using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Mono.Security.Protocol.Tls;

namespace Mono.Security.Interface
{
	/*
	 * Unfortunately, we can't use the public definitions from System.dll here, so we need to
	 * copy these.
	 *
	 * The @MonoRemoteCertificateValidationCallback also has an additional 'targetHost' argument.
	 *
	 */

	[Flags]
	public enum MonoSslPolicyErrors
	{
		None = 0,
		RemoteCertificateNotAvailable = 1,
		RemoteCertificateNameMismatch = 2,
		RemoteCertificateChainErrors = 4,
	}

	public enum MonoEncryptionPolicy
	{
		// Prohibit null ciphers (current system defaults)
		RequireEncryption = 0,

		// Add null ciphers to current system defaults
		AllowNoEncryption,

		// Request null ciphers only
		NoEncryption
	}

	public delegate bool MonoRemoteCertificateValidationCallback (
		string targetHost, X509Certificate certificate, X509Chain chain, MonoSslPolicyErrors sslPolicyErrors);

	public delegate X509Certificate MonoLocalCertificateSelectionCallback (
		string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate,
		string[] acceptableIssuers);

	public abstract class MonoTlsProvider
	{
		internal MonoTlsProvider ()
		{
		}

		public abstract Guid ID {
			get;
		}

		public abstract string Name {
			get;
		}

#region SslStream

		/*
		 * This section abstracts the @SslStream class.
		 *
		 */

		public abstract bool SupportsSslStream {
			get;
		}

		/*
		 * Does this provider support IMonoSslStream.GetConnectionInfo() ?
		 */
		public abstract bool SupportsConnectionInfo {
			get;
		}

		/*
		 * Whether or not this TLS Provider supports Mono-specific extensions
		 * (via @MonoTlsSettings).
		 */
		public abstract bool SupportsMonoExtensions {
			get;
		}

		public abstract SslProtocols SupportedProtocols {
			get;
		}

		/*
		 * Obtain a @IMonoSslStream instance.
		 *
		 */
		public abstract IMonoSslStream CreateSslStream (
			Stream innerStream, bool leaveInnerStreamOpen,
			MonoTlsSettings settings = null);

#endregion

#region Certificate Validation

		/*
		 * Allows a TLS provider to provide a custom system certificiate validator.
		 */
		public virtual bool HasCustomSystemCertificateValidator {
			get { return false; }
		}

		/*
		 * If @serverMode is true, then we're a server and want to validate a certificate
		 * that we received from a client.
		 *
		 * On OS X and Mobile, the @chain will be initialized with the @certificates, but not actually built.
		 *
		 * Returns `true` if certificate validation has been performed and `false` to invoke the
		 * default system validator.
		 */
		public virtual bool InvokeSystemCertificateValidator (
			ICertificateValidator validator, string targetHost, bool serverMode,
			X509CertificateCollection certificates, ref X509Chain chain, out bool success,
			ref MonoSslPolicyErrors errors, ref int status11)
		{
			success = false;
			return false;
		}

#endregion

#region Manged SSPI

		/*
		 * The managed SSPI implementation from the new TLS code.
		 */

		internal abstract bool SupportsTlsContext {
			get;
		}

		internal abstract IMonoTlsContext CreateTlsContext (
			string hostname, bool serverMode, TlsProtocols protocolFlags,
			X509Certificate serverCertificate, X509CertificateCollection clientCertificates,
			bool remoteCertRequired, MonoEncryptionPolicy encryptionPolicy,
			MonoTlsSettings settings);

#endregion
	}
}
