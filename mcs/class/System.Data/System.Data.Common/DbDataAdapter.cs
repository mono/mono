//
// System.Data.Common.DbDataAdapter.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc
//

namespace System.Data.Common
{
	/// <summary>
	/// Aids implementation of the IDbDataAdapter interface. Inheritors of DbDataAdapter  implement a set of functions to provide strong typing, but inherit most of the functionality needed to fully implement a DataAdapter.
	/// </summary>
	public abstract class DbDataAdapter : DataAdapter, ICloneable
	{
		public const string DefaultSourceTableName;

		[MonoTODO]
		protected DbDataAdapter() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int Fill(DataSet) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Fill(DataTable) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Fill(DataSet, string) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual int Fill(DataTable, IDataReader) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual int Fill(DataTable, IDbCommand, CommandBehavior) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Fill(DataSet, int, int, string) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual int Fill(DataSet, string, IDataReader, int, int) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual int Fill(DataSet, int, int, string, IDbCommand, CommandBehavior) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override DataTable[] FillSchema(DataSet, SchemaType) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataTable FillSchema(DataTable, SchemaType) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataTable[] FillSchema(DataSet, SchemaType, string) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual DataTable FillSchema(DataTable, SchemaType, IDbCommand, CommandBehavior) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual DataTable[] FillSchema(DataSet, SchemaType, IDbCommand, string, CommandBehavior) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IDataParameter[] GetFillParameters() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Update(DataRow[]) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int Update(DataSet) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Update(DataTable) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual int Update(DataRow[], DataTableMapping) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Update(DataSet, string) {
			throw new NotImplementedException ();
		}

		protected abstract RowUpdatedEventArgs CreateRowUpdatedEvent(
			DataRow dataRow,
			IDbCommand command,
			StatementType statementType,
			DataTableMapping tableMapping);

		protected abstract RowUpdatingEventArgs CreateRowUpdatingEvent(
			DataRow dataRow,
			IDbCommand command,
			StatementType statementType,
			DataTableMapping tableMapping);

		[MonoTODO]
		protected virtual void OnFillError(FillErrorEventArgs value) {
			throw new NotImplementedException ();
		}

		protected abstract void OnRowUpdated(RowUpdatedEventArgs value);

		protected abstract void OnRowUpdating(RowUpdatingEventArgs value);
		
		public event FillErrorEventHandler FillError;
	}
}
