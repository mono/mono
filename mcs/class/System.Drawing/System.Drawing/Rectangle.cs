//
// System.Drawing.Rectangle.cs
//
// Author:
//   Mike Kestner (mkestner@speakeasy.net)
//
// (C) 2001 Mike Kestner
//

using System;

namespace System.Drawing {
	
	public struct Rectangle { 
		
		// Private position and size fields.
		private Point loc;
		private Size sz;

		/// <summary>
		///	Empty Shared Field
		/// </summary>
		///
		/// <remarks>
		///	An uninitialized Rectangle Structure.
		/// </remarks>
		
		public static readonly Rectangle Empty;

		/// <summary>
		///	Ceiling Shared Method
		/// </summary>
		///
		/// <remarks>
		///	Produces a Rectangle structure from a RectangleF 
		///	structure by taking the ceiling of the X, Y, Width,
		///	and Height properties.
		/// </remarks>
		
		public static Rectangle Ceiling (RectangleF value)
		{
			int x, y, w, h;
			checked {
				x = (int) Math.Ceiling (value.X);
				y = (int) Math.Ceiling (value.Y);
				w = (int) Math.Ceiling (value.Width);
				h = (int) Math.Ceiling (value.Height);
			}

			return new Rectangle (x, y, w, h);
		}

		/// <summary>
		///	FromLTRB Shared Method
		/// </summary>
		///
		/// <remarks>
		///	Produces a Rectangle structure from left, top, right,
		///	and bottom coordinates.
		/// </remarks>
		
		public static Rectangle FromLTRB (int left, int top,
						  int right, int bottom)
		{
			return new Rectangle (left, top, right - left,
					      bottom - top);
		}

		/// <summary>
		///	Inflate Shared Method
		/// </summary>
		///
		/// <remarks>
		///	Produces a new Rectangle by inflating an existing 
		///	Rectangle by the specified coordinate values.
		/// </remarks>
		
		public static Rectangle Inflate (Rectangle rect, int x, int y)
		{
			Rectangle r = new Rectangle (rect.Location, rect.Size);
			r.Inflate (x, y);
			return r;
		}

		/// <summary>
		///	Inflate Method
		/// </summary>
		///
		/// <remarks>
		///	Inflates the Rectangle by a specified width and height.
		/// </remarks>
		
		public void Inflate (int width, int height)
		{
			Inflate (new Size (width, height));
		}

		/// <summary>
		///	Inflate Method
		/// </summary>
		///
		/// <remarks>
		///	Inflates the Rectangle by a specified Size.
		/// </remarks>
		
		public void Inflate (Size sz)
		{
			loc -= sz;
			Size ds = new Size (sz.Width * 2, sz.Height * 2);
			this.sz += ds;
		}

		/// <summary>
		///	Intersect Shared Method
		/// </summary>
		///
		/// <remarks>
		///	Produces a new Rectangle by intersecting 2 existing 
		///	Rectangles. Returns null if there is no	intersection.
		/// </remarks>
		
		public static Rectangle Intersect (Rectangle r1, Rectangle r2)
		{
			Rectangle r = new Rectangle (r1.Location, r1.Size);
			r.Intersect (r2);
			return r;
		}

		/// <summary>
		///	Intersect Method
		/// </summary>
		///
		/// <remarks>
		///	Replaces the Rectangle with the intersection of itself
		///	and another Rectangle.
		/// </remarks>
		
		public void Intersect (Rectangle r)
		{
			if (!IntersectsWith (r)) {
				loc = Point.Empty;
				sz = Size.Empty;
			}

			X = Math.Max (Left, r.Left);
			Y = Math.Max (Top, r.Top);
			Width = Math.Min (Right, r.Right) - X;
			Height = Math.Min (Bottom, r.Bottom) - Y;
		}

		/// <summary>
		///	Round Shared Method
		/// </summary>
		///
		/// <remarks>
		///	Produces a Rectangle structure from a RectangleF by
		///	rounding the X, Y, Width, and Height properties.
		/// </remarks>
		
