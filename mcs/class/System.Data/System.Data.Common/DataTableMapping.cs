//
// System.Data.Common.DataTableMapping.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc
//

using System.Data;

namespace System.Data.Common
{
	/// <summary>
	/// Contains a description of a mapped relationship between a source table and a DataTable. This class is used by a DataAdapter when populating a DataSet.
	/// </summary>
	public sealed class DataTableMapping : MarshalByRefObject // , ITableMapping, ICloneable
	{
		[MonoTODO]
		public DataTableMapping() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataTableMapping (string a, string b) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataTableMapping(string a, string b, DataColumnMapping[] c) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataColumnMapping GetColumnMappingBySchemaAction(
			string sourceColumn,
			MissingMappingAction mappingAction) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataTable GetDataTableBySchemaAction(
			DataSet dataSet,
			MissingSchemaAction schemaAction) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataColumnMappingCollection ColumnMappings {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string DataSetTable {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string SourceTable {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	}
}
