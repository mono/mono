//
// System.Drawing.RectangleF.cs
//
// Author:
//   Mike Kestner (mkestner@speakeasy.net)
//
// (C) 2001 Mike Kestner
//

using System;
using System.ComponentModel;

namespace System.Drawing {
	
	[Serializable]
	public struct RectangleF { 
		
		// Private position and size fields.
		private PointF loc;
		private SizeF sz;

		/// <summary>
		///	Empty Shared Field
		/// </summary>
		///
		/// <remarks>
		///	An uninitialized RectangleF Structure.
		/// </remarks>
		
		public static readonly RectangleF Empty;


		/// <summary>
		///	FromLTRB Shared Method
		/// </summary>
		///
		/// <remarks>
		///	Produces a RectangleF structure from left, top, right,
		///	and bottom coordinates.
		/// </remarks>
		
		public static RectangleF FromLTRB (float left, float top,
						   float right, float bottom)
		{
			return new RectangleF (left, top, right - left,
					       bottom - top);
		}

		/// <summary>
		///	Inflate Shared Method
		/// </summary>
		///
		/// <remarks>
		///	Produces a new RectangleF by inflating an existing 
		///	RectangleF by the specified coordinate values.
		/// </remarks>
		
		public static RectangleF Inflate (RectangleF r, 
						  float x, float y)
		{
			RectangleF ir = new RectangleF (r.Location, r.Size);
			ir.Inflate (x, y);
			return ir;
		}

		/// <summary>
		///	Inflate Method
		/// </summary>
		///
		/// <remarks>
		///	Inflates the RectangleF by a specified width and height.
		/// </remarks>
		
		public void Inflate (float width, float height)
		{
			Inflate (new SizeF (width, height));
		}

		/// <summary>
		///	Inflate Method
		/// </summary>
		///
		/// <remarks>
		///	Inflates the RectangleF by a specified Size.
		/// </remarks>
		
		public void Inflate (SizeF sz)
		{
			Offset(sz.Width, sz.Height);
			SizeF ds = new SizeF (sz.Width * 2, sz.Height * 2);
			this.sz += ds;
		}

		/// <summary>
		///	Intersect Shared Method
		/// </summary>
		///
		/// <remarks>
		///	Produces a new RectangleF by intersecting 2 existing 
		///	RectangleFs. Returns null if there is no intersection.
		/// </remarks>
		
		public static RectangleF Intersect (RectangleF r1, 
						    RectangleF r2)
		{
			RectangleF r = new RectangleF (r1.Location, r1.Size);
			r.Intersect (r2);
			return r;
		}

		/// <summary>
		///	Intersect Method
		/// </summary>
		///
		/// <remarks>
		///	Replaces the RectangleF with the intersection of itself
		///	and another RectangleF.
		/// </remarks>
		
		public void Intersect (RectangleF r)
		{
			if (!IntersectsWith (r)) {
				loc = PointF.Empty;
				sz = SizeF.Empty;
			}

			X = Math.Max (Left, r.Left);
			Y = Math.Max (Top, r.Top);
			Width = Math.Min (Right, r.Right) - X;
			Height = Math.Min (Bottom, r.Bottom) - Y;
		}

		/// <summary>
		///	Union Shared Method
		/// </summary>
		///
		/// <remarks>
		///	Produces a new RectangleF from the union of 2 existing 
		///	RectangleFs.
		/// </remarks>
		
		public static RectangleF Union (RectangleF r1, RectangleF r2)
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
		///	Compares two RectangleF objects. The return value is
		///	based on the equivalence of the Location and Size 
		///	properties of the two RectangleFs.
		/// </remarks>

