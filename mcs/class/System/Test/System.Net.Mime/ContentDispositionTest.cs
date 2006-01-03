//
// ContentDispositionTest.cs - NUnit Test Cases for System.Net.Mime.ContentDisposition
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
	public class ContentDispositionTest
	{
		ContentDisposition cd;
		
		[SetUp]
		public void GetReady ()
		{
			cd = new ContentDisposition ();
			cd.FileName = "genome.jpeg";
			cd.ModificationDate = DateTime.MaxValue;
		}

		[Test]
		public void DispositionType ()
		{
			Assert.IsTrue (cd.DispositionType == "attachment");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DispositionTypeNull ()
		{
			cd.DispositionType = null;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DispositionTypeEmpty ()
		{
			cd.DispositionType = "";
		}

		[Test]
		public void EqualsHashCode ()
		{
			ContentDisposition dummy1 = new ContentDisposition ();
			dummy1.Inline = true;
			ContentDisposition dummy2 = new ContentDisposition ("inline");
			Assert.IsTrue (dummy1.Equals (dummy2));
			Assert.IsFalse (dummy1 == dummy2);
			Assert.IsTrue (dummy1.GetHashCode () == dummy2.GetHashCode ());
		}

		[Test]
		public void Equals ()
		{
			ContentDisposition dummy1 = new ContentDisposition ();
			dummy1.FileName = "genome.jpeg";
			ContentDisposition dummy2 = new ContentDisposition ("attachment; filename=genome.jpeg");
			Assert.IsTrue (dummy1.Equals (dummy2));
		}

		[Test]
		public void FileName ()
		{
			Assert.IsTrue (cd.FileName == "genome.jpeg");
		}

		[Test]
		public void Size ()
		{
			Assert.IsTrue (cd.Size == -1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ArgumentNullException ()
		{
			new ContentDisposition (null);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void FormatException ()
		{
			new ContentDisposition ("");
		}

		[Test]
		public void NoFormatException ()
		{
			new ContentDisposition ("attachment; foo=bar");
		}

		[Test]
		public void IsInline ()
		{
			Assert.IsFalse (cd.Inline);
		}

		[Test]
		public void Parameters ()
		{
			Assert.IsNotNull (cd.Parameters, "is not null");
			Assert.IsTrue (cd.Parameters.Count == 2, "count is " + cd.Parameters.Count);
		}

		[Test]
		public void ToStringTest ()
		{
			Assert.IsTrue (cd.ToString () == "attachment; modification-date=\"31 Dec 9999 23:59:59 -0500\"; filename=genome.jpeg");
		}

		[Test]
		public void ToStringTest2 ()
		{
			ContentDisposition dummy = new ContentDisposition ();
			Assert.IsTrue (dummy.ToString () == "attachment");
		}

		[Test]
		public void ToStringTest3 ()
		{
			ContentDisposition dummy = new ContentDisposition ();
			dummy.Size = 0;
			Assert.IsTrue (dummy.ToString () == "attachment; size=0");
		}

		[Test]
		public void ToStringTest4 ()
		{
			ContentDisposition dummy = new ContentDisposition ("attachment");
			dummy.Parameters.Add ("foo", "bar");
			Assert.IsTrue (dummy.Parameters.Count == 1);
			Assert.IsTrue (dummy.ToString () == "attachment; foo=bar");
		}
	}
}
#endif
