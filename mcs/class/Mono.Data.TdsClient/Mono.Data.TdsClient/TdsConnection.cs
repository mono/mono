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
using System.ComponentModel;
using System.Data;

namespace Mono.Data.TdsClient {
        public class TdsConnection : Component, ICloneable, IDbConnection
	{
		#region Fields

		internal TdsConnectionInternal connection;

		#endregion // Fields

		#region Constructors

		public TdsConnection (TdsServerType serverType)
			: this (serverType, 15, true, false, IsolationLevel.ReadCommitted)
		{
		}

		public TdsConnection (TdsServerType serverType, int connectionTimeout)
			: this (serverType, connectionTimeout, true, false, IsolationLevel.ReadCommitted)
		{
		}

		public TdsConnection (TdsServerType serverType, int connectionTimeout, bool autoCommit)
			: this (serverType, connectionTimeout, autoCommit, false, IsolationLevel.ReadCommitted)
		{
		}

		public TdsConnection (TdsServerType serverType, int connectionTimeout, bool autoCommit, bool readOnly, IsolationLevel isolationLevel)
		{
			connection = new TdsConnectionInternal (serverType, connectionTimeout, autoCommit, readOnly, isolationLevel);
		}
			
		#endregion // Constructors

		#region Properties

		public string ConnectionString {
			get { return connection.ConnectionString; }
			set { connection.ConnectionString = value; }
		}

		public string Database {
			get { return connection.Database; }
			set { connection.Database = value; }
		}

		public ConnectionState State {
			get { return connection.State; }
		}
		
		public int ConnectionTimeout {
			get { return connection.ConnectionTimeout; }
			set { connection.ConnectionTimeout = value; }
		}
		
		#endregion // Properties

		#region Methods

		public void ChangeDatabase (string databaseName)
		{
			connection.ChangeDatabase (databaseName);
		}

		public void Close ()
		{
			connection.Close ();
		}

		IDbCommand IDbConnection.CreateCommand ()
		{
			return ((IDbConnection) connection).CreateCommand ();
		}

                object ICloneable.Clone()
                {
                        throw new NotImplementedException ();
                }

		IDbTransaction IDbConnection.BeginTransaction ()
		{
			return ((IDbConnection) connection).BeginTransaction ();
		}

		IDbTransaction IDbConnection.BeginTransaction (IsolationLevel il)
		{
			return ((IDbConnection) connection).BeginTransaction (il);
		}

		public void Open ()
		{
			connection.Open ();
		}

		#endregion // Methods
	}
}
