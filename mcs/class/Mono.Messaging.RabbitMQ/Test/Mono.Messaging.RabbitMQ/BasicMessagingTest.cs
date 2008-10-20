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
using System.Threading;
using System.Text.RegularExpressions;

using NUnit.Framework;

namespace MonoTests.Mono.Messsaging.RabbitMQ
{
	[TestFixture]
	public class BasicMessageTest {

		[Test]
		public void SendReceiveBinaryMessage ()
		{
			String qName = "testq";
			MessageQueue mq = new MessageQueue (qName);
			Assert.AreEqual (mq.QueueName, qName, "Queue name not set properly");
			String s = "Test: " + DateTime.Now;
			Message m = new Message (s, new BinaryMessageFormatter ());
			m.CorrelationId = "foo";

			mq.Send (m);

			Message m2 = mq.Receive ();
			m2.Formatter = new BinaryMessageFormatter ();
			Assert.AreEqual (s, m2.Body);

			Assert.IsTrue (DateTime.MinValue == m.ArrivedTime);
			Assert.IsNotNull (m2.Id, "Id is null");
			Assert.IsTrue (Guid.Empty.ToString () !=  m2.Id, "Id is Empty");
		   	Assert.IsTrue (DateTime.MinValue != m2.ArrivedTime, "Arrived Time is not set");
			Assert.AreEqual (Acknowledgment.None, m2.Acknowledgment, "Acknowledgment");
			Assert.AreEqual (m.CorrelationId, m2.CorrelationId, "CorrelationId not set properly");
			Assert.IsTrue (0 != m2.SenderVersion);
			Assert.IsNotNull (m2.SourceMachine, "SourceMachine is null");
		}
		
		[Test]
		public void SendReceiveXmlMessage ()
		{
			MessageQueue mq = new MessageQueue ("testq");
			String s = "Test: " + DateTime.Now;
			Message m = new Message (s, new XmlMessageFormatter (new Type[] { typeof (string) }));
		   
			mq.Send (m);
			Message m2 = mq.Receive ();
			m2.Formatter = new XmlMessageFormatter (new Type[] { typeof (string) });
			Assert.AreEqual (s, m2.Body);
		   
			Assert.AreEqual (Acknowledgment.None, m2.Acknowledgment, "Acknowledgment");
			Assert.IsNotNull (m2.ArrivedTime, "Acknowledgment");
			Assert.IsNotNull (m2.Id, "Id");
		}
	   
		[Test]
		public void CreateQueue ()
		{
			string path = @".\private$\MyQ";
			string body = "This is a test";
		   
			MessageQueue q = MessageQueue.Create (path);
			Assert.IsNotNull (q);
		}
	   
		[Test]
		public void SendBinaryText ()
		{
			string path = @".\private$\MyQ";
			string body = "This is a test";
		   
			MessageQueue q;
			if (MessageQueue.Exists (path))
					q = new MessageQueue (path);
			else
					q = MessageQueue.Create (path);

			q.Formatter = new BinaryMessageFormatter ();

			q.Send (body);

			Message m2 = q.Receive ();
		   
			Assert.IsNotNull (m2.Formatter, "Formatter is null");
			Assert.AreEqual (typeof (BinaryMessageFormatter), m2.Formatter.GetType ());
			Assert.AreEqual (body, m2.Body);
		}

		[Test]
		public void SendDefaultText ()
		{
			string path = @".\private$\MyQ";
			string body = "This is a test";

			MessageQueue q;
			if (MessageQueue.Exists (path))
				q = new MessageQueue (path);
			else
				q = MessageQueue.Create (path);

			q.Send (body);

			Message m2 = q.Receive ();
			XmlMessageFormatter xmlf = (XmlMessageFormatter) q.Formatter;
			Assert.AreEqual (typeof (string), xmlf.TargetTypes[0]);
			Assert.AreEqual (typeof (XmlMessageFormatter), m2.Formatter.GetType ());
			Assert.AreEqual (body, m2.Body);
			Assert.AreEqual (0, m2.BodyType);
		}

