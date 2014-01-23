//
// Mono.Cairo.Pattern.cs
//
// Author: Jordi Mas (jordi@ximian.com)
//         Hisham Mardam Bey (hisham.mardambey@gmail.com)
// (C) Ximian Inc, 2004.
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
   
	public class Pattern : IDisposable
	{
		[Obsolete]
		protected IntPtr pattern = IntPtr.Zero;

		public static Pattern Lookup (IntPtr pattern, bool owner)
		{
			if (pattern == IntPtr.Zero)
				return null;
			
			PatternType pt = NativeMethods.cairo_pattern_get_type (pattern);
			switch (pt) {
			case PatternType.Solid:
				return new SolidPattern (pattern, owner);
			case PatternType.Surface:
				return new SurfacePattern (pattern, owner);
			case PatternType.Linear:
				return new LinearGradient (pattern, owner);
			case PatternType.Radial:
				return new RadialGradient (pattern, owner);
			default:
				return new Pattern (pattern, owner);
			}
		}

		[Obsolete]
		protected Pattern ()
		{
		}
		
		internal Pattern (IntPtr handle, bool owned)
		{
			Handle = handle;
			if (!owned)
				NativeMethods.cairo_pattern_reference (handle);
			if (CairoDebug.Enabled)
				CairoDebug.OnAllocated (handle);
		}

		~Pattern ()
		{
			Dispose (false);
		}
		
		[Obsolete ("Use the SurfacePattern constructor")]
		public Pattern (Surface surface)
			: this ( NativeMethods.cairo_pattern_create_for_surface (surface.Handle), true)
		{
		}
		
		[Obsolete]
		protected void Reference ()
		{
			NativeMethods.cairo_pattern_reference (pattern);
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (!disposing || CairoDebug.Enabled)
				CairoDebug.OnDisposed<Pattern> (Handle, disposing);

			if (!disposing|| Handle == IntPtr.Zero)
				return;

			NativeMethods.cairo_pattern_destroy (Handle);
			Handle = IntPtr.Zero;
		}

		[Obsolete ("Use Dispose()")]
		public void Destroy ()
		{
			Dispose ();
		}

		public Status Status
		{
			get { return NativeMethods.cairo_pattern_status (Handle); }
		}

		public Extend Extend
		{
			get { return NativeMethods.cairo_pattern_get_extend (Handle); }
			set { NativeMethods.cairo_pattern_set_extend (Handle, value); }
		}

		public Matrix Matrix {
			set {
				NativeMethods.cairo_pattern_set_matrix (Handle, value);
			}

			get {
				Matrix m = new Matrix ();
				NativeMethods.cairo_pattern_get_matrix (Handle, m);
				return m;
			}
		}

#pragma warning disable 612
		public IntPtr Handle {
			get { return pattern; }
			private set { pattern = value; }
		}
#pragma warning restore 612

		[Obsolete]
		public IntPtr Pointer {
			get { return pattern; }
		}

		public PatternType PatternType {
			get { return NativeMethods.cairo_pattern_get_type (Handle); }
		}
	}
}

