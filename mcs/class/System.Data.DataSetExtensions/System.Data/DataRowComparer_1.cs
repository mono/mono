//
// DataRowComparer_1.cs
//
// Author:
//   Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc. http://www.novell.com
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
using System.Collections.Generic;

namespace System.Data
{
	public sealed class DataRowComparer<TRow> : IEqualityComparer<TRow> where TRow : DataRow
	{
		static readonly DataRowComparer<TRow> default_instance = new DataRowComparer<TRow> ();

		public static DataRowComparer<TRow> Default {
			get { return default_instance; }
		}

		private DataRowComparer ()
		{
		}

		[MonoTODO]
		public bool Equals (TRow leftRow, TRow rightRow)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int GetHashCode (TRow row)
		{
			throw new NotImplementedException ();
			/*
			int ret = row.GetType ().GetHashCode ();
			GetHashCodeForRowVersion (row, DataRowVersion.Original, ref ret);
			GetHashCodeForRowVersion (row, DataRowVersion.Current, ref ret);
			GetHashCodeForRowVersion (row, DataRowVersion.Proposed, ref ret);
			GetHashCodeForRowVersion (row, DataRowVersion.Default, ref ret);
			return ret;
		}

		void GetHashCodeForRowVersion (TRow row, DataRowVersion version, ref int hash)
		{
			if (!row.HasVersion (version))
				return;
			int local = 0;
			for (int i = 0; i < row.Table.Columns.Count; i++)
				local += row [i, version].GetHashCode () ^ 7;
			hash += local << (((int) version << 1) + 7);
			*/
		}
	}
}
