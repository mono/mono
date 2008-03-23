//
// System.Data.Odbc.OdbcDataAdapter.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc 2002
// Copyright (C) 2002 Tim Coleman
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

namespace System.Data.Odbc
{
	[DefaultEvent ("RowUpdated")]
	[DesignerAttribute ("Microsoft.VSDesigner.Data.VS.OdbcDataAdapterDesigner, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.ComponentModel.Design.IDesigner")]
	[ToolboxItemAttribute ("Microsoft.VSDesigner.Data.VS.OdbcDataAdapterToolboxItem, "+ Consts.AssemblyMicrosoft_VSDesigner)]
	public sealed class OdbcDataAdapter : DbDataAdapter, IDbDataAdapter, ICloneable
	{
		#region Fields

#if ONLY_1_1
		bool disposed;
#endif
		OdbcCommand deleteCommand;
		OdbcCommand insertCommand;
		OdbcCommand selectCommand;
		OdbcCommand updateCommand;

		#endregion

		#region Constructors
		
		public OdbcDataAdapter () : this ((OdbcCommand) null)
		{
		}

		public OdbcDataAdapter (OdbcCommand selectCommand) 
		{
			SelectCommand = selectCommand;
		}

		public OdbcDataAdapter (string selectCommandText, OdbcConnection selectConnection) 
			: this (new OdbcCommand (selectCommandText, selectConnection))
		{ 
		}

		public OdbcDataAdapter (string selectCommandText, string selectConnectionString)
			: this (selectCommandText, new OdbcConnection (selectConnectionString))
		{
		}

		#endregion

		#region Properties

		[OdbcCategory ("Update")]
		[OdbcDescription ("Used during Update for deleted rows in DataSet.")]
		[DefaultValue (null)]
		[EditorAttribute ("Microsoft.VSDesigner.Data.Design.DBCommandEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
		public new OdbcCommand DeleteCommand {
			get { return deleteCommand; }
			set { deleteCommand = value; }
		}

		[OdbcCategory ("Update")]
		[OdbcDescription ("Used during Update for new rows in DataSet.")]
		[DefaultValue (null)]
		[EditorAttribute ("Microsoft.VSDesigner.Data.Design.DBCommandEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
		public new OdbcCommand InsertCommand {
			get { return insertCommand; }
			set { insertCommand = value; }
		}

		[OdbcCategory ("Fill")]
		[OdbcDescription ("Used during Fill/FillSchema.")]
		[DefaultValue (null)]
		[EditorAttribute ("Microsoft.VSDesigner.Data.Design.DBCommandEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
		public new OdbcCommand SelectCommand {
			get { return selectCommand; }
			set { selectCommand = value; }
		}

		[OdbcCategory ("Update")]
		[OdbcDescription ("Used during Update for modified rows in DataSet.")]
		[DefaultValue (null)]
		[EditorAttribute ("Microsoft.VSDesigner.Data.Design.DBCommandEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
		public new OdbcCommand UpdateCommand {
			get { return updateCommand; }
			set { updateCommand = value; }
		}

		IDbCommand IDbDataAdapter.DeleteCommand {
			get { return DeleteCommand; }
			set { DeleteCommand = (OdbcCommand) value; }
		}

		IDbCommand IDbDataAdapter.InsertCommand {
			get { return InsertCommand; }
			set { InsertCommand = (OdbcCommand) value; }
		}

		IDbCommand IDbDataAdapter.SelectCommand {
			get { return SelectCommand; }
			set { SelectCommand = (OdbcCommand) value; }
		}

		IDbCommand IDbDataAdapter.UpdateCommand {
			get { return UpdateCommand; }
			set { UpdateCommand = (OdbcCommand) value; }
		}

		#endregion // Properties

		#region Methods

		protected override RowUpdatedEventArgs CreateRowUpdatedEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
		{
			return new OdbcRowUpdatedEventArgs (dataRow, command, statementType, tableMapping);
		}


		protected override RowUpdatingEventArgs CreateRowUpdatingEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
		{
			return new OdbcRowUpdatingEventArgs (dataRow, command, statementType, tableMapping);
		}

#if ONLY_1_1
		protected override void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					// Release managed resources
				}
				// Release unmanaged resources
				disposed = true;
			}
			base.Dispose (true);
		}
#endif

		protected override void OnRowUpdated (RowUpdatedEventArgs value)
		{
			if (RowUpdated != null)
				RowUpdated (this, (OdbcRowUpdatedEventArgs) value);
		}

		protected override void OnRowUpdating (RowUpdatingEventArgs value)
		{
			if (RowUpdating != null)
				RowUpdating (this, (OdbcRowUpdatingEventArgs) value);
		}

		[MonoTODO]
		object ICloneable.Clone ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods

		#region Events and Delegates

		public event OdbcRowUpdatedEventHandler RowUpdated;
		public event OdbcRowUpdatingEventHandler RowUpdating;

		#endregion // Events and Delegates
	}
}
