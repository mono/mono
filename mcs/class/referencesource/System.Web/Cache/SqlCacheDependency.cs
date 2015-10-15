//------------------------------------------------------------------------------
// <copyright file="SqlCacheDepModule.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * SqlCacheDepModule
 * 
 * Copyright (c) 1998-1999, Microsoft Corporation
 * 
 */

namespace System.Web.Caching {

    using System;
    using System.Threading;
    using System.Collections;
    using System.Configuration;
    using System.IO;
    using System.Web.Caching;
    using System.Web.Util;
    using System.Web.Configuration;
    using System.Xml;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Text;
    using System.Runtime.InteropServices;
    using System.EnterpriseServices;
    using System.Web.UI;
    using System.Web.DataAccess;
    using System.Security.Principal;
    using System.Web.Hosting;
    using System.Runtime.Serialization;
    using System.Web.Management;
    using System.Security;
    
    public sealed class SqlCacheDependency : CacheDependency {

        internal static bool        s_hasSqlClientPermission;
        internal static bool        s_hasSqlClientPermissionInited;

        const string SQL9_CACHE_DEPENDENCY_DIRECTIVE = "CommandNotification";
        internal const string SQL9_OUTPUT_CACHE_DEPENDENCY_COOKIE = "MS.SqlDependencyCookie";
        
        SqlDependency       _sqlYukonDep;       // SqlDependency for Yukon
        DatabaseNotifState  _sql7DatabaseState;     // Database state for SQL7/2000
        string              _uniqueID;              // used by HttpCachePolicy for the ETag
#if DBG
        bool                _isUniqueIDInitialized;
#endif

        struct Sql7DependencyInfo {
            internal string  _database;
            internal string  _table;
        }

        // For generating Unique Id
        Sql7DependencyInfo  _sql7DepInfo;  
        int                 _sql7ChangeId;
        

        // For Non-SQL9 SQL servers, we create a dependency based on an internal cached item.        

        public SqlCacheDependency(string databaseEntryName, string tableName) 
        :base(0, null, new string[1] {GetDependKey(databaseEntryName, tableName)})
        {
            Debug.Trace("SqlCacheDependency", 
                            "Depend on key=" + GetDependKey(databaseEntryName, tableName) + "; value=" +
                            HttpRuntime.CacheInternal[GetDependKey(databaseEntryName, tableName)]);

            // Permission checking is done in GetDependKey()

            _sql7DatabaseState = SqlCacheDependencyManager.AddRef(databaseEntryName);
            _sql7DepInfo._database = databaseEntryName;
            _sql7DepInfo._table = tableName;

            object o = HttpRuntime.CacheInternal[GetDependKey(databaseEntryName, tableName)];
            if (o == null) {
                // If the cache entry can't be found, this cache dependency will be set to CHANGED already.
                _sql7ChangeId = -1;
            }
            else {
                // Note that if the value in the cache changed between the base ctor and here, even though
                // we get a wrong unqiue Id, but it's okay because that change will cause the CacheDependency's
                // state to become CHANGED and any cache operation using this CacheDependency will fail anyway.
                _sql7ChangeId = (int)o;
            }

            // The ctor of every class derived from CacheDependency must call this.
            FinishInit();

            InitUniqueID();
        }

        protected override void DependencyDispose() {
            if (_sql7DatabaseState != null) {
                SqlCacheDependencyManager.Release(_sql7DatabaseState);
            }
        }

        // For SQL9, we use SqlDependency
        public SqlCacheDependency(SqlCommand sqlCmd) {
            HttpContext context = HttpContext.Current;

            if (sqlCmd == null) {
                throw new ArgumentNullException("sqlCmd");
            }

            // Prevent a conflict between using SQL9 outputcache and an explicit 
            // SQL9 SqlCacheDependency at the same time.  See VSWhidey 396429 and
            // the attached email in the 
            if (context != null && context.SqlDependencyCookie != null &&  // That means We have already setup SQL9 dependency for output cache
                sqlCmd.NotificationAutoEnlist) {    // This command will auto-enlist in that output cache dependency
                throw new HttpException(SR.GetString(SR.SqlCacheDependency_OutputCache_Conflict));
            }
            
            CreateSqlDep(sqlCmd);

            InitUniqueID();
        }

        void InitUniqueID() {
            if (_sqlYukonDep != null) {
                // Yukon does not provide us with an ID, so we'll use a Guid.
                _uniqueID = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
            }
            else if (_sql7ChangeId == -1) {
                // The database/tablen entry can't be found in the cache.  That means SQL doesn't have
                // this database/table registered for sql cache dependency.  In this case, we can't 
                // generate a unique id.
                _uniqueID = null;
            }
            else {
                _uniqueID = _sql7DepInfo._database + ":" + _sql7DepInfo._table + ":" + _sql7ChangeId.ToString(CultureInfo.InvariantCulture);
            }
#if DBG            
            _isUniqueIDInitialized = true;
#endif
        }

        public override string GetUniqueID() {
#if DBG
            Debug.Assert(_isUniqueIDInitialized == true, "_isUniqueIDInitialized == true");
#endif
            return _uniqueID;
        }
        
        private static void CheckPermission() {
            if (!s_hasSqlClientPermissionInited) {
                if (!System.Web.Hosting.HostingEnvironment.IsHosted) {
                    try {
                        new SqlClientPermission(PermissionState.Unrestricted).Demand();
                        s_hasSqlClientPermission = true;
                    } 
                    catch (SecurityException) {}
                }
                else {
                    s_hasSqlClientPermission = Permission.HasSqlClientPermission();
                }

                s_hasSqlClientPermissionInited = true;
            }
            
            if (!s_hasSqlClientPermission) {
                throw new HttpException(SR.GetString(SR.SqlCacheDependency_permission_denied));
            }
        }

        void OnSQL9SqlDependencyChanged(Object sender, SqlNotificationEventArgs e) {
            Debug.Trace("SqlCacheDependency", "SQL9 dependency changed: depId=" + _sqlYukonDep.Id);
            NotifyDependencyChanged(sender, e);
        }

        private SqlCacheDependency() {
            CreateSqlDep(null);
            InitUniqueID();
        }

        void CreateSqlDep(SqlCommand sqlCmd) {
            _sqlYukonDep = new SqlDependency();

            // Note: sqlCmd is null in output cache case.
            
            if (sqlCmd != null) {
                Debug.Trace("SqlCacheDependency", "SqlCmd added to SqlDependency object");
                _sqlYukonDep.AddCommandDependency(sqlCmd);
            }
            
            _sqlYukonDep.OnChange += new OnChangeEventHandler(OnSQL9SqlDependencyChanged);
            
            Debug.Trace("SqlCacheDependency", "SQL9 dependency created: depId=" + _sqlYukonDep.Id);
        }

        internal static void ValidateOutputCacheDependencyString(string depString, bool page) {
            if (depString == null) {
                throw new HttpException(SR.GetString(SR.Invalid_sqlDependency_argument, depString));
            }

            if (StringUtil.EqualsIgnoreCase(depString, SQL9_CACHE_DEPENDENCY_DIRECTIVE)) {
                if (!page) {
                    // It's impossible for only a page, but not its controls, to use Yukon Cache Dependency; neither
                    // can the opposite scenario possible.  It's because once we create a SqlDependency and
                    // stick it to the context, it's complicated (but not impossible) to clear it when rendering
                    // the parts (either a page or a control) that doesn't depend on Yukon.
                    // To keep things simple, we restrict Yukon Cache Dependency only to page.
                    throw new HttpException(
                        SR.GetString(SR.Attrib_Sql9_not_allowed));
                }
            }
            else {
                // It's for non-SQL 9 scenario.
                ParseSql7OutputCacheDependency(depString);
            }
        }

