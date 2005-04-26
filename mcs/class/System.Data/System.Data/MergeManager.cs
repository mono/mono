
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
using System;
using System.Collections;

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

			AdjustSchema(targetSet,sourceSet,missingSchemaAction);

		}

		internal static void Merge(DataSet targetSet, DataTable sourceTable, bool preserveChanges, MissingSchemaAction missingSchemaAction)
		{
			if(targetSet == null)
				throw new ArgumentNullException("targetSet");
			if(sourceTable == null)
				throw new ArgumentNullException("sourceTable");

			bool savedEnfoceConstraints = targetSet.EnforceConstraints;
			targetSet.EnforceConstraints = false;

			DataTable targetTable = null;
			if (!AdjustSchema(targetSet, sourceTable, missingSchemaAction,ref targetTable)) {
				return;
			}
			if (targetTable != null) {
				checkColumnTypes(targetTable, sourceTable); // check that the colums datatype is the same
				fillData(targetTable, sourceTable, preserveChanges);
			}
			targetSet.EnforceConstraints = savedEnfoceConstraints;
			
			if (!targetSet.EnforceConstraints && targetTable != null) {
				// indexes are still outdated
				targetTable.ResetIndexes();
			}
		}

		internal static void Merge(DataSet targetSet, DataRow[] sourceRows, bool preserveChanges, MissingSchemaAction missingSchemaAction)
		{
			if(targetSet == null)
				throw new ArgumentNullException("targetSet");
			if(sourceRows == null)
				throw new ArgumentNullException("sourceRows");

			bool savedEnfoceConstraints = targetSet.EnforceConstraints;
			targetSet.EnforceConstraints = false;

			ArrayList targetTables = new ArrayList();
			for (int i = 0; i < sourceRows.Length; i++) {
				DataRow row = sourceRows[i];
				DataTable sourceTable = row.Table;
				DataTable targetTable = null;
				if (!AdjustSchema(targetSet, sourceTable, missingSchemaAction,ref targetTable)) {
					return;
				}
				if (targetTable != null) {
					checkColumnTypes(targetTable, row.Table);
					MergeRow(targetTable, row, preserveChanges);
					if (!(targetTables.IndexOf(targetTable) >= 0)) {
						targetTables.Add(targetTable);
					}
				}
			}

			targetSet.EnforceConstraints = savedEnfoceConstraints;

			foreach(DataTable table in targetTables) {
				table.ResetIndexes();
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
				targetRow.Proposed = -1;
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
			

		// adjust the dataset schema according to the missingschemaaction param
		// (relations).
		// return false if adjusting fails.
		private static bool AdjustSchema(DataSet targetSet, DataSet sourceSet, MissingSchemaAction missingSchemaAction)
		{
			if (missingSchemaAction == MissingSchemaAction.Add || missingSchemaAction == MissingSchemaAction.AddWithKey) {
				foreach (DataRelation relation in sourceSet.Relations) {
					// TODO : add more precise condition (columns)
					if (!targetSet.Relations.Contains(relation.RelationName)) {
						DataTable targetTable = targetSet.Tables[relation.ParentColumns[0].Table.TableName];
						DataColumn[] parentColumns = ResolveColumns(sourceSet,targetTable,relation.ParentColumns);
						targetTable = targetSet.Tables[relation.ChildColumns[0].Table.TableName];
						DataColumn[] childColumns = ResolveColumns(sourceSet,targetTable,relation.ChildColumns);
						if (parentColumns != null && childColumns != null) {
							DataRelation newRelation = new DataRelation(relation.RelationName,parentColumns,childColumns);
							newRelation.Nested = relation.Nested; 
							targetSet.Relations.Add(newRelation);
						}
					}
					else {
						// TODO : should we throw an exeption ?
					}
				}			

				foreach(DataTable sourceTable in sourceSet.Tables) {				
					DataTable targetTable = targetSet.Tables[sourceTable.TableName];

					if (targetTable != null) {
						foreach(Constraint constraint in sourceTable.Constraints) {

							if (constraint is UniqueConstraint) {
								UniqueConstraint uc = (UniqueConstraint)constraint;
								// FIXME : add more precise condition (columns)
								if ( !targetTable.Constraints.Contains(uc.ConstraintName) ) {		
									DataColumn[] columns = ResolveColumns(sourceSet,targetTable,uc.Columns);
									if (columns != null) {
										UniqueConstraint newConstraint = new UniqueConstraint(uc.ConstraintName,columns,uc.IsPrimaryKey);
										targetTable.Constraints.Add(newConstraint);
									}
								}
								else {
									// FIXME : should we throw an exception ?
								}
							}
							else {
								ForeignKeyConstraint fc = (ForeignKeyConstraint)constraint;
								// FIXME : add more precise condition (columns)
								if (!targetTable.Constraints.Contains(fc.ConstraintName)) {
									DataColumn[] columns = ResolveColumns(sourceSet,targetTable,fc.Columns);
									DataTable relatedTable = targetSet.Tables[fc.RelatedTable.TableName];
									DataColumn[] relatedColumns = ResolveColumns(sourceSet,relatedTable,fc.RelatedColumns);
									if (columns != null && relatedColumns != null) {
										ForeignKeyConstraint newConstraint = new ForeignKeyConstraint(fc.ConstraintName,relatedColumns,columns);
										targetTable.Constraints.Add(newConstraint);
									}
								}
								else {
									// FIXME : should we throw an exception ?
								}
							}
						}
					}
				}
			}

			return true;
		}

		private static DataColumn[] ResolveColumns(DataSet targetSet,DataTable targetTable,DataColumn[] sourceColumns)
		{
			if (sourceColumns != null && sourceColumns.Length > 0) {
				// TODO : worth to check that all source colums come from the same table
				if (targetTable != null) {
					int i=0;
					DataColumn[] targetColumns = new DataColumn[sourceColumns.Length];
					foreach(DataColumn sourceColumn in sourceColumns) {
						targetColumns[i++] = targetTable.Columns[sourceColumn.ColumnName];
					}
					return targetColumns;
				}
			}
			return null;
		}

		
		// adjust the table schema according to the missingschemaaction param.
		// return false if adjusting fails.
		private static bool AdjustSchema(DataSet targetSet, DataTable sourceTable, MissingSchemaAction missingSchemaAction, ref DataTable newTable)
		{
			string tableName = sourceTable.TableName;
			
			// if the source table not exists in the target dataset
			// we act according to the missingschemaaction param.
			int tmp = targetSet.Tables.IndexOf(tableName);
			// we need to check if it is equals names
			if (tmp != -1 && !targetSet.Tables[tmp].TableName.Equals(tableName))
				tmp = -1;
			if (tmp == -1) {
				if (missingSchemaAction == MissingSchemaAction.Ignore) {
					return true;
				}
				if (missingSchemaAction == MissingSchemaAction.Error) {
					throw new ArgumentException("Target DataSet missing definition for "+ tableName + ".");
				}
				
				DataTable cloneTable = (DataTable)sourceTable.Clone();
				targetSet.Tables.Add(cloneTable);
				tableName = cloneTable.TableName;
			}								
			
			DataTable table = targetSet.Tables[tableName];
			
			for (int i = 0; i < sourceTable.Columns.Count; i++) {
				DataColumn sourceColumn = sourceTable.Columns[i];
				// if a column from the source table doesn't exists in the target table
				// we act according to the missingschemaaction param.
				DataColumn targetColumn = table.Columns[sourceColumn.ColumnName];
				if(targetColumn == null) {
					if (missingSchemaAction == MissingSchemaAction.Ignore) {
						continue;
					}
					if (missingSchemaAction == MissingSchemaAction.Error) {
						throw new ArgumentException(("Column '" + sourceColumn.ColumnName + "' does not belong to table Items."));
					}
					
					targetColumn = new DataColumn(sourceColumn.ColumnName, sourceColumn.DataType, sourceColumn.Expression, sourceColumn.ColumnMapping);
					table.Columns.Add(targetColumn);
				}

				if (sourceColumn.Unique) {
					try {
						targetColumn.Unique = sourceColumn.Unique;
					}
					catch(Exception e){
//						Console.WriteLine("targetColumn : {0}   targetTable : {1} ",targetColumn.ColumnName,table.TableName);
						foreach(DataRow row in table.Rows) {
//							Console.WriteLine(row[targetColumn]);
						}
						throw e;
					}
				}

				if(sourceColumn.AutoIncrement) {
					targetColumn.AutoIncrement = sourceColumn.AutoIncrement;
					targetColumn.AutoIncrementSeed = sourceColumn.AutoIncrementSeed;
					targetColumn.AutoIncrementStep = sourceColumn.AutoIncrementStep;
				}
			}

			if (!AdjustPrimaryKeys(table, sourceTable)) {
				return false;
			}

			newTable = table;
			return true;
		}
		
		// find if there is a valid matching between the targetTable PrimaryKey and the
		// sourceTable primatyKey.
		// return true if there is a match, else return false and raise a MergeFailedEvent.
		private static bool AdjustPrimaryKeys(DataTable targetTable, DataTable sourceTable)
		{
			// if the length of one of the tables primarykey if 0 - there is nothing to check.
			if (sourceTable.PrimaryKey.Length != 0) {
				if (targetTable.PrimaryKey.Length == 0) {
					// if target table has no primary key at all - 
					// import primary key from source table
					DataColumn[] targetColumns = new DataColumn[sourceTable.PrimaryKey.Length];
					
					for(int i=0; i < sourceTable.PrimaryKey.Length; i++){
					    DataColumn sourceColumn = sourceTable.PrimaryKey[i];
						DataColumn targetColumn = targetTable.Columns[sourceColumn.ColumnName];

						if (targetColumn == null) {
							// is target table has no column corresponding
							// to source table PK column - merge fails
							string message = "Column " + sourceColumn.ColumnName + " does not belongs to table " + targetTable.TableName;
							MergeFailedEventArgs e = new MergeFailedEventArgs(sourceTable, message);
							targetTable.DataSet.OnMergeFailed(e);
							return false;
						}
						else {
							targetColumns[i] = targetColumn;
						}
					}
					targetTable.PrimaryKey = targetColumns;
				}
				else {
					// if target table already has primary key and
					// if the length of primarykey is not equal - merge fails
					if (targetTable.PrimaryKey.Length != sourceTable.PrimaryKey.Length) {
						string message = "<target>.PrimaryKey and <source>.PrimaryKey have different Length.";
						MergeFailedEventArgs e = new MergeFailedEventArgs(sourceTable, message);
						targetTable.DataSet.OnMergeFailed(e);
						return false;
					}
					else {
						// we have to see that each primary column in the target table
						// has a column with the same name in the sourcetable primarykey columns. 
						bool foundMatch;
						DataColumn[] targetDataColumns = targetTable.PrimaryKey;
						DataColumn[] srcDataColumns = sourceTable.PrimaryKey;

						// loop on all primary key columns in the targetTable.
						for (int i = 0; i < targetDataColumns.Length; i++) {
							foundMatch = false;
							DataColumn col = targetDataColumns[i];

							// find if there is a column with the same name in the 
							// sourceTable primary key columns.
							for (int j = 0; j < srcDataColumns.Length; j++) {
								if (srcDataColumns[j].ColumnName == col.ColumnName) {
									foundMatch = true;
									break;
								}
							}
							if (!foundMatch) {
								string message = "Mismatch columns in the PrimaryKey : <target>." + col.ColumnName + " versus <source>." + srcDataColumns[i].ColumnName + ".";
								MergeFailedEventArgs e = new MergeFailedEventArgs(sourceTable, message);
								targetTable.DataSet.OnMergeFailed(e);
								return false;
							}
							
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
				if((toCol != null) && (toCol.DataType != fromCol.DataType))
					throw new DataException("<target>." + fromCol.ColumnName + " and <source>." + fromCol.ColumnName + " have conflicting properties: DataType property mismatch.");
			}
		}
	}
}
