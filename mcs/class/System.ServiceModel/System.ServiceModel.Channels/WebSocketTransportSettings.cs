// Authors:
//      Martin Baulig (martin.baulig@xamarin.com)
//
// Copyright 2012 Xamarin Inc. (http://www.xamarin.com)
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
#if NET_4_5
using System;

namespace System.ServiceModel.Channels {
	[MonoTODO]
	public sealed class WebSocketTransportSettings : IEquatable<WebSocketTransportSettings> {
		public WebSocketTransportSettings ()
		{
			throw new NotImplementedException ();
		}
		
		public const string BinaryEncoderTransferModeHeader = null;
		public const string BinaryMessageReceivedAction = "http://schemas.microsoft.com/2011/02/websockets/onbinarymessage";
		public const string ConnectionOpenedAction = null;
		public const string SoapContentTypeHeader = null;
		public const string TextMessageReceivedAction = "http://schemas.microsoft.com/2011/02/websockets/ontextmessage";

		public bool CreateNotificationOnConnection { get; set; }
		public bool DisablePayloadMasking { get; set; }
		public TimeSpan KeepAliveInterval { get; set; }
		public int MaxPendingConnections { get; set; }
		public int ReceiveBufferSize { get; set; }
		public int SendBufferSize { get; set; }
		public string SubProtocol { get; set; }
		public WebSocketTransportUsage TransportUsage { get; set; }

		public bool Equals (WebSocketTransportSettings other)
		{
			return other.CreateNotificationOnConnection == CreateNotificationOnConnection &&
				other.DisablePayloadMasking == DisablePayloadMasking &&
				other.KeepAliveInterval == KeepAliveInterval &&
				other.MaxPendingConnections == MaxPendingConnections &&
				other.ReceiveBufferSize == ReceiveBufferSize &&
				other.SendBufferSize == SendBufferSize &&
				other.SubProtocol == SubProtocol &&
				other.TransportUsage == TransportUsage;
		}
		
		
	}
}
#endif
