//
// LinkedResourceCollectionTest.cs - NUnit Test Cases for System.Net.MailAddress.LinkedResourceCollection
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
	public class LinkedResourceCollectionTest
	{
		LinkedResourceCollection lrc;
		LinkedResource lr;
		
		[SetUp]
		public void GetReady ()
		{
			lrc = AlternateView.CreateAlternateViewFromString ("test", new ContentType ("text/plain")).LinkedResources;
			lr = LinkedResource.CreateLinkedResourceFromString ("test", new ContentType ("text/plain"));
		}

		[Test]
		public void InitialCount ()
		{
			Assert.IsTrue (lrc.Count == 0);
		}

		[Test]
		public void AddCount ()
		{
			lrc.Add (lr);
			Assert.IsTrue (lrc.Count == 1);
		}

		[Test]
		public void RemoveCount ()
		{
			lrc.Remove (lr);
			Assert.IsTrue (lrc.Count == 0);
		}
	}
}
#endif
