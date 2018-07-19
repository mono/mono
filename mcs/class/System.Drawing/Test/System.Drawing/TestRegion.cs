//
// Region class testing unit
//
// Authors:
//   Jordi Mas, jordi@ximian.com
//   Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004-2008 Novell, Inc (http://www.novell.com)
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

using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security.Permissions;
using NUnit.Framework;

namespace MonoTests.System.Drawing
{

	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class TestRegion
	{
		/* For debugging */
		public static void DumpRegion (Region rgn)
		{
			Matrix matrix = new Matrix ();
			RectangleF [] rects = rgn.GetRegionScans (matrix);

			for (int i = 0; i < rects.Length; i++)
				Console.WriteLine ( rects[i]);
		}

		private Bitmap bitmap;
		private Graphics graphic;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			bitmap = new Bitmap (10, 10);
			graphic = Graphics.FromImage (bitmap);
		}

		[Test]
		public void TestBounds()
		{
			Bitmap bmp = new Bitmap (600, 800);
			Graphics dc = Graphics.FromImage (bmp);
			Rectangle rect1, rect2;
			Region rgn1, rgn2;
			RectangleF bounds;

			rect1 = new Rectangle (500, 30, 60, 80);
			rect2 = new Rectangle (520, 40, 60, 80);
			rgn1 = new Region(rect1);
			rgn2 = new Region(rect2);
			rgn1.Union(rgn2);

			bounds = rgn1.GetBounds (dc);

			Assert.AreEqual (500, bounds.X);
			Assert.AreEqual (30, bounds.Y);
			Assert.AreEqual (80, bounds.Width);
			Assert.AreEqual (90, bounds.Height);
		}

		[Test]
		public void TestCloneAndEquals()
		{
			Bitmap bmp = new Bitmap (600, 800);
			Graphics dc = Graphics.FromImage (bmp);
			Rectangle rect1, rect2;
			Region rgn1, rgn2;
			RectangleF [] rects;
			RectangleF [] rects2;
			Matrix matrix = new Matrix ();

			rect1 = new Rectangle (500, 30, 60, 80);
			rect2 = new Rectangle (520, 40, 60, 80);
			rgn1 = new Region (rect1);
			rgn1.Union (rect2);
			rgn2 = rgn1.Clone ();

			rects = rgn1.GetRegionScans (matrix);
			rects2 = rgn2.GetRegionScans (matrix);

			Assert.AreEqual (rects.Length, rects2.Length);

			for (int i = 0; i < rects.Length; i++) {

				Assert.AreEqual (rects[i].X, rects[i].X);
				Assert.AreEqual (rects[i].Y, rects[i].Y);
				Assert.AreEqual (rects[i].Width, rects[i].Width);
				Assert.AreEqual (rects[i].Height, rects[i].Height);
			}

			Assert.AreEqual (true, rgn1.Equals (rgn2, dc));
		}

		 /*Tests infinite, empty, etc*/
		[Test]
		public void TestInfiniteAndEmpty()
		{
			Bitmap bmp = new Bitmap (600, 800);
			Graphics dc = Graphics.FromImage (bmp);
			Rectangle rect1, rect2;
			Region rgn1;
			RectangleF [] rects;
			Matrix matrix = new Matrix ();

			rect1 = new Rectangle (500, 30, 60, 80);
			rect2 = new Rectangle (520, 40, 60, 80);
			rgn1 = new Region (rect1);
			rgn1.Union (rect2);

			Assert.AreEqual (false, rgn1.IsEmpty (dc));
			Assert.AreEqual (false, rgn1.IsInfinite (dc));

			rgn1.MakeEmpty();
			Assert.AreEqual (true, rgn1.IsEmpty (dc));

			rgn1 = new Region (rect1);
			rgn1.Union (rect2);
			rgn1.MakeInfinite ();
			rects = rgn1.GetRegionScans (matrix);

			Assert.AreEqual (1, rects.Length);
			Assert.AreEqual (-4194304, rects[0].X);
			Assert.AreEqual (-4194304, rects[0].Y);
			Assert.AreEqual (8388608, rects[0].Width);
			Assert.AreEqual (8388608, rects[0].Height);
			Assert.AreEqual (true, rgn1.IsInfinite (dc));
		}


