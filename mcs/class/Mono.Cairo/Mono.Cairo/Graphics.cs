//
// Mono.Cairo.Graphics.cs
//
// Author:
//   Duncan Mak (duncan@ximian.com)
//   Miguel de Icaza (miguel@novell.com)
//   Hisham Mardam Bey (hisham.mardambey@gmail.com)
//
// (C) Ximian Inc, 2003.
// (C) Novell Inc, 2003.
//
// This is an OO wrapper API for the Cairo API.
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
using System.Drawing;
using System.Runtime.InteropServices;
using Cairo;

namespace Cairo {

        public class Point
        {
		public int X;
		public int Y;
		
		public Point (int x, int y)
		{
			X = x;
			Y = y;
		}		
	}
	   
        public class PointD
        {
		public double X;
		public double Y;
		
		public PointD (double x, double y)
		{
			X = x;
			Y = y;
		}
	}
   

        public class Distance
        {
		public double Dx;
		public double Dy;
		
		public Distance (double x, double y)
		{
			Dx = x;
			Dy = y;
		}		
	}
	      
        public class Color
        {
		public double R;
		public double G;
		public double B;
		public double A;
		
		public Color(double r, double g, double b, double a)
		{
			R = r;
			G = g;
			B = b;
			A = a;
		}
	}
   
        public class Graphics : IDisposable 
        {
                internal IntPtr state = IntPtr.Zero;
		//private Surface surface;
		
                public Graphics (Surface surface)
                {
			state = CairoAPI.cairo_create (surface.Pointer);
                }
		
		public Graphics (IntPtr state)
		{
			this.state = state;
		}
		
		~Graphics ()
		{
			Console.WriteLine ("Cairo not thread safe, you might want to call IDisposable.Dispose on Cairo.Surface");
		}

		void IDisposable.Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}
		
                protected virtual void Dispose (bool disposing)
                {
			if (!disposing){
				Console.WriteLine ("Cairo.Graphics: called from thread");
				return;
			}
			
			if (state == IntPtr.Zero)
				return;

                        CairoAPI.cairo_destroy (state);
			state = IntPtr.Zero;
                }

                public Cairo.Graphics Copy ()
                {
                        IntPtr dest;
                        CairoAPI.cairo_copy (out dest, state);
                        return new Cairo.Graphics (dest);
                }
                
                public void Save ()
                {
                        CairoAPI.cairo_save (state);
                }

                public void Restore ()
                {
                        CairoAPI.cairo_restore (state);
                }
                
                public Cairo.Status Status {
                        get {
                                return CairoAPI.cairo_status (state);
                        }
                }
		
		/*
                public string StatusString {
		 get {
                                return CairoAPI.cairo_status_to_string (state);
                        }
		 }
		 */ 
                public IntPtr Handle {
                        get {
                                return state;
                        }
                }
                
                public Cairo.Operator Operator {
                        set {
                                CairoAPI.cairo_set_operator (state, value);
                        }

                        get {
                                return CairoAPI.cairo_get_operator (state);
                        }
                }
                
                public Cairo.Color Color {
			set { 
				CairoAPI.cairo_set_source_rgba (state, value.R, 
							  value.G, value.B,
							  value.A);
			}			
                }
		
                public Cairo.Color ColorRgb {
			set { 
				CairoAPI.cairo_set_source_rgb (state, value.R, 
							   value.G, value.B);
			}
                }		

                public double Tolerance {
                        set {
                                CairoAPI.cairo_set_tolerance (state, value);
                        }
                }
                
                public Cairo.FillRule FillRule {
                        set {
                                CairoAPI.cairo_set_fill_rule (state, value);
                        }

                        get {
                                return CairoAPI.cairo_get_fill_rule (state);
                        }
                }
                                        
                public double LineWidth {
                        set {
                                CairoAPI.cairo_set_line_width (state, value);
                        }

                        get {
                                return CairoAPI.cairo_get_line_width (state);
                        }
                }

                public Cairo.LineCap LineCap {
                        set {
                                CairoAPI.cairo_set_line_cap (state, value);
                        }

                        get {
                                return CairoAPI.cairo_current_line_cap (state);
                        }
                }

                public Cairo.LineJoin LineJoin {
                        set {
                                CairoAPI.cairo_set_line_join (state, value);
                        }

                        get {
                                return CairoAPI.cairo_get_line_join (state);
                        }
                }

