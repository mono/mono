//
// AlternateViewTest.cs - NUnit Test Cases for System.Net.MailAddress.AlternateView
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
	public class AlternateViewTest
	{
		AlternateView av;
		
		[SetUp]
		public void GetReady ()
		{
			av = AlternateView.CreateAlternateViewFromString ("test", new ContentType ("text/plain"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ArgumentNullException ()
		{
			string s = null;
			new AlternateView (s);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ArgumentNullException2 ()
		{
			Stream s = null;
			new AlternateView (s);
		}

		[Test]
		public void ContentType ()
		{
			Assert.IsNotNull (av.ContentType);
			Assert.IsTrue (av.ContentType.MediaType == "text/plain");
		}

		[Test]
		public void ContentType2 ()
		{
			AlternateView av = new AlternateView (new MemoryStream ());
			Assert.IsNotNull (av.ContentType, "#1");
			Assert.AreEqual ("application/octet-stream", av.ContentType.MediaType, "#2");
		}

		[Test]
		public void ContentStream ()
		{
			Assert.IsNotNull (av.ContentStream);
			Assert.IsTrue (av.ContentStream.Length == 4);
		}

		[Test]
		public void TransferEncodingTest ()
		{
			Assert.AreEqual (TransferEncoding.QuotedPrintable, av.TransferEncoding, "#1");

			MemoryStream ms = new MemoryStream (new byte [] {1, 2, 3});
			Assert.AreEqual (TransferEncoding.Base64, new AlternateView (ms).TransferEncoding, "#2");
			Assert.AreEqual (TransferEncoding.Base64, new AlternateView (ms, "text/plain").TransferEncoding, "#3");
		}

		[Test]
		public void CreateAlternateViewFromStringEncodingNull ()
		{
			AlternateView.CreateAlternateViewFromString ("<p>test message</p>", null, "text/html");
		}
	}
}
#endif
