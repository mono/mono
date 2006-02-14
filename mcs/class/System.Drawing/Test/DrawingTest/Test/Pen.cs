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
		protected int TOLERANCE = 3;


		[SetUp]
		public void SetUp () {
			t = DrawingTest.Create (256, 256);
			p = new Pen (Color.Blue);
			p.Width = 10;
			DrawingTest.ShowForms = false;
		}

		[TearDown]
		public void TearDown ()
		{
			if (t != null)
				t.Dispose ();
		}

		#region InitAlignment
		[Test]
		public void InitAlignment () {
			Pen p = new Pen (Color.Blue);
			Assert.AreEqual (PenAlignment.Center, p.Alignment);
		}
		#endregion
		
		#region PenWidth
		[Test]
		public void PenWidth_1()
		{
			Assert.AreEqual(10, p.Width);
			t.Graphics.DrawLine (p, 20, 200, 200, 20);
			t.Show();
			Assert.IsTrue (t.Compare (TOLERANCE * 1.5f)); //FIXME: Pen width in GH is not the same as in .NET
		}
		[Test]
		public void PenWidth_2()
		{
			p.Width = 25;
			t.Graphics.DrawLine (p, 20, 200, 200, 20);
			t.Show();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		public void PenWidth_3 () 
		{
			t.Graphics.DrawLine (p, 10, 100, 200, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		#endregion

		#region DashStyle Tests
		[Test]
		public void DashStyleTest_1 () 
		{
			Assert.AreEqual (DashStyle.Solid, p.DashStyle);
			p.Width = 14;
			t.Graphics.DrawLine (p, 20, 100, 230, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		public void DashStyleTest_2 () {
			p.DashStyle = DashStyle.Dash;
			p.Width = 14;
			t.Graphics.DrawLine (p, 20, 100, 230, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		public void DashStyleTest_3 () {
			p.DashStyle = DashStyle.DashDot;
			p.Width = 14;
			t.Graphics.FillRectangle (Brushes.Black, 0, 0, 256, 256);
			t.Graphics.DrawLine (p, 20, 100, 230, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		public void DashStyleTest_4 () {	
			p.DashStyle = DashStyle.DashDotDot;
			p.Width = 14;
			t.Graphics.FillRectangle (Brushes.Black, 0, 0, 256, 256);
			t.Graphics.DrawLine (p, 20, 100, 230, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		public void DashStyleTest_5 () {
			p.DashStyle = DashStyle.Dot;
			p.Width = 14;
			t.Graphics.FillRectangle (Brushes.Black, 0, 0, 256, 256);
			t.Graphics.DrawLine (p, 20, 100, 230, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		public void DashStyleTest_6 () {
			p.DashStyle = DashStyle.Custom;
			p.Width = 14;
			t.Graphics.FillRectangle (Brushes.Black, 0, 0, 256, 256);
			t.Graphics.DrawLine (p, 20, 100, 230, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		#endregion

		#region DashCustomStyle
		//The following tests DashOffset and DashPattern
		[Test]
		[Category ("NotWorking")]
		public void DashCustomStyle_1 () {
			p.DashStyle = DashStyle.Custom;
			p.Width = 10;
			Assert.AreEqual (new float [] {1F}, p.DashPattern);
			Assert.AreEqual (0F, p.DashOffset);
		}
		[Test]
		public void DashCustomStyle_2 () {
			p.DashPattern = new float [] {2, 3, 1.15F, 0.05F};
			t.Graphics.FillRectangle (Brushes.Black, 0, 0, 256, 256);
			t.Graphics.DrawLine (p, 20, 100, 200, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		public void DashCustomStyle_3 () {
			p.DashOffset = 10F;
			t.Graphics.DrawLine (p, 20, 100, 200, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		[Category ("NotWorking")]
		public void DashCustomStyle_4 () {
			p.DashPattern = new float [] {2, 3, 1.15F, 0.05F, 1.74321F};
			p.DashOffset = 10.2F;
			t.Graphics.DrawLine (p, 20, 100, 200, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		[Category ("NotWorking")]
		public void DashCustomStyle_5 () {
			p.DashPattern = new float [] {2, 3, 1.15F, 0.05F, 1.74321F};
			p.DashOffset = 10.2F;
			t.Graphics.DrawLine (p, 20, 100, 200, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		#endregion

		#region DashCapTest
		[Test]
		public void DashCapTest_Flat () {
			p.Width = 15;
			Assert.AreEqual (DashCap.Flat, p.DashCap);
			p.DashStyle = DashStyle.DashDot;
			t.Graphics.DrawLine (p, 10, 100, 230, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		[Category ("NotWorking")]
		public void DashCapTest_Round () {
			p.Width = 15;
			p.DashStyle = DashStyle.DashDot;
			p.DashCap = DashCap.Round;
			t.Graphics.DrawLine (p, 10, 100, 230, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		[Category ("NotWorking")]
		public void DashCapTest_Triangle () {
			p.Width = 15;
			p.DashStyle = DashStyle.DashDot;
			p.DashCap = DashCap.Triangle;
			t.Graphics.DrawLine (p, 10, 100, 230, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		#endregion

		#region LineJoin Round
		[Test]
		public void LineJoinTest_Round_1 () {
			Point [] points = new Point [] {
											   new Point(100, 210), new Point (120, 50),
											   new Point (140, 210)};
			p.Width = 25;
			p.LineJoin = LineJoin.Round;
			t.Graphics.DrawPolygon (p, points);
			t.Graphics.DrawPolygon (Pens.White, points);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}

		[Test]
		public void LineJoinTest_Round_2 () {
			Point [] points = new Point [] {
									 new Point(50, 210), new Point (120, 50),
									 new Point (190, 210)};
			p.Width = 25;
			p.LineJoin = LineJoin.Round;
			t.Graphics.DrawPolygon (p, points);
			t.Graphics.DrawPolygon (Pens.White, points);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		#endregion

		#region LineJoin Miter
		[Test]
		public void LineJoinTest_Miter_1 () {
			p.LineJoin = LineJoin.Miter;
			Point [] points = new Point [] {
											   new Point(100, 217), new Point (130, 100),
											   new Point (160, 217)};
			p.Width = 25;
			t.Graphics.DrawLines (p, points);
			t.Graphics.DrawLines (Pens.White, points);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		[Category ("NotWorking")]
		public void LineJoinTest_Miter_2 () {
			p.LineJoin = LineJoin.Miter;
			Point [] points = new Point [] {
											   new Point(120, 237), new Point (130, 100),
											   new Point (140, 237)};
			p.Width = 10;
			t.Graphics.DrawLines (p, points);
			t.Graphics.DrawLines (Pens.White, points);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		public void LineJoinTest_Miter_3 () {
			p.LineJoin = LineJoin.Miter;
			Point [] points = new Point [] {
											   new Point(50, 217), new Point (100, 100),
											   new Point (150, 217)};
			p.Width = 25;
			t.Graphics.DrawLines (p, points);
			t.Graphics.DrawLines (Pens.White, points);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		#endregion

		#region LineJoin MiterClipped
		[Test]
		public void LineJoinTest_MiterClipped_1 () {
			p.LineJoin = LineJoin.MiterClipped;
			Point [] points = new Point [] {
											   new Point(100, 217), new Point (130, 100),
											   new Point (160, 217)};
			p.Width = 25;
			t.Graphics.DrawLines (p, points);
			t.Graphics.DrawLines (Pens.White, points);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		public void LineJoinTest_MiterClipped_2 () {
			p.LineJoin = LineJoin.MiterClipped;
			Point [] points = new Point [] {
											   new Point(120, 217), new Point (130, 80),
											   new Point (140, 217)};
			p.Width = 25;
			t.Graphics.DrawLines (p, points);
			t.Graphics.DrawLines (Pens.White, points);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		public void LineJoinTest_MiterClipped_3 () {
			p.LineJoin = LineJoin.MiterClipped;
			Point [] points = new Point [] {
									  new Point(50, 217), new Point (100, 100),
									  new Point (150, 217)};
			p.Width = 25;
			t.Graphics.DrawLines (p, points);
			t.Graphics.DrawLines (Pens.White, points);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		#endregion

		#region "LineJoin Bevel"
		[Test]
		public void LineJoinTest_Bevel_1 () {
			p.LineJoin = LineJoin.Bevel;
			Point [] points = new Point [] {
											   new Point(90, 217), new Point (115, 55),
											   new Point (140, 217)};
			p.Width = 25;
			t.Graphics.DrawLines (p, points);
			t.Graphics.DrawLines (Pens.White, points);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		public void LineJoinTest_Bevel_2 () {
			p.LineJoin = LineJoin.Bevel;
			Point [] points = new Point [] {
									  new Point(110, 217), new Point (120, 75),
									  new Point (130, 217)};
			p.Width = 25;
			t.Graphics.DrawLines (p, points);
			t.Graphics.DrawLines (Pens.White, points);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		public void LineJoinTest_Bevel_3 () {
			p.LineJoin = LineJoin.Bevel;
			Point [] points = new Point [] {
									  new Point(50, 217), new Point (100, 100),
									  new Point (150, 217)};
			p.Width = 25;
			t.Graphics.DrawLines (p, points);
			t.Graphics.DrawLines (Pens.White, points);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		public void LineJoinTest_Bevel_4 () {
			p.LineJoin = LineJoin.Bevel;
			Point [] points = new Point [] {
									  new Point(143, 210), new Point (170, 100),
									  new Point (180, 20)};
			p.Width = 25;
			t.Graphics.DrawLines (p, points);
			t.Graphics.DrawLines (Pens.White, points);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		public void LineJoinTest_Bevel_5 () {
			p.LineJoin = LineJoin.Bevel;
			Point [] points = new Point [] {
									  new Point(50, 100), new Point (150, 100),
									  new Point (150, 20), new Point (200, 20)};
			p.Width = 25;
			t.Graphics.DrawLines (p, points);
			t.Graphics.DrawLines (Pens.White, points);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		#endregion

		#region PenAlignment
		[Test]
		public void PenAlignmentTest_1 () {
			Assert.AreEqual (PenAlignment.Center, p.Alignment);
			Point [] points = new Point [] {
											   new Point (30, 30), new Point (100, 100), new Point (170, 30),
											   new Point (170, 200), new Point (30, 200)};
			GraphicsPath path = new GraphicsPath ();
			path.AddPolygon (points);

			p.Width = 25;
			t.Graphics.DrawPath (p, path);
			t.Graphics.DrawPath (Pens.White, path);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		public void PenAlignmentTest_2 () {
			Point [] points = new Point [] {
											   new Point (30, 30), new Point (100, 100), new Point (170, 30),
											   new Point (170, 200), new Point (30, 200)};
			GraphicsPath path = new GraphicsPath ();
			path.AddPolygon (points);

			p.Width = 25;
			p.Alignment = PenAlignment.Left;
			t.Graphics.DrawPath (p, path);
			t.Graphics.DrawPath (Pens.White, path);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}

		[Test]
		[Category ("NotWorking")]
		public void PenAlignmentTest_3 () {
			Point [] points = new Point [] {
											   new Point (30, 30), new Point (100, 100), new Point (170, 30),
											   new Point (170, 200), new Point (30, 200)};
			GraphicsPath path = new GraphicsPath ();
			path.AddPolygon (points);

			p.Width = 25;
			p.Alignment = PenAlignment.Inset;
			t.Graphics.DrawPath (p, path);
			t.Graphics.DrawPath (Pens.White, path);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}

		[Test]
		public void PenAlignmentTest_4 () {
			Point [] points = new Point [] {
											   new Point (30, 30), new Point (100, 100), new Point (170, 30),
											   new Point (170, 200), new Point (30, 200)};
			GraphicsPath path = new GraphicsPath ();
			path.AddPolygon (points);

			p.Width = 25;
			p.Alignment = PenAlignment.Outset;
			t.Graphics.DrawPath (p, path);
			t.Graphics.DrawPath (Pens.White, path);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}

		[Test]
		public void PenAlignmentTest_5 () {
			Point [] points = new Point [] {
											   new Point (30, 30), new Point (100, 100), new Point (170, 30),
											   new Point (170, 200), new Point (30, 200)};
			GraphicsPath path = new GraphicsPath ();
			path.AddPolygon (points);

			p.Width = 25;
			p.Alignment = PenAlignment.Right;
			t.Graphics.DrawPath (p, path);
			t.Graphics.DrawPath (Pens.White, path);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		#endregion
		
		#region Color test
		[Test]
		public void ColorTest_1 () {
			Assert.AreEqual (Color.Blue, p.Color);
			p.Width = 25;
			t.Graphics.DrawLine (p, 10, 100, 230, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		public void ColorTest_2 () {
			p.Color = Color.Red;
			p.Width = 25;
			t.Graphics.DrawLine (p, 10, 100, 230, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		public void ColorTest_3 () {
			p.Color = Color.BurlyWood;
			p.Width = 25;
			t.Graphics.DrawLine (p, 10, 100, 230, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		public void ColorTest_4 () {
			p.Color = Color.FromArgb (100, 120, 255);
			p.Width = 25;
			t.Graphics.DrawLine (p, 10, 100, 230, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		public void ColorTest_5 () {
			p.Color = Color.FromArgb (128, Color.White);
			p.Width = 25;
			t.Graphics.DrawLine (p, 10, 100, 230, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		#endregion

		#region MiterLimit
		[Test]
		public void MitterLimit_1 () {
			p.LineJoin = LineJoin.MiterClipped;
			Point [] points = new Point [] {new Point (0,30), new Point (180, 30),
											   new Point (0, 90)};

			p.Width = 25;
			Assert.AreEqual (10F, p.MiterLimit);
			t.Graphics.DrawLines (p, points);
			t.Graphics.DrawLines (Pens.White, points);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		public void MitterLimit_2 () {
			p.MiterLimit=1F;
			p.LineJoin = LineJoin.MiterClipped;
			Point [] points = new Point [] {new Point (0,30), new Point (180, 30),
											   new Point (0, 120)};

			p.Width = 25;
			t.Graphics.DrawLines (p, points);
			t.Graphics.DrawLines (Pens.White, points);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		#endregion

		#region TRansform
		[Test]
		public void Transform () {
			p.ScaleTransform (0.5F, 2);
			t.Graphics.DrawArc (p, 70, 70, 100, 100, 0, 360);
			t.Graphics.DrawArc (Pens.White, 70, 70, 100, 100, 0, 360);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		#endregion
		
		#region Line StartCap
		[Test]
		public void StartCap_Flat() {
			Assert.AreEqual(LineCap.Flat, p.StartCap);
			p.StartCap = LineCap.Flat;
			p.Width = 25;
			t.Graphics.DrawLine (p, 50, 100, 150, 100);
			t.Graphics.DrawLine (Pens.White, 50, 100, 150, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		[Category ("NotWorking")]
		public void StartCap_Round() {
			p.StartCap = LineCap.Round;
			p.Width = 25;
			t.Graphics.DrawLine (p, 50, 100, 150, 100);
			t.Graphics.DrawLine (Pens.White, 50, 100, 150, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		[Category ("NotWorking")]
		public void StartCap_Square() {
			p.StartCap = LineCap.Square;
			p.Width = 25;
			t.Graphics.DrawLine (p, 50, 100, 150, 100);
			t.Graphics.DrawLine (Pens.White, 50, 100, 150, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		public void StartCap_AnchorMask() {
			p.StartCap = LineCap.AnchorMask;
			p.Width = 25;
			t.Graphics.DrawLine (p, 50, 100, 150, 100);
			t.Graphics.DrawLine (Pens.White, 50, 100, 150, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		public void StartCap_ArrowAnchor() {
			p.StartCap = LineCap.ArrowAnchor;
			p.Width = 25;
			t.Graphics.DrawLine (p, 50, 100, 150, 100);
			t.Graphics.DrawLine (Pens.White, 50, 100, 150, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		[Category ("NotWorking")]
		public void StartCap_DiamondAnchor() {
			p.StartCap = LineCap.DiamondAnchor;
			p.Width = 25;
			t.Graphics.DrawLine (p, 50, 100, 150, 100);
			t.Graphics.DrawLine (Pens.White, 50, 100, 150, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		public void StartCap_NoAnchor() {
			p.StartCap = LineCap.NoAnchor;
			p.Width = 25;
			t.Graphics.DrawLine (p, 50, 100, 150, 100);
			t.Graphics.DrawLine (Pens.White, 50, 100, 150, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		[Category ("NotWorking")]
		public void StartCap_RoundAnchor() {
			p.StartCap = LineCap.RoundAnchor;
			p.Width = 25;
			t.Graphics.DrawLine (p, 50, 100, 150, 100);
			t.Graphics.DrawLine (Pens.White, 50, 100, 150, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		[Category ("NotWorking")]
		public void StartCap_SquareAnchor() {
			p.StartCap = LineCap.SquareAnchor;
			p.Width = 25;
			t.Graphics.DrawLine (p, 50, 100, 150, 100);
			t.Graphics.DrawLine (Pens.White, 50, 100, 150, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		public void StartCap_Triangle() {
			p.StartCap = LineCap.Triangle;
			p.Width = 25;
			t.Graphics.DrawLine (p, 50, 100, 150, 100);
			t.Graphics.DrawLine (Pens.White, 50, 100, 150, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		public void StartCap_Custom() {
			p.StartCap = LineCap.Custom;
			p.Width = 25;
			t.Graphics.DrawLine (p, 50, 100, 150, 100);
			t.Graphics.DrawLine (Pens.White, 50, 100, 150, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		#endregion

		#region Line EndCap
		[Test]
		public void EndCap_Flat() 
		{
			Assert.AreEqual(LineCap.Flat, p.EndCap);
			p.EndCap = LineCap.Flat;
			p.Width = 25;
			t.Graphics.DrawLine (p, 50, 100, 150, 100);
			t.Graphics.DrawLine (Pens.White, 50, 100, 150, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		[Category ("NotWorking")]
		public void EndCap_Round() 
		{
			p.EndCap = LineCap.Round;
			p.Width = 25;
			t.Graphics.DrawLine (p, 50, 100, 150, 100);
			t.Graphics.DrawLine (Pens.White, 50, 100, 150, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		[Category ("NotWorking")]
		public void EndCap_Square() {
			p.EndCap = LineCap.Square;
			p.Width = 25;
			t.Graphics.DrawLine (p, 50, 100, 150, 100);
			t.Graphics.DrawLine (Pens.White, 50, 100, 150, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		[Category ("NotWorking")]
		public void EndCap_AnchorMask() {
			p.EndCap = LineCap.AnchorMask;
			p.Width = 25;
			t.Graphics.DrawLine (p, 50, 100, 150, 100);
			t.Graphics.DrawLine (Pens.White, 50, 100, 150, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		[Category ("NotWorking")]
		public void EndCap_ArrowAnchor() {
			p.EndCap = LineCap.ArrowAnchor;
			p.Width = 25;
			t.Graphics.DrawLine (p, 50, 100, 150, 100);
			t.Graphics.DrawLine (Pens.White, 50, 100, 150, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		public void EndCap_DiamondAnchor() {
			p.EndCap = LineCap.DiamondAnchor;
			p.Width = 25;
			t.Graphics.DrawLine (p, 50, 100, 150, 100);
			t.Graphics.DrawLine (Pens.White, 50, 100, 150, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		[Category ("NotWorking")]
		public void EndCap_NoAnchor() {
			p.EndCap = LineCap.NoAnchor;
			p.Width = 25;
			t.Graphics.DrawLine (p, 50, 100, 150, 100);
			t.Graphics.DrawLine (Pens.White, 50, 100, 150, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		[Category ("NotWorking")]
		public void EndCap_RoundAnchor() {
			p.EndCap = LineCap.RoundAnchor;
			p.Width = 25;
			t.Graphics.DrawLine (p, 50, 100, 150, 100);
			t.Graphics.DrawLine (Pens.White, 50, 100, 150, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		public void EndCap_SquareAnchor() {
			p.EndCap = LineCap.SquareAnchor;
			p.Width = 25;
			t.Graphics.DrawLine (p, 50, 100, 150, 100);
			t.Graphics.DrawLine (Pens.White, 50, 100, 150, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		[Category ("NotWorking")]
		public void EndCap_Triangle() {
			p.EndCap = LineCap.Triangle;
			p.Width = 25;
			t.Graphics.DrawLine (p, 50, 100, 150, 100);
			t.Graphics.DrawLine (Pens.White, 50, 100, 150, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		[Category ("NotWorking")]
		public void EndCap_Custom() {
			p.EndCap = LineCap.Custom;
			p.Width = 25;
			t.Graphics.DrawLine (p, 50, 100, 150, 100);
			t.Graphics.DrawLine (Pens.White, 50, 100, 150, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		#endregion

		#region Basic LineCaps StartEnd
		[Test]
		public void StartEndCapBasic_Flat() {
			Assert.AreEqual(LineCap.Flat, p.StartCap);

			p.Width = 21;
			p.EndCap = LineCap.Flat;
			p.StartCap = LineCap.Flat;
			t.Graphics.DrawLine (p, 50, 100, 150, 100);
			t.Graphics.DrawLine (Pens.White, 50, 100, 150, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		public void StartEndCapBasic_Round() {
			p.Width = 21;
			p.EndCap = LineCap.Round;
			p.StartCap = LineCap.Round;
			t.Graphics.DrawLine (p, 50, 100, 150, 100);
			t.Graphics.DrawLine (Pens.White, 50, 100, 150, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		public void StartEndCapBasic_Square() {
			p.Width = 21;
			p.EndCap = LineCap.Square;
			p.StartCap = LineCap.Square;
			t.Graphics.DrawLine (p, 50, 100, 150, 100);
			t.Graphics.DrawLine (Pens.White, 50, 100, 150, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		public void SetLineCap_Flat() {
			Assert.AreEqual(LineCap.Flat, p.StartCap);

			p.Width = 21;
			p.SetLineCap(LineCap.Flat, LineCap.Flat, DashCap.Flat);
			t.Graphics.DrawLine (p, 50, 100, 150, 100);
			t.Graphics.DrawLine (Pens.White, 50, 100, 150, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		public void SetLineCap_Round() {
			p.Width = 21;
			p.SetLineCap(LineCap.Round, LineCap.Round, DashCap.Round);
			t.Graphics.DrawLine (p, 50, 100, 150, 100);
			t.Graphics.DrawLine (Pens.White, 50, 100, 150, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		[Test]
		public void SetLineCap_Square() {
			p.Width = 21;
			p.SetLineCap(LineCap.Square, LineCap.Square, DashCap.Flat);
			t.Graphics.DrawLine (p, 50, 100, 150, 100);
			t.Graphics.DrawLine (Pens.White, 50, 100, 150, 100);
			t.Show ();
			Assert.IsTrue (t.Compare (TOLERANCE));
		}
		#endregion
	}
}
