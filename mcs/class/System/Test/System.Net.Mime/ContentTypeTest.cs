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

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void FormatException ()
		{
			new ContentType ("attachment; foo=bar"); // missing '/'
		}

		[Test]
		public void ArbitraryParameter ()
		{
			new ContentType ("application/xml; foo=bar");
		}

		[Test]
		public void Boundary ()
		{
			Assert.IsNull (ct.Boundary);
		}

		[Test]
		public void CharSet ()
		{
			Assert.IsNull (ct.CharSet);
		}

		[Test]
		public void Equals ()
		{
			Assert.IsTrue (ct.Equals (new ContentType ()));
			Assert.IsFalse (ct  == new ContentType ());
		}

		[Test]
		public void GetHashCode ()
		{
			Assert.IsTrue (ct.GetHashCode () == new ContentType ().GetHashCode ());
		}

		[Test]
		public void MediaType ()
		{
			Assert.IsTrue (ct.MediaType == "application/octet-stream");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void MediaTypeNullException ()
		{
			ct.MediaType = null;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MediaTypeEmptyException ()
		{
			ct.MediaType = "";
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void MediaTypeFormatException ()
		{
			ct.MediaType = "application/x-myType;";
		}

		[Test]
		public void Parameters ()
		{
			ContentType dummy = new ContentType ();
			Assert.IsTrue (dummy.Parameters.Count == 0);
			dummy.CharSet = "us-ascii";
			dummy.Name = "test";
			dummy.Boundary = "-----boundary---0";
			dummy.Parameters.Add ("foo", "bar");
			Assert.IsTrue (dummy.Parameters.Count == 4);
			Assert.IsTrue (dummy.Parameters["charset"] == "us-ascii");
			Assert.IsTrue (dummy.Parameters["name"] == "test");
			Assert.IsTrue (dummy.Parameters["boundary"] == "-----boundary---0");
			Assert.IsTrue (dummy.Parameters["foo"] == "bar");
		}

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

		[Test]
		public void ToStringTest3 ()
		{
			ct.Parameters.Add ("foo", "bar");
			Assert.IsTrue (ct.ToString () == "application/octet-stream; foo=bar");
		}
	}
}
#endif
