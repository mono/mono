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
		internal static object GetInternalValidator (object provider, object settings)
		{
			throw new NotSupportedException ();
		}

		internal static object GetDefaultValidator (object settings)
		{
			throw new NotSupportedException ();
		}

		internal static object GetProvider ()
		{
			throw new NotSupportedException ();
		}

		internal static bool IsInitialized {
			get {
				throw new NotSupportedException ();
			}
		}

		internal static void Initialize ()
		{
			throw new NotSupportedException ();
		}

		internal static void Initialize (string provider)
		{
			throw new NotSupportedException ();
		}

		internal static HttpWebRequest CreateHttpsRequest (Uri requestUri, object provider, object settings)
		{
			throw new NotSupportedException ();
		}

		internal static object CreateHttpListener (object certificate, object provider, object settings)
		{
			throw new NotSupportedException ();
		}

		internal static object GetMonoSslStream (SslStream stream)
		{
			throw new NotSupportedException ();
		}

		internal static object GetMonoSslStream (HttpListenerContext context)
		{
			throw new NotSupportedException ();
		}

		internal static bool IsProviderSupported (string name)
		{
			throw new NotSupportedException ();
		}

		internal static object GetProvider (string name)
		{
			throw new NotSupportedException ();
		}
	}
}
