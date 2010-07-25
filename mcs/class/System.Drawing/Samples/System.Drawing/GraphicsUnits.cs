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
//
// Author:
//	Jordi Mas <jordimash@gmail.com>
//
// Graphics PageUnit test sample
//

using System;
using System.Drawing;
using System.IO;
using System.Drawing.Printing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

public class PrintingMargins
{

	public static void Main (string[] args)
	{
		Bitmap	bmp = new Bitmap (600, 600);
		Graphics gr = Graphics.FromImage (bmp);
		Rectangle rect = new Rectangle (20, 20, 100, 100);

		PointF[] polygon_pnts = {new PointF(150.0F,  150.0F),
			new PointF(200.0F,  125.0F), new PointF(300.0F, 105.0F),
			new PointF(350.0F, 150.0F), new PointF(400.0F, 200.0F),
			new PointF(450.0F, 300.0F), new PointF(350.0F, 350.0F) };


		// Default Display
    		gr.DrawRectangle (Pens.Red, rect);
    		gr.DrawString ("Unit " + gr.PageUnit, new Font ("Arial", 10), Brushes.Red, 50, 50);
    		gr.DrawArc (Pens.Red, 30, 30, 60, 60, 0, 180);
    		gr.DrawPolygon (Pens.Red, polygon_pnts);

    		// Point
    		gr.PageUnit = GraphicsUnit.Point;
    		gr.DrawRectangle (Pens.Yellow, rect);
    		gr.DrawString ("Unit " + gr.PageUnit, new Font ("Arial", 10), Brushes.Yellow, 50, 50);
    		gr.DrawArc (Pens.Yellow, 30, 30, 60, 60, 0, 180);
    		gr.DrawPolygon (Pens.Yellow, polygon_pnts);

    		// Document
    		gr.PageUnit = GraphicsUnit.Document;
    		gr.DrawRectangle (Pens.Pink, rect);
    		gr.DrawString ("Unit " + gr.PageUnit, new Font ("Arial", 10), Brushes.Pink, 50, 50);
    		gr.DrawArc (Pens.Pink, 30, 30, 60, 60, 0, 180);
    		gr.DrawPolygon (Pens.Pink, polygon_pnts);

    		// Inc
    		gr.PageUnit = GraphicsUnit.Inch;
    		gr.DrawRectangle (Pens.Blue, 3f, 1f, 1f, 1f);
    		gr.DrawString ("Unit " + gr.PageUnit, new Font ("Arial", 10), Brushes.Blue, 0.7f, 0.7f);
    		gr.DrawArc (Pens.Blue, 3f, 3f, 1f, 1f, 0, 180);


       		bmp.Save ("units1.bmp");
       		bmp.Dispose ();
       		gr.Dispose ();

       		bmp = new Bitmap (600, 600);
		gr = Graphics.FromImage (bmp);

		GraphicsPath graphPath = new GraphicsPath();
    		graphPath.AddEllipse (0, 80, 100, 200);

		// Default Display
    		gr.DrawBezier (Pens.Red, new Point (10, 10), new Point (20, 10),
    			new Point (35, 50), new Point (50, 10));

    		gr.DrawEllipse (Pens.Red, 10, 50, 30, 50);
    		gr.DrawPath (Pens.Red, graphPath);
    		gr.DrawPie (Pens.Red, 150, 20, 60, 60, 100, 140);
    		gr.DrawCurve (Pens.Red, polygon_pnts, 2, 4, 0.5f);


    		// Point
    		gr.PageUnit = GraphicsUnit.Display;
    		gr.PageUnit = GraphicsUnit.Point;
    		gr.DrawBezier (Pens.Pink, new Point (10, 10), new Point (20, 10),
    			new Point (35, 50), new Point (50, 10));
    		gr.DrawCurve (Pens.Pink, polygon_pnts, 2, 4, 0.5f);

    		gr.DrawEllipse (Pens.Pink, 10, 50, 30, 50);
    		gr.DrawPath (Pens.Pink, graphPath);
    		gr.DrawPie (Pens.Pink, 150, 20, 60, 60, 100, 140);

    		// Document
    		gr.PageUnit = GraphicsUnit.Document;
    		gr.DrawBezier (Pens.Yellow, new Point (10, 10), new Point (20, 10),
    			new Point (35, 50), new Point (50, 10));

    		gr.DrawEllipse (Pens.Yellow, 10, 50, 30, 50);
    		gr.DrawPath (Pens.Yellow, graphPath);
    		gr.DrawPie (Pens.Yellow, 150, 20, 60, 60, 100, 140);
    		gr.DrawCurve (Pens.Yellow, polygon_pnts, 2, 4, 0.5f);

    		// Inc
    		gr.PageUnit = GraphicsUnit.Inch;
    		gr.DrawBezier (Pens.Blue, new Point (10, 10), new Point (20, 10),
    			new Point (35, 50), new Point (50, 10));

    		gr.DrawEllipse (Pens.Blue, 10, 50, 30, 50);
    		gr.DrawPath (Pens.Blue, graphPath);
    		gr.DrawPie (Pens.Blue, 150, 20, 60, 60, 100, 140);
    		gr.DrawCurve (Pens.Blue, polygon_pnts, 2, 4, 0.5f);

		bmp.Save ("units2.bmp");
        }
}


