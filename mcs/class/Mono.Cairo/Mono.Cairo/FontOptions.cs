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

		public FontOptions () : this (NativeMethods.cairo_font_options_create ())
		{
		}

		~FontOptions ()
		{
			Dispose (false);
		}

		internal FontOptions (IntPtr handle)
		{
			this.handle = handle;
			if (CairoDebug.Enabled)
				CairoDebug.OnAllocated (handle);
		}

		public FontOptions Copy ()
		{
			return new FontOptions (NativeMethods.cairo_font_options_copy (handle));
		}

		[Obsolete ("Use Dispose()")]
		public void Destroy ()
		{
			Dispose ();
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (!disposing || CairoDebug.Enabled)
				CairoDebug.OnDisposed<FontOptions> (handle, disposing);

			if (!disposing|| handle == IntPtr.Zero)
				return;

			NativeMethods.cairo_font_options_destroy (handle);
			handle = IntPtr.Zero;
		}

		public static bool operator == (FontOptions options, FontOptions other)
		{
			return Equals (options, other);
		}

		public static bool operator != (FontOptions options, FontOptions other)
		{
			return !(options == other);
		}

		public override bool Equals (object other)
		{
			return Equals (other as FontOptions);
		}

		bool Equals (FontOptions options)
		{
			return options != null && NativeMethods.cairo_font_options_equal (Handle, options.Handle);
		}

		public IntPtr Handle {
			get { return handle; }
		}

		public override int GetHashCode ()
		{
			return (int) NativeMethods.cairo_font_options_hash (handle);
		}
		
		public void Merge (FontOptions other)
		{
			if (other == null)
				throw new ArgumentNullException ("other");
			NativeMethods.cairo_font_options_merge (handle, other.Handle);
		}

		public Antialias Antialias {
			get { return NativeMethods.cairo_font_options_get_antialias (handle); }
			set { NativeMethods.cairo_font_options_set_antialias (handle, value); }
		}

		public HintMetrics HintMetrics {
			get { return NativeMethods.cairo_font_options_get_hint_metrics (handle);}
			set { NativeMethods.cairo_font_options_set_hint_metrics (handle, value); }
		}

		public HintStyle HintStyle {
			get { return NativeMethods.cairo_font_options_get_hint_style (handle);}
			set { NativeMethods.cairo_font_options_set_hint_style (handle, value); }
		}

		public Status Status {
			get { return NativeMethods.cairo_font_options_status (handle); }
		}

		public SubpixelOrder SubpixelOrder {
			get { return NativeMethods.cairo_font_options_get_subpixel_order (handle);}
			set { NativeMethods.cairo_font_options_set_subpixel_order (handle, value); }
		}
	}
}

