//
// System.Data.Common.DataTableMappingCollection.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc
// Copyright (C) 2002 Tim Coleman
//

using System;
using System.Collections;

namespace System.Data.Common
{
	/// <summary>
	/// A collection of DataTableMapping objects. This class cannot be inherited.
	/// </summary>
	public sealed class DataTableMappingCollection :
	MarshalByRefObject, // ITableMappingCollection, IList,
	        IEnumerable //ICollection, 
	{
		private ArrayList mappingList;
		private ArrayList sourceTableList;
		private ArrayList dataSetTableList;

		public DataTableMappingCollection() 
		{
			sourceTableList = new ArrayList ();
			dataSetTableList = new ArrayList ();
			mappingList = new ArrayList ();
		}

		public int Add (object value) 
		{
			if (!(value is System.Data.Common.DataTableMapping))
				throw new SystemException ("The object passed in was not a DataTableMapping object.");

			string sourceTable = ((DataTableMapping)value).SourceTable;
			string dataSetTable = ((DataTableMapping)value).DataSetTable;
			mappingList.Add (value);
			dataSetTableList.Add (dataSetTable);
			return sourceTableList.Add (sourceTable);
		}

		public DataTableMapping Add (string sourceTable, string dataSetTable) 
		{
			DataTableMapping dataTableMapping = new DataTableMapping (sourceTable, dataSetTable);

			mappingList.Add (dataTableMapping);
			sourceTableList.Add (sourceTable);
			dataSetTableList.Add (dataSetTable);

			return dataTableMapping ;
		}

		public void AddRange(DataTableMapping[] values) 
		{
			foreach (DataTableMapping dataTableMapping in values)
				this.Add (dataTableMapping);
		}

		public void Clear() 
		{
			sourceTableList.Clear ();
			dataSetTableList.Clear ();
			mappingList.Clear ();
		}

		public bool Contains (object value) 
		{
			return mappingList.Contains (value);
		}

		public bool Contains (string value) 
		{
			return sourceTableList.Contains (value);
		}

		[MonoTODO]
		public void CopyTo(Array array, int index) 
		{
			throw new NotImplementedException ();
		}

		public DataTableMapping GetByDataSetTable (string dataSetTable) 
		{
			return (DataTableMapping)mappingList[dataSetTableList.IndexOf(dataSetTable)];
		}

		[MonoTODO]
		public static DataTableMapping GetTableMappingBySchemaAction (DataTableMappingCollection tableMappings, string sourceTable, string dataSetTable, MissingMappingAction mappingAction) 
		{
			throw new NotImplementedException ();
		}

		public int IndexOf (object value) 
		{
			return mappingList.IndexOf (value);
		}

		public int IndexOf (string value) 
		{
			return sourceTableList.IndexOf (value);
		}

		public int IndexOfDataSetTable (string dataSetTable) 
		{
			return dataSetTableList.IndexOf (dataSetTable);
		}

		[MonoTODO]
		public void Insert (int index, object value) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Remove (object value) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RemoveAt (int index) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RemoveAt (string index) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public int Count 
		{
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public DataTableMapping this[int i] {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public DataTableMapping this[string s] {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public IEnumerator GetEnumerator ()
		{
			throw new NotImplementedException ();
		}
	}
}
