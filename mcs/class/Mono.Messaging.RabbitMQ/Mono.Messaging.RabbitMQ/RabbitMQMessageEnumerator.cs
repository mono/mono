//
// Mono.Messaging.RabbitMQ
//
// Authors:
//	  Michael Barker (mike@middlesoft.co.uk)
//
// (C) 2008 Michael Barker
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

using System;

using Mono.Messaging;

using RabbitMQ.Client;
using RabbitMQ.Client.Content;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client.MessagePatterns;
using RabbitMQ.Util;

namespace Mono.Messaging.RabbitMQ {

	public class RabbitMQMessageEnumerator : IMessageEnumerator {
		
		private readonly MessageFactory helper;
		private readonly QueueReference qRef;
		private IConnection cn = null;
		private BasicDeliverEventArgs current = null;
		private IModel model = null;
		private Subscription subscription = null;
		
		public RabbitMQMessageEnumerator (MessageFactory helper,
		                                  QueueReference qRef) {
			this.helper = helper;
			this.qRef = qRef;
		}
		
		public IMessage Current { 
			get {
				if (current == null)
					throw new InvalidOperationException ();
				
				return CreateMessage (current);
			}
		}
		
		public IntPtr CursorHandle {
			get { throw new NotImplementedException (); }
		}
		
		public void Close ()
		{
			if (subscription != null) {
				subscription.Close ();
				subscription = null;
			}
			
			if (model != null) {
				model.Dispose ();
				model = null;
			}
			
			if (cn != null) {
				cn.Dispose ();
				cn = null;
			}
		}

		public void Dispose (bool disposing)
		{
		}
		
		public void Dispose ()
		{
			Close ();
		}
		
		public void Reset ()
		{
			Close ();
		}
		
		private IModel Model {
			get {
				if (cn == null) {
					ConnectionFactory cf = new ConnectionFactory ();
					cf.Address = qRef.Host;
					cn = cf.CreateConnection ();
				}
				
				if (model == null) {
					model = cn.CreateModel ();
				}
				
				return model;
			}
		}
		
		private Subscription Subscription {
			get {
				if (subscription == null) {
					IModel ch = Model;
					
					string finalName = ch.QueueDeclare (qRef.Queue, false);
					
					subscription = new Subscription (ch, finalName);
				}
				
				return subscription;
			}
		}

		public bool MoveNext ()
		{
			Subscription sub = Subscription;
			return sub.Next (500, out current);
		}
		
		public bool MoveNext (TimeSpan timeout)
		{
			int to = MessageFactory.TimeSpanToInt32 (timeout);
			return Subscription.Next (to, out current);
		}

		public IMessage RemoveCurrent ()
		{
			if (current == null)
				throw new InvalidOperationException ();
			
			IMessage msg = CreateMessage (current);
			Subscription.Ack (current);
			return msg;
		}
		
		public IMessage RemoveCurrent (IMessageQueueTransaction transaction)
		{
			throw new NotSupportedException ("Unable to remove messages within a transaction");
		}
		
		public IMessage RemoveCurrent (MessageQueueTransactionType transactionType)
		{
			throw new NotSupportedException ("Unable to remove messages within a transaction");
		}
		
		public IMessage RemoveCurrent (TimeSpan timeout)
		{
			// Timeout makes no sense for this implementation, so we just work 
			// the same as the non-timeout based one. 
			
			if (current == null)
				throw new InvalidOperationException ();
			
			IMessage msg = CreateMessage (current);
			Subscription.Ack (current);
			return msg;
		}
		
		public IMessage RemoveCurrent (TimeSpan timeout, IMessageQueueTransaction transaction)
		{
			throw new NotImplementedException ();
		}
		
		public IMessage RemoveCurrent (TimeSpan timeout, MessageQueueTransactionType transactionType)
		{
			throw new NotImplementedException ();
		}
		
		private IMessage CreateMessage (BasicDeliverEventArgs result)
		{
			return helper.ReadMessage (qRef, result);
		}

	}
}
