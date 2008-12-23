//
// Tests for System.Drawing.Drawing2D.Matrix.cs
//
// Authors:
//	Jordi Mas i Hernandez <jordi@ximian.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005-2006 Novell, Inc (http://www.novell.com)
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

using NUnit.Framework;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security.Permissions;

namespace MonoTests.System.Drawing.Drawing2D
{
	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class MatrixTest : Assertion {

		private Matrix default_matrix;
		private Rectangle rect;
		private RectangleF rectf;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			default_matrix = new Matrix ();
		}

		[Test]
		public void Constructor_Default ()
		{
			Matrix matrix = new Matrix ();
			AssertEquals ("C#1", 6, matrix.Elements.Length);
		}

		[Test]
		public void Constructor_SixFloats ()
		{
			Matrix matrix = new Matrix (10, 20, 30, 40, 50, 60);
			AssertEquals ("C#2", 6, matrix.Elements.Length);
			AssertEquals ("C#3", 10, matrix.Elements[0]);
			AssertEquals ("C#4", 20, matrix.Elements[1]);
			AssertEquals ("C#5", 30, matrix.Elements[2]);
			AssertEquals ("C#6", 40, matrix.Elements[3]);
			AssertEquals ("C#7", 50, matrix.Elements[4]);
			AssertEquals ("C#8", 60, matrix.Elements[5]);
		}

