//
// System.Drawing.Margins.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System;

namespace System.Drawing.Printing {
	/// <summary>
	/// Summary description for Margins.
	/// </summary>

	public class Margins {// : IClonable {
		/// <summary>
		/// left margin in hundredths of an inch
		/// </summary>
		int left;
		/// <summary>
		/// right margin in hundredths of an inch
		/// </summary>
		int right;
		/// <summary>
		/// top margin in hundredths of an inch
		/// </summary>
		int top;
		/// <summary>
		/// bottom margin in hundredths of an inch
		/// </summary>
		int bottom;

		public Margins() {
			left = 100;
			right = 100;
			top = 100;
			bottom = 100;
		}
		public Margins(int left, int right, int top, int bottom) {
			//Verify parameters
			if(left < 0)
				throw new System.ArgumentException("All Margins must be greater than 0", "left");
			if(right < 0)
				throw new System.ArgumentException("All Margins must be greater than 0", "right");
			if(top < 0)
				throw new System.ArgumentException("All Margins must be greater than 0", "top");
			if(bottom < 0)
				throw new System.ArgumentException("All Margins must be greater than 0", "bottom");
			//Set proprities
			this.left = left;
			this.right = right;
			this.top = top;
			this.bottom = bottom;
		}

		public int Left{
			get{
				return left;
			}
			set{
				left = value;
			}
		}

		public int Right{
			get{
				return right;
			}
			set{
				right = value;
			}
		}

		public int Top{
			get{
				return top;
			}
			set{
				top = value;
			}
		}

		public int Bottom{
			get{
				return bottom;
			}
			set{
				bottom = value;
			}
		}
	}
}
