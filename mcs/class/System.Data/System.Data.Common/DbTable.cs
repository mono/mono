//
// System.Data.Common.DbTable.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.ComponentModel;

namespace System.Data.Common {
	public abstract class DbTable : DataTable
	{
		#region Constructors

		[MonoTODO]
		protected DbTable (DbProviderFactory providerFactory)
		{
		}

		#endregion // Constructors

		#region Properties

		[MonoTODO]	
		public ConflictOptions ConflictDetection {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]	
		public DbConnection Connection {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]	
		public DbCommand DeleteCommand {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]	
		public DbCommand InsertCommand {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]	
		public DbProviderFactory ProviderFactory {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]	
		public bool ReturnProviderSpecificTypes {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]	
		public DbCommand SelectCommand {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]	
		public override ISite Site {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]	
		public DataTableMapping TableMapping {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]	
		public int UpdateBatchSize {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]	
		public DbCommand UpdateCommand {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public DataRelation AddChildTable (string relationName, DbTable childTable, string parentColumnName, string childColumnName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRelation AddChildTable (string relationName, DbTable childTable, string[] parentColumnNames, string[] childColumnNames)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void BeginInit ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual DbCommandBuilder CreateCommandBuilder (DbConnection connection)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void EndInit ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Fill (object[] parameterValues)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Fill (FillOptions options, object[] parameterValues)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Fill (FillOptions options, DbTransaction transaction, object[] parameterValues)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int FillPage (int startRecord, int maxRecords, object[] parameterValues)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int FillPage (int startRecord, int maxRecords, FillOptions options, object[] parameterValues)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int FillPage (int startRecord, int maxRecords, FillOptions options, DbTransaction transaction, object[] parameterValues)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual string GenerateQuery (DbCommandBuilder cmdBuilder)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual string GenerateQueryForHierarchy (DbCommandBuilder builder, DataTable[] tableList)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Update ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Update (UpdateOptions updateOptions)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Update (UpdateOptions updateOptions, DbTransaction transaction)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int UpdateRows (DataRow[] dataRows)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int UpdateRows (DataRow[] dataRows, UpdateOptions updateOptions)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int UpdateRows (DataRow[] dataRows, UpdateOptions updateOptions, DbTransaction transaction)
		{
			throw new NotImplementedException ();
		}


		#endregion // Methods
	}
}

#endif // NET_1_2
