//
// Test.System.Drawing.Graphics.cs
//
// Author:
//   Konstantin Triger <kostat@mainsoft.com>
//

//
// Copyright (C) 2005 Mainsoft, Corp (http://www.mainsoft.com)
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
using System.Collections;
using System.Diagnostics;
using NUnit.Framework;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing.Imaging;
using DrawingTestHelper;
using System.IO;

namespace Test.Sys.Drawing.GraphicsFixtures {
	#region GraphicsFixtureProps

	[TestFixture]
	public class GraphicsFixtureProps {

		protected DrawingTest t;
		const int TOLERANCE = 3; //in %

		[SetUp]
		public void SetUp() {
			t = DrawingTest.Create(512, 512);
		}

		[TearDown]
		public void TearDown ()
		{
			if (t != null)
				t.Dispose ();
		}

		[Test]
		public void ClipTest_1() {
			Region r = new Region();
			Assert.IsTrue(r.Equals(t.Graphics.Clip, t.Graphics));
		}

		[Test]
		public void ClipTest_2() {
			Region r = new Region(new Rectangle(10, 10, 60, 60));
			t.Graphics.Clip = r;
			Assert.IsTrue(r.Equals(t.Graphics.Clip, t.Graphics));

			Pen redPen   = new Pen(Color.Red, 3);
			Pen greenPen = new Pen(Color.Green, 3);
			// Create points that define curve.
			Point point1 = new Point( 50,  50);
			Point point2 = new Point(100,  25);
			Point point3 = new Point(200,   5);
			Point point4 = new Point(250,  50);
			Point point5 = new Point(300, 100);
			Point point6 = new Point(350, 200);
			Point point7 = new Point(250, 250);
			Point[] curvePoints = {
									  point1,
									  point2,
									  point3,
									  point4,
									  point5,
									  point6,
									  point7
								  };
			// Draw lines between original points to screen.
			t.Graphics.DrawLines(redPen, curvePoints);
			t.Show ();
			Assert.IsTrue(t.PDCompare(TOLERANCE));
		}

		[Test]
		public void ClipTest_3() {
			t.Graphics.TranslateTransform(3, 3);
			t.Graphics.SetClip(new Rectangle(23, 24, 30, 40));

			RectangleF cb = t.Graphics.VisibleClipBounds;
			DrawingTest.AssertAlmostEqual(23, cb.X);
			DrawingTest.AssertAlmostEqual(24, cb.Y);
			DrawingTest.AssertAlmostEqual(30, cb.Width);
			DrawingTest.AssertAlmostEqual(40, cb.Height);

			t.Graphics.PageUnit = GraphicsUnit.Millimeter;

			t.Graphics.RotateTransform(128);

			t.Graphics.TranslateTransform(14, 14);
			t.Graphics.ExcludeClip(new Rectangle(0, 0, 4, 60));

			
			t.Graphics.RotateTransform(128);
			
			t.Graphics.PageUnit = GraphicsUnit.Pixel;
			
			t.Graphics.TranslateClip(5.2f, 3.1f);

			t.Graphics.ResetTransform();
			t.Graphics.PageUnit = GraphicsUnit.Pixel;

			cb = t.Graphics.VisibleClipBounds;
			DrawingTest.AssertAlmostEqual(28, cb.X);
			DrawingTest.AssertAlmostEqual(22, cb.Y);
			DrawingTest.AssertAlmostEqual(30, cb.Width);
			DrawingTest.AssertAlmostEqual(40, cb.Height);
			
			t.Graphics.ScaleTransform(5, 7);
			t.Graphics.IntersectClip(new Rectangle(7, 4, 20, 20));

			cb = t.Graphics.VisibleClipBounds;
			DrawingTest.AssertAlmostEqual(7, cb.X);
			DrawingTest.AssertAlmostEqual(4f, cb.Y);
			DrawingTest.AssertAlmostEqual(4.6f, cb.Width);
			DrawingTest.AssertAlmostEqual(4.85714245f, cb.Height);
		}

		[Test]
		public void ClipBoundsTest() {
			Region r = new Region();
			Assert.IsTrue(t.Graphics.ClipBounds.Equals(r.GetBounds(t.Graphics)));

			RectangleF rf = new RectangleF(10, 10, 60, 60);
			r = new Region(rf);
			t.Graphics.Clip = r;
			Assert.IsTrue(rf.Equals(t.Graphics.ClipBounds));
		}

		[Test]
		public void CompositingModeTest() {
			//TODO: seems to draw equal images
			Assert.AreEqual(CompositingMode.SourceOver, t.Graphics.CompositingMode);

			Bitmap b = new Bitmap(100, 100);
			Graphics g = Graphics.FromImage(b);

			Color c = Color.FromArgb(100, Color.Red);

			Brush redBrush = new SolidBrush(c);
			g.FillEllipse(redBrush, 5, 6, 100, 200);
			//t.Graphics.FillEllipse(redBrush, 5, 6, 100, 200);
			t.Graphics.DrawImage(b, 10, 10);

			t.Show ();

			t.Graphics.CompositingMode = CompositingMode.SourceCopy;
			
			t.Graphics.DrawImage(b, 300, 300);

			t.Show ();
			Assert.IsTrue(t.PDCompare(TOLERANCE));
		}

		[Test] //TBD
		public void CompositingQualityTest() {
		}

		[Test]
		public void DpiXTest() {
			Assert.IsTrue(t.Graphics.DpiX == 96f);
		}

		[Test]
		public void DpiYTest() {
			Assert.IsTrue(t.Graphics.DpiY == 96f);
		}

		[Test] //TBD
		public void InterpolationModeTest() {
			Assert.AreEqual(InterpolationMode.Bilinear, t.Graphics.InterpolationMode);
		}

		[Test]
		public void IsClipEmtpyTest() {
			Assert.IsFalse(t.Graphics.IsClipEmpty);

			try {
				t.Graphics.Clip = null;
				Assert.Fail("The ArgumentNullException was not thrown");
			}
			catch(Exception e) {
				Assert.AreEqual(e.GetType(), typeof(ArgumentNullException));
			}

			Region r = new Region(new Rectangle(10, 10, 0, 0));
			t.Graphics.Clip = r;

			Assert.IsTrue( t.Graphics.IsClipEmpty);
		}

		[Test]
		public void IsVisibleClipEmtpyTest() {
			Assert.IsFalse(t.Graphics.IsVisibleClipEmpty, "default t.Graphics.IsVisibleClipEmpty");

			Region r = new Region(new Rectangle(512, 512, 100, 100));
			t.Graphics.Clip = r;
			Assert.IsFalse(t.Graphics.IsClipEmpty);
			Assert.IsTrue(t.Graphics.IsVisibleClipEmpty);
		}

		[Test]
		public void PageScaleTest() {
			Assert.AreEqual(1f, t.Graphics.PageScale);
		}

		[Test]
		public void PageUnitTest() {
			Assert.AreEqual(GraphicsUnit.Display, t.Graphics.PageUnit);
		}

		[Test]
		public void PixelOffsetModeTest() {
			Assert.AreEqual(PixelOffsetMode.Default, t.Graphics.PixelOffsetMode);
		}

		[Test]
		[Category("NotWorking")]
		public void RenderingOriginTest() {
			Assert.AreEqual(new Point(0,0), t.Graphics.RenderingOrigin);
		}

		[Test]
		public void SmoothingModeTest() {
			Assert.AreEqual(SmoothingMode.None, t.Graphics.SmoothingMode);
		}

		[Test]
		public void TextContrastTest() {
			Assert.AreEqual(4, t.Graphics.TextContrast);
		}

		[Test]
		public void TextRenderingHintTest() {
			Assert.AreEqual(TextRenderingHint.SystemDefault, t.Graphics.TextRenderingHint);
		}

		[Test]
		public void TransformTest() {
			Assert.AreEqual(new Matrix(), t.Graphics.Transform);
		}

		[Test]
		public void VisibleClipBoundsTest() {
			Assert.AreEqual(new RectangleF(0, 0, 512, 512), t.Graphics.VisibleClipBounds);
		}
	}

	#endregion

	#region DrawImage
	[TestFixture]
	public class DrawImage {
		protected DrawingTest t;
		protected int TOLERANCE = 10; //in %;
		protected Hashtable st = new Hashtable();

		Rectangle src = new Rectangle(0, 0, 50, 50);
		RectangleF srcF = new Rectangle(0, 0, 50, 50);
		Rectangle dst = new Rectangle(170, 170, 100, 100);
		RectangleF dstF = new Rectangle(270, 270, 100, 100);

		Image bmp;
		Image bmp2;

		[SetUp]
		public virtual void SetUp() {
			SetUp("DrawImage");
			DrawingTest.ShowForms = false;
			try {
				bmp = Bitmap.FromFile("bitmap50.png");
				bmp2 = Bitmap.FromFile("bitmap25.png");
			}
			catch(Exception e) {
				Console.WriteLine(e.Message);
			}
		}
		public virtual void SetUp(string ownerClass) {
			t = DrawingTest.Create(512, 512, ownerClass);
			t.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;

			// hashtable of differents tolerance values for specified tests.
		}
		[TearDown]
		public void TearDown() {
			if (t != null)
				t.Dispose ();
		}

		[Test]
		public void DrawImage1() {
			t.Graphics.DrawImage(bmp, new Point[]{new Point(170,10), new Point(250,0), new Point(100,100)}, src, GraphicsUnit.Pixel );
			t.Graphics.DrawImage(bmp, new PointF[]{new PointF(70,10), new PointF(150,0), new PointF(10,100)}, srcF, GraphicsUnit.Pixel );
			t.Show();
			Assert.IsTrue(t.Compare());
		}
		[Test]
		public void DrawImage2() {
			t.Graphics.DrawImage(bmp, dst, src, GraphicsUnit.Pixel);
			t.Graphics.DrawImage(bmp, dstF, srcF, GraphicsUnit.Pixel);
			t.Show();
			Assert.IsTrue(t.Compare());
		}
		[Test]
		public void DrawImage3() {
			t.Graphics.DrawImage(bmp, 10.0F, 10.0F, srcF, GraphicsUnit.Pixel);
			t.Graphics.DrawImage(bmp, 70.0F, 150.0F, 250.0F, 150.0F);
			t.Show();
			Assert.IsTrue(t.Compare());
		}
		[Test]
		public void DrawImage4() {
			t.Graphics.DrawImage(bmp, dst);
			t.Graphics.DrawImage(bmp, dstF);
			t.Show();
			Assert.IsTrue(t.Compare());
		}
		[Test]
		public void DrawImage5() {
			t.Graphics.SetClip( new Rectangle(70, 0, 20, 200));
			t.Graphics.DrawImage(bmp, new Point[]{new Point(50,50), new Point(250,30), new Point(100,150)}, src, GraphicsUnit.Pixel );
			t.Show();
			Assert.IsTrue(t.Compare());
		}
		[Test]
		public void DrawImage6() {
			t.Graphics.ScaleTransform(2, 2);
			t.Graphics.SetClip( new Rectangle(70, 0, 20, 200));
			t.Graphics.DrawImage(bmp, new Point[]{new Point(50,50), new Point(250,30), new Point(100,150)}, src, GraphicsUnit.Pixel );
			t.Show();
			Assert.IsTrue(t.Compare());
		}
		[Test]
		public void DrawImage7() {
			t.Graphics.DrawImage(bmp, 170, 70, src, GraphicsUnit.Pixel);
			t.Graphics.DrawImage(bmp, 70, 350, 350, 150);
			t.Show();
			Assert.IsTrue(t.Compare());
		}
		[Test]
		public void DrawImage8() {
			t.Graphics.DrawImage(bmp, new Point[]{new Point(170,10), new Point(250,10), new Point(100,100)} );
			t.Graphics.DrawImage(bmp, new PointF[]{new PointF(170,100), new PointF(250,100), new PointF(100,190)} );
			t.Show();
			Assert.IsTrue(t.Compare());
		}
		[Test]
		public void DrawImage9() {
			t.Graphics.DrawImage(bmp, 0, 0);
			t.Graphics.DrawImage(bmp, 200, 200);
			t.Show();
			Assert.IsTrue(t.Compare());
		}
		[Test]
		public void DrawImagePageUnit() {
			t.Graphics.PageUnit = GraphicsUnit.Document;
			Point [] p = new Point[]{
										new Point(100, 100),
										new Point(200, 100),
										new Point(50, 200)
									};

			t.Graphics.DrawImage(bmp2, p, new Rectangle(100, 100, 100, 100), GraphicsUnit.Pixel);
			t.Show();
			Assert.IsTrue(t.Compare());
		}
		[Test]
		public void DrawImagePageUnit_2() {
			t.Graphics.PageUnit = GraphicsUnit.Millimeter;
			t.Graphics.ScaleTransform(0.3f, 0.3f);
			Point [] p = new Point[]{
										new Point(100, 100),
										new Point(200, 100),
										new Point(50, 200)
									};

			t.Graphics.DrawImage(bmp2, p, new Rectangle(100, 100, 100, 100), GraphicsUnit.Pixel);
			t.Show();
			Assert.IsTrue(t.Compare());
		}
		[Test]
		public void DrawImagePageUnit_3() {
			t.Graphics.PageUnit = GraphicsUnit.Millimeter;
			t.Graphics.ScaleTransform(0.3f, 0.3f);
			t.Graphics.DrawImage(bmp2, new Rectangle(100, 100, 100, 100));
			t.Show();
			Assert.IsTrue(t.Compare());
		}
		[Test]
		public void DrawImagePageUnit_4() {
			t.Graphics.PageUnit = GraphicsUnit.Millimeter;
			t.Graphics.ScaleTransform(0.5f, 0.5f);
			t.Graphics.DrawImage(bmp, 50, 50);
			t.Show();
			Assert.IsTrue(t.Compare());
		}
		[Test]
		public void DrawImagePageUnitClip() {
			t.Graphics.PageUnit = GraphicsUnit.Millimeter;
			t.Graphics.ScaleTransform(0.3f, 0.3f);
			Point [] p = new Point[]{
										new Point(100, 100),
										new Point(200, 100),
										new Point(50, 200)
									};

			t.Graphics.SetClip( new Rectangle(120, 120, 50, 100) );
			t.Graphics.DrawImage(bmp2, p, new Rectangle(100, 100, 100, 100), GraphicsUnit.Pixel);
			t.Show();
			Assert.IsTrue(t.Compare());
		}
		[Test]
#if TARGET_JVM
		[Category("NotWorking")] // defect 6145
#endif
		public void DrawImageWithResolution() {
			t.Graphics.DrawImage(bmp2, 0, 0);
			t.Show();
			Assert.IsTrue(t.Compare());
		}
		[Test]
		public void DrawImageInContainer1() {
			t.Graphics.BeginContainer(new Rectangle(10, 10, 50, 50), new Rectangle(70, 70, 100, 100), GraphicsUnit.Pixel);
			t.Graphics.DrawImage(bmp, 0, 0);
			t.Show();
			Assert.IsTrue(t.Compare());
		}
		[Test]
#if TARGET_JVM
		[Category ("NotWorking")] // defect 6145
#endif
		public void DrawImageInContainer2() {
			t.Graphics.BeginContainer(new Rectangle(10, 10, 50, 50), new Rectangle(70, 70, 100, 100), GraphicsUnit.Pixel);
			t.Graphics.DrawImage(bmp2, 0, 0);
			t.Show();
			Assert.IsTrue(t.Compare());
		}
		[Test]
		public void DrawImageInContainer3() {
			t.Graphics.BeginContainer(new Rectangle(10, 10, 50, 50), new Rectangle(70, 70, 100, 100), GraphicsUnit.Pixel);
			t.Graphics.SetClip( new Rectangle(0, 0, 15, 15) );
			t.Graphics.ScaleTransform(0.5f, 0.5f);
			t.Graphics.DrawImage(bmp2, 0, 0);
			t.Show();
			Assert.IsTrue(t.Compare());
		}
		[Test]
		public void DrawImageInContainer4() {
			Point [] p = new Point[]{
										new Point(100, 100),
										new Point(200, 100),
										new Point(50, 200)
									};

			t.Graphics.SetClip( new Rectangle(70, 70, 70, 70) );
			GraphicsContainer c = t.Graphics.BeginContainer( new Rectangle(20, 20, 10, 10), new Rectangle(77, 77, 7, 7), GraphicsUnit.Pixel);
			t.Graphics.DrawImage(bmp2, p, new Rectangle(100, 100, 100, 100), GraphicsUnit.Pixel);
			t.Graphics.EndContainer( c );
			t.Show();
			Assert.IsTrue(t.Compare());
		}
	}
	#endregion

