//
// SqlCommand.cs
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
using System.Data.Sql;
using System.Threading.Tasks;
using System.Threading;
using System.Xml;

namespace System.Data.SqlClient {
	public sealed class SqlCommand : DbCommand, IDbCommand, ICloneable
	{
		const string EXCEPTION_MESSAGE = "System.Data.SqlClient.SqlCommand is not supported on the current platform.";

		public SqlCommand()
			: this (String.Empty, null, null)
		{
		}

		public SqlCommand (string cmdText)
			: this (cmdText, null, null)
		{
		}

		public SqlCommand (string cmdText, SqlConnection connection)
			: this (cmdText, connection, null)
		{
		}

		public SqlCommand (string cmdText, SqlConnection connection, SqlTransaction transaction)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override string CommandText {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override int CommandTimeout {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override CommandType CommandType {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public new SqlConnection Connection {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override bool DesignTimeVisible {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public new SqlParameterCollection Parameters {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public new SqlTransaction Transaction {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override UpdateRowSource UpdatedRowSource {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public SqlNotificationRequest Notification {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public bool NotificationAutoEnlist {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override void Cancel ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public SqlCommand Clone ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public new SqlParameter CreateParameter ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override int ExecuteNonQuery ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public new SqlDataReader ExecuteReader ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public new SqlDataReader ExecuteReader (CommandBehavior behavior)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public new Task<SqlDataReader> ExecuteReaderAsync ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public new Task<SqlDataReader> ExecuteReaderAsync (CancellationToken cancellationToken)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public new Task<SqlDataReader> ExecuteReaderAsync (CommandBehavior behavior)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public new Task<SqlDataReader> ExecuteReaderAsync (CommandBehavior behavior, CancellationToken cancellationToken)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<XmlReader> ExecuteXmlReaderAsync ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<XmlReader> ExecuteXmlReaderAsync (CancellationToken cancellationToken)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override object ExecuteScalar ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public XmlReader ExecuteXmlReader ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		object ICloneable.Clone ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		protected override void Dispose (bool disposing)
		{
		}

		public override void Prepare ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void ResetCommandTimeout ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		protected override DbParameter CreateDbParameter ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		protected override DbDataReader ExecuteDbDataReader (CommandBehavior behavior)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		protected override DbConnection DbConnection {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		protected override DbParameterCollection DbParameterCollection {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		protected override DbTransaction DbTransaction {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public IAsyncResult BeginExecuteNonQuery ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public IAsyncResult BeginExecuteNonQuery (AsyncCallback callback, object stateObject)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public int EndExecuteNonQuery (IAsyncResult asyncResult)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public IAsyncResult BeginExecuteReader ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public IAsyncResult BeginExecuteReader (CommandBehavior behavior)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public IAsyncResult BeginExecuteReader (AsyncCallback callback, object stateObject)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public IAsyncResult BeginExecuteReader (AsyncCallback callback, object stateObject, CommandBehavior behavior)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public SqlDataReader EndExecuteReader (IAsyncResult asyncResult)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public IAsyncResult BeginExecuteXmlReader (AsyncCallback callback, object stateObject)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public IAsyncResult BeginExecuteXmlReader ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public XmlReader EndExecuteXmlReader (IAsyncResult asyncResult)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public event StatementCompletedEventHandler StatementCompleted;
	}
}
