//
// System.Data.ProviderBase.DbDataReaderBase
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Collections;
using System.Data.Common;
using System.Runtime.InteropServices;

namespace System.Data.ProviderBase {
	public abstract class DbDataReaderBase : DbDataReader
	{
		#region Fields
		
		CommandBehavior behavior;
		
		#endregion // Fields

		#region Constructors

		protected DbDataReaderBase (CommandBehavior behavior)
		{
			this.behavior = behavior;
		}

		#endregion // Constructors

		#region Properties

		protected CommandBehavior CommandBehavior {
			get { return behavior; }
		}

		[MonoTODO]
		public override int Depth {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override int FieldCount {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override bool HasRows {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override bool IsClosed {
			get { throw new NotImplementedException (); }
		}

		protected abstract bool IsValidRow { get; }

		[MonoTODO]
		public override object this [[Optional] int index] {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override object this [[Optional] string columnName] {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override int RecordsAffected {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		protected void AssertReaderHasColumns ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void AssertReaderHasData ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void AssertReaderIsOpen (string methodName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Close ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected static DataTable CreateSchemaTable ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Dispose ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void FillSchemaTable (DataTable dataTable)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool GetBoolean (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override byte GetByte (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override long GetBytes (int ordinal, long fieldoffset, byte[] buffer, int bufferoffset, int length)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override char GetChar (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override long GetChars (int ordinal, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string GetDataTypeName (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override DateTime GetDateTime (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override decimal GetDecimal (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override double GetDouble (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IEnumerator GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override Type GetFieldType (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override float GetFloat (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override Guid GetGuid (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override short GetInt16 (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int GetInt32 (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override long GetInt64 (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string GetName (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int GetOrdinal (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override DataTable GetSchemaTable ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string GetString (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override object GetValue (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int GetValues (object[] values)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected bool IsCommandBehavior (CommandBehavior condition)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool IsDBNull (int ordinal)
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

#endif
