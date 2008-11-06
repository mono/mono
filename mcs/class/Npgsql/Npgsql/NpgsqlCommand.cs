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
using System.IO;

using NpgsqlTypes;

using System.Text.RegularExpressions;

#if WITHDESIGN
using Npgsql.Design;
#endif

namespace Npgsql
{
    /// <summary>
    /// Represents a SQL statement or function (stored procedure) to execute
    /// against a PostgreSQL database. This class cannot be inherited.
    /// </summary>
    #if WITHDESIGN
    [System.Drawing.ToolboxBitmapAttribute(typeof(NpgsqlCommand)), ToolboxItem(true)]
    #endif
    public sealed class NpgsqlCommand : Component, IDbCommand, ICloneable
    {
        // Logging related values
        private static readonly String CLASSNAME = "NpgsqlCommand";
        private static ResourceManager resman = new ResourceManager(typeof(NpgsqlCommand));
        private static readonly Regex parameterReplace = new Regex(@"([:@][\w\.]*)", RegexOptions.Singleline);

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

        private Boolean						invalidTransactionDetected = false;
        
        private CommandBehavior             commandBehavior;

        private Int64                       lastInsertedOID = 0;

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
            type = CommandType.Text;
            this.Transaction = transaction;
            commandBehavior = CommandBehavior.Default;

            SetCommandTimeout();
            
            
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
            commandBehavior = CommandBehavior.Default;
            
            parameters = new NpgsqlParameterCollection();

