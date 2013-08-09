// Copyright (C) 2011 Novell, Inc (http://www.novell.com)
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

namespace Cairo
{

	[StructLayout(LayoutKind.Sequential)]
	public struct RectangleInt {
		public int X;
		public int Y;
		public int Width;
		public int Height;
	}

	public enum RegionOverlap {
		In,
		Out,
		Part,
	}

	public class Region : IDisposable {

		IntPtr handle;
		public IntPtr Handle {
			get { return handle; }
		}

		[Obsolete]
		public Region (IntPtr handle) : this (handle, false) {}

		public Region (IntPtr handle, bool owned)
		{
			this.handle = handle;
			if (!owned)
				NativeMethods.cairo_region_reference (handle);
			if (CairoDebug.Enabled)
				CairoDebug.OnAllocated (handle);
		}

		public Region () : this (NativeMethods.cairo_region_create () , true)
		{
		}

		public Region (RectangleInt rect)
		{
			handle = NativeMethods.cairo_region_create_rectangle (ref rect);
		}

		public Region (RectangleInt[] rects)
		{
			handle = NativeMethods.cairo_region_create_rectangles (rects, rects.Length);
		}

		public Region Copy ()
		{
			return new Region (NativeMethods.cairo_region_copy (Handle), true);
		}

		~Region ()
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
				CairoDebug.OnDisposed<Region> (handle, disposing);

			if (!disposing|| handle == IntPtr.Zero)
				return;

			NativeMethods.cairo_region_destroy (Handle);
			handle = IntPtr.Zero;
		}

		public override bool Equals (object obj)
		{
			return (obj is Region) && NativeMethods.cairo_region_equal (Handle, (obj as Region).Handle);
		}

		public override int GetHashCode ()
		{
			return Handle.GetHashCode ();
		}

		public Status Status {
			get { return NativeMethods.cairo_region_status (Handle); }
		}

		public RectangleInt Extents {
			get {
				RectangleInt result;
				NativeMethods.cairo_region_get_extents (Handle, out result);
				return result;
			}
		}

		public int NumRectangles {
			get { return NativeMethods.cairo_region_num_rectangles (Handle); }
		}

		public RectangleInt GetRectangle (int nth)
		{
			RectangleInt val;
			NativeMethods.cairo_region_get_rectangle (Handle, nth, out val);
			return val;
		}

		public bool IsEmpty {
			get { return NativeMethods.cairo_region_is_empty (Handle); }
		}

		public RegionOverlap ContainsPoint (RectangleInt rectangle)
		{
			return NativeMethods.cairo_region_contains_rectangle (Handle, ref rectangle);
		}

		public bool ContainsPoint (int x, int y)
		{
			return NativeMethods.cairo_region_contains_point (Handle, x, y);
		}

		public void Translate (int dx, int dy)
		{
			NativeMethods.cairo_region_translate (Handle, dx, dy);
		}

		public Status Subtract (Region other)
		{
			return NativeMethods.cairo_region_subtract (Handle, other.Handle);
		}

		public Status SubtractRectangle (RectangleInt rectangle)
		{
			return NativeMethods.cairo_region_subtract_rectangle (Handle, ref rectangle);
		}

		public Status Intersect (Region other)
		{
			return NativeMethods.cairo_region_intersect (Handle, other.Handle);
		}

		public Status IntersectRectangle (RectangleInt rectangle)
		{
			return NativeMethods.cairo_region_intersect_rectangle (Handle, ref rectangle);
		}

		public Status Union (Region other)
		{
			return NativeMethods.cairo_region_union (Handle, other.Handle);
		}

		public Status UnionRectangle (RectangleInt rectangle)
		{
			return NativeMethods.cairo_region_union_rectangle (Handle, ref rectangle);
		}

		public Status Xor (Region other)
		{
			return NativeMethods.cairo_region_xor (Handle, other.Handle);
		}

		public Status XorRectangle (RectangleInt rectangle)
		{
			return NativeMethods.cairo_region_xor_rectangle (Handle, ref rectangle);
		}
	}
}
