//
// System.Net.HttpListener
//
// Author:
//	Rolf Bjarne Kvinge <rolf@xamarin.com>
//
// Copyright (C) 2016 Xamarin Inc (http://www.xamarin.com)
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

using System.Security.Authentication.ExtendedProtection;
using System.Threading.Tasks;

namespace System.Net {
	public sealed class HttpListener : IDisposable
	{
		internal const string EXCEPTION_MESSAGE = "System.Net.HttpListener is not supported on the current platform.";

		public delegate ExtendedProtectionPolicy ExtendedProtectionSelector (HttpListenerRequest request);

		public HttpListener ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public AuthenticationSchemes AuthenticationSchemes {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

#if SECURITY_DEP
		public AuthenticationSchemeSelector AuthenticationSchemeSelectorDelegate {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}
#endif

		public bool IgnoreWriteExceptions {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public bool IsListening {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public static bool IsSupported {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public HttpListenerPrefixCollection Prefixes {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public string Realm {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public bool UnsafeConnectionNtlmAuthentication {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public HttpListenerTimeoutManager TimeoutManager {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public ExtendedProtectionPolicy ExtendedProtectionPolicy
		{
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public ExtendedProtectionSelector ExtendedProtectionSelectorDelegate
		{
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public ServiceNameCollection DefaultServiceNames
		{
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public void Abort ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void Close ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public IAsyncResult BeginGetContext (AsyncCallback callback, Object state)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public HttpListenerContext EndGetContext (IAsyncResult asyncResult)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public HttpListenerContext GetContext ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void Start ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void Stop ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		void IDisposable.Dispose ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<HttpListenerContext> GetContextAsync ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}
	}
}
