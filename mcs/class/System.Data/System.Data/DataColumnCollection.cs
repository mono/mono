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

using System;
using System.Collections;
using System.ComponentModel;

namespace System.Data {
	[Editor]
	[Serializable]
	[DefaultEvent ("CollectionChanged")]
	public class DataColumnCollection : InternalDataCollectionBase
	{
		//table should be the DataTable this DataColumnCollection belongs to.
		private DataTable parentTable = null;

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
				foreach (DataColumn column in base.List)
				{					
					if (String.Compare (column.ColumnName, name, true) == 0)
					{
						return column;
					}
				}
				return null;                
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
			//FIXME:
			string defaultName = GetNextDefaultColumnName ();
			DataColumn column = new DataColumn (defaultName);
			CollectionChangeEventArgs e = new CollectionChangeEventArgs(CollectionChangeAction.Add, this);
			
			column.SetTable(parentTable);
			base.List.Add(column);
			
			column.SetOrdinal(Count - 1);
			OnCollectionChanged(e);
			return column;
		}

		private string GetNextDefaultColumnName ()
		{
			string defColumnName = "Column1";
			for (int index = 2; Contains (defColumnName); ++index) {
				defColumnName = "Column" + index;
			}
			return defColumnName;
		}

		/// <summary>
		/// Creates and adds the specified DataColumn object to the DataColumnCollection.
		/// </summary>
		/// <param name="column">The DataColumn to add.</param>
		[MonoTODO]
		public void Add(DataColumn column)
		{

			if (column == null)
				throw new ArgumentNullException ("column", "'column' argument cannot be null.");

			if (column.ColumnName.Equals(String.Empty))
			{
				column.ColumnName = GetNextDefaultColumnName ();
			}
			else if (Contains(column.ColumnName))
			{
				if (object.ReferenceEquals(this [column.ColumnName], column))
					throw new ArgumentException ("Column '" + column.ColumnName + "' already belongs to this DataTable.");
				else
					throw new DuplicateNameException("A column named '" + column.ColumnName + "' already belongs to this DataTable.");
			}

			if (column.Table != null)
				throw new ArgumentException ("Column '" + column.ColumnName + "' already belongs to another DataTable.");

			CollectionChangeEventArgs e = new CollectionChangeEventArgs(CollectionChangeAction.Add, this);

			column.SetTable (parentTable);
			int ordinal = base.List.Add(column);
			column.SetOrdinal (ordinal);

			//add constraints if neccesary

			if (column.Unique)
			{
				UniqueConstraint uc = new UniqueConstraint(column);
				parentTable.Constraints.Add(uc);
			}

			//TODO: add missing constraints. i.e. Primary/Foreign keys

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
				columnName = GetNextDefaultColumnName ();
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
			foreach (DataColumn column in columns)
			{
				Add(column);
			}
			return;
		}

		/// <summary>
		/// Checks whether a given column can be removed from the collection.
		/// </summary>
		/// <param name="column">A DataColumn in the collection.</param>
		/// <returns>true if the column can be removed; otherwise, false.</returns>
		public bool CanRemove(DataColumn column)
		{
						
			//Check that the column does not have a null reference.
			if (column == null)
			{
                return false;
			}

			
			//Check that the column is part of this collection.
			if (!Contains(column.ColumnName))
			{
				return false;
			}


			
			//Check if this column is part of a relationship. (this could probably be written better)
			foreach (DataRelation childRelation in parentTable.ChildRelations)
			{
				foreach (DataColumn childColumn in childRelation.ChildColumns)
				{
					if (childColumn == column)
					{
						return false;
					}
				}

				foreach (DataColumn parentColumn in childRelation.ParentColumns)
				{
					if (parentColumn == column)
					{
						return false;
					}
				}
			}

			//Check if this column is part of a relationship. (this could probably be written better)
			foreach (DataRelation parentRelation in parentTable.ParentRelations)
			{
				foreach (DataColumn childColumn in parentRelation.ChildColumns)
				{
					if (childColumn == column)
					{
						return false;
					}
				}

				foreach (DataColumn parentColumn in parentRelation.ParentColumns)
				{
					if (parentColumn == column)
					{
						return false;
					}
				}
			}

			
			//Check if another column's expression depends on this column.
			
			foreach (DataColumn dataColumn in List)
			{
				if (dataColumn.Expression.ToString().IndexOf(column.ColumnName) > 0)
				{
					return false;
				}
			}
			
			//TODO: check constraints

			return true;
		}

