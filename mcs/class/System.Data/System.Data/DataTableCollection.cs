//
// System.Data.DataTableCollection.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

using System;

namespace System.Data
{
	/// <summary>
	/// Represents the collection of tables for the DataSet.
	/// </summary>
	public class DataTableCollection : InternalDataCollectionBase
	{
		private DataTable[] tables = null;
		private int size = 0;

		public virtual DataTable Add () {
			DataTable table = new DataTable ();
			DataTable[] tmp = new DataTable[size + 1];

			if (size > 0)
				Array.Copy (tables, tmp, size);
			size++;
			tables = tmp;
			tables[size - 1] = table;

			return table;
		}

		public virtual void Add (DataTable table) {
			DataTable[] tmp = new DataTable[size + 1];

			Array.Copy (tables, tmp, size);
			size++;
			tables = tmp;
			tables[size - 1] = table;
		}

		public virtual DataTable Add (string name) {
			DataTable table = this.Add ();
			table.TableName = name;

			return table;
		}

		public void AddRange (DataTable[] tables_to_add) {
			for (int i = 0; i < tables_to_add.Length; i++) {
				this.Add (tables_to_add[i]);
			}
		}

		[MonoTODO]
		[Serializable]
		public bool CanRemove (DataTable table) {
			throw new NotImplementedException ();
		}

		public void Clear () {
			/* FIXME: is this correct? */
			tables = null;
			size = 0;
		}

		public bool Contains (string name) {
			for (int i = 0; i < size; i++) {
				if (tables[i].TableName == name)
					return true;
			}

			return false;
		}

		public void CopyTo (Array ar, int index) {
			Array.Copy (tables, ar, size);
		}

		public virtual int IndexOf (DataTable table) {
			for (int i = 0; i < size; i++) {
				if (tables[i] == table)
					return i;
			}

			return -1;
		}

		public virtual int IndexOf (string name) {
			for (int i = 0; i < size; i++) {
				if (tables[i].TableName == name)
					return i;
			}

			return -1;
		}

		public void Remove (DataTable table) {
			this.RemoveAt (this.IndexOf (table));
		}

		public void Remove (string name) {
			this.RemoveAt (this.IndexOf (name));
		}

		[MonoTODO]
		public void RemoveAt (int index) {
			throw new NotImplementedException ();
		}
		
		public override int Count {
			get { return size; }
		}

		// IsReadOnly and IsSynchronized must be implemented or
		// is it safe to use InternalDataCollectionBase's

		public DataTable this[int index] {
			get {
				if (index < size)
					return tables[index];
				return null;
			}
		}

		public DataTable this[string name] {
			get {
				for (int i = 0; i < size; i++) {
					if (tables[i].TableName == name)
						return tables[i];
				}

				return null;
			}
		}

		[MonoTODO]
		protected override ArrayList List {
			throw new NotImplementedException ();
		}
		
		[Serializable]
		public event CollectionChangeEventHandler CollectionChanged;

		[Serializable]
		public event CollectionChangeEventHandler CollectionChanging;

		[Serializable]
		protected virtual void OnCollectionChanged (
			CollectionChangeEventArgs ccevent) {
		}

		[Serializable]
		protected internal virtual void OnCollectionChanging (
			CollectionChangeEventArgs ccevent) {
		}
	}
}
