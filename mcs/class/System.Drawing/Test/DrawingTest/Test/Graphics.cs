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
using System.Diagnostics;
using NUnit.Framework;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing.Imaging;
using DrawingTestHelper;
using System.IO;

namespace Test.Sys.Drawing.GraphicsFixtures
{
	#region GraphicsFixtureProps

	[TestFixture]
	public class GraphicsFixtureProps {

		protected DrawingTest t;
		const int TOLERANCE = 3; //in %

		[SetUp]
		public void SetUp() {
			t = DrawingTest.Create(512, 512);
		}

		[Test]
		public void ClipTest() {
			Region r = new Region();
			Assert.IsTrue(r.Equals(t.Graphics.Clip, t.Graphics));

			r = new Region(new Rectangle(10, 10, 60, 60));
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
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void ClipBoundsTest() {
			//Debugger.Launch();
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
			Assert.IsTrue(t.Compare(TOLERANCE));
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

	#region GraphicsFixture
	/// <summary>
	/// Summary description for Graphics.
	/// </summary>
	[TestFixture]
	public class GraphicsFixture
	{
		protected DrawingTest t;
		const int TOLERANCE = 3; //in %

		[SetUp]
		public virtual void SetUp() {
			t = DrawingTest.Create(512, 512);
		}

		[TearDown]
		public void TearDown() {
		}

		[Test]
		public void DrawStringAlighnment () {
			StringFormat f = new StringFormat ();
			DrawingTest.ShowForms = true;
	
			Rectangle r1 = new Rectangle (30, 30, 200, 20);
			t.Graphics.DrawRectangle (Pens.Blue, r1);
			f.Alignment = StringAlignment.Near;
			t.Graphics.DrawString ("Near", new Font ("Arial", 10), Brushes.Black,
				r1, f);
			t.Show ();

			Rectangle r2 = new Rectangle (30, 60, 200, 20);
			t.Graphics.DrawRectangle (Pens.Blue, r2);
			f.Alignment = StringAlignment.Center;
			t.Graphics.DrawString ("Center", new Font ("Arial", 10), Brushes.Black,
				r2, f);
			t.Show ();

			Rectangle r3 = new Rectangle (30, 90, 200, 20);
			t.Graphics.DrawRectangle (Pens.Blue, r3);
			f.Alignment = StringAlignment.Far;
			t.Graphics.DrawString ("Far", new Font ("Arial", 10), Brushes.Black,
				r3, f);
			t.Show ();

		}

		[Test]
		public void BeginContainerTest() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void ClearTest() {
			// Clear screen with teal background.
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
			t.Graphics.Clear(Color.Teal);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void DrawArcTest(){
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
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();
			 startAngle =  10.0F;
			 sweepAngle = 120.0F;
			t.Graphics.DrawArc(blackPen, new Rectangle((int)x, (int)y, (int)width, (int)height), startAngle, sweepAngle);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();
			 startAngle =  10.0F;
			 sweepAngle = 190.0F;
			t.Graphics.DrawArc(blackPen, x, y, width, height, startAngle, sweepAngle);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();
			 startAngle =  10.0F;
			 sweepAngle = 300.0F;
			t.Graphics.DrawArc(blackPen, new RectangleF(x, y, width, height), startAngle, sweepAngle);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void DrawBezierTest() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();
			t.Graphics.DrawBezier(blackPen, new PointF( startX, startY),
				new PointF(controlX1, controlY1),
				new PointF(controlX2, controlY2),
				new PointF(endX, endY));
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();
			t.Graphics.DrawBezier(blackPen, new Point((int)startX, (int)startY),
				new Point((int)controlX1, (int)controlY1),
				new Point((int)controlX2, (int)controlY2),
				new Point((int)endX, (int)endY));
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void DrawBeziersTest() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
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
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void DrawClosedCurveTest(){
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
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();

			aFillMode = FillMode.Winding;
			// Draw closed curve to screen.
			t.Graphics.DrawClosedCurve(greenPen, curvePoints, tension, aFillMode);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void DrawCurveTest() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();

			t.Graphics.DrawCurve(greenPen, curvePoints, tension);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();

			t.Graphics.DrawCurve(greenPen, curvePoints);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void DrawCurveTestF() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();

			t.Graphics.DrawCurve(greenPen, curvePoints, offset, numSegments);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();

			t.Graphics.DrawCurve(greenPen, curvePoints, tension);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();

			t.Graphics.DrawCurve(greenPen, curvePoints);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void DrawEllipseTest() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();

			t.Graphics.DrawEllipse(blackPen, new Rectangle(x, y, width, height));
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void DrawEllipseTestF() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();

			t.Graphics.DrawEllipse(blackPen, new RectangleF(x, y, width, height));
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
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
		public void DrawIconTest() {
			// Create icon.
			Icon newIcon = new Icon(getInFile ("SampIcon.ico"));
			// Create coordinates for upper-left corner of icon.
			int x = 100;
			int y = 100;
			// Draw icon to screen.
			t.Graphics.DrawIcon(newIcon, x, y);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));

			t.Graphics.DrawIcon(newIcon, new Rectangle(200, 300, 125, 345));
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void DrawIconUnstretchedTest() {
			// Create icon.
			Icon newIcon = new Icon(getInFile ("SampIcon.ico"));
			// Create rectangle for icon.
			Rectangle rect = new Rectangle( 100, 100, 200, 200);
			// Draw icon to screen.
			t.Graphics.DrawIconUnstretched(newIcon, rect);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
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
		public void DrawImageUnscaledTest() {
			// Create image.
			Image newImage = Image.FromFile(getInFile ("SampIcon.ico"));
			// Create coordinates for upper-left corner of image.
			int x = 100;
			int y = 100;
			// Draw image to screen.
			t.Graphics.DrawImageUnscaled(newImage, x, y, 100, 125);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();

			t.Graphics.DrawImageUnscaled(newImage, new Rectangle(x, y, 34, 235));
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();

			t.Graphics.DrawImageUnscaled(newImage, x, y);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();

			t.Graphics.DrawImageUnscaled(newImage, new Point(x, y));
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void DrawLineTest() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();

			t.Graphics.DrawLine(blackPen, new Point( x1, y1), new Point( x2, y2));
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void DrawLineTestF() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();

			t.Graphics.DrawLine(blackPen, new PointF( x1, y1), new PointF( x2, y2));
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void DrawLinesTest() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void DrawLinesTestF() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void DrawPathTest() {
			// Create graphics path object and add ellipse.
			GraphicsPath graphPath = new GraphicsPath();
			graphPath.AddEllipse(0, 0, 200, 100);
			// Create pen.
			Pen blackPen = new Pen(Color.Black, 3);
			// Draw graphics path to screen.
			t.Graphics.DrawPath(blackPen, graphPath);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void DrawPieTestF() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();

			t.Graphics.DrawPie(blackPen, new RectangleF( x, y, width, height), startAngle, sweepAngle);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void DrawPieTest() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();

			t.Graphics.DrawPie(blackPen, new Rectangle( x, y, width, height), startAngle, sweepAngle);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void DrawPolygonPoint() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void DrawPolygonPointF() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void DrawRectangleFloat() {
			// Create pen.
			Pen blackPen = new Pen(Color.Black, 3);
			// Create location and size of rectangle.
			float x = 0.0F;
			float y = 0.0F;
			float width = 200.0F;
			float height = 200.0F;
			// Draw rectangle to screen.
			t.Graphics.DrawRectangle(blackPen, x, y, width, height);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();

			t.Graphics.DrawRectangle(blackPen, (int)x, (int)y, (int)width, (int)height);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();

			t.Graphics.DrawRectangle(blackPen, new Rectangle( (int)x, (int)y, (int)width, (int)height));
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void DrawRectanglesRectangleF() {
			// Create pen.
			Pen blackPen = new Pen(Color.Black, 3);
			// Create array of rectangles.
			RectangleF[] rects = {
				new RectangleF(  0.0F,   0.0F, 100.0F, 200.0F),
				new RectangleF(100.0F, 200.0F, 250.0F,  50.0F),
				new RectangleF(300.0F,   0.0F,  50.0F, 100.0F)
			};
			// Draw rectangles to screen.
			t.Graphics.DrawRectangles(blackPen, rects);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void DrawRectanglesRectangle() {
			// Create pen.
			Pen blackPen = new Pen(Color.Black, 3);
			// Create array of rectangles.
			Rectangle[] rects = {
									 new Rectangle(  0,   0, 100, 200),
									 new Rectangle(100, 200, 250,  50),
									 new Rectangle(300,   0,  50, 100)
								 };
			// Draw rectangles to screen.
			t.Graphics.DrawRectangles(blackPen, rects);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test] //TBD: add more combinations
		public void DrawStringFloatFormat() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();

			drawFormat.FormatFlags = StringFormatFlags.NoClip;
			// Draw string to screen.
			t.Graphics.DrawString(drawString, drawFont, drawBrush, x, y, drawFormat);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();

			drawFormat.FormatFlags = StringFormatFlags.FitBlackBox;
			// Draw string to screen.
			t.Graphics.DrawString(drawString, drawFont, drawBrush, x, y, drawFormat);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void EndContainerState() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test] //TBD
		public void EnumerateMetafile() {
		}

		[Test]
		public void ExcludeClipRegion() {
			// Create rectangle for exclusion.
			Rectangle excludeRect = new Rectangle(100, 100, 200, 200);
			// Set clipping region to exclude rectangle.
			t.Graphics.ExcludeClip(excludeRect);
			// Fill large rectangle to show clipping region.
			t.Graphics.FillRectangle(new SolidBrush(Color.Blue), 0, 0, 300, 300);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void FillClosedCurvePointFillModeTension() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();

			t.Graphics.FillClosedCurve(redBrush, points, newFillMode);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();

			newFillMode = FillMode.Alternate;
			t.Graphics.FillClosedCurve(redBrush, points, newFillMode, tension);
			Assert.IsTrue(t.Compare(TOLERANCE));
			t.Show();
		}

		[Test]
		public void FillClosedCurvePointFFillModeTension() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();

			t.Graphics.FillClosedCurve(redBrush, points, newFillMode);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();

			newFillMode = FillMode.Alternate;
			t.Graphics.FillClosedCurve(redBrush, points, newFillMode, tension);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void FillEllipse() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();

			t.Graphics.FillEllipse(redBrush, new Rectangle( x, y, width, height));
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void FillEllipseFloat() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();

			t.Graphics.FillEllipse(redBrush, new RectangleF( x, y, width, height));
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void FillPathEllipse() {
			// Create solid brush.
			SolidBrush redBrush = new SolidBrush(Color.Red);
			// Create graphics path object and add ellipse.
			GraphicsPath graphPath = new GraphicsPath();
			graphPath.AddEllipse(0, 0, 200, 100);
			// Fill graphics path to screen.
			t.Graphics.FillPath(redBrush, graphPath);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void FillPieFloat() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();

			t.Graphics.FillPie(redBrush, x, y, width, height, (int)startAngle, (int)sweepAngle);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();

			t.Graphics.FillPie(redBrush, (float)x, (float)y, (float)width, (float)height, startAngle, sweepAngle);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void FillPolygonPointFillMode() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();

			t.Graphics.FillPolygon(blueBrush, curvePoints, FillMode.Alternate);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();

			t.Graphics.FillPolygon(blueBrush, curvePoints);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void FillPolygonPointFFillMode() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();

			t.Graphics.FillPolygon(blueBrush, curvePoints, FillMode.Alternate);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();

			t.Graphics.FillPolygon(blueBrush, curvePoints);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void FillRectangle() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();

			t.Graphics.FillRectangle(blueBrush, new Rectangle( x, y, width, height));
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void FillRectangleFloat() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
			SetUp();

			t.Graphics.FillRectangle(blueBrush, new RectangleF( x, y, width, height));
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void FillRectanglesRectangle() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void FillRectanglesRectangleF() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void FillRegionRectangle() {
			// Create solid brush.
			SolidBrush blueBrush = new SolidBrush(Color.Blue);
			// Create rectangle for region.
			Rectangle fillRect = new Rectangle(100, 150, 200, 250);
			// Create region for fill.
			Region fillRegion = new Region(fillRect);
			// Fill region to screen.
			t.Graphics.FillRegion(blueBrush, fillRegion);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void FlushTest() {
			t.Graphics.Flush();
			t.Graphics.Flush(FlushIntention.Flush);
		}

		[Test]
		public void IntersectClipRegion() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void IsVisible4Float() {
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

			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void MeasureCharacterRangesRegions() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test] //TBD: add more overloads
		public void MeasureStringSizeFFormatInts() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void MultiplyTransform() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void MultiplyTransformMatrixOrder() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void MultiplyTransformMatrixOrder1() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void ResetClipIntersectClipRectangleF() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void SaveRestoreTranslate() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void RotateTransformAngleMatrixOrder() {
			// Set world transform of graphics object to translate.
			t.Graphics.TranslateTransform(100.0F, 0.0F);
			// Then to rotate, appending rotation matrix.
			t.Graphics.RotateTransform(30.0F, MatrixOrder.Append);
			// Draw translated, rotated ellipse to screen.
			t.Graphics.DrawEllipse(new Pen(Color.Blue, 3), 0, 0, 200, 80);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void RotateTransformAngleMatrixOrder1() {
			// Set world transform of graphics object to translate.
			t.Graphics.TranslateTransform(100.0F, 0.0F);
			// Then to rotate, appending rotation matrix.
			t.Graphics.RotateTransform(30.0F, MatrixOrder.Prepend);
			// Draw translated, rotated ellipse to screen.
			t.Graphics.DrawEllipse(new Pen(Color.Blue, 3), 0, 0, 200, 80);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void ScaleTransformFloatMatrixOrder() {
			// Set world transform of graphics object to rotate.
			t.Graphics.RotateTransform(30.0F);
			// Then to scale, appending to world transform.
			t.Graphics.ScaleTransform(3.0F, 1.0F, MatrixOrder.Append);
			// Draw rotated, scaled rectangle to screen.
			t.Graphics.DrawRectangle(new Pen(Color.Blue, 3), 50, 0, 100, 40);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void ScaleTransformFloatMatrixOrder1() {
			// Set world transform of graphics object to rotate.
			t.Graphics.RotateTransform(30.0F);
			// Then to scale, appending to world transform.
			t.Graphics.ScaleTransform(3.0F, 1.0F, MatrixOrder.Prepend);
			// Draw rotated, scaled rectangle to screen.
			t.Graphics.DrawRectangle(new Pen(Color.Blue, 3), 50, 0, 100, 40);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test] //TBD: add more combination
		public void SetClipRegionCombine() {
			// Create region for clipping.
			Region clipRegion = new Region(new Rectangle(0, 0, 200, 100));
			// Set clipping region of graphics to region.
			t.Graphics.SetClip(clipRegion, CombineMode.Replace);
			// Fill rectangle to demonstrate clip region.
			t.Graphics.FillRectangle(new SolidBrush(Color.Black), 0, 0, 500, 300);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void TransformPointsPointF() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void TranslateClipFloat() {
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
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void TranslateTransformAngleMatrixOrder() {
			// Set world transform of graphics object to rotate.
			t.Graphics.RotateTransform(30.0F);
			// Then to translate, appending to world transform.
			t.Graphics.TranslateTransform(100.0F, 0.0F, MatrixOrder.Append);
			// Draw rotated, translated ellipse to screen.
			t.Graphics.DrawEllipse(new Pen(Color.Blue, 3), 0, 0, 200, 80);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
		}

		[Test]
		public void TranslateTransformAngleMatrixOrder1() {
			// Set world transform of graphics object to rotate.
			t.Graphics.RotateTransform(30.0F);
			// Then to translate, appending to world transform.
			t.Graphics.TranslateTransform(100.0F, 0.0F, MatrixOrder.Prepend);
			// Draw rotated, translated ellipse to screen.
			t.Graphics.DrawEllipse(new Pen(Color.Blue, 3), 0, 0, 200, 80);
			t.Show();
			Assert.IsTrue(t.Compare(TOLERANCE));
		}
	}


	#endregion

	#region GraphicsFixturePropClip

	[TestFixture]
	public class GraphicsFixturePropClip : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ();

			t.Graphics.Clip = new Region(new Rectangle(10, 10, 100, 100));
		}
	}

	#endregion

	#region GraphicsFixturePropCompositingMode

	[TestFixture]
	public class GraphicsFixturePropCompositingMode1 : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ();

			t.Graphics.CompositingMode = CompositingMode.SourceCopy;
		}
	}

	[TestFixture]
	public class GraphicsFixturePropCompositingMode2 : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ();

			t.Graphics.CompositingMode = CompositingMode.SourceOver;
		}
	}

	#endregion

	#region GraphicsFixturePropInterpolationMode

	[TestFixture]
	public class GraphicsFixturePropInterpolationMode1 : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ();

			t.Graphics.InterpolationMode = InterpolationMode.Bilinear;
		}
	}

	[TestFixture]
	public class GraphicsFixturePropInterpolationMode2 : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ();

			t.Graphics.InterpolationMode = InterpolationMode.Bicubic;
		}
	}

	#endregion

	#region GraphicsFixturePropPageScale

	[TestFixture]
	public class GraphicsFixturePropPageScale : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ();

			t.Graphics.PageScale = 4.34f;
		}
	}

	#endregion

	#region GraphicsFixturePropPageUnit

	[TestFixture]
	public class GraphicsFixturePropPageUnit1 : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ();

			t.Graphics.PageUnit = GraphicsUnit.Display;
		}
	}

	[TestFixture]
	public class GraphicsFixturePropPageUnit2 : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ();

			t.Graphics.PageUnit = GraphicsUnit.Document;
		}
	}

