//
// A sample application for some graphics.cs functions implementation
//
// Author:
//   Jordi Mas i Hernàndez, jordi@ximian.com
//
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Drawing.Imaging;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace MonoSamples.System.Drawing
{
	public class graphicsUI
	{
		public static void Main () {

		        Bitmap bmp = new Bitmap (500, 250);
		 	Graphics dc = Graphics.FromImage (bmp);

			Pen BluePen = new Pen (Color.Blue, 3);
			Pen GreenPen = new Pen (Color.Green, 3);
			Pen RedPen = new Pen (Color.Red, 3);
			SolidBrush redBrush = new SolidBrush (Color.Red);
			SolidBrush blueBrush = new SolidBrush (Color.Blue);

			int x = 0;
			int y = 0;

			/* First Row */
			dc.DrawRectangle (BluePen, x, y, 50, 50);
			x += 50;
			dc.DrawEllipse (RedPen, x, y, 70, 50);
			dc.DrawArc (BluePen, x, y, 50, 40, (float) 0, (float) 120);
			x += 70;

			dc.DrawBezier (GreenPen, new Point (x, y + 5),
				new Point (x + 50, y + 5),
				new Point (x + 20, y + 20),
				new Point (x + 50, y + 50));
			x += 50;

			PointF point1 = new PointF (10.0F + x, 10.0F);
			PointF point2 = new PointF (10.0F + x, 5.0F);
			PointF point3 = new PointF (40.0F + x, 5.0F);
			PointF point4 = new PointF (50.0F + x, 10.0F);
			PointF point5 = new PointF (60.0F + x, 20.0F);
			PointF point6 = new PointF (70.0F + x, 40.0F);
			PointF point7 = new PointF (50.0F + x, 50.0F);
			PointF[] curvePoints = {point1, point2, point3, point4,
						point5,	point6,	point7};
			dc.DrawLines (RedPen, curvePoints);
			float tension = 1.0F;
			FillMode aFillMode = FillMode.Alternate;
			dc.DrawClosedCurve (GreenPen, curvePoints, tension, aFillMode);

			x += 80;

			// FillClosedCurve
			PointF point10 = new PointF (x, y + 15.0F);
			PointF point20 = new PointF (x + 40.0F, y + 10.0F);
			PointF point30 = new PointF (x + 50.0F, y + 40.0F);
			PointF point40 = new PointF (x + 10.0F, y + 30.0F);
			PointF[] points = {point10, point20, point30, point40};
			FillMode newFillMode = FillMode.Winding;
			dc.FillClosedCurve (redBrush, points, newFillMode, tension);

			// Fill pie to screen.
			dc.FillPie (blueBrush, x, 0, 200.0F, 100.0f, 300.0F, 45.0F);

			/* second row */
			y += 80;
			x = 0;

			// Clipping and Graphics container test
			dc.SetClip (new Rectangle (5 + x, 5 + y, 75, 75));

			// Begin a graphics container.
			GraphicsContainer container = dc.BeginContainer ();

			// Set an additional clipping region for the container.
			dc.SetClip (new Rectangle (50 + x, 25 + y, 50, 37));

			// Fill a red rectangle in the container.
			dc.FillRectangle (redBrush, 0, 0, 200, 200);

			dc.EndContainer (container);
			SolidBrush blueBrushLight = new SolidBrush (
					Color.FromArgb (128, 0, 0, 255));
			dc.FillRectangle (blueBrushLight, 0, 0, 200, 200);

			dc.ResetClip ();
			Pen blackPen = new Pen (Color.FromArgb (255, 0, 0, 0), 2.0f);
			dc.DrawRectangle (blackPen, 5 + x, 5 + y, 75, 75);
			dc.DrawRectangle (blackPen, 50 + x, 25 + y, 50, 37);

			x = 100;
			y += 10;

			Point[] ptstrans = {new Point(x, y), new Point (50 + x, 25 + y)};
			dc.DrawLine (BluePen, ptstrans [0], ptstrans [1]);
			dc.TranslateTransform (40.0F, 30.0F);
			dc.TransformPoints (CoordinateSpace.Page, CoordinateSpace.World,
						ptstrans);
			dc.ResetTransform ();
			dc.DrawLine (RedPen, ptstrans [0], ptstrans [1]);

			bmp.Save ("graphicsui.bmp", ImageFormat.Bmp);
		}
	}
}
