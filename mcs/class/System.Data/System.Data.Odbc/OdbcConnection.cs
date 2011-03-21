//
// System.Data.Odbc.OdbcConnection
//
// Authors:
//  Brian Ritchie (brianlritchie@hotmail.com) 
//
// Copyright (C) Brian Ritchie, 2002
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

using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.EnterpriseServices;
using System.Runtime.InteropServices;
using System.Text;
#if NET_2_0 && !TARGET_JVM
using System.Transactions;
#endif

namespace System.Data.Odbc
{
	[DefaultEvent ("InfoMessage")]
#if NET_2_0
	public sealed class OdbcConnection : DbConnection, ICloneable
#else
	public sealed class OdbcConnection : Component, ICloneable, IDbConnection
#endif //NET_2_0
	{
		#region Fields

		string connectionString;
		int connectionTimeout;
		internal OdbcTransaction transaction;
		IntPtr henv = IntPtr.Zero;
		IntPtr hdbc = IntPtr.Zero;
		bool disposed;
		ArrayList linkedCommands;

		#endregion

		#region Constructors
		
		public OdbcConnection () : this (String.Empty)
		{
		}

		public OdbcConnection (string connectionString)
		{
			connectionTimeout = 15;
			ConnectionString = connectionString;
		}

		#endregion // Constructors

		#region Properties

		internal IntPtr hDbc {
			get { return hdbc; }
		}

		internal object Generation {
			// We use the linkedCommands array as a generation indicator for statement
			// handles allocated in our subsiduary OdbcCommands.  The rule is that the
			// statement handles are only valid if the generation matches the one
			// returned when the command was linked to the connection.
			get { return linkedCommands; }
		}

		[OdbcCategoryAttribute ("DataCategory_Data")]
		[DefaultValue ("")]
		[OdbcDescriptionAttribute ("Information used to connect to a Data Source")]
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		[EditorAttribute ("Microsoft.VSDesigner.Data.Odbc.Design.OdbcConnectionStringEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
		[RecommendedAsConfigurableAttribute (true)]
		public
#if NET_2_0
		override
#endif
		string ConnectionString {
			get {
				if (connectionString == null)
					return string.Empty;
				return connectionString;
			}
			set { connectionString = value; }
		}
		
		[OdbcDescriptionAttribute ("Current connection timeout value, not settable  in the ConnectionString")]
		[DefaultValue (15)]
#if NET_2_0
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
#endif
		public
#if NET_2_0
		new
#endif // NET_2_0
		int ConnectionTimeout {
			get {
				return connectionTimeout;
			}
			set {
				if (value < 0)
					throw new ArgumentException("Timout should not be less than zero.");
				connectionTimeout = value;
			}
		}

		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		[OdbcDescriptionAttribute ("Current data source Catlog value, 'Database=X' in the ConnectionString")]
		public
#if NET_2_0
		override
#endif // NET_2_0
		string Database {
			get {
				if (State == ConnectionState.Closed)
					return string.Empty;
				return GetInfo (OdbcInfo.DatabaseName);
			}
		}

		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		[OdbcDescriptionAttribute ("The ConnectionState indicating whether the connection is open or closed")]
		[BrowsableAttribute (false)]
		public
#if NET_2_0
		override
#endif // NET_2_0
		ConnectionState State {
			get {
				if (hdbc != IntPtr.Zero)
					return ConnectionState.Open;
				return ConnectionState.Closed;
			}
		}

		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		[OdbcDescriptionAttribute ("Current data source, 'Server=X' in the ConnectionString")]
#if NET_2_0
		[Browsable (false)]
#endif
		public
#if NET_2_0
		override
#endif // NET_2_0
		string DataSource {
			get {
				if (State == ConnectionState.Closed)
					return string.Empty;
				return GetInfo (OdbcInfo.DataSourceName);
			}
		}

#if NET_2_0
		[Browsable (false)]
#endif
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		[OdbcDescriptionAttribute ("Current ODBC Driver")]
		public string Driver {
			get {
				if (State == ConnectionState.Closed)
					return string.Empty;

				return GetInfo (OdbcInfo.DriverName);
			}
		}
		
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		[OdbcDescriptionAttribute ("Version of the product accessed by the ODBC Driver")]
		[BrowsableAttribute (false)]
		public
#if NET_2_0
		override
#endif // NET_2_0
		string ServerVersion {
			get {
				return GetInfo (OdbcInfo.DbmsVersion);
			}
		}

		internal string SafeDriver {
			get {
				string driver_name = GetSafeInfo (OdbcInfo.DriverName);
				if (driver_name == null)
					return string.Empty;
				return driver_name;
			}
		}

		#endregion // Properties
	
		#region Methods
	
		public
#if NET_2_0
		new
#endif // NET_2_0
		OdbcTransaction BeginTransaction ()
		{
			return BeginTransaction (IsolationLevel.Unspecified);
		}

#if ONLY_1_1
		IDbTransaction IDbConnection.BeginTransaction ()
		{
			return (IDbTransaction) BeginTransaction ();
		}
#endif // ONLY_1_1

#if NET_2_0
		protected override DbTransaction BeginDbTransaction (IsolationLevel isolationLevel)
		{
			return BeginTransaction (isolationLevel);
		}
#endif

		public
#if NET_2_0
		new
#endif // NET_2_0
		OdbcTransaction BeginTransaction (IsolationLevel isolevel)
		{
			if (State == ConnectionState.Closed)
				throw ExceptionHelper.ConnectionClosed ();

			if (transaction == null) {
				transaction = new OdbcTransaction (this, isolevel);
				return transaction;
			} else
				throw new InvalidOperationException ();
		}

#if ONLY_1_1
		IDbTransaction IDbConnection.BeginTransaction (IsolationLevel isolevel)
		{
			return (IDbTransaction) BeginTransaction (isolevel);
		}
#endif // ONLY_1_1

		public
#if NET_2_0
		override
#endif // NET_2_0
		void Close ()
		{
			OdbcReturn ret = OdbcReturn.Error;
			if (State == ConnectionState.Open) {
				lock(this) {
					// close any associated commands
					// NOTE: we may 'miss' some if the garbage collector has
					// already started to destroy them.
					if (linkedCommands != null) {
						for (int i = 0; i < linkedCommands.Count; i++) {
							WeakReference wr = (WeakReference) linkedCommands [i];
							if (wr == null)
								continue;
							OdbcCommand c = (OdbcCommand) wr.Target;
							if (c != null)
								c.Unlink ();
						}
						linkedCommands = null;
					}

					// disconnect
					ret = libodbc.SQLDisconnect (hdbc);

				}
				// There could be OdbcCommands outstanding (see NOTE above); their
				// hstmts will have been freed and therefore will be invalid.
				// However, they will find that their definition of Generation
				// does not match the connection's, so they won't try and free
				// those hstmt.
				if ((ret != OdbcReturn.Success) && (ret != OdbcReturn.SuccessWithInfo))
					throw CreateOdbcException (OdbcHandleType.Dbc, hdbc);

				FreeHandles ();
				transaction = null;
				RaiseStateChange (ConnectionState.Open, ConnectionState.Closed);
			}
		}

		public
#if NET_2_0
		new
#endif // NET_2_0
		OdbcCommand CreateCommand ()
		{
			return new OdbcCommand (string.Empty, this, transaction);
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		void ChangeDatabase (string value)
		{
			IntPtr ptr = IntPtr.Zero;
			OdbcReturn ret = OdbcReturn.Error;

			try {
				ptr = Marshal.StringToHGlobalUni (value);
				ret = libodbc.SQLSetConnectAttr (hdbc, OdbcConnectionAttribute.CurrentCatalog, ptr, value.Length * 2);

				if (ret != OdbcReturn.Success && ret != OdbcReturn.SuccessWithInfo)
					throw CreateOdbcException (OdbcHandleType.Dbc, hdbc);
			} finally {
				if (ptr != IntPtr.Zero)
					Marshal.FreeCoTaskMem (ptr);
			}
		}

		protected override void Dispose (bool disposing)
		{
			if (!this.disposed) {
				try {
					// release the native unmananged resources
					this.Close ();
					this.disposed = true;
				} finally {
					// call Dispose on the base class
					base.Dispose (disposing);
				}
			}
		}

		[MonoTODO]
		object ICloneable.Clone ()
		{
			throw new NotImplementedException ();
		}

#if ONLY_1_1
		IDbCommand IDbConnection.CreateCommand ()
		{
			return (IDbCommand) CreateCommand ();
		}
#endif //ONLY_1_1

#if NET_2_0
		protected override DbCommand CreateDbCommand ()
		{
			return CreateCommand ();
		}
#endif

		public
#if NET_2_0
		override
#endif // NET_2_0
		void Open ()
		{
			if (State == ConnectionState.Open)
				throw new InvalidOperationException ();

			OdbcReturn ret = OdbcReturn.Error;
			OdbcException e = null;
		
			try {
				// allocate Environment handle
				ret = libodbc.SQLAllocHandle (OdbcHandleType.Env, IntPtr.Zero, ref henv);
				if ((ret != OdbcReturn.Success) && (ret != OdbcReturn.SuccessWithInfo)) {
					OdbcErrorCollection errors = new OdbcErrorCollection ();
					errors.Add (new OdbcError (this));
					e = new OdbcException (errors);
					MessageHandler (e);
					throw e;
				}

				ret = libodbc.SQLSetEnvAttr (henv, OdbcEnv.OdbcVersion, (IntPtr) libodbc.SQL_OV_ODBC3 , 0); 
				if ((ret != OdbcReturn.Success) && (ret != OdbcReturn.SuccessWithInfo))
					throw CreateOdbcException (OdbcHandleType.Env, henv);

				// allocate connection handle
				ret = libodbc.SQLAllocHandle (OdbcHandleType.Dbc, henv, ref hdbc);
				if ((ret != OdbcReturn.Success) && (ret != OdbcReturn.SuccessWithInfo))
					throw CreateOdbcException (OdbcHandleType.Env, henv);

				// DSN connection
				if (ConnectionString.ToLower ().IndexOf ("dsn=") >= 0) {
					string _uid = string.Empty, _pwd = string.Empty, _dsn = string.Empty;
					string [] items = ConnectionString.Split (new char[1] {';'});
					foreach (string item in items)
					{
						string [] parts = item.Split (new char[1] {'='});
						switch (parts [0].Trim ().ToLower ()) {
						case "dsn":
							_dsn = parts [1].Trim ();
							break;
						case "uid":
							_uid = parts [1].Trim ();
							break;
						case "pwd":
							_pwd = parts [1].Trim ();
							break;
						}
					}
					ret = libodbc.SQLConnect(hdbc, _dsn, -3, _uid, -3, _pwd, -3);
					if ((ret != OdbcReturn.Success) && (ret != OdbcReturn.SuccessWithInfo))
						throw CreateOdbcException (OdbcHandleType.Dbc, hdbc);
				} else {
					// DSN-less Connection
					string OutConnectionString = new String (' ',1024);
					short OutLen = 0;
					ret = libodbc.SQLDriverConnect (hdbc, IntPtr.Zero, ConnectionString, -3, 
						OutConnectionString, (short) OutConnectionString.Length, ref OutLen, 0);
					if ((ret != OdbcReturn.Success) && (ret != OdbcReturn.SuccessWithInfo))
						throw CreateOdbcException (OdbcHandleType.Dbc, hdbc);
				}

				RaiseStateChange (ConnectionState.Closed, ConnectionState.Open);
			} catch {
				// free handles if any.
				FreeHandles ();
				throw;
			}
			disposed = false;
		}

		[MonoTODO]
		public static void ReleaseObjectPool ()
		{
			throw new NotImplementedException ();
		}

		private void FreeHandles ()
		{
			OdbcReturn ret = OdbcReturn.Error;
			if (hdbc != IntPtr.Zero) {
				ret = libodbc.SQLFreeHandle ((ushort) OdbcHandleType.Dbc, hdbc);
				if ( (ret != OdbcReturn.Success) && (ret != OdbcReturn.SuccessWithInfo))
					throw CreateOdbcException (OdbcHandleType.Dbc, hdbc);
			}
			hdbc = IntPtr.Zero;

			if (henv != IntPtr.Zero) {
				ret = libodbc.SQLFreeHandle ((ushort) OdbcHandleType.Env, henv);
				if ( (ret != OdbcReturn.Success) && (ret != OdbcReturn.SuccessWithInfo))
					throw CreateOdbcException (OdbcHandleType.Env, henv);
			}
			henv = IntPtr.Zero;
		}

#if NET_2_0
		public override DataTable GetSchema ()
		{
			if (State == ConnectionState.Closed)
				throw ExceptionHelper.ConnectionClosed ();
			return MetaDataCollections.Instance;
		}

		public override DataTable GetSchema (string collectionName)
		{
			return GetSchema (collectionName, null);
		}

		public override DataTable GetSchema (string collectionName, string [] restrictionValues)
		{
			if (State == ConnectionState.Closed)
				throw ExceptionHelper.ConnectionClosed ();
			return GetSchema (collectionName, null);
		}

		[MonoTODO]
		public override void EnlistTransaction (Transaction transaction)
		{
			throw new NotImplementedException ();
		}
#endif

		[MonoTODO]
		public void EnlistDistributedTransaction (ITransaction transaction)
		{
			throw new NotImplementedException ();
		}

		internal string GetInfo (OdbcInfo info)
		{
			if (State == ConnectionState.Closed)
				throw new InvalidOperationException ("The connection is closed.");

			OdbcReturn ret = OdbcReturn.Error;
			short max_length = 512;
			byte [] buffer = new byte [512];
			short actualLength = 0;

			ret = libodbc.SQLGetInfo (hdbc, info, buffer, max_length, ref actualLength);
			if (ret != OdbcReturn.Success && ret != OdbcReturn.SuccessWithInfo)
				throw CreateOdbcException (OdbcHandleType.Dbc, hdbc);
			return Encoding.Unicode.GetString (buffer, 0, actualLength);
		}

		string GetSafeInfo (OdbcInfo info)
		{
			if (State == ConnectionState.Closed)
				return null;

			OdbcReturn ret = OdbcReturn.Error;
			short max_length = 512;
			byte [] buffer = new byte [512];
			short actualLength = 0;

			ret = libodbc.SQLGetInfo (hdbc, info, buffer, max_length, ref actualLength);
			if (ret != OdbcReturn.Success && ret != OdbcReturn.SuccessWithInfo)
				return null;
			return Encoding.Unicode.GetString (buffer, 0, actualLength);
		}

		private void RaiseStateChange (ConnectionState from, ConnectionState to)
		{
#if ONLY_1_1
			if (StateChange != null)
				StateChange (this, new StateChangeEventArgs (from, to));
#else
			base.OnStateChange (new StateChangeEventArgs (from, to));
#endif
		}

		private OdbcInfoMessageEventArgs CreateOdbcInfoMessageEvent (OdbcErrorCollection errors)
		{
			return new OdbcInfoMessageEventArgs (errors);
		}

		private void OnOdbcInfoMessage (OdbcInfoMessageEventArgs e)
		{
			if (InfoMessage != null)
				InfoMessage (this, e);
		}

		internal OdbcException CreateOdbcException (OdbcHandleType HandleType, IntPtr Handle)
		{
			short buflen = 256;
			short txtlen = 0;
			int nativeerror = 0;
			OdbcReturn ret = OdbcReturn.Success;

			OdbcErrorCollection errors = new OdbcErrorCollection ();

			while (true) {
				byte [] buf_MsgText = new byte [buflen * 2];
				byte [] buf_SqlState = new byte [buflen * 2];

				switch (HandleType) {
				case OdbcHandleType.Dbc:
					ret = libodbc.SQLError (IntPtr.Zero, Handle, IntPtr.Zero, buf_SqlState,
						ref nativeerror, buf_MsgText, buflen, ref txtlen);
					break;
				case OdbcHandleType.Stmt:
					ret = libodbc.SQLError (IntPtr.Zero, IntPtr.Zero, Handle, buf_SqlState,
						ref nativeerror, buf_MsgText, buflen, ref txtlen);
					break;
				case OdbcHandleType.Env:
					ret = libodbc.SQLError (Handle, IntPtr.Zero, IntPtr.Zero, buf_SqlState,
						ref nativeerror, buf_MsgText, buflen, ref txtlen);
					break;
				}

				if (ret != OdbcReturn.Success)
					break;

				string state = RemoveTrailingNullChar (Encoding.Unicode.GetString (buf_SqlState));
				string message = Encoding.Unicode.GetString (buf_MsgText, 0, txtlen * 2);

				errors.Add (new OdbcError (message, state, nativeerror));
			}

			string source = SafeDriver;
			foreach (OdbcError error in errors)
				error.SetSource (source);
			return new OdbcException (errors);
		}

		static string RemoveTrailingNullChar (string value)
		{
			return value.TrimEnd ('\0');
		}

		internal object Link (OdbcCommand cmd)
		{
			lock(this) {
				if (linkedCommands == null)
					linkedCommands = new ArrayList ();
				linkedCommands.Add (new WeakReference (cmd));
				return linkedCommands;
			}
		}

		internal object Unlink (OdbcCommand cmd)
		{
			lock(this) {
				if (linkedCommands == null)
					return null;

				for (int i = 0; i < linkedCommands.Count; i++) {
					WeakReference wr = (WeakReference) linkedCommands [i];
					if (wr == null)
						continue;
					OdbcCommand c = (OdbcCommand) wr.Target;
					if (c == cmd) {
						linkedCommands [i] = null;
						break;
					}
				}
				return linkedCommands;
			}
		}

		#endregion

		#region Events and Delegates

#if ONLY_1_1
		[OdbcDescription ("DbConnection_StateChange")]
		[OdbcCategory ("DataCategory_StateChange")]
		public event StateChangeEventHandler StateChange;
#endif // ONLY_1_1

		[OdbcDescription ("DbConnection_InfoMessage")]
		[OdbcCategory ("DataCategory_InfoMessage")]
		public event OdbcInfoMessageEventHandler InfoMessage;

		private void MessageHandler (OdbcException e)
		{
			OnOdbcInfoMessage (CreateOdbcInfoMessageEvent (e.Errors));
		}

		#endregion
	}
}
