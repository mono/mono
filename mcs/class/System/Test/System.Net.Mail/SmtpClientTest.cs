//
// SmtpClientTest.cs - NUnit Test Cases for System.Net.Mail.SmtpClient
//
// Authors:
//   John Luke (john.luke@gmail.com)
//
// (C) 2006 John Luke
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
	public class SmtpClientTest
	{
		SmtpClient smtp;
		
		[SetUp]
		public void GetReady ()
		{
			smtp = new SmtpClient ();
		}

		[Test]
		public void InitialTimeout ()
		{
			Assert.AreEqual (smtp.Timeout, 100000);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void NegativePortValue ()
		{
			smtp.Port = -1;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void NegativeTimeoutValue ()
		{
			smtp.Timeout = -1;
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullHost ()
		{
			smtp.Host = null;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void EmptyHost ()
		{
			smtp.Host = String.Empty;
		}
	}
}
#endif
