/*
 *	Firebird ADO.NET Data provider for .NET	and	Mono 
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
 *	Copyright (c) 2003,	2004 Abel Eduardo Pereira
 *	All Rights Reserved.
 */

using System;
using System.Data;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;

using FirebirdSql.Data.Firebird;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Firebird.Isql
{
	#region Enumerations

	/// <summary>
	/// DSQL and ISQL statement	types.
	/// </summary>
	public enum	SqlStatementType 
	{
		/// <summary>
		/// Represents the SQL statement: <b>ALTER DATABASE</b>
		/// </summary>
		AlterDatabase = 0,

		/// <summary>
		/// Represents the SQL statement: <b>ALTER DOMAIN</b>
		/// </summary>
		AlterDomain,

		/// <summary>
		/// Represents the SQL statement: <b>ALTER EXCEPTION</b>
		/// </summary>
		AlterException,

		/// <summary>
		/// Represents the SQL statement: <b>ALTER INDEX</b>
		/// </summary>
		AlterIndex,

		/// <summary>
		/// Represents the SQL statement: <b>ALTER PROCEDURE</b>
		/// </summary>
		AlterProcedure,

		/// <summary>
		/// Represents the SQL statement: <b>ALTER TABLE</b>
		/// </summary>
		AlterTable,

		/// <summary>
		/// Represents the SQL statement: <b>ALTER TRIGGER</b>
		/// </summary>
		AlterTrigger,

		/// <summary>
		/// Represents the SQL statement: <b>CLOSE</b>
		/// </summary>
		Close,

		/// <summary>
		/// Represents the SQL statement: <b>COMMIT</b>
		/// </summary>
		Commit,

		/// <summary>
		/// Represents the SQL statement: <b>CONNECT</b>
		/// </summary>
		Connect,

		/// <summary>
		/// Represents the SQL statement: <b>CREATE	DATABASE</b>
		/// </summary>
		CreateDatabase,

		/// <summary>
		/// Represents the SQL statement: <b>CREATE	DOMAIN</b>
		/// </summary>
		CreateDomain,

		/// <summary>
		/// Represents the SQL statement: <b>CREATE	EXCEPTION</b>
		/// </summary>
		CreateException,

		/// <summary>
		/// Represents the SQL statement: <b>CREATE	GENERATOR</b>
		/// </summary>
		CreateGenerator,

		/// <summary>
		/// Represents the SQL statement: <b>CREATE	INDEX</b>
		/// </summary>
		CreateIndex,

		/// <summary>
		/// Represents the SQL statement: <b>CREATE	PROCEDURE</b>
		/// </summary>
		CreateProcedure,

		/// <summary>
		/// Represents the SQL statement: <b>CREATE	ROLE</b>
		/// </summary>
		CreateRole,

		/// <summary>
		/// Represents the SQL statement: <b>CREATE	SHADOW</b>
		/// </summary>
		CreateShadow,

		/// <summary>
		/// Represents the SQL statement: <b>CREATE	TABLE</b>
		/// </summary>
		CreateTable,

		/// <summary>
		/// Represents the SQL statement: <b>CREATE	TRIGGER</b>
		/// </summary>
		CreateTrigger,

		/// <summary>
		/// Represents the SQL statement: <b>CREATE	VIEW</b>
		/// </summary>
		CreateView,

		/// <summary>
		/// Represents the SQL statement: <b>DECLARE CURSOR</b>
		/// </summary>
		DeclareCursor,

		/// <summary>
		/// Represents the SQL statement: <b>DECLARE EXTERNAL FUNCTION</b>
		/// </summary>
		DeclareExternalFunction,

		/// <summary>
		/// Represents the SQL statement: <b>DECLARE FILTER</b>
		/// </summary>
		DeclareFilter,

		/// <summary>
		/// Represents the SQL statement: <b>DECLARE STATEMENT</b>
		/// </summary>
		DeclareStatement,

		/// <summary>
		/// Represents the SQL statement: <b>DECLARE TABLE</b>
		/// </summary>
		DeclareTable,

		/// <summary>
		/// Represents the SQL statement: <b>DELETE</b>
		/// </summary>
		Delete,

		/// <summary>
		/// Represents the SQL statement: <b>DESCRIBE</b>
		/// </summary>
		Describe,

		/// <summary>
		/// Represents the SQL statement: <b>DISCONNECT</b>
		/// </summary>
		Disconnect,

		/// <summary>
		/// Represents the SQL statement: <b>DROP DATABASE</b>
		/// </summary>
		DropDatabase,

		/// <summary>
		/// Represents the SQL statement: <b>DROP DOMAIN</b>
		/// </summary>
		DropDomain,

		/// <summary>
		/// Represents the SQL statement: <b>DROP EXCEPTION</b>
		/// </summary>
		DropException,

		/// <summary>
		/// Represents the SQL statement: <b>DROP EXTERNAL FUNCTION</b>
		/// </summary>
		DropExternalFunction,

		/// <summary>
		/// Represents the SQL statement: <b>DROP FILTER</b>
		/// </summary>
		DropFilter,

		/// <summary>
		/// Represents the SQL statement: <b>DROP GENERATOR</b>
		/// </summary>
		DropGenerator,

		/// <summary>
		/// Represents the SQL statement: <b>DROP INDEX</b>
		/// </summary>
		DropIndex,

		/// <summary>
		/// Represents the SQL statement: <b>DROP PROCEDURE</b>
		/// </summary>
		DropProcedure,

		/// <summary>
		/// Represents the SQL statement: <b>DROP ROLE</b>
		/// </summary>
		DropRole,

		/// <summary>
		/// Represents the SQL statement: <b>DROP SHADOW</b>
		/// </summary>
		DropShadow,

		/// <summary>
		/// Represents the SQL statement: <b>DROP TABLE</b>
		/// </summary>
		DropTable,

		/// <summary>
		/// Represents the SQL statement: <b>DROP TRIGGER</b>
		/// </summary>
		DropTrigger,

		/// <summary>
		/// Represents the SQL statement: <b>DROP VIEW</b>
		/// </summary>
		DropView,

		/// <summary>
		/// Represents the SQL statement: <b>END DECLARE SECTION</b>
		/// </summary>
		EndDeclareSection,

		/// <summary>
		/// Represents the SQL statement: <b>EVENT INIT</b>
		/// </summary>
		EventInit,

		/// <summary>
		/// Represents the SQL statement: <b>EVENT WAIT</b>
		/// </summary>
		EventWait,

		/// <summary>
		/// Represents the SQL statement: <b>EXECUTE</b>
		/// </summary>
		Execute,

		/// <summary>
		/// Represents the SQL statement: <b>EXECUTE IMMEDIATE</b>
		/// </summary>
		ExecuteImmediate,

		/// <summary>
		/// Represents the SQL statement: <b>EXECUTE PROCEDURE</b>
		/// </summary>
		ExecuteProcedure,

		/// <summary>
		/// Represents the SQL statement: <b>FETCH</b>
		/// </summary>
		Fetch,

		/// <summary>
		/// Represents the SQL statement: <b>GRANT</b>
		/// </summary>
		Grant,

		/// <summary>
		/// Represents the SQL statement: <b>INSERT</b>
		/// </summary>
		Insert,

		/// <summary>
		/// Represents the SQL statement: <b>INSERT	CURSOR</b>
		/// </summary>
		InsertCursor,

		/// <summary>
		/// Represents the SQL statement: <b>OPEN</b>
		/// </summary>
		Open,

		/// <summary>
		/// Represents the SQL statement: <b>PREPARE</b>
		/// </summary>
		Prepare,

		/// <summary>
		/// Represents the SQL statement: <b>REVOKE</b>
		/// </summary>
		Revoke,

		/// <summary>
		/// Represents the SQL statement: <b>ROLLBACK</b>
		/// </summary>
		Rollback,

		/// <summary>
		/// Represents the SQL statement: <b>SELECT</b>
		/// </summary>
		Select,

		/// <summary>
		/// Represents the SQL statement: <b>SET DATABASE</b>
		/// </summary>
		SetDatabase,

		/// <summary>
		/// Represents the SQL statement: <b>SET GENERATOR</b>
		/// </summary>
		SetGenerator,

		/// <summary>
		/// Represents the SQL statement: <b>SET NAMES</b>
		/// </summary>
		SetNames,

		/// <summary>
		/// Represents the SQL statement: <b>SET SQL DIALECT</b>
		/// </summary>
		SetSQLDialect,

		/// <summary>
		/// Represents the SQL statement: <b>SET STATISTICS</b>
		/// </summary>
		SetStatistics,

		/// <summary>
		/// Represents the SQL statement: <b>SET TRANSACTION</b>
		/// </summary>
		SetTransaction,

		/// <summary>
		/// Represents the SQL statement: <b>SHOW SQL DIALECT</b>
		/// </summary>
		ShowSQLDialect,

		/// <summary>
		/// Represents the SQL statement: <b>UPDATE</b>
		/// </summary>
		Update,

		/// <summary>
		/// Represents the SQL statement: <b>WHENEVER</b>
		/// </summary>
		Whenever
	}

	#endregion
	
	/// <summary>
	/// Summary	description	for	BatchExecution.
	/// </summary>
	public class FbBatchExecution
	{
		#region Events

		/// <summary>
		/// The	event trigged before a SQL statement goes for execution.
		/// </summary>
		public event CommandExecutingEventHandler CommandExecuting;

		/// <summary>
		/// The	event trigged after	a SQL statement	execution.
		/// </summary>
		public event CommandExecutedEventHandler CommandExecuted;

		#endregion

		#region Fields

		private	StringCollection			sqlStatements;
		private	FbConnection				sqlConnection;
		private	FbTransaction				sqlTransaction;
		private	FbConnectionStringBuilder	connectionString;
		private	FbCommand					sqlCommand;

		// control fields
		private	bool requiresNewConnection;

		#endregion

		#region Properties

		/// <summary>
		/// Represents the list	of SQL statements for batch	execution.
		/// </summary>
		public StringCollection	SqlStatements 
		{
			get	
			{
				if (this.sqlStatements == null)
				{
					this.sqlStatements = new StringCollection();
				}
				return this.sqlStatements; 
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Creates	an instance	of FbBatchExecution	engine.
		/// </summary>
		public FbBatchExecution()
		{
			this.sqlConnection	 = new FbConnection(); // do	not	specify	the	connection string
			this.connectionString = new FbConnectionStringBuilder();
		}

		/// <summary>
		/// Creates	an instance	of FbBatchExecution	engine with	the	given
		/// connection.
		/// </summary>
		/// <param name="sqlConnection">A <see cref="FbConnection"/> object.</param>
		public FbBatchExecution(FbConnection sqlConnection)
		{
			if (sqlConnection == null)
			{
				this.sqlConnection	 = new FbConnection(); // do	not	specify	the	connection string
				this.connectionString = new FbConnectionStringBuilder();
			}
			else
			{
				this.sqlConnection	 = sqlConnection;
				this.connectionString = new FbConnectionStringBuilder(sqlConnection.ConnectionString);
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Starts the ordered execution of	the	SQL	statements that	are	in <see	cref="SqlStatements"/> collection.
		/// </summary>
		public virtual void	Execute()
		{
			this.Execute(true);
		}

		/// <summary>
		/// Starts the ordered execution of	the	SQL	statements that	are	in <see	cref="SqlStatements"/> collection.
		/// </summary>
		/// <param name="autoCommit">Specifies if the transaction should be	committed after	a DDL command execution</param>
		public virtual void	Execute(bool autoCommit) 
		{
			if (this.SqlStatements == null || this.SqlStatements.Count == 0)	
			{
				throw new InvalidOperationException("There are no commands for execution.");
			}

			foreach	(string	sqlStatement in	this.SqlStatements)
			{
				if (sqlStatement ==	null ||	sqlStatement.Length == 0)
				{
					continue;
				}
				
				// initializate	outputs	to default
				int				rowsAffected = -1;
				FbDataReader	dataReader	 = null;
				SqlStatementType statementType = FbBatchExecution.GetStatementType(sqlStatement);

				// Update command configuration
				this.ProvideCommand().CommandText = sqlStatement;

				// Check how transactions are going	to be handled
				if (statementType == SqlStatementType.Insert ||
					statementType == SqlStatementType.Update ||
					statementType == SqlStatementType.Delete)
				{
					// DML commands	should be inside a transaction
					if (this.sqlTransaction == null)
					{
						this.sqlTransaction = this.sqlConnection.BeginTransaction();
					}
					this.sqlCommand.Transaction = this.sqlTransaction;
				}
				else if	(this.sqlTransaction !=	null &&	(statementType != SqlStatementType.Commit && statementType != SqlStatementType.Rollback))
				{
					// Non DML Statements should be	executed using
					// implicit	transaction	support
					this.sqlTransaction.Commit();
				}

				try
				{
					switch (statementType)
					{
						case SqlStatementType.AlterDatabase:
						case SqlStatementType.AlterDomain:
						case SqlStatementType.AlterException:
						case SqlStatementType.AlterIndex:
						case SqlStatementType.AlterProcedure:
						case SqlStatementType.AlterTable:
						case SqlStatementType.AlterTrigger:
							// raise the event
							this.OnCommandExecuting(this.sqlCommand);

							rowsAffected = this.ExecuteCommand(this.sqlCommand,	autoCommit);
							this.requiresNewConnection = false;

							// raise the event
							this.OnCommandExecuted(sqlStatement, null, rowsAffected);
							break;

						case SqlStatementType.Commit:
							// raise the event
							this.OnCommandExecuting(null);

							this.sqlTransaction.Commit();
							this.sqlTransaction = null;
							
							// raise the event
							this.OnCommandExecuted(sqlStatement, null, -1);
							break;

						case SqlStatementType.Connect:
							// raise the event
							this.OnCommandExecuting(null);

							this.ConnectToDatabase(sqlStatement);

							requiresNewConnection = false;

							// raise the event
							this.OnCommandExecuted(sqlStatement, null, -1);
							break;

						case SqlStatementType.CreateDatabase:
#if	(!NETCF)
							throw new NotImplementedException();
#else
							throw new NotSupportedException();
#endif

						case SqlStatementType.CreateDomain:
						case SqlStatementType.CreateException:
						case SqlStatementType.CreateGenerator:
						case SqlStatementType.CreateIndex:
						case SqlStatementType.CreateProcedure:
						case SqlStatementType.CreateRole:
						case SqlStatementType.CreateShadow:
						case SqlStatementType.CreateTable:
						case SqlStatementType.CreateTrigger:
						case SqlStatementType.CreateView:
						case SqlStatementType.DeclareCursor:
						case SqlStatementType.DeclareExternalFunction:
						case SqlStatementType.DeclareFilter:
						case SqlStatementType.DeclareStatement:
						case SqlStatementType.DeclareTable:
						case SqlStatementType.Delete:
							// raise the event
							this.OnCommandExecuting(this.sqlCommand);

							rowsAffected = this.ExecuteCommand(this.sqlCommand,	autoCommit);
							requiresNewConnection = false;

							// raise the event
							this.OnCommandExecuted(sqlStatement, null, rowsAffected);
							break;

						case SqlStatementType.Describe:
							break;

						case SqlStatementType.Disconnect:
							this.sqlConnection.Close();
							this.requiresNewConnection = false;
							break;

						case SqlStatementType.DropDatabase:
#if	(!NETCF)
							throw new NotImplementedException();
#else
							throw new NotSupportedException();
#endif

						case SqlStatementType.DropDomain:
						case SqlStatementType.DropException:
						case SqlStatementType.DropExternalFunction:
						case SqlStatementType.DropFilter:
						case SqlStatementType.DropGenerator:
						case SqlStatementType.DropIndex:
						case SqlStatementType.DropProcedure:
						case SqlStatementType.DropRole:
						case SqlStatementType.DropShadow:
						case SqlStatementType.DropTable:
						case SqlStatementType.DropTrigger:
						case SqlStatementType.DropView:
						case SqlStatementType.EventInit:
						case SqlStatementType.EventWait:
						case SqlStatementType.Execute:
						case SqlStatementType.ExecuteImmediate:
							this.ProvideCommand().CommandText = sqlStatement;

							// raise the event
							this.OnCommandExecuting(this.sqlCommand);

							rowsAffected = this.ExecuteCommand(this.sqlCommand,	autoCommit);
							this.requiresNewConnection = false;

							// raise the event
							this.OnCommandExecuted(sqlStatement, null, rowsAffected);
							break;

						case SqlStatementType.ExecuteProcedure:
						case SqlStatementType.Fetch:
							break;

						case SqlStatementType.Grant:
						case SqlStatementType.Insert:
						case SqlStatementType.InsertCursor:
						case SqlStatementType.Open:
						case SqlStatementType.Prepare:
						case SqlStatementType.Revoke:
							// raise the event
							this.OnCommandExecuting(this.sqlCommand);

							rowsAffected = this.ExecuteCommand(this.sqlCommand,	autoCommit);
							this.requiresNewConnection = false;

							// raise the event
							this.OnCommandExecuted(sqlStatement, null, rowsAffected);
							break;

						case SqlStatementType.Rollback:
							// raise the event
							this.OnCommandExecuting(null);

							this.sqlTransaction.Rollback();
							this.sqlTransaction = null;
							
							// raise the event
							this.OnCommandExecuted(sqlStatement, null, -1);
							break;

						case SqlStatementType.Select:
							this.ProvideCommand().CommandText = sqlStatement;

							// raise the event
							this.OnCommandExecuting(this.sqlCommand);

							dataReader = this.sqlCommand.ExecuteReader();
							this.requiresNewConnection = false;

							// raise the event
							this.OnCommandExecuted(sqlStatement, dataReader, -1);
							if (!dataReader.IsClosed)
							{
								dataReader.Close();
							}
							break;

						case SqlStatementType.SetDatabase:
						case SqlStatementType.SetGenerator:	
						case SqlStatementType.SetNames:
						case SqlStatementType.SetSQLDialect:
						case SqlStatementType.SetStatistics:
						case SqlStatementType.SetTransaction:
						case SqlStatementType.ShowSQLDialect:
#if	(!NETCF)
							throw new NotImplementedException();
#else
							throw new NotSupportedException();
#endif
							
						case SqlStatementType.Update:
						case SqlStatementType.Whenever:
							// raise the event
							this.OnCommandExecuting(this.sqlCommand);

							rowsAffected = this.ExecuteCommand(this.sqlCommand,	autoCommit);
							this.requiresNewConnection = false;

							// raise the event
							this.OnCommandExecuted(sqlStatement, null, rowsAffected);
							break;
					}
				}
				catch (Exception ex)
				{
					if (this.sqlTransaction	!= null)
					{
						this.sqlTransaction.Rollback();
						this.sqlTransaction = null;
					}

					throw new FbException(String.Format(CultureInfo.CurrentUICulture, "An exception was thrown when executing command: {0}\nBatch execution aborted\nThe returned message was: {1}", sqlStatement, ex.Message));
				}
			}

			if (this.sqlTransaction	!= null)
			{
				// commit root transaction
				this.sqlTransaction.Commit();
				this.sqlTransaction = null;
			}

			this.sqlConnection.Close();
		}

		
		#endregion

		#region Protected Internal Methods

		/// <summary>
		/// Updates	the	connection string with the data	parsed from	the parameter and opens	a connection
		/// to the database.
		/// </summary>
		/// <param name="connectDbStatement"></param>
		protected internal void	ConnectToDatabase(string connectDbStatement)
		{
			// CONNECT 'filespec' [USER	'username'][PASSWORD 'password'] [CACHE	int] [ROLE 'rolename']
			StringParser parser = new StringParser(connectDbStatement, false);
			parser.Token = " ";
			parser.ParseNext();
			if (parser.Result.Trim().ToUpper(CultureInfo.CurrentUICulture) != "CONNECT")
			{
				throw new Exception("Malformed isql CONNECT statement. Expected keyword CONNECT but something else was found.");
			}
			parser.ParseNext();
			this.connectionString.Database = parser.Result.Replace("'", "");
			while (parser.ParseNext() != -1)
			{
				switch (parser.Result.Trim().ToUpper(CultureInfo.CurrentUICulture))
				{
					case "USER":
						parser.ParseNext();
						this.connectionString.UserID = parser.Result.Replace("'", "");
						break;

					case "PASSWORD":
						parser.ParseNext();
						this.connectionString.Password = parser.Result.Replace("'", "");
						break;

					case "CACHE":
						parser.ParseNext();
						break;

					case "ROLE":
						parser.ParseNext();
						this.connectionString.Role = parser.Result.Replace("'", "");
						break;

					default:
						throw new Exception("Unexpected token '" + parser.Result.Trim() + "' on isql CONNECT statement.");
			
				}
			}
			this.requiresNewConnection = true;
			this.ProvideConnection();					
		}

		/// <summary>
		/// Parses the isql	statement CREATE DATABASE and creates the database and opens a connection to the recently created database.
		/// </summary>
		/// <param name="createDbStatement">the	create database	statement.</param>
		protected internal void	CreateDatabase(string createDbStatement)
		{
			// CREATE {DATABASE	| SCHEMA} 'filespec'
			// [USER 'username'	[PASSWORD 'password']]
			// [PAGE_SIZE [=] int]
			// [LENGTH [=] int [PAGE[S]]]
			// [DEFAULT	CHARACTER SET charset]
			// [<secondary_file>];	
#if	(!NETCF)
			throw new NotImplementedException();
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		protected internal FbConnection	SetDatabase(string setDbStatement)
		{
#if	(!NETCF)
			throw new NotImplementedException();
#else
			throw new NotSupportedException();
#endif
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		protected internal FbCommand ProvideCommand()
		{
			if (this.sqlCommand == null)
			{
				this.sqlCommand = new FbCommand();
			}
			// this.sqlCommand.Transaction = ProvideTransaction();
			this.sqlCommand.Connection = this.ProvideConnection();

			return this.sqlCommand;
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		protected internal FbConnection	ProvideConnection()
		{
			if (requiresNewConnection)
			{
				if ((this.sqlConnection	!= null) ||
					(this.sqlConnection.State != ConnectionState.Closed) ||
					(this.sqlConnection.State != ConnectionState.Broken))
				{
					this.sqlConnection.Close();
				}
				this.sqlConnection = new FbConnection(this.connectionString.ToString());	
			}

			if (this.sqlConnection.State ==	ConnectionState.Closed)
			{
				this.sqlConnection.Open();
			}

			return this.sqlConnection;
		}

		/// <summary>
		/// Executes a command and optionally commits the transaction.
		/// </summary>
		/// <param name="command">Command to execute.</param>
		/// <param name="autocommit">true to commit	the	transaction	after execution; or	false if not.</param>
		/// <returns>The number	of rows	affected by the	query execution.</returns>
		protected internal int ExecuteCommand(FbCommand	command, bool autocommit)
		{
			int	rowsAffected = command.ExecuteNonQuery();
			if (autocommit && command.IsDDLCommand() && command.Transaction != null)
			{
				command.Transaction.CommitRetaining();
			}

			return rowsAffected;
		}

		#endregion

		#region Event Handlers

		/// <summary>
		/// The	trigger	function for <see cref="CommandExecuting"/>	event.
		/// </summary>
		/// <param name="sqlCommand">The SQL command that is going for execution.</param>
		protected virtual void OnCommandExecuting(FbCommand	sqlCommand)
		{
			if (CommandExecuting !=	null)
			{
				CommandExecutingEventArgs e = new CommandExecutingEventArgs(sqlCommand);
				CommandExecuting(this, e);
			}
		}

		/// <summary>
		/// The	trigger	function for <see cref="CommandExecuted"/> event.
		/// </summary>
		/// <param name="commandText">The <see cref="FbCommand.CommandText"/> of the executed SQL command.</param>
		/// <param name="dataReader">The <see cref="FbDataReader"/>	instance with the returned data. If	the	
		/// command	executed is	not	meant to return	data (ex: UPDATE, INSERT...) this parameter	must be	
		/// setled to <b>null</b>.</param>
		/// <param name="rowsAffected">The rows	that were affected by the executed SQL command.	If the executed	
		/// command	is not meant to	return this	kind of	information	(ex: SELECT) this parameter	must 
		/// be setled to <b>-1</b>.</param>
		protected virtual void OnCommandExecuted(string	commandText, FbDataReader dataReader, int rowsAffected)	
		{
			if (CommandExecuted	!= null) 
			{
				CommandExecutedEventArgs e = new CommandExecutedEventArgs(dataReader, commandText, rowsAffected);
				CommandExecuted(this, e);
			}
		}

		#endregion

		#region Static Methods

		/// <summary>
		/// Determines the <see	cref="SqlStatementType"/> of the provided SQL statement.
		/// </summary>
		/// <param name="sqlStatement">The string containing the SQL statement.</param>
		/// <returns>The <see cref="SqlStatementType"/>	of the <b>sqlStatement</b>.</returns>
		/// <remarks>If	the	type of	<b>sqlStatement</b>	could not be determinated this 
		/// method will	throw an exception.</remarks>
		public static SqlStatementType GetStatementType(string sqlStatement) 
		{
			char type = sqlStatement ==	null ? ' ' : sqlStatement.Trim().ToUpper(CultureInfo.CurrentUICulture)[0];

			switch (type)
			{
				case 'A':
					if (StringParser.StartsWith(sqlStatement, "ALTER DATABASE",	true))
						return SqlStatementType.AlterDatabase;
					if (StringParser.StartsWith(sqlStatement, "ALTER DOMAIN", true))
						return SqlStatementType.AlterDomain;
					if (StringParser.StartsWith(sqlStatement, "ALTER EXCEPTION", true))
						return SqlStatementType.AlterException;
					if (StringParser.StartsWith(sqlStatement, "ALTER INDEX", true))
						return SqlStatementType.AlterIndex;
					if (StringParser.StartsWith(sqlStatement, "ALTER PROCEDURE", true))
						return SqlStatementType.AlterProcedure;
					if (StringParser.StartsWith(sqlStatement, "ALTER TABLE", true))
						return SqlStatementType.AlterTable;
					if (StringParser.StartsWith(sqlStatement, "ALTER TRIGGER", true))
						return SqlStatementType.AlterTrigger;
					throw new Exception("The type of the SQL statement could not be determinated.");

				case 'C':
				switch (char.ToUpper(sqlStatement[1], CultureInfo.CurrentUICulture)) 
				{
					case 'L':
						if (StringParser.StartsWith(sqlStatement, "CLOSE", true))
							return SqlStatementType.Close;
						throw new Exception("The type of the SQL statement could not be determinated.");

					case 'O':
						if (StringParser.StartsWith(sqlStatement, "COMMIT",	true))
							return SqlStatementType.Commit;
						if (StringParser.StartsWith(sqlStatement, "CONNECT", true))
							return SqlStatementType.Connect;
						throw new Exception("The type of the SQL statement could not be determinated.");

					case 'R':
						if (StringParser.StartsWith(sqlStatement, "CREATE DATABASE", true))
							return SqlStatementType.CreateDatabase;
						if (StringParser.StartsWith(sqlStatement, "CREATE DOMAIN", true))
							return SqlStatementType.CreateDomain;
						if (StringParser.StartsWith(sqlStatement, "CREATE EXCEPTION", true))
							return SqlStatementType.CreateException;
						if (StringParser.StartsWith(sqlStatement, "CREATE GENERATOR", true))
							return SqlStatementType.CreateGenerator;
						if (StringParser.StartsWith(sqlStatement, "CREATE INDEX", true))
							return SqlStatementType.CreateIndex;
						if (StringParser.StartsWith(sqlStatement, "CREATE PROCEDURE", true))
							return SqlStatementType.CreateProcedure;
						if (StringParser.StartsWith(sqlStatement, "CREATE ROLE", true))
							return SqlStatementType.CreateRole;
						if (StringParser.StartsWith(sqlStatement, "CREATE SHADOW", true))
							return SqlStatementType.CreateShadow;
						if (StringParser.StartsWith(sqlStatement, "CREATE TABLE", true))
							return SqlStatementType.CreateTable;
						if (StringParser.StartsWith(sqlStatement, "CREATE TRIGGER",	true))
							return SqlStatementType.CreateTrigger;
						if (StringParser.StartsWith(sqlStatement, "CREATE UNIQUE INDEX", true))
							return SqlStatementType.CreateIndex;
						if (StringParser.StartsWith(sqlStatement, "CREATE VIEW", true))
							return SqlStatementType.CreateView;
						throw new Exception("The type of the SQL statement could not be determinated.");

					default:
						throw new Exception("The type of the SQL statement could not be determinated.");
				}
					
				case 'D':
				switch (char.ToUpper(sqlStatement[1], CultureInfo.CurrentUICulture)) 
				{
					case 'E':
						if (StringParser.StartsWith(sqlStatement, "DECLARE CURSOR",	true))
							return SqlStatementType.DeclareCursor;
						if (StringParser.StartsWith(sqlStatement, "DECLARE EXTERNAL FUNCTION", true))
							return SqlStatementType.DeclareExternalFunction;
						if (StringParser.StartsWith(sqlStatement, "DECLARE FILTER",	true))
							return SqlStatementType.DeclareFilter;
						if (StringParser.StartsWith(sqlStatement, "DECLARE STATEMENT", true))
							return SqlStatementType.DeclareStatement;
						if (StringParser.StartsWith(sqlStatement, "DECLARE TABLE", true))
							return SqlStatementType.DeclareTable;
						if (StringParser.StartsWith(sqlStatement, "DELETE",	true))
							return SqlStatementType.Delete;
						if (StringParser.StartsWith(sqlStatement, "DESCRIBE", true))
							return SqlStatementType.Describe;
						throw new Exception("The type of the SQL statement could not be determinated.");

					case 'I':
						if (StringParser.StartsWith(sqlStatement, "DISCONNECT",	true))
							return SqlStatementType.Disconnect;
						throw new Exception("The type of the SQL statement could not be determinated.");

					case 'R':
						if (StringParser.StartsWith(sqlStatement, "DROP DATABASE", true))
							return SqlStatementType.DropDatabase;
						if (StringParser.StartsWith(sqlStatement, "DROP DOMAIN", true))
							return SqlStatementType.DropDomain;
						if (StringParser.StartsWith(sqlStatement, "DROP EXCEPTION",	true))
							return SqlStatementType.DropException;
						if (StringParser.StartsWith(sqlStatement, "DROP EXTERNAL FUNCTION",	true))
							return SqlStatementType.DropExternalFunction;
						if (StringParser.StartsWith(sqlStatement, "DROP FILTER", true))
							return SqlStatementType.DropFilter;
						if (StringParser.StartsWith(sqlStatement, "DROP GENERATOR",	true))
							return SqlStatementType.DropGenerator;
						if (StringParser.StartsWith(sqlStatement, "DROP INDEX",	true))
							return SqlStatementType.DropIndex;
						if (StringParser.StartsWith(sqlStatement, "DROP PROCEDURE",	true))
							return SqlStatementType.DropProcedure;
						if (StringParser.StartsWith(sqlStatement, "DROP ROLE", true))
							return SqlStatementType.DropRole;
						if (StringParser.StartsWith(sqlStatement, "DROP SHADOW", true))
							return SqlStatementType.DropShadow;
						if (StringParser.StartsWith(sqlStatement, "DROP TABLE",	true))
							return SqlStatementType.DropTable;
						if (StringParser.StartsWith(sqlStatement, "DROP TRIGGER", true))
							return SqlStatementType.DropTrigger;
						if (StringParser.StartsWith(sqlStatement, "DROP VIEW", true))
							return SqlStatementType.DropView;
						throw new Exception("The type of the SQL statement could not be determinated.");

					default:
						throw new Exception("The type of the SQL statement could not be determinated.");
				}
				
				case 'E':
					if (StringParser.StartsWith(sqlStatement, "EXECUTE PROCEDURE", true))
						return SqlStatementType.ExecuteProcedure;
					if (StringParser.StartsWith(sqlStatement, "EXECUTE IMMEDIATE", true))
						return SqlStatementType.ExecuteImmediate;
					if (StringParser.StartsWith(sqlStatement, "EXECUTE", true))
						return SqlStatementType.Execute;
					if (StringParser.StartsWith(sqlStatement, "EVENT WAIT",	true))
						return SqlStatementType.EventWait;
					if (StringParser.StartsWith(sqlStatement, "EVENT INIT",	true))
						return SqlStatementType.EventInit;
					if (StringParser.StartsWith(sqlStatement, "END DECLARE SECTION", true))
						return SqlStatementType.EndDeclareSection;
					throw new Exception("The type of the SQL statement could not be determinated.");

				case 'F':
					if (StringParser.StartsWith(sqlStatement, "FETCH", true))
						return SqlStatementType.Fetch;
					throw new Exception("The type of the SQL statement could not be determinated.");

				case 'G':
					if (StringParser.StartsWith(sqlStatement, "GRANT", true))
						return SqlStatementType.Grant;
					throw new Exception("The type of the SQL statement could not be determinated.");

				case 'I':
					if (StringParser.StartsWith(sqlStatement, "INSERT CURSOR", true))
						return SqlStatementType.InsertCursor;
					if (StringParser.StartsWith(sqlStatement, "INSERT",	true))
						return SqlStatementType.Insert;
					throw new Exception("The type of the SQL statement could not be determinated.");

				case 'O':
					if (StringParser.StartsWith(sqlStatement, "OPEN", true))
						return SqlStatementType.Open;
					throw new Exception("The type of the SQL statement could not be determinated.");

				case 'P':
					if (StringParser.StartsWith(sqlStatement, "PREPARE", true))
						return SqlStatementType.Prepare;
					throw new Exception("The type of the SQL statement could not be determinated.");

				case 'R':
					if (StringParser.StartsWith(sqlStatement, "REVOKE",	true))
						return SqlStatementType.Revoke;
					if (StringParser.StartsWith(sqlStatement, "ROLLBACK", true))
						return SqlStatementType.Rollback;
					throw new Exception("The type of the SQL statement could not be determinated.");

				case 'S':								
					if (StringParser.StartsWith(sqlStatement, "SELECT",	true))
						return SqlStatementType.Select;
					if (StringParser.StartsWith(sqlStatement, "SET DATABASE", true))
						return SqlStatementType.SetDatabase;
					if (StringParser.StartsWith(sqlStatement, "SET GENERATOR", true))
						return SqlStatementType.SetGenerator;
					if (StringParser.StartsWith(sqlStatement, "SET NAMES", true))
						return SqlStatementType.SetGenerator;
					if (StringParser.StartsWith(sqlStatement, "SET SQL DIALECT", true))
						return SqlStatementType.SetSQLDialect;
					if (StringParser.StartsWith(sqlStatement, "SET STATISTICS",	true))
						return SqlStatementType.SetStatistics;
					if (StringParser.StartsWith(sqlStatement, "SET TRANSACTION", true))
						return SqlStatementType.SetTransaction;
					if (StringParser.StartsWith(sqlStatement, "SHOW SQL DIALECT", true))
						return SqlStatementType.ShowSQLDialect;
					throw new Exception("The type of the SQL statement could not be determinated.");

				case 'U':
					if (StringParser.StartsWith(sqlStatement, "UPDATE",	true))
						return SqlStatementType.Update;
					throw new Exception("The type of the SQL statement could not be determinated.");

				case 'W':
					if (StringParser.StartsWith(sqlStatement, "WHENEVER", true))
						return SqlStatementType.Whenever;
					throw new Exception("The type of the SQL statement could not be determinated.");

				default:
					throw new Exception("The type of the SQL statement could not be determinated.");
			}
		}

		#endregion
	}
}