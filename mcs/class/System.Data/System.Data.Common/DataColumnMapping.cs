//
// System.Data.Common.DataColumnMapping
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
	/// Contains a generic column mapping for an object that inherits from DataAdapter. This class cannot be inherited.
	/// </summary>
	public sealed class DataColumnMapping : MarshalByRefObject, IColumnMapping, ICloneable
	{
		private string srcColumn;
		private string dsColumn;
		
		public DataColumnMapping () {
			srcColumn = null;
			dsColumn = null;
		}

		public DataColumnMapping(string sc, string dc) {
			srcColumn = sc;
			dsColumn = dc;
		}

		[MonoTODO]
		public DataColumn GetDataColumnBySchemaAction (
			DataTable dataTable,
			Type dataType,
			MissingSchemaAction schemaAction) {
			throw new NotImplementedException ();
		}

		public string DataSetColumn {
			get {
				return this.dsColumn;
			}
			set {
				this.dsColumn = value;
			}
		}

		[MonoTODO]
		object ICloneable.Clone ()
		{
			throw new NotImplementedException ();
		}

		public string SourceColumn {
			get {
				return this.srcColumn;
			}
			set {
				this.srcColumn = value;
			}
		}
	}
}
