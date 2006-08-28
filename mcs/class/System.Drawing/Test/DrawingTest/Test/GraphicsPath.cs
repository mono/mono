using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using NUnit.Framework;
using DrawingTestHelper;

namespace Test.Sys.Drawing
{
	[TestFixture]
	public class GraphicsPathFixture
	{
		DrawingTest t;
		GraphicsPath path;
		Pen p;

		[SetUp]
		public void SetUp () {
			DrawingTest.ShowForms = false;
			t = DrawingTest.Create (512, 512);
			p = new Pen (Color.Blue);
			p.Width = 2;
		}

		[TearDown]
		public void TearDown ()
		{
			if (t != null)
				t.Dispose ();
		}

		[Test]
		public void ctor_void()
		{
			path = new GraphicsPath ();
			Assert.AreEqual (FillMode.Alternate, path.FillMode);
			Assert.AreEqual (0, path.PathData.Points.Length);
			Assert.AreEqual (0, path.PointCount);
		}

		[Test]
		public void ctor_FillMode()
		{
			path = new GraphicsPath (FillMode.Alternate);
			Assert.AreEqual (FillMode.Alternate, path.FillMode);
			Assert.AreEqual (0, path.PathData.Points.Length);
			Assert.AreEqual (0, path.PointCount);

			path = new GraphicsPath (FillMode.Winding);
			Assert.AreEqual (FillMode.Winding, path.FillMode);
			Assert.AreEqual (0, path.PathData.Points.Length);
			Assert.AreEqual (0, path.PointCount);
		}

