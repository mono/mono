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

namespace MonoTests.System.Net.Mail
{
	[TestFixture]
	public class AttachmentTest
	{
		Attachment attach;
		
		[SetUp]
		public void GetReady ()
		{
			attach = Attachment.CreateAttachmentFromString ("test", "text/plain");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ArgumentNullException ()
		{
			Stream s = null;
			new Attachment (s, "application/octet-stream");
		}

		[Test]
		public void ContentDisposition ()
		{
			Assert.IsNotNull (attach.ContentDisposition);
			Assert.IsTrue (attach.ContentDisposition.DispositionType == "attachment");
		}

		[Test]
		public void ContentType ()
		{
			Assert.IsNotNull (attach.ContentType);
			Assert.IsTrue (attach.ContentType.MediaType == "text/plain");
		}

		[Test]
		public void NameEncoding ()
		{
			Assert.IsNull (attach.NameEncoding);
		}

		[Test]
		public void ContentStream ()
		{
			Assert.IsNotNull (attach.ContentStream);
			Assert.IsTrue (attach.ContentStream.Length == 4);
		}


		/*[Test]
		public void Name ()
		{
			Assert.IsNotNull (attach.Name);
		}

		[Test]
		public void TransferEncoding ()
		{
			Assert.IsTrue (attach.TransferEncoding == MimeTransferEncoding.Base64);
		}*/
	}
}
#endif
