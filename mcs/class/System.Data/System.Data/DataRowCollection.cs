//
// System.Data.DataRowCollection.cs
//
// Author:
//   Daniel Morgan <danmorg@sc.rr.com>
//   Tim Coleman <tim@timcoleman.com>
//
// (C) Ximian, Inc 2002
// (C) Copyright 2002 Tim Coleman
// (C) Copyright 2002 Daniel Morgan
//

using System;
using System.Collections;
using System.ComponentModel;

namespace System.Data
{
	/// <summary>
	/// Collection of DataRows in a DataTable
	/// </summary>
	[Serializable]
	public class DataRowCollection : InternalDataCollectionBase 
	{
		private DataTable table;

		/// <summary>
		/// Internal constructor used to build a DataRowCollection.
		/// </summary>
		internal DataRowCollection (DataTable table) : base ()
		{
			this.table = table;
		}

		/// <summary>
		/// Gets the row at the specified index.
		/// </summary>
		public DataRow this[int index] 
		{
			get { 
				if (index >= Count)
					throw new IndexOutOfRangeException ("There is no row at position " + index + ".");

				return (DataRow) list[index]; 
			}
		}

		/// <summary>
		/// This member overrides InternalDataCollectionBase.List
		/// </summary>
		protected override ArrayList List 
		{
			get { return list; }
		}		

		/// <summary>
		/// Adds the specified DataRow to the DataRowCollection object.
		/// </summary>
		public void Add (DataRow row) 
		{
			//TODO: AutoIncrement
			//TODO: validation
			list.Add (row);
			row.RowStateInternal = DataRowState.Added;
			row.Table.ChangedDataRow (row, DataRowAction.Add);
		}

		/// <summary>
		/// Creates a row using specified values and adds it to the DataRowCollection.
		/// </summary>
		public virtual DataRow Add (object[] values) 
		{
			DataRow row = table.NewRow ();
			row.ItemArray = values;
			Add (row);
			return row;
		}

		/// <summary>
		/// Clears the collection of all rows.
		/// </summary>
		[MonoTODO]
		public void Clear () 
		{
			list.Clear ();
		}

		/// <summary>
		/// Gets a value indicating whether the primary key of any row in the collection contains
		/// the specified value.
		/// </summary>
		[MonoTODO]
		public bool Contains (object key) 
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Gets a value indicating whether the primary key column(s) of any row in the 
		/// collection contains the values specified in the object array.
		/// </summary>
		[MonoTODO]
		public bool Contains (object[] keys) 
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Gets the row specified by the primary key value.
		/// </summary>
		[MonoTODO]
		public DataRow Find (object key) 
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Gets the row containing the specified primary key values.
		/// </summary>
		[MonoTODO]
		public DataRow Find (object[] keys) 
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Inserts a new row into the collection at the specified location.
		/// </summary>
		public void InsertAt (DataRow row, int pos) 
		{
			list.Insert (pos, row);
		}

		/// <summary>
		/// Removes the specified DataRow from the collection.
		/// </summary>
		public void Remove (DataRow row) 
		{
			list.Remove (row);
			table.DeletedDataRow (row, DataRowAction.Delete);
		}

		/// <summary>
		/// Removes the row at the specified index from the collection.
		/// </summary>
		public void RemoveAt (int index) 
		{			
			DataRow row = (DataRow)list [index];
			list.RemoveAt (index);			
			table.DeletedDataRow (row, DataRowAction.Delete);
		}

		///<summary>
		///Internal method used to validate a given DataRow with respect
		///to the DataRowCollection
		///</summary>
		[MonoTODO]
		internal void ValidateDataRowInternal(DataRow row)
		{
			//FIXME: this validates constraints in the order they appear
			//in the collection. Most probably we need to do it in a 
			//specific order like unique/primary keys first, then Foreignkeys, etc
			foreach(Constraint constraint in table.Constraints)
			{
				constraint.AssertConstraint(row);
			}

		}

	}
}
