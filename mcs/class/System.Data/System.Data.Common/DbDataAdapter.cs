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

using System.Collections;
using System.Data;

namespace System.Data.Common
{
	/// <summary>
	/// Aids implementation of the IDbDataAdapter interface. Inheritors of DbDataAdapter  implement a set of functions to provide strong typing, but inherit most of the functionality needed to fully implement a DataAdapter.
	/// </summary>
	public abstract class DbDataAdapter : DataAdapter, ICloneable
	{
		#region Fields

		public const string DefaultSourceTableName = "Table";

		#endregion
		
		#region Constructors

		protected DbDataAdapter() 
		{
		}

		#endregion

		#region Properties

		IDbCommand DeleteCommand {
			get { return ((IDbDataAdapter)this).DeleteCommand; }
		}

		IDbCommand InsertCommand {
			get { return ((IDbDataAdapter)this).InsertCommand; }
		}

		IDbCommand SelectCommand {
			get { return ((IDbDataAdapter)this).SelectCommand; }
		}


		IDbCommand UpdateCommand {
			get { return ((IDbDataAdapter)this).UpdateCommand; }
		}

		#endregion

		#region Methods

		protected abstract RowUpdatedEventArgs CreateRowUpdatedEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping);
		protected abstract RowUpdatingEventArgs CreateRowUpdatingEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping);

		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}

                public override int Fill (DataSet dataSet)
                {
			return Fill (dataSet, DefaultSourceTableName);
                }

		public int Fill (DataTable dataTable) 
		{
			return Fill (dataTable.DataSet, dataTable.TableName);
		}

		public int Fill (DataSet dataSet, string srcTable) 
		{
			return Fill (dataSet, 0, 0, srcTable);
		}

		protected virtual int Fill (DataTable dataTable, IDataReader dataReader) 
		{
			return Fill (dataTable.DataSet, dataTable.TableName, dataReader, 0, 0);
		}

		protected virtual int Fill (DataTable dataTable, IDbCommand command, CommandBehavior behavior) 
		{
			return Fill (dataTable.DataSet, 0, 0, dataTable.TableName, command, behavior);
		}

		public int Fill (DataSet dataSet, int startRecord, int maxRecords, string srcTable) 
		{
			return this.Fill (dataSet, startRecord, maxRecords, srcTable, SelectCommand, CommandBehavior.Default);
		}

		protected virtual int Fill (DataSet dataSet, string srcTable, IDataReader dataReader, int startRecord, int maxRecords) 
		{
			if (startRecord < 0)
				throw new ArgumentException ("The startRecord parameter was less than 0.");
			if (maxRecords < 0)
				throw new ArgumentException ("The maxRecords parameter was less than 0.");

                        DataTable table;
                        int readCount = 0;
                        int resultCount = 0;

			string tableName = srcTable;
			string baseColumnName;
			string baseTableName;
			string columnName;
			ArrayList primaryKey;	
			bool resultsFound;
			object[] itemArray;
			DataTableMapping tableMapping;

			DataRow row; // FIXME needed for incorrect operation below.

                        do {
				if (dataSet.Tables.Contains (tableName))
					table = dataSet.Tables[tableName];
				else
					table = new DataTable (tableName);

				primaryKey = new ArrayList ();	

				foreach (DataRow schemaRow in dataReader.GetSchemaTable ().Rows)
				{
					// generate a unique column name in the dataset table.
					if (schemaRow["BaseColumnName"].Equals (DBNull.Value))
						baseColumnName = "Column";
					else
						baseColumnName = (string) schemaRow ["BaseColumnName"];

					columnName = baseColumnName;

					for (int i = 1; table.Columns.Contains (columnName); i += 1) 
						columnName = String.Format ("{0}{1}", baseColumnName, i);

					if (schemaRow["BaseTableName"].Equals (DBNull.Value))
						baseTableName = "Table";
					else
						baseTableName = (string) schemaRow ["BaseTableName"];

					tableMapping = DataTableMappingCollection.GetTableMappingBySchemaAction (TableMappings, tableName, baseTableName, MissingMappingAction);

					// check to see if the column mapping exists
					if (tableMapping.ColumnMappings.IndexOfDataSetColumn (baseColumnName) < 0)
					{
						if (MissingSchemaAction == MissingSchemaAction.Error)
							throw new SystemException ();

						table.Columns.Add (columnName, (Type) schemaRow ["DataType"]);
						tableMapping.ColumnMappings.Add (columnName, baseColumnName);

					}
				
					if (!TableMappings.Contains (tableMapping))
						TableMappings.Add (tableMapping);

					if (!schemaRow["IsKey"].Equals (DBNull.Value))
						if ((bool) (schemaRow["IsKey"]))
							primaryKey.Add (table.Columns[columnName]);	
				}

				if (MissingSchemaAction == MissingSchemaAction.AddWithKey && primaryKey.Count > 0)
					table.PrimaryKey = (DataColumn[])(primaryKey.ToArray());


				for (int k = 0; k < startRecord; k += 1)
					dataReader.Read ();

				resultsFound = false;

                                itemArray = new object[dataReader.FieldCount];

                                while (dataReader.Read () && !(maxRecords > 0 && readCount >= maxRecords))
                                {
                                        dataReader.GetValues (itemArray);
					row = table.Rows.Add (itemArray);
					if (AcceptChangesDuringFill)
						row.AcceptChanges ();

					/* FIXME

					this is the way it should be done, but LoadDataRow has not been implemented yet.

					table.BeginLoadData ();
					table.LoadDataRow (itemArray, AcceptChangesDuringFill);
					table.EndLoadData ();
					*/

                                        readCount += 1;
					resultsFound = true;
                                }

				if (resultsFound)
				{
					dataSet.Tables.Add (table);
                                	tableName = String.Format ("{0}{1}", srcTable, ++resultCount);
				}


				startRecord = 0;
				maxRecords = 0;
                        } while (dataReader.NextResult ());

                        dataReader.Close ();
                        return readCount;
		}

		protected virtual int Fill (DataSet dataSet, int startRecord, int maxRecords, string srcTable, IDbCommand command, CommandBehavior behavior) 
		{
			if (command.Connection.State == ConnectionState.Closed)
			{
				command.Connection.Open ();
				behavior |= CommandBehavior.CloseConnection;
			}
		
			return this.Fill (dataSet, srcTable, command.ExecuteReader (behavior), startRecord, maxRecords);
		}

		[MonoTODO]
		public override DataTable[] FillSchema (DataSet dataSet, SchemaType schemaType) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataTable FillSchema (DataTable dataTable, SchemaType schemaType) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataTable[] FillSchema (DataSet dataSet, SchemaType schemaType, string srcTable) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual DataTable FillSchema (DataTable dataTable, SchemaType schemaType, IDbCommand command, CommandBehavior behavior) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual DataTable[] FillSchema (DataSet dataSet, SchemaType schemaType, IDbCommand command, string srcTable, CommandBehavior behavior) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IDataParameter[] GetFillParameters () 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		object ICloneable.Clone ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Update (DataRow[] dataRows) 
		{
			throw new NotImplementedException ();
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



		[MonoTODO]
		protected virtual void OnFillError (FillErrorEventArgs value) 
		{
			throw new NotImplementedException ();
		}

		protected abstract void OnRowUpdated (RowUpdatedEventArgs value);
		protected abstract void OnRowUpdating (RowUpdatingEventArgs value);
		
		#endregion
		
		#region Events

		public event FillErrorEventHandler FillError;

		#endregion
	}
}