	[TestFixture]
	public class GraphicsFixturePropPageUnit3 : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ();

			t.Graphics.PageUnit = GraphicsUnit.Inch;
		}
	}

	[TestFixture]
	public class GraphicsFixturePropPageUnit4 : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ();

			t.Graphics.PageUnit = GraphicsUnit.Millimeter;
		}
	}

	[TestFixture]
	public class GraphicsFixturePropPageUnit5 : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ();

			t.Graphics.PageUnit = GraphicsUnit.Pixel;
		}
	}

	[TestFixture]
	public class GraphicsFixturePropPageUnit6 : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ();

			t.Graphics.PageUnit = GraphicsUnit.Point;
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
			base.SetUp ();

			t.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
		}
	}

	[TestFixture]
	public class GraphicsFixturePropPixelOffsetMode1 : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ();

			t.Graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
		}
	}

	[TestFixture]
	public class GraphicsFixturePropPixelOffsetMode2 : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ();

			t.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
		}
	}

	#endregion

	#region GraphicsFixturePropRenderingOrigin

	[TestFixture]
	public class GraphicsFixturePropRenderingOrigin : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ();

			t.Graphics.RenderingOrigin = new Point(12, 23);
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
			base.SetUp ();

			t.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		}
	}

	[TestFixture]
	public class GraphicsFixturePropSmoothingMode1 : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ();

			t.Graphics.SmoothingMode = SmoothingMode.HighSpeed;
		}
	}

	#endregion

	#region GraphicsFixturePropTextContrast

	[TestFixture]
	public class GraphicsFixturePropTextContrast : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ();

			t.Graphics.TextContrast = 9;
		}
	}

	#endregion

	#region GraphicsFixturePropTextRenderingHint

	[TestFixture]
	public class GraphicsFixturePropTextRenderingHint : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ();

			t.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
		}
	}

	[TestFixture]
	public class GraphicsFixturePropTextRenderingHint1 : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ();

			t.Graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
		}
	}

	[TestFixture]
	public class GraphicsFixturePropTextRenderingHint2 : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ();

			t.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
		}
	}

	[TestFixture]
	public class GraphicsFixturePropTextRenderingHint3 : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ();

			t.Graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixel;
		}
	}

	[TestFixture]
	public class GraphicsFixturePropTextRenderingHint4 : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ();

			t.Graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
		}
	}

	[TestFixture]
	public class GraphicsFixturePropTextRenderingHint5 : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ();

			t.Graphics.TextRenderingHint = TextRenderingHint.SystemDefault;
		}
	}

	#endregion

	#region GraphicsFixturePropTransform

	[TestFixture]
	public class GraphicsFixturePropTransform : GraphicsFixture {
		public override void SetUp() {
			base.SetUp ();

			t.Graphics.Transform = new Matrix(0, 1, 2, 0, 0, 0);
		}
	}

	#endregion

}
