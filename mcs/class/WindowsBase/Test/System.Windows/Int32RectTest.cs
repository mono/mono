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
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using NUnit.Framework;

namespace MonoTests.System.Windows {

	[TestFixture]
	public class Int32RectTest
	{
		[Test]
		public void Ctor_Accessor ()
		{
			Int32Rect r;

			r = new Int32Rect (10, 15, 20, 30);
			Assert.AreEqual (10, r.X);
			Assert.AreEqual (15, r.Y);
			Assert.AreEqual (20, r.Width);
			Assert.AreEqual (30, r.Height);
		}

		[Test]
		public void Ctor_NegativeWidth ()
		{
			new Int32Rect (10, 10, -10, 10);
		}

		[Test]
		public void Ctor_NegativeHeight ()
		{
			new Int32Rect (10, 10, 10, -10);
		}

		[Test]
		public void Empty ()
		{
			Int32Rect r = Int32Rect.Empty;
			Assert.AreEqual (0, r.X);
			Assert.AreEqual (0, r.Y);
			Assert.AreEqual (0, r.Width);
			Assert.AreEqual (0, r.Height);
		}

		[Test]
		public void ModifyEmpty_x ()
		{
			Int32Rect r = Int32Rect.Empty;
			r.X = 5;
		}

		[Test]
		public void ModifyEmpty_y ()
		{
			Int32Rect r = Int32Rect.Empty;
			r.Y = 5;
		}

		[Test]
		public void ModifyEmpty_width ()
		{
			Int32Rect r = Int32Rect.Empty;
			r.Width = 5;
		}

		[Test]
		public void ModifyEmpty_height ()
		{
			Int32Rect r = Int32Rect.Empty;
			r.Height = 5;
		}

		[Test]
		public void ModifyEmpty_negative_width ()
		{
			Int32Rect r = Int32Rect.Empty;
			r.Width = -5;
		}

		[Test]
		public void ModifyEmpty_negative_height ()
		{
			Int32Rect r = Int32Rect.Empty;
			r.Height = -5;
		}


		[Test]
		public void Modify_negative_width ()
		{
			Int32Rect r = new Int32Rect (0, 0, 10, 10);
			r.Width = -5;
		}

		[Test]
		public void Modify_negative_height ()
		{
			Int32Rect r = new Int32Rect (0, 0, 10, 10);
			r.Height = -5;
		}

		[Test]
		public void IsEmpty ()
		{
			Assert.IsTrue (Int32Rect.Empty.IsEmpty);
			Assert.IsTrue ((new Int32Rect (0, 0, 0, 0)).IsEmpty);
			Assert.IsFalse ((new Int32Rect (5, 5, 5, 5)).IsEmpty);
		}

		[Test]
		public void ToStringTest ()
		{
			Int32Rect r = new Int32Rect (1, 2, 3, 4);
			Assert.AreEqual ("1,2,3,4", r.ToString(CultureInfo.InvariantCulture));

			Assert.AreEqual ("Empty", Int32Rect.Empty.ToString());
		}

		[Test]
		public void Parse ()
		{
			Int32Rect r = Int32Rect.Parse ("1, 2, 3, 4");
			Assert.AreEqual (new Int32Rect (1, 2, 3, 4), r);
		}

		[Test]
		public void ParseNegative ()
		{
			Int32Rect.Parse ("1, 2, -3, -4");
		}

		[Test]
		public void Equals ()
		{
			Int32Rect r1 = new Int32Rect (1, 2, 3, 4);
			Int32Rect r2 = r1;

			Assert.IsTrue (r1.Equals (r1));

			r2.X = 0;
			Assert.IsFalse (r1.Equals (r2));
			r2.X = r1.X;

			r2.Y = 0;
			Assert.IsFalse (r1.Equals (r2));
			r2.Y = r1.Y;

			r2.Width = 0;
			Assert.IsFalse (r1.Equals (r2));
			r2.Width = r1.Width;

			r2.Height = 0;
			Assert.IsFalse (r1.Equals (r2));
			r2.Height = r1.Height;

			Assert.IsFalse (r1.Equals (new object()));
		}
	}
}