		public static Rectangle Round (RectangleF value)
		{
			int x, y, w, h;
			checked {
				x = (int) Math.Round (value.X);
				y = (int) Math.Round (value.Y);
				w = (int) Math.Round (value.Width);
				h = (int) Math.Round (value.Height);
			}

			return new Rectangle (x, y, w, h);
		}

		/// <summary>
		///	Truncate Shared Method
		/// </summary>
		///
		/// <remarks>
		///	Produces a Rectangle structure from a RectangleF by
		///	truncating the X, Y, Width, and Height properties.
		/// </remarks>
		
		// LAMESPEC: Should this be floor, or a pure cast to int?

		public static Rectangle Truncate (RectangleF value)
		{
			int x, y, w, h;
			checked {
				x = (int) value.X;
				y = (int) value.Y;
				w = (int) value.Width;
				h = (int) value.Height;
			}

			return new Rectangle (x, y, w, h);
		}

		/// <summary>
		///	Union Shared Method
		/// </summary>
		///
		/// <remarks>
		///	Produces a new Rectangle from the union of 2 existing 
		///	Rectangles.
		/// </remarks>
		
		public static Rectangle Union (Rectangle r1, Rectangle r2)
		{
			return FromLTRB (Math.Min (r1.Left, r2.Left),
					 Math.Min (r1.Top, r2.Top),
					 Math.Max (r1.Right, r2.Right),
					 Math.Max (r1.Bottom, r2.Bottom));
		}

		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two Rectangle objects. The return value is
		///	based on the equivalence of the Location and Size 
		///	properties of the two Rectangles.
		/// </remarks>

