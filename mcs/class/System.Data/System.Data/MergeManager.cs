
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
			if (sourceSet == targetSet)
				return;

			bool prevEC = targetSet.EnforceConstraints;
			targetSet.EnforceConstraints = false;

			foreach (DataTable t in sourceSet.Tables)
				MergeManager.Merge(targetSet, t, preserveChanges, missingSchemaAction);

			AdjustSchemaRelations (targetSet, sourceSet, missingSchemaAction);
			targetSet.EnforceConstraints = prevEC;
		}

		internal static void Merge(DataSet targetSet, DataTable sourceTable, bool preserveChanges, MissingSchemaAction missingSchemaAction)
		{
			if(targetSet == null)
				throw new ArgumentNullException("targetSet");
			if(sourceTable == null)
				throw new ArgumentNullException("sourceTable");
			if (sourceTable.DataSet == targetSet)
				return;

			bool savedEnfoceConstraints = targetSet.EnforceConstraints;
			targetSet.EnforceConstraints = false;

			DataTable targetTable = null;
			if (!AdjustSchema(targetSet, sourceTable, missingSchemaAction,ref targetTable))
				return;
			if (targetTable != null)
				fillData(targetTable, sourceTable, preserveChanges);
			targetSet.EnforceConstraints = savedEnfoceConstraints;
		}

		internal static void Merge (DataTable targetTable, 
					    DataTable sourceTable, 
					    bool preserveChanges, 
					    MissingSchemaAction missingSchemaAction)
		{
			if(targetTable== null)
				throw new ArgumentNullException("targetTable");
			if(sourceTable == null)
				throw new ArgumentNullException("sourceTable");
			if (sourceTable == targetTable)
				return;

			bool savedEnforceConstraints = targetTable.EnforceConstraints;
			targetTable.EnforceConstraints = false;

			if (!AdjustSchema(targetTable, sourceTable, missingSchemaAction))
				return;

			fillData(targetTable, sourceTable, preserveChanges);
			targetTable.EnforceConstraints = savedEnforceConstraints;
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
				if (!AdjustSchema(targetSet, sourceTable, missingSchemaAction,ref targetTable))
					return;
				if (targetTable != null) {
					MergeRow(targetTable, row, preserveChanges);
					if (!(targetTables.IndexOf(targetTable) >= 0))
						targetTables.Add(targetTable);
				}
			}

			targetSet.EnforceConstraints = savedEnfoceConstraints;
		}

		// merge a row into a target table.
		private static void MergeRow(DataTable targetTable, DataRow row, bool preserveChanges)
		{
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
				targetRow = targetTable.Rows.Find(keyValues, DataViewRowState.OriginalRows);
				if (targetRow == null)
					targetRow = targetTable.Rows.Find(keyValues);
			}
			// row doesn't exist in target table, or there are no primary keys.
			// create new row and copy values from source row to the new row.
			if (targetRow == null)
			{ 
				DataRow newRow = targetTable.NewNotInitializedRow();
				// Don't check for ReadOnly, when cloning data to new uninitialized row.
				row.CopyValuesToRow(newRow, false);
				targetTable.Rows.AddInternal (newRow);
			}
			// row exists in target table, and presere changes is false - 
			// change the values of the target row to the values of the source row.
			else
			{
				row.MergeValuesToRow(targetRow, preserveChanges);
			}
		}
			
		private static bool AdjustSchemaRelations (DataSet targetSet, DataSet sourceSet, MissingSchemaAction missingSchemaAction)
		{
			if (missingSchemaAction == MissingSchemaAction.Ignore)
				return true;

			foreach(DataTable sourceTable in sourceSet.Tables) {

				DataTable targetTable = targetSet.Tables[sourceTable.TableName];
				if (targetTable == null)
					continue;

				foreach (Constraint constraint in sourceTable.Constraints) {

					Constraint targetConstraint = null;

					string constraintName = constraint.ConstraintName;
					if (targetTable.Constraints.Contains (constraintName))
						constraintName = "";

					UniqueConstraint uc = constraint as UniqueConstraint;
					// PrimaryKey is already taken care of while merging the table
					// ForeignKey constraint takes care of Parent Unique Constraints
					if (uc != null) {
						if (uc.IsPrimaryKey || uc.ChildConstraint != null)
							continue;
						DataColumn[] columns = ResolveColumns (targetTable, uc.Columns);
						targetConstraint = new UniqueConstraint (constraintName, columns, false);
					}

					ForeignKeyConstraint fc = constraint as ForeignKeyConstraint;
					if (fc != null) {
						DataColumn[] columns = ResolveColumns (targetTable, fc.Columns);
						DataColumn[] relatedColumns = ResolveColumns (targetSet.Tables [fc.RelatedTable.TableName],
											fc.RelatedColumns);
						targetConstraint = new ForeignKeyConstraint (constraintName, relatedColumns, columns);
					}

					bool dupConstraintFound = false;
					foreach (Constraint cons in targetTable.Constraints) {
					if (!targetConstraint.Equals (cons))
						continue;
					dupConstraintFound = true;
					break;
					}

					// If equivalent-constraint already exists, then just do nothing
					if (dupConstraintFound)
						continue;

					if (missingSchemaAction == MissingSchemaAction.Error)
						throw new DataException ("Target DataSet missing " + targetConstraint.GetType () +
								targetConstraint.ConstraintName);
					else
						targetTable.Constraints.Add (targetConstraint);
				}
			}

			foreach (DataRelation relation in sourceSet.Relations) {
				DataRelation targetRelation = targetSet.Relations [relation.RelationName];
				if (targetRelation == null) {
					if (missingSchemaAction == MissingSchemaAction.Error)
						throw new ArgumentException ("Target DataSet mising definition for " +
								relation.RelationName);

					DataColumn[] parentColumns = ResolveColumns (targetSet.Tables [relation.ParentTable.TableName],
							relation.ParentColumns);
					DataColumn[] childColumns = ResolveColumns (targetSet.Tables [relation.ChildTable.TableName],
							relation.ChildColumns);
					targetRelation = targetSet.Relations.Add (relation.RelationName, parentColumns,
							childColumns, relation.createConstraints);
					targetRelation.Nested = relation.Nested;
				} else if (!CompareColumnArrays (relation.ParentColumns, targetRelation.ParentColumns) ||
						!CompareColumnArrays (relation.ChildColumns, targetRelation.ChildColumns)) {
					RaiseMergeFailedEvent (null, "Relation " + relation.RelationName +
						" cannot be merged, because keys have mismatch columns.");
				}
			}

			return true;
		}

		private static DataColumn[] ResolveColumns(DataTable targetTable, DataColumn[] sourceColumns)
		{
			if (sourceColumns != null && sourceColumns.Length > 0) {
				// lets just assume that all columns are from the Same table
				if (targetTable != null) {
					int i=0;
					DataColumn[] targetColumns = new DataColumn[sourceColumns.Length];

					DataColumn tmpCol ;
					foreach(DataColumn sourceColumn in sourceColumns) {
						tmpCol = targetTable.Columns[sourceColumn.ColumnName];
						if (tmpCol == null)
							throw new DataException ("Column " + sourceColumn.ColumnName  + 
									" does not belong to table " + targetTable.TableName);
						targetColumns [i++] = tmpCol;
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
			// if the source table not exists in the target dataset
			// we act according to the missingschemaaction param.
			DataTable targetTable = targetSet.Tables [sourceTable.TableName];
			if (targetTable == null) {
				if (missingSchemaAction == MissingSchemaAction.Ignore)
					return true;

				if (missingSchemaAction == MissingSchemaAction.Error)
					throw new ArgumentException ("Target DataSet missing definition for " +
							sourceTable.TableName + ".");

				targetTable = (DataTable)sourceTable.Clone();
				targetSet.Tables.Add(targetTable);
			}

			AdjustSchema (targetTable, sourceTable, missingSchemaAction);

			newTable = targetTable;
			return true;
		}


		private static bool AdjustSchema(DataTable targetTable, DataTable sourceTable, MissingSchemaAction missingSchemaAction)
		{
			if (missingSchemaAction == MissingSchemaAction.Ignore)
				return true;

			for (int i = 0; i < sourceTable.Columns.Count; i++) {
				DataColumn sourceColumn = sourceTable.Columns[i];
				// if a column from the source table doesn't exists in the target table
				// we act according to the missingschemaaction param.
				DataColumn targetColumn = targetTable.Columns [sourceColumn.ColumnName];
				if(targetColumn == null) {
					if (missingSchemaAction == MissingSchemaAction.Error)
						throw new DataException ("Target table " + targetTable.TableName +
								" missing definition for column " + sourceColumn.ColumnName);
					
					targetColumn = new DataColumn(sourceColumn.ColumnName, sourceColumn.DataType, 
								sourceColumn.Expression, sourceColumn.ColumnMapping);
					targetTable.Columns.Add(targetColumn);
				}

				if(sourceColumn.AutoIncrement) {
					targetColumn.AutoIncrement = sourceColumn.AutoIncrement;
					targetColumn.AutoIncrementSeed = sourceColumn.AutoIncrementSeed;
					targetColumn.AutoIncrementStep = sourceColumn.AutoIncrementStep;
				}
			}

			if (!AdjustPrimaryKeys(targetTable, sourceTable))
				return false;

			checkColumnTypes (targetTable, sourceTable);

			return true;
		}
	
	
		// find if there is a valid matching between the targetTable PrimaryKey and the
		// sourceTable primatyKey.
		// return true if there is a match, else return false and raise a MergeFailedEvent.
		private static bool AdjustPrimaryKeys(DataTable targetTable, DataTable sourceTable)
		{
			if (sourceTable.PrimaryKey.Length == 0)
				return true;

			// If targetTable does not have a PrimaryKey, just import the sourceTable PrimaryKey
			if (targetTable.PrimaryKey.Length == 0) {
				DataColumn[] targetColumns = ResolveColumns (targetTable, sourceTable.PrimaryKey);
				targetTable.PrimaryKey = targetColumns;
				return true;
			}
			
			// If both the tables have a primary key, verify that they are equivalent.
			// raise a MergeFailedEvent if the keys are not equivalent
			if (targetTable.PrimaryKey.Length != sourceTable.PrimaryKey.Length) {
				RaiseMergeFailedEvent (targetTable, "<target>.PrimaryKey and <source>.PrimaryKey have different Length.");
				return false;
			}

			for (int i=0; i < targetTable.PrimaryKey.Length; ++i) {
				if (targetTable.PrimaryKey [i].ColumnName.Equals (sourceTable.PrimaryKey [i].ColumnName))
					continue;
				RaiseMergeFailedEvent (targetTable, "Mismatch columns in the PrimaryKey : <target>." + 
					targetTable.PrimaryKey [i].ColumnName + " versus <source>." + sourceTable.PrimaryKey [i].ColumnName);
				return false;
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
				if (toCol == null)
					continue;
				if (toCol.DataTypeMatches (fromCol))
					continue;
				throw new DataException("<target>." + fromCol.ColumnName + " and <source>." + 
						fromCol.ColumnName + " have conflicting properties: DataType " + 
						" property mismatch.");
			}
		}

		private static bool CompareColumnArrays (DataColumn[] arr1, DataColumn[] arr2)
		{
			if (arr1.Length != arr2.Length)
				return false;

			for (int i=0; i < arr1.Length; ++i)
				if (!arr1 [i].ColumnName.Equals (arr2 [i].ColumnName))
					return false;
			return true;
		}

		private static void RaiseMergeFailedEvent (DataTable targetTable, string errMsg)
		{
			MergeFailedEventArgs args = new MergeFailedEventArgs (targetTable, errMsg);
			if (targetTable.DataSet != null)
				targetTable.DataSet.OnMergeFailed (args);
		}
	}
}
