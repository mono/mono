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
			this.acceptChangesDuringFill = false;
			this.continueUpdateOnError = false;
			this.missingMappingAction = Error;
			this.missingSchemaAction = Error;
			this.tableMappings = null;
		}
		
		public abstract int Fill (DataSet dataSet);

		public abstract DataTable[] FillSchema (DataSet dataSet,
						        SchemaType schemaType);

		public abstract IDataParameter[] GetFillParameters ();

		public abstract int Update (DataSet dataSet);

		protected virtual DataAdapter CloneInternals ();

		protected virtual DataTableMappingCollection CreateTableMappings ();

		protected virtual bool ShouldSerializeTableMappings ();
		
		public bool AcceptChangesDuringFill {
			get {
				return this.acceptChangesDuringFill;
			}
			set {
				this.acceptChangesDuringFill = value;
			}
		}
		
		public bool ContinueUpdateOnError {
			get {
				return this.continueUpdateOnError;
			}
			set {
				this.continueUpdateOnError = value;
			}
		}

		public MissingMappingAction MissingMappingAction {
			get {
				return this.missingMappingAction;
			}
			set {
				this.missingMappingAction = value;
			}
		}

		public MissingSchemaAction MissingSchemaAction {
			get {
				return this.missingSchemaAction;
			}
			set {
				this.missingSchemaAction = value;
			}
		}

		public DataTableMappingCollection TableMappings {
			get {
				if (this.tableMappings == null)
					this.tableMappings = CreateTableMappings ();
				return this.tableMappings;
			}
		}
	}
}