		public static bool operator == (Rectangle r1, Rectangle r2)
		{
			return ((r1.Location == r2.Location) && 
				(r1.Size == r2.Size));
		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two Rectangle objects. The return value is
		///	based on the equivalence of the Location and Size 
		///	properties of the two Rectangles.
		/// </remarks>

		public static bool operator != (Rectangle r1, Rectangle r2)
		{
			return ((r1.Location != r2.Location) || 
				(r1.Size != r2.Size));
		}
		

		// -----------------------
		// Public Constructors
		// -----------------------

		/// <summary>
		///	Rectangle Constructor
		/// </summary>
		///
		/// <remarks>
		///	Creates a Rectangle from Point and Size values.
		/// </remarks>
		
		public Rectangle (Point loc, Size sz)
		{
			this.loc = loc;
			this.sz = sz;
		}

		/// <summary>
		///	Rectangle Constructor
		/// </summary>
		///
		/// <remarks>
		///	Creates a Rectangle from a specified x,y location and
		///	width and height values.
		/// </remarks>
		
		public Rectangle (int x, int y, int width, int height)
		{
			loc = new Point (x, y);
			sz = new Size (width, height);
		}



		/// <summary>
		///	Bottom Property
		/// </summary>
		///
		/// <remarks>
		///	The Y coordinate of the bottom edge of the Rectangle.
		///	Read only.
		/// </remarks>
		
		public int Bottom {
			get {
				return Y + Height;
			}
		}

		/// <summary>
		///	Height Property
		/// </summary>
		///
		/// <remarks>
		///	The Height of the Rectangle.
		/// </remarks>
		
		public int Height {
			get {
				return sz.Height;
			}
			set {
				sz.Height = value;
			}
		}

		/// <summary>
		///	IsEmpty Property
		/// </summary>
		///
		/// <remarks>
		///	Indicates if the width or height are zero. Read only.
		/// </remarks>
		
		public bool IsEmpty {
			get {
				return ((sz.Width == 0) || (sz.Height == 0));
			}
		}

		/// <summary>
		///	Left Property
		/// </summary>
		///
		/// <remarks>
		///	The X coordinate of the left edge of the Rectangle.
		///	Read only.
		/// </remarks>
		
		public int Left {
			get {
				return X;
			}
		}

		/// <summary>
		///	Location Property
		/// </summary>
		///
		/// <remarks>
		///	The Location of the top-left corner of the Rectangle.
		/// </remarks>
		
		public Point Location {
			get {
				return loc;
			}
			set {
				loc = value;
			}
		}

		/// <summary>
		///	Right Property
		/// </summary>
		///
		/// <remarks>
		///	The X coordinate of the right edge of the Rectangle.
		///	Read only.
		/// </remarks>
		
		public int Right {
			get {
				return X + Width;
			}
		}

		/// <summary>
		///	Size Property
		/// </summary>
		///
		/// <remarks>
		///	The Size of the Rectangle.
		/// </remarks>
		
		public Size Size {
			get {
				return sz;
			}
			set {
				sz = value;
			}
		}

		/// <summary>
		///	Top Property
		/// </summary>
		///
		/// <remarks>
		///	The Y coordinate of the top edge of the Rectangle.
		///	Read only.
		/// </remarks>
		
		public int Top {
			get {
				return Y;
			}
		}

		/// <summary>
		///	Width Property
		/// </summary>
		///
		/// <remarks>
		///	The Width of the Rectangle.
		/// </remarks>
		
		public int Width {
			get {
				return sz.Width;
			}
			set {
				sz.Width = value;
			}
		}

		/// <summary>
		///	X Property
		/// </summary>
		///
		/// <remarks>
		///	The X coordinate of the Rectangle.
		/// </remarks>
		
		public int X {
			get {
				return loc.X;
			}
			set {
				loc.X = value;
			}
		}

		/// <summary>
		///	Y Property
		/// </summary>
		///
		/// <remarks>
		///	The Y coordinate of the Rectangle.
		/// </remarks>
		
		public int Y {
			get {
				return loc.Y;
			}
			set {
				loc.Y = value;
			}
		}

		/// <summary>
		///	Contains Method
		/// </summary>
		///
		/// <remarks>
		///	Checks if an x,y coordinate lies within this Rectangle.
		/// </remarks>
		
		public bool Contains (int x, int y)
		{
			return ((x >= Left) && (x <= Right) && 
				(y >= Top) && (y <= Bottom));
		}

		/// <summary>
		///	Contains Method
		/// </summary>
		///
		/// <remarks>
		///	Checks if a Point lies within this Rectangle.
		/// </remarks>
		
		public bool Contains (Point pt)
		{
			return Contains (pt.X, pt.Y);
		}

		/// <summary>
		///	Contains Method
		/// </summary>
		///
		/// <remarks>
		///	Checks if a Rectangle lies entirely within this 
		///	Rectangle.
		/// </remarks>
		
		public bool Contains (Rectangle rect)
		{
			return (rect == Intersect (this, rect));
		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this Rectangle and another object.
		/// </remarks>
		
		public override bool Equals (object o)
		{
			if (!(o is Rectangle))
				return false;

			return (this == (Rectangle) o);
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
			return loc.GetHashCode()^sz.GetHashCode();
		}

		public bool IntersectsWith (Rectangle r)
		{
			return !((Left > r.Right) || (Right < r.Left) ||
			    (Top > r.Bottom) || (Bottom < r.Top));
		}

		/// <summary>
		///	Offset Method
		/// </summary>
		///
		/// <remarks>
		///	Moves the Rectangle a specified distance.
		/// </remarks>

		public void Offset (int dx, int dy)
		{
			X += dx;
			Y += dy;
		}
		
		/// <summary>
		///	Offset Method
		/// </summary>
		///
		/// <remarks>
		///	Moves the Rectangle a specified distance.
		/// </remarks>

		public void Offset (Point pt)
		{
			loc.Offset(pt.X, pt.Y);
		}
		
		/// <summary>
		///	ToString Method
		/// </summary>
		///
		/// <remarks>
		///	Formats the Rectangle as a string in (x,y,w,h) notation.
		/// </remarks>
		
		public override string ToString ()
		{
			return String.Format ("[{0},{1},{2},{3}]", 
					      X, Y, Width, Height);
		}

	}
}
