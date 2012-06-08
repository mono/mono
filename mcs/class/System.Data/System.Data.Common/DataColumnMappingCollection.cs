//
// System.Data.Common.DataColumnMappingCollection
//
// Authors:
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
using System.Data;

namespace System.Data.Common
{
	public sealed class DataColumnMappingCollection : MarshalByRefObject, IColumnMappingCollection , IList, ICollection, IEnumerable
	{
		#region Fields

		readonly ArrayList list;
		readonly Hashtable sourceColumns;
		readonly Hashtable dataSetColumns;

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
#if !NET_2_0
		[DataSysDescription ("The number of items in the collection")]
#endif
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int Count {
			get { return list.Count; }
		}

		[Browsable (false)]
#if !NET_2_0
		[DataSysDescription ("The specified DataColumnMapping object.")]
#endif
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public DataColumnMapping this [int index] {
			get { return (DataColumnMapping)(list[index]); }
			set { 
				DataColumnMapping mapping = (DataColumnMapping)(list[index]);
				sourceColumns[mapping] = value;
				dataSetColumns[mapping] = value;
				list[index] = value;
			}
		}

		[Browsable (false)]
#if !NET_2_0
		[DataSysDescription ("The specified DataColumnMapping object.")]
#endif
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public DataColumnMapping this [string sourceColumn] {
			get {
				if (!Contains(sourceColumn))
					throw new IndexOutOfRangeException("DataColumnMappingCollection doesn't contain DataColumnMapping with SourceColumn '" + sourceColumn + "'.");
				return (DataColumnMapping) sourceColumns [sourceColumn];
			}
			set {
				this [list.IndexOf (sourceColumns [sourceColumn])] = value;
			}
		}

		object ICollection.SyncRoot {
			get { return list.SyncRoot; }
		}

		bool ICollection.IsSynchronized {
			get { return list.IsSynchronized; }
		}

		object IColumnMappingCollection.this [string index] {
			get { return this [index]; }
			set {
				if (!(value is DataColumnMapping))
					throw new ArgumentException ();
				this [index] = (DataColumnMapping) value;
			}
		}

		object IList.this [int index] {
			get { return this [index]; }
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
			sourceColumns [((DataColumnMapping) value).SourceColumn] = value;
			dataSetColumns [((DataColumnMapping )value).DataSetColumn] = value;
			return list.IndexOf (value);
		}

		public DataColumnMapping Add (string sourceColumn, string dataSetColumn)
		{
			DataColumnMapping mapping = new DataColumnMapping (sourceColumn, dataSetColumn);
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
			if (!(value is DataColumnMapping))
				throw new InvalidCastException("Object is not of type DataColumnMapping");
			return (list.Contains (value));
		}

		public bool Contains (string value)
		{
			return (sourceColumns.Contains (value));
		}

		public void CopyTo (Array array, int index)
		{
			list.CopyTo (array,index);
		}

#if NET_2_0
		public void CopyTo (DataColumnMapping [] array, int index)
		{
			list.CopyTo (array, index);
		}
#endif

		public DataColumnMapping GetByDataSetColumn (string value)
		{
			// this should work case-insenstive.
			if (!(dataSetColumns [value] == null))
				return (DataColumnMapping) (dataSetColumns [value]);
			else {
				string lowcasevalue = value.ToLower ();
				object [] keyarray = new object [dataSetColumns.Count];
				dataSetColumns.Keys.CopyTo (keyarray, 0);
				for (int i = 0; i < keyarray.Length; i++) {
					string temp = (string) keyarray [i];
					if (lowcasevalue.Equals (temp.ToLower ()))
						return (DataColumnMapping) (dataSetColumns [keyarray [i]]);
				}
				return null;
			}
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

#if NET_2_0
		[MonoTODO]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public static DataColumn GetDataColumn (DataColumnMappingCollection columnMappings, string sourceColumn, Type dataType, DataTable dataTable, MissingMappingAction mappingAction, MissingSchemaAction schemaAction)
		{
			throw new NotImplementedException ();
		}
#endif

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
			return list.IndexOf (sourceColumns [sourceColumn]);
		}

		public int IndexOfDataSetColumn (string dataSetColumn)
		{
			// this should work case-insensitive
			if (!(dataSetColumns [dataSetColumn] == null))
				return list.IndexOf (dataSetColumns [dataSetColumn]);
			else {
				string lowcasevalue = dataSetColumn.ToLower ();
				object [] keyarray = new object[dataSetColumns.Count];
				dataSetColumns.Keys.CopyTo (keyarray,0);
				for (int i = 0; i < keyarray.Length; i++) {
					string temp = (string) keyarray [i];
					if (lowcasevalue.Equals (temp.ToLower ()))
						return list.IndexOf (dataSetColumns [keyarray [i]]);
				}
				return -1;
			}
		}

		public void Insert (int index, object value)
		{
			list.Insert (index, value);
			sourceColumns [((DataColumnMapping) value).SourceColumn] = value;
			dataSetColumns [((DataColumnMapping) value).DataSetColumn] = value;
		}

#if NET_2_0
		public void Insert (int index, DataColumnMapping value)
		{
			list.Insert (index, value);
			sourceColumns [value.SourceColumn] = value;
			dataSetColumns [value.DataSetColumn] = value;
		}
#endif

		public void Remove (object value)
		{
			int index = list.IndexOf (value);
			sourceColumns.Remove (((DataColumnMapping) value).SourceColumn);
			dataSetColumns.Remove (((DataColumnMapping) value).DataSetColumn);
			if (index < 0 || index >=list.Count)
				throw new ArgumentException("There is no such element in collection.");
			list.Remove (value);
		}

#if NET_2_0
		public void Remove (DataColumnMapping value)
		{
			int index = list.IndexOf (value);
			sourceColumns.Remove (value.SourceColumn);
			dataSetColumns.Remove (value.DataSetColumn);
			if ( index < 0 || index >=list.Count)
				throw new ArgumentException("There is no such element in collection.");
			list.Remove (value);
		}
#endif

		public void RemoveAt (int index)
		{
			if (index < 0 || index >=list.Count)
				throw new IndexOutOfRangeException("There is no element in collection.");
			Remove (list [index]);
		}

		public void RemoveAt (string sourceColumn)
		{
			RemoveAt (list.IndexOf (sourceColumns [sourceColumn]));
		}

		#endregion // Methods
	}
}
