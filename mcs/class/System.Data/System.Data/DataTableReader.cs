//
// System.Data.DataTableReader.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.Data.Common;

namespace System.Data {
	public sealed class DataTableReader : DbDataReader
	{
		bool closed;
		DataTable[] dataTables;
		int index;

		#region Constructors

		[MonoTODO]
		public DataTableReader (DataTable dt)
			: this (new DataTable[] {dt})
		{
		}

		[MonoTODO]
		public DataTableReader (DataTable[] dataTables)
		{
			this.dataTables = dataTables;
			closed = false;
			index = 0;
		}

		#endregion // Constructors

		#region Properties

		public override int Depth {
			get { return 0; }
		}

		public override int FieldCount {
			get { return dataTables [index].Columns.Count; }
		}

		public override bool HasRows {
			get { return dataTables [index].Rows.Count > 0; }
		}

		public override bool IsClosed {
			get { return closed; }
		}

		[MonoTODO]
		public override object this [int index] {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override object this [string name] {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override int RecordsAffected {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override int VisibleFieldCount {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override void Close ()
		{
			closed = true;
		}

		[MonoTODO]
		public override void Dispose ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool GetBoolean (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override byte GetByte (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override long GetBytes (int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override char GetChar (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override long GetChars (int i, long dataIndex, char[] buffer, int bufferIndex, int length)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string GetDataTypeName (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override DateTime GetDateTime (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override decimal GetDecimal (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override double GetDouble (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IEnumerator GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override Type GetFieldProviderSpecificType (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override Type GetFieldType (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override float GetFloat (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override Guid GetGuid (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override short GetInt16 (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int GetInt32 (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override long GetInt64 (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string GetName (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int GetOrdinal (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override object GetProviderSpecificValue (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int GetProviderSpecificValues (object[] values)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override DataTable GetSchemaTable ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string GetString (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override object GetValue (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int GetValues (object[] values)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool IsDBNull (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool NextResult ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool Read ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}

#endif // NET_2_0
