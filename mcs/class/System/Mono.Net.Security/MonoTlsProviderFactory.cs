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
using System.Diagnostics;
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

				InitializeProviderRegistration ();

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

		static bool enableDebug;

		[Conditional ("MONO_TLS_DEBUG")]
		static void InitializeDebug ()
		{
			if (Environment.GetEnvironmentVariable ("MONO_TLS_DEBUG") != null)
				enableDebug = true;
		}

		[Conditional ("MONO_TLS_DEBUG")]
		internal static void Debug (string message, params object[] args)
		{
			if (enableDebug)
				Console.Error.WriteLine (message, args);
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

				InitializeDebug ();

				providerRegistration = new Dictionary<string,Tuple<Guid,string>> ();
				providerCache = new Dictionary<Guid,MSI.MonoTlsProvider> ();

				PopulateProviders ();
			}
		}

#if ONLY_APPLETLS || MONOTOUCH || XAMMAC
		// TODO: Should be redundant
		static void PopulateProviders ()
		{
			var appleTlsEntry = new Tuple<Guid,String> (AppleTlsId, typeof (Mono.AppleTls.AppleTlsProvider).FullName);

			providerRegistration.Add ("default", appleTlsEntry);
			providerRegistration.Add ("apple", appleTlsEntry);
		}
#elif MONODROID
		// TODO: Should be redundant		
		static void PopulateProviders ()
		{
			var legacyEntry = new Tuple<Guid,String> (LegacyId, typeof (Mono.Net.Security.LegacyTlsProvider).FullName);

			providerRegistration.Add ("legacy", legacyEntry);

	#if MONO_FEATURE_BTLS
			var btlsEntry = new Tuple<Guid,String> (BtlsId, typeof (Mono.Btls.MonoBtlsProvider).FullName);
			if (btlsEntry != null)
				providerRegistration.Add ("default", btlsEntry);
			else
	#endif
			providerRegistration.Add ("default", legacyEntry);
		}
#else
		static void PopulateProviders ()
		{
#if MONO_FEATURE_APPLETLS
			var appleTlsEntry = new Tuple<Guid,String> (AppleTlsId, typeof (Mono.AppleTls.AppleTlsProvider).FullName);
#endif
			var legacyEntry = new Tuple<Guid,String> (LegacyId, typeof (Mono.Net.Security.LegacyTlsProvider).FullName);
			providerRegistration.Add ("legacy", legacyEntry);

			Tuple<Guid,String> btlsEntry = null;
#if MONO_FEATURE_BTLS
			if (IsBtlsSupported ()) {
				btlsEntry = new Tuple<Guid,String> (BtlsId, typeof (Mono.Btls.MonoBtlsProvider).FullName);
				providerRegistration.Add ("btls", btlsEntry);
			}
#endif

#if MONO_FEATURE_APPLETLS
			if (Platform.IsMacOS)
				providerRegistration.Add ("default", appleTlsEntry);
			else
#endif
#if MONO_FEATURE_BTLS
			if (btlsEntry != null)
				providerRegistration.Add ("default", btlsEntry);
			else
#endif
				providerRegistration.Add ("default", legacyEntry);

#if MONO_FEATURE_APPLETLS
			providerRegistration.Add ("apple", appleTlsEntry);
#endif
		}
#endif


#if MONO_FEATURE_BTLS
		[MethodImpl (MethodImplOptions.InternalCall)]
		internal extern static bool IsBtlsSupported ();
#endif

		static MSI.MonoTlsProvider CreateDefaultProviderImpl ()
		{
#if MONODROID
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

#elif ONLY_APPLETLS || MONOTOUCH || XAMMAC
			return new AppleTlsProvider ();
#else
			var type = Environment.GetEnvironmentVariable ("MONO_TLS_PROVIDER");
			if (string.IsNullOrEmpty (type))
				type = "default";

			switch (type) {
			case "default":
#if MONO_FEATURE_APPLETLS
				if (Platform.IsMacOS)
					goto case "apple";
#endif
#if MONO_FEATURE_BTLS
				if (IsBtlsSupported ())
					goto case "btls";
#endif
				goto case "legacy";
#if MONO_FEATURE_APPLETLS
			case "apple":
				return new AppleTlsProvider ();
#endif
#if MONO_FEATURE_BTLS
			case "btls":
				return new MonoBtlsProvider ();
#endif
			case "legacy":
				return new Mono.Net.Security.LegacyTlsProvider ();
			}

			return LookupProvider (type, true);
#endif
		}

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
