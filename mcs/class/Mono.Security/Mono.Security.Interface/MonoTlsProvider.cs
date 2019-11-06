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

#region Native Certificate Implementation

		internal virtual bool HasNativeCertificates {
			get { return false; }
		}

#endregion

#region Misc

		internal abstract bool SupportsCleanShutdown {
			get;
		}

#endregion

	}
}
