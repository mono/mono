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

namespace MonoTests.System.Messaging
{
	[TestFixture]
	public class TransactionMessageTest {
		
		[Test]
		public void Send2WithTransaction ()
		{
			Message sent1 = new Message ("Message 1", new BinaryMessageFormatter ());
			Message sent2 = new Message ("Message 2", new BinaryMessageFormatter ());
			MessageQueue mq = MQUtil.GetQueue (MQUtil.CreateQueueName (), true);
			mq.MessageReadPropertyFilter.SetAll ();
			Assert.IsTrue (mq.Transactional, "Message Queue should be transactional");
			using (MessageQueueTransaction tx = new MessageQueueTransaction ()) {
				tx.Begin ();
				
				mq.Send (sent1, tx);
				mq.Send (sent2, tx);
				
				tx.Commit ();
			}
			
			Message received1 = mq.Receive ();
			Assert.IsNotNull (received1.TransactionId, "TransactionId not set");
			Message received2 = mq.Receive ();
			Assert.IsNotNull (received2.TransactionId, "TransactionId not set");
			
			Assert.AreEqual (received1.TransactionId, received2.TransactionId, "Messages have differing TransactionIds");
			Assert.IsTrue (received1.TransactionId.Length > 1);
			Assert.AreEqual (sent1.Body, received1.Body, "Message 1 not delivered correctly");
			Assert.AreEqual (sent2.Body, received2.Body, "Message 2 not delivered correctly");
		}
		
		[Test]
		public void Send2WithLabelWithTransaction ()
		{
			String label1 = "label1";
			String label2 = "label2";
			Message sent1 = new Message ("Message 1", new BinaryMessageFormatter ());
			Message sent2 = new Message ("Message 2", new BinaryMessageFormatter ());
			MessageQueue mq = MQUtil.GetQueue (MQUtil.CreateQueueName (), true);
			mq.MessageReadPropertyFilter.SetAll ();
			Assert.IsTrue(mq.Transactional, "Message Queue should be transactional");
			using (MessageQueueTransaction tx = new MessageQueueTransaction ()) {
				tx.Begin ();
				
				mq.Send (sent1, label1, tx);
				mq.Send (sent2, label2, tx);
				
				tx.Commit ();
				
				Message received1 = mq.Receive ();
				Assert.IsNotNull (received1.TransactionId, "TransactionId not set");
				Message received2 = mq.Receive ();
				Assert.IsNotNull (received2.TransactionId, "TransactionId not set");
				
				Assert.AreEqual (received1.TransactionId, received2.TransactionId, "Messages have differing TransactionIds");
				Assert.IsTrue (received1.TransactionId.Length > 1);
				Assert.AreEqual (sent1.Body, received1.Body, "Message 1 not delivered correctly");
				Assert.AreEqual (sent2.Body, received2.Body, "Message 2 not delivered correctly");
				Assert.AreEqual (label1, received1.Label, "Label 1 not passed correctly");
				Assert.AreEqual (label2, received2.Label, "Label 2 not passed correctly");
			}
		}
				
		[Test]
		[ExpectedException (typeof (MessageQueueException))]
		public void Send2WithTransactionAbort ()
		{
			Message sent1 = new Message ("Message 1", new BinaryMessageFormatter ());
			Message sent2 = new Message ("Message 2", new BinaryMessageFormatter ());
			MessageQueue mq = MQUtil.GetQueue (MQUtil.CreateQueueName (), true);
			mq.MessageReadPropertyFilter.SetAll ();
			Assert.IsTrue(mq.Transactional, "Message Queue should be transactional");
			using (MessageQueueTransaction tx = new MessageQueueTransaction ()) {
				tx.Begin ();
				
				mq.Send (sent1, tx);
				mq.Send (sent2, tx);
				
				tx.Abort ();
				mq.Receive (new TimeSpan (0, 0, 2));
			}
		}
		
		[Test]
		public void ReceiveWithTransaction ()
		{
			String body = "Message 4";
			Message sent1 = new Message (body, new BinaryMessageFormatter ());
			MessageQueue mq = MQUtil.GetQueue (MQUtil.CreateQueueName (), true);
			Assert.IsTrue (mq.Transactional, "Message Queue should be transactional");
			mq.Send (sent1, MessageQueueTransactionType.Single);
			
			using (MessageQueueTransaction tx = new MessageQueueTransaction ()) {
				tx.Begin ();
				
				Message received1 = mq.Receive (tx);
				
				tx.Commit ();
				
				Assert.AreEqual (body, received1.Body);
			}
		}
		
