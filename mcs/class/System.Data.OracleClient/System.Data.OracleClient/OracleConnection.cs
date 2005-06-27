//
// OracleConnection.cs 
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient
//
// Authors: 
//    Daniel Morgan <danielmorgan@verizon.net>
//    Tim Coleman <tim@timcoleman.com>
//    Hubert FONGARNAND <informatique.internet@fiducial.fr>
//
// Copyright (C) Daniel Morgan, 2002, 2005
// Copyright (C) Tim Coleman, 2003
// Copyright (C) Hubert FONGARNAND, 2005
//
// Original source code for setting ConnectionString 
// by Tim Coleman <tim@timcoleman.com>
//
// Copyright (C) Tim Coleman, 2002
//
// Licensed under the MIT/X11 License.
//

using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.OracleClient.Oci;
using System.Drawing.Design;
using System.EnterpriseServices;
using System.Text;

namespace System.Data.OracleClient 
{
	internal struct OracleConnectionInfo 
	{
		internal string Username;
		internal string Password;
		internal string Database;
		internal string ConnectionString;
	}

	[DefaultEvent ("InfoMessage")]
	public sealed class OracleConnection : Component, ICloneable, IDbConnection
	{
		#region Fields

		OciGlue oci;
		ConnectionState state;
		OracleConnectionInfo conInfo;
		OracleTransaction transaction = null;
		string connectionString = "";
		OracleDataReader dataReader = null;
		bool pooling = true;
		static OracleConnectionPoolManager pools = new OracleConnectionPoolManager ();
		OracleConnectionPool pool;
		int minPoolSize = 0;
		int maxPoolSize = 100;

		#endregion // Fields

		#region Constructors

		public OracleConnection () 
		{
			state = ConnectionState.Closed;
		}

		public OracleConnection (string connectionString) 
			: this() 
		{
			SetConnectionString (connectionString);
		}

		#endregion // Constructors

		#region Properties

		int IDbConnection.ConnectionTimeout {
			[MonoTODO]
			get { return -1; }
		}

		string IDbConnection.Database {
			[MonoTODO]
			get { return String.Empty; }
		}

		internal OracleDataReader DataReader {
			get { return dataReader; }
			set { dataReader = value; }
		}

		internal OciEnvironmentHandle Environment {
			get { return oci.Environment; }
		}

		internal OciErrorHandle ErrorHandle {
			get { return oci.ErrorHandle; }
		}

		internal OciServiceHandle ServiceContext {
			get { return oci.ServiceContext; }
		}

