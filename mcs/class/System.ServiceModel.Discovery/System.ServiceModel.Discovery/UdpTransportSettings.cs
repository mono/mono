//
// Author: Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Discovery
{
	public class UdpTransportSettings
	{
		internal UdpTransportSettings ()
		{
			DuplicateMessageHistoryLength = 4112;
			MaxBufferPoolSize = 0x80000;
			MaxMulticastRetransmitCount = 2;
			MaxPendingMessageCount = 32;
			MaxReceivedMessageSize = 0xFFE7;
			MaxUnicastRetransmitCount = 1;
			SocketReceiveBufferSize = 0x10000;
			TimeToLive = 1;
		}

		public int DuplicateMessageHistoryLength { get; set; }
		public long MaxBufferPoolSize { get; set; }
		public int MaxMulticastRetransmitCount { get; set; }
		public int MaxPendingMessageCount { get; set; }
		public long MaxReceivedMessageSize { get; set; }
		public int MaxUnicastRetransmitCount { get; set; }
		public string MulticastInterfaceId { get; set; }
		public int SocketReceiveBufferSize { get; set; }
		public int TimeToLive { get; set; }
	}
}
