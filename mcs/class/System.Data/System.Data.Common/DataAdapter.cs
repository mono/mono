//
// System.Data.Common.DataAdapter
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc
//

namespace System.Data.Common
{
	/// <summary>
	/// Represents a set of data commands and a database connection that are used to fill the DataSet and update the data source.
	/// </summary>
	public abstract class DataAdapter : Component, IDataAdapter
	{
		[MonoTODO]
		protected DataAdapter()
		{
			throw new NotImplementedException ();
		}
		
		public abstract int Fill(DataSet dataSet);

		public abstract DataTable[] FillSchema(DataSet dataSet,
						       SchemaType schemaType);

		public abstract IDataParameter[] GetFillParameters();

		public abstract int Update(DataSet dataSet);

		protected virtual DataAdapter CloneInternals();

		protected virtual DataTableMappingCollection CreateTableMappings();

		protected virtual bool ShouldSerializeTableMappings();
		
		[MonoTODO]
		public bool AcceptChangesDuringFill
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public bool ContinueUpdateOnError
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public MissingMappingAction MissingMappingAction
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public MissingSchemaAction MissingSchemaAction
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public DataTableMappingCollection TableMappings
		{
			get { throw new NotImplementedException (); }
		}
	}
}
