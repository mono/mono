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

using NUnit.Framework;

namespace MonoTests.System.Messaging
{
	[TestFixture]
	public class PeekTest {
		
		[Test]
		public void PeekMessage ()
		{
			String body = "foo-" + DateTime.Now.ToString ();
			Message s1 = new Message(body, new BinaryMessageFormatter());
			MessageQueue mq = MQUtil.GetQueue ();
			mq.Send (s1);
			
			Message r1 = mq.Peek ();
			Assert.AreEqual (body, r1.Body);
			
			Message r2 = mq.Receive ();
			Assert.AreEqual (body, r2.Body);
		}
		
		[Test]
		public void PeekMessageWithTimeout ()
		{
			String body = "foo-" + DateTime.Now.ToString();
			Message s1 = new Message(body, new BinaryMessageFormatter());
			MessageQueue mq = MQUtil.GetQueue();
			mq.Send (s1);
			
			Message r1 = mq.Peek (new TimeSpan (0, 0, 2));
			Assert.AreEqual (body, r1.Body);
			
			Message r2 = mq.Receive ();
			Assert.AreEqual (body, r2.Body);
		}
		
		[Test]
		[ExpectedException (typeof (MessageQueueException))]
		public void PeekNoMessageWithTimeout ()
		{
			MessageQueue mq = MQUtil.GetQueue();
			Message r1 = mq.Peek (new TimeSpan (0, 0, 2));
		}
		
		[Test]
		public void PeekById ()
		{
			String body = "Foo-" + DateTime.Now.ToString ();
			Message s1 = new Message (body, new BinaryMessageFormatter());
			MessageQueue q = MQUtil.GetQueue ();
			q.Send (s1);
			
			String id = s1.Id;
			try {
				Message r1 = q.PeekById (id);
				Assert.AreEqual (body, r1.Body, "Unable to PeekById correctly");
			} finally {
				q.Purge ();
			}
		}
		
		[Test]
		public void PeekByIdWithTimeout ()
		{
			String body = "Foo-" + DateTime.Now.ToString ();
			Message s1 = new Message (body, new BinaryMessageFormatter());
			MessageQueue q = MQUtil.GetQueue ();
			q.Send (s1);
			
			String id = s1.Id;
			try {
				Message r1 = q.PeekById (id, new TimeSpan (0, 0, 2));
				Assert.AreEqual (body, r1.Body, "Unable to PeekById correctly");
			} finally {
				q.Purge ();
			}
		}
		
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void PeekByIdNotFound ()
		{
			String body = "Foo-" + DateTime.Now.ToString ();
			Message s1 = new Message (body, new BinaryMessageFormatter());
			MessageQueue q = MQUtil.GetQueue ();
			q.Send (s1);
			
			String id = "fail!";
			
			try {
				Message r1 = q.PeekById (id);
			} finally {
				q.Purge ();
			}
		}
		
		[Test]
		public void PeekByCorrelationId ()
		{
			String correlationId = Guid.NewGuid () + "\\0";
			String body = "Foo-" + DateTime.Now.ToString ();
			Message s1 = new Message (body, new BinaryMessageFormatter());
			s1.CorrelationId = correlationId;
			MessageQueue q = MQUtil.GetQueue ();
			q.Formatter = new BinaryMessageFormatter ();
			q.Send (s1);
			
			try {
				Message r1 = q.PeekByCorrelationId (correlationId);
				Assert.AreEqual (body, r1.Body, "Unable to PeekByCorrelationId correctly");
			} finally {
				q.Purge ();
			}
		}
		
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void PeekByCorrelationIdNotFound ()
		{
			String body = "Foo-" + DateTime.Now.ToString ();
			Message s1 = new Message (body);
			String correlationId = Guid.NewGuid() + "\\0";
			MessageQueue q = MQUtil.GetQueue();
			q.Formatter = new BinaryMessageFormatter ();
			q.Send (s1);
			
			try {
				Message r1 = q.PeekByCorrelationId ("fail!");
			} finally {
				q.Purge ();
			}
		}
		
		[Test]
		public void PeekByCorrelationIdWithTimeout ()
		{
			String correlationId = Guid.NewGuid () + "\\0";
			String body = "Foo-" + DateTime.Now.ToString ();
			Message s1 = new Message (body, new BinaryMessageFormatter());
			s1.CorrelationId = correlationId;
			MessageQueue q = MQUtil.GetQueue ();
			q.Formatter = new BinaryMessageFormatter ();
			q.Send (s1);
			
			try {
				Message r1 = q.PeekByCorrelationId (correlationId, new TimeSpan (0, 0, 2));
				Assert.AreEqual (body, r1.Body, "Unable to PeekByCorrelationId correctly");
			} finally {
				q.Purge ();
			}
		}
	}
}
