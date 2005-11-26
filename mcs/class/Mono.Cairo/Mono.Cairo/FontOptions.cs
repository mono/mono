//
// Mono.Cairo.FontOptions.cs
//
// Author:
//   John Luke (john.luke@gmail.com)
//
// (C) John Luke 2005.
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

namespace Cairo
{
	public class FontOptions : IDisposable
	{
		IntPtr handle;
		bool disposed = false;

		public FontOptions ()
		{
			handle = CairoAPI.cairo_font_options_create ();
		}

		~FontOptions ()
		{
			Dispose (false);
		}

		internal FontOptions (IntPtr handle)
		{
			this.handle = handle;
		}

		public FontOptions Copy ()
		{
			return new FontOptions (CairoAPI.cairo_font_options_copy (handle));
		}

		public void Destroy ()
		{
			CairoAPI.cairo_font_options_destroy (handle);
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		private void Dispose (bool disposing)
		{
			if (!disposed) {
				Destroy ();
				handle = IntPtr.Zero;
			}
			disposed = true;
		}

		public static bool operator == (FontOptions options, FontOptions other)
		{
			return CairoAPI.cairo_font_options_equal (options.Handle, other.Handle);
		}

		public static bool operator != (FontOptions options, FontOptions other)
		{
			return !(options == other);
		}

		public override bool Equals (object other)
		{
			if (other is FontOptions)
				return this == (FontOptions) other;
			return false;
		}

		public IntPtr Handle {
			get { return handle; }
		}

		public override int GetHashCode ()
		{
			return (int) CairoAPI.cairo_font_options_hash (handle);
		}
		
		public void Merge (FontOptions other)
		{
			CairoAPI.cairo_font_options_merge (handle, other.Handle);
		}

		public Antialias Antialias {
			get { return CairoAPI.cairo_font_options_get_antialias (handle); }
			set { CairoAPI.cairo_font_options_set_antialias (handle, value); }
		}

		public HintMetrics HintMetrics {
			get { return CairoAPI.cairo_font_options_get_hint_metrics (handle);}
			set { CairoAPI.cairo_font_options_set_hint_metrics (handle, value); }
		}

		public HintStyle HintStyle {
			get { return CairoAPI.cairo_font_options_get_hint_style (handle);}
			set { CairoAPI.cairo_font_options_set_hint_style (handle, value); }
		}

		public Status Status {
			get { return CairoAPI.cairo_font_options_status (handle); }
		}

		public SubpixelOrder SubpixelOrder {
			get { return CairoAPI.cairo_font_options_get_subpixel_order (handle);}
			set { CairoAPI.cairo_font_options_set_subpixel_order (handle, value); }
		}
	}
}

