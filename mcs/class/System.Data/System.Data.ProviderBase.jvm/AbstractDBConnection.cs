//
// System.Data.Common.AbstractDBConnection
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

using System.Globalization;
using System.Data;
using System.Data.ProviderBase;
using System.Data.Configuration;
using System.Configuration;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using Mainsoft.Data.Jdbc.Providers;
using System.Data.Common;

using java.sql;
using javax.sql;
using javax.naming;

using Mainsoft.Data.Configuration;

namespace System.Data.ProviderBase
{
	public abstract class AbstractDBConnection : DbConnection, ICloneable
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

				ObjectNameResolversCollection basic = (ObjectNameResolversCollection) ConfigurationSettings.GetConfig ("Mainsoft.Data.Configuration/objectnameresolution");
				
				java.sql.DatabaseMetaData metaData = connection.JdbcConnection.getMetaData();
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
		
		#region Fields

		private const int DEFAULT_TIMEOUT = 15;

		private java.sql.Connection _jdbcConnnection;
		private ConnectionState _internalState;
		private object _internalStateSync = new object();

		private string _connectionString = String.Empty;
		IConnectionStringDictionary _connectionStringBuilder;
		IConnectionProvider			_connectionProvider;

		private ArrayList _referencedObjects = new ArrayList();	
		private ObjectNameResolver[] _syntaxPatterns;

		#endregion // Fields

		#region Constructors

		public AbstractDBConnection(string connectionString)
		{
			_connectionString = connectionString;
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
				_connectionProvider = null;
				_connectionStringBuilder = null;
			}
		}

		public override int ConnectionTimeout
		{
			get {
				string timeoutStr = (string)ConnectionStringBuilder["loginTimeout"];
				if (timeoutStr != null && timeoutStr.Length > 0) {
					try {
						return Convert.ToInt32(timeoutStr);
					}
					catch(FormatException) {
						throw ExceptionHelper.InvalidValueForKey("connect timeout");
					}
					catch (OverflowException) {
						throw ExceptionHelper.InvalidValueForKey("connect timeout");
					}
				}
				return DEFAULT_TIMEOUT;
			}
		}

		public override String Database
		{
			get { 
				if ((State & ConnectionState.Open) != 0)
					return JdbcConnection.getCatalog();

				return (string)ConnectionStringBuilder["DATABASE"];
			}
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

		public override string DataSource
		{
			get {
				return (string)ConnectionStringBuilder["SERVERNAME"];
			}
		}

		internal ConnectionState InternalState
		{
			get	{ return _internalState; }
		}


		protected internal java.sql.Connection JdbcConnection
		{
			get { return _jdbcConnnection; }
			set { _jdbcConnnection = value; }
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

		protected internal IConnectionProvider ConnectionProvider { 
			get {
				try {
					if (_connectionProvider == null)
						_connectionProvider = GetConnectionProvider();

					return _connectionProvider;
				}
				catch(SQLException exp) {
					throw CreateException(exp);
				}
			}
		}
		protected internal IConnectionStringDictionary ConnectionStringBuilder {
			get {
				try {
					if (_connectionStringBuilder == null)
						_connectionStringBuilder = ConnectionProvider.GetConnectionStringBuilder(ConnectionString);

					return _connectionStringBuilder;
				}
				catch(SQLException exp) {
					throw CreateException(exp);
				}
			}
		}
		protected abstract IConnectionProvider GetConnectionProvider();

		static protected IConnectionProvider GetConnectionProvider(string sectionMame, string provider) {
			if (provider == null)
				throw new ArgumentNullException("provider");

			IList providers = (IList) ConfigurationSettings.GetConfig(sectionMame);
			if (providers.Count == 0)
				throw new ArgumentException("Configuration section is empty.", "sectionName");

			for (int i = 0; i < providers.Count; i++) {
				IDictionary providerInfo = (IDictionary) providers[i];
					
				string curProvider = (string)providerInfo[ConfigurationConsts.Name];
				if (String.Compare(provider, 0, curProvider, 0, provider.Length, StringComparison.OrdinalIgnoreCase) == 0) {
					string providerType = (string) providerInfo [ConfigurationConsts.ProviderType];
					if (providerType == null || providerType.Length == 0)
						return new GenericProvider (providerInfo); 
					else {
						Type t = Type.GetType (providerType);
						return (IConnectionProvider) Activator.CreateInstance (t , new object[] {providerInfo});
					}
				}
			}

			throw new ArgumentException(
				String.Format("Unknown provider name '{0}'", provider), "ConnectionString");
		}

		#endregion // Properties

		#region Methods
			// since WS also does not permits dynamically change of login timeout and tomcat does no implements - do not do it at all
			//ds.setLoginTimeout(ConnectionTimeout);

		protected internal abstract void OnSqlWarning(SQLWarning warning);

		protected abstract SystemException CreateException(SQLException e);

		protected abstract SystemException CreateException(string message);

		public override void Close()
		{
			ConnectionState orig = State;
			try {
				ClearReferences();
				if (JdbcConnection != null && !JdbcConnection.isClosed()) {
					if (!JdbcConnection.getAutoCommit())
						JdbcConnection.rollback();
					JdbcConnection.close();
				}
			}
			catch (Exception e) {
				// suppress exception
#if DEBUG
				Console.WriteLine("Exception catched at Conection.Close() : {0}\n{1}\n{2}",e.GetType().FullName,e.Message,e.StackTrace);
#endif
			}
			finally {
				JdbcConnection = null;
				lock(_internalStateSync) {
					_internalState = ConnectionState.Closed;
				}
			}

			ConnectionState current = State;
			if (current != orig)
				OnStateChange (new StateChangeEventArgs (orig, current));
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

				JdbcConnection = ConnectionProvider.GetConnection (ConnectionStringBuilder);

				IsOpened = true;

				OnStateChange (new StateChangeEventArgs (ConnectionState.Closed, ConnectionState.Open));
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
				java.sql.Connection con = JdbcConnection;				
				con.setCatalog(database);
//				ConnectionStringHelper.UpdateValue(UserParameters,StringManager.GetStringArray("CON_DATABASE"),database);
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

					java.sql.DatabaseMetaData metaData = JdbcConnection.getMetaData();
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

					java.sql.DatabaseMetaData metaData = JdbcConnection.getMetaData();
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

		internal void ValidateBeginTransaction()
		{
			if (State != ConnectionState.Open) {
				throw new InvalidOperationException(String.Format("{0} requires an open and available Connection. The connection's current state is {1}.", new object[] {"BeginTransaction", State}));
			}

			if (!JdbcConnection.getAutoCommit()) {
				throw new System.InvalidOperationException("Parallel transactions are not supported.");
			}
		}

		internal ArrayList GetProcedureColumns(String procedureString, AbstractDbCommand command)
		{
			ArrayList col = new ArrayList();
			try {
				ObjectNameResolver[] nameResolvers = SyntaxPatterns;
				java.sql.ResultSet res = null;
				string catalog = null;
				string schema = null;
				string spname = null;
						
				java.sql.DatabaseMetaData metadata = JdbcConnection.getMetaData();	
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

		#endregion // Methods	

		#region ICloneable Members

		public virtual object Clone() {
			AbstractDBConnection con  = (AbstractDBConnection)MemberwiseClone();
			con._internalState = ConnectionState.Closed;
			con._internalStateSync = new object();
			con._jdbcConnnection = null;
			con._referencedObjects = new ArrayList();
			con._syntaxPatterns = null;
			return con;
		}

		#endregion
	}
}