		[Test]
		public void TestUnionGroup1 ()
		{
			Bitmap bmp = new Bitmap (600, 800);
			Graphics dc = Graphics.FromImage (bmp);
			Matrix matrix = new Matrix ();
			Rectangle rect1, rect2, rect3, rect4;
			Region rgn1, rgn2, rgn3, rgn4;
			RectangleF [] rects;

			rect1 = new Rectangle (500, 30, 60, 80);
			rect2 = new Rectangle (520, 40, 60, 80);
			rgn1 = new Region(rect1);
			rgn2 = new Region(rect2);
			rgn1.Union(rgn2);
			rects = rgn1.GetRegionScans (matrix);

			Assert.AreEqual (3, rects.Length);
			Assert.AreEqual (500, rects[0].X);
			Assert.AreEqual (30, rects[0].Y);
			Assert.AreEqual (60, rects[0].Width);
			Assert.AreEqual (10, rects[0].Height);

			Assert.AreEqual (500, rects[1].X);
			Assert.AreEqual (40, rects[1].Y);
			Assert.AreEqual (80, rects[1].Width);
			Assert.AreEqual (70, rects[1].Height);

			Assert.AreEqual (520, rects[2].X);
			Assert.AreEqual (110, rects[2].Y);
			Assert.AreEqual (60, rects[2].Width);
			Assert.AreEqual (10, rects[2].Height);

			rect1 = new Rectangle (20, 180, 40, 50);
			rect2 = new Rectangle (50, 190, 40, 50);
			rect3 = new Rectangle (70, 210, 30, 50);
			rgn1 = new Region (rect1);
			rgn2 = new Region (rect2);
			rgn3 = new Region (rect3);

			rgn1.Union (rgn2);
			rgn1.Union (rgn3);
			rects = rgn1.GetRegionScans (matrix);
			Assert.AreEqual (5, rects.Length);

			Assert.AreEqual (20, rects[0].X);
			Assert.AreEqual (180, rects[0].Y);
			Assert.AreEqual (40, rects[0].Width);
			Assert.AreEqual (10, rects[0].Height);

			Assert.AreEqual (20, rects[1].X);
			Assert.AreEqual (190, rects[1].Y);
			Assert.AreEqual (70, rects[1].Width);
			Assert.AreEqual (20, rects[1].Height);

			Assert.AreEqual (20, rects[2].X);
			Assert.AreEqual (210, rects[2].Y);
			Assert.AreEqual (80, rects[2].Width);
			Assert.AreEqual (20, rects[2].Height);

			Assert.AreEqual (50, rects[3].X);
			Assert.AreEqual (230, rects[3].Y);
			Assert.AreEqual (50, rects[3].Width);
			Assert.AreEqual (10, rects[3].Height);

			Assert.AreEqual (70, rects[4].X);
			Assert.AreEqual (240, rects[4].Y);
			Assert.AreEqual (30, rects[4].Width);
			Assert.AreEqual (20, rects[4].Height);

			rect1 = new Rectangle (20, 330, 40, 50);
			rect2 = new Rectangle (50, 340, 40, 50);
			rect3 = new Rectangle (70, 360, 30, 50);
			rect4 = new Rectangle (80, 400, 30, 10);
			rgn1 = new Region (rect1);
			rgn2 = new Region (rect2);
			rgn3 = new Region (rect3);
			rgn4 = new Region (rect4);

			rgn1.Union (rgn2);
			rgn1.Union (rgn3);
			rgn1.Union (rgn4);

			rects = rgn1.GetRegionScans (matrix);

			Assert.AreEqual (6, rects.Length);

			Assert.AreEqual (20, rects[0].X);
			Assert.AreEqual (330, rects[0].Y);
			Assert.AreEqual (40, rects[0].Width);
			Assert.AreEqual (10, rects[0].Height);

			Assert.AreEqual (20, rects[1].X);
			Assert.AreEqual (340, rects[1].Y);
			Assert.AreEqual (70, rects[1].Width);
			Assert.AreEqual (20, rects[1].Height);

			Assert.AreEqual (20, rects[2].X);
			Assert.AreEqual (360, rects[2].Y);
			Assert.AreEqual (80, rects[2].Width);
			Assert.AreEqual (20, rects[2].Height);

			Assert.AreEqual (50, rects[3].X);
			Assert.AreEqual (380, rects[3].Y);
			Assert.AreEqual (50, rects[3].Width);
			Assert.AreEqual (10, rects[3].Height);

			Assert.AreEqual (70, rects[4].X);
			Assert.AreEqual (390, rects[4].Y);
			Assert.AreEqual (30, rects[4].Width);
			Assert.AreEqual (10, rects[4].Height);

			Assert.AreEqual (70, rects[5].X);
			Assert.AreEqual (400, rects[5].Y);
			Assert.AreEqual (40, rects[5].Width);
			Assert.AreEqual (10, rects[5].Height);

			rect1 = new Rectangle (10, 20, 50, 50);
			rect2 = new Rectangle (100, 100, 60, 60);
			rect3 = new Rectangle (200, 200, 80, 80);

			rgn1 = new Region (rect1);
			rgn1.Union (rect2);
			rgn1.Union (rect3);

			rects = rgn1.GetRegionScans (matrix);

			Assert.AreEqual (3, rects.Length);

			Assert.AreEqual (10, rects[0].X);
			Assert.AreEqual (20, rects[0].Y);
			Assert.AreEqual (50, rects[0].Width);
			Assert.AreEqual (50, rects[0].Height);

			Assert.AreEqual (100, rects[1].X);
			Assert.AreEqual (100, rects[1].Y);
			Assert.AreEqual (60, rects[1].Width);
			Assert.AreEqual (60, rects[1].Height);

			Assert.AreEqual (200, rects[2].X);
			Assert.AreEqual (200, rects[2].Y);
			Assert.AreEqual (80, rects[2].Width);
			Assert.AreEqual (80, rects[2].Height);
		}

		void AssertEqualRectangles (RectangleF rect1, RectangleF rect2, string text)
		{
			Assert.AreEqual (rect1.X, rect2.X, text + ".X");
			Assert.AreEqual (rect1.Y, rect2.Y, text + ".Y");
			Assert.AreEqual (rect1.Width, rect2.Width, text + ".Width");
			Assert.AreEqual (rect1.Height, rect2.Height, text + ".Height");
		}

		[Test]
		public void TestUnionGroup2 ()
		{
			RectangleF[] rects;
			Region r1  = new Region ();
			Rectangle rect2 = Rectangle.Empty;
			Rectangle rect1 = Rectangle.Empty;
			Rectangle rect3 = Rectangle.Empty;
			Rectangle rect4 = Rectangle.Empty;

			{ // TEST1: Not intersecting rects. Union just adds them

				rect1 = new Rectangle (20, 20, 20, 20);
				rect2 = new Rectangle (20, 80, 20, 10);
				rect3 = new Rectangle (60, 60, 30, 10);

				r1 = new Region (rect1);
				r1.Union (rect2);
				r1.Union (rect3);

				rects = r1.GetRegionScans (new Matrix ());
				Assert.AreEqual (3, rects.Length, "TUG1Test1");
				AssertEqualRectangles (new RectangleF (20, 20, 20, 20), rects[0], "TUG1Test2");
				AssertEqualRectangles (new RectangleF (60, 60, 30, 10), rects[1], "TUG1Test3");
				AssertEqualRectangles (new RectangleF (20, 80, 20, 10), rects[2], "TUG1Test4");
			}

			{ // TEST2: Intersecting from the right
				 /*
				 *  -----------
				 *  |         |
				 *  |     |-------- |
				 *  |     |         |
				 *  |     |-------- |
				 *  |	      |
				 *  ----------|
				 *
				 */

				rect1 = new Rectangle (10, 10, 100, 100);
				rect2 = new Rectangle (40, 60, 100, 20);
				r1 = new Region (rect1);
				r1.Union (rect2);

				rects = r1.GetRegionScans (new Matrix ());
				Assert.AreEqual  (3, rects.Length, "TUG2Test1");
				AssertEqualRectangles (new RectangleF (10, 10, 100, 50), rects[0], "TUG2Test2");
				AssertEqualRectangles (new RectangleF (10, 60, 130, 20), rects[1], "TUG2Test3");
				AssertEqualRectangles (new RectangleF (10, 80, 100, 30), rects[2], "TUG2Test4");
			}

			{ // TEST3: Intersecting from the right
				 /*
				 *  	-----------
				 *  	|         |
				 * |-------- |    |
				 * |         |    |
				 * |-------- |    |
				 *  	|	  |
				 *  	----------|
				 *
				 */

				rect1 = new Rectangle (70, 10, 100, 100);
				rect2 = new Rectangle (40, 60, 100, 20);

				r1 = new Region (rect1);
				r1.Union (rect2);

				rects = r1.GetRegionScans (new Matrix ());
				Assert.AreEqual  (3, rects.Length, "TUG3Test1");
				AssertEqualRectangles (new RectangleF (70, 10, 100, 50), rects[0], "TUG3Test2");
				AssertEqualRectangles (new RectangleF (40, 60, 130, 20), rects[1], "TUG3Test3");
				AssertEqualRectangles (new RectangleF (70, 80, 100, 30), rects[2], "TUG3Test4");
			}

			{ // TEST4: Intersecting from the top
				 /*
				 *  	   -----
				 *  	   |   |
				 *  	-----------
				 *  	|  |   |  |
				 *  	|  -----  |
				 *  	|         |
				 *  	|	  |
				 *  	----------|
				 *
				 */

				rect1 = new Rectangle (40, 100, 100, 100);
				rect2 = new Rectangle (70, 80, 50, 40);
				r1 = new Region (rect1);
				r1.Union (rect2);

				rects = r1.GetRegionScans (new Matrix ());
				Assert.AreEqual  (2, rects.Length, "TUG4Test1");
				AssertEqualRectangles (new RectangleF (70, 80, 50, 20), rects[0], "TUG4Test2");
				AssertEqualRectangles (new RectangleF (40, 100, 100, 100), rects[1], "TUG4Test3");
			}

			{ // TEST5: Intersecting from the bottom
				 /*

				 *  	-----------
				 *  	|  	  |
				 *  	|  	  |
				 *  	|         |
				 *  	|  |   |  |
				 *  	|--|   |--|
				 *	   |   |
				 *  	   -----
				 */

				rect1 = new Rectangle (40, 10, 100, 100);
				rect2 = new Rectangle (70, 80, 50, 40);

				r1 = new Region (rect1);
				r1.Union (rect2);

				rects = r1.GetRegionScans (new Matrix ());
				Assert.AreEqual  (2, rects.Length, "TUG5Test1");
				AssertEqualRectangles (new RectangleF (40, 10, 100, 100), rects[0], "TUG5Test2");
				AssertEqualRectangles (new RectangleF (70, 110, 50, 10), rects[1], "TUG5Test3");
			}

			{ // TEST6: Multiple regions, two separted by zero pixels

				rect1 = new Rectangle (30, 30, 80, 80);
				rect2 = new Rectangle (45, 45, 200, 200);
				rect3 = new Rectangle (160, 260, 10, 10);
				rect4 = new Rectangle (170, 260, 10, 10);

				r1 = new Region (rect1);
				r1.Union (rect2);
				r1.Union (rect3);
				r1.Union (rect4);

				rects = r1.GetRegionScans (new Matrix ());
				Assert.AreEqual  (4, rects.Length, "TUG6Test1");
				AssertEqualRectangles (new RectangleF (30, 30, 80, 15), rects[0], "TUG6Test2");
				AssertEqualRectangles (new RectangleF (30, 45, 215, 65), rects[1], "TUG6Test3");
				AssertEqualRectangles (new RectangleF (45, 110, 200, 135), rects[2], "TUG6Test4");
				AssertEqualRectangles (new RectangleF (160, 260, 20, 10), rects[3], "TUG6Test5");
			}
		}