	#region GraphicsFixtureFillModes
	[TestFixture]
	public class GraphicsFixtureFillModes {
		protected DrawingTest t;
		protected int TOLERANCE = 3; //in %;

		[SetUp]
		public virtual void SetUp() {
			SetUp("GraphicsFixtureFillModes");
		}
		public virtual void SetUp(string ownerClass) {
			t = DrawingTest.Create(512, 512, ownerClass);
		}
		[TearDown]
		public void TearDown() {
			if (t != null)
				t.Dispose ();
		}

		[Test]
		public void FillModeAlternate() {
			GraphicsPath p = new GraphicsPath();
			Assert.AreEqual(FillMode.Alternate, p.FillMode);
		}
		[Test]
		public void FillModeAlternate_1() {
			Point [] p = new Point[] {
										 new Point(50, 100),
										 new Point(70, 10),
										 new Point(90, 100),
										 new Point(140, 10),
										 new Point(150, 100),
										 new Point(170, 10),
										 new Point(50, 100)
									 };

			GraphicsPath path = new GraphicsPath();
			path.AddLines( p );
			path.FillMode = FillMode.Alternate;
			t.Graphics.FillPath( Brushes.Blue, path );
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}
		[Test]
		public void FillModeAlternate_2() {

			Rectangle r1 = new Rectangle(100, 100, 100, 100);
			Rectangle r2 = new Rectangle(125, 125, 50, 50);
			GraphicsPath path = new GraphicsPath();
			path.AddRectangle( r1 );
			path.AddRectangle( r2 );
			path.FillMode = FillMode.Alternate;
			t.Graphics.FillPath( Brushes.Blue, path );
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}
		[Test]
		public void FillModeAlternate_3() {
			Point [] p = new Point[] {
										 new Point(50, 100),
										 new Point(150, 50),
										 new Point(250, 100),
										 new Point(50, 75),
										 new Point(250, 50),
										 new Point(50, 100)
									 };

			GraphicsPath path = new GraphicsPath();
			path.AddLines( p );
			path.FillMode = FillMode.Alternate;
			t.Graphics.FillPath( Brushes.Blue, path );
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}
		[Test]
		public void FillModeWinding_1() {
			Point [] p = new Point[] {
										 new Point(50, 100),
										 new Point(70, 10),
										 new Point(90, 100),
										 new Point(140, 10),
										 new Point(150, 100),
										 new Point(170, 10),
										 new Point(50, 100)
									 };

			GraphicsPath path = new GraphicsPath();
			path.AddLines( p );
			path.FillMode = FillMode.Winding;
			t.Graphics.FillPath( Brushes.Blue, path );
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}
		[Test]
		public void FillModeWinding_2() {

			Rectangle r1 = new Rectangle(100, 100, 100, 100);
			Rectangle r2 = new Rectangle(125, 125, 50, 50);
			GraphicsPath path = new GraphicsPath();
			path.AddRectangle( r1 );
			path.AddRectangle( r2 );
			path.FillMode = FillMode.Winding;
			t.Graphics.FillPath( Brushes.Blue, path );
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}
		[Test]
		public void FillModeWinding_3() {
			Point [] p = new Point[] {
										 new Point(50, 100),
										 new Point(150, 50),
										 new Point(250, 100),
										 new Point(50, 75),
										 new Point(250, 50),
										 new Point(50, 100)
									 };

			GraphicsPath path = new GraphicsPath();
			path.AddLines( p );
			path.FillMode = FillMode.Winding;
			t.Graphics.FillPath( Brushes.Blue, path );
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

	}
	#endregion


	#region GraphicsFixture
	/// <summary>
	/// Summary description for Graphics.
	/// </summary>
	[TestFixture]
	public class GraphicsFixture {
		protected DrawingTest t;
		protected int TOLERANCE = 3; //in %;
		protected Hashtable st = new Hashtable();

		[SetUp]
		public virtual void SetUp() {
			SetUp("GraphicsFixture");
		}
		public virtual void SetUp(string ownerClass) {
			t = DrawingTest.Create(512, 512, ownerClass);

			// hashtable of differents tolerance values for specified tests. (for fft comparer)
			st["DrawArcTest:6"] = TOLERANCE * 2.5f;
			st["DrawCurveTestF:4"] = TOLERANCE * 2f;
			st["DrawPolygonPoint:2"] = TOLERANCE * 2f;
			st["DrawPolygonPointF:2"] = TOLERANCE * 2f;
			st["DrawStringFloatFormat:2"] = TOLERANCE * 2f; // in .net the font is shmoothed
			st["DrawStringFloatFormat:4"] = TOLERANCE * 2.5f; // in .net the font is shmoothed
			st["DrawStringFloatFormat:6"] = TOLERANCE * 2.5f; // in .net the font is shmoothed
			st["RotateTransformAngleMatrixOrder1:2"] = TOLERANCE * 2f; // Line width problem
			st["ScaleTransformFloatMatrixOrder:2"] = TOLERANCE * 2f; // Line width problem
			st["TranslateTransformAngleMatrixOrder:2"] = TOLERANCE * 2f; // Line width problem
			t.SpecialTolerance = st;
		}

		[TearDown]
		public void TearDown ()
		{
			if (t != null)
				t.Dispose ();
		}

