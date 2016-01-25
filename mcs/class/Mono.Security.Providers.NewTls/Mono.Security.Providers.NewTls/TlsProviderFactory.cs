//
// TlsProviderFactory.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2015-2016 Xamarin, Inc.
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

using System;
using System.IO;

using System.Net;
using System.Net.Security;
using System.Security.Authentication;

using MSI = Mono.Security.Interface;
using MX = Mono.Security.X509;

using PSSCX = System.Security.Cryptography.X509Certificates;
using SSCX = System.Security.Cryptography.X509Certificates;

namespace Mono.Security.Providers.NewTls
{
	static class TlsProviderFactory
	{
		const string assemblyName = "Mono.Security.NewTls, Version=4.0.0.0, Culture=neutral, PublicKeyToken=84e3aee7225169c2";
		const string tlsConfigTypeName = "Mono.Security.NewTls.TlsConfiguration";
		const string tlsContextTypeName = "Mono.Security.NewTls.TlsContext";

		static object CreateInstance (string typeName, object[] args)
		{
			var type = Type.GetType (typeName + ", " + assemblyName);
			return Activator.CreateInstance (type, args);
		}

		internal static ITlsConfiguration CreateTlsConfiguration (
			string hostname, bool serverMode, MSI.TlsProtocols protocolFlags,
			SSCX.X509Certificate serverCertificate, bool remoteCertRequired,
			MSI.MonoTlsSettings settings)
		{
			object[] args;
			ITlsConfiguration config;
			if (serverMode) {
				var cert = (PSSCX.X509Certificate2)serverCertificate;
				var monoCert = new MX.X509Certificate (cert.RawData);
				args = new object[] {
					(MSI.TlsProtocols)protocolFlags,
					(MSI.MonoTlsSettings)settings,
					monoCert,
					cert.PrivateKey
				};
			} else {
				args = new object[] {
					(MSI.TlsProtocols)protocolFlags,
					(MSI.MonoTlsSettings)settings,
					hostname
				};
			}

			config = (ITlsConfiguration)CreateInstance (tlsConfigTypeName, args);

			if (serverMode && remoteCertRequired)
				config.AskForClientCertificate = true;

			return config;
		}

		internal static ITlsContext CreateTlsContext (
			ITlsConfiguration config, bool serverMode,
			MSI.IMonoTlsEventSink eventSink)
		{
			return (ITlsContext)CreateInstance (
				tlsContextTypeName,
				new object[] { config, serverMode, eventSink });
		}
	}
}
