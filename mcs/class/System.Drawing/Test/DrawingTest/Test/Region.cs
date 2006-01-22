using System;
using NUnit.Framework;
using System.Drawing;
using System.Drawing.Drawing2D;
using DrawingTestHelper;

namespace Test.Sys.Drawing {
	[TestFixture]
	public class RegionFixture {
		DrawingTest t;
		RectangleF rect;
		Region r;
		[SetUp]
		public void SetUp ()
		{
			t = DrawingTest.Create (1000, 1000);
			rect = new RectangleF (50, 50, 50, 50);
			r = new Region(rect);
		}

		[TearDown]
		public void TearDown ()
		{
			if (t != null)
				t.Dispose ();
		}

		[Test]
		[Category ("NotWorking")]
		public void ctor_RegionData () {
			RegionData rgnData = r.GetRegionData ();
			Region r1 = new Region (rgnData);
			Assert.AreEqual (rgnData.Data, r1.GetRegionData ().Data);
		}
		[Test]
		public void ctor_GraphicsPath () {
			GraphicsPath path = new GraphicsPath ();
			path.AddRectangle (rect);
			Region r1 = new Region (path);
			r1.Xor (r);
			Assert.IsTrue (r1.IsEmpty (t.Graphics));
		}
		[Test]
		[Category ("NotWorking")]
		public void ctor_Rectangle () {
			Region r1 = new Region (new Rectangle ((int)rect.X, (int)rect.Y,
				(int)rect.Width, (int)rect.Height));
			Assert.AreEqual (r.GetRegionData ().Data,
				r1.GetRegionData ().Data);
		}
		[Test]
		[Category ("NotWorking")]
		public void ctor_RectangleF () {
			Region r1 = new Region (rect);
			Assert.AreEqual (r.GetRegionData ().Data,
				r1.GetRegionData ().Data);
		}
		[Test]
		public void ctor_void () {
			Region r1 = new Region ();
			Assert.IsTrue (r1.IsInfinite (t.Graphics));
		}
		[Test]
		public void Clone () {
			Region r1 = r.Clone ();
			Assert.IsFalse (Object.ReferenceEquals (r1, r));
			Assert.AreEqual (r1.GetBounds (t.Graphics), r.GetBounds (t.Graphics));
            r1.Xor (r);
			Assert.IsTrue (r1.IsEmpty (t.Graphics));
			Assert.IsFalse (r.IsEmpty (t.Graphics));
		}
		[Test]
		public void Complement () {
			r.Complement (new Rectangle (70, 70, 80, 20));
			Assert.AreEqual (new RectangleF (100, 70, 50, 20), 
				r.GetBounds (t.Graphics));
		}
		[Test]
		public void Equals_Graphics () {
			Region r1 = new Region (rect);
			Assert.IsTrue (r.Equals (r1, t.Graphics));
		}
		[Test]
		public void Exclude () {
			r.Exclude (new Rectangle (10, 10, 90, 60));
			Assert.AreEqual (new RectangleF (50, 70, 50, 30),
				r.GetBounds (t.Graphics));
		}
		[Test]
		public void GetBounds ()
		{
			Assert.AreEqual (rect, r.GetBounds (t.Graphics));
		}
		[Test]
		[Category ("NotWorking")]
		public void GetRegionData () {
			byte [] actual = r.GetRegionData ().Data;
			byte [] expected = new byte [] {28, 0, 0, 0, 186, 15, 11, 58, 1, 16,
				192, 219, 0, 0, 0, 0, 0, 0, 0, 16,
				0, 0, 72, 66, 0, 0, 72, 66, 0, 0,
				72, 66, 0, 0, 72, 66};
			Assert.AreEqual (expected, actual);
		}
		[Test]
		[Category ("NotWorking")]
		public void GetRegionScans () {
			Assert.AreEqual (new RectangleF [] {new Rectangle (50, 50, 50, 50)},
				r.GetRegionScans (new Matrix ()));
			r.Union (new Rectangle (100, 100, 50, 50));
			RectangleF [] rs = new RectangleF [] {
					new Rectangle (50, 50, 50, 50),
					new Rectangle (100, 100, 50, 50)}; 
			Assert.AreEqual (rs,
				r.GetRegionScans (new Matrix ()));
		}
		[Test]
		public void Intersect () {
			r.Intersect (new Rectangle (70, 70, 50, 50));
			Assert.AreEqual (new RectangleF (70, 70, 30, 30), 
				r.GetBounds (t.Graphics));
		}
		[Test]
		public void IsEmpty () {
			Assert.IsFalse (r.IsEmpty (t.Graphics));
			r.Xor (r.Clone ());
			Assert.IsTrue (r.IsEmpty (t.Graphics));
		}
		[Test]
		[Category ("NotWorking")]
		public void IsInfinite () {
			Assert.IsFalse (r.IsInfinite (t.Graphics));
            r.MakeInfinite ();
			Assert.IsTrue (r.IsInfinite (t.Graphics));
			RectangleF infiniteRect = new RectangleF (-0x400000, -0x400000, 0x800000, 0x800000);
			Assert.AreEqual (new RectangleF [] {infiniteRect},
				r.GetRegionScans (new Matrix ()));
			r.Exclude (new Rectangle (10, 10, 10, 10));
			Assert.IsFalse (r.IsInfinite (t.Graphics));
			Assert.AreEqual (infiniteRect, r.GetBounds (t.Graphics));
		}
		[Test]
		public void IsVisible_int () {
			Rectangle rectD = new Rectangle (50, 50, 10000, 10000);
			r = new Region (rectD);
			Assert.IsTrue (r.IsVisible (rectD, t.Graphics));
			Assert.IsTrue (r.IsVisible (rectD.X, rectD.Y, rectD.Height, rectD.Width,
				t.Graphics));
			Assert.IsTrue (r.IsVisible (rectD));
			Assert.IsTrue (r.IsVisible (rectD.X, rectD.Y, rectD.Height, rectD.Width));
			Assert.IsFalse (r.IsVisible (new Point (rectD.Right, rectD.Bottom),
				t.Graphics));
			Assert.IsTrue (r.IsVisible (new Point (rectD.X, rectD.Y)));
			Assert.IsFalse (r.IsVisible (new Point (rectD.X-1, rectD.Y-1)));
			Assert.IsTrue (r.IsVisible (100, 100, t.Graphics));
			Assert.IsTrue (r.IsVisible (100, 100));
		}
		[Test]
		[Category ("NotWorking")]
		public void IsVisible_float () {
			t.Graphics.PageScale = 10;
			rect = new Rectangle (50, 50, 1000, 1000);
			r = new Region (rect);
			Assert.IsTrue (r.IsVisible (rect, t.Graphics));
			Assert.IsTrue (r.IsVisible (rect.X, rect.Y, rect.Height, rect.Width,
				t.Graphics));
			Assert.IsTrue (r.IsVisible (rect));
			Assert.IsTrue (r.IsVisible (rect.X, rect.Y, rect.Height, rect.Width));
			Assert.IsFalse (r.IsVisible (new PointF (rect.Right, rect.Bottom),
				t.Graphics));
			Assert.IsFalse (r.IsVisible (new PointF (rect.Right, rect.Bottom)));
			Assert.IsFalse (r.IsVisible (new PointF (rect.Right-0.1F,
				rect.Bottom-0.1F)));
			Assert.IsTrue (r.IsVisible (new PointF (rect.Right-1,
				rect.Bottom-1)));
			Assert.IsTrue (r.IsVisible (100.0F, 100.0F, t.Graphics));
			Assert.IsTrue (r.IsVisible (100.0F, 100.0F));
			Assert.IsTrue (r.IsVisible (new PointF (rect.X, rect.Y)));
			Assert.IsFalse (r.IsVisible (new PointF (rect.X-0.4F,
				rect.Y-0.4F)));
		}
		[Test]
		public void MakeEmpty () {
			Assert.IsFalse (r.IsEmpty (t.Graphics));
			r.MakeEmpty ();
			Assert.IsTrue (r.IsEmpty (t.Graphics));
			Assert.AreEqual (new RectangleF (0,0,0,0),	r.GetBounds (t.Graphics));
			Assert.IsFalse (r.IsVisible (new Rectangle (0, 0, 0, 0)));
		}
		[Test]
		[Category ("NotWorking")]
		public void MakeInfinite () {
			Assert.IsFalse (r.IsInfinite (t.Graphics));
			r.MakeInfinite ();
			Assert.IsTrue (r.IsInfinite (t.Graphics));
			RectangleF infiniteRect = new RectangleF (-0x400000, -0x400000, 0x800000, 0x800000);
			Assert.AreEqual (new RectangleF [] {infiniteRect},
				r.GetRegionScans (new Matrix ()));
		}
		[Test]
		public new void ToString () {
			Assert.IsTrue (r.ToString ().ToLower().StartsWith("system.drawing.region"));
		}
		[Test]
		public void Transform () {
		}
		[Test]
		public void Translate () {
		}
		[Test]
		[Category ("NotWorking")]
		public void Union () {
			r.Union (new Rectangle (70, 70, 100, 100));
			RectangleF [] rs = new RectangleF [] {
				new RectangleF (50, 50, 50, 20),
				new RectangleF (50, 70, 120, 30),
				new RectangleF (70, 100, 100, 70)};
			Assert.AreEqual (rs, r.GetRegionScans (new Matrix()));
		}
		[Test]
		[Category ("NotWorking")]
		public void Xor () {
			r.Xor (new Rectangle (0, 0, 70, 70));
			RectangleF [] rs = new RectangleF [] {
				new RectangleF (0, 0, 70, 50),
				new RectangleF (0, 50, 50, 20),
				new RectangleF (70, 50, 30, 20),
				new RectangleF (50, 70, 50, 30)};
			Assert.AreEqual (rs, r.GetRegionScans (new Matrix()));
		}
	}
}