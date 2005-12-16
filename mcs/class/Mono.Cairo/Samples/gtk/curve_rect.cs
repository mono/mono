//
//
//	Mono.Cairo drawing samples using GTK# as drawing surface
//	       Hisham Mardam Bey <hisham@hisham.cc>
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
using System.Reflection;
using System.Runtime.InteropServices;
using Cairo;
using Gtk;

public class GtkCairo
{
	static DrawingArea a;

	static void Main ()
	{
		Application.Init ();
		Gtk.Window w = new Gtk.Window ("Mono.Cairo Circles demo");

		a = new CairoGraphic ();

		Box box = new HBox (true, 0);
		box.Add (a);
		w.Add (box);
		w.Resize (500,500);
		w.ShowAll ();

		Application.Run ();
	}

}

public class CairoGraphic : DrawingArea
{
	static void draw (Cairo.Context gr, int width, int height)
	{
		double x0 = 0.1;
		double y0 = 0.1;
		double rect_width  = 0.8;
		double rect_height = 0.8;
		double radius = 0.4;

		double x1,y1;

		gr.Scale (width, height);
		gr.LineWidth = 0.04;

		x1=x0+rect_width;
		y1=y0+rect_height;
		if (rect_width == 0 || rect_height == 0)
		  return;

		if (rect_width/2<radius) {
			if (rect_height/2<radius) {
				gr.MoveTo( new PointD(x0, (y0 + y1)/2) );

				gr.CurveTo ( new PointD (x0 ,y0),
					     new PointD (x0, y0),
					     new PointD ((x0 + x1)/2, y0)
					     );

				gr.CurveTo ( new PointD (x1, y0 ),
					     new PointD (x1, y0 ),
					     new PointD (x1, (y0 + y1)/2)
					     );

				gr.CurveTo ( new PointD (x1, y1),
					     new PointD (x1, y1),
					     new PointD ((x1 + x0)/2, y1)
					     );

				gr.CurveTo ( new PointD (x0, y1),
					     new PointD (x0, y1),
					     new PointD (x0, (y0 + y1)/2)
					     );
			}
			else {
				gr.MoveTo ( new PointD (x0, y0 + radius) );

				gr.CurveTo ( new PointD (x0 ,y0),
					     new PointD (x0, y0),
					     new PointD ((x0 + x1)/2, y0)
					     );

				gr.CurveTo ( new PointD (x1, y0),
					     new PointD (x1, y0),
					     new PointD (x1, y0 + radius)
					     );

				gr.LineTo ( new PointD (x1 , y1 - radius) );

				gr.CurveTo ( new PointD (x1, y1),
					     new PointD (x1, y1),
					     new PointD ((x1 + x0)/2, y1)
					     );

				gr.CurveTo ( new PointD (x0, y1),
					     new PointD (x0, y1),
					     new PointD (x0, y1- radius)
					     );
			}
		}
		else {
			if (rect_height/2<radius) {
				gr.MoveTo ( new PointD (x0, (y0 + y1)/2) );

				gr.CurveTo ( new PointD (x0 , y0),
					     new PointD (x0 , y0),
					     new PointD (x0 + radius, y0)
					     );

				gr.LineTo ( new PointD (x1 - radius, y0) );

				gr.CurveTo ( new PointD (x1, y0),
					     new PointD (x1, y0),
					     new PointD (x1, (y0 + y1)/2)
					     );

				gr.CurveTo ( new PointD (x1, y1),
					     new PointD (x1, y1),
					     new PointD (x1 - radius, y1)
					     );

				gr.LineTo ( new PointD (x0 + radius, y1) );

				gr.CurveTo ( new PointD ( x0, y1),
					     new PointD (x0, y1),
					     new PointD (x0, (y0 + y1)/2)
					     );
			}
			else {
				gr.MoveTo  ( new PointD (x0, y0 + radius) );

				gr.CurveTo ( new PointD (x0 , y0),
					     new PointD (x0 , y0),
					     new PointD (x0 + radius, y0)
					     );

				gr.LineTo ( new PointD (x1 - radius, y0) );

				gr.CurveTo ( new PointD (x1, y0),
					     new PointD (x1, y0),
					     new PointD (x1, y0 + radius)
					     );

				gr.LineTo ( new PointD (x1 , y1 - radius) );

				gr.CurveTo ( new PointD ( x1, y1),
					     new PointD (x1, y1),
					     new PointD (x1 - radius, y1)
					     );

				gr.LineTo ( new PointD (x0 + radius, y1) );
				gr.CurveTo ( new PointD ( x0, y1),
					     new PointD (x0, y1),
					     new PointD (x0, y1- radius)
					     );
			}
		}

		gr.Color = new Color (0.5,0.5,1, 1);
		gr.FillPreserve ();
		gr.Color = new Color(0.5, 0, 0, 0.5);
		gr.Stroke ();
	}

	protected override bool OnExposeEvent (Gdk.EventExpose args)
	{
                Gdk.Window win = args.Window;
		//Gdk.Rectangle area = args.Area;

		Cairo.Context g = Gdk.Context.CreateDrawable (win);

		int x, y, w, h, d;
		win.GetGeometry(out x, out y, out w, out h, out d);

		draw (g, w, h);
		return true;
	}
}

