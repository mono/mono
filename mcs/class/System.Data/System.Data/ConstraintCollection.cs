//
// System.Data.ConstraintCollection.cs
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
	/// hold collection of constraints for data table
	/// </summary>
	public class ConstraintCollection : InternalDataCollectionBase {

		[MonoTODO]
		[Serializable]
		public virtual Constraint this[string name] {
			[MonoTODO]
			get {
			}
		}

		[MonoTODO]
		[Serializable]
		public virtual Constraint this[int index] {
			[MonoTODO]
			get {
			}
		}

		// Overloaded Add method (5 of them)
		// to add Constraint object to the collection

		[Serializable]
		[MonoTODO]
		public void Add(Constraint constraint) {
		}

		[Serializable]
		[MonoTODO]
		public virtual Constraint Add(string name,
			DataColumn column, bool primaryKey) {
		}

		[Serializable]
		[MonoTODO]
		public virtual Constraint Add(string name,
			DataColumn primaryKeyColumn,
			DataColumn foreignKeyColumn) {
		}

		[Serializable]
		[MonoTODO]
		public virtual Constraint Add(string name,
			DataColumn[] columns, bool primaryKey) {
		}

		[Serializable]
		[MonoTODO]
		public virtual Constraint Add(string name,
			DataColumn[] primaryKeyColumns,
			DataColumn[] foreignKeyColumns) {
		}

		[Serializable]
		[MonoTODO]
		public void AddRange(Constraint[] constraints) {
		}

		[Serializable]
		[MonoTODO]
		public bool CanRemove(Constraint constraint) {
		}

		[Serializable]
		[MonoTODO]
		public void Clear() {
		}

		[Serializable]
		[MonoTODO]
		public bool Contains(string name) {
		}

		[Serializable]
		[MonoTODO]
		public int IndexOf(Constraint constraint) {
		}

		[Serializable]
		[MonoTODO]
		public virtual int IndexOf(string constraintName) {
		}

		[Serializable]
		[MonoTODO]
		public void Remove(Constraint constraint) {
		}

		[Serializable]
		[MonoTODO]
		public void Remove(string name) {
		}

		[Serializable]
		[MonoTODO]
		public void RemoveAt(int index) {
		}

		/*
		 * FIXME: fix this event
		[Serializable]
		[MonoTODO]
		public event CollectionChangeEventHandler CollectionChanged;
		*/

		[Serializable]
		protected override ArrayList List {
			[MonoTODO]
			get{
			}
		}

		[Serializable]
		[MonoTODO]
		protected virtual void OnCollectionChanged(
			CollectionChangeEventArgs ccevent) {
		}

	}
}
