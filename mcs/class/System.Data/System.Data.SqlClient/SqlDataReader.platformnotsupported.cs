//
// SqlDataReader.cs
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

using System;
using System.Collections;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Xml;


namespace System.Data.SqlClient
{
	public class SqlDataReader : DbDataReader , IDataReader, IDisposable, IDataRecord
	{
		const string EXCEPTION_MESSAGE = "System.Data.SqlClient.SqlDataReader is not supported on the current platform.";

		SqlDataReader () {}

		protected bool IsCommandBehavior (CommandBehavior condition)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override void Close ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override bool GetBoolean (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override byte GetByte (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override long GetBytes (int i, long dataIndex, byte [] buffer, int bufferIndex, int length)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override char GetChar (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override long GetChars (int i, long dataIndex, char [] buffer, int bufferIndex, int length)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override string GetDataTypeName (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override DateTime GetDateTime (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual DateTimeOffset GetDateTimeOffset (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual TimeSpan GetTimeSpan (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual SqlChars GetSqlChars (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override Decimal GetDecimal (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override Double GetDouble (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override Type GetFieldType (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override Single GetFloat (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override Guid GetGuid (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override short GetInt16 (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override int GetInt32 (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override long GetInt64 (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override string GetName (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override int GetOrdinal (string name)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override DataTable GetSchemaTable ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual SqlBinary GetSqlBinary (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual SqlBoolean GetSqlBoolean (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual SqlByte GetSqlByte (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual SqlDateTime GetSqlDateTime (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual SqlDecimal GetSqlDecimal (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual SqlDouble GetSqlDouble (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual SqlGuid GetSqlGuid (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual SqlInt16 GetSqlInt16 (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual SqlInt32 GetSqlInt32 (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual SqlInt64 GetSqlInt64 (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual SqlMoney GetSqlMoney (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual SqlSingle GetSqlSingle (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual SqlString GetSqlString (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual SqlXml GetSqlXml (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual object GetSqlValue (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual int GetSqlValues (object [] values)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override string GetString (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override object GetValue (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override int GetValues (object [] values)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override IEnumerator GetEnumerator ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override bool IsDBNull (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override bool NextResult ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override bool Read ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override Type GetProviderSpecificFieldType (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override object GetProviderSpecificValue (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override int GetProviderSpecificValues (object [] values)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual SqlBytes GetSqlBytes (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override T GetFieldValue<T> (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public virtual XmlReader GetXmlReader (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override Task<T> GetFieldValueAsync<T> (int i, CancellationToken cancellationToken)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override Stream GetStream (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override TextReader GetTextReader (int i)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override Task<bool> IsDBNullAsync (int i, CancellationToken cancellationToken)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override int Depth {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override int FieldCount {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override bool IsClosed {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override object this [int i] {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override object this [string name] {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override int RecordsAffected {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override bool HasRows {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override int VisibleFieldCount {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		protected SqlConnection Connection {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}
	}
}
