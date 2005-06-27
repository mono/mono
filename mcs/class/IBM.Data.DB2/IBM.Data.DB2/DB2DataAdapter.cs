
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

namespace IBM.Data.DB2 {
	
	public sealed class DB2DataAdapter : DbDataAdapter, IDbDataAdapter 
	{
		#region Fields

		bool disposed = false;	
		DB2Command deleteCommand;
		DB2Command insertCommand;
		DB2Command selectCommand;
		DB2Command updateCommand;

		#endregion

		#region Constructors
		
		public DB2DataAdapter () 	
			: this (new DB2Command ())
		{
		}

		public DB2DataAdapter (DB2Command selectCommand) 
		{
			DeleteCommand = null;
			InsertCommand = null;
			SelectCommand = selectCommand;
			UpdateCommand = null;
		}

		public DB2DataAdapter (string selectCommandText, DB2Connection selectConnection) 
			: this (new DB2Command (selectCommandText, selectConnection))
		{ 
		}

		public DB2DataAdapter (string selectCommandText, string selectConnectionString)
			: this (selectCommandText, new DB2Connection (selectConnectionString))
		{
		}

		#endregion

		#region Properties


		public DB2Command DeleteCommand {
			get { return deleteCommand; }
			set { deleteCommand = value; }
		}


		public DB2Command InsertCommand {
			get { return insertCommand; }
			set { insertCommand = value; }
		}


		public DB2Command SelectCommand {
			get { return selectCommand; }
			set { selectCommand = value; }
		}


		public DB2Command UpdateCommand {
			get { return updateCommand; }
			set { updateCommand = value; }
		}

		IDbCommand IDbDataAdapter.DeleteCommand {
			get { return DeleteCommand; }
			set { 
				if (!(value is DB2Command)) 
					throw new ArgumentException ();
				DeleteCommand = (DB2Command)value;
			}
		}

		IDbCommand IDbDataAdapter.InsertCommand {
			get { return InsertCommand; }
			set { 
				if (!(value is DB2Command)) 
					throw new ArgumentException ();
				InsertCommand = (DB2Command)value;
			}
		}

		IDbCommand IDbDataAdapter.SelectCommand {
			get { return SelectCommand; }
			set { 
				if (!(value is DB2Command)) 
					throw new ArgumentException ();
				SelectCommand = (DB2Command)value;
			}
		}

		IDbCommand IDbDataAdapter.UpdateCommand {
			get { return UpdateCommand; }
			set { 
				if (!(value is DB2Command)) 
					throw new ArgumentException ();
				UpdateCommand = (DB2Command)value;
			}
		}


		ITableMappingCollection IDataAdapter.TableMappings {
			get { return TableMappings; }
		}

		#endregion 

		#region Methods

		protected override RowUpdatedEventArgs CreateRowUpdatedEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
		{
			return new DB2RowUpdatedEventArgs (dataRow, command, statementType, tableMapping);
		}


		protected override RowUpdatingEventArgs CreateRowUpdatingEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
		{
			return new DB2RowUpdatingEventArgs (dataRow, command, statementType, tableMapping);
		}

		protected override void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					
				}
				
				disposed = true;
			}
		}

		protected override void OnRowUpdated (RowUpdatedEventArgs value) 
		{
			if (RowUpdated != null)
				RowUpdated (this, (DB2RowUpdatedEventArgs) value);
		}

		protected override void OnRowUpdating (RowUpdatingEventArgs value) 
		{
			if (RowUpdating != null)
				RowUpdating (this, (DB2RowUpdatingEventArgs) value);
		}

		#endregion 

		#region Events and Delegates

		public event DB2RowUpdatedEventHandler RowUpdated;

		public event DB2RowUpdatingEventHandler RowUpdating;

		#endregion 

	}
}