            // Internal commands aren't affected by command timeout value provided by user.
            timeout = 20;
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
                commandBehavior = CommandBehavior.Default;
            }
        }

        /// <summary>
        /// Gets or sets the wait time before terminating the attempt
        /// to execute a command and generating an error.
        /// </summary>
        /// <value>The time (in seconds) to wait for the command to execute.
        /// The default is 20 seconds.</value>
        [DefaultValue(20)]
        public Int32 CommandTimeout
        {
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
        public CommandType CommandType
        {
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

        IDbConnection IDbCommand.Connection 
        {
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
        public NpgsqlConnection Connection
        {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "Connection");
                return connection;
            }

            set
            {
                if (this.Connection == value)
                    return;

                //if (this.transaction != null && this.transaction.Connection == null)
                  //  this.transaction = null;
                                    
                if (this.transaction != null && this.connection != null && this.Connector.Transaction != null)
                    throw new InvalidOperationException(resman.GetString("Exception_SetConnectionInTransaction"));


                this.connection = value;
                Transaction = null;
                if (this.connection != null)
                    connector = this.connection.Connector;

                SetCommandTimeout();
                
                NpgsqlEventLog.LogPropertySet(LogLevel.Debug, CLASSNAME, "Connection", value);
            }
        }

        internal NpgsqlConnector Connector
        {
            get
            {
                if (this.connection != null)
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
        #if WITHDESIGN
        [Category("Data"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        #endif
        
        public NpgsqlParameterCollection Parameters
        {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "Parameters");
                return parameters;
            }
        }

        
        IDbTransaction IDbCommand.Transaction 
        {
            get
            {
                return Transaction;
            }

            set
            {
                Transaction = (NpgsqlTransaction) value;
                NpgsqlEventLog.LogPropertySet(LogLevel.Debug, CLASSNAME, "IDbCommand.Transaction", value);
            }
        }
        
        /// <summary>
        /// Gets or sets the <see cref="Npgsql.NpgsqlTransaction">NpgsqlTransaction</see>
        /// within which the <see cref="Npgsql.NpgsqlCommand">NpgsqlCommand</see> executes.
        /// </summary>
        /// <value>The <see cref="Npgsql.NpgsqlTransaction">NpgsqlTransaction</see>.
        /// The default value is a null reference.</value>
        #if WITHDESIGN
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        #endif
        
        public NpgsqlTransaction Transaction {
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
        #if WITHDESIGN
        [Category("Behavior"), DefaultValue(UpdateRowSource.Both)]
        #endif
        
        public UpdateRowSource UpdatedRowSource {
            get
            {

                NpgsqlEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "UpdatedRowSource");

                return UpdateRowSource.Both;
            }

            set
            {
            }
        }

        /// <summary>
        /// Returns oid of inserted row. This is only updated when using executenonQuery and when command inserts just a single row. If table is created without oids, this will always be 0.
        /// </summary>

	public Int64 LastInsertedOID
        {
            get
            {
                return lastInsertedOID;
            }
        }


        /// <summary>
        /// Attempts to cancel the execution of a <see cref="Npgsql.NpgsqlCommand">NpgsqlCommand</see>.
        /// </summary>
        /// <remarks>This Method isn't implemented yet.</remarks>
        public void Cancel()
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Cancel");

            try
            {
                // get copy for thread safety of null test
                NpgsqlConnector connector = Connector;
                if (connector != null)
                {
                    connector.CancelRequest();
                }
            }
            catch (IOException)
            {
                Connection.ClearPool();
            }   
            catch (NpgsqlException)
            {
                // Cancel documentation says the Cancel doesn't throw on failure
            }
        }
        
        /// <summary>
        /// Create a new command based on this one.
        /// </summary>
        /// <returns>A new NpgsqlCommand object.</returns>
        Object ICloneable.Clone()
        {
            return Clone();
        }

        /// <summary>
        /// Create a new connection based on this one.
        /// </summary>
        /// <returns>A new NpgsqlConnection object.</returns>
        public NpgsqlCommand Clone()
        {
            // TODO: Add consistency checks.

            return new NpgsqlCommand(CommandText, Connection, Transaction);
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

            // Initialize lastInsertOID
            lastInsertedOID = 0;

            ExecuteCommand();
            
            UpdateOutputParameters();
            
            
            // If nothing is returned, just return -1.
            if(Connector.Mediator.CompletedResponses.Count == 0)
            {
                return -1;
            }

            // Check if the response is available.
            String firstCompletedResponse = (String)Connector.Mediator.CompletedResponses[0];

            if (firstCompletedResponse == null)
                return -1;

            String[] ret_string_tokens = firstCompletedResponse.Split(null);        // whitespace separator.


            // Check if the command was insert, delete, update, fetch or move.
            // Only theses commands return rows affected.
            // [FIXME] Is there a better way to check this??
            if ((String.Compare(ret_string_tokens[0], "INSERT", true) == 0) ||
                    (String.Compare(ret_string_tokens[0], "UPDATE", true) == 0) ||
                    (String.Compare(ret_string_tokens[0], "DELETE", true) == 0) ||
                    (String.Compare(ret_string_tokens[0], "FETCH", true) == 0) ||
                    (String.Compare(ret_string_tokens[0], "MOVE", true) == 0))
                
                
            {
                if (String.Compare(ret_string_tokens[0], "INSERT", true) == 0)
                    // Get oid of inserted row.
                    lastInsertedOID = Int32.Parse(ret_string_tokens[1]);
                
                // The number of rows affected is in the third token for insert queries
                // and in the second token for update and delete queries.
                // In other words, it is the last token in the 0-based array.

                return Int32.Parse(ret_string_tokens[ret_string_tokens.Length - 1]);
            }
            else
                return -1;
        }
        
        
        
        private void UpdateOutputParameters()
        {
            // Check if there was some resultset returned. If so, put the result in output parameters.
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "UpdateOutputParameters");
            
            // Get ResultSets.
            ArrayList resultSets = Connector.Mediator.ResultSets;
            
            if (resultSets.Count != 0)
            {
                NpgsqlResultSet nrs = (NpgsqlResultSet)resultSets[0];
                                
                if ((nrs != null) && (nrs.Count > 0))
                {
                    NpgsqlAsciiRow nar = (NpgsqlAsciiRow)nrs[0];
                    
                    Int32 i = 0;
                    Boolean hasMapping = false;
                                        
                    // First check if there is any mapping between parameter name and resultset name.
                    // If so, just update output parameters which has mapping.
                    
                    foreach (NpgsqlParameter p in Parameters)
                    {
                        if (nrs.RowDescription.FieldIndex(p.ParameterName.Substring(1)) > -1)
                        {
                            hasMapping = true;
                            break;
                        }
                        
                    }
                                        
                    
                    if (hasMapping)
                    {
                        foreach (NpgsqlParameter p in Parameters)
                        {
                            if (((p.Direction == ParameterDirection.Output) ||
                                (p.Direction == ParameterDirection.InputOutput)) && (i < nrs.RowDescription.NumFields ))
                            {
                                Int32 fieldIndex = nrs.RowDescription.FieldIndex(p.ParameterName.Substring(1));
                                
                                if (fieldIndex > -1)
                                {
                                    p.Value = nar[fieldIndex];
                                    i++;
                                }
                                
                            }
                        }
                        
                    }
                    else
                        foreach (NpgsqlParameter p in Parameters)
                        {
                            if (((p.Direction == ParameterDirection.Output) ||
                                (p.Direction == ParameterDirection.InputOutput)) && (i < nrs.RowDescription.NumFields ))
                            {
                                p.Value = nar[i];
                                i++;
                            }
                        }
                }
                
            }   
            
            
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
            commandBehavior = cb;

            ExecuteCommand();
            
            UpdateOutputParameters();
            
            // Get the resultsets and create a Datareader with them.
            return new NpgsqlDataReader(Connector.Mediator.ResultSets, Connector.Mediator.CompletedResponses, cb, this);
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
                Int16[] parameterFormatCodes = bind.ParameterFormatCodes;
                
                for (Int32 i = 0; i < parameters.Count; i++)
                {
                    // Do not quote strings, or escape existing quotes - this will be handled by the backend.
                    // DBNull or null values are returned as null.
                    // TODO: Would it be better to remove this null special handling out of ConvertToBackend??
                    
                    // Do special handling of bytea values. They will be send in binary form.
                    // TODO: Add binary format support for all supported types. Not only bytea.
                    if (parameters[i].TypeInfo.NpgsqlDbType != NpgsqlDbType.Bytea)
                    {
                        
                        parameterValues[i] = parameters[i].TypeInfo.ConvertToBackend(parameters[i].Value, true);
                    }
                    else
                    {
                        parameterFormatCodes[i] = (Int16) FormatCode.Binary;
                        parameterValues[i]=(byte[])parameters[i].Value;
                    }
                }
                bind.ParameterValues = parameterValues;
                bind.ParameterFormatCodes = parameterFormatCodes;
            }

            Connector.Bind(bind);
            
            // See Prepare() method for a discussion of this.
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
            
            // reset any responses just before getting new ones
            Connector.Mediator.ResetResponses();
            
            // Set command timeout.
            connector.Mediator.CommandTimeout = CommandTimeout;

            if (! connector.SupportsPrepare)
            {
                return;	// Do nothing.
            }

            if (connector.BackendProtocolVersion == ProtocolVersion.Version2)
            {
                NpgsqlCommand command = new NpgsqlCommand(GetPrepareCommandText(), connector );
                command.ExecuteNonQuery();
            }
            else
            {
                try
                {
                    
                    connector.StopNotificationThread();
                    
                    // Use the extended query parsing...
                    planName = connector.NextPlanName();
                    String portalName = connector.NextPortalName();
    
                    parse = new NpgsqlParse(planName, GetParseCommandText(), new Int32[] {});
    
                    connector.Parse(parse);
                    
                    // We need that because Flush() doesn't cause backend to send
                    // ReadyForQuery on error. Without ReadyForQuery, we don't return 
                    // from query extended processing.
                    
                    // We could have used Connector.Flush() which sends us back a
                    // ReadyForQuery, but on postgresql server below 8.1 there is an error
                    // with extended query processing which hinders us from using it.
                    connector.Mediator.RequireReadyForQuery = false;
                    connector.Flush();
                    
                    // Check for errors and/or notifications and do the Right Thing.
                    connector.CheckErrorsAndNotifications();
    
                    
                    // Description...
                    NpgsqlDescribe describe = new NpgsqlDescribe('S', planName);
                
                
                    connector.Describe(describe);
                    
                    connector.Sync();
                    
                    Npgsql.NpgsqlRowDescription returnRowDesc = connector.Mediator.LastRowDescription;
                
                    Int16[] resultFormatCodes;
                    
                    
                    if (returnRowDesc != null)
                    {
                        resultFormatCodes = new Int16[returnRowDesc.NumFields];
                        
                        for (int i=0; i < returnRowDesc.NumFields; i++)
                        {
                            Npgsql.NpgsqlRowDescriptionFieldData returnRowDescData = returnRowDesc[i];
                            
                            
                            if (returnRowDescData.type_info != null && returnRowDescData.type_info.NpgsqlDbType == NpgsqlTypes.NpgsqlDbType.Bytea)
                            {
                            // Binary format
                                resultFormatCodes[i] = (Int16)FormatCode.Binary;
                            }
                            else 
                            // Text Format
                                resultFormatCodes[i] = (Int16)FormatCode.Text;
                        }
                    
                        
                    }
                    else
                        resultFormatCodes = new Int16[]{0};
                    
                    bind = new NpgsqlBind("", planName, new Int16[Parameters.Count], null, resultFormatCodes);
                }    
                catch (IOException e)
                {
                    ClearPoolAndThrowException(e);
                }
                catch
                {
                    // See ExecuteCommand method for a discussion of this.
                    connector.Sync();
                    
                    throw;
                }
                finally
                {
                    connector.ResumeNotificationThread();
                }
                
                
                
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
            if (Connector == null || Connector.State != ConnectionState.Open)
            {
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
            if (NpgsqlEventLog.Level == LogLevel.Debug)
                NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetClearCommandText");

            Boolean addProcedureParenthesis = false;    // Do not add procedure parenthesis by default.

            Boolean functionReturnsRecord = false;      // Functions don't return record by default.

            Boolean functionReturnsRefcursor = false;   // Functions don't return refcursor by default.

            String result = text;

            if (type == CommandType.StoredProcedure)
            {

                if (Parameters.Count > 0)
                    functionReturnsRecord = CheckFunctionReturn("record");

                functionReturnsRefcursor = CheckFunctionReturn("refcursor");

                // Check if just procedure name was passed. If so, does not replace parameter names and just pass parameter values in order they were added in parameters collection. Also check if command text finishes in a ";" which would make Npgsql incorrectly append a "()" when executing this command text.
                if ((!result.Trim().EndsWith(")")) && (!result.Trim().EndsWith(";")))
                {
                    addProcedureParenthesis = true;
                    result += "(";
                }

                if (Connector.SupportsPrepare)
                    result = "select * from " + result; // This syntax is only available in 7.3+ as well SupportsPrepare.
                else
                    result = "select " + result;        //Only a single result return supported. 7.2 and earlier.
            }
            else if (type == CommandType.TableDirect)
                return "select * from " + result;       // There is no parameter support on table direct.

            if (parameters == null || parameters.Count == 0)
            {
                if (addProcedureParenthesis)
                    result += ")";


                // If function returns ref cursor just process refcursor-result function call
                // and return command which will be used to return data from refcursor.

                if (functionReturnsRefcursor)
                    return ProcessRefcursorFunctionReturn(result);


                if (functionReturnsRecord)
                    result = AddFunctionReturnsRecordSupport(result);


                result = AddSingleRowBehaviorSupport(result);
                
                result = AddSchemaOnlyBehaviorSupport(result);

                return result;
            }


            // Get parameters in query string to translate them to their actual values.

            // This regular expression gets all the parameters in format :param or @param
            // and everythingelse.
            // This is only needed if query string has parameters. Else, just append the
            // parameter values in order they were put in parameter collection.


            // If parenthesis don't need to be added, they were added by user with parameter names. Replace them.
            if (!addProcedureParenthesis)
            {
                StringBuilder sb = new StringBuilder();
                NpgsqlParameter p;
                string[] queryparts = parameterReplace.Split(result);

                foreach (String s in queryparts)
                {
                    if (s == string.Empty)
                        continue;

                    if ((s[0] == ':' || s[0] == '@') &&
                        Parameters.TryGetValue(s, out p))
                    {
                        // It's a parameter. Lets handle it.
                        if ((p.Direction == ParameterDirection.Input) ||
                             (p.Direction == ParameterDirection.InputOutput))
                        {
                            // FIXME DEBUG ONLY
                            // adding the '::<datatype>' on the end of a parameter is a highly
                            // questionable practice, but it is great for debugging!
                            sb.Append(p.TypeInfo.ConvertToBackend(p.Value, false));

                            // Only add data type info if we are calling an stored procedure.

                            if (type == CommandType.StoredProcedure)
                            {
                                sb.Append("::");
                                sb.Append(p.TypeInfo.Name);

                                if (p.TypeInfo.UseSize && (p.Size > 0))
                                    sb.Append("(").Append(p.Size).Append(")");
                            }
                        }

                    }
                    else
                        sb.Append(s);
                }

                result = sb.ToString();
            }

            else
            {

                for (Int32 i = 0; i < parameters.Count; i++)
                {
                    NpgsqlParameter Param = parameters[i];


                    if ((Param.Direction == ParameterDirection.Input) ||
                         (Param.Direction == ParameterDirection.InputOutput))


                        result += Param.TypeInfo.ConvertToBackend(Param.Value, false) + "::" + Param.TypeInfo.Name + ",";
                }


                // Remove a trailing comma added from parameter handling above. If any.
                // Maybe there are only output parameters. If so, there will be no comma.
                if (result.EndsWith(","))
                    result = result.Remove(result.Length - 1, 1);

                result += ")";
            }

            if (functionReturnsRecord)
                result = AddFunctionReturnsRecordSupport(result);

            // If function returns ref cursor just process refcursor-result function call
            // and return command which will be used to return data from refcursor.

            if (functionReturnsRefcursor)
                return ProcessRefcursorFunctionReturn(result);


            result = AddSingleRowBehaviorSupport(result);
            
            result = AddSchemaOnlyBehaviorSupport(result);
            
            return result;
        }
        
        
        
        private Boolean CheckFunctionReturn(String ReturnType)
        {
            // Updated after 0.99.3 to support the optional existence of a name qualifying schema and allow for case insensitivity
            // when the schema or procedure name do not contain a quote.
            // The hard-coded schema name 'public' was replaced with code that uses schema as a qualifier, only if it is provided.

            String returnRecordQuery;

            StringBuilder parameterTypes = new StringBuilder("");

            
            // Process parameters
            
            foreach(NpgsqlParameter p in Parameters)
            {
                if ((p.Direction == ParameterDirection.Input) ||
                     (p.Direction == ParameterDirection.InputOutput))
                {
                    parameterTypes.Append(Connection.Connector.OidToNameMapping[p.TypeInfo.Name].OID + " ");
                }
            }

            
            // Process schema name.
            
            String schemaName = String.Empty;
            String procedureName = String.Empty;
            
            
            String[] fullName = CommandText.Split('.');
            
            if (fullName.Length == 2)
            {
                returnRecordQuery = "select count(*) > 0 from pg_proc p left join pg_namespace n on p.pronamespace = n.oid where prorettype = ( select oid from pg_type where typname = :typename ) and proargtypes=:proargtypes and proname=:proname and n.nspname=:nspname";

                schemaName = (fullName[0].IndexOf("\"") != -1) ? fullName[0] : fullName[0].ToLower();
                procedureName = (fullName[1].IndexOf("\"") != -1) ? fullName[1] : fullName[1].ToLower();
            }
            else
            {
                // Instead of defaulting don't use the nspname, as an alternative, query pg_proc and pg_namespace to try and determine the nspname.
                //schemaName = "public"; // This was removed after build 0.99.3 because the assumption that a function is in public is often incorrect.
                returnRecordQuery = "select count(*) > 0 from pg_proc p where prorettype = ( select oid from pg_type where typname = :typename ) and proargtypes=:proargtypes and proname=:proname";
                
                procedureName = (CommandText.IndexOf("\"") != -1) ? CommandText : CommandText.ToLower();
            }
                
            
            

            NpgsqlCommand c = new NpgsqlCommand(returnRecordQuery, Connection);
            
            c.Parameters.Add(new NpgsqlParameter("typename", NpgsqlDbType.Text));
            c.Parameters.Add(new NpgsqlParameter("proargtypes", NpgsqlDbType.Text));
            c.Parameters.Add(new NpgsqlParameter("proname", NpgsqlDbType.Text));
            
            c.Parameters[0].Value = ReturnType;
            c.Parameters[1].Value = parameterTypes.ToString();
            c.Parameters[2].Value = procedureName;

            if (schemaName != null && schemaName.Length > 0)
            {
                c.Parameters.Add(new NpgsqlParameter("nspname", NpgsqlDbType.Text));
                c.Parameters[3].Value = schemaName;
            }
            

            Boolean ret = (Boolean) c.ExecuteScalar();

            // reset any responses just before getting new ones
            connector.Mediator.ResetResponses();
            
            // Set command timeout.
            connector.Mediator.CommandTimeout = CommandTimeout;
            
            return ret;


        }
        
        
        private String AddFunctionReturnsRecordSupport(String OriginalResult)
        {
                                
            StringBuilder sb = new StringBuilder(OriginalResult);
            
            sb.Append(" as (");
            
            foreach(NpgsqlParameter p in Parameters)
            {
                if ((p.Direction == ParameterDirection.Output) ||
                (p.Direction == ParameterDirection.InputOutput))
                {
                    sb.Append(String.Format("{0} {1}, ", p.ParameterName.Substring(1), p.TypeInfo.Name));
                }
            }
            
            String result = sb.ToString();
            
            result = result.Remove(result.Length - 2, 1);
            
            result += ")";
            
            
            
            return result;
            
            
        }
        
        ///<summary>
        /// This methods takes a string with a function call witch returns a refcursor or a set of
        /// refcursor. It will return the names of the open cursors/portals which will hold
        /// results. In turn, it returns the string which is needed to get the data of this cursors
        /// in form of one resultset for each cursor open. This way, clients don't need to do anything
        /// else besides calling function normally to get results in this way.
        ///</summary>
               
        private String ProcessRefcursorFunctionReturn(String FunctionCall)
        {
            NpgsqlCommand c = new NpgsqlCommand(FunctionCall, Connection);
            
            NpgsqlDataReader dr = c.ExecuteReader();
            
            StringBuilder sb = new StringBuilder();
            
            while (dr.Read())
            {
                sb.Append("fetch all from \"").Append(dr.GetString(0)).Append("\";");
                
            }
            
            sb.Append(";"); // Just in case there is no response from refcursor function return.
            
            // reset any responses just before getting new ones
            connector.Mediator.ResetResponses();
            
            // Set command timeout.
            connector.Mediator.CommandTimeout = CommandTimeout;
            
            return sb.ToString();
                    
            
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

            Boolean addProcedureParenthesis = false;  // Do not add procedure parenthesis by default.
            
            String parseCommand = text;

            if (type == CommandType.StoredProcedure)
            {
                // Check if just procedure name was passed. If so, does not replace parameter names and just pass parameter values in order they were added in parameters collection.
                if (!parseCommand.Trim().EndsWith(")"))  
                {
                    addProcedureParenthesis = true;
                    parseCommand += "(";
                }
                
                parseCommand = "select * from " + parseCommand; // This syntax is only available in 7.3+ as well SupportsPrepare.
            }
            else if (type == CommandType.TableDirect)
                return "select * from " + parseCommand; // There is no parameter support on TableDirect.

            if (parameters.Count > 0)
            {
                // The ReplaceParameterValue below, also checks if the parameter is present.

                String parameterName;
                Int32 i;

                for (i = 0; i < parameters.Count; i++)
                {
                    if ((parameters[i].Direction == ParameterDirection.Input) ||
                    (parameters[i].Direction == ParameterDirection.InputOutput))
                    {
                    
                        if (!addProcedureParenthesis)
                        {
                            //result = result.Replace(":" + parameterName, parameters[i].Value.ToString());
                            parameterName = parameters[i].ParameterName;
                            //textCommand = textCommand.Replace(':' + parameterName, "$" + (i+1));
                            parseCommand = ReplaceParameterValue(parseCommand, parameterName, "$" + (i+1) + "::" + parameters[i].TypeInfo.Name);
                        }
                        else
                            parseCommand += "$" + (i+1) + "::" + parameters[i].TypeInfo.Name;
                    }

                }
            }

            if (addProcedureParenthesis)
                return parseCommand + ")";
            else
                return parseCommand;

        }


        private String GetPrepareCommandText()
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetPrepareCommandText");

            Boolean addProcedureParenthesis = false;  // Do not add procedure parenthesis by default.

            planName = Connector.NextPlanName();

            StringBuilder command = new StringBuilder("prepare " + planName);

            String textCommand = text;

            if (type == CommandType.StoredProcedure)
            {
                // Check if just procedure name was passed. If so, does not replace parameter names and just pass parameter values in order they were added in parameters collection.
                if (!textCommand.Trim().EndsWith(")"))  
                {
                    addProcedureParenthesis = true;
                    textCommand += "(";
                }
                
                textCommand = "select * from " + textCommand;
            }
            else if (type == CommandType.TableDirect)
                return "select * from " + textCommand; // There is no parameter support on TableDirect.


            if (parameters.Count > 0)
            {
                // The ReplaceParameterValue below, also checks if the parameter is present.

                String parameterName;
                Int32 i;

                for (i = 0; i < parameters.Count; i++)
                {
                    if ((parameters[i].Direction == ParameterDirection.Input) ||
                    (parameters[i].Direction == ParameterDirection.InputOutput))
                    {
                    
                        if (!addProcedureParenthesis)
                        {
                            //result = result.Replace(":" + parameterName, parameters[i].Value.ToString());
                            parameterName = parameters[i].ParameterName;
                            // The space in front of '$' fixes a parsing problem in 7.3 server
                            // which gives errors of operator when finding the caracters '=$' in
                            // prepare text
                            textCommand = ReplaceParameterValue(textCommand, parameterName, " $" + (i+1));
                        }
                        else
                            textCommand += " $" + (i+1);
                    }

                }

                //[TODO] Check if there are any missing parameters in the query.
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
            
            if (addProcedureParenthesis)
                textCommand += ")";

            command.Append(" as ");
            command.Append(textCommand);


            return command.ToString();

        }


        private static String ReplaceParameterValue(String result, String parameterName, String paramVal)
        {
        
            String quote_pattern = @"['][^']*[']";
            String pattern = "[- |\n\r\t,)(;=+/]" + parameterName + "([- |\n\r\t,)(;=+/]|$)";
            Int32 start, end;
            String withoutquote = result;
            Boolean found = false;
            // First of all
            // Suppress quoted string from query (because we ave to ignore them)
            MatchCollection results = Regex.Matches(result,quote_pattern);
            foreach (Match match in results)
            {
                start = match.Index;
                end = match.Index + match.Length;
                String spaces = new String(' ', match.Length-2);
                withoutquote = withoutquote.Substring(0,start + 1) + spaces + withoutquote.Substring(end - 1);
            }
            do
            {
                // Now we look for the searched parameters on the "withoutquote" string
                results = Regex.Matches(withoutquote,pattern);
                if (results.Count == 0)
                // If no parameter is found, go out!
                    break;
                // We take the first parameter found
                found = true;
                Match match = results[0];
                start = match.Index;
                if ((match.Length - parameterName.Length) == 2)
                    // If the found string is not the end of the string
                    end = match.Index + match.Length - 1;
                else
                    // If the found string is the end of the string
                    end = match.Index + match.Length;
                result = result.Substring(0, start + 1) + paramVal + result.Substring(end);
                withoutquote = withoutquote.Substring(0,start + 1) + paramVal + withoutquote.Substring(end);
            }
            while (true);
            if (!found)
                throw new IndexOutOfRangeException (String.Format(resman.GetString("Exception_ParamNotInQuery"),
                    parameterName));
            return result;
        }

        
        private String AddSingleRowBehaviorSupport(String ResultCommandText)
        {
            
            ResultCommandText = ResultCommandText.Trim();
            
            // Do not add SingleRowBehavior if SchemaOnly behavior is set.
            
            if ((commandBehavior & CommandBehavior.SchemaOnly) == CommandBehavior.SchemaOnly)
                return ResultCommandText;
        
            if ((commandBehavior & CommandBehavior.SingleRow) == CommandBehavior.SingleRow)
            {
                if (ResultCommandText.EndsWith(";"))
                    ResultCommandText = ResultCommandText.Substring(0, ResultCommandText.Length - 1);
                ResultCommandText += " limit 1;";
                
            }
            
            
            
            return ResultCommandText;
            
        }
        
        private String AddSchemaOnlyBehaviorSupport(String ResultCommandText)
        {
            
            ResultCommandText = ResultCommandText.Trim();
        
            if ((commandBehavior & CommandBehavior.SchemaOnly) == CommandBehavior.SchemaOnly)
            {
                if (ResultCommandText.EndsWith(";"))
                    ResultCommandText = ResultCommandText.Substring(0, ResultCommandText.Length - 1);
                ResultCommandText += " limit 0;";
                
            }
            
            
            return ResultCommandText;
            
        }


        private void ExecuteCommand()
        {
            try
            {
                
            // Check the connection state first.
            CheckConnectionState();

            // reset any responses just before getting new ones
            Connector.Mediator.ResetResponses();
            
            // Set command timeout.
            connector.Mediator.CommandTimeout = CommandTimeout;
            
            
            connector.StopNotificationThread();


            if (parse == null)
            {
                connector.Query(this);


                connector.ResumeNotificationThread();
                
                // Check for errors and/or notifications and do the Right Thing.
                connector.CheckErrorsAndNotifications();
                
                
                
            }
            else
            {
                try
                {

                    BindParameters();

                    connector.Execute(new NpgsqlExecute(bind.PortalName, 0));

                    // Check for errors and/or notifications and do the Right Thing.
                    connector.CheckErrorsAndNotifications();
                }
                catch
                {
                    // As per documentation:
                    // "[...] When an error is detected while processing any extended-query message,
                    // the backend issues ErrorResponse, then reads and discards messages until a
                    // Sync is reached, then issues ReadyForQuery and returns to normal message processing.[...]"
                    // So, send a sync command if we get any problems.

                    connector.Sync();
                    
                    throw;
                }
                finally
                {
                    connector.ResumeNotificationThread();
                }
            }

            }

            catch(IOException e)
            {
                ClearPoolAndThrowException(e);
            }

        }

        private void SetCommandTimeout()
        {
            if (Connector != null)
                timeout = Connector.CommandTimeout;
            else
                timeout = ConnectionStringDefaults.CommandTimeout;
        }

        private void ClearPoolAndThrowException(Exception e)
        {
            Connection.ClearPool();
            throw new NpgsqlException(resman.GetString("Exception_ConnectionBroken"), e);

        }

        
         
        
    }
}
