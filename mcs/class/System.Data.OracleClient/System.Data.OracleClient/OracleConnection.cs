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
//    Daniel Morgan <danmorg@sc.rr.com>
//    Tim Coleman <tim@timcoleman.com>
//
// Copyright (C) Daniel Morgan, 2002
// Copyright (C) Tim Coleman, 2003
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
		public string Username;
		public string Password;
		public string Database;
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

		#endregion // Fields

		#region Constructors

		public OracleConnection () 
		{
			state = ConnectionState.Closed;
			oci = new OciGlue ();
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

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string DataSource {
			get {
				throw new NotImplementedException ();
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
				throw new NotImplementedException ();
			}
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
			throw new NotImplementedException ();
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

		public void Open () 
		{
			oci.CreateConnection (conInfo);
			state = ConnectionState.Open;
		}

		public void Close () 
		{
			if (transaction != null)
				transaction.Rollback ();

			oci.Disconnect ();
			state = ConnectionState.Closed;
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
		}

		private void SetProperties (NameValueCollection parameters) 
		{	
			string value;
			foreach (string name in parameters) {
				value = parameters[name];

				switch (name) {
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
