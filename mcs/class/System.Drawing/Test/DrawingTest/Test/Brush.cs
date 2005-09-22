using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using NUnit.Framework;
using DrawingTestHelper;

namespace Test.Sys.Drawing
{
	/// <summary>
	/// Summary description for Brush.
	/// </summary>
	[TestFixture]
	public class BrushFixture
	{
		[Test]
		public void ColorTest()
		{
			SolidBrush b = new SolidBrush (Color.Azure);
			Assert.AreEqual (Color.Azure, b.Color);
			DrawingTest t = DrawingTest.Create (64, 64);
			t.Graphics.FillRectangle (b, 0, 0, 30, 30);
			t.Show ();
			b.Color = Color.FromArgb (100, 240, 30);
			t.Graphics.FillRectangle (b, 30, 5, 30, 50);
			t.Show ();
			b.Color = Color.FromArgb (70, Color.FromName ("red"));
			t.Graphics.FillRectangle (b, 15, 15, 40, 40);
			t.Show ();
		}
	}
}
