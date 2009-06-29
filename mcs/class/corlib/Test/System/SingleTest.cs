// 
// System.SingleTest.cs - Unit test for Single
//	adapted from a subset of DoubleTest.cs
//
// Authors
//	Bob Doan  <bdoan@sicompos.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
//

using System;
using System.Globalization;
using System.Threading;

using NUnit.Framework;

namespace MonoTests.System
{
	[TestFixture]
	public class SingleTest 
	{
		CultureInfo old_culture;

		[SetUp]
		public void SetUp ()
		{
			old_culture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US", false);
		}

		[TearDown]
		public void TearDown ()
		{
			Thread.CurrentThread.CurrentCulture = old_culture;
		}

		[Test]
		public void Equals ()
		{
			Single s1 = 1f;
			Single s2 = 1f;
			Assert.IsTrue (s1.Equals (s2), "Equals s1==s2");
			Assert.IsTrue (!s1.Equals (Single.NaN), "Equals s1!=NaN");

			Assert.IsTrue (!Single.NaN.Equals (s2), "Equals NaN=!s2");
			Assert.IsTrue (Single.NaN.Equals (Single.NaN), "Equals NaN==NaN");

			Single p0 = 0.0f;
			Single m0 = -0.0f;
			Assert.IsTrue (p0.Equals (m0), "0.0==-0.0");
			Assert.IsTrue (m0.Equals (p0), "-0.0==0.0");
		}

		[Test]
		public void IsInfinity ()
		{
			Assert.IsTrue ( Single.IsInfinity (Single.PositiveInfinity), "PositiveInfinity");
			Assert.IsTrue (Single.IsInfinity (Single.NegativeInfinity), "NegativeInfinity");
			Assert.IsTrue (!Single.IsInfinity(12), "12");
			Assert.IsTrue (!Single.IsInfinity (Single.NaN), "NaN");
		}

		[Test]
		public void IsNan ()
		{
			Assert.IsTrue (Single.IsNaN (Single.NaN), "Nan");
			Assert.IsTrue (!Single.IsNaN (12), "12");
			Assert.IsTrue (!Single.IsNaN (Single.PositiveInfinity), "PositiveInfinity");
			Assert.IsTrue (!Single.IsNaN (Single.PositiveInfinity), "NegativeInfinity");
		}

		[Test]
		public void IsNegativeInfinity ()
		{
			Assert.IsTrue (Single.IsNegativeInfinity (Single.NegativeInfinity), "IsNegativeInfinity");
			Assert.IsTrue (!Single.IsNegativeInfinity (12), "12");
			Assert.IsTrue (!Single.IsNegativeInfinity (Single.NaN), "NaN");
		}

		[Test]
		public void IsPositiveInfinity ()
		{
			Assert.IsTrue (Single.IsPositiveInfinity (Single.PositiveInfinity), "PositiveInfinity");
			Assert.IsTrue (!Single.IsPositiveInfinity (12), "12");
			Assert.IsTrue (!Single.IsPositiveInfinity (Single.NaN), "NaN");
		}

		[Test]
		public void ToString_Defaults () 
		{
			Single i = 254.9f;
			// everything defaults to "G"
			string def = i.ToString ("G");
			Assert.AreEqual (def, i.ToString (), "ToString()");
			Assert.AreEqual (def, i.ToString ((IFormatProvider)null), "ToString((IFormatProvider)null)");
			Assert.AreEqual (def, i.ToString ((string)null), "ToString((string)null)");
			Assert.AreEqual (def, i.ToString (String.Empty), "ToString(empty)");
			Assert.AreEqual (def, i.ToString (null, null), "ToString(null,null)");
			Assert.AreEqual (def, i.ToString (String.Empty, null), "ToString(empty,null)");
			Assert.AreEqual ("254.9", def, "ToString(G)");
		}

		[Test]
		[Category ("NotWorking")]
		public void ToString_Roundtrip ()
		{
			Assert.AreEqual (10.78f.ToString ("R", NumberFormatInfo.InvariantInfo), "10.78");
		}

#if NET_2_0
		[Test] // bug #72221
		[ExpectedException (typeof (ArgumentException))]
		public void HexNumber_WithHexToParse ()
		{
			float f;
			Single.TryParse ("0dead", NumberStyles.HexNumber, null, out f);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void HexNumber_NoHexToParse ()
		{
			float f;
			Single.TryParse ("0", NumberStyles.HexNumber, null, out f);
		}
#endif
	}
}
