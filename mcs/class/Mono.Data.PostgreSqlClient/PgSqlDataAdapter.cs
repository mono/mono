//
// System.Data.SqlClient.SqlDataAdapter.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc 2002
//

using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace System.Data.SqlClient
{
	/// <summary>
	/// Represents a set of command-related properties that are used 
	/// to fill the DataSet and update a data source, all this 
	/// from a SQL database.
	/// </summary>
	public sealed class SqlDataAdapter : DbDataAdapter, IDbDataAdapter
	{
		#region Properties

		[MonoTODO]
		public SqlCommand DeleteCommand	{
			get { 
				throw new NotImplementedException (); 
			}

			set { 
				throw new NotImplementedException (); 
			}
		}

		[MonoTODO]
		public SqlCommand InsertCommand	{
			get { 
				throw new NotImplementedException (); 
			}

			set { 
				throw new NotImplementedException (); 
			}
		}

		[MonoTODO]
		public SqlCommand SelectCommand	{
			get { 
				throw new NotImplementedException (); 
			}

			set { 
				throw new NotImplementedException (); 
			}
		}

		public SqlCommand UpdateCommand	{
			get { 
				throw new NotImplementedException (); 
			}

			set { 
				throw new NotImplementedException (); 
			}
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override int Fill(DataSet dataSet) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Fill(DataTable dataTable) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Fill(DataSet dataSet, string srcTable) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual int Fill(DataTable dataTable,	
					IDataReader dataReader) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual int Fill(DataTable dataTable,
					IDbCommand command, 
					CommandBehavior behavior) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Fill(DataSet dataSet, int startRecord,
				int maxRecords,	string srcTable) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual int Fill(DataSet dataSet,
			string srcTable, IDataReader dataReader,
			int startRecord, int maxRecords) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual int Fill(DataSet dataSet,
			int startRecord, int maxRecords,
			string srcTable, IDbCommand command,
			CommandBehavior behavior) {

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override DataTable[] FillSchema(DataSet dataSet,
				SchemaType schemaType) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataTable FillSchema(DataTable dataTable,
			SchemaType schemaType) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataTable[] FillSchema(DataSet dataSet,
			SchemaType schemaType, string srcTable) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual DataTable FillSchema(DataTable dataTable,
			SchemaType schemaType, IDbCommand command,
			CommandBehavior behavior) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual DataTable[] FillSchema(DataSet dataSet,
			SchemaType schemaType, IDbCommand command,
			string srcTable, CommandBehavior behavior) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IDataParameter[] GetFillParameters() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Update(DataRow[] dataRows) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int Update(DataSet dataSet) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Update(DataTable dataTable) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual int Update(DataRow[] dataRows,
					     DataTableMapping tableMapping) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Update(DataSet dataSet, string srcTable) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override RowUpdatedEventArgs CreateRowUpdatedEvent(
			DataRow dataRow,
			IDbCommand command,
			StatementType statementType,
			DataTableMapping tableMapping) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override RowUpdatingEventArgs CreateRowUpdatingEvent(
			DataRow dataRow,
			IDbCommand command,
			StatementType statementType,
			DataTableMapping tableMapping) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnFillError(FillErrorEventArgs value) {
			throw new NotImplementedException ();
		}

		protected override void OnRowUpdated(RowUpdatedEventArgs value) {
			throw new NotImplementedException ();
		}

		protected override void OnRowUpdating(RowUpdatingEventArgs value) {
			throw new NotImplementedException ();
		}

		#endregion // Methods

		#region Events and Delegates

		public event FillErrorEventHandler FillError;
		
		public event SqlRowUpdatedEventHandler RowUpdated;

		public event SqlRowUpdatingEventHandler RowUpdating;

		#endregion // Events and Delegates

	}
}
