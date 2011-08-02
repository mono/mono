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
	public class BasicMessagingTest {

		[Test]
		public void SendReceiveBinaryMessage ()
		{
			MessageQueue mq = MQUtil.GetQueue ();
			String s = "Test: " + DateTime.Now;
			Message m = new Message (s, new BinaryMessageFormatter ());
			m.CorrelationId = Guid.NewGuid () + "\\0";
			mq.MessageReadPropertyFilter.SetAll ();

			mq.Send (m);

			Message m2 = mq.Receive ();
			m2.Formatter = new BinaryMessageFormatter ();
			Assert.AreEqual (s, m2.Body);

			//Assert.IsTrue (DateTime.MinValue == m.ArrivedTime);
			Assert.IsNotNull(m2.Id, "Id is null");
			Assert.IsTrue (Guid.Empty.ToString () !=  m2.Id, "Id is Empty");
			Assert.IsTrue (DateTime.MinValue != m2.ArrivedTime, "Arrived Time is not set");
			Assert.AreEqual (Acknowledgment.None, m2.Acknowledgment, "Acknowledgment");
			Assert.AreEqual (m.CorrelationId, m2.CorrelationId, "CorrelationId not set properly");
			//Assert.IsTrue (0 != m2.SenderVersion);
			// TODO: This is not supported on a workgroup installation.
			//Assert.IsNotNull (m2.SourceMachine, "SourceMachine is null");
			Assert.AreEqual (mq.QueueName, m2.DestinationQueue.QueueName, "Destination Queue not set");
			
			mq.Close ();
		}
		
		[Test]
		public void SendMessageWithLabel ()
		{
			MessageQueue mq = MQUtil.GetQueue ();
			String label = "mylabel";
			String s = "Test: " + DateTime.Now;
			Message m = new Message (s, new BinaryMessageFormatter ());
			m.CorrelationId = Guid.NewGuid () + "\\0" ;

			mq.Send (m, label);

			Message m2 = mq.Receive ();
			m2.Formatter = new BinaryMessageFormatter ();
			Assert.AreEqual (s, m2.Body, "Message not passed correctly");
			Assert.AreEqual (label, m2.Label, "Label not passed correctly");
		}
		
		
		[Test]
		public void CheckDefaults ()
		{
			Message m = new Message ("Test", new BinaryMessageFormatter ());
			Assert.AreEqual (true, m.AttachSenderId, "AttachSenderId has incorrect default");
			Assert.AreEqual (Guid.Empty.ToString () + "\\0", m.Id, "Id has incorrect default");
			Assert.AreEqual ("Microsoft Base Cryptographic Provider, Ver. 1.0", 
			                 m.AuthenticationProviderName, 
			                 "AuthenticationProviderName has incorrect default"); 
			Assert.AreEqual (0, m.Extension.Length, "Extension has incorrect default");
			Assert.AreEqual ("", m.Label, "Label has incorrect default");
			Assert.IsFalse (m.Recoverable, "Recoverable has incorrect default");
			Assert.IsFalse (m.IsFirstInTransaction, "IsFirstInTransaction has incorrect default");
			Assert.IsFalse (m.IsLastInTransaction, "IsLastInTransaction has incorrect default");
			Assert.AreEqual ("", m.TransactionId, "TransactionId has incorrect default");
			Assert.AreEqual (MessagePriority.Normal, m.Priority, "MessagePriority has incorrect default");
		}
		
		private static void CheckInvalidOperation (Message m, String property)
		{
			PropertyInfo pi = m.GetType ().GetProperty (property);
			try {
				Assert.IsNotNull (pi, "Property not defined: " + property);
				object o = pi.GetValue (m, null);
				Assert.Fail (property + ": " + o);
			} catch (InvalidOperationException) {
			} catch (TargetInvocationException e) {
				Assert.AreEqual (typeof (InvalidOperationException), 
				                 e.InnerException.GetType ());
			}
		}
		
		[Test]
		public void CheckInvalidPropertyOperations ()
		{
			Message m = new Message ("Test", new BinaryMessageFormatter ());
			CheckInvalidOperation (m, "Acknowledgment");
			CheckInvalidOperation (m, "ArrivedTime");
			CheckInvalidOperation (m, "Authenticated");
			CheckInvalidOperation (m, "DestinationQueue");
			//CheckInvalidOperation (m, "Id");
			//CheckInvalidOperation (m, "IsFirstInTransaction");
			//CheckInvalidOperation (m, "IsLastInTransaction");
			// TODO: Support 2.0 features.
			//CheckInvalidOperation (m, "LookupId");
			CheckInvalidOperation (m, "MessageType");
			CheckInvalidOperation (m, "SenderId");
			CheckInvalidOperation (m, "SenderVersion");
			CheckInvalidOperation (m, "SentTime");
			CheckInvalidOperation (m, "SourceMachine");
			//CheckInvalidOperation (m, "TransactionId");
		}
		
		private static void CheckArgumentInvalid(Message m, String property, Type exceptionType)
		{
			PropertyInfo pi = m.GetType().GetProperty(property);
			try {
				Assert.IsNotNull(pi, "Property not defined: " + property);
				pi.SetValue(m, null, null);
				Assert.Fail(property);
			} catch (InvalidOperationException) {
			} catch (TargetInvocationException e) {
				Assert.AreEqual(exceptionType,
				                e.InnerException.GetType(),
				                property);
			}
		}

		[Test]
		public void CheckArgumentInvalidForProperties ()
		{
			Message m = new Message ("Stuff");
			CheckArgumentInvalid (m, "DestinationSymmetricKey", typeof (ArgumentNullException));
			CheckArgumentInvalid (m, "DigitalSignature", typeof(ArgumentNullException));
			CheckArgumentInvalid (m, "Extension", typeof(ArgumentNullException));
		}

		[Test]
		public void SendReceiveBinaryMessageWithAllPropertiesSet ()
		{
			MessageQueue mq = MQUtil.GetQueue ();
			mq.MessageReadPropertyFilter.SetAll ();
			
			MessageQueue adminQ = MQUtil.GetQueue (@".\private$\myadmin");
			MessageQueue responseQ = MQUtil.GetQueue (@".\private$\myresponse");
			Guid connectorType = Guid.NewGuid ();
			String s = "Test: " + DateTime.Now;
			
			Message m = new Message (s, new BinaryMessageFormatter ());
			m.CorrelationId = Guid.NewGuid () + "\\0";
			m.AcknowledgeType = AcknowledgeTypes.PositiveArrival;
			m.AdministrationQueue = adminQ;
			m.AppSpecific = 5;
			//m.AuthenticationProviderName = "Test Provider Name";
			//m.AuthenticationProviderType = CryptographicProviderType.None;
			//m.ConnectorType = connectorType;
			//m.DestinationSymmetricKey = new byte[] { 0x0A, 0x0B, 0x0C };
			//m.DigitalSignature = new byte[] { 0x0C, 0x0D, 0x0E };
			//m.EncryptionAlgorithm = EncryptionAlgorithm.Rc4;
			m.Extension = new byte[] { 0x01, 0x02, 0x03 };
			//m.HashAlgorithm = HashAlgorithm.Sha;
			m.Label = "MyLabel";
			m.Priority = MessagePriority.AboveNormal;
			m.Recoverable = true;
			m.ResponseQueue = responseQ;
			m.SenderCertificate = new byte[] { 0x04, 0x05, 0x06 };
			m.TimeToBeReceived = new TimeSpan(0, 0, 10);
			m.TimeToReachQueue = new TimeSpan(0, 0, 5);
			//m.UseAuthentication = true;
			m.UseDeadLetterQueue = true;
			//m.UseEncryption = true;
			
			mq.Send (m);
			
			Message m2 = mq.Receive ();
			
			m2.Formatter = new BinaryMessageFormatter ();
			Assert.AreEqual (s, m2.Body);
			
			Assert.AreEqual (AcknowledgeTypes.PositiveArrival, m2.AcknowledgeType, 
			                 "AcknowledgeType not passed correctly");
			Assert.AreEqual (adminQ.QueueName, m2.AdministrationQueue.QueueName, 
			                 "AdministrationQueue not passed correctly");
			Assert.AreEqual (5, m2.AppSpecific, "AppSpecific not passed correctly");
			//Assert.AreEqual (m.AuthenticationProviderName, m2.AuthenticationProviderName,
			//                 "AuthenticationProviderName not passed correctly");
			//Assert.AreEqual (m.AuthenticationProviderType, m2.AuthenticationProviderType,
			//                 "AuthenticationProviderType not passed correctly");
			//Assert.AreEqual (connectorType, m2.ConnectorType, 
			//                 "ConnectorType not passed correctly");
			Assert.AreEqual (m.CorrelationId, m2.CorrelationId, 
			                 "CorrelationId not passed correctly");
			//AreEqual (m.DestinationSymmetricKey, m2.DestinationSymmetricKey, 
			//          "DestinationSymmetricKey not passed correctly");
			//AreEqual (m.DigitalSignature, m2.DigitalSignature,
			//          "DigitalSignature not passed properly");
			//Assert.AreEqual (EncryptionAlgorithm.Rc4, m2.EncryptionAlgorithm,
			//                 "EncryptionAlgorithm not passed properly");
			AreEqual (m.Extension, m2.Extension, "Extension not passed properly");
			//Assert.AreEqual (m.HashAlgorithm, m2.HashAlgorithm,
			//                 "HashAlgorithm not passed properly");
			Assert.AreEqual (m.Label, m2.Label, "Label not passed correctly");
			Assert.AreEqual (MessagePriority.AboveNormal, m2.Priority,
			                 "Priority not passed properly");
			Assert.AreEqual (true, m2.Recoverable, "Recoverable not passed properly");
			Assert.AreEqual (responseQ.QueueName, m2.ResponseQueue.QueueName,
			                 "ResponseQueue not passed properly");
			AreEqual (m.SenderCertificate, m2.SenderCertificate,
			          "SenderCertificate not passed properly");
			Assert.AreEqual (m.TimeToBeReceived, m2.TimeToBeReceived,
			                 "TimeToBeReceived not passed properly");
			Assert.AreEqual (m.TimeToReachQueue, m2.TimeToReachQueue,
			                 "TimeToReachQueue not passed properly");
			//Assert.IsTrue (m2.UseAuthentication,
			//               "UseAuthentication not passed properly");
			Assert.IsTrue (m2.UseDeadLetterQueue,
			               "UseDeadLetterQueue not passed properly");
			//Assert.IsTrue (m2.UseEncryption, "UseEncryption not pass properly");
			//Assert.AreEqual ();
			
			Assert.IsNotNull (m2.Id, "Id is null");
			Assert.IsTrue (Guid.Empty.ToString () !=  m2.Id, "Id is Empty");
			Assert.IsTrue (DateTime.MinValue != m2.ArrivedTime, "Arrived Time is not set");
			Assert.AreEqual (Acknowledgment.None, m2.Acknowledgment, "Acknowledgment");
			//Assert.IsTrue (0 != m2.SenderVersion);
			
			//Assert.IsNotNull (m2.SourceMachine, "SourceMachine is null");
			
		}
		
		private static void AreEqual(byte[] expected, byte[] actual, string message)
		{
			Assert.AreEqual (expected.Length, actual.Length, message);
			for (int i = 0; i < expected.Length; i++)
				Assert.AreEqual (expected[i], actual[i], message);
		}
		
		//[Test]
		// No supported by Rabbit
		public void SendPriorityMessages ()
		{
			MessageQueue mq = MQUtil.GetQueue ();
			Message sent1 = new Message ("Highest", new BinaryMessageFormatter ());
			sent1.Priority = MessagePriority.Highest;
			Message sent2 = new Message ("Lowest", new BinaryMessageFormatter ());
			sent2.Priority = MessagePriority.Lowest;
			
			mq.Send (sent1);
			mq.Send (sent2);
			
			Message received1 = mq.Receive ();
			Message received2 = mq.Receive ();
			
			Assert.AreEqual (MessagePriority.Highest, received2.Priority,
			                 "Priority delivery incorrect");
			Assert.AreEqual (MessagePriority.Lowest, received1.Priority,
			                 "Priority delivery incorrect");
		}
		
		[Test]
		public void SendReceiveXmlMessage ()
		{
			MessageQueue mq = MQUtil.GetQueue ();
			String s = "Test: " + DateTime.Now;
			Message m = new Message (s, new XmlMessageFormatter (new Type[] { typeof (string) }));
			mq.MessageReadPropertyFilter.SetAll();
		   
			mq.Send (m);
			Message m2 = mq.Receive ();
			m2.Formatter = new XmlMessageFormatter (new Type[] { typeof (string) });
			Assert.AreEqual (s, m2.Body);
		   
			Assert.AreEqual (Acknowledgment.None, m2.Acknowledgment, "Acknowledgment");
			Assert.IsNotNull (m2.ArrivedTime, "Acknowledgment");
			Assert.IsNotNull (m2.Id, "Id");
		}
	   
		[Test]
		public void SendBinaryText ()
		{
			string body = "This is a test";
		   
			MessageQueue q = MQUtil.GetQueue ();

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
			string body = "This is a test";

			MessageQueue q = MQUtil.GetQueue (MQUtil.CreateQueueName (), new XmlMessageFormatter ());

			q.Send (body);

			Message m2 = q.Receive ();
			XmlMessageFormatter xmlf = (XmlMessageFormatter) q.Formatter;
			Assert.AreEqual (typeof (XmlMessageFormatter), m2.Formatter.GetType ());
			Assert.AreEqual (body, m2.Body);
			Assert.AreEqual (0, m2.BodyType);
		}

		[Test]
		public void SendBinaryObject ()
		{
			Thingy body = new Thingy ();
			body.MyProperty1 = 42;
			body.MyProperty2 = "Something";
			body.MyProperty3 = "Something else";

			MessageQueue q = MQUtil.GetQueue ();

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
			string path = MQUtil.CreateQueueName ();
			Thingy body = new Thingy();
			body.MyProperty1 = 42;
			body.MyProperty2 = "Something";
			body.MyProperty3 = "Something else";

			MessageQueue q = MQUtil.GetQueue (path, new XmlMessageFormatter ());

			q.Send (body);

			MessageQueue q2 = MQUtil.GetQueue (path);
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
			Thingy body = new Thingy ();
			body.MyProperty1 = 42;
			body.MyProperty2 = "Something";
			body.MyProperty3 = "Something else";
			Message m1 = new Message (body);
			m1.Formatter = new BinaryMessageFormatter ();

			MessageQueue q = MQUtil.GetQueue ();

			q.Send (m1);

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
			string path = MQUtil.CreateQueueName ();
			Thingy body = new Thingy ();
			body.MyProperty1 = 42;
			body.MyProperty2 = "Something";
			body.MyProperty3 = "Something else";
			Message m1 = new Message (body);
			Assert.IsNull (m1.Formatter);

			MessageQueue q = MQUtil.GetQueue (path, new XmlMessageFormatter ());

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
