//
// WebSocketReceiveResult.cs
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


using System;
using System.Security.Principal;
using System.Runtime.CompilerServices;

namespace System.Net.WebSockets
{
	public class WebSocketReceiveResult
	{
		public WebSocketReceiveResult (int count, WebSocketMessageType messageType, bool endOfMessage)
		     : this (count, messageType, endOfMessage, null, null)
		{
		}

		public WebSocketReceiveResult (int count,
		                               WebSocketMessageType messageType,
		                               bool endOfMessage,
		                               WebSocketCloseStatus? closeStatus,
		                               string closeStatusDescription)
		{
			MessageType = messageType;
			CloseStatus = closeStatus;
			CloseStatusDescription = closeStatusDescription;
			Count = count;
			EndOfMessage = endOfMessage;
		}

		public WebSocketCloseStatus? CloseStatus {
			get;
			private set;
		}

		public string CloseStatusDescription {
			get;
			private set;
		}

		public int Count {
			get;
			private set;
		}

		public bool EndOfMessage {
			get;
			private set;
		}

		public WebSocketMessageType MessageType {
			get;
			private set;
		}
	}
}

