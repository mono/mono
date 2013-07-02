//
// WebSocketException.cs
//
// Authors:
//    Jérémie Laval <jeremie dot laval at xamarin dot com>
//
// Copyright 2013 Xamarin Inc (http://www.xamarin.com).
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
//
//

#if NET_4_5

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.Net.WebSockets
{
	public sealed class WebSocketException : Win32Exception
	{
		public WebSocketException ()
		{
			
		}

		public WebSocketException (int nativeError) : base (nativeError)
		{
			
		}

		public WebSocketException (string message) : base (message)
		{
			
		}

		public WebSocketException (WebSocketError error)
		{
			WebSocketErrorCode = error;
		}

		public WebSocketException (int nativeError, Exception innerException)
		{
			
		}

		public WebSocketException (int nativeError, string message) : base (nativeError, message)
		{
			
		}

		public WebSocketException (string message, Exception innerException) : base (message, innerException)
		{
			
		}

		public WebSocketException (WebSocketError error, Exception innerException)
		{
			WebSocketErrorCode = error;
		}

		public WebSocketException (WebSocketError error, int nativeError) : base (nativeError)
		{
			WebSocketErrorCode = error;
		}

		public WebSocketException (WebSocketError error, string message) : base (message)
		{
			WebSocketErrorCode = error;
		}

		public WebSocketException (WebSocketError error, int nativeError, Exception innerException) : base (nativeError)
		{
			WebSocketErrorCode = error;
		}

		public WebSocketException (WebSocketError error, int nativeError, string message) : base (nativeError, message)
		{
			WebSocketErrorCode = error;
		}

		public WebSocketException (WebSocketError error, string message, Exception innerException)
		{
			WebSocketErrorCode = error;
		}

		public WebSocketException (WebSocketError error, int nativeError, string message, Exception innerException) : base (nativeError, message)
		{
			WebSocketErrorCode = error;
		}

		public WebSocketError WebSocketErrorCode {
			get;
			private set;
		}
	}
}

#endif
