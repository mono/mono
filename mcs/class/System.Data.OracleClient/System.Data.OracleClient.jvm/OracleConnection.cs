//
// System.Data.OracleClient.OracleConnection
//
// Authors:
//	Konstantin Triger <kostat@mainsoft.com>
//	Boris Kirzner <borisk@mainsoft.com>
//	
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
//

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



using System.Data;
using System.Data.Common;
using System.Data.ProviderBase;
using System.Collections;

using java.sql;

namespace System.Data.OracleClient {
	public sealed class OracleConnection : AbstractDBConnection, System.ICloneable {
		#region Fields 

		static readonly Hashtable _skippedUserParameters = new Hashtable(new CaseInsensitiveHashCodeProvider(),new CaseInsensitiveComparer());

		private static readonly object _lockObjectStringManager = new object();
		//private static DbStringManager _stringManager = new DbStringManager("System.Data.System.Data.ProviderBase.jvm.OracleStrings");

		private static readonly string[] _resourceIgnoredKeys = new string[] {"CON_PROVIDER","CON_DATA_SOURCE","CON_DATABASE",
																				 "CON_PASSWORD","CON_USER_ID","CON_TIMEOUT",
																				 "CON_SERVER_NAME","CON_PORT","CON_SERVICE_NAME",
																				 "CON_JNDI_NAME","CON_JNDI_PROVIDER","CON_JNDI_FACTORY",
																				 "JDBC_DRIVER","JDBC_URL","DB2_CON_LOCATION" };

		#endregion //Fields

		#region Events

		public event OracleInfoMessageEventHandler InfoMessage;
		public event StateChangeEventHandler StateChange;

		#endregion // Events
		
		#region Constructors

		public OracleConnection() : this(null) {
		}

		public OracleConnection(String connectionString) : base(connectionString) {			
		}

		#endregion // Constructors

		#region Properties

		protected override string[] ResourceIgnoredKeys {
			get { return _resourceIgnoredKeys; }
		}

		public String Provider {
			get { return ConnectionStringHelper.FindValue(UserParameters,StringManager.GetStringArray("CON_PROVIDER")); }
		}

		protected override Hashtable SkippedUserParameters {
			get { return _skippedUserParameters; }
		}

		protected override string ServerName {
			get { 
				if (ProviderType == PROVIDER_TYPE.IBMDADB2 || ProviderType == PROVIDER_TYPE.MSDAORA) {
					string host = ConnectionStringHelper.FindValue(UserParameters,StringManager.GetStringArray("CON_SERVER_NAME"));

					if (!String.Empty.Equals(host)) {
						return host;
					}

					if (ProviderType == PROVIDER_TYPE.IBMDADB2) {
						string location = ConnectionStringHelper.FindValue(UserParameters,StringManager.GetStringArray("DB2_CON_LOCATION"));

						if (!String.Empty.Equals(location)) {
							int semicolumnIndex = location.IndexOf(':');
							if (semicolumnIndex != -1) {
								return location.Substring(0,semicolumnIndex);
							}
							else {
								return location;
							}
						}
					}

				}
				return base.ServerName; 
			}
		}

		protected override string CatalogName {
			get { 
				switch (ProviderType) {
					case PROVIDER_TYPE.IBMDADB2:
					case PROVIDER_TYPE.MSDAORA:
						return DataSource;
				}
				return base.CatalogName;
			}
		}

		protected override string Port {
			get {
				string port = ConnectionStringHelper.FindValue(UserParameters, StringManager.GetStringArray("CON_PORT"));
				switch (ProviderType) {
					case PROVIDER_TYPE.MSDAORA :
						if (String.Empty.Equals(port)) {
							return StringManager.GetString("ORA_CON_PORT");
						}
						return port;
					case PROVIDER_TYPE.IBMDADB2 :
						if (String.Empty.Equals(port)) {
							string location = ConnectionStringHelper.FindValue(UserParameters,StringManager.GetStringArray("DB2_CON_LOCATION"));

							if (!String.Empty.Equals(location)) {
								int semicolumnIndex = location.IndexOf(':');
								if (semicolumnIndex != -1) {
									return location.Substring(semicolumnIndex + 1);
								}
							}
							return StringManager.GetString("DB2_CON_PORT");
						}
						return port;
				}
				return base.Port;
			}
		}

		protected override string JdbcDriverName {
			get {
				JDBC_MODE jdbcMode = JdbcMode;
				switch (jdbcMode) {
					case JDBC_MODE.DATA_SOURCE_MODE :
						return base.JdbcDriverName;
					case JDBC_MODE.JDBC_DRIVER_MODE :
						return ConnectionStringHelper.FindValue(UserParameters,StringManager.GetStringArray("JDBC_DRIVER"));
					case JDBC_MODE.PROVIDER_MODE:
					switch (ProviderType) {
						case PROVIDER_TYPE.SQLOLEDB :
							return StringManager.GetString("SQL_JDBC_DRIVER");
						case PROVIDER_TYPE.MSDAORA :
							return StringManager.GetString("ORA_JDBC_DRIVER");
						case PROVIDER_TYPE.IBMDADB2 :
							return StringManager.GetString("DB2_JDBC_DRIVER");
					}
						break;
				};
				return base.JdbcDriverName;
			}
		}

