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

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.EnterpriseServices;
#if NET_2_0
using System.Transactions;
#endif

namespace System.Data.OleDb
{
	[DefaultEvent ("InfoMessage")]
#if NET_2_0
	public sealed class OleDbConnection : DbConnection, ICloneable
#else
	public sealed class OleDbConnection : Component, ICloneable, IDbConnection
#endif
	{
		#region Fields

		string connectionString;
		int connectionTimeout;
		IntPtr gdaConnection;

		#endregion

		#region Constructors
		
		public OleDbConnection ()
		{
			gdaConnection = IntPtr.Zero;
			connectionTimeout = 15;
		}

		public OleDbConnection (string connectionString) : this ()
		{
			this.connectionString = connectionString;
		}

		#endregion // Constructors

		#region Properties
		
		[DataCategory ("Data")]
		[DefaultValue ("")]
		[EditorAttribute ("Microsoft.VSDesigner.Data.ADO.Design.OleDbConnectionStringEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
		[RecommendedAsConfigurable (true)]
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		public override string ConnectionString {
			get {
				if (connectionString == null)
					return string.Empty;
				return connectionString;
			}
			set {
				connectionString = value;
			}
		}

		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public override int ConnectionTimeout {
			get {
				return connectionTimeout;
			}
		}

		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public override 
		string Database {
			get {
				if (gdaConnection != IntPtr.Zero
					&& libgda.gda_connection_is_open (gdaConnection)) {
					return libgda.gda_connection_get_database (gdaConnection);
				}

				return string.Empty;
			}
		}

		[BrowsableAttribute (true)]
		public override string DataSource {
			get {
				if (gdaConnection != IntPtr.Zero
					&& libgda.gda_connection_is_open (gdaConnection)) {
					return libgda.gda_connection_get_dsn (gdaConnection);
				}

				return string.Empty;
			}
		}

		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		[BrowsableAttribute (true)]
		public string Provider {
			get {
				if (gdaConnection != IntPtr.Zero
					&& libgda.gda_connection_is_open (gdaConnection)) {
					return libgda.gda_connection_get_provider (gdaConnection);
				}

				return string.Empty;
			}
		}

		public override string ServerVersion {
			get {
				if (State == ConnectionState.Closed)
					throw ExceptionHelper.ConnectionClosed ();
				return libgda.gda_connection_get_server_version (gdaConnection);
			}
		}

		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		[BrowsableAttribute (false)]
		public override ConnectionState State {
			get {
				if (gdaConnection != IntPtr.Zero) {
					if (libgda.gda_connection_is_open (gdaConnection))
						return ConnectionState.Open;
				}

				return ConnectionState.Closed;
			}
		}

		internal IntPtr GdaConnection {
			get {
				return gdaConnection;
			}
		}
		
		#endregion // Properties
	
		#region Methods
	
		public new OleDbTransaction BeginTransaction ()
		{
			if (State == ConnectionState.Closed)
				throw ExceptionHelper.ConnectionClosed ();
			return new OleDbTransaction (this);
		}

		public new OleDbTransaction BeginTransaction (IsolationLevel isolationLevel)
		{
			if (State == ConnectionState.Closed)
				throw ExceptionHelper.ConnectionClosed ();
			return new OleDbTransaction (this, isolationLevel);
		}

		protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
		{
			return BeginTransaction (isolationLevel);
		}

		protected override DbCommand CreateDbCommand()
		{
			return CreateCommand ();
		}

		public override void ChangeDatabase (string value)
		{
			if (State != ConnectionState.Open)
				throw new InvalidOperationException ();

			if (!libgda.gda_connection_change_database (gdaConnection, value))
				throw new OleDbException (this);
		}

		public override void Close ()
		{
			if (State == ConnectionState.Open) {
				libgda.gda_connection_close (gdaConnection);
				gdaConnection = IntPtr.Zero;
			}
		}

		public new OleDbCommand CreateCommand ()
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

		public
#if NET_2_0
		override
#endif
		void Open ()
		{
//			string provider = "Default";
//			string gdaCncStr = string.Empty;
//			string[] args;
//			int len;
//			char [] separator = { ';' };
			
			if (State == ConnectionState.Open)
				throw new InvalidOperationException ();

			libgda.gda_init ("System.Data.OleDb", "1.0", 0, new string [0]);

			gdaConnection = libgda.gda_client_open_connection (libgda.GdaClient,
				ConnectionString, string.Empty, string.Empty, 0);

			if (gdaConnection == IntPtr.Zero)
				throw new OleDbException (this);
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

		[MonoTODO]
		public void EnlistDistributedTransaction (ITransaction transaction)
		{
			throw new NotImplementedException ();
		}

#if NET_2_0
		[MonoTODO]
		public override void EnlistTransaction (Transaction transaction)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override DataTable GetSchema ()
		{
			if (State == ConnectionState.Closed)
				throw ExceptionHelper.ConnectionClosed ();
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override DataTable GetSchema(string collectionName)
		{
			return GetSchema (collectionName, null);
		}

		[MonoTODO]
		public override DataTable GetSchema (String collectionName, string [] restrictionValues)
		{
			if (State == ConnectionState.Closed)
				throw ExceptionHelper.ConnectionClosed ();
			throw new NotImplementedException ();
		}

		[MonoTODO]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public void ResetState ()
		{
			throw new NotImplementedException ();
		}
#endif

		#endregion

		#region Events and Delegates

#if !NET_2_0
		[DataSysDescription ("Event triggered when messages arrive from the DataSource.")]
#endif
		[DataCategory ("DataCategory_InfoMessage")]
		public event OleDbInfoMessageEventHandler InfoMessage;

#if !NET_2_0
		[DataSysDescription ("Event triggered when the connection changes state.")]
		[DataCategory ("DataCategory_StateChange")]
		public event StateChangeEventHandler StateChange;
#endif

		#endregion
	}
}
