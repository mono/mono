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
	public class VectorTest
	{
		const double DELTA = 0.000000001d;

		[Test]
		public void Accessors ()
		{
			Vector v = new Vector (4, 5);
			Assert.AreEqual (4, v.X);
			Assert.AreEqual (5, v.Y);
		}

		[Test]
		public void Equals ()
		{
			Vector v = new Vector (4, 5);
			Assert.IsTrue (v.Equals (new Vector (4, 5)));
			Assert.IsFalse (v.Equals (new Vector (5, 4)));
			Assert.IsFalse (v.Equals (new object()));
		}

		[Test]
		public void ToStringTest ()
		{
			Vector v = new Vector (4, 5);
			Assert.AreEqual ("4,5", v.ToString(CultureInfo.InvariantCulture));
		}

		[Test]
		[Category ("NotWorking")]
		public void Parse ()
		{
			Vector v = Vector.Parse ("4,5");
			Assert.AreEqual (new Vector (4, 5), v);

			v = Vector.Parse ("-4,-5");
			Assert.AreEqual (new Vector (-4, -5), v);

			v = Vector.Parse ("-4.4,-5.5");
			Assert.AreEqual (new Vector (-4.4, -5.5), v);
		}

		[Test]
		public void Add ()
		{
			Point p = Vector.Add (new Vector (2, 3), new Point (4, 5));
			Assert.AreEqual (new Point (6, 8), p);

			Vector v = Vector.Add (new Vector (2, 3), new Vector (4, 5));
			Assert.AreEqual (new Vector (6, 8), v);
		}

		[Test]
		public void Length ()
		{
			Vector v = new Vector (1, 0);
			Assert.AreEqual (1, v.LengthSquared);
			Assert.AreEqual (1, v.Length);

			v = new Vector (5, 5);
			Assert.AreEqual (50, v.LengthSquared);
			Assert.AreEqual (Math.Sqrt(50), v.Length);
		}

		[Test]
		public void AngleBetween ()
		{
			double angle = Vector.AngleBetween (new Vector (1, 0), new Vector (0, 1));
			Assert.AreEqual (90, angle);

			angle = Vector.AngleBetween (new Vector (1, 0), new Vector (0.5, 0.5));
			Assert.AreEqual (45, angle, DELTA);
		}

		[Test]
		public void CrossProduct ()
		{
			Assert.AreEqual (1, Vector.CrossProduct (new Vector (1, 0), new Vector (0, 1)));
			Assert.AreEqual (50, Vector.CrossProduct (new Vector (20, 30), new Vector (45, 70)));
		}

		[Test]
		public void Determinant ()
		{
			Assert.AreEqual (1, Vector.Determinant (new Vector (1, 0), new Vector (0, 1)));
			Assert.AreEqual (50, Vector.Determinant (new Vector (20, 30), new Vector (45, 70)));
		}

		[Test]
		public void Divide ()
		{
			Assert.AreEqual (new Vector (5, 7), Vector.Divide (new Vector (10, 14), 2));
		}

		[Test]
		public void Multiply_VV_M ()
		{
			Assert.AreEqual (60, Vector.Multiply (new Vector (5, 7), new Vector (5, 5)));
		}

		[Test]
		public void Multiply_VM_V ()
		{
			Matrix m = Matrix.Identity;
			m.Rotate (45);

			Console.WriteLine (Vector.Multiply (new Vector (3, 4), m));
		}

		[Test]
		public void Multiply_dV_V ()
		{
			Assert.AreEqual (new Vector (10, 18), Vector.Multiply (2, new Vector (5, 9)));
		}

		[Test]
		public void Multiply_Vd_V ()
		{
			Assert.AreEqual (new Vector (10, 18), Vector.Multiply (new Vector (5, 9), 2));
		}

		[Test]
		public void Negate ()
		{
			Vector v = new Vector (4, 5);
			v.Negate ();

			Assert.AreEqual (new Vector (-4, -5), v);
		}

		[Test]
		public void Normalize ()
		{
			Vector v = new Vector (5, 5);
			v.Normalize ();

			Assert.AreEqual (v.X, v.Y);
			Assert.AreEqual (1, v.Length, DELTA);
		}

		[Test]
		public void Subtract ()
		{
			Assert.AreEqual (new Vector (3, 4), Vector.Subtract (new Vector (5, 7), new Vector (2, 3)));
		}
	}
}

