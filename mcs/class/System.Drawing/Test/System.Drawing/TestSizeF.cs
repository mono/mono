// Tests for System.Drawing.SizeF.cs
//
// Author: Ravindra (rkumar@novell.com)
//
//	Modified TestPoint.cs for testing SizeF.cs.
//
// Copyright (c) 2004 Novell, Inc.
//

using NUnit.Framework;
using System;
using System.Drawing;

namespace MonoTests.System.Drawing 
{
	[TestFixture]	
	public class SizeFTest : Assertion 
	{
		SizeF sz11_99;
		SizeF sz11_0;
		SizeF sz0_11;

		[TearDown]
		public void TearDown () {}

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
			AssertEquals ("C#1", 1.5F, sz_wh.Width);
			AssertEquals ("C#2", 5.8F, sz_wh.Height);

			SizeF sz_pf = new SizeF (new PointF (1.5F, 5.8F));
			AssertEquals ("C#3", 1.5F, sz_pf.Width);
			AssertEquals ("C#4", 5.8F, sz_pf.Height);

			SizeF sz_sz = new SizeF (sz_wh);
			AssertEquals ("C#5", 1.5F, sz_sz.Width);
			AssertEquals ("C#6", 5.8F, sz_sz.Height);

			AssertEquals ("C#7", sz_wh, sz_pf);
			AssertEquals ("C#8", sz_pf, sz_sz);
			AssertEquals ("C#9", sz_wh, sz_sz);
		}

		[Test]
		public void TestEmptyField () 
		{
			SizeF sz = new SizeF (0.0F, 0.0F);
			AssertEquals ("EMP#1", sz, SizeF.Empty);
		}

		[Test]
		public void TestProperties () 
		{
			SizeF sz = new SizeF (0.0F, 0.0F);
	
			Assert ("P#1", sz.IsEmpty);
			Assert ("P#2", ! sz11_99.IsEmpty);
			AssertEquals ("P#3", 1.1F, sz11_0.Width);
			AssertEquals ("P#4", 1.1F, sz0_11.Height);
		}

		[Test]
		public void TestEquals () 
		{
			AssertEquals ("EQ#1", sz11_99, sz11_99);
			AssertEquals ("EQ#2", sz11_99, new SizeF (1.1F, 9.9F));
			Assert ("EQ#3", ! sz11_99.Equals (sz11_0));
			Assert ("EQ#4", ! sz11_99.Equals (sz0_11));
			Assert ("EQ#5", ! sz11_0.Equals (sz0_11));
		}

		[Test]
		public void Test2PointF ()
		{
			PointF p1 = new PointF (1.1F, 9.9F);
			PointF p2 = sz11_99.ToPointF ();

			AssertEquals ("2PF#1", p1, p2);
		}
		
		[Test]
		public void Test2Size ()
		{
			Size sz1 = new Size (1, 9);
			Size sz2 = sz11_99.ToSize ();

			AssertEquals ("2SZ#1", sz1, sz2);
		}

		[Test]
		public void Test2String ()
		{
			AssertEquals ("2STR#1", "{Width=1.1, Height=9.9}", sz11_99.ToString ());
		}

		[Test]
		public void TestAddition ()
		{
			AssertEquals ("ADD#1", sz11_99, sz11_0 + new SizeF (0.0F, 9.9F));
			AssertEquals ("ADD#2", sz11_99, new SizeF (0.0F, 0.0F) + new SizeF (1.1F, 9.9F));
		}

		[Test]
		public void TestEqualityOp () 
		{
			Assert ("EOP#1", sz11_99 == sz11_99);
			Assert ("EOP#2", sz11_99 == new SizeF (1.1F, 9.9F));
			Assert ("EOP#3", ! (sz11_99 == sz11_0));
			Assert ("EOP#4", ! (sz11_99 == sz0_11));
			Assert ("EOP#5", ! (sz11_0 == sz0_11));
		}

		[Test]
		public void TestInequalityOp () 
		{
			Assert ("IOP#1", ! (sz11_99 != sz11_99));
			Assert ("IOP#2", ! (sz11_99 != new SizeF (1.1F, 9.9F)));
			Assert ("IOP#3", sz11_99 != sz11_0);
			Assert ("IOP#4", sz11_99 != sz0_11);
			Assert ("IOP#5", sz11_0 != sz0_11);
		}
	
		[Test]
		public void TestSubtraction () 
		{
			AssertEquals ("SUB#1", sz11_0, sz11_99 - new SizeF (0.0F, 9.9F));
			AssertEquals ("SUB#2", sz0_11, new SizeF (1.1F, 1.1F) - new SizeF (1.1F, 0.0F));
		}
	
		[Test]
		public void TestSizeF2PointF ()
		{
			PointF pf1 = new PointF (1.1F, 9.9F);
			PointF pf2 = (PointF) sz11_99;
	
			AssertEquals ("SF2PF#1", pf1, pf2);
		}
	}
}

