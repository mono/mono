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

		bool autoCommit = true;
		string connectionString;
		int connectionTimeout = 15;
		string database;
		IsolationLevel isolationLevel = IsolationLevel.ReadCommitted;
		int packetSize = 512;
		ConnectionState state = ConnectionState.Closed;

		string user;
		string password;

		ArrayList tdsPool;

		TdsServerType serverType = TdsServerType.Generic;
		TdsVersion tdsVersion = TdsVersion.tds42;
		TdsConnectionParameters parms;

		#endregion // Fields

		#region Constructors

		public TdsConnection ()
			: this (String.Empty)
		{
		}

		public TdsConnection (string connectionString)
		{
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

		public ConnectionState State {
			get { return state; }
		}
		
		public int ConnectionTimeout {
			get { return connectionTimeout; }
			set { connectionTimeout = value; }
		}
		
		#endregion // Properties

		#region Methods

		private Tds AllocateTds ()
		{
			Tds result;

			int index = FindAnAvailableTds ();
			if (index == -1) {
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
			return new TdsTransaction (this, il);
		}

		public void ChangeDatabase (string databaseName)
		{
			if (Database == databaseName)
				return;

			foreach (TdsInstance instance in tdsPool) {
				lock (instance.Tds) {
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

		[System.MonoTODO]
		public void Close ()
		{
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
			if (user == String.Empty || user == null)
			{
				throw new ArgumentException ();
			}
			if (password == null)
			{
				throw new ArgumentException ();
			}

			if (tdsPool == null)
				tdsPool = new ArrayList ();

			Tds tmpTds = AllocateTds ();
			tdsVersion = tmpTds.TdsVersion;
			database = tmpTds.Database;
			FreeTds (tmpTds);
			tmpTds.Logon (parms);
		}

		[System.MonoTODO]
		private void SetConnectionString (string connectionString)
		{
			if (connectionString == String.Empty)
				return;

			throw new NotImplementedException ();
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
