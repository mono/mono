//
// System.Data.OleDb.OleDbConnection
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Rodrigo Moya, 2002
// Copyright (C) Tim Coleman, 2002
//

using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace System.Data.OleDb
{
	public sealed class OleDbConnection : Component, ICloneable, IDbConnection
	{
		#region Fields

		string connectionString;
		int connectionTimeout;
		OleDbDataReader dataReader;
		bool dataReaderOpen;
		IntPtr gdaConnection;

		#endregion

		#region Constructors
		
		public OleDbConnection ()
		{
			libgda.gda_init ("System.Data.OleDb", "1.0", 0, new string [0]);
			gdaConnection = IntPtr.Zero;
			connectionTimeout = 15;
			connectionString = null;
		}

		public OleDbConnection (string connectionString) : this ()
		{
			this.connectionString = connectionString;
		}

		#endregion // Constructors

		#region Properties

		public string ConnectionString {
			get {
				return connectionString;
			}
			set {
				connectionString = value;
			}
		}

		public int ConnectionTimeout {
			get {
				return connectionTimeout;
			}
		}

		public string Database { 
			get {
				if (gdaConnection != IntPtr.Zero && libgda.gda_connection_is_open (gdaConnection)) {
					return libgda.gda_connection_get_database (gdaConnection);
				}

				return null;
			}
		}

		public string DataSource {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public string Provider {
			get {
				if (gdaConnection != IntPtr.Zero && libgda.gda_connection_is_open (gdaConnection)) {
					return libgda.gda_connection_get_provider (gdaConnection);
				}

				return null;
			}
		}

		public string ServerVersion {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public ConnectionState State
		{
			get {
				if (gdaConnection != IntPtr.Zero) {
					if (libgda.gda_connection_is_open (gdaConnection))
						return ConnectionState.Open;
				}

				return ConnectionState.Closed;
			}
		}

		internal IntPtr GdaConnection
		{
			get {
				return gdaConnection;
			}
		}

		#endregion // Properties
	
		#region Methods
	
		public OleDbTransaction BeginTransaction ()
		{
			if (gdaConnection != IntPtr.Zero)
				return new OleDbTransaction (this);

			return null;
		}

		IDbTransaction IDbConnection.BeginTransaction ()
		{
			return BeginTransaction ();
		}
		
		public OleDbTransaction BeginTransaction (IsolationLevel level)
		{
			if (gdaConnection != IntPtr.Zero)
				return new OleDbTransaction (this, level);

			return null;
		}

		IDbTransaction IDbConnection.BeginTransaction (IsolationLevel level)
		{
			return BeginTransaction (level);
		}

		public void ChangeDatabase (string name)
		{
			// FIXME: see http://bugzilla.gnome.org/show_bug.cgi?id=83315
		}

		public void Close ()
		{
			if (gdaConnection != IntPtr.Zero) {
				libgda.gda_connection_close (gdaConnection);
				gdaConnection = IntPtr.Zero;
			}
		}

		public OleDbCommand CreateCommand ()
		{
			if (gdaConnection != IntPtr.Zero && libgda.gda_connection_is_open (gdaConnection))
				return new OleDbCommand (null, this);

			return null;
		}

		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataTable GetOleDbSchemaTable (Guid schema, object[] restrictions)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		object ICloneable.Clone ()
		{
			throw new NotImplementedException();
		}

		IDbCommand IDbConnection.CreateCommand ()
		{
			return CreateCommand ();
		}

		public void Open ()
		{
			if (State == ConnectionState.Open)
				throw new InvalidOperationException ();

			gdaConnection = libgda.gda_client_open_connection (libgda.GdaClient,
									   connectionString,
									   "", "");
		}

		[MonoTODO]
		public static void ReleaseObjectPool ()
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region Internal Methods

		// Used to prevent OleDbConnection
		// from doing anything while
		// OleDbDataReader is open.
		// Open the Reader. (called from OleDbCommand)
		internal void OpenReader (OleDbDataReader reader)
		{
			if(dataReaderOpen == true) {
				// TODO: throw exception here?
				//       because a reader
				//       is already open
			}
			else {
				dataReader = reader;
				dataReaderOpen = true;
			}
		}


		#endregion

		#region Events and Delegates

		public event OleDbInfoMessageEventHandler InfoMessage;
		public event StateChangeEventHandler StateChange;

		#endregion
	}
}
