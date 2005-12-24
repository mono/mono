//
// ContentTypeTest.cs - NUnit Test Cases for System.Net.Mime.ContentType
//
// Authors:
//   John Luke (john.luke@gmail.com)
//
// (C) 2005 John Luke
//
#if NET_2_0
using NUnit.Framework;
using System;
using System.Net.Mime;

namespace MonoTests.System.Net.Mime
{
	[TestFixture]
	public class ContentTypeTest
	{
		ContentType ct;
		
		[SetUp]
		public void GetReady ()
		{
			ct = new ContentType ();
		}

		[Test]
		public void MediaType ()
		{
			Assert.IsTrue (ct.MediaType == "application/octet-stream");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ArgumentNullException ()
		{
			new ContentType (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ArgumentException ()
		{
			new ContentType ("");
		}

		//[Test]
		//[ExpectedException (typeof (FormatException))]
		//public void FormatException ()
		//{
			// new ContentType ("some unparsable format");
		//}

		[Test]
		public void ToStringTest ()
		{
			Assert.IsTrue (ct.ToString () == "application/octet-stream");
		}

		[Test]
		public void ToStringTest2 ()
		{
			ContentType dummy = new ContentType ("text/plain; charset=us-ascii");
			Assert.IsTrue (dummy.ToString () == "text/plain; charset=us-ascii");
		}
	}
}
#endif
