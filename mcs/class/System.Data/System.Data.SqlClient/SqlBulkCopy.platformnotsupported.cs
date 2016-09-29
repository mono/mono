//
// SqlBulkCopy.cs
//
// Author:
//       Rolf Bjarne Kvinge <rolf@xamarin.com>
//
// Copyright (c) 2016 Xamarin, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.SqlClient {
	public sealed class SqlBulkCopy : IDisposable
	{
		const string EXCEPTION_MESSAGE = "System.Data.SqlClient.SqlBulkCopy is not supported on the current platform.";

		public SqlBulkCopy (SqlConnection connection)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public SqlBulkCopy (string connectionString)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public SqlBulkCopy (string connectionString, SqlBulkCopyOptions copyOptions)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public SqlBulkCopy (SqlConnection connection, SqlBulkCopyOptions copyOptions, SqlTransaction externalTransaction)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public int BatchSize {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public int BulkCopyTimeout {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public SqlBulkCopyColumnMappingCollection ColumnMappings  {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public string DestinationTableName {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public bool EnableStreaming {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public int NotifyAfter {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public void Close ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void WriteToServer (DataRow [] rows)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void WriteToServer (DataTable table)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void WriteToServer (IDataReader reader)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void WriteToServer (DataTable table, DataRowState rowState)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void WriteToServer (DbDataReader reader)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task WriteToServerAsync (DbDataReader reader)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task WriteToServerAsync (DbDataReader reader, CancellationToken cancellationToken)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		private void RowsCopied (long rowsCopied)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public event SqlRowsCopiedEventHandler SqlRowsCopied;

		void IDisposable.Dispose ()
		{
		}
	}
}
