//
// Mono.Data.TdsClient.TdsConnection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

using Mono.Data.TdsClient.Internal;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Net;
using System.Text;

namespace Mono.Data.TdsClient {
        public class TdsConnection : Component, ICloneable, IDbConnection
	{
		#region Fields

		public static readonly string LibraryName = "Mono.Data.TdsClient";

		string dataSource;    // the database server name
		int port;             // which port to use
		int packetSize = 512; // what size are the packets we send/receive?
		bool connectionReset;
		bool pooling;
		int minPoolSize;
		int maxPoolSize;

		string connectionString = null;

		int connectionTimeout = 15;
		IsolationLevel isolationLevel = IsolationLevel.ReadCommitted;

		ConnectionState state = ConnectionState.Closed;
		TdsConnectionParameters parms = new TdsConnectionParameters ();
		TdsTransaction transaction = null;

		// This is the collection of connection pools available
		static Hashtable pools = new Hashtable ();

		// Our TDS object, the real workhorse
		ITds tds = null;

		#endregion // Fields

		#region Constructors

		public TdsConnection ()
			: this (String.Empty, 1433)
		{
		}

		public TdsConnection (string connectionString)
			: this (connectionString, 1433)
		{
		}

		public TdsConnection (string connectionString, int port)
		{
			this.port = port;
			parms.User = null;
			parms.Password = null;
			parms.LibraryName = LibraryName;
			SetConnectionString (connectionString);
		}
			
		#endregion // Constructors

		#region Properties

		public string ConnectionString {
			get { return connectionString; }
			set { SetConnectionString (value); }
		}

		public int ConnectionTimeout {
			get { return connectionTimeout; }
		}

		public string Database {
			get { return tds.Database; }
		}

		public string DataSource {
			get { return DataSource; }
		}

		public int PacketSize {
			get { return PacketSize; }
		}

		public string ServerVersion {
			get {
				if (state == ConnectionState.Closed)
					throw new InvalidOperationException ();
				return tds.ServerVersion; 
			}
		}

		public ConnectionState State {
			get { return state; }
		}
		
		internal ITds Tds {
			get { return tds; }
		}
	
		public string WorkstationId {
			get { return parms.Hostname; }
		}

		#endregion // Properties

		#region Methods

		public TdsTransaction BeginTransaction ()
		{
			return BeginTransaction (IsolationLevel.ReadCommitted);
		}

		public TdsTransaction BeginTransaction (IsolationLevel il)
		{
			if (state == ConnectionState.Closed)
				throw new InvalidOperationException ("Invalid operation. The connection is closed.");
			if (transaction != null && transaction.Open)
				throw new InvalidOperationException ("TdsConnection does not support parallel transactions.");

			transaction = new TdsTransaction (this, il);
			return transaction;
		}

		public void ChangeDatabase (string databaseName)
		{
			if (Database == databaseName)
				return;
			tds.ExecuteNonQuery (String.Format ("use {0}", databaseName));
		}

		public void Close ()
		{
			// rollback any open transactions
			if (transaction != null && transaction.Open)
				transaction.Rollback ();

			// if we aren't pooling, just close the connection
			// otherwise, just set the InUse flag to false
			if (pooling)
				tds.InUse = false;
			else
				tds.Disconnect ();

			this.state = ConnectionState.Closed;
		}

		public TdsCommand CreateCommand ()
		{
			return (new TdsCommand (null, this, transaction));
		}

                object ICloneable.Clone()
                {
                        throw new NotImplementedException ();
                }

		IDbTransaction IDbConnection.BeginTransaction ()
		{
			return BeginTransaction ();
		}

		IDbTransaction IDbConnection.BeginTransaction (IsolationLevel il)
		{
			return BeginTransaction (il);
		}

		IDbCommand IDbConnection.CreateCommand ()
		{
			return CreateCommand ();
		}

		public void Open ()
		{
			if (connectionString == null)
				throw new InvalidOperationException ("The ConnectionString property has not been initialized.");
			if (parms.User == null)
				throw new ArgumentException ("User name is null.");
			if (parms.Password == null)
				throw new ArgumentException ("Password is null.  This may be a bug with blank passwords.");

			if (!pooling)
				tds = new Tds42 (dataSource, port, packetSize);
			else {
				TdsConnectionPool pool = (TdsConnectionPool) pools[connectionString];
				if (pool == null) {
					lock (pools) {
						pool = new TdsConnectionPool (dataSource, port, packetSize, minPoolSize, maxPoolSize);
						pools[connectionString] = pool;
					}
				}
				tds = pool.FindAnAvailableTds ();
			}

			lock (tds) {
				if (!tds.IsConnected) {
					tds.Connect (parms);
					ChangeDatabase (parms.Database);
				}
				tds.InUse = true;
			}
			this.state = ConnectionState.Open;
		}

