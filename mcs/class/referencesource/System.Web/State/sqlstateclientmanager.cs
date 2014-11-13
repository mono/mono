//------------------------------------------------------------------------------
// <copyright file="SqlSessionStateStore.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * SqlSessionStateStore.cs
 *
 * Copyright (c) 1998-2000, Microsoft Corporation
 *
 */

namespace System.Web.SessionState {

    using System;
    using System.Configuration;
    using System.Collections;
    using System.Threading;
    using System.IO;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Util;
    using System.Data;
    using System.Data.SqlClient;
    using System.Data.Common;
    using System.Text;
    using System.Security.Principal;
    using System.Xml;
    using System.Collections.Specialized;
    using System.Configuration.Provider;
    using System.Globalization;
    using System.Web.Management;
    using System.Web.Hosting;
    using System.Web.Configuration;

    /*
     * Provides session state via SQL Server
     */
    internal class SqlSessionStateStore : SessionStateStoreProviderBase {

        internal enum SupportFlags : uint {
            None =              0x00000000,
            GetLockAge =        0x00000001,
            Uninitialized =     0xFFFFFFFF
        }

        #pragma warning disable 0649
        static ReadWriteSpinLock    s_lock;
        #pragma warning restore 0649
        static int          s_isClearPoolInProgress;
        static int          s_commandTimeout;
        static TimeSpan     s_retryInterval;
        static SqlPartitionInfo s_singlePartitionInfo;
        static PartitionManager s_partitionManager;
        static bool         s_oneTimeInited;
        static bool         s_usePartition;
        static EventHandler s_onAppDomainUnload;


        // We keep these info because we don't want to hold on to the config object.
        static string       s_configPartitionResolverType;
        static string       s_configSqlConnectionFileName;
        static int          s_configSqlConnectionLineNumber;
        static bool         s_configAllowCustomSqlDatabase;
        static bool         s_configCompressionEnabled;

        // Per request info
        HttpContext         _rqContext;
        int                 _rqOrigStreamLen;
        IPartitionResolver  _partitionResolver;
        SqlPartitionInfo    _partitionInfo;

        const int ITEM_SHORT_LENGTH =   7000;
        const int SQL_ERROR_PRIMARY_KEY_VIOLATION = 2627;
        const int SQL_LOGIN_FAILED = 18456;
        const int SQL_LOGIN_FAILED_2 = 18452;
        const int SQL_LOGIN_FAILED_3 = 18450;
        const int SQL_CANNOT_OPEN_DATABASE_FOR_LOGIN = 4060;
        const int SQL_TIMEOUT_EXPIRED = -2;
        const int APP_SUFFIX_LENGTH = 8;
        const int FIRST_RETRY_SLEEP_TIME = 5000;
        const int RETRY_SLEEP_TIME = 1000;

        static int ID_LENGTH = SessionIDManager.SessionIDMaxLength + APP_SUFFIX_LENGTH;
        internal const int SQL_COMMAND_TIMEOUT_DEFAULT = 30;        // in sec

        internal SqlSessionStateStore() {
        }

        internal override void Initialize(string name, NameValueCollection config, IPartitionResolver partitionResolver) {
            _partitionResolver = partitionResolver;
            Initialize(name, config);
        }

#if DBG
        SessionStateModule  _module;

        internal void SetModule(SessionStateModule module) {
            _module = module;
        }
#endif

        public override void Initialize(string name, NameValueCollection config)
        {
            if (String.IsNullOrEmpty(name))
                name = "SQL Server Session State Provider";

            base.Initialize(name, config);

            if (!s_oneTimeInited) {
                s_lock.AcquireWriterLock();
                try {
                    if (!s_oneTimeInited) {
                        OneTimeInit();
                    }
                }
                finally {
                    s_lock.ReleaseWriterLock();
                }
            }

            if (!s_usePartition) {
                // For single partition, the connection info won't change from request to request
                Debug.Assert(_partitionResolver == null);
                _partitionInfo = s_singlePartitionInfo;
            }
        }

        void OneTimeInit() {
            SessionStateSection config = RuntimeConfig.GetAppConfig().SessionState;

            s_configPartitionResolverType = config.PartitionResolverType;
            s_configSqlConnectionFileName = config.ElementInformation.Properties["sqlConnectionString"].Source;
            s_configSqlConnectionLineNumber = config.ElementInformation.Properties["sqlConnectionString"].LineNumber;
            s_configAllowCustomSqlDatabase = config.AllowCustomSqlDatabase;
            s_configCompressionEnabled = config.CompressionEnabled;

            if (_partitionResolver == null) {
                String sqlConnectionString = config.SqlConnectionString;

                SessionStateModule.ReadConnectionString(config, ref sqlConnectionString, "sqlConnectionString");
                s_singlePartitionInfo = (SqlPartitionInfo)CreatePartitionInfo(sqlConnectionString);
            }
            else {
                s_usePartition = true;
                s_partitionManager = new PartitionManager(new CreatePartitionInfo(CreatePartitionInfo));
            }

            s_commandTimeout = (int)config.SqlCommandTimeout.TotalSeconds;
            s_retryInterval = config.SqlConnectionRetryInterval;

            s_isClearPoolInProgress = 0;

            // We only need to do this in one instance
            s_onAppDomainUnload = new EventHandler(OnAppDomainUnload);
            Thread.GetDomain().DomainUnload += s_onAppDomainUnload;

            // Last thing to set.
            s_oneTimeInited = true;
        }

        void OnAppDomainUnload(Object unusedObject, EventArgs unusedEventArgs) {
            Debug.Trace("SqlSessionStateStore", "OnAppDomainUnload called");

            Thread.GetDomain().DomainUnload -= s_onAppDomainUnload;

            if (_partitionResolver == null) {
                if (s_singlePartitionInfo != null) {
                    s_singlePartitionInfo.Dispose();
                }
            }
            else {
                if (s_partitionManager != null) {
                    s_partitionManager.Dispose();
                }
            }
        }

        internal IPartitionInfo CreatePartitionInfo(string sqlConnectionString) {
            /*
             * Parse the connection string for errors. We want to ensure
             * that the user's connection string doesn't contain an
             * Initial Catalog entry, so we must first create a dummy connection.
             */
            SqlConnection   dummyConnection;
            string          attachDBFilename = null;

            try {
                dummyConnection = new SqlConnection(sqlConnectionString);
            }
            catch (Exception e) {
                if (s_usePartition) {
                    HttpException outerException = new HttpException(
                           SR.GetString(SR.Error_parsing_sql_partition_resolver_string, s_configPartitionResolverType, e.Message), e);

                    outerException.SetFormatter(new UseLastUnhandledErrorFormatter(outerException));

                    throw outerException;
                }
                else {
                    throw new ConfigurationErrorsException(
                        SR.GetString(SR.Error_parsing_session_sqlConnectionString, e.Message), e,
                        s_configSqlConnectionFileName, s_configSqlConnectionLineNumber);
                }
            }

            // Search for both Database and AttachDbFileName.  Don't append our
            // database name if either of them exists.
            string database = dummyConnection.Database;
            SqlConnectionStringBuilder  scsb = new SqlConnectionStringBuilder(sqlConnectionString);

            if (String.IsNullOrEmpty(database)) {
                database = scsb.AttachDBFilename;
                attachDBFilename = database;
            }

            if (!String.IsNullOrEmpty(database)) {
                if (!s_configAllowCustomSqlDatabase) {
                    if (s_usePartition) {
                        throw new HttpException(
                                SR.GetString(SR.No_database_allowed_in_sql_partition_resolver_string,
                                            s_configPartitionResolverType, dummyConnection.DataSource, database));
                    }
                    else {
                        throw new ConfigurationErrorsException(
                                SR.GetString(SR.No_database_allowed_in_sqlConnectionString),
                                s_configSqlConnectionFileName, s_configSqlConnectionLineNumber);
                    }
                }

                if (attachDBFilename != null) {
                    HttpRuntime.CheckFilePermission(attachDBFilename, true);
                }
            }
            else {
                sqlConnectionString += ";Initial Catalog=ASPState";
            }

            return new SqlPartitionInfo(new ResourcePool(new TimeSpan(0, 0, 5), int.MaxValue),
                                            scsb.IntegratedSecurity,
                                            sqlConnectionString);

        }

