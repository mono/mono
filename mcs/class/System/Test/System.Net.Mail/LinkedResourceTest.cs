//
// LinkedResourceTest.cs - NUnit Test Cases for System.Net.MailAddress.LinkedResource
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
	public class LinkedResourceTest
	{
		LinkedResource lr;
		
		[SetUp]
		public void GetReady ()
		{
			lr = LinkedResource.CreateLinkedResourceFromString ("test", new ContentType ("text/plain"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ArgumentNullException ()
		{
			string s = null;
			new LinkedResource (s);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ArgumentNullException2 ()
		{
			Stream s = null;
			new LinkedResource (s);
		}

		[Test]
		public void TransferEncodingTest ()
		{
			Assert.AreEqual (TransferEncoding.QuotedPrintable, lr.TransferEncoding);
		}
	}
}
#endif
