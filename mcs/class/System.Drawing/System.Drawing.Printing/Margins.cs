//
// System.Drawing.Margins.cs
//
// Authors:
//   Dennis Hayes (dennish@Raytek.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Ximian, Inc
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
using System.ComponentModel;

namespace System.Drawing.Printing
{
#if NET_2_0
	[Serializable]
#endif
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
					throw new System.ArgumentException("All Margins must be greater than 0", "right");
				right = value;
			}
		}

		public int Top {
			get {
				return top;
			}
			set {
				if (top < 0)
					throw new System.ArgumentException("All Margins must be greater than 0", "top");
				top = value;
			}
		}

		public int Bottom {
			get {
				return bottom;
			}
			set {
				if (bottom < 0)
					throw new System.ArgumentException("All Margins must be greater than 0", "bottom");
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
