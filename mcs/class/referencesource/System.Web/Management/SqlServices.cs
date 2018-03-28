//------------------------------------------------------------------------------
// <copyright file="Configuration.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Management {
    using System;
    using System.Web.Util;
    using System.IO;
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading;
    using System.Text.RegularExpressions;
    using System.Text;
    using System.Security;
    using System.Security.Permissions;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Collections;


    [Flags]
    public enum SqlFeatures : int {
        None =                      0x00000000,
        Membership =                0x00000001,
        Profile =                   0x00000002,
        RoleManager =               0x00000004,
        Personalization =           0x00000008,
        SqlWebEventProvider=        0x00000010,

        // We need to add 0x40000000 to All in order to differentiate between All, and
        // an combination of all other flags.  Because if the user pass in All, we have
        // to remove the common tables too.
        All =               Membership|Profile|RoleManager|
                            Personalization|SqlWebEventProvider|(int)0x40000000,
    };

    public enum SessionStateType {
        Temporary,
        Persisted,
        Custom,
    }

    [Serializable()]
    public sealed class SqlExecutionException : SystemException {
        string          _server;
        string          _database;
        string          _sqlFile;
        string          _commands;
        SqlException    _sqlException;

        public SqlExecutionException(string message, string server, string database,
                    string sqlFile, string commands, SqlException sqlException)
        : base(message) {
            _server = server;
            _database = database;
            _sqlFile = sqlFile;
            _commands = commands;
            _sqlException = sqlException;
        }

        public SqlExecutionException(String message)
        : base(message) {
        }

        public SqlExecutionException(string message, Exception innerException)
        : base (message, innerException) {
        }

        public SqlExecutionException() {
        }

        private SqlExecutionException(SerializationInfo info, StreamingContext context)
           :base(info, context) {
           _server = info.GetString("_server");
           _database = info.GetString("_database");
           _sqlFile = info.GetString("_sqlFile");
           _commands = info.GetString("_commands");
           _sqlException = (SqlException)info.GetValue("_sqlException", typeof(SqlException));
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_server", _server);
            info.AddValue("_database", _database);
            info.AddValue("_sqlFile", _sqlFile);
            info.AddValue("_commands", _commands);
            info.AddValue("_sqlException", _sqlException);
        }

        // Name of the server we're connecting to
        public string Server {
            get { return _server; }
        }

        // Name of database we use
        public string Database {
            get { return _database; }
        }

        // Name of the SQL file we load
        public string SqlFile {
            get { return _sqlFile; }
        }

        // The set of batched commands that failed
        public string Commands {
            get { return _commands; }
        }

        // The SQL exeception from the failure
        public SqlException Exception {
            get { return _sqlException; }
        }
    }

    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.High)]
    public static class SqlServices {
        public static void Install(string server, string user, string password, string database, SqlFeatures features) {
            SetupApplicationServices(server, user, password, false, null, database, null, features, true);
        }

        // For trusted connection
        public static void Install(string server, string database, SqlFeatures features) {
            SetupApplicationServices(server, null, null, true, null, database, null, features, true);
        }

        internal static void Install( string database, string dbFileName, string connectionString ) {
            SetupApplicationServices( null, null, null, false, connectionString, database, dbFileName, SqlFeatures.All, true );
        }

        // For connection using connection string
        public static void Install(string database, SqlFeatures features, string connectionString) {
            SetupApplicationServices(null, null, null, true, connectionString, database, null, features, true);
        }

        public static void Uninstall(string server, string user, string password, string database, SqlFeatures features) {
            SetupApplicationServices(server, user, password, false, null, database, null, features, false);
        }

        // For trusted connection
        public static void Uninstall(string server, string database, SqlFeatures features) {
            SetupApplicationServices(server, null, null, true, null, database, null, features, false);
        }

        // For connection using connection string
        public static void Uninstall(string database, SqlFeatures features, string connectionString) {
            SetupApplicationServices(null, null, null, true, connectionString, database, null, features, false);
        }

        public static void InstallSessionState(string server, string user, string password, string customDatabase, SessionStateType type) {
            SetupSessionState(server, user, password, false, null, customDatabase, type, true);
        }

        // For trusted connection
        public static void InstallSessionState(string server, string customDatabase, SessionStateType type) {
            SetupSessionState(server, null, null, true, null, customDatabase, type, true);
        }

        // For connection using connection string
        public static void InstallSessionState(string customDatabase, SessionStateType type, string connectionString) {
            SetupSessionState(null, null, null, true, connectionString, customDatabase, type, true);
        }


        public static void UninstallSessionState(string server, string user, string password, string customDatabase, SessionStateType type) {
            SetupSessionState(server, user, password, false, null, customDatabase, type, false);
        }

        // For trusted connection
        public static void UninstallSessionState(string server, string customDatabase, SessionStateType type) {
            SetupSessionState(server, null, null, true, null, customDatabase, type, false);
        }

        // For connection using connection string
        public static void UninstallSessionState(string customDatabase, SessionStateType type, string connectionString) {
            SetupSessionState(null, null, null, true, connectionString, customDatabase, type, false);
        }

        // Used by suite
        internal static ArrayList ApplicationServiceTables {
            get {
                ArrayList   tables = new ArrayList();
                for (int i=0; i <  s_featureInfos.Length; i++) {
                    tables.InsertRange(tables.Count, s_featureInfos[i]._tablesRemovedInUninstall);
                }
                return tables;
            }
        }

        public static string GenerateSessionStateScripts(bool install, SessionStateType type, string customDatabase) {
            SessionStateParamCheck(type, ref customDatabase);
            string  fullpath = Path.Combine(HttpRuntime.AspInstallDirectory, install ? SESSION_STATE_INSTALL_FILE : SESSION_STATE_UNINSTALL_FILE);
            string  content  = File.ReadAllText(fullpath);
            return FixContent(content, customDatabase, null, true, type);
        }


        // Information about a feature.
        internal struct FeatureInfo {
            internal SqlFeatures    _feature;
            internal string[]       _installFiles;      // SQL file to run to install the feature
            internal string[]       _uninstallFiles;    // SQL file to run to uninstall the feature
            internal string[]       _tablesRemovedInUninstall;     // SQL tables to be removed during uninstall
            internal int            _dataCheckBitMask;

            internal FeatureInfo(SqlFeatures feature, string[] installFiles,
                string[] uninstallFiles, string[] tablesRemovedInUninstall, int dataCheckBitMask) {
                _feature = feature;
                _installFiles = installFiles;
                _uninstallFiles = uninstallFiles;
                _tablesRemovedInUninstall = tablesRemovedInUninstall;
                _dataCheckBitMask = dataCheckBitMask;
            }
        };

        static string INSTALL_COMMON_SQL = "InstallCommon.sql";

        static FeatureInfo[] s_featureInfos = {
            new FeatureInfo(SqlFeatures.Membership,
                            new string[] {INSTALL_COMMON_SQL, "InstallMembership.sql"},
                            new string[] {"UninstallMembership.sql"},
                            new string[] {"aspnet_Membership"},
                            1),

            new FeatureInfo(SqlFeatures.Profile,
                            new string[] {INSTALL_COMMON_SQL, "InstallProfile.sql"},
                            new string[] {"UninstallProfile.sql"},
                            new string[] {"aspnet_Profile"},
                            4),

            new FeatureInfo(SqlFeatures.RoleManager,
                            new string[] {INSTALL_COMMON_SQL, "InstallRoles.sql"},
                            new string[] {"UninstallRoles.sql"},
                            new string[] {"aspnet_Roles", "aspnet_UsersInRoles"},
                            2),

            new FeatureInfo(SqlFeatures.Personalization,
                            new string[] {INSTALL_COMMON_SQL, "InstallPersonalization.sql"},
                            new string[] {"UninstallPersonalization.sql"},
                            new string[] {"aspnet_PersonalizationPerUser", "aspnet_Paths", "aspnet_PersonalizationAllUsers"},
                            8),

            new FeatureInfo(SqlFeatures.SqlWebEventProvider,
                            new string[] {INSTALL_COMMON_SQL, "InstallWebEventSqlProvider.sql"},
                            new string[] {"UninstallWebEventSqlProvider.sql"},
                            new string[] {"aspnet_WebEvent_Events"},
                            16),

            // The following are the files to run in addition to those listed in other feature
            new FeatureInfo(SqlFeatures.All,
                            new string[] {},
                            new string[] {"UninstallCommon.sql"},
                            new string[] {"aspnet_Applications", "aspnet_Users", "aspnet_SchemaVersions"},
                            0x7FFFFFFF),
        };

        // Default database to use if the database name isn't supplied by the user
        static string   DEFAULT_DB = "aspnetdb";
        static string   ASPSTATE_DB = "ASPState";
        static string   SSTYPE_PERSISTED = "sstype_persisted";
        static string   SSTYPE_CUSTOM = "sstype_custom";

        static string   SESSION_STATE_INSTALL_FILE = "InstallSqlState.sql";
        static string   SESSION_STATE_UNINSTALL_FILE = "UninstallSqlState.sql";

        // Return a list of files based on install (bool) and features
        static ArrayList GetFiles(bool install, SqlFeatures features) {
            ArrayList   results = new ArrayList();
            bool        installCommonProcessed = false;

            // Load and modify the sql file for each feature
            for (int i=0; i <  s_featureInfos.Length; i++) {
                string[] sqlFiles = null;

                if (((int)s_featureInfos[i]._feature & (int)features) == (int)s_featureInfos[i]._feature) {
                    // We found one feature
                    if (install) {
                        sqlFiles = s_featureInfos[i]._installFiles;
                    }
                    else {
                        sqlFiles = s_featureInfos[i]._uninstallFiles;
                    }
                }

                if (sqlFiles != null) {
                    for(int j = 0; j < sqlFiles.Length; j++) {
                        string sqlFile = sqlFiles[j];
                        if (sqlFile != null) {
                            // We only need to process InstallCommon.sql once
                            if (sqlFile == INSTALL_COMMON_SQL && installCommonProcessed) {
                                continue;
                            }

                            results.Add(sqlFile);

                            if (!installCommonProcessed && sqlFile == INSTALL_COMMON_SQL) {
                                installCommonProcessed = true;
                            }
                        }
                    }
                }
            }

            return results;
        }

        // Replace the name of the database with the one specified by the caller
        static string FixContent(
            string content,
            string database,
            string dbFileName,
            bool sessionState,
            SessionStateType sessionStatetype
            )
        {
            if (database != null) {
                database = RemoveSquareBrackets(database);
            }

            if (sessionState) {
                if (sessionStatetype == SessionStateType.Temporary) {
                    // No change
                }
                else if (sessionStatetype == SessionStateType.Persisted) {
                    content = content.Replace("'sstype_temp'", "'" + SSTYPE_PERSISTED + "'");
                    content = content.Replace("[tempdb]", "[" + ASPSTATE_DB + "]");
                }
                else if (sessionStatetype == SessionStateType.Custom) {
                    content = content.Replace("'sstype_temp'", "'" + SSTYPE_CUSTOM + "'");
                    content = content.Replace("[tempdb]", "[" + database + "]");
                    content = content.Replace("'ASPState'", "'" + database + "'");
                    content = content.Replace("[ASPState]", "[" + database + "]");
                }
            }
            else {
                content = content.Replace("'aspnetdb'", "'" + database.Replace("'", "''") + "'");
                content = content.Replace("[aspnetdb]", "[" + database + "]");
            }

            if( dbFileName != null )
            {
                if (dbFileName.Contains("[") || dbFileName.Contains("]") || dbFileName.Contains("'"))
                    throw new ArgumentException(SR.GetString(SR.DbFileName_can_not_contain_invalid_chars));
                database = database.TrimStart( '[' );
                database = database.TrimEnd( ']' );

                string logicalFileName = database + "_DAT";
                if (!char.IsLetter(logicalFileName[0]))
                    logicalFileName = "A" + logicalFileName;

                //
                // Build the database options string for SQL Express database
                //
                string dbOptions = "ON ( NAME = " + logicalFileName + ", FILENAME = ''" +
                                    dbFileName + "'', " + "SIZE = 10MB, FILEGROWTH = 5MB )";

                content = content.Replace("SET @dboptions = N'/**/'",
                                            "SET @dboptions = N'" + dbOptions + "'");
            }

            return content;
        }

        static void ExecuteSessionFile(
            string file,
            string server,
            string database,
            string dbFileName,
            SqlConnection connection,
            bool isInstall,
            SessionStateType sessionStatetype
            ) {
            ExecuteFile(file, server, database, dbFileName, connection, true, isInstall, sessionStatetype);
        }

        // Load the SQL file, change the database name within, and execute it.
        static void ExecuteFile(
            string file,
            string server,
            string database,
            string dbFileName,
            SqlConnection connection,
            bool sessionState,
            bool isInstall,
            SessionStateType sessionStatetype
            )
        {
            string          fullpath = Path.Combine(HttpRuntime.AspInstallDirectory, file);
            string          content = File.ReadAllText(fullpath);
            StringReader    sr;
            string          cmdText = null;
            string          cur;
            SqlCommand      sqlCmd;

            Debug.Trace("SqlServices", "Execute File: about to run " + fullpath);
            // We need to replace the name of the database with the one specified by the caller
            if( file.Equals( INSTALL_COMMON_SQL ) )
            {
                content = FixContent(content, database, dbFileName, sessionState, sessionStatetype);
            }
            else
            {
                content = FixContent(content, database, null, sessionState, sessionStatetype);
            }

            sr = new StringReader(content);

            sqlCmd = new SqlCommand(null, connection);
            do {
                bool    run = false;

                // Read a line from a file.  If it's not a "GO", batch it up.
                // It it's a "GO" (or the last line), send over all batched
                // commands over to SQL.

                cur = sr.ReadLine();

                if (cur == null) {
                    run = true;
                }
                else {
                    if (StringUtil.EqualsIgnoreCase(cur.Trim(), "GO"))
                    {
                        run = true;
                    }
                    else {
                        if (cmdText != null) {
                            cmdText += "\n";
                        }

                        cmdText += cur;
                    }
                }

                if (run & cmdText != null) {
                    sqlCmd.CommandText = cmdText;
                    try {
                        sqlCmd.ExecuteNonQuery();
                    }
                    catch (Exception e) {
                        SqlException sqlExpt = e as SqlException;

                        if (sqlExpt != null) {
                            Debug.Trace("SqlServices", "Error executing command.  SqlException:" +
                                "\nMessage=" + sqlExpt.Message +
                                "\nNumber=" + sqlExpt.Number);
                            int expectedError = -1;

                            // There are some errors we might expect.
                            // See VSWhidbey 376433

                            if (cmdText.IndexOf("sp_add_category", StringComparison.Ordinal) > -1) {
                                expectedError = 14261; /* already exists */
                            }
                            else if (cmdText.IndexOf("sp_delete_job", StringComparison.Ordinal) > -1) {
                                expectedError = 14262; /* doesn't exists */

                                if (sessionState && !isInstall) {
                                    throw new SqlExecutionException(SR.GetString(SR.SQL_Services_Error_Deleting_Session_Job),
                                        server, database, file, cmdText, sqlExpt);
                                }
                            }

                            if (sqlExpt.Number == expectedError) {
                                Debug.Trace("SqlServices", "Got expected error: " + expectedError + "; not throwing");
                            }
                            else {
                                throw new SqlExecutionException(
                                    SR.GetString(SR.SQL_Services_Error_Executing_Command,
                                        file, sqlExpt.Number.ToString(CultureInfo.CurrentCulture), sqlExpt.Message),
                                    server, database, file, cmdText, sqlExpt);
                            }
                        }
                    }
#pragma warning disable 1058
                    catch {
                        throw;
                    }
#pragma warning restore 1058
                    cmdText = null;
                }
            } while (cur != null);
        }

        static void ApplicationServicesParamCheck(SqlFeatures features, ref string database) {
            if (features == SqlFeatures.None) {
                return;
            }

            if ((features & SqlFeatures.All) != features) {
                throw new ArgumentException(SR.GetString(SR.SQL_Services_Invalid_Feature));
            }

            // VSWhidbey 355946:
            // SQL will ignore trailing space of database names. It's easier to just
            // trim those spaces here than to fix all the SQL statements to handle
            // errors due to trailing space.
            CheckDatabaseName(ref database);
        }
        private static void CheckDatabaseName(ref string database)
        {
            if (database != null) {
                database = database.TrimEnd();

                if (database.Length == 0)
                    throw new ArgumentException(SR.GetString(SR.SQL_Services_Database_Empty_Or_Space_Only_Arg));

                database = RemoveSquareBrackets(database);

                if (database.Contains("'") || database.Contains("[") || database.Contains("]"))
                    throw new ArgumentException(SR.GetString(SR.SQL_Services_Database_contains_invalid_chars));
            }

            if (database == null) {
                database = DEFAULT_DB;
            }
            else {
                // Wrap it with [] if not already
                //if (!(StringUtil.StringStartsWith(database, '[') && StringUtil.StringEndsWith(database, ']'))) {
                database = "[" + database + "]";
                //}
            }
        }

        public static string GenerateApplicationServicesScripts(bool install, SqlFeatures features, string database) {
            string          content;
            StringBuilder   sb = new StringBuilder();
            ArrayList       files;

            ApplicationServicesParamCheck(features, ref database);

            files = GetFiles(install, features);

            foreach (string sqlFile in files) {
                string fullpath = Path.Combine(HttpRuntime.AspInstallDirectory, sqlFile);
                content = File.ReadAllText(fullpath);
                sb.Append(FixContent(content, database, null, false, SessionStateType.Temporary));
            }

            return sb.ToString();
        }

        static string RemoveSquareBrackets(string database) {
            if (database != null && StringUtil.StringStartsWith(database, '[') && StringUtil.StringEndsWith(database, ']'))
                return database.Substring(1, database.Length-2);
            return database;
        }

        static void EnsureDatabaseExists(string database, SqlConnection sqlConnection) {
            SqlCommand      cmd;
            string          databaseNoSquareBrackets = RemoveSquareBrackets(database);

            // First, make sure the database exists
            cmd = new SqlCommand("SELECT DB_ID(@database)", sqlConnection);
            cmd.Parameters.Add(new SqlParameter("@database", databaseNoSquareBrackets));

            object res = cmd.ExecuteScalar();

            if (res == null || res == System.DBNull.Value) {
                // The database doesn't even exist.
                throw new HttpException(
                    SR.GetString(SR.SQL_Services_Error_Cant_Uninstall_Nonexisting_Database,
                        databaseNoSquareBrackets));
            }
        }

        // Add/Remove all requested general features
        static void SetupApplicationServices(
            string server,
            string user,
            string password,
            bool trusted,
            string connectionString,
            string database,
            string dbFileName,
            SqlFeatures features,
            bool install )
        {
            SqlConnection   sqlConnection   = null;
            ArrayList       files;

            Debug.Trace("SqlServices",
                            "SetupApplicationServices called: server=" + server + ", database=" +
                            database  + ", user=" + user + ", password=" + password +
                            ", trusted=" + trusted + ", connectionString=" + connectionString +
                            ", features=" + features + ", install=" + install);

            ApplicationServicesParamCheck(features, ref database);

            files = GetFiles(install, features);

            try {
                sqlConnection = GetSqlConnection(server, user, password, trusted, connectionString);

                // If uninstall, make sure all the asp.net tables are empty
                if (!install) {
                    EnsureDatabaseExists(database, sqlConnection);
                    string databaseNoSquareBrackets = RemoveSquareBrackets(database);
                    if (sqlConnection.Database != databaseNoSquareBrackets)
                        sqlConnection.ChangeDatabase(databaseNoSquareBrackets);
                    int itablesToCheck = 0;
                    for (int i=0; i <  s_featureInfos.Length; i++)
                        if (((int)s_featureInfos[i]._feature & (int)features) == (int)s_featureInfos[i]._feature)
                            itablesToCheck |= s_featureInfos[i]._dataCheckBitMask;
                    SqlCommand cmd = new SqlCommand("dbo.aspnet_AnyDataInTables", sqlConnection);
                    cmd.Parameters.Add(new SqlParameter("@TablesToCheck", itablesToCheck));
                    cmd.CommandType = CommandType.StoredProcedure;
                    string table = null;
                    try {
                        table = cmd.ExecuteScalar() as string;
                    } catch (SqlException e) {
                        if (e.Number != 2812)
                            throw;
                    }
                    if (!string.IsNullOrEmpty(table))
                        throw new NotSupportedException(
                                    SR.GetString(SR.SQL_Services_Error_Cant_Uninstall_Nonempty_Table,
                                        table, database));
                }

                // Load and run the sql file for each feature
                foreach (string sqlFile in files) {
                    ExecuteFile(sqlFile, server, database, dbFileName, sqlConnection, false, false, SessionStateType.Temporary);
                }
            }
            finally {
                if (sqlConnection != null) {
                    try {
                        sqlConnection.Close();
                    }
                    catch {
                    }
                    finally {
                        sqlConnection = null;
                    }
                }
            }

        }

        static void SessionStateParamCheck(SessionStateType type, ref string customDatabase) {
            if (type == SessionStateType.Custom && String.IsNullOrEmpty(customDatabase)) {
                throw new ArgumentException(
                    SR.GetString(SR.SQL_Services_Error_missing_custom_database), "customDatabase");
            }

            if (type != SessionStateType.Custom && customDatabase != null) {
                throw new ArgumentException(
                    SR.GetString(SR.SQL_Services_Error_Cant_use_custom_database), "customDatabase");
            }
            CheckDatabaseName(ref customDatabase);
        }

        static void SetupSessionState(string server, string user, string password, bool trusted,
                           string connectionString, string customDatabase, SessionStateType type, bool install) {
            SqlConnection   sqlConnection   = null;

            Debug.Trace("SqlServices",
                            "SetupSessionState called: server=" + server + ", customDatabase=" +
                            customDatabase  + ", user=" + user + ", password=" + password +
                            ", trusted=" + trusted + ", connectionString=" + connectionString +
                            ", type=" + type + ", install=" + install);

            SessionStateParamCheck(type, ref customDatabase);

            try {
                sqlConnection = GetSqlConnection(server, user, password, trusted, connectionString);

                if (!install && type == SessionStateType.Custom) {
                    EnsureDatabaseExists(customDatabase, sqlConnection);
                }

                // Load and run the sql file for each feature
                ExecuteSessionFile(install ? SESSION_STATE_INSTALL_FILE : SESSION_STATE_UNINSTALL_FILE,
                                server, customDatabase, null, sqlConnection, install, type);
            }
            finally {
                if (sqlConnection != null) {
                    try {
                        sqlConnection.Close();
                    }
                    catch {
                    }
                    finally {
                        sqlConnection = null;
                    }
                }
            }

        }

        static string ConstructConnectionString(string server, string user, string password, bool trusted) {
            string          connectionString = null;

            // Construct the connection string

            if (String.IsNullOrEmpty(server)) {
                throw ExceptionUtil.ParameterNullOrEmpty("server");
            }

            connectionString += "server=" + server;

            if (trusted) {
                connectionString += ";Trusted_Connection=true;";
            }
            else {
                if (String.IsNullOrEmpty(user)) {
                    throw ExceptionUtil.ParameterNullOrEmpty("user");
                }

                connectionString += ";UID=" + user + ";" + "PWD=" + password + ";";
            }

            return connectionString;

        }

        static SqlConnection GetSqlConnection(string server, string user, string password,
                                            bool trusted, string connectionString) {
            SqlConnection   sqlConnection;

            if (connectionString == null) {
                connectionString = ConstructConnectionString(server, user, password, trusted);
            }

            try {
                Debug.Trace("SqlServices", "Connecting to SQL: " + connectionString);
                sqlConnection = new SqlConnection(connectionString);
                sqlConnection.Open();
            }
            catch (Exception e) {
                sqlConnection = null;
                throw new HttpException(
                    SR.GetString(SR.SQL_Services_Cant_connect_sql_database),
                    e);
            }

            return sqlConnection;
        }
    }
}
