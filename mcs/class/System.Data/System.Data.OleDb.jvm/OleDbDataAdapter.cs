//
// System.Data.OleDb.OleDbDataAdapter
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//	Konstantin Triger <kostat@mainsoft.com>
//	Boris Kirzner <borisk@mainsoft.com>
//
// Copyright (C) Rodrigo Moya, 2002
// Copyright (C) Tim Coleman, 2002
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
//

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
	public sealed class OleDbDataAdapter : DbDataAdapter, IDbDataAdapter
	{
		#region Fields

		OleDbCommand deleteCommand;
		OleDbCommand insertCommand;
		OleDbCommand selectCommand;
		OleDbCommand updateCommand;

		#endregion

		#region Constructors

		public OleDbDataAdapter ()
			: this (new OleDbCommand ())
		{
		}

		public OleDbDataAdapter (OleDbCommand selectCommand)
		{
			DeleteCommand = new OleDbCommand ();
			InsertCommand = new OleDbCommand ();
			SelectCommand = selectCommand;
			UpdateCommand = new OleDbCommand ();
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

		public OleDbCommand DeleteCommand {
			get {
				return deleteCommand;
			}
			set {
				deleteCommand = value;
			}
		}

		public OleDbCommand InsertCommand {
			get {
				return insertCommand;
			}
			set {
				insertCommand = value;
			}
		}

		public OleDbCommand SelectCommand {
			get {
				return selectCommand;
			}
			set {
				selectCommand = value;
			}
		}

		public OleDbCommand UpdateCommand {
			get {
				return updateCommand;
			}
			set {
				updateCommand = value;
			}
		}

		IDbCommand IDbDataAdapter.DeleteCommand {
			get {
				return DeleteCommand;
			}
			set { 
				if (value != null && !(value is OleDbCommand))
					throw new ArgumentException ("DeleteCommand is not of Type OleDbCommand");
				DeleteCommand = (OleDbCommand)value;
			}
		}

		IDbCommand IDbDataAdapter.InsertCommand {
			get {
				return InsertCommand;
			}
			set { 
				if (value != null && !(value is OleDbCommand))
					throw new ArgumentException ("InsertCommand is not of Type OleDbCommand");
				InsertCommand = (OleDbCommand)value;
			}
		}

		IDbCommand IDbDataAdapter.SelectCommand {
			get {
				return SelectCommand;
			}
			set { 
				if (value != null && !(value is OleDbCommand))
					throw new ArgumentException ("SelectCommand is not of Type OleDbCommand");
				SelectCommand = (OleDbCommand)value;
			}
		}

		
		IDbCommand IDbDataAdapter.UpdateCommand {
			get {
				return UpdateCommand;
			}
			set { 
				if (value != null && !(value is OleDbCommand))
					throw new ArgumentException ("UpdateCommand is not of Type OleDbCommand");
				UpdateCommand = (OleDbCommand)value;
			}
		}

		ITableMappingCollection IDataAdapter.TableMappings {
			get {
				return TableMappings;
			}
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
		
		#endregion // Methods

		#region Events and Delegates

		public event OleDbRowUpdatedEventHandler RowUpdated;
		public event OleDbRowUpdatingEventHandler RowUpdating;

		#endregion // Events and Delegates

	}
}