                public void SetDash (double [] dashes, int ndash, double offset)
                {
                        CairoAPI.cairo_set_dash (state, dashes, ndash, offset);
                }
		
                public Pattern Pattern {
                        set {
                                CairoAPI.cairo_set_source (state, value.Pointer);
                        }
			
			get {
				return new Pattern (CairoAPI.cairo_get_source (state));
			}
                }		
		
                public Pattern Source {
                        set {
                                CairoAPI.cairo_set_source (state, value.Pointer);
                        }
			
			get {
				return new Pattern (CairoAPI.cairo_get_source (state));
			}
                }

                public double MiterLimit {
                        set {
                                CairoAPI.cairo_set_miter_limit (state, value);
                        }

                        get {
                                return CairoAPI.cairo_get_miter_limit (state);
                        }
                }

                public PointD CurrentPoint {
                        get {
                                double x, y;
                                CairoAPI.cairo_get_current_point (state, out x, out y);
                                return new PointD (x, y);
                        }
                }

                public Cairo.Surface TargetSurface {
                        set {
				state = CairoAPI.cairo_create (value.Pointer);				
                                //CairoAPI.cairo_set_target_surface (state, value.Handle);
                        }

                        get {
                                return Cairo.Surface.LookupExternalSurface (
                                        CairoAPI.cairo_get_target (state));
                        }
                }

#region Path methods
                
                public void NewPath ()
                {
                        CairoAPI.cairo_new_path (state);
                }
        
		public void CurrentPath (CairoAPI.MoveToCallback move_to, 
					 CairoAPI.LineToCallback line_to,
					 CairoAPI.CurveToCallback curve_to,
					 CairoAPI.ClosePathCallback close_path,
					 object closure)
		{
			
		}
		
                public void MoveTo (PointD p)
                {
                        CairoAPI.cairo_move_to (state, p.X, p.Y);
                }
                
                public void LineTo (PointD p)
                {
                        CairoAPI.cairo_line_to (state, p.X, p.Y);
                }

                public void CurveTo (PointD p1, PointD p2, PointD p3)
                {
                        CairoAPI.cairo_curve_to (state, p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y);
                }

                public void RelMoveTo (PointD p)
                {
                        CairoAPI.cairo_rel_move_to (state, p.X, p.Y);
                }

                public void RelLineTo (PointD p)
                {
                        CairoAPI.cairo_rel_line_to (state, p.X, p.Y);
                }

                public void RelCurveTo (double dx1, double dy1, double dx2, double dy2, double dx3, double dy3)
                {
                        CairoAPI.cairo_rel_curve_to (state, dx1, dy1, dx2, dy2, dx3, dy3); 
                }

                public void Arc (double xc, double yc, double radius, double angel1, double angel2)
                {
                        CairoAPI.cairo_arc (state, xc, yc, radius, angel1, angel2);
                }

                public void ArcNegative (double xc, double yc, double radius, double angel1, double angel2)
                {
                        CairoAPI.cairo_arc_negative (state, xc, yc, radius, angel1, angel2);
                }
		
		public void ArcTo (PointD p1, PointD p2, double radius)
		{
			CairoAPI.cairo_arc_to (state, p1.X, p1.Y, p2.X, p2.Y, radius);
		}
                
                public void Rectangle (PointD p, double width, double height)
                {
                        CairoAPI.cairo_rectangle (state, p.X, p.Y, width, height);
                }
                
                public void ClosePath ()
                {
                        CairoAPI.cairo_close_path (state);
                }
#endregion

#region Painting Methods

                public void Stroke ()
                {
                        CairoAPI.cairo_stroke (state);
                }
		
                public void StrokePreserve ()
                {
                        CairoAPI.cairo_stroke_preserve (state);
                }		

                public void Fill ()
                {
                        CairoAPI.cairo_fill (state);
                }
		
		public void FillPreserve ()
		{
			CairoAPI.cairo_fill_preserve (state);
		}

#endregion

                public void Clip ()
                {
                        CairoAPI.cairo_clip (state);
                }

		public void ClipReset ()
		{
			CairoAPI.cairo_reset_clip (state);
		}
		
		public bool InStroke (double x, double y)
		{
			return CairoAPI.cairo_in_stroke (state, x, y);
		}

		public bool InFill (double x, double y)
		{
			return CairoAPI.cairo_in_fill (state, x, y);
		}


#region Modified state

