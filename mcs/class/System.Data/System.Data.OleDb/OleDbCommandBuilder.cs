//
// System.Data.OleDb.OleDbCommandBuilder
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Rodrigo Moya, 2002
// Copyright (C) Tim Coleman, 2002
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

using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace System.Data.OleDb
{
	/// <summary>
	/// Provides a means of automatically generating single-table commands used to reconcile changes made to a DataSet with the associated database. This class cannot be inherited.
	/// </summary>
	public sealed class OleDbCommandBuilder :
#if NET_2_0
		DbCommandBuilder
#else
		Component
#endif
	{
		#region Fields

		OleDbDataAdapter adapter;
#if !NET_2_0
		string quotePrefix;
		string quoteSuffix;
#endif

		#endregion // Fields

		#region Constructors
		
		public OleDbCommandBuilder ()
		{
#if !NET_2_0
			quotePrefix = String.Empty;
			quoteSuffix = String.Empty;
#endif
		}

		public OleDbCommandBuilder (OleDbDataAdapter adapter) 
			: this ()
		{
			this.adapter = adapter;
		}

		#endregion // Constructors

		#region Properties

#if !NET_2_0
		[DataSysDescriptionAttribute ("The DataAdapter for which to automatically generate OleDbCommands")]
#endif
		[DefaultValue (null)]
		public new OleDbDataAdapter DataAdapter {
			get {
				return adapter;
			}
			set {
				adapter = value;
			}
		}

#if !NET_2_0
		[BrowsableAttribute (false)]
		[DataSysDescriptionAttribute ("The prefix string wrapped around sql objects")]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public string QuotePrefix {
			get {
				return quotePrefix;
			}
			set {
				quotePrefix = value;
			}
		}

		[BrowsableAttribute (false)]
		[DataSysDescriptionAttribute ("The suffix string wrapped around sql objects")]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public string QuoteSuffix {
			get {
				return quoteSuffix;
			}
			set {
				quoteSuffix = value;
			}
		}
#endif

		#endregion // Properties

		#region Methods

#if NET_2_0
		[MonoTODO]
		protected override void ApplyParameterInfo (DbParameter parameter, DataRow datarow, StatementType statementType, bool whereClause)
		{
			throw new NotImplementedException ();
		}
#endif

		public static void DeriveParameters (OleDbCommand command)
		{
			throw new NotImplementedException ();
		}

#if !NET_2_0
		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}
#endif

		[MonoTODO]
		public new OleDbCommand GetDeleteCommand ()
		{
			throw new NotImplementedException ();
		}

#if NET_2_0
		[MonoTODO]
		public new OleDbCommand GetDeleteCommand (bool useColumnsForParameterNames)
		{
			throw new NotImplementedException ();
		}
#endif

		[MonoTODO]
		public new OleDbCommand GetInsertCommand ()
		{
			throw new NotImplementedException ();
		}

#if NET_2_0
		[MonoTODO]
		public new OleDbCommand GetInsertCommand (bool useColumnsForParameterNames)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override string GetParameterName (int parameterOrdinal)
		{
			throw new NotImplementedException ();
		}

		protected override string GetParameterName (string parameterName)
		{
			return parameterName;
		}

		[MonoTODO]
		protected override string GetParameterPlaceholder(int parameterOrdinal)
		{
			throw new NotImplementedException ();
		}
#endif

		[MonoTODO]
		public new OleDbCommand GetUpdateCommand ()
		{
			throw new NotImplementedException ();
		}

#if NET_2_0
		[MonoTODO]
		public new OleDbCommand GetUpdateCommand (bool useColumnsForParameterNames)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string QuoteIdentifier(string unquotedIdentifier)
		{
			return base.QuoteIdentifier (unquotedIdentifier);
		}

		[MonoTODO]
		public string QuoteIdentifier(string unquotedIdentifier, OleDbConnection connection)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void SetRowUpdatingHandler(DbDataAdapter adapter)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string UnquoteIdentifier(string quotedIdentifier)
		{
			return base.UnquoteIdentifier (quotedIdentifier);
		}

		[MonoTODO]
		public string UnquoteIdentifier(string quotedIdentifier, OleDbConnection connection)
		{
			throw new NotImplementedException ();
		}
#else
		[MonoTODO]
		public void RefreshSchema ()
		{
			throw new NotImplementedException ();
		}
#endif

		#endregion // Methods
	}
}
