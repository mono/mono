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
	public class AdminTest {
		
		[Test]
		public void CreateNonTransactionalQueue ()
		{
			string qName = @".\private$\admin-queue-1";
			Assert.IsFalse (MessageQueue.Exists (qName), "Queue should not exist");
			MessageQueue q = MessageQueue.Create (qName);
			Assert.IsFalse (q.Transactional);
			Assert.IsTrue (MessageQueue.Exists (qName), "Queue should exist");
		}
		
		[Test]
		public void CreateTransactionalQueue ()
		{
			string qName = @".\private$\admin-queue-2";
			Assert.IsFalse (MessageQueue.Exists (qName), "Queue should not exist");
			MessageQueue q = MessageQueue.Create (qName, true);
			Assert.IsTrue (q.Transactional, "Queue should be transactional");
			Assert.IsTrue (MessageQueue.Exists (qName), "Queue should exist");
		}
		
		private bool Contains(MessageQueue[] qs, String qName)
		{
			foreach (MessageQueue q in qs)
			{
				if (q.QueueName == qName)
					return true;
			}
			return false;
		}

		[Test]
		public void GetPublicQueues ()
		{
			string qName1 = @".\admin-queue-3";
			string qName2 = @".\admin-queue-4";
			
			MessageQueue.Create (qName1);
			MessageQueue.Create (qName2);
			
			MessageQueue[] mq = MessageQueue.GetPublicQueues ();
			Assert.IsTrue (Contains (mq, "admin-queue-3"), qName1 + " not found");
			Assert.IsTrue (Contains (mq, "admin-queue-4"), qName2 + " not found");
		}
		
		[Test]
		public void GetQueue ()
		{
			MessageQueue q1 = MQUtil.GetQueue(@".\private$\admin-queue-5", true);
			Assert.IsTrue (q1.Transactional, "Queue should be transactional");
			MessageQueue q2 = MQUtil.GetQueue(@".\private$\admin-queue-5", true);
			Assert.IsTrue (q2.Transactional, "Queue should be transactional");
		}
		
		[Test]
		[ExpectedException (typeof (MessageQueueException))]
		public void PurgeQueue ()
		{
			MessageQueue q = MQUtil.GetQueue(@".\private$\purge-queue");
			Message m1 = new Message ("foobar1", new BinaryMessageFormatter ());
			Message m2 = new Message ("foobar2", new BinaryMessageFormatter ());
			Message m3 = new Message ("foobar3", new BinaryMessageFormatter ());
			Message m4 = new Message ("foobar4", new BinaryMessageFormatter ());
			
			q.Send (m1);
			q.Send (m2);
			q.Send (m3);
			q.Send (m4);
			
			q.Receive ();			
			q.Purge ();
			q.Receive (new TimeSpan (0, 0, 2));
		}
				
		[Test]
		public void DeleteQueue ()
		{
			MessageQueue q = MQUtil.GetQueue(@".\private$\delete-queue");
			Message m1 = new Message ("foobar1", new BinaryMessageFormatter ());
			
			q.Send (m1);
			
			q.Receive ();

			MessageQueue.Delete(@".\private$\delete-queue");
		}
	}
}