		internal OciSessionHandle Session {
			get { return oci.SessionHandle; }
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string DataSource {
			get {
				return conInfo.Database;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public ConnectionState State {
			get { return state; }
		}

		[DefaultValue ("")]
		[RecommendedAsConfigurable (true)]
		[RefreshProperties (RefreshProperties.All)]
		[Editor ("Microsoft.VSDesigner.Data.Oracle.Design.OracleConnectionStringEditor, " + Consts.AssemblyMicrosoft_VSDesigner, typeof(UITypeEditor))]
		public string ConnectionString {
			get { return connectionString; }
			set { SetConnectionString (value); }
		}

		[MonoTODO]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string ServerVersion {
			get {
				if (this.State != ConnectionState.Open)
					throw new System.InvalidOperationException ("Invalid operation. The connection is closed.");
				return GetOracleVersion ();
			}
		}

		internal string GetOracleVersion () 
		{
			byte[] buffer = new Byte[256];
			uint bufflen = (uint) buffer.Length;

			IntPtr sh = oci.ServiceContext;
			IntPtr eh = oci.ErrorHandle;

			OciCalls.OCIServerVersion (sh, eh, ref buffer,  bufflen, OciHandleType.Service);
			
			// Get length of returned string
			int 	rsize = 0;
			IntPtr	env = oci.Environment;
			OciCalls.OCICharSetToUnicode (env, null, buffer, out rsize);
			
			// Get string
			StringBuilder ret = new StringBuilder(rsize);
			OciCalls.OCICharSetToUnicode (env, ret, buffer, out rsize);

			return ret.ToString ();
		}

		internal OciGlue Oci {
			get { return oci; }
		}

		internal OracleTransaction Transaction {
			get { return transaction; }
			set { transaction = value; }
		}

		#endregion // Properties

		#region Methods

		public OracleTransaction BeginTransaction ()
		{
			return BeginTransaction (IsolationLevel.ReadCommitted);
		}

		public OracleTransaction BeginTransaction (IsolationLevel il)
		{
			if (state == ConnectionState.Closed)
				throw new InvalidOperationException ("The connection is not open.");
			if (transaction != null)
				throw new InvalidOperationException ("OracleConnection does not support parallel transactions.");

			OciTransactionHandle transactionHandle = oci.CreateTransaction ();
			if (transactionHandle == null) 
				throw new Exception("Error: Unable to start transaction");
			else {
				transactionHandle.Begin ();
				transaction = new OracleTransaction (this, il, transactionHandle);
			}

			return transaction;
		}

		[MonoTODO]
		void IDbConnection.ChangeDatabase (string databaseName)
		{
			throw new NotImplementedException ();
		}

		public OracleCommand CreateCommand ()
		{
			OracleCommand command = new OracleCommand ();
			command.Connection = this;
			return command;
		}

		[MonoTODO]
		object ICloneable.Clone ()
		{
			OracleConnection con = new OracleConnection ();
			con.ConnectionString = this.ConnectionString;
			if (this.State == ConnectionState.Open)
				con.Open ();
			// TODO: what other properties need to be cloned?
			return con;
		}

		IDbTransaction IDbConnection.BeginTransaction ()
		{
			return BeginTransaction ();
		}

		IDbTransaction IDbConnection.BeginTransaction (IsolationLevel iso)
		{
			return BeginTransaction (iso);
		}

		IDbCommand IDbConnection.CreateCommand ()
		{
			return CreateCommand ();
		}

		void IDisposable.Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}

		[MonoTODO]
		public void EnlistDistributedTransaction (ITransaction distributedTransaction)
		{
			throw new NotImplementedException ();
		}

		// Get NLS_DATE_FORMAT string from Oracle server
		internal string GetSessionDateFormat () 
		{
			// 23 is 22 plus 1 for NUL terminated character
			// a DATE format has a max size of 22
			return GetNlsInfo (Session, 23, OciNlsServiceType.DATEFORMAT);
		}

		// Get NLS Info
		//
		// handle = OciEnvironmentHandle or OciSessionHandle
		// bufflen = Length of byte buffer to allocate to retrieve the NLS info
		// item = OciNlsServiceType enum value
		//
		// if unsure how much you need, use OciNlsServiceType.MAXBUFSZ
		internal string GetNlsInfo (OciHandle handle, uint bufflen, OciNlsServiceType item) 
		{
			byte[] buffer = new Byte[bufflen];

			int st = OciCalls.OCINlsGetInfo (handle, ErrorHandle, 
				ref buffer, bufflen, (ushort) item);

			// Get length of returned string
			int rsize = 0;
			OciCalls.OCICharSetToUnicode (Environment, null, buffer, out rsize);
			
			// Get string
			StringBuilder ret = new StringBuilder (rsize);
			OciCalls.OCICharSetToUnicode (Environment, ret, buffer, out rsize);

			return ret.ToString ();
		}

		public void Open () 
		{
			if (!pooling) {	
				oci = new OciGlue ();
				oci.CreateConnection (conInfo);
			}
			else {
				pool = pools.GetConnectionPool (conInfo, minPoolSize, maxPoolSize);
				oci = pool.GetConnection ();
			}
			state = ConnectionState.Open;
			CreateStateChange (ConnectionState.Closed, ConnectionState.Open);
		}

		internal void CreateInfoMessage (OciErrorInfo info) 
		{
			OracleInfoMessageEventArgs a = new OracleInfoMessageEventArgs (info);
			OnInfoMessage (a);
		}

		private void OnInfoMessage (OracleInfoMessageEventArgs e) 
		{
			if (InfoMessage != null)
				InfoMessage (this, e);
		}

		internal void CreateStateChange (ConnectionState original, ConnectionState current) 
		{
			StateChangeEventArgs a = new StateChangeEventArgs (original, current);
			OnStateChange (a);
		}

		private void OnStateChange (StateChangeEventArgs e) 
		{
			if (StateChange != null)
				StateChange (this, e);
		}

		public void Close () 
		{
			if (transaction != null)
				transaction.Rollback ();

			if (!pooling)
				oci.Disconnect ();
			else if (pool != null)
				pool.ReleaseConnection (oci);

			state = ConnectionState.Closed;
			CreateStateChange (ConnectionState.Open, ConnectionState.Closed);
		}

		void SetConnectionString (string connectionString) 
		{
			this.connectionString = connectionString;
			conInfo.Username = "";
			conInfo.Database = "";
			conInfo.Password = "";

			if (connectionString == String.Empty)
				return;
			
			connectionString += ";";
			NameValueCollection parameters = new NameValueCollection ();

			bool inQuote = false;
			bool inDQuote = false;

			string name = String.Empty;
			string value = String.Empty;
			StringBuilder sb = new StringBuilder ();

			foreach (char c in connectionString) {
				switch (c) {
				case '\'':
					inQuote = !inQuote;
					break;
				case '"' :
					inDQuote = !inDQuote;
					break;
				case ';' :
					if (!inDQuote && !inQuote) {
						if (name != String.Empty && name != null) {
							value = sb.ToString ();
							parameters [name.ToUpper ().Trim ()] = value.Trim ();
						}
						name = String.Empty;
						value = String.Empty;
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
			}

			SetProperties (parameters);

			conInfo.ConnectionString = connectionString;
		}

		private void SetProperties (NameValueCollection parameters) 
		{	
			string value;
			foreach (string name in parameters) {
				value = parameters[name];

				switch (name) {
				case "UNICODE":
					break;
				case "ENLIST":
					break;
				case "CONNECTION LIFETIME":
					// TODO:
					break;
				case "INTEGRATED SECURITY":
					throw new NotImplementedException ();
				case "PERSIST SECURITY INFO":
					// TODO:
					break;
				case "MIN POOL SIZE":
					minPoolSize = int.Parse (value);
					break;
				case "MAX POOL SIZE":
					maxPoolSize = int.Parse (value);
					break;
				case "DATA SOURCE" :
				case "SERVER" :
					conInfo.Database = value;
					break;
				case "PASSWORD" :
				case "PWD" :
					conInfo.Password = value;
					break;
				case "UID" :
				case "USER ID" :
					conInfo.Username = value;
					break;
				case "POOLING" :
					switch (value.ToUpper ()) {
					case "YES":
					case "TRUE":
						pooling = true;
						break;
					case "NO":
					case "FALSE":
						pooling = false;
						break;
					default:
						throw new ArgumentException("Connection parameter not supported: '" + name + "'");
					}
					break;
				default:
					throw new ArgumentException("Connection parameter not supported: '" + name + "'");
				}
			}
		}

		#endregion // Methods

		public event OracleInfoMessageEventHandler InfoMessage;
		public event StateChangeEventHandler StateChange;
	}
}
