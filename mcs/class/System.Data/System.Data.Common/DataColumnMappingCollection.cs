//
// System.Data.Common.DataColumnCollection
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc
// (C) Copyright 2002 Tim Coleman
//

using System;
using System.Collections;
using System.Data;

namespace System.Data.Common
{
	/// <summary>
	/// Contains a collection of DataColumnMapping objects. This class cannot be inherited.
	/// </summary>
	public sealed class DataColumnMappingCollection : MarshalByRefObject // , IColumnMappingCollection , IList, ICollection, IEnumerable
	{
		#region Fields

		ArrayList list;
		Hashtable sourceColumns;
		Hashtable dataSetColumns;

		#endregion

		#region Constructors 

		public DataColumnMappingCollection () 
		{
			list = new ArrayList ();
			sourceColumns = new Hashtable ();
			dataSetColumns = new Hashtable ();
		}

		#endregion

		#region Properties

		public int Count {
			get { return list.Count; }
		}

		public DataColumnMapping this[int index] {
			get { return (DataColumnMapping)(list[index]); }
			set { 
				DataColumnMapping mapping = (DataColumnMapping)(list[index]);
				sourceColumns[mapping] = value;
				dataSetColumns[mapping] = value;
				list[index] = value;
			}
		}

		public DataColumnMapping this[string sourceColumn] {
			get { return (DataColumnMapping)(sourceColumns[sourceColumn]); }
			set { this[list.IndexOf (sourceColumns[sourceColumn])] = value; }
		}

		#endregion

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

		public static DataColumnMapping GetColumnMappingBySchemaAction (DataColumnMappingCollection columnMappings, string sourceColumn, MissingMappingAction mappingAction) 
		{
			if (columnMappings.Contains (sourceColumn))
				return columnMappings[sourceColumn];

			if (mappingAction == MissingMappingAction.Ignore)
				return null;

			if (mappingAction == MissingMappingAction.Error)
				throw new SystemException ();

			return new DataColumnMapping (sourceColumn, sourceColumn);
		}

		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

/* FIXME
		IColumnMapping IColumnMappingCollection.Add (string sourceColumnName, string dataSetColumnName)
		{
			return (IColumnMapping)(Add (sourceColumnName, dataSetColumnName));
		}

		IColumnMapping IColumnMappingCollection.GetByDataSetColumn (string dataSetColumnName)
		{
			return (IColumnMapping)(GetByDataSetColumn (dataSetColumnName));
		}
*/

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

		#endregion
	}
}