		[Test]
		public void TestComplementGroup1 ()
		{
			RectangleF[] rects;
			Region r1  = new Region ();
			Region r2 = new Region ();
			Rectangle rect1 = Rectangle.Empty;
			Rectangle rect2 = Rectangle.Empty;
			Rectangle rect3 = Rectangle.Empty;
			Rectangle rect4 = Rectangle.Empty;
			Rectangle rect5 = Rectangle.Empty;
			Rectangle rect6 = Rectangle.Empty;
			Rectangle rect7 = Rectangle.Empty;


			{ // TEST1

				rect1 = new Rectangle (20, 20, 20, 20);
				rect2 = new Rectangle (20, 80, 20, 10);
				rect3 = new Rectangle (60, 60, 30, 10);

				r1 = new Region (rect1);
				r2 = new Region (rect2);
				r2.Union (rect3);
				r1.Complement (r2);

				rects = r1.GetRegionScans (new Matrix ());
				Assert.AreEqual  (2, rects.Length, "TCG1Test1");
				AssertEqualRectangles (new RectangleF (60, 60, 30, 10), rects[0], "TCG1Test2");
				AssertEqualRectangles (new RectangleF (20, 80, 20, 10), rects[1], "TCG1Test3");
			}


			{ // TEST2

				rect1 = new Rectangle (10, 10, 100, 100);
				rect2 = new Rectangle (40, 60, 100, 20);

				r1 = new Region (rect1);
				r1.Complement (rect2);

				rects = r1.GetRegionScans (new Matrix ());
				Assert.AreEqual  (1, rects.Length, "TCG2Test1");
				AssertEqualRectangles (new RectangleF (110, 60, 30, 20), rects[0], "TCG2Test2");
			}

			{ // TEST3

				rect1 = new Rectangle (70, 10, 100, 100);
				rect2 = new Rectangle (40, 60, 100, 20);

				r1 = new Region (rect1);
				r1.Complement (rect2);

				rects = r1.GetRegionScans (new Matrix ());
				Assert.AreEqual  (1, rects.Length, "TCG3Test1");
				AssertEqualRectangles (new RectangleF (40, 60, 30, 20), rects[0], "TCG3Test2");
			}

			{ // TEST4

				rect1 = new Rectangle (40, 100, 100, 100);
				rect2 = new Rectangle (70, 80, 50, 40);

				r1 = new Region (rect1);
				r1.Complement (rect2);

				rects = r1.GetRegionScans (new Matrix ());
				Assert.AreEqual  (1, rects.Length, "TCG4Test1");
				AssertEqualRectangles (new RectangleF (70, 80, 50, 20), rects[0], "TCG4Test2");
			}

			{ // TEST5

				rect1 = new Rectangle (40, 10, 100, 100);
				rect2 = new Rectangle (70, 80, 50, 40);

				r1 = new Region (rect1);
				r1.Complement (rect2);

				rects = r1.GetRegionScans (new Matrix ());
				Assert.AreEqual  (1, rects.Length, "TCG5Test1");
				AssertEqualRectangles (new RectangleF (70, 110, 50, 10), rects[0], "TCG5Test2");
			}

			{ // TEST6: Multiple regions

				rect1 = new Rectangle (30, 30, 80, 80);
				rect2 = new Rectangle (45, 45, 200, 200);
				rect3 = new Rectangle (160, 260, 10, 10);
				rect4 = new Rectangle (170, 260, 10, 10);

				r1 = new Region (rect1);
				r1.Complement (rect2);
				r1.Complement (rect3);
				r1.Complement (rect4);

				rects = r1.GetRegionScans (new Matrix ());
				Assert.AreEqual  (1, rects.Length, "TCG6Test1");
				AssertEqualRectangles (new RectangleF (170, 260, 10, 10), rects[0], "TCG6Test2");
			}

		}

		[Test]
		public void TestComplementGroup2 ()
		{

			Bitmap bmp = new Bitmap (600, 800);
			Graphics dc = Graphics.FromImage (bmp);
			Matrix matrix = new Matrix ();
			Rectangle rect1, rect2;
			Region rgn1, rgn2;
			RectangleF [] rects;

			rect1 = new Rectangle (20, 30, 60, 80);
			rect2 = new Rectangle (50, 40, 60, 80);
			rgn1 = new Region (rect1);
			rgn2 = new Region (rect2);
			dc.DrawRectangle (Pens.Green, rect1);
			dc.DrawRectangle (Pens.Red, rect2);
			rgn1.Complement (rgn2);
			dc.FillRegion (Brushes.Blue, rgn1);
			dc.DrawRectangles (Pens.Yellow, rgn1.GetRegionScans (matrix));

			rects = rgn1.GetRegionScans (matrix);

			Assert.AreEqual (2, rects.Length);

			Assert.AreEqual (80, rects[0].X);
			Assert.AreEqual (40, rects[0].Y);
			Assert.AreEqual (30, rects[0].Width);
			Assert.AreEqual (70, rects[0].Height);

			Assert.AreEqual (50, rects[1].X);
			Assert.AreEqual (110, rects[1].Y);
			Assert.AreEqual (60, rects[1].Width);
			Assert.AreEqual (10, rects[1].Height);

		}