		[Test]
		public void ReceiveWithTransactionAbort ()
		{
			String body = "foo-" + DateTime.Now.ToString ();
			Message sent1 = new Message (body, new BinaryMessageFormatter ());
			MessageQueue mq = MQUtil.GetQueue (MQUtil.CreateQueueName (), true);
			Assert.IsTrue (mq.Transactional, "Message Queue should be transactional");
			mq.Send (sent1, MessageQueueTransactionType.Single);
			
			using (MessageQueueTransaction tx = new MessageQueueTransaction ()) {
				tx.Begin ();
				
				Message received1 = mq.Receive (tx);
				
				tx.Abort ();
			}
			
			Message received2 = mq.Receive ();
			Assert.AreEqual (body, received2.Body);
		}
		
		[Test]
		public void ReceiveWithTransactionType ()
		{
			String body = "foo-" + DateTime.Now.ToString ();
			Message sent1 = new Message (body, new BinaryMessageFormatter ());
			MessageQueue mq = MQUtil.GetQueue (MQUtil.CreateQueueName (), true);
			Assert.IsTrue (mq.Transactional, "Message Queue should be transactional");
			mq.Send (sent1, MessageQueueTransactionType.Single);
			
			Message received1 = mq.Receive (MessageQueueTransactionType.Single);
			
			Assert.AreEqual (body, received1.Body);
		}
		
		[Test]
		public void SendWithTransactionType ()
		{
			Message sent1 = new Message ("Message 1");
			MessageQueue mq = MQUtil.GetQueue (MQUtil.CreateQueueName (), true);
			mq.MessageReadPropertyFilter.SetAll();
			mq.Send (sent1, MessageQueueTransactionType.Single);
			
			Message received1 = mq.Receive ();
			Assert.IsNotNull (received1.TransactionId, "TransactionId not set");
		}
		
		[Test]
		public void SendWithTransactionTypeAndLabel ()
		{
			Message sent1 = new Message ("Message 1");
			MessageQueue mq = MQUtil.GetQueue (MQUtil.CreateQueueName (), true);
			mq.MessageReadPropertyFilter.SetAll();
			String label = "mylabel";
			
			mq.Send (sent1, label, MessageQueueTransactionType.Single);
			
			Message received1 = mq.Receive ();
			Assert.IsNotNull (received1.TransactionId, "TransactionId not set");
			Assert.AreEqual (label, received1.Label, "Label not set");
		}

		[Test]
		public void ReceiveByIdWithTransaction ()
		{
			String body = "Message 4";
			Message sent1 = new Message (body, new BinaryMessageFormatter ());
			MessageQueue mq = MQUtil.GetQueue (MQUtil.CreateQueueName (), true);
			Assert.IsTrue (mq.Transactional, "Message Queue should be transactional");
			mq.Send (sent1, MessageQueueTransactionType.Single);
			string id = sent1.Id;
			
			using (MessageQueueTransaction tx = new MessageQueueTransaction ()) {
				tx.Begin ();
				
				Message received1 = mq.ReceiveById (id, tx);
				
				tx.Commit ();
				
				Assert.AreEqual (body, received1.Body);
			}
		}
		
		[Test]
		public void ReceiveByIdWithTransactionAbort ()
		{
			String body = "foo-" + DateTime.Now.ToString ();
			Message sent1 = new Message (body, new BinaryMessageFormatter ());
			MessageQueue mq = MQUtil.GetQueue (MQUtil.CreateQueueName (), true);
			Assert.IsTrue (mq.Transactional, "Message Queue should be transactional");
			mq.Send (sent1, MessageQueueTransactionType.Single);
			string id = sent1.Id;
			
			using (MessageQueueTransaction tx = new MessageQueueTransaction ()) {
				tx.Begin ();
				
				Message received1 = mq.ReceiveById (id, tx);
				
				tx.Abort ();
			}
			
			Message received2 = mq.Receive ();
			Assert.AreEqual (body, received2.Body);
		}
		
		[Test]
		public void ReceiveByIdWithTransactionType ()
		{
			String body = "Message 4";
			Message sent1 = new Message (body, new BinaryMessageFormatter ());
			MessageQueue mq = MQUtil.GetQueue (MQUtil.CreateQueueName (), true);
			Assert.IsTrue (mq.Transactional, "Message Queue should be transactional");
			mq.Send (sent1, MessageQueueTransactionType.Single);
			string id = sent1.Id;

			Message received1 = mq.ReceiveById (id, MessageQueueTransactionType.Single);
			Assert.AreEqual (body, received1.Body);
		}

