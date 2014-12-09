//
// System.AndroidPlatform.cs
//
// Author:
//   Jonathan Pryor (jonp@xamarin.com)
//
// Copyright (C) 2012 Xamarin Inc (http://xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
#if MONODROID
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
#if SECURITY_DEP
using MSX = Mono.Security.X509;
#endif

namespace System {

	internal static class AndroidPlatform {
		delegate int GetInterfaceAddressesDelegate (out IntPtr ifap);
		delegate void FreeInterfaceAddressesDelegate (IntPtr ifap);
		
#if SECURITY_DEP
		static readonly Converter<List <byte[]>, bool> trustEvaluateSsl;
#endif  // SECURITY_DEP
		static readonly Func<IWebProxy> getDefaultProxy;
		static readonly GetInterfaceAddressesDelegate getInterfaceAddresses;
		static readonly FreeInterfaceAddressesDelegate freeInterfaceAddresses;

		static AndroidPlatform ()
		{
			var t = Type.GetType ("Android.Runtime.AndroidEnvironment, Mono.Android", throwOnError:true);
#if SECURITY_DEP
			trustEvaluateSsl = (Converter<List<byte[]>, bool>)
				Delegate.CreateDelegate (typeof (Converter<List<byte[]>, bool>),
							t,
							"TrustEvaluateSsl",
							ignoreCase:false,
							throwOnBindFailure:true);
#endif  // SECURITY_DEP
			getDefaultProxy = (Func<IWebProxy>)Delegate.CreateDelegate (
				typeof (Func<IWebProxy>), t, "GetDefaultProxy",
				ignoreCase:false,
				throwOnBindFailure:true);

			getInterfaceAddresses = (GetInterfaceAddressesDelegate)Delegate.CreateDelegate (
				typeof (GetInterfaceAddressesDelegate), t, "GetInterfaceAddresses",
				ignoreCase: false,
				throwOnBindFailure: false);
			
			freeInterfaceAddresses = (FreeInterfaceAddressesDelegate)Delegate.CreateDelegate (
				typeof (FreeInterfaceAddressesDelegate), t, "FreeInterfaceAddresses",
				ignoreCase: false,
				throwOnBindFailure: false);
		}

#if SECURITY_DEP
		internal static bool TrustEvaluateSsl (MSX.X509CertificateCollection collection, object sender, X509Certificate2 certificate, X509Chain chain, SslPolicyErrors errors)
		{
			var certsRawData = new List <byte[]> (collection.Count);
			foreach (MSX.X509Certificate cert in collection)
				certsRawData.Add (cert.RawData);
			return trustEvaluateSsl (certsRawData);
		}
#endif  // SECURITY_DEP

		internal static IWebProxy GetDefaultProxy ()
		{
			return getDefaultProxy ();
		}

		internal static int GetInterfaceAddresses (out IntPtr ifap)
		{
			ifap = IntPtr.Zero;
			if (getInterfaceAddresses == null)
				return -1;

			return getInterfaceAddresses (out ifap);
		}

		internal static void FreeInterfaceAddresses (IntPtr ifap)
		{
			if (freeInterfaceAddresses == null)
				return;

			freeInterfaceAddresses (ifap);
		}
	}
}
#endif  // MONODROID
