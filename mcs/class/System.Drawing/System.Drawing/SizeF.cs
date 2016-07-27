//
// System.Drawing.SizeF.cs
//
// Author:
//   Mike Kestner (mkestner@speakeasy.net)
//
// Copyright (C) 2001 Mike Kestner
// Copyright (C) 2004 Novell, Inc. http://www.novell.com
//

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
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace System.Drawing
{
	[Serializable]
	[ComVisible (true)]
#if !MONOTOUCH && !MONOMAC && FEATURE_TYPECONVERTER
	[TypeConverter (typeof (SizeFConverter))]
#endif
	public struct SizeF
	{
		// Private height and width fields.
		private float width, height;

		// -----------------------
		// Public Shared Members
		// -----------------------

		/// <summary>
		///	Empty Shared Field
		/// </summary>
		///
		/// <remarks>
		///	An uninitialized SizeF Structure.
		/// </remarks>
		
		public static readonly SizeF Empty;

		/// <summary>
		///	Addition Operator
		/// </summary>
		///
		/// <remarks>
		///	Addition of two SizeF structures.
		/// </remarks>

		public static SizeF operator + (SizeF sz1, SizeF sz2)
		{
			return new SizeF (sz1.Width + sz2.Width, 
					  sz1.Height + sz2.Height);
		}
		
		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two SizeF objects. The return value is
		///	based on the equivalence of the Width and Height 
		///	properties of the two Sizes.
		/// </remarks>

		public static bool operator == (SizeF sz1, SizeF sz2)
		{
			return ((sz1.Width == sz2.Width) && 
				(sz1.Height == sz2.Height));
		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two SizeF objects. The return value is
		///	based on the equivalence of the Width and Height 
		///	properties of the two Sizes.
		/// </remarks>

		public static bool operator != (SizeF sz1, SizeF sz2)
		{
			return ((sz1.Width != sz2.Width) || 
				(sz1.Height != sz2.Height));
		}
		
		/// <summary>
		///	Subtraction Operator
		/// </summary>
		///
		/// <remarks>
		///	Subtracts two SizeF structures.
		/// </remarks>

		public static SizeF operator - (SizeF sz1, SizeF sz2)
		{
			return new SizeF (sz1.Width - sz2.Width, 
					  sz1.Height - sz2.Height);
		}
		
		/// <summary>
		///	SizeF to PointF Conversion
		/// </summary>
		///
		/// <remarks>
		///	Returns a PointF based on the dimensions of a given 
		///	SizeF. Requires explicit cast.
		/// </remarks>

		public static explicit operator PointF (SizeF size)
		{
			return new PointF (size.Width, size.Height);
		}


		// -----------------------
		// Public Constructors
		// -----------------------

		/// <summary>
		///	SizeF Constructor
		/// </summary>
		///
		/// <remarks>
		///	Creates a SizeF from a PointF value.
		/// </remarks>
		
		public SizeF (PointF pt)
		{
			width = pt.X;
			height = pt.Y;
		}

		/// <summary>
		///	SizeF Constructor
		/// </summary>
		///
		/// <remarks>
		///	Creates a SizeF from an existing SizeF value.
		/// </remarks>
		
		public SizeF (SizeF size)
		{
			width = size.Width;
			height = size.Height;
		}

		/// <summary>
		///	SizeF Constructor
		/// </summary>
		///
		/// <remarks>
		///	Creates a SizeF from specified dimensions.
		/// </remarks>
		
		public SizeF (float width, float height)
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
		
		[Browsable (false)]
		public bool IsEmpty {
			get {
				return ((width == 0.0) && (height == 0.0));
			}
		}

		/// <summary>
		///	Width Property
		/// </summary>
		///
		/// <remarks>
		///	The Width coordinate of the SizeF.
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
		///	Height Property
		/// </summary>
		///
		/// <remarks>
		///	The Height coordinate of the SizeF.
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
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this SizeF and another object.
		/// </remarks>
		
		public override bool Equals (object obj)
		{
			if (!(obj is SizeF))
				return false;

			return (this == (SizeF) obj);
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
			return (int) width ^ (int) height;
		}

		public PointF ToPointF ()
		{
			return new PointF (width, height);
		}

		public Size ToSize ()
		{
			int w, h;
			checked {
				w = (int) width;
				h = (int) height;
			}

			return new Size (w, h);
		}

		/// <summary>
		///	ToString Method
		/// </summary>
		///
		/// <remarks>
		///	Formats the SizeF as a string in coordinate notation.
		/// </remarks>
		
		public override string ToString ()
		{
			return string.Format ("{{Width={0}, Height={1}}}", width.ToString (CultureInfo.CurrentCulture),
				height.ToString (CultureInfo.CurrentCulture));
		}

		public static SizeF Add (SizeF sz1, SizeF sz2)
		{
			return new SizeF (sz1.Width + sz2.Width, 
					  sz1.Height + sz2.Height);
		}
		
		public static SizeF Subtract (SizeF sz1, SizeF sz2)
		{
			return new SizeF (sz1.Width - sz2.Width, 
					  sz1.Height - sz2.Height);
		}
	}
}
