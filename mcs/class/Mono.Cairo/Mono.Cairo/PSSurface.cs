//
// Mono.Cairo.PostscriptSurface.cs
//
// Authors:
//    John Luke
//
// (C) John Luke, 2006.
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

namespace Cairo {

	public class PSSurface : Surface
	{
		internal PSSurface (IntPtr handle, bool owns) : base (handle, owns)
		{
		}

		public PSSurface (string filename, double width, double height)
			: base (NativeMethods.cairo_ps_surface_create (filename, width, height), true)
		{
		}

		public void BeginPageSetup ()
		{
			NativeMethods.cairo_ps_surface_dsc_begin_page_setup (Handle);
		}

		public void BeginSetup ()
		{
			NativeMethods.cairo_ps_surface_dsc_begin_setup (Handle);
		}

		public void DscComment (string comment)
		{
			NativeMethods.cairo_ps_surface_dsc_comment (Handle, comment);
		}

		public void SetSize (double width, double height)
		{
			NativeMethods.cairo_ps_surface_set_size (Handle, width, height);
		}
	}
}
