//
// System.Net.FtpWebResponse.cs
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

namespace System.Net
{
	public class FtpWebResponse : WebResponse
	{
		const string EXCEPTION_MESSAGE = "System.Net.FtpWebResponse is not supported on the current platform.";

		FtpWebResponse ()
		{
		}

		public override long ContentLength {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override WebHeaderCollection Headers {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override Uri ResponseUri {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public DateTime LastModified {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public string BannerMessage {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public string WelcomeMessage {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public string ExitMessage {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public FtpStatusCode StatusCode {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override bool SupportsHeaders {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public string StatusDescription {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override void Close ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override Stream GetResponseStream ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}
	}
}