		[Test]
		public void SendBinaryObject ()
		{
			string path = @".\private$\MyQ";
			Thingy body = new Thingy ();
			body.MyProperty1 = 42;
			body.MyProperty2 = "Something";
			body.MyProperty3 = "Something else";

			MessageQueue q;
			if (MessageQueue.Exists (path))
					q = new MessageQueue (path);
			else
					q = MessageQueue.Create (path);

			q.Formatter = new BinaryMessageFormatter ();

			q.Send (body);
		   
			Message m2 = q.Receive ();
			Thingy body2 = (Thingy) m2.Body;

			Assert.AreEqual (typeof (BinaryMessageFormatter), m2.Formatter.GetType ());
			Assert.AreEqual (body.MyProperty1, body2.MyProperty1);
			Assert.AreEqual (body.MyProperty2, body2.MyProperty2);
			Assert.AreEqual (body.MyProperty3, body2.MyProperty3);
			Assert.AreEqual (768, m2.BodyType);
		}

		[Test]
		public void SendDefaultObject ()
		{
			string path = @".\private$\MyQ";
			Thingy body = new Thingy();
			body.MyProperty1 = 42;
			body.MyProperty2 = "Something";
			body.MyProperty3 = "Something else";

			MessageQueue q;
			if (MessageQueue.Exists (path))
					q = new MessageQueue (path);
			else
					q = MessageQueue.Create (path);

			q.Send (body);

			MessageQueue q2 = new MessageQueue (path);
			q2.Formatter = new XmlMessageFormatter (new Type[] { typeof(Thingy) });

			Message m2 = q2.Receive ();
			Thingy body2 = (Thingy) m2.Body;

			Assert.AreEqual (typeof (XmlMessageFormatter), m2.Formatter.GetType ());
			Assert.AreEqual (body.MyProperty1, body2.MyProperty1);
			Assert.AreEqual (body.MyProperty2, body2.MyProperty2);
			Assert.AreEqual (body.MyProperty3, body2.MyProperty3);
		}

		[Test]
		public void SendBinaryMessage ()
		{
			string path = @".\private$\MyQ";
			Thingy body = new Thingy ();
			body.MyProperty1 = 42;
			body.MyProperty2 = "Something";
			body.MyProperty3 = "Something else";
			Message m1 = new Message (body);
			m1.Formatter = new BinaryMessageFormatter ();

			MessageQueue q;
			if (MessageQueue.Exists (path))
					q = new MessageQueue (path);
			else
					q = MessageQueue.Create (path);


			q.Send (m1);
			//Assert.IsNotNull (m1.Id);

			Message m2 = q.Receive ();
			m2.Formatter = new BinaryMessageFormatter ();
			Assert.IsNotNull (m2.Formatter);
			Assert.AreEqual (typeof (BinaryMessageFormatter), m2.Formatter.GetType ());
			Thingy body2 = (Thingy) m2.Formatter.Read (m2);

			Assert.AreEqual (body.MyProperty1, body2.MyProperty1);
			Assert.AreEqual (body.MyProperty2, body2.MyProperty2);
			Assert.AreEqual (body.MyProperty3, body2.MyProperty3);
		}

		[Test]
		public void SendDefaultMessage ()
		{
			string path = @".\private$\MyQ";
			Thingy body = new Thingy ();
			body.MyProperty1 = 42;
			body.MyProperty2 = "Something";
			body.MyProperty3 = "Something else";
			Message m1 = new Message (body);
			Assert.IsNull (m1.Formatter);

			MessageQueue q;
			if (MessageQueue.Exists (path))
					q = new MessageQueue (path);
			else
					q = MessageQueue.Create (path);

			q.Send (m1);

			Message m2 = q.Receive ();
			m2.Formatter = new XmlMessageFormatter (new Type[] { typeof (Thingy) });
			Assert.IsNotNull (m2.Formatter);
			Assert.AreEqual (typeof (XmlMessageFormatter), m2.Formatter.GetType ());
			Thingy body2 = (Thingy) m2.Formatter.Read (m2);

			Assert.AreEqual (body.MyProperty1, body2.MyProperty1);
			Assert.AreEqual (body.MyProperty2, body2.MyProperty2);
			Assert.AreEqual (body.MyProperty3, body2.MyProperty3);
		}			  
	}

	[Serializable]
	public class Thingy
	{
		private int myVar1;

		public int MyProperty1
		{
			get { return myVar1; }
			set { myVar1 = value; }
		}

		private string myVar2;

		public string MyProperty2
		{
			get { return myVar2; }
			set { myVar2 = value; }
		}

		private string myVar3;

		public string MyProperty3
		{
			get { return myVar3; }
			set { myVar3 = value; }
		}
	}
}
