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
using System.Text;
using System.Resources;
using System.ComponentModel;
using System.Collections;

using NpgsqlTypes;
using Npgsql.Design;

namespace Npgsql
{
    /// <summary>
    /// Represents a SQL statement or function (stored procedure) to execute
    /// against a PostgreSQL database. This class cannot be inherited.
    /// </summary>
    [System.Drawing.ToolboxBitmapAttribute(typeof(NpgsqlCommand)), ToolboxItem(true)]
    public sealed class NpgsqlCommand : Component, IDbCommand
    {
        // Logging related values
        private static readonly String CLASSNAME = "NpgsqlCommand";
        private static ResourceManager resman = new ResourceManager(typeof(NpgsqlCommand));

        private NpgsqlConnection            connection;
        private NpgsqlConnector             connector;
        private NpgsqlTransaction           transaction;
        private String                      text;
        private Int32                       timeout;
        private CommandType                 type;
        private NpgsqlParameterCollection   parameters;
        private String                      planName;

        private NpgsqlParse                 parse;
        private NpgsqlBind                  bind;

        // Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Npgsql.NpgsqlCommand">NpgsqlCommand</see> class.
        /// </summary>
        public NpgsqlCommand() : this(String.Empty, null, null)
        {}
        /// <summary>
        /// Initializes a new instance of the <see cref="Npgsql.NpgsqlCommand">NpgsqlCommand</see> class with the text of the query.
        /// </summary>
        /// <param name="cmdText">The text of the query.</param>
        public NpgsqlCommand(String cmdText) : this(cmdText, null, null)
        {}
        /// <summary>
        /// Initializes a new instance of the <see cref="Npgsql.NpgsqlCommand">NpgsqlCommand</see> class with the text of the query and a <see cref="Npgsql.NpgsqlConnection">NpgsqlConnection</see>.
        /// </summary>
        /// <param name="cmdText">The text of the query.</param>
        /// <param name="connection">A <see cref="Npgsql.NpgsqlConnection">NpgsqlConnection</see> that represents the connection to a PostgreSQL server.</param>
        public NpgsqlCommand(String cmdText, NpgsqlConnection connection) : this(cmdText, connection, null)
        {}
        /// <summary>
        /// Initializes a new instance of the <see cref="Npgsql.NpgsqlCommand">NpgsqlCommand</see> class with the text of the query, a <see cref="Npgsql.NpgsqlConnection">NpgsqlConnection</see>, and the <see cref="Npgsql.NpgsqlTransaction">NpgsqlTransaction</see>.
        /// </summary>
        /// <param name="cmdText">The text of the query.</param>
        /// <param name="connection">A <see cref="Npgsql.NpgsqlConnection">NpgsqlConnection</see> that represents the connection to a PostgreSQL server.</param>
        /// <param name="transaction">The <see cref="Npgsql.NpgsqlTransaction">NpgsqlTransaction</see> in which the <see cref="Npgsql.NpgsqlCommand">NpgsqlCommand</see> executes.</param>
        public NpgsqlCommand(String cmdText, NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME);

            planName = String.Empty;
            text = cmdText;
            this.connection = connection;
            if (this.connection != null)
            	this.connector = connection.Connector;
            
            parameters = new NpgsqlParameterCollection();
            timeout = 20;
            type = CommandType.Text;
            this.Transaction = transaction;
        }

        /// <summary>
        /// Used to execute internal commands.
        /// </summary>
        internal NpgsqlCommand(String cmdText, NpgsqlConnector connector)
        {
            resman = new System.Resources.ResourceManager(this.GetType());
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME);

