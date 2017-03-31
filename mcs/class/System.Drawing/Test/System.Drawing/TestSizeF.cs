// Tests for System.Drawing.SizeF.cs
//
// Author: Ravindra (rkumar@novell.com)
//
//	Modified TestPoint.cs for testing SizeF.cs.
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
using System.Security.Permissions;
using System.Globalization;
using System.Threading;

using NUnit.Framework;

namespace MonoTests.System.Drawing 
{
	[TestFixture]	
	public class SizeFTest
	{
		SizeF sz11_99;
		SizeF sz11_0;
		SizeF sz0_11;

		[SetUp]
		public void SetUp ()
		{
			sz11_99 = new SizeF (1.1F, 9.9F);
			sz11_0 = new SizeF (1.1F, 0F);
			sz0_11 = new SizeF (0F, 1.1F);
		}

		[Test]
		public void TestConstructors ()
		{
			SizeF sz_wh = new SizeF (1.5F, 5.8F);
			Assert.AreEqual (1.5F, sz_wh.Width, "C#1");
			Assert.AreEqual (5.8F, sz_wh.Height, "C#2");

			SizeF sz_pf = new SizeF (new PointF (1.5F, 5.8F));
			Assert.AreEqual (1.5F, sz_pf.Width, "C#3");
			Assert.AreEqual (5.8F, sz_pf.Height, "C#4");

			SizeF sz_sz = new SizeF (sz_wh);
			Assert.AreEqual (1.5F, sz_sz.Width, "C#5");
			Assert.AreEqual (5.8F, sz_sz.Height, "C#6");

			Assert.AreEqual (sz_wh, sz_pf, "C#7");
			Assert.AreEqual (sz_pf, sz_sz, "C#8");
			Assert.AreEqual (sz_wh, sz_sz, "C#9");
		}

		[Test]
		public void TestEmptyField () 
		{
			SizeF sz = new SizeF (0.0F, 0.0F);
			Assert.AreEqual (sz, SizeF.Empty, "EMP#1");
		}

		[Test]
		public void TestProperties () 
		{
			SizeF sz = new SizeF (0.0F, 0.0F);

			Assert.IsTrue (sz.IsEmpty, "P#1");
			Assert.IsFalse (sz11_99.IsEmpty, "P#2");
			Assert.AreEqual (1.1F, sz11_0.Width, "P#3");
			Assert.AreEqual (1.1F, sz0_11.Height, "P#4");
		}

		[Test]
		public void TestEquals () 
		{
			Assert.AreEqual (sz11_99, sz11_99, "EQ#1");
			Assert.AreEqual (sz11_99, new SizeF (1.1F, 9.9F), "EQ#2");
			Assert.IsFalse (sz11_99.Equals (sz11_0), "EQ#3");
			Assert.IsFalse (sz11_99.Equals (sz0_11), "EQ#4");
			Assert.IsFalse (sz11_0.Equals (sz0_11), "EQ#5");
		}

		[Test]
		public void Test2PointF ()
		{
			PointF p1 = new PointF (1.1F, 9.9F);
			PointF p2 = sz11_99.ToPointF ();

			Assert.AreEqual (p1, p2, "2PF#1");
		}
		
		[Test]
		public void Test2Size ()
		{
			// note: using Size (not SizeF) is normal for this test
			Size sz1 = new Size (1, 9);
			Size sz2 = sz11_99.ToSize ();

			Assert.AreEqual (sz1, sz2, "2SZ#1");
		}

		
		[Test]
		public void TestAddition ()
		{
			Assert.AreEqual (sz11_99, sz11_0 + new SizeF (0.0F, 9.9F), "ADD#1");
			Assert.AreEqual (sz11_99, new SizeF (0.0F, 0.0F) + new SizeF (1.1F, 9.9F), "ADD#2");
		}

