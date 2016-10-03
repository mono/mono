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
using System.Security.Cryptography.X509Certificates;
#endif

using System;
using System.Net;
using System.Collections.Generic;

#if !MOBILE
using System.Reflection;
#endif

namespace Mono.Net.Security
{
	/*
	 * Keep in sync with Mono.Security/Mono.Security.Interface/MonoTlsProvider.cs.
	 *
	 */
	static partial class MonoTlsProviderFactory
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
			#if SECURITY_DEP
			lock (locker) {
				InitializeInternal ();
				return defaultProvider;
			}
			#else
			throw new NotSupportedException ("TLS Support not available.");
			#endif
		}

#if SECURITY_DEP
		internal static void InitializeInternal ()
		{
			lock (locker) {
				if (initialized)
					return;

				MSI.MonoTlsProvider provider;
				try {
					provider = CreateDefaultProviderImpl ();
				} catch (Exception ex) {
					throw new NotSupportedException ("TLS Support not available.", ex);
				}

				if (provider == null)
					throw new NotSupportedException ("TLS Support not available.");

				defaultProvider = new Private.MonoTlsProviderWrapper (provider);
				initialized = true;
			}
		}

		internal static void InitializeInternal (string provider) 
		{
			lock (locker) {
				if (initialized)
					throw new NotSupportedException ("TLS Subsystem already initialized.");

				var msiProvider = LookupProvider (provider, true);
				defaultProvider = new Private.MonoTlsProviderWrapper (msiProvider);
				initialized = true;
			}
		}
#endif

		static object locker = new object ();
		static bool initialized;
		static IMonoTlsProvider defaultProvider;

		#endregion

#if SECURITY_DEP

		static Dictionary<string,string> providerRegistration;

		static Type LookupProviderType (string name, bool throwOnError)
		{
			lock (locker) {
				InitializeProviderRegistration ();
				string typeName;
				if (!providerRegistration.TryGetValue (name, out typeName)) {
					if (throwOnError)
						throw new NotSupportedException (string.Format ("No such TLS Provider: `{0}'.", name));
					return null;
				}
				var type = Type.GetType (typeName, false);
				if (type == null && throwOnError)
					throw new NotSupportedException (string.Format ("Could not find TLS Provider: `{0}'.", typeName));
				return type;
			}
		}

		static MSI.MonoTlsProvider LookupProvider (string name, bool throwOnError)
		{
			var type = LookupProviderType (name, throwOnError);
			if (type == null)
				return null;

			try {
				return (MSI.MonoTlsProvider)Activator.CreateInstance (type, true);
			} catch (Exception ex) {
				throw new NotSupportedException (string.Format ("Unable to instantiate TLS Provider `{0}'.", type), ex);
			}
		}

		static void InitializeProviderRegistration ()
		{
			lock (locker) {
				if (providerRegistration != null)
					return;
				providerRegistration = new Dictionary<string,string> ();
				providerRegistration.Add ("legacy", "Mono.Net.Security.LegacyTlsProvider");
				providerRegistration.Add ("default", "Mono.Net.Security.LegacyTlsProvider");
				if (Mono.Btls.MonoBtlsProvider.IsSupported ())
					providerRegistration.Add ("btls", "Mono.Btls.MonoBtlsProvider");
				X509Helper2.Initialize ();
			}
		}

#if MOBILE_STATIC || !MOBILE
		static MSI.MonoTlsProvider TryDynamicLoad ()
		{
			var variable = Environment.GetEnvironmentVariable ("MONO_TLS_PROVIDER");
			if (string.IsNullOrEmpty (variable))
				variable = "default";

			return LookupProvider (variable, true);
		}

		static MSI.MonoTlsProvider CreateDefaultProviderImpl ()
		{
			var provider = TryDynamicLoad ();
			if (provider != null)
				return provider;

			return new LegacyTlsProvider ();
		}
#endif

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

		internal static bool IsProviderSupported (string name)
		{
			return LookupProvider (name, false) != null;
		}

		internal static MSI.MonoTlsProvider GetProvider (string name)
		{
			return LookupProvider (name, false);
		}

		internal static bool IsInitialized {
			get {
				lock (locker) {
					return initialized;
				}
			}
		}

		internal static void Initialize ()
		{
			#if SECURITY_DEP
			InitializeInternal ();
			#else
			throw new NotSupportedException ("TLS Support not available.");
			#endif
		}

		internal static void Initialize (string provider)
		{
			#if SECURITY_DEP
			InitializeInternal (provider);
			#else
			throw new NotSupportedException ("TLS Support not available.");
			#endif
		}

		internal static HttpWebRequest CreateHttpsRequest (Uri requestUri, MSI.MonoTlsProvider provider, MSI.MonoTlsSettings settings)
		{
			lock (locker) {
				var internalProvider = provider != null ? new Private.MonoTlsProviderWrapper (provider) : null;
				return new HttpWebRequest (requestUri, internalProvider, settings);
			}
		}

		internal static HttpListener CreateHttpListener (X509Certificate certificate, MSI.MonoTlsProvider provider, MSI.MonoTlsSettings settings)
		{
			lock (locker) {
				var internalProvider = provider != null ? new Private.MonoTlsProviderWrapper (provider) : null;
				return new HttpListener (certificate, internalProvider, settings);
			}
		}
		#endregion

#endif

	}
}

