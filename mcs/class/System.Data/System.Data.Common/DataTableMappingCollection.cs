//
// System.Data.Common.DataTableMappingCollection.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc
//

namespace System.Data.Common
{
	/// <summary>
	/// A collection of DataTableMapping objects. This class cannot be inherited.
	/// </summary>
	public sealed class DataTableMappingCollection :
		MarshalByRefObject, ITableMappingCollection, IList,
		ICollection, IEnumerable
	{
		[MonoTODO]
		public DataTableMappingCollection() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Add (object obj) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataTableMapping Add (string a, string b) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void AddRange(DataTableMapping[] values) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Clear() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool Contains(object) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool Contains(string) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CopyTo(Array array, int index) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataTableMapping GetByDataSetTable(string dataSetTable) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static DataTableMapping GetTableMappingBySchemaAction(
			DataTableMappingCollection tableMappings,
			string sourceTable,
			string dataSetTable,
			MissingMappingAction mappingAction) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int IndexOf(object) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int IndexOf(string) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int IndexOfDataSetTable(string dataSetTable) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Insert(int index, object value) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Remove(object value) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RemoveAt(int index) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RemoveAt(string index) {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public int Count {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public DataTableMapping this[int] {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public DataTableMapping this[string] {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	}
}
