//
// AttachmentTest.cs - NUnit Test Cases for System.Net.MailAddress.Attachment
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
using System.Net.Mime;

namespace MonoTests.System.Net.Mail
{
	[TestFixture]
	public class AttachmentTest
	{
		Attachment attach;
		
		[SetUp]
		public void GetReady ()
		{
			attach = Attachment.CreateAttachmentFromString ("test", "attachment-name");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ArgumentNullException ()
		{
			Stream s = null;
			new Attachment (s, "application/octet-stream");
		}

		[Test]
		public void ConstructorNullName ()
		{
			new Attachment (new MemoryStream (), null, "application/octet-stream");
		}

		[Test]
		public void CreateAttachmentFromStringNullName ()
		{
			Attachment.CreateAttachmentFromString ("", null, Encoding.ASCII, "application/octet-stream");
		}

		[Test]
		public void ContentDisposition ()
		{
			Assert.IsNotNull (attach.ContentDisposition, "#1");
			Assert.AreEqual ("attachment", attach.ContentDisposition.DispositionType, "#2");
		}

		[Test]
		public void ContentType ()
		{
			Assert.IsNotNull (attach.ContentType, "#1");
			Assert.AreEqual ("text/plain", attach.ContentType.MediaType, "#2");
			Attachment a2 = new Attachment (new MemoryStream (), "myname");
			Assert.IsNotNull (a2.ContentType, "#1");
			Assert.AreEqual ("application/octet-stream", a2.ContentType.MediaType, "#2");
		}

		[Test]
		public void NameEncoding ()
		{
			Assert.IsNull (attach.NameEncoding, "#1");
			Attachment a = new Attachment (new MemoryStream (), "myname");
			Assert.IsNull (a.NameEncoding, "#2");
			a = new Attachment (new MemoryStream (), "myname\u3067");
			Assert.IsNull (a.NameEncoding, "#3");
		}

		[Test]
		public void ContentStream ()
		{
			Assert.IsNotNull (attach.ContentStream);
			Assert.IsTrue (attach.ContentStream.Length == 4);
		}


		[Test]
		public void Name ()
		{
			Assert.AreEqual ("attachment-name", attach.Name, "#1");
			Attachment a2 = new Attachment (new MemoryStream (), new ContentType ("image/jpeg"));
			Assert.AreEqual (null, a2.Name, "#2");
			a2.Name = null; // nullable
		}

		[Test]
		public void TransferEncodingTest ()
		{
			Assert.AreEqual (TransferEncoding.QuotedPrintable, attach.TransferEncoding);
		}
	}
}
#endif
