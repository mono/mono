//
// System.Data.Common.DataAdapter
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc
// Copyright (C) 2002 Tim Coleman
//

using System.ComponentModel;
using System.Data;

namespace System.Data.Common
{
	/// <summary>
	/// Represents a set of data commands and a database connection that are used to fill the DataSet and update the data source.
	/// </summary>
	public abstract class DataAdapter : Component
	{
		#region Fields

		private bool acceptChangesDuringFill;
		private bool continueUpdateOnError;
		private MissingMappingAction missingMappingAction;
		private MissingSchemaAction missingSchemaAction;
		private DataTableMappingCollection tableMappings;

		#endregion

		#region Constructors

		protected DataAdapter () 
		{
			acceptChangesDuringFill = true;
			continueUpdateOnError = false;
			missingMappingAction = MissingMappingAction.Passthrough;
			missingSchemaAction = MissingSchemaAction.Add;
			tableMappings = new DataTableMappingCollection ();
		}

		#endregion

		#region Properties

		public bool AcceptChangesDuringFill {
			get { return acceptChangesDuringFill; }
			set { acceptChangesDuringFill = value; }
		}

		public bool ContinueUpdateOnError {
			get { return continueUpdateOnError; }
			set { continueUpdateOnError = value; }
		}

		public MissingMappingAction MissingMappingAction {
			get { return missingMappingAction; }
			set { missingMappingAction = value; }
		}

		public MissingSchemaAction MissingSchemaAction {
			get { return missingSchemaAction; }
			set { missingSchemaAction = value; }
		}

		public DataTableMappingCollection TableMappings {
			get { return tableMappings; }
		}

		#endregion

		#region Methods
		

		[MonoTODO]
		protected virtual DataAdapter CloneInternals ()
		{
			throw new NotImplementedException ();
		}

		protected virtual DataTableMappingCollection CreateTableMappings ()
		{
			tableMappings = new DataTableMappingCollection ();
			return tableMappings;
		}

		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}

		public abstract int Fill (DataSet dataSet);
		public abstract DataTable[] FillSchema (DataSet dataSet, SchemaType schemaType);
		public abstract IDataParameter[] GetFillParameters ();

		[MonoTODO]
		protected virtual bool ShouldSerializeTableMappings ()
		{
			throw new NotImplementedException ();
		}

		public abstract int Update (DataSet dataSet);

		#endregion
		
	}
}
