//
// SqlDataRecord.cs
//
// Authors:
//   Marek Safar (marek.safar@gmail.com)
//
// Copyright (C) 2015 Novell, Inc (http://www.xamarin.com)
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
using System.Data;
using System.Data.SqlTypes;

namespace Microsoft.SqlServer.Server 
{
	public class SqlDataRecord : IDataRecord
	{
		public SqlDataRecord (params SqlMetaData[] metaData)
		{
		}

		public virtual bool GetBoolean (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public virtual byte GetByte (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public virtual long GetBytes (int ordinal, long fieldOffset, byte[] buffer, int bufferOffset, int length)
		{
			throw new NotImplementedException ();
		}

		public virtual char GetChar (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public virtual long GetChars (int ordinal, long fieldOffset, char[] buffer, int bufferOffset, int length)
		{
			throw new NotImplementedException ();
		}

		public virtual IDataReader GetData (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public virtual string GetDataTypeName (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public virtual DateTime GetDateTime (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public virtual DateTimeOffset GetDateTimeOffset (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public virtual decimal GetDecimal (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public virtual double GetDouble (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public virtual Type GetFieldType (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public virtual float GetFloat (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public virtual Guid GetGuid (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public virtual short GetInt16 (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public virtual int GetInt32 (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public virtual long GetInt64 (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public virtual string GetName (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public virtual int GetOrdinal (string name)
		{
			throw new NotImplementedException ();
		}

		public virtual SqlBinary GetSqlBinary (int ordinal)
 		{
 			throw new NotImplementedException ();
 		}

		public virtual SqlBoolean GetSqlBoolean (int ordinal)
 		{
 			throw new NotImplementedException ();
 		}

		public virtual SqlByte GetSqlByte (int ordinal)
 		{
 			throw new NotImplementedException ();
 		}

		public virtual SqlBytes GetSqlBytes (int ordinal)
 		{
 			throw new NotImplementedException ();
 		}

		public virtual SqlChars GetSqlChars (int ordinal)
 		{
 			throw new NotImplementedException ();
 		}

		public virtual SqlDateTime GetSqlDateTime (int ordinal)
 		{
 			throw new NotImplementedException ();
 		}

		public virtual SqlDecimal GetSqlDecimal (int ordinal)
 		{
 			throw new NotImplementedException ();
 		}

		public virtual SqlDouble GetSqlDouble (int ordinal)
 		{
 			throw new NotImplementedException ();
 		}

		public virtual Type GetSqlFieldType (int ordinal)
 		{
 			throw new NotImplementedException ();
 		}

		public virtual SqlGuid GetSqlGuid (int ordinal)
 		{
 			throw new NotImplementedException ();
 		}

		public virtual SqlInt16 GetSqlInt16 (int ordinal)
 		{
 			throw new NotImplementedException ();
 		}

		public virtual SqlInt32 GetSqlInt32 (int ordinal)
 		{
 			throw new NotImplementedException ();
 		}

		public virtual SqlInt64 GetSqlInt64 (int ordinal)
 		{
 			throw new NotImplementedException ();
 		}

		public virtual SqlMetaData GetSqlMetaData (int ordinal)
 		{
 			throw new NotImplementedException ();
 		}

		public virtual SqlMoney GetSqlMoney (int ordinal)
 		{
 			throw new NotImplementedException ();
 		}

		public virtual SqlSingle GetSqlSingle (int ordinal)
 		{
 			throw new NotImplementedException ();
 		}

		public virtual SqlString GetSqlString (int ordinal)
 		{
 			throw new NotImplementedException ();
 		}

		public virtual object GetSqlValue (int ordinal)
 		{
 			throw new NotImplementedException ();
 		}

		public virtual int GetSqlValues (object[] values)
 		{
 			throw new NotImplementedException ();
 		}

		public virtual SqlXml GetSqlXml (int ordinal)
 		{
 			throw new NotImplementedException ();
 		}

		public virtual string GetString (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public virtual TimeSpan GetTimeSpan (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public virtual object GetValue (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public virtual int GetValues (object[] values)
		{
			throw new NotImplementedException ();
		}

		public virtual void SetBoolean (int ordinal, bool value)
 		{ 
			throw new NotImplementedException ();
 		}

		public virtual void SetByte (int ordinal, byte value)
 		{ 
			throw new NotImplementedException ();
 		}

		public virtual void SetBytes (int ordinal, long fieldOffset, byte[] buffer, int bufferOffset, int length)
 		{ 
			throw new NotImplementedException ();
 		}

		public virtual void SetChar (int ordinal, char value)
 		{ 
			throw new NotImplementedException ();
 		}

		public virtual void SetChars (int ordinal, long fieldOffset, char[] buffer, int bufferOffset, int length)
 		{ 
			throw new NotImplementedException ();
 		}

		public virtual void SetDateTime (int ordinal, DateTime value)
 		{ 
			throw new NotImplementedException ();
 		}

		public virtual void SetDateTimeOffset (int ordinal, DateTimeOffset value)
 		{ 
			throw new NotImplementedException ();
 		}

		public virtual void SetDBNull (int ordinal)
 		{ 
			throw new NotImplementedException ();
 		}

		public virtual void SetDecimal (int ordinal, decimal value)
 		{ 
			throw new NotImplementedException ();
 		}

		public virtual void SetDouble (int ordinal, double value)
 		{ 
			throw new NotImplementedException ();
 		}

		public virtual void SetFloat (int ordinal, float value)
 		{ 
			throw new NotImplementedException ();
 		}

		public virtual void SetGuid (int ordinal, Guid value)
 		{ 
			throw new NotImplementedException ();
 		}

		public virtual void SetInt16 (int ordinal, short value)
 		{ 
			throw new NotImplementedException ();
 		}

		public virtual void SetInt32 (int ordinal, int value)
 		{ 
			throw new NotImplementedException ();
 		}

		public virtual void SetInt64 (int ordinal, long value)
 		{ 
			throw new NotImplementedException ();
 		}

		public virtual void SetSqlBinary (int ordinal, SqlBinary value)
 		{ 
			throw new NotImplementedException ();
 		}

		public virtual void SetSqlBoolean (int ordinal, SqlBoolean value)
 		{ 
			throw new NotImplementedException ();
 		}

		public virtual void SetSqlByte (int ordinal, SqlByte value)
 		{ 
			throw new NotImplementedException ();
 		}

		public virtual void SetSqlBytes (int ordinal, SqlBytes value)
 		{ 
			throw new NotImplementedException ();
 		}

		public virtual void SetSqlChars (int ordinal, SqlChars value)
 		{ 
			throw new NotImplementedException ();
 		}

		public virtual void SetSqlDateTime (int ordinal, SqlDateTime value)
 		{ 
			throw new NotImplementedException ();
 		}

		public virtual void SetSqlDecimal (int ordinal, SqlDecimal value)
 		{ 
			throw new NotImplementedException ();
 		}

		public virtual void SetSqlDouble (int ordinal, SqlDouble value)
 		{ 
			throw new NotImplementedException ();
 		}

		public virtual void SetSqlGuid (int ordinal, SqlGuid value)
 		{ 
			throw new NotImplementedException ();
 		}

		public virtual void SetSqlInt16 (int ordinal, SqlInt16 value)
 		{ 
			throw new NotImplementedException ();
 		}

		public virtual void SetSqlInt32 (int ordinal, SqlInt32 value)
 		{ 
			throw new NotImplementedException ();
 		}

		public virtual void SetSqlInt64 (int ordinal, SqlInt64 value)
 		{ 
			throw new NotImplementedException ();
 		}

		public virtual void SetSqlMoney (int ordinal, SqlMoney value)
 		{ 
			throw new NotImplementedException ();
 		}

		public virtual void SetSqlSingle (int ordinal, SqlSingle value)
 		{ 
			throw new NotImplementedException ();
 		}

		public virtual void SetSqlString (int ordinal, SqlString value)
 		{ 
			throw new NotImplementedException ();
 		}

		public virtual void SetSqlXml (int ordinal, SqlXml value)
 		{ 
			throw new NotImplementedException ();
 		}

		public virtual void SetString (int ordinal, string value)
 		{ 
			throw new NotImplementedException ();
 		}

		public virtual void SetTimeSpan (int ordinal, TimeSpan value)
 		{ 
			throw new NotImplementedException ();
 		}

		public virtual void SetValue (int ordinal, object value)
 		{ 
			throw new NotImplementedException ();
 		}

		public virtual int SetValues (params object[] values)
 		{ 
			throw new NotImplementedException ();
 		}

		public virtual bool IsDBNull (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public virtual int FieldCount {
			get {
				throw new NotImplementedException ();
			}
		}

		public virtual object this [string name] {
			get {
				throw new NotImplementedException ();
			}
		}

		public virtual object this [int ordinal] {
			get {
				throw new NotImplementedException ();
			}
		}
	}
}