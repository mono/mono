//
// System.Data.Common.DataTableMappingCollection.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc
// Copyright (C) Tim Coleman, 2002-2003
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.ComponentModel;

namespace System.Data.Common
{
	[ListBindable (false)]
	[EditorAttribute ("Microsoft.VSDesigner.Data.Design.DataTableMappingCollectionEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
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

		[Browsable (false)]
#if !NET_2_0
		[DataSysDescription ("The number of items in the collection")]
#endif
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int Count {
			get { return mappings.Count; }
		}

		[Browsable (false)]
#if !NET_2_0
		[DataSysDescription ("The specified DataTableMapping object")]
#endif
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
#if !NET_2_0
		[DataSysDescription ("The specified DataTableMapping object")]
#endif
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

		object ITableMappingCollection.this [string index] {
			get { return this [index]; }
			set {
				if (!(value is DataTableMapping))
					throw new ArgumentException ();
				this [index] = (DataTableMapping) value;
			}
		}

		#endregion // Properties

		#region Methods

		public int Add (object value)
		{
			if (!(value is System.Data.Common.DataTableMapping))
				throw new InvalidCastException ("The object passed in was not a DataTableMapping object.");

			sourceTables [((DataTableMapping) value).SourceTable] = value;
			dataSetTables [((DataTableMapping) value).DataSetTable] = value;
			return mappings.Add (value);
		}

		public DataTableMapping Add (string sourceTable, string dataSetTable) 
		{
			DataTableMapping mapping = new DataTableMapping (sourceTable, dataSetTable);
			Add (mapping);
			return mapping;
		}

#if NET_2_0
		public void AddRange (Array values)
		{
			for (int i = 0; i < values.Length; ++i)
				Add (values.GetValue (i));
		}
#endif

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

#if NET_2_0
		public void CopyTo (DataTableMapping[] array, int index) 
		{
			mappings.CopyTo (array, index);
		}
#endif

		public DataTableMapping GetByDataSetTable (string dataSetTable) 
		{
			// this should work case-insenstive.
			if (!(dataSetTables[dataSetTable] == null))
				return (DataTableMapping) (dataSetTables [dataSetTable]);
			else {
				string lowcasevalue = dataSetTable.ToLower ();
				object [] keyarray = new object [dataSetTables.Count];
				dataSetTables.Keys.CopyTo (keyarray, 0);
				for (int i=0; i<keyarray.Length; i++) {
					string temp = (string) keyarray [i];
					if (lowcasevalue.Equals (temp.ToLower ()))
						return (DataTableMapping) (dataSetTables [keyarray [i]]);
				}
				return null;
			}
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public static DataTableMapping GetTableMappingBySchemaAction (DataTableMappingCollection tableMappings, string sourceTable, string dataSetTable, MissingMappingAction mappingAction) 
		{
			if (tableMappings.Contains (sourceTable))
				return tableMappings[sourceTable];
			if (mappingAction == MissingMappingAction.Error)
				throw new InvalidOperationException (String.Format ("Missing source table mapping: '{0}'",
										    sourceTable));
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
			// this should work case-insensitive
			if (!(dataSetTables[dataSetTable] == null)) 
				return IndexOf ((DataTableMapping)(dataSetTables[dataSetTable]));
			else {
				string lowcasevalue = dataSetTable.ToLower();
				object [] keyarray = new object[dataSetTables.Count];
				dataSetTables.Keys.CopyTo(keyarray,0);
				for (int i=0; i<keyarray.Length; i++) {
					string temp = (string) keyarray[i];
					if (lowcasevalue.Equals(temp.ToLower()))
						return IndexOf ((DataTableMapping)(dataSetTables[keyarray[i]]));
				}
				return -1;
			}

		}

		public void Insert (int index, object value) 
		{
			mappings.Insert (index, value);
			sourceTables [((DataTableMapping) value).SourceTable] = value;
			dataSetTables [((DataTableMapping) value).DataSetTable] = value;
		}

#if NET_2_0
		public void Insert (int index, DataTableMapping value) 
		{
			mappings.Insert (index, value);
			sourceTables [value.SourceTable] = value;
			dataSetTables [value.DataSetTable] = value;
		}
#endif

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
			if (!(value is DataTableMapping))
				throw new InvalidCastException ();
			int index = mappings.IndexOf (value);
			if (index < 0 || index >= mappings.Count)
				throw new ArgumentException("There is no such element in collection.");
			mappings.Remove ((DataTableMapping) value);
		}

#if NET_2_0
		public void Remove (DataTableMapping value) 
		{
			int index = mappings.IndexOf (value);
			if (index < 0 || index >= mappings.Count)
				throw new ArgumentException("There is no such element in collection."); 
			mappings.Remove ((DataTableMapping) value);
		}
#endif

		public void RemoveAt (int index) 
		{
			 if (index < 0 || index >= mappings.Count)
				throw new IndexOutOfRangeException("There is no element in collection.");

			mappings.RemoveAt (index);
		}

		public void RemoveAt (string sourceTable) 
		{
			RemoveAt (mappings.IndexOf (sourceTables[sourceTable]));
		}

		#endregion // Methods
	}
}
