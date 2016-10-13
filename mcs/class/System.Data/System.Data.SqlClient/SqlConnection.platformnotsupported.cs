//
// SqlConnection.cs
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

using System.Collections;
using System.Data.Common;

namespace System.Data.SqlClient
{
	public sealed class SqlConnection : DbConnection, IDbConnection, ICloneable
	{
		const string EXCEPTION_MESSAGE = "System.Data.SqlClient.SqlConnection is not supported on the current platform.";

		public SqlConnection () : this (null)
		{
		}

		public SqlConnection (string connectionString)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public SqlConnection (string connectionString, SqlCredential cred)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override string ConnectionString {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public SqlCredential Credentials {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public Guid ClientConnectionId {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override int ConnectionTimeout {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override string Database {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override string DataSource {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public int PacketSize {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override string ServerVersion {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override ConnectionState State {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public string WorkstationId {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public bool FireInfoMessageEventOnUserErrors {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public bool StatisticsEnabled {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		protected override DbProviderFactory DbProviderFactory {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public event SqlInfoMessageEventHandler InfoMessage;

		public new SqlTransaction BeginTransaction ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public new SqlTransaction BeginTransaction (IsolationLevel iso)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public SqlTransaction BeginTransaction (string transactionName)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public SqlTransaction BeginTransaction (IsolationLevel iso, string transactionName)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override void ChangeDatabase (string database)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override void Close ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public new SqlCommand CreateCommand ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		protected override void Dispose (bool disposing)
		{
		}

#if !MOBILE
		public void EnlistDistributedTransaction (ITransaction transaction)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}
#endif

		object ICloneable.Clone ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		protected override DbTransaction BeginDbTransaction (IsolationLevel isolationLevel)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		protected override DbCommand CreateDbCommand ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override void Open ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override DataTable GetSchema ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override DataTable GetSchema (String collectionName)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override DataTable GetSchema (String collectionName, string [] restrictionValues)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public static void ChangePassword (string connectionString, string newPassword)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public static void ClearAllPools ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public static void ClearPool (SqlConnection connection)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void ResetStatistics ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public IDictionary RetrieveStatistics ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}
	}
}
