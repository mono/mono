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
				if (gdaConnection != IntPtr.Zero
				    && libgda.gda_connection_is_open (gdaConnection)) {
					return libgda.gda_connection_get_database (gdaConnection);
				}

				return null;
			}
		}

		public string DataSource {
			get {
				if (gdaConnection != IntPtr.Zero
				    && libgda.gda_connection_is_open (gdaConnection)) {
					return libgda.gda_connection_get_dsn (gdaConnection);
				}

				return null;
			}
		}

		public string Provider {
			get {
				if (gdaConnection != IntPtr.Zero
				    && libgda.gda_connection_is_open (gdaConnection)) {
					return libgda.gda_connection_get_provider (gdaConnection);
				}

				return null;
			}
		}

		public string ServerVersion {
			get {
				if (gdaConnection != IntPtr.Zero
				    && libgda.gda_connection_is_open (gdaConnection)) {
					return libgda.gda_connection_get_server_version (gdaConnection);
				}

				return null;
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
			if (gdaConnection == IntPtr.Zero)
				throw new ArgumentException ();
			if (State != ConnectionState.Open)
				throw new InvalidOperationException ();

			if (!libgda.gda_connection_change_database (gdaConnection, name))
				throw new OleDbException (this);
		}

		public void Close ()
		{
			if (State == ConnectionState.Open) {
				libgda.gda_connection_close (gdaConnection);
				gdaConnection = IntPtr.Zero;
			}
		}

		public OleDbCommand CreateCommand ()
		{
			if (State == ConnectionState.Open)
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
			string provider = "Default";
			string gdaCncStr = "";
			string[] args;
			int len;
			char [] separator = { ';' };
			
			if (State == ConnectionState.Open)
				throw new InvalidOperationException ();

			gdaConnection = libgda.gda_client_open_connection (libgda.GdaClient,
                                                                          connectionString,
                                                                          "", "", 0);
			
			/* convert the connection string to its GDA equivalent */
			//args = connectionString.Split (';');
			//len = args.Length;
			//for (int i = 0; i < len; i++) {
			//	string[] values = args[i].Split (separator, 2);
			//	if (values[0] == "Provider") {
			//		if (values[1] == "SQLOLEDB")
			//			provider = "FreeTDS";
			//		else if (values[1] == "MSDAORA")
			//			provider = "Oracle";
			//		else if (values[2] == "Microsoft.Jet.OLEDB.4.0")
			//			provider = "MS Access";
			//		else
			//			provider = values[2];
			//	}
			//	else if (values[0] == "Addr" || values[0] == "Address")
			//		gdaCncStr = String.Concat (gdaCncStr, "HOST=", values[1], ";");
			//	else if (values[0] == "Database")
			//		gdaCncStr = String.Concat (gdaCncStr, "DATABASE=", values[1], ";");
			//	else if (values[0] == "Connection Lifetime")
			//		connectionTimeout = System.Convert.ToInt32 (values[1]);
			//	else if (values[0] == "File Name")
			//		gdaCncStr = String.Concat (gdaCncStr, "FILENAME=", values[1], ";");
			//	else if (values[0] == "Password" || values[0] == "Pwd")
			//		gdaCncStr = String.Concat (gdaCncStr, "PASSWORD=", values[1], ";");
			//	else if (values[0] == "User ID")
			//		gdaCncStr = String.Concat (gdaCncStr, "USERNAME=", values[1], ";");
			//}

			/* open the connection */
			//System.Console.WriteLine ("Opening connection for provider " +
			//		  provider + " with " + gdaCncStr);
			//gdaConnection = libgda.gda_client_open_connection_from_string (libgda.GdaClient,
			//							       provider,
			//							       gdaCncStr);
		}

		[MonoTODO]
		public static void ReleaseObjectPool ()
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region Events and Delegates

		public event OleDbInfoMessageEventHandler InfoMessage;
		public event StateChangeEventHandler StateChange;

		#endregion
	}
}