        public static CacheDependency CreateOutputCacheDependency(string dependency) {
            if (dependency == null) {
                throw new HttpException(SR.GetString(SR.Invalid_sqlDependency_argument, dependency));
            }
            
            if (StringUtil.EqualsIgnoreCase(dependency, SQL9_CACHE_DEPENDENCY_DIRECTIVE)) {
                HttpContext context = HttpContext.Current;
                Debug.Assert(context != null);
                
                SqlCacheDependency  dep = new SqlCacheDependency();

                Debug.Trace("SqlCacheDependency", "Setting depId=" + dep._sqlYukonDep.Id);
                context.SqlDependencyCookie = dep._sqlYukonDep.Id;

                return dep;
            }
            else {
                ArrayList                   sqlDependencies;
                AggregateCacheDependency    aggr = null;
                Sql7DependencyInfo           info;
              
                sqlDependencies = ParseSql7OutputCacheDependency(dependency);

                // ParseSql7OutputCacheDependency will throw if we cannot find a single entry
                Debug.Assert(sqlDependencies.Count > 0, "sqlDependencies.Count > 0");

                Debug.Trace("SqlCacheDependency", "Creating SqlCacheDependency for SQL8 output cache");

                if (sqlDependencies.Count == 1) {
                    info = (Sql7DependencyInfo)sqlDependencies[0];
                    return CreateSql7SqlCacheDependencyForOutputCache(info._database, info._table, dependency);
                }

                aggr = new AggregateCacheDependency();
                
                for(int i=0; i < sqlDependencies.Count; i++) {
                    info = (Sql7DependencyInfo)sqlDependencies[i];
                    aggr.Add(CreateSql7SqlCacheDependencyForOutputCache(info._database, info._table, dependency));
                }

                return aggr;
            }
        }

        static SqlCacheDependency CreateSql7SqlCacheDependencyForOutputCache(string database, string table, string depString) {
            try {
                return new SqlCacheDependency(database, table);
            }
            catch (HttpException e) {
                HttpException outerException = new HttpException(
                       SR.GetString(SR.Invalid_sqlDependency_argument2, depString, e.Message), e);
                
                outerException.SetFormatter(new UseLastUnhandledErrorFormatter(outerException));
                
                throw outerException;
            }
        }

        static string GetDependKey(string database, string tableName) {

            // This is called by ctor SqlCacheDependency(string databaseEntryName, string tableName)
            // before the body of that ctor is executed.  So we have to make sure the app has
            // the right permission here.
            CheckPermission();

            // First is to check whether Sql cache polling is enabled in config or not.
            if (database == null) {
                throw new ArgumentNullException("database");
            }
            
            if (tableName == null) {
                throw new ArgumentNullException("tableName");
            }

            if (tableName.Length == 0) {
                throw new ArgumentException(SR.GetString(SR.Cache_null_table));
            }

            string  monitorKey = SqlCacheDependencyManager.GetMoniterKey(database, tableName);
            
            // Make sure the table is already registered with the database and
            // we've polled the database at least once, so that there is an
            // entry in the cache.
            SqlCacheDependencyManager.EnsureTableIsRegisteredAndPolled(database, tableName);
            return monitorKey;
        }

        static string VerifyAndRemoveEscapeCharacters(string s) {
            int     i;
            bool    escape = false;

            for (i=0; i < s.Length; i++) {
                if (escape) {
                    if (s[i] != '\\' && s[i] != ':' && s[i] != ';') {
                        // Only '\\', '\:' and '\;' are allowed
                        throw new ArgumentException();
                    }
                    escape = false;
                    continue;
                }
                
                if (s[i] == '\\') {
                    if (i+1 == s.Length) {
                        // No character following escape char
                        throw new ArgumentException();
                    }
                    escape = true;
                    s = s.Remove(i, 1);
                    i--;
                }
            }

            return s;
        }

        internal static ArrayList ParseSql7OutputCacheDependency(string outputCacheString) {
            // The database and the table name are separated by a ":".  If the name
            // contains a ":" character, specify it by doing "\:"
            // Pairs of entries are separated by a ";"
            bool        escape = false;
            int         iDatabaseStart = 0;
            int         iTableStart = -1;
            string      database = null;        // The database portion of the pair
            ArrayList   dependencies = null;
            int         len;
            Sql7DependencyInfo   info;

            try {
                for (int i = 0; i < outputCacheString.Length+1; i++) {
                    if (escape) {
                        escape = false;
                        continue;
                    }

                    if (i != outputCacheString.Length && outputCacheString[i] == '\\') {
                        escape = true;
                        continue;
                    }

                    // We have reached ';' or the end of the string
                    if (i == outputCacheString.Length || outputCacheString[i] == ';' ) {
                        if (database==null) {
                            // No database name
                            throw new ArgumentException();
                        }

                        // Get the lenght of the table portion
                        len = i - iTableStart;
                        if (len == 0) {
                            // No table name
                            throw new ArgumentException();
                        }

                        info = new Sql7DependencyInfo();
                        info._database = VerifyAndRemoveEscapeCharacters(database);
                        info._table = VerifyAndRemoveEscapeCharacters(outputCacheString.Substring(iTableStart, len));

                        if (dependencies == null) {
                            dependencies = new ArrayList(1);
                        }

                        dependencies.Add(info);

                        // Reset below values.  We are searching for the next pair.
                        iDatabaseStart = i+1;
                        database = null;
                    }

                    // Have we reached the end of the string?
                    if (i == outputCacheString.Length) {
                        break;
                    }
                    
                    if (outputCacheString[i] == ':') {
                        if (database != null) {
                            // We have already got the database portion
                            throw new ArgumentException();
                        }

                        // Do we get the database part?
                        len = i - iDatabaseStart;
                        if (len == 0) {
                            // No database name
                            throw new ArgumentException();
                        }

                        database = outputCacheString.Substring(iDatabaseStart, len);
                        iTableStart = i+1;
                        continue;
                    }
                }

                return dependencies;
                
            }
            catch (ArgumentException) {
                throw new ArgumentException(SR.GetString(SR.Invalid_sqlDependency_argument, outputCacheString));
            }
        }
    }


    [Serializable()]
    public sealed class DatabaseNotEnabledForNotificationException : SystemException {

        public DatabaseNotEnabledForNotificationException() {
        }
        

        public DatabaseNotEnabledForNotificationException(String message)
        : base(message) {
        }


        public DatabaseNotEnabledForNotificationException(string message, Exception innerException)
        : base (message, innerException) {
        }
        

        internal DatabaseNotEnabledForNotificationException(SerializationInfo info, StreamingContext context) 
        : base(info, context) {
        }
    
    }


    [Serializable()]
    public sealed class TableNotEnabledForNotificationException : SystemException {

        public TableNotEnabledForNotificationException() {
        }
        

        public TableNotEnabledForNotificationException(String message)
        : base(message) {
        }


        public TableNotEnabledForNotificationException(string message, Exception innerException)
        : base (message, innerException) {
        }
        

        internal TableNotEnabledForNotificationException(SerializationInfo info, StreamingContext context) 
        : base(info, context) {
        }
    
    }

    // A class to store the state of a timer for a specific database
    internal class DatabaseNotifState : IDisposable {
        internal string         _database;
        internal string         _connectionString;
        internal int            _rqInCallback;
        internal bool           _notifEnabled;     // true means the ChangeNotif table was found in the database
        internal bool           _init;      // true means timer callback was called at least once
        internal Timer          _timer;
        internal Hashtable      _tables; // Names of all the tables registered for notification
        internal Exception      _pollExpt;
        internal int            _pollSqlError;
        internal SqlConnection  _sqlConn;
        internal SqlCommand     _sqlCmd;
        internal bool           _poolConn;
        internal DateTime       _utcTablesUpdated; // Time when _tables was last updated
        internal int            _refCount = 0;
        
        public void Dispose() {
            if (_sqlConn != null) {
                _sqlConn.Close();
                _sqlConn = null;
            }

            if (_timer != null) {
                _timer.Dispose();
                _timer = null;
            }
        }

        internal DatabaseNotifState(string database, string connection, int polltime) {
            _database = database;
            _connectionString = connection;
            _timer = null;
            _tables = new Hashtable();
            _pollExpt = null;
            _utcTablesUpdated = DateTime.MinValue;

            // We will pool the connection if the polltime is less than 5 s.
            if (polltime <= 5000) {
                _poolConn = true;
            }

        }
        