		protected override DbStringManager StringManager {
			get {
				object stringManager = AppDomain.CurrentDomain.GetData("System.Data.OracleConnection.stringManager");
				if (stringManager == null) {
					lock(_lockObjectStringManager) {
						stringManager = AppDomain.CurrentDomain.GetData("System.Data.OracleConnection.stringManager");
						if (stringManager != null)
							return (DbStringManager)stringManager;
						stringManager = new DbStringManager("System.Data.System.Data.ProviderBase.jvm.OracleStrings");
						AppDomain.CurrentDomain.SetData("System.Data.OracleConnection.stringManager", stringManager);
					}
				}
				return (DbStringManager)stringManager;
			}
		}

		#endregion // Properties

		#region Methods

		public new OracleTransaction BeginTransaction(IsolationLevel level) {
			return new OracleTransaction(level, this);
		}

		public new OracleTransaction BeginTransaction() {
			return BeginTransaction(IsolationLevel.ReadCommitted);
		}

		public new OracleCommand CreateCommand() {
			return new OracleCommand(this);
		}

		protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) {
			return BeginTransaction();
		}

		protected override DbCommand CreateDbCommand() {
			return CreateCommand();
		}


		protected override void CopyTo(AbstractDBConnection target) {
			base.CopyTo(target);
		}

		public object Clone() {
			OracleConnection clone = new OracleConnection();
			CopyTo(clone);
			return clone;
		}

		protected sealed override SystemException CreateException(SQLException e) {
			return new OracleException(e,this);		
		}

		protected sealed override SystemException CreateException(string message) {
			return new OracleException(message, null, this);	
		}

		public DataTable GetOracleSchemaTable (Guid schema, object[] restrictions) {
			DataTable schemaTable = new DataTable("Tables");
			schemaTable.Columns.Add("TABLE_CATALOG");
			schemaTable.Columns.Add("TABLE_SCHEMA");
			schemaTable.Columns.Add("TABLE_NAME");
			schemaTable.Columns.Add("TABLE_TYPE");
			schemaTable.Columns.Add("TABLE_GUID");
			schemaTable.Columns.Add("DESCRIPTION");
			schemaTable.Columns.Add("TABLE_PROPID");
			schemaTable.Columns.Add("DATE_CREATED");
			schemaTable.Columns.Add("DATE_MODIFIED");
            
            
			Connection con = JdbcConnection;
			String catalog = con.getCatalog();
            
			DatabaseMetaData meta = con.getMetaData();
			ResultSet schemaRes = meta.getSchemas();
			System.Collections.ArrayList schemas = new System.Collections.ArrayList();
			while(schemaRes.next()) {
				schemas.Add(schemaRes.getString(1));
			}
			schemaRes.close();

			for(int i = 0; i < schemas.Count; i++) {
				ResultSet tableRes = meta.getTables(catalog, schemas[i].ToString(), null, null);
				while(tableRes.next()) {
					DataRow row = schemaTable.NewRow();
					row["TABLE_CATALOG"] = catalog;
					row["TABLE_SCHEMA"] = schemas[i];
					row["TABLE_NAME"] = tableRes.getString("TABLE_NAME");
					row["TABLE_TYPE"] = tableRes.getString("TABLE_TYPE");
					row["DESCRIPTION"] = tableRes.getString("REMARKS");
                    
					schemaTable.Rows.Add(row);
				}
				tableRes.close();
			}
			return schemaTable;
		}

		protected override void ValidateConnectionString(string connectionString) {
			base.ValidateConnectionString(connectionString);
		}

		protected override string BuildJdbcUrl() {
			return base.BuildJdbcUrl();
		}

		public static void ReleaseObjectPool() {
			// since we're using connection pool from app servet, this is by design
			//throw new NotImplementedException();
		}

		protected sealed override void OnSqlWarning(SQLWarning warning) {
			OracleErrorCollection col = new OracleErrorCollection(warning, this);
			OnOracleInfoMessage(new OracleInfoMessageEventArgs(col));
		}

//		protected sealed override Connection GetConnectionFromProvider() {
//			if ((ProviderType == PROVIDER_TYPE.MSDAORA) && 
//				("true").Equals(StringManager.GetString("ORA_CONNECTION_POOLING_ENABLED","false"))) {
//				ActivateJdbcDriver(JdbcDriverName);
//				return OracleConnectionFactory.GetConnection(ProviderType,JdbcUrl,User,Password,ConnectionTimeout);
//			}
//			else {
//				return base.GetConnectionFromProvider();
//			}
			//TBD
//		}

		private String BuildDb2Url() {
			return StringManager.GetString("DB2_JDBC_URL") //jdbc:db2://
				+ ServerName + ":" + Port + "/" + CatalogName;
		}

		private String BuildOracleUrl() {
			return StringManager.GetString("ORA_JDBC_URL") //"jdbc:oracle:thin:@"
				+ ServerName + ":" + Port + ":" + CatalogName;
		}
        
		protected sealed override void OnStateChanged(ConnectionState orig, ConnectionState current) {
			if(StateChange != null) {
				StateChange(this, new StateChangeEventArgs(orig, current));
			}
		}

		public override void Close() {
			ConnectionState orig = State;
			base.Close();
			ConnectionState current = State;
			if(current != orig) {
				OnStateChanged(orig, current);
			}
		}

		private void OnOracleInfoMessage (OracleInfoMessageEventArgs value) {
			if (InfoMessage != null) {
				InfoMessage (this, value);
			}
		}

		#endregion // Methods

	}
}