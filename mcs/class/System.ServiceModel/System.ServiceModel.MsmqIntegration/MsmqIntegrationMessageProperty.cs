//
// MsmqIntegrationMessageProperty.cs
//
// Author: Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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
using System.Messaging;
using System.ServiceModel;
using System.ServiceModel.Channels;

using SMessage = System.ServiceModel.Channels.Message;

namespace System.ServiceModel.MsmqIntegration
{
	public sealed class MsmqIntegrationMessageProperty
	{
		public const string Name = "MsmqIntegrationMessageProperty";

		public MsmqIntegrationMessageProperty ()
		{
		}

		[MonoTODO]
		public static MsmqIntegrationMessageProperty Get (SMessage message)
		{
			throw new NotImplementedException ();
		}

		AcknowledgeTypes? ack_type;
		Acknowledgment? ack;
		Uri adm_queue, dst_queue, res_queue;
		int? app_specific, body_type;
		DateTime? arrived_time, sent_time;
		bool? authenticated;
		object body;
		string correlation_id, id, label;
		byte [] extension, sender_id;
		MessageType? msg_type;
		MessagePriority? priority;
		TimeSpan? ttrq;

		public AcknowledgeTypes? AcknowledgeType {
			get { return ack_type; }
			set { ack_type = value; }
		}

		public Acknowledgment? Acknowledgment {
			get { return ack; }
		}
		
		public Uri AdministrationQueue {
			get { return adm_queue; }
			set { adm_queue = value; }
		}

		public int? AppSpecific {
			get { return app_specific; }
			set { app_specific = value; }
		}

		public DateTime? ArrivedTime {
			get { return arrived_time; }
		}

		public bool? Authenticated {
			get { return authenticated; }
		}

		public object Body {
			get { return body; }
			set { body = value; }
		}

		public int? BodyType {
			get { return body_type; }
			set { body_type = value; }
		}

		public string CorrelationId {
			get { return correlation_id; }
			set { correlation_id = value; }
		}

		public Uri DestinationQueue {
			get { return dst_queue; }
		}

		public byte [] Extension {
			get { return extension; }
			set { extension = value; }
		}

		public string Id {
			get { return id; }
		}

		public string Label {
			get { return label; }
			set { label = value; }
		}

		public MessageType? MessageType {
			get { return msg_type; }
		}

		public MessagePriority? Priority {
			get { return priority; }
			set { priority = value; }
		}

		public Uri ResponseQueue {
			get { return res_queue; }
			set { res_queue = value; }
		}

		public byte [] SenderId {
			get { return sender_id; }
		}

		public DateTime? SentTime {
			get { return sent_time; }
		}

		public TimeSpan? TimeToReachQueue {
			get { return ttrq; }
			set { ttrq = value; }
		}
	}
}
