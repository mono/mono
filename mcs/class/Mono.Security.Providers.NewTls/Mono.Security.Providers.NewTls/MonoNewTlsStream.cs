//
// MonoNewTlsStream.cs
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

extern alias NewSystemSource;

using EncryptionPolicy = NewSystemSource::System.Net.Security.EncryptionPolicy;
using LocalCertificateSelectionCallback = NewSystemSource::System.Net.Security.LocalCertificateSelectionCallback;
using RemoteCertificateValidationCallback = NewSystemSource::System.Net.Security.RemoteCertificateValidationCallback;
using SslStream = NewSystemSource::System.Net.Security.SslStream;

using System;
using System.IO;
using System.Threading.Tasks;

using MSI = Mono.Security.Interface;

using XAuthenticatedStream = System.Net.Security.AuthenticatedStream;
using System.Security.Cryptography.X509Certificates;

namespace Mono.Security.Providers.NewTls
{
	public class MonoNewTlsStream : SslStream, MSI.IMonoSslStream
	{
		MSI.MonoTlsProvider provider;

		internal MonoNewTlsStream (Stream innerStream, MSI.MonoTlsProvider provider, MSI.MonoTlsSettings settings)
			: this (innerStream, false, provider, settings)
		{
		}

		internal MonoNewTlsStream (Stream innerStream, bool leaveOpen, MSI.MonoTlsProvider provider, MSI.MonoTlsSettings settings)
			: base (innerStream, leaveOpen, EncryptionPolicy.RequireEncryption, provider, settings)
		{
			this.provider = provider;
		}

		public MSI.MonoTlsProvider Provider {
			get { return provider; }
		}

		new public bool IsClosed {
			get { return base.IsClosed; }
		}

		public MSI.MonoTlsConnectionInfo GetConnectionInfo ()
		{
			return GetMonoConnectionInfo ();
		}

		public Task Shutdown ()
		{
			return Task.Factory.FromAsync ((state, result) => BeginShutdown (state, result), EndShutdown, null);
		}

		public Task RequestRenegotiation ()
		{
			return Task.Factory.FromAsync ((state, result) => BeginRenegotiate (state, result), EndRenegotiate, null);
		}

		 X509Certificate MSI.IMonoSslStream.InternalLocalCertificate {
			get { return InternalLocalCertificate; }
		}

		XAuthenticatedStream MSI.IMonoSslStream.AuthenticatedStream {
			get { return this; }
		}
	}
}


