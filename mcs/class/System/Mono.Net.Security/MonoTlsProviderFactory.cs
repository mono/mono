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

using System;
using System.Net;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#if MONO_FEATURE_BTLS
using Mono.Btls;
#endif

#if MONO_FEATURE_APPLETLS
using Mono.AppleTls;
#endif

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
		 */

		internal static MSI.MonoTlsProvider GetProviderInternal ()
		{
			lock (locker) {
				InitializeInternal ();
				return defaultProvider;
			}
		}

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

				if (!providerCache.ContainsKey (provider.ID))
					providerCache.Add (provider.ID, provider);

				X509Helper2.Initialize ();

				defaultProvider = provider;
				initialized = true;
			}
		}

		internal static void InitializeInternal (string provider) 
		{
			lock (locker) {
				if (initialized)
					throw new NotSupportedException ("TLS Subsystem already initialized.");

				defaultProvider = LookupProvider (provider, true);

				X509Helper2.Initialize ();
				initialized = true;
			}
		}

		static object locker = new object ();
		static bool initialized;

		static MSI.MonoTlsProvider defaultProvider;

		/*
		 * @providerRegistration maps provider names to a tuple containing its ID and full type name.
		 * On non-reflection enabled systems (such as XI and XM), we can use the Guid to uniquely
		 * identify the provider.
		 *
		 * @providerCache maps the provider's Guid to the MSI.MonoTlsProvider instance.
		 *
		 */
		static Dictionary<string,Tuple<Guid,string>> providerRegistration;
		static Dictionary<Guid,MSI.MonoTlsProvider> providerCache;

#if !ONLY_APPLETLS && !MONOTOUCH && !XAMMAC
		static Type LookupProviderType (string name, bool throwOnError)
		{
			lock (locker) {
				InitializeProviderRegistration ();
				Tuple<Guid,string> entry;
				if (!providerRegistration.TryGetValue (name, out entry)) {
					if (throwOnError)
						throw new NotSupportedException (string.Format ("No such TLS Provider: `{0}'.", name));
					return null;
				}
				var type = Type.GetType (entry.Item2, false);
				if (type == null && throwOnError)
					throw new NotSupportedException (string.Format ("Could not find TLS Provider: `{0}'.", entry.Item2));
				return type;
			}
		}
#endif

		static MSI.MonoTlsProvider LookupProvider (string name, bool throwOnError)
		{
			lock (locker) {
				InitializeProviderRegistration ();
				Tuple<Guid,string> entry;
				if (!providerRegistration.TryGetValue (name, out entry)) {
					if (throwOnError)
						throw new NotSupportedException (string.Format ("No such TLS Provider: `{0}'.", name));
					return null;
				}

				// Check cache before doing the reflection lookup.
				MSI.MonoTlsProvider provider;
				if (providerCache.TryGetValue (entry.Item1, out provider))
					return provider;

#if !ONLY_APPLETLS && !MONOTOUCH && !XAMMAC
				var type = Type.GetType (entry.Item2, false);
				if (type == null && throwOnError)
					throw new NotSupportedException (string.Format ("Could not find TLS Provider: `{0}'.", entry.Item2));

				try {
					provider = (MSI.MonoTlsProvider)Activator.CreateInstance (type, true);
				} catch (Exception ex) {
					throw new NotSupportedException (string.Format ("Unable to instantiate TLS Provider `{0}'.", type), ex);
				}
#endif

				if (provider == null) {
					if (throwOnError)
						throw new NotSupportedException (string.Format ("No such TLS Provider: `{0}'.", name));
					return null;
				}

				providerCache.Add (entry.Item1, provider);
				return provider;
			}
		}

#endregion

		internal static readonly Guid AppleTlsId = new Guid ("981af8af-a3a3-419a-9f01-a518e3a17c1c");
		internal static readonly Guid BtlsId = new Guid ("432d18c9-9348-4b90-bfbf-9f2a10e1f15b");
		internal static readonly Guid LegacyId = new Guid ("809e77d5-56cc-4da8-b9f0-45e65ba9cceb");

		static void InitializeProviderRegistration ()
		{
			lock (locker) {
				if (providerRegistration != null)
					return;
				providerRegistration = new Dictionary<string,Tuple<Guid,string>> ();
				providerCache = new Dictionary<Guid,MSI.MonoTlsProvider> ();

				var appleTlsEntry = new Tuple<Guid,String> (AppleTlsId, "Mono.AppleTls.AppleTlsProvider");

#if ONLY_APPLETLS || MONOTOUCH || XAMMAC
				providerRegistration.Add ("default", appleTlsEntry);
				providerRegistration.Add ("apple", appleTlsEntry);
#else
				var legacyEntry = new Tuple<Guid,String> (LegacyId, "Mono.Net.Security.LegacyTlsProvider");
				providerRegistration.Add ("legacy", legacyEntry);

				Tuple<Guid,String> btlsEntry = null;
#if MONO_FEATURE_BTLS
				if (IsBtlsSupported ()) {
					btlsEntry = new Tuple<Guid,String> (BtlsId, "Mono.Btls.MonoBtlsProvider");
					providerRegistration.Add ("btls", btlsEntry);
				}
#endif

				if (Platform.IsMacOS)
					providerRegistration.Add ("default", appleTlsEntry);
				else if (btlsEntry != null)
					providerRegistration.Add ("default", btlsEntry);
				else
					providerRegistration.Add ("default", legacyEntry);

				providerRegistration.Add ("apple", appleTlsEntry);
#endif
			}
		}

#region Platform-Specific code

#if MONO_FEATURE_BTLS
		[MethodImpl (MethodImplOptions.InternalCall)]
		internal extern static bool IsBtlsSupported ();
#endif

#if MONODROID
		static MSI.MonoTlsProvider CreateDefaultProviderImpl ()
		{
			MSI.MonoTlsProvider provider = null;
			var type = Environment.GetEnvironmentVariable ("XA_TLS_PROVIDER");
			switch (type) {
			case null:
			case "default":
			case "legacy":
				return new LegacyTlsProvider ();
#if MONO_FEATURE_BTLS
			case "btls":
				if (!IsBtlsSupported ())
					throw new NotSupportedException ("BTLS in not supported!");
				return new MonoBtlsProvider ();
#endif
			default:
				throw new NotSupportedException (string.Format ("Invalid TLS Provider: `{0}'.", provider));
			}
		}
#elif ONLY_APPLETLS || MONOTOUCH || XAMMAC
		static MSI.MonoTlsProvider CreateDefaultProviderImpl ()
		{
			return new AppleTlsProvider ();
		}
#else
		static MSI.MonoTlsProvider CreateDefaultProviderImpl ()
		{
			var variable = Environment.GetEnvironmentVariable ("MONO_TLS_PROVIDER");
			if (string.IsNullOrEmpty (variable))
				variable = "default";

			return LookupProvider (variable, true);
		}
#endif

#endregion

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

			return provider;
		}

		internal static bool IsProviderSupported (string name)
		{
			lock (locker) {
				InitializeProviderRegistration ();
				return providerRegistration.ContainsKey (name);
			}
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
			InitializeInternal ();
		}

		internal static void Initialize (string provider)
		{
			InitializeInternal (provider);
		}
#endregion
	}
}
#endif
