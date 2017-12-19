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

using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using NUnit.Framework;

namespace MonoTests.System.Windows {

	[TestFixture]
	public class PointTest {

		[Test]
		public void Accessors ()
		{
			Point p = new Point (4, 5);
			Assert.AreEqual (4, p.X);
			Assert.AreEqual (5, p.Y);
		}

		[Test]
		public void Equals ()
		{
			Point p = new Point (4, 5);
			Assert.IsTrue (p.Equals (new Point (4, 5)));
			Assert.IsFalse (p.Equals (new Point (5, 4)));
			Assert.IsFalse (p.Equals (new object()));
		}

			[Test]
			public void GetHashCodeTest()
			{
				Point p1 = new Point(-5, -4);
				Point p2 = new Point(5, 4);
				Point p3 = new Point(5, 4);

				Assert.AreEqual (p2.GetHashCode (), p3.GetHashCode ());
				Assert.AreEqual (p1.GetHashCode (),p2.GetHashCode ());
			}

		[Test]
		public void ToStringTest ()
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-us");
			Point p = new Point (4, 5);
			Assert.AreEqual ("4,5", p.ToString());
			Point p2 = new Point(4.1, 5.1);
			Assert.AreEqual("4.1,5.1",p2.ToString());
			Point p3 = new Point(0, 0);
			Assert.AreEqual("0,0", p3.ToString());

			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("de-de");
			Point p4 = new Point(4, 5);
			Assert.AreEqual("4;5", p4.ToString());
			Point p5 = new Point(4.1, 5.1);
			Assert.AreEqual("4,1;5,1", p5.ToString());
			Point p6 = new Point(0, 0);
			Assert.AreEqual("0;0", p6.ToString());
		}

		[Test]
		public void Parse ()
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("fr-fr");

			Point p = Point.Parse ("4,5");
			Assert.AreEqual (new Point (4, 5), p);

			p = Point.Parse ("-4,-5");
			Assert.AreEqual (new Point (-4, -5), p);

			p = Point.Parse ("-4.4,-5.5");
			Assert.AreEqual (new Point (-4.4, -5.5), p);

			p = Point.Parse("4.4,5.5");
			Assert.AreEqual(new Point(4.4, 5.5), p);

			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-us");
			Assert.AreEqual(new Point(4.4, 5.5), p);
		}

		[Test]
		public void Offset ()
		{
			Point p = new Point (4, 5);
			p.Offset (3, 4);
			Assert.AreEqual (new Point (7, 9), p);
		}

		[Test]
		public void Add ()
		{
			Point p = Point.Add (new Point (4, 5), new Vector (2, 3));
			Assert.AreEqual (new Point (6, 8), p);
		}

		[Test]
		public void Subtract1 ()
		{
			Point p = Point.Subtract (new Point (4, 5), new Vector (2, 3));
			Assert.AreEqual (new Point (2, 2), p);
		}

		[Test]
		public void Subtract2 ()
		{
			Vector v = Point.Subtract (new Point (4, 5), new Point (2, 3));
			Assert.AreEqual (new Vector (2, 2), v);
		}

		[Test]
		public void Multiply ()
		{
			Matrix m = Matrix.Identity;
			m.Scale (2, 2);

			Point p = Point.Multiply (new Point (2, 3), m);

			Assert.AreEqual (new Point (4, 6), p);
		}
	}

}