        public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback) {
            return false;
        }

        public override void Dispose() {
        }

        public override void InitializeRequest(HttpContext context) {
            Debug.Assert(context != null, "context != null");

            _rqContext = context;
            _rqOrigStreamLen = 0;

            if (s_usePartition) {
                // For multiple partition case, the connection info can change from request to request
                _partitionInfo = null;
            }
        }

        public override void EndRequest(HttpContext context) {
            Debug.Assert(context != null, "context != null");
            _rqContext = null;
        }

        public bool KnowForSureNotUsingIntegratedSecurity {
            get {
                if (_partitionInfo == null) {
                    Debug.Assert(s_usePartition, "_partitionInfo can be null only if we're using paritioning and we haven't called GetConnection yet.");
                    // If we're using partitioning, we need the session id to figure out the connection
                    // string.  Without it, we can't know for sure.
                    return false;
                }
                else {
                    Debug.Assert(_partitionInfo != null);
                    return !_partitionInfo.UseIntegratedSecurity;
                }
            }
        }

        //
        // Regarding resource pool, we will turn it on if in <identity>:
        //  - User is not using integrated security
        //  - impersonation = "false"
        //  - impersonation = "true" and userName/password is NON-null
        //  - impersonation = "true" and IIS is using Anonymous
        //
        // Otherwise, the impersonated account will be dynamic and we have to turn
        // resource pooling off.
        //
        // Note:
        // In case 2. above, the user can specify different usernames in different
        // web.config in different subdirs in the app.  In this case, we will just
        // cache the connections in the resource pool based on the identity of the
        // connection.  So in this specific scenario it is possible to have the
        // resource pool filled with mixed identities.
        //
        bool CanUsePooling() {
            bool    ret;

            if (KnowForSureNotUsingIntegratedSecurity) {
                Debug.Trace("SessionStatePooling", "CanUsePooling: not using integrated security");
                ret = true;
            }
            else if (_rqContext == null) {
                // One way this can happen is we hit an error on page compilation,
                // and SessionStateModule.OnEndRequest is called
                Debug.Trace("SessionStatePooling", "CanUsePooling: no context");
                ret = false;
            }
            else if (!_rqContext.IsClientImpersonationConfigured) {
                Debug.Trace("SessionStatePooling", "CanUsePooling: mode is None or Application");
                ret = true;
            }
            else if (HttpRuntime.IsOnUNCShareInternal) {
                Debug.Trace("SessionStatePooling", "CanUsePooling: mode is UNC");
                ret = false;
            }
            else {
                string logon = _rqContext.WorkerRequest.GetServerVariable("LOGON_USER");

                Debug.Trace("SessionStatePooling", "LOGON_USER = '" + logon + "'; identity = '" + _rqContext.User.Identity.Name + "'; IsUNC = " + HttpRuntime.IsOnUNCShareInternal);

                if (String.IsNullOrEmpty(logon)) {
                    ret = true;
                }
                else {
                    ret = false;
                }
            }

            Debug.Trace("SessionStatePooling", "CanUsePooling returns " + ret);
            return ret;
        }

        SqlStateConnection GetConnection(string id, ref bool usePooling) {
            SqlStateConnection conn = null;

            if (_partitionInfo == null) {
                Debug.Assert(s_partitionManager != null);
                Debug.Assert(_partitionResolver != null);

                _partitionInfo = (SqlPartitionInfo)s_partitionManager.GetPartition(_partitionResolver, id);
            }

            Debug.Trace("SessionStatePooling", "Calling GetConnection under " + WindowsIdentity.GetCurrent().Name);
#if DBG
            Debug.Assert(_module._rqChangeImpersonationRefCount != 0,
                "SessionStateModule.ChangeImpersonation should have been called before making any call to SQL");
#endif

            usePooling = CanUsePooling();
            if (usePooling) {
                conn = (SqlStateConnection) _partitionInfo.RetrieveResource();
                if (conn != null && (conn.Connection.State & ConnectionState.Open) == 0) {
                    conn.Dispose();
                    conn = null;
                }
            }

            if (conn == null) {
                conn = new SqlStateConnection(_partitionInfo, s_retryInterval);
            }

            return conn;
        }

        void DisposeOrReuseConnection(ref SqlStateConnection conn, bool usePooling) {
            try {
                if (conn == null) {
                    return;
                }

                if (usePooling) {
                    conn.ClearAllParameters();
                    _partitionInfo.StoreResource(conn);
                    conn = null;
                }
            }
            finally {
                if (conn != null) {
                    conn.Dispose();
                }
            }
        }

        internal static void ThrowSqlConnectionException(SqlConnection conn, Exception e) {
            if (s_usePartition) {
                throw new HttpException(
                    SR.GetString(SR.Cant_connect_sql_session_database_partition_resolver,
                                s_configPartitionResolverType, conn.DataSource, conn.Database));
            }
            else {
                throw new HttpException(
                    SR.GetString(SR.Cant_connect_sql_session_database),
                    e);
            }
        }

