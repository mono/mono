//
// System.Data.Common.DbDataAdapter.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc
// Copyright (C) 2002 Tim Coleman
//

using System.Data.SqlClient;

namespace System.Data.Common
{
	/// <summary>
	/// Aids implementation of the IDbDataAdapter interface. Inheritors of DbDataAdapter  implement a set of functions to provide strong typing, but inherit most of the functionality needed to fully implement a DataAdapter.
	/// </summary>
	public abstract class DbDataAdapter : DataAdapter, ICloneable
	{
		#region Fields

		public const string DefaultSourceTableName = "default";

		IDbCommand selectCommand;
		IDbCommand insertCommand;
		IDbCommand deleteCommand;
		IDbCommand updateCommand;

		bool isDirty;

		#endregion

		#region Constructors

		protected DbDataAdapter() 
		{
			isDirty = true;
		}

		#endregion

		#region Properties

		public IDbCommand SelectCommand {
			get { return selectCommand; }
			set { 
				isDirty = true;
				selectCommand = value; 
			}
		}

		public IDbCommand InsertCommand {
			get { return insertCommand; }
			set { insertCommand = value; }
		}

		public IDbCommand DeleteCommand {
			get { return deleteCommand; }
			set { deleteCommand = value; }
		}

		public IDbCommand UpdateCommand {
			get { return updateCommand; }
			set { updateCommand = value; }
		}

		#endregion

		#region Methods

                public override int Fill (DataSet dataSet)
                {
			return this.Fill (dataSet, "Table", selectCommand.ExecuteReader (), 0, 0);
                }

		[MonoTODO]
		public int Fill (DataTable dt) 
		{
			throw new NotImplementedException ();
		}

		public int Fill (DataSet dataSet, string srcTable) 
		{
			return this.Fill (dataSet, srcTable, selectCommand.ExecuteReader (), 0, 0);
		}

		[MonoTODO]
		protected virtual int Fill (DataTable dt, IDataReader idr) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual int Fill (DataTable dt, IDbCommand idc, CommandBehavior behavior) 
		{
			throw new NotImplementedException ();
		}

		public int Fill (DataSet dataSet, int startRecord, int maxRecords, string srcTable) 
		{
			if (startRecord < 0)
				throw new ArgumentException ("The startRecord parameter was less than 0.");
			if (maxRecords < 0)
				throw new ArgumentException ("The maxRecords parameter was less than 0.");
			return this.Fill (dataSet, srcTable, selectCommand.ExecuteReader (), startRecord, maxRecords);
		}

		protected virtual int Fill (DataSet dataSet, string srcTable, IDataReader dataReader, int startRecord, int maxRecords) 
		{
                        DataTable table;
                        int changeCount = 0;
			string tableName = srcTable;
                        int i = 0;

                        if (this.isDirty)
                                dataSet.Tables.Clear ();

                        do
                        {
                                if (!this.isDirty)  // table already exists
                                {
                                        table = dataSet.Tables[tableName];
                                }
                                else // create a new table
                                {
                                        table = new DataTable (tableName);
                                        for (int j = 0; j < dataReader.FieldCount; j += 1)
                                        {
                                                string baseColumnName = dataReader.GetName (j);
                                                string columnName = "";

                                                if (baseColumnName == "")
                                                        baseColumnName = "Column";
                                                else
                                                        columnName = baseColumnName;

                                                for (int k = 1; table.Columns.Contains (columnName) || columnName == ""; k += 1)
                                                        columnName = String.Format ("{0}{1}", baseColumnName, k);

                                                table.Columns.Add (new DataColumn (columnName, dataReader.GetFieldType (j)));
                                        }
                                        dataSet.Tables.Add (table);
                                }

                                DataRow thisRow;
                                object[] itemArray = new object[dataReader.FieldCount];

                                while (dataReader.Read () && (changeCount != 0 || changeCount < maxRecords))
                                {
                                        // need to check for existing rows to reconcile if we have key
                                        // information.  skip this step for now

                                        // append rows to the end of the current table.
                                        dataReader.GetValues (itemArray);
                                        thisRow = table.NewRow ();
                                        thisRow.ItemArray = itemArray;
                                        table.ImportRow (thisRow);
					
					if (AcceptChangesDuringFill) thisRow.AcceptChanges ();

                                        changeCount += 1;
                                }

                                i += 1;
                                tableName = String.Format ("{0}{1}", srcTable, i);
                        } while (dataReader.NextResult ());

                        dataReader.Close ();
                        this.isDirty = false;
                        return changeCount;
		}

		[MonoTODO]
		protected virtual int Fill (DataSet dataSet, int startRecord, int maxRecords, string srcTable, IDbCommand idc, CommandBehavior behavior) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override DataTable[] FillSchema (DataSet ds, SchemaType type) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataTable FillSchema (DataTable dt, SchemaType type) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataTable[] FillSchema (DataSet ds, SchemaType type, string s) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual DataTable FillSchema (DataTable dt, SchemaType type, IDbCommand idc, CommandBehavior behavior) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual DataTable[] FillSchema (DataSet ds, SchemaType type, IDbCommand idc, string s, CommandBehavior behavior) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IDataParameter[] GetFillParameters () 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Update (DataRow[] dataRows) 
		{
			throw new NotImplementedException ();
			foreach (DataRow dataRow in dataRows) 
			{
				switch (dataRow.RowState)
				{
				case DataRowState.Added:
					dataRow.AcceptChanges ();
				case DataRowState.Deleted:
					dataRow.AcceptChanges ();
				case DataRowState.Modified:
					dataRow.AcceptChanges ();
				}

			}
		}

		[MonoTODO]
		public override int Update (DataSet ds) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Update (DataTable dt) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual int Update (DataRow[] row, DataTableMapping dtm) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Update (DataSet ds, string s) 
		{
			throw new NotImplementedException ();
		}

		protected abstract RowUpdatedEventArgs CreateRowUpdatedEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping);

		protected abstract RowUpdatingEventArgs CreateRowUpdatingEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping);

		[MonoTODO]
		protected virtual void OnFillError (FillErrorEventArgs value) 
		{
			throw new NotImplementedException ();
		}

		protected abstract void OnRowUpdated (RowUpdatedEventArgs value);
		protected abstract void OnRowUpdating (RowUpdatingEventArgs value);
		
		public event FillErrorEventHandler FillError;

		[MonoTODO]
		public object Clone ()
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}
