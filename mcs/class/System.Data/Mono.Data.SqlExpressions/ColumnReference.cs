//
// ColumnReference.cs
//
// Author:
//   Juraj Skripsky (juraj@hotfeet.ch)
//
// (C) 2004 HotFeet GmbH (http://www.hotfeet.ch)
//

using System;
using System.Collections;
using System.Data;

namespace Mono.Data.SqlExpressions {
	public enum ReferencedTable {
		Self,
		Parent,
		Child
	}
	
	public class ColumnReference : IExpression {
		ReferencedTable refTable;
		string relationName, columnName;

		public ColumnReference (string columnName) : this (ReferencedTable.Self, null, columnName) {}

		public ColumnReference (ReferencedTable refTable, string relationName, string columnName)
		{
			this.refTable = refTable;
			this.relationName = relationName;
			this.columnName = columnName;
		}

		protected DataRelation GetRelation (DataRow row)
		{
			DataRelationCollection relations;
			if (relationName != null) {
				relations = row.Table.DataSet.Relations;
				return relations[relations.IndexOf(relationName)];
			}

			if (refTable == ReferencedTable.Parent)
				relations = row.Table.ParentRelations;
			else
				relations = row.Table.ChildRelations;
				
			if (relations.Count > 1)
				throw new EvaluateException (String.Format (
					"The table [{0}] is involved in more than one relation." +
					"You must explicitly mention a relation name.",
					row.Table.TableName));
			else
				return relations[0];
		}

		public DataRow GetReferencedRow (DataRow row)
		{
			switch (refTable) {
			case ReferencedTable.Self:
			default:
				return row;

			case ReferencedTable.Parent:
				return row.GetParentRow (GetRelation (row));

			case ReferencedTable.Child:
				return row.GetChildRows (GetRelation (row)) [0];
			}
		}
		
		public DataRow[] GetReferencedRows (DataRow row)
		{
			switch (refTable) {
			case ReferencedTable.Self:
			default:
				DataRow[] rows = new DataRow [row.Table.Rows.Count];
				row.Table.Rows.CopyTo (rows, 0);
				return rows;
				
			case ReferencedTable.Parent:
				return row.GetParentRows (GetRelation (row));

			case ReferencedTable.Child:
				return row.GetChildRows (GetRelation (row));
			}
		}
		
		public object[] GetValues (DataRow[] rows)
		{
			object[] values = new object [rows.Length];
			for (int i = 0; i < rows.Length; i++)
				values [i] = Unify (rows [i][columnName]);
				
			return values;
		}

		private object Unify (object val) {
			if (Numeric.IsNumeric (val))
				return Numeric.Unify ((IConvertible)val);
				
			if (val == null || val == DBNull.Value)
				return null;
				
			if (val is bool || val is string || val is DateTime)
				return val;
			
			if (val is Enum)
				return (int)val;
			
			throw new EvaluateException (String.Format ("Cannot handle data type found in column '{0}'.", columnName));			
		}

		public object Eval (DataRow row)
		{
			DataRow referencedRow = GetReferencedRow (row);
			if (referencedRow == null)
				return null;
				
			object val;
			try {			
				val = referencedRow [columnName];
			} catch (IndexOutOfRangeException) {
				throw new EvaluateException (String.Format ("Column '{0}' does not exist.", columnName));
			}
			return Unify (val);
		}
	}
}
