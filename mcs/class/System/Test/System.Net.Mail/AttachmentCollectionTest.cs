//
// AttachmentCollectionTest.cs - NUnit Test Cases for System.Net.MailAddress.AttachmentCollection
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
using System.Net.Mail;
using System.Net.Mime;

namespace MonoTests.System.Net.Mail
{
	[TestFixture]
	public class AttachmentCollectionTest
	{
		AttachmentCollection ac;
		Attachment a;
		
		[SetUp]
		public void GetReady ()
		{
			ac = new MailMessage ("foo@bar.com", "foo@bar.com").Attachments;
			a = Attachment.CreateAttachmentFromString ("test", new ContentType ("text/plain"));
		}

		[Test]
		public void InitialCount ()
		{
			Assert.IsTrue (ac.Count == 0);
		}

		[Test]
		public void AddCount ()
		{
			ac.Add (a);
			Assert.IsTrue (ac.Count == 1);
		}

		[Test]
		public void RemoveCount ()
		{
			ac.Remove (a);
			Assert.IsTrue (ac.Count == 0);
		}
	}
}
#endif
