// created on 10/5/2002 at 23:01

// Npgsql.NpgsqlConnection.cs
// 
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
//
//	Copyright (C) 2002 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
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
using System.Collections.Specialized;
using NpgsqlTypes;


namespace Npgsql
{
  /// <summary>
  /// This class represents a connection to 
  /// PostgreSQL Server.
  /// </summary>
  /// 
  /// <remarks> remarks test </remarks>
  /// 
  public sealed class NpgsqlConnection : IDbConnection
  {
  	
  	private NpgsqlState			state;
  	
    private ConnectionState	connection_state;
    private String					connection_string;
    private ListDictionary	connection_string_values;

    // In the connection string
    private readonly Char		CONN_DELIM 		= ';';  // Delimeter
    private readonly Char 	CONN_ASSIGN 	= '=';
    private readonly String CONN_SERVER 	= "SERVER";
    private readonly String CONN_USERID 	= "USER ID";
    private readonly String CONN_PASSWORD = "PASSWORD";
    private readonly String CONN_DATABASE = "DATABASE";
    private readonly String CONN_PORT 		= "PORT";

		// Postgres default port
    private readonly String PG_PORT = "5432";
		
    // These are for ODBC connection string compatibility
    private readonly String ODBC_USERID 	= "UID";
    private readonly String ODBC_PASSWORD = "PWD";
      		
    // Values for possible CancelRequest messages.
    private NpgsqlBackEndKeyData backend_keydata;
  	
  	// Flag for transaction status.
  	private Boolean							_inTransaction = false;
    
    // Mediator which will hold data generated from backend
    private NpgsqlMediator	_mediator;
        
    // Logging related values
    private readonly String CLASSNAME = "NpgsqlConnection";
  		
    private TcpClient				connection;
    /*private BufferedStream	output_stream;
    private Byte[]					input_buffer;*/
    private Encoding				connection_encoding;
  	
  	private Boolean					_supportsPrepare = false;
  	
  	private String 					_serverVersion; // Contains string returned from select version();
  	
  	private Hashtable				_oidToNameMapping; 
  	
  	
    public NpgsqlConnection() : this(String.Empty){}

    public NpgsqlConnection(String ConnectionString)
    {
      NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".NpgsqlConnection()", LogLevel.Debug);
      
      connection_state = ConnectionState.Closed;
    	state = NpgsqlClosedState.Instance;
    	connection_string = ConnectionString;
      connection_string_values = new ListDictionary();
      connection_encoding = Encoding.Default;
    	
    	_mediator = new NpgsqlMediator();
    	
    	_oidToNameMapping = new Hashtable();
    	
    	if (connection_string != String.Empty)
				ParseConnectionString();
    }

		///<value> This is the ConnectionString value </value>
    public String ConnectionString
    {
      get
      {
        return connection_string;
      }
      set
      {
        connection_string = value;
        NpgsqlEventLog.LogMsg("Set " + CLASSNAME + ".ConnectionString = " + value, LogLevel.Normal);
      	if (connection_string != String.Empty)
        	ParseConnectionString();
      }
    }
	
    public Int32 ConnectionTimeout
    {
      get
      {
        return 0;
      }
    }

    ///<summary>
    /// 
    /// </summary>	
    public String Database
    {
      get
      {
        return DatabaseName;
      }
    }
	
    public ConnectionState State
    {
      get 
      {	    		
        return connection_state; 
      }
    }
    		
