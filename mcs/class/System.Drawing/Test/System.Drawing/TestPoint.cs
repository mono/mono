// Tests for System.Drawing.Point.cs
//
// Author: Mike Kestner (mkestner@speakeasy.net)
//
// Copyright (c) 2001 Ximian, Inc.

using NUnit.Framework;
using System;
using System.Drawing;

public class PointTest : TestCase {
	Point pt1_1;
	Point pt1_0;
	Point pt0_1;

	protected override void SetUp ()
	{
		pt1_1 = new Point (1, 1);
		pt1_0 = new Point (1, 0);
		pt0_1 = new Point (0, 1);
	}

	public PointTest(String name) : base (name) {}

	public static ITest Suite {
		get {
			TestSuite suite = new TestSuite ();
			suite.AddTest (new PointTest ("EqualsTest"));
			suite.AddTest (new PointTest ("EqualityOpTest"));
			suite.AddTest (new PointTest ("InequalityOpTest"));
			suite.AddTest (new PointTest ("CeilingTest"));
			suite.AddTest (new PointTest ("RoundTest"));
			suite.AddTest (new PointTest ("TruncateTest"));
			suite.AddTest (new PointTest ("NullTest"));
			suite.AddTest (new PointTest ("AdditionTest"));
			suite.AddTest (new PointTest ("SubtractionTest"));
			suite.AddTest (new PointTest ("Point2SizeTest"));
			suite.AddTest (new PointTest ("Point2PointFTest"));
			suite.AddTest (new PointTest ("ConstructorTest"));
			suite.AddTest (new PointTest ("PropertyTest"));
			suite.AddTest (new PointTest ("OffsetTest"));
			return suite;
		}
	}

	public void EqualsTest () 
	{
		AssertEquals (pt1_1, pt1_1);
		AssertEquals (pt1_1, new Point (1, 1));
		Assert (!pt1_1.Equals (pt1_0));
		Assert (!pt1_1.Equals (pt0_1));
		Assert (!pt1_0.Equals (pt0_1));
	}

	public void EqualityOpTest () 
	{
		Assert (pt1_1 == pt1_1);
		Assert (pt1_1 == new Point (1, 1));
		Assert (!(pt1_1 == pt1_0));
		Assert (!(pt1_1 == pt0_1));
		Assert (!(pt1_0 == pt0_1));
	}

	public void InequalityOpTest () 
	{
		Assert (!(pt1_1 != pt1_1));
		Assert (!(pt1_1 != new Point (1, 1)));
		Assert (pt1_1 != pt1_0);
		Assert (pt1_1 != pt0_1);
		Assert (pt1_0 != pt0_1);
	}

	public void CeilingTest () 
	{
		PointF ptf = new PointF (0.8f, 0.3f);
		AssertEquals (pt1_1, Point.Ceiling (ptf));
	}

	public void RoundTest () 
	{
		PointF ptf = new PointF (0.8f, 1.3f);
		AssertEquals (pt1_1, Point.Round (ptf));
	}

	public void TruncateTest () 
	{
		PointF ptf = new PointF (0.8f, 1.3f);
		AssertEquals (pt0_1, Point.Truncate (ptf));
	}

	public void NullTest () 
	{
		Point pt = new Point (0, 0);
		AssertEquals (pt, Point.Empty);
	}

	public void AdditionTest () 
	{
		AssertEquals (pt1_1, pt1_0 + new Size (0, 1));
		AssertEquals (pt1_1, pt0_1 + new Size (1, 0));
	}

	public void SubtractionTest () 
	{
		AssertEquals (pt1_0, pt1_1 - new Size (0, 1));
		AssertEquals (pt0_1, pt1_1 - new Size (1, 0));
	}

	public void Point2SizeTest () 
	{
		Size sz1 = new Size (1, 1);
		Size sz2 = (Size) pt1_1;

		AssertEquals (sz1, sz2);
	}

	public void Point2PointFTest () 
	{
		PointF ptf1 = new PointF (1, 1);
		PointF ptf2 = pt1_1;

		AssertEquals (ptf1, ptf2);
	}

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

	public void PropertyTest () 
	{
		Point pt = new Point (0, 0);

		Assert (pt.IsEmpty);
		Assert (!pt1_1.IsEmpty);
		AssertEquals (1, pt1_0.X);
		AssertEquals (1, pt0_1.Y);
	}

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


