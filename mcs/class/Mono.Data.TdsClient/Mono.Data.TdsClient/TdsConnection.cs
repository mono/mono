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
		string connectionString = String.Empty;
		int connectionTimeout = 15;
		string database;
		IsolationLevel isolationLevel = IsolationLevel.ReadCommitted;
		ConnectionState state = ConnectionState.Closed;

		ArrayList tdsPool = null;
		int minPoolSize;
		int maxPoolSize;

		TdsConnectionParameters parms = new TdsConnectionParameters ();
		TdsTransaction transaction = null;

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
			set { parms.Database = value; }
		}

		public string DataSource {
			get { return parms.DataSource; }
			set { parms.DataSource = value; }
		}

		public ConnectionState State {
			get { return state; }
		}
		
		public int ConnectionTimeout {
			get { return connectionTimeout; }
			set { connectionTimeout = value; }
		}

		public int PacketSize {
			get { return parms.PacketSize; }
			set { parms.PacketSize = value; }
		}

		public string User {
			get { return parms.User; }
			set { parms.User = value; }
		}

		public string Password {
			get { return parms.Password; }
			set { parms.Password = value; }
		}
		
		#endregion // Properties

		#region Methods

		internal Tds AllocateTds ()
		{
			Tds result;

			// Make sure we have at least the minimum pool size.
			while (tdsPool.Count < minPoolSize)
				tdsPool.Add (new TdsInstance (new Tds (parms)));

			int index = FindAnAvailableTds ();
			if (index == -1) {
				// Make sure we don't exceed the maximum
				if (tdsPool.Count > maxPoolSize)
					throw new TdsException ("Connection pool has reached maximum size and cannot be expanded.");

				Tds tmpTds = new Tds (parms);
				TdsInstance tmp = new TdsInstance (tmpTds);
				tdsPool.Add (tmp);
				index = FindAnAvailableTds ();
			}
			if (index == -1) 
				throw new TdsException ("Internal error. Could not get a tds instance.");

			if (((TdsInstance) tdsPool [index]).InUse) 
				throw new TdsException ("Internal error. Tds instance already in use.");
			((TdsInstance) tdsPool [index]).InUse = true;
			result = ((TdsInstance) tdsPool [index]).Tds;
			result.ChangeSettings (autoCommit, isolationLevel);

			return result;
		}

		public TdsTransaction BeginTransaction ()
		{
			return BeginTransaction (IsolationLevel.ReadCommitted);
		}

		public TdsTransaction BeginTransaction (IsolationLevel il)
		{
			if (transaction != null)
				throw new InvalidOperationException ("TdsConnection does not support parallel transactions.");
			transaction = new TdsTransaction (this, il);
			return transaction;
		}

		public void ChangeDatabase (string databaseName)
		{
			if (Database == databaseName)
				return;

			foreach (TdsInstance instance in tdsPool) {
				lock (instance.Tds) {
					instance.Tds.ChangeDatabase (databaseName);
					/*
					TdsCommand command = instance.Tds.Command;
					object o = (command == null ? (object) instance.Tds : command);
					lock (o) {
						if (command != null)
							command.SkipToEnd ();
						instance.Tds.ChangeDatabase (databaseName);
					}
					*/
				}
			}
		}

		[MonoTODO]
		public void Close ()
		{
			this.state = ConnectionState.Closed;
			throw new NotImplementedException ();
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

		private int FindAnAvailableTds ()
		{
			for (int i = tdsPool.Count - 1; i >= 0; i --)
				if (!((TdsInstance) tdsPool [i]).InUse)
					return i;
			return -1;
		}

		private void FreeTds (Tds tds)
		{
			int i = -1;
			foreach (TdsInstance instance in tdsPool)
				if (instance.Tds == tds) {
					instance.InUse = false;
					//tds.Command = null;
					return;
				}
			throw new TdsException ("Tried to free a tds that wasn't in use.");
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
			if (User == null)
			{
				throw new ArgumentException ();
			}
			if (Password == null)
			{
				throw new ArgumentException ();
			}

			if (tdsPool == null)
				tdsPool = new ArrayList ();

			Tds tmpTds = AllocateTds ();
			database = tmpTds.Database;
			FreeTds (tmpTds);
			tmpTds.Logon (parms);

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

			if (this.ConnectionString == String.Empty)
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
					this.ConnectionTimeout = Int32.Parse (value);
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

		private class TdsInstance 
		{
			#region Fields

			bool inUse;
			Tds tds;

			#endregion // Fields 

			#region Constructors

			public TdsInstance (Tds tds) 
			{
				this.tds = tds;
				this.inUse = false;
			}

			#endregion // Constructors

			#region Properties

			public bool InUse {
				get { return inUse; }
				set { inUse = value; }
			}

			public Tds Tds {
				get { return tds; }
				set { tds = value; }
			}

			#endregion // Properties
		}

	}
}
