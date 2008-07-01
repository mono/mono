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
			Assert.AreEqual ("attachment", cd.DispositionType);
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
			Assert.AreEqual ("genome.jpeg", cd.FileName);
		}

		[Test]
		public void Size ()
		{
			Assert.AreEqual (-1, cd.Size);
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
			Assert.AreEqual (2, cd.Parameters.Count);
		}

		[Test]
		public void ToStringTest ()
		{
			string rfc822 = "dd MMM yyyy HH':'mm':'ss zz00";
			string modification_date = DateTime.MaxValue.ToString (rfc822);
			string to_string = cd.ToString ();
			Assert.IsTrue (to_string.StartsWith ("attachment; "), "#1");
			Assert.IsTrue (to_string.Contains ("modification-date=\"" + modification_date + "\""), "#2");
			Assert.IsTrue (to_string.Contains ("filename=genome.jpeg"), "#3");
		}

		[Test]
		public void ToStringTest2 ()
		{
			ContentDisposition dummy = new ContentDisposition ();
			Assert.AreEqual ("attachment", dummy.ToString ());
		}

		[Test]
		public void ToStringTest3 ()
		{
			ContentDisposition dummy = new ContentDisposition ();
			dummy.Size = 0;
			Assert.AreEqual ("attachment; size=0", dummy.ToString ());
		}

		[Test]
		public void ToStringTest4 ()
		{
			ContentDisposition dummy = new ContentDisposition ("attachment");
			dummy.Parameters.Add ("foo", "bar");
			Assert.AreEqual (1, dummy.Parameters.Count);
			Assert.AreEqual ("attachment; foo=bar", dummy.ToString ());
		}
	}
}
#endif
