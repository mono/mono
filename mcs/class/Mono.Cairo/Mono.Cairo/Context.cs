//
// Mono.Cairo.Context.cs
//
// Author:
//   Duncan Mak (duncan@ximian.com)
//   Miguel de Icaza (miguel@novell.com)
//   Hisham Mardam Bey (hisham.mardambey@gmail.com)
//   Alp Toker (alp@atoker.com)
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
using System.Runtime.InteropServices;
using Cairo;

namespace Cairo {

        public struct Point
        {		
		public Point (int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		int x, y;
		public int X {
			get { return x; }
			set { x = value; }
		}

		public int Y {
			get { return y; }
			set { y = value; }
		}
	}
	   
        public struct PointD
        {
		public PointD (double x, double y)
		{
			this.x = x;
			this.y = y;
		}

		double x, y;
		public double X {
			get { return x; }
			set { x = value; }
		}

		public double Y {
			get { return y; }
			set { y = value; }
		}
	}
   

        public struct Distance
        {
		public Distance (double dx, double dy)
		{
			this.dx = dx;
			this.dy = dy;
		}

		double dx, dy;
		public double Dx {
			get { return dx; }
			set { dx = value; }
		}

		public double Dy {
			get { return dy; }
			set { dy = value; }
		}
	}
	      
        public struct Color
        {		
		public Color(double r, double g, double b) : this (r, g, b, 1.0)
		{
		}

		public Color(double r, double g, double b, double a)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}

		double r, g, b, a;
		
		public double R {
			get { return r; }
			set { r = value; }
		}

		public double G {
			get { return g; }
			set { g = value; }
		}
		
		public double B {
			get { return b; }
			set { b = value; }
		}

		public double A {
			get { return a; }
			set { a = value; }
		}
		
	}
   
	[Obsolete ("Renamed Cairo.Context per suggestion from cairo binding guidelines.")]
	public class Graphics : Context {
		public Graphics (IntPtr state) : base (state) {}
		public Graphics (Surface surface) : base (surface) {}
	}

        public class Context : IDisposable 
        {
                internal IntPtr state = IntPtr.Zero;

		static int native_glyph_size, c_compiler_long_size;
		
		static Context ()
		{
			//
			// This is used to determine what kind of structure
			// we should use to marshal Glyphs, as the public
			// definition in Cairo uses `long', which can be
			// 32 bits or 64 bits depending on the platform.
			//
			// We assume that sizeof(long) == sizeof(void*)
			// except in the case of Win64 where sizeof(long)
			// is 32 bits
			//
			int ptr_size = Marshal.SizeOf (typeof (IntPtr));
			
			PlatformID platform = Environment.OSVersion.Platform;
			if (platform == PlatformID.Win32NT ||
			    platform == PlatformID.Win32S ||
			    platform == PlatformID.Win32Windows ||
			    platform == PlatformID.WinCE ||
			    ptr_size == 4){
				c_compiler_long_size = 4;
				native_glyph_size = Marshal.SizeOf (typeof (NativeGlyph_4byte_longs));
			} else {
				c_compiler_long_size = 8;
				native_glyph_size = Marshal.SizeOf (typeof (Glyph));
			}
		}
		
                public Context (Surface surface)
                {
			state = CairoAPI.cairo_create (surface.Handle);
                }
		
		public Context (IntPtr state)
		{
			this.state = state;
		}
		
		~Context ()
		{
			Dispose (false);
		}

		void IDisposable.Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}
		
                protected virtual void Dispose (bool disposing)
                {
			if (!disposing){
				//Console.WriteLine ("Cairo.Context: called from thread");
				return;
			}
			
			if (state == IntPtr.Zero)
				return;

			//Console.WriteLine ("Destroying");
                        CairoAPI.cairo_destroy (state);
			state = IntPtr.Zero;
                }

                public void Save ()
                {
                        CairoAPI.cairo_save (state);
                }

                public void Restore ()
                {
                        CairoAPI.cairo_restore (state);
                }

