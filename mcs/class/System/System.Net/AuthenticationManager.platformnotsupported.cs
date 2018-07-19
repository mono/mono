//
// AuthenticationManager.cs
//
// Author:
//       Rolf Bjarne Kvinge <rolf@xamarin.com>
//
// Copyright (c) 2016 Xamarin, Inc.
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

using System.Collections;
using System.Collections.Specialized;

namespace System.Net
{
	public class AuthenticationManager {
		const string EXCEPTION_MESSAGE = "System.Net.AuthenticationManager is not supported on the current platform.";

		private AuthenticationManager ()
		{
		}

		public static ICredentialPolicy CredentialPolicy {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public static StringDictionary CustomTargetNameDictionary {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public static IEnumerator RegisteredModules {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public static Authorization Authenticate (string challenge, WebRequest request, ICredentials credentials)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public static Authorization PreAuthenticate (WebRequest request, ICredentials credentials)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public static void Register (IAuthenticationModule authenticationModule)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public static void Unregister (IAuthenticationModule authenticationModule)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public static void Unregister (string authenticationScheme)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}
	}
}
