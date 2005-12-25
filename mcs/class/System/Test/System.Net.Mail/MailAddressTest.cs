//
// MailAddressTest.cs - NUnit Test Cases for System.Net.MailAddress.MailAddress
//
// Authors:
//   John Luke (john.luke@gmail.com)
//
// (C) 2005 John Luke
//
#if NET_2_0
using NUnit.Framework;
using System;
using System.Net.Mail;

namespace MonoTests.System.Net.Mail
{
	[TestFixture]
	public class MailAddressTest
	{
		MailAddress address;
		
		[SetUp]
		public void GetReady ()
		{
			address = new MailAddress ("foo@example.com", "Mr. Foo Bar");
		}

		[Test]
		public void Address ()
		{
			Assert.IsTrue (address.Address == "foo@example.com");
		}

		[Test]
		public void DisplayName ()
		{
			Assert.IsTrue (address.DisplayName == "Mr. Foo Bar");
		}

		[Test]
		public void User ()
		{
			Assert.IsTrue (address.User == "foo");
		}

		[Test]
		public void Host ()
		{
			Assert.IsTrue (address.Host == "example.com");
		}

		[Test]
		public void SimpleToStringTest ()
		{
			Assert.IsTrue (new MailAddress ("foo@example.com").ToString () == "foo@example.com");
		}

		[Test]
		public void ToStringTest ()
		{
			Assert.IsTrue (address.ToString () == "\"Mr. Foo Bar\" <foo@example.com>");
		}
	}
}
#endif
