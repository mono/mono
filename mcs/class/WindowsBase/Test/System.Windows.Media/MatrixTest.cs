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

namespace MonoTests.System.Windows.Media {

	[TestFixture]
	public class MatrixTest {
		const double DELTA = 0.000000001d;

		void CheckMatrix (Matrix expected, Matrix actual)
		{
			Assert.AreEqual (expected.M11, actual.M11, DELTA);
			Assert.AreEqual (expected.M12, actual.M12, DELTA);
			Assert.AreEqual (expected.M21, actual.M21, DELTA);
			Assert.AreEqual (expected.M22, actual.M22, DELTA);
			Assert.AreEqual (expected.OffsetX, actual.OffsetX, DELTA);
			Assert.AreEqual (expected.OffsetY, actual.OffsetY, DELTA);
		}

		[Test]
		public void TestAccessors ()
		{
			Matrix m = new Matrix (1, 2, 3, 4, 5, 6);
			Assert.AreEqual (1, m.M11);
			Assert.AreEqual (2, m.M12);
			Assert.AreEqual (3, m.M21);
			Assert.AreEqual (4, m.M22);
			Assert.AreEqual (5, m.OffsetX);
			Assert.AreEqual (6, m.OffsetY);
		}

		[Test]
		public void Append ()
		{
			Matrix m = new Matrix (1, 2, 3, 4, 5, 6);
			m.Append (m);
			CheckMatrix (new Matrix (7, 10, 15, 22, 28, 40), m);
		}

		[Test]
		public void Equals ()
		{
			Matrix m = new Matrix (1, 2, 3, 4, 5, 6);
			Assert.IsTrue  (m.Equals (new Matrix (1, 2, 3, 4, 5, 6)));
			Assert.IsFalse (m.Equals (new Matrix (0, 2, 3, 4, 5, 6)));
			Assert.IsFalse (m.Equals (new Matrix (1, 0, 3, 4, 5, 6)));
			Assert.IsFalse (m.Equals (new Matrix (1, 2, 0, 4, 5, 6)));
			Assert.IsFalse (m.Equals (new Matrix (1, 2, 3, 0, 5, 6)));
			Assert.IsFalse (m.Equals (new Matrix (1, 2, 3, 4, 0, 6)));
			Assert.IsFalse (m.Equals (new Matrix (1, 2, 3, 4, 5, 0)));

			Assert.IsFalse (m.Equals (0));
			Assert.IsTrue (m.Equals ((object)m));
		}

		[Test]
		public void Determinant ()
		{
			Assert.AreEqual (0, (new Matrix (2, 2, 2, 2, 0, 0)).Determinant);
			Assert.AreEqual (-6, (new Matrix (1, 4, 2, 2, 0, 0)).Determinant);
			Assert.AreEqual (1, (new Matrix (1, 0, 0, 1, 0, 0)).Determinant);
			Assert.AreEqual (1, (new Matrix (1, 0, 0, 1, 5, 5)).Determinant);
			Assert.AreEqual (-1, (new Matrix (0, 1, 1, 0, 5, 5)).Determinant);
		}

		[Test]
		public void HasInverse ()
		{
			/* same matrices as in Determinant() */
			Assert.IsFalse ((new Matrix (2, 2, 2, 2, 0, 0)).HasInverse);
			Assert.IsTrue ((new Matrix (1, 4, 2, 2, 0, 0)).HasInverse);
			Assert.IsTrue  ((new Matrix (1, 0, 0, 1, 0, 0)).HasInverse);
			Assert.IsTrue  ((new Matrix (1, 0, 0, 1, 5, 5)).HasInverse);
			Assert.IsTrue  ((new Matrix (0, 1, 1, 0, 5, 5)).HasInverse);
		}

