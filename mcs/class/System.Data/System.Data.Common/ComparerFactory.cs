using System;
using System.Collections;

namespace System.Data.Common
{
	/// <summary>
	/// Summary description for ComparerFactory.
	/// </summary>
	public class DBComparerFactory
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