		[Test]
		public void ReceiveByCorrelationIdWithTransaction ()
		{
			string correlationId = Guid.NewGuid() + "\\0";
			String body = "Message 4";
			Message sent1 = new Message (body, new BinaryMessageFormatter ());
			sent1.CorrelationId = correlationId;
			MessageQueue mq = MQUtil.GetQueue (MQUtil.CreateQueueName (), true);
			Assert.IsTrue (mq.Transactional, "Message Queue should be transactional");
			mq.Send (sent1, MessageQueueTransactionType.Single);
			string id = sent1.Id;
			
			using (MessageQueueTransaction tx = new MessageQueueTransaction ()) {
				tx.Begin ();

				Message received1 = mq.ReceiveByCorrelationId(correlationId, tx);
				
				tx.Commit ();
				
				Assert.AreEqual (body, received1.Body);
			}
		}
		
		[Test]
		public void ReceiveByCorrelationIdWithTransactionAbort ()
		{
			string correlationId = Guid.NewGuid() + "\\0";
			String body = "foo-" + DateTime.Now.ToString();
			Message sent1 = new Message (body, new BinaryMessageFormatter ());
			sent1.CorrelationId = correlationId;
			MessageQueue mq = MQUtil.GetQueue (MQUtil.CreateQueueName (), true);
			Assert.IsTrue (mq.Transactional, "Message Queue should be transactional");
			mq.Send (sent1, MessageQueueTransactionType.Single);
			string id = sent1.Id;
			
			using (MessageQueueTransaction tx = new MessageQueueTransaction ()) {
				tx.Begin ();
				
				Message received1 = mq.ReceiveByCorrelationId (correlationId, tx);
				
				tx.Abort ();
			}
			
			Message received2 = mq.Receive ();
			Assert.AreEqual (body, received2.Body);
		}

		[Test]
		public void ReceiveByCorrelationIdWithTransactionType ()
		{
			string correlationId = Guid.NewGuid() + "\\0";
			String body = "Message 10";
			Message sent1 = new Message (body, new BinaryMessageFormatter ());
			sent1.CorrelationId = correlationId;
			MessageQueue mq = MQUtil.GetQueue (MQUtil.CreateQueueName (), true);
			mq.Formatter = new BinaryMessageFormatter ();
			Assert.IsTrue (mq.Transactional, "Message Queue should be transactional");
			mq.Send (sent1, MessageQueueTransactionType.Single);
			string id = sent1.Id;
			
			Message received1 = mq.ReceiveByCorrelationId (correlationId, MessageQueueTransactionType.Single);
			Assert.AreEqual (body, received1.Body);
		}

		[Test]
		public void ReceiveWithTransactionAndTimeout ()
		{
			String body = "Message 11";
			Message sent1 = new Message (body, new BinaryMessageFormatter ());
			MessageQueue mq = MQUtil.GetQueue (MQUtil.CreateQueueName (), true);
			mq.Formatter = new BinaryMessageFormatter ();
			Assert.IsTrue (mq.Transactional, "Message Queue should be transactional");
			mq.Send (sent1, MessageQueueTransactionType.Single);
			
			using (MessageQueueTransaction tx = new MessageQueueTransaction ()) {
				tx.Begin ();
				
				Message received1 = mq.Receive (new TimeSpan (0, 0, 2), tx);
				
				tx.Commit ();
				
				Assert.AreEqual (body, received1.Body);
			}
		}

		[Test]
		public void ReceiveWithTransactionAndTimeoutAndAbort ()
		{
			String body = "foo-" + DateTime.Now.ToString ();
			Message sent1 = new Message (body, new BinaryMessageFormatter ());
			MessageQueue mq = MQUtil.GetQueue (MQUtil.CreateQueueName (), true);
			mq.Formatter = new BinaryMessageFormatter ();
			Assert.IsTrue (mq.Transactional, "Message Queue should be transactional");
			mq.Send (sent1, MessageQueueTransactionType.Single);
			
			using (MessageQueueTransaction tx = new MessageQueueTransaction ()) {
				tx.Begin ();
				
				Message received1 = mq.Receive (new TimeSpan (0, 0, 2), tx);
				
				tx.Abort ();
			}
			
			Message received2 = mq.Receive ();
			Assert.AreEqual (body, received2.Body);
		}
		
