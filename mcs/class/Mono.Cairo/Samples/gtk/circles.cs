//
//
//	Mono.Cairo drawing samples using GTK# as drawing surface
//	Autor: Jordi Mas <jordi@ximian.com>. Based on work from Owen Taylor
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
        static readonly double  M_PI = 3.14159265358979323846;
   
	static void oval_path (Cairo.Context gr, double xc, double yc, double xr, double yr)
	{
		gr.Translate (xc, yc);
		gr.Scale (1.0, yr / xr);
		gr.MoveTo (new PointD (xr, 0.0) );
		gr.Arc (0, 0, xr, 0, 2 * M_PI);
		gr.ClosePath ();
	}
	
	/* 
	* Draw a red, green, and blue circle equally spaced inside
	* the larger circle of radius r at (xc, yc)
	*/
	static void draw_3circles (Cairo.Context gr, double xc, double yc, double radius)
	{
		double subradius = radius * (2 / 3.0 - 0.1);
		
		gr.Color =  new Color (1, 0, 0, 0.5);
		
		oval_path (gr,
			xc + radius / 3 * Math.Cos (M_PI * (0.5)),
			yc - radius / 3 * Math.Sin (M_PI * (0.5)),
			subradius, subradius);
			
		gr.Fill ();
		
		gr.Color  = new Color (0, 1, 0, 0.5);
		oval_path (gr,
			xc + radius / 3 * Math.Cos (M_PI * (0.5 + 2/.3)),
			yc - radius / 3 * Math.Sin (M_PI * (0.5 + 2/.3)),
			subradius, subradius);
			
		gr.Fill ();
		
		gr.Color = new Color (0, 0, 1, 0.5);
		
		oval_path (gr,
			xc + radius / 3 * Math.Cos (M_PI * (0.5 + 4/.3)),
			yc - radius / 3 * Math.Sin (M_PI * (0.5 + 4/.3)),
			subradius, subradius);
			
		gr.Fill ();
	}

	static void draw (Cairo.Context gr, int width, int height)
	{
		Surface overlay, punch, circles;
		
		/* Fill the background */
		double radius = 0.5 * (width < height ? width : height) - 10;
		double xc = width / 2;
		double yc = height / 2;
		
		Surface target = gr.Target;
		overlay = target.CreateSimilar (Content.ColorAlpha, width, height);		
		punch = target.CreateSimilar (Content.Alpha, width, height);		
		circles = target.CreateSimilar (Content.ColorAlpha, width, height);
		
		gr.Save ();
		gr.Target = overlay;
		
		/* Draw a black circle on the overlay
		*/
		
		gr.Color = new Color (0, 0, 0, 1);
		
		oval_path (gr, xc, yc, radius, radius);
		gr.Fill ();		
		gr.Save ();
		gr.Target =  punch;
		
		
		/* Draw 3 circles to the punch surface, then cut
		* that out of the main circle in the overlay
		*/
		draw_3circles (gr, xc, yc, radius);
		
		gr.Restore ();
		
		gr.Operator = Operator.DestOut;
		punch.Show (gr, width, height);
		
		
		/* Now draw the 3 circles in a subgroup again
		* at half intensity, and use OperatorAdd to join up
		* without seams.
		*/
		gr.Save ();
		gr.Target =  circles;
		
		//gr.Alpha = 0.5;
		gr.Operator = Operator.Over;
		draw_3circles (gr, xc, yc, radius);
		
		gr.Restore ();
		
		gr.Operator = Operator.Add;
		circles.Show (gr, width, height);
	
		
		gr.Restore ();
		
		overlay.Show (gr, width, height);				
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