		/// <summary>
		/// Clears the collection of any columns.
		/// </summary>
		public void Clear()
		{
			CollectionChangeEventArgs e = new CollectionChangeEventArgs(CollectionChangeAction.Refresh, this);


			// FIXME: Hmm... This loop could look little nicer :)
			foreach (DataColumn Col in List) {

				foreach (DataRelation Rel in Col.Table.ParentRelations) {

					foreach (DataColumn Col2 in Rel.ParentColumns) {
						if (Object.ReferenceEquals (Col, Col2))
							throw new ArgumentException ("Cannot remove this column, because " + 
										     "it is part of the parent key for relationship " + 
										     Rel.RelationName + ".");
					}

					foreach (DataColumn Col2 in Rel.ChildColumns) {
						if (Object.ReferenceEquals (Col, Col2))
							throw new ArgumentException ("Cannot remove this column, because " + 
										     "it is part of the parent key for relationship " + 
										     Rel.RelationName + ".");

					}
				}

				foreach (DataRelation Rel in Col.Table.ChildRelations) {

					foreach (DataColumn Col2 in Rel.ParentColumns) {
						if (Object.ReferenceEquals (Col, Col2))
							throw new ArgumentException ("Cannot remove this column, because " + 
										     "it is part of the parent key for relationship " + 
										     Rel.RelationName + ".");
					}

					foreach (DataColumn Col2 in Rel.ChildColumns) {
						if (Object.ReferenceEquals (Col, Col2))
							throw new ArgumentException ("Cannot remove this column, because " + 
										     "it is part of the parent key for relationship " + 
										     Rel.RelationName + ".");
					}
				}

			}
					
			base.List.Clear();
			OnCollectionChanged(e);
			return;
		}

		/// <summary>
		/// Checks whether the collection contains a column with the specified name.
		/// </summary>
		/// <param name="name">The ColumnName of the column to check for.</param>
		/// <returns>true if a column exists with this name; otherwise, false.</returns>
		public bool Contains(string name)
		{
			return (IndexOf(name) != -1);
		}

		/// <summary>
		/// Gets the index of a column specified by name.
		/// </summary>
		/// <param name="column">The name of the column to return.</param>
		/// <returns>The index of the column specified by column if it is found; otherwise, -1.</returns>
		public virtual int IndexOf(DataColumn column)
		{
			return base.List.IndexOf(column);
		}

		/// <summary>
		/// Gets the index of the column with the given name (the name is not case sensitive).
		/// </summary>
		/// <param name="columnName">The name of the column to find.</param>
		/// <returns>The zero-based index of the column with the specified name, or -1 if the column doesn't exist in the collection.</returns>
		public int IndexOf(string columnName)
		{
			
			DataColumn column = this[columnName];
			
			if (column != null)
			{
				return IndexOf(column);
			}
			else
			{
				return -1;
			}
		}

		/// <summary>
		/// Raises the OnCollectionChanged event.
		/// </summary>
		/// <param name="ccevent">A CollectionChangeEventArgs that contains the event data.</param>
		protected virtual void OnCollectionChanged(CollectionChangeEventArgs ccevent)
		{
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
			if (IndexOf (column) == -1)
				throw new ArgumentException ("Cannot remove a column that doesn't belong to this table.");

			//TODO: can remove first with exceptions
			//and OnChanging Event
			CollectionChangeEventArgs e = new CollectionChangeEventArgs(CollectionChangeAction.Remove, this);
			
			int ordinal = column.Ordinal;
			base.List.Remove(column);
			
			//Update the ordinals
			for( int i = ordinal ; i < this.Count ; i ++ )
			{
				this[i].SetOrdinal( i );
			}
			
			OnCollectionChanged(e);
			return;
		}

		/// <summary>
		/// Removes the DataColumn object with the specified name from the collection.
		/// </summary>
		/// <param name="name">The name of the column to remove.</param>
		public void Remove(string name)
		{
			DataColumn column = this[name];
			
			if (column == null)
				throw new ArgumentException ("Column '" + name + "' does not belong to table test_table.");

			Remove( column );
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
			Remove( column );
		}

		/// <summary>
		/// Occurs when the columns collection changes, either by adding or removing a column.
		/// </summary>
		public event CollectionChangeEventHandler CollectionChanged;
	}
}