		[MonoTODO]
		private void SetConnectionString (string connectionString)
		{
			connectionString += ";";
			NameValueCollection parameters = new NameValueCollection ();

			if (connectionString == String.Empty)
				return;

			bool inQuote = false;
			bool inDQuote = false;

			string name = String.Empty;
			string value = String.Empty;
			StringBuilder sb = new StringBuilder ();

			foreach (char c in connectionString)
			{
				switch (c) {
				case '\'':
					inQuote = !inQuote;
					break;
				case '"' :
					inDQuote = !inDQuote;
					break;
				case ';' :
					if (!inDQuote && !inQuote) {
						value = sb.ToString ();
						sb = new StringBuilder ();
					}
					else
						sb.Append (c);
					break;
				case '=' :
					if (!inDQuote && !inQuote) {
						name = sb.ToString ();
						sb = new StringBuilder ();
					}
					else
						sb.Append (c);
					break;
				default:
					sb.Append (c);
					break;
				}
				if (value != String.Empty) {
					parameters [name.ToUpper ().Trim ()] = value.Trim ();
					name = String.Empty;
					value = String.Empty;
				} 
			}

			if (this.ConnectionString == null)
			{
				SetDefaultConnectionParameters (parameters);
			}

			SetProperties (parameters);

			this.connectionString = connectionString;
		}


		private void SetDefaultConnectionParameters (NameValueCollection parameters)
		{
			if (null == parameters.Get ("APPLICATION NAME"))
				parameters["APPLICATION NAME"] = ".Net SqlClient Data Provider";
			if (null == parameters.Get ("CONNECT TIMEOUT") && null == parameters.Get ("CONNECTION TIMEOUT"))
				parameters["CONNECT TIMEOUT"] = "15";
			if (null == parameters.Get ("CONNECTION LIFETIME"))
				parameters["CONNECTION LIFETIME"] = "0";
			if (null == parameters.Get ("CONNECTION RESET"))
				parameters["CONNECTION RESET"] = "true";
			if (null == parameters.Get ("ENLIST"))
				parameters["ENLIST"] = "true";
			if (null == parameters.Get ("INTEGRATED SECURITY") && null == parameters.Get ("TRUSTED_CONNECTION"))
				parameters["INTEGRATED SECURITY"] = "false";
			if (null == parameters.Get ("MAX POOL SIZE"))
				parameters["MAX POOL SIZE"] = "100";
			if (null == parameters.Get ("MIN POOL SIZE"))
				parameters["MIN POOL SIZE"] = "0";
			if (null == parameters.Get ("NETWORK LIBRARY") && null == parameters.Get ("NET"))
				parameters["NETWORK LIBRARY"] = "dbmssocn";
			if (null == parameters.Get ("PACKET SIZE"))
				parameters["PACKET SIZE"] = "512";
			if (null == parameters.Get ("PERSIST SECURITY INFO"))
				parameters["PERSIST SECURITY INFO"] = "false";
			if (null == parameters.Get ("POOLING"))
				parameters["POOLING"] = "true";
			if (null == parameters.Get ("WORKSTATION ID"))
				parameters["WORKSTATION ID"] = Dns.GetHostByName ("localhost").HostName;
		}

		private void SetProperties (NameValueCollection parameters)
		{
			string value;
			foreach (string name in parameters) {
				value = parameters[name];

				switch (name) {
				case "APPLICATION NAME" :
					parms.ApplicationName = value;
					break;
				case "ATTACHDBFILENAME" :
				case "EXTENDED PROPERTIES" :
				case "INITIAL FILE NAME" :
					break;
				case "CONNECT TIMEOUT" :
				case "CONNECTION TIMEOUT" :
					connectionTimeout = Int32.Parse (value);
					break;
				case "CONNECTION LIFETIME" :
					break;
				case "CONNECTION RESET" :
					connectionReset = !(value.ToUpper ().Equals ("FALSE") || value.ToUpper ().Equals ("NO"));
					break;
				case "CURRENT LANGUAGE" :
					parms.Language = value;
					break;
				case "DATA SOURCE" :
				case "SERVER" :
				case "ADDRESS" :
				case "ADDR" :
				case "NETWORK ADDRESS" :
					dataSource = value;
					break;
				case "ENLIST" :
					break;
				case "INITIAL CATALOG" :
				case "DATABASE" :
					parms.Database = value;
					break;
				case "INTEGRATED SECURITY" :
				case "TRUSTED_CONNECTION" :
					break;
				case "MAX POOL SIZE" :
					maxPoolSize = Int32.Parse (value);
					break;
				case "MIN POOL SIZE" :
					minPoolSize = Int32.Parse (value);
					break;
				case "NET" :
				case "NETWORK LIBRARY" :
					if (!value.ToUpper ().Equals ("DBMSSOCN"))
						throw new TdsException ("Unsupported network library.");
					break;
				case "PACKET SIZE" :
					packetSize = Int32.Parse (value);
					break;
				case "PASSWORD" :
				case "PWD" :
					parms.Password = value;
					break;
				case "PERSIST SECURITY INFO" :
					break;
				case "POOLING" :
					pooling = !(value.ToUpper ().Equals ("FALSE") || value.ToUpper ().Equals ("NO"));
					break;
				case "USER ID" :
					parms.User = value;
					break;
				case "WORKSTATION ID" :
					parms.Hostname = value;
					break;
				}
			}
		}
		#endregion // Methods
	}
}