		[Test]
		public void Constructor_Float ()
		{
			Matrix matrix = new Matrix (10, 20, 30, 40, 50, 60);
			AssertEquals ("C#2", 6, matrix.Elements.Length);
			AssertEquals ("C#3", 10, matrix.Elements[0]);
			AssertEquals ("C#4", 20, matrix.Elements[1]);
			AssertEquals ("C#5", 30, matrix.Elements[2]);
			AssertEquals ("C#6", 40, matrix.Elements[3]);
			AssertEquals ("C#7", 50, matrix.Elements[4]);
			AssertEquals ("C#8", 60, matrix.Elements[5]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_Int_Null ()
		{
			new Matrix (rect, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Constructor_Int_Empty ()
		{
			new Matrix (rect, new Point[0]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Constructor_Int_4Point ()
		{
			new Matrix (rect, new Point[4]);
		}

		[Test]
		public void Constructor_Rect_Point ()
		{
			Rectangle r = new Rectangle (100, 200, 300, 400);
			Matrix m = new Matrix (r, new Point[3] { new Point (10, 20), new Point (30, 40), new Point (50, 60) });
			float[] elements = m.Elements;
			AssertEquals ("0", 0.06666666, elements[0], 0.00001);
			AssertEquals ("1", 0.06666666, elements[1], 0.00001);
			AssertEquals ("2", 0.09999999, elements[2], 0.00001);
			AssertEquals ("3", 0.09999999, elements[3], 0.00001);
			AssertEquals ("4", -16.6666679, elements[4], 0.00001);
			AssertEquals ("5", -6.666667, elements[5], 0.00001);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_Float_Null ()
		{
			new Matrix (rectf, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Constructor_Float_Empty ()
		{
			new Matrix (rectf, new PointF[0]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Constructor_Float_2PointF ()
		{
			new Matrix (rectf, new PointF[2]);
		}

		[Test]
		public void Constructor_RectF_PointF ()
		{
			RectangleF r = new RectangleF (100, 200, 300, 400);
			Matrix m = new Matrix (r, new PointF[3] { new PointF (10, 20), new PointF (30, 40), new PointF (50, 60) });
			float[] elements = m.Elements;
			AssertEquals ("0", 0.06666666, elements[0], 0.00001);
			AssertEquals ("1", 0.06666666, elements[1], 0.00001);
			AssertEquals ("2", 0.09999999, elements[2], 0.00001);
			AssertEquals ("3", 0.09999999, elements[3], 0.00001);
			AssertEquals ("4", -16.6666679, elements[4], 0.00001);
			AssertEquals ("5", -6.666667, elements[5], 0.00001);
		}

		// Properties

		[Test]
		public void Invertible ()
		{
			Matrix matrix = new Matrix (123, 24, 82, 16, 47, 30);
			AssertEquals ("I#1", false, matrix.IsInvertible);

			matrix = new Matrix (156, 46, 0, 0, 106, 19);
			AssertEquals ("I#2", false, matrix.IsInvertible);

			matrix = new Matrix (146, 66, 158, 104, 42, 150);
			AssertEquals ("I#3", true, matrix.IsInvertible);

			matrix = new Matrix (119, 140, 145, 74, 102, 58);
			AssertEquals ("I#4", true, matrix.IsInvertible);
		}
		
		[Test]
		public void IsIdentity ()
		{
			Matrix identity = new Matrix ();
			Matrix matrix = new Matrix (123, 24, 82, 16, 47, 30);
			AssertEquals ("N#1-identity", false, matrix.IsIdentity);
			Assert ("N#1-equals", !identity.Equals (matrix));
			
			matrix = new Matrix (1, 0, 0, 1, 0, 0);
			AssertEquals ("N#2-identity", true, matrix.IsIdentity);
			Assert ("N#2-equals", identity.Equals (matrix));

			// so what's the required precision ?

			matrix = new Matrix (1.1f, 0.1f, -0.1f, 0.9f, 0, 0);
			Assert ("N#3-identity", !matrix.IsIdentity);
			Assert ("N#3-equals", !identity.Equals (matrix));

			matrix = new Matrix (1.01f, 0.01f, -0.01f, 0.99f, 0, 0);
			Assert ("N#4-identity", !matrix.IsIdentity);
			Assert ("N#4-equals", !identity.Equals (matrix));

			matrix = new Matrix (1.001f, 0.001f, -0.001f, 0.999f, 0, 0);
			Assert ("N#5-identity", !matrix.IsIdentity);
			Assert ("N#5-equals", !identity.Equals (matrix));

			matrix = new Matrix (1.0001f, 0.0001f, -0.0001f, 0.9999f, 0, 0);
			Assert ("N#6-identity", matrix.IsIdentity);
			// note: NOT equal
			Assert ("N#6-equals", !identity.Equals (matrix));

			matrix = new Matrix (1.0009f, 0.0009f, -0.0009f, 0.99995f, 0, 0);
			Assert ("N#7-identity", !matrix.IsIdentity);
			Assert ("N#7-equals", !identity.Equals (matrix));
		}
		
		[Test]
		public void IsOffsetX ()
		{
			Matrix matrix = new Matrix (123, 24, 82, 16, 47, 30);
			AssertEquals ("X#1", 47, matrix.OffsetX);			
		}
		
		[Test]
		public void IsOffsetY ()
		{
			Matrix matrix = new Matrix (123, 24, 82, 16, 47, 30);
			AssertEquals ("Y#1", 30, matrix.OffsetY);			
		}
		
		// Elements Property is checked implicity in other test

		//
		// Methods
		//
		

		[Test]
		public void Clone ()
		{
			Matrix matsrc = new Matrix (10, 20, 30, 40, 50, 60);
			Matrix matrix  = matsrc.Clone ();

			AssertEquals ("D#1", 6, matrix.Elements.Length);
			AssertEquals ("D#2", 10, matrix.Elements[0]);
			AssertEquals ("D#3", 20, matrix.Elements[1]);
			AssertEquals ("D#4", 30, matrix.Elements[2]);
			AssertEquals ("D#5", 40, matrix.Elements[3]);
			AssertEquals ("D#6", 50, matrix.Elements[4]);
			AssertEquals ("D#7", 60, matrix.Elements[5]);
		}

		[Test]
		public void HashCode ()
		{
			Matrix matrix = new Matrix (10, 20, 30, 40, 50, 60);
			Matrix clone = matrix.Clone ();
			Assert ("HashCode/Clone", matrix.GetHashCode () != clone.GetHashCode ());

			Matrix matrix2 = new Matrix (10, 20, 30, 40, 50, 60);
			Assert ("HashCode/Identical", matrix.GetHashCode () != matrix2.GetHashCode ());
		}

		[Test]
		public void Reset ()
		{
			Matrix matrix = new Matrix (51, 52, 53, 54, 55, 56);
			matrix.Reset ();

			AssertEquals ("F#1", 6, matrix.Elements.Length);
			AssertEquals ("F#2", 1, matrix.Elements[0]);
			AssertEquals ("F#3", 0, matrix.Elements[1]);
			AssertEquals ("F#4", 0, matrix.Elements[2]);
			AssertEquals ("F#5", 1, matrix.Elements[3]);
			AssertEquals ("F#6", 0, matrix.Elements[4]);
			AssertEquals ("F#7", 0, matrix.Elements[5]);
		}

		[Test]
		public void Rotate ()
		{
			Matrix matrix = new Matrix (10, 20, 30, 40, 50, 60);
			matrix.Rotate (180);

			AssertEquals ("H#1", -10.0f, matrix.Elements[0], 0.0001);
			AssertEquals ("H#2", -20, matrix.Elements[1], 0.0001);
			AssertEquals ("H#3", -30.0000019f, matrix.Elements[2], 0.0001);
			AssertEquals ("H#4", -40.0000038f, matrix.Elements[3], 0.0001);
			AssertEquals ("H#5", 50, matrix.Elements[4]);
			AssertEquals ("H#6", 60, matrix.Elements[5]);
		}

		[Test]
		public void Rotate_45_135 ()
		{
			Matrix matrix = new Matrix ();
			Assert ("original.IsIdentity", matrix.IsIdentity);

			matrix.Rotate (45);
			Assert ("+45.!IsIdentity", !matrix.IsIdentity);
			float[] elements = matrix.Elements;
			AssertEquals ("45#1", 0.707106769f, elements[0], 0.0001);
			AssertEquals ("45#2", 0.707106769f, elements[1], 0.0001);
			AssertEquals ("45#3", -0.707106829f, elements[2], 0.0001);
			AssertEquals ("45#4", 0.707106769f, elements[3], 0.0001);
			AssertEquals ("45#5", 0, elements[4], 0.001f);
			AssertEquals ("45#6", 0, elements[5], 0.001f);

			matrix.Rotate (135);
			Assert ("+135.!IsIdentity", !matrix.IsIdentity);
			elements = matrix.Elements;
			AssertEquals ("180#1", -1, elements[0], 0.0001);
			AssertEquals ("180#2", 0, elements[1], 0.0001);
			AssertEquals ("180#3", 0, elements[2], 0.0001);
			AssertEquals ("180#4", -1, elements[3], 0.0001);
			AssertEquals ("180#5", 0, elements[4]);
			AssertEquals ("180#6", 0, elements[5]);
		}

		[Test]
		public void Rotate_90_270_Matrix ()
		{
			Matrix matrix = new Matrix ();
			Assert ("original.IsIdentity", matrix.IsIdentity);

			matrix.Rotate (90);
			Assert ("+90.!IsIdentity", !matrix.IsIdentity);
			float[] elements = matrix.Elements;
			AssertEquals ("90#1", 0, elements[0], 0.0001);
			AssertEquals ("90#2", 1, elements[1], 0.0001);
			AssertEquals ("90#3", -1, elements[2], 0.0001);
			AssertEquals ("90#4", 0, elements[3], 0.0001);
			AssertEquals ("90#5", 0, elements[4]);
			AssertEquals ("90#6", 0, elements[5]);

			matrix.Rotate (270);
			// this isn't a perfect 1, 0, 0, 1, 0, 0 matrix - but close enough
			Assert ("360.IsIdentity", matrix.IsIdentity);
			Assert ("360.Equals", !new Matrix ().Equals (matrix));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Rotate_InvalidOrder ()
		{
			new Matrix ().Rotate (180, (MatrixOrder) Int32.MinValue);
		}

		[Test]
		public void RotateAt ()
		{
			Matrix matrix = new Matrix (10, 20, 30, 40, 50, 60);
			matrix.RotateAt (180, new PointF (10, 10));

			AssertEquals ("I#1", -10, matrix.Elements[0], 0.01);
			AssertEquals ("I#2", -20, matrix.Elements[1], 0.01);
			AssertEquals ("I#3", -30, matrix.Elements[2], 0.01);
			AssertEquals ("I#4", -40, matrix.Elements[3], 0.01);
			AssertEquals ("I#5", 850, matrix.Elements[4], 0.01);
			AssertEquals ("I#6", 1260, matrix.Elements[5], 0.01);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void RotateAt_InvalidOrder ()
		{
			new Matrix ().RotateAt (180, new PointF (10, 10), (MatrixOrder) Int32.MinValue);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Multiply_Null ()
		{
			new Matrix (10, 20, 30, 40, 50, 60).Multiply (null);
		}

		[Test]
		public void Multiply ()
		{
			Matrix matrix = new Matrix (10, 20, 30, 40, 50, 60);
			matrix.Multiply (new Matrix (10, 20, 30, 40, 50, 60));

			AssertEquals ("J#1", 700, matrix.Elements[0]);
			AssertEquals ("J#2", 1000, matrix.Elements[1]);
			AssertEquals ("J#3", 1500, matrix.Elements[2]);
			AssertEquals ("J#4", 2200, matrix.Elements[3]);
			AssertEquals ("J#5", 2350, matrix.Elements[4]);
			AssertEquals ("J#6", 3460, matrix.Elements[5]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Multiply_Null_Order ()
		{
			new Matrix (10, 20, 30, 40, 50, 60).Multiply (null, MatrixOrder.Append);
		}

		[Test]
		public void Multiply_Append ()
		{
			Matrix matrix = new Matrix (10, 20, 30, 40, 50, 60);
			matrix.Multiply (new Matrix (10, 20, 30, 40, 50, 60), MatrixOrder.Append);

			AssertEquals ("J#1", 700, matrix.Elements[0]);
			AssertEquals ("J#2", 1000, matrix.Elements[1]);
			AssertEquals ("J#3", 1500, matrix.Elements[2]);
			AssertEquals ("J#4", 2200, matrix.Elements[3]);
			AssertEquals ("J#5", 2350, matrix.Elements[4]);
			AssertEquals ("J#6", 3460, matrix.Elements[5]);
		}

		[Test]
		public void Multiply_Prepend ()
		{
			Matrix matrix = new Matrix (10, 20, 30, 40, 50, 60);
			matrix.Multiply (new Matrix (10, 20, 30, 40, 50, 60), MatrixOrder.Prepend);

			AssertEquals ("J#1", 700, matrix.Elements[0]);
			AssertEquals ("J#2", 1000, matrix.Elements[1]);
			AssertEquals ("J#3", 1500, matrix.Elements[2]);
			AssertEquals ("J#4", 2200, matrix.Elements[3]);
			AssertEquals ("J#5", 2350, matrix.Elements[4]);
			AssertEquals ("J#6", 3460, matrix.Elements[5]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Multiply_InvalidOrder ()
		{
			Matrix matrix = new Matrix (10, 20, 30, 40, 50, 60);
			matrix.Multiply (new Matrix (10, 20, 30, 40, 50, 60), (MatrixOrder)Int32.MinValue);
		}

		[Test]
		public void Equals ()
		{
			Matrix mat1 = new Matrix (10, 20, 30, 40, 50, 60);
			Matrix mat2 = new Matrix (10, 20, 30, 40, 50, 60);
			Matrix mat3 = new Matrix (10, 20, 30, 40, 50, 10);

			AssertEquals ("E#1", true, mat1.Equals (mat2));
			AssertEquals ("E#2", false, mat2.Equals (mat3));
			AssertEquals ("E#3", false, mat1.Equals (mat3));
		}
		
		[Test]
		public void Invert ()
		{
			Matrix matrix = new Matrix (1, 2, 3, 4, 5, 6);
			matrix.Invert ();
			
			AssertEquals ("V#1", -2, matrix.Elements[0]);
			AssertEquals ("V#2", 1, matrix.Elements[1]);
			AssertEquals ("V#3", 1.5, matrix.Elements[2]);
			AssertEquals ("V#4", -0.5, matrix.Elements[3]);
			AssertEquals ("V#5", 1, matrix.Elements[4]);
			AssertEquals ("V#6", -2, matrix.Elements[5]);			
		}

		[Test]
		public void Invert_Translation ()
		{
			Matrix matrix = new Matrix (1, 0, 0, 1, 8, 8);
			matrix.Invert ();

			float[] elements = matrix.Elements;
			AssertEquals ("#1", 1, elements[0]);
			AssertEquals ("#2", 0, elements[1]);
			AssertEquals ("#3", 0, elements[2]);
			AssertEquals ("#4", 1, elements[3]);
			AssertEquals ("#5", -8, elements[4]);
			AssertEquals ("#6", -8, elements[5]);
		}

		[Test]
		public void Invert_Identity ()
		{
			Matrix matrix = new Matrix ();
			Assert ("IsIdentity", matrix.IsIdentity);
			Assert ("IsInvertible", matrix.IsInvertible);
			matrix.Invert ();
			Assert ("IsIdentity-2", matrix.IsIdentity);
			Assert ("IsInvertible-2", matrix.IsInvertible);
		}

		[Test]
		public void Scale ()
		{
			Matrix matrix = new Matrix (10, 20, 30, 40, 50, 60);
			matrix.Scale (2, 4);

			AssertEquals ("S#1", 20, matrix.Elements[0]);
			AssertEquals ("S#2", 40, matrix.Elements[1]);
			AssertEquals ("S#3", 120, matrix.Elements[2]);
			AssertEquals ("S#4", 160, matrix.Elements[3]);
			AssertEquals ("S#5", 50, matrix.Elements[4]);
			AssertEquals ("S#6", 60, matrix.Elements[5]);

			matrix.Scale (0.5f, 0.25f);

			AssertEquals ("SB#1", 10, matrix.Elements[0]);
			AssertEquals ("SB#2", 20, matrix.Elements[1]);
			AssertEquals ("SB#3", 30, matrix.Elements[2]);
			AssertEquals ("SB#4", 40, matrix.Elements[3]);
			AssertEquals ("SB#5", 50, matrix.Elements[4]);
			AssertEquals ("SB#6", 60, matrix.Elements[5]);
		}

		[Test]
		public void Scale_Negative ()
		{
			Matrix matrix = new Matrix (10, 20, 30, 40, 50, 60);
			matrix.Scale (-2, -4);

			AssertEquals ("S#1", -20, matrix.Elements[0]);
			AssertEquals ("S#2", -40, matrix.Elements[1]);
			AssertEquals ("S#3", -120, matrix.Elements[2]);
			AssertEquals ("S#4", -160, matrix.Elements[3]);
			AssertEquals ("S#5", 50, matrix.Elements[4]);
			AssertEquals ("S#6", 60, matrix.Elements[5]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Scale_InvalidOrder ()
		{
			new Matrix ().Scale (2, 1, (MatrixOrder) Int32.MinValue);
		}
		
		[Test]
		public void Shear ()
		{
			Matrix matrix = new Matrix (10, 20, 30, 40, 50, 60);
			matrix.Shear (2, 4);

			AssertEquals ("H#1", 130, matrix.Elements[0]);
			AssertEquals ("H#2", 180, matrix.Elements[1]);
			AssertEquals ("H#3", 50, matrix.Elements[2]);
			AssertEquals ("H#4", 80, matrix.Elements[3]);
			AssertEquals ("H#5", 50, matrix.Elements[4]);
			AssertEquals ("H#6", 60, matrix.Elements[5]);
			
			matrix = new Matrix (5, 3, 9, 2, 2, 1);
			matrix.Shear  (10, 20);			
			
			AssertEquals ("H#7", 185, matrix.Elements[0]);
			AssertEquals ("H#8", 43, matrix.Elements[1]);
			AssertEquals ("H#9", 59, matrix.Elements[2]);
			AssertEquals ("H#10", 32, matrix.Elements[3]);
			AssertEquals ("H#11", 2, matrix.Elements[4]);
			AssertEquals ("H#12", 1, matrix.Elements[5]);			    
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Shear_InvalidOrder ()
		{
			new Matrix ().Shear (-1, 1, (MatrixOrder) Int32.MinValue);
		}
		
		[Test]
		public void TransformPoints ()
		{
			Matrix matrix = new Matrix (2, 4, 6, 8, 10, 12);
			PointF [] pointsF = new PointF [] {new PointF (2, 4), new PointF (4, 8)};
			matrix.TransformPoints (pointsF);
						
			AssertEquals ("K#1", 38, pointsF[0].X);
			AssertEquals ("K#2", 52, pointsF[0].Y);
			AssertEquals ("K#3", 66, pointsF[1].X);
			AssertEquals ("K#4", 92, pointsF[1].Y);
			
			Point [] points = new Point [] {new Point (2, 4), new Point (4, 8)};
			matrix.TransformPoints (points);
			AssertEquals ("K#5", 38, pointsF[0].X);
			AssertEquals ("K#6", 52, pointsF[0].Y);
			AssertEquals ("K#7", 66, pointsF[1].X);
			AssertEquals ("K#8", 92, pointsF[1].Y);						    
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TransformPoints_Point_Null ()
		{
			new Matrix ().TransformPoints ((Point[]) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TransformPoints_PointF_Null ()
		{
			new Matrix ().TransformPoints ((PointF[]) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TransformPoints_Point_Empty ()
		{
			new Matrix ().TransformPoints (new Point[0]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TransformPoints_PointF_Empty ()
		{
			new Matrix ().TransformPoints (new PointF[0]);
		}
		
		[Test]
		public void TransformVectors  ()
		{
			Matrix matrix = new Matrix (2, 4, 6, 8, 10, 12);
			PointF [] pointsF = new PointF [] {new PointF (2, 4), new PointF (4, 8)};
			matrix.TransformVectors (pointsF);
						
			AssertEquals ("N#1", 28, pointsF[0].X);
			AssertEquals ("N#2", 40, pointsF[0].Y);
			AssertEquals ("N#3", 56, pointsF[1].X);
			AssertEquals ("N#4", 80, pointsF[1].Y);
			
			Point [] points = new Point [] {new Point (2, 4), new Point (4, 8)};
			matrix.TransformVectors (points);
			AssertEquals ("N#5", 28, pointsF[0].X);
			AssertEquals ("N#6", 40, pointsF[0].Y);
			AssertEquals ("N#7", 56, pointsF[1].X);
			AssertEquals ("N#8", 80, pointsF[1].Y);						    
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TransformVectors_Point_Null ()
		{
			new Matrix ().TransformVectors ((Point[]) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TransformVectors_PointF_Null ()
		{
			new Matrix ().TransformVectors ((PointF[]) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TransformVectors_Point_Empty ()
		{
			new Matrix ().TransformVectors (new Point[0]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TransformVectors_PointF_Empty ()
		{
			new Matrix ().TransformVectors (new PointF[0]);
		}

		[Test]
		public void Translate  ()
		{
			Matrix matrix = new Matrix (2, 4, 6, 8, 10, 12);			
			matrix.Translate (5, 10);
						
			AssertEquals ("Y#1", 2, matrix.Elements[0]);
			AssertEquals ("Y#2", 4, matrix.Elements[1]);
			AssertEquals ("Y#3", 6, matrix.Elements[2]);
			AssertEquals ("Y#4", 8, matrix.Elements[3]);
			AssertEquals ("Y#5", 80, matrix.Elements[4]);
			AssertEquals ("Y#6", 112, matrix.Elements[5]);	
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Translate_InvalidOrder ()
		{
			new Matrix ().Translate (-1, 1, (MatrixOrder) Int32.MinValue);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void VectorTransformPoints_Null ()
		{
			new Matrix ().VectorTransformPoints ((Point[]) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void VectorTransformPoints_Empty ()
		{
			new Matrix ().VectorTransformPoints (new Point[0]);
		}
	}
}
