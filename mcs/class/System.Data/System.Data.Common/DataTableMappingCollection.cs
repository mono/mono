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
using System.ComponentModel;

namespace System.Data.Common {
	[ListBindable (false)]
	public sealed class DataTableMappingCollection : MarshalByRefObject, ITableMappingCollection, IList, ICollection, IEnumerable
	{
		#region Fields

		ArrayList mappings;
		Hashtable sourceTables;
		Hashtable dataSetTables;

		#endregion

		#region Constructors 

		public DataTableMappingCollection() 
		{
			mappings = new ArrayList ();
			sourceTables = new Hashtable ();
			dataSetTables = new Hashtable ();
		}

		#endregion // Constructors

		#region Properties

		[DataSysDescription ("The number of items in the collection")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int Count {
			get { return mappings.Count; }
		}

		[Browsable (false)]
		[DataSysDescription ("The specified DataTableMapping object")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public DataTableMapping this [int index] {
			get { return (DataTableMapping)(mappings[index]); }
			set { 
				DataTableMapping mapping = (DataTableMapping) mappings[index];
				sourceTables [mapping.SourceTable] = value;
				dataSetTables [mapping.DataSetTable] = value;
				mappings [index] = value; 
			}
		}

		[Browsable (false)]
		[DataSysDescription ("The specified DataTableMapping object")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public DataTableMapping this [string sourceTable] {
			get { return (DataTableMapping) sourceTables[sourceTable]; }
			set { this [mappings.IndexOf (sourceTables[sourceTable])] = value; }
		}
	
		object IList.this [int index] {
			get { return (object)(this[index]); }
			set { 
				if (!(value is DataTableMapping))
					throw new ArgumentException (); 
				this[index] = (DataTableMapping)value;
			 } 
		}

		bool ICollection.IsSynchronized {
			get { return mappings.IsSynchronized; }
		}

		object ICollection.SyncRoot {
			get { return mappings.SyncRoot; }
		}

		bool IList.IsFixedSize {
			get { return false; }
		}

		bool IList.IsReadOnly {
			get { return false; }
		}

		object ITableMappingCollection.this [string sourceTable] {
			get { return this [sourceTable]; }
			set { 
				if (!(value is DataTableMapping))
					throw new ArgumentException ();
				this [sourceTable] = (DataTableMapping) value;
			}
		}

		#endregion // Properties

		#region Methods

		public int Add (object value) 
		{
			if (!(value is System.Data.Common.DataTableMapping))
				throw new SystemException ("The object passed in was not a DataTableMapping object.");

			sourceTables[((DataTableMapping)value).SourceTable] = value;	
			dataSetTables[((DataTableMapping)value).DataSetTable] = value;	
			return mappings.Add (value);
		}

		public DataTableMapping Add (string sourceTable, string dataSetTable) 
		{
			DataTableMapping mapping = new DataTableMapping (sourceTable, dataSetTable);
			Add (mapping);
			return mapping;
		}

		public void AddRange (DataTableMapping[] values) 
		{
			foreach (DataTableMapping dataTableMapping in values)
				this.Add (dataTableMapping);
		}

		public void Clear () 
		{
			sourceTables.Clear ();
			dataSetTables.Clear ();
			mappings.Clear ();
		}

		public bool Contains (object value) 
		{
			return mappings.Contains (value);
		}

		public bool Contains (string value) 
		{
			return sourceTables.Contains (value);
		}

		public void CopyTo (Array array, int index) 
		{
			mappings.CopyTo (array, index);
		}

		public DataTableMapping GetByDataSetTable (string dataSetTable) 
		{
			return (DataTableMapping)(dataSetTables[dataSetTable]);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public static DataTableMapping GetTableMappingBySchemaAction (DataTableMappingCollection tableMappings, string sourceTable, string dataSetTable, MissingMappingAction mappingAction) 
		{
			if (tableMappings.Contains (sourceTable))
				return tableMappings[sourceTable];
			if (mappingAction == MissingMappingAction.Error)
				throw new InvalidOperationException ();
			if (mappingAction == MissingMappingAction.Ignore)
				return null;
			return new DataTableMapping (sourceTable, dataSetTable);
		}

		public IEnumerator GetEnumerator ()
		{
			return mappings.GetEnumerator ();
		}

		public int IndexOf (object value) 
		{
			return mappings.IndexOf (value);
		}

		public int IndexOf (string sourceTable) 
		{
			return IndexOf (sourceTables[sourceTable]);
		}

		public int IndexOfDataSetTable (string dataSetTable) 
		{
			return IndexOf ((DataTableMapping)(dataSetTables[dataSetTable]));
		}

		public void Insert (int index, object value) 
		{
			mappings.Insert (index, value);
		}

		ITableMapping ITableMappingCollection.Add (string sourceTableName, string dataSetTableName)
		{
			ITableMapping tableMapping = new DataTableMapping (sourceTableName, dataSetTableName);
			Add (tableMapping);
			return tableMapping;
		}

		ITableMapping ITableMappingCollection.GetByDataSetTable (string dataSetTableName)
		{
			return this [mappings.IndexOf (dataSetTables [dataSetTableName])];
		}

		public void Remove (object value) 
		{
			mappings.Remove ((DataTableMapping) value);
		}

		public void RemoveAt (int index) 
		{
			mappings.RemoveAt (index);
		}

		public void RemoveAt (string sourceTable) 
		{
			RemoveAt (mappings.IndexOf (sourceTables[sourceTable]));
		}

		#endregion // Methods
	}
}
