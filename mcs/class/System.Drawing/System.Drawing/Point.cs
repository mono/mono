//
// System.Drawing.Point.cs
//
// Author:
//   Mike Kestner (mkestner@speakeasy.net)
//
// (C) 2001 Mike Kestner
//

using System;

namespace System.Drawing {

	[Serializable]	
	public struct Point { 
		
		// Private x and y coordinate fields.
		int x, y;

		// -----------------------
		// Public Shared Members
		// -----------------------

		/// <summary>
		///	Empty Shared Field
		/// </summary>
		///
		/// <remarks>
		///	An uninitialized Point Structure.
		/// </remarks>
		
		public static readonly Point Empty;

		/// <summary>
		///	Ceiling Shared Method
		/// </summary>
		///
		/// <remarks>
		///	Produces a Point structure from a PointF structure by
		///	taking the ceiling of the X and Y properties.
		/// </remarks>
		
		public static Point Ceiling (PointF value)
		{
			int x, y;
			checked {
				x = (int) Math.Ceiling (value.X);
				y = (int) Math.Ceiling (value.Y);
			}

			return new Point (x, y);
		}

		/// <summary>
		///	Round Shared Method
		/// </summary>
		///
		/// <remarks>
		///	Produces a Point structure from a PointF structure by
		///	rounding the X and Y properties.
		/// </remarks>
		
		public static Point Round (PointF value)
		{
			int x, y;
			checked {
				x = (int) Math.Round (value.X);
				y = (int) Math.Round (value.Y);
			}

			return new Point (x, y);
		}

		/// <summary>
		///	Truncate Shared Method
		/// </summary>
		///
		/// <remarks>
		///	Produces a Point structure from a PointF structure by
		///	truncating the X and Y properties.
		/// </remarks>
		
		// LAMESPEC: Should this be floor, or a pure cast to int?

		public static Point Truncate (PointF value)
		{
			int x, y;
			checked {
				x = (int) value.X;
				y = (int) value.Y;
			}

			return new Point (x, y);
		}

		/// <summary>
		///	Addition Operator
		/// </summary>
		///
		/// <remarks>
		///	Translates a Point using the Width and Height
		///	properties of the given <typeref>Size</typeref>.
		/// </remarks>

		public static Point operator + (Point pt, Size sz)
		{
			return new Point (pt.X + sz.Width, pt.Y + sz.Height);
		}
		
		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two Point objects. The return value is
		///	based on the equivalence of the X and Y properties 
		///	of the two points.
		/// </remarks>

		public static bool operator == (Point pt_a, Point pt_b)
		{
			return ((pt_a.X == pt_b.X) && (pt_a.Y == pt_b.Y));
		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two Point objects. The return value is
		///	based on the equivalence of the X and Y properties 
		///	of the two points.
		/// </remarks>

		public static bool operator != (Point pt_a, Point pt_b)
		{
			return ((pt_a.X != pt_b.X) || (pt_a.Y != pt_b.Y));
		}
		
		/// <summary>
		///	Subtraction Operator
		/// </summary>
		///
		/// <remarks>
		///	Translates a Point using the negation of the Width 
		///	and Height properties of the given Size.
		/// </remarks>

		public static Point operator - (Point pt, Size sz)
		{
			return new Point (pt.X - sz.Width, pt.Y - sz.Height);
		}
		
		/// <summary>
		///	Point to Size Conversion
		/// </summary>
		///
		/// <remarks>
		///	Returns a Size based on the Coordinates of a given 
		///	Point. Requires explicit cast.
		/// </remarks>

		public static explicit operator Size (Point pt)
		{
			return new Size (pt.X, pt.Y);
		}

		/// <summary>
		///	Point to PointF Conversion
		/// </summary>
		///
		/// <remarks>
		///	Creates a PointF based on the coordinates of a given 
		///	Point. No explicit cast is required.
		/// </remarks>

		public static implicit operator PointF (Point pt)
		{
			return new PointF (pt.X, pt.Y);
		}


		// -----------------------
		// Public Constructors
		// -----------------------

		/// <summary>
		///	Point Constructor
		/// </summary>
		///
		/// <remarks>
		///	Creates a Point from an integer which holds the X
		///	coordinate in the high order 16 bits and the Y
		///	coordinate in the low order 16 bits.
		/// </remarks>
		
		public Point (int dw)
		{
			x = dw >> 16;
			y = dw & 0xffff;
		}

		/// <summary>
		///	Point Constructor
		/// </summary>
		///
		/// <remarks>
		///	Creates a Point from a Size value.
		/// </remarks>
		
		public Point (Size sz)
		{
			x = sz.Width;
			y = sz.Height;
		}

		/// <summary>
		///	Point Constructor
		/// </summary>
		///
		/// <remarks>
		///	Creates a Point from a specified x,y coordinate pair.
		/// </remarks>
		
		public Point (int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		// -----------------------
		// Public Instance Members
		// -----------------------

		/// <summary>
		///	IsEmpty Property
		/// </summary>
		///
		/// <remarks>
		///	Indicates if both X and Y are zero.
		/// </remarks>
		
		public bool IsEmpty {
			get {
				return ((x == 0) && (y == 0));
			}
		}

		/// <summary>
		///	X Property
		/// </summary>
		///
		/// <remarks>
		///	The X coordinate of the Point.
		/// </remarks>
		
		public int X {
			get {
				return x;
			}
			set {
				x = value;
			}
		}

		/// <summary>
		///	Y Property
		/// </summary>
		///
		/// <remarks>
		///	The Y coordinate of the Point.
		/// </remarks>
		
		public int Y {
			get {
				return y;
			}
			set {
				y = value;
			}
		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this Point and another object.
		/// </remarks>
		
		public override bool Equals (object o)
		{
			if (!(o is Point))
				return false;

			return (this == (Point) o);
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
			return x^y;
		}

		/// <summary>
		///	Offset Method
		/// </summary>
		///
		/// <remarks>
		///	Moves the Point a specified distance.
		/// </remarks>

		public void Offset (int dx, int dy)
		{
			x += dx;
			y += dy;
		}
		
		/// <summary>
		///	ToString Method
		/// </summary>
		///
		/// <remarks>
		///	Formats the Point as a string in coordinate notation.
		/// </remarks>
		
		public override string ToString ()
		{
			return String.Format ("[{0},{1}]", x, y);
		}

	}
}
