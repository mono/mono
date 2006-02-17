//
// System.Data.Common.DbTable.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if TARGET_JVM

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
		public ConflictOption ConflictOption {
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

#endif
