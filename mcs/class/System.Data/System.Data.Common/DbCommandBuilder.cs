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
		bool _setAllValues = false;
		DbDataAdapter _dbDataAdapter;

		#region Constructors

		[MonoTODO]
		protected DbCommandBuilder ()
		{
		}

		#endregion // Constructors

		#region Properties

		[MonoTODO]
		[DefaultValue (CatalogLocation.Start)]
		public virtual CatalogLocation CatalogLocation {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		[DefaultValue (".")]
		public virtual string CatalogSeparator {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		[DefaultValue (ConflictOption.CompareAllSearchableValues)]
		public virtual ConflictOption ConflictOption {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public DbDataAdapter DataAdapter {
			get { return _dbDataAdapter; }
			set { _dbDataAdapter = value; }
		}

		[MonoTODO]
		[DefaultValue ("")]
		public virtual string QuotePrefix {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		[DefaultValue ("")]
		public virtual string QuoteSuffix {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		[DefaultValue (".")]
		public virtual string SchemaSeparator {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[DefaultValue (false)]
		public bool SetAllValues {
			get { return _setAllValues; }
			set { _setAllValues = value; }
		}
		
		#endregion // Properties

		#region Methods

		protected abstract void ApplyParameterInfo (DbParameter parameter, 
							    DataRow row, 
							    StatementType statementType, 
							    bool whereClause);

		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DbCommand GetDeleteCommand ()
		{
			return (DbCommand) _dbDataAdapter._deleteCommand;
		}

		[MonoTODO]
		public DbCommand GetDeleteCommand (bool option)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DbCommand GetInsertCommand ()
		{
			return (DbCommand) _dbDataAdapter._insertCommand;
		}

		[MonoTODO]
		public DbCommand GetInsertCommand (bool option)
		{
			throw new NotImplementedException ();
		}

		protected abstract string GetParameterName (int parameterOrdinal);
		protected abstract string GetParameterName (String parameterName);
		protected abstract string GetParameterPlaceholder (int parameterOrdinal);

		[MonoTODO]
		public DbCommand GetUpdateCommand ()
		{
			return (DbCommand) _dbDataAdapter._updateCommand;
		}

		[MonoTODO]
		public DbCommand GetUpdateCommand (bool option)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual DbCommand InitializeCommand (DbCommand command)
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
		protected void RowUpdatingHandler (RowUpdatingEventArgs rowUpdatingEvent)
		{
			throw new NotImplementedException ();
		}

		protected abstract void SetRowUpdatingHandler (DbDataAdapter adapter);

		[MonoTODO]
		public virtual string UnquoteIdentifier (string quotedIdentifier)
		{
			throw new NotImplementedException ();
		}

		protected virtual DataTable GetSchemaTable (DbCommand cmd)
		{
			using (DbDataReader rdr = cmd.ExecuteReader ())
				return rdr.GetSchemaTable ();
		}

		#endregion // Methods
	}
}

#endif
