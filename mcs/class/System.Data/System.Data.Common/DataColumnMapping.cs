//
// System.Data.Common.DataColumnMapping
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc
//

namespace System.Data.Common
{
	/// <summary>
	/// Contains a generic column mapping for an object that inherits from DataAdapter. This class cannot be inherited.
	/// </summary>
	public sealed class DataColumnMapping : MarshalByRefObject, IColumnMapping, ICloneable
	{
		[MonoTODO]
		public DataColumnMapping()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataColumnMapping(string src_column, string ds_column)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataColumn GetDataColumnBySchemaAction(DataTable dataTable,
							      Type dataType,
							      MissingSchemaAction schemaAction)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string DataSetColumn
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string SourceColumn
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	}
}
