//
// NoReflectionHelper.cs
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
using MSI = MonoSecurity::Mono.Security.Interface;
using MX = MonoSecurity::Mono.Security.X509;
#else
using MSI = Mono.Security.Interface;
using MX = Mono.Security.X509;
#endif
using System.Security.Cryptography.X509Certificates;
#endif

using System;
using System.Net;

namespace Mono.Net.Security
{
	//
	// Internal APIs which are used by Mono.Security.dll to avoid using reflection.
	//
	internal static class NoReflectionHelper
	{
		internal static object GetDefaultCertificateValidator (object provider, object settings)
		{
			#if SECURITY_DEP
			return ChainValidationHelper.GetDefaultValidator ((MSI.MonoTlsProvider)provider, (MSI.MonoTlsSettings)settings);
			#else
			throw new NotSupportedException ();
			#endif
		}

		internal static object GetProvider ()
		{
			#if SECURITY_DEP
			return MonoTlsProviderFactory.GetProvider ();
			#else
			throw new NotSupportedException ();
			#endif
		}

		internal static object GetDefaultProvider ()
		{
			#if SECURITY_DEP
			return MonoTlsProviderFactory.GetDefaultProvider ();
			#else
			throw new NotSupportedException ();
			#endif
		}

		internal static bool HasProvider {
			get {
				#if SECURITY_DEP
				return MonoTlsProviderFactory.HasProvider;
				#else
				throw new NotSupportedException ();
				#endif
			}
		}

		internal static void InstallProvider (object provider)
		{
			#if SECURITY_DEP
			MonoTlsProviderFactory.InstallProvider ((MSI.MonoTlsProvider)provider);
			#else
			throw new NotSupportedException ();
			#endif
		}

		internal static HttpWebRequest CreateHttpsRequest (Uri requestUri, object provider, object settings)
		{
			#if SECURITY_DEP
			return MonoTlsProviderFactory.CreateHttpsRequest (requestUri, (MSI.MonoTlsProvider)provider, (MSI.MonoTlsSettings)settings);
			#else
			throw new NotSupportedException ();
			#endif
		}

		internal static object CreateHttpListener (object certificate, object provider, object settings)
		{
			#if SECURITY_DEP
			return MonoTlsProviderFactory.CreateHttpListener ((X509Certificate)certificate, (MSI.MonoTlsProvider)provider, (MSI.MonoTlsSettings)settings);
			#else
			throw new NotSupportedException ();
			#endif
		}
	}
}
