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
#if SECURITY_DEP
extern alias MonoSecurity;
#endif

#if MONODROID
using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
#if SECURITY_DEP
using MSX = MonoSecurity::Mono.Security.X509;
#endif

namespace System {

	internal static class AndroidPlatform {

#if SECURITY_DEP
		static readonly Converter<MSX.X509CertificateCollection, bool> trustEvaluateSsl;
		static readonly Func<MSX.X509CertificateCollection, object, X509Certificate2, X509Chain, SslPolicyErrors, bool> trustEvaluateSsl2;
#endif  // SECURITY_DEP


		static AndroidPlatform ()
		{
#if SECURITY_DEP
			var t = Type.GetType ("Android.Runtime.AndroidEnvironment, Mono.Android", throwOnError:true);
			trustEvaluateSsl2 = (Func<MSX.X509CertificateCollection, object, X509Certificate2, X509Chain, SslPolicyErrors, bool>)
				Delegate.CreateDelegate (
						typeof (Func<MSX.X509CertificateCollection, object, X509Certificate2, X509Chain, SslPolicyErrors, bool>),
						t,
						"TrustEvaluateSsl2",
						ignoreCase:false,
						throwOnBindFailure:false);
			if (trustEvaluateSsl2 == null)
				trustEvaluateSsl = (Converter<MSX.X509CertificateCollection, bool>)
					Delegate.CreateDelegate (typeof (Converter<MSX.X509CertificateCollection, bool>),
							t,
							"TrustEvaluateSsl",
							ignoreCase:false,
							throwOnBindFailure:true);
#endif  // SECURITY_DEP
		}

#if SECURITY_DEP
		internal static bool TrustEvaluateSsl (MSX.X509CertificateCollection collection, object sender, X509Certificate2 certificate, X509Chain chain, SslPolicyErrors errors)
		{
			if (trustEvaluateSsl2 != null)
				return trustEvaluateSsl2 (collection, sender, certificate, chain, errors);
			return trustEvaluateSsl (collection);
		}
#endif  // SECURITY_DEP
	}
}
#endif  // MONODROID
