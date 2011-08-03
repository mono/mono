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



namespace Mono.Messaging.RabbitMQ {

	public class MessagingContextPool
	{
		private readonly CreateConnectionDelegate createConnectionDelegate;
		private readonly ConcurrentLinkedQueue<MessagingContext> pool = 
			new ConcurrentLinkedQueue<MessagingContext>();
		private readonly MessageFactory messageFactory;
		
		public MessagingContextPool (MessageFactory messageFactory, 
									 CreateConnectionDelegate createConnectionDelegate)
		{
			this.messageFactory = messageFactory;
			this.createConnectionDelegate = createConnectionDelegate;
		}
		
		public MessagingContext GetContext (string host)
		{
			MessagingContext context = pool.Dequeue ();
			if (context == null) {
				context = new MessagingContext (messageFactory, host, createConnectionDelegate);
				context.Pool = this;
			}
			context.Host = host;
			
			return context;
		}
		
		public void ReturnContext (MessagingContext context)
		{
			pool.Enqueue (context);
		}
	}
}
