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
		}
		
		public int Count {
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
	}
}
