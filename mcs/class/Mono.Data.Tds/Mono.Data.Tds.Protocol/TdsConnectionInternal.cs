//
// Mono.Data.TdsClient.Internal.TdsConnectionInternal.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Mono.Data.TdsClient.Internal {
        internal class TdsConnectionInternal : Component, ICloneable, IDbConnection
	{
		#region Fields

		bool autoCommit;
		//TdsCommandCollection commands;
		ConnectionState connectionState;
		string connectionString;
		int connectionTimeout;
		string database;
		//TdsDbMetadata databaseMetadata;		
		string host;
		bool isClosed;
		int packetSize;
		string password;
		int port;
		bool readOnly;
		ArrayList tdsPool;

		TdsInternal tds;
		TdsConnectionParametersInternal parms;
		TdsServerTypeInternal serverType;
		TdsVersionInternal tdsVersion = TdsVersionInternal.tds42; // default to TDS version 4.2 which is used by both servers
		IsolationLevel isolationLevel;
		string user;
		string encodingName;
		Encoding encoding ;


		#endregion // Fields

		#region Constructors

		public TdsConnectionInternal (TdsServerTypeInternal serverType)
			: this (serverType, 15, true, false, IsolationLevel.ReadCommitted)
		{
		}

		public TdsConnectionInternal (TdsServerTypeInternal serverType, int connectionTimeout)
			: this (serverType, connectionTimeout, true, false, IsolationLevel.ReadCommitted)
		{
		}

		public TdsConnectionInternal (TdsServerTypeInternal serverType, int connectionTimeout, bool autoCommit)
			: this (serverType, connectionTimeout, autoCommit, false, IsolationLevel.ReadCommitted)
		{
		}

		public TdsConnectionInternal (TdsServerTypeInternal serverType, int connectionTimeout, bool autoCommit, bool readOnly, IsolationLevel isolationLevel)
		{
			this.connectionState = ConnectionState.Closed;
			this.serverType = serverType;
			this.autoCommit = autoCommit;
			this.isolationLevel = isolationLevel;
			this.readOnly = readOnly;
			this.connectionTimeout = connectionTimeout;
			this.packetSize = 512; // Minimum TDS packet size
			this.encoding = Encoding.GetEncoding (encodingName);
		}
			
		#endregion // Constructors

		#region Properties

		public string ApplicationName {
			get { return parms.ApplicationName; }
			set { parms.ApplicationName = value; }
		}

		public ConnectionState State {
			get { return connectionState; }
		}

		public string ConnectionString {
			get { return connectionString; }
			set { connectionString = value; }
		}
		
		public int ConnectionTimeout {
			get { return connectionTimeout; }
			set { connectionTimeout = value; }
		}
		
		public string Host {
			get { return host; }
			set { host = value; }
		}
		
		public int Port {
			get { return port; }
			set { port = value; }
		}
		
		public string Database {
			get { return database; }
			set { database = value; }
		}
		
		public string User {
			get { return parms.User; }
			set { parms.User = value; }
		}
		
		public string Password {
			get { return parms.Password; }
			set { parms.Password = value; }
		}
		
		public int PacketSize {
			get { return parms.PacketSize; }
			set { parms.PacketSize = value; }
		}

		public TdsVersionInternal TdsVersion {
			get { return parms.TdsVersion; }
			set { parms.TdsVersion = value; }
		}

		#endregion // Properties

		#region Methods

		public TdsTransactionInternal BeginTransaction ()
		{
			return BeginTransaction (IsolationLevel.ReadCommitted);
		}

		public TdsTransactionInternal BeginTransaction (IsolationLevel il)
		{
			return new TdsTransactionInternal (this, il);
		}

		[System.MonoTODO]
		public void ChangeDatabase (string databaseName)
		{
			if (Database == databaseName)
				return;

			foreach (TdsInstance instance in tdsPool)
			{
				lock (instance.Tds) {
					TdsCommandInternal command = instance.Tds.Command;

					object o = (command == null ? (object) instance.Tds : command);

					lock (o) {
						if (command != null)
							command.SkipToEnd ();
						instance.Tds.ChangeDatabase (databaseName);
					}
				}
			}
		}

		[System.MonoTODO("Logout?")]
		public void Close ()
		{
			//socket.Shutdown (SocketShutdown.Both);
			//socket.Close ();
		}

		public TdsCommandInternal CreateCommand ()
		{
			TdsCommandInternal command = new TdsCommandInternal ();
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
			if (user == String.Empty || user == null)
			{
				//throw new TdsException ("Need a username.");
			}
			if (password == null)
			{
				//throw new TdsException ("Need a username.");
			}

			if (tdsPool == null)
				tdsPool = new ArrayList ();

        		TdsInternal tmpTds = AllocateTds ();
        		tdsVersion = tmpTds.TdsVersion;
        		database = tmpTds.Database;
        		FreeTds (tmpTds);
			tds.Logon (parms);
		}

		private TdsInternal AllocateTds ()
		{
			TdsInternal result;

			int index = FindAnAvailableTds ();
			if (index == -1) {
				TdsInternal tmpTds = null;
				tmpTds = new TdsInternal (this, parms);

				TdsInstance tmp = new TdsInstance (tmpTds);
				tdsPool.Add (tmp);
				index = FindAnAvailableTds ();
			}
			if (index == -1) {
				//throw new TdsException ("Internal error.  Could not get a tds instance.");
			}
			if (((TdsInstance) tdsPool[index]).InUse) {
				//throw new TdsException ("Internal error. Tds instance already in use.");
			}
			((TdsInstance) tdsPool[index]).InUse = true;
			result = ((TdsInstance) tdsPool[index]).Tds;
			result.ChangeSettings (autoCommit, isolationLevel);

			return result;
		}

		private int FindAnAvailableTds ()
		{
			for (int i = tdsPool.Count - 1; i >= 0; i --)
				if (!((TdsInstance) tdsPool[i]).InUse)
					return i;
			return -1;
		}

		private void FreeTds (TdsInternal tds)
		{
			int i = -1;
			foreach (TdsInstance instance in tdsPool)
				if (instance.Tds == tds) {
					instance.InUse = false;
					tds.Command = null;
					return;
				}
			// throw new TdsException ("Tried to free a tds that wasn't in use");
		}

		#endregion // Methods

		class TdsInstance 
		{
			bool inUse;
			TdsInternal tds;

			public bool InUse {
				get { return inUse; }
				set { inUse = true; }
			}

			public TdsInternal Tds {
				get { return tds; }
				set { tds = value; }
			}

			public TdsInstance (TdsInternal tds)
			{
				this.tds = tds;
				this.inUse = false;
			}
		}
	}
}
