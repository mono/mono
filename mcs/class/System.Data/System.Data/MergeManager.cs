using System;

namespace System.Data
{
	/// <summary>
	/// Summary description for MergeManager.
	/// </summary>
	internal class MergeManager
	{
		internal static void Merge(DataSet targetSet, DataSet sourceSet, bool preserveChanges, MissingSchemaAction missingSchemaAction)
		{
			if(targetSet == null)
				throw new ArgumentNullException("targetSet");
			if(sourceSet == null)
				throw new ArgumentNullException("sourceSet");

			foreach (DataTable t in sourceSet.Tables)
				MergeManager.Merge(targetSet, t, preserveChanges, missingSchemaAction);

		}

		internal static void Merge(DataSet targetSet, DataTable sourceTable, bool preserveChanges, MissingSchemaAction missingSchemaAction)
		{
			if(targetSet == null)
				throw new ArgumentNullException("targetSet");
			if(sourceTable == null)
				throw new ArgumentNullException("sourceTable");

			
			if (!AdjustSchema(targetSet, sourceTable, missingSchemaAction))
				return;
			checkColumnTypes(targetSet.Tables[sourceTable.TableName], sourceTable); // check that the colums datatype is the same
			fillData(targetSet.Tables[sourceTable.TableName], sourceTable, preserveChanges);
			
		}

		internal static void Merge(DataSet targetSet, DataRow[] sourceRows, bool preserveChanges, MissingSchemaAction missingSchemaAction)
		{
			if(targetSet == null)
				throw new ArgumentNullException("targetSet");
			if(sourceRows == null)
				throw new ArgumentNullException("sourceRows");

			for (int i = 0; i < sourceRows.Length; i++)
			{
				DataRow row = sourceRows[i];
				DataTable sourceTable = row.Table;
				if (!AdjustSchema(targetSet, sourceTable, missingSchemaAction))
					return;
				checkColumnTypes(targetSet.Tables[row.Table.TableName], row.Table);
				MergeRow(targetSet.Tables[sourceTable.TableName], row, preserveChanges);
			}
		}

		// merge a row into a target table.
		private static void MergeRow(DataTable targetTable, DataRow row, bool preserveChanges)
		{
			DataColumnCollection columns = row.Table.Columns;
			DataColumn[] primaryKeys = targetTable.PrimaryKey;
			DataRow targetRow = null;
			DataRowVersion version = DataRowVersion.Default;
			if (row.RowState == DataRowState.Deleted)
				version = DataRowVersion.Original;

			if (primaryKeys != null && primaryKeys.Length > 0) // if there are any primary key.
			{
				// initiate an array that has the values of the primary keys.
				object[] keyValues = new object[primaryKeys.Length];
				
				for (int j = 0; j < keyValues.Length; j++)
				{
					keyValues[j] = row[primaryKeys[j].ColumnName, version];
				}
			
				// find the row in the target table.
				targetRow = targetTable.Rows.Find(keyValues);
			}
			// row doesn't exist in target table, or there are no primary keys.
			// create new row and copy values from source row to the new row.
			if (targetRow == null)
			{ 
				targetRow = targetTable.NewRow();
				row.CopyValuesToRow(targetRow);
				targetTable.Rows.Add(targetRow);
			}
			// row exists in target table, and presere changes is false - 
			// change the values of the target row to the values of the source row.
			else if (!preserveChanges)
			{
				row.CopyValuesToRow(targetRow);
			}

		}
			
		
		
