//
// Shapes.cs
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Samples.Common {

	public class Shapes {

		static public object[] GetList ()
		{
			return new object[] {
				"Arc",
				"Bezier",
				"Beziers",
				"Closed Curve",
				"Curve",
				"Ellipse",
				"Line",
				"Lines",
				"Pie",
				"Polygon",
				"Rectangle",
				"Rectangles",
				"String",
				"Complex (AddPath)"
			};
		}

		static public GraphicsPath GetShape (int index)
		{
			switch (index) {
			case 0:
				return Arc ();
			case 1:
				return Bezier ();
			case 2:
				return Beziers ();
			case 3:
				return ClosedCurve ();
			case 4:
				return Curve ();
			case 5:
				return Ellipse ();
			case 6:
				return Line ();
			case 7:
				return Lines ();
			case 8:
				return Pie ();
			case 9:
				return Polygon ();
			case 10:
				return Rectangle ();
			case 11:
				return Rectangles ();
			case 12:
				return String ();
			case 13:
				return Complex ();
			default:
				// nothing to show
				return null;
			}
		}

		static private GraphicsPath Arc ()
		{
			GraphicsPath path = new GraphicsPath ();
			path.AddArc (20, 20, 200, 200, 60, 120);
			return path;
		}

		static private GraphicsPath Bezier ()
		{
			GraphicsPath path = new GraphicsPath ();
			path.AddBezier (
				new Point (20, 100), new Point (70, 10),
				new Point (130, 200), new Point (180, 100)
				);
			return path;
		}

		static private GraphicsPath Beziers ()
		{
			GraphicsPath path = new GraphicsPath ();
			path.AddBeziers (new Point[7] { 
				new Point (20, 100), new Point (70, 10),
				new Point (130, 200), new Point (180, 100),
				new Point (200, 100), new Point (240, 240),
				new Point (20, 100)
				});
			return path;
		}

		static private GraphicsPath ClosedCurve ()
		{
			GraphicsPath path = new GraphicsPath ();
			path.AddClosedCurve (new Point[4] { 
				new Point (20, 100), new Point (70, 10),
				new Point (130, 200), new Point (180, 100)
				});
			return path;
		}

		static private GraphicsPath Curve ()
		{
			GraphicsPath path = new GraphicsPath ();
			path.AddCurve (new Point[4] { 
				new Point (20, 100), new Point (70, 10),
				new Point (130, 200), new Point (180, 100)
				});
			return path;
		}

		static private GraphicsPath Ellipse ()
		{
			GraphicsPath path = new GraphicsPath ();
			path.AddEllipse (20, 20, 200, 100);
			return path;
		}

		static private GraphicsPath Line ()
		{
			GraphicsPath path = new GraphicsPath ();
			path.AddLine (20, 20, 200, 100);
			return path;
		}

		static private GraphicsPath Lines ()
		{
			GraphicsPath path = new GraphicsPath ();
			path.AddLines (new Point[4] { 
				new Point (20, 100), new Point (70, 10),
				new Point (130, 200), new Point (180, 100)
				});
			return path;
		}

		static private GraphicsPath Pie ()
		{
			GraphicsPath path = new GraphicsPath ();
			path.AddPie (20, 20, 200, 200, 60, 120);
			return path;
		}

		static private GraphicsPath Polygon ()
		{
			GraphicsPath path = new GraphicsPath ();
			path.AddPolygon (new Point[4] { 
				new Point (20, 100), new Point (70, 10),
				new Point (130, 200), new Point (180, 100)
				});
			return path;
		}

		static private GraphicsPath Rectangle ()
		{
			GraphicsPath path = new GraphicsPath ();
			path.AddRectangle (new Rectangle (20, 20, 200, 200));
			return path;
		}

		static private GraphicsPath Rectangles ()
		{
			GraphicsPath path = new GraphicsPath ();
			path.AddRectangles (new Rectangle[2] {
				new Rectangle (20, 20, 100, 100),
				new Rectangle (100, 100, 20, 20)
				});
			return path;
		}

		static private GraphicsPath String ()
		{
			GraphicsPath path = new GraphicsPath ();
			try {
				path.AddString ("Mono", FontFamily.GenericMonospace, 0, 10f, new Point (20, 20), StringFormat.GenericDefault);
			}
			catch (NotImplementedException) {
				// not implemented in libgdiplus
			}
			return path;
		}

		static private GraphicsPath Complex ()
		{
			GraphicsPath path = new GraphicsPath ();
			path.AddPath (Pie (), false);
			path.AddPath (Rectangle (), true);
			path.AddPath (Polygon (), false);
			return path;
		}
	}
}
