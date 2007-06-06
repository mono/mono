//
// System.Data.SqlClient.SqlBulkCopyColumnMappingCollection.cs
//
// Author:
//   Nagappan A <anagappan@novell.com>
//

//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.Data;
using System.Data.Common;
using System.Collections;

namespace System.Data.SqlClient
{
	public sealed class SqlBulkCopyColumnMappingCollection : CollectionBase {

		#region Methods
	
		[MonoTODO]
		public SqlBulkCopyColumnMapping Add (SqlBulkCopyColumnMapping bulkCopyColumnMapping)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlBulkCopyColumnMapping Add (int sourceColumnIndex, int destinationColumnIndex)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlBulkCopyColumnMapping Add (int sourceColumnIndex, string destinationColumn)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlBulkCopyColumnMapping Add (string sourceColumn, int destinationColumnIndex)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlBulkCopyColumnMapping Add (string sourceColumn, string destinationColumn)
		{
			throw new NotImplementedException ();
		}

		public new void Clear ()
		{
			throw new NotImplementedException ();
		}

		public bool Contains (SqlBulkCopyColumnMapping value)
		{
			return List.Contains (value);
		}

		public int IndexOf (SqlBulkCopyColumnMapping value)
		{
			return List.IndexOf (value);
		}

		public void Insert (int index, SqlBulkCopyColumnMapping value)
		{
			if (index < 0 || index > base.Count)
				throw new ArgumentOutOfRangeException ("Index is out of range");
			List.Insert (index, value);
		}

		public void Remove (SqlBulkCopyColumnMapping value)
		{
			List.Remove (value);
		}

		[MonoTODO]
		public new void RemoveAt (int index)
		{
			if (index < 0 || index > base.Count)
				throw new ArgumentOutOfRangeException ("Index is out of range");
			// FIXME: Implement WriteToServer
			base.RemoveAt (index);
		}

		#endregion
	
	}
}

#endif
