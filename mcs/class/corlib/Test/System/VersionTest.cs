//
// VersionTest.cs - NUnit Test Cases for the System.Version class
//
// Authors:
//	Duco Fijma (duco@lorentz.xs4all.nl)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
//   (C) 2002 Duco Fijma
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
//

using System;

using NUnit.Framework;

namespace MonoTests.System
{
	[TestFixture]
	public class VersionTest
	{
		[Test]
		public void TestCtors ()
		{
			Version v1 = new Version ();
			Assert.AreEqual (0, v1.Major, "#A1");
			Assert.AreEqual (0, v1.Minor, "#A2");
			Assert.AreEqual (-1, v1.Build, "#A3");
			Assert.AreEqual (-1, v1.Revision, "#A4");

			v1 = new Version (23, 7);
			Assert.AreEqual (23, v1.Major, "#B1");
			Assert.AreEqual (7, v1.Minor, "#B2");
			Assert.AreEqual (-1, v1.Build, "#B3");
			Assert.AreEqual (-1, v1.Revision, "#B4");

			v1 = new Version (23, 7, 99);
			Assert.AreEqual (23, v1.Major, "#C1");
			Assert.AreEqual (7, v1.Minor, "#C2");
			Assert.AreEqual (99, v1.Build, "#C3");
			Assert.AreEqual (-1, v1.Revision, "#C4");

			v1 = new Version (23, 7, 99, 42);
			Assert.AreEqual (23, v1.Major, "#D1");
			Assert.AreEqual (7, v1.Minor, "#D2");
			Assert.AreEqual (99, v1.Build, "#D3");
			Assert.AreEqual (42, v1.Revision, "#D4");
		
			try {
				v1 = new Version (23, -1);
				Assert.Fail ("#E");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				v1 = new Version (23, 7, -1);
				Assert.Fail ("#F");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				v1 = new Version (23, 7, 99, -1);
				Assert.Fail ("#G");
			} catch (ArgumentOutOfRangeException) {
			}
		}

		[Test]
		public void TestStringCtor () 
		{
			Version v1 = new Version ("1.42");
			Assert.AreEqual (1, v1.Major, "#A1");
			Assert.AreEqual (42, v1.Minor, "#A2");
			Assert.AreEqual (-1, v1.Build, "#A3");
			Assert.AreEqual (-1, v1.Revision, "#A4");

			v1 = new Version ("1.42.79");
			Assert.AreEqual (1, v1.Major, "#B1");
			Assert.AreEqual (42, v1.Minor, "#B2");
			Assert.AreEqual (79, v1.Build, "#B3");
			Assert.AreEqual (-1, v1.Revision, "#B4");

			v1 = new Version ("1.42.79.66");
			Assert.AreEqual (1, v1.Major, "#C1");
			Assert.AreEqual (42, v1.Minor, "#C2");
			Assert.AreEqual (79, v1.Build, "#C3");
			Assert.AreEqual (66, v1.Revision, "#C4");

			try {
				v1 = new Version ("1.42.-79");
				Assert.Fail ("#D");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				v1 = new Version ("1");
				Assert.Fail ("#E");
			} catch (ArgumentException) {
			}

			try {
				v1 = new Version ("1.2.3.4.5");
				Assert.Fail ("#F");
			} catch (ArgumentException) {
			}

			try {
				v1 = new Version ((string) null);
				Assert.Fail ("#G");
			} catch (ArgumentNullException) {
			}
		}

		[Test]
		public void TestClone () 
		{
			Version v1 = new Version (1, 2, 3, 4);
			Version v2 = (Version) v1.Clone ();

			Assert.IsTrue (v1.Equals (v2), "#A1");
			Assert.IsFalse (object.ReferenceEquals (v1, v2), "#A2");

			Version v3 = new Version (); // 0.0
			v2 = (Version) v3.Clone ();

			Assert.IsTrue (v3.Equals (v2), "#B1");
			Assert.IsFalse (object.ReferenceEquals (v3, v2), "#B2");
		}