		[Test]
		public void TestExcludeGroup1 ()
		{
			RectangleF[] rects;
			Region r1  = new Region ();
			Region r2 = new Region ();
			Rectangle rect1 = Rectangle.Empty;
			Rectangle rect2 = Rectangle.Empty;
			Rectangle rect3 = Rectangle.Empty;
			Rectangle rect4 = Rectangle.Empty;
			Rectangle rect5 = Rectangle.Empty;
			Rectangle rect6 = Rectangle.Empty;
			Rectangle rect7 = Rectangle.Empty;


			{ // TEST1: Not intersecting rects. Exclude just adds them

				rect1 = new Rectangle (20, 20, 20, 20);
				rect2 = new Rectangle (20, 80, 20, 10);
				rect3 = new Rectangle (60, 60, 30, 10);

				r1 = new Region (rect1);
				r2 = new Region (rect2);
				r2.Union (rect3);
				r1.Exclude (r2);

				rects = r1.GetRegionScans (new Matrix ());
				Assert.AreEqual  (1, rects.Length, "TEG1Test1");
				AssertEqualRectangles (new RectangleF (20, 20, 20, 20), rects[0], "TEG1Test2");
			}

			{ // TEST2: Excluding from the right
				 /*
				 *  -----------
				 *  |         |
				 *  |     |-------- |
				 *  |     |         |
				 *  |     |-------- |
				 *  |	      |
				 *  ----------|
				 *
				 */

				rect1 = new Rectangle (10, 10, 100, 100);
				rect2 = new Rectangle (40, 60, 100, 20);
				r1 = new Region (rect1);
				r1.Exclude (rect2);

				rects = r1.GetRegionScans (new Matrix ());
				Assert.AreEqual  (3, rects.Length, "TEG2Test1");
				AssertEqualRectangles (new RectangleF (10, 10, 100, 50), rects[0], "TEG2Test2");
				AssertEqualRectangles (new RectangleF (10, 60, 30, 20), rects[1], "TEG2Test3");
				AssertEqualRectangles (new RectangleF (10, 80, 100, 30), rects[2], "TEG2Test4");
			}


			{ // TEST3: Intersecting from the right
				 /*
				 *  	-----------
				 *  	|         |
				 * |-------- |    |
				 * |         |    |
				 * |-------- |    |
				 *  	|	  |
				 *  	----------|
				 *
				 */

				rect1 = new Rectangle (70, 10, 100, 100);
				rect2 = new Rectangle (40, 60, 100, 20);

				r1 = new Region (rect1);
				r1.Exclude (rect2);

				rects = r1.GetRegionScans (new Matrix ());
				Assert.AreEqual  (3, rects.Length, "TEG3Test1");
				AssertEqualRectangles (new RectangleF (70, 10, 100, 50), rects[0], "TEG3Test2");
				AssertEqualRectangles (new RectangleF (140, 60, 30, 20), rects[1], "TEG3Test3");
				AssertEqualRectangles (new RectangleF (70, 80, 100, 30), rects[2], "TEG3Test4");
			}


			{ // TEST4: Intersecting from the top
				 /*
				 *  	   -----
				 *  	   |   |
				 *  	-----------
				 *  	|  |   |  |
				 *  	|  -----  |
				 *  	|         |
				 *  	|	  |
				 *  	----------|
				 *
				 */

				rect1 = new Rectangle (40, 100, 100, 100);
				rect2 = new Rectangle (70, 80, 50, 40);

				r1 = new Region (rect1);
				r1.Exclude (rect2);

				rects = r1.GetRegionScans (new Matrix ());
				Assert.AreEqual  (3, rects.Length, "TEG4Test1");
				AssertEqualRectangles (new RectangleF (40, 100, 30, 20), rects[0], "TEG4Test2");
				AssertEqualRectangles (new RectangleF (120, 100, 20, 20), rects[1], "TEG4Test3");
				AssertEqualRectangles (new RectangleF (40, 120, 100, 80), rects[2], "TEG4Test4");
			}


			{ // TEST5: Intersecting from the bottom
				 /*
				 *  	-----------
				 *  	|  	  |
				 *  	|         |
				 *  	|         |
				 *  	|  |   |  |
				 *  	|--|   |--|
				 *	   |   |
				 *  	   -----
				 *
				 */

				rect1 = new Rectangle (40, 10, 100, 100);
				rect2 = new Rectangle (70, 80, 50, 40);

				r1 = new Region (rect1);
				r1.Exclude (rect2);

				rects = r1.GetRegionScans (new Matrix ());
				Assert.AreEqual (3, rects.Length, "TEG5Test1");
				AssertEqualRectangles (new RectangleF (40, 10, 100, 70), rects[0], "TEG5Test2");
				AssertEqualRectangles (new RectangleF (40, 80, 30, 30), rects[1], "TEG5Test3");
				AssertEqualRectangles (new RectangleF (120, 80, 20, 30), rects[2], "TEG5Test4");
			}


			{ // TEST6: Multiple regions

				rect1 = new Rectangle (30, 30, 80, 80);
				rect2 = new Rectangle (45, 45, 200, 200);
				rect3 = new Rectangle (160, 260, 10, 10);
				rect4 = new Rectangle (170, 260, 10, 10);

				r1 = new Region (rect1);
				r1.Exclude (rect2);
				r1.Exclude (rect3);
				r1.Exclude (rect4);

				rects = r1.GetRegionScans (new Matrix ());
				Assert.AreEqual (2, rects.Length, "TEG6Test1");
				AssertEqualRectangles (new RectangleF (30, 30, 80, 15), rects[0], "TEG6Test2");
				AssertEqualRectangles (new RectangleF (30, 45, 15, 65), rects[1], "TEG6Test3");
			}


			{ // TEST7: Intersecting from the top with a larger rect
				 /*
				 *    -----------------
				 *    |               |
				 *    |	-----------   |
				 *  	|  |   |  |
				 *  	|  -----  |
				 *  	|         |
				 *  	|	  |
				 *  	----------|
				 *
				 */

				rect1 = new Rectangle (50, 100, 100, 100);
				rect2 = new Rectangle (30, 70, 150, 40);

				r1 = new Region (rect1);
				r1.Exclude (rect2);

				rects = r1.GetRegionScans (new Matrix ());
				Assert.AreEqual (1, rects.Length, "TEG7Test1");
				AssertEqualRectangles (new RectangleF (50, 110, 100, 90), rects[0], "TEG7Test2");
			}

			{ // TEST8: Intersecting from the right with a larger rect
				 /*
				 *
				 * |--------|
				 * |	    |
				 * |	-----------
				 * | 	|         |
				 * |	|	  |
				 * |    |	  |
				 * |    |         |
				 * | 	|	  |
				 * | 	----------|
				 * |-------|
				 */

				rect1 = new Rectangle (70, 60, 100, 70);
				rect2 = new Rectangle (40, 10, 100, 150);

				r1 = new Region (rect1);
				r1.Exclude (rect2);

				rects = r1.GetRegionScans (new Matrix ());
				Assert.AreEqual (1, rects.Length, "TEG8Test1");
				AssertEqualRectangles (new RectangleF (140, 60, 30, 70), rects[0], "TEG8Test2");

			}

			{ // TEST9: Intersecting from the left with a larger rect
				 /*
				 *
				 * 		|--------|
				 * 		|	 |
				 * 	-----------      |
				 *  	|         |      |
				 * 	|	  |      |
				 *      |	  |	 |
				 *      |         |	 |
				 *  	|	  |	 |
				 *  	----------|      |
				 * 		|--------|
				 *
				 */


				rect1 = new Rectangle (70, 60, 100, 70);
				rect2 = new Rectangle (100, 10, 100, 150);

				r1 = new Region (rect1);
				r1.Exclude (rect2);

				rects = r1.GetRegionScans (new Matrix ());
				Assert.AreEqual (1, rects.Length, "TEG9Test1");
				AssertEqualRectangles (new RectangleF (70, 60, 30, 70), rects[0], "TEG9Test2");
			}


			{ // TEST10: Intersecting from the bottom with a larger rect
				 /*
				 * *
				 * 		|--------|
				 * 		|	 |
				 * 		|	 |
				 * 		|	 |
				 * 	  --------------------
				 *  	  |        	     |
				 * 	  |  		     |
				 *  	  |------------------|
				 */


				rect1 = new Rectangle (20, 20, 100, 100);
				rect2 = new Rectangle (10, 80, 140, 150);

				r1 = new Region (rect1);
				r1.Exclude (rect2);

				rects = r1.GetRegionScans (new Matrix ());
				Assert.AreEqual (1, rects.Length, "TEG10Test1");
				AssertEqualRectangles (new RectangleF (20, 20, 100, 60), rects[0], "TEG10Test2");
			}


		}

