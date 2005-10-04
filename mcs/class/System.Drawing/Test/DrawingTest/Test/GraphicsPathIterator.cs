using System;
using NUnit.Framework;
using System.Drawing.Drawing2D;
using System.Drawing;

using DrawingTestHelper;
 
namespace Test.Sys.Drawing 
{
    [TestFixture]
    public class GraphicsPathIteratorFixture 
	{        
        [Test]
        public virtual void NextSubpath_Int_Int_Bool() 
		{
            GraphicsPath path = new GraphicsPath ();
			path.AddLine (new Point (100, 100), new Point (400, 100));
			path.AddLine (new Point (400, 200), new Point (10, 100));
			path.StartFigure ();
			path.SetMarkers ();
			path.AddBezier( 10, 10, 50, 250, 100, 5, 200, 280);
			path.CloseFigure ();
			path.StartFigure ();
			path.SetMarkers ();
			path.AddRectangle (new Rectangle (10, 20, 300, 400));
			path.StartFigure ();
			path.SetMarkers ();
			path.AddLine (new Point (400, 400), new Point (400, 10));

			GraphicsPathIterator iterator = new GraphicsPathIterator (path);

			int start;
			int end;
			bool isClosed;

			int count = iterator.NextSubpath (out start, out end, out isClosed);
			Assert.AreEqual (4, count);
			Assert.AreEqual (0, start);
			Assert.AreEqual (3, end);
			Assert.IsFalse (isClosed);

			count = iterator.NextSubpath (out start, out end, out isClosed);
			Assert.AreEqual (4, count);
			Assert.AreEqual (4, start);
			Assert.AreEqual (7, end);
			Assert.IsTrue (isClosed);

			count = iterator.NextSubpath (out start, out end, out isClosed);
			Assert.AreEqual (4, count);
			Assert.AreEqual (8, start);
			Assert.AreEqual (11, end);
			Assert.IsTrue (isClosed);

			count = iterator.NextSubpath (out start, out end, out isClosed);
			Assert.AreEqual (2, count);
			Assert.AreEqual (12, start);
			Assert.AreEqual (13, end);
			Assert.IsFalse (isClosed);

			count = iterator.NextSubpath (out start, out end, out isClosed);
			Assert.AreEqual (0, count);
			Assert.AreEqual (0, start);
			Assert.AreEqual (0, end);
			Assert.IsTrue (isClosed);
        }
        
