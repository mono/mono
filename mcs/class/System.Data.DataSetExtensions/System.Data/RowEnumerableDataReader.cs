//
// RowEnumerableDataReader.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace System.Data
{
	internal class RowEnumerableDataReader : IDataReader
	{
		EnumerableRowCollection source;
		IEnumerator e;
		int depth;
		bool done;

		public RowEnumerableDataReader (IEnumerable source, int depth)
		{
			this.source = source as EnumerableRowCollection;
			if (this.source == null)
				this.source = new EnumerableRowCollection<DataRow> ((IEnumerable<DataRow>) source);
			this.depth = depth;
		}

		public DataRow Current {
			get { return e != null ? (DataRow) e.Current : null; }
		}

		public int Depth {
			get { return depth; }
		}

		public bool IsClosed {
			get { return done; }
		}

		public int RecordsAffected {
			get { return -1; }
		}

		public void Close ()
		{
			done = true;
		}

		public DataTable GetSchemaTable ()
		{
			return new DataTableReader (source.Table).GetSchemaTable ();
		}

		public bool NextResult ()
		{
			return e.MoveNext ();
		}

		public bool Read ()
		{
			if (e == null)
				e = ((IEnumerable) source).GetEnumerator ();
			return NextResult ();
		}

		// IDisposable
		public void Dispose ()
		{
			Close ();
		}

		// IDataRecord

		DataTable GetTable ()
		{
			DataRow r = Current;
			if (r == null)
				foreach (DataRow rr in source) {
					r = rr;
					break;
				}
			return r.Table;
		}

		public int FieldCount {
			get { return GetTable ().Columns.Count; }
		}

		public object this [int i] {
			get { return Current [i]; }
		}

		public object this [string name] {
			get { return Current [name]; }
		}

		public string GetDataTypeName (int i)
		{
			return GetFieldType (i).Name;
		}

		public Type GetFieldType (int i)
		{
			return GetTable ().Columns [i].DataType;
		}

		public string GetName (int i)
		{
			return GetTable ().Columns [i].ColumnName;
		}

		public int GetOrdinal (string name)
		{
			return GetTable ().Columns [name].Ordinal;
		}

		public long GetBytes (int i, long fieldOffset, byte [] buffer, int bufferoffset, int length)
		{
			// FIXME: do we need it?
			throw new NotSupportedException ();
		}

		public long GetChars (int i, long fieldOffset, char [] buffer, int bufferoffset, int length)
		{
			// FIXME: do we need it?
			throw new NotSupportedException ();
		}

		public IDataReader GetData (int i)
		{
			// FIXME: do we need it?
			throw new NotSupportedException ();
		}

		public int GetValues (object [] values)
		{
			int fieldCount = FieldCount;
			int i;

			//target object is byval so we can not just assign new object[] to values , calling side will not change
			//hence copy each item into values
			for (i = 0; i < values.Length && i < fieldCount; ++i)
				values[i] = Current[i];
			return i - 1;
		}
		
		public bool IsDBNull (int i)
		{
			return Current.IsNull (i);
		}

		public bool GetBoolean (int i)
		{
			return (bool) Current [i];
		}

		public byte GetByte (int i)
		{
			return (byte) Current [i];
		}

		public char GetChar (int i)
		{
			return (char) Current [i];
		}

		public DateTime GetDateTime (int i)
		{
			return (DateTime) Current [i];
		}

		public decimal GetDecimal (int i)
		{
			return (decimal) Current [i];
		}

		public double GetDouble (int i)
		{
			return (double) Current [i];
		}

		public float GetFloat (int i)
		{
			return (float) Current [i];
		}

		public Guid GetGuid (int i)
		{
			return (Guid) Current [i];
		}

		public short GetInt16 (int i)
		{
			return (short) Current [i];
		}

		public int GetInt32 (int i)
		{
			return (int) Current [i];
		}

		public long GetInt64 (int i)
		{
			return (long) Current [i];
		}

		public string GetString (int i)
		{
			return (string) Current [i];
		}

		public object GetValue (int i)
		{
			return Current [i];
		}
	}
}
