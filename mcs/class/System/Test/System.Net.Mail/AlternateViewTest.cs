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
		public void ContentStream ()
		{
			Assert.IsNotNull (av.ContentStream);
			Assert.IsTrue (av.ContentStream.Length == 4);
		}

		[Test]
		public void TransferEncoding ()
		{
			//Assert.IsTrue (av.TransferEncoding = TransferEncoding.QuotedPrintable);
		}
	}
}
#endif
