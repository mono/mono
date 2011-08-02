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
	[Ignore]
	public class MessageEnumeratorTest {
		
		string qName;
		
		[SetUp]
		public void SetUp()
		{
			qName = MQUtil.CreateQueueName ();
		}
		
		private void SendMessage (string s) {
			MessageQueue mq = MQUtil.GetQueue (qName);
			using (mq) {
				Message m = new Message (s, new BinaryMessageFormatter ());
				m.CorrelationId = Guid.NewGuid () + "\\0";
				mq.Send (m);
			}
		}

		[Test]
		public void GetMessagesInOrder ()
		{
			SendMessage ("message 1");
			SendMessage ("message 2");
			SendMessage ("message 3");
			SendMessage ("message 4");
			
			MessageQueue mq0 = MQUtil.GetQueue (qName);
			MessageEnumerator me0 = mq0.GetMessageEnumerator ();
			
			me0.MoveNext ();
			Console.WriteLine("Message0 {0}", me0.Current.Body);
			me0.MoveNext ();
			Console.WriteLine("Message0 {0}", me0.Current.Body);
			me0.MoveNext ();
			Console.WriteLine("Message0 {0}", me0.Current.Body);
			me0.MoveNext ();
			Console.WriteLine("Message0 {0}", me0.Current.Body);
			
			me0.Dispose ();
			mq0.Dispose ();
			
			MessageQueue mq1 = MQUtil.GetQueue (qName);
			MessageEnumerator me1 = mq1.GetMessageEnumerator ();
			
			me1.MoveNext ();
			Console.WriteLine("Message1 {0}", me1.Current.Body);
			me1.MoveNext ();
			Console.WriteLine("Message1 {0}", me1.Current.Body);
			me1.MoveNext ();
			Console.WriteLine("Message1 {0}", me1.Current.Body);
			me1.MoveNext ();
			Console.WriteLine("Message1 {0}", me1.Current.Body);
			
			Message m1 = me1.Current;
			m1.Formatter = new BinaryMessageFormatter ();
			Assert.AreEqual ("message 4", (String) m1.Body, "body incorrect");
			
			mq1.Purge ();
			MessageQueue.Delete (qName);
		}
		
		[Test]
		public void RemoveMessage ()
		{
			SendMessage ("message 1");
			SendMessage ("message 2");
			SendMessage ("message 3");
			SendMessage ("message 4");
			
			MessageQueue mq0 = MQUtil.GetQueue (qName);
			MessageEnumerator me0 = mq0.GetMessageEnumerator ();
			
			me0.MoveNext ();
			me0.MoveNext ();
			me0.MoveNext ();
			
			Message m0 = me0.RemoveCurrent ();
			
			me0.MoveNext ();
			
			me0.Dispose ();
			mq0.Dispose ();
			
			MessageQueue mq1 = MQUtil.GetQueue (qName);
			MessageEnumerator me1 = mq1.GetMessageEnumerator ();
			
			me1.MoveNext();
			me1.MoveNext();
			me1.MoveNext();
			
			Message m1 = me1.Current;
			m1.Formatter = new BinaryMessageFormatter ();
			Assert.AreEqual ("message 4", (String) m1.Body, "body incorrect");
			
			mq1.Purge ();
			MessageQueue.Delete (qName);
		}
		
		[Test]
		public void RemoveMessageWithTimeout ()
		{
			SendMessage ("message 1");
			SendMessage ("message 2");
			SendMessage ("message 3");
			SendMessage ("message 4");
			
			MessageQueue mq0 = MQUtil.GetQueue (qName);
			MessageEnumerator me0 = mq0.GetMessageEnumerator ();
			
			TimeSpan ts = new TimeSpan (0, 0, 2);
			
			me0.MoveNext (ts);
			me0.MoveNext (ts);
			me0.MoveNext (ts);
			
			Message m0 = me0.RemoveCurrent (ts);
			
			me0.MoveNext (ts);
			
			me0.Dispose ();
			mq0.Dispose ();
			
			MessageQueue mq1 = MQUtil.GetQueue (qName);
			MessageEnumerator me1 = mq1.GetMessageEnumerator ();
			
			me1.MoveNext (ts);
			me1.MoveNext (ts);
			me1.MoveNext (ts);
			
			Message m1 = me1.Current;
			m1.Formatter = new BinaryMessageFormatter ();
			Assert.AreEqual ("message 4", (String) m1.Body, "body incorrect");
			
			mq1.Purge ();
			MessageQueue.Delete (qName);
		}
		
		//[Test]
		// Not supported with AMQP
		public void RemoveMessageWithTx ()
		{
			MessageQueue q = MQUtil.GetQueue (qName);
			
			q.Formatter = new BinaryMessageFormatter ();
			q.Send ("foo1");
			q.Send ("foo2");
			
			MessageEnumerator me1 = q.GetMessageEnumerator ();
			MessageQueueTransaction tx = new MessageQueueTransaction ();
			me1.MoveNext ();
			Message m1 = me1.Current;
			me1.RemoveCurrent (tx);
			tx.Commit ();
			me1.Close ();
			
			MessageEnumerator me2 = q.GetMessageEnumerator ();
			Assert.IsTrue (me1.MoveNext ());
			me2.RemoveCurrent ();
			Assert.IsFalse (me2.MoveNext ());
		}
	}
}
