//
// System.Runtime.Remoting.Channels.CommonTransportKeys.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//         Lluis Sanchez Gual (lsg@ctv.es)
//
// 2002 (C) Copyright, Ximian, Inc.
//

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

namespace System.Runtime.Remoting.Channels
{
	public class CommonTransportKeys
	{
		public const string ConnectionId = "__ConnectionId";
		public const string IPAddress = "__IPAddress";
		public const string RequestUri = "__RequestUri";
		
		internal const string RequestVerb = "__RequestVerb";
		internal const string HttpVersion = "__HttpVersion";
		internal const string ContentType = "Content-Type";
		internal const string UserAgent = "User-Agent";
		internal const string Host = "Host";
		internal const string SoapAction = "SOAPAction";
		
		internal const string HttpStatusCode = "__HttpStatusCode";
		internal const string HttpReasonPhrase = "__HttpReasonPhrase";
		
		public CommonTransportKeys ()
		{
		}
	}
}
