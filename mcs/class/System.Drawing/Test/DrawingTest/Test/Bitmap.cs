using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using NUnit.Framework;
using DrawingTestHelper;

namespace Test.Sys.Drawing
{
	/// <summary>
	/// Summary description for Bitmap.
	/// </summary>
	[TestFixture]
	public class BitmapFixture {
		DrawingTest t;

		[SetUp]
		public void SetUp () {
			t = DrawingTest.Create (64, 64);
			Bitmap b = new Bitmap ("Bitmap1.png");
			t.Graphics.DrawImageUnscaled (b, 0, 0);
			DrawingTest.ShowForms = false;
		}

		[TearDown]
		public void TearDown ()
		{
			if (t != null)
				t.Dispose ();
		}

		[Test]
		public void CloneTest () {
			Bitmap b1 = (Bitmap) t.Bitmap.Clone ();
			Assert.IsFalse (Object.ReferenceEquals (t.Bitmap, b1));
			Assert.AreEqual (DrawingTest.CalculateNorm (t.Bitmap),
				DrawingTest.CalculateNorm (b1));
			t.Graphics.FillRectangle (Brushes.Black, 0, 0, 64, 64);
			Assert.IsFalse (DrawingTest.CalculateNorm (t.Bitmap) ==
				DrawingTest.CalculateNorm (b1));
		}
		[Test]
		public void GetPixel () {
			Assert.AreEqual (Color.FromArgb (255, Color.White),
				t.Bitmap.GetPixel (0, 0));
			t.Graphics.FillRectangle (Brushes.Black, 30, 30, 30, 30);
			Assert.AreEqual (Color.FromArgb (255, Color.Black),
				t.Bitmap.GetPixel (31, 31));
		}
		[Test]
		public void MakeTransparent () {
			t.Show ();
			Bitmap b = (Bitmap) t.Bitmap.Clone ();
			b.MakeTransparent (Color.White);
			t.Graphics.FillRectangle (Brushes.Black, 0, 0, 64, 64);
			t.Graphics.DrawImageUnscaled (b, 0, 0);
			t.Show ();
			Assert.IsTrue (t.Compare (10));
		}
	}
}
