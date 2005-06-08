// Tests for System.Drawing.RectangleF.cs

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

namespace MonoTests.System.Drawing
{

	[TestFixture]
	public class TestRectangleF : Assertion
	{
		RectangleF rect_0;
		RectangleF rect_1;
		RectangleF rect_2;
		RectangleF rect_3;
		RectangleF rect_4;
		RectangleF rect_5;

		[TearDown]
		public void Clean () {}

		[SetUp]
		public void GetReady ()
		{
			rect_0 = new RectangleF (10, 10, 40, 40);
			rect_1 = new RectangleF (5, 5, 5, 5);
			rect_2 = RectangleF.Empty;
			rect_3 = new RectangleF (25, 25, 0, 0);
			rect_4 = new RectangleF (25, 252, 10, 20);
			rect_5 = new RectangleF (40, 40, 50, 50);
		}

		[Test]
		public void Contains ()
		{
			AssertEquals (rect_0.Contains (5, 5), false);
			AssertEquals (rect_0.Contains (12, 12), true);
			AssertEquals (rect_0.Contains (10, 10), true);
			AssertEquals (rect_0.Contains (10, 50), false);
			AssertEquals (rect_0.Contains (50, 10), false);
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
		public void Contents ()
		{
			AssertEquals (rect_4.X, 25);
			AssertEquals (rect_4.Y, 252);
			AssertEquals (rect_4.Width, 10);
			AssertEquals (rect_4.Height, 20);
			AssertEquals (rect_4.Size, new SizeF (10, 20));
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

	}
}

