// Tests for System.Drawing.Point.cs
//
// Author: Mike Kestner (mkestner@speakeasy.net)
// 		   Improvements by Jordi Mas i Hernandez <jmas@softcatala.org>
// Copyright (c) 2001 Ximian, Inc.

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

using NUnit.Framework;
using System;
using System.Drawing;
using System.Security.Permissions;

namespace MonoTests.System.Drawing{

	[TestFixture]	
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class PointTest {
		Point pt1_1;
		Point pt1_0;
		Point pt0_1;
	
		[TearDown]
		public void Clean() {}
		
		[SetUp]
		public void GetReady()		
		{
			pt1_1 = new Point (1, 1);
			pt1_0 = new Point (1, 0);
			pt0_1 = new Point (0, 1);
		}
				
	
		[Test]
		public void EqualsTest () 
		{
			Assert.AreEqual (pt1_1, pt1_1, "#1");
			Assert.AreEqual (pt1_1, new Point (1, 1), "#2");
			Assert.IsTrue (!pt1_1.Equals (pt1_0), "#3");
			Assert.IsTrue (!pt1_1.Equals (pt0_1), "#4");
			Assert.IsTrue (!pt1_0.Equals (pt0_1), "#5");
		}
		
		[Test]
		public void EqualityOpTest () 
		{
#pragma warning disable 1718 // Comparison made to same variable
			Assert.IsTrue (pt1_1 == pt1_1, "#1");
#pragma warning restore 1718
			Assert.IsTrue (pt1_1 == new Point (1, 1), "#2");
			Assert.IsTrue (!(pt1_1 == pt1_0), "#3");
			Assert.IsTrue (!(pt1_1 == pt0_1), "#4");
			Assert.IsTrue (!(pt1_0 == pt0_1), "#5");
		}

		[Test]
		public void InequalityOpTest () 
		{
#pragma warning disable 1718 // Comparison made to same variable
			Assert.IsTrue (!(pt1_1 != pt1_1), "#1");
#pragma warning restore 1718
			Assert.IsTrue (!(pt1_1 != new Point (1, 1)), "#2");
			Assert.IsTrue (pt1_1 != pt1_0, "#3");
			Assert.IsTrue (pt1_1 != pt0_1, "#4");
			Assert.IsTrue (pt1_0 != pt0_1, "#5");
		}
	
		[Test]
		public void CeilingTest () 
		{
			PointF ptf = new PointF (0.8f, 0.3f);
			Assert.AreEqual (pt1_1, Point.Ceiling (ptf), "#1");
		}
	
		[Test]
		public void RoundTest () 
		{
			PointF ptf = new PointF (0.8f, 1.3f);
			Assert.AreEqual (pt1_1, Point.Round (ptf), "#1");
		}
	
		[Test]
		public void TruncateTest () 
		{
			PointF ptf = new PointF (0.8f, 1.3f);
			Assert.AreEqual (pt0_1, Point.Truncate (ptf), "#1");
		}
	
		[Test]
		public void NullTest () 
		{
			Point pt = new Point (0, 0);
			Assert.AreEqual (pt, Point.Empty, "#1");
		}
	
		[Test]
		public void AdditionTest () 
		{
			Assert.AreEqual (pt1_1, pt1_0 + new Size (0, 1), "#1");
			Assert.AreEqual (pt1_1, pt0_1 + new Size (1, 0), "#2");
		}
	
		[Test]
		public void SubtractionTest () 
		{
			Assert.AreEqual (pt1_0, pt1_1 - new Size (0, 1), "#1");
			Assert.AreEqual (pt0_1, pt1_1 - new Size (1, 0), "#2");
		}
	
		[Test]
		public void Point2SizeTest () 
		{
			Size sz1 = new Size (1, 1);
			Size sz2 = (Size) pt1_1;
	
			Assert.AreEqual (sz1, sz2, "#1");
		}
	
		[Test]
		public void Point2PointFTest () 
		{
			PointF ptf1 = new PointF (1, 1);
			PointF ptf2 = pt1_1;
	
			Assert.AreEqual (ptf1, ptf2, "#1");
		}
	
		[Test]
		public void ConstructorTest () 
		{
			int i = (1 << 16) + 1;
			Size sz = new Size (1, 1);
			Point pt_i = new Point (i);
			Point pt_sz = new Point (sz);
	
			Assert.AreEqual (pt_i, pt_sz, "#1");
			Assert.AreEqual (pt_i, pt1_1, "#2");
			Assert.AreEqual (pt_sz, pt1_1, "#3");
		}

		[Test]
		public void ConstructorNegativeLocationTest ()
		{
			var pt = new Point (unchecked ((int) 0xffe0fc00));

			Assert.AreEqual (-32, pt.Y, "#1"); // (short) 0xffe0
			Assert.AreEqual (-1024, pt.X, "#2"); // (short) 0xfc00
		}
		
		[Test]
		public void PropertyTest () 
		{
			Point pt = new Point (0, 0);
	
			Assert.IsTrue (pt.IsEmpty, "#1");
			Assert.IsTrue (!pt1_1.IsEmpty, "#2");
			Assert.AreEqual (1, pt1_0.X, "#3");
			Assert.AreEqual (1, pt0_1.Y, "#4");
		}
		
		[Test]
		public void OffsetTest () 
		{
			Point pt = new Point (0, 0);
			pt.Offset (0, 1);
			Assert.AreEqual (pt, pt0_1, "#1");
			pt.Offset (1, 0);
			Assert.AreEqual (pt, pt1_1, "#2");
			pt.Offset (0, -1);
			Assert.AreEqual (pt, pt1_0, "#3");
		}
		
		[Test]
		public void GetHashCodeTest ()
		{
			Assert.AreEqual (32, pt1_1.GetHashCode (), "#1");
			Assert.AreEqual (33, pt1_0.GetHashCode (), "#2");
			Assert.AreEqual (1, pt0_1.GetHashCode (), "#3");
			Point pt = new Point(0xFF, 0xFF00);
			Assert.AreEqual (57311, pt.GetHashCode (), "#4");
		}

		[Test]
		public void ToStringTest ()
		{
			Assert.AreEqual ("{X=1,Y=1}", pt1_1.ToString (), "#1");
			Assert.AreEqual ("{X=1,Y=0}", pt1_0.ToString (), "#2");
			Assert.AreEqual ("{X=0,Y=1}", pt0_1.ToString (), "#3");
		}

		[Test]
		public void AddTest ()
		{
			Assert.AreEqual (pt1_1, Point.Add (pt1_0, new Size (0, 1)), "#1");
			Assert.AreEqual (pt1_1, Point.Add (pt0_1, new Size (1, 0)), "#2");
		}

		[Test]
		public void OffsetTestPoint ()
		{
			Point pt = new Point (0, 0);
			pt.Offset (new Point (0, 1));
			Assert.AreEqual (pt, pt0_1, "#1");
			pt.Offset (new Point (1, 0));
			Assert.AreEqual (pt, pt1_1, "#2");
			pt.Offset (new Point (0, -1));
			Assert.AreEqual (pt, pt1_0, "#3");
		}

		[Test]
		public void SubtractTest ()
		{
			Assert.AreEqual (pt1_0, Point.Subtract (pt1_1, new Size (0, 1)), "#1");
			Assert.AreEqual (pt0_1, Point.Subtract (pt1_1, new Size (1, 0)), "#2");
		}

	}
}

