//
// CallbackHelpers.cs
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

#if SECURITY_DEP

#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
#endif

#if MONO_SECURITY_ALIAS
using MSI = MonoSecurity::Mono.Security.Interface;
#else
using MSI = Mono.Security.Interface;
#endif

using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Mono.Net.Security.Private
{
	/*
	 * Strictly private - do not use outside the Mono.Net.Security directory.
	 */
	static class CallbackHelpers
	{
		internal static MSI.MonoRemoteCertificateValidationCallback PublicToMono (RemoteCertificateValidationCallback callback)
		{
			if (callback == null)
				return null;

			return (h, c, ch, e) => callback (h, c, ch, (SslPolicyErrors)e);
		}

		internal static MSI.MonoRemoteCertificateValidationCallback InternalToMono (RemoteCertValidationCallback callback)
		{
			if (callback == null)
				return null;

			return (h, c, ch, e) => callback (h, c, ch, (SslPolicyErrors)e);
		}

		internal static RemoteCertificateValidationCallback InternalToPublic (string hostname, RemoteCertValidationCallback callback)
		{
			if (callback == null)
				return null;

			return (s, c, ch, e) => callback (hostname, c, ch, e);
		}

		internal static MSI.MonoLocalCertificateSelectionCallback InternalToMono (LocalCertSelectionCallback callback)
		{
			if (callback == null)
				return null;

			return (t, lc, rc, ai) => callback (t, lc, rc, ai);
		}

		internal static LocalCertificateSelectionCallback MonoToPublic (MSI.MonoLocalCertificateSelectionCallback callback)
		{
			if (callback == null)
				return null;

			return (s, t, lc, rc, ai) => callback (t, lc, rc, ai);
		}

		internal static RemoteCertValidationCallback MonoToInternal (MSI.MonoRemoteCertificateValidationCallback callback)
		{
			if (callback == null)
				return null;

			return (h, c, ch, e) => callback (h, c, ch, (MSI.MonoSslPolicyErrors)e);
		}

		internal static LocalCertSelectionCallback MonoToInternal (MSI.MonoLocalCertificateSelectionCallback callback)
		{
			if (callback == null)
				return null;

			return (t, lc, rc, ai) => callback (t, lc, rc, ai);
		}

		internal static ServerCertificateSelectionCallback MonoToPublic (MSI.MonoServerCertificateSelectionCallback callback)
		{
			if (callback == null)
				return null;

			return (s, h) => callback (s, h);
		}

		internal static MSI.MonoServerCertificateSelectionCallback PublicToMono (ServerCertificateSelectionCallback callback)
		{
			if (callback == null)
				return null;

			return (s, h) => callback (s, h);
		}
	}
}

#endif
