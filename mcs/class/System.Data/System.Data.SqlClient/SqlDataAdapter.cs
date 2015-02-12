//
// System.Data.SqlClient.SqlDataAdapter.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//	 Veerapuram Varadhan  (vvaradhan@novell.com)
//
// (C) Ximian, Inc 2002
// Copyright (C) 2002 Tim Coleman
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
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace System.Data.SqlClient {
	[DefaultEvent ("RowUpdated")]
	[DesignerAttribute ("Microsoft.VSDesigner.Data.VS.SqlDataAdapterDesigner, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.ComponentModel.Design.IDesigner")]
	[ToolboxItemAttribute ("Microsoft.VSDesigner.Data.VS.SqlDataAdapterToolboxItem, "+ Consts.AssemblyMicrosoft_VSDesigner)]

	public sealed class SqlDataAdapter : DbDataAdapter, IDbDataAdapter, IDataAdapter, ICloneable
	{
		#region Fields

		int updateBatchSize;
		#endregion

		#region Constructors
		
		public SqlDataAdapter () : this ((SqlCommand) null)
		{
		}

		public SqlDataAdapter (SqlCommand selectCommand) 
		{
			SelectCommand = selectCommand;
			UpdateBatchSize = 1;
		}

		public SqlDataAdapter (string selectCommandText, SqlConnection selectConnection) 
			: this (new SqlCommand (selectCommandText, selectConnection))
		{
		}

		public SqlDataAdapter (string selectCommandText, string selectConnectionString)
			: this (selectCommandText, new SqlConnection (selectConnectionString))
		{
		}

		#endregion

		#region Properties

		[DefaultValue (null)]
		[EditorAttribute ("Microsoft.VSDesigner.Data.Design.DBCommandEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
		public new SqlCommand DeleteCommand {
			get { 
				return (SqlCommand)base.DeleteCommand; 
			}
			set { 
				base.DeleteCommand = value; 
			}
		}

		[DefaultValue (null)]
		[EditorAttribute ("Microsoft.VSDesigner.Data.Design.DBCommandEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
		public new SqlCommand InsertCommand {
			get { 
				return (SqlCommand)base.InsertCommand; 
			}
			set { 
				base.InsertCommand = value; 
			}
		}

		[DefaultValue (null)]
		[EditorAttribute ("Microsoft.VSDesigner.Data.Design.DBCommandEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
		public new SqlCommand SelectCommand {
			get { 
				return (SqlCommand)base.SelectCommand; 
			}
			set { 
				base.SelectCommand = value; 
			}
		}

		[DefaultValue (null)]
		[EditorAttribute ("Microsoft.VSDesigner.Data.Design.DBCommandEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
		public new  SqlCommand UpdateCommand {
			get { 
				return (SqlCommand)base.UpdateCommand; 
			}
			set { 
				base.UpdateCommand = value; 
			}
		}
		
		IDbCommand IDbDataAdapter.SelectCommand {
			get { return SelectCommand; }
			set { SelectCommand = (SqlCommand) value; }
		}
		
		IDbCommand IDbDataAdapter.InsertCommand {
			get { return InsertCommand; }
			set { InsertCommand = (SqlCommand) value; }
		}
		
		IDbCommand IDbDataAdapter.UpdateCommand {
			get { return UpdateCommand; }
			set { UpdateCommand = (SqlCommand) value; }
		}
		IDbCommand IDbDataAdapter.DeleteCommand {
			get { return DeleteCommand; }
			set { DeleteCommand = (SqlCommand) value; }
		}

		public override int UpdateBatchSize {
			get { return updateBatchSize; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("UpdateBatchSize");
				updateBatchSize = value; 
			}
		}

		#endregion // Properties

		#region Methods

		protected override RowUpdatedEventArgs CreateRowUpdatedEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
		{
			return new SqlRowUpdatedEventArgs (dataRow, command, statementType, tableMapping);
		}


		protected override RowUpdatingEventArgs CreateRowUpdatingEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
		{
			return new SqlRowUpdatingEventArgs (dataRow, command, statementType, tableMapping);
		}


		protected override void OnRowUpdated (RowUpdatedEventArgs value) 
		{
			if (RowUpdated != null)
				RowUpdated (this, (SqlRowUpdatedEventArgs) value);
		}

		protected override void OnRowUpdating (RowUpdatingEventArgs value) 
		{
			if (RowUpdating != null)
				RowUpdating (this, (SqlRowUpdatingEventArgs) value);
		}

		[MonoTODO]
		object ICloneable.Clone()
		{
			throw new NotImplementedException ();
		}

		// All the batch methods, should be implemented, if supported,
		// by individual providers 

		[MonoTODO]
		protected override int AddToBatch (IDbCommand command)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void ClearBatch ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override int ExecuteBatch ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override IDataParameter GetBatchedParameter (int commandIdentifier, int  parameterIndex)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void InitializeBatching ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void TerminateBatching ()
		{
			throw new NotImplementedException ();
		}
		#endregion // Methods

		#region Events and Delegates

		public event SqlRowUpdatedEventHandler RowUpdated;

		public event SqlRowUpdatingEventHandler RowUpdating;

		#endregion // Events and Delegates
	}
}