        SessionStateStoreData DoGet(HttpContext context, String id, bool getExclusive,
                                        out bool locked,
                                        out TimeSpan lockAge,
                                        out object lockId,
                                        out SessionStateActions actionFlags) {
            SqlDataReader       reader;
            byte []             buf;
            MemoryStream        stream = null;
            SessionStateStoreData    item;
            bool                useGetLockAge = false;
            SqlStateConnection  conn = null;
            SqlCommand          cmd = null;
            bool                usePooling = true;

            Debug.Assert(id.Length <= SessionIDManager.SESSION_ID_LENGTH_LIMIT, "id.Length <= SessionIDManager.SESSION_ID_LENGTH_LIMIT");
            Debug.Assert(context != null, "context != null");

            // Set default return values
            locked = false;
            lockId = null;
            lockAge = TimeSpan.Zero;
            actionFlags = 0;

            buf = null;
            reader = null;

            conn = GetConnection(id, ref usePooling);

            Debug.Assert(_partitionInfo != null, "_partitionInfo != null");
            Debug.Assert(_partitionInfo.SupportFlags != SupportFlags.Uninitialized, "_partitionInfo.SupportFlags != SupportFlags.Uninitialized");

            //
            // In general, if we're talking to a SQL 2000 or above, we use LockAge; otherwise we use LockDate.
            // Below are the details:
            //
            // Version 1
            // ---------
            // In v1, the lockDate is generated and stored in SQL using local time, and we calculate the "lockage"
            // (i.e. how long the item is locked) by having the web server read lockDate from SQL and substract it
            // from DateTime.Now.  But this approach introduced two problems:
            //  1. SQL server and web servers need to be in the same time zone.
            //  2. Daylight savings problem.
            //
            // Version 1.1
            // -----------
            // In v1.1, if using SQL 2000 we fixed the problem by calculating the "lockage" directly in SQL
            // so that the SQL server and the web server don't have to be in the same time zone.  We also
            // use UTC date to store time in SQL so that the Daylight savings problem is solved.
            //
            // In summary, if using SQL 2000 we made the following changes to the SQL tables:
            //      i. The column Expires is using now UTC time
            //     ii. Add new SP TempGetStateItem2 and TempGetStateItemExclusive2 to return a lockage
            //         instead of a lockDate.
            //    iii. To support v1 web server, we still need to have TempGetStateItem and
            //         TempGetStateItemExclusive.  However, we modify it a bit so that they use
            //         UTC time to update Expires column.
            //
            // If using SQL 7, we decided not to fix the problem, and the SQL scripts for SQL 7 remain pretty much
            // the same. That means v1.1 web server will continue to call TempGetStateItem and
            // TempGetStateItemExclusive and use v1 way to calculate the "lockage".
            //
            // Version 2.0
            // -----------
            // In v2.0 we added some new SP TempGetStateItem3 and TempGetStateItemExclusive3
            // because we added a new return value 'actionFlags'.  However, the principle remains the same
            // that we support lockAge only if talking to SQL 2000.
            //
            // (When one day MS stops supporting SQL 7 we can remove all the SQL7-specific scripts and
            //  stop all these craziness.)
            //
            if ((_partitionInfo.SupportFlags & SupportFlags.GetLockAge) != 0) {
                useGetLockAge = true;
            }

            try {
                if (getExclusive) {
                    cmd = conn.TempGetExclusive;
                }
                else {
                    cmd = conn.TempGet;
                }

                cmd.Parameters[0].Value = id + _partitionInfo.AppSuffix; // @id
                cmd.Parameters[1].Value = Convert.DBNull;   // @itemShort
                cmd.Parameters[2].Value = Convert.DBNull;   // @locked
                cmd.Parameters[3].Value = Convert.DBNull;   // @lockDate or @lockAge
                cmd.Parameters[4].Value = Convert.DBNull;   // @lockCookie
                cmd.Parameters[5].Value = Convert.DBNull;   // @actionFlags

                using(reader = SqlExecuteReaderWithRetry(cmd, CommandBehavior.Default)) {

                    /* If the cmd returned data, we must read it all before getting out params */
                    if (reader != null) {
                        try {
                            if (reader.Read()) {
                                Debug.Trace("SqlSessionStateStore", "Sql Get returned long item");
                                buf = (byte[]) reader[0];
                            }
                        } catch(Exception e) {
                            ThrowSqlConnectionException(cmd.Connection, e);
                        }
                    }
                }

                /* Check if value was returned */
                if (Convert.IsDBNull(cmd.Parameters[2].Value)) {
                    Debug.Trace("SqlSessionStateStore", "Sql Get returned null");
                    return null;
                }

                /* Check if item is locked */
                Debug.Assert(!Convert.IsDBNull(cmd.Parameters[3].Value), "!Convert.IsDBNull(cmd.Parameters[3].Value)");
                Debug.Assert(!Convert.IsDBNull(cmd.Parameters[4].Value), "!Convert.IsDBNull(cmd.Parameters[4].Value)");

                locked = (bool) cmd.Parameters[2].Value;
                lockId = (int) cmd.Parameters[4].Value;

                if (locked) {
                    Debug.Trace("SqlSessionStateStore", "Sql Get returned item that was locked");
                    Debug.Assert(((int)cmd.Parameters[5].Value & (int)SessionStateActions.InitializeItem) == 0,
                        "(cmd.Parameters[5].Value & SessionStateActions.InitializeItem) == 0; uninit item shouldn't be locked");

                    if (useGetLockAge) {
                        lockAge = new TimeSpan(0, 0, (int) cmd.Parameters[3].Value);
                    }
                    else {
                        DateTime            lockDate;
                        lockDate = (DateTime) cmd.Parameters[3].Value;
                        lockAge = DateTime.Now - lockDate;
                    }

                    Debug.Trace("SqlSessionStateStore", "LockAge = " + lockAge);

                    if (lockAge > new TimeSpan(0, 0, Sec.ONE_YEAR)) {
                        Debug.Trace("SqlSessionStateStore", "Lock age is more than 1 year!!!");
                        lockAge = TimeSpan.Zero;
                    }
                    return null;
                }

                actionFlags = (SessionStateActions) cmd.Parameters[5].Value;

                if (buf == null) {
                    /* Get short item */
                    Debug.Assert(!Convert.IsDBNull(cmd.Parameters[1].Value), "!Convert.IsDBNull(cmd.Parameters[1].Value)");
                    Debug.Trace("SqlSessionStateStore", "Sql Get returned short item");
                    buf = (byte[]) cmd.Parameters[1].Value;
                    Debug.Assert(buf != null, "buf != null");
                }

                // Done with the connection.
                DisposeOrReuseConnection(ref conn, usePooling);

                using(stream = new MemoryStream(buf)) {
                    item = SessionStateUtility.DeserializeStoreData(context, stream, s_configCompressionEnabled);
                    _rqOrigStreamLen = (int) stream.Position;
                }
                return item;
            }
            finally {
                DisposeOrReuseConnection(ref conn, usePooling);
            }
        }

        public override SessionStateStoreData  GetItem(HttpContext context,
                                                        String id,
                                                        out bool locked,
                                                        out TimeSpan lockAge,
                                                        out object lockId,
                                                        out SessionStateActions actionFlags) {
            Debug.Trace("SqlSessionStateStore", "Calling Sql Get, id=" + id);

            SessionIDManager.CheckIdLength(id, true /* throwOnFail */);
            return DoGet(context, id, false, out locked, out lockAge, out lockId, out actionFlags);
        }

        public override SessionStateStoreData  GetItemExclusive(HttpContext context,
                                                String id,
                                                out bool locked,
                                                out TimeSpan lockAge,
                                                out object lockId,
                                                out SessionStateActions actionFlags) {
            Debug.Trace("SqlSessionStateStore", "Calling Sql GetExclusive, id=" + id);

            SessionIDManager.CheckIdLength(id, true /* throwOnFail */);
            return DoGet(context, id, true, out locked, out lockAge, out lockId, out actionFlags);
        }


        public override void ReleaseItemExclusive(HttpContext context,
                                String id,
                                object lockId) {
            Debug.Trace("SqlSessionStateStore", "Calling Sql ReleaseExclusive, id=" + id);
            Debug.Assert(lockId != null, "lockId != null");
            Debug.Assert(context != null, "context != null");

            bool                usePooling = true;
            SqlStateConnection  conn = null;
            int                 lockCookie = (int)lockId;

            try {
                SessionIDManager.CheckIdLength(id, true /* throwOnFail */);

                conn = GetConnection(id, ref usePooling);
                SqlCommand cmd = conn.TempReleaseExclusive;
                cmd.Parameters[0].Value = id + _partitionInfo.AppSuffix;
                cmd.Parameters[1].Value = lockCookie;
                SqlExecuteNonQueryWithRetry(cmd, false, null);

            }
            finally {
                DisposeOrReuseConnection(ref conn, usePooling);
            }
        }

