//
// System.Data.Common.DbCommandBuilder
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

#if NET_2_0 || TARGET_JVM

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
