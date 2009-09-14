//
// MsmqMessage.cs
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

namespace System.ServiceModel.MsmqIntegration
{
	[MessageContract]
	public sealed class MsmqMessage<T>
	{
		MsmqIntegrationMessageProperty prop;

		public MsmqMessage (T body)
		{
			prop = new MsmqIntegrationMessageProperty ();
			Body = body;
		}

		public AcknowledgeTypes? AcknowledgeType {
			get { return prop.AcknowledgeType; }
			set { prop.AcknowledgeType = value; }
		}

		public Acknowledgment? Acknowledgment {
			get { return prop.Acknowledgment; }
		}
		
		public Uri AdministrationQueue {
			get { return prop.AdministrationQueue; }
			set { prop.AdministrationQueue = value; }
		}

		public int? AppSpecific {
			get { return prop.AppSpecific; }
			set { prop.AppSpecific = value; }
		}

		public DateTime? ArrivedTime {
			get { return prop.ArrivedTime; }
		}

		public bool? Authenticated {
			get { return prop.Authenticated; }
		}

		public T Body {
			get { return (T) prop.Body; }
			set { prop.Body = value; }
		}

		public int? BodyType {
			get { return prop.BodyType; }
			set { prop.BodyType = value; }
		}

		public string CorrelationId {
			get { return prop.CorrelationId; }
			set { prop.CorrelationId = value; }
		}

		public Uri DestinationQueue {
			get { return prop.DestinationQueue; }
		}

		public byte [] Extension {
			get { return prop.Extension; }
			set { prop.Extension = value; }
		}

		public string Id {
			get { return prop.Id; }
		}

		public string Label {
			get { return prop.Label; }
			set { prop.Label = value; }
		}

		public MessageType? MessageType {
			get { return prop.MessageType; }
		}

		public MessagePriority? Priority {
			get { return prop.Priority; }
			set { prop.Priority = value; }
		}

		public Uri ResponseQueue {
			get { return prop.ResponseQueue; }
			set { prop.ResponseQueue = value; }
		}

		public byte [] SenderId {
			get { return prop.SenderId; }
		}

		public DateTime? SentTime {
			get { return prop.SentTime; }
		}

		public TimeSpan? TimeToReachQueue {
			get { return prop.TimeToReachQueue; }
			set { prop.TimeToReachQueue = value; }
		}
	}
}
