//
// System.Runtime.Remoting.Channels.CommonTransportKeys.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//         Lluis Sanchez Gual (lsg@ctv.es)
//
// 2002 (C) Copyright, Ximian, Inc.
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
