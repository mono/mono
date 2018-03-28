//
// System.Net.HttpListenerResponse
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

using System.IO;
using System.Text;

namespace System.Net {
	public sealed class HttpListenerResponse : IDisposable
	{
		const string EXCEPTION_MESSAGE = "System.Net.HttpListenerResponse is not supported on the current platform.";

		HttpListenerResponse ()
		{
		}

		public Encoding ContentEncoding {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public long ContentLength64 {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public string ContentType {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public CookieCollection Cookies {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public WebHeaderCollection Headers {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public bool KeepAlive {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public Stream OutputStream {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public Version ProtocolVersion {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public string RedirectLocation {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public bool SendChunked {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public int StatusCode {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public string StatusDescription {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		void IDisposable.Dispose ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void Abort ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void AddHeader (string name, string value)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void AppendCookie (Cookie cookie)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void AppendHeader (string name, string value)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void Close ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void Close (byte [] responseEntity, bool willBlock)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void CopyFrom (HttpListenerResponse templateResponse)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void Redirect (string url)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void SetCookie (Cookie cookie)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}
	}
}
