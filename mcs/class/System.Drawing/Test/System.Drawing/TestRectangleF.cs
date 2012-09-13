// Tests for System.Drawing.RectangleF.cs

// Copyright (C) 2005, 2006 Novell, Inc (http://www.novell.com)
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
// Author: Jordi Mas i Hernandez <jordi@ximian.com>
//

using NUnit.Framework;
using System;
using System.Drawing;
using System.Security.Permissions;

namespace MonoTests.System.Drawing
{

	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class TestRectangleF {

		RectangleF rect_0;
		RectangleF rect_1;
		RectangleF rect_2;
		RectangleF rect_3;
		RectangleF rect_4;
		RectangleF rect_5;
		RectangleF rect_6;

		[SetUp]
		public void GetReady ()
		{
			rect_0 = new RectangleF (new PointF (10, 10), new SizeF (40, 40));
			rect_1 = new RectangleF (5, 5, 5, 5);
			rect_2 = RectangleF.Empty;
			rect_3 = new RectangleF (25, 25, 0, 0);
			rect_4 = new RectangleF (25, 252, 10, 20);
			rect_5 = new RectangleF (40, 40, 50, 50);
			rect_6 = new RectangleF (40, 40, 0, 50);
		}

		[Test]
		public void Contains ()
		{
			Assert.IsFalse (rect_0.Contains (5, 5), "a");
			Assert.IsTrue (rect_0.Contains (12, 12), "b");
			Assert.IsTrue (rect_0.Contains (10, 10), "c");
			Assert.IsFalse (rect_0.Contains (10, 50), "d");
			Assert.IsFalse (rect_0.Contains (10, 50F - float.Epsilon), "e");
			Assert.IsTrue (rect_0.Contains (10, 49.9F), "f");
			Assert.IsFalse (rect_0.Contains (50, 10), "g");
		}

		[Test]
		public void ContainsF ()
		{
			// from bug #5985
			RectangleF outer = new RectangleF (100, 150, 300, 300);
			RectangleF inner = new RectangleF (139.3323f, 188.4053f, 140.2086f, 210.3129f);
			
			Assert.IsTrue (outer.Contains (inner), "a");
		}

		[Test]
		public void Empty ()
		{
			Assert.AreEqual (rect_2.X, 0, "X");
			Assert.AreEqual (rect_2.Y, 0, "Y");
			Assert.AreEqual (rect_2.Width, 0, "Width");
			Assert.AreEqual (rect_2.Height, 0, "Height");
		}

		[Test]
		public void IsEmpty ()
		{
			Assert.IsFalse (rect_0.IsEmpty, "0");
			Assert.IsTrue (rect_2.IsEmpty, "2");
			Assert.IsTrue (rect_3.IsEmpty, "3");
			Assert.IsTrue (rect_6.IsEmpty, "6");
			Assert.IsTrue (new RectangleF (0, 0, -1, -1).IsEmpty, "negative w/h");
		}

		[Test]
		public void GetContents () 
		{
			Assert.AreEqual (rect_4.Right, rect_4.X + rect_4.Width, "Right");
			Assert.AreEqual (rect_4.Left, rect_4.X, "Left");
			Assert.AreEqual (rect_4.Bottom, rect_4.Y + rect_4.Height, "Bottom");
			Assert.AreEqual (rect_4.Top, rect_4.Y, "Top");
		}

		[Test]
		public void IntersectsWith () 
		{
			Assert.IsFalse (rect_0.IntersectsWith (rect_1), "0 N 1");
			Assert.IsFalse (rect_0.IntersectsWith (rect_2), "0 N 2");
			Assert.IsTrue (rect_0.IntersectsWith (rect_5), "0 N 5");
			Assert.IsTrue (rect_5.IntersectsWith (rect_0), "5 N 0");
			Assert.IsFalse (rect_0.IntersectsWith (rect_4), "0 N 4");
		}

		[Test]
		public void Location () 
		{
			Assert.AreEqual (new PointF (25, 252), rect_4.Location, "Location");
			PointF p = new PointF (11, 121);
			rect_4.Location = p;
			Assert.AreEqual (p, rect_4.Location, "Localtion-2");
			Assert.AreEqual (rect_4.X, 11, "X");
			Assert.AreEqual (rect_4.Y, 121, "Y");
			rect_4.X = 10;
			rect_4.Y = 15;
			Assert.AreEqual (new PointF (10, 15), rect_4.Location, "Localtion-3");
		}

		[Test]
		public void Size () 
		{
			Assert.AreEqual (rect_4.Width, 10, "Width-1");
			Assert.AreEqual (rect_4.Height, 20, "Height-1");
			rect_4.Width = 40;
			rect_4.Height = 100;
			Assert.AreEqual (rect_4.Size, new SizeF (40, 100), "Size");
			rect_4.Size = new SizeF (1, 2);
			Assert.AreEqual (rect_4.Width, 1, "Width-2");
			Assert.AreEqual (rect_4.Height, 2, "Height-2");
		}

		[Test]
		public void GetHashCodeTest () 
		{
			Assert.IsTrue (rect_0.GetHashCode () != rect_1.GetHashCode ());
		}

		[Test]
		public void Inflate () 
		{
			rect_0.Inflate (new SizeF (8, 5));
			Assert.AreEqual (new RectangleF (2, 5, 56, 50), rect_0, "INF#1");
			rect_1.Inflate (4, 4);
			Assert.AreEqual (new RectangleF (1, 1, 13, 13), rect_1, "INF#2");
			Assert.AreEqual (new RectangleF (30, 20, 70, 90),
				RectangleF.Inflate (rect_5, 10, 20), "INF#3");
			Assert.AreEqual (new RectangleF (40, 40, 50, 50), rect_5, "INF#4");
		}

		[Test]
		public void Intersect () 
		{
			Assert.AreEqual (new RectangleF (40, 40, 10, 10),
				RectangleF.Intersect (rect_0, rect_5), "INT#1");
			Assert.AreEqual (new RectangleF (10, 10, 40, 40), rect_0, "INT#2");
			rect_0.Intersect (rect_5);
			Assert.AreEqual (new RectangleF (40, 40, 10, 10), rect_0, "INT#3");
			Assert.AreEqual (RectangleF.Empty, RectangleF.Intersect (rect_1, rect_5), "INT#4");
		}

		[Test]
		public void Offset () 
		{
			rect_0.Offset (5, 5);
			Assert.AreEqual (new RectangleF (15, 15, 40, 40), rect_0, "OFS#1");
			rect_1.Offset (new Point (7, 0));
			Assert.AreEqual (new RectangleF (12, 5, 5, 5), rect_1, "OFS#2");
		}

		[Test]
		public void ToStringTest () 
		{
			Assert.AreEqual ("{X=10,Y=10,Width=40,Height=40}", rect_0.ToString (), "0");
			Assert.AreEqual ("{X=5,Y=5,Width=5,Height=5}", rect_1.ToString (), "1");
			Assert.AreEqual ("{X=0,Y=0,Width=0,Height=0}", rect_2.ToString (), "2");
			Assert.AreEqual ("{X=25,Y=25,Width=0,Height=0}", rect_3.ToString (), "3");
		}

		[Test]
		public void RectangleToRectangleF ()
		{
			Rectangle r = new Rectangle (1, 2, 3, 4);
			RectangleF rf = r;
			Assert.AreEqual (new RectangleF (1F, 2F, 3F, 4F), rf);
		}

		[Test]
		public void Union () 
		{
			Assert.AreEqual (RectangleF.FromLTRB (5, 5, 50, 50), RectangleF.Union (rect_0, rect_1));
		}

		[Test]
		public void Operator_Equal ()
		{
			RectangleF r0 = new RectangleF (1, 2, 3, 4);
			RectangleF r1 = r0;
			Assert.IsTrue (r0 == r1, "self");
			Assert.IsFalse (r0 == new RectangleF (0, 2, 3, 4), "X");
			Assert.IsFalse (r0 == new RectangleF (1, 0, 3, 4), "Y");
			Assert.IsFalse (r0 == new RectangleF (1, 2, 0, 4), "Width");
			Assert.IsFalse (r0 == new RectangleF (1, 2, 3, 0), "Height");
		}

		[Test]
		public void Operator_NotEqual ()
		{
			RectangleF r0 = new RectangleF (1, 2, 3, 4);
			RectangleF r1 = r0;
			Assert.IsFalse (r0 != r1, "self");
			Assert.IsTrue (r0 != new RectangleF (0, 2, 3, 4), "X");
			Assert.IsTrue (r0 != new RectangleF (1, 0, 3, 4), "Y");
			Assert.IsTrue (r0 != new RectangleF (1, 2, 0, 4), "Width");
			Assert.IsTrue (r0 != new RectangleF (1, 2, 3, 0), "Height");
		}

		[Test]
		public void NegativeWidth ()
		{
			RectangleF r = new RectangleF (0, 0, -1, 10);
			Assert.AreEqual (0, r.X, "X");
			Assert.AreEqual (0, r.Y, "Y");
			Assert.AreEqual (-1, r.Width, "Width");
			Assert.AreEqual (10, r.Height, "Height");
		}

		[Test]
		public void NegativeHeight ()
		{
			RectangleF r = new RectangleF (10, 10, 10, -1);
			Assert.AreEqual (10, r.X, "X");
			Assert.AreEqual (10, r.Y, "Y");
			Assert.AreEqual (10, r.Width, "Width");
			Assert.AreEqual (-1, r.Height, "Height");
		}

		[Test]
		public void EdgeIntersection ()
		{
			// https://bugzilla.novell.com/show_bug.cgi?id=431587
			RectangleF one = new RectangleF(10, 10, 10, 10);
			RectangleF two = new RectangleF(20, 10, 10, 10);

			one.Intersect(two);
			Assert.IsTrue (one.IsEmpty, "Empty");
			Assert.AreEqual (20f, one.X, "X");
			Assert.AreEqual (10f, one.Y, "Y");
			Assert.AreEqual (0f, one.Width, "Width");
			Assert.AreEqual (10f, one.Height, "Height");
		}
	}
}
