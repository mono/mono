//
// System.Drawing.Size.cs
//
// Author:
//   Mike Kestner (mkestner@speakeasy.net)
//
// (C) 2001 Mike Kestner
//

using System;
using System.Runtime.Serialization;

namespace System.Drawing {
	
	[Serializable]
	public struct Size { 
		
		// Private Height and width fields.
		int width, height;

		// -----------------------
		// Public Shared Members
		// -----------------------

		/// <summary>
		///	Empty Shared Field
		/// </summary>
		///
		/// <remarks>
		///	An uninitialized Size Structure.
		/// </remarks>
		
		public static readonly Size Empty;

		/// <summary>
		///	Ceiling Shared Method
		/// </summary>
		///
		/// <remarks>
		///	Produces a Size structure from a SizeF structure by
		///	taking the ceiling of the Width and Height properties.
		/// </remarks>
		
		public static Size Ceiling (SizeF value)
		{
			int w, h;
			checked {
				w = (int) Math.Ceiling (value.Width);
				h = (int) Math.Ceiling (value.Height);
			}

			return new Size (w, h);
		}

		/// <summary>
		///	Round Shared Method
		/// </summary>
		///
		/// <remarks>
		///	Produces a Size structure from a SizeF structure by
		///	rounding the Width and Height properties.
		/// </remarks>
		
		public static Size Round (SizeF value)
		{
			int w, h;
			checked {
				w = (int) Math.Round (value.Width);
				h = (int) Math.Round (value.Height);
			}

			return new Size (w, h);
		}

		/// <summary>
		///	Truncate Shared Method
		/// </summary>
		///
		/// <remarks>
		///	Produces a Size structure from a SizeF structure by
		///	truncating the Width and Height properties.
		/// </remarks>
		
		public static Size Truncate (SizeF value)
		{
			int w, h;
			checked {
				w = (int) value.Width;
				h = (int) value.Height;
			}

			return new Size (w, h);
		}

		/// <summary>
		///	Addition Operator
		/// </summary>
		///
		/// <remarks>
		///	Addition of two Size structures.
		/// </remarks>

		public static Size operator + (Size sz1, Size sz2)
		{
			return new Size (sz1.Width + sz2.Width, 
					 sz1.Height + sz2.Height);
		}
		
		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two Size objects. The return value is
		///	based on the equivalence of the Width and Height 
		///	properties of the two Sizes.
		/// </remarks>

		public static bool operator == (Size sz_a, Size sz_b)
		{
			return ((sz_a.Width == sz_b.Width) && 
				(sz_a.Height == sz_b.Height));
		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two Size objects. The return value is
		///	based on the equivalence of the Width and Height 
		///	properties of the two Sizes.
		/// </remarks>

		public static bool operator != (Size sz_a, Size sz_b)
		{
			return ((sz_a.Width != sz_b.Width) || 
				(sz_a.Height != sz_b.Height));
		}
		
		/// <summary>
		///	Subtraction Operator
		/// </summary>
		///
		/// <remarks>
		///	Subtracts two Size structures.
		/// </remarks>

		public static Size operator - (Size sz1, Size sz2)
		{
			return new Size (sz1.Width - sz2.Width, 
					 sz1.Height - sz2.Height);
		}
		
		/// <summary>
		///	Size to Point Conversion
		/// </summary>
		///
		/// <remarks>
		///	Returns a Point based on the dimensions of a given 
		///	Size. Requires explicit cast.
		/// </remarks>

		public static explicit operator Point (Size sz)
		{
			return new Point (sz.Width, sz.Height);
		}

		/// <summary>
		///	Size to SizeF Conversion
		/// </summary>
		///
		/// <remarks>
		///	Creates a SizeF based on the dimensions of a given 
		///	Size. No explicit cast is required.
		/// </remarks>

		public static implicit operator SizeF (Size sz)
		{
			return new SizeF (sz.Width, sz.Height);
		}


		// -----------------------
		// Public Constructors
		// -----------------------

		/// <summary>
		///	Size Constructor
		/// </summary>
		///
		/// <remarks>
		///	Creates a Size from a Point value.
		/// </remarks>
		
		public Size (Point pt)
		{
			width = pt.X;
			height = pt.Y;
		}

		/// <summary>
		///	Size Constructor
		/// </summary>
		///
		/// <remarks>
		///	Creates a Size from specified dimensions.
		/// </remarks>
		
		public Size (int width, int height)
		{
			this.width = width;
			this.height = height;
		}

		// -----------------------
		// Public Instance Members
		// -----------------------

		/// <summary>
		///	IsEmpty Property
		/// </summary>
		///
		/// <remarks>
		///	Indicates if both Width and Height are zero.
		/// </remarks>
		
		public bool IsEmpty {
			get {
				return ((Width == 0) && (Height == 0));
			}
		}

		/// <summary>
		///	Width Property
		/// </summary>
		///
		/// <remarks>
		///	The Width coordinate of the Size.
		/// </remarks>
		
		public int Width {
			get {
				return width;
			}
			set {
				width = value;
			}
		}

		/// <summary>
		///	Height Property
		/// </summary>
		///
		/// <remarks>
		///	The Height coordinate of the Size.
		/// </remarks>
		
		public int Height {
			get {
				return height;
			}
			set {
				height = value;
			}
		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this Size and another object.
		/// </remarks>
		
		public override bool Equals (object o)
		{
			if (!(o is Size))
				return false;

			return (this == (Size) o);
		}

		/// <summary>
		///	GetHashCode Method
		/// </summary>
		///
		/// <remarks>
		///	Calculates a hashing value.
		/// </remarks>
		
		public override int GetHashCode ()
		{
			return Width^Height;
		}

		/// <summary>
		///	ToString Method
		/// </summary>
		///
		/// <remarks>
		///	Formats the Size as a string in coordinate notation.
		/// </remarks>
		
		public override string ToString ()
		{
			return String.Format ("[{0},{1}]", Width, Height);
		}

	}
}
