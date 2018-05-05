//
// Mono.Cairo.GLSurface.cs
//
// Authors:
//			JP Bruyère (jp_bruyere@hotmail.com)
//
// This is an OO wrapper API for the Cairo API
//
// Copyright (C) 2016 JP Bruyère
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

	public class GLSurface : Surface
	{
		
		public GLSurface (IntPtr ptr, bool own) : base (ptr, own)
		{}

		public GLSurface (Device device, Cairo.Content content, uint tex, int width, int height)
			: base (NativeMethods.cairo_gl_surface_create_for_texture (device.Handle, (uint)content, tex, width, height), true)
		{}

		public GLSurface (EGLDevice device, IntPtr eglSurf, int width, int height)
			: base (NativeMethods.cairo_gl_surface_create_for_egl (device.Handle, eglSurf, width, height), true)
		{}

		public GLSurface (GLXDevice device, IntPtr window, int width, int height)
			: base (NativeMethods.cairo_gl_surface_create_for_window (device.Handle, window, width, height),true)
		{}

		public GLSurface (WGLDevice device, IntPtr hdc, int width, int height)
			: base (NativeMethods.cairo_gl_surface_create_for_dc (device.Handle, hdc, width, height), true)
		{}

		public void SwapBuffers(){
			NativeMethods.cairo_gl_surface_swapbuffers (this.Handle);
		}
	}
}