    IDbTransaction IDbConnection.BeginTransaction()
    {
      NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + "IDbConnection.BeginTransaction()", LogLevel.Debug);
      //throw new NotImplementedException();
    	return (NpgsqlTransaction) BeginTransaction();
    }
	
    IDbTransaction IDbConnection.BeginTransaction(IsolationLevel level)
    {
      NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + "IDbConnection.BeginTransaction(" + level + ")", LogLevel.Debug);
      //throw new NotImplementedException();
    	return (NpgsqlTransaction) BeginTransaction(level);
    }

		
		public NpgsqlTransaction BeginTransaction()
		{
			NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".BeginTransaction()", LogLevel.Debug);
			return this.BeginTransaction(IsolationLevel.ReadCommitted);
		}
		
		public NpgsqlTransaction BeginTransaction(IsolationLevel level)
		{
			NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".BeginTransaction(" + level + ")", LogLevel.Debug);
			
			if (_inTransaction)
				throw new InvalidOperationException("Nested/Concurrent transactions aren't supported.");
			
			InTransaction = true;
			
			return new NpgsqlTransaction(this, level);
		}
	
		///
		/// <summary>
		/// This method changes the current database by disconnecting from the actual 
		/// database and connecting to the specified.
		/// </summary>
		
		 
    public void ChangeDatabase(String dbName)
    {
      NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".ChangeDatabase(" + dbName + ")", LogLevel.Debug);
      //throw new NotImplementedException();
    	
    	if (dbName == null)
    		throw new ArgumentNullException("dbName");
    	
    	if (dbName == String.Empty)
    		throw new ArgumentException("Invalid database name", "dbName");
    	
    	
    	String oldDatabaseName = (String)connection_string_values[CONN_DATABASE];
    	
    	Close();
    	    	
    	connection_string_values[CONN_DATABASE] = dbName;
    	
    	Open();
    	    	

    
    }
	
    public void Open()
    {
      NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".Open()", LogLevel.Debug);
	    	
      try
      {
		    		    	
        // Check if the connection is already open.
        if (connection_state == ConnectionState.Open)
          throw new NpgsqlException("Connection already open");
		    		    
		   	if (connection_string == String.Empty)
		   		throw new InvalidOperationException("ConnectionString cannot be empty.");
      	
		    CurrentState.Open(this);
      	
      	// Check if there were any errors.
      	if (_mediator.Errors.Count > 0)
      	{
      		StringWriter sw = new StringWriter();
      		sw.WriteLine("There have been errors on Open()");
      		uint i = 1;
      		foreach(string error in _mediator.Errors){
      			sw.WriteLine("{0}. {1}", i++, error);
      		}
      		CurrentState = NpgsqlClosedState.Instance;
      		_mediator.Reset();
      		throw new NpgsqlException(sw.ToString());
      	}
      	
      	backend_keydata = _mediator.GetBackEndKeyData();
      	
        // Change the state of connection to open.
        connection_state = ConnectionState.Open;
      	
      	// Get version information to enable/disable server version features.
      	NpgsqlCommand command = new NpgsqlCommand("select version();set DATESTYLE TO ISO;", this);
      	_serverVersion = (String) command.ExecuteScalar();
      	ProcessServerVersion();
      	_oidToNameMapping = NpgsqlTypesHelper.LoadTypesMapping(this);
      	
      	
      		    			    		
      }
      catch(SocketException e)
      {
        // [TODO] Very ugly message. Needs more working.
        throw new NpgsqlException("A SocketException occured", e);
      }
	    	
      catch(IOException e)
      {
        // This exception was thrown by StartupPacket handling functions.
        // So, close the connection and throw the exception.
        // [TODO] Better exception handling. :)
        Close();
	    		
        throw new NpgsqlException("Error in Open()", e);
      }
	    	
    }
	
    public void Close()
    
    {
      NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".Close()", LogLevel.Debug);
    
      try
      {
      	if ((connection_state == ConnectionState.Open))
        {
          CurrentState.Close(this);
        }
      }
      catch (IOException e)
      {
        throw new NpgsqlException("Error in Close()", e);
      }
      finally
      {
        // Even if an exception occurs, let object in a consistent state.
        if (TcpClient != null)
        	TcpClient.Close();
        connection_state = ConnectionState.Closed;
      }
    }
	    
    IDbCommand IDbConnection.CreateCommand()
    {
      NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".CreateCommand()", LogLevel.Debug);
      return (NpgsqlCommand) CreateCommand();
    }
    
    public NpgsqlCommand CreateCommand()
    {
    	NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".CreateCommand()", LogLevel.Debug);
      return new NpgsqlCommand("", this);
    }
    
    // Implement the IDisposable interface.
    public void Dispose()
    {
      NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".Dispose()", LogLevel.Debug);
	    		    	
    }
	    
    // Private util methods
	    
    /// <summary>
    /// This method parses the connection string.
    /// It translates it to a list of key-value pairs.
    /// Valid values are:
    /// Server 		- Address/Name of Postgresql Server
    /// Port		- Port to connect to.
    /// Database 	- Database name. Defaults to user name if not specified
    /// User		- User name
    /// Password	- Password for clear text authentication
    /// </summary>
    private void ParseConnectionString()
    {
      NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".ParseConnectionString()", LogLevel.Debug);
	    
	    connection_string_values.Clear();
	    
	    // Get the key-value pairs delimited by CONN_DELIM
      String[] pairs = connection_string.Split(new Char[] {CONN_DELIM});
	    	
      String[] keyvalue;
      // Now, for each pair, get its key-value.
      foreach(String s in pairs)
      {
        // This happen when there are trailling/empty CONN_DELIMs
        // Just ignore them.
        if (s == "")	
          continue;
	    		
        keyvalue = s.Split(new Char[] {CONN_ASSIGN});
	    		
        // Check if there is a key-value pair.
        if (keyvalue.Length != 2)
          throw new ArgumentException("key=value argument incorrect in ConnectionString", connection_string);
	    	
				// Shift the key to upper case, and substitute ODBC style keys
				keyvalue[0] = keyvalue[0].ToUpper();
				if (keyvalue[0] == ODBC_USERID)
	  			keyvalue[0] = CONN_USERID;
        if (keyvalue[0] == ODBC_PASSWORD)
          keyvalue[0] = CONN_PASSWORD;	    	 
	    	
				// Add the pair to the dictionary. The key is shifted to upper
				// case for case insensitivity.
				    	
				NpgsqlEventLog.LogMsg("Connection string option: " + keyvalue[0] + " = " + keyvalue[1], LogLevel.Normal);
        connection_string_values.Add(keyvalue[0], keyvalue[1]);
      }
	    	
      // Now check if there is any missing argument.
      if (connection_string_values[CONN_SERVER] == null)
        throw new ArgumentException("Connection string argument missing!", CONN_SERVER);
      if ((connection_string_values[CONN_USERID] == null) & (connection_string_values[ODBC_USERID] == null))
        throw new ArgumentException("Connection string argument missing!", CONN_USERID);
      if ((connection_string_values[CONN_PASSWORD] == null) & (connection_string_values[ODBC_PASSWORD] == null))
        throw new ArgumentException("Connection string argument missing!", CONN_PASSWORD);
      if (connection_string_values[CONN_DATABASE] == null)
        // Database is optional. "[...] defaults to the user name if empty"
        connection_string_values[CONN_DATABASE] = connection_string_values[CONN_USERID];
      if (connection_string_values[CONN_PORT] == null)
        // Port is optional. Defaults to PG_PORT.
        connection_string_values[CONN_PORT] = PG_PORT;
    	
    }


		/// <summary>
		/// This method is required to set all the version dependent features flags.
		/// SupportsPrepare means the server can use prepared query plans (7.3+)
		/// 
		/// </summary>
		 		 
		private void ProcessServerVersion()
		{
			NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".ProcessServerVersion()", LogLevel.Debug);
			
			
			SupportsPrepare = (_serverVersion.IndexOf("PostgreSQL 7.3") != -1) || 
												(_serverVersion.IndexOf("PostgreSQL 7.4") != -1) ;
			
		}
    
    // State 
		internal void Query( NpgsqlCommand queryCommand )
		{
			CurrentState.Query( this, queryCommand );
		}
		internal void Authenticate(string password)
		{
			CurrentState.Authenticate( this, password );
		}
		internal void Startup()
		{
			CurrentState.Startup( this );
		}
		
		internal NpgsqlState CurrentState
		{
			get 
			{
				return state;
			}
			set
			{
				state = value;
			}
		}
		// Internal properties

		internal NpgsqlBackEndKeyData BackEndKeyData
		{
			get
			{
				return backend_keydata;
			}
			set
			{
				backend_keydata = value;
			}
		}
		
		internal String ServerName
		{
			get
			{
				return (String)connection_string_values[CONN_SERVER];
			}
		}
		internal String ServerPort
		{
			get
			{
				return   (String)connection_string_values[CONN_PORT];
			}
		}
		internal String DatabaseName
		{
			get
			{
				return (String)connection_string_values[CONN_DATABASE];
			}
		}
		internal String UserName
		{
			get
			{
				return (String)connection_string_values[CONN_USERID];
			}
		}
		internal String ServerPassword
		{
			get
			{
				return (String)connection_string_values[CONN_PASSWORD];
			}
		}
		internal TcpClient TcpClient
		{
			get
			{
				return connection;
			}
			set
			{
				connection = value;
			}
		}
		internal Encoding Encoding
		{
			get
			{
				return connection_encoding;
			}
		}
		
		internal NpgsqlMediator	Mediator
		{
			get
			{
				return _mediator;
			}
		}
		
		internal Boolean InTransaction
		{
			get
			{
				return _inTransaction;
			}
			
			set
			{
				_inTransaction = value;
			}
		}
		
		internal Boolean SupportsPrepare
		{
			get
			{
				return _supportsPrepare;
			}
			
			set
			{
				_supportsPrepare = value;
			}
		}
		
		internal String ServerVersion
		{
			get
			{
				return _serverVersion;
			}
		}
		
		internal Hashtable OidToNameMapping
		{
			get
			{
				return _oidToNameMapping;
			}
			
			set 
			{
				_oidToNameMapping = value;
			}
			
		}
		
  }
}