		[Test]
		public void TestExcludeGroup2 ()
		{
			Bitmap bmp = new Bitmap (600, 800);
			Graphics dc = Graphics.FromImage (bmp);
			Matrix matrix = new Matrix ();
			Rectangle rect1, rect2;
			Region rgn1;
			RectangleF [] rects;

			rect1 = new Rectangle (130, 30, 60, 80);
			rect2 = new Rectangle (170, 40, 60, 80);
			rgn1 = new Region (rect1);
			rgn1.Exclude (rect2);
			rects = rgn1.GetRegionScans (matrix);

			Assert.AreEqual (2, rects.Length);

			Assert.AreEqual (130, rects[0].X);
			Assert.AreEqual (30, rects[0].Y);
			Assert.AreEqual (60, rects[0].Width);
			Assert.AreEqual (10, rects[0].Height);

			Assert.AreEqual (130, rects[1].X);
			Assert.AreEqual (40, rects[1].Y);
			Assert.AreEqual (40, rects[1].Width);
			Assert.AreEqual (70, rects[1].Height);
		}

		[Test]
		public void ExcludeBug402613 ()
		{
			Region r = new Region();
			r.MakeInfinite ();
			r.Exclude (new Rectangle (387,292,189,133));
			r.Exclude (new Rectangle (387,66,189,133));
			Assert.IsTrue (r.IsVisible (new Rectangle (66,292,189,133)));
		}

		[Test]
		public void TestIntersect()
		{


			Bitmap bmp = new Bitmap (600, 800);
			Graphics dc = Graphics.FromImage (bmp);
			Matrix matrix = new Matrix ();
			RectangleF [] rects;
			RectangleF rect3, rect4;
			Region rgn3, rgn4;

			/* Two simple areas */
			Rectangle rect1 = new Rectangle (260, 30, 60, 80);
			Rectangle rect2 = new Rectangle (290, 40, 60, 80);
			Region rgn1 = new Region (rect1);
			Region rgn2 = new Region (rect2);
			rgn1.Intersect (rgn2);

			rects = rgn1.GetRegionScans (matrix);
			Assert.AreEqual (1, rects.Length);

			Assert.AreEqual (290, rects[0].X);
			Assert.AreEqual (40, rects[0].Y);
			Assert.AreEqual (30, rects[0].Width);
			Assert.AreEqual (70, rects[0].Height);							

			/* No intersect */
			rect1 = new Rectangle (20, 330, 40, 50);
			rect2 = new Rectangle (50, 340, 40, 50);
			rect3 = new Rectangle (70, 360, 30, 50);
			rect4 = new Rectangle (80, 400, 30, 10);
			rgn1 = new Region (rect1);
			rgn2 = new Region (rect2);
			rgn3 = new Region (rect3);
			rgn4 = new Region (rect4);

			rgn1.Intersect (rgn2);
			rgn1.Intersect (rgn3);
			rgn1.Intersect (rgn4);
			rects = rgn1.GetRegionScans (matrix);
			Assert.AreEqual (0, rects.Length);
		}

		[Test]
		public void TestXor()
		{
			Bitmap bmp = new Bitmap (600, 800);
			Graphics dc = Graphics.FromImage (bmp);
			Matrix matrix = new Matrix ();
			RectangleF [] rects;

			Rectangle rect1 = new Rectangle (380, 30, 60, 80);
			Rectangle rect2 = new Rectangle (410, 40, 60, 80);
			Region rgn1 = new Region (rect1);
			Region rgn2 = new Region (rect2);
			rgn1.Xor (rgn2);


			rects = rgn1.GetRegionScans (matrix);

			Assert.AreEqual (4, rects.Length);

			Assert.AreEqual (380, rects[0].X);
			Assert.AreEqual (30, rects[0].Y);
			Assert.AreEqual (60, rects[0].Width);
			Assert.AreEqual (10, rects[0].Height);

			Assert.AreEqual (380, rects[1].X);
			Assert.AreEqual (40, rects[1].Y);
			Assert.AreEqual (30, rects[1].Width);
			Assert.AreEqual (70, rects[1].Height);

			Assert.AreEqual (440, rects[2].X);
			Assert.AreEqual (40, rects[2].Y);
			Assert.AreEqual (30, rects[2].Width);
			Assert.AreEqual (70, rects[2].Height);

			Assert.AreEqual (410, rects[3].X);
			Assert.AreEqual (110, rects[3].Y);
			Assert.AreEqual (60, rects[3].Width);
			Assert.AreEqual (10, rects[3].Height);
		}


