//
// System.Web.Util.WebEqualComparer.cs
//
// Authors:
//   Gaurav Vaish (my_scripts2001@yahoo.com, gvaish@iitk.ac.in)
//
// (c) Gaurav Vaish 2001
//

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
using System.Collections;

namespace System.Web.Util
{
	internal class WebEqualComparer : IComparer
	{
		private static IComparer defC;

		public WebEqualComparer()
		{
		}

		public static IComparer Default
		{
			get
			{
				if(defC == null)
				{
					defC = new WebEqualComparer();
				}
				return defC;
			}
		}

		/// <summary>
		/// To compare two strings
		/// </summary>
		/// <remarks>
		/// Cannot apply String.Compare(..) since I am at web
		/// </remarks>
		int IComparer.Compare(object left, object right)
		{
			string leftStr, rightStr;
			leftStr = null;
			rightStr = null;
			if(left is string)
			{
				leftStr = (string)left;
			}
			if(right is string)
			{
				rightStr = (string)right;
			}

			if(leftStr==null || rightStr==null)
			{
				throw new ArgumentException();
			}

			int ll = leftStr.Length;
			int lr = rightStr.Length;
			if(ll==0 && lr==0)
			{
				return 0;
			}

			if(ll==0 || lr==0)
			{
				return ( (ll > 0) ? 1 : -1);
			}

			char cl,cr;
			int i=0;
			for(i=0; i < leftStr.Length; i++)
			{
				if(i==lr)
				{
					return 1;
				}
				cl = leftStr[i];
				cr = leftStr[i];
				if(cl==cr)
				{
					continue;
				}
				UnicodeCategory ucl = Char.GetUnicodeCategory(cl);
				UnicodeCategory ucr = Char.GetUnicodeCategory(cr);
				if(ucl==ucr)
				{
					return ( (cl > cr) ? 1 : -1 );
				}
				cl = Char.ToLower(cl);
				cr = Char.ToLower(cr);
				if(cl!=cr)
				{
					return ( (cl > cr) ? 1 : -1);
				}
			}
			return ( (i==lr) ? 0 : -1 );
		}
	}
}