		[Test]
		public virtual void BeginContainerTest() {
			// Define transformation for container.
			RectangleF srcRect = new RectangleF(0.0F, 0.0F, 200.0F, 200.0F);
			RectangleF destRect = new RectangleF(100.0F, 100.0F, 150.0F, 150.0F);
			// Begin graphics container.
			GraphicsContainer containerState = t.Graphics.BeginContainer(
				destRect, srcRect,
				GraphicsUnit.Pixel);
			// Fill red rectangle in container.
			t.Graphics.FillRectangle(new SolidBrush(Color.Red), 0.0F, 0.0F, 200.0F, 200.0F);
			t.Show ();
			// End graphics container.
			t.Graphics.EndContainer(containerState);
			// Fill untransformed rectangle with green.
			t.Graphics.FillRectangle(new SolidBrush(Color.Green), 0.0F, 0.0F, 200.0F, 200.0F);
			t.Show ();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public void MeasureString () {
			Bitmap bmp = new Bitmap (400, 300, PixelFormat.Format32bppArgb);
			Graphics graphics = Graphics.FromImage (bmp);
			graphics.PageUnit = GraphicsUnit.Point;

			string drawString = "Sample Text in points";
			Font drawFont = new Font ("Arial Black", 70, FontStyle.Regular);
			SolidBrush drawBrush = new SolidBrush (Color.Blue);

			float netWidth1 = 836.1719f;
			float netWidth2 = 1114.896f;
			float netHeight1 = 98.71094f;
			float netHeight2 = 131.6146f;

			SizeF size = graphics.MeasureString (drawString, drawFont, new PointF (0, 0), StringFormat.GenericTypographic);

			Assert.IsTrue (Math.Abs (size.Width - netWidth1) / netWidth1 < 0.01);
			Assert.IsTrue (Math.Abs (size.Height - netHeight1) / netHeight1 < 0.01);

			graphics.PageUnit = GraphicsUnit.Pixel;
			size = graphics.MeasureString (drawString, drawFont, new PointF (0, 0), StringFormat.GenericTypographic);

			Assert.IsTrue (Math.Abs (size.Width - netWidth2) / netWidth2 < 0.01);
			Assert.IsTrue (Math.Abs (size.Height - netHeight2) / netHeight2 < 0.01);
		}

		[Test]
		public virtual void BeginContainerTest_2() {
			t.Graphics.DrawRectangle( Pens.Black, new Rectangle(70, 70, 50, 100) );
			t.Graphics.DrawRectangle( Pens.Black, new Rectangle(50, 100, 150, 50) );
			t.Graphics.DrawRectangle( Pens.Black, new Rectangle(80, 120, 10, 10) );

			t.Graphics.SetClip( new Rectangle(70, 70, 50, 100) );
			t.Graphics.Clear( Color.Blue );

			GraphicsContainer c1 = t.Graphics.BeginContainer();
			t.Graphics.SetClip( new Rectangle(50, 100, 150, 50) );
			t.Graphics.Clear( Color.Green );

			GraphicsContainer c2 = t.Graphics.BeginContainer();
			t.Graphics.SetClip( new Rectangle(80, 120, 10, 10) );
			t.Graphics.Clear( Color.Red );

			t.Graphics.EndContainer( c2 );
			t.Graphics.FillRectangle( Brushes.Yellow, new Rectangle(100, 120, 10, 10) );
			t.Graphics.FillRectangle( Brushes.Yellow, new Rectangle(150, 120, 10, 10) );

			t.Graphics.EndContainer( c1 );
			t.Graphics.FillRectangle( Brushes.Yellow, new Rectangle(100, 80, 10, 10) );
			t.Graphics.FillRectangle( Brushes.Yellow, new Rectangle(150, 80, 10, 10) );

			t.Show ();
			Assert.IsTrue(t.PDCompare());
		}
		[Test]
		public virtual void ClearTest() {
			// Clear screen with teal background.
			t.Show();
			Assert.IsTrue(t.PDCompare());
			t.Graphics.Clear(Color.Teal);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void DrawArcTest() {
			// Create pen.
			Pen blackPen= new Pen(Color.Black, 1);
			// Create coordinates of rectangle to bound ellipse.
			float x = 10.0F;
			float y = 10.0F;
			float width = 400.0F;
			float height = 100.0F;
			// Create start and sweep angles on ellipse.
			float startAngle =  370.0F;
			float sweepAngle = 70.0F;
			// Draw arc to screen.
			t.Graphics.DrawArc(blackPen, (int)x, (int)y, (int)width, (int)height, (int)startAngle, (int)sweepAngle);
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();
			startAngle =  10.0F;
			sweepAngle = 120.0F;
			t.Graphics.DrawArc(blackPen, new Rectangle((int)x, (int)y, (int)width, (int)height), startAngle, sweepAngle);
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();
			startAngle =  10.0F;
			sweepAngle = 190.0F;
			t.Graphics.DrawArc(blackPen, x, y, width, height, startAngle, sweepAngle);
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();
			startAngle =  10.0F;
			sweepAngle = 300.0F;
			t.Graphics.DrawArc(blackPen, new RectangleF(x, y, width, height), startAngle, sweepAngle);
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();
			startAngle =  -179.9F;
			sweepAngle = -359.9F;
			t.Graphics.DrawArc(blackPen, new RectangleF(x, y, width, height), startAngle, sweepAngle);
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();
			startAngle =  -10.0F;
			sweepAngle = -300.0F;
			t.Graphics.DrawArc(blackPen, new RectangleF(x, y, width, height), startAngle, sweepAngle);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void DrawBezierTest() {
			// Create pen.
			Pen blackPen = new Pen(Color.Black, 3);
			// Create coordinates of points for curve.
			float startX = 100.0F;
			float startY = 100.0F;
			float controlX1 = 200.0F;
			float controlY1 =  10.0F;
			float controlX2 = 350.0F;
			float controlY2 =  50.0F;
			float endX = 500.0F;
			float endY = 100.0F;
			// Draw arc to screen.
			t.Graphics.DrawBezier(blackPen, startX, startY,
				controlX1, controlY1,
				controlX2, controlY2,
				endX, endY);
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();
			t.Graphics.DrawBezier(blackPen, new PointF( startX, startY),
				new PointF(controlX1, controlY1),
				new PointF(controlX2, controlY2),
				new PointF(endX, endY));
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();
			t.Graphics.DrawBezier(blackPen, new Point((int)startX, (int)startY),
				new Point((int)controlX1, (int)controlY1),
				new Point((int)controlX2, (int)controlY2),
				new Point((int)endX, (int)endY));
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void DrawBeziersTest() {
			// Create pen.
			Pen blackPen = new Pen(Color.Black, 3);
			// Create points for curve.
			Point start = new Point(100, 100);
			Point control1 = new Point(200, 10);
			Point control2 = new Point(350, 50);
			Point end1 = new Point(500, 100);
			Point control3 = new Point(600, 150);
			Point control4 = new Point(650, 250);
			Point end2 = new Point(500, 300);
			Point[] bezierPoints = {
									   start, control1, control2, end1,
									   control3, control4, end2
								   };
			// Draw arc to screen.
			t.Graphics.DrawBeziers(blackPen, bezierPoints);
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();

			PointF startF = new PointF(100.0F, 100.0F);
			PointF control1F = new PointF(200.0F, 10.0F);
			PointF control2F = new PointF(350.0F, 50.0F);
			PointF end1F = new PointF(500.0F, 100.0F);
			PointF control3F = new PointF(600.0F, 150.0F);
			PointF control4F = new PointF(650.0F, 250.0F);
			PointF end2F = new PointF(500.0F, 300.0F);
			PointF[] bezierPointsF = {
										 startF, control1F, control2F, end1F,
										 control3F, control4F, end2F
									 };
			// Draw arc to screen.
			t.Graphics.DrawBeziers(blackPen, bezierPointsF);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void DrawClosedCurveTest() {
			// Create pens.
			Pen redPen   = new Pen(Color.Red, 3);
			Pen greenPen = new Pen(Color.Green, 3);
			// Create points that define curve.
			PointF point1 = new PointF( 50.0F,  50.0F);
			PointF point2 = new PointF(100.0F,  25.0F);
			PointF point3 = new PointF(200.0F,   5.0F);
			PointF point4 = new PointF(250.0F,  50.0F);
			PointF point5 = new PointF(300.0F, 100.0F);
			PointF point6 = new PointF(350.0F, 200.0F);
			PointF point7 = new PointF(250.0F, 250.0F);
			PointF[] curvePoints = {
									   point1,
									   point2,
									   point3,
									   point4,
									   point5,
									   point6,
									   point7
								   };
			// Draw lines between original points to screen.
			t.Graphics.DrawLines(redPen, curvePoints);
			// Create tension and fill mode.
			float tension = 0.7F;
			FillMode aFillMode = FillMode.Alternate;
			// Draw closed curve to screen.
			t.Graphics.DrawClosedCurve(greenPen, curvePoints, tension, aFillMode);
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();

			aFillMode = FillMode.Winding;
			// Draw closed curve to screen.
			t.Graphics.DrawClosedCurve(greenPen, curvePoints, tension, aFillMode);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void DrawCurveTest() {
			// Create pens.
			Pen redPen   = new Pen(Color.Red, 3);
			Pen greenPen = new Pen(Color.Green, 3);
			// Create points that define curve.
			Point point1 = new Point( 50,  50);
			Point point2 = new Point(100,  25);
			Point point3 = new Point(200,   5);
			Point point4 = new Point(250,  50);
			Point point5 = new Point(300, 100);
			Point point6 = new Point(350, 200);
			Point point7 = new Point(250, 250);
			Point[] curvePoints = {
									  point1,
									  point2,
									  point3,
									  point4,
									  point5,
									  point6,
									  point7
								  };
			// Draw lines between original points to screen.
			t.Graphics.DrawLines(redPen, curvePoints);
			// Create offset, number of segments, and tension.
			int offset = 2;
			int numSegments = 4;
			float tension = 0.7F;
			// Draw curve to screen.
			t.Graphics.DrawCurve(greenPen, curvePoints, offset, numSegments, tension);
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();

			t.Graphics.DrawCurve(greenPen, curvePoints, tension);
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();

			t.Graphics.DrawCurve(greenPen, curvePoints);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void DrawCurveTestF() {
			// Create pens.
			Pen redPen   = new Pen(Color.Red, 3);
			Pen greenPen = new Pen(Color.Green, 3);
			// Create points that define curve.
			PointF point1 = new PointF( 50.0F,  50.0F);
			PointF point2 = new PointF(100.0F,  25.0F);
			PointF point3 = new PointF(200.0F,   5.0F);
			PointF point4 = new PointF(250.0F,  50.0F);
			PointF point5 = new PointF(300.0F, 100.0F);
			PointF point6 = new PointF(350.0F, 200.0F);
			PointF point7 = new PointF(250.0F, 250.0F);
			PointF[] curvePoints = {
									   point1,
									   point2,
									   point3,
									   point4,
									   point5,
									   point6,
									   point7
								   };
			// Draw lines between original points to screen.
			t.Graphics.DrawLines(redPen, curvePoints);
			// Create offset, number of segments, and tension.
			int offset = 2;
			int numSegments = 4;
			float tension = 0.7F;
			// Draw curve to screen.
			t.Graphics.DrawCurve(greenPen, curvePoints, offset, numSegments, tension);
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();

			t.Graphics.DrawCurve(greenPen, curvePoints, offset, numSegments);
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();

			t.Graphics.DrawCurve(greenPen, curvePoints, tension);
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();

			t.Graphics.DrawCurve(greenPen, curvePoints);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void DrawEllipseTest() {
			// Create pen.
			Pen blackPen = new Pen(Color.Black, 3);
			// Create location and size of ellipse.
			int x = 0;
			int y = 0;
			int width = 200;
			int height = 100;
			// Draw ellipse to screen.
			t.Graphics.DrawEllipse(blackPen, x, y, width, height);
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();

			t.Graphics.DrawEllipse(blackPen, new Rectangle(x, y, width, height));
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void DrawEllipseTestF() {
			// Create pen.
			Pen blackPen = new Pen(Color.Black, 3);
			// Create location and size of ellipse.
			float x = 0.0F;
			float y = 0.0F;
			float width = 200.0F;
			float height = 100.0F;
			// Draw ellipse to screen.
			t.Graphics.DrawEllipse(blackPen, x, y, width, height);
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();

			t.Graphics.DrawEllipse(blackPen, new RectangleF(x, y, width, height));
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		static string getInFile (string file) {
			string sRslt;						
			
			sRslt = Path.GetFullPath (file);
			
			if (! File.Exists (file))
				sRslt = Path.Combine (
					Path.Combine ("..", ".."),
					file);

			return sRslt;
		}

		[Test]
		[Category("NotWorking")]
		public virtual void DrawIconTest() {
			// Create icon.
			Icon newIcon = new Icon(getInFile ("SampIcon.ico"));
			// Create coordinates for upper-left corner of icon.
			int x = 100;
			int y = 100;
			// Draw icon to screen.
			t.Graphics.DrawIcon(newIcon, x, y);
			t.Show();
			Assert.IsTrue(t.PDCompare());

			t.Graphics.DrawIcon(newIcon, new Rectangle(200, 300, 125, 345));
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		[Category("NotWorking")]
		public virtual void DrawIconUnstretchedTest() {
			// Create icon.
			Icon newIcon = new Icon(getInFile ("SampIcon.ico"));
			// Create rectangle for icon.
			Rectangle rect = new Rectangle( 100, 100, 200, 200);
			// Draw icon to screen.
			t.Graphics.DrawIconUnstretched(newIcon, rect);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}
#if INTPTR_SUPPORTED
		// Define DrawImageAbort callback method.
		private bool DrawImageCallback(IntPtr callBackData) {
			// Test for call that passes callBackData parameter.
			if(callBackData==IntPtr.Zero) {
				// If no callBackData passed, abort DrawImage method.
				return true;
			}
			else {
				// If callBackData passed, continue DrawImage method.
				return false;
			}
		}
		
		[Test] //TBD: add more overrides
		public void DrawImageTest() {
			// Create callback method.
			Graphics.DrawImageAbort imageCallback
				= new Graphics.DrawImageAbort(DrawImageCallback);
			IntPtr imageCallbackData = new IntPtr(1);
			// Create image.
			Image newImage = Image.FromFile("SampIcon.ico");
			// Create rectangle for displaying original image.
			Rectangle destRect1 = new Rectangle( 100, 25, 450, 150);
			// Create coordinates of rectangle for source image.
			float x = 50.0F;
			float y = 50.0F;
			float width = 150.0F;
			float height = 150.0F;
			GraphicsUnit units = GraphicsUnit.Pixel;
			// Draw original image to screen.
			t.Graphics.DrawImage(newImage, destRect1, x, y, width, height, units);
			t.Show();
			// Create rectangle for adjusted image.
			Rectangle destRect2 = new Rectangle(100, 175, 450, 150);
			// Create image attributes and set large gamma.
			ImageAttributes imageAttr = new ImageAttributes();
			imageAttr.SetGamma(4.0F);
			// Draw adjusted image to screen.

			t.Graphics.DrawImage(
				newImage,
				destRect2,
				x, y,
				width, height,
				units,
				imageAttr,
				imageCallback,
				imageCallbackData);

			t.Show();
		}
#endif
		[Test]
		[Category("NotWorking")]
		public virtual void DrawImageUnscaledTest() {
			// Create image.
			Image newImage = Bitmap.FromFile(getInFile ("bitmap_gh.png"));
			// Create coordinates for upper-left corner of image.
			int x = 100;
			int y = 100;
			// Draw image to screen.
			t.Graphics.DrawImageUnscaled(newImage, x, y, 100, 125);
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();

			t.Graphics.DrawImageUnscaled(newImage, new Rectangle(x, y, 34, 235));
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();

			t.Graphics.DrawImageUnscaled(newImage, x, y);
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();

			t.Graphics.DrawImageUnscaled(newImage, new Point(x, y));
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void DrawLineTest() {
			// Create pen.
			Pen blackPen = new Pen(Color.Black, 3);
			// Create coordinates of points that define line.
			int x1 = 100;
			int y1 = 100;
			int x2 = 500;
			int y2 = 100;
			// Draw line to screen.
			t.Graphics.DrawLine(blackPen, x1, y1, x2, y2);
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();

			t.Graphics.DrawLine(blackPen, new Point( x1, y1), new Point( x2, y2));
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void DrawLineTestF() {
			// Create pen.
			Pen blackPen = new Pen(Color.Black, 3);
			// Create coordinates of points that define line.
			float x1 = 100.0F;
			float y1 = 100.0F;
			float x2 = 500.0F;
			float y2 = 100.0F;
			// Draw line to screen.
			t.Graphics.DrawLine(blackPen, x1, y1, x2, y2);
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();

			t.Graphics.DrawLine(blackPen, new PointF( x1, y1), new PointF( x2, y2));
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void DrawLinesTest() {
			// Create pen.
			Pen pen = new Pen(Color.Black, 3);
			// Create array of points that define lines to draw.
			Point[] points = {
								 new Point( 10,  10),
								 new Point( 10, 100),
								 new Point(200,  50),
								 new Point(250, 300)
							 };
			//Draw lines to screen.
			t.Graphics.DrawLines(pen, points);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void DrawLinesTestF() {
			// Create pen.
			Pen pen = new Pen(Color.Black, 3);
			// Create array of points that define lines to draw.
			PointF[] points = {
								  new PointF( 10.0F,  10.0F),
								  new PointF( 10.0F, 100.0F),
								  new PointF(200.0F,  50.0F),
								  new PointF(250.0F, 300.0F)
							  };
			//Draw lines to screen.
			t.Graphics.DrawLines(pen, points);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void DrawPathTest() {
			// Create graphics path object and add ellipse.
			GraphicsPath graphPath = new GraphicsPath();
			graphPath.AddEllipse(0, 0, 200, 100);
			// Create pen.
			Pen blackPen = new Pen(Color.Black, 3);
			// Draw graphics path to screen.
			t.Graphics.DrawPath(blackPen, graphPath);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void DrawPieTestF() {
			// Create pen.
			Pen blackPen = new Pen(Color.Black, 3);
			// Create location and size of ellipse.
			float x = 0.0F;
			float y = 0.0F;
			float width = 200.0F;
			float height = 100.0F;
			// Create start and sweep angles.
			float startAngle =  0.0F;
			float sweepAngle = 45.0F;
			// Draw pie to screen.
			t.Graphics.DrawPie(blackPen, x, y, width, height, startAngle, sweepAngle);
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();

			t.Graphics.DrawPie(blackPen, new RectangleF( x, y, width, height), startAngle, sweepAngle);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void DrawPieTest() {
			// Create pen.
			Pen blackPen = new Pen(Color.Black, 3);
			// Create location and size of ellipse.
			int x = 0;
			int y = 0;
			int width = 200;
			int height = 100;
			// Create start and sweep angles.
			int startAngle =  0;
			int sweepAngle = 45;
			// Draw pie to screen.
			t.Graphics.DrawPie(blackPen, x, y, width, height, startAngle, sweepAngle);
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();

			t.Graphics.DrawPie(blackPen, new Rectangle( x, y, width, height), startAngle, sweepAngle);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void DrawPolygonPoint() {
			// Create pen.
			Pen blackPen = new Pen(Color.Black, 3);
			// Create points that define polygon.
			Point point1 = new Point( 50,  50);
			Point point2 = new Point(100,  25);
			Point point3 = new Point(200,   5);
			Point point4 = new Point(250,  50);
			Point point5 = new Point(300, 100);
			Point point6 = new Point(350, 200);
			Point point7 = new Point(250, 250);
			Point[] curvePoints = {
									  point1,
									  point2,
									  point3,
									  point4,
									  point5,
									  point6,
									  point7
								  };
			// Draw polygon to screen.
			t.Graphics.DrawPolygon(blackPen, curvePoints);
			t.Show();
			Assert.IsTrue(t.PDCompare()); // .NET's lines of polygon is more wide
		}

		[Test]
		public virtual void DrawPolygonPointF() {
			// Create pen.
			Pen blackPen = new Pen(Color.Black, 3);
			// Create points that define polygon.
			PointF point1 = new PointF( 50,  50);
			PointF point2 = new PointF(100,  25);
			PointF point3 = new PointF(200,   5);
			PointF point4 = new PointF(250,  50);
			PointF point5 = new PointF(300, 100);
			PointF point6 = new PointF(350, 200);
			PointF point7 = new PointF(250, 250);
			PointF[] curvePoints = {
									   point1,
									   point2,
									   point3,
									   point4,
									   point5,
									   point6,
									   point7
								   };
			// Draw polygon to screen.
			t.Graphics.DrawPolygon(blackPen, curvePoints);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void DrawRectangleFloat() {
			// Create pen.
			Pen blackPen = new Pen(Color.Black, 3);
			// Create location and size of rectangle.
			float x = 7.0F;
			float y = 7.0F;
			float width = 200.0F;
			float height = 200.0F;
			// Draw rectangle to screen.
			t.Graphics.DrawRectangle(blackPen, x, y, width, height);
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();

			t.Graphics.DrawRectangle(blackPen, (int)x, (int)y, (int)width, (int)height);
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();

			t.Graphics.DrawRectangle(blackPen, new Rectangle( (int)x, (int)y, (int)width, (int)height));
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void DrawRectanglesRectangleF() {
			// Create pen.
			Pen blackPen = new Pen(Color.Black, 3);
			// Create array of rectangles.
			RectangleF[] rects = {
									 new RectangleF(  20.0F,   20.0F, 100.0F, 200.0F),
									 new RectangleF(100.0F, 200.0F, 250.0F,  50.0F),
									 new RectangleF(300.0F,   20.0F,  50.0F, 100.0F)
								 };
			// Draw rectangles to screen.
			t.Graphics.DrawRectangles(blackPen, rects);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void DrawRectanglesRectangle() {
			// Create pen.
			Pen blackPen = new Pen(Color.Black, 3);
			// Create array of rectangles.
			Rectangle[] rects = {
									new Rectangle(  20,   20, 100, 200),
									new Rectangle(100, 200, 250,  50),
									new Rectangle(300,   20,  50, 100)
								};
			// Draw rectangles to screen.
			t.Graphics.DrawRectangles(blackPen, rects);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test] //TBD: add more combinations
		[Category("NotWorking")]
		public virtual void DrawStringFloatFormat() {
			// Create string to draw.
			String drawString = "Sample Text";
			// Create font and brush.
			Font drawFont = new Font("Arial", 34, FontStyle.Italic);
			SolidBrush drawBrush = new SolidBrush(Color.Black);
			// Create point for upper-left corner of drawing.
			float x = 150.0F;
			float y =  50.0F;
			// Set format of string.
			StringFormat drawFormat = new StringFormat();
			drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
			// Draw string to screen.
			t.Graphics.DrawString(drawString, drawFont, drawBrush, x, y, drawFormat);
			t.Show();
			Assert.IsTrue(t.PDCompare()); // in .net the font is shmoothed
			SetUp();

			drawFormat.FormatFlags = StringFormatFlags.NoClip;
			// Draw string to screen.
			t.Graphics.DrawString(drawString, drawFont, drawBrush, x, y, drawFormat);
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();

			drawFormat.FormatFlags = StringFormatFlags.FitBlackBox;
			// Draw string to screen.
			t.Graphics.DrawString(drawString, drawFont, drawBrush, x, y, drawFormat);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void EndContainerState() {
			// Begin graphics container.
			GraphicsContainer containerState = t.Graphics.BeginContainer();
			// Translate world transformation.
			t.Graphics.TranslateTransform(100.0F, 100.0F);
			// Fill translated rectangle in container with red.
			t.Graphics.FillRectangle(new SolidBrush(Color.Red), 0, 0, 200, 200);
			t.Show();
			// End graphics container.
			t.Graphics.EndContainer(containerState);
			// Fill untransformed rectangle with green.
			t.Graphics.FillRectangle(new SolidBrush(Color.Green), 0, 0, 200, 200);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test] //TBD
		public virtual void EnumerateMetafile() {
		}

		[Test]
		public virtual void ExcludeClipRegion() {
			// Create rectangle for exclusion.
			Rectangle excludeRect = new Rectangle(100, 100, 200, 200);
			// Set clipping region to exclude rectangle.
			t.Graphics.ExcludeClip(excludeRect);
			// Fill large rectangle to show clipping region.
			t.Graphics.FillRectangle(new SolidBrush(Color.Blue), 0, 0, 300, 300);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void FillClosedCurvePointFillModeTension() {
			// Create solid brush.
			SolidBrush redBrush = new SolidBrush(Color.Red);
			// Create array of points for curve.
			Point point1 = new Point(100, 100);
			Point point2 = new Point(200,  50);
			Point point3 = new Point(250, 200);
			Point point4 = new Point( 50, 150);
			Point[] points = {point1, point2, point3, point4};
			// Set fill mode.
			FillMode newFillMode = FillMode.Winding;
			// Set tension.
			float tension = 0.68F;
			// Fill curve on screen.
			t.Graphics.FillClosedCurve(redBrush, points);
			t.Show();
			Assert.IsTrue(t.PDCompare());

			SetUp();
			t.Graphics.FillClosedCurve(redBrush, points, newFillMode);
			t.Show();
			Assert.IsTrue(t.PDCompare());

			SetUp();
			newFillMode = FillMode.Alternate;
			t.Graphics.FillClosedCurve(redBrush, points, newFillMode, tension);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void FillClosedCurvePointFFillModeTension() {
			// Create solid brush.
			SolidBrush redBrush = new SolidBrush(Color.Red);
			// Create array of points for curve.
			PointF point1 = new PointF(100.0F, 100.0F);
			PointF point2 = new PointF(200.0F,  50.0F);
			PointF point3 = new PointF(250.0F, 200.0F);
			PointF point4 = new PointF( 50.0F, 150.0F);
			PointF[] points = {point1, point2, point3, point4};
			// Set fill mode.
			FillMode newFillMode = FillMode.Winding;
			// Set tension.
			float tension = 0.68F;
			// Fill curve on screen.
			t.Graphics.FillClosedCurve(redBrush, points);
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();

			t.Graphics.FillClosedCurve(redBrush, points, newFillMode);
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();

			newFillMode = FillMode.Alternate;
			t.Graphics.FillClosedCurve(redBrush, points, newFillMode, tension);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void FillEllipse() {
			// Create solid brush.
			SolidBrush redBrush = new SolidBrush(Color.Red);
			// Create location and size of ellipse.
			int x = 0;
			int y = 0;
			int width = 200;
			int height = 100;
			// Fill ellipse on screen.
			t.Graphics.FillEllipse(redBrush, x, y, width, height);
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();

			t.Graphics.FillEllipse(redBrush, new Rectangle( x, y, width, height));
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void FillEllipseFloat() {
			// Create solid brush.
			SolidBrush redBrush = new SolidBrush(Color.Red);
			// Create location and size of ellipse.
			float x = 0.0F;
			float y = 0.0F;
			float width = 200.0F;
			float height = 100.0F;
			// Fill ellipse on screen.
			t.Graphics.FillEllipse(redBrush, x, y, width, height);
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();

			t.Graphics.FillEllipse(redBrush, new RectangleF( x, y, width, height));
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void FillPathEllipse() {
			// Create solid brush.
			SolidBrush redBrush = new SolidBrush(Color.Red);
			// Create graphics path object and add ellipse.
			GraphicsPath graphPath = new GraphicsPath();
			graphPath.AddEllipse(0, 0, 200, 100);
			// Fill graphics path to screen.
			t.Graphics.FillPath(redBrush, graphPath);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void FillPieFloat() {
			// Create solid brush.
			SolidBrush redBrush = new SolidBrush(Color.Red);
			// Create location and size of ellipse.
			int x = 0;
			int y = 0;
			int width = 200;
			int height = 100;
			// Create start and sweep angles.
			float startAngle =  0.0F;
			float sweepAngle = 45.0F;
			// Fill pie to screen.
			t.Graphics.FillPie(redBrush, new Rectangle(x, y, width, height), startAngle, sweepAngle);
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();

			t.Graphics.FillPie(redBrush, x, y, width, height, (int)startAngle, (int)sweepAngle);
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();

			t.Graphics.FillPie(redBrush, (float)x, (float)y, (float)width, (float)height, startAngle, sweepAngle);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void FillPolygonPointFillMode() {
			// Create solid brush.
			SolidBrush blueBrush = new SolidBrush(Color.Blue);
			// Create points that define polygon.
			Point point1 = new Point( 50,  50);
			Point point2 = new Point(100,  25);
			Point point3 = new Point(200,   5);
			Point point4 = new Point(250,  50);
			Point point5 = new Point(300, 100);
			Point point6 = new Point(350, 200);
			Point point7 = new Point(250, 250);
			Point[] curvePoints = {
									  point1,
									  point2,
									  point3,
									  point4,
									  point5,
									  point6,
									  point7
								  };

			// Fill polygon to screen.
			t.Graphics.FillPolygon(blueBrush, curvePoints, FillMode.Winding);
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();

			t.Graphics.FillPolygon(blueBrush, curvePoints, FillMode.Alternate);
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();

			t.Graphics.FillPolygon(blueBrush, curvePoints);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void FillPolygonPointFFillMode() {
			// Create solid brush.
			SolidBrush blueBrush = new SolidBrush(Color.Blue);
			// Create points that define polygon.
			PointF point1 = new PointF( 50.0F,  50.0F);
			PointF point2 = new PointF(100.0F,  25.0F);
			PointF point3 = new PointF(200.0F,   5.0F);
			PointF point4 = new PointF(250.0F,  50.0F);
			PointF point5 = new PointF(300.0F, 100.0F);
			PointF point6 = new PointF(350.0F, 200.0F);
			PointF point7 = new PointF(250.0F, 250.0F);
			PointF[] curvePoints = {
									   point1,
									   point2,
									   point3,
									   point4,
									   point5,
									   point6,
									   point7
								   };

			// Fill polygon to screen.
			t.Graphics.FillPolygon(blueBrush, curvePoints, FillMode.Winding);
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();

			t.Graphics.FillPolygon(blueBrush, curvePoints, FillMode.Alternate);
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();

			t.Graphics.FillPolygon(blueBrush, curvePoints);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void FillRectangle() {
			// Create solid brush.
			SolidBrush blueBrush = new SolidBrush(Color.Blue);
			// Create location and size of rectangle.
			int x = 0;
			int y = 0;
			int width = 300;
			int height = 200;
			// Fill rectangle to screen.
			t.Graphics.FillRectangle(blueBrush, x, y, width, height);
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();

			t.Graphics.FillRectangle(blueBrush, new Rectangle( x, y, width, height));
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void FillRectangleFloat() {
			// Create solid brush.
			SolidBrush blueBrush = new SolidBrush(Color.Blue);
			// Create location and size of rectangle.
			float x = 0.0F;
			float y = 0.0F;
			float width = 300.0F;
			float height = 200.0F;
			// Fill rectangle to screen.
			t.Graphics.FillRectangle(blueBrush, x, y, width, height);
			t.Show();
			Assert.IsTrue(t.PDCompare());
			SetUp();

			t.Graphics.FillRectangle(blueBrush, new RectangleF( x, y, width, height));
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void FillRectanglesRectangle() {
			// Create solid brush.
			SolidBrush blueBrush = new SolidBrush(Color.Blue);
			// Create array of rectangles.
			Rectangle[] rects = {
									new Rectangle(  0,   0, 100, 200),
									new Rectangle(100, 200, 250,  50),
									new Rectangle(300,   0,  50, 100)
								};
			// Fill rectangles to screen.
			t.Graphics.FillRectangles(blueBrush, rects);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void FillRectanglesRectangleF() {
			// Create solid brush.
			SolidBrush blueBrush = new SolidBrush(Color.Blue);
			// Create array of rectangles.
			RectangleF[] rects = {
									 new RectangleF(  0.0F,   0.0F, 100.0F, 200.0F),
									 new RectangleF(100.0F, 200.0F, 250.0F,  50.0F),
									 new RectangleF(300.0F,   0.0F,  50.0F, 100.0F)
								 };
			// Fill rectangles to screen.
			t.Graphics.FillRectangles(blueBrush, rects);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void FillRegionRectangle() {
			// Create solid brush.
			SolidBrush blueBrush = new SolidBrush(Color.Blue);
			// Create rectangle for region.
			Rectangle fillRect = new Rectangle(100, 150, 200, 250);
			// Create region for fill.
			Region fillRegion = new Region(fillRect);
			// Fill region to screen.
			t.Graphics.FillRegion(blueBrush, fillRegion);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void FlushTest() {
			t.Graphics.Flush();
			t.Graphics.Flush(FlushIntention.Flush);
		}

		[Test]
		public virtual void IntersectClipRegion() {
			// Set clipping region.
			Rectangle clipRect = new Rectangle(0, 0, 200, 300);
			Region clipRegion = new Region(clipRect);
			t.Graphics.SetClip(clipRegion, CombineMode.Replace);
			// Update clipping region to intersection of
			//  existing region with specified rectangle.
			Rectangle intersectRect = new Rectangle(100, 100, 200, 300);
			Region intersectRegion = new Region(intersectRect);
			t.Graphics.IntersectClip(intersectRegion);
			// Fill rectangle to demonstrate effective clipping region.
			t.Graphics.FillRectangle(new SolidBrush(Color.Blue), 0, 0, 500, 600);
			t.Show();
			// Reset clipping region to infinite.
			t.Graphics.ResetClip();
			// Draw clipRect and intersectRect to screen.
			t.Graphics.DrawRectangle(new Pen(Color.Black), clipRect);
			t.Show();
			t.Graphics.DrawRectangle(new Pen(Color.Red), intersectRect);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void IsVisible4Float() {
			// Set clip region.
			Region clipRegion = new Region(new Rectangle(50, 50, 100, 100));
			t.Graphics.SetClip(clipRegion, CombineMode.Replace);
			// Set up coordinates of rectangles.
			float x1 =  100.0F;
			float y1 =  100.0F;
			float width1 = 20.0F;
			float height1 = 20.0F;
			float x2 = 200.0F;
			float y2 = 200.0F;
			float width2 = 20.0F;
			float height2 = 20.0F;
			// If rectangle is visible, fill it.
			if (t.Graphics.IsVisible(x1, y1, width1, height1)) {
				t.Graphics.FillRectangle(new SolidBrush(Color.Red), x1, y1, width1, height1);
				t.Show();
			}
			if (t.Graphics.IsVisible(x2, y2, width2, height2)) {
				t.Graphics.FillRectangle(new SolidBrush(Color.Blue), x2, y2, width2, height2);
				t.Show();
			}

			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		[Category("NotWorking")]
		public virtual void MeasureCharacterRangesRegions() {
			// Set up string.
			string measureString = "First and Second ranges";
			Font stringFont = new Font("Times New Roman", 16.0F);
			// Set character ranges to "First" and "Second".
			CharacterRange[] characterRanges = {
												   new CharacterRange(0, 5),
												   new CharacterRange(10, 6)
											   };
			// Create rectangle for layout.
			float x = 50.0F;
			float y = 50.0F;
			float width = 35.0F;
			float height = 200.0F;
			RectangleF layoutRect = new RectangleF(x, y, width, height);
			// Set string format.
			StringFormat stringFormat = new StringFormat();
			stringFormat.FormatFlags = StringFormatFlags.DirectionVertical;
			stringFormat.SetMeasurableCharacterRanges(characterRanges);
			// Draw string to screen.
			t.Graphics.DrawString(
				measureString,
				stringFont,
				Brushes.Black,
				x, y,
				stringFormat);
			// Measure two ranges in string.
			Region[] stringRegions = new Region[2];
			stringRegions = t.Graphics.MeasureCharacterRanges(
				measureString,
				stringFont,
				layoutRect,
				stringFormat);
			// Draw rectangle for first measured range.
			RectangleF measureRect1 = stringRegions[0].GetBounds(t.Graphics);
			t.Graphics.DrawRectangle(
				new Pen(Color.Red, 1),
				Rectangle.Round(measureRect1));
			t.Show();
			// Draw rectangle for second measured range.
			RectangleF measureRect2 = stringRegions[1].GetBounds(t.Graphics);
			t.Graphics.DrawRectangle(
				new Pen(Color.Blue, 1),
				Rectangle.Round(measureRect2));
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test] //TBD: add more overloads
		[Category("NotWorking")]
		public virtual void MeasureStringSizeFFormatInts() {
			// Set up string.
			string measureString = "Measure String";
			Font stringFont = new Font("Arial", 16);
			// Set maximum layout size.
			SizeF layoutSize = new SizeF(100.0F, 200.0F);
			// Set string format.
			StringFormat newStringFormat = new StringFormat();
			newStringFormat.FormatFlags = StringFormatFlags.DirectionVertical;
			// Measure string.
			int charactersFitted;
			int linesFilled;
			SizeF stringSize = new SizeF();
			stringSize = t.Graphics.MeasureString(
				measureString,
				stringFont,
				layoutSize,
				newStringFormat,
				out charactersFitted,
				out linesFilled);
			// Draw rectangle representing size of string.
			t.Graphics.DrawRectangle(
				new Pen(Color.Red, 1),
				0.0F, 0.0F, stringSize.Width, stringSize.Height);
			t.Show();
			// Draw string to screen.
			t.Graphics.DrawString(
				measureString,
				stringFont,
				Brushes.Black,
				new PointF(0, 0),
				newStringFormat);
			t.Show();
			// Draw output parameters to screen.
			string outString = "chars " + charactersFitted + ", lines " + linesFilled;
			t.Graphics.DrawString(
				outString,
				stringFont,
				Brushes.Black,
				new PointF(100, 0));
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void MultiplyTransform() {
			// Create transform matrix.
			Matrix transformMatrix = new Matrix();
			// Translate matrix, prepending translation vector.
			transformMatrix.Translate(200.0F, 100.0F);
			// Rotate transformation matrix of graphics object,
			//  prepending rotation matrix.
			t.Graphics.RotateTransform(30.0F);
			// Multiply (append to) transformation matrix of
			//  graphics object to translate graphics transformation.
			t.Graphics.MultiplyTransform(transformMatrix);
			// Draw rotated, translated ellipse.
			t.Graphics.DrawEllipse(new Pen(Color.Blue, 3), -80, -40, 160, 80);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void MultiplyTransformMatrixOrder() {
			// Create transform matrix.
			Matrix transformMatrix = new Matrix();
			// Translate matrix, prepending translation vector.
			transformMatrix.Translate(200.0F, 100.0F);
			// Rotate transformation matrix of graphics object,
			//  prepending rotation matrix.
			t.Graphics.RotateTransform(30.0F);
			// Multiply (append to) transformation matrix of
			//  graphics object to translate graphics transformation.
			t.Graphics.MultiplyTransform(transformMatrix, MatrixOrder.Append);
			// Draw rotated, translated ellipse.
			t.Graphics.DrawEllipse(new Pen(Color.Blue, 3), -80, -40, 160, 80);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void MultiplyTransformMatrixOrder1() {
			// Create transform matrix.
			Matrix transformMatrix = new Matrix();
			// Translate matrix, prepending translation vector.
			transformMatrix.Translate(200.0F, 100.0F);
			// Rotate transformation matrix of graphics object,
			//  prepending rotation matrix.
			t.Graphics.RotateTransform(30.0F);
			// Multiply (append to) transformation matrix of
			//  graphics object to translate graphics transformation.
			t.Graphics.MultiplyTransform(transformMatrix, MatrixOrder.Prepend);
			// Draw rotated, translated ellipse.
			t.Graphics.DrawEllipse(new Pen(Color.Blue, 3), -80, -40, 160, 80);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void ResetClipIntersectClipRectangleF() {
			// Set clipping region.
			Rectangle clipRect = new Rectangle(0, 0, 200, 200);
			t.Graphics.SetClip(clipRect);
			// Update clipping region to intersection of existing region with new rectangle.
			RectangleF intersectRectF = new RectangleF(100.0F, 100.0F, 200.0F, 200.0F);
			t.Graphics.IntersectClip(intersectRectF);
			// Fill rectangle to demonstrate effective clipping region.
			t.Graphics.FillRectangle(new SolidBrush(Color.Blue), 0, 0, 500, 500);
			// Reset clipping region to infinite.
			t.Graphics.ResetClip();
			// Draw clipRect and intersectRect to screen.
			t.Graphics.DrawRectangle(new Pen(Color.Black), clipRect);
			t.Graphics.DrawRectangle(new Pen(Color.Red), Rectangle.Round(intersectRectF));
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void SaveRestoreTranslate() {
			// Translate transformation matrix.
			t.Graphics.TranslateTransform(100, 0);
			// Save translated graphics state.
			GraphicsState transState = t.Graphics.Save();
			// Reset transformation matrix to identity and fill rectangle.
			t.Graphics.ResetTransform();
			t.Graphics.FillRectangle(new SolidBrush(Color.Red), 0, 0, 100, 100);
			t.Show();
			// Restore graphics state to translated state and fill second rectangle.
			t.Graphics.Restore(transState);
			t.Graphics.FillRectangle(new SolidBrush(Color.Blue), 0, 0, 100, 100);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void RotateTransformAngleMatrixOrder() {
			// Set world transform of graphics object to translate.
			t.Graphics.TranslateTransform(100.0F, 0.0F);
			// Then to rotate, appending rotation matrix.
			t.Graphics.RotateTransform(30.0F, MatrixOrder.Append);
			// Draw translated, rotated ellipse to screen.
			t.Graphics.DrawEllipse(new Pen(Color.Blue, 3), 0, 0, 200, 80);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void RotateTransformAngleMatrixOrder1() {
			// Set world transform of graphics object to translate.
			t.Graphics.TranslateTransform(100.0F, 0.0F);
			// Then to rotate, appending rotation matrix.
			t.Graphics.RotateTransform(30.0F, MatrixOrder.Prepend);
			// Draw translated, rotated ellipse to screen.
			t.Graphics.DrawEllipse(new Pen(Color.Blue, 3), 0, 0, 200, 80);
			t.Show();
			Assert.IsTrue(t.PDCompare());  // Line width problem
		}

		[Test]
		public virtual void ScaleTransformFloatMatrixOrder() {
			// Set world transform of graphics object to rotate.
			t.Graphics.RotateTransform(30.0F);
			// Then to scale, appending to world transform.
			t.Graphics.ScaleTransform(3.0F, 1.0F, MatrixOrder.Append);
			// Draw rotated, scaled rectangle to screen.
			t.Graphics.DrawRectangle(new Pen(Color.Blue, 3), 50, 0, 100, 40);
			t.Show();
			Assert.IsTrue(t.PDCompare()); // Line width problem
		}

		[Test]
		public virtual void ScaleTransformFloatMatrixOrder1() {
			// Set world transform of graphics object to rotate.
			t.Graphics.RotateTransform(30.0F);
			// Then to scale, appending to world transform.
			t.Graphics.ScaleTransform(3.0F, 1.0F, MatrixOrder.Prepend);
			// Draw rotated, scaled rectangle to screen.
			t.Graphics.DrawRectangle(new Pen(Color.Blue, 3), 50, 0, 100, 40);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test] //TBD: add more combination
		public virtual void SetClipRegionCombine() {
			// Create region for clipping.
			Region clipRegion = new Region(new Rectangle(0, 0, 200, 100));
			// Set clipping region of graphics to region.
			t.Graphics.SetClip(clipRegion, CombineMode.Replace);
			// Fill rectangle to demonstrate clip region.
			t.Graphics.FillRectangle(new SolidBrush(Color.Black), 0, 0, 500, 300);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void TransformPointsPointF() {
			// Create array of two points.
			PointF[] points = {new PointF(0.0F, 0.0F),
								  new PointF(100.0F, 50.0F)};
			// Draw line connecting two untransformed points.
			t.Graphics.DrawLine(new Pen(Color.Blue, 3),
				points[0],
				points[1]);
			// Set world transformation of Graphics object to translate.
			t.Graphics.TranslateTransform(40.0F, 30.0F);
			// Transform points in array from world to page coordinates.
			t.Graphics.TransformPoints(CoordinateSpace.Page,
				CoordinateSpace.World,
				points);
			// Reset world transformation.
			t.Graphics.ResetTransform();
			// Draw line that connects transformed points.
			t.Graphics.DrawLine(new Pen(Color.Red, 3),
				points[0],
				points[1]);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void TranslateClipFloat() {
			// Create rectangle for clipping region.
			RectangleF clipRect = new RectangleF(0.0F, 0.0F, 100.0F, 100.0F);
			// Set clipping region of graphics to rectangle.
			t.Graphics.SetClip(clipRect);
			// Translate clipping region.
			float dx = 50.0F;
			float dy = 50.0F;
			t.Graphics.TranslateClip(dx, dy);
			// Fill rectangle to demonstrate translated clip region.
			t.Graphics.FillRectangle(new SolidBrush(Color.Black), 0, 0, 500, 300);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void TranslateTransformAngleMatrixOrder() {
			// Set world transform of graphics object to rotate.
			t.Graphics.RotateTransform(30.0F);
			// Then to translate, appending to world transform.
			t.Graphics.TranslateTransform(100.0F, 0.0F, MatrixOrder.Append);
			// Draw rotated, translated ellipse to screen.
			t.Graphics.DrawEllipse(new Pen(Color.Blue, 3), 0, 0, 200, 80);
			t.Show();
			Assert.IsTrue(t.PDCompare()); // Line width problem
		}

		[Test]
		public virtual void TranslateTransformAngleMatrixOrder1() {
			// Set world transform of graphics object to rotate.
			t.Graphics.RotateTransform(30.0F);
			// Then to translate, appending to world transform.
			t.Graphics.TranslateTransform(100.0F, 0.0F, MatrixOrder.Prepend);
			// Draw rotated, translated ellipse to screen.
			t.Graphics.DrawEllipse(new Pen(Color.Blue, 3), 0, 0, 200, 80);
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void TransfromPageScaleUnits() {
			t.Graphics.PageUnit = GraphicsUnit.Millimeter;
			t.Graphics.PageScale = 1.0F;
			t.Graphics.DrawLine(Pens.Red, 10, 70, 70, 10);

			t.Graphics.PageUnit = GraphicsUnit.Document;
			t.Graphics.PageScale = 10.0F;
			t.Graphics.DrawLine(Pens.Blue, 10, 70, 70, 10);

			t.Graphics.PageUnit = GraphicsUnit.Inch;
			t.Graphics.PageScale = 0.055F;
			t.Graphics.DrawLine(Pens.Green, 10, 70, 70, 10);

			Matrix mx=new Matrix(0.5f,0,0,0.5f,0,0);
			t.Graphics.Transform = mx;

			t.Graphics.PageUnit = GraphicsUnit.Inch;
			t.Graphics.DrawLine(Pens.Black, 10, 70, 70, 10);

			t.Graphics.PageUnit = GraphicsUnit.Point;
			t.Graphics.PageScale = 2.7F;
			t.Graphics.DrawLine(Pens.Yellow, 10, 70, 70, 10);

			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public virtual void TransfromPageScaleUnits_2() {
			t.Graphics.RotateTransform(45);
			t.Graphics.PageUnit = GraphicsUnit.Millimeter;
			t.Graphics.PageScale = 1.0F;
			t.Graphics.DrawLine(Pens.Red, 10, 70, 70, 10);

			t.Graphics.TranslateTransform(100, 0);
			t.Graphics.PageUnit = GraphicsUnit.Pixel;
			t.Graphics.PageScale = 2.0F;
			t.Graphics.DrawLine(Pens.Blue, 10, 70, 70, 10);

			t.Graphics.ResetTransform();
			t.Graphics.DrawLine(Pens.Green, 10, 70, 70, 10);

			t.Show();
			Assert.IsTrue(t.PDCompare());
		}
		[Test]
		public virtual void TransfromPageScaleUnits_3() {
			t.Graphics.TranslateTransform(20, 20);
			t.Graphics.PageUnit = GraphicsUnit.Millimeter;
			t.Graphics.PageScale = 1.0F;
			t.Graphics.DrawLine(Pens.Red, 10, 70, 70, 10);
		
			t.Graphics.TranslateTransform(10, 10);
			t.Graphics.PageUnit = GraphicsUnit.Millimeter;
			t.Graphics.PageScale = 1.0F;
			t.Graphics.DrawLine(Pens.Red, 10, 70, 70, 10);
		
			t.Graphics.RotateTransform(15);
		
			t.Graphics.PageUnit = GraphicsUnit.Millimeter;
			t.Graphics.PageScale = 0.5F;
			t.Graphics.DrawLine(Pens.Red, 10, 70, 70, 10);
		
			t.Graphics.PageUnit = GraphicsUnit.Pixel;
			t.Graphics.PageScale = 0.5F;
			t.Graphics.DrawLine(Pens.Red, 10, 70, 70, 10);
					
			t.Graphics.PageUnit = GraphicsUnit.Pixel;
			t.Graphics.TranslateTransform(0, 0);
			t.Graphics.PageScale = 1.5F;
			t.Graphics.DrawLine(Pens.Red, 10, 70, 70, 10);

			t.Show();
			Assert.IsTrue(t.PDCompare());
		}
	}


	#endregion

	#region GraphicsFixturePropClip

	[TestFixture]
	public class GraphicsFixturePropClip : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ("GraphicsFixturePropClip");
			t.Graphics.Clip = new Region(new Rectangle(10, 10, 100, 100));

			st["DrawArcTest:6"] = TOLERANCE * 5.0f;
			st["DrawArcTest:8"] = TOLERANCE * 3.7f;
			st["DrawLinesTest:2"] = TOLERANCE * 3.0f;
			st["DrawLinesTestF:2"] = TOLERANCE * 3.0f;
			st["DrawPieTestF:2"] = TOLERANCE * 2.0f;
			st["DrawPieTestF:4"] = TOLERANCE * 2.0f;
			st["DrawPieTest:2"] = TOLERANCE * 2.0f;
			st["DrawPieTest:4"] = TOLERANCE * 2.0f;
			st["FillClosedCurvePointFillModeTension:2"] = TOLERANCE * 1.5f;
			st["FillClosedCurvePointFFillModeTension:2"] = TOLERANCE * 1.5f;
			st["FillClosedCurvePointFillModeTension:4"] = TOLERANCE * 1.5f;
			st["FillClosedCurvePointFFillModeTension:4"] = TOLERANCE * 1.5f;
			st["FillClosedCurvePointFillModeTension:5"] = TOLERANCE * 1.5f;
			st["FillClosedCurvePointFFillModeTension:6"] = TOLERANCE * 1.5f;
			st["ScaleTransformFloatMatrixOrder1:2"] = TOLERANCE * 3.5f;
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconTest() {
			base.DrawIconTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconUnstretchedTest() {
			base.DrawIconUnstretchedTest ();
		}

		[Test]
		public override void DrawImageUnscaledTest() {
			base.DrawImageUnscaledTest ();
		}

		[Test]
		public override void DrawStringFloatFormat() {
			base.DrawStringFloatFormat ();
		}

		[Test]
		[Category("NotWorking")]
		public override void MeasureCharacterRangesRegions() {
			base.MeasureCharacterRangesRegions ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void MeasureStringSizeFFormatInts() {
			base.MeasureStringSizeFFormatInts ();
		}
	}

	#endregion

	#region GraphicsFixturePropCompositingMode

	[TestFixture]
	public class GraphicsFixturePropCompositingMode1 : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ("GraphicsFixturePropCompositingMode1");
			t.Graphics.CompositingMode = CompositingMode.SourceCopy;
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconTest() {
			base.DrawIconTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconUnstretchedTest() {
			base.DrawIconUnstretchedTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawImageUnscaledTest() {
			base.DrawImageUnscaledTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawStringFloatFormat() {
			base.DrawStringFloatFormat ();
		}

		[Test]
		[Category("NotWorking")]
		public override void MeasureCharacterRangesRegions() {
			base.MeasureCharacterRangesRegions ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void MeasureStringSizeFFormatInts() {
			base.MeasureStringSizeFFormatInts ();
		}
	}

	[TestFixture]
	public class GraphicsFixturePropCompositingMode2 : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ("GraphicsFixturePropCompositingMode2");
			t.Graphics.CompositingMode = CompositingMode.SourceOver;
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconTest() {
			base.DrawIconTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconUnstretchedTest() {
			base.DrawIconUnstretchedTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawImageUnscaledTest() {
			base.DrawImageUnscaledTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawStringFloatFormat() {
			base.DrawStringFloatFormat ();
		}

		[Test]
		[Category("NotWorking")]
		public override void MeasureCharacterRangesRegions() {
			base.MeasureCharacterRangesRegions ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void MeasureStringSizeFFormatInts() {
			base.MeasureStringSizeFFormatInts ();
		}
	}

	#endregion

	#region GraphicsFixturePropInterpolationMode

	[TestFixture]
	public class GraphicsFixturePropInterpolationMode1 : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ("GraphicsFixturePropInterpolationMode1");
			t.Graphics.InterpolationMode = InterpolationMode.Bilinear;
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconTest() {
			base.DrawIconTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconUnstretchedTest() {
			base.DrawIconUnstretchedTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawImageUnscaledTest() {
			base.DrawImageUnscaledTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawStringFloatFormat() {
			base.DrawStringFloatFormat ();
		}

		[Test]
		[Category("NotWorking")]
		public override void MeasureCharacterRangesRegions() {
			base.MeasureCharacterRangesRegions ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void MeasureStringSizeFFormatInts() {
			base.MeasureStringSizeFFormatInts ();
		}
	}

	[TestFixture]
	public class GraphicsFixturePropInterpolationMode2 : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ("GraphicsFixturePropInterpolationMode2");
			t.Graphics.InterpolationMode = InterpolationMode.Bicubic;
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconTest() {
			base.DrawIconTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconUnstretchedTest() {
			base.DrawIconUnstretchedTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawImageUnscaledTest() {
			base.DrawImageUnscaledTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawStringFloatFormat() {
			base.DrawStringFloatFormat ();
		}

		[Test]
		[Category("NotWorking")]
		public override void MeasureCharacterRangesRegions() {
			base.MeasureCharacterRangesRegions ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void MeasureStringSizeFFormatInts() {
			base.MeasureStringSizeFFormatInts ();
		}
	}

	#endregion

	#region GraphicsFixturePropPageScale

	[TestFixture]
	public class GraphicsFixturePropPageScale : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ("GraphicsFixturePropPageScale");

			t.Graphics.PageScale = 4.34f;
			t.Graphics.PageUnit = GraphicsUnit.Pixel;

			st["IntersectClipRegion:4"] = TOLERANCE * 1.5f;
			st["ResetClipIntersectClipRectangleF:2"] = TOLERANCE * 1.5f;
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconTest() {
			base.DrawIconTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconUnstretchedTest() {
			base.DrawIconUnstretchedTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawImageUnscaledTest() {
			base.DrawImageUnscaledTest ();
		}

		[Test]
		public override void DrawStringFloatFormat() {
			base.DrawStringFloatFormat ();
		}

		[Test]
		[Category("NotWorking")]
		public override void MeasureCharacterRangesRegions() {
			base.MeasureCharacterRangesRegions ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void MeasureStringSizeFFormatInts() {
			base.MeasureStringSizeFFormatInts ();
		}
	}

	#endregion

	#region GraphicsFixturePropPageUnit

	[TestFixture]
	public class GraphicsFixturePropPageUnit1 : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ("GraphicsFixturePropPageUnit1");
			t.Graphics.PageUnit = GraphicsUnit.Display;
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconTest() {
			base.DrawIconTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconUnstretchedTest() {
			base.DrawIconUnstretchedTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawImageUnscaledTest() {
			base.DrawImageUnscaledTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawStringFloatFormat() {
			base.DrawStringFloatFormat ();
		}

		[Test]
		[Category("NotWorking")]
		public override void MeasureCharacterRangesRegions() {
			base.MeasureCharacterRangesRegions ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void MeasureStringSizeFFormatInts() {
			base.MeasureStringSizeFFormatInts ();
		}
	}

	[TestFixture]
	public class GraphicsFixturePropPageUnit2 : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ("GraphicsFixturePropPageUnit2");
			t.Graphics.PageUnit = GraphicsUnit.Document;

			// FIXME: scaling down loss some pixels.
			st["DrawBezierTest:2"] = TOLERANCE * 2.5f; 
			st["DrawBezierTest:4"] = TOLERANCE * 2.5f; 
			st["DrawBezierTest:6"] = TOLERANCE * 2.5f; 
			st["DrawBeziersTest:2"] = TOLERANCE * 2.0f;
			st["DrawBeziersTest:4"] = TOLERANCE * 2.0f;
			st["DrawClosedCurveTest:2"] = TOLERANCE * 3.0f;
			st["DrawClosedCurveTest:4"] = TOLERANCE * 3.7f;
			st["DrawCurveTest:2"] = TOLERANCE * 2.5f;
			st["DrawCurveTest:4"] = TOLERANCE * 2.0f;
			st["DrawCurveTest:6"] = TOLERANCE * 4.0f;
			st["DrawCurveTestF:2"] = TOLERANCE * 2.5f;
			st["DrawCurveTestF:4"] = TOLERANCE * 6.0f;
			st["DrawCurveTestF:6"] = TOLERANCE * 6.0f;
			st["DrawCurveTestF:8"] = TOLERANCE * 6.0f;
			st["DrawEllipseTest:2"] = TOLERANCE * 2.0f;
			st["DrawEllipseTest:4"] = TOLERANCE * 2.0f;
			st["DrawEllipseTestF:2"] = TOLERANCE * 2.0f;
			st["DrawEllipseTestF:4"] = TOLERANCE * 2.0f;
			st["DrawLinesTest:2"] = TOLERANCE * 2.0f;
			st["DrawLinesTestF:2"] = TOLERANCE * 2.0f;
			st["DrawPathTest:2"] = TOLERANCE * 2.0f;
			st["DrawPolygonPoint:2"] = TOLERANCE * 7.0f;
			st["DrawPolygonPointF:2"] = TOLERANCE * 7.0f;
			st["FillPieFloat:2"] = TOLERANCE * 1.5f;
			st["FillPieFloat:4"] = TOLERANCE * 1.5f;
			st["FillPieFloat:6"] = TOLERANCE * 1.5f;
			st["IntersectClipRegion:4"] = TOLERANCE * 3.0f;
			st["MultiplyTransform:2"] = TOLERANCE * 2.5f;
			st["MultiplyTransformMatrixOrder1:2"] = TOLERANCE * 2.5f;
			st["TranslateTransformAngleMatrixOrder1:2"] = TOLERANCE * 4.0f;
			st["ScaleTransformFloatMatrixOrder:2"] = TOLERANCE * 4.0f;
			st["ScaleTransformFloatMatrixOrder1:2"] = TOLERANCE * 5.5f;
			st["RotateTransformAngleMatrixOrder:2"] = TOLERANCE * 3.5f;
		}

		[Test]
		[Category("NotWorking")]
		public override void BeginContainerTest() {
			base.BeginContainerTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconTest() {
			base.DrawIconTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconUnstretchedTest() {
			base.DrawIconUnstretchedTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawImageUnscaledTest() {
			base.DrawImageUnscaledTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawStringFloatFormat() {
			base.DrawStringFloatFormat ();
		}

		[Test]
		[Category("NotWorking")]
		public override void EndContainerState() {
			base.EndContainerState ();
		}

		[Test]
		[Category("NotWorking")]
		public override void MeasureCharacterRangesRegions() {
			base.MeasureCharacterRangesRegions ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void MeasureStringSizeFFormatInts() {
			base.MeasureStringSizeFFormatInts ();
		}
	}

	[TestFixture]
	public class GraphicsFixturePropPageUnit3 : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ("GraphicsFixturePropPageUnit3");
			t.Graphics.PageUnit = GraphicsUnit.Inch;
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconTest() {
			base.DrawIconTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconUnstretchedTest() {
			base.DrawIconUnstretchedTest ();
		}

		[Test]
		public override void DrawImageUnscaledTest() {
			base.DrawImageUnscaledTest ();
		}

		[Test]
		public override void DrawStringFloatFormat() {
			base.DrawStringFloatFormat ();
		}

		[Test]
		[Category("NotWorking")]
		public override void IsVisible4Float() {
			base.IsVisible4Float ();
		}

		[Test]
		public override void MeasureCharacterRangesRegions() {
			base.MeasureCharacterRangesRegions ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void MeasureStringSizeFFormatInts() {
			base.MeasureStringSizeFFormatInts ();
		}
	}

	[TestFixture]
	public class GraphicsFixturePropPageUnit4 : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ("GraphicsFixturePropPageUnit4");
			t.Graphics.PageUnit = GraphicsUnit.Millimeter;

			st["DrawArcTest:8"] = TOLERANCE * 1.5f; 
			st["DrawRectangleFloat:2"] = TOLERANCE * 1.5f; // line width problem
			st["DrawRectangleFloat:4"] = TOLERANCE * 1.5f; 
			st["DrawRectangleFloat:6"] = TOLERANCE * 1.5f; 
			st["DrawRectanglesRectangle:2"] = TOLERANCE * 1.5f; 
			st["DrawRectanglesRectangleF:2"] = TOLERANCE * 1.5f; 
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawClosedCurveTest() {
			base.DrawClosedCurveTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconTest() {
			base.DrawIconTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconUnstretchedTest() {
			base.DrawIconUnstretchedTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawImageUnscaledTest() {
			base.DrawImageUnscaledTest ();
		}

		[Test]
		public override void DrawStringFloatFormat() {
			base.DrawStringFloatFormat ();
		}

		[Test]
		[Category("NotWorking")]
		public override void MeasureCharacterRangesRegions() {
			base.MeasureCharacterRangesRegions ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void MeasureStringSizeFFormatInts() {
			base.MeasureStringSizeFFormatInts ();
		}
	}

	[TestFixture]
	public class GraphicsFixturePropPageUnit5 : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ("GraphicsFixturePropPageUnit5");

			t.Graphics.PageUnit = GraphicsUnit.Pixel;
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconTest() {
			base.DrawIconTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconUnstretchedTest() {
			base.DrawIconUnstretchedTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawImageUnscaledTest() {
			base.DrawImageUnscaledTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawStringFloatFormat() {
			base.DrawStringFloatFormat ();
		}

		[Test]
		[Category("NotWorking")]
		public override void MeasureCharacterRangesRegions() {
			base.MeasureCharacterRangesRegions ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void MeasureStringSizeFFormatInts() {
			base.MeasureStringSizeFFormatInts ();
		}
	}

	[TestFixture]
	public class GraphicsFixturePropPageUnit6 : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ("GraphicsFixturePropPageUnit6");
			t.Graphics.PageUnit = GraphicsUnit.Point;

			st["DrawArcTest:2"] = TOLERANCE * 2.5f; 
			st["DrawArcTest:4"] = TOLERANCE * 8.0f; // big difference in width of line
			st["DrawArcTest:6"] = TOLERANCE * 8.0f; // big difference in width of line
			st["DrawArcTest:8"] = TOLERANCE * 6.0f; // big difference in width of line
			st["IsVisible4Float:2"] = TOLERANCE * 1.5f; 
			st["TransformPointsPointF:2"] = TOLERANCE * 2.0f; 
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawClosedCurveTest() {
			base.DrawClosedCurveTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawCurveTest() {
			base.DrawCurveTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawCurveTestF() {
			base.DrawCurveTestF ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawEllipseTest() {
			base.DrawEllipseTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawEllipseTestF() {
			base.DrawEllipseTestF ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconTest() {
			base.DrawIconTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconUnstretchedTest() {
			base.DrawIconUnstretchedTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawImageUnscaledTest() {
			base.DrawImageUnscaledTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawPathTest() {
			base.DrawPathTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawStringFloatFormat() {
			base.DrawStringFloatFormat ();
		}

		[Test]
		[Category("NotWorking")]
		public override void EndContainerState() {
			base.EndContainerState ();
		}

		[Test]
		[Category("NotWorking")]
		public override void MeasureCharacterRangesRegions() {
			base.MeasureCharacterRangesRegions ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void MeasureStringSizeFFormatInts() {
			base.MeasureStringSizeFFormatInts ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void MultiplyTransform() {
			base.MultiplyTransform ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void MultiplyTransformMatrixOrder1() {
			base.MultiplyTransformMatrixOrder1 ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void RotateTransformAngleMatrixOrder1() {
			base.RotateTransformAngleMatrixOrder1 ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void TranslateTransformAngleMatrixOrder() {
			base.TranslateTransformAngleMatrixOrder ();
		}
	}

	//	[TestFixture]
	//	public class GraphicsFixturePropPageUnit7 : GraphicsFixture {
	//		public override void SetUp() {
	//			base.SetUp ();
	//
	//			t.Graphics.PageUnit = GraphicsUnit.World;
	//		}
	//	}

	#endregion

	#region GraphicsFixturePropPixelOffsetMode

	[TestFixture]
	public class GraphicsFixturePropPixelOffsetMode : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ("GraphicsFixturePropPixelOffsetMode");
			t.Graphics.PixelOffsetMode = PixelOffsetMode.Half;

			st["TransformPointsPointF:2"] = TOLERANCE * 3.0f;
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconTest() {
			base.DrawIconTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconUnstretchedTest() {
			base.DrawIconUnstretchedTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawImageUnscaledTest() {
			base.DrawImageUnscaledTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawStringFloatFormat() {
			base.DrawStringFloatFormat ();
		}

		[Test]
		[Category("NotWorking")]
		public override void MeasureCharacterRangesRegions() {
			base.MeasureCharacterRangesRegions ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void MeasureStringSizeFFormatInts() {
			base.MeasureStringSizeFFormatInts ();
		}
	}

	[TestFixture]
	public class GraphicsFixturePropPixelOffsetMode1 : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ("GraphicsFixturePropPixelOffsetMode1");
			t.Graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconTest() {
			base.DrawIconTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconUnstretchedTest() {
			base.DrawIconUnstretchedTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawImageUnscaledTest() {
			base.DrawImageUnscaledTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawStringFloatFormat() {
			base.DrawStringFloatFormat ();
		}

		[Test]
		[Category("NotWorking")]
		public override void MeasureCharacterRangesRegions() {
			base.MeasureCharacterRangesRegions ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void MeasureStringSizeFFormatInts() {
			base.MeasureStringSizeFFormatInts ();
		}
	}

	[TestFixture]
	public class GraphicsFixturePropPixelOffsetMode2 : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ("GraphicsFixturePropPixelOffsetMode2");
			t.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

			st["TransformPointsPointF:2"] = TOLERANCE * 3.0f;
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconTest() {
			base.DrawIconTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconUnstretchedTest() {
			base.DrawIconUnstretchedTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawImageUnscaledTest() {
			base.DrawImageUnscaledTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawStringFloatFormat() {
			base.DrawStringFloatFormat ();
		}

		[Test]
		[Category("NotWorking")]
		public override void MeasureCharacterRangesRegions() {
			base.MeasureCharacterRangesRegions ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void MeasureStringSizeFFormatInts() {
			base.MeasureStringSizeFFormatInts ();
		}
	}

	#endregion

	#region GraphicsFixturePropRenderingOrigin

	[TestFixture]
	[Category("NotWorking")]
	public class GraphicsFixturePropRenderingOrigin : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ("GraphicsFixturePropRenderingOrigin");
			t.Graphics.RenderingOrigin = new Point(12, 23);
		}

		[Test]
		[Category("NotWorking")]
		public override void BeginContainerTest() {
			base.BeginContainerTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void BeginContainerTest_2() {
			base.BeginContainerTest_2 ();
		}

		[Test]
		[Category("NotWorking")]
		public override void ClearTest() {
			base.ClearTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawArcTest() {
			base.DrawArcTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawBezierTest() {
			base.DrawBezierTest ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void DrawBeziersTest() {
			base.DrawBeziersTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawClosedCurveTest() {
			base.DrawClosedCurveTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawCurveTest() {
			base.DrawCurveTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawCurveTestF() {
			base.DrawCurveTestF ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawEllipseTest() {
			base.DrawEllipseTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawEllipseTestF() {
			base.DrawEllipseTestF ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void DrawIconTest() {
			base.DrawIconTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconUnstretchedTest() {
			base.DrawIconUnstretchedTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawImageUnscaledTest() {
			base.DrawImageUnscaledTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawLineTest() {
			base.DrawLineTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawLineTestF() {
			base.DrawLineTestF ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void DrawLinesTest() {
			base.DrawLinesTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawLinesTestF() {
			base.DrawLinesTestF ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawPathTest() {
			base.DrawPathTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawPieTestF() {
			base.DrawPieTestF ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawPieTest() {
			base.DrawPieTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawPolygonPoint() {
			base.DrawPolygonPoint ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void DrawPolygonPointF() {
			base.DrawPolygonPointF ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawRectangleFloat() {
			base.DrawRectangleFloat ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawRectanglesRectangleF() {
			base.DrawRectanglesRectangleF ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawRectanglesRectangle() {
			base.DrawRectanglesRectangle ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawStringFloatFormat() {
			base.DrawStringFloatFormat ();
		}

		[Test]
		[Category("NotWorking")]
		public override void EndContainerState() {
			base.EndContainerState  ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void EnumerateMetafile() {
			base.EnumerateMetafile ();
		}

		[Test]
		[Category("NotWorking")]
		public override void ExcludeClipRegion() {
			base.ExcludeClipRegion ();
		}

		[Test]
		[Category("NotWorking")]
		public override void FillClosedCurvePointFillModeTension() {
			base.FillClosedCurvePointFillModeTension ();
		}

		[Test]
		[Category("NotWorking")]
		public override void FillClosedCurvePointFFillModeTension() {
			base.FillClosedCurvePointFFillModeTension ();
		}

		[Test]
		[Category("NotWorking")]
		public override void FillEllipse() {
			base.FillEllipse ();
		}

		[Test]
		[Category("NotWorking")]
		public override void FillEllipseFloat() {
			base.FillEllipseFloat ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void FillPathEllipse() {
			base.FillPathEllipse ();
		}

		[Test]
		[Category("NotWorking")]
		public override void FillPieFloat() {
			base.FillPieFloat ();
		}

		[Test]
		[Category("NotWorking")]
		public override void FillPolygonPointFillMode() {
			base.FillPolygonPointFillMode ();
		}

		[Test]
		[Category("NotWorking")]
		public override void FillPolygonPointFFillMode() {
			base.FillPolygonPointFFillMode ();
		}

		[Test]
		[Category("NotWorking")]
		public override void FillRectangle() {
			base.FillRectangle ();
		}

		[Test]
		[Category("NotWorking")]
		public override void FillRectangleFloat() {
			base.FillRectangleFloat ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void FillRectanglesRectangle() {
			base.FillRectanglesRectangle ();
		}

		[Test]
		[Category("NotWorking")]
		public override void FillRectanglesRectangleF() {
			base.FillRectanglesRectangleF ();
		}

		[Test]
		[Category("NotWorking")]
		public override void FillRegionRectangle() {
			base.FillRegionRectangle ();
		}

		[Test]
		[Category("NotWorking")]
		public override void FlushTest() {
			base.FlushTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void IsVisible4Float() {
			base.IsVisible4Float ();
		}

		[Test]
		[Category("NotWorking")]
		public override void MeasureCharacterRangesRegions() {
			base.MeasureCharacterRangesRegions ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void MeasureStringSizeFFormatInts() {
			base.MeasureStringSizeFFormatInts ();
		}

		[Test]
		[Category("NotWorking")]
		public override void MultiplyTransform() {
			base.MultiplyTransform ();
		}

		[Test]
		[Category("NotWorking")]
		public override void MultiplyTransformMatrixOrder() {
			base.MultiplyTransformMatrixOrder ();
		}

		[Test]
		[Category("NotWorking")]
		public override void MultiplyTransformMatrixOrder1() {
			base.MultiplyTransformMatrixOrder1 ();
		}

		[Test]
		[Category("NotWorking")]
		public override void ResetClipIntersectClipRectangleF() {
			base.ResetClipIntersectClipRectangleF ();
		}

		[Test]
		[Category("NotWorking")]
		public override void SaveRestoreTranslate() {
			base.SaveRestoreTranslate ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void RotateTransformAngleMatrixOrder() {
			base.RotateTransformAngleMatrixOrder ();
		}

		[Test]
		[Category("NotWorking")]
		public override void RotateTransformAngleMatrixOrder1() {
			base.RotateTransformAngleMatrixOrder1 ();
		}

		[Test]
		[Category("NotWorking")]
		public override void ScaleTransformFloatMatrixOrder() {
			base.ScaleTransformFloatMatrixOrder ();
		}

		[Test]
		[Category("NotWorking")]
		public override void ScaleTransformFloatMatrixOrder1() {
			base.ScaleTransformFloatMatrixOrder1 ();
		}

		[Test]
		[Category("NotWorking")]
		public override void SetClipRegionCombine() {
			base.SetClipRegionCombine ();
		}

		[Test]
		[Category("NotWorking")]
		public override void TransformPointsPointF() {
			base.TransformPointsPointF ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void TranslateClipFloat() {
			base.TranslateClipFloat ();
		}

		[Test]
		[Category("NotWorking")]
		public override void TranslateTransformAngleMatrixOrder() {
			base.TranslateTransformAngleMatrixOrder ();
		}

		[Test]
		[Category("NotWorking")]
		public override void TranslateTransformAngleMatrixOrder1() {
			base.TranslateTransformAngleMatrixOrder1 ();
		}

		[Test]
		[Category("NotWorking")]
		public override void TransfromPageScaleUnits() {
			base.TransfromPageScaleUnits ();
		}

		[Test]
		[Category("NotWorking")]
		public override void TransfromPageScaleUnits_2() {
			base.TransfromPageScaleUnits_2 ();
		}

		[Test]
		[Category("NotWorking")]
		public override void TransfromPageScaleUnits_3() {
			base.TransfromPageScaleUnits_3 ();
		}
	}

	#endregion

	/// <summary>
	/// TBD: add more variants
	/// </summary>
	#region GraphicsFixturePropSmoothingMode 

	[TestFixture]
	public class GraphicsFixturePropSmoothingMode : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ("GraphicsFixturePropSmoothingMode");
			t.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

			st["DrawArcTest:4"] = TOLERANCE * 3.0f;
			st["DrawLineTest:2"] = TOLERANCE * 3.0f;
			st["DrawLineTest:4"] = TOLERANCE * 3.0f; // difference in line width even in horizontal lines
			st["DrawLineTestF:2"] = TOLERANCE * 3.0f;
			st["DrawLineTestF:4"] = TOLERANCE * 3.0f;
			st["DrawPieTest:2"] = TOLERANCE * 1.5f;
			st["DrawPieTestF:2"] = TOLERANCE * 1.5f;
			st["DrawPieTest:4"] = TOLERANCE * 1.5f;
			st["DrawPieTestF:4"] = TOLERANCE * 1.5f;
			st["DrawRectangleFloat:2"] = TOLERANCE * 3.0f; // big difference in line width
			st["DrawRectangleFloat:4"] = TOLERANCE * 3.0f; // big difference in line width
			st["DrawRectangleFloat:6"] = TOLERANCE * 3.0f;
			st["DrawRectanglesRectangle:2"] = TOLERANCE * 3.0f;
			st["DrawRectanglesRectangleF:2"] = TOLERANCE * 3.0f;
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconTest() {
			base.DrawIconTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconUnstretchedTest() {
			base.DrawIconUnstretchedTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawImageUnscaledTest() {
			base.DrawImageUnscaledTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawStringFloatFormat() {
			base.DrawStringFloatFormat ();
		}

		[Test]
		[Category("NotWorking")]
		public override void MeasureCharacterRangesRegions() {
			base.MeasureCharacterRangesRegions ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void MeasureStringSizeFFormatInts() {
			base.MeasureStringSizeFFormatInts ();
		}
	}

	[TestFixture]
	public class GraphicsFixturePropSmoothingMode1 : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ("GraphicsFixturePropSmoothingMode1");
			t.Graphics.SmoothingMode = SmoothingMode.HighSpeed;
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconTest() {
			base.DrawIconTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconUnstretchedTest() {
			base.DrawIconUnstretchedTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawImageUnscaledTest() {
			base.DrawImageUnscaledTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawStringFloatFormat() {
			base.DrawStringFloatFormat ();
		}

		[Test]
		[Category("NotWorking")]
		public override void MeasureCharacterRangesRegions() {
			base.MeasureCharacterRangesRegions ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void MeasureStringSizeFFormatInts() {
			base.MeasureStringSizeFFormatInts ();
		}
	}

	#endregion

	#region GraphicsFixturePropTextContrast

	[TestFixture]
	public class GraphicsFixturePropTextContrast : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ("GraphicsFixturePropTextContrast");
			t.Graphics.TextContrast = 9;
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconTest() {
			base.DrawIconTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconUnstretchedTest() {
			base.DrawIconUnstretchedTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawImageUnscaledTest() {
			base.DrawImageUnscaledTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawStringFloatFormat() {
			base.DrawStringFloatFormat ();
		}

		[Test]
		[Category("NotWorking")]
		public override void MeasureCharacterRangesRegions() {
			base.MeasureCharacterRangesRegions ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void MeasureStringSizeFFormatInts() {
			base.MeasureStringSizeFFormatInts ();
		}
	}

	#endregion

	#region GraphicsFixtureGraphicsState

	[TestFixture]
	public class GraphicsFixtureGraphicsState {
		protected DrawingTest t;
		protected int TOLERANCE = 3; //in %;

		[SetUp]
		public virtual void SetUp() {
			t = DrawingTest.Create(512, 512, "GraphicsFixtureGraphicsState");
		}

		[TearDown]
		public void TearDown ()
		{
			if (t != null)
				t.Dispose ();
		}

		[Test]
		public void BeginEndContainer() {
			t.Graphics.FillRectangle( Brushes.Blue, 0, 0, 100, 100 );

			GraphicsContainer c1 = t.Graphics.BeginContainer( 
				new Rectangle(100, 100, 100, 100), 
				new Rectangle(0, 0, 100, 100), 
				GraphicsUnit.Pixel);

			t.Graphics.FillRectangle( Brushes.Green, 0, 0, 100, 100 );

			GraphicsContainer c2 = t.Graphics.BeginContainer( 
				new Rectangle(100, 100, 100, 100), 
				new Rectangle(0, 0, 100, 100), 
				GraphicsUnit.Pixel);

			t.Graphics.FillRectangle( Brushes.Red, 0, 0, 100, 100 );

			GraphicsState s1 = t.Graphics.Save();
			t.Graphics.PageUnit = GraphicsUnit.Pixel;

			t.Graphics.PageScale = 0.7f;
			t.Graphics.FillRectangle( Brushes.SeaGreen, 0, 0, 100, 100 );

			t.Graphics.EndContainer(c2);
			t.Graphics.PageScale = 0.7f;
			t.Graphics.FillRectangle( Brushes.SeaGreen, 0, 0, 100, 100 );

			t.Graphics.EndContainer(c1);
			t.Graphics.PageScale = 0.7f;
			t.Graphics.FillRectangle( Brushes.SeaGreen, 0, 0, 100, 100 );

			t.Show();
			Assert.IsTrue(t.PDCompare());
		}

		[Test]
		public void SaveRestoreGraphicsProps() {
			t.Graphics.CompositingQuality = CompositingQuality.GammaCorrected;
			t.Graphics.CompositingMode = CompositingMode.SourceCopy;
			t.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
			t.Graphics.PageScale = 7;
			t.Graphics.PageUnit = GraphicsUnit.Inch;
			t.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
			t.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			t.Graphics.Transform = new Matrix(1, 2, 3, 4, 5, 6);
			t.Graphics.TextContrast = 10;
			t.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

			GraphicsContainer c1 = t.Graphics.BeginContainer();

			Assert.AreEqual(CompositingQuality.Default, t.Graphics.CompositingQuality);
			Assert.AreEqual(CompositingMode.SourceOver, t.Graphics.CompositingMode);
			Assert.AreEqual(InterpolationMode.Bilinear, t.Graphics.InterpolationMode);
			Assert.AreEqual(1.0F, t.Graphics.PageScale);
			Assert.AreEqual(GraphicsUnit.Display, t.Graphics.PageUnit);
			Assert.AreEqual(PixelOffsetMode.Default, t.Graphics.PixelOffsetMode);
			Assert.AreEqual(SmoothingMode.None, t.Graphics.SmoothingMode);
			Assert.AreEqual(true, t.Graphics.Transform.IsIdentity);
			Assert.AreEqual(4.0f, t.Graphics.TextContrast);
			Assert.AreEqual(TextRenderingHint.SystemDefault, t.Graphics.TextRenderingHint);

			t.Graphics.EndContainer(c1);
		}
		[Test]
		public void SaveRestoreGraphicsProps_2() {
			GraphicsState s = t.Graphics.Save();

			t.Graphics.CompositingQuality = CompositingQuality.GammaCorrected;
			t.Graphics.CompositingMode = CompositingMode.SourceCopy;
			t.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
			t.Graphics.PageScale = 7;
			t.Graphics.PageUnit = GraphicsUnit.Inch;
			t.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
			t.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			t.Graphics.Transform = new Matrix(1, 2, 3, 4, 5, 6);
			t.Graphics.TextContrast = 10;
			t.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

			t.Graphics.Restore(s);

			Assert.AreEqual(CompositingQuality.Default, t.Graphics.CompositingQuality);
			Assert.AreEqual(CompositingMode.SourceOver, t.Graphics.CompositingMode);
			Assert.AreEqual(InterpolationMode.Bilinear, t.Graphics.InterpolationMode);
			Assert.AreEqual(1.0F, t.Graphics.PageScale);
			Assert.AreEqual(GraphicsUnit.Display, t.Graphics.PageUnit);
			Assert.AreEqual(PixelOffsetMode.Default, t.Graphics.PixelOffsetMode);
			Assert.AreEqual(SmoothingMode.None, t.Graphics.SmoothingMode);
			Assert.AreEqual(true, t.Graphics.Transform.IsIdentity);
			Assert.AreEqual(4.0f, t.Graphics.TextContrast);
			Assert.AreEqual(TextRenderingHint.SystemDefault, t.Graphics.TextRenderingHint);
		}

		[Test]
		public void SaveRestoreGraphicsProps_3() {
			t.Graphics.PageScale = 2;
			GraphicsContainer c1 = t.Graphics.BeginContainer();

			t.Graphics.PageScale = 3;
			GraphicsContainer c2 = t.Graphics.BeginContainer();

			t.Graphics.PageScale = 4;
			GraphicsContainer c3 = t.Graphics.BeginContainer();

			t.Graphics.EndContainer(c2);
			Assert.AreEqual(3, t.Graphics.PageScale);

			t.Graphics.PageScale = 5;
			GraphicsState c5 = t.Graphics.Save();

			t.Graphics.EndContainer(c3);
			Assert.AreEqual(5, t.Graphics.PageScale);

			t.Graphics.Restore(c5);
			Assert.AreEqual(5, t.Graphics.PageScale);

			t.Graphics.EndContainer(c1);
			Assert.AreEqual(2, t.Graphics.PageScale);
		}
		[Test]
		public void SaveRestoreGraphicsProps_4() {
			t.Graphics.PageScale = 2;
			GraphicsContainer c1 = t.Graphics.BeginContainer();

			t.Graphics.PageScale = 3;
			GraphicsState c2 = t.Graphics.Save();

			t.Graphics.EndContainer(c1);
			Assert.AreEqual(2, t.Graphics.PageScale);

			t.Graphics.Restore(c2);
			Assert.AreEqual(2, t.Graphics.PageScale);
		}
	}
	#endregion

	#region GraphicsFixturePropTextRenderingHint

	[TestFixture]
	public class GraphicsFixturePropTextRenderingHint : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ("GraphicsFixturePropTextRenderingHint");
			t.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconTest() {
			base.DrawIconTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconUnstretchedTest() {
			base.DrawIconUnstretchedTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawImageUnscaledTest() {
			base.DrawImageUnscaledTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawStringFloatFormat() {
			base.DrawStringFloatFormat ();
		}

		[Test]
		[Category("NotWorking")]
		public override void MeasureCharacterRangesRegions() {
			base.MeasureCharacterRangesRegions ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void MeasureStringSizeFFormatInts() {
			base.MeasureStringSizeFFormatInts ();
		}
	}

	[TestFixture]
	public class GraphicsFixturePropTextRenderingHint1 : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ("GraphicsFixturePropTextRenderingHint1");
			t.Graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconTest() {
			base.DrawIconTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconUnstretchedTest() {
			base.DrawIconUnstretchedTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawImageUnscaledTest() {
			base.DrawImageUnscaledTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawStringFloatFormat() {
			base.DrawStringFloatFormat ();
		}

		[Test]
		[Category("NotWorking")]
		public override void MeasureCharacterRangesRegions() {
			base.MeasureCharacterRangesRegions ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void MeasureStringSizeFFormatInts() {
			base.MeasureStringSizeFFormatInts ();
		}
	}

	[TestFixture]
	public class GraphicsFixturePropTextRenderingHint2 : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ("GraphicsFixturePropTextRenderingHint2");
			t.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconTest() {
			base.DrawIconTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconUnstretchedTest() {
			base.DrawIconUnstretchedTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawImageUnscaledTest() {
			base.DrawImageUnscaledTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawStringFloatFormat() {
			base.DrawStringFloatFormat ();
		}

		[Test]
		[Category("NotWorking")]
		public override void MeasureCharacterRangesRegions() {
			base.MeasureCharacterRangesRegions ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void MeasureStringSizeFFormatInts() {
			base.MeasureStringSizeFFormatInts ();
		}
	}

	[TestFixture]
	public class GraphicsFixturePropTextRenderingHint3 : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ("GraphicsFixturePropTextRenderingHint3");
			t.Graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixel;
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconTest() {
			base.DrawIconTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconUnstretchedTest() {
			base.DrawIconUnstretchedTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawImageUnscaledTest() {
			base.DrawImageUnscaledTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawStringFloatFormat() {
			base.DrawStringFloatFormat ();
		}

		[Test]
		[Category("NotWorking")]
		public override void MeasureCharacterRangesRegions() {
			base.MeasureCharacterRangesRegions ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void MeasureStringSizeFFormatInts() {
			base.MeasureStringSizeFFormatInts ();
		}
	}

	[TestFixture]
	public class GraphicsFixturePropTextRenderingHint4 : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ("GraphicsFixturePropTextRenderingHint4");
			t.Graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconTest() {
			base.DrawIconTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconUnstretchedTest() {
			base.DrawIconUnstretchedTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawImageUnscaledTest() {
			base.DrawImageUnscaledTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawStringFloatFormat() {
			base.DrawStringFloatFormat ();
		}

		[Test]
		[Category("NotWorking")]
		public override void MeasureCharacterRangesRegions() {
			base.MeasureCharacterRangesRegions ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void MeasureStringSizeFFormatInts() {
			base.MeasureStringSizeFFormatInts ();
		}
	}

	[TestFixture]
	public class GraphicsFixturePropTextRenderingHint5 : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ("GraphicsFixturePropTextRenderingHint5");
			t.Graphics.TextRenderingHint = TextRenderingHint.SystemDefault;
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconTest() {
			base.DrawIconTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconUnstretchedTest() {
			base.DrawIconUnstretchedTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawImageUnscaledTest() {
			base.DrawImageUnscaledTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawStringFloatFormat() {
			base.DrawStringFloatFormat ();
		}

		[Test]
		[Category("NotWorking")]
		public override void MeasureCharacterRangesRegions() {
			base.MeasureCharacterRangesRegions ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void MeasureStringSizeFFormatInts() {
			base.MeasureStringSizeFFormatInts ();
		}
	}

	#endregion

	#region GraphicsFixturePropTransform

	[TestFixture]
	public class GraphicsFixturePropTransform : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ("GraphicsFixturePropTransform");
			t.Graphics.Transform = new Matrix(0, 1, 2, 0, 0, 0);

			st["DrawArcTest:2"] = TOLERANCE * 11.0f; // FIXME: Transfrom is ok, but very big difference in width
			st["DrawArcTest:4"] = TOLERANCE * 12.0f; // FIXME: Transfrom is ok, but very big difference in width
			st["DrawArcTest:6"] = TOLERANCE * 12.0f; // FIXME: Transfrom is ok, but very big difference in width
			st["DrawArcTest:8"] = TOLERANCE * 10.0f; // FIXME: Transfrom is ok, but very big difference in width
			st["DrawClosedCurveTest:4"] = TOLERANCE * 2.0f;
			st["RotateTransformAngleMatrixOrder:2"] = TOLERANCE * 1.5f;
			st["TransformPointsPointF:2"] = TOLERANCE * 3.5f;
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconTest() {
			base.DrawIconTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawIconUnstretchedTest() {
			base.DrawIconUnstretchedTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawImageUnscaledTest() {
			base.DrawImageUnscaledTest ();
		}

		[Test]
		[Category("NotWorking")]
		public override void DrawStringFloatFormat() {
			base.DrawStringFloatFormat ();
		}

		[Test]
		[Category("NotWorking")]
		public override void MeasureCharacterRangesRegions() {
			base.MeasureCharacterRangesRegions ();
		}

		[Test] 
		[Category("NotWorking")]
		public override void MeasureStringSizeFFormatInts() {
			base.MeasureStringSizeFFormatInts ();
		}
	}

	#endregion

}
