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
using System.ComponentModel;

namespace Mono.Data.SqlExpressions {
	internal enum ReferencedTable {
		Self,
		Parent,
		Child
	}
	
	internal class ColumnReference : BaseExpression {
		ReferencedTable refTable;
		string relationName, columnName;
		DataColumn _cachedColumn;
		DataRelation _cachedRelation;

		public ColumnReference (string columnName) : this (ReferencedTable.Self, null, columnName) {}

		public ColumnReference (ReferencedTable refTable, string relationName, string columnName)
		{
			this.refTable = refTable;
			this.relationName = relationName;
			this.columnName = columnName;
		}

		public override bool Equals(object obj)
		{
			if (!base.Equals (obj))
				return false;

			if (!(obj is ColumnReference))
				return false;

			ColumnReference other = (ColumnReference) obj;
			if (other.refTable != refTable)
				return false;

			if (other.columnName != columnName)
				return false;		

			if (other.relationName != relationName)
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			int hashCode = base.GetHashCode ();
			hashCode ^= refTable.GetHashCode ();
			hashCode ^= columnName.GetHashCode ();
			hashCode ^= relationName.GetHashCode ();
			return hashCode;
		}

		public ReferencedTable ReferencedTable {
			get { return refTable; }
		}

		private DataRelation GetRelation (DataRow row)
		{
			if (_cachedRelation == null) {
				DataTable table = row.Table;
				DataRelationCollection relations;
				if (relationName != null) {
					relations = table.DataSet.Relations;
					_cachedRelation = relations [relations.IndexOf (relationName)];
				}
				else {
					if (refTable == ReferencedTable.Parent)
						relations = table.ParentRelations;
					else
						relations = table.ChildRelations;
						
					if (relations.Count > 1)
						throw new EvaluateException (String.Format (
							"The table [{0}] is involved in more than one relation." +
							"You must explicitly mention a relation name.",
							table.TableName));
					else
						_cachedRelation = relations [0];
				}
				_cachedRelation.DataSet.Relations.CollectionChanged += new CollectionChangeEventHandler (OnRelationRemoved);
			}
			return _cachedRelation;
		}

		private DataColumn GetColumn (DataRow row)
		{
			if (_cachedColumn == null) {
				DataTable table = row.Table;
				switch (refTable) {
					case ReferencedTable.Parent:
						table = GetRelation (row).ParentTable;
						break;
					case ReferencedTable.Child:
						table = GetRelation (row).ChildTable;
						break;
				}
				_cachedColumn = table.Columns [columnName];
				if (_cachedColumn == null)
					throw new EvaluateException (String.Format ("Cannot find column [{0}].", columnName));

				_cachedColumn.PropertyChanged += new PropertyChangedEventHandler (OnColumnPropertyChanged);
				_cachedColumn.Table.Columns.CollectionChanged += new CollectionChangeEventHandler (OnColumnRemoved);
			}
			return _cachedColumn;
		}

		public DataRow GetReferencedRow (DataRow row)
		{
			// Verify the column reference is valid 
			GetColumn (row);

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
			// Verify the column reference is valid 
			GetColumn (row);

			switch (refTable) {
			case ReferencedTable.Self:
			default:
				DataRow[] rows = row.Table.NewRowArray(row.Table.Rows.Count);
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
				values [i] = Unify (rows [i][GetColumn (rows [i])]);
				
			return values;
		}

		private object Unify (object val) {
			if (Numeric.IsNumeric (val))
				return Numeric.Unify ((IConvertible)val);
				
			if (val == null || val == DBNull.Value)
				return null;
				
			if (val is bool || val is string || val is DateTime || val is Guid || val is char)
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
				val = referencedRow [GetColumn (row)];
				referencedRow._inExpressionEvaluation = false;
			} catch (IndexOutOfRangeException) {
				throw new EvaluateException (String.Format ("Cannot find column [{0}].", columnName));
			}
			return Unify (val);
		}

		public override bool EvalBoolean (DataRow row)
		{
			DataColumn col = GetColumn (row);
			if (col.DataType != typeof (bool))
				throw new EvaluateException ("Not a Boolean Expression");

			object result  = Eval (row);
			if (result == null || result == DBNull.Value)
				return false;
			else
				return (bool)result;
		}

		override public bool DependsOn(DataColumn other)
		{
			return refTable == ReferencedTable.Self && columnName == other.ColumnName;
		}

		private void DropCached (DataColumnCollection columnCollection, DataRelationCollection relationCollection)
		{
			if (_cachedColumn != null) {
				// unregister column listener
				_cachedColumn.PropertyChanged -= new PropertyChangedEventHandler (OnColumnPropertyChanged);

				// unregister column collection listener
				if (columnCollection != null)	
					columnCollection.CollectionChanged -= new CollectionChangeEventHandler (OnColumnRemoved);
				else if (_cachedColumn.Table != null)
					_cachedColumn.Table.Columns.CollectionChanged -= new CollectionChangeEventHandler (OnColumnRemoved);
				
				_cachedColumn = null;
			}

			if (_cachedRelation != null) {
				// unregister relation collection listener
				if (relationCollection != null)				
					relationCollection.CollectionChanged -= new CollectionChangeEventHandler (OnRelationRemoved);
				else if (_cachedRelation.DataSet != null)
					_cachedRelation.DataSet.Relations.CollectionChanged -= new CollectionChangeEventHandler (OnRelationRemoved);

				_cachedRelation = null;
			}			
		}

		private void OnColumnPropertyChanged (object sender, PropertyChangedEventArgs args)
		{
			if (!(sender is DataColumn))
				return;
			
			DataColumn dc = (DataColumn) sender;
			if ((dc == _cachedColumn) && args.PropertyName == "ColumnName")
				DropCached (null, null);
		}

		private void OnColumnRemoved (object sender, CollectionChangeEventArgs args)
		{
			if (!(args.Element is DataColumnCollection))
				return;

			if (args.Action != CollectionChangeAction.Remove)
				return;

			DataColumnCollection columnCollection = (DataColumnCollection) args.Element;
			if (_cachedColumn != null && columnCollection != null && (columnCollection.IndexOf (_cachedColumn)) == -1)
				DropCached (columnCollection, null);
		}

		private void OnRelationRemoved (object sender, CollectionChangeEventArgs args)
		{
			if (!(args.Element is DataRelationCollection))
				return;

			if (args.Action != CollectionChangeAction.Remove)
				return;			

			DataRelationCollection relationCollection = (DataRelationCollection) args.Element;
			if (_cachedRelation != null && relationCollection != null && (relationCollection.IndexOf (_cachedRelation)) == -1)
				DropCached (null, relationCollection);
		}
	}
}