		[Test]
		public void IsIdentity ()
		{
			Assert.IsTrue (Matrix.Identity.IsIdentity);;
			Assert.IsFalse ((new Matrix (1, 0, 0, 1, 5, 5)).IsIdentity);
			Assert.IsFalse ((new Matrix (5, 5, 5, 5, 5, 5)).IsIdentity);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))] // "Transform is not invertible."
		public void InvertException1 ()
		{
			Matrix m = new Matrix (2, 2, 2, 2, 0, 0);
			m.Invert ();
		}

		[Test]
		public void Invert ()
		{
			Matrix m;

			m = new Matrix (1, 0, 0, 1, 0, 0);
			m.Invert ();
			CheckMatrix (new Matrix (1, 0, 0, 1, 0, 0), m);

			m = new Matrix (1, 0, 0, 1, 5, 5);
			m.Invert ();
			CheckMatrix (new Matrix (1, 0, 0, 1, -5, -5), m);

			m = new Matrix (1, 0, 0, 2, 5, 5);
			m.Invert ();
			CheckMatrix (new Matrix (1, 0, 0, 0.5, -5, -2.5), m);

			m = new Matrix (0, 2, 4, 0, 5, 5);
			m.Invert ();
			CheckMatrix (new Matrix (0, 0.25, 0.5, 0, -2.5, -1.25), m);
		}

		[Test]
		public void Identity ()
		{
			CheckMatrix (new Matrix (1, 0, 0, 1, 0, 0), Matrix.Identity);
		}

		[Test]
		public void Multiply ()
		{
			CheckMatrix (new Matrix (5, 0, 0, 5, 10, 10),
				     Matrix.Multiply (new Matrix (1, 0, 0, 1, 2, 2),
						      new Matrix (5, 0, 0, 5, 0, 0)));

			CheckMatrix (new Matrix (0, 0, 0, 0, 10, 10),
				     Matrix.Multiply (new Matrix (1, 0, 0, 1, 0, 0),
						      new Matrix (0, 0, 0, 0, 10, 10)));
		}

		[Test]
		public void Parse ()
		{
			CheckMatrix (Matrix.Identity, Matrix.Parse ("Identity"));
			CheckMatrix (new Matrix (1, 0, 0, 1, 0, 0), Matrix.Parse ("1, 0, 0, 1, 0, 0"));
			CheckMatrix (new Matrix (0.1, 0.2, 0.3, 0.4, 0.5, 0.6), Matrix.Parse ("0.1,0.2,0.3,0.4,0.5,0.6"));
			// XXX what about locales where . and , are switched?
		}

		[Test]
		public void Prepend ()
		{
			Matrix m = new Matrix (1, 2, 3, 4, 5, 6);
			m.Prepend (new Matrix (2, 4, 6, 8, 10, 12));

			CheckMatrix (new Matrix (14, 20, 30, 44, 51, 74), m);
		}

		[Test]
		public void Rotate ()
		{
			Matrix m = Matrix.Identity;
			m.Rotate (45);
			CheckMatrix (new Matrix (0.707106781186548,
						 0.707106781186547,
						 -0.707106781186547,
						 0.707106781186548, 0, 0), m);

			m = new Matrix (1, 2, 3, 4, 5, 6);
			m.Rotate (33);
			CheckMatrix (new Matrix (-0.25060750208463,
						 2.22198017090588,
						 0.337455563776164,
						 4.98859937682678,
						 0.925518629636958,
						 7.75521858274768), m);
		}

		[Test]
		public void RotateAt ()
		{
			Matrix m = new Matrix (1, 2, 3, 4, 5, 6);
			m.RotateAt (33, 5, 5);

			CheckMatrix (new Matrix (-0.25060750208463,
						 2.22198017090588,
						 0.337455563776164,
						 4.98859937682678,
						 4.45536096498497,
						 5.83867056794542), m);
		}

		[Test]
		public void RotateAtPrepend ()
		{
			Matrix m = new Matrix (1, 2, 3, 4, 5, 6);
			m.RotateAtPrepend (33, 5, 5);

			CheckMatrix (new Matrix (2.47258767299051,
						 3.85589727595096,
						 1.97137266882125,
						 2.26540420175164,
						 2.78019829094125,
						 5.39349261148701), m);
		}

		[Test]
		public void RotatePrepend ()
		{
			Matrix m = new Matrix (1, 2, 3, 4, 5, 6);
			m.RotatePrepend (33);

			CheckMatrix (new Matrix (2.47258767299051,
						 3.85589727595096,
						 1.97137266882125,
						 2.26540420175164,
						 5, 6), m);
		}

		[Test]
		public void Scale ()
		{
			Matrix m = Matrix.Identity;

			m.Scale (5, 6);
			CheckMatrix (new Matrix (5, 0, 0, 6, 0, 0), m);

			m = new Matrix (1, 2, 2, 1, 3, 3);
			m.Scale (5, 5);
			CheckMatrix (new Matrix (5, 10, 10, 5, 15, 15), m);
		}

		[Test]
		public void ScaleAt ()
		{
			Matrix m = new Matrix (1, 0, 0, 1, 2, 2);
			m.ScaleAt (2, 2, 0, 0);
			CheckMatrix (new Matrix (2, 0, 0, 2, 4, 4), m);

			m = new Matrix (1, 0, 0, 1, 2, 2);
			m.ScaleAt (2, 2, 4, 4);
			CheckMatrix (new Matrix (2, 0, 0, 2, 0, 0), m);

			m = new Matrix (1, 0, 0, 1, 2, 2);
			m.ScaleAt (2, 2, 2, 2);
			CheckMatrix (new Matrix (2, 0, 0, 2, 2, 2), m);
		}

		[Test]
		public void ScaleAtPrepend ()
		{
			Matrix m = new Matrix (1, 0, 0, 1, 2, 2);
			m.ScaleAtPrepend (2, 2, 0, 0);
			CheckMatrix (new Matrix (2, 0, 0, 2, 2, 2), m);

			m = new Matrix (1, 0, 0, 1, 2, 2);
			m.ScaleAtPrepend (2, 2, 4, 4);
			CheckMatrix (new Matrix (2, 0, 0, 2, -2, -2), m);

			m = new Matrix (1, 0, 0, 1, 2, 2);
			m.ScaleAtPrepend (2, 2, 2, 2);
			CheckMatrix (new Matrix (2, 0, 0, 2, 0, 0), m);
		}

		[Test]
		public void ScalePrepend ()
		{
			Matrix m = Matrix.Identity;

			m.ScalePrepend (5, 6);
			CheckMatrix (new Matrix (5, 0, 0, 6, 0, 0), m);

			m = new Matrix (1, 2, 2, 1, 3, 3);
			m.ScalePrepend (5, 5);
			CheckMatrix (new Matrix (5, 10, 10, 5, 3, 3), m);
		}

		[Test]
		public void SetIdentity ()
		{
			Matrix m = new Matrix (5, 5, 5, 5, 5, 5);
			m.SetIdentity ();
			CheckMatrix (Matrix.Identity, m);
		}

		[Test]
		public void Skew ()
		{
			Matrix m = Matrix.Identity;

			m.Skew (10, 15);
			CheckMatrix (new Matrix (1,
						 0.267949192431123,
						 0.176326980708465,
						 1, 0, 0), m);

			m = new Matrix (1, 2, 2, 1, 3, 3);
			m.Skew (10, 15);
			CheckMatrix (new Matrix (1.35265396141693,
						 2.26794919243112,
						 2.17632698070847,
						 1.53589838486225,
						 3.52898094212539,
						 3.80384757729337), m);
		}

		[Test]
		public void SkewPrepend ()
		{
			Matrix m = Matrix.Identity;

			m.SkewPrepend (10, 15);
			CheckMatrix (new Matrix (1,
						 0.267949192431123,
						 0.176326980708465,
						 1, 0, 0), m);

			m = new Matrix (1, 2, 2, 1, 3, 3);
			m.SkewPrepend (10, 15);
			CheckMatrix (new Matrix (1.53589838486225,
						 2.26794919243112,
						 2.17632698070847,
						 1.35265396141693,
						 3, 3), m);
		}

		[Test]
		public void ToStringTest ()
		{
			Matrix m = new Matrix (1, 2, 3, 4, 5, 6);
			Assert.AreEqual ("1,2,3,4,5,6", m.ToString(CultureInfo.InvariantCulture));
			m = Matrix.Identity;
			Assert.AreEqual ("Identity", m.ToString());
		}

		[Test]
		public void PointTransform ()
		{
			Matrix m = new Matrix (2, 0, 0, 2, 4, 4);

			Point p = new Point (5, 6);

			Assert.AreEqual (new Point (14, 16), m.Transform (p));

			Point[] ps = new Point[10];
			for (int i = 0; i < ps.Length; i ++)
				ps[i] = new Point (3 * i, 2 * i);

			m.Transform (ps);

			for (int i = 0; i < ps.Length; i ++)
				Assert.AreEqual (m.Transform (new Point (3 * i, 2 * i)), ps[i]);
		}

		[Test]
		public void VectorTransform ()
		{
			Matrix m = new Matrix (2, 0, 0, 2, 4, 4);

			Vector p = new Vector (5, 6);

			Assert.AreEqual (new Vector (10, 12), m.Transform (p));

			Vector[] ps = new Vector[10];
			for (int i = 0; i < ps.Length; i ++)
				ps[i] = new Vector (3 * i, 2 * i);

			m.Transform (ps);

			for (int i = 0; i < ps.Length; i ++)
				Assert.AreEqual (m.Transform (new Vector (3 * i, 2 * i)), ps[i]);
		}

		[Test]
		public void Translate ()
		{
			Matrix m = new Matrix (1, 0, 0, 1, 0, 0);
			m.Translate (5, 5);
			CheckMatrix (new Matrix (1, 0, 0, 1, 5, 5), m);

			m = new Matrix (2, 0, 0, 2, 0, 0);
			m.Translate (5, 5);
			CheckMatrix (new Matrix (2, 0, 0, 2, 5, 5), m);
		}

		[Test]
		public void TranslatePrepend ()
		{
			Matrix m = new Matrix (1, 0, 0, 1, 0, 0);
			m.TranslatePrepend (5, 5);
			CheckMatrix (new Matrix (1, 0, 0, 1, 5, 5), m);

			m = new Matrix (2, 0, 0, 2, 0, 0);
			m.TranslatePrepend (5, 5);
			CheckMatrix (new Matrix (2, 0, 0, 2, 10, 10), m);
		}

	}
}

