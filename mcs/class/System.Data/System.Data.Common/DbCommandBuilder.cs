//
// System.Data.Common.DbCommandBuilder
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.ComponentModel;
using System.Data;

namespace System.Data.Common {
	public abstract class DbCommandBuilder : Component
	{
		#region Constructors

		[MonoTODO]
		protected DbCommandBuilder ()
		{
		}

		#endregion // Constructors

		#region Properties

		[MonoTODO]
		public virtual CatalogLocation CatalogLocation {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual string CatalogSeparator {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual ConflictOptions ConflictDetection {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public DbDataAdapter DataAdapter {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		protected abstract DbProviderFactory ProviderFactory { get; }

		[MonoTODO]
		public virtual string QuotePrefix {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual string QuoteSuffix {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public SchemaLocation SchemaLocation {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual string SchemaSeparator {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		protected abstract void ApplyParameterInfo (IDbDataParameter p, DataRow row);

		[MonoTODO]
		protected virtual void BuildCache (bool closeConnection, DataRow dataRow)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Delegate FindBUilder (MulticastDelegate mcd)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string FormatLiteral (DbConnection connection, string dataTypeName, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DbCommand GetDeleteCommand ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DbCommand GetDeleteCommand (DataRow dataRow)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DbCommand GetInsertCommand ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DbCommand GetInsertCommand (DataRow dataRow)
		{
			throw new NotImplementedException ();
		}

		protected abstract string GetParameterName (int parameterOrdinal);
		protected abstract string GetParameterPlaceholder (int parameterOrdinal);

		[MonoTODO]
		protected DbCommand GetSelectCommand ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DbCommand GetUpdateCommand ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DbCommand GetUpdateCommand (DataRow dataRow)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual DbCommand InitializeCommand (DbCommand command)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal static string[] ParseProcedureName (string procedure)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string QuoteIdentifier (string unquotedIdentifier)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void RefreshSchema ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void ResolveObjectName (DbConnection connection, string objectType, string[] identifierParts)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void RowUpdatingHandler (object sender, RowUpdatingEventArgs rowUpdatingEvent)
		{
			throw new NotImplementedException ();
		}

		protected abstract void SetRowUpdatingHandler (DbDataAdapter adapter);

		[MonoTODO]
		public virtual object UnformatLiteral (DbConnection connection, string dataTypeName, string literalValue)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string UnquoteIdentifier (string quotedIdentifier)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}

#endif
