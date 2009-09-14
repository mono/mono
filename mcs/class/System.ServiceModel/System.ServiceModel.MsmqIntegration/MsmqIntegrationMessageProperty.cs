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
using MQMessage = System.Messaging.Message;
using MQMessageType = System.Messaging.MessageType;

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

		// not verified / consider queue property filter
		internal static MsmqIntegrationMessageProperty Get (MQMessage message)
		{
			if (message == null)
				throw new ArgumentNullException ("message");
			var p = new MsmqIntegrationMessageProperty ();
			p.Id = message.Id;
			p.Label = message.Label;
			p.CorrelationId = message.CorrelationId;
			p.Body = message.Body;
			p.Extension = message.Extension;
			p.SenderId = message.SenderId;

			p.DestinationQueue = CreateUriFromQueue (message.DestinationQueue);
			if (message.AdministrationQueue != null)
				p.AdministrationQueue = CreateUriFromQueue (message.AdministrationQueue);
			if (message.ResponseQueue != null)
				p.ResponseQueue = CreateUriFromQueue (message.ResponseQueue);

			// FIXME: check property filter in the queue
			p.AppSpecific = message.AppSpecific;
			p.ArrivedTime = message.ArrivedTime;
			p.Authenticated = message.Authenticated;
			p.Priority = message.Priority;
			p.SentTime = message.SentTime;
			p.TimeToReachQueue = message.TimeToReachQueue;

			switch (message.MessageType) {
			case MQMessageType.Acknowledgment:
				p.AcknowledgeType = message.AcknowledgeType;
				p.Acknowledgment = message.Acknowledgment;
				break;
			case MQMessageType.Normal:
			case MQMessageType.Report:
				// anything to do?
				break;
			}

			return p;
		}

		static Uri CreateUriFromQueue (MessageQueue queue)
		{
			// FIXME: implement
			throw new NotImplementedException ();
		}

		public AcknowledgeTypes? AcknowledgeType { get; set; }

		public Acknowledgment? Acknowledgment { get; private set; }
		
		public Uri AdministrationQueue { get; set; }

		public int? AppSpecific { get; set; }

		public DateTime? ArrivedTime { get; private set; }

		public bool? Authenticated { get; private set; }

		public object Body { get; set; }

		public int? BodyType { get; set; }

		public string CorrelationId { get; set; }

		public Uri DestinationQueue { get; private set; }

		public byte [] Extension { get; set; }

		public string Id { get; private set; }

		public string Label { get; set; }

		public MessageType? MessageType { get; private set; }

		public MessagePriority? Priority { get; set; }

		public Uri ResponseQueue { get; set; }

		public byte [] SenderId { get; private set; }

		public DateTime? SentTime { get; private set; }

		public TimeSpan? TimeToReachQueue { get; set; }
	}
}