		[Test]
		public void TestIsVisible()
		{
			Bitmap bmp = new Bitmap (600, 800);
			Graphics dc = Graphics.FromImage (bmp);
			Rectangle rect1, rect2;
			Region rgn1, rgn2;
			Matrix matrix = new Matrix ();

			rect1 = new Rectangle (500, 30, 60, 80);
			rect2 = new Rectangle (520, 40, 60, 80);

			rgn1 = new Region (new RectangleF (0, 0, 10,10));
			Assert.AreEqual (false, rgn1.IsVisible (0,0,0,1));

			rgn1 = new Region (rect1);
			Assert.AreEqual (false, rgn1.IsVisible (500,29));
			Assert.AreEqual (true, rgn1.IsVisible (500,30));
			Assert.AreEqual (true, rgn1.IsVisible (rect1));
			Assert.AreEqual (true, rgn1.IsVisible (rect2));
			Assert.AreEqual (false, rgn1.IsVisible (new Rectangle (50,50,2,5)));

			Rectangle r = new Rectangle (1,1, 2,1);
			rgn2 = new Region (r);
			Assert.AreEqual (true, rgn2.IsVisible (r));
			Assert.AreEqual (true, rgn2.IsVisible (new Rectangle (1,1, 2,2)));
			Assert.AreEqual (true, rgn2.IsVisible (new Rectangle (1,1, 10,10)));
			Assert.AreEqual (true, rgn2.IsVisible (new Rectangle (1,1, 1,1)));
			Assert.AreEqual (false, rgn2.IsVisible (new Rectangle (2,2, 1,1)));
			Assert.AreEqual (false, rgn2.IsVisible (new Rectangle (0,0, 1,1)));
			Assert.AreEqual (false, rgn2.IsVisible (new Rectangle (3,3, 1,1)));

			Assert.AreEqual (false, rgn2.IsVisible (0,0));
			Assert.AreEqual (false, rgn2.IsVisible (1,0));
			Assert.AreEqual (false, rgn2.IsVisible (2,0));
			Assert.AreEqual (false, rgn2.IsVisible (3,0));
			Assert.AreEqual (false, rgn2.IsVisible (0,1));
			Assert.AreEqual (true, rgn2.IsVisible (1,1));
			Assert.AreEqual (true, rgn2.IsVisible (2,1));
			Assert.AreEqual (false, rgn2.IsVisible (3,1));
			Assert.AreEqual (false, rgn2.IsVisible (0,2));
			Assert.AreEqual (false, rgn2.IsVisible (1,2));
			Assert.AreEqual (false, rgn2.IsVisible (2,2));
			Assert.AreEqual (false, rgn2.IsVisible (3,2));


		}

		[Test]
		public void TestTranslate()
		{
			Region rgn1 = new Region (new RectangleF (10, 10, 120,120));
			rgn1.Translate (30,20);
			Matrix matrix = new Matrix ();

			RectangleF [] rects = rgn1.GetRegionScans (matrix);

			Assert.AreEqual (1, rects.Length);

			Assert.AreEqual (40, rects[0].X);
			Assert.AreEqual (30, rects[0].Y);
			Assert.AreEqual (120, rects[0].Width);
			Assert.AreEqual (120, rects[0].Height);
		}

		[Test]
		public void Constructor_GraphicsPath_Null ()
		{
			GraphicsPath gp = null;
			Assert.Throws<ArgumentNullException> (() => new Region (gp));
		}

		[Test]
		public void Constructor_RegionData_Null ()
		{
			RegionData rd = null;
			Assert.Throws<ArgumentNullException> (() => new Region (rd));
		}

		[Test]
		public void Union_GraphicsPath_Null ()
		{
			GraphicsPath gp = null;
			Assert.Throws<ArgumentNullException> (() => new Region ().Union (gp));
		}

		[Test]
		public void Union_Region_Null ()
		{
			Region r = null;
			Assert.Throws<ArgumentNullException> (() => new Region ().Union (r));
		}

		[Test]
		public void Union_Region_Infinite ()
		{
			// default ctor creates an infinite region
			Region r = new Region ();
			CheckEmpty ("default .ctor", r);
			// union-ing to infinity doesn't change the results
			r.Union (new Rectangle (10, 10, 100, 100));
			CheckEmpty ("U infinity", r);
		}

		[Test]
		public void Intersect_GraphicsPath_Null ()
		{
			GraphicsPath gp = null;
			Assert.Throws<ArgumentNullException> (() => new Region ().Intersect (gp));
		}

		[Test]
		public void Intersect_Region_Null ()
		{
			Region r = null;
			Assert.Throws<ArgumentNullException> (() => new Region ().Intersect (r));
		}

		[Test]
		public void Complement_GraphicsPath_Null ()
		{
			GraphicsPath gp = null;
			Assert.Throws<ArgumentNullException> (() => new Region ().Complement (gp));
		}

		[Test]
		public void Complement_Region_Null ()
		{
			Region r = null;
			Assert.Throws<ArgumentNullException> (() => new Region ().Complement (r));
		}

		[Test]
		public void Exclude_GraphicsPath_Null ()
		{
			GraphicsPath gp = null;
			Assert.Throws<ArgumentNullException> (() => new Region ().Exclude (gp));
		}

		[Test]
		public void Exclude_Region_Null ()
		{
			Region r = null;
			Assert.Throws<ArgumentNullException> (() => new Region ().Exclude (r));
		}

		[Test]
		public void Xor_GraphicsPath_Null ()
		{
			GraphicsPath gp = null;
			Assert.Throws<ArgumentNullException> (() => new Region ().Xor (gp));
		}

		[Test]
		public void Xor_Region_Null ()
		{
			Region r = null;
			Assert.Throws<ArgumentNullException> (() => new Region ().Xor (r));
		}

		[Test]
		public void GetBounds_Null ()
		{
			Assert.Throws<ArgumentNullException> (() => new Region ().GetBounds (null));
		}

		[Test]
		public void IsVisible_IntIntNull ()
		{
			Assert.IsTrue (new Region ().IsVisible (0, 0, null));
		}

		[Test]
		public void IsVisible_IntIntIntIntNull ()
		{
			Assert.IsFalse (new Region ().IsVisible (0, 0, 0, 0, null));
		}

		[Test]
		public void IsVisible_PointNull ()
		{
			Point p = new Point ();
			Assert.IsTrue (new Region ().IsVisible (p, null));
		}

		[Test]
		public void IsVisible_PointFNull ()
		{
			PointF p = new PointF ();
			Assert.IsTrue (new Region ().IsVisible (p, null));
		}

		[Test]
		public void IsVisible_RectangleNull ()
		{
			Rectangle r = new Rectangle ();
			Assert.IsFalse (new Region ().IsVisible (r, null));
		}

		[Test]
		public void IsVisible_RectangleFNull ()
		{
			RectangleF r = new RectangleF ();
			Assert.IsFalse (new Region ().IsVisible (r, null));
		}

		[Test]
		public void IsVisible_SingleSingleNull ()
		{
			Assert.IsTrue (new Region ().IsVisible (0f, 0f, null));
		}

		[Test]
		public void IsVisible_SingleSingleSingleSingleNull ()
		{
			Assert.IsFalse (new Region ().IsVisible (0f, 0f, 0f, 0f, null));
		}