        public override void SetAndReleaseItemExclusive(HttpContext context,
                                    String id,
                                    SessionStateStoreData item,
                                    object lockId,
                                    bool newItem) {
            byte []             buf;
            int                 length;
            SqlCommand          cmd;
            bool                usePooling = true;
            SqlStateConnection  conn = null;
            int                 lockCookie;

            Debug.Assert(context != null, "context != null");

            try {
                Debug.Trace("SqlSessionStateStore", "Calling Sql Set, id=" + id);

                Debug.Assert(item.Items != null, "item.Items != null");
                Debug.Assert(item.StaticObjects != null, "item.StaticObjects != null");

                SessionIDManager.CheckIdLength(id, true /* throwOnFail */);

                try {
                    SessionStateUtility.SerializeStoreData(item, ITEM_SHORT_LENGTH, out buf, out length, s_configCompressionEnabled);
                }
                catch {
                    if (!newItem) {
                        ((SessionStateStoreProviderBase)this).ReleaseItemExclusive(context, id, lockId);
                    }
                    throw;
                }

                // Save it to the store

                if (lockId == null) {
                    lockCookie = 0;
                }
                else {
                    lockCookie = (int)lockId;
                }

                conn = GetConnection(id, ref usePooling);

                if (!newItem) {
                    Debug.Assert(_rqOrigStreamLen > 0, "_rqOrigStreamLen > 0");
                    if (length <= ITEM_SHORT_LENGTH) {
                        if (_rqOrigStreamLen <= ITEM_SHORT_LENGTH) {
                            cmd = conn.TempUpdateShort;
                        }
                        else {
                            cmd = conn.TempUpdateShortNullLong;
                        }
                    }
                    else {
                        if (_rqOrigStreamLen <= ITEM_SHORT_LENGTH) {
                            cmd = conn.TempUpdateLongNullShort;
                        }
                        else {
                            cmd = conn.TempUpdateLong;
                        }
                    }

                }
                else {
                    if (length <= ITEM_SHORT_LENGTH) {
                        cmd = conn.TempInsertShort;
                    }
                    else {
                        cmd = conn.TempInsertLong;
                    }
                }

                cmd.Parameters[0].Value = id + _partitionInfo.AppSuffix;
                cmd.Parameters[1].Size = length;
                cmd.Parameters[1].Value = buf;
                cmd.Parameters[2].Value = item.Timeout;
                if (!newItem) {
                    cmd.Parameters[3].Value = lockCookie;
                }
                SqlExecuteNonQueryWithRetry(cmd, newItem, id);
            }
            finally {
                DisposeOrReuseConnection(ref conn, usePooling);
            }
        }

        public override void RemoveItem(HttpContext context,
                                        String id,
                                        object lockId,
                                        SessionStateStoreData item) {
            Debug.Trace("SqlSessionStateStore", "Calling Sql Remove, id=" + id);
            Debug.Assert(lockId != null, "lockId != null");
            Debug.Assert(context != null, "context != null");

            bool                usePooling = true;
            SqlStateConnection  conn = null;
            int                 lockCookie = (int)lockId;

            try {
                SessionIDManager.CheckIdLength(id, true /* throwOnFail */);

                conn = GetConnection(id, ref usePooling);
                SqlCommand cmd = conn.TempRemove;
                cmd.Parameters[0].Value = id + _partitionInfo.AppSuffix;
                cmd.Parameters[1].Value = lockCookie;
                SqlExecuteNonQueryWithRetry(cmd, false, null);
            }
            finally {
                DisposeOrReuseConnection(ref conn, usePooling);
            }
        }

        public override void ResetItemTimeout(HttpContext context, String id) {
            Debug.Trace("SqlSessionStateStore", "Calling Sql ResetTimeout, id=" + id);
            Debug.Assert(context != null, "context != null");

            bool                usePooling = true;
            SqlStateConnection  conn = null;

            try {
                SessionIDManager.CheckIdLength(id, true /* throwOnFail */);

                conn = GetConnection(id, ref usePooling);
                SqlCommand cmd = conn.TempResetTimeout;
                cmd.Parameters[0].Value = id + _partitionInfo.AppSuffix;
                SqlExecuteNonQueryWithRetry(cmd, false, null);
            }
            finally {
                DisposeOrReuseConnection(ref conn, usePooling);
            }
        }

