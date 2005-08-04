using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using NUnit.Framework;
using DrawingTestHelper;

namespace Test.Sys.Drawing
{
	/// <summary>
	/// Summary description for Pen.
	/// </summary>
	[TestFixture]
	public class PenFixture {
		//TODO: Brush, CompoundArray, CustomEndCap, CustomStartCap,
		//StartCap, EndCap, PenType, Transform
		DrawingTest t;
		Pen p;
		[SetUp]
		public void SetUp () {
			t = DrawingTest.Create (256, 256);
			p = new Pen (Color.Blue);
			p.Width = 10;
		}
		[Test]
		public void InitAlignment () {
			Pen p = new Pen (Color.Blue);
			Assert.AreEqual (PenAlignment.Center, p.Alignment);
		}
		[Test]
		[Category ("test1")]
		public void Width () {
			Assert.AreEqual (10, p.Width);
			t.Graphics.DrawLine (p, 0, 0, 64, 64);
			t.Show ();
			Assert.IsTrue (t.Compare (10));
			p.Width = 0;
			t.Graphics.DrawLine (p, 32, 0, 35, 64);
			t.Show ();
			Assert.IsTrue (t.Compare (10));
			p.Width = 10;
			t.Graphics.DrawLine (p, 0, 64, 64, 70);
			t.Show ();
			Assert.IsTrue (t.Compare (10));
		}
		[Test]
		public void DashStyleTest () {
			Assert.AreEqual (DashStyle.Solid, p.DashStyle);
			t.Graphics.DrawLine (p, 0, 30, 256, 30);
			t.Show ();
			Assert.IsTrue (t.Compare (10));
			
			p.DashStyle = DashStyle.Dash;
			p.Width = 10;
			t.Graphics.DrawLine (p, 0, 600, 200, 60);
			t.Show ();
			Assert.IsTrue (t.Compare (10));
			
			p.DashStyle = DashStyle.DashDot;
			t.Graphics.FillRectangle (Brushes.Black, 0, 0, 256, 256);
			t.Graphics.DrawLine (p, 0, 0, 200, 200);
			t.Show ();
			Assert.IsTrue (t.Compare (10));
			
			p.DashStyle = DashStyle.DashDotDot;
			t.Graphics.FillRectangle (Brushes.Black, 0, 0, 256, 256);
			t.Graphics.DrawLine (p, 0, 0, 200, 200);
			t.Show ();
			Assert.IsTrue (t.Compare (10));

			p.DashStyle = DashStyle.Dot;
			t.Graphics.FillRectangle (Brushes.Black, 0, 0, 256, 256);
			t.Graphics.DrawLine (p, 0, 0, 200, 200);
			t.Show ();
			Assert.IsTrue (t.Compare (10));

			p.DashStyle = DashStyle.Custom;
			t.Graphics.FillRectangle (Brushes.Black, 0, 0, 256, 256);
			t.Graphics.DrawLine (p, 0, 0, 200, 200);
			t.Show ();
			Assert.IsTrue (t.Compare (10));
		}
		//The following tests DashOffset and DashPattern
		[Test]
		public void DashCustomStyle () {
			p.DashStyle = DashStyle.Custom;
			p.Width = 10;
			Assert.AreEqual (new float [] {1F}, p.DashPattern);
			Assert.AreEqual (0F, p.DashOffset);

			p.DashPattern = new float [] {2, 3, 1.15F, 0.05F};
			t.Graphics.FillRectangle (Brushes.Black, 0, 0, 256, 256);
			t.Graphics.DrawLine (p, 0, 0, 200, 10);
			t.Show ();
			Assert.IsTrue (t.Compare (10));
			p.DashOffset = 10F;
			t.Graphics.DrawLine (p, 0, 50, 200, 50);
			t.Show ();
			Assert.IsTrue (t.Compare (10));
			p.DashPattern = new float [] {2, 3, 1.15F, 0.05F, 1.74321F};
			p.DashOffset = 10.2F;
			t.Graphics.DrawLine (p, 0, 100, 200, 90);
			t.Show ();
			Assert.IsTrue (t.Compare (10));
			p.DashPattern = new float [] {2, 3, 1.15F, 0.05F, 1.74321F};
			p.DashOffset = 10.2F;
			t.Graphics.DrawLine (p, 0, 100, 200, 90);
			t.Show ();
			Assert.IsTrue (t.Compare (10));
		}
		[Test]
		public void DashCapTest () {
			Assert.AreEqual (DashCap.Flat, p.DashCap);
			p.DashStyle = DashStyle.DashDot;
			t.Graphics.DrawLine (p, 0, 0, 200, 10);
			t.Show ();
			Assert.IsTrue (t.Compare (10));

			p.DashCap = DashCap.Round;
			t.Graphics.DrawLine (p, 0, 50, 200, 61.7F);
			t.Show ();
			Assert.IsTrue (t.Compare (10));

			p.DashCap = DashCap.Triangle;
			t.Graphics.DrawLine (p, 0F, 92.3F, 200F, 117.9F);
			t.Show ();
			Assert.IsTrue (t.Compare (10));
		}
		[Test]
		public void LineJoinTest () {
			Assert.AreEqual (LineJoin.Miter, p.LineJoin);
			Point [] points = new Point [] {
											   new Point(40, 10), new Point (200, 11),
											   new Point (100, 40)};
			t.Graphics.DrawPolygon (p, points);
			t.Graphics.DrawPolygon (Pens.White, points);
			t.Show ();
			Assert.IsTrue (t.Compare (10));

			p.LineJoin = LineJoin.Bevel;
			points =new Point [] {
									 new Point(40, 70), new Point (200, 79),
									 new Point (100, 100)};
			t.Graphics.DrawPolygon (p, points);
			t.Graphics.DrawPolygon (Pens.White, points);
			t.Show ();
			Assert.IsTrue (t.Compare (10));

			p.LineJoin = LineJoin.MiterClipped;
			points = new Point [] {
									  new Point(40, 120), new Point (200, 117),
									  new Point (80, 135)};
			t.Graphics.DrawPolygon (p, points);
			t.Graphics.DrawPolygon (Pens.White, points);
			t.Show ();
			Assert.IsTrue (t.Compare (10));

			p.LineJoin = LineJoin.Round;
			points = new Point [] {
									  new Point(40, 170), new Point (200, 175),
									  new Point (100, 210)};
			t.Graphics.DrawPolygon (p, points);
			t.Graphics.DrawPolygon (Pens.White, points);
			t.Show ();
			Assert.IsTrue (t.Compare (10));
		}
		[Test]
		public void PenAlignmentTest () {
			Point [] points = new Point [] {
				new Point (20, 20), new Point (40, 40), new Point (60, 20),
				new Point (60, 60), new Point (20, 60)};
			GraphicsPath path = new GraphicsPath ();
			path.AddPolygon (points);
			Matrix mx = new Matrix (1, 0, 0, 1, 90, 0);
			Matrix mrx = new Matrix (1, 0, 0, 1, -180, 0);
			Matrix my = new Matrix (1, 0, 0, 1, 0, 100);
						
			Assert.AreEqual (PenAlignment.Center, p.Alignment);
			t.Graphics.DrawPath (p, path);
			t.Graphics.DrawPath (Pens.White, path);
			t.Show ();
			Assert.IsTrue (t.Compare (10));

			p.Alignment = PenAlignment.Left;
			path.Transform (mx);
			t.Graphics.DrawPath (p, path);
			t.Graphics.DrawPath (Pens.White, path);
			t.Show ();
			Assert.IsTrue (t.Compare (10));

			p.Alignment = PenAlignment.Inset;
			path.Transform (mx);
			t.Graphics.DrawPath (p, path);
			t.Graphics.DrawPath (Pens.White, path);
			t.Show ();
			Assert.IsTrue (t.Compare (10));

			p.Alignment = PenAlignment.Outset;
			path.Transform (mrx);
			path.Transform (my);
			t.Graphics.DrawPath (p, path);
			t.Graphics.DrawPath (Pens.White, path);
			t.Show ();
			Assert.IsTrue (t.Compare (10));

			p.Alignment = PenAlignment.Right;
			path.Transform (mx);
			t.Graphics.DrawPath (p, path);
			t.Graphics.DrawPath (Pens.White, path);
			t.Show ();
			Assert.IsTrue (t.Compare (10));
		}
		[Test]
		public void ColorTest () {
			Assert.AreEqual (Color.Blue, p.Color);
			t.Graphics.DrawLine (p, 0, 0, 200, 200);
			t.Show ();
			Assert.IsTrue (t.Compare (10));

			p.Color = Color.Red;
			t.Graphics.DrawLine (p, 0, 0, 200, 200);
			t.Show ();
			Assert.IsTrue (t.Compare (10));

			p.Color = Color.BurlyWood;
			t.Graphics.DrawLine (p, 0, 0, 200, 200);
			t.Show ();
			Assert.IsTrue (t.Compare (10));

			p.Color = Color.FromArgb (100, 120, 255);
			t.Graphics.DrawLine (p, 0, 0, 200, 200);
			t.Show ();
			Assert.IsTrue (t.Compare (10));

			p.Color = Color.FromArgb (128, Color.White);
			t.Graphics.DrawLine (p, 0, 200, 200, 0);
			t.Show ();
			Assert.IsTrue (t.Compare (10));
		}
		[Test]
		public void MitterLimit () {
			p.LineJoin = LineJoin.MiterClipped;
			Point [] points = new Point [] {new Point (0,30), new Point (180, 31),
											   new Point (0, 90)};

			Assert.AreEqual (10F, p.MiterLimit);
			t.Graphics.DrawLines (p, points);
			t.Graphics.DrawLines (Pens.White, points);
			t.Show ();
			Assert.IsTrue (t.Compare (10));

			Matrix dy = new Matrix (1, 0, 0, 1, 0, 110);
			dy.TransformPoints (points);
			p.MiterLimit=1F;
			t.Graphics.DrawLines (p, points);
			t.Graphics.DrawLines (Pens.White, points);
			t.Show ();
			Assert.IsTrue (t.Compare (10));
		}
		[Test]
		public void Transform () {
			p.ScaleTransform (0.5F, 2);
			t.Graphics.DrawArc (p, 0, 0, 100, 100, 0, 360);
			t.Graphics.DrawArc (Pens.White, 0, 0, 100, 100, 0, 360);
			t.Show ();
		}
	}
}
