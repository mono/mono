// Tests for System.Drawing.Rectangle.cs

// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
	public class TestRectangle : Assertion
	{
		Rectangle rect_0;
		Rectangle rect_1;
		Rectangle rect_2;
		Rectangle rect_3;
		Rectangle rect_4;
		Rectangle rect_5;

		[TearDown]
		public void Clean () {}

		[SetUp]
		public void GetReady ()
		{
			rect_0 = new Rectangle (10, 10, 40, 40);
			rect_1 = new Rectangle (5, 5, 5, 5);
			rect_2 = Rectangle.Empty;
			rect_3 = new Rectangle (new Point (25, 25), new Size (0, 0));
			rect_4 = new Rectangle (new Point (25, 252), new Size (10, 20));
			rect_5 = new Rectangle (40, 40, 50, 50);
		}

		[Test]
		public void Contains ()
		{
			AssertEquals (false, rect_0.Contains (5, 5));
			AssertEquals (true, rect_0.Contains (12, 12));
			AssertEquals (true, rect_0.Contains (new Point (10, 10)));
			AssertEquals (false, rect_0.Contains (new Point (10, 50)));
			AssertEquals (false, rect_0.Contains (50, 10));
			AssertEquals (true, rect_0.Contains (new Rectangle (20, 20, 15, 15)));
			AssertEquals (false, rect_0.Contains (new Rectangle (5, 5, 20, 20)));
			AssertEquals (true, rect_2.Contains (rect_2));
		}

		[Test]
		public void Empty ()
		{
			AssertEquals (rect_2.X, 0);
			AssertEquals (rect_2.Y, 0);
			AssertEquals (rect_2.Width, 0);
			AssertEquals (rect_2.Height, 0);
		}

		[Test]
		public void IsEmpty ()
		{
			AssertEquals (rect_0.IsEmpty, false);
			AssertEquals (rect_2.IsEmpty, true);
			AssertEquals (rect_3.IsEmpty, false);
		}

		[Test]
		public void GetContents ()
		{
			AssertEquals (rect_4.Right, rect_4.X + rect_4.Width);
			AssertEquals (rect_4.Left, rect_4.X);
			AssertEquals (rect_4.Bottom, rect_4.Y + rect_4.Height);
			AssertEquals (rect_4.Top, rect_4.Y);
		}

		[Test]
		public void IntersectsWith  ()
		{						
			AssertEquals (rect_0.IntersectsWith (rect_1), false);
			AssertEquals (rect_0.IntersectsWith (rect_2), false);
			AssertEquals (rect_0.IntersectsWith (rect_5), true);
			AssertEquals (rect_5.IntersectsWith (rect_0), true);
			AssertEquals (rect_0.IntersectsWith (rect_4), false);
		}

		[Test]
		public void Location ()
		{
			AssertEquals (new Point (25, 252), rect_4.Location);
			Point p = new Point (11, 121);
			rect_4.Location = p;
			AssertEquals (p, rect_4.Location);
			AssertEquals (rect_4.X, 11);
			AssertEquals (rect_4.Y, 121);
			rect_4.X = 10;
			rect_4.Y = 15;
			AssertEquals (new Point (10, 15), rect_4.Location);
		}

		[Test]
		public void Size ()
		{
			AssertEquals (rect_4.Width, 10);
			AssertEquals (rect_4.Height, 20);
			rect_4.Width = 40;
			rect_4.Height = 100;
			AssertEquals (rect_4.Size, new Size (40, 100));
			rect_4.Size = new Size (1, 2);
			AssertEquals (rect_4.Width, 1);
			AssertEquals (rect_4.Height, 2);
		}

		[Test]
		public void ConvertFromRectangleF ()
		{
			AssertEquals (rect_0, Rectangle.Ceiling (
				new RectangleF (9.9F, 9.1F, 39.04F, 39.999F)));
			AssertEquals (rect_0, Rectangle.Round  (
				new RectangleF (9.5F, 10.499F, 40.01F, 39.6F)));
			AssertEquals (rect_0, Rectangle.Truncate (
				new RectangleF (10.999F, 10.01F, 40.3F, 40.0F)));
		}

		[Test]
		public void GetHashCodeTest ()
		{
			Assert ("GHC#1", rect_0.GetHashCode () != rect_1.GetHashCode ());
		}

		[Test]
		public void Inflate ()
		{
			rect_0.Inflate (new Size (8, 5));
			AssertEquals ("INF#1", new Rectangle (2, 5, 56, 50), rect_0);
			rect_1.Inflate (4, 4);
			AssertEquals ("INF#2", new Rectangle (1, 1, 13, 13), rect_1);
			AssertEquals ("INF#3", new Rectangle (30, 20, 70, 90),
				Rectangle.Inflate (rect_5, 10, 20));
			AssertEquals ("INF#4", new Rectangle (40, 40, 50, 50), rect_5);
		}

		[Test]
		public void Intersect ()
		{
			AssertEquals ("INT#1", new Rectangle (40, 40, 10, 10), 
				Rectangle.Intersect (rect_0, rect_5));
			AssertEquals ("INT#2", new Rectangle (10, 10, 40, 40), rect_0);
			rect_0.Intersect (rect_5);
			AssertEquals ("INT#3", new Rectangle (40, 40, 10, 10), rect_0);
			AssertEquals ("INT#4", Rectangle.Empty, Rectangle.Intersect (rect_1, rect_5));

			// Two rectangles touching each other
			AssertEquals ("INT#5", new Rectangle (3, 0, 0, 7), Rectangle.Intersect (new Rectangle (0, 0, 3, 7), new Rectangle (3, 0, 8, 14)));
		}

		[Test]
		public void Offset ()
		{
			rect_0.Offset (5, 5);
			AssertEquals ("OFS#1", new Rectangle (15, 15, 40, 40), rect_0);
			rect_1.Offset (new Point (7, 0));
			AssertEquals ("OFS#2", new Rectangle (12, 5, 5, 5), rect_1);
		}

		[Test]
		public void ToStringTest ()
		{
			AssertEquals ("{X=10,Y=10,Width=40,Height=40}", rect_0.ToString ());
			AssertEquals ("{X=5,Y=5,Width=5,Height=5}", rect_1.ToString ());
			AssertEquals ("{X=0,Y=0,Width=0,Height=0}", rect_2.ToString ());
			AssertEquals ("{X=25,Y=25,Width=0,Height=0}", rect_3.ToString ());
		}

		[Test]
		public void FromTRLB ()
		{
			AssertEquals (rect_0, Rectangle.FromLTRB (10, 10, 50, 50));
			AssertEquals (rect_1, Rectangle.FromLTRB (5, 5, 10, 10));
			AssertEquals (rect_2, Rectangle.FromLTRB (0, 0, 0, 0));
		}

		[Test]
		public void Union ()
		{
			AssertEquals (Rectangle.FromLTRB (5, 5, 50, 50), Rectangle.Union (rect_0, rect_1));
		}
	}
}

