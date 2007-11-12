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
	public class SingleTest : Assertion
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
			Assert ("Equals s1==s2", s1.Equals (s2));
			Assert ("Equals s1!=NaN", !s1.Equals (Single.NaN));

			Assert ("Equals NaN=!s2", !Single.NaN.Equals (s2));
			Assert ("Equals NaN==NaN", Single.NaN.Equals (Single.NaN));

			Single p0 = 0.0f;
			Single m0 = -0.0f;
			Assert ("0.0==-0.0", p0.Equals (m0));
			Assert ("-0.0==0.0", m0.Equals (p0));
		}

		[Test]
		public void IsInfinity ()
		{
			Assert ("PositiveInfinity",  Single.IsInfinity (Single.PositiveInfinity));
			Assert ("NegativeInfinity", Single.IsInfinity (Single.NegativeInfinity));
			Assert ("12", !Single.IsInfinity(12));
			Assert ("NaN", !Single.IsInfinity (Single.NaN));
		}

		[Test]
		public void IsNan ()
		{
			Assert ("Nan", Single.IsNaN (Single.NaN));
			Assert ("12", !Single.IsNaN (12));
			Assert ("PositiveInfinity", !Single.IsNaN (Single.PositiveInfinity));
			Assert ("NegativeInfinity", !Single.IsNaN (Single.PositiveInfinity));
		}

		[Test]
		public void IsNegativeInfinity ()
		{
			Assert ("IsNegativeInfinity", Single.IsNegativeInfinity (Single.NegativeInfinity));
			Assert ("12", !Single.IsNegativeInfinity (12));
			Assert ("NaN", !Single.IsNegativeInfinity (Single.NaN));
		}

		[Test]
		public void IsPositiveInfinity ()
		{
			Assert ("PositiveInfinity", Single.IsPositiveInfinity (Single.PositiveInfinity));
			Assert ("12", !Single.IsPositiveInfinity (12));
			Assert ("NaN", !Single.IsPositiveInfinity (Single.NaN));
		}

		[Test]
		public void ToString_Defaults () 
		{
			Single i = 254.9f;
			// everything defaults to "G"
			string def = i.ToString ("G");
			AssertEquals ("ToString()", def, i.ToString ());
			AssertEquals ("ToString((IFormatProvider)null)", def, i.ToString ((IFormatProvider)null));
			AssertEquals ("ToString((string)null)", def, i.ToString ((string)null));
			AssertEquals ("ToString(empty)", def, i.ToString (String.Empty));
			AssertEquals ("ToString(null,null)", def, i.ToString (null, null));
			AssertEquals ("ToString(empty,null)", def, i.ToString (String.Empty, null));
			AssertEquals ("ToString(G)", "254.9", def);
		}

		[Test]
		[Category ("NotWorking")]
		public void ToString_Roundtrip ()
		{
			AssertEquals ("10.78", 10.78f.ToString ("R", NumberFormatInfo.InvariantInfo));
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
