//
// System.Data.SqlClient.SqlCommandBuilder.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc 2002
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Data;
using System.ComponentModel;

namespace System.Data.SqlClient {
	/// <summary>
	/// Builder of one command
	/// that will be used in manipulating a table for
	/// a DataSet that is assoicated with a database.
	/// </summary>
	public sealed class SqlCommandBuilder : Component 
	{
		#region Fields

		string quotePrefix;
		string quoteSuffix;
		SqlDataAdapter adapter;

		#endregion // Fields

		#region Constructors

		public SqlCommandBuilder () 
			: this (null)
		{
		}

		public SqlCommandBuilder (SqlDataAdapter adapter) 
		{
			this.adapter = adapter;
			this.quotePrefix = String.Empty;
			this.quoteSuffix = String.Empty;
		}

		#endregion // Constructors

		#region Properties

		[DataSysDescription ("The DataAdapter for which to automatically generator SqlCommands.")]
		[DefaultValue (null)]
		public SqlDataAdapter DataAdapter {
			get { return adapter; }
			set { 
				if (adapter != null)
					adapter.RowUpdating -= new SqlRowUpdatingEventHandler (RowUpdatingHandler);
				adapter = value;
				adapter.RowUpdating += new SqlRowUpdatingEventHandler (RowUpdatingHandler);
			}
		}

		[Browsable (false)]
		[DataSysDescription ("The character used in a text command as the opening quote for quoting identifiers that contain special characters.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string QuotePrefix {
			get { return quotePrefix; }
			set { quotePrefix = value; }
		}

		[Browsable (false)]
		[DataSysDescription ("The character used in a text command as the closing quote for quoting identifiers that contain special characters.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string QuoteSuffix {
			get { return quoteSuffix; }
			set { quoteSuffix = value; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public static void DeriveParameters (SqlCommand command) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlCommand GetDeleteCommand () 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlCommand GetInsertCommand () 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlCommand GetUpdateCommand () 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RefreshSchema () 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		private void RowUpdatingHandler (object sender, SqlRowUpdatingEventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void Dispose (bool disposing) 
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}

