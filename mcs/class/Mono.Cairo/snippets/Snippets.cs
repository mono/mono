using System;
using System.Reflection;
using Cairo;

namespace Cairo.Snippets
{
	public class Snippets
	{
		public static string[] snippets = {
			"arc",
			"arc_negative",
			"clip",
			"clip_image",
			"curve_rectangle",
			"curve_to",
			"fill_and_stroke",
			"fill_and_stroke2",
			"gradient",
			"image",
			"imagepattern",
			"path",
			"set_line_cap",
			"set_line_join",
			"text",
			"text_align_center",
			"text_extents",
			"xxx_clip_rectangle",
			"xxx_dash",
			"xxx_long_lines",
			"xxx_multi_segment_caps",
			"xxx_self_intersect"
		};
	
		static Type[] types = new Type[] {typeof (Context), typeof (int), typeof (int)};
		public static void InvokeSnippet (Snippets snip, string snippet, Context cr, int width, int height)
		{
			MethodInfo m = snip.GetType ().GetMethod(snippet, types);
			m.Invoke (snip, new Object[] {cr, width, height});
		}

		public void Normalize (Context cr, int width, int height)
		{
			cr.Scale (width, height);
			cr.LineWidth = 0.04;
		}
	
		public void arc(Context cr, int width, int height)
		{
			PointD c = new PointD (0.5, 0.5);
			double radius = 0.4;
			double angle1 = 45.0  * (Math.PI/180.0);  /* angles are specified */
			double angle2 = 180.0 * (Math.PI/180.0);  /* in radians           */

			Normalize(cr, width, height);

			cr.Arc(c.X, c.Y, radius, angle1, angle2);
			cr.Stroke();

			// draw helping lines
			cr.Color = new Color (1, 0.2, 0.2, 0.6);
			cr.Arc(c.X, c.Y, 0.05, 0, 2*Math.PI);
			cr.Fill();
			cr.LineWidth = 0.03;
			cr.Arc(c.X, c.Y, radius, angle1, angle1);
			cr.LineTo(c);
			cr.Arc(c.X, c.Y, radius, angle2, angle2);
			cr.LineTo(c);
			cr.Stroke();
		}
	
		public void arc_negative(Context cr, int width, int height)
		{
			PointD c = new PointD(0.5, 0.5);
			double radius = 0.4;
			double angle1 = 45.0  * (Math.PI/180.0);  /* angles are specified */
			double angle2 = 180.0 * (Math.PI/180.0);  /* in radians           */

			Normalize(cr, width, height);

			cr.ArcNegative(c.X, c.Y, radius, angle1, angle2);
			cr.Stroke();

			// draw helping lines
			cr.Color = new Color (1, 0.2, 0.2, 0.6);
			cr.Arc(c.X, c.Y, 0.05, 0, 2*Math.PI);
			cr.Fill();
			cr.LineWidth = 0.03;
			cr.Arc(c.X, c.Y, radius, angle1, angle1);
			cr.LineTo(c);
			cr.Arc(c.X, c.Y, radius, angle2, angle2);
			cr.LineTo(c);
			cr.Stroke();
		}
	
		public void clip(Context cr, int width, int height)
		{
			Normalize (cr, width, height);

			cr.Arc(0.5, 0.5, 0.3, 0, 2 * Math.PI);
			cr.Clip();

			cr.NewPath();  // current path is not consumed by cairo_clip()
			cr.Rectangle(0, 0, 1, 1);
			cr.Fill();
			cr.Color = new Color (0, 1, 0);
			cr.MoveTo(0, 0);
			cr.LineTo(1, 1);
			cr.MoveTo(1, 0);
			cr.LineTo(0, 1);
			cr.Stroke();
		}

