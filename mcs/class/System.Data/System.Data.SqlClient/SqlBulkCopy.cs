//
// System.Data.SqlClient.SqlBulkCopy.cs
//
// Author:
//   Nagappan A (anagappan@novell.com)
//
// (C) Novell, Inc 2007

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

namespace System.Data.SqlClient {
	public sealed class SqlBulkCopy : IDisposable 
	{
		#region Fields

		private int _batchSize = 0;
		private int _bulkCopyTimeout = 0;
		private SqlBulkCopyColumnMappingCollection _columnMappingCollection = new SqlBulkCopyColumnMappingCollection ();
		private string _destinationTableName = null;

		#endregion

		#region Constructors
		[MonoTODO]
		public SqlBulkCopy (SqlConnection connection)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlBulkCopy (string connectionString)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlBulkCopy (string connectionString, SqlBulkCopyOptions copyOptions)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlBulkCopy (SqlConnection connection, SqlBulkCopyOptions copyOptions, SqlTransaction externalTransaction)
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region Properties

		public int BatchSize {
			get { return _batchSize; }
			set { _batchSize = value; }
		}

		public int BulkCopyTimeout {
			get { return _bulkCopyTimeout; }
			set { _bulkCopyTimeout = value; }
		}

		public SqlBulkCopyColumnMappingCollection ColumnMappings  {
			get { return _columnMappingCollection; }
		}

		[MonoTODO]
		public string DestinationTableName {
			// FIXME: Related to WriteToServer
			get { return _destinationTableName; }
			set { _destinationTableName = value; }
		}

		#endregion

		#region Methods

		[MonoTODO]
		public void Close ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void WriteToServer (DataRow [] rows)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void WriteToServer (DataTable table)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void WriteToServer (IDataReader reader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void WriteToServer (DataTable table, DataRowState rowState)
		{
			throw new NotImplementedException ();
		}

		#endregion

		[MonoTODO]
		void IDisposable.Dispose ()
		{
			throw new NotImplementedException ();
		}

	}
}

#endif
