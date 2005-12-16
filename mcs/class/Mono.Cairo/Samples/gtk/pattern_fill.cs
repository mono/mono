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
        static readonly double  M_PI = 3.14159265358979323846;
   
	static void draw (Cairo.Context gr, int width, int height)
	{		
		//gr.Scale (width, height);
		//gr.LineWidth = 0.04;
						
                double X_FUZZ = 0.08;
		double Y_FUZZ = 0.08;
		
		double X_INNER_RADIUS = 0.3;
		double Y_INNER_RADIUS = 0.2;
		
		double X_OUTER_RADIUS = 0.45;
		double Y_OUTER_RADIUS = 0.35;
		
		double SPIKES = 10;
		
		int i;
		double x;
		double y;
		string text = "KAPOW!";
		
		cairo_text_extents_t extents;
		
		srand (45);
		cairo_set_line_width (cr, 0.01);
		
		for (i = 0; i < SPIKES * 2; i++) {
			
			x = 0.5 + cos (M_PI * i / SPIKES) * X_INNER_RADIUS +
			  (double) rand() * X_FUZZ / RAND_MAX;
			y = 0.5 + sin (M_PI * i / SPIKES) * Y_INNER_RADIUS +
			  (double) rand() * Y_FUZZ / RAND_MAX;
			
			if (i == 0)
			  cairo_move_to (cr, x, y);
			else
			  cairo_line_to (cr, x, y);
			
			i++;
			
			x = 0.5 + cos (M_PI * i / SPIKES) * X_OUTER_RADIUS +
			  (double) rand() * X_FUZZ / RAND_MAX;
			y = 0.5 + sin (M_PI * i / SPIKES) * Y_OUTER_RADIUS +
			  (double) rand() * Y_FUZZ / RAND_MAX;
			
			cairo_line_to (cr, x, y);
		}
		
		cairo_close_path (cr);
		cairo_stroke (cr);
		
		
		cairo_select_font_face (cr, "Sans",
					CAIRO_FONT_SLANT_NORMAL,
					CAIRO_FONT_WEIGHT_BOLD);
		
		cairo_move_to (cr, x, y);
		cairo_text_path (cr, text);
		
		
		
		cairo_set_font_size (cr, 0.2);
		cairo_text_extents (cr, text, &extents);
		x = 0.5-(extents.width/2 + extents.x_bearing);
		y = 0.5-(extents.height/2 + extents.y_bearing);
		
		
		cairo_set_source_rgb (cr, 1 , 1, 0.5);
		cairo_fill (cr);
		
		cairo_move_to (cr, x, y);
		cairo_text_path (cr, text);
		cairo_set_source_rgb (cr, 0 , 0, 0);
		cairo_stroke (cr);
		
		
		
		
		
		
		
		
		
		gr.Arc (xc, yc, radius, angle1, angle2);
		gr.Stroke ();
		
		/* draw helping lines */
		gr.Color = new Color(1, 0.2, 0.2, 0.6);
		gr.Arc (xc, yc, 0.05, 0, 2*M_PI);
		gr.Fill ();
		gr.LineWidth = 0.03;
		gr.Arc (xc, yc, radius, angle1, angle1);
		gr.LineTo (new PointD(xc, yc));
		gr.Arc (xc, yc, radius, angle2, angle2);
		gr.LineTo (new PointD(xc, yc));
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

