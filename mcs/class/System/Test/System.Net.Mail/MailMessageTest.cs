//
// MailMessageTest.cs - NUnit Test Cases for System.Net.MailAddress.MailMessage
//
// Authors:
//   John Luke (john.luke@gmail.com)
//
// (C) 2005 John Luke
//
#if NET_2_0
using NUnit.Framework;
using System;
using System.IO;
using System.Text;
using System.Net.Mail;

namespace MonoTests.System.Net.Mail
{
	[TestFixture]
	public class MailMessageTest
	{
		MailMessage msg;
		
		[SetUp]
		public void GetReady ()
		{
			msg = new MailMessage ("from@example.com", "to@example.com");
			msg.Subject = "the subject";
			msg.Body = "hello";
			//msg.AlternateViews.Add (AlternateView.CreateAlternateViewFromString ("<html><body>hello</body></html>", "text/html"));
			//Attachment a = Attachment.CreateAttachmentFromString ("blah blah", "text/plain");
			//msg.Attachments.Add (a);
		}

		/*[Test]
		public void AlternateView ()
		{
			Assert.IsTrue (msg.AlternateViews.Count == 1);
			AlternateView av = msg.AlternateViews[0];
			// test that the type is ok, etc.
		}*/

		/*[Test]
		public void Attachment ()
		{
			Assert.IsTrue (msg.Attachments.Count == 1);
			Attachment at = msg.Attachments[0];
			Assert.IsTrue (at.ContentType.MediaType == "text/plain");
		}*/

		[Test]
		public void Body ()
		{
			Assert.IsTrue (msg.Body == "hello");
		}

		[Test]
		public void From ()
		{
			Assert.IsTrue (msg.From.Address == "from@example.com");
		}

		[Test]
		public void IsBodyHtml ()
		{
			Assert.IsFalse (msg.IsBodyHtml);
		}

		[Test]
		public void Priority ()
		{
			Assert.IsTrue (msg.Priority == MailPriority.Normal);
		}

		[Test]
		public void Subject ()
		{
			Assert.IsTrue (msg.Subject == "the subject");
		}

		[Test]
		public void To ()
		{
			Assert.IsTrue (msg.To.Count == 1);
			Assert.IsTrue (msg.To[0].Address == "to@example.com");
		}
	}
}
#endif