		[Test]
		public void TestCompareTo ()
		{
			Assert.AreEqual (1, new Version (1, 2).CompareTo (new Version (1, 1)), "#A1");
			Assert.AreEqual (-1, new Version (1, 2, 3).CompareTo (new Version (2, 2, 3)), "#A2");
			Assert.AreEqual (1, new Version (1, 2, 3, 4).CompareTo (new Version (1, 2, 2, 1)), "#A3");
			Assert.AreEqual (1, new Version (1, 3).CompareTo (new Version (1, 2, 2, 1)), "#A4");
			Assert.AreEqual (-1, new Version (1, 2, 2).CompareTo (new Version (1, 2, 2, 1)), "#A5");
			Assert.AreEqual (0, new Version (1, 2).CompareTo (new Version (1, 2)), "#A6");
			Assert.AreEqual (0, new Version (1, 2, 3).CompareTo (new Version (1, 2, 3)), "#A7");
			Assert.IsTrue (new Version (1, 1) < new Version (1, 2), "#A8");
			Assert.IsTrue (new Version (1, 2) <= new Version (1, 2), "#A9");
			Assert.IsTrue (new Version (1, 2) <= new Version (1, 3), "#A10");
			Assert.IsTrue (new Version (1, 2) >= new Version (1, 2), "#A11");
			Assert.IsTrue (new Version (1, 3) >= new Version (1, 2), "#A12");
			Assert.IsTrue (new Version (1, 3) > new Version (1, 2), "#A13");

			Version v1 = new Version(1, 2);
			bool exception;

			Assert.AreEqual (1, v1.CompareTo (null), "#B");

			try {
				v1.CompareTo ("A string is not a version");
				Assert.Fail ("#C");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void TestEquals ()
		{
			Version v1 = new Version (1, 2);
			Version v2 = new Version (1, 2, 3, 4);
			Version v3 = new Version (1, 2, 3, 4);

			Assert.IsTrue (v2.Equals (v3), "#A1");
			Assert.IsTrue (v2.Equals (v2), "#A2");
			Assert.IsFalse (v1.Equals (v3), "#A3");

			Assert.IsTrue (v2 == v3, "#B1");
#pragma warning disable 1718
			Assert.IsTrue (v2 == v2, "#B2");
#pragma warning restore 1718
			Assert.IsFalse (v1 == v3, "#B3");

			Assert.IsFalse (v2.Equals ((Version) null), "#C1");
			Assert.IsFalse (v2.Equals ("A string"), "#C2");
		}

		[Test]
		public void TestToString ()
		{
			Version v1 = new Version (1,2);
			Version v2 = new Version (1,2,3);
			Version v3 = new Version (1,2,3,4);

			Assert.AreEqual ("1.2", v1.ToString (), "#A1");
			Assert.AreEqual ("1.2.3", v2.ToString (), "#A2");
			Assert.AreEqual ("1.2.3.4", v3.ToString (), "#A3");
			Assert.AreEqual (string.Empty, v3.ToString (0), "#A4");
			Assert.AreEqual ("1", v3.ToString (1), "#A5");
			Assert.AreEqual ("1.2", v3.ToString (2), "#A6");
			Assert.AreEqual ("1.2.3", v3.ToString (3), "#A7");
			Assert.AreEqual ("1.2.3.4", v3.ToString (4), "#A8");

			try {
				v2.ToString (4);
				Assert.Fail ("#B");
			} catch (ArgumentException) {
			}

			try {
				v3.ToString (42);
				Assert.Fail ("#C");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void HashCode ()
		{
			Version v1 = new Version ("1.2.3.4");
			Version v2 = new Version (1, 2, 3, 4);
			Assert.AreEqual (v1.GetHashCode (), v2.GetHashCode (), "HashCode");
		}
#if NET_2_0
		[Test]
		public void MajorMinorRevisions ()
		{
			Version v = new Version ();
			Assert.AreEqual (-1, v.MajorRevision, "MajorRevision/Empty");
			Assert.AreEqual (-1, v.MinorRevision, "MinorRevision/Empty");
			v = new Version ("1.2.3.4");
			Assert.AreEqual (0, v.MajorRevision, "MajorRevision/string");
			Assert.AreEqual (4, v.MinorRevision, "MinorRevision/string");
			v = new Version (1, 2);
			Assert.AreEqual (-1, v.MajorRevision, "MajorRevision/int,int");
			Assert.AreEqual (-1, v.MinorRevision, "MinorRevision/int,int");
			v = new Version (10, 20, 30);
			Assert.AreEqual (-1, v.MajorRevision, "MajorRevision/int,int,int");
			Assert.AreEqual (-1, v.MinorRevision, "MinorRevision/int,int,int");
			v = new Version (100, 200, 300, 400);
			Assert.AreEqual (0, v.MajorRevision, "MajorRevision/int,int,int,int");
			Assert.AreEqual (400, v.MinorRevision, "MinorRevision/int,int,int,int");
			v = new Version (100, 200, 300, (256 << 16) | 384);
			Assert.AreEqual (256, v.MajorRevision, "MajorRevision/int,int,int,bint");
			Assert.AreEqual (384, v.MinorRevision, "MinorRevision/int,int,int,bint");
		}

		[Test]
		public void CompareTo ()
		{
			Version v1234 = new Version (1, 2, 3, 4);
			Version vnull = null;
			Assert.AreEqual (1, v1234.CompareTo (vnull), "1234-null");
			Version v2234 = new Version (2, 2, 3, 4);
			Assert.AreEqual (1, v2234.CompareTo (v1234), "2234-1234");
			Version v1224 = new Version (1, 2, 2, 4);
			Assert.AreEqual (-1, v1224.CompareTo (v1234), "1224-1234");
			Version v1235 = new Version (1, 2, 3, 5);
			Assert.AreEqual (1, v1235.CompareTo (v1234), "1235-1234");
		}
#endif

#if NET_4_0
		[Test]
		public void Parse ()
		{
			Assert.AreEqual (new Version (1, 7), Version.Parse ("1.7"), "#A1");
			Assert.AreEqual (new Version (1, 7, 3), Version.Parse ("1.7.3"), "#A2");
			Assert.AreEqual (new Version (1, 7, 3, 4001), Version.Parse ("1.7.3.4001"), "#A3");
			Assert.AreEqual (new Version (2, 9), Version.Parse ("  2.9  "), "#A4");
			Assert.AreEqual (new Version (2, 9, 3), Version.Parse ("2. 9. 3"), "#A5");

			try {
				Version.Parse (null);
				Assert.Fail ("#EXC1");
			} catch (ArgumentNullException) {
			}

			try {
				Version.Parse ("1");
				Assert.Fail ("#EXC2");
			} catch (ArgumentException) {
			}

			try {
				Version.Parse ("1.4.7.8.9");
				Assert.Fail ("#EXC3");
			} catch (ArgumentException) {
			}

			try {
				Version.Parse ("1.2.a.7");
				Assert.Fail ("#EXC4");
			} catch (FormatException) {
			}

			try {
				Version.Parse ("1.2.");
				Assert.Fail ("#EXC5");
			} catch (FormatException) {
			}

			try {
				Version.Parse ("1.2.0." + ((long)Int32.MaxValue + 1).ToString ());
				Assert.Fail ("#EXC6");
			} catch (OverflowException) {
			}

			try {
				Version.Parse ("1.3.-6.0");
				Assert.Fail ("#EXC7");
			} catch (ArgumentOutOfRangeException) {
			}
		}

		[Test]
		public void TryParse ()
		{
			Version result;
			bool success;

			success = Version.TryParse ("1.7", out result);
			Assert.AreEqual (new Version (1, 7), result, "#A1");
			Assert.AreEqual (true, success, "#A2");

			success = Version.TryParse ("1.7.3", out result);
			Assert.AreEqual (new Version (1, 7, 3), result, "#B1");
			Assert.AreEqual (true, success, "#B2");

			success = Version.TryParse ("1.7.0.4001", out result);
			Assert.AreEqual (new Version (1, 7, 0, 4001), result, "#C1");
			Assert.AreEqual (true, success, "#C2");

			success = Version.TryParse ("  2.9  ", out result);
			Assert.AreEqual (new Version (2, 9), result, "#D1");
			Assert.AreEqual (true, success, "#D2");

			success = Version.TryParse ("2. 9. 3", out result);
			Assert.AreEqual (new Version (2, 9, 3), result, "#E1");
			Assert.AreEqual (true, success, "#E2");

			//
			// Errors
			//
			success = Version.TryParse (null, out result);
			Assert.AreEqual (null, result, "#F1");
			Assert.AreEqual (false, success, "#F2");

			success = Version.TryParse ("1", out result);
			Assert.AreEqual (null, result, "#G1");
			Assert.AreEqual (false, success, "#G2");

			success = Version.TryParse ("1.4.7.8.9", out result);
			Assert.AreEqual (null, result, "#H1");
			Assert.AreEqual (false, success, "#H2");

			success = Version.TryParse ("1.2.a.7", out result);
			Assert.AreEqual (null, result, "#I1");
			Assert.AreEqual (false, success, "#I2");

			success = Version.TryParse ("1.2.", out result);
			Assert.AreEqual (null, result, "#J1");
			Assert.AreEqual (false, success, "#J2");

			success = Version.TryParse ("1.2.0." + ((long)Int32.MaxValue + 1).ToString (), out result);
			Assert.AreEqual (null, result, "#K1");
			Assert.AreEqual (false, success, "#K2");

			success = Version.TryParse ("1.-6", out result);
			Assert.AreEqual (null, result, "#L1");
			Assert.AreEqual (false, success, "#L2");
		}
#endif
	}
}