		public static bool operator == (RectangleF r1, RectangleF r2)
		{
			return ((r1.Location == r2.Location) && 
				(r1.Size == r2.Size));
		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two RectangleF objects. The return value is
		///	based on the equivalence of the Location and Size 
		///	properties of the two RectangleFs.
		/// </remarks>

		public static bool operator != (RectangleF r1, RectangleF r2)
		{
			return ((r1.Location != r2.Location) || 
				(r1.Size != r2.Size));
		}
		
		/// <summary>
		///	Rectangle to RectangleF Conversion
		/// </summary>
		///
		/// <remarks>
		///	Converts a Rectangle object to a RectangleF.
		/// </remarks>

		public static implicit operator RectangleF (Rectangle r)
		{
			return new RectangleF (r.Location, r.Size);
		}
		

		// -----------------------
		// Public Constructors
		// -----------------------

		/// <summary>
		///	RectangleF Constructor
		/// </summary>
		///
		/// <remarks>
		///	Creates a RectangleF from PointF and SizeF values.
		/// </remarks>
		
		public RectangleF (PointF loc, SizeF sz)
		{
			this.loc = loc;
			this.sz = sz;
		}

		/// <summary>
		///	RectangleF Constructor
		/// </summary>
		///
		/// <remarks>
		///	Creates a RectangleF from a specified x,y location and
		///	width and height values.
		/// </remarks>
		
		public RectangleF (float x, float y, float width, float height)
		{
			loc = new PointF (x, y);
			sz = new SizeF (width, height);
		}



		/// <summary>
		///	Bottom Property
		/// </summary>
		///
		/// <remarks>
		///	The Y coordinate of the bottom edge of the RectangleF.
		///	Read only.
		/// </remarks>
		
		[Browsable (false)]
		public float Bottom {
			get {
				return Y + Height;
			}
		}

		/// <summary>
		///	Height Property
		/// </summary>
		///
		/// <remarks>
		///	The Height of the RectangleF.
		/// </remarks>
		
		public float Height {
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
		
		[Browsable (false)]
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
		///	The X coordinate of the left edge of the RectangleF.
		///	Read only.
		/// </remarks>
		
		[Browsable (false)]
		public float Left {
			get {
				return X;
			}
		}

		/// <summary>
		///	Location Property
		/// </summary>
		///
		/// <remarks>
		///	The Location of the top-left corner of the RectangleF.
		/// </remarks>
		
		[Browsable (false)]
		public PointF Location {
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
		///	The X coordinate of the right edge of the RectangleF.
		///	Read only.
		/// </remarks>
		
		[Browsable (false)]
		public float Right {
			get {
				return X + Width;
			}
		}

		/// <summary>
		///	Size Property
		/// </summary>
		///
		/// <remarks>
		///	The Size of the RectangleF.
		/// </remarks>
		
		[Browsable (false)]
		public SizeF Size {
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
		///	The Y coordinate of the top edge of the RectangleF.
		///	Read only.
		/// </remarks>
		
		[Browsable (false)]
		public float Top {
			get {
				return Y;
			}
		}

		/// <summary>
		///	Width Property
		/// </summary>
		///
		/// <remarks>
		///	The Width of the RectangleF.
		/// </remarks>
		
		public float Width {
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
		///	The X coordinate of the RectangleF.
		/// </remarks>
		
		public float X {
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
		///	The Y coordinate of the RectangleF.
		/// </remarks>
		
		public float Y {
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
		///	Checks if an x,y coordinate lies within this RectangleF.
		/// </remarks>
		
		public bool Contains (float x, float y)
		{
			return ((x >= Left) && (x <= Right) && 
				(y >= Top) && (y <= Bottom));
		}

		/// <summary>
		///	Contains Method
		/// </summary>
		///
		/// <remarks>
		///	Checks if a Point lies within this RectangleF.
		/// </remarks>
		
		public bool Contains (PointF pt)
		{
			return Contains (pt.X, pt.Y);
		}

		/// <summary>
		///	Contains Method
		/// </summary>
		///
		/// <remarks>
		///	Checks if a RectangleF lies entirely within this 
		///	RectangleF.
		/// </remarks>
		
		public bool Contains (RectangleF rect)
		{
			return (rect == Intersect (this, rect));
		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this RectangleF and an object.
		/// </remarks>
		
		public override bool Equals (object o)
		{
			if (!(o is RectangleF))
				return false;

			return (this == (RectangleF) o);
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

		/// <summary>
		///	IntersectsWith Method
		/// </summary>
		///
		/// <remarks>
		///	Checks if a RectangleF intersects with this one.
		/// </remarks>

		public bool IntersectsWith (RectangleF r)
		{
			return !((Left > r.Right) || (Right < r.Left) ||
			    (Top > r.Bottom) || (Bottom < r.Top));
		}

		/// <summary>
		///	Offset Method
		/// </summary>
		///
		/// <remarks>
		///	Moves the RectangleF a specified distance.
		/// </remarks>

		public void Offset (float dx, float dy)
		{
			X += dx;
			Y += dy;
		}
		
		/// <summary>
		///	Offset Method
		/// </summary>
		///
		/// <remarks>
		///	Moves the RectangleF a specified distance.
		/// </remarks>

		public void Offset (PointF pt)
		{
			Offset(pt.X, pt.Y);
		}
		
		/// <summary>
		///	ToString Method
		/// </summary>
		///
		/// <remarks>
		///	Formats the RectangleF in (x,y,w,h) notation.
		/// </remarks>
		
		public override string ToString ()
		{
			return String.Format ("[{0},{1},{2},{3}]", 
					      X, Y, Width, Height);
		}

	}
}
