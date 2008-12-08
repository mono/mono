//
// Test.Mono.Messaging.RabbitMQ
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
using System.Messaging;
using System.Reflection;
using System.Threading;
using System.Text.RegularExpressions;

using NUnit.Framework;

namespace MonoTests.Mono.Messaging.RabbitMQ
{
	[TestFixture]
	public class FailuresTest {

		[Test]
		[ExpectedException (typeof (MessageQueueException))]
		public void SendWithPathNotSet ()
		{
			MessageQueue q = new MessageQueue ();
			Message m = new Message ("foobar", new BinaryMessageFormatter ());
			
			q.Send (m);
		}
		
		[Test]
		[ExpectedException (typeof (MessageQueueException))]
		public void SendInTransactionWithPathNotSet ()
		{
			MessageQueue q = new MessageQueue ();
			Message m = new Message ("foobar", new BinaryMessageFormatter ());
			MessageQueueTransaction tx = new MessageQueueTransaction ();
			
			q.Send (m, tx);
		}
		
	}
}
