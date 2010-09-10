//
// Author: Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
#if NET_4_0
using System;
using System.ComponentModel;
using System.Configuration;
using System.ServiceModel.Configuration;

namespace System.ServiceModel.Discovery.Configuration
{
	public sealed class UdpTransportSettingsElement : ConfigurationElement
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty duplicate_message_history_length, max_buffer_pool_size, max_multicast_retransmit_count, max_pending_message_count, max_received_message_size, max_unicast_retransmit_count, multicast_interface_id,socket_receive_buffer_size, ttl;
		
		static UdpTransportSettingsElement ()
		{
			duplicate_message_history_length = new ConfigurationProperty ("duplicateMessageHistoryLength", typeof (int), 4112, null, new IntegerValidator (0, int.MaxValue), ConfigurationPropertyOptions.None);
			max_buffer_pool_size = new ConfigurationProperty ("maxBufferPoolSize", typeof (long), 0x80000, null, new LongValidator (0, long.MaxValue), ConfigurationPropertyOptions.None);
			max_multicast_retransmit_count = new ConfigurationProperty ("maxMulticastRetransmitCount", typeof (int), 2, null, new IntegerValidator (0, int.MaxValue), ConfigurationPropertyOptions.None);
			max_pending_message_count = new ConfigurationProperty ("maxPendingMessageCount", typeof (int), 32, null, new IntegerValidator (0, int.MaxValue), ConfigurationPropertyOptions.None);
			max_received_message_size = new ConfigurationProperty ("maxReceivedMessageSize", typeof (long), 0xFFE7, null, new LongValidator (0, long.MaxValue), ConfigurationPropertyOptions.None);
			max_unicast_retransmit_count = new ConfigurationProperty ("maxUnicastRetransmitCount", typeof (long), 1, null, new IntegerValidator (0, int.MaxValue), ConfigurationPropertyOptions.None);
			multicast_interface_id = new ConfigurationProperty ("multicastInterfaceId", typeof (string), null, null, null, ConfigurationPropertyOptions.None);
			ttl = new ConfigurationProperty ("timeToLive", typeof (int), 1, null, new IntegerValidator (0, int.MaxValue), ConfigurationPropertyOptions.None);
			properties = new ConfigurationPropertyCollection ();
			ConfigurationProperty [] props = {duplicate_message_history_length, max_buffer_pool_size, max_multicast_retransmit_count, max_pending_message_count, max_received_message_size, max_unicast_retransmit_count, multicast_interface_id, ttl};
			foreach (var cp in props)
				properties.Add (cp);
		}

		public UdpTransportSettingsElement ()
		{
		}

		[ConfigurationProperty ("duplicateMessageHistoryLength", DefaultValue = 4112)]
		[IntegerValidator (MinValue = 0, MaxValue = int.MaxValue)]
		public int DuplicateMessageHistoryLength {
			get { return (int) base [duplicate_message_history_length]; }
			set { base [duplicate_message_history_length] = value; }
		}

		[LongValidator (MinValue = 0, MaxValue = long.MaxValue)]
		[ConfigurationProperty ("maxBufferPoolSize", DefaultValue = 0x80000)]
		public long MaxBufferPoolSize {
			get { return (long) base [max_buffer_pool_size]; }
			set { base [max_buffer_pool_size] = value; }
		}

		[ConfigurationProperty ("maxMulticastRetransmitCount", DefaultValue = 2)]
		[IntegerValidator (MinValue = 0, MaxValue = int.MaxValue)]
		public int MaxMulticastRetransmitCount {
			get { return (int) base [max_multicast_retransmit_count]; }
			set { base [max_multicast_retransmit_count] = value; }
		}

		[ConfigurationProperty ("maxPendingMessageCount", DefaultValue = 32)]
		[IntegerValidator (MinValue = 0, MaxValue = int.MaxValue)]
		public int MaxPendingMessageCount {
			get { return (int) base [max_pending_message_count]; }
			set { base [max_pending_message_count] = value; }
		}

		[LongValidator (MinValue = 0, MaxValue = long.MaxValue)]
		[ConfigurationProperty ("maxReceivedMessageSize", DefaultValue = 0xFFE7)]
		public long MaxReceivedMessageSize {
			get { return (long) base [max_received_message_size]; }
			set { base [max_received_message_size] = value; }
		}

		[ConfigurationProperty ("maxUnicastRetransmitCount", DefaultValue = 1)]
		[IntegerValidator (MinValue = 0, MaxValue = int.MaxValue)]
		public int MaxUnicastRetransmitCount {
			get { return (int) base [max_unicast_retransmit_count]; }
			set { base [max_unicast_retransmit_count] = value; }
		}

		[ConfigurationProperty ("multicastInterfaceId")]
		public string MulticastInterfaceId {
			get { return (string) base [multicast_interface_id]; }
			set { base [multicast_interface_id] = value; }
		}

		[ConfigurationProperty ("socketReceiveBufferSize", DefaultValue = 0x10000)]
		[IntegerValidator (MinValue = 0, MaxValue = int.MaxValue)]
		public int SocketReceiveBufferSize {
			get { return (int) base [socket_receive_buffer_size]; }
			set { base [socket_receive_buffer_size] = value; }
		}

		[IntegerValidator (MinValue = 0, MaxValue = int.MaxValue)]
		[ConfigurationProperty ("timeToLive", DefaultValue = 1)]
		public int TimeToLive {
			get { return (int) base [ttl]; }
			set { base [ttl] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		internal void ApplyConfiguration (UdpTransportSettings t)
		{
			t.DuplicateMessageHistoryLength = DuplicateMessageHistoryLength;
			t.MaxBufferPoolSize = MaxBufferPoolSize;
			t.MaxMulticastRetransmitCount = MaxMulticastRetransmitCount;
			t.MaxPendingMessageCount = MaxPendingMessageCount;
			t.MaxReceivedMessageSize = MaxReceivedMessageSize;
			t.MaxUnicastRetransmitCount = MaxUnicastRetransmitCount;
			t.MulticastInterfaceId = MulticastInterfaceId;
			t.SocketReceiveBufferSize = SocketReceiveBufferSize;
			t.TimeToLive = TimeToLive;
		}

		internal void InitializeFrom (UdpTransportSettings t)
		{
			DuplicateMessageHistoryLength = t.DuplicateMessageHistoryLength;
			MaxBufferPoolSize = t.MaxBufferPoolSize;
			MaxMulticastRetransmitCount = t.MaxMulticastRetransmitCount;
			MaxPendingMessageCount = t.MaxPendingMessageCount;
			MaxReceivedMessageSize = t.MaxReceivedMessageSize;
			MaxUnicastRetransmitCount = t.MaxUnicastRetransmitCount;
			MulticastInterfaceId = t.MulticastInterfaceId;
			SocketReceiveBufferSize = t.SocketReceiveBufferSize;
			TimeToLive = t.TimeToLive;
		}
	}
}

#endif
