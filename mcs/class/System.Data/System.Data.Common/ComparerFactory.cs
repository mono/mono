
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
using System.Collections;

namespace System.Data.Common
{
	/// <summary>
	/// Summary description for ComparerFactory.
	/// </summary>
	internal class DBComparerFactory
	{
		private static IComparer comparableComparer = new ComparebleComparer();
		private static IComparer ignoreCaseComparer = new IgnoreCaseComparer();
		private static IComparer caseComparer = new CaseComparer();
		private static IComparer byteArrayComparer = new ByteArrayComparer();
		private static Type icomparerType = typeof (IComparable);

		public static IComparer GetComparer (Type type, bool ignoreCase)
		{
			if (type == typeof (string)) {
				if (ignoreCase)
					return ignoreCaseComparer;
				return caseComparer;
			}
			if (icomparerType.IsAssignableFrom(type))
				return comparableComparer;
			if (type == typeof (byte[]))
				return byteArrayComparer;
			return null;
		}

		class ComparebleComparer :IComparer
		{
			#region IComparer Members

			public int Compare(object x, object y)
			{
				if (x == DBNull.Value) {
					if (y == DBNull.Value) 
						return 0;

					return -1;
				}

				if (y == DBNull.Value) 
					return 1;
				
				return ((IComparable)x).CompareTo (y);
			}

			#endregion
		}

		class CaseComparer : IComparer
		{
			#region IComparer Members

			public int Compare(object x, object y)
			{
				if (x == DBNull.Value) {
					if (y == DBNull.Value) 
						return 0;

					return -1;
				}

				if (y == DBNull.Value) 
					return 1;
				
				return String.Compare ((string)x, (string)y, false);
			}

			#endregion
		}

		class IgnoreCaseComparer : IComparer
		{
			#region IComparer Members

			public int Compare(object x, object y)
			{
				if (x == DBNull.Value) {
					if (y == DBNull.Value) 
						return 0;

					return -1;
				}

				if (y == DBNull.Value) 
					return 1;
				
				return String.Compare ((string)x, (string)y, true);
			}

			#endregion
		}

		class ByteArrayComparer : IComparer
		{
			#region IComparer Members

			public int Compare(object x, object y)
			{
				if (x == DBNull.Value) {
					if (y == DBNull.Value) 
						return 0;

					return -1;
				}

				if (y == DBNull.Value) 
					return 1;
				
				byte[] o1 = (byte[])x;
				byte[] o2 = (byte[])y;
				int len  = o1.Length;
				int lenb = o2.Length;

				for (int i = 0; ; i++) {
					int a = 0;
					int b = 0;

					if (i < len) {
						a = o1[i];
					} 
					else if (i >= lenb) {
						return 0;
					}

					if (i < lenb) {
						b = o2[i];
					}

					if (a > b) {
						return 1;
					}

					if (b > a) {
						return -1;
					}
				}
			}

			#endregion
		}
	}
}
