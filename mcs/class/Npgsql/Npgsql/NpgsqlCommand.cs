// created on 21/5/2002 at 20:03

// Npgsql.NpgsqlCommand.cs
// 
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
//
//	Copyright (C) 2002 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA


using System;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Collections;
using NpgsqlTypes;

namespace Npgsql
{
	public sealed class NpgsqlCommand : IDbCommand
	{
		
		private NpgsqlConnection 					connection;
		private String										text;
		private Int32											timeout;
		private CommandType								type;
		private NpgsqlParameterCollection	parameters;
		private String										planName;
		private static Int32							planIndex = 0;
    // Logging related values
    private static readonly String CLASSNAME = "NpgsqlCommand";
		
		// Constructors
		
		public NpgsqlCommand() : this(null, null){}
		
		public NpgsqlCommand(String cmdText) : this(cmdText, null){}
		
		
		public NpgsqlCommand(String cmdText, NpgsqlConnection connection)
		{
		  NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".NpgsqlCommand()", LogLevel.Debug);
			
			planName = String.Empty;
			text = cmdText;
			this.connection = connection;
			parameters = new NpgsqlParameterCollection();
			timeout = 20;
			type = CommandType.Text;			
		}
				
		// Public properties.
		
		public String CommandText
		{
			get
			{
				return text;
			}
			
			set
			{
				// [TODO] Validate commandtext.
				text = value;
				NpgsqlEventLog.LogMsg("Set " + CLASSNAME + ".CommandText = " + value, LogLevel.Normal);
				planName = String.Empty;
			}
		}
		
		public Int32 CommandTimeout
		{
			get
			{
				return timeout;
			}
			
			set
			{
				if (value < 0)
					throw new ArgumentException("CommandTimeout can't be less than zero");
				
				timeout = value;
				NpgsqlEventLog.LogMsg("Set " + CLASSNAME + ".CommandTimeout = " + value, LogLevel.Normal);
			}
		}
		
		public CommandType CommandType
		{
			get
			{
				return type;
			}
			
			set
			{
				type = value;
				NpgsqlEventLog.LogMsg("Set " + CLASSNAME + ".CommandType = " + value, LogLevel.Normal);
			}
			
		}
		
		IDbConnection IDbCommand.Connection
		{
			get
			{
				return Connection;
			}
			
			set
			{
				connection = (NpgsqlConnection) value;
				NpgsqlEventLog.LogMsg("Set " + CLASSNAME + ".IDbCommand.Connection", LogLevel.Debug);
			}
		}
		
		public NpgsqlConnection Connection
		{
			get
			{
				NpgsqlEventLog.LogMsg(CLASSNAME + ".get_Connection", LogLevel.Debug);
				return connection;
			}
			
			set
			{
				connection = value;
				NpgsqlEventLog.LogMsg("Set " + CLASSNAME + ".Connection", LogLevel.Debug);
			}
		}
		
		IDataParameterCollection IDbCommand.Parameters
		{
			get
			{
				return Parameters;
			}
		}
		
		public NpgsqlParameterCollection Parameters
		{
			get
			{
				NpgsqlEventLog.LogMsg(CLASSNAME + ".get_Parameters", LogLevel.Debug);
				return parameters;
			}
		}
		
		public IDbTransaction Transaction
		{
			get
			{
				NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".get_Transaction()", LogLevel.Debug);
				throw new NotImplementedException();
			}
			