		public void clip_image(Context cr, int width, int height)
		{
			Normalize (cr, width, height);
			cr.Arc (0.5, 0.5, 0.3, 0, 2*Math.PI);
			cr.Clip ();
			cr.NewPath (); // path not consumed by clip()

			ImageSurface image = new ImageSurface ("data/romedalen.png");
			int w = image.Width;
			int h = image.Height;

			cr.Scale (1.0/w, 1.0/h);

			cr.SetSourceSurface (image, 0, 0);
			cr.Paint ();

			image.Destroy ();
		}

		public void curve_to(Context cr, int width, int height)
		{
			double x=0.1,  y=0.5;
			double x1=0.4, y1=0.9, x2=0.6, y2=0.1, x3=0.9, y3=0.5;

			Normalize (cr, width, height);

			cr.MoveTo(x, y);
			cr.CurveTo(x1, y1, x2, y2, x3, y3);

			cr.Stroke();

			cr.Color = new Color (1, 0.2, 0.2, 0.6);
			cr.LineWidth = 0.03;
			cr.MoveTo(x,y);
			cr.LineTo(x1,y1);
			cr.MoveTo(x2,y2);
			cr.LineTo(x3,y3);
			cr.Stroke();
		}

		public void curve_rectangle(Context cr, int width, int height)
		{
			// a custom shape, that could be wrapped in a function
			double x0	   = 0.1,   //< parameters like cairo_rectangle
			       y0	   = 0.1,
			       rect_width  = 0.8,
		    	   rect_height = 0.8,
				   radius = 0.4;   //< and an approximate curvature radius

			double x1,y1;

			Normalize(cr, width, height);

			x1=x0+rect_width;
			y1=y0+rect_height;

			if (rect_width/2<radius) {
				if (rect_height/2<radius) {
					cr.MoveTo(x0, (y0 + y1)/2);
					cr.CurveTo(x0 ,y0, x0, y0, (x0 + x1)/2, y0);
					cr.CurveTo(x1, y0, x1, y0, x1, (y0 + y1)/2);
					cr.CurveTo(x1, y1, x1, y1, (x1 + x0)/2, y1);
					cr.CurveTo(x0, y1, x0, y1, x0, (y0 + y1)/2);
				} else {
					cr.MoveTo(x0, y0 + radius);
					cr.CurveTo(x0 ,y0, x0, y0, (x0 + x1)/2, y0);
					cr.CurveTo(x1, y0, x1, y0, x1, y0 + radius);
					cr.LineTo(x1 , y1 - radius);
					cr.CurveTo(x1, y1, x1, y1, (x1 + x0)/2, y1);
					cr.CurveTo(x0, y1, x0, y1, x0, y1- radius);
				}
			} else {
				if (rect_height/2<radius) {
					cr.MoveTo(x0, (y0 + y1)/2);
					cr.CurveTo(x0 , y0, x0 , y0, x0 + radius, y0);
					cr.LineTo(x1 - radius, y0);
					cr.CurveTo(x1, y0, x1, y0, x1, (y0 + y1)/2);
					cr.CurveTo(x1, y1, x1, y1, x1 - radius, y1);
					cr.LineTo(x0 + radius, y1);
					cr.CurveTo(x0, y1, x0, y1, x0, (y0 + y1)/2);
				} else {
					cr.MoveTo(x0, y0 + radius);
					cr.CurveTo(x0 , y0, x0 , y0, x0 + radius, y0);
					cr.LineTo(x1 - radius, y0);
					cr.CurveTo(x1, y0, x1, y0, x1, y0 + radius);
					cr.LineTo(x1 , y1 - radius);
					cr.CurveTo(x1, y1, x1, y1, x1 - radius, y1);
					cr.LineTo(x0 + radius, y1);
					cr.CurveTo(x0, y1, x0, y1, x0, y1- radius);
				}
			}
			cr.ClosePath();

			// and fill/stroke it
			cr.Color = new Color (0.5, 0.5, 1);
			cr.FillPreserve();
			cr.Color = new Color (0.5, 0, 0, 0.5);
			cr.Stroke();
		}

