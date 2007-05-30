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

		#region Fields

		private SqlBulkCopyColumnMappingCollection _collection = new SqlBulkCopyColumnMappingCollection ();

		#endregion
	
		#region Properties
	
		public new int Capacity {
			get { return _collection.Capacity; }
			set { _collection.Capacity = value; }
		}

		public new int Count {
			get { return _collection.Count; }
		}

		public SqlBulkCopyColumnMapping this [int index] {
			get { return _collection [index]; }
		}

		protected new ArrayList InnerList {
			get { return _collection.InnerList; }
		}

		protected new IList List {
			get { return _collection.List; }
		}

		#endregion
	
		#region Methods
	
		public SqlBulkCopyColumnMapping Add (SqlBulkCopyColumnMapping bulkCopyColumnMapping)
		{
			return _collection.Add (bulkCopyColumnMapping);
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
			return _collection.Contains (value);
		}

		public int IndexOf (SqlBulkCopyColumnMapping value)
		{
			return _collection.IndexOf (value);
		}

		public int Insert (int index, SqlBulkCopyColumnMapping value)
		{
			if (index < 0 || index > _collection.Count)
				throw new ArgumentOutOfRangeException ("Index is out of range");
			return _collection.Insert (index, value);
		}

		public int Remove (SqlBulkCopyColumnMapping value)
		{
			return _collection.Remove (value);
		}

		[MonoTODO]
		public new int RemoveAt (int index)
		{
			if (index < 0 || index > _collection.Count)
				throw new ArgumentOutOfRangeException ("Index is out of range");
			// FIXME: Implement WriteToServer
			return _collection.RemoveAt (index);
		}

		#endregion
	
	}
}

#endif
