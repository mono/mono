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

#if SECURITY_DEP
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
using MSI = MonoSecurity::Mono.Security.Interface;
using MX = MonoSecurity::Mono.Security.X509;
#else
using MSI = Mono.Security.Interface;
using MX = Mono.Security.X509;
#endif
#endif

using System;
using System.Net;

namespace Mono.Net.Security
{
	/*
	 * Keep in sync with Mono.Security/Mono.Security.Interface/MonoTlsProvider.cs.
	 *
	 */
	static class MonoTlsProviderFactory
	{
		#region Internal API

		/*
		 * APIs in this section are for consumption within System.dll only - do not access via
		 * reflection or from friend assemblies.
		 * 
		 * @IMonoTlsProvider is defined as empty interface outside 'SECURITY_DEP', so we don't need
		 * this conditional here.
		 */

		internal static IMonoTlsProvider GetProviderInternal ()
		{
			lock (locker) {
				if (currentProvider != null)
					return currentProvider;

				try {
					defaultProvider = CreateDefaultProvider ();
				} catch (Exception ex) {
					throw new NotSupportedException ("TLS Support not available.", ex);
				}

				if (defaultProvider == null)
					throw new NotSupportedException ("TLS Support not available.");

				currentProvider = defaultProvider;
				return currentProvider;
			}
		}

		internal static IMonoTlsProvider GetDefaultProviderInternal ()
		{
			lock (locker) {
				if (defaultProvider != null)
					return defaultProvider;

				try {
					defaultProvider = CreateDefaultProvider ();
				} catch (Exception ex) {
					throw new NotSupportedException ("TLS Support not available.", ex);
				}

				if (defaultProvider == null)
					throw new NotSupportedException ("TLS Support not available.");

				return defaultProvider;
			}
		}

		static IMonoTlsProvider CreateDefaultProvider ()
		{
#if SECURITY_DEP
#if MONO_FEATURE_NEW_SYSTEM_SOURCE
			/*
			 * This is a hack, which is used in the Mono.Security.Providers.NewSystemSource
			 * assembly, which will provide a "fake" System.dll.  Use the public Mono.Security
			 * API to get the "real" System.dll's provider via reflection, then wrap it with
			 * the "fake" version's perceived view.
			 *
			 * NewSystemSource needs to compile MonoTlsProviderFactory.cs, IMonoTlsProvider.cs,
			 * MonoTlsProviderWrapper.cs and CallbackHelpers.cs from this directory and only these.
			 */
			var userProvider = MSI.MonoTlsProviderFactory.GetProvider ();
			return new Private.MonoTlsProviderWrapper (userProvider);
#else
			return new Private.MonoDefaultTlsProvider ();
#endif
#else
			return null;
#endif
		}

		static object locker = new object ();
		static IMonoTlsProvider defaultProvider;
		static IMonoTlsProvider currentProvider;

		#endregion

		#region Internal Interface

		internal static object GetDefaultCertificateValidator (object settings)
		{
			#if SECURITY_DEP
			return ChainValidationHelper.GetDefaultValidator ((MSI.MonoTlsSettings)settings);
			#else
			throw new NotSupportedException ();
			#endif
		}

		#endregion

#if SECURITY_DEP && !MONO_FEATURE_NEW_SYSTEM_SOURCE

		#region Mono.Security visible API

		/*
		 * "Public" section, intended to be consumed via reflection.
		 * 
		 * Mono.Security.dll provides a public wrapper around these.
		 */

		internal static MSI.MonoTlsProvider GetProvider ()
		{
			var provider = GetProviderInternal ();
			if (provider == null)
				throw new NotSupportedException ("No TLS Provider available.");

			return provider.Provider;
		}

		internal static MSI.MonoTlsProvider GetDefaultProvider ()
		{
			var provider = GetDefaultProviderInternal ();
			if (provider == null)
				throw new NotSupportedException ("No TLS Provider available.");

			return provider.Provider;
		}

		internal static bool HasProvider {
			get {
				lock (locker) {
					return currentProvider != null;
				}
			}
		}

		internal static void InstallProvider (MSI.MonoTlsProvider provider)
		{
			lock (locker) {
				currentProvider = new Private.MonoTlsProviderWrapper (provider);
			}
		}

		internal static HttpWebRequest CreateHttpsRequest (Uri requestUri, MSI.MonoTlsProvider provider, MSI.MonoTlsSettings settings)
		{
			lock (locker) {
				var internalProvider = provider != null ? new Private.MonoTlsProviderWrapper (provider) : null;
				return new HttpWebRequest (requestUri, internalProvider, settings);
			}
		}
		#endregion

#endif

	}
}

