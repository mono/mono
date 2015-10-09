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
using System.Reflection;

namespace Mono.Security.Interface
{
	/*
	 * Public API front-end to System.dll's version.
	 *
	 * Keep in sync with System/Mono.Net.Security/MonoTlsProviderFactory.cs.
	 */
	public static class MonoTlsProviderFactory
	{
		const string FactoryTypeName = "Mono.Net.Security.MonoTlsProviderFactory";

		static MonoTlsProviderFactory ()
		{
			factoryType = Type.GetType (FactoryTypeName + ", " + Consts.AssemblySystem, true);
			getProviderMethod = factoryType.GetMethod ("GetProvider", BindingFlags.Static | BindingFlags.NonPublic);
			getDefaultProviderMethod = factoryType.GetMethod ("GetDefaultProvider", BindingFlags.Static | BindingFlags.NonPublic);
			installProviderMethod = factoryType.GetMethod ("InstallProvider", BindingFlags.Static | BindingFlags.NonPublic);
			hasProviderProperty = factoryType.GetProperty ("HasProvider", BindingFlags.Static | BindingFlags.NonPublic);
			createHttpsRequestMethod = factoryType.GetMethod ("CreateHttpsRequest", BindingFlags.Static | BindingFlags.NonPublic);
			if (getProviderMethod == null || getDefaultProviderMethod == null || installProviderMethod == null ||
			    hasProviderProperty == null || createHttpsRequestMethod == null)
				throw new NotSupportedException ();
		}

		/*
		 * Returns the currently installed @MonoTlsProvider, falling back to the default one.
		 *
		 * This method throws @NotSupportedException if no TLS Provider can be found.
		 */
		public static MonoTlsProvider GetProvider ()
		{
			return (MonoTlsProvider)getProviderMethod.Invoke (null, null);
		}

		/*
		 * Returns the default @MonoTlsProvider.
		 *
		 * This method throws @NotSupportedException if no TLS Provider can be found.
		 */
		public static MonoTlsProvider GetDefaultProvider ()
		{
			return (MonoTlsProvider)getDefaultProviderMethod.Invoke (null, null);
		}

		/*
		 * GetProvider() attempts to load and install the default provider and throws on error.
		 *
		 * This property checks whether a provider has previously been installed by a call
		 * to either GetProvider() or InstallProvider().
		 *
		 */
		public static bool HasProvider {
			get {
				return (bool)hasProviderProperty.GetValue (null);
			}
		}

		/*
		 * Installs a custom TLS Provider.
		 *
		 * May only be called at application startup and will throw
		 * @InvalidOperationException if a provider has already been installed.
		 */
		public static void InstallProvider (MonoTlsProvider provider)
		{
			installProviderMethod.Invoke (null, new object[] { provider });
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
			return (HttpWebRequest)createHttpsRequestMethod.Invoke (null, new object[] { requestUri, provider, settings });
		}

		static Type factoryType;
		static MethodInfo getProviderMethod;
		static MethodInfo getDefaultProviderMethod;
		static MethodInfo installProviderMethod;
		static PropertyInfo hasProviderProperty;
		static MethodInfo createHttpsRequestMethod;
	}
}

