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
			get { return (DataTable)(list[index]); }
		}

		public DataTable this[string name] {
			get { 
				foreach (DataTable dt in list) {
					if (dt.TableName == name)
						return dt;
				}
				
				return null;
			}			
		}

		protected override ArrayList List {
			get { return list; }
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
			if (table.TableName == null || table.TableName == string.Empty)
				NameTable (table);
				
			list.Add (table);
			table.dataSet = dataSet;
			OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Add, table));
		}

		public virtual DataTable Add (string name) 
		{
			DataTable table = new DataTable (name);
			this.Add (table);
			return table;
		}

		public void AddRange (DataTable[] tables) 
		{
			foreach (DataTable table in tables)
				this.Add (table);
		}

		[MonoTODO]
		public bool CanRemove (DataTable table) 
		{
			throw new NotImplementedException ();
		}

		public void Clear () 
		{
			list.Clear ();
		}

		public bool Contains (string name) 
		{
			foreach (DataTable dt in list) {
				if (dt.TableName == name)
					return true;
			}

			return false;
		}

		public virtual int IndexOf (DataTable table) 
		{
			return list.IndexOf (table);
		}

		public virtual int IndexOf (string name) 
		{
			return list.IndexOf (this [name]);
		}

		public void Remove (DataTable table) 
		{
			this.Remove (table.TableName);
			OnCollectionChanged (new CollectionChangeEventArgs (CollectionChangeAction.Remove, table));
		}

		public void Remove (string name) 
		{
			list.Remove (this [name]);
		}

		public void RemoveAt (int index) 
		{
			list.RemoveAt (index);
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

		#endregion // Private methods

		#region Events

		[ResDescriptionAttribute ("Occurs whenever this collection's membership changes.")]		
		public event CollectionChangeEventHandler CollectionChanged;

		public event CollectionChangeEventHandler CollectionChanging;

		#endregion
	}
}