		[Test]
		public void TestEqualityOp () 
		{
#pragma warning disable 1718 // Comparison made to same variable
			Assert.IsTrue (sz11_99 == sz11_99, "EOP#1");
#pragma warning restore 1718
			Assert.IsTrue (sz11_99 == new SizeF (1.1F, 9.9F), "EOP#2");
			Assert.IsFalse (sz11_99 == sz11_0, "EOP#3");
			Assert.IsFalse (sz11_99 == sz0_11, "EOP#4");
			Assert.IsFalse (sz11_0 == sz0_11, "EOP#5");
		}

		[Test]
		public void TestInequalityOp () 
		{
#pragma warning disable 1718 // Comparison made to same variable
			Assert.IsFalse (sz11_99 != sz11_99, "IOP#1");
#pragma warning restore 1718
			Assert.IsFalse (sz11_99 != new SizeF (1.1F, 9.9F), "IOP#2");
			Assert.IsTrue (sz11_99 != sz11_0, "IOP#3");
			Assert.IsTrue (sz11_99 != sz0_11, "IOP#4");
			Assert.IsTrue (sz11_0 != sz0_11, "IOP#5");
		}
	
		[Test]
		public void TestSubtraction () 
		{
			Assert.AreEqual (sz11_0, sz11_99 - new SizeF (0.0F, 9.9F), "SUB#1");
			Assert.AreEqual (sz0_11, new SizeF (1.1F, 1.1F) - new SizeF (1.1F, 0.0F), "SUB#2");
		}
	
		[Test]
		public void TestSizeF2PointF ()
		{
			PointF pf1 = new PointF (1.1F, 9.9F);
			PointF pf2 = (PointF) sz11_99;

			Assert.AreEqual (pf1, pf2, "SF2PF#1");
		}

		[Test]
		public void GetHashCodeTest ()
		{
			Assert.AreEqual (sz11_0.GetHashCode (), new SizeF (1.1f, 0).GetHashCode (), "GHC#1");
			Assert.AreEqual (SizeF.Empty.GetHashCode (), new SizeF (0, 0).GetHashCode (), "GHC#2");
		}

		[Test]
		public void ToStringTest () {
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

		private void PerformToStringTest (CultureInfo culture)
		{
			// set current culture
			Thread.CurrentThread.CurrentCulture = culture;

			// perform tests
			Assert.AreEqual (GetExpectedToString (culture, sz11_0), sz11_0.ToString (),
				"TS#1-" + culture.Name);
			Assert.AreEqual (GetExpectedToString (culture, sz0_11), sz0_11.ToString (),
				"TS#2-" + culture.Name);
			Assert.AreEqual (GetExpectedToString (culture, SizeF.Empty), SizeF.Empty.ToString (),
				"TS#3-" + culture.Name);
			SizeF size = new SizeF (float.NaN, float.NegativeInfinity);
			Assert.AreEqual (GetExpectedToString (culture, size), size.ToString (),
				"TS#4-" + culture.Name);
		}

		private static string GetExpectedToString (CultureInfo culture, SizeF size)
		{
			return string.Format ("{{Width={0}, Height={1}}}", size.Width.ToString (culture),
				size.Height.ToString (culture));
		}

		[Test]
		public void AddTest ()
		{
			Assert.AreEqual (sz11_99, SizeF.Add (sz11_0, new SizeF (0.0F, 9.9F)), "ADD#1");
			Assert.AreEqual (sz11_99, SizeF.Add (new SizeF (0.0F, 0.0F), new SizeF (1.1F, 9.9F)), "ADD#2");
		}

		[Test]
		public void SubtractTest ()
		{
			Assert.AreEqual (sz11_0, SizeF.Subtract (sz11_99, new SizeF (0.0F, 9.9F)), "SUB#1");
			Assert.AreEqual (sz0_11, SizeF.Subtract (new SizeF (1.1F, 1.1F), new SizeF (1.1F, 0.0F)), "SUB#2");
		}

	}
}