        internal void GetConnection(out SqlConnection sqlConn, out SqlCommand sqlCmd) {
            sqlConn = null;
            sqlCmd = null;

            // !!! Please note that GetConnection and ReleaseConnection does NOT support
            // multithreading.  The caller must do the locking.

            if (_sqlConn != null) {
                // We already have a pooled connection.
                Debug.Assert(_poolConn, "_poolConn");
                Debug.Assert(_sqlCmd != null, "_sqlCmd != null");

                sqlConn = _sqlConn;
                sqlCmd = _sqlCmd;

                _sqlConn = null;
                _sqlCmd = null;
            }
            else {
                SqlConnectionHolder holder = null;
                
                try {
                    holder = SqlConnectionHelper.GetConnection(_connectionString, true);
                    
                    sqlCmd = new SqlCommand(SqlCacheDependencyManager.SQL_POLLING_SP_DBO, holder.Connection);
                    
                    sqlConn = holder.Connection;
                }
                catch {
                    if (holder != null) {
                        holder.Close();
                        holder = null;
                    }

                    sqlCmd = null;

                    throw;
                }
            }
        }

        internal void ReleaseConnection(ref SqlConnection sqlConn, ref SqlCommand sqlCmd, bool error) {
            // !!! Please note that GetConnection and ReleaseConnection does NOT support
            // multithreading.  The caller must do the locking.

            if (sqlConn == null) {
                Debug.Assert(sqlCmd == null, "sqlCmd == null");
                return;
            }

            Debug.Assert(sqlCmd != null, "sqlCmd != null");

            if (_poolConn && !error) {
                _sqlConn = sqlConn; 
                _sqlCmd = sqlCmd;
            }
            else {
                sqlConn.Close();
            }

            sqlConn = null;
            sqlCmd = null;
        }
    }
    
    internal static class SqlCacheDependencyManager{
    
        internal const bool     ENABLED_DEFAULT = true;
        internal const int      POLLTIME_DEFAULT = 60000;
        internal const int      TABLE_NAME_LENGTH = 128;

        internal const int      SQL_EXCEPTION_SP_NOT_FOUND = 2812;
        internal const int      SQL_EXCEPTION_PERMISSION_DENIED_ON_OBJECT = 229;
        internal const int      SQL_EXCEPTION_PERMISSION_DENIED_ON_DATABASE = 262;
        internal const int      SQL_EXCEPTION_PERMISSION_DENIED_ON_USER = 2760;
        internal const int      SQL_EXCEPTION_NO_GRANT_PERMISSION = 4613;
        internal const int      SQL_EXCEPTION_ADHOC = 50000;

        const char              CacheKeySeparatorChar = ':';
        const string            CacheKeySeparator = ":";
        const string            CacheKeySeparatorEscaped = "\\:";
        
        internal const string   SQL_CUSTOM_ERROR_TABLE_NOT_FOUND = "00000001";

        internal const string   SQL_NOTIF_TABLE =
                                "AspNet_SqlCacheTablesForChangeNotification";

        internal const string   SQL_POLLING_SP =
                                "AspNet_SqlCachePollingStoredProcedure";

        internal const string   SQL_POLLING_SP_DBO =
                                "dbo.AspNet_SqlCachePollingStoredProcedure";

        internal static TimeSpan    OneSec = new TimeSpan(0, 0, 1);
        
        internal static Hashtable   s_DatabaseNotifStates = new Hashtable();
        static TimerCallback        s_timerCallback = new TimerCallback(PollCallback);
        static int                  s_activePolling = 0;
        static bool                 s_shutdown = false;

        static internal string GetMoniterKey(string database, string table) {
            if (database.IndexOf(CacheKeySeparatorChar) != -1) {
                database = database.Replace(CacheKeySeparator, CacheKeySeparatorEscaped);
            }
            
            if (table.IndexOf(CacheKeySeparatorChar) != -1) {
                table = table.Replace(CacheKeySeparator, CacheKeySeparatorEscaped);
            }
            
            // If we don't escape our separator char (':') in database and table, 
            // these two pairs of inputs will then generate the same key:
            // 1. database = "b", table = "b:b"
            // 2. database = "b:b", table = "b"
            return CacheInternal.PrefixSqlCacheDependency + database + CacheKeySeparator + table;
        }

        static internal void Dispose(int waitTimeoutMs) {
            try {
                DateTime waitLimit = DateTime.UtcNow.AddMilliseconds(waitTimeoutMs);

                Debug.Assert(s_shutdown != true, "s_shutdown != true");
                Debug.Trace("SqlCacheDependencyManager", "Dispose is called");
                
                s_shutdown = true;
                
                if (s_DatabaseNotifStates != null && s_DatabaseNotifStates.Count > 0) {
                    // Lock it because InitPolling could be modifying it.
                    lock(s_DatabaseNotifStates) {
                        foreach(DictionaryEntry entry in s_DatabaseNotifStates) {
                            object  obj = entry.Value;
                            if (obj != null) {
                                ((DatabaseNotifState)obj).Dispose();
                            }
                        }
                    }

                    for (;;) {
                        if (s_activePolling == 0)
                            break;
                            
                        Thread.Sleep(250);

                        // only apply timeout if a managed debugger is not attached
                        if (!System.Diagnostics.Debugger.IsAttached && DateTime.UtcNow > waitLimit) {
                            break; // give it up
                        }
                    }
                }
            }
            catch {
                // It's called by HttpRuntime.Dispose. It can't throw anything.
                return;
            }
        }

        internal static SqlCacheDependencyDatabase GetDatabaseConfig(string database) {            
            SqlCacheDependencySection config = RuntimeConfig.GetAppConfig().SqlCacheDependency;
            object  obj;
            
            obj =  config.Databases[database];
            if (obj == null) {
                throw new HttpException(SR.GetString(SR.Database_not_found, database));
            }
            
            return  (SqlCacheDependencyDatabase)obj;
        }

        // Initialize polling for a database.  It will:
        // 1. Create the DatabaseNotifState that holds the polling status about this database.
        // 2. Create the timer to poll.
        internal static void InitPolling(string database) {
            SqlCacheDependencySection config = RuntimeConfig.GetAppConfig().SqlCacheDependency;;
            SqlCacheDependencyDatabase  sqlDepDB;
            string connectionString;

            Debug.Trace("SqlCacheDependencyManager", 
                                "InitPolling is called.  Database=" + database);

            // Return if polling isn't even enabled.            
            if (!config.Enabled) {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.Polling_not_enabled_for_sql_cache),
                    config.ElementInformation.Properties["enabled"].Source, config.ElementInformation.Properties["enabled"].LineNumber);
            }

