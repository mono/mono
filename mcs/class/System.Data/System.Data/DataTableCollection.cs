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

namespace System.Data {
	/// <summary>
	/// Represents the collection of tables for the DataSet.
	/// </summary>
	[DefaultEvent ("CollectionChanged")]
	[ListBindable (false)]
	[Serializable]
	public class DataTableCollection : InternalDataCollectionBase
	{
		DataSet dataSet;
		const string defaultTableName = "Table1";
		
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
			return this.Add (defaultTableName);
		}

		public virtual void Add (DataTable table) 
		{
			list.Add (table);
			table.dataSet = dataSet;
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

		#region Events

		[ResDescriptionAttribute ("Occurs whenever this collection's membership changes.")]		
		public event CollectionChangeEventHandler CollectionChanged;

		public event CollectionChangeEventHandler CollectionChanging;

		#endregion
	}
}
