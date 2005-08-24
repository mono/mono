//
// Tests for System.Drawing.Drawing2D.Matrix.cs
//
// Author:
//  Jordi Mas i Hernandez <jordi@ximian.com>
//
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

using NUnit.Framework;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace MonoTests.System.Drawing.Drawing2D
{
	[TestFixture]
	public class MatrixTest : Assertion
	{
		[TearDown]
		public void TearDown () { }

		[SetUp]
		public void SetUp () { }

		[Test]
		public void Constructors ()
		{
			{
				Matrix matrix = new Matrix ();
				AssertEquals ("C#1", 6, matrix.Elements.Length);
			}

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


		}

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

			AssertEquals ("H#1", -10, matrix.Elements[0]);
			AssertEquals ("H#2", -20, matrix.Elements[1]);
			AssertEquals ("H#3", -30, matrix.Elements[2]);
			AssertEquals ("H#4", -40, matrix.Elements[3]);
			AssertEquals ("H#5", 50, matrix.Elements[4]);
			AssertEquals ("H#6", 60, matrix.Elements[5]);
		}

		[Test]
		public void RotateAt ()
		{
			Matrix matrix = new Matrix (10, 20, 30, 40, 50, 60);
			matrix.RotateAt (180, new PointF (10, 10));

			AssertEquals ("I#1", -10, matrix.Elements[0]);
			AssertEquals ("I#2", -20, matrix.Elements[1]);
			AssertEquals ("I#3", -30, matrix.Elements[2]);
			AssertEquals ("I#4", -40, matrix.Elements[3]);
			AssertEquals ("I#5", 850, matrix.Elements[4]);
			AssertEquals ("I#6", 1260, matrix.Elements[5]);
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
		public void Equals ()
		{
			Matrix mat1 = new Matrix (10, 20, 30, 40, 50, 60);
			Matrix mat2 = new Matrix (10, 20, 30, 40, 50, 60);
			Matrix mat3 = new Matrix (10, 20, 30, 40, 50, 10);

			AssertEquals ("E#1", true, mat1.Equals (mat2));
			AssertEquals ("E#2", false, mat2.Equals (mat3));
			AssertEquals ("E#3", false, mat1.Equals (mat3));
		}

	}
}
