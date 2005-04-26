//
// System.Data.Common.AbstractDBConnection
//
// Author:
//   Boris Kirzner (borisk@mainsoft.com)
//

using System.Data;
using System.Data.ProviderBase;
using System.Data.Configuration;
using System.Configuration;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;

using java.sql;
using javax.sql;
using javax.naming;
// can not use java.util here - it manes ArrayList an ambiguous reference

namespace System.Data.Common
{
	public abstract class AbstractDBConnection : DbConnection
	{
		#region ObjectNamesHelper

		private sealed class ObjectNamesHelper
		{
			//static readonly Regex NameOrder = new Regex(@"^\s*((\[(?<NAME>(\s*[^\[\]\s])+)\s*\])|(?<NAME>(\w|!|\#|\$)+(\s*(\w|!|\#|\$)+)*))\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
			static readonly Regex NameOrder = new Regex(@"^((\[(?<NAME>[^\]]+)\])|(?<NAME>[^\.\[\]]+))$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

			//static readonly Regex SchemaNameOrder = new Regex(@"^\s*((\[(?<SCHEMA>(\s*[^\[\]\s])+)\s*\])|(?<SCHEMA>(\w|!|\#|\$)*(\s*(\w|!|\#|\$)+)*))\s*\.\s*((\[(?<NAME>(\s*[^\[\]\s])+)\s*\])|(?<NAME>(\w|!|\#|\$)+(\s*(\w|!|\#|\$)+)*))\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
			static readonly Regex SchemaNameOrder = new Regex(@"^((\[(?<SCHEMA>[^\]]+)\])|(?<SCHEMA>[^\.\[\]]+))\s*\.\s*((\[(?<NAME>[^\]]+)\])|(?<NAME>[^\.\[\]]+))$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
			//static readonly Regex CatalogSchemaNameOrder = new Regex(@"^\s*((\[\s*(?<CATALOG>(\s*[^\[\]\s])+)\s*\])|(?<CATALOG>(\w|!|\#|\$)*(\s*(\w|!|\#|\$)+)*))\s*\.\s*((\[(?<SCHEMA>(\s*[^\[\]\s])+)\s*\])|(?<SCHEMA>(\w|!|\#|\$)*(\s*(\w|!|\#|\$)+)*))\s*\.\s*((\[(?<NAME>(\s*[^\[\]\s])+)\s*\])|(?<NAME>(\w|!|\#|\$)+(\s*(\w|!|\#|\$)+)*))\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
			//static readonly Regex CatalogSchemaNameOrder = new Regex(@"^\s*((\[\s*(?<CATALOG>(\s*[^\]\s])+)\s*\])|(?<CATALOG>([^\.\s])*(\s*([^\.\s])+)*))\s*\.\s*((\[(?<SCHEMA>(\s*[^\]\s])+)\s*\])|(?<SCHEMA>([^\.\s])*(\s*([^\.\s])+)*))\s*\.\s*((\[(?<NAME>(\s*[^\]\s])+)\s*\])|(?<NAME>([^\.\s])+(\s*([^\.\s])+)*))\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
			static readonly Regex CatalogSchemaNameOrder = new Regex(@"^((\[(?<CATALOG>[^\]]+)\])|(?<CATALOG>[^\.\[\]]+))\s*\.\s*((\[(?<SCHEMA>[^\]]+)\])|(?<SCHEMA>[^\.\[\]]+))\s*\.\s*((\[(?<NAME>[^\]]+)\])|(?<NAME>[^\.\[\]]+))$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

			//static readonly Regex CatalogNameOrder = new Regex(@"^\s*((\[(?<CATALOG>(\s*[^\[\]\s])+)\s*\])|(?<CATALOG>(\w|!|\#|\$)*(\s*(\w|!|\#|\$)+)*))\s*\.\s*((\[(?<NAME>(\s*[^\[\]\s])+)\s*\])|(?<NAME>(\w|!|\#|\$)+(\s*(\w|!|\#|\$)+)*))\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
			//static readonly Regex CatalogNameOrder = new Regex(@"^\s*((\[(?<CATALOG>(\s*[^\]\s])+)\s*\])|(?<CATALOG>([^\.\s])*(\s*([^\.\s])+)*))\s*\.\s*((\[(?<NAME>(\s*[^\]\s])+)\s*\])|(?<NAME>([^\.\s])+(\s*([^\.\s])+)*))\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
			static readonly Regex CatalogNameOrder = new Regex(@"^((\[(?<CATALOG>[^\]]+)\])|(?<CATALOG>[^\.\[\]]+))\s*\.\s*((\[(?<NAME>[^\]]+)\])|(?<NAME>[^\.\[\]]+))$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
			//static readonly Regex SchemaCatalogNameOrder = new Regex(@"^\s*((\[\s*(?<SCHEMA>(\s*[^\[\]\s])+)\s*\])|(?<SCHEMA>(\w|!|\#|\$)*(\s*(\w|!|\#|\$)+)*))\s*\.\s*((\[(?<CATALOG>(\s*[^\[\]\s])+)\s*\])|(?<CATALOG>(\w|!|\#|\$)*(\s*(\w|!|\#|\$)+)*))\s*\.\s*((\[(?<NAME>(\s*[^\[\]\s])+)\s*\])|(?<NAME>(\w|!|\#|\$)+(\s*(\w|!|\#|\$)+)*))\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
			//static readonly Regex SchemaCatalogNameOrder = new Regex(@"^\s*((\[\s*(?<SCHEMA>(\s*[^\]\s])+)\s*\])|(?<SCHEMA>([^\.\s])*(\s*([^\.\s])+)*))\s*\.\s*((\[(?<CATALOG>(\s*[^\]\s])+)\s*\])|(?<CATALOG>([^\.\s])*(\s*([^\.\s])+)*))\s*\.\s*((\[(?<NAME>(\s*[^\]\s])+)\s*\])|(?<NAME>([^\.\s])+(\s*([^\.\s])+)*))\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
			static readonly Regex SchemaCatalogNameOrder = new Regex(@"^((\[(?<SCHEMA>[^\]]+)\])|(?<SCHEMA>[^\.\[\]]+))\s*\.\s*((\[(?<CATALOG>[^\]]+)\])|(?<CATALOG>[^\.\[\]]+))\s*\.\s*((\[(?<NAME>[^\]]+)\])|(?<NAME>[^\.\[\]]+))$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

			internal static ObjectNameResolver[] GetSyntaxPatterns(AbstractDBConnection connection)
			{
				ArrayList collection = new ArrayList();
				collection.Add(new ObjectNameResolver(NameOrder));

				ObjectNameResolversCollection basic = (ObjectNameResolversCollection)ConfigurationSettings.GetConfig("system.data/objectnameresolution");
				
				DatabaseMetaData metaData = connection.JdbcConnection.getMetaData();
				string productName = metaData.getDatabaseProductName();

				foreach(ObjectNameResolver nameResolver in basic) {
					if (productName.IndexOf(nameResolver.DbName) != -1) {
						collection.Add(nameResolver);
					}
				}

				//defaults
				if (metaData.isCatalogAtStart()) {
					collection.Add(new ObjectNameResolver(SchemaNameOrder));
					collection.Add(new ObjectNameResolver(CatalogNameOrder));
					collection.Add(new ObjectNameResolver(CatalogSchemaNameOrder));
					collection.Add(new ObjectNameResolver(SchemaCatalogNameOrder));
				}
				else {
					collection.Add(new ObjectNameResolver(CatalogNameOrder));
					collection.Add(new ObjectNameResolver(SchemaNameOrder));
					collection.Add(new ObjectNameResolver(SchemaCatalogNameOrder));
					collection.Add(new ObjectNameResolver(CatalogSchemaNameOrder));
				}

				return (ObjectNameResolver[])collection.ToArray(typeof(ObjectNameResolver));				
			}
		}

		#endregion // ObjectNamesHelper

		#region ConnectionStringHelper

		internal sealed class ConnectionStringHelper
		{
			internal static string FindValue(NameValueCollection collection, string[] keys)
			{
				if (collection == null || keys == null || keys.Length == 0) {
					return String.Empty;
				}

				for(int i=0; i < keys.Length; i++) {
					string value = FindValue(collection,keys[i]);
					if (!String.Empty.Equals(value)) {
						return value;
					}
				}
				return String.Empty;
			}

			internal static string FindValue(NameValueCollection collection, string key)
			{
				if (collection == null) {
					return String.Empty;
				}

				string value = collection[key];
				return (value != null) ? value : String.Empty;
			}

			internal static void UpdateValue(NameValueCollection collection,string[] keys,string value)
			{
				for(int i=0; i < keys.Length; i++) {
					if (collection[keys[i]] != null) {
						collection[keys[i]] = value;
					}
				}
			}

			internal static void AddValue(NameValueCollection collection,string[] keys,string value)
			{
				for(int i=0; i < keys.Length; i++) {
					collection[keys[i]] = value;
				}
			}

			/**
			* Parses connection string and builds NameValueCollection 
			* for all keys.
			*/ 
			internal static NameValueCollection BuildUserParameters (string connectionString)
			{
				NameValueCollection userParameters = new NameValueCollection();

				if (connectionString == null || connectionString.Length == 0) {
					return userParameters;
				}
				connectionString += ";";

				bool inQuote = false;
				bool inDQuote = false;
				bool inName = true;

				string name = String.Empty;
				string value = String.Empty;
				StringBuilder sb = new StringBuilder ();

				for (int i = 0; i < connectionString.Length; i += 1) {
					char c = connectionString [i];
					char peek;
					if (i == connectionString.Length - 1)
						peek = '\0';
					else
						peek = connectionString [i + 1];

					switch (c) {
						case '\'':
							if (inDQuote)
								sb.Append (c);
							else if (peek.Equals(c)) {
								sb.Append(c);
								i += 1;
							}
							else
								inQuote = !inQuote;
							break;
						case '"':
							if (inQuote)
								sb.Append(c);
							else if (peek.Equals(c)) {
								sb.Append(c);
								i += 1;
							}
							else
								inDQuote = !inDQuote;
							break;
						case ';':
							if (inDQuote || inQuote)
								sb.Append(c);
							else {
								if (name != String.Empty && name != null) {
									value = sb.ToString();
									userParameters [name.Trim()] = value.Trim();
								}
								inName = true;
								name = String.Empty;
								value = String.Empty;
								sb = new StringBuilder();
							}
							break;
						case '=':
							if (inDQuote || inQuote || !inName)
								sb.Append (c);
							else if (peek.Equals(c)) {
								sb.Append (c);
								i += 1;
							}
							else {
								name = sb.ToString();
								sb = new StringBuilder();
								inName = false;
							}
							break;
						case ' ':
							if (inQuote || inDQuote)
								sb.Append(c);
							else if (sb.Length > 0 && !peek.Equals(';'))
								sb.Append(c);
							break;
						default:
							sb.Append(c);
							break;
					}
				}
				return userParameters;
			}
		}

		#endregion // ConnectionStringHelper

		#region DataSourceCache

		private sealed class DataSourceCache : AbstractDbMetaDataCache
		{
			internal DataSource GetDataSource(string dataSourceName,string namingProviderUrl,string namingFactoryInitial)
			{
				Hashtable cache = Cache;

				DataSource ds = cache[dataSourceName] as DataSource;

				if (ds != null) {
					return ds;
				}

				Context ctx = null;
				
				java.util.Properties properties = new java.util.Properties();

				if ((namingProviderUrl != null) && (namingProviderUrl.Length > 0)) {
					properties.put("java.naming.provider.url",namingProviderUrl);
				}
				
				if ((namingFactoryInitial != null) && (namingFactoryInitial.Length > 0)) {
					properties.put("java.naming.factory.initial",namingFactoryInitial);
				}

				ctx = new InitialContext(properties);
 
				try {
					ds = (DataSource)ctx.lookup(dataSourceName);
				}
				catch(javax.naming.NameNotFoundException e) {
					// possible that is a Tomcat bug,
					// so try to lookup for jndi datasource with "java:comp/env/" appended
					ds = (DataSource)ctx.lookup("java:comp/env/" + dataSourceName);
				}

				cache[dataSourceName] = ds;
				return ds;
			}
		}

		#endregion // DatasourceCache

		#region Declarations

		protected internal enum JDBC_MODE { NONE, DATA_SOURCE_MODE, JDBC_DRIVER_MODE, PROVIDER_MODE }
		protected internal enum PROVIDER_TYPE { NONE, SQLOLEDB, MSDAORA, IBMDADB2 }

		#endregion // Declarations
		
		#region Fields

		private static DataSourceCache _dataSourceCache = new DataSourceCache();
		private const int DEFAULT_TIMEOUT = 15;

		private Connection _jdbcConnnection;
		private ConnectionState _internalState;
		private object _internalStateSync = new object();

		private NameValueCollection _userParameters;

		protected string _connectionString = String.Empty;
		protected string _jdbcUrl;		

		private ArrayList _referencedObjects = new ArrayList();	
		private ObjectNameResolver[] _syntaxPatterns;

		#endregion // Fields

		#region Constructors

		public AbstractDBConnection(string connectionString)
		{
			_connectionString = connectionString;
			InitializeSkippedUserParameters();
		}

		#endregion // Constructors

		#region Properties

		public override String ConnectionString
		{
			get { return _connectionString; }
			set {
				if (IsOpened) {
					throw ExceptionHelper.NotAllowedWhileConnectionOpen("ConnectionString",_internalState);
				}					
				_connectionString = value;
				_userParameters = null;
				_jdbcUrl = null;
			}
		}

		public override int ConnectionTimeout
		{
			get {
				string timeoutStr = ConnectionStringHelper.FindValue(UserParameters,StringManager.GetStringArray("CON_TIMEOUT"));
				if (!String.Empty.Equals(timeoutStr)) {
					try {
						return Convert.ToInt32(timeoutStr);
					}
					catch(FormatException e) {
						throw ExceptionHelper.InvalidValueForKey("connect timeout");
					}
					catch (OverflowException e) {
						throw ExceptionHelper.InvalidValueForKey("connect timeout");
					}
				}
				return DEFAULT_TIMEOUT;
			}
		}

		public override String Database
		{
			get { return ConnectionStringHelper.FindValue(UserParameters,StringManager.GetStringArray("CON_DATABASE")); }
		}

		public override ConnectionState State
		{
			get {
				try {
					if ((JdbcConnection == null) || JdbcConnection.isClosed()) {
						// jdbc connection not initialized or closed
						if (_internalState == ConnectionState.Closed ) {
							return ConnectionState.Closed;
						}
					}
					else {
						// jdbc connection is opened
						if ((_internalState & ConnectionState.Open) != 0) {
							return ConnectionState.Open;
						}
					}
					return ConnectionState.Broken;										
				}	
				catch (SQLException) {
					return ConnectionState.Broken;
				}				
			}
		}

		internal bool IsExecuting
		{
			get { 
				return ((_internalState & ConnectionState.Executing) != 0);
			}

			set {
				lock(_internalStateSync) {
					// to switch to executing, the connection must be in opened
					if (value) {
						if (_internalState != ConnectionState.Open) {
							if (IsFetching) {
								throw ExceptionHelper.OpenedReaderExists();
							}
							throw ExceptionHelper.OpenConnectionRequired("",_internalState);
						}
						_internalState |= ConnectionState.Executing;
					}
					else { 
						if (!IsExecuting) {
							throw new InvalidOperationException("Connection : Impossible to tear down from state " + ConnectionState.Executing.ToString() + " while in state " + _internalState.ToString());
						}
						_internalState &= ~ConnectionState.Executing;
					}
				}
			}
		}

		internal bool IsFetching
		{
			get {
				return ((_internalState & ConnectionState.Fetching) != 0);
			}

			set {
				lock(_internalStateSync) {
					if (value) {
						// to switch to fetching connection must be in opened, executing
						if (((_internalState & ConnectionState.Open) == 0) || ((_internalState & ConnectionState.Executing) == 0)) {
							throw ExceptionHelper.OpenConnectionRequired("",_internalState);
						}
						_internalState |= ConnectionState.Fetching;
					}
					else {
						if (!IsFetching) {
							throw new InvalidOperationException("Connection : Impossible to tear down from state " + ConnectionState.Fetching.ToString() + " while in state " + _internalState.ToString());
						}
						_internalState &= ~ConnectionState.Fetching;
					}
				}
			}
		}

		internal bool IsOpened
		{
			get {
				return ((_internalState & ConnectionState.Open) != 0);
			}

			set {
				lock(_internalStateSync) {			
					if (value) {
						// only connecting connection can be opened
						if ((_internalState != ConnectionState.Connecting)) {
							throw ExceptionHelper.ConnectionAlreadyOpen(_internalState);
						}
						_internalState |= ConnectionState.Open;
					}
					else {
						if (!IsOpened) {
							throw new InvalidOperationException("Connection : Impossible to tear down from state " + ConnectionState.Open.ToString() + " while in state " + _internalState.ToString());
						}
						_internalState &= ~ConnectionState.Open;
					}
				}
			}
		}

		internal bool IsConnecting
		{
			get {
				return ((_internalState & ConnectionState.Connecting) != 0);
			}

			set {
				lock(_internalStateSync) {			
					if (value) {
						// to switch to connecting conection must be in closed or in opened
						if ((_internalState != ConnectionState.Closed) && (_internalState != ConnectionState.Open)) {
							throw ExceptionHelper.ConnectionAlreadyOpen(_internalState);
						}
						_internalState |= ConnectionState.Connecting;
					}
					else {
						if (!IsConnecting) {
							throw new InvalidOperationException("Connection : Impossible to tear down from state " + ConnectionState.Connecting.ToString() + " while in state " + _internalState.ToString());
						}
						_internalState &= ~ConnectionState.Connecting;
					}
				}
			}
		}

		protected virtual PROVIDER_TYPE ProviderType
		{
			get {
				if (JdbcMode != JDBC_MODE.PROVIDER_MODE) {
					return PROVIDER_TYPE.NONE;
				}
				
				string providerStr = ConnectionStringHelper.FindValue(UserParameters,StringManager.GetStringArray("CON_PROVIDER"));
				if (providerStr.StartsWith("SQLOLEDB")) {
					return PROVIDER_TYPE.SQLOLEDB;
				}
				else if (providerStr.StartsWith("MSDAORA")) {
					return PROVIDER_TYPE.MSDAORA;
				}
				else if (providerStr.StartsWith("IBMDADB2")) {
					return PROVIDER_TYPE.IBMDADB2;
				}
				return PROVIDER_TYPE.NONE;
			}
		}

		protected internal virtual JDBC_MODE JdbcMode
		{
			get { 
				string[] conJndiNameStr = StringManager.GetStringArray("CON_JNDI_NAME");
				if ( !String.Empty.Equals(ConnectionStringHelper.FindValue(UserParameters,conJndiNameStr))) {
					return JDBC_MODE.DATA_SOURCE_MODE;
				}

				string[] jdbcDriverStr = StringManager.GetStringArray("JDBC_DRIVER");
				string[] jdbcUrlStr = StringManager.GetStringArray("JDBC_URL");
				bool jdbcDriverSpecified = !String.Empty.Equals(ConnectionStringHelper.FindValue(UserParameters,jdbcDriverStr));
				bool jdbcUrlSpecified = !String.Empty.Equals(ConnectionStringHelper.FindValue(UserParameters,jdbcUrlStr));

				if (jdbcDriverSpecified && jdbcUrlSpecified) {
					return JDBC_MODE.JDBC_DRIVER_MODE;
				}

				string[] providerStr = StringManager.GetStringArray("CON_PROVIDER");
				if (!String.Empty.Equals(ConnectionStringHelper.FindValue(UserParameters,providerStr))) {
					return JDBC_MODE.PROVIDER_MODE;
				}
				
				return JDBC_MODE.NONE;
			}
		}

		protected virtual string JdbcDriverName
		{
			get { return String.Empty; }
		}

		protected abstract DbStringManager StringManager
		{
			get;
		}

		protected virtual string ServerName
		{
			get { return DataSource; }
		}

		protected virtual string CatalogName
		{
			get { return Database; }
		}

		protected virtual string Port
		{
			get {
				string port = ConnectionStringHelper.FindValue(UserParameters, StringManager.GetStringArray("CON_PORT"));
				switch (ProviderType) {
					case PROVIDER_TYPE.SQLOLEDB : 
						if (String.Empty.Equals(port)) {
							// if needed - resolve MSSQL port
							// FIXME : decide about behaviour in the case all the timeout spent on port resolution
							//long start = DateTime.Now.Ticks;
							port = DbPortResolver.getMSSqlPort(DataSource,InstanceName,ConnectionTimeout).ToString();
							//long end = DateTime.Now.Ticks;													
							//if( (end - start) < ConnectionTimeout*1000000) {								
								//timeout -= (int)(end - start)/1000000;
							//}
						}
						// todo : what should we do if all the timeout spent on port resolution ?
						if ("-1".Equals(port)) {
							port = StringManager.GetString("SQL_CON_PORT", "1433"); //default port of MSSql Server 3167.
						}
						ConnectionStringHelper.AddValue(UserParameters,StringManager.GetStringArray("CON_PORT"),port);
						break;
				}
				return port;
			}
		}

		public override string DataSource
		{
			get {
				string dataSource = ConnectionStringHelper.FindValue(UserParameters,StringManager.GetStringArray("CON_DATA_SOURCE"));

				if (ProviderType == PROVIDER_TYPE.SQLOLEDB) {
					int instanceIdx;
					if ((instanceIdx = dataSource.IndexOf("\\")) != -1) {
						// throw out named instance name
						dataSource = dataSource.Substring(0,instanceIdx);
					}

					if (dataSource != null && dataSource.StartsWith("(") && dataSource.EndsWith(")")) {						
						dataSource = dataSource.Substring(1,dataSource.Length - 2);
					}

					if(String.Empty.Equals(dataSource) || (String.Compare("local",dataSource,true) == 0)) {
						dataSource = "localhost";
					}
				}
				return dataSource;
			}
		}

		protected virtual string InstanceName
		{
			get {
				string dataSource = ConnectionStringHelper.FindValue(UserParameters,StringManager.GetStringArray("CON_DATA_SOURCE"));
				string instanceName = String.Empty;
				if (ProviderType == PROVIDER_TYPE.SQLOLEDB) {
					int instanceIdx;
					if ((instanceIdx = dataSource.IndexOf("\\")) == -1) {
						// no named instance specified - use a default name
						instanceName = StringManager.GetString("SQL_DEFAULT_INSTANCE_NAME");
					}
					else {
						// get named instance name
						instanceName = dataSource.Substring(instanceIdx + 1);
					}
				}
				return instanceName;
			}
		}

		protected virtual string User
		{
			get { return ConnectionStringHelper.FindValue(UserParameters,StringManager.GetStringArray("CON_USER_ID")); }
		}

		protected virtual string Password
		{
			get { return ConnectionStringHelper.FindValue(UserParameters,StringManager.GetStringArray("CON_PASSWORD")); }
		}

		protected NameValueCollection UserParameters
		{
			get {
				if (_userParameters == null) {
					_userParameters = ConnectionStringHelper.BuildUserParameters(ConnectionString);
				}
				return _userParameters;
			}
		}

		internal String JdbcUrl 
		{
			get { 
				if ( UserParameters == null) {
					return String.Empty;
				}

				if (_jdbcUrl == null) {
					_jdbcUrl = BuildJdbcUrl();
				}
				return _jdbcUrl;
			}
		}

		internal ConnectionState InternalState
		{
			get	{ return _internalState; }
		}


		protected internal Connection JdbcConnection
		{
			get { return _jdbcConnnection; }
			set { _jdbcConnnection = value; }
		}

		protected virtual string[] ResourceIgnoredKeys
		{
			get { return new string[0]; }
		}

		protected virtual Hashtable SkippedUserParameters
		{
			get { return new Hashtable(new CaseInsensitiveHashCodeProvider(),new CaseInsensitiveComparer()); }
		}

		internal ObjectNameResolver[] SyntaxPatterns
		{
			get {
				if (_syntaxPatterns == null) {
					_syntaxPatterns = ObjectNamesHelper.GetSyntaxPatterns(this);
				}
				return _syntaxPatterns;
			}
		}

		#endregion // Properties

		#region Methods
			// since WS also does not permits dynamically change of login timeout and tomcat does no implements - do not do it at all
			//ds.setLoginTimeout(ConnectionTimeout);

		internal abstract void OnSqlWarning(SQLWarning warning);

		internal abstract void OnStateChanged(ConnectionState orig, ConnectionState current);

		protected abstract SystemException CreateException(SQLException e);

		public override void Close()
		{
			try {
				ClearReferences();
				if (JdbcConnection != null && !JdbcConnection.isClosed()) {
					JdbcConnection.close();
				}
			}
			catch (SQLException e) {
				// suppress exception
				JdbcConnection = null;
#if DEBUG
				Console.WriteLine("Exception catched at Conection.Close() : {0}\n{1}\n{2}",e.GetType().FullName,e.Message,e.StackTrace);
#endif
			}
			catch (Exception e) {
				// suppress exception
				JdbcConnection = null;
#if DEBUG
				Console.WriteLine("Exception catched at Conection.Close() : {0}\n{1}\n{2}",e.GetType().FullName,e.Message,e.StackTrace);
#endif
			}
			finally {
				lock(_internalStateSync) {
					_internalState = ConnectionState.Closed;
				}
			}
		}

		protected internal virtual void CopyTo(AbstractDBConnection target)
		{
			target._connectionString = _connectionString;
		}

		internal protected virtual void OnSqlException(SQLException exp)
		{
			throw CreateException(exp);
		}

		internal void AddReference(object referencedObject)
		{	lock(_referencedObjects.SyncRoot) {
				_referencedObjects.Add(new WeakReference(referencedObject));
			}
		}

		internal void RemoveReference(object referencedObject)
		{
			lock(_referencedObjects.SyncRoot) {
				for(int i = 0; i < _referencedObjects.Count; i++) {
					WeakReference wr = (WeakReference) _referencedObjects[i];
					if (wr.IsAlive && (wr.Target == referencedObject)) {
						_referencedObjects.RemoveAt(i);
					}
				}
			}
		}

		private void ClearReferences()
		{
			ArrayList oldList = _referencedObjects;
			_referencedObjects = new ArrayList();

			for(int i = 0; i < oldList.Count; i++) {
				WeakReference wr = (WeakReference) oldList[i];
				if (wr.IsAlive) {
					ClearReference(wr.Target);
				}
			}
		}

		private void ClearReference(object referencedObject)
		{
			try {
				if (referencedObject is AbstractDbCommand) {
					((AbstractDbCommand)referencedObject).CloseInternal();
				}
				else if (referencedObject is AbstractDataReader) {
					((AbstractDataReader)referencedObject).CloseInternal();
				}
			}
			catch (SQLException) {
				// suppress exception since it's possible that command or reader are in inconsistent state
			}
		}

		public override void Open()
		{
			if (_connectionString == null || _connectionString.Length == 0) {
				throw ExceptionHelper.ConnectionStringNotInitialized();
			}

			IsConnecting = true;
			try {			
				if (JdbcConnection != null && !JdbcConnection.isClosed()) {
					throw ExceptionHelper.ConnectionAlreadyOpen(_internalState);
				}
	
				switch(JdbcMode) {
					case JDBC_MODE.DATA_SOURCE_MODE :
						JdbcConnection = GetConnectionFromDataSource();
						break;

					case JDBC_MODE.JDBC_DRIVER_MODE:
						JdbcConnection = GetConnectionFromJdbcDriver();
						break;

					case JDBC_MODE.PROVIDER_MODE : 					
						JdbcConnection = GetConnectionFromProvider();
						break;
				}
				IsOpened = true;

				OnStateChanged(ConnectionState.Closed, ConnectionState.Open);
			}
			catch (SQLWarning warning) {
				OnSqlWarning(warning);
			}
			catch (SQLException exp) {
				OnSqlException(exp);
			}
			finally {
				IsConnecting = false;
			}
		}

		public override void ChangeDatabase(String database)
		{
			IsConnecting = true;
			try {
				ClearReferences();
				Connection con = JdbcConnection;				
				con.setCatalog(database);
				ConnectionStringHelper.UpdateValue(UserParameters,StringManager.GetStringArray("CON_DATABASE"),database);
			}
			catch (SQLWarning warning) {
				OnSqlWarning(warning);
			}
			catch (SQLException exp) {
				throw CreateException(exp);
			}
			finally {
				IsConnecting = false;
			}
		}

		public override string ServerVersion {
			get {
				// only if the driver support this methods
				try {
					if (JdbcConnection == null)
						return String.Empty;

					DatabaseMetaData metaData = JdbcConnection.getMetaData();
					return metaData.getDatabaseProductVersion();
				}
				catch (SQLException exp) {
					throw CreateException(exp);
				}
			}
		}

		internal string JdbcProvider {
			get {
				// only if the driver support this methods
				try {
					if (JdbcConnection == null)
						return String.Empty;

					DatabaseMetaData metaData = JdbcConnection.getMetaData();
					return metaData.getDriverName() + " " + metaData.getDriverVersion();
				}
				catch (SQLException exp) {
					return String.Empty; //suppress
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				try {
					if (JdbcConnection != null && !JdbcConnection.isClosed()) {
						JdbcConnection.close();
					}	                
					JdbcConnection = null;
				}
				catch (java.sql.SQLException exp) {
					throw CreateException(exp);
				}
			}
			base.Dispose(disposing);
		}

		protected internal virtual void ValidateConnectionString(string connectionString)
		{
			JDBC_MODE currentJdbcMode = JdbcMode;
			
			if (currentJdbcMode == JDBC_MODE.NONE) {
				string[] jdbcDriverStr = StringManager.GetStringArray("JDBC_DRIVER");
				string[] jdbcUrlStr = StringManager.GetStringArray("JDBC_URL");
				bool jdbcDriverSpecified = !String.Empty.Equals(ConnectionStringHelper.FindValue(UserParameters,jdbcDriverStr));
				bool jdbcUrlSpecified = !String.Empty.Equals(ConnectionStringHelper.FindValue(UserParameters,jdbcUrlStr));

				if (jdbcDriverSpecified ^ jdbcUrlSpecified) {
					throw new ArgumentException("Invalid format of connection string. If you want to use third-party JDBC driver, the format is: \"JdbcDriverClassName=<jdbc driver class name>;JdbcURL=<jdbc url>\"");
				}				
			}
		}

		protected virtual string BuildJdbcUrl()
		{
			switch (JdbcMode) {
				case JDBC_MODE.JDBC_DRIVER_MODE :
					return ConnectionStringHelper.FindValue(UserParameters,StringManager.GetStringArray("JDBC_URL"));
				default :
					return String.Empty;
			}
		}

		protected java.util.Properties BuildProperties()
		{
			java.util.Properties properties = new java.util.Properties();

			string user = User;
			if (user != null && user.Length > 0)
				properties.put("user", user);
			string password = Password;
			if (user != null && user.Length > 0)
				properties.put("password", password);

			string[] userKeys = UserParameters.AllKeys;

			for(int i=0; i < userKeys.Length; i++) {
				string userKey = userKeys[i];
				string userParameter = UserParameters[userKey];
				if (!SkipUserParameter(userKey)) {
					properties.put(userKey,userParameter);
				}
			}
			return properties;
		}

		protected virtual bool SkipUserParameter(string parameterName)
		{
			if (SkippedUserParameters.Count == 0) {
				// skipped parameters not initialized - skip all
				return true;
			}

			return SkippedUserParameters.Contains(parameterName);
		}

		protected virtual void InitializeSkippedUserParameters()
		{
			if (SkippedUserParameters.Count > 0) {
				return;
			}

			for(int i=0; i < ResourceIgnoredKeys.Length; i++) {
				string[] userKeys = StringManager.GetStringArray(ResourceIgnoredKeys[i]);
				for(int j=0; j < userKeys.Length; j++) {
					SkippedUserParameters.Add(userKeys[j],userKeys[j]);
				}
			}
		}
 
		internal void ValidateBeginTransaction()
		{
			if (State != ConnectionState.Open) {
				throw new InvalidOperationException(Res.GetString("ADP_OpenConnectionRequired_BeginTransaction", new object[] {"BeginTransaction", State}));
			}

			if (!JdbcConnection.getAutoCommit()) {
				throw new System.InvalidOperationException("Parallel transactions are not supported.");
			}
		}

		internal virtual Connection GetConnectionFromProvider()
		{
			ActivateJdbcDriver(JdbcDriverName);
			DriverManager.setLoginTimeout(ConnectionTimeout);
			java.util.Properties properties = BuildProperties();
			return DriverManager.getConnection (JdbcUrl, properties);
		}

		internal Connection GetConnectionFromDataSource()
		{
			string dataSourceJndi = ConnectionStringHelper.FindValue(UserParameters, StringManager.GetStringArray("CON_JNDI_NAME"));
			string namingProviderUrl = ConnectionStringHelper.FindValue(UserParameters, StringManager.GetStringArray("CON_JNDI_PROVIDER"));
			string namingFactoryInitial = ConnectionStringHelper.FindValue(UserParameters, StringManager.GetStringArray("CON_JNDI_FACTORY"));
			DataSource ds = _dataSourceCache.GetDataSource(dataSourceJndi,namingProviderUrl,namingFactoryInitial);
			try {
				ds.setLoginTimeout(ConnectionTimeout);
			}
			catch (java.lang.Exception) {
				// WebSphere does not allows dynamicall change of login timeout
				// setLoginTimeout is not supported yet
				// in Tomcat data source.
				// In this case we work wthout timeout.
			}
			return ds.getConnection();
		}

		internal virtual Connection GetConnectionFromJdbcDriver()
		{
			string[] jdbcDriverStr = StringManager.GetStringArray("JDBC_DRIVER");
			string[] jdbcUrlStr = StringManager.GetStringArray("JDBC_URL");
		
			string jdbcDriverName = ConnectionStringHelper.FindValue(UserParameters,jdbcDriverStr);
			string jdbcUrl = ConnectionStringHelper.FindValue(UserParameters,jdbcUrlStr);

			ActivateJdbcDriver(jdbcDriverName);
			DriverManager.setLoginTimeout(ConnectionTimeout);

			java.util.Properties properties = BuildProperties();

			return DriverManager.getConnection(jdbcUrl,properties);
		}

		internal ArrayList GetProcedureColumns(String procedureString, AbstractDbCommand command)
		{
			ArrayList col = new ArrayList();
			try {
				ObjectNameResolver[] nameResolvers = SyntaxPatterns;
				ResultSet res = null;
				string catalog = null;
				string schema = null;
				string spname = null;
						
				DatabaseMetaData metadata = JdbcConnection.getMetaData();	
				bool storesUpperCaseIdentifiers = false;
				bool storesLowerCaseIdentifiers = false;
				try {
					storesUpperCaseIdentifiers = metadata.storesUpperCaseIdentifiers();
					storesLowerCaseIdentifiers = metadata.storesLowerCaseIdentifiers();
				}
				catch (SQLException e) {
					// suppress
				}

				for(int i=0; i < nameResolvers.Length; i++) {
					ObjectNameResolver nameResolver = nameResolvers[i];
					Match match = nameResolver.Match(procedureString);

					if (match.Success) {
						spname = ObjectNameResolver.GetName(match);				
						schema = ObjectNameResolver.GetSchema(match);						
						catalog = ObjectNameResolver.GetCatalog(match);						

						// make all identifiers uppercase or lowercase according to database metadata
						if (storesUpperCaseIdentifiers) {
							spname = (spname.Length > 0) ? spname.ToUpper() : null;
							schema = (schema.Length > 0) ? schema.ToUpper() : null;
							catalog = (catalog.Length > 0) ? catalog.ToUpper() : null;
						}
						else if (storesLowerCaseIdentifiers) {
							spname = (spname.Length > 0) ? spname.ToLower() : null;
							schema = (schema.Length > 0) ? schema.ToLower() : null;
							catalog = (catalog.Length > 0) ? catalog.ToLower() : null;
						}
						else {
							spname = (spname.Length > 0) ? spname : null;
							schema = (schema.Length > 0) ? schema : null;
							catalog = (catalog.Length > 0) ? catalog : null;
						}

						// catalog from db is always in correct caps
						if (catalog == null) {
							catalog = JdbcConnection.getCatalog();
						}

						try {
							// always get the first procedure that db returns
							res = metadata.getProcedures(catalog, schema, spname);												
							if (res.next()) {
								catalog = res.getString(1);
								schema = res.getString(2);
								spname = res.getString(3);
								break;
							}

							spname = null;
						}
						catch { // suppress exception
							return null;
						}
						finally {
							if (res != null) {
								res.close();
							}
						}
					}
				}	
		
				if (spname == null || spname.Length == 0) {
					return null;
				}
				
				try {
					// get procedure columns based o  procedure metadata
					res = metadata.getProcedureColumns(catalog, schema, spname, null);				
					while (res.next()) {
						// since there is still a possibility that some of the parameters to getProcedureColumn were nulls, 
						// we need to filter the results with strict matching
						if ((res.getString(1) != catalog ) || (res.getString(2) != schema) || (res.getString(3) != spname)) {
							continue;
						}

						AbstractDbParameter parameter = (AbstractDbParameter)command.CreateParameter();
						
						parameter.SetParameterName(res);
						parameter.SetParameterDbType(res);
						parameter.SetSpecialFeatures(res);

						//get parameter direction
						short direction = res.getShort("COLUMN_TYPE");
						if(direction == 1) //DatabaseMetaData.procedureColumnIn
							parameter.Direction = ParameterDirection.Input;
						else if(direction == 2) //DatabaseMetaData.procedureColumnInOut
							parameter.Direction = ParameterDirection.InputOutput;
						else if(direction == 4) //DatabaseMetaData.procedureColumnOut
							parameter.Direction = ParameterDirection.Output;
						else if(direction == 5) //DatabaseMetaData.procedureColumnReturn
							parameter.Direction = ParameterDirection.ReturnValue;
					
						//get parameter precision and scale
						parameter.SetParameterPrecisionAndScale(res);

						parameter.SetParameterSize(res);
						parameter.SetParameterIsNullable(res);

						col.Add(parameter);
					}
				}
				finally {
					if (res != null) {
						res.close();
					}
				}				
			}
			catch(Exception e) {
				//supress
#if DEBUG
				Console.WriteLine("Exception catched at AbstractDBConnection.GetProcedureColumns() : {0}\n{1}\n{2}",e.GetType().FullName,e.Message,e.StackTrace);
#endif
			}
			return col;
		}

		protected static void ActivateJdbcDriver(string driver)
		{
			if(driver != null) {
				try {
					java.lang.Class.forName(driver).newInstance();
				}
				catch (java.lang.ClassNotFoundException e) {
					throw new TypeLoadException(e.Message);
				}
				catch (java.lang.InstantiationException e) {
					throw new MemberAccessException(e.Message);
				}
                catch (java.lang.IllegalAccessException e) {
					throw new MissingMethodException(e.Message);
				}
			}
		}

		protected String BuildMsSqlUrl()
		{
			return StringManager.GetString("SQL_JDBC_URL") //"jdbc:microsoft:sqlserver://"
				+ ServerName + ":" + Port + ";DatabaseName=" + CatalogName;
		}

		#endregion // Methods	
	}
}