/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2003, 2004 Abel Eduardo Pereira
 *  All Rights Reserved.
 */

using System;
using FirebirdSql.Data.Firebird;

namespace FirebirdSql.Data.Firebird.Isql
{
	#region Delegates

	/// <summary>
	/// The event handler type trigged after a SQL statement execution.
	/// </summary>
	public delegate void CommandExecutedEventHandler(object sender, CommandExecutedEventArgs e);

	#endregion

	/// <summary>
	/// Summary description for CommandExecutedEventArgs.
	/// </summary>
	public class CommandExecutedEventArgs
	{
		#region Fields

		private string		commandText;
		private FbDataReader	dataReader;
		private int		rowsAffected;

		#endregion

		#region Properties

		/// <summary>
		/// Returns the <see cref="SqlStatementType"/> of the current <see cref="CommandText"/>.
		/// </summary>
		public SqlStatementType StatementType 
		{
			get { return FbBatchExecution.GetStatementType(this.commandText); }
		}

		/// <summary>
		/// Returns the SQL statement that was executed.
		/// </summary>
		public string CommandText 
		{
			get { return this.commandText; }
		}

		/// <summary>
		/// Returns a <see cref="FbDataReader"/> instance case the executed SQL command returns data. If
		/// the executed SQL command does not returns data, (for instance: the case of an UPDATE statement), 
		/// the <b>DataReader</b> is setled to <b>null</b>.
		/// </summary>
		public FbDataReader DataReader
		{
			get { return this.dataReader; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Creates an instance of CommandExecutedEventArgs class.
		/// </summary>
		/// <param name="dataReader"></param>
		/// <param name="commandText">The CommandText of the <see cref="FbCommand"/> that was executed.</param>
		/// <param name="rowsAffected"></param>
		public CommandExecutedEventArgs(
			FbDataReader	dataReader,
			string		commandText,
			int		rowsAffected)
		{
			this.dataReader	 = dataReader;
			this.commandText = commandText;
			this.rowsAffected = rowsAffected;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Overrided. Returns the SQL statement that was executed.
		/// </summary>
		/// <returns>The SQL statement that will be executed.</returns>
		public override string ToString() 
		{
			return this.commandText;
		}

		#endregion
	}
}