		[Test]
		public void IsEmpty_Null ()
		{
			Assert.Throws<ArgumentNullException> (() => new Region ().IsEmpty (null));
		}

		[Test]
		public void IsInfinite_Null ()
		{
			Assert.Throws<ArgumentNullException> (() => new Region ().IsInfinite (null));
		}

		[Test]
		public void Equals_NullGraphics ()
		{
			Assert.Throws<ArgumentNullException> (() => new Region ().Equals (null, Graphics.FromImage (new Bitmap (10, 10))));
		}

		[Test]
		public void Equals_RegionNull ()
		{
			Assert.Throws<ArgumentNullException> (() => new Region ().Equals (new Region (), null));
		}

		[Test]
		[Category ("NotWorking")] // caused regression in SWF
		public void GetHrgn_Null ()
		{
			Assert.Throws<ArgumentNullException> (() => new Region ().GetHrgn (null));
		}

		[Test]
		public void GetRegionScans_Null ()
		{
			Assert.Throws<ArgumentNullException> (() => new Region ().GetRegionScans (null));
		}

		[Test]
		public void Transform_Null ()
		{
			Assert.Throws<ArgumentNullException> (() => new Region ().Transform (null));
		}

		// an "empty ctor" Region is infinite
		private void CheckEmpty (string prefix, Region region)
		{
			Assert.IsFalse (region.IsEmpty (graphic), prefix + "IsEmpty");
			Assert.IsTrue (region.IsInfinite (graphic), prefix + "graphic");

			RectangleF rect = region.GetBounds (graphic);
			Assert.AreEqual (-4194304f, rect.X, prefix + "GetBounds.X");
			Assert.AreEqual (-4194304f, rect.Y, prefix + "GetBounds.Y");
			Assert.AreEqual (8388608f, rect.Width, prefix + "GetBounds.Width");
			Assert.AreEqual (8388608f, rect.Height, prefix + "GetBounds.Height");
		}

		[Test]
		public void Region_Empty ()
		{
			Region region = new Region ();
			CheckEmpty ("Empty.", region);

			Region clone = region.Clone ();
			CheckEmpty ("Clone.", region);

			RegionData data = region.GetRegionData ();
			Region r2 = new Region (data);
			CheckEmpty ("RegionData.", region);
		}

		[Test]
		[Category ("NotWorking")]
		public void Region_Infinite_MultipleRectangles ()
		{
			Region region = new Region ();
			Assert.IsTrue (region.IsInfinite (graphic), "Empty.IsInfinite");

			GraphicsPath gp = new GraphicsPath ();
			gp.AddRectangle (new Rectangle (-4194304, -4194304, 8388608, 8388608));
			region = new Region (gp);
			Assert.IsTrue (region.IsInfinite (graphic), "OneRectangle.IsInfinite");

			gp.AddRectangle (new Rectangle (1, 1, 2, 2));
			region = new Region (gp);
			Assert.IsFalse (region.IsInfinite (graphic), "TwoOverlappingRectangle.IsInfinite");

			gp = new GraphicsPath ();
			gp.AddRectangle (new Rectangle (-4194304, -4194304, 4194304, 8388608));
			gp.AddRectangle (new Rectangle (0, -4194304, 4194304, 8388608));
			Assert.IsFalse (region.IsInfinite (graphic), "TwoSideBySideRectangle.IsInfinite");
		}

		[Test]
		public void Rectangle_GetRegionScans ()
		{
			Matrix matrix = new Matrix ();
			GraphicsPath gp = new GraphicsPath ();
			gp.AddRectangle (new Rectangle (10, 10, 10, 10));
			Region region = new Region (gp);
			Assert.AreEqual (1, region.GetRegionScans (matrix).Length, "1");

			gp.AddRectangle (new Rectangle (20, 20, 20, 20));
			region = new Region (gp);
			Assert.AreEqual (2, region.GetRegionScans (matrix).Length, "2");
		}

		[Test]
		public void InfinityExclude ()
		{
			using (Region r = new Region ()) {
				Assert.IsTrue (r.IsInfinite (graphic), "before");
				r.Exclude (new Rectangle (5, 5, 10, 10));
				Assert.IsFalse (r.IsInfinite (graphic), "after");
				RectangleF bounds = r.GetBounds (graphic);
				Assert.AreEqual (-4194304, bounds.X, "X");
				Assert.AreEqual (-4194304, bounds.Y, "Y");
				Assert.AreEqual (8388608, bounds.Width, "Width");
				Assert.AreEqual (8388608, bounds.Height, "Height");
			}
		}

		[Test]
		public void InfinityIntersect ()
		{
			using (Region r = new Region ()) {
				Assert.IsTrue (r.IsInfinite (graphic), "before");
				r.Intersect (new Rectangle (-10, -10, 20, 20));
				Assert.IsFalse (r.IsInfinite (graphic), "after");
				RectangleF bounds = r.GetBounds (graphic);
				Assert.AreEqual (-10, bounds.X, "X");
				Assert.AreEqual (-10, bounds.Y, "Y");
				Assert.AreEqual (20, bounds.Width, "Width");
				Assert.AreEqual (20, bounds.Height, "Height");
			}
		}

		[Test]
		public void InfinityIntersectTranslate ()
		{
			using (Region r = new Region ()) {
				Assert.IsTrue (r.IsInfinite (graphic), "before");
				r.Intersect (new Rectangle (-10, -10, 20, 20));
				r.Translate (10, 10);
				RectangleF bounds = r.GetBounds (graphic);
				Assert.AreEqual (0, bounds.X, "X");
				Assert.AreEqual (0, bounds.Y, "Y");
				Assert.AreEqual (20, bounds.Width, "Width");
				Assert.AreEqual (20, bounds.Height, "Height");
			}
		}

		[Test]
		public void InfinityIntersectScale ()
		{
			using (Region r = new Region ()) {
				Assert.IsTrue (r.IsInfinite (graphic), "before");
				r.Intersect (new Rectangle (-10, -10, 20, 20));
				using (Matrix m = new Matrix ()) {
					m.Scale (2, 0.5f);
					r.Transform (m);
				}
				RectangleF bounds = r.GetBounds (graphic);
				Assert.AreEqual (-20, bounds.X, "X");
				Assert.AreEqual (-5, bounds.Y, "Y");
				Assert.AreEqual (40, bounds.Width, "Width");
				Assert.AreEqual (10, bounds.Height, "Height");
			}
		}

		[Test]
		public void InfinityIntersectTransform ()
		{
			using (Region r = new Region ()) {
				Assert.IsTrue (r.IsInfinite (graphic), "before");
				r.Intersect (new Rectangle (-10, -10, 20, 20));
				using (Matrix m = new Matrix (2, 0, 0, 0.5f, 10, 10)) {
					r.Transform (m);
				}
				RectangleF bounds = r.GetBounds (graphic);
				Assert.AreEqual (-10, bounds.X, "X");
				Assert.AreEqual (5, bounds.Y, "Y");
				Assert.AreEqual (40, bounds.Width, "Width");
				Assert.AreEqual (10, bounds.Height, "Height");
			}
		}

