//
// AlternateViewCollectionTest.cs - NUnit Test Cases for System.Net.MailAddress.AlternateViewCollection
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
	public class AlternateViewCollectionTest
	{
		AlternateViewCollection avc;
		AlternateView av;
		
		[SetUp]
		public void GetReady ()
		{
			avc = new  MailMessage ("foo@bar.com", "foo@bar.com").AlternateViews;
			av = AlternateView.CreateAlternateViewFromString ("test", new ContentType ("text/plain"));
		}

		[Test]
		public void InitialCount ()
		{
			Assert.IsTrue (avc.Count == 0);
		}

		[Test]
		public void AddCount ()
		{
			avc.Add (av);
			Assert.IsTrue (avc.Count == 1);
		}

		[Test]
		public void RemoveCount ()
		{
			avc.Remove (av);
			Assert.IsTrue (avc.Count == 0);
		}
	}
}
#endif
