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
using System.Net.Security;

namespace Mono.Net.Security
{
	//
	// Internal APIs which are used by Mono.Security.dll to avoid using reflection.
	//
	internal static class NoReflectionHelper
	{
		internal static object GetDefaultValidator (object settings)
		{
			#if SECURITY_DEP
			return ChainValidationHelper.GetDefaultValidator ((MSI.MonoTlsSettings)settings);
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

		internal static bool IsInitialized {
			get {
				#if SECURITY_DEP
				return MonoTlsProviderFactory.IsInitialized;
				#else
				throw new NotSupportedException ();
				#endif
			}
		}

		internal static void Initialize ()
		{
			#if SECURITY_DEP
			MonoTlsProviderFactory.Initialize ();
			#else
			throw new NotSupportedException ();
			#endif
		}

		internal static void Initialize (string provider)
		{
			#if SECURITY_DEP
			MonoTlsProviderFactory.Initialize (provider);
			#else
			throw new NotSupportedException ();
			#endif
		}

		internal static HttpWebRequest CreateHttpsRequest (Uri requestUri, object provider, object settings)
		{
			#if SECURITY_DEP
			return new HttpWebRequest (requestUri, (MobileTlsProvider)provider, (MSI.MonoTlsSettings)settings);
			#else
			throw new NotSupportedException ();
			#endif
		}

		internal static object CreateHttpListener (object certificate, object provider, object settings)
		{
			#if SECURITY_DEP
			return new HttpListener ((X509Certificate)certificate, (MSI.MonoTlsProvider)provider, (MSI.MonoTlsSettings)settings);
			#else
			throw new NotSupportedException ();
			#endif
		}

		internal static object GetMonoSslStream (SslStream stream)
		{
			#if SECURITY_DEP
			return stream.Impl;
			#else
			throw new NotSupportedException ();
			#endif
		}

		internal static object GetMonoSslStream (HttpListenerContext context)
		{
#if SECURITY_DEP
			return context.Connection.SslStream?.Impl;
#else
			throw new NotSupportedException ();
#endif
		}

		internal static bool IsProviderSupported (string name)
		{
			#if SECURITY_DEP
			return MonoTlsProviderFactory.IsProviderSupported (name);
			#else
			throw new NotSupportedException ();
			#endif
		}

		internal static object GetProvider (string name)
		{
			#if SECURITY_DEP
			return MonoTlsProviderFactory.GetProvider (name);
			#else
			throw new NotSupportedException ();
			#endif
		}
	}
}