        public override SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout)
        {
            Debug.Assert(context != null, "context != null");
            return SessionStateUtility.CreateLegitStoreData(context, null, null, timeout);
        }

        public override void CreateUninitializedItem(HttpContext context, String id, int timeout) {
            Debug.Trace("SqlSessionStateStore", "Calling Sql InsertUninitializedItem, id=" + id);
            Debug.Assert(context != null, "context != null");

            bool                    usePooling = true;
            SqlStateConnection      conn = null;
            byte []                 buf;
            int                     length;

            try {
                SessionIDManager.CheckIdLength(id, true /* throwOnFail */);

                // Store an empty data
                SessionStateUtility.SerializeStoreData(CreateNewStoreData(context, timeout),
                                ITEM_SHORT_LENGTH, out buf, out length, s_configCompressionEnabled);

                conn = GetConnection(id, ref usePooling);

                SqlCommand cmd = conn.TempInsertUninitializedItem;
                cmd.Parameters[0].Value = id + _partitionInfo.AppSuffix;
                cmd.Parameters[1].Size = length;
                cmd.Parameters[1].Value = buf;
                cmd.Parameters[2].Value = timeout;
                SqlExecuteNonQueryWithRetry(cmd, true, id);
            }
            finally {
                DisposeOrReuseConnection(ref conn, usePooling);
            }
        }

        static bool IsInsertPKException(SqlException ex, bool ignoreInsertPKException, string id) {
            // If the severity is greater than 20, we have a serious error.
            // The server usually closes the connection in these cases.
           if (ex != null &&
                ex.Number == SQL_ERROR_PRIMARY_KEY_VIOLATION &&
                ignoreInsertPKException) {

                Debug.Trace("SessionStateClientSet",
                    "Insert failed because of primary key violation; just leave gracefully; id=" + id);

                // It's possible that two threads (from the same session) are creating the session
                // state, both failed to get it first, and now both tried to insert it.
                // One thread may lose with a Primary Key Violation error. If so, that thread will
                // just lose and exit gracefully.
                return true;
           }
           return false;
        }

        static bool IsFatalSqlException(SqlException ex) {
            // We will retry sql operations for serious errors.
            // We consider fatal exceptions any error with severity >= 20.
            // In this case, the SQL server closes the connection.
            // 



            if(ex != null &&
                (ex.Class >= 20 ||
                 ex.Number == SQL_CANNOT_OPEN_DATABASE_FOR_LOGIN ||
                 ex.Number == SQL_TIMEOUT_EXPIRED)) {
                return true;
            }
            return false;
        }

        static void ClearFlagForClearPoolInProgress() {
           // clear s_isClearPoolInProgress if it was set
           Interlocked.CompareExchange(ref s_isClearPoolInProgress, 0, 1);
        }

        static bool CanRetry(SqlException ex, SqlConnection conn,
                                            ref bool isFirstAttempt, ref DateTime endRetryTime) {
            if (s_retryInterval.Seconds <= 0) {
                // no retry policy set
                return false;
            }
            if (!IsFatalSqlException(ex)) {
                if (!isFirstAttempt) {
                    ClearFlagForClearPoolInProgress();
                }
                return false;
            }
            if (isFirstAttempt) {
                // check if someone has called ClearPool for this connection string
                // s_isClearPoolInProgress can be:
                // 0 - no one called ClearPool;
                // 1 - ClearPool is in progress or has already been called

                // If no one called ClearPool (s_isClearPoolInProgress = 0), then
                // make s_isClearPoolInProgress 1 and call clear pool
                if (0 == Interlocked.CompareExchange(ref s_isClearPoolInProgress, 1, 0)) {
                    Debug.Trace("SqlSessionStateStore", "CanRetry: Call ClearPool to destroy the corrupted connections in the pool");
                    SqlConnection.ClearPool(conn);
                }

                // First time we sleep longer than for subsequent retries.
                Thread.Sleep(FIRST_RETRY_SLEEP_TIME);
                endRetryTime = DateTime.UtcNow.Add(s_retryInterval);

                isFirstAttempt = false;
                return true;
            }
            if (DateTime.UtcNow > endRetryTime) {
                // the specified retry interval is up, we can't retry anymore
                if (!isFirstAttempt) {
                    ClearFlagForClearPoolInProgress();
                }
                return false;
            }
            // sleep the specified time and allow retry
            Thread.Sleep(RETRY_SLEEP_TIME);
            return true;
        }

        static int SqlExecuteNonQueryWithRetry(SqlCommand cmd, bool ignoreInsertPKException, string id) {
            bool isFirstAttempt = true;
            DateTime endRetryTime = DateTime.UtcNow;

            while(true) {
                try {
                   if (cmd.Connection.State != ConnectionState.Open) {
                       // reopen the connection
                       // (gets closed if a previous operation throwed a SQL exception with severity >= 20)
                       cmd.Connection.Open();
                   }
                   int result = cmd.ExecuteNonQuery();
                   // the operation succeeded
                   // If we retried, it's possible ClearPool has been called.
                   // In this case, we clear the flag that shows ClearPool is in progress.
                   if (!isFirstAttempt) {
                       ClearFlagForClearPoolInProgress();
                   }
                   return result;
                }
                catch (SqlException e) {
                   // if specified, ignore primary key violations
                   if (IsInsertPKException(e, ignoreInsertPKException, id)) {
                       // ignoreInsertPKException = insert && newItem
                       return -1;
                   }

                   if (!CanRetry(e, cmd.Connection, ref isFirstAttempt, ref endRetryTime)) {
                       // just throw, because not all conditions to retry are satisfied
                       ThrowSqlConnectionException(cmd.Connection, e);
                   }
                }
                catch (Exception e) {
                   // just throw, we have a different Exception
                   ThrowSqlConnectionException(cmd.Connection, e);
                }
            }
        }

        static SqlDataReader SqlExecuteReaderWithRetry(SqlCommand cmd, CommandBehavior cmdBehavior) {
            bool isFirstAttempt = true;
            DateTime endRetryTime = DateTime.UtcNow;

            while(true) {
                try {
                    if (cmd.Connection.State != ConnectionState.Open) {
                       // reopen the connection
                       // (gets closed if a previous operation throwed a SQL exception with severity >= 20)
                        cmd.Connection.Open();
                    }
                    SqlDataReader reader = cmd.ExecuteReader(cmdBehavior);
                    // the operation succeeded
                    if (!isFirstAttempt) {
                        ClearFlagForClearPoolInProgress();
                    }
                    return reader;
                }
                catch (SqlException e) {
                    if (!CanRetry(e, cmd.Connection, ref isFirstAttempt, ref endRetryTime)) {
                        // just throw, default to previous behavior
                        ThrowSqlConnectionException(cmd.Connection, e);
                    }
                }
                catch (Exception e) {
                   // just throw, we have a different Exception
                   ThrowSqlConnectionException(cmd.Connection, e);
                }
            }
        }


        internal class SqlPartitionInfo : PartitionInfo {
            bool            _useIntegratedSecurity;
            string          _sqlConnectionString;
            string          _tracingPartitionString;
            SupportFlags    _support = SupportFlags.Uninitialized;
            string          _appSuffix;
            object          _lock = new object();
            bool            _sqlInfoInited;

            const string APP_SUFFIX_FORMAT = "x8";
            const int   APPID_MAX = 280;
            const int   SQL_2000_MAJ_VER = 8;

            internal SqlPartitionInfo(ResourcePool rpool, bool useIntegratedSecurity, string sqlConnectionString)
                    : base(rpool) {
                _useIntegratedSecurity = useIntegratedSecurity;
                _sqlConnectionString = sqlConnectionString;
                Debug.Trace("PartitionInfo", "Created a new info, sqlConnectionString=" + sqlConnectionString);
            }

            internal bool UseIntegratedSecurity {
                get { return _useIntegratedSecurity; }
            }

            internal string SqlConnectionString {
                get { return _sqlConnectionString; }
            }

            internal SupportFlags SupportFlags {
                get { return _support; }
                set { _support = value; }
            }

            protected override string TracingPartitionString {
                get {
                    if (_tracingPartitionString == null) {
                        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(_sqlConnectionString);
                        builder.Password = String.Empty;
                        builder.UserID = String.Empty;
                        _tracingPartitionString = builder.ConnectionString;
                    }
                    return _tracingPartitionString;
                }
            }

            internal string AppSuffix {
                get { return _appSuffix; }
            }

            void GetServerSupportOptions(SqlConnection sqlConnection) {
                Debug.Assert(SupportFlags == SupportFlags.Uninitialized);

                SqlCommand      cmd;
                SqlDataReader   reader = null;
                SupportFlags    flags = SupportFlags.None;
                bool            v2 = false;
                SqlParameter    p;

                // First, check if the SQL server is running Whidbey scripts
                cmd = new SqlCommand("Select name from sysobjects where type = 'P' and name = 'TempGetVersion'", sqlConnection);
                cmd.CommandType = CommandType.Text;

                using(reader = SqlExecuteReaderWithRetry(cmd, CommandBehavior.SingleRow)) {
                    if (reader.Read()) {
                        // This function first appears in Whidbey (v2).  So we know it's
                        // at least 2.0 even without reading its content.
                        v2 = true;
                    }
                }

                if (!v2) {

                    if (s_usePartition) {
                        throw new HttpException(
                                SR.GetString(SR.Need_v2_SQL_Server_partition_resolver,
                                            s_configPartitionResolverType, sqlConnection.DataSource, sqlConnection.Database));
                    }
                    else {
                        throw new HttpException(
                            SR.GetString(SR.Need_v2_SQL_Server));
                    }
                }

                // Then, see if it's SQL 2000 or above

                cmd = new SqlCommand("dbo.GetMajorVersion", sqlConnection);
                cmd.CommandType = CommandType.StoredProcedure;
                p = cmd.Parameters.Add(new SqlParameter("@@ver", SqlDbType.Int));
                p.Direction = ParameterDirection.Output;

                SqlExecuteNonQueryWithRetry(cmd, false, null);
                try {
                    if ((int)p.Value >= SQL_2000_MAJ_VER) {
                        // For details, see the extensive doc in DoGet method.
                        flags |= SupportFlags.GetLockAge;
                    }

                    Debug.Trace("PartitionInfo", "SupportFlags initialized to " + flags);

                    SupportFlags = flags;
                }
                catch (Exception e) {
                    SqlSessionStateStore.ThrowSqlConnectionException(sqlConnection, e);
                }

            }


            internal void InitSqlInfo(SqlConnection sqlConnection) {
                if (_sqlInfoInited) {
                    return;
                }

                lock (_lock) {
                    if (_sqlInfoInited) {
                        return;
                    }

                    GetServerSupportOptions(sqlConnection);

                    // Get AppSuffix info

                    SqlParameter p;

                    SqlCommand  cmdTempGetAppId = new SqlCommand("dbo.TempGetAppID", sqlConnection);
                    cmdTempGetAppId.CommandType = CommandType.StoredProcedure;
                    cmdTempGetAppId.CommandTimeout = s_commandTimeout;

                    // AppDomainAppId will contain the whole metabase path of the request's app
                    // e.g. /lm/w3svc/1/root/fxtest
                    p = cmdTempGetAppId.Parameters.Add(new SqlParameter("@appName", SqlDbType.VarChar, APPID_MAX));
                    p.Value = HttpRuntime.AppDomainAppId;

                    p = cmdTempGetAppId.Parameters.Add(new SqlParameter("@appId", SqlDbType.Int));
                    p.Direction = ParameterDirection.Output;
                    p.Value = Convert.DBNull;

                    cmdTempGetAppId.ExecuteNonQuery();
                    Debug.Assert(!Convert.IsDBNull(p), "!Convert.IsDBNull(p)");
                    int appId = (int) p.Value;
                    _appSuffix = (appId).ToString(APP_SUFFIX_FORMAT, CultureInfo.InvariantCulture);

                    _sqlInfoInited = true;
                }
            }
        };

        /*
            Here are all the sprocs created for session state and how they're used:

            CreateTempTables
            - Called during setup

            DeleteExpiredSessions
            - Called by SQL agent to remove expired sessions

            GetHashCode
            - Called by sproc TempGetAppID

            GetMajorVersion
            - Called during setup

            TempGetAppID
            - Called when an asp.net application starts up

            TempGetStateItem
            - Used for ReadOnly session state
            - Called by v1 asp.net
            - Called by v1.1 asp.net against SQL 7

            TempGetStateItem2
            - Used for ReadOnly session state
            - Called by v1.1 asp.net against SQL 2000

            TempGetStateItem3
            - Used for ReadOnly session state
            - Called by v2 asp.net

            TempGetStateItemExclusive
            - Called by v1 asp.net
            - Called by v1.1 asp.net against SQL 7

            TempGetStateItemExclusive2
            - Called by v1.1 asp.net against SQL 2000

            TempGetStateItemExclusive3
            - Called by v2 asp.net

            TempGetVersion
            - Called by v2 asp.net when an application starts up

            TempInsertStateItemLong
            - Used when creating a new session state with size > 7000 bytes

            TempInsertStateItemShort
            - Used when creating a new session state with size <= 7000 bytes

            TempInsertUninitializedItem
            - Used when creating a new uninitilized session state (cookieless="true" and regenerateExpiredSessionId="true" in config)

            TempReleaseStateItemExclusive
            - Used when a request that has acquired the session state (exclusively) hit an error during the page execution

            TempRemoveStateItem
            - Used when a session is abandoned

            TempResetTimeout
            - Used when a request (with an active session state) is handled by an HttpHandler which doesn't support IRequiresSessionState interface.

            TempUpdateStateItemLong
            - Used when updating a session state with size > 7000 bytes

            TempUpdateStateItemLongNullShort
            - Used when updating a session state where original size <= 7000 bytes but new size > 7000 bytes

            TempUpdateStateItemShort
            - Used when updating a session state with size <= 7000 bytes

            TempUpdateStateItemShortNullLong
            - Used when updating a session state where original size > 7000 bytes but new size <= 7000 bytes

        */
        class SqlStateConnection : IDisposable {
            SqlConnection   _sqlConnection;
            SqlCommand      _cmdTempGet;
            SqlCommand      _cmdTempGetExclusive;
            SqlCommand      _cmdTempReleaseExclusive;
            SqlCommand      _cmdTempInsertShort;
            SqlCommand      _cmdTempInsertLong;
            SqlCommand      _cmdTempUpdateShort;
            SqlCommand      _cmdTempUpdateShortNullLong;
            SqlCommand      _cmdTempUpdateLong;
            SqlCommand      _cmdTempUpdateLongNullShort;
            SqlCommand      _cmdTempRemove;
            SqlCommand      _cmdTempResetTimeout;
            SqlCommand      _cmdTempInsertUninitializedItem;

            SqlPartitionInfo    _partitionInfo;

            internal SqlStateConnection(SqlPartitionInfo sqlPartitionInfo, TimeSpan retryInterval) {
                Debug.Trace("SessionStateConnectionIdentity", "Connecting under " + WindowsIdentity.GetCurrent().Name);

                _partitionInfo = sqlPartitionInfo;
                _sqlConnection = new SqlConnection(sqlPartitionInfo.SqlConnectionString);

                bool isFirstAttempt = true;
                DateTime endRetryTime = DateTime.UtcNow;

                while(true) {
                    try {
                        _sqlConnection.Open();
                        // the operation succeeded, exit the loop
                        if(!isFirstAttempt) {
                            ClearFlagForClearPoolInProgress();
                        }
                        break;
                    }
                    catch (SqlException e) {
                        if (e != null &&
                            (e.Number == SQL_LOGIN_FAILED ||
                             e.Number == SQL_LOGIN_FAILED_2 ||
                             e.Number == SQL_LOGIN_FAILED_3))
                        {
                            string  user;

                            SqlConnectionStringBuilder  scsb = new SqlConnectionStringBuilder(sqlPartitionInfo.SqlConnectionString);
                            if (scsb.IntegratedSecurity) {
                                user = WindowsIdentity.GetCurrent().Name;
                            }
                            else {
                                user = scsb.UserID;
                            }

                            HttpException outerException = new HttpException(
                                    SR.GetString(SR.Login_failed_sql_session_database, user ), e);

                            outerException.SetFormatter(new UseLastUnhandledErrorFormatter(outerException));
                            ClearConnectionAndThrow(outerException);
                        }
                        if (!CanRetry(e, _sqlConnection, ref isFirstAttempt, ref endRetryTime))
                        {
                            // just throw, the retry conditions are not satisfied
                            ClearConnectionAndThrow(e);
                        }
                    }
                    catch (Exception e) {
                        // just throw, we have a different Exception
                        ClearConnectionAndThrow(e);
                    }
                }

                try {
                    _partitionInfo.InitSqlInfo(_sqlConnection);
                    Debug.Assert(sqlPartitionInfo.SupportFlags != SupportFlags.Uninitialized);

                    PerfCounters.IncrementCounter(AppPerfCounter.SESSION_SQL_SERVER_CONNECTIONS);
                }
                catch {
                    Dispose();
                    throw;
                }
            }

            void ClearConnectionAndThrow(Exception e) {
                SqlConnection connection = _sqlConnection;
                _sqlConnection = null;
                ThrowSqlConnectionException(connection, e);
            }

             internal void ClearAllParameters() {
                 ClearAllParameters(_cmdTempGet);
                 ClearAllParameters(_cmdTempGetExclusive);
                 ClearAllParameters(_cmdTempReleaseExclusive);
                 ClearAllParameters(_cmdTempInsertShort);
                 ClearAllParameters(_cmdTempInsertLong);
                 ClearAllParameters(_cmdTempUpdateShort);
                 ClearAllParameters(_cmdTempUpdateShortNullLong);
                 ClearAllParameters(_cmdTempUpdateLong);
                 ClearAllParameters(_cmdTempUpdateLongNullShort);
                 ClearAllParameters(_cmdTempRemove);
                 ClearAllParameters(_cmdTempResetTimeout);
                 ClearAllParameters(_cmdTempInsertUninitializedItem);
             }
 
             internal void ClearAllParameters(SqlCommand cmd) {
                 if (cmd == null) {
                     return;
                 }

                 foreach (SqlParameter param in cmd.Parameters) {
                     param.Value = Convert.DBNull;
                 }
             }

            internal SqlCommand TempGet {
                get {
                    if (_cmdTempGet == null) {
                        SqlParameter p;

                        _cmdTempGet = new SqlCommand("dbo.TempGetStateItem3", _sqlConnection);
                        _cmdTempGet.CommandType = CommandType.StoredProcedure;
                        _cmdTempGet.CommandTimeout = s_commandTimeout;

                        // Use a different set of parameters for the sprocs that support GetLockAge
                        if ((_partitionInfo.SupportFlags &  SupportFlags.GetLockAge) != 0) {
                            _cmdTempGet.Parameters.Add(new SqlParameter("@id", SqlDbType.NVarChar, ID_LENGTH));
                            p = _cmdTempGet.Parameters.Add(new SqlParameter("@itemShort", SqlDbType.VarBinary, ITEM_SHORT_LENGTH));
                            p.Direction = ParameterDirection.Output;
                            p = _cmdTempGet.Parameters.Add(new SqlParameter("@locked", SqlDbType.Bit));
                            p.Direction = ParameterDirection.Output;
                            p = _cmdTempGet.Parameters.Add(new SqlParameter("@lockAge", SqlDbType.Int));
                            p.Direction = ParameterDirection.Output;
                            p = _cmdTempGet.Parameters.Add(new SqlParameter("@lockCookie", SqlDbType.Int));
                            p.Direction = ParameterDirection.Output;
                            p = _cmdTempGet.Parameters.Add(new SqlParameter("@actionFlags", SqlDbType.Int));
                            p.Direction = ParameterDirection.Output;
                        }
                        else {
                            _cmdTempGet.Parameters.Add(new SqlParameter("@id", SqlDbType.NVarChar, ID_LENGTH));
                            p = _cmdTempGet.Parameters.Add(new SqlParameter("@itemShort", SqlDbType.VarBinary, ITEM_SHORT_LENGTH));
                            p.Direction = ParameterDirection.Output;
                            p = _cmdTempGet.Parameters.Add(new SqlParameter("@locked", SqlDbType.Bit));
                            p.Direction = ParameterDirection.Output;
                            p = _cmdTempGet.Parameters.Add(new SqlParameter("@lockDate", SqlDbType.DateTime));
                            p.Direction = ParameterDirection.Output;
                            p = _cmdTempGet.Parameters.Add(new SqlParameter("@lockCookie", SqlDbType.Int));
                            p.Direction = ParameterDirection.Output;
                            p = _cmdTempGet.Parameters.Add(new SqlParameter("@actionFlags", SqlDbType.Int));
                            p.Direction = ParameterDirection.Output;
                        }
                    }

                    return _cmdTempGet;
                }
            }

            internal SqlCommand TempGetExclusive {
                get {
                    if (_cmdTempGetExclusive == null) {
                        SqlParameter p;

                        _cmdTempGetExclusive = new SqlCommand("dbo.TempGetStateItemExclusive3", _sqlConnection);
                        _cmdTempGetExclusive.CommandType = CommandType.StoredProcedure;
                        _cmdTempGetExclusive.CommandTimeout = s_commandTimeout;

                        // Use a different set of parameters for the sprocs that support GetLockAge
                        if ((_partitionInfo.SupportFlags &  SupportFlags.GetLockAge) != 0) {
                            _cmdTempGetExclusive.Parameters.Add(new SqlParameter("@id", SqlDbType.NVarChar, ID_LENGTH));
                            p = _cmdTempGetExclusive.Parameters.Add(new SqlParameter("@itemShort", SqlDbType.VarBinary, ITEM_SHORT_LENGTH));
                            p.Direction = ParameterDirection.Output;
                            p = _cmdTempGetExclusive.Parameters.Add(new SqlParameter("@locked", SqlDbType.Bit));
                            p.Direction = ParameterDirection.Output;
                            p = _cmdTempGetExclusive.Parameters.Add(new SqlParameter("@lockAge", SqlDbType.Int));
                            p.Direction = ParameterDirection.Output;
                            p = _cmdTempGetExclusive.Parameters.Add(new SqlParameter("@lockCookie", SqlDbType.Int));
                            p.Direction = ParameterDirection.Output;
                            p = _cmdTempGetExclusive.Parameters.Add(new SqlParameter("@actionFlags", SqlDbType.Int));
                            p.Direction = ParameterDirection.Output;
                        }
                        else {
                            _cmdTempGetExclusive.Parameters.Add(new SqlParameter("@id", SqlDbType.NVarChar, ID_LENGTH));
                            p = _cmdTempGetExclusive.Parameters.Add(new SqlParameter("@itemShort", SqlDbType.VarBinary, ITEM_SHORT_LENGTH));
                            p.Direction = ParameterDirection.Output;
                            p = _cmdTempGetExclusive.Parameters.Add(new SqlParameter("@locked", SqlDbType.Bit));
                            p.Direction = ParameterDirection.Output;
                            p = _cmdTempGetExclusive.Parameters.Add(new SqlParameter("@lockDate", SqlDbType.DateTime));
                            p.Direction = ParameterDirection.Output;
                            p = _cmdTempGetExclusive.Parameters.Add(new SqlParameter("@lockCookie", SqlDbType.Int));
                            p.Direction = ParameterDirection.Output;
                            p = _cmdTempGetExclusive.Parameters.Add(new SqlParameter("@actionFlags", SqlDbType.Int));
                            p.Direction = ParameterDirection.Output;
                        }
                    }

                    return _cmdTempGetExclusive;
                }
            }

            internal SqlCommand TempReleaseExclusive {
                get {
                    if (_cmdTempReleaseExclusive == null) {
                        /* ReleaseExlusive */
                        _cmdTempReleaseExclusive = new SqlCommand("dbo.TempReleaseStateItemExclusive", _sqlConnection);
                        _cmdTempReleaseExclusive.CommandType = CommandType.StoredProcedure;
                        _cmdTempReleaseExclusive.CommandTimeout = s_commandTimeout;
                        _cmdTempReleaseExclusive.Parameters.Add(new SqlParameter("@id", SqlDbType.NVarChar, ID_LENGTH));
                        _cmdTempReleaseExclusive.Parameters.Add(new SqlParameter("@lockCookie", SqlDbType.Int));
                    }

                    return _cmdTempReleaseExclusive;
                }
            }

            internal SqlCommand TempInsertLong {
                get {
                    if (_cmdTempInsertLong == null) {
                        _cmdTempInsertLong = new SqlCommand("dbo.TempInsertStateItemLong", _sqlConnection);
                        _cmdTempInsertLong.CommandType = CommandType.StoredProcedure;
                        _cmdTempInsertLong.CommandTimeout = s_commandTimeout;
                        _cmdTempInsertLong.Parameters.Add(new SqlParameter("@id", SqlDbType.NVarChar, ID_LENGTH));
                        _cmdTempInsertLong.Parameters.Add(new SqlParameter("@itemLong", SqlDbType.Image, 8000));
                        _cmdTempInsertLong.Parameters.Add(new SqlParameter("@timeout", SqlDbType.Int));
                    }

                    return _cmdTempInsertLong;
                }
            }

            internal SqlCommand TempInsertShort {
                get {
                    /* Insert */
                    if (_cmdTempInsertShort == null) {
                        _cmdTempInsertShort = new SqlCommand("dbo.TempInsertStateItemShort", _sqlConnection);
                        _cmdTempInsertShort.CommandType = CommandType.StoredProcedure;
                        _cmdTempInsertShort.CommandTimeout = s_commandTimeout;
                        _cmdTempInsertShort.Parameters.Add(new SqlParameter("@id", SqlDbType.NVarChar, ID_LENGTH));
                        _cmdTempInsertShort.Parameters.Add(new SqlParameter("@itemShort", SqlDbType.VarBinary, ITEM_SHORT_LENGTH));
                        _cmdTempInsertShort.Parameters.Add(new SqlParameter("@timeout", SqlDbType.Int));
                    }

                    return _cmdTempInsertShort;
                }
            }

            internal SqlCommand TempUpdateLong {
                get {
                    if (_cmdTempUpdateLong == null) {
                        _cmdTempUpdateLong = new SqlCommand("dbo.TempUpdateStateItemLong", _sqlConnection);
                        _cmdTempUpdateLong.CommandType = CommandType.StoredProcedure;
                        _cmdTempUpdateLong.CommandTimeout = s_commandTimeout;
                        _cmdTempUpdateLong.Parameters.Add(new SqlParameter("@id", SqlDbType.NVarChar, ID_LENGTH));
                        _cmdTempUpdateLong.Parameters.Add(new SqlParameter("@itemLong", SqlDbType.Image, 8000));
                        _cmdTempUpdateLong.Parameters.Add(new SqlParameter("@timeout", SqlDbType.Int));
                        _cmdTempUpdateLong.Parameters.Add(new SqlParameter("@lockCookie", SqlDbType.Int));
                    }

                    return _cmdTempUpdateLong;
                }
            }

            internal SqlCommand TempUpdateShort {
                get {
                    /* Update */
                    if (_cmdTempUpdateShort == null) {
                        _cmdTempUpdateShort = new SqlCommand("dbo.TempUpdateStateItemShort", _sqlConnection);
                        _cmdTempUpdateShort.CommandType = CommandType.StoredProcedure;
                        _cmdTempUpdateShort.CommandTimeout = s_commandTimeout;
                        _cmdTempUpdateShort.Parameters.Add(new SqlParameter("@id", SqlDbType.NVarChar, ID_LENGTH));
                        _cmdTempUpdateShort.Parameters.Add(new SqlParameter("@itemShort", SqlDbType.VarBinary, ITEM_SHORT_LENGTH));
                        _cmdTempUpdateShort.Parameters.Add(new SqlParameter("@timeout", SqlDbType.Int));
                        _cmdTempUpdateShort.Parameters.Add(new SqlParameter("@lockCookie", SqlDbType.Int));
                    }

                    return _cmdTempUpdateShort;

                }
            }

            internal SqlCommand TempUpdateShortNullLong {
                get {
                    if (_cmdTempUpdateShortNullLong == null) {
                        _cmdTempUpdateShortNullLong = new SqlCommand("dbo.TempUpdateStateItemShortNullLong", _sqlConnection);
                        _cmdTempUpdateShortNullLong.CommandType = CommandType.StoredProcedure;
                        _cmdTempUpdateShortNullLong.CommandTimeout = s_commandTimeout;
                        _cmdTempUpdateShortNullLong.Parameters.Add(new SqlParameter("@id", SqlDbType.NVarChar, ID_LENGTH));
                        _cmdTempUpdateShortNullLong.Parameters.Add(new SqlParameter("@itemShort", SqlDbType.VarBinary, ITEM_SHORT_LENGTH));
                        _cmdTempUpdateShortNullLong.Parameters.Add(new SqlParameter("@timeout", SqlDbType.Int));
                        _cmdTempUpdateShortNullLong.Parameters.Add(new SqlParameter("@lockCookie", SqlDbType.Int));
                    }

                    return _cmdTempUpdateShortNullLong;
                }
            }

            internal SqlCommand TempUpdateLongNullShort {
                get {
                    if (_cmdTempUpdateLongNullShort == null) {
                        _cmdTempUpdateLongNullShort = new SqlCommand("dbo.TempUpdateStateItemLongNullShort", _sqlConnection);
                        _cmdTempUpdateLongNullShort.CommandType = CommandType.StoredProcedure;
                        _cmdTempUpdateLongNullShort.CommandTimeout = s_commandTimeout;
                        _cmdTempUpdateLongNullShort.Parameters.Add(new SqlParameter("@id", SqlDbType.NVarChar, ID_LENGTH));
                        _cmdTempUpdateLongNullShort.Parameters.Add(new SqlParameter("@itemLong", SqlDbType.Image, 8000));
                        _cmdTempUpdateLongNullShort.Parameters.Add(new SqlParameter("@timeout", SqlDbType.Int));
                        _cmdTempUpdateLongNullShort.Parameters.Add(new SqlParameter("@lockCookie", SqlDbType.Int));
                    }

                    return _cmdTempUpdateLongNullShort;
                }
            }

            internal SqlCommand TempRemove {
                get {
                    if (_cmdTempRemove == null) {
                        /* Remove */
                        _cmdTempRemove = new SqlCommand("dbo.TempRemoveStateItem", _sqlConnection);
                        _cmdTempRemove.CommandType = CommandType.StoredProcedure;
                        _cmdTempRemove.CommandTimeout = s_commandTimeout;
                        _cmdTempRemove.Parameters.Add(new SqlParameter("@id", SqlDbType.NVarChar, ID_LENGTH));
                        _cmdTempRemove.Parameters.Add(new SqlParameter("@lockCookie", SqlDbType.Int));

                    }

                    return _cmdTempRemove;
                }
            }

            internal SqlCommand TempInsertUninitializedItem {
                get {
                    if (_cmdTempInsertUninitializedItem == null) {
                        _cmdTempInsertUninitializedItem = new SqlCommand("dbo.TempInsertUninitializedItem", _sqlConnection);
                        _cmdTempInsertUninitializedItem.CommandType = CommandType.StoredProcedure;
                        _cmdTempInsertUninitializedItem.CommandTimeout = s_commandTimeout;
                        _cmdTempInsertUninitializedItem.Parameters.Add(new SqlParameter("@id", SqlDbType.NVarChar, ID_LENGTH));
                        _cmdTempInsertUninitializedItem.Parameters.Add(new SqlParameter("@itemShort", SqlDbType.VarBinary, ITEM_SHORT_LENGTH));
                        _cmdTempInsertUninitializedItem.Parameters.Add(new SqlParameter("@timeout", SqlDbType.Int));
                    }

                    return _cmdTempInsertUninitializedItem;
                }
            }

            internal SqlCommand TempResetTimeout {
                get {
                    if (_cmdTempResetTimeout == null) {
                        /* ResetTimeout */
                        _cmdTempResetTimeout = new SqlCommand("dbo.TempResetTimeout", _sqlConnection);
                        _cmdTempResetTimeout.CommandType = CommandType.StoredProcedure;
                        _cmdTempResetTimeout.CommandTimeout = s_commandTimeout;
                        _cmdTempResetTimeout.Parameters.Add(new SqlParameter("@id", SqlDbType.NVarChar, ID_LENGTH));
                    }

                    return _cmdTempResetTimeout;
                }
            }

            public void Dispose() {
                Debug.Trace("ResourcePool", "Disposing SqlStateConnection");
                if (_sqlConnection != null) {
                    _sqlConnection.Close();
                    _sqlConnection = null;
                    PerfCounters.DecrementCounter(AppPerfCounter.SESSION_SQL_SERVER_CONNECTIONS);
                }
            }

            internal SqlConnection Connection {
                get { return _sqlConnection; }
            }
        }
    }
}
