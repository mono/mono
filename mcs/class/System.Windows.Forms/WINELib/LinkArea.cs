//
// System.Drawing.LinkArea.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Windows.Forms {
	[Serializable]
	public struct LinkArea { 

		private int start;
		private int length;

		// -----------------------
		// Public Constructor
		// -----------------------

		/// <summary>
		/// 
		/// </summary>
		///
		/// <remarks>
		///
		/// </remarks>
		
		public LinkArea (int Start, int Length)
		{
			start = Start;
			length = Length;
		}

		// -----------------------
		// Public Shared Members
		// -----------------------

		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two LinkArea objects. The return value is
		///	based on the equivalence of the Start and Length properties of the two objects.
		/// </remarks>

		public static bool operator == (LinkArea la_a, 
			LinkArea la_b) {

			return ((la_a.start == la_b.start) &&
				(la_a.length == la_b.length));
		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two LinkArea objects. The return value is
		///	based on the equivalence of the Start and Length properties of the two objects.
		/// </remarks>
		public static bool operator != (LinkArea la_a, 
			LinkArea la_b) {
			return ((la_a.start != la_b.start) ||
				(la_a.length != la_b.length));
		}
		
		// -----------------------
		// Public Instance Members
		// -----------------------

		public bool IsEmpty {
			get{
				// Start can be 0, so no way to know if it is empty.
				// Docs seem to say Start must/should be set before
				// length, os if length is valid, start must also be ok.
				return length!=0;
			}
		}

		public int Start {
			get{
				return start;
			}
			set{
				start = value;
			}
		}

		public int Length {
			get{
				return length;
			}
			set{
				length = value;
			}
		}



		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this LinkArea and another object.
		/// </remarks>
		
		public override bool Equals (object obj)
		{
			if (!(obj is LinkArea))
				return false;

			return (this == (LinkArea) obj);
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
			return (int)( start ^ length);
		}

		/// <summary>
		///	ToString Method
		/// </summary>
		///
		/// <remarks>
		///	Formats the LinkArea as a string.
		/// </remarks>
		
		public override string ToString ()
		{
			return String.Format ("[{0},{1}]", start, length );
		}
	}
}
