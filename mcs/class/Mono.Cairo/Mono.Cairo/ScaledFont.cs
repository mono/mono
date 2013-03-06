//
// Mono.Cairo.ScaledFont.cs
//
// (c) 2008 Jordi Mas i Hernandez (jordimash@gmail.com)
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

namespace Cairo {
   
	public class ScaledFont : IDisposable
	{
		protected IntPtr handle = IntPtr.Zero;

		internal ScaledFont (IntPtr handle, bool owner)
		{
			this.handle = handle;
			if (!owner)
				NativeMethods.cairo_scaled_font_reference (handle);
			if (CairoDebug.Enabled)
				CairoDebug.OnAllocated (handle);
		}

		public ScaledFont (FontFace fontFace, Matrix matrix, Matrix ctm, FontOptions options)
			: this (NativeMethods.cairo_scaled_font_create (fontFace.Handle, matrix, ctm, options.Handle), true)
		{
		}

		~ScaledFont ()
		{
			Dispose (false);
		}

		public IntPtr Handle {
			get {
				return handle;
			}
		}

		public FontExtents FontExtents {
			get {
				FontExtents extents;
				NativeMethods.cairo_scaled_font_extents (handle, out extents);
				return extents;
			}
		}

		public Matrix FontMatrix {
			get {
				Matrix m;
				NativeMethods.cairo_scaled_font_get_font_matrix (handle, out m);
				return m;
			}
		}

		public FontType FontType {
			get {
				return NativeMethods.cairo_scaled_font_get_type (handle);
			}
		}

		public TextExtents GlyphExtents (Glyph[] glyphs)
		{
			IntPtr ptr = Context.FromGlyphToUnManagedMemory (glyphs);
			TextExtents extents;

			NativeMethods.cairo_scaled_font_glyph_extents (handle, ptr, glyphs.Length, out extents);

			Marshal.FreeHGlobal (ptr);
			return extents;
		}
	
		public Status Status
		{
			get { return NativeMethods.cairo_scaled_font_status (handle); }
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (!disposing || CairoDebug.Enabled)
				CairoDebug.OnDisposed<ScaledFont> (handle, disposing);

			if (!disposing|| handle == IntPtr.Zero)
				return;

			NativeMethods.cairo_scaled_font_destroy (handle);
			handle = IntPtr.Zero;
		}

		[Obsolete]
		protected void Reference ()
		{
			NativeMethods.cairo_scaled_font_reference (handle);
		}
	}
}

