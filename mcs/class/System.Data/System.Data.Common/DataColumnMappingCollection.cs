//
// System.Data.Common.DataColumnCollection
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc
//

using System;
using System.Collections;
using System.Data;

namespace System.Data.Common
{
	/// <summary>
	/// Contains a collection of DataColumnMapping objects. This class cannot be inherited.
	/// </summary>
	public sealed class DataColumnMappingCollection :
		MarshalByRefObject, IColumnMappingCollection, IList,
		ICollection, IEnumerable
	{
		private DataColumnMapping[] mappings = null;
		private int size = 0;
		
		public DataColumnMappingCollection () {
		}

		public int Add (object obj) {
			DataColumnMapping[] tmp = new DataColumnMapping[size + 1];

			Array.Copy (mappings, tmp, size);
			size++;
			mappings = tmp;
			mappings[size - 1] = obj;

			return size;
		}

		public void AddRange (DataColumnMapping[] values) {
			DataColumnMapping[] tmp = new DataColumnMapping[size + values.Length];

			Array.Copy (mappings, tmp, size);
			for (int i = 0; i < values.Length; i++) {
				tmp[i + size] = values[i];
			}
			
			size += values.Length;
			mappings = tmp;
		}

		public void Clear () {
			/* FIXME */
			for (int i = 0; i < size; i++)
				mappings[i] = null;

			size = 0;
		}

		public bool Contains (object obj) {
			for (int i = 0; i < size; i++) {
				if (obj.Equals (mappings[i]))
				    return true;
			}

			return false;
		}

		public void CopyTo (Array array, int index) {
			DataColumnMapping[] tmp = new DataColumnMapping[size];
			Array.Copy (mappings, tmp, size);
		}

		public DataColumnMapping GetByDataSetColumn (string value) {
			for (int i = 0; i < size; i++) {
				if (mappings[i].DataSetColumn == value)
					return mappings[i];
			}

			return null;
		}

		[MonoTODO]
		public static DataColumnMapping GetColumnMappingBySchemaAction (
			DataColumnMappingCollection columnMappings,
			string sourceColumn,
			MissingMappingAction mappingAction) {
			throw new NotImplementedException ();
		}

		public int IndexOf (object obj) {
			for (int i = 0; i < size; i++) {
				if (obj.Equals (mappings[i]))
					return i;
			}

			return -1;
		}

		public int IndexOfDataSetColumn (string value) {
			for (int i = 0; i < size; i++) {
				if (mappings[i].DataSetColumn == value)
					return i;
			}

			return -1;
		}

		[MonoTODO]
		public void Insert (int index, object value) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Remove (object value) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RemoveAt (int index) {
			throw new NotImplementedException ();
		}
		
		public int Count {
			get { return size; }
		}

		public DataColumnMapping this[int index] {
			get {
				if (index < size)
					return mappings[index];
				return null;
			}
			set {
				if (index < size)
					mappings[index] = value;
			}
		}
	}
}
