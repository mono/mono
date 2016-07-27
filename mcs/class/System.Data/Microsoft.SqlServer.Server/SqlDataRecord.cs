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

		public virtual decimal GetDecimal (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public virtual double GetDouble (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public virtual System.Type GetFieldType (int ordinal)
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

		public virtual string GetString (int ordinal)
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