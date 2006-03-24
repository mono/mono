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
	
	#region BrushFixture

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

	#endregion

	#region TextureBrushFixture
	[TestFixture]
	public class GraphicsFixtureFillModes {
		protected DrawingTest t;
		protected int TOLERANCE = 3;
		Image bmp;
		Image bmp2;

		[SetUp]
		public virtual void SetUp() {
			SetUp("TextureBrushFixture");
		}
		public virtual void SetUp(string ownerClass) {
			t = DrawingTest.Create(512, 512, ownerClass);
			bmp = Bitmap.FromFile("bitmap50.png");
			bmp2 = Bitmap.FromFile("bitmap25.png");
		}
		[TearDown]
		public void TearDown() {
			if (t != null)
				t.Dispose ();
		}

		[Test]
		public void WrapMode_1() {
			TextureBrush b = new TextureBrush( bmp );
			Assert.AreEqual(WrapMode.Tile, b.WrapMode);
		}
		[Test]
		public void TextureBush_1() {
			TextureBrush b = new TextureBrush( bmp );
			t.Graphics.FillRectangle( b, 100, 100, 300, 300 );
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}
		[Test]
		public void TextureBush_2() {
			TextureBrush b = new TextureBrush( bmp, WrapMode.TileFlipX );
			t.Graphics.FillRectangle( b, 100, 100, 300, 300 );
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}
		[Test]
		public void TextureBush_3() {
			TextureBrush b = new TextureBrush( bmp, WrapMode.TileFlipY );
			t.Graphics.FillRectangle( b, 100, 100, 300, 300 );
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}
		[Test]
		public void TextureBush_4() {
			TextureBrush b = new TextureBrush( bmp, WrapMode.TileFlipXY );
			t.Graphics.FillRectangle( b, 100, 100, 300, 300 );
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}
		[Test]
		public void TextureBush_5() {
			TextureBrush b = new TextureBrush( bmp2, new Rectangle(100, 100, 50, 50) );
			t.Graphics.FillRectangle( b, 100, 100, 300, 300 );
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}
		[Test]
		[Category("NotWorking")]
		public void TextureBush_6() {
			TextureBrush b = new TextureBrush( bmp, WrapMode.TileFlipX, new Rectangle(100, 100, 50, 50) );
			t.Graphics.FillRectangle( b, 100, 100, 300, 300 );
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}
		[Test]
		[Category("NotWorking")]
		public void TextureBush_7() {
			TextureBrush b = new TextureBrush( bmp, WrapMode.TileFlipY, new Rectangle(100, 100, 50, 50) );
			t.Graphics.FillRectangle( b, 100, 100, 300, 300 );
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}
		[Test]
		[Category("NotWorking")]
		public void TextureBush_8() {
			TextureBrush b = new TextureBrush( bmp, WrapMode.TileFlipXY, new Rectangle(100, 100, 50, 50) );
			t.Graphics.FillRectangle( b, 100, 100, 300, 300 );
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}
		[Test]
		[Category("NotWorking")]
		public void TextureBush_9() {
			TextureBrush b = new TextureBrush( bmp, WrapMode.TileFlipXY, new Rectangle(100, 100, 50, 50) );
			t.Graphics.RotateTransform(30);
			t.Graphics.FillRectangle( b, 100, 100, 300, 300 );
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}
		[Test]
		[Category("NotWorking")]
		public void TextureBush_10() {
			TextureBrush b = new TextureBrush( bmp, WrapMode.TileFlipXY, new Rectangle(100, 100, 50, 50) );
			t.Graphics.RotateTransform(30);
			b.RotateTransform(30);
			t.Graphics.FillRectangle( b, 100, 100, 300, 300 );
			t.Show();
			Assert.IsTrue(t.PDCompare());
		}
	}
	#endregion

}
