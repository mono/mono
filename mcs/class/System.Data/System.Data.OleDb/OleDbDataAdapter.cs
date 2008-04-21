//
// System.Data.OleDb.OleDbDataAdapter
//
// Authors:
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

using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace System.Data.OleDb
{
	[DefaultEvent ("RowUpdated")]
	[DesignerAttribute ("Microsoft.VSDesigner.Data.VS.OleDbDataAdapterDesigner, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.ComponentModel.Design.IDesigner")]
	[ToolboxItemAttribute ("Microsoft.VSDesigner.Data.VS.OleDbDataAdapterToolboxItem, "+ Consts.AssemblyMicrosoft_VSDesigner)]
	public sealed class OleDbDataAdapter : DbDataAdapter, IDbDataAdapter, ICloneable
	{
		#region Fields

		OleDbCommand deleteCommand;
		OleDbCommand insertCommand;
		OleDbCommand selectCommand;
		OleDbCommand updateCommand;

		#endregion

		#region Constructors

		public OleDbDataAdapter () : this ((OleDbCommand) null)
		{
		}

		public OleDbDataAdapter (OleDbCommand selectCommand)
		{
			SelectCommand = selectCommand;
		}

		public OleDbDataAdapter (string selectCommandText, OleDbConnection selectConnection)
			: this (new OleDbCommand (selectCommandText, selectConnection))
		{
		}

		public OleDbDataAdapter (string selectCommandText, string selectConnectionString)
			: this (selectCommandText, new OleDbConnection (selectConnectionString))
		{
		}

		#endregion // Fields

		#region Properties
		
		[DefaultValue (null)]
		[DataCategory ("Update")]
#if !NET_2_0
		[DataSysDescriptionAttribute ("Used during Update for deleted rows in DataSet.")]
#endif
		[EditorAttribute ("Microsoft.VSDesigner.Data.Design.DBCommandEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing)]
		public new OleDbCommand DeleteCommand {
			get {
				return deleteCommand;
			}
			set {
				deleteCommand = value;
			}
		}

		[DefaultValue (null)]
		[DataCategory ("Update")]
#if !NET_2_0
		[DataSysDescriptionAttribute ("Used during Update for new rows in DataSet.")]
#endif
		[EditorAttribute ("Microsoft.VSDesigner.Data.Design.DBCommandEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing)]
		public new OleDbCommand InsertCommand {
			get {
				return insertCommand;
			}
			set {
				insertCommand = value;
			}
		}

		[DefaultValue (null)]
		[DataCategory ("Fill")]
#if !NET_2_0
		[DataSysDescriptionAttribute ("Used during Fill/FillSchema.")]
#endif
		[EditorAttribute ("Microsoft.VSDesigner.Data.Design.DBCommandEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing)]
		public new OleDbCommand SelectCommand {
			get {
				return selectCommand;
			}
			set {
				selectCommand = value;
			}
		}

		[DefaultValue (null)]
		[DataCategory ("Update")]
#if !NET_2_0
		[DataSysDescriptionAttribute ("Used during Update for modified rows in DataSet.")]
#endif
		[EditorAttribute ("Microsoft.VSDesigner.Data.Design.DBCommandEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing)]
		public new OleDbCommand UpdateCommand {
			get { return updateCommand; }
			set { updateCommand = value; }
		}

		IDbCommand IDbDataAdapter.DeleteCommand {
			get { return DeleteCommand; }
			set { DeleteCommand = (OleDbCommand) value; }
		}

		IDbCommand IDbDataAdapter.InsertCommand {
			get { return InsertCommand; }
			set { InsertCommand = (OleDbCommand) value; }
		}

		IDbCommand IDbDataAdapter.SelectCommand {
			get {
				return SelectCommand;
			}
			set {
				SelectCommand = (OleDbCommand) value;
			}
		}

		IDbCommand IDbDataAdapter.UpdateCommand {
			get { return UpdateCommand; }
			set { UpdateCommand = (OleDbCommand) value; }
		}

		#endregion // Properties

		#region Methods

		protected override RowUpdatedEventArgs CreateRowUpdatedEvent (DataRow dataRow,
									      IDbCommand command,
									      StatementType statementType,
									      DataTableMapping tableMapping) 
		{
			return new OleDbRowUpdatedEventArgs (dataRow, command, statementType, tableMapping);
		}


		protected override RowUpdatingEventArgs CreateRowUpdatingEvent (DataRow dataRow,
										IDbCommand command,
										StatementType statementType,
										DataTableMapping tableMapping)
		{
			return new OleDbRowUpdatingEventArgs (dataRow, command, statementType, tableMapping);
		}

		protected override void OnRowUpdated (RowUpdatedEventArgs value)
		{
			if (RowUpdated != null)
				RowUpdated (this, (OleDbRowUpdatedEventArgs) value);
		}

		protected override void OnRowUpdating (RowUpdatingEventArgs value)
		{
			if (RowUpdating != null)
				RowUpdating (this, (OleDbRowUpdatingEventArgs) value);
		}
		
		[MonoTODO]
		public int Fill (DataTable dataTable, Object ADODBRecordSet)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Fill (DataSet dataSet, Object ADODBRecordSet, String srcTable)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		object ICloneable.Clone ()
		{
			throw new NotImplementedException ();
		}

#if !NET_2_0
		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}
#endif

		#endregion // Methods

		#region Events and Delegates

#if !NET_2_0
		[DataSysDescription ("Event triggered before every DataRow during Update.")]
#endif
		[DataCategory ("DataCategory_Update")]
		public event OleDbRowUpdatedEventHandler RowUpdated;

#if !NET_2_0
		[DataSysDescription ("Event triggered after every DataRow during Update.")]
#endif
		[DataCategory ("DataCategory_Update")]
		public event OleDbRowUpdatingEventHandler RowUpdating;

		#endregion // Events and Delegates
	}
}