		[Test]
		public void InfinityTranslate ()
		{
			using (Region r = new Region ()) {
				Assert.IsTrue (r.IsInfinite (graphic), "before");
				r.Translate (10, 10);
				Assert.IsTrue (r.IsInfinite (graphic), "after");
				CheckEmpty ("InfinityTranslate", r);
			}
		}

		[Test]
		public void InfinityScaleUp ()
		{
			using (Region r = new Region ()) {
				Assert.IsTrue (r.IsInfinite (graphic), "before");
				using (Matrix m = new Matrix ()) {
					m.Scale (2, 2);
					r.Transform (m);
				}
				Assert.IsTrue (r.IsInfinite (graphic), "after");
				CheckEmpty ("InfinityScaleUp", r);
			}
		}

		[Test]
		public void InfinityScaleDown ()
		{
			using (Region r = new Region ()) {
				Assert.IsTrue (r.IsInfinite (graphic), "before");
				using (Matrix m = new Matrix ()) {
					m.Scale (0.5f, 0.5f);
					r.Transform (m);
				}
				Assert.IsTrue (r.IsInfinite (graphic), "after");
				CheckEmpty ("InfinityScaleDown", r);
			}
		}

		[Test]
		public void InfinityRotate ()
		{
			using (Region r = new Region ()) {
				Assert.IsTrue (r.IsInfinite (graphic), "before");
				using (Matrix m = new Matrix ()) {
					m.Rotate (45);
					r.Transform (m);
				}
				Assert.IsTrue (r.IsInfinite (graphic), "after");
				CheckEmpty ("InfinityRotate", r);
			}
		}

		[Test]
		public void Intersect_383878 ()
		{
			using (Region clipRegion = new Region ()) {
				clipRegion.MakeInfinite ();

				Rectangle smaller = new Rectangle (5, 5, -10, -10);

				clipRegion.Intersect (smaller);
				Assert.IsFalse (clipRegion.IsEmpty (graphic), "IsEmpty");
				Assert.IsFalse (clipRegion.IsInfinite (graphic), "IsInfinite");

				RectangleF [] rects = clipRegion.GetRegionScans (new Matrix ());
				Assert.AreEqual (1, rects.Length, "Length");
				Assert.AreEqual (new RectangleF (-5, -5, 10, 10), rects [0], "0");
			}
		}

		[Test]
		public void Complement_383878 ()
		{
			using (Region clipRegion = new Region ()) {
				clipRegion.MakeInfinite ();

				Rectangle smaller = new Rectangle (5, 5, -10, -10);
				Rectangle bigger = new Rectangle (-5, -5, 12, 12);

				clipRegion.Intersect (smaller);
				clipRegion.Complement (bigger);

				Assert.IsFalse (clipRegion.IsEmpty (graphic), "IsEmpty");
				Assert.IsFalse (clipRegion.IsInfinite (graphic), "IsInfinite");

				RectangleF [] rects = clipRegion.GetRegionScans (new Matrix ());
				Assert.AreEqual (2, rects.Length, "Length");
				Assert.AreEqual (new RectangleF (5, -5, 2, 10), rects [0], "0");
				Assert.AreEqual (new RectangleF (-5, 5, 12, 2), rects [1], "1");
			}
		}
	}

	[TestFixture]
	// the test cases in this fixture aren't restricted wrt running unmanaged code
	public class RegionTestUnmanaged {
		private Bitmap bitmap;
		private Graphics graphic;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			bitmap = new Bitmap (10, 10);
			graphic = Graphics.FromImage (bitmap);
		}

		// Note: Test cases calling GetHrng will leak memory unless ReleaseHrgn
		// (which only exists in 2.0) is called.

		[Test]
		public void GetHrgn_Infinite_MakeEmpty ()
		{
			Region r = new Region ();
			Assert.IsFalse (r.IsEmpty (graphic), "!Empty");
			Assert.IsTrue (r.IsInfinite (graphic), "Infinite");
			Assert.AreEqual (IntPtr.Zero, r.GetHrgn (graphic), "Handle==0");

			r.MakeEmpty ();
			Assert.IsTrue (r.IsEmpty (graphic), "Empty");
			Assert.IsFalse (r.IsInfinite (graphic), "!Infinite");
			IntPtr h = r.GetHrgn (graphic);
			Assert.IsFalse (h == IntPtr.Zero, "Handle!=0");
			r.ReleaseHrgn (h);
		}

		[Test]
		public void GetHrgn_Empty_MakeInfinite ()
		{
			Region r = new Region (new GraphicsPath ());
			Assert.IsTrue (r.IsEmpty (graphic), "Empty");
			Assert.IsFalse (r.IsInfinite (graphic), "!Infinite");
			IntPtr h = r.GetHrgn (graphic);
			Assert.IsFalse (h == IntPtr.Zero, "Handle!=0");

			r.MakeInfinite ();
			Assert.IsFalse (r.IsEmpty (graphic), "!Empty");
			Assert.IsTrue (r.IsInfinite (graphic), "Infinite");
			Assert.AreEqual (IntPtr.Zero, r.GetHrgn (graphic), "Handle==0");
			r.ReleaseHrgn (h);
		}

		[Test]
		public void GetHrgn_TwiceFromSameRegionInstance ()
		{
			Region r = new Region (new GraphicsPath ());
			IntPtr h1 = r.GetHrgn (graphic);
			IntPtr h2 = r.GetHrgn (graphic);
			Assert.IsFalse (h1 == h2, "Handle_1!=Handle_2");
			r.ReleaseHrgn (h1);
			r.ReleaseHrgn (h2);
		}

		[Test]
		public void GetHrgn_FromHrgn ()
		{
			Region r1 = new Region (new GraphicsPath ());
			IntPtr h1 = r1.GetHrgn (graphic);
			Assert.IsFalse (h1 == IntPtr.Zero, "Handle_1!=0");

			Region r2 = Region.FromHrgn (h1);
			IntPtr h2 = r2.GetHrgn (graphic);
			Assert.IsFalse (h2 == IntPtr.Zero, "Handle_2!=0");
			Assert.IsFalse (h1 == h2, "Handle_1!=Handle_2");
			r1.ReleaseHrgn (h1);
			r2.ReleaseHrgn (h2);
		}

		[Test]
		public void FromHrgn_Zero ()
		{
			Assert.Throws<ArgumentException> (() => Region.FromHrgn (IntPtr.Zero));
		}
		[Test]
		public void ReleaseHrng_Zero ()
		{
			Region r = new Region (new GraphicsPath ());
			Assert.Throws<ArgumentNullException> (() => r.ReleaseHrgn (IntPtr.Zero));
		}

		[Test]
		public void ReleaseHrng ()
		{
			Region r = new Region (new GraphicsPath ());
			IntPtr ptr = r.GetHrgn (graphic);
			Assert.IsFalse (IntPtr.Zero == ptr, "ptr");
			r.ReleaseHrgn (ptr);
		}
	}
}
