// Tests for System.Drawing.Point.cs
//
// Author: Mike Kestner (mkestner@speakeasy.net)
// 		   Improvements by Jordi Mas i Hernàndez <jmas@softcatala.org>
// Copyright (c) 2001 Ximian, Inc.

using NUnit.Framework;
using System;
using System.Drawing;

namespace MonoTests.System.Drawing{

	[TestFixture]	
	public class PointTest : Assertion {
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
			AssertEquals (pt1_1, pt1_1);
			AssertEquals (pt1_1, new Point (1, 1));
			Assert (!pt1_1.Equals (pt1_0));
			Assert (!pt1_1.Equals (pt0_1));
			Assert (!pt1_0.Equals (pt0_1));
		}
		
		[Test]
		public void EqualityOpTest () 
		{
			Assert (pt1_1 == pt1_1);
			Assert (pt1_1 == new Point (1, 1));
			Assert (!(pt1_1 == pt1_0));
			Assert (!(pt1_1 == pt0_1));
			Assert (!(pt1_0 == pt0_1));
		}

		[Test]
		public void InequalityOpTest () 
		{
			Assert (!(pt1_1 != pt1_1));
			Assert (!(pt1_1 != new Point (1, 1)));
			Assert (pt1_1 != pt1_0);
			Assert (pt1_1 != pt0_1);
			Assert (pt1_0 != pt0_1);
		}
	
		[Test]
		public void CeilingTest () 
		{
			PointF ptf = new PointF (0.8f, 0.3f);
			AssertEquals (pt1_1, Point.Ceiling (ptf));
		}
	
		[Test]
		public void RoundTest () 
		{
			PointF ptf = new PointF (0.8f, 1.3f);
			AssertEquals (pt1_1, Point.Round (ptf));
		}
	
		[Test]
		public void TruncateTest () 
		{
			PointF ptf = new PointF (0.8f, 1.3f);
			AssertEquals (pt0_1, Point.Truncate (ptf));
		}
	
		[Test]
		public void NullTest () 
		{
			Point pt = new Point (0, 0);
			AssertEquals (pt, Point.Empty);
		}
	
		[Test]
		public void AdditionTest () 
		{
			AssertEquals (pt1_1, pt1_0 + new Size (0, 1));
			AssertEquals (pt1_1, pt0_1 + new Size (1, 0));
		}
	
		[Test]
		public void SubtractionTest () 
		{
			AssertEquals (pt1_0, pt1_1 - new Size (0, 1));
			AssertEquals (pt0_1, pt1_1 - new Size (1, 0));
		}
	
		[Test]
		public void Point2SizeTest () 
		{
			Size sz1 = new Size (1, 1);
			Size sz2 = (Size) pt1_1;
	
			AssertEquals (sz1, sz2);
		}
	
		[Test]
		public void Point2PointFTest () 
		{
			PointF ptf1 = new PointF (1, 1);
			PointF ptf2 = pt1_1;
	
			AssertEquals (ptf1, ptf2);
		}
	
		[Test]
		public void ConstructorTest () 
		{
			int i = (1 << 16) + 1;
			Size sz = new Size (1, 1);
			Point pt_i = new Point (i);
			Point pt_sz = new Point (sz);
	
			AssertEquals (pt_i, pt_sz);
			AssertEquals (pt_i, pt1_1);
			AssertEquals (pt_sz, pt1_1);
		}
		
		[Test]
		public void PropertyTest () 
		{
			Point pt = new Point (0, 0);
	
			Assert (pt.IsEmpty);
			Assert (!pt1_1.IsEmpty);
			AssertEquals (1, pt1_0.X);
			AssertEquals (1, pt0_1.Y);
		}
		
		[Test]
		public void OffsetTest () 
		{
			Point pt = new Point (0, 0);
			pt.Offset (0, 1);
			AssertEquals (pt, pt0_1);
			pt.Offset (1, 0);
			AssertEquals (pt, pt1_1);
			pt.Offset (0, -1);
			AssertEquals (pt, pt1_0);
		}
	}
}