		public void fill_and_stroke(Context cr, int width, int height)
		{
			Normalize(cr, width, height);

			cr.MoveTo(0.5, 0.1);
			cr.LineTo(0.9, 0.9);
			cr.RelLineTo(-0.4, 0.0);
			cr.CurveTo(0.2, 0.9, 0.2, 0.5, 0.5, 0.5);
			cr.ClosePath();

			cr.Color = new Color (0, 0, 1);
			cr.FillPreserve();
			cr.Color = new Color (0, 0, 0);

			cr.Stroke();
		}

		public void fill_and_stroke2(Context cr, int width, int height)
		{
			Normalize (cr, width, height);

			cr.MoveTo(0.5, 0.1);
			cr.LineTo(0.9, 0.9);
			cr.RelLineTo(-0.4, 0.0);
			cr.CurveTo(0.2, 0.9, 0.2, 0.5, 0.5, 0.5);
			cr.ClosePath();

			cr.MoveTo(0.25, 0.1);
			cr.RelLineTo(0.2, 0.2);
			cr.RelLineTo(-0.2, 0.2);
			cr.RelLineTo(-0.2, -0.2);
			cr.ClosePath();

			cr.Color = new Color (0, 0, 1);
			cr.FillPreserve();
			cr.Color = new Color (0, 0, 0);

			cr.Stroke();
		}

		public void gradient(Context cr, int width, int height)
		{
			Normalize (cr, width, height);

			LinearGradient lg = new LinearGradient(0.0, 0.0, 0.0, 1.0);
			lg.AddColorStop(1, new Color(0, 0, 0, 1));
			lg.AddColorStop(0, new Color(1, 1, 1, 1));
			cr.Rectangle(0,0,1,1);
			cr.Source = lg;
			cr.Fill();

			RadialGradient rg = new RadialGradient(0.45, 0.4, 0.1, 0.4, 0.4, 0.5);
			rg.AddColorStop(0, new Color (1, 1, 1, 1));
			rg.AddColorStop(1, new Color (0, 0, 0, 1));
			cr.Source = rg;
			cr.Arc(0.5, 0.5, 0.3, 0, 2 * Math.PI);
			cr.Fill();
		}

		public void image(Context cr, int width, int height)
		{
			Normalize (cr, width, height);
			ImageSurface image = new ImageSurface ("data/romedalen.png");
			int w = image.Width;
			int h = image.Height;

			cr.Translate (0.5, 0.5);
			cr.Rotate (45* Math.PI/180);
			cr.Scale  (1.0/w, 1.0/h);
			cr.Translate (-0.5*w, -0.5*h);

			cr.SetSourceSurface (image, 0, 0);
			cr.Paint ();
			image.Destroy ();
		}
		
		public void imagepattern(Context cr, int width, int height)
		{
			Normalize (cr, width, height);
			
			ImageSurface image = new ImageSurface ("data/romedalen.png");
			int w = image.Width;
			int h = image.Height;

			SurfacePattern pattern = new SurfacePattern (image);
			pattern.Extend = Extend.Repeat;

			cr.Translate (0.5, 0.5);
			cr.Rotate (Math.PI / 4);
			cr.Scale (1 / Math.Sqrt (2), 1 / Math.Sqrt (2));
			cr.Translate (- 0.5, - 0.5);

			Matrix matrix = new Matrix ();
			matrix.InitScale (w * 5.0, h * 5.0);
			pattern.Matrix = matrix;

			cr.Source = pattern;

			cr.Rectangle (0, 0, 1.0, 1.0);
			cr.Fill ();

			pattern.Destroy ();
			image.Destroy ();
		}
		
