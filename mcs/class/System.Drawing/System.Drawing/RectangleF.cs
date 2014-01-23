//
// System.Drawing.RectangleF.cs
//
// Author:
//   Mike Kestner (mkestner@speakeasy.net)
//
// Copyright (C) 2001 Mike Kestner
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
using System.ComponentModel;

namespace System.Drawing
{
	[Serializable]
	public struct RectangleF
	{
                private float x, y, width, height;

		/// <summary>
		///	Empty Shared Field
		/// </summary>
		///
		/// <remarks>
		///	An uninitialized RectangleF Structure.
		/// </remarks>
		
		public static readonly RectangleF Empty;

#if TARGET_JVM
		internal java.awt.geom.Rectangle2D NativeObject {
			get {
				return new java.awt.geom.Rectangle2D.Float(X,Y,Width,Height);
			}
		}
#endif

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
			return new RectangleF (left, top, right - left, bottom - top);
		}

		/// <summary>
		///	Inflate Shared Method
		/// </summary>
		///
		/// <remarks>
		///	Produces a new RectangleF by inflating an existing 
		///	RectangleF by the specified coordinate values.
		/// </remarks>
		
		public static RectangleF Inflate (RectangleF rect, 
						  float x, float y)
		{
			RectangleF ir = new RectangleF (rect.X, rect.Y, rect.Width, rect.Height);
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
		
		public void Inflate (float x, float y)
		{
			Inflate (new SizeF (x, y));
		}

		/// <summary>
		///	Inflate Method
		/// </summary>
		///
		/// <remarks>
		///	Inflates the RectangleF by a specified Size.
		/// </remarks>
		
		public void Inflate (SizeF size)
		{
                        x -= size.Width;
                        y -= size.Height;
                        width += size.Width * 2;
                        height += size.Height * 2;                        
		}

		/// <summary>
		///	Intersect Shared Method
		/// </summary>
		///
		/// <remarks>
		///	Produces a new RectangleF by intersecting 2 existing 
		///	RectangleFs. Returns null if there is no intersection.
		/// </remarks>
		
		public static RectangleF Intersect (RectangleF a, 
						    RectangleF b)
		{
			// MS.NET returns a non-empty rectangle if the two rectangles
			// touch each other
			if (!a.IntersectsWithInclusive (b))
				return Empty;

			return FromLTRB (
				Math.Max (a.Left, b.Left),
				Math.Max (a.Top, b.Top),
				Math.Min (a.Right, b.Right),
				Math.Min (a.Bottom, b.Bottom));
		}

		/// <summary>
		///	Intersect Method
		/// </summary>
		///
		/// <remarks>
		///	Replaces the RectangleF with the intersection of itself
		///	and another RectangleF.
		/// </remarks>
		
		public void Intersect (RectangleF rect)
		{
			this = RectangleF.Intersect (this, rect);
		}

		/// <summary>
		///	Union Shared Method
		/// </summary>
		///
		/// <remarks>
		///	Produces a new RectangleF from the union of 2 existing 
		///	RectangleFs.
		/// </remarks>
		
		public static RectangleF Union (RectangleF a, RectangleF b)
		{
			return FromLTRB (Math.Min (a.Left, b.Left),
					 Math.Min (a.Top, b.Top),
					 Math.Max (a.Right, b.Right),
					 Math.Max (a.Bottom, b.Bottom));
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

		public static bool operator == (RectangleF left, RectangleF right)
		{
			return (left.X == right.X) && (left.Y == right.Y) &&
                                (left.Width == right.Width) && (left.Height == right.Height);
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

		public static bool operator != (RectangleF left, RectangleF right)
		{
			return (left.X != right.X) || (left.Y != right.Y) ||
                                (left.Width != right.Width) || (left.Height != right.Height);
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
			return new RectangleF (r.X, r.Y, r.Width, r.Height);
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
		
		public RectangleF (PointF location, SizeF size)
		{
			x = location.X;
                        y = location.Y;
                        width = size.Width;
                        height = size.Height;
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
			this.x = x;
                        this.y = y;
                        this.width = width;
                        this.height = height;
		}


#if TARGET_JVM
		internal RectangleF (java.awt.geom.RectangularShape r2d) {
			this.x = (float) r2d.getX ();
			this.y = (float) r2d.getY ();
			this.width = (float) r2d.getWidth ();
			this.height = (float) r2d.getHeight ();
		}
#endif

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
				return height;
			}
			set {
				height = value;
			}
		}

		/// <summary>
		///	IsEmpty Property
		/// </summary>
		///
		/// <remarks>
		///	Indicates if the width or height are zero. Read only.
		/// </remarks>
		//		
		[Browsable (false)]
		public bool IsEmpty {
			get {
				return (width <= 0 || height <= 0);
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
				return new PointF (x, y);
			}
			set {
				x = value.X;
                                y = value.Y;
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
				return new SizeF (width, height);
			}
			set {
				width = value.Width;
                                height = value.Height;
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
				return width;
			}
			set {
				width = value;
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
		///	The Y coordinate of the RectangleF.
		/// </remarks>
		
		public float Y {
			get {
				return y;
			}
			set {
				y = value;
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
			return ((x >= Left) && (x < Right) && 
				(y >= Top) && (y < Bottom));
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
			return X <= rect.X && Right >= rect.Right && Y <= rect.Y && Bottom >= rect.Bottom;
		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this RectangleF and an object.
		/// </remarks>
		
		public override bool Equals (object obj)
		{
			if (!(obj is RectangleF))
				return false;

			return (this == (RectangleF) obj);
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
			return (int) (x + y + width + height);
		}

		/// <summary>
		///	IntersectsWith Method
		/// </summary>
		///
		/// <remarks>
		///	Checks if a RectangleF intersects with this one.
		/// </remarks>

		public bool IntersectsWith (RectangleF rect)
		{
			return !((Left >= rect.Right) || (Right <= rect.Left) ||
			    (Top >= rect.Bottom) || (Bottom <= rect.Top));
		}

		private bool IntersectsWithInclusive (RectangleF r)
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

		public void Offset (float x, float y)
		{
			X += x;
			Y += y;
		}
		
		/// <summary>
		///	Offset Method
		/// </summary>
		///
		/// <remarks>
		///	Moves the RectangleF a specified distance.
		/// </remarks>

		public void Offset (PointF pos)
		{
			Offset (pos.X, pos.Y);
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
			return String.Format ("{{X={0},Y={1},Width={2},Height={3}}}",
						 x, y, width, height);
		}

	}
}
