//
// System.Data.Common.DataAdapter
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
	/// Represents a set of data commands and a database connection that are used to fill the DataSet and update the data source.
	/// </summary>
	public abstract class DataAdapter : Component, IDataAdapter
	{
		private bool acceptChangesDuringFill;
		private bool continueUpdateOnError;
		private MissingMappingAction missingMappingAction;
		private MissingSchemaAction missingSchemaAction;
		private DataTableMappingCollection tableMappings;

		protected DataAdapter () {
			acceptChangesDuringFill = false;
			continueUpdateOnError = false;
			missingMappingAction = MissingMappingAction.Error;
			missingSchemaAction = MissingSchemaAction.Error;
			tableMappings = null;
		}
		
		public abstract int Fill (DataSet dataSet);

		public abstract DataTable[] FillSchema (DataSet dataSet,
						        SchemaType schemaType);

		public abstract IDataParameter[] GetFillParameters ();

		public abstract int Update (DataSet dataSet);

		protected virtual DataAdapter CloneInternals () {
		}

		protected virtual DataTableMappingCollection CreateTableMappings () {
		}

		protected virtual bool ShouldSerializeTableMappings () {
		}
		
		public bool AcceptChangesDuringFill {
			get {
				return acceptChangesDuringFill;
			}
			set {
				acceptChangesDuringFill = value;
			}
		}

		public bool ContinueUpdateOnError {
			get {
				return continueUpdateOnError;
			}
			set {
				continueUpdateOnError = value;
			}
		}

		public MissingMappingAction MissingMappingAction {
			get {
				return missingMappingAction;
			}
			set {
				missingMappingAction = value;
			}
		}

		public MissingSchemaAction MissingSchemaAction {
			get {
				return missingSchemaAction;
			}
			set {
				missingSchemaAction = value;
			}
		}

		public DataTableMappingCollection TableMappings {
			get {
				if (tableMappings == null)
					tableMappings = CreateTableMappings ();
				return tableMappings;
			}
		}
	}
}
