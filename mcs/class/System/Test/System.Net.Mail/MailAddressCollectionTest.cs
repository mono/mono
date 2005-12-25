//
// MailAddressCollectionTest.cs - NUnit Test Cases for System.Net.MailAddress.MailAddressCollection
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
	public class MailAddressCollectionTest
	{
		MailAddressCollection ac;
		MailAddress a;
		
		[SetUp]
		public void GetReady ()
		{
			ac = new MailAddressCollection ();
			a = new MailAddress ("foo@bar.com");
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
