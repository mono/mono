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

		bool autoCommit = true;
		bool disablePooling = false;
		string connectionString = null;
		int connectionTimeout = 15;
		string database;
		IsolationLevel isolationLevel = IsolationLevel.ReadCommitted;
		int minPoolSize;
		int maxPoolSize;
		ConnectionState state = ConnectionState.Closed;

		TdsConnectionParameters parms = new TdsConnectionParameters ();
		TdsTransaction transaction = null;

		// This is the collection of connection pools available
		static Hashtable pools = new Hashtable ();

		// Our TDS object, the real workhorse
		Tds tds = null;

		#endregion // Fields

		#region Constructors

		public TdsConnection ()
			: this (String.Empty)
		{
		}

		public TdsConnection (string connectionString)
		{
			parms.PacketSize = 512;
			parms.TdsVersion = TdsVersion.tds42;
			parms.User = null;
			parms.Password = null;
			SetConnectionString (connectionString);
		}
			
		#endregion // Constructors

		#region Properties

		public string ConnectionString {
			get { return connectionString; }
			set { SetConnectionString (value); }
		}

		public string Database {
			get { return parms.Database; }
		}

		public string DataSource {
			get { return parms.DataSource; }
		}

		public ConnectionState State {
			get { return state; }
		}
		
		public int ConnectionTimeout {
			get { return connectionTimeout; }
		}

		public int PacketSize {
			get { return parms.PacketSize; }
		}

		public string User {
			get { return parms.User; }
		}

		public string Password {
			get { return parms.Password; }
		}

		internal Tds Tds {
			get { return tds; }
		}
		
		#endregion // Properties

		#region Methods

		private static Tds AllocateTds (TdsConnectionParameters parms, string connectionString, bool disablePooling, int minPoolSize, int maxPoolSize)
		{
			if (disablePooling)
				return new Tds (parms);

			TdsConnectionPool pool = (TdsConnectionPool) pools[connectionString];
			if (pool == null) {
				lock (pools) {
					pool = new TdsConnectionPool (parms, minPoolSize, maxPoolSize);
					pools[connectionString] = pool;
				}
			}

			return pool.FindAnAvailableTds ();
		}

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

			tds.ChangeDatabase (databaseName);
		}

		public void Close ()
		{
			// rollback any open transactions
			if (transaction.Open)
				transaction.Rollback ();

			// if we aren't pooling, just close the connection
			// otherwise, just set the InUse flag to false
			if (disablePooling)
				tds.Close ();
			else
				tds.InUse = false;
			this.state = ConnectionState.Closed;
		}

		public TdsCommand CreateCommand ()
		{
			TdsCommand command = new TdsCommand ();
			command.Connection = this; 
			return command;
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
			if (User == null)
			{
				throw new ArgumentException ();
			}
			if (Password == null)
			{
				throw new ArgumentException ();
			}

			tds = AllocateTds (parms, connectionString, disablePooling, minPoolSize, maxPoolSize);
			tds.InUse = true;
			tds.Logon (parms);
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
				parameters["PACKET SIZE"] = "8192";
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
					break;
				case "CURRENT LANGUAGE" :
					parms.Language = value;
					break;
				case "DATA SOURCE" :
				case "SERVER" :
				case "ADDRESS" :
				case "ADDR" :
				case "NETWORK ADDRESS" :
					parms.DataSource = value;
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
					parms.PacketSize = Int32.Parse (value);
					break;
				case "PASSWORD" :
				case "PWD" :
					parms.Password = value;
					break;
				case "PERSIST SECURITY INFO" :
					break;
				case "POOLING" :
					disablePooling = (value.ToUpper ().Equals ("FALSE") || value.ToUpper ().Equals ("NO"));
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