		[Test]
		public void ctor_PointArr_ByteArr()
		{
			Point [] points = new Point [] {	new Point (0, 0), 
												new Point (250, 250), 
												new Point (60, 70),
												new Point (230, 10)};

			byte [] types = new byte [] {(byte) PathPointType.Start,
										(byte) PathPointType.Bezier3,
										(byte) PathPointType.Bezier3,
										(byte) (PathPointType.Bezier | PathPointType.CloseSubpath)};

			path = new GraphicsPath (points, types);

			Assert.AreEqual (FillMode.Alternate, path.FillMode);
			Assert.AreEqual (4, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF (0f, 0f), 
														new PointF (250f, 250f), 
														new PointF (60f, 70f),
														new PointF (230f, 10f)};
			for(int i = 0; i < path.PointCount; i++) {
				Assert.AreEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = types;
			for(int i = 0; i < path.PointCount; i++) {
				Assert.AreEqual(expectedTypes [i], path.PathTypes [i]);
			}

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void ctor_PointFArr_ByteArr()
		{
			PointF [] points = new PointF [] {	new PointF (100.1f, 200.2f), 
												new PointF (10.2f, 150.6f),
												new PointF (60.3f, 70.7f),
												new PointF (250.4f, 10.7f)};

			byte [] types = new byte [] {(byte) PathPointType.Start,
										(byte) PathPointType.Bezier3,
										(byte) PathPointType.Bezier3,
										(byte) (PathPointType.Bezier | PathPointType.CloseSubpath)};

			path = new GraphicsPath (points, types, FillMode.Alternate);

			Assert.AreEqual (FillMode.Alternate, path.FillMode);
			Assert.AreEqual (4, path.PointCount);

			PointF [] expectedPoints = points;
			for(int i = 0; i < path.PointCount; i++) {
				Assert.AreEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = types;
			for(int i = 0; i < path.PointCount; i++) {
				Assert.AreEqual(expectedTypes [i], path.PathTypes [i]);
			}

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void ctor_PointArr_ByteArr_FillMode()
		{
			Point [] points = new Point [] {new Point (0, 0), 
											new Point (250, 250), 
											new Point (60, 70),
											new Point (230, 10)};

			byte [] types = new byte [] {	(byte) PathPointType.Start,
											(byte) PathPointType.Bezier3,
											(byte) PathPointType.Bezier3,
											(byte) (PathPointType.Bezier | PathPointType.CloseSubpath)};

			path = new GraphicsPath (points, types, FillMode.Alternate);

			Assert.AreEqual (FillMode.Alternate, path.FillMode);
			Assert.AreEqual (4, path.PointCount);

			t.Graphics.DrawPath(p, path);
			t.Show();

			path = new GraphicsPath (points, types, FillMode.Winding);

			Assert.AreEqual (FillMode.Winding, path.FillMode);
			Assert.AreEqual (4, path.PointCount);

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void ctor_PointFArr_ByteArr_FillMode()
		{
			PointF [] points = new PointF [] {	new PointF (100.1f, 200.2f), 
												new PointF (10.2f, 150.6f),
												new PointF (60.3f, 70.7f),
												new PointF (250.4f, 10.7f)};

			byte [] types = new byte [] {	(byte) PathPointType.Start,
											(byte) PathPointType.Bezier3,
											(byte) PathPointType.Bezier3,
											(byte) (PathPointType.Bezier | PathPointType.CloseSubpath)};

			path = new GraphicsPath (points, types, FillMode.Alternate);

			Assert.AreEqual (FillMode.Alternate, path.FillMode);
			Assert.AreEqual (4, path.PointCount);

			t.Graphics.DrawPath (p, path);
			t.Show();

			path = new GraphicsPath (points, types, FillMode.Winding);

			Assert.AreEqual (FillMode.Winding, path.FillMode);
			Assert.AreEqual (4, path.PointCount);

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		[Category ("NotWorking")]
		public void AddArc_Rectangle_Float_Float()
		{
			path = new GraphicsPath ();

			path.AddArc (new Rectangle (50, 50, 150, 170), 10.34f, 240.15f);

			Assert.AreEqual (10, path.PointCount);

			path.AddArc (new Rectangle (50, 50, 70, 95), -45.001f, 135.87f);

			Assert.AreEqual (17, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF(199.0466f, 148.5099f), 
														new PointF(192.4631f, 194.8574f), 
														new PointF(153.9743f, 226.3808f), 
														new PointF(113.0795f, 218.9195f), 
														new PointF(72.18465f, 211.4582f), 
														new PointF(44.36986f, 167.8375f), 
														new PointF(50.95338f, 121.4901f), 
														new PointF(55.13617f, 92.0436f), 
														new PointF(72.63087f, 67.23608f), 
														new PointF(97.05219f, 56.12194f), 
														new PointF(113.1766f, 69.32237f), 
														new PointF(124.6434f, 90.44156f), 
														new PointF(121.324f, 120.1776f), 
														new PointF(105.7625f, 135.7396f), 
														new PointF(99.54897f, 141.9534f), 
														new PointF(91.99629f, 145.2055f), 
														new PointF(84.27972f, 144.9899f)};
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [17] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3 }; 

			for (int i = 0; i < path.PointCount; i++) {
				Assert.AreEqual(expectedTypes [i], path.PathTypes [i]);
			}

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}


		[Test]
		[Category ("NotWorking")]
		public void AddArc_RectangleF_Float_Float()
		{
			path = new GraphicsPath ();

			path.AddArc (new RectangleF (20.02f, 30.56f, 150.67f, 170.34f), 10.34f, 240.15f);

			Assert.AreEqual (10, path.PointCount);

			path.AddArc (new RectangleF (50.09f, 50.345f, 70.15f, 95.98f), -45.001f, 135.87f);

			Assert.AreEqual (17, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF(169.7277f, 129.2994f), 
														new PointF(163.0989f, 175.7367f), 
														new PointF(124.4274f, 207.3063f), 
														new PointF(83.3525f, 199.8121f), 
														new PointF(42.27758f, 192.3179f), 
														new PointF(14.35347f, 148.5978f), 
														new PointF(20.98226f, 102.1606f), 
														new PointF(25.1958f, 72.64315f), 
														new PointF(42.79146f, 47.78527f), 
														new PointF(67.34177f, 36.66724f), 
														new PointF(113.4824f, 70.01659f), 
														new PointF(124.9132f, 91.4144f), 
														new PointF(121.5016f, 121.4393f), 
														new PointF(105.8624f, 137.0791f), 
														new PointF(99.65379f, 143.288f), 
														new PointF(92.12586f, 146.533f), 
														new PointF(84.43728f, 146.3147f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier }; 

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}			

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		[Category ("NotWorking")]
		public void AddArc_Int_Int_Int_Int_Float_Float()
		{
			path = new GraphicsPath ();

			path.AddArc (50, 50, 150, 170, 10.34f, 240.15f);

			Assert.AreEqual (10, path.PointCount);

			path.AddArc (50, 50, 70, 95, -45.001f, 135.87f);

			Assert.AreEqual (17, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF(199.0466f, 148.5099f), 
														new PointF(192.4631f, 194.8574f), 
														new PointF(153.9743f, 226.3808f), 
														new PointF(113.0795f, 218.9195f), 
														new PointF(72.18465f, 211.4582f), 
														new PointF(44.36986f, 167.8375f), 
														new PointF(50.95338f, 121.4901f), 
														new PointF(55.13617f, 92.0436f), 
														new PointF(72.63087f, 67.23608f), 
														new PointF(97.05219f, 56.12194f), 
														new PointF(113.1766f, 69.32237f), 
														new PointF(124.6434f, 90.44156f), 
														new PointF(121.324f, 120.1776f), 
														new PointF(105.7625f, 135.7396f), 
														new PointF(99.54897f, 141.9534f), 
														new PointF(91.99629f, 145.2055f), 
														new PointF(84.27972f, 144.9899f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}			

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}


		[Test]
		[Category ("NotWorking")]
		public void AddArc_Float_Float_Float_Float_Float_Float()
		{
			path = new GraphicsPath ();

			path.AddArc (20.02f, 30.56f, 150.67f, 170.34f, 10.34f, 240.15f);

			Assert.AreEqual (10, path.PointCount);

			path.AddArc (50.09f, 50.345f, 70.15f, 95.98f, -45.001f, 135.87f);

			Assert.AreEqual (17, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF(169.7277f, 129.2994f), 
														new PointF(163.0989f, 175.7367f), 
														new PointF(124.4274f, 207.3063f), 
														new PointF(83.3525f, 199.8121f), 
														new PointF(42.27758f, 192.3179f), 
														new PointF(14.35347f, 148.5978f), 
														new PointF(20.98226f, 102.1606f), 
														new PointF(25.1958f, 72.64315f), 
														new PointF(42.79146f, 47.78527f), 
														new PointF(67.34177f, 36.66724f), 
														new PointF(113.4824f, 70.01659f), 
														new PointF(124.9132f, 91.4144f), 
														new PointF(121.5016f, 121.4393f), 
														new PointF(105.8624f, 137.0791f), 
														new PointF(99.65379f, 143.288f), 
														new PointF(92.12586f, 146.533f), 
														new PointF(84.43728f, 146.3147f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}			

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void AddBezier_Point_Point_Point_Point()
		{
			path = new GraphicsPath ();
			path.AddBezier( new Point (10, 10),
							new Point (50, 250),
							new Point (100, 5),
							new Point (200, 280));

			Assert.AreEqual (4, path.PointCount);

			path.AddBezier( new Point (0, 210),
							new Point (50, 6),
							new Point (150, 150),
							new Point (250, 10));

			Assert.AreEqual (8, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF(10f, 10f), 
														new PointF(50f, 250f), 
														new PointF(100f, 5f), 
														new PointF(200f, 280f), 
														new PointF(0f, 210f), 
														new PointF(50f, 6f), 
														new PointF(150f, 150f), 
														new PointF(250f, 10f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}						

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void AddBezier_PointF_PointF_PointF_PointF()
		{
			path = new GraphicsPath ();
			path.AddBezier( new PointF (10.01f, 10.02f),
							new PointF (50.3f, 250.4f),
							new PointF (100.005f, 5.006f),
							new PointF (200.78f, 280.90f));

			Assert.AreEqual (4, path.PointCount);

			path.AddBezier( new PointF (0.15f, 210.23f),
							new PointF (50.34f, 6.45f),
							new PointF (150.65f, 150.87f),
							new PointF (250.0001f, 10.2345f));

			Assert.AreEqual (8, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF (10.01f, 10.02f),
														new PointF (50.3f, 250.4f),
														new PointF (100.005f, 5.006f),
														new PointF (200.78f, 280.90f), 
														new PointF (0.15f, 210.23f),
														new PointF (50.34f, 6.45f),
														new PointF (150.65f, 150.87f),
														new PointF (250.0001f, 10.2345f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void AddBezier_Int_Int_Int_Int_Int_Int_Int_Int()
		{
			path = new GraphicsPath ();
			path.AddBezier( 10, 10, 50, 250, 100, 5, 200, 280);

			Assert.AreEqual (4, path.PointCount);

			path.AddBezier( 0, 210, 50, 6, 150, 150, 250, 10);

			Assert.AreEqual (8, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF(10f, 10f), 
														new PointF(50f, 250f), 
														new PointF(100f, 5f), 
														new PointF(200f, 280f), 
														new PointF(0f, 210f), 
														new PointF(50f, 6f), 
														new PointF(150f, 150f), 
														new PointF(250f, 10f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}							

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void AddBezier_Float_Float_Float_Float_Float_Float_Float_Float()
		{
			path = new GraphicsPath ();
			path.AddBezier( 10.01f, 10.02f, 50.3f, 250.4f, 100.005f, 5.006f, 200.78f, 280.90f);

			Assert.AreEqual (4, path.PointCount);

			path.AddBezier( 0.15f, 210.23f, 50.34f, 6.45f, 150.65f, 150.87f, 250.0001f, 10.2345f);

			Assert.AreEqual (8, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF (10.01f, 10.02f),
														new PointF (50.3f, 250.4f),
														new PointF (100.005f, 5.006f),
														new PointF (200.78f, 280.90f), 
														new PointF (0.15f, 210.23f),
														new PointF (50.34f, 6.45f),
														new PointF (150.65f, 150.87f),
														new PointF (250.0001f, 10.2345f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void AddBeziers_PointArr()
		{
			Point [] points = new Point [] {new Point(20, 100),
											new Point(40, 75),
											new Point(60, 125),
											new Point(80, 100),
											new Point(100, 50),
											new Point(120, 150),
											new Point(140, 100)};

			path = new GraphicsPath();
			path.AddBeziers(points);

			Assert.AreEqual (7, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF(20f, 100f), 
														new PointF(40f, 75f), 
														new PointF(60f, 125f), 
														new PointF(80f, 100f), 
														new PointF(100f, 50f), 
														new PointF(120f, 150f), 
														new PointF(140f, 100f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}							

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void AddBeziers_PointFArr()
		{
			PointF [] points = new PointF [] {	new PointF(20.01f, 100.1f),
												new PointF(40.02f, 75.2f),
												new PointF(60.03f, 125.3f),
												new PointF(80.04f, 100.4f),
												new PointF(100.05f, 50.5f),
												new PointF(120.06f, 150.6f),
												new PointF(140.07f, 100.7f)};

			path = new GraphicsPath();
			path.AddBeziers(points);

			Assert.AreEqual (7, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF(20.01f, 100.1f),
														new PointF(40.02f, 75.2f),
														new PointF(60.03f, 125.3f),
														new PointF(80.04f, 100.4f),
														new PointF(100.05f, 50.5f),
														new PointF(120.06f, 150.6f),
														new PointF(140.07f, 100.7f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}								

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void AddClosedCurve_PointArr()
		{
			Point [] points = new Point [] {new Point(20, 100),
											new Point(40, 75),
											new Point(60, 125),
											new Point(80, 100),
											new Point(100, 50),
											new Point(120, 150),
											new Point(140, 100)};

			path = new GraphicsPath();
			path.AddClosedCurve(points);

			Assert.AreEqual (22, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF(20f, 100f), 
														new PointF(3.333333f, 95.83333f), 
														new PointF(33.33333f, 70.83333f), 
														new PointF(40f, 75f), 
														new PointF(46.66666f, 79.16666f), 
														new PointF(53.33333f, 120.8333f), 
														new PointF(60f, 125f), 
														new PointF(66.66666f, 129.1667f), 
														new PointF(73.33333f, 112.5f), 
														new PointF(80f, 100f), 
														new PointF(86.66666f, 87.49999f), 
														new PointF(93.33333f, 41.66666f), 
														new PointF(100f, 50f), 
														new PointF(106.6667f, 58.33333f), 
														new PointF(113.3333f, 141.6667f), 
														new PointF(120f, 150f), 
														new PointF(126.6667f, 158.3333f), 
														new PointF(156.6667f, 108.3333f), 
														new PointF(140f, 100f), 
														new PointF(123.3333f, 91.66666f), 
														new PointF(36.66666f, 104.1667f), 
														new PointF(20f, 100f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) (PathPointType.Bezier | PathPointType.CloseSubpath)};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void AddClosedCurve_PointFArr()
		{
			PointF [] points = new PointF [] {	new PointF(20.01f, 100.1f),
												new PointF(40.02f, 75.2f),
												new PointF(60.03f, 125.3f),
												new PointF(80.04f, 100.4f),
												new PointF(100.05f, 50.5f),
												new PointF(120.06f, 150.6f),
												new PointF(140.07f, 100.7f)};

			path = new GraphicsPath();
			path.AddClosedCurve(points);

			Assert.AreEqual (22, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF(20.01f, 100.1f), 
														new PointF(3.334998f, 95.84999f), 
														new PointF(33.35f, 70.99999f), 
														new PointF(40.02f, 75.2f), 
														new PointF(46.69f, 79.39999f), 
														new PointF(53.36f, 121.1f), 
														new PointF(60.03f, 125.3f), 
														new PointF(66.7f, 129.5f), 
														new PointF(73.37f, 112.8667f), 
														new PointF(80.04f, 100.4f), 
														new PointF(86.71f, 87.93333f), 
														new PointF(93.38f, 42.13333f), 
														new PointF(100.05f, 50.5f), 
														new PointF(106.72f, 58.86666f), 
														new PointF(113.39f, 142.2333f), 
														new PointF(120.06f, 150.6f), 
														new PointF(126.73f, 158.9667f), 
														new PointF(156.745f, 109.1167f), 
														new PointF(140.07f, 100.7f), 
														new PointF(123.395f, 92.28333f), 
														new PointF(36.685f, 104.35f), 
														new PointF(20.01f, 100.1f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) (PathPointType.Bezier | PathPointType.CloseSubpath)};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void AddClosedCurve_PointArr_Float()
		{
			Point [] points = new Point [] {new Point(20, 100),
											new Point(40, 75),
											new Point(60, 125),
											new Point(80, 100),
											new Point(100, 50),
											new Point(120, 150),
											new Point(140, 100)};

			path = new GraphicsPath();
			path.AddClosedCurve(points, 0.9f);

			Assert.AreEqual (22, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF(20f, 100f), 
														new PointF(-10f, 92.49999f), 
														new PointF(28f, 67.49999f), 
														new PointF(40f, 75f), 
														new PointF(52f, 82.5f), 
														new PointF(48f, 117.5f), 
														new PointF(60f, 125f), 
														new PointF(72f, 132.5f), 
														new PointF(67.99999f, 122.5f), 
														new PointF(80f, 100f), 
														new PointF(92f, 77.49999f), 
														new PointF(87.99999f, 35f), 
														new PointF(100f, 50f), 
														new PointF(112f, 65f), 
														new PointF(108f, 135f), 
														new PointF(120f, 150f), 
														new PointF(132f, 165f), 
														new PointF(170f, 115f), 
														new PointF(140f, 100f), 
														new PointF(110f, 84.99999f), 
														new PointF(50f, 107.5f), 
														new PointF(20f, 100f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) (PathPointType.Bezier | PathPointType.CloseSubpath)};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void AddClosedCurve_PointFArr_Float()
		{
			PointF [] points = new PointF [] {	new PointF(20.01f, 100.1f),
												new PointF(40.02f, 75.2f),
												new PointF(60.03f, 125.3f),
												new PointF(80.04f, 100.4f),
												new PointF(100.05f, 50.5f),
												new PointF(120.06f, 150.6f),
												new PointF(140.07f, 100.7f)};

			path = new GraphicsPath();
			path.AddClosedCurve(points, 0.8f);

			Assert.AreEqual (22, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF(20.01f, 100.1f), 
														new PointF(-6.670003f, 93.3f), 
														new PointF(29.348f, 68.47999f), 
														new PointF(40.02f, 75.2f), 
														new PointF(50.692f, 81.92f), 
														new PointF(49.358f, 118.58f), 
														new PointF(60.03f, 125.3f), 
														new PointF(70.702f, 132.02f), 
														new PointF(69.368f, 120.3467f), 
														new PointF(80.04f, 100.4f), 
														new PointF(90.712f, 80.45333f), 
														new PointF(89.378f, 37.11333f), 
														new PointF(100.05f, 50.5f), 
														new PointF(110.722f, 63.88667f), 
														new PointF(109.388f, 137.2133f), 
														new PointF(120.06f, 150.6f), 
														new PointF(130.732f, 163.9867f), 
														new PointF(166.75f, 114.1667f), 
														new PointF(140.07f, 100.7f), 
														new PointF(113.39f, 87.23332f), 
														new PointF(46.69f, 106.9f), 
														new PointF(20.01f, 100.1f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) (PathPointType.Bezier | PathPointType.CloseSubpath)};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void AddCurve_PointArr()
		{
			Point [] points = new Point [] {new Point(20, 100),
											new Point(40, 75),
											new Point(60, 125),
											new Point(80, 100),
											new Point(100, 50),
											new Point(120, 150),
											new Point(140, 100)};

			path = new GraphicsPath();
			path.AddCurve(points);

			Assert.AreEqual (19, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF(20f, 100f), 
														new PointF(23.33333f, 95.83333f), 
														new PointF(33.33333f, 70.83333f), 
														new PointF(40f, 75f), 
														new PointF(46.66666f, 79.16666f), 
														new PointF(53.33333f, 120.8333f), 
														new PointF(60f, 125f), 
														new PointF(66.66666f, 129.1667f), 
														new PointF(73.33333f, 112.5f), 
														new PointF(80f, 100f), 
														new PointF(86.66666f, 87.49999f), 
														new PointF(93.33333f, 41.66666f), 
														new PointF(100f, 50f), 
														new PointF(106.6667f, 58.33333f), 
														new PointF(113.3333f, 141.6667f), 
														new PointF(120f, 150f), 
														new PointF(126.6667f, 158.3333f), 
														new PointF(136.6667f, 108.3333f), 
														new PointF(140f, 100f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void AddCurve_PointFArr()
		{
			PointF [] points = new PointF [] {	new PointF(20.01f, 100.1f),
												new PointF(40.02f, 75.2f),
												new PointF(60.03f, 125.3f),
												new PointF(80.04f, 100.4f),
												new PointF(100.05f, 50.5f),
												new PointF(120.06f, 150.6f),
												new PointF(140.07f, 100.7f)};

			path = new GraphicsPath();
			path.AddCurve(points);

			Assert.AreEqual (19, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF(20.01f, 100.1f), 
														new PointF(23.345f, 95.95f), 
														new PointF(33.35f, 70.99999f), 
														new PointF(40.02f, 75.2f), 
														new PointF(46.69f, 79.39999f), 
														new PointF(53.36f, 121.1f), 
														new PointF(60.03f, 125.3f), 
														new PointF(66.7f, 129.5f), 
														new PointF(73.37f, 112.8667f), 
														new PointF(80.04f, 100.4f), 
														new PointF(86.71f, 87.93333f), 
														new PointF(93.38f, 42.13333f), 
														new PointF(100.05f, 50.5f), 
														new PointF(106.72f, 58.86666f), 
														new PointF(113.39f, 142.2333f), 
														new PointF(120.06f, 150.6f), 
														new PointF(126.73f, 158.9667f), 
														new PointF(136.735f, 109.0167f), 
														new PointF(140.07f, 100.7f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void AddCurve_PointArr_Float()
		{
			Point [] points = new Point [] {new Point(20, 100),
											new Point(40, 75),
											new Point(60, 125),
											new Point(80, 100),
											new Point(100, 50),
											new Point(120, 150),
											new Point(140, 100)};

			path = new GraphicsPath();
			path.AddCurve(points, 0.9f);

			Assert.AreEqual (19, path.PointCount);
			
			PointF [] expectedPoints = new PointF [] {	new PointF(20f, 100f), 
														new PointF(26f, 92.49999f), 
														new PointF(28f, 67.49999f), 
														new PointF(40f, 75f), 
														new PointF(52f, 82.5f), 
														new PointF(48f, 117.5f), 
														new PointF(60f, 125f), 
														new PointF(72f, 132.5f), 
														new PointF(67.99999f, 122.5f), 
														new PointF(80f, 100f), 
														new PointF(92f, 77.49999f), 
														new PointF(87.99999f, 35f), 
														new PointF(100f, 50f), 
														new PointF(112f, 65f), 
														new PointF(108f, 135f), 
														new PointF(120f, 150f), 
														new PointF(132f, 165f), 
														new PointF(134f, 115f), 
														new PointF(140f, 100f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void AddCurve_PointFArr_Float()
		{
			PointF [] points = new PointF [] {	new PointF(20.01f, 100.1f),
												new PointF(40.02f, 75.2f),
												new PointF(60.03f, 125.3f),
												new PointF(80.04f, 100.4f),
												new PointF(100.05f, 50.5f),
												new PointF(120.06f, 150.6f),
												new PointF(140.07f, 100.7f)};

			path = new GraphicsPath();
			path.AddCurve(points, 0.8f);

			Assert.AreEqual (19, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF(20.01f, 100.1f), 
														new PointF(25.346f, 93.45999f), 
														new PointF(29.348f, 68.47999f), 
														new PointF(40.02f, 75.2f), 
														new PointF(50.692f, 81.92f), 
														new PointF(49.358f, 118.58f), 
														new PointF(60.03f, 125.3f), 
														new PointF(70.702f, 132.02f), 
														new PointF(69.368f, 120.3467f), 
														new PointF(80.04f, 100.4f), 
														new PointF(90.712f, 80.45333f), 
														new PointF(89.378f, 37.11333f), 
														new PointF(100.05f, 50.5f), 
														new PointF(110.722f, 63.88667f), 
														new PointF(109.388f, 137.2133f), 
														new PointF(120.06f, 150.6f), 
														new PointF(130.732f, 163.9867f), 
														new PointF(134.734f, 114.0067f), 
														new PointF(140.07f, 100.7f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void AddCurve_PointArr_Int_Int_Float()
		{

			Point [] points = new Point [] {new Point(20, 100),
											new Point(40, 75),
											new Point(60, 125),
											new Point(80, 100),
											new Point(100, 50),
											new Point(120, 150),
											new Point(140, 100)};

			path = new GraphicsPath();
			path.AddCurve(points, 0, 3, 0.8f);

			Assert.AreEqual (10, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF(20f, 100f), 
														new PointF(25.33333f, 93.33333f), 
														new PointF(29.33333f, 68.33333f), 
														new PointF(40f, 75f), 
														new PointF(50.66666f, 81.66666f), 
														new PointF(49.33333f, 118.3333f), 
														new PointF(60f, 125f), 
														new PointF(70.66666f, 131.6667f), 
														new PointF(69.33333f, 120f), 
														new PointF(80f, 100f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void AddCurve_PointFArr_Int_Int_Float()
		{

			PointF [] points = new PointF [] {	new PointF(20.01f, 100.1f),
												new PointF(40.02f, 75.2f),
												new PointF(60.03f, 125.3f),
												new PointF(80.04f, 100.4f),
												new PointF(100.05f, 50.5f),
												new PointF(120.06f, 150.6f),
												new PointF(140.07f, 100.7f)};

			path = new GraphicsPath();
			path.AddCurve(points, 0, 3, 0.8f);

			Assert.AreEqual (10, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF(20.01f, 100.1f), 
														new PointF(25.346f, 93.45999f), 
														new PointF(29.348f, 68.47999f), 
														new PointF(40.02f, 75.2f), 
														new PointF(50.692f, 81.92f), 
														new PointF(49.358f, 118.58f), 
														new PointF(60.03f, 125.3f), 
														new PointF(70.702f, 132.02f), 
														new PointF(69.368f, 120.3467f), 
														new PointF(80.04f, 100.4f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void AddEllipse_Rectangle()
		{
			path = new GraphicsPath();
			path.AddEllipse (new Rectangle(50, 50, 400, 80));

			Assert.AreEqual (13, path.PointCount);
			
			PointF [] expectedPoints = new PointF [] {	new PointF(450f, 90f), 
														new PointF(450f, 112.0914f), 
														new PointF(360.4569f, 130f), 
														new PointF(250f, 130f), 
														new PointF(139.543f, 130f), 
														new PointF(50f, 112.0914f), 
														new PointF(50f, 90f), 
														new PointF(50f, 67.90861f), 
														new PointF(139.543f, 50f), 
														new PointF(250f, 50f), 
														new PointF(360.4569f, 50f), 
														new PointF(450f, 67.90861f), 
														new PointF(450f, 90f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) (PathPointType.Bezier | PathPointType.CloseSubpath)};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void AddEllipse_RectangleF()
		{
			path = new GraphicsPath();
			path.AddEllipse (new RectangleF(50.1f, 50.4f, 400.12f, 80.123f));

			Assert.AreEqual (13, path.PointCount);
			
			PointF [] expectedPoints = new PointF [] {	new PointF(450.22f, 90.4615f), 
														new PointF(450.22f, 112.5869f), 
														new PointF(360.6501f, 130.523f), 
														new PointF(250.16f, 130.523f), 
														new PointF(139.6699f, 130.523f), 
														new PointF(50.09999f, 112.5869f), 
														new PointF(50.09999f, 90.4615f), 
														new PointF(50.09999f, 68.33614f), 
														new PointF(139.6699f, 50.4f), 
														new PointF(250.16f, 50.4f), 
														new PointF(360.6501f, 50.4f), 
														new PointF(450.22f, 68.33614f), 
														new PointF(450.22f, 90.4615f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) (PathPointType.Bezier | PathPointType.CloseSubpath)};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void AddEllipse_Int_Int_Int_Int()
		{
			path = new GraphicsPath();
			path.AddEllipse (50, 50, 400, 80);

			Assert.AreEqual (13, path.PointCount);
			
			PointF [] expectedPoints = new PointF [] {	new PointF(450f, 90f), 
														new PointF(450f, 112.0914f), 
														new PointF(360.4569f, 130f), 
														new PointF(250f, 130f), 
														new PointF(139.543f, 130f), 
														new PointF(50f, 112.0914f), 
														new PointF(50f, 90f), 
														new PointF(50f, 67.90861f), 
														new PointF(139.543f, 50f), 
														new PointF(250f, 50f), 
														new PointF(360.4569f, 50f), 
														new PointF(450f, 67.90861f), 
														new PointF(450f, 90f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) (PathPointType.Bezier | PathPointType.CloseSubpath)};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void AddEllipse_Float_Float_Float_Float()
		{
			path = new GraphicsPath();
			path.AddEllipse (50.1f, 50.4f, 400.12f, 80.123f);

			Assert.AreEqual (13, path.PointCount);
			
			PointF [] expectedPoints = new PointF [] {	new PointF(450.22f, 90.4615f), 
														new PointF(450.22f, 112.5869f), 
														new PointF(360.6501f, 130.523f), 
														new PointF(250.16f, 130.523f), 
														new PointF(139.6699f, 130.523f), 
														new PointF(50.09999f, 112.5869f), 
														new PointF(50.09999f, 90.4615f), 
														new PointF(50.09999f, 68.33614f), 
														new PointF(139.6699f, 50.4f), 
														new PointF(250.16f, 50.4f), 
														new PointF(360.6501f, 50.4f), 
														new PointF(450.22f, 68.33614f), 
														new PointF(450.22f, 90.4615f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) (PathPointType.Bezier | PathPointType.CloseSubpath)};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}


		[Test]
		public void AddLine_Point_Point()
		{
			path = new GraphicsPath ();
			
			path.AddLine (new Point (20, 20), new Point (10, 120));
			Assert.AreEqual (2, path.PointCount);

			path.AddLine (new Point (40, 320), new Point (310, 45));
			Assert.AreEqual (4, path.PointCount);

			path.AddLine (new Point (300, 300), new Point (48, 62));
			Assert.AreEqual (6, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF(20f, 20f), 
														new PointF(10f, 120f), 
														new PointF(40f, 320f), 
														new PointF(310f, 45f), 
														new PointF(300f, 300f), 
														new PointF(48f, 62f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void AddLine_PointF_PointF()
		{
			path = new GraphicsPath ();
			
			path.AddLine (new PointF (20.02f, 20.123f), new PointF (10.0001f, 120.23f));
			Assert.AreEqual (2, path.PointCount);

			path.AddLine (new PointF (40.00f, 320.234f), new PointF (310.9999f, 45.33333333f));
			Assert.AreEqual (4, path.PointCount);

			path.AddLine (new PointF (300f, 300.97f), new PointF (48.18f, 62.54f));
			Assert.AreEqual (6, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF(20.02f, 20.123f), 
														new PointF(10.0001f, 120.23f), 
														new PointF(40f, 320.234f), 
														new PointF(310.9999f, 45.33333f), 
														new PointF(300f, 300.97f), 
														new PointF(48.18f, 62.54f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void AddLine_Int_Int_Int_Int()
		{
			path = new GraphicsPath ();
			
			path.AddLine (20, 20, 10, 120);
			Assert.AreEqual (2, path.PointCount);

			path.AddLine (40, 320, 310, 45);
			Assert.AreEqual (4, path.PointCount);

			path.AddLine (300, 300, 48, 62);
			Assert.AreEqual (6, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF(20f, 20f), 
														new PointF(10f, 120f), 
														new PointF(40f, 320f), 
														new PointF(310f, 45f), 
														new PointF(300f, 300f), 
														new PointF(48f, 62f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void AddLine_Float_Float_Float_Float()
		{
			path = new GraphicsPath ();
			
			path.AddLine (20.02f, 20.123f, 10.0001f, 120.23f);
			Assert.AreEqual (2, path.PointCount);

			path.AddLine (40.00f, 320.234f, 310.9999f, 45.33333333f);
			Assert.AreEqual (4, path.PointCount);

			path.AddLine (300f, 300.97f, 48.18f, 62.54f);
			Assert.AreEqual (6, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF(20.02f, 20.123f), 
														new PointF(10.0001f, 120.23f), 
														new PointF(40f, 320.234f), 
														new PointF(310.9999f, 45.33333f), 
														new PointF(300f, 300.97f), 
														new PointF(48.18f, 62.54f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void AddLines_PointArr()
		{
			Point [] points = new Point [] {new Point(20, 100),
											new Point(40, 75),
											new Point(60, 125),
											new Point(80, 100),
											new Point(100, 50),
											new Point(120, 150),
											new Point(140, 100)};

			path = new GraphicsPath();
			path.AddLines(points);

			Assert.AreEqual (7, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF(20f, 100f), 
														new PointF(40f, 75f), 
														new PointF(60f, 125f), 
														new PointF(80f, 100f), 
														new PointF(100f, 50f), 
														new PointF(120f, 150f), 
														new PointF(140f, 100f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}							

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void AddLines_PointFArr()
		{
			PointF [] points = new PointF [] {	new PointF(20.01f, 100.1f),
												new PointF(40.02f, 75.2f),
												new PointF(60.03f, 125.3f),
												new PointF(80.04f, 100.4f),
												new PointF(100.05f, 50.5f),
												new PointF(120.06f, 150.6f),
												new PointF(140.07f, 100.7f)};

			path = new GraphicsPath();
			path.AddLines(points);

			Assert.AreEqual (7, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF(20.01f, 100.1f),
														new PointF(40.02f, 75.2f),
														new PointF(60.03f, 125.3f),
														new PointF(80.04f, 100.4f),
														new PointF(100.05f, 50.5f),
														new PointF(120.06f, 150.6f),
														new PointF(140.07f, 100.7f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}								

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void AddPath_GraphicsPath_Bool_1()
		{
			Point [] points1 = new Point [] {	new Point (302, 302),
												new Point (360, 360),
												new Point (0, 460),
												new Point (130, 230)};

			GraphicsPath path1 = new GraphicsPath ();
			path1.AddLines (points1);

			Point [] points2 = {	new Point (350, 350),
									new Point (0, 0),
									new Point (260, 100),
									new Point (310, 30)};

			path = new GraphicsPath ();
			path.AddLines (points2);

			path.AddPath (path1, true);

			Assert.AreEqual (8, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF(350f, 350f), 
														new PointF(0f, 0f), 
														new PointF(260f, 100f), 
														new PointF(310f, 30f), 
														new PointF(302f, 302f), 
														new PointF(360f, 360f), 
														new PointF(0f, 460f), 
														new PointF(130f, 230f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}
	
			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void AddPath_GraphicsPath_Bool_2()
		{
			Point [] points1 = new Point [] {	new Point (302, 302),
												new Point (360, 360),
												new Point (0, 460),
												new Point (130, 230)};

			GraphicsPath path1 = new GraphicsPath ();
			path1.AddLines (points1);

			Point [] points2 = {	new Point (350, 350),
									new Point (0, 0),
									new Point (260, 100),
									new Point (310, 30)};

			path = new GraphicsPath ();
			path.AddLines (points2);

			path.AddPath (path1, false);

			Assert.AreEqual (8, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF(350f, 350f), 
														new PointF(0f, 0f), 
														new PointF(260f, 100f), 
														new PointF(310f, 30f), 
														new PointF(302f, 302f), 
														new PointF(360f, 360f), 
														new PointF(0f, 460f), 
														new PointF(130f, 230f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}
	
			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		[Category ("NotWorking")]
		public void AddPie_Rectangle_Float_Float()
		{
			path = new GraphicsPath ();

			path.AddPie (new Rectangle (20, 30, 350, 370), 10.34f, 240.15f);

			Assert.AreEqual (11, path.PointCount);

			path.AddPie (new Rectangle (150, 150, 170, 35), -45.001f, 135.87f);

			Assert.AreEqual (19, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF(195f, 215f), 
														new PointF(367.4504f, 246.4639f), 
														new PointF(351.0127f, 347.148f), 
														new PointF(260.4786f, 414.6818f), 
														new PointF(165.2368f, 397.3047f), 
														new PointF(69.99509f, 379.9277f), 
														new PointF(6.111823f, 284.2202f), 
														new PointF(22.54954f, 183.5361f), 
														new PointF(33.12234f, 118.7757f), 
														new PointF(75.40034f, 64.80574f), 
														new PointF(133.6162f, 41.75421f), 
														new PointF(235f, 167.5f), 
														new PointF(252.1399f, 150.3595f), 
														new PointF(298.1198f, 152.3084f), 
														new PointF(327.72f, 161.5623f), 
														new PointF(318.254f, 171.0288f), 
														new PointF(310.1f, 179.1831f), 
														new PointF(275.1718f, 185.0259f), 
														new PointF(234.7346f, 184.9999f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) (PathPointType.Bezier3 | PathPointType.CloseSubpath),
													(byte) PathPointType.Start,
													(byte) PathPointType.Line, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) (PathPointType.Bezier3 | PathPointType.CloseSubpath)}; 

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}			

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		[Category ("NotWorking")]
		public void AddPie_Int_Int_Int_Int_Float_Float()
		{
			path = new GraphicsPath ();

			path.AddPie (20, 30, 350, 370, 10.34f, 240.15f);

			Assert.AreEqual (11, path.PointCount);

			path.AddPie (150, 150, 170, 35, -45.001f, 135.87f);

			Assert.AreEqual (19, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF(195f, 215f), 
														new PointF(367.4504f, 246.4639f), 
														new PointF(351.0127f, 347.148f), 
														new PointF(260.4786f, 414.6818f), 
														new PointF(165.2368f, 397.3047f), 
														new PointF(69.99509f, 379.9277f), 
														new PointF(6.111823f, 284.2202f), 
														new PointF(22.54954f, 183.5361f), 
														new PointF(33.12234f, 118.7757f), 
														new PointF(75.40034f, 64.80574f), 
														new PointF(133.6162f, 41.75421f), 
														new PointF(235f, 167.5f), 
														new PointF(252.1399f, 150.3595f), 
														new PointF(298.1198f, 152.3084f), 
														new PointF(327.72f, 161.5623f), 
														new PointF(318.254f, 171.0288f), 
														new PointF(310.1f, 179.1831f), 
														new PointF(275.1718f, 185.0259f), 
														new PointF(234.7346f, 184.9999f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) (PathPointType.Bezier3 | PathPointType.CloseSubpath),
													(byte) PathPointType.Start,
													(byte) PathPointType.Line, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) (PathPointType.Bezier3 | PathPointType.CloseSubpath)}; 

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}			

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		[Category ("NotWorking")]
		public void AddPie_Float_Float_Float_Float_Float_Float()
		{
			path = new GraphicsPath ();

			path.AddPie (20f, 30.01f, 350.34f, 370.56f, 10.34f, 240.15f);

			Assert.AreEqual (11, path.PointCount);

			path.AddPie (150.23f, 150.12f, 170.99f, 35.098f, -45.001f, 135.87f);

			Assert.AreEqual (19, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF(195.17f, 215.29f), 
														new PointF(367.7906f, 246.785f), 
														new PointF(351.3456f, 347.623f), 
														new PointF(260.7293f, 415.2677f), 
														new PointF(165.3936f, 397.8735f), 
														new PointF(70.05784f, 380.4793f), 
														new PointF(6.104279f, 284.6331f), 
														new PointF(22.54932f, 183.7951f), 
														new PointF(33.12589f, 118.9412f), 
														new PointF(75.43399f, 64.88889f), 
														new PointF(133.6974f, 41.79355f), 
														new PointF(235.725f, 167.669f), 
														new PointF(252.9149f, 150.4784f), 
														new PointF(299.1682f, 152.4271f), 
														new PointF(328.9677f, 161.7033f), 
														new PointF(319.474f, 171.1974f), 
														new PointF(311.2924f, 179.3794f), 
														new PointF(276.1503f, 185.2439f), 
														new PointF(235.4589f, 185.2179f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) (PathPointType.Bezier3 | PathPointType.CloseSubpath),
													(byte) PathPointType.Start,
													(byte) PathPointType.Line, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) (PathPointType.Bezier3 | PathPointType.CloseSubpath)}; 

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}			

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void AddPolygon_PointArr()
		{
			Point [] points = new Point [] {new Point(20, 100),
											new Point(40, 75),
											new Point(60, 125),
											new Point(80, 100),
											new Point(100, 50),
											new Point(120, 150),
											new Point(140, 100)};

			path = new GraphicsPath();
			path.AddPolygon(points);

			Assert.AreEqual (7, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF(20f, 100f), 
														new PointF(40f, 75f), 
														new PointF(60f, 125f), 
														new PointF(80f, 100f), 
														new PointF(100f, 50f), 
														new PointF(120f, 150f), 
														new PointF(140f, 100f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line | PathPointType.CloseSubpath)}; 

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}							

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void AddPolygon_PointFArr()
		{
			PointF [] points = new PointF [] {	new PointF(20.01f, 100.1f),
												new PointF(40.02f, 75.2f),
												new PointF(60.03f, 125.3f),
												new PointF(80.04f, 100.4f),
												new PointF(100.05f, 50.5f),
												new PointF(120.06f, 150.6f),
												new PointF(140.07f, 100.7f)};

			path = new GraphicsPath();
			path.AddPolygon(points);

			Assert.AreEqual (7, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF(20.01f, 100.1f),
														new PointF(40.02f, 75.2f),
														new PointF(60.03f, 125.3f),
														new PointF(80.04f, 100.4f),
														new PointF(100.05f, 50.5f),
														new PointF(120.06f, 150.6f),
														new PointF(140.07f, 100.7f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line | PathPointType.CloseSubpath)}; 

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}								

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void AddRectangle_Rectangle()
		{
			path = new GraphicsPath();
			path.AddRectangle (new Rectangle(50, 50, 400, 80));

			Assert.AreEqual (4, path.PointCount);
			
			PointF [] expectedPoints = new PointF [] {	new PointF(50f, 50f), 
														new PointF(450f, 50f), 
														new PointF(450f, 130f), 
														new PointF(50f, 130f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line,  
													(byte) (PathPointType.Line | PathPointType.CloseSubpath)};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void AddRectangle_RectangleF()
		{
			path = new GraphicsPath();
			path.AddRectangle (new RectangleF(50.1f, 50.4f, 400.12f, 80.123f));

			Assert.AreEqual (4, path.PointCount);
			
			PointF [] expectedPoints = new PointF [] {	new PointF(50.1f, 50.4f), 
														new PointF(450.22f, 50.4f), 
														new PointF(450.22f, 130.523f), 
														new PointF(50.1f, 130.523f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line | PathPointType.CloseSubpath)};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void AddRectangles_RectangleArr()
		{
			path = new GraphicsPath();
			Rectangle [] rectangles = new Rectangle [] {new Rectangle (50, 50, 400, 80),
														new Rectangle (150, 150, 100, 400),
														new Rectangle (0, 0, 200, 480),
														new Rectangle (450, 450, 40, 80)};
			path.AddRectangles (rectangles);

			Assert.AreEqual (16, path.PointCount);
			
			PointF [] expectedPoints = new PointF [] {	new PointF(50f, 50f), 
														new PointF(450f, 50f), 
														new PointF(450f, 130f), 
														new PointF(50f, 130f), 
														new PointF(150f, 150f), 
														new PointF(250f, 150f), 
														new PointF(250f, 550f), 
														new PointF(150f, 550f), 
														new PointF(0f, 0f), 
														new PointF(200f, 0f), 
														new PointF(200f, 480f), 
														new PointF(0f, 480f), 
														new PointF(450f, 450f), 
														new PointF(490f, 450f), 
														new PointF(490f, 530f), 
														new PointF(450f, 530f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line | PathPointType.CloseSubpath), 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line | PathPointType.CloseSubpath), 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line | PathPointType.CloseSubpath),
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line | PathPointType.CloseSubpath)};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void AddRectangles_RectangleFArr()
		{
			path = new GraphicsPath();
			RectangleF [] rectangles = new RectangleF [] {	new RectangleF (50.10f, 50.11f, 400.1f, 80.15f),
															new RectangleF (150f, 150.87f, 100.09f, 400.99f),
															new RectangleF (0.123245f, 0.23f, 200.98f, 480.56f),
															new RectangleF (450.3333333333f, 450.6666666f, 40.8f, 80.4f)};
			path.AddRectangles (rectangles);

			Assert.AreEqual (16, path.PointCount);
			
			PointF [] expectedPoints = new PointF [] {	new PointF(50.1f, 50.11f), 
														new PointF(450.2f, 50.11f), 
														new PointF(450.2f, 130.26f), 
														new PointF(50.1f, 130.26f), 
														new PointF(150f, 150.87f), 
														new PointF(250.09f, 150.87f), 
														new PointF(250.09f, 551.86f), 
														new PointF(150f, 551.86f), 
														new PointF(0.123245f, 0.23f), 
														new PointF(201.1032f, 0.23f), 
														new PointF(201.1032f, 480.79f), 
														new PointF(0.123245f, 480.79f), 
														new PointF(450.3333f, 450.6667f), 
														new PointF(491.1333f, 450.6667f), 
														new PointF(491.1333f, 531.0667f), 
														new PointF(450.3333f, 531.0667f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line | PathPointType.CloseSubpath), 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line | PathPointType.CloseSubpath), 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line | PathPointType.CloseSubpath),
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line | PathPointType.CloseSubpath)};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		[Category ("NotWorking")] 
		public void AddString_String_FontFamily_Int_Float_Point_StringFormat()
		{
			path = new GraphicsPath();
			
			string stringText = "System@Drawing";
			FontFamily family = new FontFamily ("Arial");
			int fontStyle = (int)FontStyle.Italic;
			int emSize = 56;
			Point origin = new Point (10, 120);
			StringFormat format = StringFormat.GenericDefault;
			
			path.AddString (stringText, family, fontStyle, emSize, origin, format);

			Assert.AreEqual (939, path.PointCount);

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		[Category ("NotWorking")]
		public void AddString_String_FontFamily_Int_Float_PointF_StringFormat()
		{
			path = new GraphicsPath();
			
			string stringText = "System@Drawing";
			FontFamily family = new FontFamily ("Arial");
			int fontStyle = (int)FontStyle.Italic;
			int emSize = 56;
			PointF origin = new PointF (10.15f, 120.01f);
			StringFormat format = StringFormat.GenericDefault;
			
			path.AddString (stringText, family, fontStyle, emSize, origin, format);

			Assert.AreEqual (939, path.PointCount);

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		[Category ("NotWorking")]
		public void AddString_String_FontFamily_Int_Float_Rectangle_StringFormat()
		{
			path = new GraphicsPath();
			
			string stringText = "System@Drawing";
			FontFamily family = new FontFamily ("Arial");
			int fontStyle = (int)FontStyle.Italic;
			int emSize = 56;
			Rectangle bound = new Rectangle (10, 120, 335, 50);
			StringFormat format = StringFormat.GenericDefault;
			
			path.AddString (stringText, family, fontStyle, emSize, bound, format);

			Assert.AreEqual (657, path.PointCount);

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		[Category ("NotWorking")]
		public void AddString_String_FontFamily_Int_Float_RectangleFF_StringFormat()
		{
			path = new GraphicsPath();
			
			string stringText = "System@Drawing";
			FontFamily family = new FontFamily ("Arial");
			int fontStyle = (int)FontStyle.Italic;
			int emSize = 56;
			RectangleF bound = new RectangleF (10f, 120.1f, 335.13f, 50.99f);
			StringFormat format = StringFormat.GenericDefault;
			
			path.AddString (stringText, family, fontStyle, emSize, bound, format);

			Assert.AreEqual (657, path.PointCount);

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void ClearMarkers()
		{
			path = new GraphicsPath ();
			path.AddEllipse (0, 0, 100, 200);
			path.SetMarkers ();
			path.AddLine (new Point (100, 100), new Point (200, 100));
			Rectangle rect = new Rectangle (200, 0, 100, 200);
			path.AddRectangle (rect);
			path.SetMarkers ();
			path.AddLine (new Point (250, 200), new Point (250, 300));
			path.SetMarkers ();

			path.ClearMarkers();

			GraphicsPathIterator pathIterator = new GraphicsPathIterator(path);
			pathIterator.Rewind ();
			int [] pointsNumber = new int [] {21, 0, 0, 0};
			for (int i=0; i < 4; i ++) {
				Assert.AreEqual (pointsNumber [i], pathIterator.NextMarker (path));
			}
			//t.AssertCompare ();
		}

		[Test]
		public void Clone()
		{
			path = new GraphicsPath ();
			path.AddEllipse (0, 0, 100, 200);
			path.SetMarkers ();
			path.AddLine (new Point (100, 100), new Point (200, 100));
			Rectangle rect = new Rectangle (200, 0, 100, 200);
			path.AddRectangle (rect);
			path.SetMarkers ();
			path.AddLine (new Point (250, 200), new Point (250, 300));
			path.SetMarkers ();

			GraphicsPath cloned = (GraphicsPath) path.Clone ();

			Assert.AreEqual (path.PointCount, cloned.PointCount);

			for ( int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(path.PathPoints [i], cloned.PathPoints [i]);
				Assert.AreEqual (path.PathTypes [i], cloned.PathTypes [i]);
			}

			GraphicsPathIterator pathIterator = new GraphicsPathIterator(path);
			pathIterator.Rewind ();

			GraphicsPathIterator clonedIterator = new GraphicsPathIterator(cloned);
			clonedIterator.Rewind ();

			for (int i=0; i < 4; i ++) {
				Assert.AreEqual (pathIterator.NextMarker (path), clonedIterator.NextMarker (cloned));
			}
			//t.AssertCompare ();
		}

		[Test]
		public void CloseAllFigures()
		{
			path = new GraphicsPath ();
			path.AddLine (new Point (100, 100), new Point (400, 100));
			path.AddLine (new Point (400, 200), new Point (10, 100));

			path.StartFigure();
			path.AddBezier( 10, 10, 50, 250, 100, 5, 200, 280);
			path.StartFigure();
			path.AddRectangle (new Rectangle (10, 20, 300, 400));

			path.StartFigure();
			path.AddLine (new Point (400, 400), new Point (400, 10));

			Assert.AreEqual (14, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF(100f, 100f), 
														new PointF(400f, 100f), 
														new PointF(400f, 200f), 
														new PointF(10f, 100f), 
														new PointF(10f, 10f), 
														new PointF(50f, 250f), 
														new PointF(100f, 5f), 
														new PointF(200f, 280f), 
														new PointF(10f, 20f), 
														new PointF(310f, 20f), 
														new PointF(310f, 420f), 
														new PointF(10f, 420f), 
														new PointF(400f, 400f), 
														new PointF(400f, 10f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line  |  PathPointType.CloseSubpath), 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	

			path.CloseAllFigures();

			Assert.AreEqual (14, path.PointCount);
			
			expectedPoints = new PointF [] {	new PointF(100f, 100f), 
												new PointF(400f, 100f), 
												new PointF(400f, 200f), 
												new PointF(10f, 100f), 
												new PointF(10f, 10f), 
												new PointF(50f, 250f), 
												new PointF(100f, 5f), 
												new PointF(200f, 280f), 
												new PointF(10f, 20f), 
												new PointF(310f, 20f), 
												new PointF(310f, 420f), 
												new PointF(10f, 420f), 
												new PointF(400f, 400f), 
												new PointF(400f, 10f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			expectedTypes = new byte [] {	(byte) PathPointType.Start, 
											(byte) PathPointType.Line, 
											(byte) PathPointType.Line, 
											(byte) (PathPointType.Line |  PathPointType.CloseSubpath), 
											(byte) PathPointType.Start, 
											(byte) PathPointType.Bezier3, 
											(byte) PathPointType.Bezier3, 
											(byte) (PathPointType.Bezier3 |  PathPointType.CloseSubpath), 
											(byte) PathPointType.Start, 
											(byte) PathPointType.Line, 
											(byte) PathPointType.Line, 
											(byte) (PathPointType.Line |  PathPointType.CloseSubpath), 
											(byte) PathPointType.Start, 
											(byte) (PathPointType.Line |  PathPointType.CloseSubpath)};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void CloseFigure()
		{
			path = new GraphicsPath ();
			path.AddLine (new Point (100, 100), new Point (400, 100));
			path.AddLine (new Point (400, 200), new Point (10, 100));

			path.StartFigure();
			path.AddBezier( 10, 10, 50, 250, 100, 5, 200, 280);
			path.StartFigure();
			path.AddRectangle (new Rectangle (10, 20, 300, 400));

			path.StartFigure();
			path.AddLine (new Point (400, 400), new Point (400, 10));

			Assert.AreEqual (14, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF(100f, 100f), 
														new PointF(400f, 100f), 
														new PointF(400f, 200f), 
														new PointF(10f, 100f), 
														new PointF(10f, 10f), 
														new PointF(50f, 250f), 
														new PointF(100f, 5f), 
														new PointF(200f, 280f), 
														new PointF(10f, 20f), 
														new PointF(310f, 20f), 
														new PointF(310f, 420f), 
														new PointF(10f, 420f), 
														new PointF(400f, 400f), 
														new PointF(400f, 10f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line  |  PathPointType.CloseSubpath), 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	

			path.CloseFigure();

			Assert.AreEqual (14, path.PointCount);
			
			expectedPoints = new PointF [] {	new PointF(100f, 100f), 
												new PointF(400f, 100f), 
												new PointF(400f, 200f), 
												new PointF(10f, 100f), 
												new PointF(10f, 10f), 
												new PointF(50f, 250f), 
												new PointF(100f, 5f), 
												new PointF(200f, 280f), 
												new PointF(10f, 20f), 
												new PointF(310f, 20f), 
												new PointF(310f, 420f), 
												new PointF(10f, 420f), 
												new PointF(400f, 400f), 
												new PointF(400f, 10f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			expectedTypes = new byte [] {	(byte) PathPointType.Start, 
											(byte) PathPointType.Line, 
											(byte) PathPointType.Line, 
											(byte) PathPointType.Line, 
											(byte) PathPointType.Start, 
											(byte) PathPointType.Bezier3, 
											(byte) PathPointType.Bezier3, 
											(byte) PathPointType.Bezier3, 
											(byte) PathPointType.Start, 
											(byte) PathPointType.Line, 
											(byte) PathPointType.Line, 
											(byte) (PathPointType.Line |  PathPointType.CloseSubpath), 
											(byte) PathPointType.Start, 
											(byte) (PathPointType.Line |  PathPointType.CloseSubpath)};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		[Category ("NotWorking")]
		public void Flatten()
		{
			path = new GraphicsPath ();
			path.AddBezier( new Point (10, 10),
							new Point (50, 250),
							new Point (100, 5),
							new Point (200, 280));

			path.AddBezier( new Point (0, 210),
							new Point (50, 6),
							new Point (150, 150),
							new Point (250, 10));

			Assert.AreEqual (8, path.PointCount);

			path.Flatten();

			Assert.AreEqual (87, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF(10f, 10f), 
														new PointF(11.875f, 20.875f), 
														new PointF(13.75f, 31.125f), 
														new PointF(15.6875f, 40.6875f), 
														new PointF(17.625f, 49.5625f), 
														new PointF(19.5625f, 57.875f), 
														new PointF(21.5625f, 65.5625f), 
														new PointF(23.5625f, 72.6875f), 
														new PointF(25.5625f, 79.25f), 
														new PointF(27.5625f, 85.25f), 
														new PointF(29.625f, 90.8125f), 
														new PointF(31.6875f, 95.875f), 
														new PointF(33.8125f, 100.5f), 
														new PointF(35.9375f, 104.625f), 
														new PointF(38.125f, 108.375f), 
														new PointF(40.3125f, 111.75f), 
														new PointF(42.5f, 114.75f), 
														new PointF(47f, 119.75f), 
														new PointF(51.625f, 123.5625f), 
														new PointF(56.4375f, 126.375f), 
														new PointF(61.3125f, 128.375f), 
														new PointF(66.375f, 129.75f), 
														new PointF(71.5625f, 130.6875f), 
														new PointF(82.5f, 131.875f), 
														new PointF(88.1875f, 132.5625f), 
														new PointF(94.125f, 133.5f), 
														new PointF(100.1875f, 134.9375f), 
														new PointF(106.5f, 137f), 
														new PointF(113f, 139.9375f), 
														new PointF(119.6875f, 143.875f), 
														new PointF(126.625f, 149f), 
														new PointF(130.125f, 152.0625f), 
														new PointF(133.75f, 155.5625f), 
														new PointF(137.4375f, 159.375f), 
														new PointF(141.125f, 163.6875f), 
														new PointF(144.9375f, 168.375f), 
														new PointF(148.75f, 173.5625f), 
														new PointF(152.6875f, 179.1875f), 
														new PointF(156.625f, 185.375f), 
														new PointF(160.6875f, 192.0625f), 
														new PointF(164.75f, 199.3125f), 
														new PointF(168.9375f, 207.125f), 
														new PointF(173.1875f, 215.5625f), 
														new PointF(177.4375f, 224.5625f), 
														new PointF(181.8125f, 234.3125f), 
														new PointF(186.25f, 244.625f), 
														new PointF(190.75f, 255.6875f), 
														new PointF(195.375f, 267.5f), 
														new PointF(200f, 280f), 
														new PointF(0f, 210f), 
														new PointF(2.375f, 200.6875f), 
														new PointF(4.8125f, 191.875f), 
														new PointF(7.375f, 183.5625f), 
														new PointF(9.9375f, 175.6875f), 
														new PointF(12.625f, 168.25f), 
														new PointF(15.3125f, 161.25f), 
														new PointF(18.125f, 154.75f), 
														new PointF(21f, 148.5625f), 
														new PointF(23.9375f, 142.8125f), 
														new PointF(26.9375f, 137.4375f), 
														new PointF(33.0625f, 127.8125f), 
														new PointF(39.4375f, 119.4375f), 
														new PointF(46.125f, 112.375f), 
														new PointF(52.9375f, 106.375f), 
														new PointF(60f, 101.4375f), 
														new PointF(67.25f, 97.3125f), 
														new PointF(74.6875f, 94f), 
														new PointF(82.3125f, 91.3125f), 
														new PointF(90.125f, 89.125f), 
														new PointF(98.125f, 87.4375f), 
														new PointF(106.25f, 86f), 
														new PointF(122.9375f, 83.625f), 
														new PointF(140.125f, 81f), 
														new PointF(148.9375f, 79.375f), 
														new PointF(157.75f, 77.3125f), 
														new PointF(166.75f, 74.8125f), 
														new PointF(175.8125f, 71.625f), 
														new PointF(184.875f, 67.75f), 
														new PointF(194.0625f, 62.9375f), 
														new PointF(203.3125f, 57.25f), 
														new PointF(212.625f, 50.4375f), 
														new PointF(221.9375f, 42.375f), 
														new PointF(231.25f, 33.0625f), 
														new PointF(235.9375f, 27.875f), 
														new PointF(240.625f, 22.3125f), 
														new PointF(245.3125f, 16.375f), 
														new PointF(250f, 10f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}						

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		[Category ("NotWorking")]
		public void Flatten_Matrix()
		{
			path = new GraphicsPath ();
			path.AddBezier( new Point (10, 10),
							new Point (50, 250),
							new Point (100, 5),
							new Point (200, 280));

			path.AddBezier( new Point (0, 210),
							new Point (50, 6),
							new Point (150, 150),
							new Point (250, 10));

			Assert.AreEqual (8, path.PointCount);

			Matrix matrix = new Matrix();
			matrix.Scale(2f,3f);
			path.Flatten(matrix);

			Assert.AreEqual (141, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF(20f, 30f), 
														new PointF(21.875f, 46.625f), 
														new PointF(23.75f, 62.6875f), 
														new PointF(25.6875f, 78.25f), 
														new PointF(27.5625f, 93.3125f), 
														new PointF(29.5f, 107.875f), 
														new PointF(31.375f, 122f), 
														new PointF(33.3125f, 135.5625f), 
														new PointF(35.25f, 148.6875f), 
														new PointF(37.1875f, 161.3125f), 
														new PointF(39.125f, 173.5625f), 
														new PointF(41.125f, 185.3125f), 
														new PointF(43.0625f, 196.625f), 
														new PointF(45.0625f, 207.5f), 
														new PointF(47.0625f, 218f), 
														new PointF(49.0625f, 228f), 
														new PointF(51.125f, 237.6875f), 
														new PointF(53.125f, 246.9375f), 
														new PointF(55.1875f, 255.8125f), 
														new PointF(57.1875f, 264.3125f), 
														new PointF(59.25f, 272.4375f), 
														new PointF(63.4375f, 287.625f), 
														new PointF(67.625f, 301.375f), 
														new PointF(71.875f, 313.875f), 
														new PointF(76.1875f, 325.1875f), 
														new PointF(80.5625f, 335.25f), 
														new PointF(85f, 344.25f), 
														new PointF(89.5f, 352.25f), 
														new PointF(94f, 359.25f), 
														new PointF(98.625f, 365.375f), 
														new PointF(103.3125f, 370.6875f), 
														new PointF(108.0625f, 375.25f), 
														new PointF(112.8125f, 379.125f), 
														new PointF(117.6875f, 382.375f), 
														new PointF(122.6875f, 385.0625f), 
														new PointF(127.6875f, 387.3125f), 
														new PointF(132.75f, 389.125f), 
														new PointF(143.1875f, 391.875f), 
														new PointF(153.9375f, 393.75f), 
														new PointF(165f, 395.4375f), 
														new PointF(176.4375f, 397.375f), 
														new PointF(188.25f, 400.1875f), 
														new PointF(194.25f, 402.125f), 
														new PointF(200.375f, 404.4375f), 
														new PointF(206.625f, 407.25f), 
														new PointF(213f, 410.625f), 
														new PointF(219.4375f, 414.5625f), 
														new PointF(225.9375f, 419.3125f), 
														new PointF(232.625f, 424.75f), 
														new PointF(239.375f, 431.0625f), 
														new PointF(246.25f, 438.25f), 
														new PointF(253.1875f, 446.375f), 
														new PointF(260.3125f, 455.625f), 
														new PointF(267.5f, 465.9375f), 
														new PointF(274.8125f, 477.4375f), 
														new PointF(282.25f, 490.1875f), 
														new PointF(289.8125f, 504.25f), 
														new PointF(297.5f, 519.6875f), 
														new PointF(301.4375f, 527.9375f), 
														new PointF(305.3125f, 536.625f), 
														new PointF(309.3125f, 545.625f), 
														new PointF(313.25f, 555.0625f), 
														new PointF(317.3125f, 564.8125f), 
														new PointF(321.3125f, 575.0625f), 
														new PointF(325.4375f, 585.6875f), 
														new PointF(329.5625f, 596.75f), 
														new PointF(333.6875f, 608.25f), 
														new PointF(337.875f, 620.1875f), 
														new PointF(342.0625f, 632.5625f), 
														new PointF(346.3125f, 645.375f), 
														new PointF(350.625f, 658.6875f), 
														new PointF(354.9375f, 672.4375f), 
														new PointF(359.25f, 686.75f), 
														new PointF(363.625f, 701.5f), 
														new PointF(368.0625f, 716.75f), 
														new PointF(372.5f, 732.5f), 
														new PointF(377f, 748.8125f), 
														new PointF(381.5625f, 765.625f), 
														new PointF(386.125f, 782.9375f), 
														new PointF(390.6875f, 800.875f), 
														new PointF(395.3125f, 819.3125f), 
														new PointF(400f, 838.3125f), 
														new PointF(0f, 630f), 
														new PointF(2.375f, 615.875f), 
														new PointF(4.75f, 602.0625f), 
														new PointF(9.6875f, 575.625f), 
														new PointF(14.6875f, 550.625f), 
														new PointF(19.875f, 527f), 
														new PointF(25.25f, 504.75f), 
														new PointF(30.6875f, 483.8125f), 
														new PointF(36.25f, 464.1875f), 
														new PointF(42f, 445.75f), 
														new PointF(47.8125f, 428.5f), 
														new PointF(53.8125f, 412.375f), 
														new PointF(59.9375f, 397.3125f), 
														new PointF(66.125f, 383.375f), 
														new PointF(72.4375f, 370.375f), 
														new PointF(78.9375f, 358.375f), 
														new PointF(85.5f, 347.3125f), 
														new PointF(92.1875f, 337.125f), 
														new PointF(98.9375f, 327.75f), 
														new PointF(105.875f, 319.1875f), 
														new PointF(112.875f, 311.375f), 
														new PointF(119.9375f, 304.25f), 
														new PointF(127.1875f, 297.8125f), 
														new PointF(134.4375f, 291.9375f), 
														new PointF(141.875f, 286.6875f), 
														new PointF(149.3125f, 281.9375f), 
														new PointF(156.9375f, 277.6875f), 
														new PointF(164.5625f, 273.875f), 
														new PointF(172.375f, 270.5f), 
														new PointF(180.1875f, 267.4375f), 
														new PointF(196.125f, 262.25f), 
														new PointF(212.3125f, 258f), 
														new PointF(228.875f, 254.3125f), 
														new PointF(245.625f, 250.8125f), 
														new PointF(262.6875f, 247.125f), 
														new PointF(279.9375f, 243.0625f), 
														new PointF(297.4375f, 238.125f), 
														new PointF(306.25f, 235.25f), 
														new PointF(315.125f, 232f), 
														new PointF(324f, 228.375f), 
														new PointF(333f, 224.375f), 
														new PointF(342f, 219.875f), 
														new PointF(351f, 214.875f), 
														new PointF(360.0625f, 209.3125f), 
														new PointF(369.1875f, 203.1875f), 
														new PointF(378.3125f, 196.375f), 
														new PointF(387.4375f, 188.875f), 
														new PointF(396.625f, 180.6875f), 
														new PointF(405.8125f, 171.6875f), 
														new PointF(415.0625f, 161.9375f), 
														new PointF(424.3125f, 151.25f), 
														new PointF(433.5625f, 139.6875f), 
														new PointF(442.8125f, 127.25f), 
														new PointF(452.125f, 113.75f), 
														new PointF(461.4375f, 99.25f), 
														new PointF(470.75f, 83.625f), 
														new PointF(480.0625f, 66.9375f), 
														new PointF(489.375f, 49.0625f), 
														new PointF(498.6875f, 30f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}						

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		[Category ("NotWorking")]
		public void Flatten_Matrix_Float()
		{
			path = new GraphicsPath ();
			path.AddBezier( new Point (10, 10),
							new Point (50, 250),
							new Point (100, 5),
							new Point (200, 280));

			Assert.AreEqual (4, path.PointCount);

			Matrix matrix = new Matrix();
			matrix.Scale(2f,3f);
			path.Flatten(matrix, 0.1f);

			Assert.AreEqual (78, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF(20f, 30f), 
														new PointF(21.875f, 46.6f), 
														new PointF(23.775f, 62.7f), 
														new PointF(25.65f, 78.27499f), 
														new PointF(27.55f, 93.325f), 
														new PointF(29.475f, 107.9f), 
														new PointF(31.4f, 121.975f), 
														new PointF(33.325f, 135.575f), 
														new PointF(35.25f, 148.675f), 
														new PointF(37.2f, 161.35f), 
														new PointF(39.15f, 173.55f), 
														new PointF(41.125f, 185.3f), 
														new PointF(43.1f, 196.625f), 
														new PointF(45.075f, 207.5f), 
														new PointF(47.075f, 217.975f), 
														new PointF(49.075f, 228.025f), 
														new PointF(51.1f, 237.675f), 
														new PointF(55.15f, 255.825f), 
														new PointF(59.275f, 272.425f), 
														new PointF(63.425f, 287.6f), 
														new PointF(67.625f, 301.425f), 
														new PointF(71.89999f, 313.925f), 
														new PointF(76.2f, 325.2f), 
														new PointF(80.575f, 335.3f), 
														new PointF(85f, 344.3f), 
														new PointF(89.475f, 352.275f), 
														new PointF(94.02499f, 359.3f), 
														new PointF(98.625f, 365.425f), 
														new PointF(103.3f, 370.75f), 
														new PointF(108.025f, 375.3f), 
														new PointF(112.85f, 379.175f), 
														new PointF(117.7f, 382.45f), 
														new PointF(122.65f, 385.175f), 
														new PointF(127.675f, 387.4f), 
														new PointF(132.775f, 389.25f), 
														new PointF(143.175f, 392f), 
														new PointF(153.925f, 393.925f), 
														new PointF(165f, 395.625f), 
														new PointF(176.425f, 397.625f), 
														new PointF(188.225f, 400.5f), 
														new PointF(194.25f, 402.425f), 
														new PointF(200.4f, 404.775f), 
														new PointF(206.625f, 407.6f), 
														new PointF(212.975f, 411f), 
														new PointF(219.4f, 415.025f), 
														new PointF(225.95f, 419.75f), 
														new PointF(232.6f, 425.25f), 
														new PointF(239.35f, 431.575f), 
														new PointF(246.225f, 438.825f), 
														new PointF(253.2f, 447.025f), 
														new PointF(260.3f, 456.275f), 
														new PointF(267.5f, 466.65f), 
														new PointF(274.825f, 478.175f), 
														new PointF(282.275f, 490.975f), 
														new PointF(289.825f, 505.1f), 
														new PointF(297.525f, 520.6f), 
														new PointF(305.325f, 537.55f), 
														new PointF(313.275f, 556.05f), 
														new PointF(317.275f, 565.875f), 
														new PointF(321.325f, 576.125f), 
														new PointF(325.425f, 586.775f), 
														new PointF(329.525f, 597.85f), 
														new PointF(333.675f, 609.375f), 
														new PointF(337.85f, 621.35f), 
														new PointF(342.075f, 633.75f), 
														new PointF(346.325f, 646.625f), 
														new PointF(350.6f, 659.95f), 
														new PointF(354.925f, 673.775f), 
														new PointF(359.275f, 688.075f), 
														new PointF(363.65f, 702.85f), 
														new PointF(368.075f, 718.15f), 
														new PointF(372.525f, 733.95f), 
														new PointF(377.025f, 750.275f), 
														new PointF(381.55f, 767.125f), 
														new PointF(386.1f, 784.525f), 
														new PointF(390.7f, 802.45f), 
														new PointF(395.325f, 820.95f), 
														new PointF(400f, 840f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}						

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		public void GetBounds()
		{
			path = new GraphicsPath ();
			path.AddLine (new Point (100, 100), new Point (400, 100));
			path.AddLine (new Point (400, 200), new Point (10, 100));

			path.StartFigure ();
			path.AddBezier( 10, 10, 50, 250, 100, 5, 200, 280);
			path.StartFigure ();
			path.AddRectangle (new Rectangle (10, 20, 300, 400));

			path.StartFigure ();
			path.AddLine (new Point (400, 400), new Point (400, 10));

			RectangleF actual = path.GetBounds ();
			RectangleF expected = new RectangleF (10f, 5f, 390f, 415f);

			DrawingTest.AssertAlmostEqual(expected.X, actual.X);
			DrawingTest.AssertAlmostEqual(expected.Y, actual.Y);
			DrawingTest.AssertAlmostEqual(expected.Width, actual.Width);
			DrawingTest.AssertAlmostEqual(expected.Height, actual.Height);

			//t.AssertCompare ();
		}

		[Test]
		public void GetBounds_Matrix()
		{
			path = new GraphicsPath ();
			path.AddLine (new Point (100, 100), new Point (400, 100));
			path.AddLine (new Point (400, 200), new Point (10, 100));

			path.StartFigure ();
			path.AddBezier( 10, 10, 50, 250, 100, 5, 200, 280);
			path.StartFigure ();
			path.AddRectangle (new Rectangle (10, 20, 300, 400));

			path.StartFigure ();
			path.AddLine (new Point (400, 400), new Point (400, 10));

			Matrix matrix = new Matrix ();
			matrix.Scale (1.2f,1.3f);
			matrix.Shear (1.5f, 1.9f);

			RectangleF actual = path.GetBounds (matrix);
			RectangleF expected = new RectangleF (21f, 31.2f, 1215f, 1502.8f);

			DrawingTest.AssertAlmostEqual(expected.X, actual.X);
			DrawingTest.AssertAlmostEqual(expected.Y, actual.Y);
			DrawingTest.AssertAlmostEqual(expected.Width, actual.Width);
			DrawingTest.AssertAlmostEqual(expected.Height, actual.Height);

			//t.AssertCompare ();
		}

		[Test]
		public void GetBounds_Matrix_Pen()
		{
			path = new GraphicsPath ();
			path.AddLine (new Point (100, 100), new Point (400, 100));
			path.AddLine (new Point (400, 200), new Point (10, 100));

			path.StartFigure ();
			path.AddBezier( 10, 10, 50, 250, 100, 5, 200, 280);
			path.StartFigure ();
			path.AddRectangle (new Rectangle (10, 20, 300, 400));

			path.StartFigure ();
			path.AddLine (new Point (400, 400), new Point (400, 10));

			Matrix matrix = new Matrix ();
			matrix.Scale (0.2f,0.3f);
			matrix.Shear (0.5f, 0.5f);

			Pen p = new Pen (Color.AliceBlue, 45);

			RectangleF actual = path.GetBounds (matrix, p);
			RectangleF expected = new RectangleF (21f, 31.2f, 2758.363f, 3046.163f);

			// we do not know exacly how the bounding rectangle 
			// is calculated so we just want to obtain bounds
			// that still contain the path widened by oen and transformed by matrix
			path.Widen (p, matrix);
			RectangleF widened = path.GetBounds ();
			Assert.IsTrue (actual.Contains (widened));

//			DrawingTest.AssertAlmostEqual(expected.X, actual.X);
//			DrawingTest.AssertAlmostEqual(expected.Y, actual.Y);
//			DrawingTest.AssertAlmostEqual(expected.Width, actual.Width);
//			DrawingTest.AssertAlmostEqual(expected.Height, actual.Height);

			path = new GraphicsPath ();
			path.AddLine (new Point (100, 100), new Point (400, 100));
			path.AddLine (new Point (400, 200), new Point (10, 100));

			path.StartFigure ();
			path.AddBezier( 10, 10, 50, 250, 100, 5, 200, 280);
			path.StartFigure ();
			path.AddRectangle (new Rectangle (10, 20, 300, 400));

			path.StartFigure ();
			path.AddLine (new Point (400, 400), new Point (400, 10));

			p = new Pen (Color.AliceBlue, 45);
			p.DashCap = DashCap.Triangle;
			p.DashStyle = DashStyle.Dash;

			actual = path.GetBounds (matrix, p);
			expected = new RectangleF (21f, 31.2f, 2758.363f, 3046.163f);

			// we do not know exacly how the bounding rectangle 
			// is calculated so we just want to obtain bounds
			// that still contain the path widened by oen and transformed by matrix
			path.Widen (p, matrix);
			widened = path.GetBounds ();
			Assert.IsTrue (actual.Contains (widened));

//			DrawingTest.AssertAlmostEqual(expected.X, actual.X);
//			DrawingTest.AssertAlmostEqual(expected.Y, actual.Y);
//			DrawingTest.AssertAlmostEqual(expected.Width, actual.Width);
//			DrawingTest.AssertAlmostEqual(expected.Height, actual.Height);

			//t.AssertCompare ();
		}

		[Test]
		public void GetLastPoint()
		{
			path = new GraphicsPath ();
			path.AddLine (new Point (100, 100), new Point (400, 100));
			path.AddLine (new Point (400, 200), new Point (10, 100));

			PointF expected = new PointF (10f, 100f);
			PointF actual = path.GetLastPoint ();

			DrawingTest.AssertAlmostEqual(expected, actual);

			path.StartFigure ();
			path.AddBezier( 10, 10, 50, 250, 100, 5, 200, 280);
			path.StartFigure ();
			path.AddRectangle (new Rectangle (10, 20, 300, 400));

			expected = new PointF (10f, 420f);
			actual = path.GetLastPoint ();

			DrawingTest.AssertAlmostEqual(expected, actual);

			path.StartFigure ();
			path.AddLine (new Point (400, 400), new Point (400, 10));

			expected = new PointF (400f, 10f);
			actual = path.GetLastPoint ();

			DrawingTest.AssertAlmostEqual(expected, actual);

			//t.AssertCompare ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetLastPoint2()
		{
			path = new GraphicsPath ();
			
			PointF actual = path.GetLastPoint ();		
		}

		[Test]
		public void IsOutlineVisible_Float_Float_Pen()
		{
			path = new GraphicsPath ();
			path.AddRectangle (new RectangleF (10f, 10f, 300f, 300f));
			
			path.StartFigure();
			path.AddRectangle (new RectangleF (150f, 10f, 50f, 400f));

			Pen pen = new Pen (Color.Red, 5);

			Assert.IsFalse (path.IsOutlineVisible (0f, 0f, pen));

			Assert.IsFalse (path.IsOutlineVisible (40f, 40f, pen));

			Assert.IsTrue (path.IsOutlineVisible (9f, 9f, pen));

			Assert.IsFalse (path.IsOutlineVisible (400f, 400f, pen));

			Assert.IsTrue (path.IsOutlineVisible (312f, 312f, pen));

			Assert.IsFalse (path.IsOutlineVisible (313f, 313f, pen));

			//t.AssertCompare ();
		}


		[Test]
		public void IsOutlineVisible_PointF_Pen()
		{
			path = new GraphicsPath ();
			path.AddRectangle (new RectangleF (10f, 10f, 300f, 300f));
			
			path.StartFigure();
			path.AddRectangle (new RectangleF (150f, 10f, 50f, 400f));

			Pen pen = new Pen (Color.Red, 5);

			Assert.IsFalse (path.IsOutlineVisible (new PointF (0f, 0f), pen));

			Assert.IsFalse (path.IsOutlineVisible (new PointF (40f, 40f), pen));

			Assert.IsTrue (path.IsOutlineVisible (new PointF (9f, 9f), pen));

			Assert.IsFalse (path.IsOutlineVisible (new PointF (400f, 400f), pen));

			Assert.IsTrue (path.IsOutlineVisible (new PointF (312f, 312f), pen));

			Assert.IsFalse (path.IsOutlineVisible (new PointF (313f, 313f), pen));

			//t.AssertCompare ();
		}


		[Test]
		public void IsOutlineVisible_Float_Float_Pen_Graphics()
		{
			path = new GraphicsPath ();
			path.AddRectangle (new RectangleF (10f, 10f, 300f, 300f));
			
			path.StartFigure();
			path.AddRectangle (new RectangleF (150f, 10f, 50f, 400f));

			Pen pen = new Pen (Color.Red, 5);
			Graphics gr = Graphics.FromImage (new Bitmap (512, 512));
			gr.Clip = new Region (new Rectangle ( 5, 5, 500, 50));

			Assert.IsFalse (path.IsOutlineVisible (0f, 0f, pen, gr));

			Assert.IsFalse (path.IsOutlineVisible (40f, 40f, pen, gr));

			Assert.IsTrue (path.IsOutlineVisible (9f, 9f, pen, gr));

			Assert.IsFalse (path.IsOutlineVisible (400f, 400f, pen, gr));

			Assert.IsTrue (path.IsOutlineVisible (312f, 312f, pen, gr));

			Assert.IsFalse (path.IsOutlineVisible (313f, 313f, pen, gr));

			//t.AssertCompare ();
		}


		[Test]
		public void IsOutlineVisible_PointF_Pen_Graphics()
		{
			path = new GraphicsPath ();
			path.AddRectangle (new RectangleF (10f, 10f, 300f, 300f));
			
			path.StartFigure();
			path.AddRectangle (new RectangleF (150f, 10f, 50f, 400f));

			Pen pen = new Pen (Color.Red, 5);
			Graphics gr = Graphics.FromImage (new Bitmap (512, 512));
			gr.Clip = new Region (new Rectangle ( 5, 5, 500, 50));

			Assert.IsFalse (path.IsOutlineVisible (new PointF (0f, 0f), pen, gr));

			Assert.IsFalse (path.IsOutlineVisible (new PointF (40f, 40f), pen, gr));

			Assert.IsTrue (path.IsOutlineVisible (new PointF (9f, 9f), pen, gr));

			Assert.IsFalse (path.IsOutlineVisible (new PointF (400f, 400f), pen, gr));

			Assert.IsTrue (path.IsOutlineVisible (new PointF (312f, 312f), pen, gr));

			Assert.IsFalse (path.IsOutlineVisible (new PointF (313f, 313f), pen, gr));

			//t.AssertCompare ();
		}


		[Test]
		public void IsOutlineVisible_Int_Int_Pen()
		{
			path = new GraphicsPath ();
			path.AddRectangle (new RectangleF (10f, 10f, 300f, 300f));
			
			path.StartFigure();
			path.AddRectangle (new RectangleF (150f, 10f, 50f, 400f));

			Pen pen = new Pen (Color.Red, 5);

			Assert.IsFalse (path.IsOutlineVisible (0, 0, pen));

			Assert.IsFalse (path.IsOutlineVisible (40, 40, pen));

			Assert.IsTrue (path.IsOutlineVisible (9, 9, pen));

			Assert.IsFalse (path.IsOutlineVisible (400, 400, pen));

			Assert.IsTrue (path.IsOutlineVisible (312, 312, pen));

			Assert.IsFalse (path.IsOutlineVisible (313, 313, pen));

			//t.AssertCompare ();
		}


		[Test]
		public void IsOutlineVisible_Point_Pen()
		{				
			path = new GraphicsPath ();
			path.AddRectangle (new RectangleF (10f, 10f, 300f, 300f));
			
			path.StartFigure();
			path.AddRectangle (new RectangleF (150f, 10f, 50f, 400f));

			Pen pen = new Pen (Color.Red, 5);

			Assert.IsFalse (path.IsOutlineVisible (new Point (0, 0), pen));

			Assert.IsFalse (path.IsOutlineVisible (new Point (40, 40), pen));

			Assert.IsTrue (path.IsOutlineVisible (new Point (9, 9), pen));

			Assert.IsFalse (path.IsOutlineVisible (new Point (400, 400), pen));

			Assert.IsTrue (path.IsOutlineVisible (new Point (312, 312), pen));

			Assert.IsFalse (path.IsOutlineVisible (new Point (313, 313), pen));

			//t.AssertCompare ();
		}


		[Test]
		public void IsOutlineVisible_Int_Int_Pen_Graphics()
		{
			path = new GraphicsPath ();
			path.AddRectangle (new RectangleF (10f, 10f, 300f, 300f));
			
			path.StartFigure();
			path.AddRectangle (new RectangleF (150f, 10f, 50f, 400f));

			Pen pen = new Pen (Color.Red, 5);
			Graphics gr = Graphics.FromImage (new Bitmap (512, 512));
			gr.Clip = new Region (new Rectangle ( 5, 5, 500, 50));

			Assert.IsFalse (path.IsOutlineVisible (0, 0, pen, gr));

			Assert.IsFalse (path.IsOutlineVisible (40, 40, pen, gr));

			Assert.IsTrue (path.IsOutlineVisible (9, 9, pen, gr));

			Assert.IsFalse (path.IsOutlineVisible (400, 400, pen, gr));

			Assert.IsTrue (path.IsOutlineVisible (312, 312, pen, gr));

			Assert.IsFalse (path.IsOutlineVisible (313, 313, pen, gr));

			//t.AssertCompare ();
		}


		[Test]
		public void IsOutlineVisible_Point_Pen_Graphics()
		{
			path = new GraphicsPath ();
			path.AddRectangle (new RectangleF (10f, 10f, 300f, 300f));
			
			path.StartFigure();
			path.AddRectangle (new RectangleF (150f, 10f, 50f, 400f));

			Pen pen = new Pen (Color.Red, 5);
			Graphics gr = Graphics.FromImage (new Bitmap (512, 512));
			gr.Clip = new Region (new Rectangle ( 5, 5, 500, 50));

			Assert.IsFalse (path.IsOutlineVisible (new Point (0, 0), pen, gr));

			Assert.IsFalse (path.IsOutlineVisible (new Point (40, 40), pen, gr));

			Assert.IsTrue (path.IsOutlineVisible (new Point (9, 9), pen, gr));

			Assert.IsFalse (path.IsOutlineVisible (new Point (400, 400), pen, gr));

			Assert.IsTrue (path.IsOutlineVisible (new Point (312, 312), pen, gr));

			Assert.IsFalse (path.IsOutlineVisible (new Point (313, 313), pen, gr));

			Assert.IsTrue (path.IsOutlineVisible (new Point (310, 10), pen, gr));
			Assert.IsTrue (path.IsOutlineVisible (new Point (310, 10), pen, null));

			Assert.IsTrue (path.IsOutlineVisible (new Point (310, 210), pen, gr));
			Assert.IsTrue (path.IsOutlineVisible (new Point (310, 210), pen, null));

			//t.AssertCompare ();
		}

		[Test]
		public void IsVisible_Float_Float()
		{
			path = new GraphicsPath ();
			path.AddLine (10, 10, 400, 10);
			path.AddLine (400, 10, 10, 400);
			path.AddLine (10, 400, 400, 400);
			path.CloseFigure();

			Assert.IsFalse (path.IsVisible (9f, 9f));
			
			Assert.IsTrue (path.IsVisible (10f, 10f));

			Assert.IsFalse (path.IsVisible (400f, 400f));

			Assert.IsTrue (path.IsVisible (397f, 399f));

			Assert.IsFalse (path.IsVisible (399f, 397f));

			Assert.IsTrue (path.IsVisible (190f, 190f));

			Assert.IsFalse (path.IsVisible (50f, 190f));

			Assert.IsTrue (path.IsVisible (190f, 50f));

			//t.AssertCompare ();
		}


		[Test]
		public void IsVisible_PointF()
		{
			path = new GraphicsPath ();
			path.AddLine (10, 10, 400, 10);
			path.AddLine (400, 10, 10, 400);
			path.AddLine (10, 400, 400, 400);
			path.CloseFigure();

			Assert.IsFalse (path.IsVisible (new PointF (9f, 9f)));
			
			Assert.IsTrue (path.IsVisible (new PointF (10f, 10f)));

			Assert.IsFalse (path.IsVisible (new PointF (400f, 400f)));

			Assert.IsTrue (path.IsVisible (new PointF (397f, 399f)));

			Assert.IsFalse (path.IsVisible (new PointF (399f, 397f)));

			Assert.IsTrue (path.IsVisible (new PointF (190f, 190f)));

			Assert.IsFalse (path.IsVisible (new PointF (50f, 190f)));

			Assert.IsTrue (path.IsVisible (new PointF (190f, 50f)));

			//t.AssertCompare ();
		}


		[Test]
		public void IsVisible_Float_Float_Graphics()
		{
			path = new GraphicsPath ();
			path.AddLine (10, 10, 400, 10);
			path.AddLine (400, 10, 10, 400);
			path.AddLine (10, 400, 400, 400);
			path.CloseFigure();

			Graphics gr = Graphics.FromImage (new Bitmap (500, 100));
			gr.Clip = new Region (new Rectangle(0, 0, 50, 50));

			Assert.IsFalse (path.IsVisible (9f, 9f, gr));
			
			Assert.IsTrue (path.IsVisible (10f, 10f, gr));

			Assert.IsFalse (path.IsVisible (400f, 400f, gr));

			Assert.IsTrue (path.IsVisible (397f, 399f, gr));

			Assert.IsFalse (path.IsVisible (399f, 397f, gr));

			Assert.IsTrue (path.IsVisible (190f, 190f, gr));

			Assert.IsFalse (path.IsVisible (50f, 190f, gr));

			Assert.IsTrue (path.IsVisible (190f, 50f, gr));

			//t.AssertCompare ();
		}


		[Test]
		public void IsVisible_PointF_Graphics()
		{
			path = new GraphicsPath ();
			path.AddLine (10, 10, 400, 10);
			path.AddLine (400, 10, 10, 400);
			path.AddLine (10, 400, 400, 400);
			path.CloseFigure();

			Graphics gr = Graphics.FromImage (new Bitmap (500, 100));
			gr.Clip = new Region (new Rectangle(0, 0, 50, 50));

			Assert.IsFalse (path.IsVisible (new PointF (9f, 9f), gr));
			
			Assert.IsTrue (path.IsVisible (new PointF (10f, 10f), gr));

			Assert.IsFalse (path.IsVisible (new PointF (400f, 400f), gr));

			Assert.IsTrue (path.IsVisible (new PointF (397f, 399f), gr));

			Assert.IsFalse (path.IsVisible (new PointF (399f, 397f), gr));

			Assert.IsTrue (path.IsVisible (new PointF (190f, 190f), gr));

			Assert.IsFalse (path.IsVisible (new PointF (50f, 190f), gr));

			Assert.IsTrue (path.IsVisible (new PointF (190f, 50f), gr));

			//t.AssertCompare ();
		}


		[Test]
		public void IsVisible_Int_Int()
		{
			path = new GraphicsPath ();
			path.AddLine (10, 10, 400, 10);
			path.AddLine (400, 10, 10, 400);
			path.AddLine (10, 400, 400, 400);
			path.CloseFigure();

			Assert.IsFalse (path.IsVisible (9, 9));
			
			Assert.IsTrue (path.IsVisible (10, 10));

			Assert.IsFalse (path.IsVisible (400, 400));

			Assert.IsTrue (path.IsVisible (397, 399));

			Assert.IsFalse (path.IsVisible (399, 397));

			Assert.IsTrue (path.IsVisible (190, 190));

			Assert.IsFalse (path.IsVisible (50, 190));

			Assert.IsTrue (path.IsVisible (190, 50));

			//t.AssertCompare ();
		}


		[Test]
		public void IsVisible_Point()
		{
			path = new GraphicsPath ();
			path.AddLine (10, 10, 400, 10);
			path.AddLine (400, 10, 10, 400);
			path.AddLine (10, 400, 400, 400);
			path.CloseFigure();

			Assert.IsFalse (path.IsVisible (new Point (9, 9)));
			
			Assert.IsTrue (path.IsVisible (new Point (10, 10)));

			Assert.IsFalse (path.IsVisible (new Point (400, 400)));

			Assert.IsTrue (path.IsVisible (new Point (397, 399)));

			Assert.IsFalse (path.IsVisible (new Point (399, 397)));

			Assert.IsTrue (path.IsVisible (new Point (190, 190)));

			Assert.IsFalse (path.IsVisible (new Point (50, 190)));

			Assert.IsTrue (path.IsVisible (new Point (190, 50)));

			//t.AssertCompare ();
		}


		[Test]
		public void IsVisible_Int_Int_Graphics()
		{
			path = new GraphicsPath ();
			path.AddLine (10, 10, 400, 10);
			path.AddLine (400, 10, 10, 400);
			path.AddLine (10, 400, 400, 400);
			path.CloseFigure();

			Graphics gr = Graphics.FromImage (new Bitmap (500, 100));
			gr.Clip = new Region (new Rectangle(0, 0, 50, 50));

			Assert.IsFalse (path.IsVisible (9, 9, gr));
			
			Assert.IsTrue (path.IsVisible (10, 10, gr));

			Assert.IsFalse (path.IsVisible (400, 400, gr));

			Assert.IsTrue (path.IsVisible (397, 399, gr));

			Assert.IsFalse (path.IsVisible (399, 397, gr));

			Assert.IsTrue (path.IsVisible (190, 190, gr));

			Assert.IsFalse (path.IsVisible (50, 190, gr));

			Assert.IsTrue (path.IsVisible (190, 50));

			//t.AssertCompare ();
		}


		[Test]
		public void IsVisible_Point_Graphics()
		{
			path = new GraphicsPath ();
			path.AddLine (10, 10, 400, 10);
			path.AddLine (400, 10, 10, 400);
			path.AddLine (10, 400, 400, 400);
			path.CloseFigure();

			Graphics gr = Graphics.FromImage (new Bitmap (500, 100));
			gr.Clip = new Region (new Rectangle(0, 0, 50, 50));

			Assert.IsFalse (path.IsVisible (new Point (9, 9), gr));
			
			Assert.IsTrue (path.IsVisible (new Point (10, 10), gr));

			Assert.IsFalse (path.IsVisible (new Point (400, 400), gr));

			Assert.IsTrue (path.IsVisible (new Point (397, 399), gr));

			Assert.IsFalse (path.IsVisible (new Point (399, 397), gr));

			Assert.IsTrue (path.IsVisible (new Point (190, 190), gr));

			Assert.IsFalse (path.IsVisible (new Point (50, 190), gr));

			Assert.IsTrue (path.IsVisible (new Point (190, 50), gr));

			//t.AssertCompare ();
		}

		[Test]
		public void PathData ()
		{
			path = new GraphicsPath ();
			path.AddLine (new Point (100, 100), new Point (400, 100));
			path.AddLine (new Point (400, 200), new Point (10, 100));
			path.StartFigure ();
			path.SetMarkers ();
			path.AddBezier( 10, 10, 50, 250, 100, 5, 200, 280);
			path.StartFigure ();
			path.SetMarkers ();
			path.AddRectangle (new Rectangle (10, 20, 300, 400));
			path.StartFigure ();
			path.SetMarkers ();
			path.AddLine (new Point (400, 400), new Point (400, 10));

			PointF [] expectedPoints = new PointF [] {	new PointF(100f, 100f), 
														new PointF(400f, 100f), 
														new PointF(400f, 200f), 
														new PointF(10f, 100f), 
														new PointF(10f, 10f), 
														new PointF(50f, 250f), 
														new PointF(100f, 5f), 
														new PointF(200f, 280f), 
														new PointF(10f, 20f), 
														new PointF(310f, 20f), 
														new PointF(310f, 420f), 
														new PointF(10f, 420f), 
														new PointF(400f, 400f), 
														new PointF(400f, 10f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line | PathPointType.PathMarker), 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) (PathPointType.Bezier3 | PathPointType.PathMarker), 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line | PathPointType.CloseSubpath | PathPointType.PathMarker), 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}


			path = new GraphicsPath ();
			path.AddEllipse (0, 0, 100, 200);
			path.SetMarkers ();
			path.AddLine (new Point (100, 100), new Point (200, 100));
			Rectangle rect = new Rectangle (200, 0, 100, 200);
			path.AddRectangle (rect);
			path.SetMarkers ();
			path.AddLine (new Point (250, 200), new Point (250, 300));
			path.SetMarkers ();

			expectedTypes = new byte [] {	(byte) PathPointType.Start, 
											(byte) PathPointType.Bezier3, 
											(byte) PathPointType.Bezier3, 
											(byte) PathPointType.Bezier3, 
											(byte) PathPointType.Bezier3, 
											(byte) PathPointType.Bezier3, 
											(byte) PathPointType.Bezier3, 
											(byte) PathPointType.Bezier3, 
											(byte) PathPointType.Bezier3, 
											(byte) PathPointType.Bezier3, 
											(byte) PathPointType.Bezier3, 
											(byte) PathPointType.Bezier3, 
											(byte) (PathPointType.Bezier3 | PathPointType.CloseSubpath | PathPointType.PathMarker), 
											(byte) PathPointType.Start, 
											(byte) PathPointType.Line, 
											(byte) PathPointType.Start, 
											(byte) PathPointType.Line, 
											(byte) PathPointType.Line, 
											(byte) (PathPointType.Line | PathPointType.CloseSubpath | PathPointType.PathMarker), 
											(byte) PathPointType.Start, 
											(byte) (PathPointType.Line | PathPointType.PathMarker) };

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	
		}

		[Test]
		[Category("NotWorking")]
		// In .Net PathData seems to clone Types and Points before returning them to user
		public void PathData2 ()
		{
			path = new GraphicsPath ();
			path.AddLine (new Point (100, 100), new Point (400, 100));
			path.AddLine (new Point (400, 200), new Point (10, 100));

			byte [] types = path.PathTypes;
			PointF [] points = path.PathPoints;

			types [1] = 88;
			points [1] = new PointF (-88, -88);

			Assert.AreEqual ( 88, types [1]);
			DrawingTest.AssertAlmostEqual ( new PointF (-88,-88), points [1]);

			Assert.AreEqual ( 1, path.PathData.Types [1]);
			DrawingTest.AssertAlmostEqual ( new PointF (400,100), path.PathData.Points [1]);
		}

		[Test]
		public void Reset()
		{
			path = new GraphicsPath ();
			path.AddLine (new Point (100, 100), new Point (400, 100));
			path.AddLine (new Point (400, 200), new Point (10, 100));

			path.StartFigure ();
			path.AddBezier( 10, 10, 50, 250, 100, 5, 200, 280);
			path.StartFigure ();
			path.AddRectangle (new Rectangle (10, 20, 300, 400));

			path.StartFigure ();
			path.AddLine (new Point (400, 400), new Point (400, 10));

			path.Reset ();

			Assert.AreEqual (0, path.PointCount);
			Assert.AreEqual (FillMode.Alternate, path.FillMode);

			//t.AssertCompare ();
		}

		
		[Test]
		public void Reverse()
		{
			path = new GraphicsPath ();
			path.AddLine (new Point (100, 100), new Point (400, 100));
			path.SetMarkers ();
			path.AddLine (new Point (400, 200), new Point (10, 100));

			path.SetMarkers ();
			path.StartFigure ();
			path.AddBezier( 10, 10, 50, 250, 100, 5, 200, 280);
			path.SetMarkers ();
			path.StartFigure ();
			path.AddRectangle (new Rectangle (10, 20, 300, 400));

			path.StartFigure ();
			path.AddLine (new Point (400, 400), new Point (400, 10));
			path.AddLine (new Point (400, 450), new Point (500, 510));
			path.SetMarkers ();
			path.CloseFigure ();
			
			path.Reverse ();

			PointF [] expectedPoints = new PointF [] {	new PointF(500f, 510f), 
														new PointF(400f, 450f), 
														new PointF(400f, 10f), 
														new PointF(400f, 400f), 
														new PointF(10f, 420f), 
														new PointF(310f, 420f), 
														new PointF(310f, 20f),
														new PointF(10f, 20f), 
														new PointF(200f, 280f), 
														new PointF(100f, 5f), 
														new PointF(50f, 250f), 
														new PointF(10f, 10f), 
														new PointF(10f, 100f), 
														new PointF(400f, 200f), 
														new PointF(400f, 100f), 
														new PointF(100f, 100f)};

			
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line | PathPointType.CloseSubpath), 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line | PathPointType.CloseSubpath | PathPointType.PathMarker), 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) (PathPointType.Bezier3 | PathPointType.PathMarker), 
													(byte) PathPointType.Start, 
													(byte) (PathPointType.Line | PathPointType.PathMarker), 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	
			
			//t.AssertCompare ();
		}

		[Test]
		public void Reverse2()
		{
			path = new GraphicsPath ();
			path.AddLine (new Point (100, 100), new Point (400, 100));
			path.SetMarkers ();
			path.AddLine (new Point (400, 200), new Point (10, 100));

			path.SetMarkers ();
			path.StartFigure ();
			path.AddBezier( 10, 10, 50, 250, 100, 5, 200, 280);
			path.SetMarkers ();
			path.StartFigure ();
			path.AddRectangle (new Rectangle (10, 20, 300, 400));

			path.StartFigure ();
			path.AddLine (new Point (400, 400), new Point (400, 10));
			path.AddBezier( 100, 100, 500, 250, 150, 500, 250, 300);
			path.SetMarkers ();
			path.AddLine (new Point (400, 450), new Point (500, 510));
			path.SetMarkers ();
			path.CloseFigure ();

			path.Reverse ();

			PointF [] expectedPoints = new PointF [] {	new PointF(500f, 510f), 
														new PointF(400f, 450f), 
														new PointF(250f, 300f), 
														new PointF(150f, 500f), 
														new PointF(500f, 250f), 
														new PointF(100f, 100f), 
														new PointF(400f, 10f), 
														new PointF(400f, 400f), 
														new PointF(10f, 420f), 
														new PointF(310f, 420f), 
														new PointF(310f, 20f), 
														new PointF(10f, 20f), 
														new PointF(200f, 280f), 
														new PointF(100f, 5f), 
														new PointF(50f, 250f), 
														new PointF(10f, 10f), 
														new PointF(10f, 100f), 
														new PointF(400f, 200f), 
														new PointF(400f, 100f), 
														new PointF(100f, 100f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) (PathPointType.Line | PathPointType.PathMarker), 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line | PathPointType.CloseSubpath), 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line | PathPointType.CloseSubpath | PathPointType.PathMarker), 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) (PathPointType.Bezier3 | PathPointType.PathMarker), 
													(byte) PathPointType.Start, 
													(byte) (PathPointType.Line | PathPointType.PathMarker),
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	
			
			//t.AssertCompare ();
		}

		[Test]
		public void SetMarkers()
		{
			path = new GraphicsPath ();
			path.AddEllipse (0, 0, 100, 200);
			path.SetMarkers ();
			path.AddLine (new Point (100, 100), new Point (200, 100));
			Rectangle rect = new Rectangle (200, 0, 100, 200);
			path.AddRectangle (rect);
			path.SetMarkers ();
			path.AddLine (new Point (250, 200), new Point (250, 300));
			path.SetMarkers ();

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) (PathPointType.Bezier3 | PathPointType.CloseSubpath | PathPointType.PathMarker), 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line | PathPointType.CloseSubpath | PathPointType.PathMarker), 
													(byte) PathPointType.Start, 
													(byte) (PathPointType.Line | PathPointType.PathMarker) };

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	


			//t.AssertCompare ();
		}

		[Test]
		public void StartFigure()
		{
			path = new GraphicsPath ();
			path.AddLine (new Point (100, 100), new Point (400, 100));
			path.AddLine (new Point (400, 200), new Point (10, 100));

			path.StartFigure();
			path.AddBezier( 10, 10, 50, 250, 100, 5, 200, 280);
			path.StartFigure();
			path.AddRectangle (new Rectangle (10, 20, 300, 400));

			path.StartFigure();
			path.AddLine (new Point (400, 400), new Point (400, 10));

			path.StartFigure();

			Assert.AreEqual (14, path.PointCount);

			PointF [] expectedPoints = new PointF [] {	new PointF(100f, 100f), 
														new PointF(400f, 100f), 
														new PointF(400f, 200f), 
														new PointF(10f, 100f), 
														new PointF(10f, 10f), 
														new PointF(50f, 250f), 
														new PointF(100f, 5f), 
														new PointF(200f, 280f), 
														new PointF(10f, 20f), 
														new PointF(310f, 20f), 
														new PointF(310f, 420f), 
														new PointF(10f, 420f), 
														new PointF(400f, 400f), 
														new PointF(400f, 10f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line  |  PathPointType.CloseSubpath), 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	

			//t.AssertCompare ();
		}

		[Test]
		public void Transform_Matrix()
		{
			path = new GraphicsPath ();
			path.AddLine (new Point (100, 100), new Point (200, 100));
			path.AddLine (new Point (200, 200), new Point (10, 100));

			path.StartFigure();
			path.AddBezier( 10, 10, 50, 250, 100, 5, 200, 280);
			path.StartFigure();
			path.AddRectangle (new Rectangle (10, 20, 200, 200));

			path.StartFigure();
			path.AddLine (new Point (200, 200), new Point (200, 10));

			Matrix matrix = new Matrix ();
			matrix.Scale (1.2f, 1.4f);
			matrix.Shear (0.9f, -1.15f);
			matrix.Rotate (5);

			path.Transform (matrix);

			PointF [] expectedPoints = new PointF [] {	new PointF(226.0865f, 5.313778f), 
														new PointF(355.0427f, -142.8718f), 
														new PointF(452.173f, 10.62756f), 
														new PointF(110.0259f, 138.6808f), 
														new PointF(22.60865f, 0.5313778f), 
														new PointF(307.3039f, 309.6555f), 
														new PointF(133.8127f, -140.5106f), 
														new PointF(529.8773f, 133.427f), 
														new PointF(32.32168f, 15.88131f), 
														new PointF(290.234f, -280.4898f), 
														new PointF(484.4947f, 26.50887f), 
														new PointF(226.5823f, 322.8799f), 
														new PointF(452.173f, 10.62756f), 
														new PointF(267.6254f, -281.0212f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line  |  PathPointType.CloseSubpath), 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		[Category ("NotWorking")]
		public void Warp_PointFArr_RectangleF()
		{
			path = new GraphicsPath ();
			path.AddLine (new Point (100, 100), new Point (200, 100));
			path.AddLine (new Point (200, 200), new Point (10, 100));

			path.StartFigure();
			path.AddBezier( 10, 10, 50, 250, 100, 5, 200, 280);
			path.StartFigure();
			path.AddRectangle (new Rectangle (10, 20, 200, 200));

			path.StartFigure();
			path.AddLine (new Point (200, 200), new Point (200, 10));

			RectangleF rectangle = new RectangleF (0f, 0f, 40f, 40f);
			PointF [] warp = new PointF [] {	new PointF (0f, 0f), 
												new PointF (50f, 50f),
												new PointF (20f, 40f)};

			path.Warp (warp, rectangle);

			PointF [] expectedPoints = new PointF [] {	new PointF(175f, 225f), 
														new PointF(300f, 350f), 
														new PointF(350f, 450f), 
														new PointF(62.5f, 112.5f), 
														new PointF(17.5f, 22.5f), 
														new PointF(71.54785f, 111.1621f), 
														new PointF(110.5078f, 167.8906f), 
														new PointF(140.8545f, 205.0488f), 
														new PointF(169.0625f, 235f), 
														new PointF(201.6064f, 270.1074f), 
														new PointF(244.9609f, 322.7344f), 
														new PointF(305.6006f, 405.2441f), 
														new PointF(390f, 530f), 
														new PointF(22.5f, 32.5f), 
														new PointF(272.5f, 282.5f), 
														new PointF(372.5f, 482.5f), 
														new PointF(122.5f, 232.5f), 
														new PointF(350f, 450f), 
														new PointF(255f, 260f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line,
													(byte) (PathPointType.Line  |  PathPointType.CloseSubpath), 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}


		[Test]
		[Category ("NotWorking")]
		public void Warp_PointFArr_RectangleF_Matrix()
		{
			path = new GraphicsPath ();
			path.AddLine (new Point (100, 100), new Point (200, 100));
			path.AddLine (new Point (200, 200), new Point (10, 100));

			path.StartFigure();
			path.AddBezier( 10, 10, 50, 250, 100, 5, 200, 280);
			path.StartFigure();
			path.AddRectangle (new Rectangle (10, 20, 200, 200));

			path.StartFigure();
			path.AddLine (new Point (200, 200), new Point (200, 10));

			RectangleF rectangle = new RectangleF (0f, 0f, 40f, 40f);
			PointF [] warp = new PointF [] {	new PointF (0f, 0f), 
												new PointF (50f, 50f),
												new PointF (20f, 40f)};

			Matrix matrix = new Matrix();
			matrix.Scale(1.5f, 0.5f);

			path.Warp (warp, rectangle, matrix);

			PointF [] expectedPoints = new PointF [] {	new PointF(262.5f, 112.5f), 
														new PointF(450f, 175f), 
														new PointF(525f, 225f), 
														new PointF(93.75f, 56.25f), 
														new PointF(26.25f, 11.25f), 
														new PointF(107.3218f, 55.58105f), 
														new PointF(165.7617f, 83.94531f), 
														new PointF(211.2817f, 102.5244f), 
														new PointF(253.5938f, 117.5f), 
														new PointF(302.4097f, 135.0537f), 
														new PointF(367.4414f, 161.3672f), 
														new PointF(458.4009f, 202.6221f), 
														new PointF(585f, 265f), 
														new PointF(33.75f, 16.25f), 
														new PointF(408.75f, 141.25f), 
														new PointF(558.75f, 241.25f), 
														new PointF(183.75f, 116.25f), 
														new PointF(525f, 225f), 
														new PointF(382.5f, 130f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line,
													(byte) (PathPointType.Line  |  PathPointType.CloseSubpath), 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}


		[Test]
		[Category ("NotWorking")]
		public void Warp_PointFArr_RectangleF_Matrix_WarpMode()
		{
			path = new GraphicsPath ();
			path.AddLine (new Point (100, 100), new Point (200, 100));
			path.AddLine (new Point (200, 200), new Point (10, 100));

			path.StartFigure();
			path.AddBezier( 10, 10, 50, 250, 100, 5, 200, 280);
			path.StartFigure();
			path.AddRectangle (new Rectangle (10, 20, 200, 200));

			path.StartFigure();
			path.AddLine (new Point (200, 200), new Point (200, 10));

			RectangleF rectangle = new RectangleF (0f, 0f, 40f, 40f);
			PointF [] warp = new PointF [] {	new PointF (0f, 0f), 
												new PointF (50f, 50f),
												new PointF (20f, 40f)};

			Matrix matrix = new Matrix();
			matrix.Scale(1.5f, 0.5f);

			path.Warp (warp, rectangle, matrix, WarpMode.Bilinear);

			PointF [] expectedPoints = new PointF [] {	new PointF(262.5f, 112.5f), 
														new PointF(449.9999f, 175f), 
														new PointF(524.9999f, 225f), 
														new PointF(412.9687f, 180f), 
														new PointF(292.4999f, 129.375f), 
														new PointF(163.5937f, 73.12499f), 
														new PointF(26.25f, 11.25f), 
														new PointF(153.75f, 83.74999f), 
														new PointF(153.75f, 83.74999f), 
														new PointF(192.6927f, 98.78391f), 
														new PointF(226.0163f, 109.1132f), 
														new PointF(253.6658f, 116.3978f), 
														new PointF(266.8857f, 118.041f), 
														new PointF(254.0196f, 109.4254f), 
														new PointF(213.4754f, 89.22914f), 
														new PointF(408.7499f, 141.25f), 
														new PointF(558.7499f, 241.25f), 
														new PointF(456.5624f, 205.9375f), 
														new PointF(469.4531f, 208.6719f), 
														new PointF(524.9999f, 225f), 
														new PointF(382.5f, 130f), 
														new PointF(5.064195E-08f, 8.370257E-08f), 
														new PointF(3.344191E-06f, 2.124933E-06f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	

			path = new GraphicsPath ();
			path.AddLine (new Point (100, 100), new Point (200, 100));
			path.AddLine (new Point (200, 200), new Point (10, 100));

			path.StartFigure();
			path.AddBezier( 10, 10, 50, 250, 100, 5, 200, 280);
			path.StartFigure();
			path.AddRectangle (new Rectangle (10, 20, 200, 200));

			path.StartFigure();
			path.AddLine (new Point (200, 200), new Point (200, 10));

			path.Warp (warp, rectangle, matrix, WarpMode.Perspective);

			expectedPoints = new PointF [] {new PointF(262.5f, 112.5f), 
											new PointF(450f, 175f), 
											new PointF(525f, 225f), 
											new PointF(93.75f, 56.25f), 
											new PointF(26.25f, 11.25f), 
											new PointF(107.3218f, 55.58105f), 
											new PointF(165.7617f, 83.94531f), 
											new PointF(211.2817f, 102.5244f), 
											new PointF(253.5938f, 117.5f), 
											new PointF(302.4097f, 135.0537f), 
											new PointF(367.4414f, 161.3672f), 
											new PointF(458.4009f, 202.6221f), 
											new PointF(585f, 265f), 
											new PointF(33.75f, 16.25f), 
											new PointF(408.75f, 141.25f), 
											new PointF(558.75f, 241.25f), 
											new PointF(183.75f, 116.25f), 
											new PointF(525f, 225f), 
											new PointF(382.5f, 130f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			expectedTypes = new byte [] {	(byte) PathPointType.Start, 
											(byte) PathPointType.Line, 
											(byte) PathPointType.Line, 
											(byte) PathPointType.Line, 
											(byte) PathPointType.Start, 
											(byte) PathPointType.Line, 
											(byte) PathPointType.Line, 
											(byte) PathPointType.Line, 
											(byte) PathPointType.Line, 
											(byte) PathPointType.Line, 
											(byte) PathPointType.Line, 
											(byte) PathPointType.Line, 
											(byte) PathPointType.Line, 
											(byte) PathPointType.Start, 
											(byte) PathPointType.Line, 
											(byte) PathPointType.Line, 
											(byte) (PathPointType.Line  |  PathPointType.CloseSubpath), 
											(byte) PathPointType.Start, 
											(byte) PathPointType.Line};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}


		[Test]
		[Category ("NotWorking")]
		public void Warp_PointFArr_RectangleF_Matrix_WarpMode_Float()
		{
			path = new GraphicsPath ();
			path.AddLine (new Point (100, 100), new Point (200, 100));
			path.AddLine (new Point (200, 200), new Point (10, 100));

			path.StartFigure();
			path.AddBezier( 10, 10, 50, 250, 100, 5, 200, 280);
			path.StartFigure();
			path.AddRectangle (new Rectangle (10, 20, 200, 200));

			path.StartFigure();
			path.AddLine (new Point (200, 200), new Point (200, 10));

			RectangleF rectangle = new RectangleF (0f, 0f, 40f, 40f);
			PointF [] warp = new PointF [] {	new PointF (0f, 0f), 
												new PointF (50f, 50f),
												new PointF (20f, 40f)};

			Matrix matrix = new Matrix();
			matrix.Scale(1.5f, 0.5f);

			path.Warp (warp, rectangle, matrix, WarpMode.Perspective, 0.2f);

			PointF [] expectedPoints = new PointF [] {	new PointF(262.5f, 112.5f), 
														new PointF(450f, 175f), 
														new PointF(525f, 225f), 
														new PointF(93.75f, 56.25f), 
														new PointF(26.25f, 11.25f), 
														new PointF(107.3218f, 55.58105f), 
														new PointF(165.7617f, 83.94531f), 
														new PointF(211.2817f, 102.5244f), 
														new PointF(253.5938f, 117.5f), 
														new PointF(302.4097f, 135.0537f), 
														new PointF(367.4414f, 161.3672f), 
														new PointF(458.4009f, 202.6221f), 
														new PointF(585f, 265f), 
														new PointF(33.75f, 16.25f), 
														new PointF(408.75f, 141.25f), 
														new PointF(558.75f, 241.25f), 
														new PointF(183.75f, 116.25f), 
														new PointF(525f, 225f), 
														new PointF(382.5f, 130f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line,
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line  |  PathPointType.CloseSubpath), 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}	

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		[Test]
		[Category ("NotWorking")]
		public void Widen_Pen()
		{
			path = new GraphicsPath ();
			path.AddBezier( 10, 10, 50, 250, 100, 5, 200, 280);
			path.StartFigure();
			path.AddRectangle (new Rectangle (10, 20, 200, 200));

			Pen pen = new Pen (Color.Red, 15);

			path.Widen (pen);

			PointF [] expectedPoints = new PointF [] {	 new PointF(17.37995f, 8.663473f), 
														new PointF(21.21328f, 29.83014f), 
														new PointF(21.17457f, 29.63168f), 
														new PointF(25.00791f, 47.96501f), 
														new PointF(24.96026f, 47.75257f), 
														new PointF(28.79359f, 63.75257f), 
														new PointF(28.69803f, 63.39326f), 
														new PointF(32.69803f, 77.05992f), 
														new PointF(32.56306f, 76.64414f), 
														new PointF(36.72973f, 88.31081f), 
														new PointF(36.5541f, 87.86461f), 
														new PointF(40.72076f, 97.53127f), 
														new PointF(40.39609f, 96.86954f), 
														new PointF(44.72942f, 104.7029f), 
														new PointF(44.40704f, 104.1731f), 
														new PointF(48.74038f, 110.6731f), 
														new PointF(48.0747f, 109.8161f), 
														new PointF(52.5747f, 114.8161f), 
														new PointF(51.63366f, 113.9359f), 
														new PointF(56.30032f, 117.6026f), 
														new PointF(55.45956f, 117.0298f), 
														new PointF(60.2929f, 119.8631f), 
														new PointF(59.36763f, 119.4032f), 
														new PointF(64.20096f, 121.4032f), 
														new PointF(62.98528f, 121.0175f), 
														new PointF(73.31861f, 123.3508f), 
														new PointF(72.46971f, 123.2098f), 
														new PointF(83.43214f, 124.3903f), 
														new PointF(95.72781f, 126.1469f), 
														new PointF(109.1111f, 129.9448f), 
														new PointF(116.4476f, 133.3309f), 
														new PointF(123.7762f, 137.5449f), 
														new PointF(131.4605f, 143.2166f), 
														new PointF(138.9887f, 150.2071f), 
														new PointF(146.8953f, 158.8165f), 
														new PointF(154.9268f, 169.1177f), 
														new PointF(163.0379f, 181.3707f), 
														new PointF(171.4102f, 195.7232f), 
														new PointF(179.9099f, 212.2126f), 
														new PointF(188.6968f, 231.3073f), 
														new PointF(197.8003f, 252.886f), 
														new PointF(207.0185f, 277.356f), 
														new PointF(192.9815f, 282.644f), 
														new PointF(183.8148f, 258.3106f), 
														new PointF(183.9231f, 258.5819f), 
														new PointF(174.9231f, 237.2486f), 
														new PointF(175.0201f, 237.4686f), 
														new PointF(166.3534f, 218.6353f), 
														new PointF(166.5002f, 218.9363f), 
														new PointF(158.1669f, 202.7696f), 
														new PointF(158.355f, 203.1124f), 
														new PointF(150.1883f, 189.1124f), 
														new PointF(150.4128f, 189.4732f), 
														new PointF(142.5794f, 177.6399f), 
														new PointF(142.9186f, 178.1115f), 
														new PointF(135.2519f, 168.2781f), 
														new PointF(135.6427f, 168.7397f), 
														new PointF(128.1427f, 160.573f), 
														new PointF(128.5633f, 160.9959f), 
														new PointF(121.5633f, 154.4959f), 
														new PointF(122.2128f, 155.0343f), 
														new PointF(115.2128f, 149.8676f), 
														new PointF(115.9281f, 150.3351f), 
														new PointF(109.2615f, 146.5018f), 
														new PointF(109.8571f, 146.8097f), 
														new PointF(103.3571f, 143.8097f), 
														new PointF(104.4525f, 144.2151f), 
														new PointF(92.11913f, 140.7151f), 
														new PointF(93.106f, 140.9246f), 
														new PointF(81.43933f, 139.2579f), 
														new PointF(81.69695f, 139.2902f), 
														new PointF(70.4351f, 138.0774f), 
														new PointF(59.05708f, 135.5082f), 
														new PointF(53.15385f, 133.0655f), 
														new PointF(47.43391f, 129.7124f), 
														new PointF(41.85787f, 125.3312f), 
														new PointF(36.56138f, 119.4462f), 
														new PointF(31.75414f, 112.2354f), 
														new PointF(27.09196f, 103.8076f), 
														new PointF(22.68428f, 93.58176f), 
														new PointF(18.36339f, 81.48326f), 
														new PointF(14.24973f, 67.42826f), 
														new PointF(10.3477f, 51.14154f), 
														new PointF(6.471401f, 32.6027f), 
														new PointF(2.620048f, 11.33653f), 
														new PointF(2.5f, 12.5f), 
														new PointF(217.5f, 12.5f), 
														new PointF(217.5f, 227.5f), 
														new PointF(2.5f, 227.5f), 
														new PointF(17.5f, 220f), 
														new PointF(10f, 212.5f), 
														new PointF(210f, 212.5f), 
														new PointF(202.5f, 220f), 
														new PointF(202.5f, 20f), 
														new PointF(210f, 27.5f), 
														new PointF(10f, 27.5f), 
														new PointF(17.5f, 20f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line,
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line  |  PathPointType.CloseSubpath),  
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line  |  PathPointType.CloseSubpath),  
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line  |  PathPointType.CloseSubpath)};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}


		[Test]
		[Category ("NotWorking")]
		public void Widen_Pen_Matrix()
		{
			path = new GraphicsPath ();
			path.AddBezier( 10, 10, 50, 250, 100, 5, 200, 280);
			path.StartFigure();
			path.AddRectangle (new Rectangle (10, 20, 200, 200));

			Pen pen = new Pen (Color.Red, 15);

			Matrix matrix = new Matrix();
			matrix.Scale(1.5f, 0.5f);

			path.Widen (pen, matrix);

			PointF [] expectedPoints = new PointF [] {	new PointF(26.07226f, 4.336054f), 
														new PointF(31.73893f, 14.83605f), 
														new PointF(31.68019f, 14.73517f), 
														new PointF(37.51352f, 24.0685f), 
														new PointF(37.43172f, 23.94766f), 
														new PointF(43.26505f, 31.94765f), 
														new PointF(43.13037f, 31.77996f), 
														new PointF(49.13037f, 38.61329f), 
														new PointF(48.90903f, 38.3879f), 
														new PointF(55.07569f, 44.05457f), 
														new PointF(54.85265f, 43.86571f), 
														new PointF(61.01931f, 48.69905f), 
														new PointF(60.29364f, 48.22634f), 
														new PointF(73.29364f, 55.393f), 
														new PointF(71.52402f, 54.64954f), 
														new PointF(85.35734f, 59.14954f), 
														new PointF(82.3909f, 58.45626f), 
														new PointF(96.8909f, 60.78959f), 
														new PointF(94.50352f, 60.51069f), 
														new PointF(109.8369f, 61.67735f), 
														new PointF(108.7007f, 61.61112f), 
														new PointF(125.034f, 62.27779f), 
														new PointF(124.944f, 62.27424f), 
														new PointF(143.4256f, 62.9783f), 
														new PointF(163.835f, 65.00085f), 
														new PointF(185.6939f, 68.67461f), 
														new PointF(208.6403f, 74.99842f), 
														new PointF(232.2171f, 84.42915f), 
														new PointF(244.575f, 90.69513f), 
														new PointF(257.0276f, 97.85966f), 
														new PointF(269.7801f, 106.0213f), 
														new PointF(282.9691f, 115.6594f), 
														new PointF(296.6128f, 126.4395f), 
														new PointF(310.5198f, 138.671f), 
														new PointF(289.4801f, 141.329f), 
														new PointF(275.6468f, 129.1623f), 
														new PointF(275.8013f, 129.2909f), 
														new PointF(262.3013f, 118.6243f), 
														new PointF(262.4312f, 118.7229f), 
														new PointF(249.4311f, 109.2229f), 
														new PointF(249.6888f, 109.3989f), 
														new PointF(237.1888f, 101.3989f), 
														new PointF(237.4324f, 101.5466f), 
														new PointF(225.2657f, 94.54655f), 
														new PointF(225.5994f, 94.72665f), 
														new PointF(213.7661f, 88.72665f), 
														new PointF(214.5241f, 89.06734f), 
														new PointF(192.0241f, 80.06734f), 
														new PointF(193.4982f, 80.55679f), 
														new PointF(172.3315f, 74.72345f), 
														new PointF(174.4351f, 75.18177f), 
														new PointF(154.6017f, 71.84844f), 
														new PointF(156.4607f, 72.0945f), 
														new PointF(137.9607f, 70.26116f), 
														new PointF(139.8892f, 70.3924f), 
														new PointF(122.3442f, 69.72402f), 
														new PointF(105.3928f, 69.03212f), 
														new PointF(88.25542f, 67.7282f), 
														new PointF(70.95618f, 64.94441f), 
														new PointF(54.78223f, 59.68301f), 
														new PointF(40.62651f, 51.87922f), 
														new PointF(34.02815f, 46.70752f), 
														new PointF(27.63626f, 40.83389f), 
														new PointF(21.46311f, 33.80336f), 
														new PointF(15.52437f, 25.65881f), 
														new PointF(9.621692f, 16.21452f), 
														new PointF(3.927732f, 5.663945f), 
														new PointF(3.749999f, 6.249999f), 
														new PointF(326.25f, 6.249999f), 
														new PointF(326.2499f, 113.75f), 
														new PointF(3.749999f, 113.75f), 
														new PointF(26.25f, 110f), 
														new PointF(15f, 106.25f), 
														new PointF(315f, 106.25f), 
														new PointF(303.75f, 110f), 
														new PointF(303.75f, 9.999999f), 
														new PointF(315f, 13.75f), 
														new PointF(15f, 13.75f), 
														new PointF(26.25f, 9.999999f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line  |  PathPointType.CloseSubpath), 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line  |  PathPointType.CloseSubpath), 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line  |  PathPointType.CloseSubpath)};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}


		[Test]
		[Category ("NotWorking")]
		public void Widen_Pen_Matrix_Float()
		{
			path = new GraphicsPath ();
			path.AddBezier( 10, 10, 50, 250, 100, 5, 200, 280);
			path.StartFigure();
			path.AddRectangle (new Rectangle (10, 20, 200, 200));

			Pen pen = new Pen (Color.Red, 15);

			Matrix matrix = new Matrix();
			matrix.Scale(1.5f, 0.5f);

			path.Widen (pen, matrix, 0.2f);

			PointF [] expectedPoints = new PointF [] {	new PointF(26.08857f, 4.367013f), 
														new PointF(28.88857f, 9.817012f), 
														new PointF(28.85975f, 9.763281f), 
														new PointF(31.70975f, 14.86328f), 
														new PointF(31.67857f, 14.80965f), 
														new PointF(34.57857f, 19.60965f), 
														new PointF(34.5436f, 19.55396f), 
														new PointF(37.4436f, 24.00396f), 
														new PointF(37.39977f, 23.93945f), 
														new PointF(40.29977f, 28.03945f), 
														new PointF(40.25008f, 27.972f), 
														new PointF(43.20008f, 31.82199f), 
														new PointF(43.09911f, 31.69899f), 
														new PointF(49.09911f, 38.54898f), 
														new PointF(48.90694f, 38.35032f), 
														new PointF(55.05694f, 44.15032f), 
														new PointF(54.77378f, 43.90996f), 
														new PointF(61.07379f, 48.75996f), 
														new PointF(60.64857f, 48.46797f), 
														new PointF(67.04858f, 52.41797f), 
														new PointF(66.42046f, 52.07551f), 
														new PointF(73.02045f, 55.27551f), 
														new PointF(72.11205f, 54.89137f), 
														new PointF(78.86205f, 57.39137f), 
														new PointF(77.63413f, 57.00044f), 
														new PointF(84.58413f, 58.90044f), 
														new PointF(83.11855f, 58.56083f), 
														new PointF(90.31853f, 59.96083f), 
														new PointF(88.90129f, 59.72806f), 
														new PointF(96.2513f, 60.72805f), 
														new PointF(94.81331f, 60.56914f), 
														new PointF(102.3633f, 61.21914f), 
														new PointF(101.4567f, 61.15424f), 
														new PointF(109.3067f, 61.60424f), 
														new PointF(108.7323f, 61.57637f), 
														new PointF(125.1848f, 62.23045f), 
														new PointF(143.1808f, 63.05786f), 
														new PointF(153.344f, 63.83537f), 
														new PointF(163.8406f, 65.00166f), 
														new PointF(174.5957f, 66.60938f), 
														new PointF(185.6805f, 68.76014f), 
														new PointF(197.0758f, 71.55418f), 
														new PointF(208.587f, 75.10439f), 
														new PointF(220.3038f, 79.37941f), 
														new PointF(232.2803f, 84.50476f), 
														new PointF(244.5031f, 90.61616f), 
														new PointF(256.9783f, 97.77407f), 
														new PointF(263.386f, 101.7725f), 
														new PointF(269.8497f, 106.0477f), 
														new PointF(276.4059f, 110.6218f), 
														new PointF(283.0547f, 115.5449f), 
														new PointF(289.7812f, 120.8047f), 
														new PointF(296.6029f, 126.3632f), 
														new PointF(303.5159f, 132.3174f), 
														new PointF(310.5394f, 138.5884f), 
														new PointF(289.4605f, 141.2115f), 
														new PointF(282.4605f, 134.9615f), 
														new PointF(282.5082f, 135.0034f), 
														new PointF(275.6582f, 129.1034f), 
														new PointF(275.7375f, 129.1699f), 
														new PointF(268.9875f, 123.6699f), 
														new PointF(269.051f, 123.7205f), 
														new PointF(262.401f, 118.5205f), 
														new PointF(262.4915f, 118.5893f), 
														new PointF(255.9415f, 113.7393f), 
														new PointF(256.049f, 113.8166f), 
														new PointF(249.599f, 109.3166f), 
														new PointF(249.7036f, 109.3877f), 
														new PointF(243.3536f, 105.1877f), 
														new PointF(243.477f, 105.2669f), 
														new PointF(237.227f, 101.3669f), 
														new PointF(237.4224f, 101.4837f), 
														new PointF(225.2224f, 94.48374f), 
														new PointF(225.5894f, 94.68011f), 
														new PointF(213.7894f, 88.78011f), 
														new PointF(214.2746f, 89.00435f), 
														new PointF(202.8246f, 84.10435f), 
														new PointF(203.3942f, 84.32931f), 
														new PointF(192.2942f, 80.27931f), 
														new PointF(192.9597f, 80.50254f), 
														new PointF(182.2597f, 77.20254f), 
														new PointF(183.2339f, 77.47076f), 
														new PointF(172.8339f, 74.92076f), 
														new PointF(173.8405f, 75.14091f), 
														new PointF(163.7905f, 73.19091f), 
														new PointF(164.8466f, 73.37167f), 
														new PointF(155.1466f, 71.92167f), 
														new PointF(156.1924f, 72.05755f), 
														new PointF(146.7424f, 71.00755f), 
														new PointF(147.7834f, 71.10496f), 
														new PointF(138.6334f, 70.40496f), 
														new PointF(139.6128f, 70.46482f), 
														new PointF(122.2128f, 69.66482f), 
														new PointF(122.4177f, 69.6736f), 
														new PointF(105.7794f, 69.01213f), 
														new PointF(97.18594f, 68.51952f), 
														new PointF(88.45159f, 67.76756f), 
														new PointF(79.66547f, 66.57217f), 
														new PointF(71.00809f, 64.88879f), 
														new PointF(62.7075f, 62.61956f), 
														new PointF(54.89606f, 59.72644f), 
														new PointF(47.53793f, 56.15886f), 
														new PointF(40.61982f, 51.88909f), 
														new PointF(33.97219f, 46.77147f), 
														new PointF(27.58867f, 40.75123f), 
														new PointF(21.44726f, 33.73978f), 
														new PointF(18.42414f, 29.79435f), 
														new PointF(15.47745f, 25.62836f), 
														new PointF(12.53827f, 21.11823f), 
														new PointF(9.605241f, 16.26356f), 
														new PointF(6.725243f, 11.10988f), 
														new PointF(3.911426f, 5.632986f), 
														new PointF(3.749999f, 6.249999f), 
														new PointF(326.25f, 6.249999f), 
														new PointF(326.2499f, 113.75f), 
														new PointF(3.749999f, 113.75f), 
														new PointF(26.25f, 110f), 
														new PointF(15f, 106.25f), 
														new PointF(315f, 106.25f), 
														new PointF(303.75f, 110f), 
														new PointF(303.75f, 9.999999f), 
														new PointF(315f, 13.75f), 
														new PointF(15f, 13.75f), 
														new PointF(26.25f, 9.999999f)};
			
			for(int i = 0; i < path.PointCount; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], path.PathPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line  |  PathPointType.CloseSubpath), 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line  |  PathPointType.CloseSubpath), 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line  |  PathPointType.CloseSubpath)};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], path.PathTypes [i]);
			}

			t.Graphics.DrawPath (p, path);
			t.Show ();
			//t.AssertCompare ();
		}

		#region Private helpers

		public void Print (GraphicsPath path)
		{
//			foreach(PointF point in path.PathPoints) {
//				Console.WriteLine("new PointF({0}f, {1}f), ",point.X, point.Y);
//			}
//			foreach(PathPointType type in path.PathTypes) {
//				Console.WriteLine("(byte) PathPointType.{0}, ",type);
//			}

			for (int i=0; i < path.PointCount; i++) {
				Console.WriteLine (" ({0},{1}) [{2}]",path.PathPoints[i].X,path.PathPoints[i].Y,ToString ((PathPointType)path.PathTypes[i]));
			}
		}

		public string ToString (PathPointType type)
		{
			foreach (PathPointType t in Enum.GetValues(typeof (PathPointType)))
				if (type == t)
				return type.ToString ();

			string s = (type & PathPointType.PathTypeMask).ToString ();

			foreach (PathPointType t in new PathPointType[] {PathPointType.PathMarker, PathPointType.CloseSubpath})
				if ((type & t) != 0)
					s += " | " + t.ToString ();

			return s;
		}
		
		#endregion // Private helpers
	}
}