		// adjust the table schema according to the missingschemaaction param.
		// return false if adjusting fails.
		private static bool AdjustSchema(DataSet targetSet, DataTable sourceTable, MissingSchemaAction missingSchemaAction)
		{
			string tableName = sourceTable.TableName;
			
			// if the source table not exists in the target dataset
			// we act according to the missingschemaaction param.
			if (!targetSet.Tables.Contains(tableName))
			{
				if (missingSchemaAction == MissingSchemaAction.Ignore)
					return true;
				if (missingSchemaAction == MissingSchemaAction.Error)
					throw new ArgumentException("Target DataSet missing definition for "+ tableName + ".");
				targetSet.Tables.Add((DataTable)sourceTable.Clone());
			}
			
			DataTable table = targetSet.Tables[tableName];
			
			if (!CheckPrimaryKeys(table, sourceTable))
				return false;
			
			for (int i = 0; i < sourceTable.Columns.Count; i++)
			{
				DataColumn col = sourceTable.Columns[i];
				// if a column from the source table doesn't exists in the target table
				// we act according to the missingschemaaction param.
				if(!table.Columns.Contains(col.ColumnName))
				{
					if (missingSchemaAction == MissingSchemaAction.Ignore)
						continue;
					if (missingSchemaAction == MissingSchemaAction.Error)
						throw new ArgumentException(("Column '" + col.ColumnName + "' does not belong to table Items."));
					
					table.Columns.Add(new DataColumn(col.ColumnName, col.DataType, col.Expression, col.ColumnMapping));
				}
			}

			return true;
		}
		
		// find if there is a valid matching between the targetTable PrimaryKey and the
		// sourceTable primatyKey.
		// return true if there is a match, else return false and raise a MergeFailedEvent.
		private static bool CheckPrimaryKeys(DataTable targetTable, DataTable sourceTable)
		{
			// if the length of one of the tables primarykey if 0 - there is nothing to check.
			if (targetTable.PrimaryKey.Length != 0 && sourceTable.PrimaryKey.Length != 0)
			{
				// if the length of primarykey is not equal - merge fails
				if (targetTable.PrimaryKey.Length != sourceTable.PrimaryKey.Length)
				{
					string message = "<target>.PrimaryKey and <source>.PrimaryKey have different Length.";
					MergeFailedEventArgs e = new MergeFailedEventArgs(sourceTable, message);
					targetTable.DataSet.OnMergeFailed(e);
					return false;
				}
				else
				{
					// we have to see that each primary column in the target table
					// has a column with the same name in the sourcetable primarykey columns. 
					bool foundMatch;
					DataColumn[] targetDataColumns = targetTable.PrimaryKey;
					DataColumn[] srcDataColumns = sourceTable.PrimaryKey;

					// loop on all primary key columns in the targetTable.
					for (int i = 0; i < targetDataColumns.Length; i++)
					{
						foundMatch = false;
						DataColumn col = targetDataColumns[i];

						// find if there is a column with the same name in the 
						// sourceTable primary key columns.
						for (int j = 0; j < srcDataColumns.Length; j++)
						{
							if (srcDataColumns[j].ColumnName == col.ColumnName)
							{
								foundMatch = true;
								break;
							}
						}
						if (!foundMatch)
						{
							string message = "Mismatch columns in the PrimaryKey : <target>." + col.ColumnName + " versus <source>." + srcDataColumns[i].ColumnName + ".";
							MergeFailedEventArgs e = new MergeFailedEventArgs(sourceTable, message);
							targetTable.DataSet.OnMergeFailed(e);
							return false;
						}
						
					}
				}
			}

			return true;
		}
		
		// fill the data from the source table to the target table
		private static void fillData(DataTable targetTable, DataTable sourceTable, bool preserveChanges)
		{
			for (int i = 0; i < sourceTable.Rows.Count; i++)
			{
				DataRow row = sourceTable.Rows[i];
				MergeRow(targetTable, row, preserveChanges);
			}
		}
		
		// check tha column from 2 tables that has the same name also has the same datatype.
		private static void checkColumnTypes(DataTable targetTable, DataTable sourceTable)
		{
			for (int i = 0; i < sourceTable.Columns.Count; i++)
			{
				DataColumn fromCol = sourceTable.Columns[i];
				DataColumn toCol = targetTable.Columns[fromCol.ColumnName];
				if(toCol.DataType != fromCol.DataType)
					throw new DataException("<target>." + fromCol.ColumnName + " and <source>." + fromCol.ColumnName + " have conflicting properties: DataType property mismatch.");
			}
		}
	}
}
