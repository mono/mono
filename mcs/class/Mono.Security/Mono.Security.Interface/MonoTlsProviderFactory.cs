//
// MonoTlsProviderFactory.cs
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
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Mono.Net.Security;

namespace Mono.Security.Interface
{
	/*
	 * Public API front-end to System.dll's version.
	 *
	 * Keep in sync with System/Mono.Net.Security/MonoTlsProviderFactory.cs.
	 */
	public static partial class MonoTlsProviderFactory
	{
		/*
		 * TLS Provider Initialization
		 * ===========================
		 * 
		 * The "global" TLS Provider (returned by GetProvider()) may only be modified at
		 * application startup (before any of the TLS / Certificate code has been used).
		 * 
		 * On mobile, the default provider is specified at compile time using a property
		 * in the .csproj file (which can be set from the IDE).  When using the linker, all
		 * other providers will be linked-out, so you won't be able to choose a different
		 * provider at run-time.
		 * 
		 * On desktop, the default provider can be specified with the MONO_TLS_PROVIDER
		 * environment variable.  The following options are currently supported:
		 * 
		 *    "default" - let Mono pick the best one for you (recommended)
		 *    "old" or "legacy" - Mono's old managed TLS implementation
		 *    "appletls" (currently XamMac only, set via .csproj property)
		 *    "btls" - the new boringssl based provider (coming soon).
		 * 
		 * On all platforms (except mobile with linker), you can call
		 * 
		 *     MonoTlsProviderFactory.Initialize(string)
		 * 
		 * to use a different provider.
		 * 
		 */

		#region Provider Initialization

		/*
		 * Returns the global @MonoTlsProvider, initializing the TLS Subsystem if necessary.
		 *
		 * This method throws @NotSupportedException if no TLS Provider can be found.
		 */
		public static MonoTlsProvider GetProvider ()
		{
			return (MonoTlsProvider)NoReflectionHelper.GetProvider ();
		}

		/*
		 * Check whether the TLS Subsystem is initialized.
		 */
		public static bool IsInitialized {
			get {
				return NoReflectionHelper.IsInitialized;
			}
		}

		/*
		 * Initialize the TLS Subsystem.
		 * 
		 * This method may be called at any time.  It ensures that the TLS Subsystem is
		 * initialized and a provider available.
		 */
		public static void Initialize ()
		{
			NoReflectionHelper.Initialize ();
		}

		/*
		 * Initialize the TLS Subsystem with a specific provider.
		 * 
		 * May only be called at application startup (before any of the TLS / Certificate
		 * APIs have been used).
		 * 
		 * Throws @NotSupportedException if the TLS Subsystem is already initialized
		 * (@IsInitialized returns true) or the requested provider is not supported.
		 * 
		 * On mobile, this will always throw @NotSupportedException when using the linker.
		 */
		public static void Initialize (string provider)
		{
			NoReflectionHelper.Initialize (provider);
		}

		/*
		 * Checks whether @provider is supported.
		 *
		 * On mobile, this will always return false when using the linker.
		 */
		public static bool IsProviderSupported (string provider)
		{
			return NoReflectionHelper.IsProviderSupported (provider);
		}

		#endregion

		#region Call-by-call selection

		/*
		 * Returns the requested TLS Provider, for use with the call-by-call APIs below.
		 * 
		 * Throw @NotSupportedException if the requested provider is not supported or
		 * when using the linker on mobile.
		 */
		public static MonoTlsProvider GetProvider (string provider)
		{
			return (MonoTlsProvider)NoReflectionHelper.GetProvider (provider);
		}

		/*
		 * Create @HttpWebRequest with the specified @provider (may be null to use the default one).
		 * 
		 * NOTE: This needs to be written as "System.Uri" to avoid ambiguity with Mono.Security.Uri in the
		 *        mobile build.
		 * 
		 */
		public static HttpWebRequest CreateHttpsRequest (System.Uri requestUri, MonoTlsProvider provider, MonoTlsSettings settings = null)
		{
			return NoReflectionHelper.CreateHttpsRequest (requestUri, provider, settings);
		}

		public static HttpListener CreateHttpListener (X509Certificate certificate, MonoTlsProvider provider = null, MonoTlsSettings settings = null)
		{
			return (HttpListener)NoReflectionHelper.CreateHttpListener (certificate, provider, settings);
		}

		public static IMonoSslStream GetMonoSslStream (SslStream stream)
		{
			return (IMonoSslStream)NoReflectionHelper.GetMonoSslStream (stream);
		}

		public static IMonoSslStream GetMonoSslStream (HttpListenerContext context)
		{
			return (IMonoSslStream)NoReflectionHelper.GetMonoSslStream (context);
		}

		#endregion

		#region Internal Version

		/*
		 * Internal version number (not in any way related to the TLS Version).
		 *
		 * Used by the web-tests to check whether
		 * the current Mono contains certain features or bug fixes.
		 *
		 * Negative version numbers are reserved for martin work branches.
		 *
		 * Version History:
		 *
		 * - 1: everything up until May 2018
		 * - 2: the new ServicePointScheduler changes have landed
		 * - 3: full support for Client Certificates
		 * - 4: Legacy TLS Removal
		 *
		 */
		internal const int InternalVersion = 4;

		#endregion
	}
}