		public Antialias Antialias {
			get { return CairoAPI.cairo_get_antialias (state); }
			set { CairoAPI.cairo_set_antialias (state, value); }
		}
                
                public Cairo.Status Status {
                        get {
                                return CairoAPI.cairo_status (state);
                        }
                }
		
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
                
                //FIXME: obsolete this property
                public Cairo.Color Color {
			set { 
				CairoAPI.cairo_set_source_rgba (state, value.R, 
							  value.G, value.B,
							  value.A);
			}			
                }

		[Obsolete ("Use Color property")]
                public Cairo.Color ColorRgb {
			set { 
				Color = new Color (value.R, value.G, value.B);
			}
                }		

                public double Tolerance {
						get { return CairoAPI.cairo_get_tolerance (state); }
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
                                return CairoAPI.cairo_get_line_cap (state);
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

                public void SetDash (double [] dashes, double offset)
                {
                        CairoAPI.cairo_set_dash (state, dashes, dashes.Length, offset);
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

                public Cairo.Surface Target {
                        set {
				state = CairoAPI.cairo_create (value.Handle);
                        }

                        get {
                                return Cairo.Surface.LookupExternalSurface (
                                        CairoAPI.cairo_get_target (state));
                        }
                }

		[Obsolete ("Use Color property")]
		public void SetSourceRGB (double r, double g, double b)
		{
			Color = new Color (r, g, b);
		}

		[Obsolete ("Use Color property")]
		public void SetSourceRGBA (double r, double g, double b, double a)
		{
			Color = new Color (r, g, b, a);
		}

		//[Obsolete ("Use SetSource method (with double parameters)")]
		public void SetSourceSurface (Surface source, int x, int y)
		{
			CairoAPI.cairo_set_source_surface (state, source.Handle, x, y);
		}

		public void SetSource (Surface source, double x, double y)
		{
			CairoAPI.cairo_set_source_surface (state, source.Handle, x, y);
		}

#region Path methods
                
                public void NewPath ()
                {
                        CairoAPI.cairo_new_path (state);
                }

		public void NewSubPath ()
		{
			CairoAPI.cairo_new_sub_path (state);
		}
        
                public void MoveTo (PointD p)
                {
						MoveTo (p.X, p.Y);
                }

				public void MoveTo (double x, double y)
				{
                        CairoAPI.cairo_move_to (state, x, y);
				}
                
                public void LineTo (PointD p)
				{
						LineTo (p.X, p.Y);
				}

				public void LineTo (double x, double y)
                {
                        CairoAPI.cairo_line_to (state, x, y);
                }

                public void CurveTo (PointD p1, PointD p2, PointD p3)
				{
						CurveTo (p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y);
				}
				
                public void CurveTo (double x1, double y1, double x2, double y2, double x3, double y3)
                {
                        CairoAPI.cairo_curve_to (state, x1, y1, x2, y2, x3, y3);
                }

                public void RelMoveTo (Distance d)
				{
						RelMoveTo (d.Dx, d.Dy);
				}
				
                public void RelMoveTo (double dx, double dy)
                {
                        CairoAPI.cairo_rel_move_to (state, dx, dy);
                }

                public void RelLineTo (Distance d)
                {
						RelLineTo (d.Dx, d.Dy);
                }

                public void RelLineTo (double dx, double dy)
				{
                        CairoAPI.cairo_rel_line_to (state, dx, dy);
				}

                public void RelCurveTo (Distance d1, Distance d2, Distance d3)
		{
			RelCurveTo (d1.Dx, d1.Dy, d2.Dx, d2.Dy, d3.Dx, d3.Dy);
		}

                public void RelCurveTo (double dx1, double dy1, double dx2, double dy2, double dx3, double dy3)
                {
                        CairoAPI.cairo_rel_curve_to (state, dx1, dy1, dx2, dy2, dx3, dy3); 
                }

                public void Arc (double xc, double yc, double radius, double angle1, double angle2)
                {
                        CairoAPI.cairo_arc (state, xc, yc, radius, angle1, angle2);
                }

                public void ArcNegative (double xc, double yc, double radius, double angle1, double angle2)
                {
                        CairoAPI.cairo_arc_negative (state, xc, yc, radius, angle1, angle2);
                }
		
                public void Rectangle (Rectangle rectangle)
		{
			Rectangle (rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
		}

                public void Rectangle (PointD p, double width, double height)
		{
			Rectangle (p.X, p.Y, width, height);
		}

                public void Rectangle (double x, double y, double width, double height)
                {
                        CairoAPI.cairo_rectangle (state, x, y, width, height);
                }
                
                public void ClosePath ()
                {
                        CairoAPI.cairo_close_path (state);
                }
#endregion

#region Painting Methods
				public void Paint ()
				{
					CairoAPI.cairo_paint (state);
				}

				public void PaintWithAlpha (double alpha)
				{
					CairoAPI.cairo_paint_with_alpha (state, alpha);
				}

				public void Mask (Pattern pattern)
				{
					CairoAPI.cairo_mask (state, pattern.Pointer);
				}

				public void MaskSurface (Surface surface, double x, double y)
				{
					CairoAPI.cairo_mask_surface (state, surface.Handle, x, y);
				}

                public void Stroke ()
                {
                        CairoAPI.cairo_stroke (state);
                }
		
                public void StrokePreserve ()
                {
                        CairoAPI.cairo_stroke_preserve (state);
                }		

		public Rectangle StrokeExtents ()
		{
			double x1, y1, x2, y2;
			CairoAPI.cairo_stroke_extents (state, out x1, out y1, out x2, out y2);
			return new Rectangle (x1, y1, x2, y2);
		}

                public void Fill ()
                {
                        CairoAPI.cairo_fill (state);
                }

                public Rectangle FillExtents ()
		{
			double x1, y1, x2, y2;
			CairoAPI.cairo_fill_extents (state, out x1, out y1, out x2, out y2);
			return new Rectangle (x1, y1, x2, y2);
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

				public void ClipPreserve ()
				{
					CairoAPI.cairo_clip_preserve (state);
				}

		public void ResetClip ()
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

		public Pattern PopGroup ()
		{
			IntPtr pattern = CairoAPI.cairo_pop_group (state);
			PatternType pt = CairoAPI.cairo_pattern_get_type (pattern);
			switch (pt) {
				case PatternType.Solid:
					return new SolidPattern (pattern);
				case PatternType.Surface:
					return new SurfacePattern (pattern);
				case PatternType.Linear:
					return new LinearGradient (pattern);
				case PatternType.Radial:
					return new RadialGradient (pattern);
				default:
					return new Pattern (pattern);
			}
		}

		public void PopGroupToSource ()
		{
			CairoAPI.cairo_pop_group_to_source (state);
		}

		public void PushGroup ()
		{
			CairoAPI.cairo_push_group (state);
		}

		public void PushGroup (Content content)
		{
			CairoAPI.cairo_push_group_with_content (state, content);
		}

		public Surface GroupTarget {
			get {
				IntPtr surface = CairoAPI.cairo_get_group_target (state);
				return Surface.LookupSurface (surface);
			}
		}

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
                
		public void Transform (Matrix m)
		{
			CairoAPI.cairo_transform (state, m);
		}
                	

		//FIXME: obsolete these methods
		public void TransformPoint (ref double x, ref double y)
		{
                	CairoAPI.cairo_user_to_device (state, ref x, ref y);
		}
		
                public void TransformDistance (ref double dx, ref double dy) 
		{
			CairoAPI.cairo_user_to_device_distance (state, ref dx, ref dy);
		}
			
		public void InverseTransformPoint (ref double x, ref double y)
		{
			CairoAPI.cairo_device_to_user (state, ref x, ref y);
		}

		public void InverseTransformDistance (ref double dx, ref double dy)
		{
			CairoAPI.cairo_device_to_user_distance (state, ref dx, ref dy);
		}
		
                public Cairo.Matrix Matrix {
                        set {
                                CairoAPI.cairo_set_matrix (state, value);
                        }

                        get {
				Matrix m = new Matrix();
				CairoAPI.cairo_get_matrix (state, m);
                                return m;
                        }
                }

		public void SetFontSize (double scale)
		{
			CairoAPI.cairo_set_font_size (state, scale);
		}

		[Obsolete ("Use SetFontSize() instead.")]
		public void FontSetSize (double scale)
		{
			SetFontSize (scale);
		}

		[Obsolete ("Use SetFontSize() instead.")]
		public double FontSize {
			set { SetFontSize (value); }
		}
		
		public Matrix FontMatrix {
			get {
				Matrix m;
				CairoAPI.cairo_get_font_matrix (state, out m);
				return m;
			}
			set { CairoAPI.cairo_set_font_matrix (state, value); }
		}

		public FontOptions FontOptions {
			get {
				FontOptions options = new FontOptions ();
				CairoAPI.cairo_get_font_options (state, options.Handle);
				return options;
			}
			set { CairoAPI.cairo_set_font_options (state, value.Handle); }
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct NativeGlyph_4byte_longs {
			public int index;
			public double x;
			public double y;

			public NativeGlyph_4byte_longs (Glyph source)
			{
				index = (int) source.index;
				x = source.x;
				y = source.y;
			}
		}

		static internal IntPtr FromGlyphToUnManagedMemory(Glyph [] glyphs)
		{
			IntPtr dest = Marshal.AllocHGlobal (native_glyph_size * glyphs.Length);
			int pos = dest.ToInt32();

			if (c_compiler_long_size == 8){
				foreach (Glyph g in glyphs){
					Marshal.StructureToPtr (g, (IntPtr)pos, false);
					pos += native_glyph_size;
				}
			} else {
				foreach (Glyph g in glyphs){
					NativeGlyph_4byte_longs n = new NativeGlyph_4byte_longs (g);
					
					Marshal.StructureToPtr (n, (IntPtr)pos, false);
					pos += native_glyph_size;
				}
			}

			return dest;
		}

                public void ShowGlyphs (Glyph[] glyphs)
		{
                        IntPtr ptr;

                        ptr = FromGlyphToUnManagedMemory (glyphs);
                        
                        CairoAPI.cairo_show_glyphs (state, ptr, glyphs.Length);

                        Marshal.FreeHGlobal (ptr);		
		}

		[Obsolete("The matrix argument was never used, use ShowGlyphs(Glyphs []) instead")]
                public void ShowGlyphs (Matrix matrix, Glyph[] glyphs)
                {
			ShowGlyphs (glyphs);
                }

		[Obsolete("The matrix argument was never used, use GlyphPath(Glyphs []) instead")]
                public void GlyphPath (Matrix matrix, Glyph[] glyphs)
                {
			GlyphPath (glyphs);
		}

		public void GlyphPath (Glyph[] glyphs)
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
		
		public void CopyPage ()
		{
			CairoAPI.cairo_copy_page (state);
		}

		[Obsolete ("Use SelectFontFace() instead.")]
		public void FontFace (string family, FontSlant slant, FontWeight weight)
		{
			SelectFontFace (family, slant, weight);
		}
		
		public void SelectFontFace (string family, FontSlant slant, FontWeight weight)
		{
			CairoAPI.cairo_select_font_face (state, family, slant, weight);
		}

		public void ShowPage ()
		{
			CairoAPI.cairo_show_page (state);
		}
		
                public void ShowText (string str)
                {
                        CairoAPI.cairo_show_text (state, str);
                }		
		
                public void TextPath (string str)
                {
                        CairoAPI.cairo_text_path  (state, str);
                }		

		public TextExtents TextExtents (string utf8)
		{
			TextExtents extents;
			CairoAPI.cairo_text_extents (state, utf8, out extents);
			return extents;
		}

		public TextExtents GlyphExtents (Glyph[] glyphs)
		{
			IntPtr ptr = FromGlyphToUnManagedMemory (glyphs);

			TextExtents extents;

			CairoAPI.cairo_glyph_extents (state, ptr, glyphs.Length, out extents);

			Marshal.FreeHGlobal (ptr);

			return extents;
		}
        }
}