            // Return if the polltime is zero.  It means polling is disabled for this database.
            sqlDepDB = GetDatabaseConfig(database);
            if (sqlDepDB.PollTime == 0) {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.Polltime_zero_for_database_sql_cache, database),
                    sqlDepDB.ElementInformation.Properties["pollTime"].Source, sqlDepDB.ElementInformation.Properties["pollTime"].LineNumber);
            }

            if (s_DatabaseNotifStates.ContainsKey(database)) {
                // Someone has already started the timer for this database.
                Debug.Trace("SqlCacheDependencyManager", 
                                "InitPolling: Timer already started for " + database);

                return;
            }

            connectionString = SqlConnectionHelper.GetConnectionString(sqlDepDB.ConnectionStringName, true, true);
            if (connectionString == null || connectionString.Length < 1) {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.Connection_string_not_found, sqlDepDB.ConnectionStringName),
                    sqlDepDB.ElementInformation.Properties["connectionStringName"].Source, sqlDepDB.ElementInformation.Properties["connectionStringName"].LineNumber);
            }

            lock(s_DatabaseNotifStates) {
                DatabaseNotifState   state;

                if (s_DatabaseNotifStates.ContainsKey(database)) {
                    // Someone has already started the timer for this database.
                    Debug.Trace("SqlCacheDependencyManager", 
                                "InitPolling: Timer already started for " + database);

                    return;
                }

                Debug.Trace("SqlCacheDependencyManager", 
                                "InitPolling: Creating timer for " + database);

                state = new DatabaseNotifState(database, connectionString, sqlDepDB.PollTime);
                state._timer = new Timer(s_timerCallback, state, 0 /* dueTime */, sqlDepDB.PollTime /* period */);

                s_DatabaseNotifStates.Add(database, state);
            }
        }

        // Timer callback function.
        static void PollCallback(object state) {
            using (new ApplicationImpersonationContext()) {
                PollDatabaseForChanges((DatabaseNotifState)state, true /*fromTimer*/);
            }
        }

        // Query all the entries from the AspNet_SqlCacheTablesForChangeNotification 
        // table and update the values in the cache accordingly.
        //
        // This is mainly called by the timer callback.  But will also be called by
        // UpdateDatabaseNotifState, which polls for changes on demand.
        internal static void PollDatabaseForChanges(DatabaseNotifState dbState, bool fromTimer) {
            SqlDataReader       sqlReader = null;
            SqlConnection       sqlConn = null;
            SqlCommand          sqlCmd = null;
            int                 changeId;
            string              tableName;
            CacheInternal       cacheInternal = HttpRuntime.CacheInternal;
            string              monitorKey;
            object              obj;
            bool                notifEnabled = false;
            Exception           pollExpt = null;
            SqlException        sqlExpt = null;

            Debug.Trace("SqlCacheDependencyManagerPolling", 
                "PollCallback called; connection=" + dbState._connectionString);

            if (s_shutdown) {
                return;
            }

            // If this call is from a timer, and if the refcount for this database is zero,
            // we will ignore it. The exception is if dbState._init == false, 
            // which means the timer is polling it for the first time.
            if (dbState._refCount == 0 && fromTimer && dbState._init  ) {
                Debug.Trace("SqlCacheDependencyManagerPolling", 
                    "PollCallback ignored for " + dbState._database + " because refcount is 0");
                return;
            }

            // Grab the lock, which allows only one thread to enter this method.
            if (Interlocked.CompareExchange(ref dbState._rqInCallback, 1, 0) != 0) {

                // We can't get the lock.
                
                if (!fromTimer) {
                    // A non-timer caller will really want to make a call to SQL and
                    // get the result.  So if another thread is calling this, we'll
                    // wait for it to be done.
                    int         timeout;
                    HttpContext context = HttpContext.Current;

                    if (context == null) {
                        timeout = 30;
                    }
                    else {
                        timeout = Math.Max(context.Timeout.Seconds / 3, 30);
                    }
                    DateTime waitLimit = DateTime.UtcNow.Add(new TimeSpan(0, 0, timeout));
                    
                    for (;;) {
                        if (Interlocked.CompareExchange(ref dbState._rqInCallback, 1, 0) == 0) {
                            break;
                        }
                            
                        Thread.Sleep(250);

                        if (s_shutdown) {
                            return;
                        }

                        // only apply timeout if a managed debugger is not attached
                        if (!System.Diagnostics.Debugger.IsAttached && DateTime.UtcNow > waitLimit) {
                            // We've waited and retried for 5 seconds.
                            // Somehow PollCallback haven't finished its first call for this database
                            // Assume we cannot connect to SQL.
                            throw new HttpException(
                                SR.GetString(SR.Cant_connect_sql_cache_dep_database_polling, dbState._database));
                        }
                    }
                }
                else {
                    // For a timer callback, if another thread is updating the data for
                    // this database, this thread will just leave and let that thread
                    // finish the update job.
                    Debug.Trace("SqlCacheDependencyManagerPolling", 
                        "PollCallback returned because another thread is updating the data");
                    return;
                }
            }

            try {
                try {
                    // Keep a count on how many threads are polling right now
                    // This counter is used by Dispose()
                    Interlocked.Increment(ref s_activePolling);

                    // The below assert was commented out because this method is either
                    // called by a timer thread, or thru the SqlCacheDependencyAdmin APIs.
                    // In the latter case, the caller should have the permissions.
                    //(new SqlClientPermission(PermissionState.Unrestricted)).Assert();

                    dbState.GetConnection(out sqlConn, out sqlCmd);
                    sqlReader = sqlCmd.ExecuteReader();

                    // If we got stuck for a long time in the ExecuteReader above, 
                    // Dispose() may have given up already while waiting for this thread to finish
                    if (s_shutdown) {
                        return;
                    }

                    // ExecuteReader() succeeded, and that means we at least have found the notification table.
                    notifEnabled = true;

                    // Remember the original list of tables that are enabled
                    Hashtable   originalTables = (Hashtable)dbState._tables.Clone();
                    
                    while(sqlReader.Read()) {
                        tableName = sqlReader.GetString(0);
                        changeId = sqlReader.GetInt32(1);
                        
                        Debug.Trace("SqlCacheDependencyManagerPolling", 
                                "Database=" + dbState._database+ "; tableName=" + tableName + "; changeId=" + changeId);

                        monitorKey = GetMoniterKey(dbState._database, tableName);
                        obj = cacheInternal[monitorKey];

                        if (obj == null) {
                            Debug.Assert(!dbState._tables.ContainsKey(tableName), 
                                        "DatabaseNotifStae._tables and internal cache keys should be in-sync");
                            
                            Debug.Trace("SqlCacheDependencyManagerPolling", 
                                "Add Database=" + dbState._database+ "; tableName=" + tableName + "; changeId=" + changeId);
                            
                            cacheInternal.UtcAdd(monitorKey, changeId, null, 
                                        Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration,
                                        CacheItemPriority.NotRemovable, null);

                            dbState._tables.Add(tableName, null);
                        }
                        else if (changeId != (int)obj) {
                            Debug.Assert(dbState._tables.ContainsKey(tableName), 
                                        "DatabaseNotifStae._tables and internal cache keys should be in-sync");
                            
                            Debug.Trace("SqlCacheDependencyManagerPolling", 
                                    "Change Database=" + dbState._database+ "; tableName=" + tableName + "; old=" + (int)obj + "; new=" + changeId);
                            
                            // ChangeId is different. It means some table changes have happened.
                            // Update local cache value
                            cacheInternal.UtcInsert(monitorKey, changeId, null, 
                                        Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration,
                                        CacheItemPriority.NotRemovable, null);
                        }

                        originalTables.Remove(tableName);
                    }

                    // What's left in originalTables are the ones that're no longer
                    // contained in the AspNet_SqlCacheTablesForChangeNotification
                    // table in the database.
                    
                    // Remove tables which are no longer enabled for notification
                    foreach(object key in originalTables.Keys) {
                        dbState._tables.Remove((string)key);
                        cacheInternal.Remove(GetMoniterKey(dbState._database, (string)key));
                        
                        Debug.Trace("SqlCacheDependencyManagerPolling", 
                                "Remove Database=" + dbState._database+ "; key=" + key);
                    }

                    // Clear old error, if any.
                    if (dbState._pollSqlError != 0) {
                        dbState._pollSqlError = 0;
                    }
                }
                catch (Exception e) {
                    pollExpt = e;
                    
                    sqlExpt = e as SqlException;
                    if (sqlExpt != null) {
                        Debug.Trace("SqlCacheDependencyManagerPolling", "Error reading rows.  SqlException:"+
                            "\nMessage=" + sqlExpt.Message +
                            "\nNumber=" + sqlExpt.Number);
                        
                        dbState._pollSqlError = sqlExpt.Number;
                    }
                    else {
                        dbState._pollSqlError = 0;
                        Debug.Trace("SqlCacheDependencyManagerPolling", "Error reading rows.  Exception:"+ pollExpt);
                    }
                }
                finally {
                    try {
                        if (sqlReader != null) {
                            sqlReader.Close();
                        }

                        dbState.ReleaseConnection(ref sqlConn, ref sqlCmd, pollExpt != null);
                    }
                    catch {
                    }

                    // Need locking because EnsureTableIsRegisteredAndPolled() assumes 
                    // the fields in a dbState are set atomically.
                    lock(dbState) {
                        dbState._pollExpt = pollExpt;

                        // If we have changed from being enabled to disabled, and
                        // it's because we cannot find the SP for polling, it means
                        // the database is no longer enabled for sql cache dependency.
                        // we should invalidate all cache items depending on any
                        // table on this database
                        if (dbState._notifEnabled && !notifEnabled && 
                            pollExpt != null && dbState._pollSqlError == SQL_EXCEPTION_SP_NOT_FOUND) {
                            foreach(object key in dbState._tables.Keys) {
                                try {
                                    cacheInternal.Remove(GetMoniterKey(dbState._database, (string)key));
                                }
                                catch {}
                                
                                Debug.Trace("SqlCacheDependencyManagerPolling", 
                                    "Changed to disabled.  Remove Database=" + dbState._database+ "; key=" + key);
                            }

                            // Since we have removed all the cache items related to this database,
                            // the _refCount of this database will drop to zero, and thus the timer
                            // callback will not poll this database.
                            // So we have to cleanup _tables now.
                            dbState._tables.Clear();
                        }
                        
                        dbState._notifEnabled = notifEnabled;
                        dbState._utcTablesUpdated = DateTime.UtcNow;
                        
                        Debug.Trace("SqlCacheDependencyManagerPolling", "dbState:_pollExpt="+ dbState._pollExpt + 
                                "; _pollSqlError=" + dbState._pollSqlError + "; _notifEnabled=" + dbState._notifEnabled +
                                "; __utcTablesUpdated=" + dbState._utcTablesUpdated);
                    }

                    // Mark dbState as initialized by PollCallback for the first time.
                    // EnsureTableIsRegisteredAndPolled() depends on this.
                    if (dbState._init != true) {
                        dbState._init = true;
                    }

                    Interlocked.Decrement(ref s_activePolling);
                    
                    // Release the lock                
                    Interlocked.Exchange(ref dbState._rqInCallback, 0);
                }
            }
            catch { throw; }    // Prevent Exception Filter Security Issue (ASURT 122835)            
        }

        // Called by SqlCacheDependency.GetDependKey
        static internal void EnsureTableIsRegisteredAndPolled(string database, string table) {
            bool    doubleChecked = false;
            
            // First check.  If the cache key exists, that means the first poll request
            // for this table has successfully completed
            Debug.Trace("SqlCacheDependencyManagerCheck", 
                                "Check is called.  Database=" + database+ "; table=" + table);
            
            if (HttpRuntime.CacheInternal[GetMoniterKey(database, table)] != null) {
                return;
            }

            // Initilize polling for this database, if needed.
            InitPolling(database);

            // Wait until this database is initialized by PollCallback for the first time
            DatabaseNotifState  dbState = (DatabaseNotifState)s_DatabaseNotifStates[database];

            if (!dbState._init) {
                int         timeout;
                HttpContext context = HttpContext.Current;

                if (context == null) {
                    timeout = 30;
                }
                else {
                    timeout = Math.Max(context.Timeout.Seconds / 3, 30);
                }
                DateTime waitLimit = DateTime.UtcNow.Add(new TimeSpan(0, 0, timeout));
                
                Debug.Trace("SqlCacheDependencyManagerCheck", "Waiting for intialization: timeout=" + timeout + "s");
                
                for (;;) {
                    if (dbState._init)
                        break;
                        
                    Thread.Sleep(250);

                    // only apply timeout if a managed debugger is not attached
                    if (!System.Diagnostics.Debugger.IsAttached && DateTime.UtcNow > waitLimit) {
                        // We've waited and retried for waitLimit amount of time.
                        // Still PollCallback haven't finished its first call for this database
                        // Assume we cannot connect to SQL.
                        throw new HttpException(
                            SR.GetString(SR.Cant_connect_sql_cache_dep_database_polling, database));
                    }
                }
            }

            while(true) {
                DateTime    utcTablesLastUpdated;
                bool        dbRegistered;
                Exception   pollException;
                int         pollSqlError = 0;

                lock(dbState) {
                     Debug.Trace("SqlCacheDependencyManagerCheck", "dbState:_pollExpt="+ dbState._pollExpt + 
                                "; _pollSqlError=" + dbState._pollSqlError + "; _notifEnabled=" + dbState._notifEnabled );
                     
                    pollException = dbState._pollExpt;
                    if (pollException != null) {
                        pollSqlError = dbState._pollSqlError;
                    }
                    
                    utcTablesLastUpdated = dbState._utcTablesUpdated;
                    dbRegistered = dbState._notifEnabled;
                }

                if (pollException == null &&    // No exception from polling
                    dbRegistered &&             // The database is registered
                    dbState._tables.ContainsKey(table)) {   // The table is also registered
                    Debug.Trace("SqlCacheDependencyManagerCheck", "The table is registered too.  Exit now!");
                    return;
                }

                // Either we hit an error in the last polling, or the database or the table 
                // isn't registered.  
                //
                // See if we can double check.  Double checking is needed because the 
                // results we just looked at might be collected only at last poll time, 
                // which could be quite old, depending on the pollTime setting.
                //
                // The scenario we try to solve is:
                // 1. Let's say polling is configured to happen every 1 minute, and we just poll.
                // 2. A page then registers a table for notification.
                // 3. The page then try to use SqlCacheDependency on that table.
                // 4. If we don't call UpdateDatabaseNotifStat to query the database now,
                //    we'll have to wait for a whole minute before we can use that table.
                //

                // To prevent the SQL server from being bombarded by this kind of per-client-request
                // adhoc check, we only allow a max of one double check per second per database
                if (!doubleChecked && 
                    DateTime.UtcNow - utcTablesLastUpdated >= OneSec) {
                    
                    Debug.Trace("SqlCacheDependencyManagerCheck", "Double check...");
                    UpdateDatabaseNotifState(database);
                    doubleChecked = true;
                    continue;
                }

                if (pollSqlError == SQL_EXCEPTION_SP_NOT_FOUND) {
                    // This error happens if the database isn't enabled for notification.
                    // This doesn't count as a real Sql error
                    Debug.Assert(dbRegistered == false, "When this error happened, we shouldn't be able to poll the database");
                    pollException = null;
                }
                
                // Report any error if we failed in the last PollCallback
                if (pollException != null) {
                    string  error;
                    
                    if (pollSqlError == SQL_EXCEPTION_PERMISSION_DENIED_ON_OBJECT ||
                        pollSqlError == SQL_EXCEPTION_PERMISSION_DENIED_ON_DATABASE) {
                        error = SR.Permission_denied_database_polling;
                    }
                    else {
                        error = SR.Cant_connect_sql_cache_dep_database_polling;
                    }

                    HttpException outerException = new HttpException(
                           SR.GetString(error, database), pollException);

                    outerException.SetFormatter(new UseLastUnhandledErrorFormatter(outerException));
                    
                    throw outerException;
                }

                // If we don't get any error, then either the database or the table isn't registered.
                if (dbRegistered == false) {
                    throw new DatabaseNotEnabledForNotificationException(
                            SR.GetString(SR.Database_not_enabled_for_notification, database));
                }
                else {
                    throw new TableNotEnabledForNotificationException(
                            SR.GetString(SR.Table_not_enabled_for_notification, table, database));
                }
            }
        }

        // Do a on-demand polling of the database in order to obtain the latest
        // data.
        internal static void UpdateDatabaseNotifState(string database) {
            using (new ApplicationImpersonationContext()) {
                Debug.Trace("SqlCacheDependencyManager", "UpdateDatabaseNotifState called for database " + database +
                    "; running as " + WindowsIdentity.GetCurrent().Name);

                // Make sure we have initialized the polling of this database
                InitPolling(database);
                Debug.Assert(s_DatabaseNotifStates[database] != null, "s_DatabaseNotifStates[database] != null");

                PollDatabaseForChanges((DatabaseNotifState)s_DatabaseNotifStates[database], false /*fromTimer*/);
            }
        }

        // Update all initialized databases
        internal static void UpdateAllDatabaseNotifState() {
            lock(s_DatabaseNotifStates) {
                foreach(DictionaryEntry entry in s_DatabaseNotifStates) {
                    DatabaseNotifState  state = (DatabaseNotifState)entry.Value;
                    if (state._init) {
                        UpdateDatabaseNotifState((string)entry.Key);
                    }
                }
            }
        }

        internal static DatabaseNotifState AddRef(string database) {
            DatabaseNotifState dbState = (DatabaseNotifState)s_DatabaseNotifStates[database];
            Debug.Assert(dbState != null, "AddRef: s_DatabaseNotifStates[database] != null");

#if DBG
            int res = 
#endif            
            Interlocked.Increment(ref dbState._refCount);
#if DBG
            Debug.Trace("SqlCacheDependencyManager", "AddRef called for " + database + "; res=" + res);
            Debug.Assert(res > 0, "AddRef result for " + database + " must be > 0");
#endif
            return dbState;
        }
        
        internal static void Release(DatabaseNotifState dbState) {
#if DBG
            int res = 
#endif            
            Interlocked.Decrement(ref dbState._refCount);
#if DBG
            Debug.Trace("SqlCacheDependencyManager", "Release called for " + dbState._database + "; res=" + res);
            Debug.Assert(res >= 0, "Release result for " + dbState._database + " must be >= 0");
#endif
        }
    }

    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.High)]
    public static class SqlCacheDependencyAdmin {
        
        // Note:
        // In all the SQL statements below, we will do an unlocking
        // SELECT, followed by a locking SELECT.  This is to avoid
        // duplication operation.
        
        //
        // {0} = SQL_NOTIF_TABLE
        // {1} = SQL_POLLING_SP
        // {2} = SQL_REGISTER_TABLE_SP
        // {3} = SQL_TRIGGER_NAME_POSTFIX
        // {4} = SQL_UNREGISTER_TABLE_SP
        // {5} = SQL_QUERY_REGISTERED_TABLES_SP
        // {6} = SQL_UPDATE_CHANGE_ID_SP
        // 
        internal const string   SQL_CREATE_ENABLE_DATABASE_SP =
        "/* Create notification table */ \n" +
        "IF NOT EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = '{0}' AND type = 'U') \n" +
        "   IF NOT EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = '{0}' AND type = 'U') \n" +
        "      CREATE TABLE dbo.{0} (\n" +
        "      tableName             NVARCHAR(450) NOT NULL PRIMARY KEY,\n" +
        "      notificationCreated   DATETIME NOT NULL DEFAULT(GETDATE()),\n" +
        "      changeId              INT NOT NULL DEFAULT(0)\n" +
        "      )\n" +
        "\n" +
        "/* Create polling SP */\n" +
        "IF NOT EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = '{1}' AND type = 'P') \n" +
        "   IF NOT EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = '{1}' AND type = 'P') \n" +
        "   EXEC('CREATE PROCEDURE dbo.{1} AS\n" +
        "         SELECT tableName, changeId FROM dbo.{0}\n" +
        "         RETURN 0')\n" +
        "\n" +
        "/* Create SP for registering a table. */ \n" +
        "IF NOT EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = '{2}' AND type = 'P') \n" +
        "   IF NOT EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = '{2}' AND type = 'P') \n" +
        "   EXEC('CREATE PROCEDURE dbo.{2} \n" +
        "             @tableName NVARCHAR(450) \n" +
        "         AS\n" +
        "         BEGIN\n" +
        "\n" +
        "         DECLARE @triggerName AS NVARCHAR(3000) \n" +
        "         DECLARE @fullTriggerName AS NVARCHAR(3000)\n" +
        "         DECLARE @canonTableName NVARCHAR(3000) \n" +
        "         DECLARE @quotedTableName NVARCHAR(3000) \n" +
        "\n" +
        "         /* Create the trigger name */ \n" +
        "         SET @triggerName = REPLACE(@tableName, ''['', ''__o__'') \n" +
        "         SET @triggerName = REPLACE(@triggerName, '']'', ''__c__'') \n" +
        "         SET @triggerName = @triggerName + ''{3}'' \n" +
        "         SET @fullTriggerName = ''dbo.['' + @triggerName + '']'' \n" +
        "\n" +
        "         /* Create the cannonicalized table name for trigger creation */ \n" +
        "         /* Do not touch it if the name contains other delimiters */ \n" +
        "         IF (CHARINDEX(''.'', @tableName) <> 0 OR \n" +
        "             CHARINDEX(''['', @tableName) <> 0 OR \n" +
        "             CHARINDEX('']'', @tableName) <> 0) \n" +
        "             SET @canonTableName = @tableName \n" +
        "         ELSE \n" +
        "             SET @canonTableName = ''['' + @tableName + '']'' \n" +
        "\n" +
        "         /* First make sure the table exists */ \n" +
        "         IF (SELECT OBJECT_ID(@tableName, ''U'')) IS NULL \n" +
        "         BEGIN \n" +
        "             RAISERROR (''" + SqlCacheDependencyManager.SQL_CUSTOM_ERROR_TABLE_NOT_FOUND + "'', 16, 1) \n" +
        "             RETURN \n" +
        "         END \n" +
        "\n" +
        "         BEGIN TRAN\n" +
        "         /* Insert the value into the notification table */ \n" +
        "         IF NOT EXISTS (SELECT tableName FROM dbo.{0} WITH (NOLOCK) WHERE tableName = @tableName) \n" +
        "             IF NOT EXISTS (SELECT tableName FROM dbo.{0} WITH (TABLOCKX) WHERE tableName = @tableName) \n" +
        "                 INSERT  dbo.{0} \n" +
        "                 VALUES (@tableName, GETDATE(), 0)\n" +
        "\n" +
        "         /* Create the trigger */ \n" +
        "         SET @quotedTableName = QUOTENAME(@tableName, '''''''') \n" +
        "         IF NOT EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = @triggerName AND type = ''TR'') \n" +
        "             IF NOT EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = @triggerName AND type = ''TR'') \n" +
        "                 EXEC(''CREATE TRIGGER '' + @fullTriggerName + '' ON '' + @canonTableName +''\n" +
        "                       FOR INSERT, UPDATE, DELETE AS BEGIN\n" +
        "                       SET NOCOUNT ON\n" +
        "                       EXEC dbo.{6} N'' + @quotedTableName + ''\n" +
        "                       END\n" +
        "                       '')\n" +
        "         COMMIT TRAN\n" +
        "         END\n" +
        "   ')\n" +
        "\n" +
        "/* Create SP for updating the change Id of a table. */ \n" +
        "IF NOT EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = '{6}' AND type = 'P') \n" +
        "   IF NOT EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = '{6}' AND type = 'P') \n" +
        "   EXEC('CREATE PROCEDURE dbo.{6} \n" +
        "             @tableName NVARCHAR(450) \n" +
        "         AS\n" +
        "\n" +
        "         BEGIN \n" +
        "             UPDATE dbo.{0} WITH (ROWLOCK) SET changeId = changeId + 1 \n" +
        "             WHERE tableName = @tableName\n" +
        "         END\n" +
        "   ')\n" +
        "\n" +
        "/* Create SP for unregistering a table. */ \n" +
        "IF NOT EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = '{4}' AND type = 'P') \n" +
        "   IF NOT EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = '{4}' AND type = 'P') \n" +
        "   EXEC('CREATE PROCEDURE dbo.{4} \n" +
        "             @tableName NVARCHAR(450) \n" +
        "         AS\n" +
        "         BEGIN\n" +
        "\n" +
        "         BEGIN TRAN\n" +
        "         DECLARE @triggerName AS NVARCHAR(3000) \n" +
        "         DECLARE @fullTriggerName AS NVARCHAR(3000)\n" +
        "         SET @triggerName = REPLACE(@tableName, ''['', ''__o__'') \n" +
        "         SET @triggerName = REPLACE(@triggerName, '']'', ''__c__'') \n" +
        "         SET @triggerName = @triggerName + ''{3}'' \n" +
        "         SET @fullTriggerName = ''dbo.['' + @triggerName + '']'' \n" +
        "\n" +
        "         /* Remove the table-row from the notification table */ \n" +
        "         IF EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = ''{0}'' AND type = ''U'') \n" +
        "             IF EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = ''{0}'' AND type = ''U'') \n" +
        "             DELETE FROM dbo.{0} WHERE tableName = @tableName \n" +
        "\n" +
        "         /* Remove the trigger */ \n" +
        "         IF EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = @triggerName AND type = ''TR'') \n" +
        "             IF EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = @triggerName AND type = ''TR'') \n" +
        "             EXEC(''DROP TRIGGER '' + @fullTriggerName) \n" +
        "\n" +
        "         COMMIT TRAN\n" +
        "         END\n" +
        "   ')\n" +
        "\n" +
        "/* Create SP for querying all registered table */ \n" +
        "IF NOT EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = '{5}' AND type = 'P') \n" +
        "   IF NOT EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = '{5}' AND type = 'P') \n" +
        "   EXEC('CREATE PROCEDURE dbo.{5} \n" +
        "         AS\n" +
        "         SELECT tableName FROM dbo.{0}" +
        "   ')\n" +
        "\n" +
        "/* Create roles and grant them access to SP  */ \n" +
        "IF NOT EXISTS (SELECT name FROM sysusers WHERE issqlrole = 1 AND name = N'aspnet_ChangeNotification_ReceiveNotificationsOnlyAccess') \n" +
        "    EXEC sp_addrole N'aspnet_ChangeNotification_ReceiveNotificationsOnlyAccess' \n" +
        "\n" +
        "GRANT EXECUTE ON dbo.{1} to aspnet_ChangeNotification_ReceiveNotificationsOnlyAccess\n" +
        "\n";


        //
        // {0} = SQL_NOTIF_TABLE
        // {1} = SQL_POLLING_SP
        // {2} = SQL_REGISTER_TABLE_SP
        // {3} = SQL_UNREGISTER_TABLE_SP
        // {4} = SQL_QUERY_REGISTERED_TABLES_SP
        // {5} = SQL_UPDATE_CHANGE_ID_SP
        // 
        internal const string   SQL_DISABLE_DATABASE =
        "/* Remove notification table */ \n" +
        "IF EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = '{0}' AND type = 'U') \n" +
        "    IF EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = '{0}' AND type = 'U') \n" +
        "    BEGIN\n" +
        "      /* First, unregister all registered tables */ \n" +
        "      DECLARE tables_cursor CURSOR FOR \n" +
        "      SELECT tableName FROM dbo.{0} \n" +
        "      DECLARE @tableName AS NVARCHAR(450) \n" +
        "\n" +
        "      OPEN tables_cursor \n" +
        "\n" +
        "      /* Perform the first fetch. */ \n" +
        "      FETCH NEXT FROM tables_cursor INTO @tableName \n" +
        "\n" +
        "      /* Check @@FETCH_STATUS to see if there are any more rows to fetch. */ \n" +
        "      WHILE @@FETCH_STATUS = 0 \n" +
        "      BEGIN \n" +
        "          EXEC {3} @tableName \n" +
        "\n" +
        "          /* This is executed as long as the previous fetch succeeds. */ \n" +
        "          FETCH NEXT FROM tables_cursor INTO @tableName \n" +
        "      END \n" +
        "      CLOSE tables_cursor \n" +
        "      DEALLOCATE tables_cursor \n" +
        "\n" +
        "      /* Drop the table */\n" +
        "      DROP TABLE dbo.{0} \n" +
        "    END\n" +
        "\n" +
        "/* Remove polling SP */ \n" +
        "IF EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = '{1}' AND type = 'P') \n" +
        "    IF EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = '{1}' AND type = 'P') \n" +
        "      DROP PROCEDURE dbo.{1} \n" +
        "\n" +
        "/* Remove SP that registers a table */ \n" +
        "IF EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = '{2}' AND type = 'P') \n" +
        "    IF EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = '{2}' AND type = 'P') \n" +
        "      DROP PROCEDURE dbo.{2} \n" +
        "\n" +
        "/* Remove SP that unregisters a table */ \n" +
        "IF EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = '{3}' AND type = 'P') \n" +
        "    IF EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = '{3}' AND type = 'P') \n" +
        "      DROP PROCEDURE dbo.{3} \n"+
        "\n" +
        "/* Remove SP that querys the registered table */ \n" +
        "IF EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = '{4}' AND type = 'P') \n" +
        "    IF EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = '{4}' AND type = 'P') \n" +
        "      DROP PROCEDURE dbo.{4} \n" +
        "\n" +
        "/* Remove SP that updates the change Id of a table. */ \n" +
        "IF EXISTS (SELECT name FROM sysobjects WITH (NOLOCK) WHERE name = '{5}' AND type = 'P') \n" +
        "    IF EXISTS (SELECT name FROM sysobjects WITH (TABLOCKX) WHERE name = '{5}' AND type = 'P') \n" +
        "      DROP PROCEDURE dbo.{5} \n" +
        "\n" +
        "/* Drop roles */ \n" +
        "IF EXISTS ( SELECT name FROM sysusers WHERE issqlrole = 1 AND name = 'aspnet_ChangeNotification_ReceiveNotificationsOnlyAccess') BEGIN\n" +
        DROP_MEMBERS +
        "    EXEC sp_droprole 'aspnet_ChangeNotification_ReceiveNotificationsOnlyAccess'\n" +
        "END\n";

        internal const string   DROP_MEMBERS =
        "CREATE TABLE #aspnet_RoleMembers \n" +
        "( \n" +
        "    Group_name      sysname, \n" +
        "    Group_id        smallint, \n" +
        "    Users_in_group  sysname, \n" +
        "    User_id         smallint \n" +
        ") \n" +
        "INSERT INTO #aspnet_RoleMembers \n" +
        "EXEC sp_helpuser 'aspnet_ChangeNotification_ReceiveNotificationsOnlyAccess' \n" +
        " \n" +
        "DECLARE @user_id smallint \n" +
        "DECLARE @cmd nvarchar(500) \n" +
        "DECLARE c1 CURSOR FORWARD_ONLY FOR  \n" +
        "    SELECT User_id FROM #aspnet_RoleMembers \n" +
        "  \n" +
        "OPEN c1 \n" +
        "  \n" +
        "FETCH c1 INTO @user_id \n" +
        "WHILE (@@fetch_status = 0)  \n" +
        "BEGIN \n" +
        "    SET @cmd = 'EXEC sp_droprolemember ''aspnet_ChangeNotification_ReceiveNotificationsOnlyAccess'',''' + USER_NAME(@user_id) + '''' \n" +
        "    EXEC (@cmd) \n" +
        "    FETCH c1 INTO @user_id \n" +
        "END \n" +
        " \n" +
        "close c1 \n" +
        "deallocate c1 \n";
            
                
        internal const string   SQL_REGISTER_TABLE_SP =
                                "AspNet_SqlCacheRegisterTableStoredProcedure";

        internal const string   SQL_REGISTER_TABLE_SP_DBO =
                                "dbo.AspNet_SqlCacheRegisterTableStoredProcedure";

        internal const string   SQL_UNREGISTER_TABLE_SP =
                                "AspNet_SqlCacheUnRegisterTableStoredProcedure";

        internal const string   SQL_UNREGISTER_TABLE_SP_DBO =
                                "dbo.AspNet_SqlCacheUnRegisterTableStoredProcedure";

        internal const string   SQL_TRIGGER_NAME_POSTFIX =
                                "_AspNet_SqlCacheNotification_Trigger";

        internal const string   SQL_QUERY_REGISTERED_TABLES_SP =
                                "AspNet_SqlCacheQueryRegisteredTablesStoredProcedure";

        internal const string   SQL_QUERY_REGISTERED_TABLES_SP_DBO =
                                "dbo.AspNet_SqlCacheQueryRegisteredTablesStoredProcedure";

        internal const string   SQL_UPDATE_CHANGE_ID_SP=
                                "AspNet_SqlCacheUpdateChangeIdStoredProcedure";

        const int   SETUP_TABLE =           0x00000001;
        const int   SETUP_DISABLE =         0x00000002;
        const int   SETUP_HTTPREQUEST =     0x00000004;
        const int   SETUP_TABLES =          0x00000008; // We're called in a loop to setup an array of tables.

        internal static void SetupNotifications(int flags, string table, string connectionString) {
            SqlConnection   sqlConnection = null;
            SqlCommand      sqlCmd = null;
            bool            tableOp = (flags & (SETUP_TABLES|SETUP_TABLE)) != 0;
            bool            disable = (flags & SETUP_DISABLE) != 0;

            if (tableOp) {
                bool    tables = (flags & SETUP_TABLES) != 0;
                if (table == null) {
                    if (tables) {
                        throw new ArgumentException(SR.GetString(SR.Cache_null_table_in_tables),
                                            "tables");
                    }
                    else {
                        throw new ArgumentNullException("table");
                    }
                }
                else if (table.Length == 0) {
                    if (tables) {
                        throw new ArgumentException(SR.GetString(SR.Cache_null_table_in_tables),
                                    "tables");
                    }
                    else {
                        throw new ArgumentException(SR.GetString(SR.Cache_null_table),
                                    "table");
                    }
                }
            }
            
            try {
                sqlConnection = new SqlConnection(connectionString);
                sqlConnection.Open();

                sqlCmd = new SqlCommand(null, sqlConnection);

                if (tableOp) {
                    sqlCmd.CommandText = !disable ? SQL_REGISTER_TABLE_SP_DBO : SQL_UNREGISTER_TABLE_SP_DBO;
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Parameters.Add(new SqlParameter("@tableName", SqlDbType.NVarChar, table.Length));
                    sqlCmd.Parameters[0].Value = table;
                }
                else {
                    if (!disable) {
                        // Enable the database
                        sqlCmd.CommandText = String.Format(CultureInfo.InvariantCulture,
                                                SQL_CREATE_ENABLE_DATABASE_SP, 
                                                SqlCacheDependencyManager.SQL_NOTIF_TABLE, 
                                                SqlCacheDependencyManager.SQL_POLLING_SP,
                                                SQL_REGISTER_TABLE_SP,
                                                SQL_TRIGGER_NAME_POSTFIX,
                                                SQL_UNREGISTER_TABLE_SP,
                                                SQL_QUERY_REGISTERED_TABLES_SP,
                                                SQL_UPDATE_CHANGE_ID_SP);
                        sqlCmd.CommandType = CommandType.Text;
                    }
                    else {
                        // Disable the database
                        sqlCmd.CommandText = String.Format(CultureInfo.InvariantCulture,
                                                SQL_DISABLE_DATABASE, 
                                                SqlCacheDependencyManager.SQL_NOTIF_TABLE, 
                                                SqlCacheDependencyManager.SQL_POLLING_SP,
                                                SQL_REGISTER_TABLE_SP,
                                                SQL_UNREGISTER_TABLE_SP,
                                                SQL_QUERY_REGISTERED_TABLES_SP,
                                                SQL_UPDATE_CHANGE_ID_SP);
                        sqlCmd.CommandType = CommandType.Text;
                    }
                }
                
                Debug.Trace("SqlCacheDependencyAdmin", "\n" +
                    sqlCmd.CommandText);
                
                sqlCmd.ExecuteNonQuery();

                // Clear CommandText so that error handling won't mistakenly
                // report it as a SQL error
                sqlCmd.CommandText = String.Empty;

                // If we are being called as part of an ASP.NET Http request
                if (HttpRuntime.IsAspNetAppDomain) {
                    // Need to update the status of all initialized databases
                    //
                    // Note: we can actually try to figure out which database we want
                    // to update based on the connectionString.  But updating
                    // all initialized ones are good enough.
                    SqlCacheDependencyManager.UpdateAllDatabaseNotifState();
                }
            }
            catch (Exception e) {
                SqlException sqlExpt = e as SqlException;
                bool throwError = true;
                
                if (sqlExpt != null) {
                    Debug.Trace("SqlCacheDependencyAdmin", "SqlException:"+
                        "\nMessage=" + sqlExpt.Message +
                        "\nNumber=" + sqlExpt.Number);

                    if (sqlExpt.Number == SqlCacheDependencyManager.SQL_EXCEPTION_SP_NOT_FOUND) {
                        if (!disable) {
                            if (table != null) {
                                throw new DatabaseNotEnabledForNotificationException(
                                    SR.GetString(SR.Database_not_enabled_for_notification,
                                                                    sqlConnection.Database));
                            }
                            else {
                                throw;
                            }
                        }
                        else {
                            if (table != null) {
                                throw new DatabaseNotEnabledForNotificationException(
                                    SR.GetString(SR.Cant_disable_table_sql_cache_dep));
                            }
                            else {
                                // If we cannot find the SP for disabling the database, it maybe because
                                // SQL cache dep is already disabled, or the SP is missing.
                                // In either case, we just exit silently.
                                throwError = false;
                            }
                        }
                    }
                    else if (sqlExpt.Number == SqlCacheDependencyManager.SQL_EXCEPTION_PERMISSION_DENIED_ON_OBJECT ||
                            sqlExpt.Number == SqlCacheDependencyManager.SQL_EXCEPTION_PERMISSION_DENIED_ON_DATABASE ||
                            sqlExpt.Number == SqlCacheDependencyManager.SQL_EXCEPTION_PERMISSION_DENIED_ON_USER ||
                            sqlExpt.Number == SqlCacheDependencyManager.SQL_EXCEPTION_NO_GRANT_PERMISSION) {
                        string error;

                        if (!disable) {
                            if (table != null) {
                                error = SR.Permission_denied_table_enable_notification;
                            }
                            else {
                                error = SR.Permission_denied_database_enable_notification;
                            }
                        }
                        else {
                            if (table != null) {
                                error = SR.Permission_denied_table_disable_notification;
                            }
                            else {
                                error = SR.Permission_denied_database_disable_notification;
                            }
                        }

                        if (table != null) {
                            throw new HttpException(
                                SR.GetString(error, table));
                        }
                        else {
                            throw new HttpException(
                                SR.GetString(error));
                        }
                    }
                    else if (sqlExpt.Number == SqlCacheDependencyManager.SQL_EXCEPTION_ADHOC &&
                            sqlExpt.Message == SqlCacheDependencyManager.SQL_CUSTOM_ERROR_TABLE_NOT_FOUND) {
                        Debug.Assert(!disable && table != null, "disable && table != null");
                        throw new HttpException(SR.GetString(SR.Cache_dep_table_not_found, table));
                    }
                }

                string  errString;
                
                if (sqlCmd != null && sqlCmd.CommandText.Length != 0) {
                    errString = SR.GetString(SR.Cant_connect_sql_cache_dep_database_admin_cmdtxt, 
                                    sqlCmd.CommandText);
                }
                else {
                    errString = SR.GetString(SR.Cant_connect_sql_cache_dep_database_admin);
                }

                if (throwError) {
                    throw new HttpException(errString, e);
                }
            }
            finally {
                if (sqlConnection != null) {
                    sqlConnection.Close();
                }
            }
        }

        public static void EnableNotifications(string connectionString) {
            SetupNotifications(0, null, connectionString);
        }
        
        public static void DisableNotifications(string connectionString) {
            SetupNotifications(SETUP_DISABLE, null, connectionString);
        }
        
        public static void EnableTableForNotifications(string connectionString, string table) {
            SetupNotifications(SETUP_TABLE, table, connectionString);
        }
        
        public static void EnableTableForNotifications(string connectionString, string[] tables) {
            if (tables == null) {
                throw new ArgumentNullException("tables");
            }
            
            foreach (string table in tables) {
                SetupNotifications(SETUP_TABLES, table, connectionString);
            }
        }
        
        public static void DisableTableForNotifications(string connectionString, string table) {
            SetupNotifications(SETUP_TABLE|SETUP_DISABLE, table, connectionString);
        }
        
        public static void DisableTableForNotifications(string connectionString, string[] tables) {
            if (tables == null) {
                throw new ArgumentNullException("tables");
            }
            
            foreach (string table in tables) {
                SetupNotifications(SETUP_TABLES|SETUP_DISABLE, table, connectionString);
            }
        }
        
        static string[] GetEnabledTables(string connectionString) {
        
            SqlDataReader       sqlReader = null;
            SqlConnection       sqlConn = null;
            SqlCommand          sqlCmd = null;
            ArrayList           tablesObj = new ArrayList();

            try {
                sqlConn = new SqlConnection(connectionString);
                sqlConn.Open();

                sqlCmd = new SqlCommand(SQL_QUERY_REGISTERED_TABLES_SP_DBO, sqlConn);
                sqlCmd.CommandType = CommandType.StoredProcedure;
                
                sqlReader = sqlCmd.ExecuteReader();

                while(sqlReader.Read()) {
                    tablesObj.Add(sqlReader.GetString(0));
                }
            }
            catch (Exception e) {
                SqlException sqlExpt = e as SqlException;
                
                if (sqlExpt != null &&
                    sqlExpt.Number == SqlCacheDependencyManager.SQL_EXCEPTION_SP_NOT_FOUND) {
                    
                        throw new DatabaseNotEnabledForNotificationException(
                                SR.GetString(SR.Database_not_enabled_for_notification,
                                                                sqlConn.Database));
                }
                else {
                    throw new HttpException(SR.GetString(SR.Cant_get_enabled_tables_sql_cache_dep), e);
                }
            }
            finally {
                try {
                    if (sqlReader != null) {
                        sqlReader.Close();
                    }

                    if (sqlConn != null) {
                        sqlConn.Close();
                    }
                }
                catch {
                }
            }

            return (string[])tablesObj.ToArray(Type.GetType("System.String"));
        }

        public static string[] GetTablesEnabledForNotifications(string connectionString) {
            return GetEnabledTables(connectionString);
        }
    }
}