			set
			{
				NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".set_Transaction()", LogLevel.Debug);
				throw new NotImplementedException();	
			}
		}
		
		public UpdateRowSource UpdatedRowSource
		{
			get
			{
				
				NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".get_UpdatedRowSource()", LogLevel.Debug);
				// [FIXME] Strange, the line below doesn't appears in the stack trace.
				
				//throw new NotImplementedException();
				return UpdateRowSource.Both;
			}
			
			set
			{
				throw new NotImplementedException();
			}
		}
		
		public void CheckNotification()
 		{
 			if (connection.Mediator.Notifications.Count > 0)
 				for (int i=0; i < connection.Mediator.Notifications.Count; i++)
 					connection.Notification((NpgsqlNotificationEventArgs) connection.Mediator.Notifications[i]);
 
 		}

		public void Cancel()
		{
		  NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".Cancel()", LogLevel.Debug);
		  
			// [TODO] Finish method implementation.
			throw new NotImplementedException();
		}
		
		
		IDbDataParameter IDbCommand.CreateParameter()
		{
		  NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".IDbCommand.CreateParameter()", LogLevel.Debug);
		  
			return (NpgsqlParameter) CreateParameter();
		}
		
		public NpgsqlParameter CreateParameter()
		{
		  NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".CreateParameter()", LogLevel.Debug);
		  
			return new NpgsqlParameter();
		}
		
		public Int32 ExecuteNonQuery()
		{
		  NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".ExecuteNonQuery()", LogLevel.Debug);
		  
			// Check the connection state.
			CheckConnectionState();
			
			if ((type == CommandType.Text) || (type == CommandType.StoredProcedure))
				connection.Query(this); 
			else
				throw new NotImplementedException("Only Text and StoredProcedure types supported!");
			
			
			
			// Check if there were any errors.
			// [FIXME] Just check the first error.
			if (connection.Mediator.Errors.Count > 0)
				throw new NpgsqlException(connection.Mediator.Errors[0].ToString());
			
			CheckNotification();
		  
		  
			// The only expected result is the CompletedResponse result.
			
			String[] ret_string_tokens = ((String)connection.Mediator.GetCompletedResponses()[0]).Split(null);	// whitespace separator.
						
			// Check if the command was insert, delete or update.
			// Only theses commands return rows affected.
			// [FIXME] Is there a better way to check this??
			if ((String.Compare(ret_string_tokens[0], "INSERT", true) == 0) ||
			    (String.Compare(ret_string_tokens[0], "UPDATE", true) == 0) ||
			    (String.Compare(ret_string_tokens[0], "DELETE", true) == 0))
			    
				// The number of rows affected is in the third token for insert queries
				// and in the second token for update and delete queries.
				// In other words, it is the last token in the 0-based array.
										
				return Int32.Parse(ret_string_tokens[ret_string_tokens.Length - 1]);
			else
				return -1;
			
		}
		
		IDataReader IDbCommand.ExecuteReader()
		{
		  NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".ExecuteReader() implicit impl", LogLevel.Debug);
		  
			return (NpgsqlDataReader) ExecuteReader();
		}
		
		IDataReader IDbCommand.ExecuteReader(CommandBehavior cb)
		{
		  NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".ExecuteReader() implicit impl(cb)", LogLevel.Debug);
		  
			return (NpgsqlDataReader) ExecuteReader(cb);
			
		}
		
		public NpgsqlDataReader ExecuteReader()
		{
		  NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".ExecuteReader()", LogLevel.Debug);
		  
			
			return ExecuteReader(CommandBehavior.Default);
			
		}
		
		public NpgsqlDataReader ExecuteReader(CommandBehavior cb)
		{
			// [FIXME] No command behavior handling.
			
			NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".ExecuteReader(CommandBehavior)", LogLevel.Debug);
		  
			// Check the connection state.
			CheckConnectionState();
			
			if ((type == CommandType.Text) || (type == CommandType.StoredProcedure))
				connection.Query(this); 
			else
				throw new NotImplementedException("Only Text and StoredProcedure types supported!");
			
						
			// Check if there were any errors.
			// [FIXME] Just check the first error.
			if (connection.Mediator.Errors.Count > 0)
				throw new NpgsqlException(connection.Mediator.Errors[0].ToString());
			
			CheckNotification();
		  
			
			// Get the resultsets and create a Datareader with them.
			return new NpgsqlDataReader(connection.Mediator.GetResultSets(), connection.Mediator.GetCompletedResponses(), connection);
		}
		
		public Object ExecuteScalar()
		{
		  NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".ExecuteScalar()", LogLevel.Debug);
		  
			// Check the connection state.
			CheckConnectionState();
						
			if ((type == CommandType.Text) || (type == CommandType.StoredProcedure))
				connection.Query(this); 
			else
				throw new NotImplementedException("Only Text and StoredProcedure types supported!");
			
			
			// Check if there were any errors.
			// [FIXME] Just check the first error.
			if (connection.Mediator.Errors.Count > 0)
				throw new NpgsqlException(connection.Mediator.Errors[0].ToString());
			
			CheckNotification();
		  
			//ArrayList results = connection.Mediator.Data;
			
			Object result = null;	// Result of the ExecuteScalar().
			
			
			// Now get the results.
			// Only the first column of the first row must be returned.
			
			// Get ResultSets.
			ArrayList resultSets = connection.Mediator.GetResultSets();
			
			
			// First data is the RowDescription object.
			//NpgsqlRowDescription rd = (NpgsqlRowDescription)results[0];
			
			NpgsqlResultSet firstResultSet = (NpgsqlResultSet)resultSets[0];
			
			NpgsqlRowDescription rd = firstResultSet.RowDescription;
						
			NpgsqlAsciiRow ascii_row = (NpgsqlAsciiRow)firstResultSet[0];
			
			// Now convert the string to the field type.
			
			// [FIXME] Hardcoded values for int types and string.
			// Change to NpgsqlDbType.
			// For while only int4 and string are strong typed.
			// Any other type will be returned as string.
			
			/*switch (rd[0].type_oid)
			{
				case 20:	// int8, integer.
					result = Convert.ToInt64(ascii_row[0]);
					break;
				case 23:	// int4, integer.
					result = Convert.ToInt32(ascii_row[0]);
					break;
				case 25:  // text
					// Get only the first column.
					result = ascii_row[0];
					break;
				default:
					NpgsqlEventLog.LogMsg("Unrecognized datatype returned by ExecuteScalar():" + 
					                      rd[0].type_oid + " Returning String...", LogLevel.Debug);
					result = ascii_row[0];
					break;
			}
			
			
			return result;*/
			
			//return NpgsqlTypesHelper.ConvertNpgsqlTypeToSystemType(connection.OidToNameMapping, ascii_row[0], rd[0].type_oid);
			return ascii_row[0];
			
			
		}
		
		
		
		public void Prepare()
		{
		  NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".Prepare()", LogLevel.Debug);
			
		  
		  if (!connection.SupportsPrepare)
		  	return;	// Do nothing.
		  	
			// Check the connection state.
			CheckConnectionState();
			
			// [TODO] Finish method implementation.
			//throw new NotImplementedException();
			
			//NpgsqlCommand command = new NpgsqlCommand("prepare plan1 as " + GetCommandText(), connection );
			NpgsqlCommand command = new NpgsqlCommand(GetPrepareCommandText(), connection );						
			command.ExecuteNonQuery();
			
						
			
		}
		
		public void Dispose()
		{
		  NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".Dispose()", LogLevel.Debug);
			
		}
		
		///<summary>
		/// This method checks the connection state to see if the connection
		/// is set or it is open. If one of this conditions is not met, throws
		/// an InvalidOperationException
		///</summary>
		
		private void CheckConnectionState()
		{
		  NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".CheckConnectionState()", LogLevel.Debug);
		  
			// Check the connection state.
			if (connection == null)
				throw new InvalidOperationException("The Connection is not set");
			if (connection.State != ConnectionState.Open)
				throw new InvalidOperationException("The Connection is not open");
			
		}
		
		///<summary>
		/// This method substitutes the parameters, if exist, in the command
		/// to their actual values.
		/// The parameter name format is <b>:ParameterName</b>.
		/// </summary>
		/// 
		
		internal String GetCommandText()
		{
			NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".GetCommandText()", LogLevel.Debug);
			
			if (planName == String.Empty)
				return GetClearCommandText();
			else
				return GetPreparedCommandText();
			
					
		}
		
		
		private String GetClearCommandText()
		{
			NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".GetClearCommandText()", LogLevel.Debug);
			
			
			String result = text;
			
			if (type == CommandType.StoredProcedure)
				if (connection.SupportsPrepare)
					result = "select * from " + result; // This syntax is only available in 7.3+ as well SupportsPrepare.
				else
					result = "select " + result;				// Only a single result return supported. 7.2 and earlier.
						
			if (parameters.Count == 0)
				return result;
						
			
			CheckParameters();
			
			String parameterName;
						
			for (Int32 i = 0; i < parameters.Count; i++)
			{
				parameterName = parameters[i].ParameterName;
				//result = result.Replace(":" + parameterName, parameters[i].Value.ToString());
				result = result.Replace(":" + parameterName, NpgsqlTypesHelper.ConvertNpgsqlParameterToBackendStringValue(parameters[i]));
			}
			
			return result;
			
		}
		
		
		
		private String GetPreparedCommandText()
		{
			NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".GetPreparedCommandText()", LogLevel.Debug);
			
			if (parameters.Count == 0)
				return "execute " + planName;
			
			CheckParameters();
			
			StringBuilder result = new StringBuilder("execute " + planName + '(');
			
			
			for (Int32 i = 0; i < parameters.Count; i++)
			{
				//result.Append(parameters[i].Value.ToString() + ',');
				result.Append(NpgsqlTypesHelper.ConvertNpgsqlParameterToBackendStringValue(parameters[i]) + ',');
				//result = result.Replace(":" + parameterName, parameters[i].Value.ToString());
			}
			
			result = result.Remove(result.Length - 1, 1);
			result.Append(')');
			
			return result.ToString();
			
		}
		
		
		private String GetPrepareCommandText()
		{
			NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".GetPrepareCommandText()", LogLevel.Debug);
			
			
			planName = "NpgsqlPlan" + System.Threading.Interlocked.Increment(ref planIndex);
			
			StringBuilder command = new StringBuilder("prepare " + planName);
			
			String textCommand = text;
			
			if (type == CommandType.StoredProcedure)
				textCommand = "select * from " + textCommand;
			
			
			
			if (parameters.Count > 0)
			{
				CheckParameters();
				
				command.Append('(');
				Int32 i;
				for (i = 0; i < parameters.Count; i++)
				{
					//[TODO] Add support for all types. 
					
					/*switch (parameters[i].DbType)
					{
						case DbType.Int32:
							command.Append("int4");
							break;
														
						case DbType.Int64:
							command.Append("int8");
							break;
						
						default:
							throw new InvalidOperationException("Only DbType.Int32, DbType.Int64 datatypes supported");
							
					}*/
					command.Append(NpgsqlTypesHelper.GetBackendTypeNameFromDbType(parameters[i].DbType));
					
					command.Append(',');
				}
				
				command = command.Remove(command.Length - 1, 1);
				command.Append(')');
				
				
				String parameterName;
				
				for (i = 0; i < parameters.Count; i++)
				{
					//result = result.Replace(":" + parameterName, parameters[i].Value.ToString());
					parameterName = parameters[i].ParameterName;
					textCommand = textCommand.Replace(':' + parameterName, "$" + (i+1));
				}
				
			}
			
			
			command.Append(" as ");
			command.Append(textCommand);
			
			
			return command.ToString();
					
		}
		
		private void CheckParameters()
		{
			String parameterName;
			
			for (Int32 i = 0; i < parameters.Count; i++)
			{
				parameterName = parameters[i].ParameterName;
				if (text.IndexOf(':' + parameterName) <= 0)
					throw new NpgsqlException("Parameter :" + parameterName + " wasn't found in the query.");
			}
			
			
			
		}
	}
	
}
