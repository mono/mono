using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Channels
{
	[TestFixture]
	public class MessageBufferTest
	{
#if !MOBILE && !XAMMAC_4_5
		[Test]
		public void TestCreateNavigator ()
		{
			Message m = Message.CreateMessage (MessageVersion.Default, "http://tempuri.org/");
			MessageBuffer mb = m.CreateBufferedCopy (1024);

			XPathNavigator nav = mb.CreateNavigator ();

			Assert.IsTrue (nav is SeekableXPathNavigator);
		}
#endif
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestCreateBufferedCopyTwice ()
		{
			Message m = Message.CreateMessage (MessageVersion.Default, "action", 1);
			m.CreateBufferedCopy (64);
			m.CreateBufferedCopy (64);
		}

		[Test]
		public void TestCreateMessage ()
		{
			Message m = Message.CreateMessage (MessageVersion.Default, "action", 1);
			Message m2 = Message.CreateMessage (MessageVersion.Default, "action", 1);
			MessageBuffer mb = m.CreateBufferedCopy (64);

			Message n = mb.CreateMessage ();
			Assert.AreEqual (m2.ToString (), n.ToString (), "#2");
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void TestWriteMessageNull ()
		{
			Message m = Message.CreateMessage (MessageVersion.Default, "action", 1);
			MessageBuffer mb = m.CreateBufferedCopy (64);

			mb.WriteMessage (null);
		}
		
		[Test]
		public void WriteMessageWithDictionaryWriter ()
		{
			Message m = Message.CreateMessage (MessageVersion.Default, "action", 1);

			StringBuilder sb = new StringBuilder ();
			XmlWriter w = XmlWriter.Create (sb);
			XmlDictionaryWriter dw = XmlDictionaryWriter.CreateDictionaryWriter (w);			
			m.WriteMessage (dw);
			dw.Close ();
		}

		[Test]
		public void TestWriteMessage ()
		{
			Message m = Message.CreateMessage (MessageVersion.Default, "action", 1);
			Message n = Message.CreateMessage (MessageVersion.Default, "action", 1);
			Assert.AreEqual (m.ToString (), n.ToString ());

			// Write out a message using a Binary XmlDictionaryWriter
			MemoryStream ms = new MemoryStream ();
			XmlDictionaryWriter w = XmlDictionaryWriter.CreateBinaryWriter (ms);
			m.WriteMessage (w);
			w.Close ();
			ms.Close ();
			byte [] expected = ms.GetBuffer ();
			StringBuilder sb = new StringBuilder ();
			sb.AppendLine ("expected: " + expected.Length);

			for (int i = 0; i < 24; i++) {
				byte b = expected [i];
				sb.Append (String.Format ("{0:X02} ", b));
			}
			string exp = sb.ToString ();

			// Write out an equivalent MessageBuffer
			MessageBuffer mb = n.CreateBufferedCopy (1024);
			sb = new StringBuilder ();
			ms = new MemoryStream ();
			mb.WriteMessage (ms);
			ms.Close ();
			byte [] actual = ms.GetBuffer ();
			sb.AppendLine ("actual: " + actual.Length);
			for (int i = 0; i < 24; i++) {
				byte b = actual [i];
				sb.Append (String.Format ("{0:X02} ", b));
			}
			string act = sb.ToString ();

//			Console.WriteLine (exp + "\n" + act);

			// So the lengths of the buffer are the same....
			Assert.AreEqual (expected.Length, actual.Length);

			// TODO: 
			// There are three possible XmlDictionaryWriters:
			// Binary, Dictionary, Mtom
			// 
			// It doesn't have a MIME type header, so it's definitely not Mtom.
			// Dictionary outputs text, so it's not that either.
			// It's got to be the Binary one.
			//
			// But the content differs, why? 

			// FIXME: we don't have AreNotEqual
			// Assert.AreNotEqual (expected, actual);
		}
		
		[Test]
		public void TestEmptyMessageBuffer ()
		{
			Message m = Message.CreateMessage (MessageVersion.Default, String.Empty);
			MessageBuffer b = m.CreateBufferedCopy (0);

			Assert.IsTrue (m.IsEmpty, "#1");
			Assert.AreEqual (0, b.BufferSize, "#2");
			Assert.AreEqual ("application/soap+msbin1", b.MessageContentType, "#3");
		}

		[Test]
		public void TestSimpleMessageBuffer ()
		{
			Message m = Message.CreateMessage (MessageVersion.Default, 
							   String.Empty,
							   new MyBodyWriter ());
			MessageBuffer b = m.CreateBufferedCopy (Int32.MaxValue);

			Assert.IsFalse (m.IsEmpty, "#1");
			Assert.AreEqual (0, b.BufferSize, "#2");
			Assert.AreEqual ("application/soap+msbin1", b.MessageContentType, "#3");
			// BodyWriter must not be used twice.
			using (XmlWriter w = XmlWriter.Create (TextWriter.Null)) {
				b.CreateMessage ().WriteMessage (w);
			}
			using (XmlWriter w = XmlWriter.Create (TextWriter.Null)) {
				b.CreateMessage ().WriteMessage (w);
			}
		}

		[Test, ExpectedException (typeof (ObjectDisposedException))]
		public void TestCreateMessageFromClosedBuffer ()
		{
			Message m = Message.CreateMessage (MessageVersion.Default, String.Empty);
			MessageBuffer b = m.CreateBufferedCopy (0);

			b.Close ();
			b.CreateMessage ();
		}

		[Test, ExpectedException (typeof (InvalidOperationException))]
		public void CreateBufferedCopyConsumesMessage ()
		{
			Message msg = Message.CreateMessage (
				MessageVersion.Default, "foo", new MyBodyWriter ());
			MessageBuffer buf = msg.CreateBufferedCopy (1);
			// This WriteMessage() complains that the Message
			// is already consumed.
			using (XmlWriter w = XmlWriter.Create (TextWriter.Null)) {
				msg.WriteMessage (w);
			}
		}

		[Test]
		public void IsFaultCopied ()
		{
			var ret = Message.CreateMessage (MessageVersion.Soap12,
				MessageFault.CreateFault (new FaultCode ("mycode"), "private affair"),
				"http://tempuri.org/IFoo/Test");
			Assert.IsTrue (ret.IsFault, "#1");
			var mb = ret.CreateBufferedCopy (0x1000);
			ret = mb.CreateMessage ();
			Assert.IsTrue (ret.IsFault, "#2");
		}
	}

	internal class MyBodyWriter : BodyWriter
	{
		bool done;

		internal MyBodyWriter ()
			: base (false)
		{
		}

		protected override void OnWriteBodyContents (XmlDictionaryWriter writer)
		{
			if (done)
				Assert.Fail ("Allow two or more write.");
			done = true;
		}
	}
}
