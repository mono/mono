//
// ColumnReference.cs
//
// Author:
//   Juraj Skripsky (juraj@hotfeet.ch)
//
// (C) 2004 HotFeet GmbH (http://www.hotfeet.ch)
//

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
using System.Data;

namespace Mono.Data.SqlExpressions {
	internal enum ReferencedTable {
		Self,
		Parent,
		Child
	}
	
	internal class ColumnReference : BaseExpression {
		ReferencedTable refTable;
		string relationName, columnName;

		public ColumnReference (string columnName) : this (ReferencedTable.Self, null, columnName) {}

		public ColumnReference (ReferencedTable refTable, string relationName, string columnName)
		{
			this.refTable = refTable;
			this.relationName = relationName;
			this.columnName = columnName;
		}

		public ReferencedTable ReferencedTable {
			get { return refTable; }
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

		public override object Eval (DataRow row)
		{
			DataRow referencedRow = GetReferencedRow (row);
			if (referencedRow == null)
				return null;
				
			object val;
			try {
				referencedRow._inExpressionEvaluation = true;
				val = referencedRow [columnName];
				referencedRow._inExpressionEvaluation = false;
			} catch (IndexOutOfRangeException) {
				throw new EvaluateException (String.Format ("Cannot find column [{0}].", columnName));
			}
			return Unify (val);
		}
	}
}
