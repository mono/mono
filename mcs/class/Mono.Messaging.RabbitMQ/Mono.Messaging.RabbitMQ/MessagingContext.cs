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
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Text;

using RabbitMQ.Client;
using RabbitMQ.Client.Content;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client.MessagePatterns;
using RabbitMQ.Util;

namespace Mono.Messaging.RabbitMQ {

	/// <summary>
	/// A context containing a connection and a model, holds state information
	/// regarding current messaging context.
	///
	/// Do not share across threads.
	/// </summary>
	public class MessagingContext : IMessagingContext {
		
		private readonly MessageFactory helper;
		private readonly CreateConnectionDelegate createConnection;
		
		private IConnection cn;
		private MessagingContextPool contextPool = null;
		
		private IModel model = null;
		private String host = null;
		
		private bool isDisposed = false;
		private Object syncObj = new Object ();
		
		public MessagingContext (MessageFactory factory, string host, CreateConnectionDelegate createConnection)
		{
			this.helper = factory;
			this.host = host;
			this.createConnection = createConnection;
		}
		
		public IConnection Connection {
			get {
				if (null == cn) {
					cn = createConnection (host);
				}
				
				return cn;
			}
		}
		
		public IModel Model {
			get {
				if (null == model) {
					model = Connection.CreateModel ();
				}
				return model;
			}
		}
		
		internal MessagingContextPool Pool {
			set { contextPool = value; }
			get { return contextPool; }
		}
		
		internal string Host {
			set { 
				if (host != value) {
					ResetConnection ();
					host = value;
				}
			}
			get { return host; }
		}
		
		public IMessage Receive (QueueReference qRef, TimeSpan timeout, IsMatch matcher, bool ack)
		{
			if (matcher == null)
			{
				return Receive (qRef, MessageFactory.TimeSpanToMillis (timeout), ack);
			}
			else
			{
				return ReceiveWithMatcher (qRef, MessageFactory.TimeSpanToMillis (timeout), matcher, ack);
			}
		}
		
		private IMessage Receive (QueueReference qRef, int timeout, bool ack)
		{
			IModel _model = Model;
			
			string finalName = _model.QueueDeclare (qRef.Queue, false);
			using (Subscription sub = new Subscription (_model, finalName)) {
				BasicDeliverEventArgs result;
				if (sub.Next (timeout, out result)) {
					IMessage m = helper.ReadMessage (qRef, result);
					if (ack)
						sub.Ack (result);
					return m;
				} else {
					throw new MonoMessagingException ("No Message Available");
				}
			}
		}
		
		private IMessage ReceiveWithMatcher (QueueReference qRef, int timeout, IsMatch matcher, bool ack)
		{
			IModel _model = Model;
			
			string finalName = _model.QueueDeclare (qRef.Queue, false);
			using (Subscription sub = new Subscription (_model, finalName)) {
				BasicDeliverEventArgs result;
				while (sub.Next (timeout, out result)) {
					
					if (matcher (result)) {
						IMessage m = helper.ReadMessage (qRef, result);
						if (ack)
							sub.Ack (result);
						return m;
					}
				}
				
				throw new MessageUnavailableException ("Message not available");
			}
		}
		
		public void Send (QueueReference qRef, IMessage message)
		{
			IModel _model = Model;
			
			string finalName = _model.QueueDeclare (qRef.Queue, false);
			IMessageBuilder mb = helper.WriteMessage (_model, message);
			
			model.BasicPublish ("", finalName,
				(IBasicProperties) mb.GetContentHeader (),
				mb.GetContentBody ());
		}
		
		public void Delete (QueueReference qRef)
		{
			Model.QueueDelete (qRef.Queue, false, false, false);
		}
		
		public void Purge (QueueReference qRef)
		{
			Model.QueuePurge (qRef.Queue, false);
		}
		
		public void Dispose ()
		{
			if (model != null) {
				model.Close ();
				model = null;
			}
			
			if (null != contextPool) {
				contextPool.ReturnContext (this);
				return;
			}
			
			Dispose (true);
			GC.SuppressFinalize (this);
		}
		
		private void ResetConnection ()
		{
			if (cn != null)
				cn.Dispose ();
		}
		
		protected virtual void Dispose (bool disposing)
		{
			lock (syncObj) {
				if (!isDisposed && disposing) {
					isDisposed = true;
					ResetConnection ();
				}
			}
		}		
	}
}
