//
// Mono.Cairo.Surface.cs
//
// Authors:
//    Duncan Mak
//    Miguel de Icaza.
//    Alp Toker
//
// (C) Ximian Inc, 2003.
// (C) Novell, Inc. 2003.
//
// This is an OO wrapper API for the Cairo API
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
using System.Collections;

namespace Cairo {

	public class Surface : IDisposable 
        {						
		protected static Hashtable surfaces = new Hashtable ();
                internal IntPtr surface = IntPtr.Zero;

		protected Surface()
		{
		}
		
                protected Surface (IntPtr ptr, bool owns)
                {
                        surface = ptr;
			lock (surfaces.SyncRoot){
				surfaces [ptr] = this;
			}
			if (!owns)
				NativeMethods.cairo_surface_reference (ptr);
                }

		static internal Surface LookupExternalSurface (IntPtr p)
		{
			lock (surfaces.SyncRoot){
				object o = surfaces [p];
				if (o == null){
					return new Surface (p, false);
				}
				return (Surface) o;
			}
		}		

		static internal Surface LookupSurface (IntPtr surface)
		{
			SurfaceType st = NativeMethods.cairo_surface_get_type (surface);
			switch (st) {
			case SurfaceType.Image:
				return new ImageSurface (surface, true);
			case SurfaceType.Xlib:
				return new XlibSurface (surface, true);
			case SurfaceType.Xcb:
				return new XcbSurface (surface, true);
			case SurfaceType.Glitz:
				return new GlitzSurface (surface, true);
			case SurfaceType.Win32:
				return new Win32Surface (surface, true);

			case SurfaceType.Pdf:
				return new PdfSurface (surface, true);
			case SurfaceType.PS:
				return new PSSurface (surface, true);
			case SurfaceType.DirectFB:
				return new DirectFBSurface (surface, true);
			case SurfaceType.Svg:
				return new SvgSurface (surface, true);

			default:
				return Surface.LookupExternalSurface (surface);
			}
		}
		
		[Obsolete ("Use an ImageSurface constructor instead.")]
                public static Cairo.Surface CreateForImage (
                        ref byte[] data, Cairo.Format format, int width, int height, int stride)
                {
                        IntPtr p = NativeMethods.cairo_image_surface_create_for_data (
                                data, format, width, height, stride);
                        
                        return new Cairo.Surface (p, true);
                }

		[Obsolete ("Use an ImageSurface constructor instead.")]
                public static Cairo.Surface CreateForImage (
                        Cairo.Format format, int width, int height)
                {
                        IntPtr p = NativeMethods.cairo_image_surface_create (
                                format, width, height);

                        return new Cairo.Surface (p, true);
                }


                public Cairo.Surface CreateSimilar (
                        Cairo.Content content, int width, int height)
                {
                        IntPtr p = NativeMethods.cairo_surface_create_similar (
                                this.Handle, content, width, height);

                        return new Cairo.Surface (p, true);
                }

		~Surface ()
		{
			Dispose (false);
		}

		//[Obsolete ("Use Context.SetSource() followed by Context.Paint()")]
		public void Show (Context gr, double x, double y) 
		{
			NativeMethods.cairo_set_source_surface (gr.Handle, surface, x, y);
			NativeMethods.cairo_paint (gr.Handle);
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (surface == IntPtr.Zero)
				return;
			
			lock (surfaces.SyncRoot)
				surfaces.Remove (surface);

			NativeMethods.cairo_surface_destroy (surface);
			surface = IntPtr.Zero;
		}
		
		public Status Finish ()
		{
			NativeMethods.cairo_surface_finish (surface);
			return Status;
		}
		
		public void Flush ()
		{
			NativeMethods.cairo_surface_flush (surface);
		}
		
		public void MarkDirty ()
		{
			NativeMethods.cairo_surface_mark_dirty (Handle);
		}
		
		public void MarkDirty (Rectangle rectangle)
		{
			NativeMethods.cairo_surface_mark_dirty_rectangle (Handle, (int)rectangle.X, (int)rectangle.Y, (int)rectangle.Width, (int)rectangle.Height);
		}
		
                public IntPtr Handle {
                        get {
				return surface;
			}
                }

		public PointD DeviceOffset {
			get {
				double x, y;
				NativeMethods.cairo_surface_get_device_offset (surface, out x, out y);
				return new PointD (x, y);
			}

			set {
				NativeMethods.cairo_surface_set_device_offset (surface, value.X, value.Y);
			}
		}
		
		public void Destroy()
		{
			Dispose (true);
		}

		public void SetFallbackResolution (double x, double y)
		{
			NativeMethods.cairo_surface_set_fallback_resolution (surface, x, y);
		}

		public void WriteToPng (string filename)
		{
			NativeMethods.cairo_surface_write_to_png (surface, filename);
		}
		
		[Obsolete ("Use Handle instead.")]
                public IntPtr Pointer {
                        get {
				return surface;
			}
                }
		
		public Status Status {
			get { return NativeMethods.cairo_surface_status (surface); }
		}

		public Content Content {
			get { return NativeMethods.cairo_surface_get_content (surface); }
		}

		public SurfaceType SurfaceType {
			get { return NativeMethods.cairo_surface_get_type (surface); }
		}

		public uint ReferenceCount {
			get { return NativeMethods.cairo_surface_get_reference_count (surface); }
		}
        }
}
