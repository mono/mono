//
// System.Data.Common.DataColumnMappingCollection
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;

namespace System.Data.Common {
	public sealed class DataColumnMappingCollection : MarshalByRefObject, IColumnMappingCollection , IList, ICollection, IEnumerable
	{
		#region Fields

		ArrayList list;
		Hashtable sourceColumns;
		Hashtable dataSetColumns;

		#endregion // Fields

		#region Constructors 

		public DataColumnMappingCollection () 
		{
			list = new ArrayList ();
			sourceColumns = new Hashtable ();
			dataSetColumns = new Hashtable ();
		}

		#endregion // Constructors

		#region Properties

		[Browsable (false)]
		[DataSysDescription ("The number of items in the collection")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int Count {
			get { return list.Count; }
		}

		[Browsable (false)]
		[DataSysDescription ("The specified DataColumnMapping object.")]
		public DataColumnMapping this [int index] {
			get { return (DataColumnMapping)(list[index]); }
			set { 
				DataColumnMapping mapping = (DataColumnMapping)(list[index]);
				sourceColumns[mapping] = value;
				dataSetColumns[mapping] = value;
				list[index] = value;
			}
		}

		public DataColumnMapping this [string sourceColumn] {
			get { return (DataColumnMapping) sourceColumns[sourceColumn]; }
			set { this [list.IndexOf (sourceColumns[sourceColumn])] = value; }
		}

                object ICollection.SyncRoot {
                        get { return list.SyncRoot; }
                }

                bool ICollection.IsSynchronized {
                        get { return list.IsSynchronized; }
                }

		object IColumnMappingCollection.this [string sourceColumn] {
			get { return this [sourceColumn]; }
			set {
				if (!(value is DataColumnMapping))
					throw new ArgumentException ();
				this [sourceColumn] = (DataColumnMapping) value;
			}
		}

                object IList.this [int index] {
                        get { return this[index]; }
                        set {
                                if (!(value is DataColumnMapping))
                                        throw new ArgumentException ();
                                this [index] = (DataColumnMapping) value;
                         }
                }

                bool IList.IsReadOnly {
                        get { return false; }
                }

                bool IList.IsFixedSize {
                        get { return false; }
                }
		
		#endregion // Properties

		#region Methods

		public int Add (object value)
		{
			if (!(value is DataColumnMapping))
				throw new InvalidCastException ();

			list.Add (value);
			sourceColumns[((DataColumnMapping)value).SourceColumn] = value;
			dataSetColumns[((DataColumnMapping)value).DataSetColumn] = value;
			return list.IndexOf (value);
		}

		public DataColumnMapping Add (string sourceColumn, string dataSetColumn)
		{
			DataColumnMapping mapping = new DataColumnMapping (sourceColumn, dataSetColumn);
			Add (mapping);
			return mapping;
		}

		public void AddRange (DataColumnMapping[] values) 
		{
			foreach (DataColumnMapping mapping in values)
				Add (mapping);
		}

		public void Clear () 
		{
			list.Clear ();
		}

		public bool Contains (object value) 
		{
			return (list.Contains (value));
		}

		public bool Contains (string value)
		{
			return (sourceColumns.Contains (value));
		}

		public void CopyTo (Array array, int index) 
		{
			((DataColumn[])(list.ToArray())).CopyTo (array, index);
		}

		public DataColumnMapping GetByDataSetColumn (string value) 
		{
			return (DataColumnMapping)(dataSetColumns[value]);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public static DataColumnMapping GetColumnMappingBySchemaAction (DataColumnMappingCollection columnMappings, string sourceColumn, MissingMappingAction mappingAction) 
		{
			if (columnMappings.Contains (sourceColumn))
				return columnMappings[sourceColumn];
			if (mappingAction == MissingMappingAction.Ignore)
				return null;
			if (mappingAction == MissingMappingAction.Error)
				throw new InvalidOperationException (String.Format ("Missing SourceColumn mapping for '{0}'", sourceColumn));
			return new DataColumnMapping (sourceColumn, sourceColumn);
		}

		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		IColumnMapping IColumnMappingCollection.Add (string sourceColumnName, string dataSetColumnName)
		{
			return Add (sourceColumnName, dataSetColumnName);
		}

		IColumnMapping IColumnMappingCollection.GetByDataSetColumn (string dataSetColumnName)
		{
			return GetByDataSetColumn (dataSetColumnName);
		}

		public int IndexOf (object value) 
		{
			return list.IndexOf (value);
		}

		public int IndexOf (string sourceColumn)
		{
			return list.IndexOf (sourceColumns[sourceColumn]);
		}

		public int IndexOfDataSetColumn (string value) 
		{
			return list.IndexOf (dataSetColumns[value]);
		}

		public void Insert (int index, object value) 
		{
			list.Insert (index, value);
			sourceColumns[((DataColumnMapping)value).SourceColumn] = value;
			dataSetColumns[((DataColumnMapping)value).DataSetColumn] = value;
		}

		public void Remove (object value) 
		{
			sourceColumns.Remove(((DataColumnMapping)value).SourceColumn);
			dataSetColumns.Remove(((DataColumnMapping)value).DataSetColumn);
			list.Remove (value);
		}

		public void RemoveAt (int index) 
		{
			Remove (list[index]);
		}

		public void RemoveAt (string sourceColumn)
		{
			RemoveAt (list.IndexOf (sourceColumns[sourceColumn]));
		}

	 	#endregion // Methods
	}
}
