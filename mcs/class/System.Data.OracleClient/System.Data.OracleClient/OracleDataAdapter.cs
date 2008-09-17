//
// OracleDataAdapter.cs
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient
//
// Author: Tim Coleman <tim@timcoleman.com>
//
// Parts transferred from System.Data.SqlClient/SqlDataAdapter.cs
// Authors:
//      Rodrigo Moya (rodrigo@ximian.com)
//      Daniel Morgan (danmorg@sc.rr.com)
//      Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
// (C) Ximian, Inc 2002
//
// Licensed under the MIT/X11 License.
//

using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing.Design;

namespace System.Data.OracleClient
{
	[DefaultEvent ("RowUpdated")]
	[Designer ("Microsoft.VSDesigner.Data.VS.OracleDataAdapterDesigner, " + Consts.AssemblyMicrosoft_VSDesigner)]
	[ToolboxItem ("Microsoft.VSDesigner.Data.VS.OracleDataAdapterToolboxItem, " + Consts.AssemblyMicrosoft_VSDesigner)]
	public sealed class OracleDataAdapter : DbDataAdapter, IDbDataAdapter
	{
		#region Fields

		OracleCommand deleteCommand;
		OracleCommand insertCommand;
		OracleCommand selectCommand;
		OracleCommand updateCommand;
#if NET_2_0
		int updateBatchSize;
#endif

		#endregion

		#region Constructors

		public OracleDataAdapter () : this ((OracleCommand) null)
		{
		}

		public OracleDataAdapter (OracleCommand selectCommand)
		{
			SelectCommand = selectCommand;
#if NET_2_0
			UpdateBatchSize = 1;
#endif
		}

		public OracleDataAdapter (string selectCommandText, OracleConnection selectConnection)
			: this (new OracleCommand (selectCommandText, selectConnection))
		{
		}

		public OracleDataAdapter (string selectCommandText, string selectConnectionString)
			: this (selectCommandText, new OracleConnection (selectConnectionString))
		{
		}

		#endregion

		#region Properties

		[DefaultValue (null)]
		[Editor ("Microsoft.VSDesigner.Data.Design.DBCommandEditor, " + Consts.AssemblyMicrosoft_VSDesigner, typeof(UITypeEditor))]
		public
#if NET_2_0
		new
#endif
		OracleCommand DeleteCommand {
			get { return deleteCommand; }
			set { deleteCommand = value; }
		}

		[DefaultValue (null)]
		[Editor ("Microsoft.VSDesigner.Data.Design.DBCommandEditor, " + Consts.AssemblyMicrosoft_VSDesigner, typeof(UITypeEditor))]
		public
#if NET_2_0
		new
#endif
		OracleCommand InsertCommand {
			get { return insertCommand; }
			set { insertCommand = value; }
		}

		[DefaultValue (null)]
		[Editor ("Microsoft.VSDesigner.Data.Design.DBCommandEditor, " + Consts.AssemblyMicrosoft_VSDesigner, typeof(UITypeEditor))]
		public
#if NET_2_0
		new
#endif
		OracleCommand SelectCommand {
			get { return selectCommand; }
			set { selectCommand = value; }
		}

		[DefaultValue (null)]
		[Editor ("Microsoft.VSDesigner.Data.Design.DBCommandEditor, " + Consts.AssemblyMicrosoft_VSDesigner, typeof(UITypeEditor))]
		public
#if NET_2_0
		new
#endif
		OracleCommand UpdateCommand {
			get { return updateCommand; }
			set { updateCommand = value; }
		}

#if NET_2_0
		public override int UpdateBatchSize {
			get { return updateBatchSize; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("UpdateBatchSize");
				updateBatchSize = value; 
			}
		}
#endif

		IDbCommand IDbDataAdapter.DeleteCommand {
			get { return DeleteCommand; }
			set { DeleteCommand = (OracleCommand) value; }
		}

		IDbCommand IDbDataAdapter.InsertCommand {
			get { return InsertCommand; }
			set { InsertCommand = (OracleCommand) value; }
		}

		IDbCommand IDbDataAdapter.SelectCommand {
			get { return SelectCommand; }
			set { SelectCommand = (OracleCommand) value; }
		}

		IDbCommand IDbDataAdapter.UpdateCommand {
			get { return UpdateCommand; }
			set { UpdateCommand = (OracleCommand) value; }
		}

		#endregion // Properties

		#region Methods

		protected override RowUpdatedEventArgs CreateRowUpdatedEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
		{
			return new OracleRowUpdatedEventArgs (dataRow, command, statementType, tableMapping);
		}


		protected override RowUpdatingEventArgs CreateRowUpdatingEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
		{
			return new OracleRowUpdatingEventArgs (dataRow, command, statementType, tableMapping);
		}

		protected override void OnRowUpdated (RowUpdatedEventArgs value)
		{
			if (RowUpdated != null)
				RowUpdated (this, (OracleRowUpdatedEventArgs) value);
		}

		protected override void OnRowUpdating (RowUpdatingEventArgs value)
		{
			if (RowUpdating != null)
				RowUpdating (this, (OracleRowUpdatingEventArgs) value);
		}

		#endregion // Methods

		#region Events and Delegates

		public event OracleRowUpdatedEventHandler RowUpdated;
		public event OracleRowUpdatingEventHandler RowUpdating;

		#endregion // Events and Delegates
	}
}