            planName = String.Empty;
            text = cmdText;
            this.connector = connector;
            type = CommandType.Text;
        }

        // Public properties.
        /// <summary>
        /// Gets or sets the SQL statement or function (stored procedure) to execute at the data source.
        /// </summary>
        /// <value>The Transact-SQL statement or stored procedure to execute. The default is an empty string.</value>
        [Category("Data"), DefaultValue("")]
        public String CommandText {
            get
            {
                return text;
            }

            set
            {
                // [TODO] Validate commandtext.
                NpgsqlEventLog.LogPropertySet(LogLevel.Debug, CLASSNAME, "CommandText", value);
                text = value;
                planName = String.Empty;
                parse = null;
                bind = null;
            }
        }

        /// <summary>
        /// Gets or sets the wait time before terminating the attempt
        /// to execute a command and generating an error.
        /// </summary>
        /// <value>The time (in seconds) to wait for the command to execute.
        /// The default is 20 seconds.</value>
        [DefaultValue(20)]
        public Int32 CommandTimeout {
            get
            {
                return timeout;
            }

            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(resman.GetString("Exception_CommandTimeoutLessZero"));

                timeout = value;
                NpgsqlEventLog.LogPropertySet(LogLevel.Debug, CLASSNAME, "CommandTimeout", value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating how the
        /// <see cref="Npgsql.NpgsqlCommand.CommandText">CommandText</see> property is to be interpreted.
        /// </summary>
        /// <value>One of the <see cref="System.Data.CommandType">CommandType</see> values. The default is <see cref="System.Data.CommandType">CommandType.Text</see>.</value>
        [Category("Data"), DefaultValue(CommandType.Text)]
        public CommandType CommandType {
            get
            {
                return type;
            }

            set
            {
                type = value;
                NpgsqlEventLog.LogPropertySet(LogLevel.Debug, CLASSNAME, "CommandType", value);
            }
        }

        IDbConnection IDbCommand.Connection {
            get
            {
                return Connection;
            }

            set
            {
                Connection = (NpgsqlConnection) value;
                NpgsqlEventLog.LogPropertySet(LogLevel.Debug, CLASSNAME, "IDbCommand.Connection", value);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Npgsql.NpgsqlConnection">NpgsqlConnection</see>
        /// used by this instance of the <see cref="Npgsql.NpgsqlCommand">NpgsqlCommand</see>.
        /// </summary>
        /// <value>The connection to a data source. The default value is a null reference.</value>
        [Category("Behavior"), DefaultValue(null)]
        public NpgsqlConnection Connection {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "Connection");
                return connection;
            }

            set
            {
                if (this.transaction != null && this.transaction.Connection == null)
                    this.transaction = null;
                if (this.connection != null && this.Connector.Transaction != null)
                    throw new InvalidOperationException(resman.GetString("Exception_SetConnectionInTransaction"));
                this.connection = value;
                if (this.connection != null)
                	connector = this.connection.Connector;
                
                NpgsqlEventLog.LogPropertySet(LogLevel.Debug, CLASSNAME, "Connection", value);
            }
        }

        internal NpgsqlConnector Connector {
            get
            {
                if (connector == null && this.connection != null)
                    connector = this.connection.Connector;
                    
                return connector;
            }
        }

        IDataParameterCollection IDbCommand.Parameters {
            get
            {
                return Parameters;
            }
        }

        /// <summary>
        /// Gets the <see cref="Npgsql.NpgsqlParameterCollection">NpgsqlParameterCollection</see>.
        /// </summary>
        /// <value>The parameters of the SQL statement or function (stored procedure). The default is an empty collection.</value>
        [Category("Data"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public NpgsqlParameterCollection Parameters {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "Parameters");
                return parameters;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Npgsql.NpgsqlTransaction">NpgsqlTransaction</see>
        /// within which the <see cref="Npgsql.NpgsqlCommand">NpgsqlCommand</see> executes.
        /// </summary>
        /// <value>The <see cref="Npgsql.NpgsqlTransaction">NpgsqlTransaction</see>.
        /// The default value is a null reference.</value>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IDbTransaction Transaction {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "Transaction");

                if (this.transaction != null && this.transaction.Connection == null)
                {
                    this.transaction = null;
                }
                return this.transaction;
            }

            set
            {
                NpgsqlEventLog.LogPropertySet(LogLevel.Debug, CLASSNAME, "Transaction" ,value);

                this.transaction = (NpgsqlTransaction) value;
            }
        }

        /// <summary>
        /// Gets or sets how command results are applied to the <see cref="System.Data.DataRow">DataRow</see>
        /// when used by the <see cref="System.Data.Common.DbDataAdapter.Update">Update</see>
        /// method of the <see cref="System.Data.Common.DbDataAdapter">DbDataAdapter</see>.
        /// </summary>
        /// <value>One of the <see cref="System.Data.UpdateRowSource">UpdateRowSource</see> values.</value>
        [Category("Behavior"), DefaultValue(UpdateRowSource.Both)]
        public UpdateRowSource UpdatedRowSource {
            get
            {

                NpgsqlEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "UpdatedRowSource");

                return UpdateRowSource.Both;
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Attempts to cancel the execution of a <see cref="Npgsql.NpgsqlCommand">NpgsqlCommand</see>.
        /// </summary>
        /// <remarks>This Method isn't implemented yet.</remarks>
        public void Cancel()
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Cancel");

            // [TODO] Finish method implementation.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a new instance of an <see cref="System.Data.IDbDataParameter">IDbDataParameter</see> object.
        /// </summary>
        /// <returns>An <see cref="System.Data.IDbDataParameter">IDbDataParameter</see> object.</returns>
        IDbDataParameter IDbCommand.CreateParameter()
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "IDbCommand.CreateParameter");

            return (NpgsqlParameter) CreateParameter();
        }

        /// <summary>
        /// Creates a new instance of a <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> object.
        /// </summary>
        /// <returns>A <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> object.</returns>
        public NpgsqlParameter CreateParameter()
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "CreateParameter");

            return new NpgsqlParameter();
        }

        /// <summary>
        /// Executes a SQL statement against the connection and returns the number of rows affected.
        /// </summary>
        /// <returns>The number of rows affected.</returns>
        public Int32 ExecuteNonQuery()
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ExecuteNonQuery");

            ExecuteCommand();

            // If nothing is returned, just return -1.
            if(Connector.Mediator.CompletedResponses.Count == 0) {
                return -1;
            }

            // Check if the response is available.
            String firstCompletedResponse = (String)Connector.Mediator.CompletedResponses[0];

            if (firstCompletedResponse == null)
                return -1;

            String[] ret_string_tokens = firstCompletedResponse.Split(null);        // whitespace separator.


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

        /// <summary>
        /// Sends the <see cref="Npgsql.NpgsqlCommand.CommandText">CommandText</see> to
        /// the <see cref="Npgsql.NpgsqlConnection">Connection</see> and builds a
        /// <see cref="Npgsql.NpgsqlDataReader">NpgsqlDataReader</see>.
        /// </summary>
        /// <returns>A <see cref="Npgsql.NpgsqlDataReader">NpgsqlDataReader</see> object.</returns>
        IDataReader IDbCommand.ExecuteReader()
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "IDbCommand.ExecuteReader");

            return (NpgsqlDataReader) ExecuteReader();
        }

        /// <summary>
        /// Sends the <see cref="Npgsql.NpgsqlCommand.CommandText">CommandText</see> to
        /// the <see cref="Npgsql.NpgsqlConnection">Connection</see> and builds a
        /// <see cref="Npgsql.NpgsqlDataReader">NpgsqlDataReader</see>
        /// using one of the <see cref="System.Data.CommandBehavior">CommandBehavior</see> values.
        /// </summary>
        /// <param name="cb">One of the <see cref="System.Data.CommandBehavior">CommandBehavior</see> values.</param>
        /// <returns>A <see cref="Npgsql.NpgsqlDataReader">NpgsqlDataReader</see> object.</returns>
        IDataReader IDbCommand.ExecuteReader(CommandBehavior cb)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "IDbCommand.ExecuteReader", cb);

            return (NpgsqlDataReader) ExecuteReader(cb);
        }

        /// <summary>
        /// Sends the <see cref="Npgsql.NpgsqlCommand.CommandText">CommandText</see> to
        /// the <see cref="Npgsql.NpgsqlConnection">Connection</see> and builds a
        /// <see cref="Npgsql.NpgsqlDataReader">NpgsqlDataReader</see>.
        /// </summary>
        /// <returns>A <see cref="Npgsql.NpgsqlDataReader">NpgsqlDataReader</see> object.</returns>
        public NpgsqlDataReader ExecuteReader()
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ExecuteReader");

            return ExecuteReader(CommandBehavior.Default);
        }

        /// <summary>
        /// Sends the <see cref="Npgsql.NpgsqlCommand.CommandText">CommandText</see> to
        /// the <see cref="Npgsql.NpgsqlConnection">Connection</see> and builds a
        /// <see cref="Npgsql.NpgsqlDataReader">NpgsqlDataReader</see>
        /// using one of the <see cref="System.Data.CommandBehavior">CommandBehavior</see> values.
        /// </summary>
        /// <param name="cb">One of the <see cref="System.Data.CommandBehavior">CommandBehavior</see> values.</param>
        /// <returns>A <see cref="Npgsql.NpgsqlDataReader">NpgsqlDataReader</see> object.</returns>
        /// <remarks>Currently the CommandBehavior parameter is ignored.</remarks>
        public NpgsqlDataReader ExecuteReader(CommandBehavior cb)
        {
            // [FIXME] No command behavior handling.

            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ExecuteReader", cb);

            ExecuteCommand();

            // Get the resultsets and create a Datareader with them.
            return new NpgsqlDataReader(Connector.Mediator.ResultSets, Connector.Mediator.CompletedResponses, connection, cb);
        }

        ///<summary>
        /// This method binds the parameters from parameters collection to the bind
        /// message.
        /// </summary>
        private void BindParameters()
        {

            if (parameters.Count != 0)
            {
                Object[] parameterValues = new Object[parameters.Count];
                for (Int32 i = 0; i < parameters.Count; i++)
                {
                    // Do not quote strings, or escape existing quotes - this will be handled by the backend.
                    // DBNull or null values are returned as null.
                    // TODO: Would it be better to remove this null special handling out of ConvertToBackend?? 
                    parameterValues[i] = parameters[i].TypeInfo.ConvertToBackend(parameters[i].Value, true);
                }
                bind.ParameterValues = parameterValues;
            }

            Connector.Bind(bind);
            Connector.Mediator.RequireReadyForQuery = false;
            Connector.Flush();

            connector.CheckErrorsAndNotifications();
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row
        /// in the result set returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <returns>The first column of the first row in the result set,
        /// or a null reference if the result set is empty.</returns>
        public Object ExecuteScalar()
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ExecuteScalar");

            /*if ((type == CommandType.Text) || (type == CommandType.StoredProcedure))
              if (parse == null)
            			connection.Query(this); 
               else
               {
                 BindParameters();
                 connection.Execute(new NpgsqlExecute(bind.PortalName, 0));
               }
            else
            	throw new NotImplementedException(resman.GetString("Exception_CommandTypeTableDirect"));
            */

            ExecuteCommand();

            // Now get the results.
            // Only the first column of the first row must be returned.

            // Get ResultSets.
            ArrayList resultSets = Connector.Mediator.ResultSets;

            // First data is the RowDescription object.
            // Check all resultsets as insert commands could have been sent along
            // with resultset queries. The insert commands return null and and some queries
            // may return empty resultsets, so, if we find one of these, skip to next resultset.
            // If no resultset is found, return null as per specification.

            NpgsqlAsciiRow ascii_row = null;
            foreach( NpgsqlResultSet nrs in resultSets )
            {
                if( (nrs != null) && (nrs.Count > 0) )
                {
                    ascii_row = (NpgsqlAsciiRow) nrs[0];
                    return ascii_row[0];
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a prepared version of the command on a PostgreSQL server.
        /// </summary>
        public void Prepare()
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Prepare");

            // Check the connection state.
            CheckConnectionState();

            if (! Connector.SupportsPrepare) {
                return;	// Do nothing.
            }

            if (connector.BackendProtocolVersion == ProtocolVersion.Version2)
            {
                NpgsqlCommand command = new NpgsqlCommand(GetPrepareCommandText(), connector );
                command.ExecuteNonQuery();
            }
            else
            {
                // Use the extended query parsing...
                planName = "NpgsqlPlan" + Connector.NextPlanIndex();
                String portalName = "NpgsqlPortal" + Connector.NextPortalIndex();

                parse = new NpgsqlParse(planName, GetParseCommandText(), new Int32[] {});

                Connector.Parse(parse);
                Connector.Mediator.RequireReadyForQuery = false;
                Connector.Flush();

                // Check for errors and/or notifications and do the Right Thing.
                connector.CheckErrorsAndNotifications();

                bind = new NpgsqlBind(portalName, planName, new Int16[] {0}, null, new Int16[] {0});
            }
        }

        /*
        /// <summary>
        /// Releases the resources used by the <see cref="Npgsql.NpgsqlCommand">NpgsqlCommand</see>.
        /// </summary>
        protected override void Dispose (bool disposing)
        {
            
            if (disposing)
            {
                // Only if explicitly calling Close or dispose we still have access to 
                // managed resources.
                NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Dispose");
                if (connection != null)
                {
                    connection.Dispose();
                }
                base.Dispose(disposing);
                
            }
        }*/

        ///<summary>
        /// This method checks the connection state to see if the connection
        /// is set or it is open. If one of this conditions is not met, throws
        /// an InvalidOperationException
        ///</summary>
        private void CheckConnectionState()
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "CheckConnectionState");
            
            // Check the connection state.
            if (Connector == null || Connector.State != ConnectionState.Open) {
                throw new InvalidOperationException(resman.GetString("Exception_ConnectionNotOpen"));
            }
        }

        /// <summary>
        /// This method substitutes the <see cref="Npgsql.NpgsqlCommand.Parameters">Parameters</see>, if exist, in the command
        /// to their actual values.
        /// The parameter name format is <b>:ParameterName</b>.
        /// </summary>
        /// <returns>A version of <see cref="Npgsql.NpgsqlCommand.CommandText">CommandText</see> with the <see cref="Npgsql.NpgsqlCommand.Parameters">Parameters</see> inserted.</returns>
        internal String GetCommandText()
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetCommandText");

            if (planName == String.Empty)
                return GetClearCommandText();
            else
                return GetPreparedCommandText();
        }


        private String GetClearCommandText()
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetClearCommandText");


            String result = text;

            if (type == CommandType.StoredProcedure)
                if (Connector.SupportsPrepare)
                    result = "select * from " + result; // This syntax is only available in 7.3+ as well SupportsPrepare.
                else
                    result = "select " + result;				// Only a single result return supported. 7.2 and earlier.
            else if (type == CommandType.TableDirect)
                return "select * from " + result; // There is no parameter support on table direct.

            if (parameters == null || parameters.Count == 0)
                return result;


            //CheckParameters();

            for (Int32 i = 0; i < parameters.Count; i++)
            {
                NpgsqlParameter Param = parameters[i];
                
                // FIXME DEBUG ONLY
                // adding the '::<datatype>' on the end of a parameter is a highly
                // questionable practice, but it is great for debugging!
                result = ReplaceParameterValue(
                            result,
                            Param.ParameterName,
                            Param.TypeInfo.ConvertToBackend(Param.Value, false) + "::" + Param.TypeInfo.Name
                         );
            }

            return result;
        }



        private String GetPreparedCommandText()
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetPreparedCommandText");

            if (parameters.Count == 0)
                return "execute " + planName;


            StringBuilder result = new StringBuilder("execute " + planName + '(');


            for (Int32 i = 0; i < parameters.Count; i++)
            {
                result.Append(parameters[i].TypeInfo.ConvertToBackend(parameters[i].Value, false) + ',');
            }

            result = result.Remove(result.Length - 1, 1);
            result.Append(')');

            return result.ToString();

        }



        private String GetParseCommandText()
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetParseCommandText");

            String parseCommand = text;

            if (type == CommandType.StoredProcedure)
                parseCommand = "select * from " + parseCommand; // This syntax is only available in 7.3+ as well SupportsPrepare.
            else if (type == CommandType.TableDirect)
                return "select * from " + parseCommand; // There is no parameter support on TableDirect.

            if (parameters.Count > 0)
            {
                // The ReplaceParameterValue below, also checks if the parameter is present.

                String parameterName;
                Int32 i;

                for (i = 0; i < parameters.Count; i++)
                {
                    //result = result.Replace(":" + parameterName, parameters[i].Value.ToString());
                    parameterName = parameters[i].ParameterName;
                    //textCommand = textCommand.Replace(':' + parameterName, "$" + (i+1));
                    parseCommand = ReplaceParameterValue(parseCommand, parameterName, "$" + (i+1));

                }
            }

            return parseCommand;

        }


        private String GetPrepareCommandText()
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetPrepareCommandText");



            planName = "NpgsqlPlan" + Connector.NextPlanIndex();

            StringBuilder command = new StringBuilder("prepare " + planName);

            String textCommand = text;

            if (type == CommandType.StoredProcedure)
                textCommand = "select * from " + textCommand;
            else if (type == CommandType.TableDirect)
                return "select * from " + textCommand; // There is no parameter support on TableDirect.


            if (parameters.Count > 0)
            {
                // The ReplaceParameterValue below, also checks if the parameter is present.

                String parameterName;
                Int32 i;

                for (i = 0; i < parameters.Count; i++)
                {
                    //result = result.Replace(":" + parameterName, parameters[i].Value.ToString());
                    parameterName = parameters[i].ParameterName;
                    // The space in front of '$' fixes a parsing problem in 7.3 server
                    // which gives errors of operator when finding the caracters '=$' in
                    // prepare text
                    textCommand = ReplaceParameterValue(textCommand, parameterName, " $" + (i+1));

                }

                //[TODO] Check if there is any missing parameters in the query.
                // For while, an error is thrown saying about the ':' char.

                command.Append('(');

                for (i = 0; i < parameters.Count; i++)
                {
//                    command.Append(NpgsqlTypesHelper.GetDefaultTypeInfo(parameters[i].DbType));
                    command.Append(parameters[i].TypeInfo.Name);

                    command.Append(',');
                }

                command = command.Remove(command.Length - 1, 1);
                command.Append(')');

            }

            command.Append(" as ");
            command.Append(textCommand);


            return command.ToString();

        }


        private String ReplaceParameterValue(String result, String parameterName, String paramVal)
        {
            Int32 resLen = result.Length;
            Int32 paramStart = result.IndexOf(parameterName);
            Int32 paramLen = parameterName.Length;
            Int32 paramEnd = paramStart + paramLen;
            Boolean found = false;

            while(paramStart > -1)
            {
                if((resLen > paramEnd) &&
                        (result[paramEnd] == ' ' ||
                         result[paramEnd] == ',' ||
                         result[paramEnd] == ')' ||
                         result[paramEnd] == ';' ||
                         result[paramEnd] == '\n' ||
                         result[paramEnd] == '\t'))
                {
                    result = result.Substring(0, paramStart) + paramVal + result.Substring(paramEnd);
                    found = true;
                }
                else if(resLen == paramEnd)
                {
                    result = result.Substring(0, paramStart)+ paramVal;
                    found = true;
                }
                else
                    break;
                resLen = result.Length;
                paramStart = result.IndexOf(parameterName, paramStart);
                paramEnd = paramStart + paramLen;

            }//while
            if(!found)
                throw new IndexOutOfRangeException (String.Format(resman.GetString("Exception_ParamNotInQuery"), parameterName));

            return result;
        }//ReplaceParameterValue


        private void ExecuteCommand()
        {
            // Check the connection state first.
            CheckConnectionState();
	    
            // reset any responses just before getting new ones
            connector.Mediator.ResetResponses();


            if (parse == null) {
                Connector.Query(this);

                // Check for errors and/or notifications and do the Right Thing.
                connector.CheckErrorsAndNotifications();
            } 
            else 
            {
                try
                {
					
                    BindParameters();

                    // Check for errors and/or notifications and do the Right Thing.
                    connector.CheckErrorsAndNotifications();

                    connector.Execute(new NpgsqlExecute(bind.PortalName, 0));

                    // Check for errors and/or notifications and do the Right Thing.
                    connector.CheckErrorsAndNotifications();
                }
                finally
                {
                    // As per documentation:
                    // "[...] When an error is detected while processing any extended-query message,
                    // the backend issues ErrorResponse, then reads and discards messages until a
                    // Sync is reached, then issues ReadyForQuery and returns to normal message processing.[...]"
                    // So, send a sync command if we get any problems.

                    connector.Sync();
                }
            }           
        }
    }
}
