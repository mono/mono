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
	public abstract class DataAdapter : Component, IDataAdapter
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

		[DataCategory ("Fill")]
		[DataSysDescription ("Whether or not Fill will call DataRow.AcceptChanges.")]
		[DefaultValue (true)]
		public bool AcceptChangesDuringFill {
			get { return acceptChangesDuringFill; }
			set { acceptChangesDuringFill = value; }
		}

		[DataCategory ("Update")]
		[DataSysDescription ("Whether or not to continue to the next DataRow when the Update events, RowUpdating and RowUpdated, Status is UpdateStatus.ErrorsOccurred.")]
		[DefaultValue (false)]
		public bool ContinueUpdateOnError {
			get { return continueUpdateOnError; }
			set { continueUpdateOnError = value; }
		}

		ITableMappingCollection IDataAdapter.TableMappings {
			get { return TableMappings; }
		}

		[DataCategory ("Mapping")]
		[DataSysDescription ("The action taken when a table or column in the TableMappings is missing.")]
		[DefaultValue (MissingMappingAction.Passthrough)]
		public MissingMappingAction MissingMappingAction {
			get { return missingMappingAction; }
			set { missingMappingAction = value; }
		}

		[DataCategory ("Mapping")]
		[DataSysDescription ("The action taken when a table or column in the DataSet is missing.")]
		[DefaultValue (MissingSchemaAction.Add)]
		public MissingSchemaAction MissingSchemaAction {
			get { return missingSchemaAction; }
			set { missingSchemaAction = value; }
		}

		[DataCategory ("Mapping")]
		[DataSysDescription ("How to map source table to DataSet table.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public DataTableMappingCollection TableMappings {
			get { return tableMappings; }
		}

		#endregion

		#region Methods

#if NET_1_1
                [Obsolete ("Use the protected constructor instead", false)]
#endif
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