		[Test]
		public void ReceiveWithTransactionTypeAndTimeout ()
		{
			String body = "foo-" + DateTime.Now.ToString ();
			Message sent1 = new Message (body, new BinaryMessageFormatter ());
			MessageQueue mq = MQUtil.GetQueue (MQUtil.CreateQueueName (), true);
			mq.Formatter = new BinaryMessageFormatter ();
			Assert.IsTrue (mq.Transactional, "Message Queue should be transactional");
			mq.Send (sent1, MessageQueueTransactionType.Single);
			
			Message received1 = mq.Receive (new TimeSpan (0, 0, 5), MessageQueueTransactionType.Single);
			
			Assert.AreEqual (body, received1.Body);
		}

		[Test]
		[ExpectedException (typeof (MessageQueueException))]
		public void ReceiveWithTransactionTypeAndTimeoutFailure ()
		{
			MessageQueue mq = MQUtil.GetQueue (MQUtil.CreateQueueName (), true);
			Assert.IsTrue (mq.Transactional, "Message Queue should be transactional");
			Message received1 = mq.Receive (new TimeSpan (0, 0, 2), MessageQueueTransactionType.Single);
		}
		
		[Test]
		public void ReceiveByIdWithTransactionAndTimeout ()
		{
			String body = "foo-" + DateTime.Now.ToString ();
			Message sent1 = new Message (body, new BinaryMessageFormatter ());
			MessageQueue mq = MQUtil.GetQueue (MQUtil.CreateQueueName (), true);
			mq.Formatter = new BinaryMessageFormatter ();
			Assert.IsTrue (mq.Transactional, "Message Queue should be transactional");
			mq.Send (sent1, MessageQueueTransactionType.Single);
			string id = sent1.Id;
			
			using (MessageQueueTransaction tx = new MessageQueueTransaction ()) {
				tx.Begin ();
				
				Message received1 = mq.ReceiveById (id, new TimeSpan (0, 0, 2), tx);
				
				tx.Commit ();
				
				Assert.AreEqual (body, received1.Body);
			}
		}
		
		[Test]
		public void ReceiveByIdWithTransactionTypeAndTimeout ()
		{
			String body = "foo-" + DateTime.Now.ToString ();
			Message sent1 = new Message (body, new BinaryMessageFormatter ());
			MessageQueue mq = MQUtil.GetQueue (MQUtil.CreateQueueName (), true);
			Assert.IsTrue (mq.Transactional, "Message Queue should be transactional");
			mq.Send (sent1, MessageQueueTransactionType.Single);
			string id = sent1.Id;
			
			Message received1 = mq.ReceiveById (id, new TimeSpan (0, 0, 2), MessageQueueTransactionType.Single);
			Assert.AreEqual (body, received1.Body);
		}

		[Test]
		public void ReceiveByCorrelationIdWithTransactionAndTimeout ()
		{
			string correlationId = Guid.NewGuid () + "\\0";
			String body = "foo-" + DateTime.Now.ToString ();
			Message sent1 = new Message (body, new BinaryMessageFormatter ());
			sent1.CorrelationId = correlationId;
			MessageQueue mq = MQUtil.GetQueue (MQUtil.CreateQueueName (), true);
			Assert.IsTrue (mq.Transactional, "Message Queue should be transactional");
			mq.Send (sent1, MessageQueueTransactionType.Single);
			string id = sent1.Id;
			
			using (MessageQueueTransaction tx = new MessageQueueTransaction ()) {
				tx.Begin ();

				Message received1 = mq.ReceiveByCorrelationId (correlationId, new TimeSpan (0, 0, 2), tx);
				tx.Commit ();
				Assert.AreEqual (body, received1.Body);
			}
		}		

		[Test]
		public void ReceiveByCorrelationIdWithTransactionTypeAndTimeout ()
		{
			string correlationId = Guid.NewGuid() + "\\0";
			String body = "foo-" + DateTime.Now.ToString();
			Message sent1 = new Message (body, new BinaryMessageFormatter ());
			sent1.CorrelationId = correlationId;
			MessageQueue mq = MQUtil.GetQueue (MQUtil.CreateQueueName (), true);
			Assert.IsTrue (mq.Transactional, "Message Queue should be transactional");
			mq.Send (sent1, MessageQueueTransactionType.Single);
			string id = sent1.Id;

			Message received1 = mq.ReceiveByCorrelationId (correlationId,  new TimeSpan (0, 0, 2), MessageQueueTransactionType.Single);
			Assert.AreEqual (body, received1.Body);
		}		
	}
}
