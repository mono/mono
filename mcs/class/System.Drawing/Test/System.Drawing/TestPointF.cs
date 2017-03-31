// Tests for System.Drawing.PointF.cs
//
// Author:
//	Ravindra (rkumar@novell.com)
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Drawing;
using System.Globalization;
using System.Security.Permissions;
using System.Threading;

using NUnit.Framework;

namespace MonoTests.System.Drawing
{
	[TestFixture]	
	public class PointFTest
	{
		PointF pt11_99;
		PointF pt11_0;
		PointF pt0_11;

		[TearDown]
		public void TearDown () {}

		[SetUp]
		public void SetUp ()
		{
			pt11_99 = new PointF (1.1F, 9.9F);
			pt11_0 = new PointF (1.1F, 0F);
			pt0_11 = new PointF (0F, 1.1F);
		}

		[Test]
		public void TestConstructors ()
		{
			PointF pt = new PointF (1.5F, 5.8F);
			Assert.AreEqual (1.5F, pt.X, "C#1");
			Assert.AreEqual (5.8F, pt.Y, "C#2");
		}

		[Test]
		public void TestEmptyField () 
		{
			PointF pt = new PointF (0.0F, 0.0F);
			Assert.AreEqual (pt, PointF.Empty, "#EMP1");
		}

		[Test]
		public void TestProperties () 
		{
			PointF pt = new PointF (0.0F, 0.0F);
	
			Assert.IsTrue (pt.IsEmpty, "P#1");
			Assert.IsTrue (!pt11_99.IsEmpty, "P#2");
			Assert.AreEqual (1.1F, pt11_0.X, "P#3");
			Assert.AreEqual (1.1F, pt0_11.Y, "P#4");
		}

		[Test]
		public void TestEquals () 
		{
			Assert.AreEqual (pt11_99, pt11_99, "EQ#1");
			Assert.AreEqual (pt11_99, new PointF (1.1F, 9.9F), "EQ#2");
			Assert.IsFalse (pt11_99.Equals (pt11_0), "EQ#3");
			Assert.IsFalse (pt11_99.Equals (pt0_11), "EQ#4");
			Assert.IsFalse (pt11_0.Equals (pt0_11), "EQ#5");
		}

		
		[Test]
		public void TestAddition ()
		{
			Assert.AreEqual (pt11_0, pt11_0 + new Size (0, 0), "ADD#1");
			Assert.AreEqual (pt0_11, pt0_11 + new Size (0, 0), "ADD#2");
			Assert.AreEqual (new PointF (2, 5.1F), pt0_11 + new Size (2, 4), "ADD#3");
		}

		[Test]
		public void TestEqualityOp () 
		{
#pragma warning disable 1718 // Comparison made to same variable
			Assert.IsTrue (pt11_99 == pt11_99, "EOP#1");
#pragma warning restore 1718
			Assert.IsTrue (pt11_99 == new PointF (1.1F, 9.9F), "EOP#2");
			Assert.IsFalse (pt11_99 == pt11_0, "EOP#3");
			Assert.IsFalse (pt11_99 == pt0_11, "EOP#4");
			Assert.IsFalse (pt11_0 == pt0_11, "EOP#5");
		}

		[Test]
		public void TestInequalityOp () 
		{
#pragma warning disable 1718 // Comparison made to same variable
			Assert.IsFalse (pt11_99 != pt11_99, "IOP#1");
#pragma warning restore 1718
			Assert.IsFalse (pt11_99 != new PointF (1.1F, 9.9F), "IOP#2");
			Assert.IsTrue (pt11_99 != pt11_0, "IOP#3");
			Assert.IsTrue (pt11_99 != pt0_11, "IOP#4");
			Assert.IsTrue (pt11_0 != pt0_11, "IOP#5");
		}
	
		[Test]
		public void TestSubtraction () 
		{
			Assert.AreEqual (pt11_0, pt11_0 - new Size (0, 0), "SUB#1");
			Assert.AreEqual (pt0_11, pt0_11 - new Size (0, 0), "SUB#2");
			PointF expected = new PointF (0.1F, 1.9F);
			PointF actual = pt11_99 - new Size (1, 8);
			//need to permit a small delta on floating point
			Assert.AreEqual (expected.X, actual.X, 1e-5, "SUB#3");
			Assert.AreEqual (expected.Y, actual.Y, 1e-5, "SUB#4");
		}

		[Test]
		public void GetHashCodeTest ()
		{
			PointF pt = new PointF (1.1F, 9.9F);
			Assert.AreEqual (pt.GetHashCode (), pt11_99.GetHashCode (), "GHC#1");
		}

		[Test]
		public void ToStringTest ()
		{
			// save current culture
			CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;

			try {
				PerformToStringTest (new CultureInfo ("en-US"));
				PerformToStringTest (new CultureInfo ("nl-BE"));
			} finally {
				// restore original culture
				Thread.CurrentThread.CurrentCulture = currentCulture;
			}
		}

		private void PerformToStringTest(CultureInfo culture)
		{
			// set current culture
			Thread.CurrentThread.CurrentCulture = culture;

			// perform tests
			Assert.AreEqual (GetExpectedToString (culture, pt0_11), pt0_11.ToString (),
				"TS#1-" + culture.Name);
			Assert.AreEqual (GetExpectedToString (culture, pt11_0), pt11_0.ToString (),
				"TS#2-" + culture.Name);
			Assert.AreEqual (GetExpectedToString (culture, pt11_99), pt11_99.ToString (),
				"TS#3-" + culture.Name);
			PointF pt = new PointF (float.NaN, float.NegativeInfinity);
			Assert.AreEqual (GetExpectedToString (culture, pt), pt.ToString (),
				"TS#4-" + culture.Name);
		}

		private static string GetExpectedToString (CultureInfo culture, PointF point)
		{
			return string.Format ("{{X={0}, Y={1}}}", point.X.ToString (culture),
				point.Y.ToString (culture));
		}


		[Test]
		public void AddTest ()
		{
			Assert.AreEqual (new PointF (3, 4), PointF.Add (new PointF (1, 1), new Size (2, 3)), "ADDTEST#1");
			Assert.AreEqual (new PointF (4, 5), PointF.Add (new PointF (2, 2), new SizeF (2, 3)), "ADDTEST#2");			
		}

		[Test]
		public void SubtractTest ()
		{
			Assert.AreEqual (new PointF (2, 1), PointF.Subtract (new PointF (4, 4), new Size (2, 3)), "SUBTEST#1");
			Assert.AreEqual (new PointF (3, 3), PointF.Subtract (new PointF (5, 6), new SizeF (2, 3)), "SUBTEST#2");						
		}


	}
}

