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
		public const string DefaultSourceTableName = "default";

		[MonoTODO]
		protected DbDataAdapter() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int Fill (DataSet ds) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Fill (DataTable dt) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Fill (DataSet ds, string s) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual int Fill (DataTable dt, IDataReader idr) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual int Fill (DataTable dt,
					    IDbCommand idc,
					    CommandBehavior behavior) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Fill (DataSet ds, int i, int j, string s) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual int Fill (DataSet ds,
					    string s,
					    IDataReader idr,
					    int i,
					    int j) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual int Fill (DataSet ds,
					    int i,
					    int j,
					    string s,
					    IDbCommand idc,
					    CommandBehavior behavior) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override DataTable[] FillSchema (DataSet ds, SchemaType type) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataTable FillSchema (DataTable dt, SchemaType type) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataTable[] FillSchema (DataSet ds, SchemaType type, string s) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual DataTable FillSchema (DataTable dt,
							SchemaType type,
							IDbCommand idc,
							CommandBehavior behavior) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual DataTable[] FillSchema (DataSet ds,
							  SchemaType type,
							  IDbCommand idc,
							  string s,
							  CommandBehavior behavior) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IDataParameter[] GetFillParameters() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Update (DataRow[] row) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int Update (DataSet ds) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Update (DataTable dt) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual int Update (DataRow[] row, DataTableMapping dtm) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Update (DataSet ds, string s) {
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
