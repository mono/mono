/*
 *	Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *	   The contents of this file are subject to the Initial 
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this file except in compliance with the 
 *	   License. You may obtain a copy of the License at 
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on 
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied. See the License for the specific 
 *	   language governing rights and limitations under the License.
 * 
 *	Copyright (c) 2003, 2005 Abel Eduardo Pereira
 *	All Rights Reserved.
 */

using System;
using FirebirdSql.Data.Firebird;

namespace FirebirdSql.Data.Firebird.Isql
{
	#region Delegates

	/// <summary>
	/// The	event handler type trigged before a	SQL	statement execution.
	/// </summary>
	public delegate	void CommandExecutingEventHandler(object sender, CommandExecutingEventArgs e);

	#endregion

	/// <summary>
	/// CommandExecutingEventArgs encapsulates the events arguments	for	the	event trigged 
	/// from the <see cref="FbBatchExecution"/>	during the execution. 
	/// </summary>
	/// <remarks>
	/// 
	/// </remarks>
	public class CommandExecutingEventArgs:	EventArgs
	{
		#region Private

		private	FbCommand sqlCommand;

		#endregion

		#region Properties

		/// <summary>
		/// Returns	the	<see cref="FbCommand"/>	instance that created for the SQL statement	that goes 
		/// for	execution. 
		/// </summary>
		public FbCommand SqlCommand	
		{
			get	{ return this.sqlCommand; }
		}

		/// <summary>
		/// Returns	the	<see cref="SqlStatementType"/> of the current <see cref="SqlCommand"/>.
		/// </summary>
		public SqlStatementType	StatementType 
		{
			get	{ return FbBatchExecution.GetStatementType(this.SqlCommand.CommandText); }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Creates	an instance	of CommandExecutingEventArgs class.
		/// </summary>
		/// <param name="sqlCommand">The FbCommand properly	instanciated.</param>
		/// <remarks>The <b>sqlCommand</b> should be proper	instanciated with a	valid 
		/// <see cref="FbCommand"/> and with the SQL statement loaded in <see cref="FbCommand.CommandText"/>.
		/// </remarks>
		public CommandExecutingEventArgs(FbCommand sqlCommand)
		{
			this.sqlCommand = sqlCommand;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Overrided. Returns the SQL statement that goes for execution.
		/// </summary>
		/// <returns>The SQL statement that	will be	executed.</returns>
		public override	string ToString() 
		{
			return this.sqlCommand.CommandText;
		}

		#endregion
	}
}
