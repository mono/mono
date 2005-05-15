//
// System.Data.DataTableCollection.cs
//
// Authors:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//   Tim Coleman <tim@timcoleman.com>
//
// (C) Chris Podurgiel
// (C) Copyright 2002 Tim Coleman
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
using System.ComponentModel;
using System.Globalization;

namespace System.Data {
	/// <summary>
	/// Represents the collection of tables for the DataSet.
	/// </summary>
	[Editor]
	[DefaultEvent ("CollectionChanged")]
	[ListBindable (false)]
	[Serializable]
	public class DataTableCollection : InternalDataCollectionBase
	{
		DataSet dataSet;
		
		#region Constructors 

		internal DataTableCollection (DataSet dataSet)
			: base ()
		{
			this.dataSet = dataSet;
		}
		
		#endregion
		
		#region Properties

		public DataTable this[int index] {
			get {
				if (index < 0 || index >= List.Count)
					throw new IndexOutOfRangeException(String.Format("Cannot find table {0}", index));
				return (DataTable)(List[index]);
			}
		}

		public DataTable this[string name] {
			get { 
				int index = IndexOf (name, true);
				return index < 0 ? null : (DataTable) List[index];
			}
		}

#if NET_2_0
		[MonoTODO]
		public DataTable this [string name, string tbNamespace] {
			get { throw new NotImplementedException (); }
		}
#endif

		protected override ArrayList List {
			get { return base.List; }
		}

		#endregion
	
		#region Methods	

		public virtual DataTable Add () 
		{
			DataTable Table = new DataTable ();
			Add (Table);
			return Table;
		}

		public virtual void Add (DataTable table) 
		{
			OnCollectionChanging (new CollectionChangeEventArgs (CollectionChangeAction.Add, table));
			// check if the reference is a null reference
			if(table == null)
				throw new ArgumentNullException("table");
            
			// check if the list already contains this tabe.
			if(List.Contains(table))
				throw new ArgumentException("DataTable already belongs to this DataSet.");
            
			// if the table name is null or empty string.
			// give her a name. 
			if (table.TableName == null || table.TableName == string.Empty)
				NameTable (table);
		    
			// check if the collection has a table with the same name.
			int tmp = IndexOf(table.TableName);
			// if we found a table with same name we have to check
			// that it is the same case.
			// indexof can return a table with different case letters.
			if (tmp != -1)
			{
				if(table.TableName == this[tmp].TableName)
					throw new DuplicateNameException("A DataTable named '" + table.TableName + "' already belongs to this DataSet.");
			}
	
			List.Add (table);
			table.dataSet = dataSet;
			OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Add, table));
		}

		public virtual DataTable Add (string name) 
		{
			DataTable table = new DataTable (name);
			this.Add (table);
			return table;
		}

#if NET_2_0
		public virtual DataTable Add (string name, string tbNamespace)
		{
			DataTable table = new DataTable (name, tbNamespace);
			this.Add (table);
			return table;
		}
#endif

		public void AddRange (DataTable[] tables) {
			if (tables == null)
				return;

			for (int i = 0; i < tables.Length; i++){
				DataTable table = tables[i];
				if (table != null)
					this.Add (table);
			}
		}

		public bool CanRemove (DataTable table) 
		{
			return CanRemove(table, false);
		}

		public void Clear () 
		{
			List.Clear ();
		}

		public bool Contains (string name) 
		{
			return (-1 != IndexOf (name, false));
		}

		public virtual int IndexOf (DataTable table) 
		{
			return List.IndexOf (table);
		}

		public virtual int IndexOf (string name) 
		{
			return IndexOf (name, false);
		}

		public void Remove (DataTable table) 
		{
			OnCollectionChanging (new CollectionChangeEventArgs (CollectionChangeAction.Remove, table));
			if (CanRemove(table, true))
				table.dataSet = null;

			List.Remove(table);
			table.dataSet = null;
			OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Remove, table));
		}

		public void Remove (string name) 
		{
			Remove (this [name]);
		}

		public void RemoveAt (int index) 
		{
			Remove(this[index]);
		}

		#endregion

		#region Protected methods

		protected internal virtual void OnCollectionChanging (CollectionChangeEventArgs Args)
		{
			if (CollectionChanging != null)
				CollectionChanging (this, Args);
		}

		protected virtual void OnCollectionChanged (CollectionChangeEventArgs Args)
		{
			if (CollectionChanged != null)
				CollectionChanged (this, Args);
		}

		#endregion

		#region Private methods

		private int IndexOf (string name, bool error)
		{
			int count = 0, match = -1;
			for (int i = 0; i < List.Count; i++)
			{
				String name2 = ((DataTable) List[i]).TableName;
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

		/// <summary>
		/// gives name to Table (Table1, Table2, Table3,...)
		/// </summary>
		private void NameTable (DataTable Table)
		{
			string Name = "Table";
			int i = 1;
			while (Contains (Name + i))
				i++;

			Table.TableName = Name + i;			       
		}
		
		// check if a table can be removed from this collectiuon.
		// if the table can not be remved act according to throwException parameter.
		// if it is true throws an Exception, else return false.
		private bool CanRemove(DataTable table, bool throwException)
		{

			// check if table is null reference
			if (table == null)
			{
				if(throwException)
					throw new ArgumentNullException("table");
				return false;
			}

			// check if the table doesnot belong to this collection
			bool tableexists = true;
			int tmp = IndexOf(table.TableName);
                        // if we found a table with same name we have to check
                        // that it is the same case.
                        // indexof can return a table with different case letters.
                        if (tmp != -1)
			{
				 if(table.TableName != this[tmp].TableName)
					tableexists = false;
			}
			else
				tableexists = false;

			if (!tableexists)
			{
				if(throwException)
                                        throw new ArgumentException("Table does not exist in collection");
                                return false;

			}		
				
						
			// check if the table has the same DataSet as this collection.
			if(table.DataSet != this.dataSet)
			{
				if(throwException)
					throw new ArgumentException("Table " + table.TableName + " does not belong to this DataSet.");
				return false;
			}
			
			// check the table has a relation attached to it.
			if (table.ParentRelations.Count > 0 || table.ChildRelations.Count > 0)
			{
				if(throwException)
					throw new ArgumentException("Cannot remove a table that has existing relations. Remove relations first.");
				return false;
			}
			

			// now we check if any ForeignKeyConstraint is referncing 'table'.
			IEnumerator tableEnumerator = this.dataSet.Tables.GetEnumerator();
			
			// loop on all tables in dataset
			while (tableEnumerator.MoveNext())
			{
				IEnumerator constraintEnumerator = ((DataTable) tableEnumerator.Current).Constraints.GetEnumerator();
				// loop on all constrains in the current table
				while (constraintEnumerator.MoveNext())
				{
					Object o = constraintEnumerator.Current;
					// we only check ForeignKeyConstraint
					if (o is ForeignKeyConstraint)
					{
						ForeignKeyConstraint fc = (ForeignKeyConstraint) o;
						if(fc.Table == table || fc.RelatedTable == table)
						{
							if(throwException)
								throw new ArgumentException("Cannot remove table " + table.TableName + ", because it referenced in ForeignKeyConstraint " + fc.ConstraintName + ". Remove the constraint first.");
							return false;
						}
					}
				}
			}

			return true;
		}

		#endregion // Private methods

		#region Events

		[ResDescriptionAttribute ("Occurs whenever this collection's membership changes.")]		
		public event CollectionChangeEventHandler CollectionChanged;

		public event CollectionChangeEventHandler CollectionChanging;

		#endregion
	}
}
