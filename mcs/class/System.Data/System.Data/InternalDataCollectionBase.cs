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

		// Constructor
		[MonoTODO]
		public InternalDataCollectionBase() {
			// FIXME: TODO
		}
		
		public virtual int Count {
			[MonoTODO]
			get {
			}
		}

		public bool IsReadOnly {
			[MonoTODO]
			get {
			}
		}

		public bool IsSynchronized {
			[MonoTODO]
			get {
				
			}
		}

		public object SyncRoot {
			[MonoTODO]
			get {
				
			}
		}

		protected virtual ArrayList List {
			[MonoTODO]
			get {
			}
		}

		[MonoTODO]
		public void CopyTo(Array ar, int index) {

		}

		[MonoTODO]
		public IEnumerator GetEnumerator() {

		}

		[MonoTODO]
		~InternalDataCollectionBase() {

		}

	}

}
