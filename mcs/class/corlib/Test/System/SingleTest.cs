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

#if !MOBILE
using NUnit.Framework.SyntaxHelpers;
#endif

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
		public void ToString_Roundtrip ()
		{
			Assert.AreEqual (10.78f.ToString ("R", NumberFormatInfo.InvariantInfo), "10.78");
		}

		[Test]
		public void Parse_Roundtrip ()
		{
			string maxVal = float.MaxValue.ToString ("r");
			string minVal = float.MinValue.ToString ("r");
			string epsilon = float.Epsilon.ToString ("r");
			string nan = float.NaN.ToString ("r");
			string negInf = float.NegativeInfinity.ToString ("r");
			string posInf = float.PositiveInfinity.ToString ("r");

			float result;
			Assert.IsTrue (float.TryParse (maxVal, NumberStyles.Float, CultureInfo.CurrentCulture, out result), "MaxValue#1a");
			Assert.AreEqual (float.MaxValue, result, "MaxValue#1b");
			Assert.IsTrue (float.TryParse (minVal, NumberStyles.Float, CultureInfo.CurrentCulture, out result), "MinValue#1a");
			Assert.AreEqual (float.MinValue, result, "MinValue#1b");
			Assert.IsTrue (float.TryParse (epsilon, NumberStyles.Float, CultureInfo.CurrentCulture, out result), "Epsilon#1a");
			Assert.AreEqual (float.Epsilon, result, "Epsilon#1b");
			Assert.IsTrue (float.TryParse (nan, NumberStyles.Float, CultureInfo.CurrentCulture, out result), "NaN#1a");
			Assert.AreEqual (float.NaN, result, "NaN#1b");
			Assert.That (result, Is.NaN, "NaN#1c");
			Assert.IsTrue (float.TryParse (negInf, NumberStyles.Float, CultureInfo.CurrentCulture, out result), "-Inf#1a");
			Assert.AreEqual (float.NegativeInfinity, result, "-Inf#1b");
			Assert.IsTrue (float.TryParse (posInf, NumberStyles.Float, CultureInfo.CurrentCulture, out result), "+Inf#1a");
			Assert.AreEqual (float.PositiveInfinity, result, "+Inf#1b");

			Assert.AreEqual (float.MaxValue, float.Parse (maxVal), "MaxValue#2");
			Assert.AreEqual (float.MinValue, float.Parse (minVal), "MinValue#2");
			Assert.AreEqual (float.Epsilon, float.Parse (epsilon), "Epsilon#2");
			Assert.AreEqual (float.NaN, float.Parse (nan), "NaN#2a");
			Assert.That (float.Parse (nan), Is.NaN, "NaN#2b");
			Assert.AreEqual (float.NegativeInfinity, float.Parse (negInf), "-Inf#2");
			Assert.AreEqual (float.PositiveInfinity, float.Parse (posInf), "+Inf#2");

			Assert.AreEqual (float.MaxValue, float.Parse (maxVal, CultureInfo.CurrentCulture), "MaxValue#3");
			Assert.AreEqual (float.MinValue, float.Parse (minVal, CultureInfo.CurrentCulture), "MinValue#3");
			Assert.AreEqual (float.Epsilon, float.Parse (epsilon, CultureInfo.CurrentCulture), "Epsilon#3");
			Assert.AreEqual (float.NaN, float.Parse (nan, CultureInfo.CurrentCulture), "NaN#3a");
			Assert.That (float.Parse (nan, CultureInfo.CurrentCulture), Is.NaN, "NaN#3b");
			Assert.AreEqual (float.NegativeInfinity, float.Parse (negInf, CultureInfo.CurrentCulture), "-Inf#3");
			Assert.AreEqual (float.PositiveInfinity, float.Parse (posInf, CultureInfo.CurrentCulture), "+Inf#3");

			Assert.AreEqual (float.MaxValue, float.Parse (maxVal, NumberStyles.Float), "MaxValue#4");
			Assert.AreEqual (float.MinValue, float.Parse (minVal, NumberStyles.Float), "MinValue#4");
			Assert.AreEqual (float.Epsilon, float.Parse (epsilon, NumberStyles.Float), "Epsilon#4");
			Assert.AreEqual (float.NaN, float.Parse (nan, NumberStyles.Float), "NaN#4a");
			Assert.That (float.Parse (nan, NumberStyles.Float), Is.NaN, "NaN#4b");
			Assert.AreEqual (float.NegativeInfinity, float.Parse (negInf, NumberStyles.Float), "-Inf#4");
			Assert.AreEqual (float.PositiveInfinity, float.Parse (posInf, NumberStyles.Float), "+Inf#4");

			Assert.AreEqual (float.MaxValue, float.Parse (maxVal, NumberStyles.Float, CultureInfo.CurrentCulture), "MaxValue#5");
			Assert.AreEqual (float.MinValue, float.Parse (minVal, NumberStyles.Float, CultureInfo.CurrentCulture), "MinValue#5");
			Assert.AreEqual (float.Epsilon, float.Parse (epsilon, NumberStyles.Float, CultureInfo.CurrentCulture), "Epsilon#5");
			Assert.AreEqual (float.NaN, float.Parse (nan, NumberStyles.Float, CultureInfo.CurrentCulture), "NaN#5a");
			Assert.That (float.Parse (nan, NumberStyles.Float, CultureInfo.CurrentCulture), Is.NaN, "NaN#5b");
			Assert.AreEqual (float.NegativeInfinity, float.Parse (negInf, NumberStyles.Float, CultureInfo.CurrentCulture), "-Inf#5");
			Assert.AreEqual (float.PositiveInfinity, float.Parse (posInf, NumberStyles.Float, CultureInfo.CurrentCulture), "+Inf#5");
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
