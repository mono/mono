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
			this.srcColumn = null;
			this.dsColumn = null;
		}

		public DataColumnMapping(string sc, string dc) {
			this.srcColumn = sc;
			this.dsColumn = dc;
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
