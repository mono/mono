//
// System.Data.ConstraintCollection.cs
//
// Author:
//   Daniel Morgan
//
// (C) Ximian, Inc. 2002
// (C) 2002 Daniel Morgan
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

		public virtual Constraint this[string name] {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}
		
		public virtual Constraint this[int index] {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		// Overloaded Add method (5 of them)
		// to add Constraint object to the collection

		[MonoTODO]
		public void Add(Constraint constraint) {
			
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual Constraint Add(string name,
			DataColumn column, bool primaryKey) {

			throw new NotImplementedException ();

		}

		[MonoTODO]
		public virtual Constraint Add(string name,
			DataColumn primaryKeyColumn,
			DataColumn foreignKeyColumn) {

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual Constraint Add(string name,
			DataColumn[] columns, bool primaryKey) {

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual Constraint Add(string name,
			DataColumn[] primaryKeyColumns,
			DataColumn[] foreignKeyColumns) {

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void AddRange(Constraint[] constraints) {

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool CanRemove(Constraint constraint) {

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Clear() {
			
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool Contains(string name) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int IndexOf(Constraint constraint) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual int IndexOf(string constraintName) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Remove(Constraint constraint) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Remove(string name) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RemoveAt(int index) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public event CollectionChangeEventHandler CollectionChanged;

		protected override ArrayList List {
			[MonoTODO]
			get{
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		protected virtual void OnCollectionChanged(
			CollectionChangeEventArgs ccevent) {

			throw new NotImplementedException ();
		}
	}
}
