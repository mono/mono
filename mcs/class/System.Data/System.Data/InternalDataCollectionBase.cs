//
// System.Data.InternalDataCollectionBase.cs
//
// Base class for:
//     DataRowCollection
//     DataColumnCollection
//     DataTableCollection
//     DataRelationCollection
//     DataConstraintCollection
//
// Author:
//   Daniel Morgan <danmorg@sc.rr.com>
//   Tim Coleman <tim@timcoleman.com>
//
// (C) Copyright 2002 Daniel Morgan
// (C) Copyright 2002 Tim Coleman
//

using System;
using System.Collections;
using System.ComponentModel;

namespace System.Data
{
	/// <summary>
	/// Base class for System.Data collection classes 
	/// that are used within a DataTable object
	/// to represent a collection of 
	/// relations, tables, rows, columns, and constraints
	/// </summary>

	public class InternalDataCollectionBase : ICollection, IEnumerable 
	{
		#region Fields

		private ArrayList list = null;
		private bool readOnly = false;
		private bool synchronized = false; 

		#endregion

		#region Constructors

		public InternalDataCollectionBase() 
		{
			list = new ArrayList();
		}

		#endregion

		#region Properties
	
		/// <summary>
		/// Gets the total number of elements in a collection.
		/// </summary>
		[Browsable (false)]
		public virtual int Count {
			get { return list.Count; }
		}

		/// <summary>
		/// Gets a value indicating whether the InternalDataCollectionBase is read-only.
		/// </summary>
		[Browsable (false)]
		public bool IsReadOnly {
			get { return readOnly; }
		}

		/// <summary>
		/// Gets a value indicating whether the InternalDataCollectionBase is synchronized.
		/// </summary>
		[Browsable (false)]
		public bool IsSynchronized {
			get { return synchronized; }
		}

		/// <summary>
		/// Gets the items of the collection as a list.
		/// </summary>
		protected virtual ArrayList List {
			get { return list; }
		}

		/// <summary>
		/// Gets an object that can be used to synchronize the collection.
		/// </summary>
		[Browsable (false)]
		public object SyncRoot {
			get {
				return this;
			}
		}


		#endregion

		#region Methods

		/// <summary>
		/// Copies all the elements in the current InternalDataCollectionBase to a one-
		/// dimensional Array, starting at the specified InternalDataCollectionBase index.
		/// </summary>
		public void CopyTo(Array ar, int index) 
		{
			list.CopyTo (ar, index);

		}

		/// <summary>
		/// Gets an IEnumerator for the collection.
		/// </summary>
		public IEnumerator GetEnumerator() 
		{
			return list.GetEnumerator ();
		}

		#endregion
	}
}
