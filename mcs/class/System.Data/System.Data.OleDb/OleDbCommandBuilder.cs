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

using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace System.Data.OleDb
{
	/// <summary>
	/// Provides a means of automatically generating single-table commands used to reconcile changes made to a DataSet with the associated database. This class cannot be inherited.
	/// </summary>
	public sealed class OleDbCommandBuilder : Component
	{
		#region Fields

		OleDbDataAdapter adapter;
		string quotePrefix;
		string quoteSuffix;

		#endregion // Fields

		#region Constructors
		
		public OleDbCommandBuilder ()
		{
			adapter = null;
			quotePrefix = String.Empty;
			quoteSuffix = String.Empty;
		}

		public OleDbCommandBuilder (OleDbDataAdapter adapter) 
			: this ()
		{
			this.adapter = adapter;
		}

		#endregion // Constructors

		#region Properties

		public OleDbDataAdapter DataAdapter {
			get {
				return adapter;
			}
			set {
				adapter = value;
			}
		}

		public string QuotePrefix {
			get {
				return quotePrefix;
			}
			set {
				quotePrefix = value;
			}
		}

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

		public static void DeriveParameters (OleDbCommand command) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void Dispose (bool disposing) 
		{
			throw new NotImplementedException ();		
		}

		[MonoTODO]
		public OleDbCommand GetDeleteCommand ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public OleDbCommand GetInsertCommand ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public OleDbCommand GetUpdatetCommand ()
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
