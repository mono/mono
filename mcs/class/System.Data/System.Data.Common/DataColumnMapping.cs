//
// System.Data.Common.DataColumnMapping.cs
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

using System.ComponentModel;
using System.Data;

namespace System.Data.Common {
#if NET_2_0
	[TypeConverterAttribute ("System.Data.Common.DataColumnMapping+DataColumnMappingConverter, " + Consts.AssemblySystem_Data)]
#else
	[TypeConverterAttribute (typeof (DataColumnMappingConverter))]
#endif
	public sealed class DataColumnMapping : MarshalByRefObject, IColumnMapping, ICloneable
	{
		#region Fields

		string sourceColumn;
		string dataSetColumn;

		#endregion // Fields

		#region Constructors
		
		public DataColumnMapping () 
		{
			sourceColumn = String.Empty;
			dataSetColumn = String.Empty;
		}

		public DataColumnMapping (string sourceColumn, string dataSetColumn) 
		{
			this.sourceColumn = sourceColumn;
			this.dataSetColumn = dataSetColumn;
		}

		#endregion // Constructors

		#region Properties

#if ONLY_1_1
		[DataSysDescription ("DataColumn.ColumnName")]
#endif
		[DefaultValue ("")]
		public string DataSetColumn {
			get { return dataSetColumn; }
			set { dataSetColumn = value; }
		}

#if !NET_2_0
		[DataSysDescription ("Source column name - case sensitive.")]
#endif
		[DefaultValue ("")]
		public string SourceColumn {
			get { return sourceColumn; }
			set { sourceColumn = value; }
		}

		#endregion // Properties

		#region Methods

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public DataColumn GetDataColumnBySchemaAction (DataTable dataTable, Type dataType, MissingSchemaAction schemaAction) 
		{
			if (dataTable.Columns.Contains (dataSetColumn))
				return dataTable.Columns [dataSetColumn];
			if (schemaAction == MissingSchemaAction.Ignore)
				return null;
			if (schemaAction == MissingSchemaAction.Error)
				throw new InvalidOperationException (String.Format ("Missing the DataColumn '{0}' in the DataTable '{1}' for the SourceColumn '{2}'", DataSetColumn, dataTable.TableName, SourceColumn));
			return new DataColumn (dataSetColumn, dataType);
		}

#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public static DataColumn GetDataColumnBySchemaAction (string sourceColumn, string dataSetColumn, DataTable dataTable, Type dataType, MissingSchemaAction schemaAction)
		{
			if (dataTable.Columns.Contains (dataSetColumn))
				return dataTable.Columns [dataSetColumn];
			if (schemaAction == MissingSchemaAction.Ignore)
				return null;
			if (schemaAction == MissingSchemaAction.Error)
				throw new InvalidOperationException (String.Format ("Missing the DataColumn '{0}' in the DataTable '{1}' for the SourceColumn '{2}'", dataSetColumn, dataTable.TableName, sourceColumn));
			return new DataColumn (dataSetColumn, dataType);
		}
#endif

		object ICloneable.Clone ()
		{
			return new DataColumnMapping (SourceColumn, DataSetColumn);
		}

		public override string ToString ()
		{
			return SourceColumn; 
		}

		#endregion // Methods
	}
}
