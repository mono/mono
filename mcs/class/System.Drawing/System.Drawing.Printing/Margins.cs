//
// System.Drawing.Margins.cs
//
// Authors:
//   Dennis Hayes (dennish@Raytek.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Ximian, Inc
//

using System;
using System.ComponentModel;

namespace System.Drawing.Printing
{
	[TypeConverter (typeof (MarginsConverter))]
	public class Margins : ICloneable
	{
		int left;
		int right;
		int top;
		int bottom;

		public Margins()
		{
			left = 100;
			right = 100;
			top = 100;
			bottom = 100;
		}

		public Margins(int left, int right, int top, int bottom)
		{
			//Verify parameters
			if (left < 0)
				throw new System.ArgumentException("All Margins must be greater than 0", "left");
			if (right < 0)
				throw new System.ArgumentException("All Margins must be greater than 0", "right");
			if (top < 0)
				throw new System.ArgumentException("All Margins must be greater than 0", "top");
			if (bottom < 0)
				throw new System.ArgumentException("All Margins must be greater than 0", "bottom");

			//Set proprities
			this.left = left;
			this.right = right;
			this.top = top;
			this.bottom = bottom;
		}

		public int Left {
			get {
				return left;
			}
			set {
				if (left < 0)
					throw new System.ArgumentException("All Margins must be greater than 0", "left");
				left = value;
			}
		}

		public int Right {
			get {
				return right;
			}
			set {
				if (right < 0)
					throw new System.ArgumentException("All Margins must be greater than 0", "left");
				right = value;
			}
		}

		public int Top {
			get {
				return top;
			}
			set {
				if (top < 0)
					throw new System.ArgumentException("All Margins must be greater than 0", "left");
				top = value;
			}
		}

		public int Bottom {
			get {
				return bottom;
			}
			set {
				if (bottom < 0)
					throw new System.ArgumentException("All Margins must be greater than 0", "left");
				bottom = value;
			}
		}
		
		public object Clone()
		{
			return new Margins (this.Left, this.Right, this.Top, this.Bottom);
		}

		public override bool Equals (object obj)
		{	
			Margins m = obj as Margins;

			if (m == null)
				return false;
			if (m.Left == left && m.Right == right && m.Top == top && m.Bottom == bottom)
				return true;

			return false;
		}

		public override int GetHashCode ()
		{
			// Try to create a somewhat meaningful hash
			int hash = left + right * 2^8 + top * 2^16 + bottom * 2^24;
			return hash;
		}
		
		public override string ToString()
		{
			string ret = "[Margins Left={0} Right={1} Top={2} Bottom={3}]";
			return String.Format (ret, this.Left, this.Right, this.Top, this.Bottom);
		}
	}
}