		public void path(Context cr, int width, int height)
		{
			Normalize(cr, width, height);
			cr.MoveTo(0.5, 0.1);
			cr.LineTo(0.9, 0.9);
			cr.RelLineTo(-0.4, 0.0);
			cr.CurveTo(0.2, 0.9, 0.2, 0.5, 0.5, 0.5);

			cr.Stroke();
		}

		public void set_line_cap(Context cr, int width, int height)
		{
			Normalize(cr, width, height);
			cr.LineWidth = 0.12;
			cr.LineCap = LineCap.Butt; /* default */
			cr.MoveTo(0.25, 0.2); 
			cr.LineTo(0.25, 0.8);
			cr.Stroke();
			cr.LineCap = LineCap.Round;
			cr.MoveTo(0.5, 0.2); 
			cr.LineTo(0.5, 0.8);
			cr.Stroke();
			cr.LineCap = LineCap.Square;
			cr.MoveTo(0.75, 0.2); 
			cr.LineTo(0.75, 0.8);
			cr.Stroke();

			// draw helping lines
			cr.Color = new Color (1,0.2,0.2);
			cr.LineWidth = 0.01;
			cr.MoveTo(0.25, 0.2); 
			cr.LineTo(0.25, 0.8);
			cr.MoveTo(0.5, 0.2);  
			cr.LineTo(0.5, 0.8);
			cr.MoveTo(0.75, 0.2); 
			cr.LineTo(0.75, 0.8);
			cr.Stroke();
		}

		public void set_line_join(Context cr, int width, int height)
		{
			Normalize(cr, width, height);
			cr.LineWidth = 0.16;
			cr.MoveTo(0.3, 0.33);
			cr.RelLineTo(0.2, -0.2);
			cr.RelLineTo(0.2, 0.2);
			cr.LineJoin = LineJoin.Miter; // default
			cr.Stroke();

			cr.MoveTo(0.3, 0.63);
			cr.RelLineTo(0.2, -0.2);
			cr.RelLineTo(0.2, 0.2);
			cr.LineJoin = LineJoin.Bevel;
			cr.Stroke();

			cr.MoveTo(0.3, 0.93);
			cr.RelLineTo(0.2, -0.2);
			cr.RelLineTo(0.2, 0.2);
			cr.LineJoin = LineJoin.Round;
			cr.Stroke();
		}

		public void text(Context cr, int width, int height)
		{
			Normalize (cr, width, height);
			cr.SelectFontFace("Sans", FontSlant.Normal, FontWeight.Bold);
			cr.SetFontSize(0.35);

			cr.MoveTo(0.04, 0.53);
			cr.ShowText("Hello");

			cr.MoveTo(0.27, 0.65);
			cr.TextPath("void");
			cr.Save();
			cr.Color = new Color (0.5,0.5,1);
			cr.Fill();
			cr.Restore();
			cr.LineWidth = 0.01;
			cr.Stroke();

			// draw helping lines
			cr.Color = new Color (1.0, 0.2, 0.2, 0.6);
			cr.Arc(0.04, 0.53, 0.02, 0, 2*Math.PI);
			cr.Arc(0.27, 0.65, 0.02, 0, 2*Math.PI);
			cr.Fill();
		}

		public void text_align_center(Context cr, int width, int height)
		{
			Normalize (cr, width, height);

			cr.SelectFontFace("Sans", FontSlant.Normal, FontWeight.Normal);
			cr.SetFontSize(0.2);
			TextExtents extents = cr.TextExtents("cairo");
			double x = 0.5 -((extents.Width/2.0) + extents.XBearing);
			double y = 0.5 -((extents.Height/2.0) + extents.YBearing);

			cr.MoveTo(x, y);
			cr.ShowText("cairo");

			// draw helping lines
			cr.Color = new Color (1, 0.2, 0.2, 0.6);
			cr.Arc(x, y, 0.05, 0, 2*Math.PI);
			cr.Fill();
			cr.MoveTo(0.5, 0);
			cr.RelLineTo(0, 1);
			cr.MoveTo(0, 0.5);
			cr.RelLineTo(1, 0);
			cr.Stroke();
		}
	
