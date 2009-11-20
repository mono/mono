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
//    Daniel Morgan <monodanmorg@yahoo.com>
//    Tim Coleman <tim@timcoleman.com>
//    Hubert FONGARNAND <informatique.internet@fiducial.fr>
//    Marek Safar <marek.safar@gmail.com>
//
// Copyright (C) Daniel Morgan, 2002, 2005, 2006, 2009
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

//#define ORACLE_DATA_ACCESS
// define ORACLE_DATA_ACCESS for Oracle.DataAccess functionality
// otherwise it defaults to Microsoft's System.Data.OracleClient

using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.OracleClient.Oci;
using System.Drawing.Design;
using System.EnterpriseServices;
using System.Globalization;
using System.Text;

//#if ORACLE_DATA_ACCESS
//namespace Oracle.DataAccess
//#else
namespace System.Data.OracleClient
//#endif
{
	internal struct OracleConnectionInfo
	{
		internal string Username;
		internal string Password;
		internal string Database;
		internal string ConnectionString;
		internal OciCredentialType CredentialType;
		internal bool SetNewPassword;
		internal string NewPassword;
	}

	[DefaultEvent ("InfoMessage")]
	public sealed class OracleConnection :
#if NET_2_0
		Common.DbConnection, ICloneable
#else
		Component, ICloneable, IDbConnection
#endif
	{
		#region Fields

		OciGlue oci;
		ConnectionState state;
		OracleConnectionInfo conInfo;
		OracleTransaction transaction;
		string connectionString = String.Empty;
		string parsedConnectionString;
		OracleDataReader dataReader;
		bool pooling = true;
		static OracleConnectionPoolManager pools = new OracleConnectionPoolManager ();
		OracleConnectionPool pool;
		int minPoolSize;
		int maxPoolSize = 100;
		byte persistSecurityInfo = 1;
		bool disposed;
		IFormatProvider format_info;

		#endregion // Fields

		#region Constructors

		public OracleConnection ()
		{
			state = ConnectionState.Closed;
		}

		public OracleConnection (string connectionString)
			: this()
		{
			SetConnectionString (connectionString, false);
		}

		#endregion // Constructors

		#region Properties

#if NET_2_0
		[MonoTODO ("Currently not respected.")]
		public override int ConnectionTimeout {
			get { return 0; }
		}
#else
		[MonoTODO ("Currently not respected.")]
		int IDbConnection.ConnectionTimeout {
			get { return 0; }
		}
#endif

#if NET_2_0
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override string Database {
#else
		string IDbConnection.Database {
#endif
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

#if NET_2_0
		[Browsable (false)]
#endif
		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public
#if NET_2_0
		override
#endif
		string DataSource {
			get {
				return conInfo.Database;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public
#if NET_2_0
		override
#endif		
		ConnectionState State {
			get { return state; }
		}

		[DefaultValue ("")]
#if NET_2_0
		[SettingsBindableAttribute (true)]
#else
		[RecommendedAsConfigurable (true)]
#endif
		[RefreshProperties (RefreshProperties.All)]
		[Editor ("Microsoft.VSDesigner.Data.Oracle.Design.OracleConnectionStringEditor, " + Consts.AssemblyMicrosoft_VSDesigner, typeof(UITypeEditor))]
		public
#if NET_2_0
		override
#endif
		string ConnectionString {
			get {
				if (parsedConnectionString == null)
					return string.Empty;
				return parsedConnectionString;
			}
			set {
				SetConnectionString (value, false);
			}
		}

		[MonoTODO]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public
#if NET_2_0
		override
#endif		
		string ServerVersion {
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

		public
#if NET_2_0
		new
#endif
		OracleTransaction BeginTransaction ()
		{
			return BeginTransaction (IsolationLevel.ReadCommitted);
		}

		public
#if NET_2_0
		new
#endif
		OracleTransaction BeginTransaction (IsolationLevel il)
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
#if NET_2_0
		public override void ChangeDatabase (string value)
#else
		void IDbConnection.ChangeDatabase (string value)
#endif
		{
			throw new NotImplementedException ();
		}

		public
#if NET_2_0
		new
#endif
		OracleCommand CreateCommand ()
		{
			OracleCommand command = new OracleCommand ();
			command.Connection = this;
			return command;
		}

		[MonoTODO]
		object ICloneable.Clone ()
		{
			OracleConnection con = new OracleConnection ();
			con.SetConnectionString (connectionString, true);
			// TODO: what other properties need to be cloned?
			return con;
		}

#if !NET_2_0
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
#endif

		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
			if (!disposed) {
				if (State == ConnectionState.Open)
					Close ();
				dataReader = null;
				transaction = null;
				oci = null;
				pool = null;
				conInfo.Username = string.Empty;
				conInfo.Database = string.Empty;
				conInfo.Password = string.Empty;
				connectionString = null;
				parsedConnectionString = null;
				base.Dispose (disposing);
				disposed = true;
			}
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

			OciCalls.OCINlsGetInfo (handle, ErrorHandle, 
				ref buffer, bufflen, (ushort) item);

			// Get length of returned string
			int rsize = 0;
			OciCalls.OCICharSetToUnicode (Environment, null, buffer, out rsize);
			
			// Get string
			StringBuilder ret = new StringBuilder (rsize);
			OciCalls.OCICharSetToUnicode (Environment, ret, buffer, out rsize);

			return ret.ToString ();
		}

		// An instance of IFormatProvider for locale - independent IFormattable.ToString () in Bind ()
		[MonoTODO("Handle other culture-specific informations, restrict buffer sizes")]
		internal IFormatProvider SessionFormatProvider {
			get {
				if (format_info == null && state == ConnectionState.Open) {
					NumberFormatInfo numberFormatInfo = new NumberFormatInfo ();
					numberFormatInfo.NumberGroupSeparator
					= GetNlsInfo (Session, (uint)OciNlsServiceType.MAXBUFSZ, OciNlsServiceType.GROUP);
					numberFormatInfo.NumberDecimalSeparator
					= GetNlsInfo (Session, (uint)OciNlsServiceType.MAXBUFSZ, OciNlsServiceType.DECIMAL);
					numberFormatInfo.CurrencyGroupSeparator
					= GetNlsInfo (Session, (uint)OciNlsServiceType.MAXBUFSZ, OciNlsServiceType.MONGROUP);
					numberFormatInfo.CurrencyDecimalSeparator
					= GetNlsInfo (Session, (uint)OciNlsServiceType.MAXBUFSZ, OciNlsServiceType.MONDECIMAL);
					format_info = numberFormatInfo;
				}
				return format_info;
			}
		}

		public
#if NET_2_0
		override
#endif		
		void Open ()
		{
			if (State == ConnectionState.Open)
				return;

			PersistSecurityInfo ();

			if (!pooling || conInfo.SetNewPassword == true) {
				oci = new OciGlue ();
				oci.CreateConnection (conInfo);
			} else {
				pool = pools.GetConnectionPool (conInfo, minPoolSize, maxPoolSize);
				oci = pool.GetConnection ();
			}
			state = ConnectionState.Open;

			CreateStateChange (ConnectionState.Closed, ConnectionState.Open);
		}

#if ORACLE_DATA_ACCESS
		public void OpenWithNewPassword (string newPassword) 
		{
			if (State == ConnectionState.Open)
				throw new InvalidOperationException ();

			conInfo.SetNewPassword = true;
			conInfo.NewPassword = newPassword;

			Open ();

			conInfo.SetNewPassword = false;
			conInfo.NewPassword = string.Empty;
			conInfo.Password = newPassword;
		}
#endif

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

#if !NET_2_0
		private void OnStateChange (StateChangeEventArgs e) 
		{
			if (StateChange != null)
				StateChange (this, e);
		}
#endif

		public
#if NET_2_0
		override
#endif
		void Close ()
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

#if NET_2_0
		protected override Common.DbTransaction BeginDbTransaction (IsolationLevel isolationLevel)
		{
			return BeginTransaction (isolationLevel);
		}
		
		protected override Common.DbCommand CreateDbCommand ()
		{
			return CreateCommand ();
		}
#endif

		private void PersistSecurityInfo ()
		{
			// persistSecurityInfo:
			// 0 = true/yes
			// 1 = false/no (have not parsed out password yet)
			// 2 = like 1, but have parsed out password

			if (persistSecurityInfo == 0 || persistSecurityInfo == 2)
				return;

			persistSecurityInfo = 2;

			if (connectionString == null || connectionString.Length == 0)
				return;

			string conString = connectionString + ";";

			bool inQuote = false;
			bool inDQuote = false;
			int inParen = 0;

			string name = String.Empty;
			StringBuilder sb = new StringBuilder ();
			int nStart = 0;
			int nFinish = 0;
			int i = -1;

			foreach (char c in conString) {
				i ++;

				switch (c) {
				case '\'':
					inQuote = !inQuote;
					break;
				case '"' :
					inDQuote = !inDQuote;
					break;
				case '(':
					inParen++;
					sb.Append (c);
					break;
				case ')':
					inParen--;
					sb.Append (c);
					break;
				case ';' :
					if (!inDQuote && !inQuote) {
						if (name != String.Empty && name != null) {
							name = name.ToUpper ().Trim ();
							if (name.Equals ("PASSWORD") || name.Equals ("PWD")) {
								nFinish = i;
								string part1 = String.Empty;
								string part3 = String.Empty;
								sb = new StringBuilder ();
								if (nStart > 0) {
									part1 = conString.Substring (0, nStart);
									if (part1[part1.Length - 1] == ';')
										part1 = part1.Substring (0, part1.Length - 1);
									sb.Append (part1);
								}
								if (!part1.Equals (String.Empty))
									sb.Append (';');
								if (conString.Length - nFinish - 1 > 0) {
									part3 = conString.Substring (nFinish, conString.Length - nFinish);
									if (part3[0] == ';')  
										part3 = part3.Substring(1, part3.Length - 1);
									sb.Append (part3);
								}
								parsedConnectionString = sb.ToString ();
								return;
							}
						}
						name = String.Empty;
						sb = new StringBuilder ();
						nStart = i;
						nFinish = i;
					}
					else
						sb.Append (c);
					break;
				case '=' :
					if (!inDQuote && !inQuote && inParen == 0) {
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
		}

		internal void SetConnectionString (string connectionString, bool persistSecurity)
		{
			persistSecurityInfo = 1;

			conInfo.Username = string.Empty;
			conInfo.Database = string.Empty;
			conInfo.Password = string.Empty;
			conInfo.CredentialType = OciCredentialType.RDBMS;
			conInfo.SetNewPassword = false;
			conInfo.NewPassword = string.Empty;

			if (connectionString == null || connectionString.Length == 0) {
				this.connectionString = connectionString;
				this.parsedConnectionString = connectionString;
				return;
			}

			this.connectionString = String.Copy (connectionString);
			this.parsedConnectionString = this.connectionString;

			connectionString += ";";
			NameValueCollection parameters = new NameValueCollection ();

			bool inQuote = false;
			bool inDQuote = false;
			int inParen = 0;

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
				case '(':
					inParen++;
					sb.Append (c);
					break;
				case ')':
					inParen--;
					sb.Append (c);
					break;
				case ';' :
					if (!inDQuote && !inQuote) {
						if (name != String.Empty && name != null) {
							name = name.ToUpper ().Trim ();
							value = sb.ToString ().Trim ();
							parameters [name] = value;
						}
						name = String.Empty;
						value = String.Empty;
						sb = new StringBuilder ();
					}
					else
						sb.Append (c);
					break;
				case '=' :
					if (!inDQuote && !inQuote && inParen == 0) {
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

			conInfo.ConnectionString = this.connectionString;

			if (persistSecurity)
				PersistSecurityInfo ();
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
					if (!ConvertToBoolean ("integrated security", value))
						conInfo.CredentialType = OciCredentialType.RDBMS;
					else
						conInfo.CredentialType = OciCredentialType.External;
					break;
				case "PERSIST SECURITY INFO":
					if (!ConvertToBoolean ("persist security info", value))
						persistSecurityInfo = 1;
					else
						persistSecurityInfo = 0;
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
					pooling = ConvertToBoolean("pooling", value);
					break;
				default:
					throw new ArgumentException("Connection parameter not supported: '" + name + "'");
				}
			}
		}

		private bool ConvertToBoolean(string key, string value)
		{
			string upperValue = value.ToUpper();

			if (upperValue == "TRUE" || upperValue == "YES") {
				return true;
			} else if (upperValue == "FALSE" || upperValue == "NO") {
				return false;
			}

			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
				"Invalid value \"{0}\" for key '{1}'.", value, key));
		}

		#endregion // Methods

		public event OracleInfoMessageEventHandler InfoMessage;
#if !NET_2_0
		public event StateChangeEventHandler StateChange;
#endif

#if NET_2_0
		public override DataTable GetSchema ()
		{
			if (State != ConnectionState.Open)
				throw new InvalidOperationException ("Invalid operation.  The connection is closed.");

			return GetSchemaMetaDataCollections ();
		}

		public override DataTable GetSchema (String collectionName)
		{
			return GetSchema (collectionName, null);
		}

		public override DataTable GetSchema (String collectionName, string [] restrictionValues)
		{
			if (State != ConnectionState.Open)
				throw new InvalidOperationException ("Invalid operation.  The connection is closed.");

			int restrictionsCount = 0;
			if (restrictionValues != null)
				restrictionsCount = restrictionValues.Length;

			DataTable metaTable = GetSchemaMetaDataCollections ();
			foreach (DataRow row in metaTable.Rows) {
				if (String.Compare (row ["CollectionName"].ToString (), collectionName, true) == 0) {
				    int restrictions = (int)row ["NumberOfRestrictions"];
				    if (restrictionsCount > restrictions)
					throw new ArgumentException ("More restrictions were provided than needed.");
				}
			}

			switch (collectionName.ToUpper ()) {
			case "METADATACOLLECTIONS":
				return metaTable;
			case "DATASOURCEINFORMATION":
				return GetSchemaDataSourceInformation ();
			case "DATATYPES":
				return GetSchemaDataTypes ();
			case "RESTRICTIONS":
				return GetSchemaRestrictions ();
			case "RESERVEDWORDS":
				return GetSchemaReservedWords ();
			case "USERS":
				return GetSchemaUsers (restrictionValues);
			case "TABLES":
				return GetSchemaTables (restrictionValues);
			case "COLUMNS":
				return GetSchemaColumns (restrictionValues);
			case "VIEWS":
				return GetSchemaViews (restrictionValues);
			case "SYNONYMS":
				return GetSchemaSynonyms (restrictionValues);
			case "SEQUENCES":
				return GetSchemaSequences (restrictionValues);
			case "FUNCTIONS":
				return GetSchemaProcedures (restrictionValues, "FUNCTION");
			case "PACKAGES":
				return GetSchemaProcedures (restrictionValues, "PACKAGE");
			case "PACKAGEBODIES":
				return GetSchemaProcedures (restrictionValues, "PACKAGE BODY");
			case "PROCEDURES":
				return GetSchemaProcedures (restrictionValues, "PROCEDURE");
			case "PROCEDUREPARAMETERS":
				throw new NotImplementedException (collectionName); // see ALL_ARGUMENTS
			case "ARGUMENTS":
				throw new NotImplementedException (collectionName); // see ALL_ARGUMENTS
			case "INDEXCOLUMNS":
				throw new NotImplementedException (collectionName); // see ALL_IND_COLS
			case "INDEXES":
				throw new NotImplementedException (collectionName); // see ALL_INDEXES
			case "UNIQUEKEYS":
				throw new NotImplementedException (collectionName); // see ALL_CONSTRAINTS and CONSTRAINT_TYPE of U
			case "PRIMARYKEYS":
				throw new NotImplementedException (collectionName); // see ALL_CONSTRAINTS and CONSTRAINT_TYPE of P
			case "FOREIGNKEYS":
				throw new NotImplementedException (collectionName); // see ALL_CONSTRAINTS and CONSTRAINT_TYPE of R
			case "FOREIGNKEYCOLUMNS":
				throw new NotImplementedException (collectionName); // see ALL_CONS_COLUMNS
			}

			throw new ArgumentException ("The requested collection is not defined.");
		}

		static DataTable metaDataCollections = null;
		DataTable GetSchemaMetaDataCollections ()
		{
			if (metaDataCollections != null)
				return metaDataCollections;

			DataTable dt = new DataTable ();

			dt.Columns.Add ("CollectionName", typeof(System.String));
			dt.Columns.Add ("NumberOfRestrictions", typeof(System.Int32));
			dt.Columns.Add ("NumberOfIdentifierParts", typeof(System.Int32));

			dt.LoadDataRow (new object [] { "MetaDataCollections", 0, 0 }, true);
			dt.LoadDataRow (new object [] { "DataSourceInformation", 0, 0 }, true);
			dt.LoadDataRow (new object [] { "DataTypes", 0, 0 }, true);
			dt.LoadDataRow (new object [] { "Restrictions", 0, 0 }, true);
			dt.LoadDataRow (new object [] { "ReservedWords", 0, 0 }, true);
			dt.LoadDataRow (new object [] { "Users", 1, 1 }, true);
			dt.LoadDataRow (new object [] { "Tables", 2, 2 }, true);
			dt.LoadDataRow (new object [] { "Columns", 3, 3 }, true);
			dt.LoadDataRow (new object [] { "Views", 2, 2 }, true);
			dt.LoadDataRow (new object [] { "Synonyms", 2, 2 }, true);
			dt.LoadDataRow (new object [] { "Sequences", 2, 2 }, true);
			dt.LoadDataRow (new object [] { "ProcedureParameters", 2, 2 }, true);
			dt.LoadDataRow (new object [] { "Functions", 2, 2 }, true);
			dt.LoadDataRow (new object [] { "IndexColumns", 5, 3 }, true);
			dt.LoadDataRow (new object [] { "Indexes", 4, 2 }, true);
			dt.LoadDataRow (new object [] { "Packages", 2, 2 }, true);
			dt.LoadDataRow (new object [] { "PackageBodies", 2, 2 }, true);
			dt.LoadDataRow (new object [] { "Arguments", 4, 4 }, true);
			dt.LoadDataRow (new object [] { "Procedures", 2, 2 }, true);
			dt.LoadDataRow (new object [] { "UniqueKeys", 3, 3 }, true);
			dt.LoadDataRow (new object [] { "PrimaryKeys", 3, 3 }, true);
			dt.LoadDataRow (new object [] { "ForeignKeys", 3, 3 }, true);
			dt.LoadDataRow (new object [] { "ForeignKeyColumns", 3, 2 }, true);

			return dt;
		}

		DataTable GetSchemaRestrictions ()
		{
			DataTable dt = new DataTable ();

			dt.Columns.Add ("CollectionName", typeof (System.String));
			dt.Columns.Add ("RestrictionName", typeof (System.String));
			dt.Columns.Add ("ParameterName", typeof (System.String));
			dt.Columns.Add ("RestrictionDefault", typeof (System.String));
			dt.Columns.Add ("RestrictionNumber", typeof (System.Int32));

			dt.LoadDataRow (new object [] { "Users", "UserName", "NAME", "USERNAME", 1 }, true);
			dt.LoadDataRow (new object [] { "Tables", "Owner", "OWNER", "OWNER", 1 }, true);
			dt.LoadDataRow (new object [] { "Tables", "Table", "TABLENAME", "TABLE_NAME", 2 }, true);
			dt.LoadDataRow (new object [] { "Columns", "Owner", "OWNER", "OWNER", 1 }, true);
			dt.LoadDataRow (new object [] { "Columns", "Table", "TABLENAME", "TABLE_NAME", 2 }, true);
			dt.LoadDataRow (new object [] { "Columns", "Column", "COLUMNNAME", "COLUMN_NAME", 3 }, true);
			dt.LoadDataRow (new object [] { "Views", "Owner", "OWNER", "OWNER", 1 }, true);
			dt.LoadDataRow (new object [] { "Views", "View", "VIEWNAME", "VIEW_NAME", 2 }, true);
			dt.LoadDataRow (new object [] { "Synonyms", "Owner", "OWNER", "OWNER", 1 }, true);
			dt.LoadDataRow (new object [] { "Synonyms", "Synonym", "SYNONYMNAME", "SYNONYM_NAME", 2 }, true);
			dt.LoadDataRow (new object [] { "Sequences", "Owner", "OWNER", "SEQUENCE_OWNER", 1 }, true);
			dt.LoadDataRow (new object [] { "Sequences", "Sequence", "SEQUENCE", "SEQUENCE_NAME", 2 }, true);
			dt.LoadDataRow (new object [] { "ProcedureParameters", "Owner", "OWNER", "OWNER", 1 }, true);
			dt.LoadDataRow (new object [] { "ProcedureParameters", "ObjectName", "OBJECTNAME", "OBJECT_NAME", 2 }, true);
			dt.LoadDataRow (new object [] { "Functions", "Owner", "OWNER", "OWNER", 1 }, true);
			dt.LoadDataRow (new object [] { "Functions", "Name", "NAME", "OBJECT_NAME", 2 }, true);
			dt.LoadDataRow (new object [] { "IndexColumns", "Owner", "OWNER", "INDEX_OWNER", 1 }, true);
			dt.LoadDataRow (new object [] { "IndexColumns", "Name", "NAME", "INDEX_NAME", 2 }, true);
			dt.LoadDataRow (new object [] { "IndexColumns", "TableOwner", "TABLEOWNER", "TABLE_OWNER", 3 }, true);
			dt.LoadDataRow (new object [] { "IndexColumns", "TableName", "TABLENAME", "TABLE_NAME", 4 }, true);
			dt.LoadDataRow (new object [] { "IndexColumns", "Column", "COLUMNNAME", "COLUMN_NAME", 5 }, true);
			dt.LoadDataRow (new object [] { "Indexes", "Owner", "OWNER", "OWNER", 1 }, true);
			dt.LoadDataRow (new object [] { "Indexes", "Name", "NAME", "INDEX_NAME", 2 }, true);
			dt.LoadDataRow (new object [] { "Indexes", "TableOwner", "TABLEOWNER", "TABLE_OWNER", 3 }, true);
			dt.LoadDataRow (new object [] { "Indexes", "TableName", "TABLENAME", "TABLE_NAME", 4 }, true);
			dt.LoadDataRow (new object [] { "Packages", "Owner", "OWNER", "OWNER", 1 }, true);
			dt.LoadDataRow (new object [] { "Packages", "Name", "PACKAGENAME", "OBJECT_NAME", 2 }, true);
			dt.LoadDataRow (new object [] { "PackageBodies", "Owner", "OWNER", "OWNER", 1 }, true);
			dt.LoadDataRow (new object [] { "PackageBodies", "Name", "NAME", "OBJECT_NAME", 2 }, true);
			dt.LoadDataRow (new object [] { "Arguments", "Owner", "OWNER", "OWNER", 1 }, true);
			dt.LoadDataRow (new object [] { "Arguments", "PackageName", "PACKAGENAME", "PACKAGE_NAME", 2 }, true);
			dt.LoadDataRow (new object [] { "Arguments", "ObjectName", "OBJECTNAME", "OBJECT_NAME", 3 }, true);
			dt.LoadDataRow (new object [] { "Arguments", "ArgumentName", "ARGUMENTNAME", "ARGUMENT_NAME", 4 }, true);
			dt.LoadDataRow (new object [] { "Procedures", "Owner", "OWNER", "OWNER", 1 }, true);
			dt.LoadDataRow (new object [] { "Procedures", "Name", "NAME", "OBJECT_NAME", 2 }, true);
			dt.LoadDataRow (new object [] { "UniqueKeys", "Owner", "OWNER", "OWNER", 1 }, true);
			dt.LoadDataRow (new object [] { "UniqueKeys", "Table_Name", "TABLENAME", "TABLE_NAME", 2 }, true);
			dt.LoadDataRow (new object [] { "UniqueKeys", "Constraint_Name", "CONSTRAINTNAME", "CONSTRAINT_NAME", 3 }, true);
			dt.LoadDataRow (new object [] { "PrimaryKeys", "Owner", "OWNER", "OWNER", 1 }, true);
			dt.LoadDataRow (new object [] { "PrimaryKeys", "Table_Name", "TABLENAME", "TABLE_NAME", 2 }, true);
			dt.LoadDataRow (new object [] { "PrimaryKeys", "Constraint_Name", "CONSTRAINTNAME", "CONSTRAINT_NAME", 3 }, true);
			dt.LoadDataRow (new object [] { "ForeignKeys", "Foreign_Key_Owner", "OWNER", "FKCON.OWNER", 1 }, true);
			dt.LoadDataRow (new object [] { "ForeignKeys", "Foreign_Key_Table_Name", "TABLENAME", "FKCON.TABLE_NAME", 2 }, true);
			dt.LoadDataRow (new object [] { "ForeignKeys", "Foreign_Key_Constraint_Name", "CONSTRAINTNAME", "FKCON.CONSTRAINT_NAME", 3 }, true);
			dt.LoadDataRow (new object [] { "ForeignKeyColumns", "Owner", "OWNER", "FKCOLS.OWNER", 1 }, true);
			dt.LoadDataRow (new object [] { "ForeignKeyColumns", "Table_Name", "TABLENAME", "FKCOLS.TABLE_NAME", 2 }, true);
			dt.LoadDataRow (new object [] { "ForeignKeyColumns", "Constraint_Name", "CONSTRAINTNAME", "FKCOLS.CONSTRAINT_NAME", 3 }, true);

			return dt;
		}

		DataTable GetSchemaTables (string [] restrictionValues)
		{
			OracleCommand cmd = CreateCommand ();

			// TODO: determine whether a table is a System or a User type
			cmd.CommandText = "SELECT OWNER, TABLE_NAME, DECODE(OWNER, 'SYS', 'System', 'User') AS TYPE " +
				" FROM SYS.ALL_TABLES " +
				" WHERE (OWNER = :POWNER OR :POWNER IS NULL) " +
				" AND (TABLE_NAME = :PTABLE_NAME OR :PTABLE_NAME IS NULL) " +
				" ORDER BY OWNER, TABLE_NAME";

			cmd.Parameters.Add (":POWNER", OracleType.VarChar, 30).Value = DBNull.Value;
			cmd.Parameters.Add (":PTABLE_NAME", OracleType.VarChar, 30).Value = DBNull.Value;

			return GetSchemaDataTable (cmd, restrictionValues);
		}

		DataTable GetSchemaColumns (string [] restrictionValues)
		{
			OracleCommand cmd = CreateCommand();

			cmd.CommandText = "SELECT OWNER, TABLE_NAME, COLUMN_NAME, COLUMN_ID AS ID, DATA_TYPE AS DATATYPE, " +
				" DATA_LENGTH AS LENGTH, DATA_PRECISION AS PRECISION, DATA_SCALE AS SCALE, NULLABLE " +
				" FROM SYS.ALL_TAB_COLUMNS " +
				" WHERE (OWNER = :POWNER OR :POWNER IS NULL) " +
				" AND (TABLE_NAME = :PTABLE_NAME OR :PTABLE_NAME IS NULL) " +
				" AND (COLUMN_NAME = 'ENAME' OR :PCOLUMN_NAME IS NULL) " +
				" ORDER BY OWNER, TABLE_NAME, COLUMN_ID;"; 

			cmd.Parameters.Add (":POWNER", OracleType.VarChar, 30).Value = DBNull.Value;
			cmd.Parameters.Add (":PTABLE_NAME", OracleType.VarChar, 30).Value = DBNull.Value;
			cmd.Parameters.Add (":PCOLUMN_NAME", OracleType.VarChar, 30).Value = DBNull.Value;

			return GetSchemaDataTable (cmd, restrictionValues);
		}

		DataTable GetSchemaViews (string [] restrictionValues)
		{
			OracleCommand cmd = CreateCommand ();

			cmd.CommandText = "SELECT OWNER,VIEW_NAME,TEXT_LENGTH,TEXT,TYPE_TEXT_LENGTH,TYPE_TEXT,OID_TEXT_LENGTH,OID_TEXT, " +
				"   VIEW_TYPE_OWNER,VIEW_TYPE,SUPERVIEW_NAME " +
				" FROM SYS.ALL_VIEWS " +
				" WHERE (OWNER = :POWNER OR :POWNER IS NULL) " +
				" AND (VIEW_NAME = :PVIEW_NAME OR :PVIEW_NAME IS NULL) " +
				" ORDER BY OWNER, VIEW_NAME";

			cmd.Parameters.Add (":POWNER", OracleType.VarChar, 30).Value = DBNull.Value;
			cmd.Parameters.Add (":PVIEW_NAME", OracleType.VarChar, 30).Value = DBNull.Value;

			return GetSchemaDataTable (cmd, restrictionValues);
		}

		DataTable GetSchemaUsers (string [] restrictionValues)
		{
			OracleCommand cmd = CreateCommand ();

			cmd.CommandText = "SELECT USERNAME AS NAME, USER_ID AS ID, CREATED AS CREATEDATE " +
				" FROM SYS.ALL_USERS " +
				" WHERE (USERNAME = :PUSERNAME OR :PUSERNAME IS NULL)";

			cmd.Parameters.Add (":PUSERNAME", OracleType.VarChar, 30).Value = DBNull.Value;

			return GetSchemaDataTable (cmd, restrictionValues);
		}

		DataTable GetSchemaSynonyms (string [] restrictionValues)
		{
			OracleCommand cmd = CreateCommand ();

			cmd.CommandText = "SELECT OWNER, SYNONYM_NAME, TABLE_OWNER, TABLE_NAME, DB_LINK " + 
				" FROM SYS.ALL_SYNONYMS " +
				" WHERE (OWNER = :POWNER OR :POWNER IS NULL) " +
				" AND (SYNONYM_NAME = :PSYNONYM_NAME OR :PSYNONYM_NAME IS NULL) " +
				" ORDER BY OWNER, SYNONYM_NAME";

			cmd.Parameters.Add (":POWNER", OracleType.VarChar, 30).Value = DBNull.Value;
			cmd.Parameters.Add (":PSYNONYM_NAME", OracleType.VarChar, 30).Value = DBNull.Value;

			return GetSchemaDataTable (cmd, restrictionValues);
		}

		DataTable GetSchemaSequences (string [] restrictionValues)
		{
			OracleCommand cmd = CreateCommand ();

			cmd.CommandText = "SELECT SEQUENCE_OWNER, SEQUENCE_NAME, MIN_VALUE, MAX_VALUE, " +
				"   INCREMENT_BY, CYCLE_FLAG, ORDER_FLAG, CACHE_SIZE, LAST_NUMBER " +
				" FROM SYS.ALL_SEQUENCES " +
				" WHERE (SEQUENCE_OWNER = :PSEQUENCE_OWNER OR :PSEQUENCE_OWNER IS NULL) " +
				" AND (SEQUENCE_NAME = :PSEQUENCE_NAME OR :PSEQUENCE_NAME IS NULL) " +
				" ORDER BY SEQUENCE_OWNER, SEQUENCE_NAME";

			cmd.Parameters.Add (":SEQUENCE_OWNER", OracleType.VarChar, 30).Value = DBNull.Value;
			cmd.Parameters.Add (":SEQUENCE_NAME", OracleType.VarChar, 30).Value = DBNull.Value;

			return GetSchemaDataTable (cmd, restrictionValues);
		}

		DataTable GetSchemaProcedures(string [] restrictionValues, string objType)
		{
			OracleCommand cmd = CreateCommand ();

			cmd.CommandText = "SSELECT OWNER, OBJECT_NAME, SUBOBJECT_NAME, OBJECT_ID, DATA_OBJECT_ID, LAST_DDL_TIME, " +
				"    TIMESTAMP, STATUS, TEMPORARY, GENERATED, SECONDARY, CREATED " +
				" FROM ALL_OBJECTS " +
				" WHERE OBJECT_TYPE = '" + objType + "' " +
				" AND (OWNER = :POWNER OR :POWNER IS NULL) " +
				" AND (OBJECT_NAME = :POBJECT_NAME OR :POBJECT_NAME IS NULL) " +
				" ORDER BY OWNER, OBJECT_NAME, SUBOBJECT_NAME";

			cmd.Parameters.Add (":POWNER", OracleType.VarChar, 30).Value = DBNull.Value;
			cmd.Parameters.Add (":POBJECT_NAME", OracleType.VarChar, 30).Value = DBNull.Value;

			return GetSchemaDataTable (cmd, restrictionValues);
		}

		DataTable GetSchemaDataSourceInformation ()
		{
			DataTable dt = new DataTable ();

			dt.Columns.Add ("CompositeIdentifierSeparatorPattern", typeof (System.String));
			dt.Columns.Add ("DataSourceProductName", typeof (System.String));
			dt.Columns.Add ("DataSourceProductVersion", typeof (System.String));
			dt.Columns.Add ("DataSourceProductVersionNormalized", typeof (System.String));
			dt.Columns.Add ("GroupByBehavior", typeof (System.Data.Common.GroupByBehavior));
			dt.Columns.Add ("IdentifierPattern", typeof (System.String));
			dt.Columns.Add ("IdentifierCase", typeof (System.Data.Common.IdentifierCase));
			dt.Columns.Add ("OrderByColumnsInSelect", typeof (System.Boolean));
			dt.Columns.Add ("ParameterMarkerFormat", typeof (System.String));
			dt.Columns.Add ("ParameterMarkerPattern", typeof (System.String));
			dt.Columns.Add ("ParameterNameMaxLength", typeof (System.Int32));
			dt.Columns.Add ("ParameterNamePattern", typeof (System.String));
			dt.Columns.Add ("QuotedIdentifierPattern", typeof (System.String));
			dt.Columns.Add ("QuotedIdentifierCase", typeof (System.Data.Common.IdentifierCase));
			dt.Columns.Add ("StatementSeparatorPattern", typeof (System.String));
			dt.Columns.Add ("StringLiteralPattern", typeof (System.String));
			dt.Columns.Add ("SupportedJoinOperators", typeof (System.Data.Common.SupportedJoinOperators));

			string ver = ServerVersion;
			string [] ver2 = ver.Substring (0, ver.IndexOf (' ')).Split (new char [] { '.' });
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < ver2.Length; i++) {
				if (i > 0) {
				    sb.Append (".");
				}

				sb.AppendFormat ("{0:00}", Int32.Parse (ver2 [i]));
			}
			sb.Append (' ');
			ver = sb.ToString ();

			dt.LoadDataRow (new object [] { "@|\\.", 
				"Oracle", 
				ServerVersion, 
				ver, 
				3, 
				"^[\\p{Lo}\\p{Lu}\\p{Ll}\\p{Lm}＿_#$][\\p{Lo}\\p{Lu}\\p{Ll}\\p{Lm}\\p{Nd}＿_#$]*$", 
				1, 
				false, 
				":{0}", 
				":([\\p{Lo}\\p{Lu}\\p{Ll}\\p{Lm}＿_#$][\\p{Lo}\\p{Lu}\\p{Ll}\\p{Lm}\\p{Nd}＿_#$]*)", 
				30,
				"^[\\p{Lo}\\p{Lu}\\p{Ll}\\p{Lm}＿_#$][\\p{Lo}\\p{Lu}\\p{Ll}\\p{Lm}\\p{Nd}＿_#$]*$", 
				"\"^(([^\"]|\"\")*)$\"", 
				2, 
				DBNull.Value, 
				"'(([^']|'')*)'", 15 }, 
				true);

			return dt;
		}

		DataTable GetSchemaDataTypes ()
		{
			DataTable dt = new DataTable ();

			dt.Columns.Add ("TypeName", typeof (System.String));
			dt.Columns.Add ("ProviderDbType", typeof (System.Int32));
			dt.Columns.Add ("ColumnSize", typeof (System.Int64));
			dt.Columns.Add ("CreateFormat", typeof (System.String));
			dt.Columns.Add ("CreateParameters", typeof (System.String));
			dt.Columns.Add ("DataType", typeof (System.String));
			dt.Columns.Add ("IsAutoIncrementable", typeof (System.Boolean));
			dt.Columns.Add ("IsBestMatch", typeof (System.Boolean));
			dt.Columns.Add ("IsCaseSensitive", typeof (System.Boolean));
			dt.Columns.Add ("IsFixedLength", typeof (System.Boolean));
			dt.Columns.Add ("IsFixedPrecisionScale", typeof (System.Boolean));
			dt.Columns.Add ("IsLong", typeof (System.Boolean));
			dt.Columns.Add ("IsNullable", typeof (System.Boolean));
			dt.Columns.Add ("IsSearchable", typeof (System.Boolean));
			dt.Columns.Add ("IsSearchableWithLike", typeof (System.Boolean));
			dt.Columns.Add ("IsUnsigned", typeof (System.Boolean));
			dt.Columns.Add ("MaximumScale", typeof (System.Int16));
			dt.Columns.Add ("MinimumScale", typeof (System.Int16));
			dt.Columns.Add ("IsConcurrencyType", typeof (System.Boolean));
			dt.Columns.Add ("IsLiteralSupported", typeof (System.Boolean));
			dt.Columns.Add ("LiteralPrefix", typeof (System.String));
			dt.Columns.Add ("LiteralSuffix", typeof (System.String));

			dt.LoadDataRow (new object [] { "BFILE", 1, 4294967296, "BFILE", DBNull.Value, "System.Byte[]", false, false, false, false, false, true, true, false, false, DBNull.Value, DBNull.Value, DBNull.Value, false, false, DBNull.Value, DBNull.Value }, true);
			dt.LoadDataRow (new object [] { "BLOB", 2, 4294967296, "BLOB", DBNull.Value, "System.Byte[]", false, false, false, false, false, true, true, false, false, DBNull.Value, DBNull.Value, DBNull.Value, false, false, DBNull.Value, DBNull.Value }, true);
			dt.LoadDataRow (new object [] { "CHAR", 3, 2000, "CHAR({0})", "size", "System.String", false, false, true, true, false, false, true, true, true, DBNull.Value, DBNull.Value, DBNull.Value, false, true, "'", "'" }, true);
			dt.LoadDataRow (new object [] { "CLOB", 4, 4294967296, "CLOB", DBNull.Value, "System.String", false, true, true, false, false, false, true, false, false, DBNull.Value, DBNull.Value, DBNull.Value, false, false, DBNull.Value, DBNull.Value }, true);
			dt.LoadDataRow (new object [] { "DATE", 6, 19, "DATE", DBNull.Value, "System.DateTime", false, true, false, true, false, false, true, true, true, DBNull.Value, DBNull.Value, DBNull.Value, false, true, "TO_DATE('", "','YYYY-MM-DD HH24:MI:SS')" }, true);
			dt.LoadDataRow (new object [] { "FLOAT", 29, 38, "FLOAT", DBNull.Value, "System.Decimal", false, true, false, true, false, false, true, true, true, false, DBNull.Value, DBNull.Value, false, true, DBNull.Value, DBNull.Value }, true);
			dt.LoadDataRow (new object [] { "INTERVAL DAY TO SECOND", 7, 0, "INTERVAL DAY({0}) TO SECOND({1})", "dayprecision,secondsprecision", "System.TimeSpan", false, true, false, true, false, false, true, true, false, DBNull.Value, DBNull.Value, DBNull.Value, false, true, "TO_DSINTERVAL('", "')" }, true);
			dt.LoadDataRow (new object [] { "INTERVAL YEAR TO MONTH", 8, 0, "INTERVAL YEAR({0}) TO MONTH", "yearprecision", "System.Int32", false, false, false, true, false, false, true, true, false, DBNull.Value, DBNull.Value, DBNull.Value, false, true, "TO_YMINTERVAL('", "')" }, true);
			dt.LoadDataRow (new object [] { "LONG", 10, 2147483647, "LONG", DBNull.Value, "System.String", false, false, false, false, false, true, true, false, false, DBNull.Value, DBNull.Value, DBNull.Value, false, false, DBNull.Value, DBNull.Value }, true);
			dt.LoadDataRow (new object [] { "LONG RAW", 9, 2147483647, "LONG RAW", DBNull.Value, "System.Byte[]", false, false, false, false, false, true, true, false, false, DBNull.Value, DBNull.Value, DBNull.Value, false, false, DBNull.Value, DBNull.Value }, true);
			dt.LoadDataRow (new object [] { "NCHAR", 11, 2000, "NCHAR({0})", "size", "System.String", false, false, true, true, false, false, true, true, true, DBNull.Value, DBNull.Value, DBNull.Value, false, true, "N'", "'" }, true);
			dt.LoadDataRow (new object [] { "NCLOB", 12, 4294967296, "NCLOB", DBNull.Value, "System.String", false, false, true, false, false, true, true, false, false, DBNull.Value, DBNull.Value, DBNull.Value, false, false, DBNull.Value, DBNull.Value }, true);
			dt.LoadDataRow (new object [] { "NUMBER", 13, 38, "NUMBER ({0},{1})", "precision,scale", "System.Decimal", false, true, false, true, false, false, true, true, true, false, 127, -84, false, true, "", "" }, true);
			dt.LoadDataRow (new object [] { "NVARCHAR2", 14, 4000, "NVARCHAR2({0})", "size", "System.String", false, false, true, false, false, false, true, true, false, DBNull.Value, DBNull.Value, DBNull.Value, false, true, "N'", "'" }, true);
			dt.LoadDataRow (new object [] { "RAW", 15, 2000, "RAW({0})", "size", "System.Byte[]", false, true, false, false, true, false, true, true, true, DBNull.Value, DBNull.Value, DBNull.Value, false, true, "HEXTORAW('", "')" }, true);
			dt.LoadDataRow (new object [] { "ROWID", 16, 3950, "ROWID", DBNull.Value, "System.String", true, false, false, false, false, false, false, true, false, DBNull.Value, DBNull.Value, DBNull.Value, false, false, DBNull.Value, DBNull.Value }, true);
			dt.LoadDataRow (new object [] { "TIMESTAMP", 18, 27, "TIMESTAMP({0})", "precision of fractional seconds", "System.DateTime", false, false, false, true, false, false, true, true, false, DBNull.Value, DBNull.Value, DBNull.Value, false, true, "TO_TIMESTAMP('", "','YYYY-MM-DD HH24:MI:SS.FF')" }, true);
			dt.LoadDataRow (new object [] { "TIMESTAMP WITH LOCAL TIME ZONE", 19, 27, "TIMESTAMP({0} WITH LOCAL TIME ZONE)", "precision of fractional seconds", "System.DateTime", false, false, false, true, false, false, true, true, false, DBNull.Value, DBNull.Value, DBNull.Value, false, true, "TO_TIMESTAMP_TZ('", "','YYYY-MM-DD HH24:MI:SS.FF')" }, true);
			dt.LoadDataRow (new object [] { "TIMESTAMP WITH TIME ZONE", 20, 34, "TIMESTAMP({0} WITH TIME ZONE)", "precision of fractional seconds", "System.DateTime", false, false, false, true, false, false, true, true, false, DBNull.Value, DBNull.Value, DBNull.Value, false, true, "TO_TIMESTAMP_TZ('", "','YYYY-MM-DD HH24:MI:SS.FF TZH:TZM')" }, true);
			dt.LoadDataRow (new object [] { "VARCHAR2", 22, 4000, "VARCHAR2({0})", "size", "System.String", false, false, true, false, true, false, true, true, true, DBNull.Value, DBNull.Value, DBNull.Value, false, true, "'", "'" }, true);

			return dt;
		}

		DataTable GetSchemaReservedWords ()
		{
			DataTable dt = new DataTable ();

			dt.Columns.Add ("ReservedWord", typeof (System.String));

			dt.LoadDataRow (new object [] { "ACCESS" }, true);
			dt.LoadDataRow (new object [] { "ADD" }, true);
			dt.LoadDataRow (new object [] { "ALL" }, true);
			dt.LoadDataRow (new object [] { "ALTER" }, true);
			dt.LoadDataRow (new object [] { "AND" }, true);
			dt.LoadDataRow (new object [] { "ANY" }, true);
			dt.LoadDataRow (new object [] { "AS" }, true);
			dt.LoadDataRow (new object [] { "ASC" }, true);
			dt.LoadDataRow (new object [] { "AUDIT" }, true);
			dt.LoadDataRow (new object [] { "BETWEEN" }, true);
			dt.LoadDataRow (new object [] { "BY" }, true);
			dt.LoadDataRow (new object [] { "CHAR" }, true);
			dt.LoadDataRow (new object [] { "CHECK" }, true);
			dt.LoadDataRow (new object [] { "CLUSTER" }, true);
			dt.LoadDataRow (new object [] { "COLUMN" }, true);
			dt.LoadDataRow (new object [] { "COMMENT" }, true);
			dt.LoadDataRow (new object [] { "COMPRESS" }, true);
			dt.LoadDataRow (new object [] { "CONNECT" }, true);
			dt.LoadDataRow (new object [] { "CREATE" }, true);
			dt.LoadDataRow (new object [] { "CURRENT" }, true);
			dt.LoadDataRow (new object [] { "DATE" }, true);
			dt.LoadDataRow (new object [] { "DECIMAL" }, true);
			dt.LoadDataRow (new object [] { "DEFAULT" }, true);
			dt.LoadDataRow (new object [] { "DELETE" }, true);
			dt.LoadDataRow (new object [] { "DESC" }, true);
			dt.LoadDataRow (new object [] { "DISTINCT" }, true);
			dt.LoadDataRow (new object [] { "DROP" }, true);
			dt.LoadDataRow (new object [] { "ELSE" }, true);
			dt.LoadDataRow (new object [] { "EXCLUSIVE" }, true);
			dt.LoadDataRow (new object [] { "EXISTS" }, true);
			dt.LoadDataRow (new object [] { "FILE" }, true);
			dt.LoadDataRow (new object [] { "FLOAT" }, true);
			dt.LoadDataRow (new object [] { "FOR" }, true);
			dt.LoadDataRow (new object [] { "FROM" }, true);
			dt.LoadDataRow (new object [] { "GRANT" }, true);
			dt.LoadDataRow (new object [] { "GROUP" }, true);
			dt.LoadDataRow (new object [] { "HAVING" }, true);
			dt.LoadDataRow (new object [] { "IDENTIFIED" }, true);
			dt.LoadDataRow (new object [] { "IMMEDIATE" }, true);
			dt.LoadDataRow (new object [] { "IN" }, true);
			dt.LoadDataRow (new object [] { "INCREMENT" }, true);
			dt.LoadDataRow (new object [] { "INDEX" }, true);
			dt.LoadDataRow (new object [] { "INITAL" }, true);
			dt.LoadDataRow (new object [] { "INSERT" }, true);
			dt.LoadDataRow (new object [] { "INTEGER" }, true);
			dt.LoadDataRow (new object [] { "INTERSECT" }, true);
			dt.LoadDataRow (new object [] { "INTO" }, true);
			dt.LoadDataRow (new object [] { "IS" }, true);
			dt.LoadDataRow (new object [] { "LEVEL" }, true);
			dt.LoadDataRow (new object [] { "LIKE" }, true);
			dt.LoadDataRow (new object [] { "LOCK" }, true);
			dt.LoadDataRow (new object [] { "LONG" }, true);
			dt.LoadDataRow (new object [] { "MAXEXTENTS" }, true);
			dt.LoadDataRow (new object [] { "MINUS" }, true);
			dt.LoadDataRow (new object [] { "MLSLABEL" }, true);
			dt.LoadDataRow (new object [] { "MODE" }, true);
			dt.LoadDataRow (new object [] { "MODIFY" }, true);
			dt.LoadDataRow (new object [] { "NOAUDIT" }, true);
			dt.LoadDataRow (new object [] { "NOCOMPRESS" }, true);
			dt.LoadDataRow (new object [] { "NOT" }, true);
			dt.LoadDataRow (new object [] { "NOWAIT" }, true);
			dt.LoadDataRow (new object [] { "NULL" }, true);
			dt.LoadDataRow (new object [] { "NUMBER" }, true);
			dt.LoadDataRow (new object [] { "OF" }, true);
			dt.LoadDataRow (new object [] { "OFFLINE" }, true);
			dt.LoadDataRow (new object [] { "ON" }, true);
			dt.LoadDataRow (new object [] { "ONLINE" }, true);
			dt.LoadDataRow (new object [] { "OPTION" }, true);
			dt.LoadDataRow (new object [] { "OR" }, true);
			dt.LoadDataRow (new object [] { "ORDER" }, true);
			dt.LoadDataRow (new object [] { "PCTFREE" }, true);
			dt.LoadDataRow (new object [] { "PRIOR" }, true);
			dt.LoadDataRow (new object [] { "PRIVILEGES" }, true);
			dt.LoadDataRow (new object [] { "PUBLIC" }, true);
			dt.LoadDataRow (new object [] { "RAW" }, true);
			dt.LoadDataRow (new object [] { "RENAME" }, true);
			dt.LoadDataRow (new object [] { "RESOURCE" }, true);
			dt.LoadDataRow (new object [] { "REVOKE" }, true);
			dt.LoadDataRow (new object [] { "ROW" }, true);
			dt.LoadDataRow (new object [] { "ROWID" }, true);
			dt.LoadDataRow (new object [] { "ROWNUM" }, true);
			dt.LoadDataRow (new object [] { "ROWS" }, true);
			dt.LoadDataRow (new object [] { "SELECT" }, true);
			dt.LoadDataRow (new object [] { "SESSION" }, true);
			dt.LoadDataRow (new object [] { "SET" }, true);
			dt.LoadDataRow (new object [] { "SHARE" }, true);
			dt.LoadDataRow (new object [] { "SIZE" }, true);
			dt.LoadDataRow (new object [] { "SMALLINT" }, true);
			dt.LoadDataRow (new object [] { "START" }, true);
			dt.LoadDataRow (new object [] { "SUCCESSFUL" }, true);
			dt.LoadDataRow (new object [] { "SYNONYM" }, true);
			dt.LoadDataRow (new object [] { "SYSDATE" }, true);
			dt.LoadDataRow (new object [] { "TABLE" }, true);
			dt.LoadDataRow (new object [] { "THEN" }, true);
			dt.LoadDataRow (new object [] { "TO" }, true);
			dt.LoadDataRow (new object [] { "TRIGGER" }, true);
			dt.LoadDataRow (new object [] { "UID" }, true);
			dt.LoadDataRow (new object [] { "UNION" }, true);
			dt.LoadDataRow (new object [] { "UNIQUE" }, true);
			dt.LoadDataRow (new object [] { "UPDATE" }, true);
			dt.LoadDataRow (new object [] { "USER" }, true);
			dt.LoadDataRow (new object [] { "VALIDATE" }, true);
			dt.LoadDataRow (new object [] { "VALUES" }, true);
			dt.LoadDataRow (new object [] { "VARCHAR" }, true);
			dt.LoadDataRow (new object [] { "VARCHAR2" }, true);
			dt.LoadDataRow (new object [] { "VIEW" }, true);
			dt.LoadDataRow (new object [] { "WHENEVER" }, true);
			dt.LoadDataRow (new object [] { "WHERE" }, true);
			dt.LoadDataRow (new object [] { "WITH" }, true);

			return dt;
		}

		DataTable GetSchemaDataTable (OracleCommand cmd, string [] restrictionValues)
		{
			if (restrictionValues != null) {
				for (int i = 0; i < restrictionValues.Length; i++)
					cmd.Parameters [i].Value = restrictionValues [i];
			}

			OracleDataAdapter adapter = new OracleDataAdapter (cmd);
			DataTable dt = new DataTable ();
			adapter.Fill (dt);

			return dt;
		}
#endif
	}
}


