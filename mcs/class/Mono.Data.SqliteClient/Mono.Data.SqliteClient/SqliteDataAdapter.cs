//
// Mono.Data.SqliteClient.SqliteDataAdapter.cs
//
// Represents a set of data commands and a database connection that are used 
// to fill the DataSet and update the data source.
//
// Author(s): Everaldo Canuto  <everaldo_canuto@yahoo.com.br>
//
// Copyright (C) 2004  Everaldo Canuto
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
using System.Data;
using System.Data.Common;
using System.Collections;
using System.Text;

namespace Mono.Data.SqliteClient
{
	/// <summary>
	/// Represents a set of data commands and a database connection that are used 
	/// to fill the <see cref="DataSet">DataSet</see> and update the data source.
	/// </summary>
	public class SqliteDataAdapter : DbDataAdapter
#if !NET_2_0
	, IDbDataAdapter
#endif
	{
		#region Fields
		
#if !NET_2_0
		private IDbCommand _deleteCommand;
		private IDbCommand _insertCommand;
		private IDbCommand _selectCommand;
		private IDbCommand _updateCommand;
#endif
		
		#endregion

		#region Public Events
		
		/// <summary>
		/// Occurs during <see cref="DbDataAdapter.Update">Update</see> after a 
		/// command is executed against the data source. The attempt to update 
		/// is made, so the event fires.
		/// </summary>
		public event SqliteRowUpdatedEventHandler RowUpdated;
		
		/// <summary>
		/// Occurs during <see cref="DbDataAdapter.Update">Update</see> before a 
		/// command is executed against the data source. The attempt to update 
		/// is made, so the event fires.
		/// </summary>
		public event SqliteRowUpdatingEventHandler RowUpdating;
		
		#endregion

		#region Contructors
		
		/// <summary>
		/// Initializes a new instance of the <see cref="SqliteDataAdapter">SqliteDataAdapter</see> class.
		/// </summary>
		public SqliteDataAdapter() 
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="SqliteDataAdapter">SqliteDataAdapter</see> class 
		/// with the specified SqliteCommand as the SelectCommand property.
		/// </summary>
		/// <param name="selectCommand"></param>
#if NET_2_0
		public SqliteDataAdapter(DbCommand selectCommand)
#else
		public SqliteDataAdapter(IDbCommand selectCommand) 
#endif
		{
			SelectCommand = selectCommand;
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="SqliteDataAdapter">SqliteDataAdapter</see> class 
		/// with a SelectCommand and a SqliteConnection object.
		/// </summary>
		/// <param name="selectCommandText"></param>
		/// <param name="connection"></param>
		public SqliteDataAdapter(string selectCommandText, SqliteConnection connection)
		{
#if NET_2_0
			DbCommand cmd;
#else
			IDbCommand cmd;
#endif

			cmd = connection.CreateCommand();
			cmd.CommandText = selectCommandText;
			SelectCommand = cmd;
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="SqliteDataAdapter">SqliteDataAdapter</see> class 
		/// with a SelectCommand and a connection string.
		/// </summary>
		/// <param name="selectCommandText"></param>
		/// <param name="connectionString"></param>
		public SqliteDataAdapter(string selectCommandText, string connectionString) : this(selectCommandText ,new SqliteConnection(connectionString))
		{
		}
		
		#endregion

		#region Public Properties
		
#if !NET_2_0
		/// <summary>
		/// Gets or sets a Transact-SQL statement or stored procedure to delete 
		/// records from the data set.
		/// </summary>
		public IDbCommand DeleteCommand {
			get { return _deleteCommand; }
			set { _deleteCommand = value; }
		}
		
		/// <summary>
		/// Gets or sets a Transact-SQL statement or stored procedure to insert 
		/// new records into the data source.
		/// </summary>
		public IDbCommand InsertCommand {
			get { return _insertCommand; }
			set { _insertCommand = value; }
		}
		
		/// <summary>
		/// Gets or sets a Transact-SQL statement or stored procedure used to 
		/// select records in the data source.
		/// </summary>
		public IDbCommand SelectCommand {
			get { return _selectCommand; }
			set { _selectCommand = value; }
		}
		
		/// <summary>
		/// Gets or sets a Transact-SQL statement or stored procedure used to 
		/// update records in the data source.
		/// </summary>
		public IDbCommand UpdateCommand {
			get { return _updateCommand; }
			set { _updateCommand = value; }
		}
#endif		
		#endregion

		#region Protected Methods
		
		/// <summary>
		/// Initializes a new instance of the <see cref="RowUpdatedEventArgs">RowUpdatedEventArgs</see> class.
		/// </summary>
		/// <param name="dataRow">The DataRow used to update the data source.</param>
		/// <param name="command">The IDbCommand executed during the Update.</param>
		/// <param name="statementType">Whether the command is an UPDATE, INSERT, DELETE, or SELECT statement.</param>
		/// <param name="tableMapping">A DataTableMapping object.</param>
		/// <returns></returns>
		protected override RowUpdatedEventArgs CreateRowUpdatedEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
		{
			return new SqliteRowUpdatedEventArgs (dataRow, command, statementType, tableMapping);
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="dataRow">The DataRow used to update the data source.</param>
		/// <param name="command">The IDbCommand executed during the Update.</param>
		/// <param name="statementType">Whether the command is an UPDATE, INSERT, DELETE, or SELECT statement.</param>
		/// <param name="tableMapping">A DataTableMapping object.</param>
		/// <returns></returns>
		protected override RowUpdatingEventArgs CreateRowUpdatingEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
		{
			return new SqliteRowUpdatingEventArgs(dataRow, command, statementType, tableMapping);
		}
		
		/// <summary>
		/// Raises the RowUpdated event of a Sqlite data provider.
		/// </summary>
		/// <param name="args">A RowUpdatedEventArgs that contains the event data.</param>
		protected override void OnRowUpdating (RowUpdatingEventArgs args)
		{
			if (RowUpdating != null)
				RowUpdating(this, args);
		}
		
		/// <summary>
		/// Raises the RowUpdating event of Sqlite data provider.
		/// </summary>
		/// <param name="args">An RowUpdatingEventArgs that contains the event data.</param>
		protected override void OnRowUpdated (RowUpdatedEventArgs args)
		{
			if (RowUpdated != null)
				RowUpdated(this, args);
		}
		
		#endregion
	}
}
