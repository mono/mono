//
// System.Data.Odbc.OdbcCommandBuilder
//
// Author:
//   Umadevi S (sumadevi@novell.com)
//
// Copyright (C) Novell Inc, 2004
//

using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace System.Data.Odbc
{
	/// <summary>
	/// Provides a means of automatically generating single-table commands used to reconcile changes made to a DataSet with the associated database. This class cannot be inherited.
	/// </summary>
	public sealed class OdbcCommandBuilder : Component
	{
		#region Fields

		OdbcDataAdapter adapter;
		string quotePrefix;
		string quoteSuffix;

		#endregion // Fields

		#region Constructors
		
		public OdbcCommandBuilder ()
		{
			adapter = null;
			quotePrefix = String.Empty;
			quoteSuffix = String.Empty;
		}

		public OdbcCommandBuilder (OdbcDataAdapter adapter) 
			: this ()
		{
			this.adapter = adapter;
		}

		#endregion // Constructors

		#region Properties

		[DataSysDescriptionAttribute ("The DataAdapter for which to automatically generate OleDbCommands")]
		[DefaultValue (null)]
		public OdbcDataAdapter DataAdapter {
			get {
				return adapter;
			}
			set {
				adapter = value;
			}
		}

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

		#endregion // Properties

		#region Methods

		public static void DeriveParameters (OdbcCommand command) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void Dispose (bool disposing) 
		{
			throw new NotImplementedException ();		
		}

		[MonoTODO]
		public OdbcCommand GetDeleteCommand ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public OdbcCommand GetInsertCommand ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public OdbcCommand GetUpdateCommand ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RefreshSchema ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
