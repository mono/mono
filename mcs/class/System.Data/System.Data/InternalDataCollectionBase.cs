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
//   Daniel Morgan
//
// (C) Ximian, Inc. 2002
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
	public class InternalDataCollectionBase : ICollection, IEnumerable {

		// Fields
		private ArrayList list;
		private bool readOnly = false;
		private bool synchronized = false; 

		// Constructor
		[MonoTODO]
		public InternalDataCollectionBase() {
			// FIXME: TODO
			list = new ArrayList();
		}
		
		public virtual int Count {
			[MonoTODO]
			get {
				return list.Count;
			}
		}

		public bool IsReadOnly {
			[MonoTODO]
			get {
				return readOnly;
			}
		}

		public bool IsSynchronized {
			[MonoTODO]
			get {
				return synchronized;
			}
		}

		public object SyncRoot {
			[MonoTODO]
			get {
				// FIXME: how do we sync?	
			}
		}

		protected virtual ArrayList List {
			[MonoTODO]
			get {
				return list;
			}
		}

		[MonoTODO]
		public void CopyTo(Array ar, int index) {

		}

		[MonoTODO]
		public IEnumerator GetEnumerator() {

		}

		//[MonoTODO]
		//~InternalDataCollectionBase() {
                //
		//}

	}

}
