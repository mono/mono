//
// System.Data.SqlClient.SqlCommandBuilder.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Veerapuram Varadhan (vvaradhan@novell.com)
//
// Copyright (C) Tim Coleman, 2002
//

//
// Copyright (C) 2004, 2009 Novell, Inc (http://www.novell.com)
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

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Text;

namespace System.Data.SqlClient
{
	public sealed class SqlCommandBuilder : DbCommandBuilder
	{
		#region Fields

		readonly string _catalogSeparator = ".";
		readonly string _schemaSeparator = ".";
		readonly CatalogLocation _catalogLocation = CatalogLocation.Start;
	
		#endregion // Fields

		#region Constructors

		public SqlCommandBuilder ()
		{
			QuoteSuffix = "]";
			QuotePrefix = "[";
		}

		public SqlCommandBuilder (SqlDataAdapter adapter)
			: this ()
		{
			DataAdapter = adapter;
		}

		#endregion // Constructors

		#region Properties

		[DefaultValue (null)]
		public new SqlDataAdapter DataAdapter {
			get { 
				return (SqlDataAdapter)base.DataAdapter;
			} set {
				base.DataAdapter = value;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public
		override
		string QuotePrefix {
			get {
				return base.QuotePrefix;
			}
			set {
				if (value != "[" && value != "\"")
					throw new ArgumentException ("Only '[' " +
						"and '\"' are allowed as value " +
						"for the 'QuoteSuffix' property.");
				base.QuotePrefix = value;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public
		override
		string QuoteSuffix {
			get {
				return base.QuoteSuffix;
			}
			set {
				if (value != "]" && value != "\"")
					throw new ArgumentException ("Only ']' " +
						"and '\"' are allowed as value " +
						"for the 'QuoteSuffix' property.");
				base.QuoteSuffix = value;
			}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override string CatalogSeparator {
			get { return _catalogSeparator; }
			set {
				if (value != _catalogSeparator)
					throw new ArgumentException ("Only " +
						"'.' is allowed as value " +
						"for the 'CatalogSeparator' " +
						"property.");
			}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override string SchemaSeparator {
			get { return _schemaSeparator; }
			set {
				if (value != _schemaSeparator)
					throw new ArgumentException ("Only " +
						"'.' is allowed as value " +
						"for the 'SchemaSeparator' " +
						"property.");
			}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override CatalogLocation CatalogLocation {
			get { return _catalogLocation; }
			set {
				if (value != CatalogLocation.Start)
					throw new ArgumentException ("Only " +
						"'Start' is allowed as value " +
						"for the 'CatalogLocation' " +
						"property.");
			}
		}



		#endregion // Properties

		#region Methods

				
		public static void DeriveParameters (SqlCommand command)
		{
			command.DeriveParameters ();
		}


		public
		new
		SqlCommand GetDeleteCommand ()
		{
			return (SqlCommand) base.GetDeleteCommand (false);
		}

		public
		new
		SqlCommand GetInsertCommand ()
		{
			return (SqlCommand) base.GetInsertCommand (false);
		}

		public 
		new
		SqlCommand GetUpdateCommand ()
		{
			return (SqlCommand) base.GetUpdateCommand (false);
		}

		public new SqlCommand GetUpdateCommand (bool useColumnsForParameterNames)
		{
			return (SqlCommand) base.GetUpdateCommand (useColumnsForParameterNames);
		}

		public new SqlCommand GetDeleteCommand (bool useColumnsForParameterNames)
		{
			return (SqlCommand) base.GetDeleteCommand (useColumnsForParameterNames);
		}

		public new SqlCommand GetInsertCommand (bool useColumnsForParameterNames)
		{
			return (SqlCommand) base.GetInsertCommand (useColumnsForParameterNames);
		}
		
		public override string QuoteIdentifier (string unquotedIdentifier)
		{
			if (unquotedIdentifier == null)
				throw new ArgumentNullException ("unquotedIdentifier");

			string prefix = QuotePrefix;
			string suffix = QuoteSuffix;

			if ((prefix == "[" && suffix != "]") || (prefix == "\"" && suffix != "\""))
				throw new ArgumentException ("The QuotePrefix " +
					"and QuoteSuffix properties do not match.");

			string escaped = unquotedIdentifier.Replace (suffix,
				suffix + suffix);
			return string.Concat (prefix, escaped, suffix);
		}
		
		public override string UnquoteIdentifier (string quotedIdentifier)
		{
			return base.UnquoteIdentifier (quotedIdentifier);
		}

		private bool IncludedInInsert (DataRow schemaRow)
		{
			// If the parameter has one of these properties, then we don't include it in the insert:
			// AutoIncrement, Hidden, Expression, RowVersion, ReadOnly

			if (!schemaRow.IsNull ("IsAutoIncrement") && (bool) schemaRow ["IsAutoIncrement"])
				return false;
			if (!schemaRow.IsNull ("IsHidden") && (bool) schemaRow ["IsHidden"])
				return false;
			if (!schemaRow.IsNull ("IsExpression") && (bool) schemaRow ["IsExpression"])
				return false;
			if (!schemaRow.IsNull ("IsRowVersion") && (bool) schemaRow ["IsRowVersion"])
				return false;
			if (!schemaRow.IsNull ("IsReadOnly") && (bool) schemaRow ["IsReadOnly"])
				return false;
			return true;
		}

		private bool IncludedInUpdate (DataRow schemaRow)
		{
			// If the parameter has one of these properties, then we don't include it in the insert:
			// AutoIncrement, Hidden, RowVersion

			if (!schemaRow.IsNull ("IsAutoIncrement") && (bool) schemaRow ["IsAutoIncrement"])
				return false;
			if (!schemaRow.IsNull ("IsHidden") && (bool) schemaRow ["IsHidden"])
				return false;
			if (!schemaRow.IsNull ("IsRowVersion") && (bool) schemaRow ["IsRowVersion"])
				return false;
			if (!schemaRow.IsNull ("IsExpression") && (bool) schemaRow ["IsExpression"])
				return false;
			if (!schemaRow.IsNull ("IsReadOnly") && (bool) schemaRow ["IsReadOnly"])
				return false;

			return true;
		}

		private bool IncludedInWhereClause (DataRow schemaRow)
		{
			if ((bool) schemaRow ["IsLong"])
				return false;
			return true;
		}


		protected override void ApplyParameterInfo (DbParameter parameter,
		                                            DataRow datarow,
		                                            StatementType statementType,
		                                            bool whereClause)
		{
			SqlParameter sqlParam = (SqlParameter) parameter;
			sqlParam.SqlDbType = (SqlDbType) datarow ["ProviderType"];

			object precision = datarow ["NumericPrecision"];
			if (precision != DBNull.Value) {
				short val = (short) precision;
				if (val < byte.MaxValue && val >= byte.MinValue)
					sqlParam.Precision = (byte) val;
			}

			object scale = datarow ["NumericScale"];
			if (scale != DBNull.Value) {
				short val = ((short) scale);
				if (val < byte.MaxValue && val >= byte.MinValue)
					sqlParam.Scale = (byte) val;
			}
		}

		protected override
		string GetParameterName (int parameterOrdinal)
		{
			return String.Format ("@p{0}",  parameterOrdinal);
		}

		protected override
		string GetParameterName (string parameterName)
		{
			return String.Format ("@{0}", parameterName);
		}

		protected override string GetParameterPlaceholder (int parameterOrdinal)
		{
			return GetParameterName (parameterOrdinal);
		}

		#endregion // Methods

		#region Event Handlers

		void RowUpdatingHandler (object sender, SqlRowUpdatingEventArgs args)
		{
			base.RowUpdatingHandler (args);
		}

		protected override void SetRowUpdatingHandler (DbDataAdapter adapter)
		{
				SqlDataAdapter sda = adapter as SqlDataAdapter;
				if (sda == null) {
					throw new InvalidOperationException ("Adapter needs to be a SqlDataAdapter");
				}
				
				if (sda != base.DataAdapter)
					sda.RowUpdating += new SqlRowUpdatingEventHandler (RowUpdatingHandler);
				else
					sda.RowUpdating -= new SqlRowUpdatingEventHandler (RowUpdatingHandler);;
		}

		protected override DataTable GetSchemaTable (DbCommand srcCommand)
		{
			using (SqlDataReader rdr = (SqlDataReader) srcCommand.ExecuteReader (CommandBehavior.KeyInfo | CommandBehavior.SchemaOnly))
				return rdr.GetSchemaTable ();
		}

		protected override DbCommand InitializeCommand (DbCommand command)
		{
			if (command == null) {
				command = new SqlCommand ();
			} else {
				command.CommandTimeout = 30;
				command.Transaction = null;
				command.CommandType = CommandType.Text;
				command.UpdatedRowSource = UpdateRowSource.None;
			}
			return command;
		}

		#endregion // Event Handlers
	}
}
