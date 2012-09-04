//
// System.Data.Common.DbDataReader.cs
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

using System.Collections;
using System.ComponentModel;
using System.Data;
using System.IO;

#if NET_4_5
using System.Threading;
using System.Threading.Tasks;
#endif

namespace System.Data.Common {
	public abstract class DbDataReader : MarshalByRefObject, IDataReader, IDataRecord, IDisposable, IEnumerable
	{
		#region Constructors

		protected DbDataReader ()
		{
		}

		#endregion // Constructors

		#region Properties

		public abstract int Depth { get; }
		public abstract int FieldCount { get; }
		public abstract bool HasRows { get; }
		public abstract bool IsClosed { get; }
		public abstract object this [int ordinal] { get; }
		public abstract object this [string name] { get; }
		public abstract int RecordsAffected { get; }

#if NET_2_0
		public virtual int VisibleFieldCount {
			get { return FieldCount; }
		}
#endif
		#endregion // Properties

		#region Methods

		public abstract void Close ();
		public abstract bool GetBoolean (int ordinal);
		public abstract byte GetByte (int ordinal);
		public abstract long GetBytes (int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length);
		public abstract char GetChar (int ordinal);
		public abstract long GetChars (int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length);

		[EditorBrowsable (EditorBrowsableState.Never)]
		public void Dispose ()
		{
			Dispose (true);	
		}
		
		protected virtual void Dispose (bool disposing)
		{
			if (disposing)
				Close ();
		}
#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Never)]
		public DbDataReader GetData (int ordinal)
		{
			return ((DbDataReader) this [ordinal]);
		}
#endif

		public abstract string GetDataTypeName (int ordinal);
		public abstract DateTime GetDateTime (int ordinal);
		public abstract decimal GetDecimal (int ordinal);
		public abstract double GetDouble (int ordinal);

		[EditorBrowsable (EditorBrowsableState.Never)]
		public abstract IEnumerator GetEnumerator ();

		public abstract Type GetFieldType (int ordinal);
		public abstract float GetFloat (int ordinal);
		public abstract Guid GetGuid (int ordinal);
		public abstract short GetInt16 (int ordinal);
		public abstract int GetInt32 (int ordinal);
		public abstract long GetInt64 (int ordinal);
		public abstract string GetName (int ordinal);
		public abstract int GetOrdinal (string name);

#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Never)]
		public virtual Type GetProviderSpecificFieldType (int ordinal)
		{
			return GetFieldType (ordinal);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public virtual object GetProviderSpecificValue (int ordinal)
		{
			return GetValue (ordinal);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public virtual int GetProviderSpecificValues (object[] values)
		{
			return GetValues (values);
		}
	
		protected virtual DbDataReader GetDbDataReader (int ordinal)
		{
			return ((DbDataReader) this [ordinal]);
		}
#endif 

		public abstract DataTable GetSchemaTable ();
		public abstract string GetString (int ordinal);
		public abstract object GetValue (int ordinal);
		public abstract int GetValues (object[] values);

		IDataReader IDataRecord.GetData (int ordinal)
		{
			return ((IDataReader) this).GetData (ordinal);
		}

		public abstract bool IsDBNull (int ordinal);
		public abstract bool NextResult ();
		public abstract bool Read ();

                internal static DataTable GetSchemaTableTemplate ()
		{
			Type booleanType = typeof (bool);
			Type stringType = typeof (string);
			Type intType = typeof (int);
			Type typeType = typeof (Type);
			Type shortType = typeof (short);

			DataTable schemaTable = new DataTable ("SchemaTable");
			schemaTable.Columns.Add ("ColumnName",       stringType);
			schemaTable.Columns.Add ("ColumnOrdinal",    intType);
			schemaTable.Columns.Add ("ColumnSize",       intType);
			schemaTable.Columns.Add ("NumericPrecision", shortType);
			schemaTable.Columns.Add ("NumericScale",     shortType);
			schemaTable.Columns.Add ("IsUnique",         booleanType);
			schemaTable.Columns.Add ("IsKey",            booleanType);
			schemaTable.Columns.Add ("BaseServerName",   stringType);
			schemaTable.Columns.Add ("BaseCatalogName",  stringType);
			schemaTable.Columns.Add ("BaseColumnName",   stringType);
			schemaTable.Columns.Add ("BaseSchemaName",   stringType);
			schemaTable.Columns.Add ("BaseTableName",    stringType);
			schemaTable.Columns.Add ("DataType",         typeType);
			schemaTable.Columns.Add ("AllowDBNull",      booleanType);
			schemaTable.Columns.Add ("ProviderType",     intType);
			schemaTable.Columns.Add ("IsAliased",        booleanType);
			schemaTable.Columns.Add ("IsExpression",     booleanType);
			schemaTable.Columns.Add ("IsIdentity",       booleanType);
			schemaTable.Columns.Add ("IsAutoIncrement",  booleanType);
			schemaTable.Columns.Add ("IsRowVersion",     booleanType);
			schemaTable.Columns.Add ("IsHidden",         booleanType);
			schemaTable.Columns.Add ("IsLong",           booleanType);
			schemaTable.Columns.Add ("IsReadOnly",       booleanType);

			return schemaTable;
		}
		
#if NET_4_5
		[MonoTODO]
		public virtual T GetFieldValue<T> (int i)
		{
			throw new NotImplementedException ();
		}

		public Task<T> GetFieldValueAsync<T> (int ordinal)
		{
			return GetFieldValueAsync<T> (ordinal, CancellationToken.None);
		}
		
		[MonoTODO]
		public virtual Task<T> GetFieldValueAsync<T> (int ordinal, CancellationToken cancellationToken)
		{
			throw new NotImplementedException ();
		}
		
		public Task<bool> NextResultAsync ()
		{
			return NextResultAsync (CancellationToken.None);
		}
		
		public Task<bool> IsDBNullAsync (int ordinal)
		{
			return IsDBNullAsync (ordinal, CancellationToken.None);
		}

		[MonoTODO]
		public virtual Stream GetStream (int i)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public virtual TextReader GetTextReader (int i)
		{
			throw new NotImplementedException ();	
		}

		[MonoTODO]
		public virtual Task<bool> IsDBNullAsync (int ordinal, CancellationToken cancellationToken)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public virtual Task<bool> NextResultAsync (CancellationToken cancellationToken)
		{
			throw new NotImplementedException ();
		}
		
		public Task<bool> ReadAsync ()
		{
			return ReadAsync (CancellationToken.None);
		}
		
		[MonoTODO]
		public virtual Task<bool> ReadAsync (CancellationToken cancellationToken)
		{
			throw new NotImplementedException ();
		}
#endif

		#endregion // Methods
	}
}

