//
// System.Data.DataColumnCollection.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//   Stuart Caborn	<stuart.caborn@virgin.net>
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Chris Podurgiel
// Copyright (C) Tim Coleman, 2002
// Copyright (C) Daniel Morgan, 2003
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
using System.Text;
using System.Collections;
using System.ComponentModel;

namespace System.Data {
	[Editor ("Microsoft.VSDesigner.Data.Design.ColumnsCollectionEditor, " + Consts.AssemblyMicrosoft_VSDesigner,
		 "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
	[Serializable]
	[DefaultEvent ("CollectionChanged")]
	public class DataColumnCollection : InternalDataCollectionBase
	{
		//This hashtable maps between column name to DataColumn object.
		private Hashtable columnFromName = new Hashtable();
		//This ArrayList contains the auto-increment columns names
		private ArrayList autoIncrement = new ArrayList();
		//This holds the next index to use for default column name.
		private int defaultColumnIndex = 1;
		//table should be the DataTable this DataColumnCollection belongs to.
		private DataTable parentTable = null;
		// Keep reference to most recent columns passed to AddRange()
		// so that they can be added when EndInit() is called.
		DataColumn[] _mostRecentColumns = null;

		// Internal Constructor.  This Class can only be created from other classes in this assembly.
		internal DataColumnCollection(DataTable table):base()
		{
			parentTable = table;
		}

		/// <summary>
		/// Gets the DataColumn from the collection at the specified index.
		/// </summary>
		public virtual DataColumn this[int index]
		{
			get
			{
				if (index < 0 || index > base.List.Count) {
					throw new IndexOutOfRangeException("Cannot find column " + index + ".");
				}
				return (DataColumn) base.List[index];
			}
		}

		/// <summary>
		/// Gets the DataColumn from the collection with the specified name.
		/// </summary>
		public virtual DataColumn this[string name]
		{
			get
			{
				DataColumn dc = columnFromName[name] as DataColumn;
				
				if (dc != null)
					return dc;

				int tmp = IndexOf(name, true);
				if (tmp == -1)
					return null;
				return this[tmp]; 
			}
		}

		/// <summary>
		/// Gets a list of the DataColumnCollection items.
		/// </summary>
		protected override ArrayList List 
		{
			get
			{
				return base.List;
			}
		}

		internal ArrayList AutoIncrmentColumns 
		{
			get
			{
				return autoIncrement;
			}
		}

		//Add Logic
		//
		//Changing Event
		//DefaultValue set and AutoInc set check
		//?Validate Expression??
		//Name check and creation
		//Set Table
		//Check Unique if true then add a unique constraint
		//?Notify Rows of new column ?
		//Add to collection
		//Changed Event

		/// <summary>
		/// Creates and adds a DataColumn object to the DataColumnCollection.
		/// </summary>
		/// <returns></returns>
		public virtual DataColumn Add()
		{
			string defaultName = GetNextDefaultColumnName ();
			DataColumn column = new DataColumn (defaultName);
			Add (column);
			return column;
		}

		internal void RegisterName(string name, DataColumn column)
		{
			if (columnFromName.Contains(name))
				throw new DuplicateNameException("A DataColumn named '" + name + "' already belongs to this DataTable.");

			columnFromName[name] = column;

			if (name.StartsWith("Column") && name == MakeName(defaultColumnIndex + 1))
			{
				do
				{
					defaultColumnIndex++;
				}
				while (Contains(MakeName(defaultColumnIndex + 1)));
			}
		}

		internal void UnregisterName(string name)
		{
			if (columnFromName.Contains(name))
				columnFromName.Remove(name);

			if (name.StartsWith("Column") && name == MakeName(defaultColumnIndex - 1))
			{
				do
				{
					defaultColumnIndex--;
				}
				while (!Contains(MakeName(defaultColumnIndex - 1)) && defaultColumnIndex > 1);
			}
		}

		private string GetNextDefaultColumnName ()
		{
			string defColumnName = MakeName(defaultColumnIndex);
			for (int index = defaultColumnIndex + 1; Contains(defColumnName); ++index) {
				defColumnName = MakeName(index);
				defaultColumnIndex++;
			}
			defaultColumnIndex++;
			return defColumnName;
		}

		static readonly string[] TenColumns = { "Column0", "Column1", "Column2", "Column3", "Column4", "Column5", "Column6", "Column7", "Column8", "Column9" };

		private string MakeName(int index)
		{
			if (index < 10)
				return TenColumns[index];

			return String.Concat("Column", index.ToString());
		}

		/// <summary>
		/// Creates and adds the specified DataColumn object to the DataColumnCollection.
		/// </summary>
		/// <param name="column">The DataColumn to add.</param>
		public void Add(DataColumn column)
		{

			if (column == null)
				throw new ArgumentNullException ("column", "'column' argument cannot be null.");

			if (column.ColumnName.Equals(String.Empty))
			{
				column.ColumnName = GetNextDefaultColumnName ();
			}

//			if (Contains(column.ColumnName))
//				throw new DuplicateNameException("A DataColumn named '" + column.ColumnName + "' already belongs to this DataTable.");

			if (column.Table != null)
				throw new ArgumentException ("Column '" + column.ColumnName + "' already belongs to this or another DataTable.");

			CollectionChangeEventArgs e = new CollectionChangeEventArgs(CollectionChangeAction.Add, this);

			column.SetTable (parentTable);
			RegisterName(column.ColumnName, column);
			int ordinal = base.List.Add(column);
			column.SetOrdinal (ordinal);
		
			// Check if the Column Expression is ok	
			if (column.CompiledExpression != null)
				if (parentTable.Rows.Count == 0)
					column.CompiledExpression.Eval (parentTable.NewRow());
				else
					column.CompiledExpression.Eval (parentTable.Rows[0]);

			// if table already has rows we need to allocate space 
			// in the column data container 
			if ( parentTable.Rows.Count > 0 ) {
				column.DataContainer.Capacity = parentTable.RecordCache.CurrentCapacity;
			}

			if (column.AutoIncrement) {
				DataRowCollection rows = column.Table.Rows;
				for (int i = 0; i < rows.Count; i++)
					rows [i] [ordinal] = column.AutoIncrementValue ();
			}

			if (column.AutoIncrement)
				autoIncrement.Add(column);

			OnCollectionChanged (e);
		}

		/// <summary>
		/// Creates and adds a DataColumn object with the specified name to the DataColumnCollection.
		/// </summary>
		/// <param name="columnName">The name of the column.</param>
		/// <returns>The newly created DataColumn.</returns>
		public virtual DataColumn Add(string columnName)
		{
			if (columnName == null || columnName == String.Empty)
			{
				columnName = GetNextDefaultColumnName();
			}
			
			DataColumn column = new DataColumn(columnName);
			Add (column);
			return column;
		}

		/// <summary>
		/// Creates and adds a DataColumn object with the specified name and type to the DataColumnCollection.
		/// </summary>
		/// <param name="columnName">The ColumnName to use when cretaing the column.</param>
		/// <param name="type">The DataType of the new column.</param>
		/// <returns>The newly created DataColumn.</returns>
		public virtual DataColumn Add(string columnName, Type type)
		{
			if (columnName == null || columnName == "")
			{
				columnName = GetNextDefaultColumnName ();
			}
			
			DataColumn column = new DataColumn(columnName, type);
			Add (column);
			return column;
		}

		/// <summary>
		/// Creates and adds a DataColumn object with the specified name, type, and expression to the DataColumnCollection.
		/// </summary>
		/// <param name="columnName">The name to use when creating the column.</param>
		/// <param name="type">The DataType of the new column.</param>
		/// <param name="expression">The expression to assign to the Expression property.</param>
		/// <returns>The newly created DataColumn.</returns>
		public virtual DataColumn Add(string columnName, Type type, string expression)
		{
			if (columnName == null || columnName == "")
			{
				columnName = GetNextDefaultColumnName ();
			}
			
			DataColumn column = new DataColumn(columnName, type, expression);
			Add (column);
			return column;
		}

		/// <summary>
		/// Copies the elements of the specified DataColumn array to the end of the collection.
		/// </summary>
		/// <param name="columns">The array of DataColumn objects to add to the collection.</param>
		public void AddRange(DataColumn[] columns)
		{
			if (parentTable.fInitInProgress){
				_mostRecentColumns = columns;
				return;
			}

			if (columns == null)
				return;

			foreach (DataColumn column in columns)
			{
				Add(column);
			}
		}

		private string GetColumnDependency (DataColumn column)
		{

			foreach (DataRelation rel in parentTable.ParentRelations)
				if (Array.IndexOf (rel.ChildColumns, column) != -1)
					return String.Format (" child key for relationship {0}.", rel.RelationName);
			foreach (DataRelation rel in parentTable.ChildRelations)
				if (Array.IndexOf (rel.ParentColumns, column) != -1)
					return String.Format (" parent key for relationship {0}.", rel.RelationName);

			foreach (Constraint c in parentTable.Constraints) 
				if (c.IsColumnContained (column))
					return String.Format (" constraint {0} on the table {1}.", 
							c.ConstraintName, parentTable);
			
			// check if the foreign-key constraint on any table in the dataset refers to this column.
			// though a forignkeyconstraint automatically creates a uniquecontrainton the parent 
			// table and would fail above, but we still need to check, as it is legal to manually remove
			// the constraint on the parent table.
			if (parentTable.DataSet != null)
				foreach (DataTable table in parentTable.DataSet.Tables)
					foreach (Constraint c in table.Constraints)
						if (c is ForeignKeyConstraint && c.IsColumnContained(column))
							return String.Format (" constraint {0} on the table {1}.", 
									c.ConstraintName, table.TableName);
			foreach (DataColumn col in this) 
				if (col.CompiledExpression != null && col.CompiledExpression.DependsOn (column))
					return  col.Expression;
			return String.Empty;
		}

		/// <summary>
		/// Checks whether a given column can be removed from the collection.
		/// </summary>
		/// <param name="column">A DataColumn in the collection.</param>
		/// <returns>true if the column can be removed; otherwise, false.</returns>
		public bool CanRemove(DataColumn column)
		{
			if (column == null || column.Table != parentTable || GetColumnDependency(column) != String.Empty) 
				return false;
			return true;
		}

		/// <summary>
		/// Clears the collection of any columns.
		/// </summary>
		public void Clear()
		{
			CollectionChangeEventArgs e = new CollectionChangeEventArgs(CollectionChangeAction.Refresh, this);

			// its not necessary to check if each column in the collection can removed.
			// Can simply check, if there are any constraints/relations related to the table,
			// in which case, throw an exception.
			// Also, shudnt check for expression columns since all the columns in the table
			// are being removed.
			if (parentTable.Constraints.Count != 0 || 
			    parentTable.ParentRelations.Count != 0 ||
			    parentTable.ChildRelations.Count != 0)
				foreach (DataColumn col in this) {
					string s = GetColumnDependency (col);
					if (s != String.Empty)
						throw new ArgumentException (
								"Cannot remove this column, because it is part of the"+ s);
				}

			if (parentTable.DataSet != null)
				foreach (DataTable table in parentTable.DataSet.Tables)
					foreach (Constraint c in table.Constraints) {
						if (!(c is ForeignKeyConstraint) ||
						    ((ForeignKeyConstraint)c).RelatedTable != parentTable)
							continue;
						throw new ArgumentException (String.Format ("Cannot remove this column, " +
									"because it is part of the constraint {0} on " +
									"the table {1}", c.ConstraintName, table.TableName));
					}
			
			foreach (DataColumn col in this)
				col.ResetColumnInfo ();

			columnFromName.Clear();
			autoIncrement.Clear();
			base.List.Clear();
			OnCollectionChanged(e);
		}

		/// <summary>
		/// Checks whether the collection contains a column with the specified name.
		/// </summary>
		/// <param name="name">The ColumnName of the column to check for.</param>
		/// <returns>true if a column exists with this name; otherwise, false.</returns>
		public bool Contains(string name)
		{
			if (columnFromName.Contains(name))
				return true;
			
			return (IndexOf(name, false) != -1);
		}

		/// <summary>
		/// Gets the index of a column specified by name.
		/// </summary>
		/// <param name="column">The name of the column to return.</param>
		/// <returns>The index of the column specified by column if it is found; otherwise, -1.</returns>
		public virtual int IndexOf(DataColumn column)
		{
			if (column == null)
				return -1;
			return base.List.IndexOf(column);
		}

		/// <summary>
		/// Gets the index of the column with the given name (the name is not case sensitive).
		/// </summary>
		/// <param name="columnName">The name of the column to find.</param>
		/// <returns>The zero-based index of the column with the specified name, or -1 if the column doesn't exist in the collection.</returns>
		public int IndexOf(string columnName)
		{
			if (columnName == null)
				return -1;
			DataColumn dc = columnFromName[columnName] as DataColumn;
				
			if (dc != null)
				return IndexOf(dc);

			return IndexOf(columnName, false);
		}

		/// <summary>
		/// Raises the OnCollectionChanged event.
		/// </summary>
		/// <param name="ccevent">A CollectionChangeEventArgs that contains the event data.</param>
		protected virtual void OnCollectionChanged(CollectionChangeEventArgs ccevent)
		{
			parentTable.ResetPropertyDescriptorsCache();
			if (CollectionChanged != null) 
			{
				CollectionChanged(this, ccevent);
			}
		}

		/// <summary>
		/// Raises the OnCollectionChanging event.
		/// </summary>
		/// <param name="ccevent">A CollectionChangeEventArgs that contains the event data.</param>
		protected internal virtual void OnCollectionChanging(CollectionChangeEventArgs ccevent)
		{
			if (CollectionChanged != null) 
			{
				//FIXME: this is not right
				//CollectionChanged(this, ccevent);
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Removes the specified DataColumn object from the collection.
		/// </summary>
		/// <param name="column">The DataColumn to remove.</param>
		public void Remove(DataColumn column)
		{
			if (column == null)
				throw new ArgumentNullException ("column", "'column' argument cannot be null.");

			if (!Contains(column.ColumnName))
				throw new ArgumentException ("Cannot remove a column that doesn't belong to this table.");

			string dependency = GetColumnDependency (column);
			if (dependency != String.Empty)
				throw new ArgumentException ("Cannot remove this column, because it is part of " + dependency);

			CollectionChangeEventArgs e = new CollectionChangeEventArgs(CollectionChangeAction.Remove, this);
			
			int ordinal = column.Ordinal;
			UnregisterName(column.ColumnName);
			base.List.Remove(column);
			
			// Reset column info
			column.ResetColumnInfo ();
	
			//Update the ordinals
			for( int i = ordinal ; i < this.Count ; i ++ )
				this[i].SetOrdinal( i );

			if (parentTable != null)
				parentTable.OnRemoveColumn(column);

			if (column.AutoIncrement)
				autoIncrement.Remove(column);

			OnCollectionChanged(e);
		}

		/// <summary>
		/// Removes the DataColumn object with the specified name from the collection.
		/// </summary>
		/// <param name="name">The name of the column to remove.</param>
		public void Remove(string name)
		{
			DataColumn column = this[name];
			
			if (column == null)
				throw new ArgumentException ("Column '" + name + "' does not belong to table " + ( parentTable == null ? "" : parentTable.TableName ) + ".");
			Remove(column);
		}

		/// <summary>
		/// Removes the column at the specified index from the collection.
		/// </summary>
		/// <param name="index">The index of the column to remove.</param>
		public void RemoveAt(int index)
		{
			if (Count <= index)
				throw new IndexOutOfRangeException ("Cannot find column " + index + ".");

			DataColumn column = this[index];
			Remove(column);
		}

		// Helper AddRange() - Call this function when EndInit is called
		internal void PostEndInit() {
			DataColumn[] cols = _mostRecentColumns;
			_mostRecentColumns = null;
			AddRange (cols);
		}


		/// <summary>
		///  Do the same as Constains -method but case sensitive
		/// </summary>
		private bool CaseSensitiveContains(string columnName)
		{
			DataColumn column = this[columnName];
			
			if (column != null)
				return string.Compare(column.ColumnName, columnName, false) == 0; 

			return false;
		}

		internal void UpdateAutoIncrement(DataColumn col,bool isAutoIncrement)
		{
			if (isAutoIncrement)
			{
				if (!autoIncrement.Contains(col))
					autoIncrement.Add(col);
			}
			else
			{
				if (autoIncrement.Contains(col))
					autoIncrement.Remove(col);
			}
		}

		private int IndexOf (string name, bool error)
		{
			int count = 0, match = -1;
			for (int i = 0; i < List.Count; i++)
			{
				String name2 = ((DataColumn) List[i]).ColumnName;
				if (String.Compare (name, name2, true) == 0)
				{
					if (String.Compare (name, name2, false) == 0)
						return i;
					match = i;
					count++;
				}
			}
			if (count == 1)
				return match;
			if (count > 1 && error)
				throw new ArgumentException ("There is no match for the name in the same case and there are multiple matches in different case.");
			return -1;
		}
		
		#region Events

		/// <summary>
		/// Occurs when the columns collection changes, either by adding or removing a column.
		/// </summary>
                [ResDescriptionAttribute ("Occurs whenever this collection's membership changes.")] 
		public event CollectionChangeEventHandler CollectionChanged;

		#endregion 
	}
}