        [Test]
        public virtual void NextSubpath_GraphicsPath_Bool() 
		{
            GraphicsPath path = new GraphicsPath ();
			path.AddLine (new Point (100, 100), new Point (400, 100));
			path.AddLine (new Point (400, 200), new Point (10, 100));
			path.StartFigure ();
			path.SetMarkers ();
			path.AddBezier( 10, 10, 50, 250, 100, 5, 200, 280);
			path.CloseFigure ();
			path.StartFigure ();
			path.SetMarkers ();
			path.AddRectangle (new Rectangle (10, 20, 300, 400));
			path.StartFigure ();
			path.SetMarkers ();
			path.AddLine (new Point (400, 400), new Point (400, 10));

			GraphicsPathIterator iterator = new GraphicsPathIterator (path);
			GraphicsPath path2 = new GraphicsPath ();

			bool isClosed;

			int count = iterator.NextSubpath (path2, out isClosed);
			Assert.AreEqual (4, count);
			Assert.IsFalse (isClosed);

			PointF [] actualPoints = path2.PathPoints;
			byte [] actualTypes = path2.PathTypes;

			PointF [] expectedPoints = new PointF [] {	new PointF(100f, 100f), 
														new PointF(400f, 100f), 
														new PointF(400f, 200f), 
														new PointF(10f, 100f)};
			
			for(int i = 0; i < expectedPoints.Length; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], actualPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line | PathPointType.PathMarker)};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], actualTypes [i]);
			}

			count = iterator.NextSubpath (path2, out isClosed);
			Assert.AreEqual (4, count);
			Assert.IsTrue (isClosed);

			actualPoints = path2.PathPoints;
			actualTypes = path2.PathTypes;

			expectedPoints = new PointF [] {new PointF(10f, 10f), 
											new PointF(50f, 250f), 
											new PointF(100f, 5f), 
											new PointF(200f, 280f)};
			
			for(int i = 0; i < expectedPoints.Length; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], actualPoints [i]);
			}

			expectedTypes = new byte [] {	(byte) PathPointType.Start, 
								(byte) PathPointType.Bezier3, 
								(byte) PathPointType.Bezier3, 
								(byte) (PathPointType.Bezier3 | PathPointType.CloseSubpath | PathPointType.PathMarker)};
			
			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], actualTypes [i]);
			}

			count = iterator.NextSubpath (path2, out isClosed);
			Assert.AreEqual (4, count);
			Assert.IsTrue (isClosed);

			actualPoints = path2.PathPoints;
			actualTypes = path2.PathTypes;

			expectedPoints = new PointF [] {new PointF(10f, 20f), 
											new PointF(310f, 20f), 
											new PointF(310f, 420f), 
											new PointF(10f, 420f)};
			
			for(int i = 0; i < expectedPoints.Length; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], actualPoints [i]);
			}

			expectedTypes = new byte [] {	(byte) PathPointType.Start, 
											(byte) PathPointType.Line, 
											(byte) PathPointType.Line, 
											(byte) (PathPointType.Line | PathPointType.CloseSubpath | PathPointType.PathMarker)};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], actualTypes [i]);
			}

			count = iterator.NextSubpath (path2, out isClosed);
			Assert.AreEqual (2, count);
			Assert.IsFalse (isClosed);

			actualPoints = path2.PathPoints;
			actualTypes = path2.PathTypes;

			expectedPoints = new PointF [] {new PointF(400f, 400f), 
											new PointF(400f, 10f)};
			
			for(int i = 0; i < expectedPoints.Length; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], actualPoints [i]);
			}

			expectedTypes = new byte [] {	(byte) PathPointType.Start, 
											(byte) PathPointType.Line};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], actualTypes [i]);
			}

			count = iterator.NextSubpath (path2, out isClosed);
			Assert.AreEqual (0, count);

			count = iterator.NextSubpath (path2, out isClosed);
			Assert.AreEqual (0, count);
			Assert.IsTrue (isClosed);
			Assert.AreEqual (2, path2.PointCount);

			actualPoints = path2.PathPoints;
			actualTypes = path2.PathTypes;

			expectedPoints = new PointF [] {new PointF(400f, 400f), 
											new PointF(400f, 10f)};
			
			for(int i = 0; i < expectedPoints.Length; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], actualPoints [i]);
			}

			expectedTypes = new byte [] {	(byte) PathPointType.Start, 
											(byte) PathPointType.Line};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], actualTypes [i]);
			}

			path = new GraphicsPath ();
			path.AddBezier( 10, 10, 50, 250, 100, 5, 200, 280);
			iterator = new GraphicsPathIterator (path);
			
			path2 = new GraphicsPath ();			
			count = iterator.NextSubpath (path2, out isClosed);
			Assert.AreEqual (4, count);
			Assert.IsFalse (isClosed);
			
			path2 = new GraphicsPath ();
			count = iterator.NextSubpath (path2, out isClosed);
			Assert.AreEqual (0, count);
			Assert.IsTrue (isClosed);
        }
        
        [Test]
        public virtual void NextPathType() 
		{
            GraphicsPath path = new GraphicsPath ();
			path.AddLine (new Point (100, 100), new Point (400, 100));
			path.AddBezier( 100, 100, 500, 250, 100, 50, 250, 280);
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

			GraphicsPathIterator iterator = new GraphicsPathIterator (path);

			byte pathType;
			int start;
			int end;
			bool isClosed;

			int count = iterator.NextPathType (out pathType, out start, out end);
			Assert.AreEqual (0, count);
			Assert.AreEqual ((byte)PathPointType.Start, pathType);
			Assert.AreEqual (0, start);
			Assert.AreEqual (0, end);

			iterator.NextSubpath (out start, out end, out isClosed);
			count = iterator.NextPathType (out pathType, out start, out end);
			Assert.AreEqual (3, count);
			Assert.AreEqual ((byte)PathPointType.Line, pathType);
			Assert.AreEqual (0, start);
			Assert.AreEqual (2, end);

			count = iterator.NextPathType (out pathType, out start, out end);
			Assert.AreEqual (4, count);
			Assert.AreEqual ((byte)PathPointType.Bezier3, pathType);
			Assert.AreEqual (2, start);
			Assert.AreEqual (5, end);

			count = iterator.NextPathType (out pathType, out start, out end);
			Assert.AreEqual (3, count);
			Assert.AreEqual ((byte)PathPointType.Line, pathType);
			Assert.AreEqual (5, start);
			Assert.AreEqual (7, end);
			
			// we don't want to be a bug compliant with .net
			/* 
			count = iterator.NextPathType (out pathType, out start, out end);
			Assert.AreEqual (0, count);
			Assert.AreEqual ((byte)PathPointType.Line, pathType);
			Assert.AreEqual (5, start);
			Assert.AreEqual (7, end);
			*/

			iterator.NextSubpath (out start, out end, out isClosed);
			count = iterator.NextPathType (out pathType, out start, out end);
			Assert.AreEqual (4, count);
			Assert.AreEqual ((byte)PathPointType.Bezier3, pathType);
			Assert.AreEqual (8, start);
			Assert.AreEqual (11, end);

			iterator.NextSubpath (out start, out end, out isClosed);
			count = iterator.NextPathType (out pathType, out start, out end);
			Assert.AreEqual (4, count);
			Assert.AreEqual ((byte)PathPointType.Line, pathType);
			Assert.AreEqual (12, start);
			Assert.AreEqual (15, end);

			iterator.NextSubpath (out start, out end, out isClosed);
			count = iterator.NextPathType (out pathType, out start, out end);
			Assert.AreEqual (2, count);
			Assert.AreEqual ((byte)PathPointType.Line, pathType);
			Assert.AreEqual (16, start);
			Assert.AreEqual (17, end);

			iterator.NextSubpath (out start, out end, out isClosed);
			count = iterator.NextPathType (out pathType, out start, out end);
			Assert.AreEqual (0, count);
			Assert.AreEqual ((byte)PathPointType.Line, pathType);
			Assert.AreEqual (0, start);
			Assert.AreEqual (0, end);
        }
        
        [Test]
        public virtual void NextMarker_Int32_Int32() 
		{
            GraphicsPath path = new GraphicsPath ();
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

			GraphicsPathIterator iterator = new GraphicsPathIterator (path);

			int start;
			int end;
			int count = iterator.NextMarker (out start, out end);
			Assert.AreEqual (4, count);
			Assert.AreEqual (0, start);
			Assert.AreEqual (3, end);
			
			count = iterator.NextMarker (out start, out end);
			Assert.AreEqual (4, count);
			Assert.AreEqual (4, start);
			Assert.AreEqual (7, end);

			count = iterator.NextMarker (out start, out end);
			Assert.AreEqual (4, count);
			Assert.AreEqual (8, start);
			Assert.AreEqual (11, end);

			count = iterator.NextMarker (out start, out end);
			Assert.AreEqual (2, count);
			Assert.AreEqual (12, start);
			Assert.AreEqual (13, end);

			// FIXME - should return all 0'z?
			/*
			count = iterator.NextMarker (out start, out end);
			Assert.AreEqual (0, count);
			Assert.AreEqual (12, start);
			Assert.AreEqual (13, end);
			*/
        }

		[Test]
		public void NextSubpath_NextMarker()
		{
			GraphicsPath path = new GraphicsPath();
			
			path.AddLine (10, 10, 50, 50); // figure #1
			path.AddLine (50, 50, 80, 80);
			path.AddLine (90, 90, 100, 100);
			path.SetMarkers (); // marker #1
			path.AddLine (150, 150, 180, 180);
			path.SetMarkers (); // marker #2
			path.StartFigure (); // figure #2
			path.SetMarkers (); // marker #3 is actually marker #2
			path.AddRectangle (new Rectangle (200, 200, 200, 200)); 
			path.SetMarkers (); // marker #4
			path.AddLine (150, 150, 180, 180); 
			path.StartFigure (); // figure #3
			path.AddBezier (400, 400, 500, 500, 600, 600, 700, 700);
			path.AddBezier (450, 450, 550, 550, 650, 650, 750, 750);

			GraphicsPathIterator iterator = new GraphicsPathIterator (path);

			int start;
			int end;
			bool isClosed;
			int count = iterator.NextMarker (out start,out end); // marker #1
			Assert.AreEqual (5, count);
			Assert.AreEqual (0, start);
			Assert.AreEqual (4, end);

			count = iterator.NextSubpath (out start,out end,out isClosed); // figure #1
			Assert.AreEqual (7, count);
			Assert.AreEqual (0, start);
			Assert.AreEqual (6, end);
			Assert.AreEqual (false, isClosed);

			count = iterator.NextMarker (out start,out end); // marker #2 (and #3)
			Assert.AreEqual (2, count);
			Assert.AreEqual (5, start);
			Assert.AreEqual (6, end);

			count = iterator.NextSubpath (out start,out end,out isClosed); // figure #2
			Assert.AreEqual (4, count);
			Assert.AreEqual (7, start);
			Assert.AreEqual (10, end);
			Assert.AreEqual (true, isClosed);

			count = iterator.NextSubpath (out start,out end,out isClosed); // figure #3
			Assert.AreEqual (2, count);
			Assert.AreEqual (11, start);
			Assert.AreEqual (12, end);
			Assert.AreEqual (false, isClosed);

			count = iterator.NextMarker (out start,out end); // marker #5 (end)
			Assert.AreEqual (4, count);
			Assert.AreEqual (7, start);
			Assert.AreEqual (10, end);

			count = iterator.NextMarker (out start,out end); // marker #5 (end)
			Assert.AreEqual (10, count);
			Assert.AreEqual (11, start);
			Assert.AreEqual (20, end);

			// we dont want to be bug compliant with .net
			/*
			count = iterator.NextMarker (out start,out end); // no more markers
			Assert.AreEqual (0, count);
			Assert.AreEqual (11, start);
			Assert.AreEqual (20, end);
			*/

			count = iterator.NextSubpath (out start,out end,out isClosed); // figure #4
			Assert.AreEqual (8, count);
			Assert.AreEqual (13, start);
			Assert.AreEqual (20, end);
			Assert.AreEqual (false, isClosed);

			// we dont want to be bug compliant with .net
			/*
			count = iterator.NextMarker (out start,out end); // no more markers
			Assert.AreEqual (0, count);
			Assert.AreEqual (13, start);
			Assert.AreEqual (20, end);
			*/

			count = iterator.NextSubpath (out start,out end,out isClosed); // no more figures
			Assert.AreEqual (0, count);
			Assert.AreEqual (0, start);
			Assert.AreEqual (0, end);
			Assert.AreEqual (true, isClosed);

			count = iterator.NextMarker (out start,out end); // no more markers
			Assert.AreEqual (0, count);
			Assert.AreEqual (0, start);
			Assert.AreEqual (0, end);			
		}

        
        [Test]
        public virtual void NextMarker_GraphicsPath() 
		{
            GraphicsPath path = new GraphicsPath ();
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

			GraphicsPath path2 = new GraphicsPath ();
			path.AddLine (new Point (150, 150), new Point (450, 150));
			path.AddLine (new Point (450, 250), new Point (50, 150));

			GraphicsPathIterator iterator = new GraphicsPathIterator (path);

			iterator.NextMarker (null);
			iterator.NextMarker (path2);

			Assert.AreEqual (4, path2.PointCount);
			PointF [] actualPoints = path2.PathPoints;
			byte [] actualTypes = path2.PathTypes;

			PointF [] expectedPoints = new PointF [] {	new PointF(100f, 100f), 
														new PointF(400f, 100f), 
														new PointF(400f, 200f), 
														new PointF(10f, 100f)};
			
			for(int i = 0; i < expectedPoints.Length; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], actualPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line | PathPointType.PathMarker)};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], actualTypes [i]);
			}

			iterator.NextMarker (null);
			iterator.NextMarker (null);
			iterator.NextMarker (null);
			iterator.NextMarker (path2);

			Assert.AreEqual (4, path2.PointCount);
			actualPoints = path2.PathPoints;
			actualTypes = path2.PathTypes;

			expectedPoints = new PointF [] {new PointF(10f, 10f), 
											new PointF(50f, 250f), 
											new PointF(100f, 5f), 
											new PointF(200f, 280f)};
			
			for(int i = 0; i < expectedPoints.Length; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], actualPoints [i]);
			}

			expectedTypes = new byte [] {	(byte) PathPointType.Start, 
												(byte) PathPointType.Bezier3, 
												(byte) PathPointType.Bezier3, 
												(byte) (PathPointType.Bezier3 | PathPointType.PathMarker)};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], actualTypes [i]);
			}	
			
        }
        
        [Test]
        public virtual void Count() 
		{
            GraphicsPath path = new GraphicsPath ();

			GraphicsPathIterator iterator = new GraphicsPathIterator (path);
			Assert.AreEqual (0, iterator.Count);

			path.AddLine (new Point (100, 100), new Point (400, 100));
			path.AddLine (new Point (400, 200), new Point (10, 100));

			path.StartFigure ();
			path.AddBezier( 10, 10, 50, 250, 100, 5, 200, 280);
			path.StartFigure ();
			path.AddRectangle (new Rectangle (10, 20, 300, 400));

			path.StartFigure ();
			path.AddLine (new Point (400, 400), new Point (400, 10));

			iterator = new GraphicsPathIterator (path);
			Assert.AreEqual (14, iterator.Count);
        }
        
        [Test]
        public virtual void SubpathCount() 
		{
            GraphicsPath path = new GraphicsPath ();

			GraphicsPathIterator iterator = new GraphicsPathIterator (path);
			Assert.AreEqual (0, iterator.SubpathCount);

			path.AddLine (new Point (100, 100), new Point (400, 100));
			path.AddLine (new Point (400, 200), new Point (10, 100));

			path.StartFigure ();
			path.AddBezier( 10, 10, 50, 250, 100, 5, 200, 280);
			path.StartFigure ();
			path.AddRectangle (new Rectangle (10, 20, 300, 400));

			path.StartFigure ();
			path.AddLine (new Point (400, 400), new Point (400, 10));

			iterator = new GraphicsPathIterator (path);
			Assert.AreEqual (4, iterator.SubpathCount);
        }
        
        [Test]
        public virtual void HasCurve() 
		{
            GraphicsPath path = new GraphicsPath ();

			GraphicsPathIterator iterator = new GraphicsPathIterator (path);
			Assert.IsFalse (iterator.HasCurve ());

			path.AddLine (new Point (100, 100), new Point (400, 100));
			path.AddLine (new Point (400, 200), new Point (10, 100));

			iterator = new GraphicsPathIterator (path);
			Assert.IsFalse (iterator.HasCurve ());

			path.StartFigure ();
			path.AddBezier( 10, 10, 50, 250, 100, 5, 200, 280);
			path.StartFigure ();
			path.AddRectangle (new Rectangle (10, 20, 300, 400));

			path.StartFigure ();
			path.AddLine (new Point (400, 400), new Point (400, 10));

			iterator = new GraphicsPathIterator (path);
			Assert.IsTrue (iterator.HasCurve ());
        }
        
        [Test]
        public virtual void Rewind() 
		{
            GraphicsPath path = new GraphicsPath ();
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

			GraphicsPathIterator iterator = new GraphicsPathIterator (path);

			int i;
			int j;
			iterator.NextMarker (out i, out j);
			iterator.NextMarker (out i, out j);

			iterator.Rewind ();
			iterator.NextMarker (out i, out j);

			Assert.AreEqual (0, i);
			Assert.AreEqual (3, j);
        }
        
        [Test]
        public virtual void Enumerate() 
		{
            GraphicsPath path = new GraphicsPath ();
			path.AddLine (new Point (100, 100), new Point (400, 100));
			path.AddLine (new Point (400, 200), new Point (10, 100));

			path.StartFigure ();
			path.AddBezier( 10, 10, 50, 250, 100, 5, 200, 280);
			path.StartFigure ();
			path.AddRectangle (new Rectangle (10, 20, 300, 400));

			path.StartFigure ();
			path.AddLine (new Point (400, 400), new Point (400, 10));

			path.Reverse ();

			GraphicsPathIterator iterator = new GraphicsPathIterator (path);
			PointF [] actualPoints = new PointF [14];
			byte [] actualTypes = new byte [14];
			iterator.Enumerate (ref actualPoints, ref actualTypes);

			PointF [] expectedPoints = new PointF [] {	new PointF(400f, 10f), 
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
			
			for(int i = 0; i < expectedPoints.Length; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], actualPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line | PathPointType.CloseSubpath), 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Bezier3, 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], actualTypes [i]);
			}	
        }
        
        [Test]
        public virtual void CopyData() 
		{
            GraphicsPath path = new GraphicsPath ();
			path.AddLine (new Point (100, 100), new Point (400, 100));
			path.AddLine (new Point (400, 200), new Point (10, 100));
			path.StartFigure ();
			path.SetMarkers ();
			path.AddRectangle (new Rectangle (10, 20, 300, 400));
			path.StartFigure ();
			path.SetMarkers ();
			path.AddLine (new Point (400, 400), new Point (400, 10));

			GraphicsPathIterator pathIterator = new GraphicsPathIterator(path);
			pathIterator.Rewind ();
			PointF [] actualPoints = new PointF [10];
			byte [] actualTypes = new byte [10];
			pathIterator.CopyData (ref actualPoints, ref actualTypes, 0, 9);

			PointF [] expectedPoints = new PointF [] {	new PointF(100f, 100f), 
														new PointF(400f, 100f), 
														new PointF(400f, 200f), 
														new PointF(10f, 100f), 
														new PointF(10f, 20f), 
														new PointF(310f, 20f), 
														new PointF(310f, 420f), 
														new PointF(10f, 420f), 
														new PointF(400f, 400f), 
														new PointF(400f, 10f)};
			
			for(int i = 0; i < expectedPoints.Length; i++) {
				DrawingTest.AssertAlmostEqual(expectedPoints [i], actualPoints [i]);
			}

			byte [] expectedTypes = new byte [] {	(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line | PathPointType.PathMarker), 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line, 
													(byte) PathPointType.Line, 
													(byte) (PathPointType.Line | PathPointType.CloseSubpath | PathPointType.PathMarker), 
													(byte) PathPointType.Start, 
													(byte) PathPointType.Line};

			for (int i=0; i < expectedTypes.Length; i++) {
				Assert.AreEqual (expectedTypes [i], actualTypes [i]);
			}	
        }
    }
}

