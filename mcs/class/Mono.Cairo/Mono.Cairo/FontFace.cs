//
// Mono.Cairo.FontFace.cs
//
// Author:
//   Alp Toker (alp@atoker.com)
//   Miguel de Icaza (miguel@novell.com)
//
// (C) Ximian Inc, 2003.
//
// This is an OO wrapper API for the Cairo API.
//
// Copyright (C) 2004, 2007 Novell, Inc (http://www.novell.com)
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
	public class FontFace : IDisposable
	{
		IntPtr handle;

		internal static FontFace Lookup (IntPtr handle, bool owner)
		{
			if (handle == IntPtr.Zero)
				return null;
			return new FontFace (handle, owner);
		}

		~FontFace ()
		{
			Dispose (false);
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (!disposing || CairoDebug.Enabled)
				CairoDebug.OnDisposed<FontFace> (handle, disposing);

			if (!disposing|| handle == IntPtr.Zero)
				return;

			NativeMethods.cairo_font_face_destroy (handle);
			handle = IntPtr.Zero;
		}

		[Obsolete]
		public FontFace (IntPtr handle) : this (handle, true)
		{
		}

		public FontFace (IntPtr handle, bool owned)
		{
			this.handle = handle;
			if (!owned)
				NativeMethods.cairo_font_face_reference (handle);
			if (CairoDebug.Enabled)
				CairoDebug.OnAllocated (handle);
		}

		public IntPtr Handle {
			get {
				return handle;
			}
		}

		public Status Status {
			get {
				return NativeMethods.cairo_font_face_status (handle);
			}
		}
		
		public FontType FontType {
			get {
				return NativeMethods.cairo_font_face_get_type (handle);
			}
		}

		public uint ReferenceCount {
			get { return NativeMethods.cairo_font_face_get_reference_count (handle); }
		}
	}
}