		public void text_extents(Context cr, int width, int height)
		{
			double x=0.1;
			double y=0.6;
			string utf8 = "cairo";
			Normalize (cr, width, height);

			cr.SelectFontFace("Sans", FontSlant.Normal, FontWeight.Normal);

			cr.SetFontSize(0.4);
			TextExtents extents = cr.TextExtents(utf8);

			cr.MoveTo(x,y);
			cr.ShowText(utf8);

			// draw helping lines
			cr.Color = new Color (1, 0.2, 0.2, 0.6);
			cr.Arc(x, y, 0.05, 0, 2*Math.PI);
			cr.Fill();
			cr.MoveTo(x,y);
			cr.RelLineTo(0, -extents.Height);
			cr.RelLineTo(extents.Width, 0);
			cr.RelLineTo(extents.XBearing, -extents.YBearing);
			cr.Stroke();
		}

		public void xxx_clip_rectangle(Context cr, int width, int height)
		{
			Normalize (cr, width, height);

			cr.NewPath();
			cr.MoveTo(.25, .25);
			cr.LineTo(.25, .75);
			cr.LineTo(.75, .75);
			cr.LineTo(.75, .25);
			cr.LineTo(.25, .25);
			cr.ClosePath();

			cr.Clip();

			cr.MoveTo(0, 0);
			cr.LineTo(1, 1);
			cr.Stroke();
		}

		public void xxx_dash(Context cr, int width, int height)
		{
			double[] dashes = new double[] {
				0.20,  // ink
				0.05,  // skip
				0.05,  // ink
				0.05   // skip 
			};
			double offset = -0.2; 

			Normalize(cr, width, height);

			cr.SetDash(dashes, offset);

			cr.MoveTo(0.5, 0.1);
			cr.LineTo(0.9, 0.9);
			cr.RelLineTo(-0.4, 0.0);
			cr.CurveTo(0.2, 0.9, 0.2, 0.5, 0.5, 0.5);
			cr.Stroke();
		}

		public void xxx_long_lines(Context cr, int width, int height)
		{
			Normalize(cr, width, height);

			cr.MoveTo(0.1, -50);
			cr.LineTo(0.1,  50);
			cr.Color = new Color (1, 0 ,0);
			cr.Stroke();

			cr.MoveTo(0.2, -60);
			cr.LineTo(0.2,  60);
			cr.Color = new Color (1, 1 ,0);
			cr.Stroke();

			cr.MoveTo(0.3, -70);
			cr.LineTo(0.3,  70);
			cr.Color = new Color (0, 1 ,0);
			cr.Stroke();

			cr.MoveTo(0.4, -80);
			cr.LineTo(0.4,  80);
			cr.Color = new Color (0, 0 ,1);
			cr.Stroke();
		}

		public void xxx_multi_segment_caps(Context cr, int width, int height)
		{
			Normalize(cr, width, height);

			cr.MoveTo(0.2, 0.3);
			cr.LineTo(0.8, 0.3);

			cr.MoveTo(0.2, 0.5);
			cr.LineTo(0.8, 0.5);

			cr.MoveTo(0.2, 0.7);
			cr.LineTo(0.8, 0.7);

			cr.LineWidth = 0.12;
			cr.LineCap = LineCap.Round;
			cr.Stroke();
		}

		public void xxx_self_intersect(Context cr, int width, int height)
		{
			Normalize(cr, width, height);

			cr.MoveTo(0.3, 0.3);
			cr.LineTo(0.7, 0.3);

			cr.LineTo(0.5, 0.3);
			cr.LineTo(0.5, 0.7);

			cr.LineWidth = 0.22;
			cr.LineCap = LineCap.Round;
			cr.LineJoin = LineJoin.Round;
			cr.Stroke();
		}
	}
}