                public void SetTargetImage (
                        string data, Cairo.Format format, int width, int height, int stride)
                {
                        CairoAPI.cairo_image_surface_create_for_data (data, format, width, height, stride);
                }

		public void SetTargetDrawable (IntPtr dpy, IntPtr drawable, IntPtr visual, int width, int height)
		{
			CairoAPI.cairo_xlib_surface_create (dpy, drawable, visual, width, height);
		}
#endregion

                public void Rotate (double angle)
                {
                        CairoAPI.cairo_rotate (state, angle);
                }

                public void Scale (double sx, double sy)
                {
                        CairoAPI.cairo_scale (state, sx, sy);
                }

                public void Translate (double tx, double ty)
                {
                        CairoAPI.cairo_translate (state, tx, ty);
                }

                public PointD TransformPoint
                {
			get {
				double x; double y;				
				CairoAPI.cairo_user_to_device (state, out x, out y);
				return new PointD(x, y);
			}
                }
		
                public Distance TransformDistance 
                {
			get {
				double dx; double dy;
				CairoAPI.cairo_user_to_device_distance (state, out dx, out dy);
				return new Distance(dx, dy);
			}
                }

                public PointD InverseTransformPoint
                {
			get {
				double x; double y;
				CairoAPI.cairo_device_to_user (state, out x, out y);
				return new PointD (x, y);
			}
                }

                public Distance InverseTransformDistance
                {
			get {
				double dx; double dy;
				CairoAPI.cairo_device_to_user_distance (state, out dx, out dy);
				return new Distance (dx, dy);
			}
                }
		
                public Cairo.Matrix Matrix {
                        set {
                                CairoAPI.cairo_set_matrix (state, value.Pointer);
                        }

                        get {
				Matrix_T m = new Matrix_T ();
				CairoAPI.cairo_get_matrix (state, m);
                                return new Matrix (m);
                        }
                }
		/*
                public Font Font {
                        set {
                                CairoAPI.cairo_set_font (state, value.Pointer);

                        }

                        get {
                                IntPtr fnt = IntPtr.Zero;

                                fnt = CairoAPI.cairo_current_font (state);

                                return new Font (fnt);
                        }
                }
		 */ 

                public void ScaleFont (double scale)
                {
                        CairoAPI.cairo_scale_font (state, scale);
                }
                
		/*
                public void TransformFont (Matrix matrix)
                {
                        CairoAPI.cairo_transform_font (state, matrix.Pointer);
                }
		 */ 

                
		static internal IntPtr FromGlyphToUnManagedMemory(Glyph [] glyphs)
		{
			int size =  Marshal.SizeOf (glyphs[0]);
			IntPtr dest = Marshal.AllocHGlobal (size * glyphs.Length);
			int pos = dest.ToInt32();

			for (int i=0; i < glyphs.Length; i++, pos += size)
				Marshal.StructureToPtr (glyphs[i], (IntPtr)pos, false);

			return dest;
		}


                public void ShowGlyphs (Matrix matrix, Glyph[] glyphs)
                {

                        IntPtr ptr;

                        ptr = FromGlyphToUnManagedMemory (glyphs);
                        
                        CairoAPI.cairo_show_glyphs (state, ptr, glyphs.Length);

                        Marshal.FreeHGlobal (ptr);		
                     
                }

                public void GlyphPath (Matrix matrix, Glyph[] glyphs)
                {

                        IntPtr ptr;

                        ptr = FromGlyphToUnManagedMemory (glyphs);

                        CairoAPI.cairo_glyph_path (state, ptr, glyphs.Length);

                        Marshal.FreeHGlobal (ptr);

                }

                public FontExtents FontExtents {
                        get {
				
                                FontExtents f_extents = new FontExtents();
                                CairoAPI.cairo_font_extents (state, ref f_extents);
                                return f_extents;
                        }
                }
		
		public void FontFace (string family, FontSlant s, FontWeight w)
		{
			CairoAPI.cairo_select_font_face (state, family, s, w);
		}
		
		public double FontSize {
			set { CairoAPI.cairo_set_font_size (state, value); }
		}
		
                public void ShowText (string str)
                {
                        CairoAPI.cairo_show_text (state, str);
                }		
		
                public void TextPath (string str)
                {
                        CairoAPI.cairo_text_path  (state, str);
                }		
        }
}
