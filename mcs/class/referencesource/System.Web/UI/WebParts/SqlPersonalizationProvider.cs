//------------------------------------------------------------------------------
// <copyright file="SqlPersonalizationProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration.Provider;
    using System.ComponentModel;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Web;
    using System.Web.DataAccess;
    using System.Web.Util;

    /// <devdoc>
    /// The provider used to access the personalization store for WebPart pages from a SQL Server
    /// database.
    /// </devdoc>
    public class SqlPersonalizationProvider : PersonalizationProvider {

        private enum ResetUserStateMode {
            PerInactiveDate,
            PerPaths,
            PerUsers
        }

        private const int maxStringLength = 256;

        private string _applicationName;
        private int    _commandTimeout;
        private string _connectionString;
        private int    _SchemaVersionCheck;

        /// <devdoc>
        /// Initializes an instance of SqlPersonalizationProvider.
        /// </devdoc>
        public SqlPersonalizationProvider() {
        }

        public override string ApplicationName {
            get {
                if (String.IsNullOrEmpty(_applicationName)) {
                    _applicationName = SecUtility.GetDefaultAppName();
                }
                return _applicationName;
            }
            set {
                if (value != null && value.Length > maxStringLength) {
                    throw new ProviderException(SR.GetString(SR.PersonalizationProvider_ApplicationNameExceedMaxLength, maxStringLength.ToString(CultureInfo.CurrentCulture)));
                }
                _applicationName = value;
            }
        }

        /// <devdoc>
        /// </devdoc>
        private SqlParameter CreateParameter(string name, SqlDbType dbType, object value) {
            SqlParameter param = new SqlParameter(name, dbType);

            param.Value = value;
            return param;
        }

        private PersonalizationStateInfoCollection FindSharedState(string path,
                                                                   int pageIndex,
                                                                   int pageSize,
                                                                   out int totalRecords) {
            SqlConnectionHolder connectionHolder = null;
            SqlConnection connection = null;
            SqlDataReader reader = null;
            totalRecords = 0;

            // Extra try-catch block to prevent elevation of privilege attack via exception filter
            try {
                try {
                    connectionHolder = GetConnectionHolder();
                    connection = connectionHolder.Connection;
                    Debug.Assert(connection != null);

                    CheckSchemaVersion( connection );

                    SqlCommand command = new SqlCommand("dbo.aspnet_PersonalizationAdministration_FindState", connection);
                    SetCommandTypeAndTimeout(command);
                    SqlParameterCollection parameters = command.Parameters;

                    SqlParameter parameter = parameters.Add(new SqlParameter("AllUsersScope", SqlDbType.Bit));
                    parameter.Value = true;

                    parameters.AddWithValue("ApplicationName", ApplicationName);
                    parameters.AddWithValue("PageIndex", pageIndex);
                    parameters.AddWithValue("PageSize", pageSize);

                    SqlParameter returnValue = new SqlParameter("@ReturnValue", SqlDbType.Int);
                    returnValue.Direction = ParameterDirection.ReturnValue;
                    parameters.Add(returnValue);

                    parameter = parameters.Add("Path", SqlDbType.NVarChar);
                    if (path != null) {
                        parameter.Value = path;
                    }

                    parameter = parameters.Add("UserName", SqlDbType.NVarChar);
                    parameter = parameters.Add("InactiveSinceDate", SqlDbType.DateTime);

                    reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
                    PersonalizationStateInfoCollection sharedStateInfoCollection = new PersonalizationStateInfoCollection();

                    if (reader != null) {
                        if (reader.HasRows) {
                            while(reader.Read()) {
                                string returnedPath = reader.GetString(0);

                                // Data can be null if there is no data associated with the path
                                DateTime lastUpdatedDate = (reader.IsDBNull(1)) ? DateTime.MinValue :
                                                                DateTime.SpecifyKind(reader.GetDateTime(1), DateTimeKind.Utc);
                                int size = (reader.IsDBNull(2)) ? 0 : reader.GetInt32(2);
                                int userDataSize = (reader.IsDBNull(3)) ? 0 : reader.GetInt32(3);
                                int userCount = (reader.IsDBNull(4)) ? 0 : reader.GetInt32(4);
                                sharedStateInfoCollection.Add(new SharedPersonalizationStateInfo(
                                    returnedPath, lastUpdatedDate, size, userDataSize, userCount));
                            }
                        }

                        // The reader needs to be closed so return value can be accessed
                        // See MSDN doc for SqlParameter.Direction for details.
                        reader.Close();
                        reader = null;
                    }

                    // Set the total count at the end after all operations pass
                    if (returnValue.Value != null && returnValue.Value is int) {
                        totalRecords = (int)returnValue.Value;
                    }

                    return sharedStateInfoCollection;
                }
                finally {
                    if (reader != null) {
                        reader.Close();
                    }

                    if (connectionHolder != null) {
                        connectionHolder.Close();
                        connectionHolder = null;
                    }
                }
            }
            catch {
                throw;
            }
        }

        public override PersonalizationStateInfoCollection FindState(PersonalizationScope scope,
                                                                     PersonalizationStateQuery query,
                                                                     int pageIndex, int pageSize,
                                                                     out int totalRecords) {
            PersonalizationProviderHelper.CheckPersonalizationScope(scope);
            PersonalizationProviderHelper.CheckPageIndexAndSize(pageIndex, pageSize);

            if (scope == PersonalizationScope.Shared) {
                string pathToMatch = null;
                if (query != null) {
                    pathToMatch = StringUtil.CheckAndTrimString(query.PathToMatch, "query.PathToMatch", false, maxStringLength);
                }
                return FindSharedState(pathToMatch, pageIndex, pageSize, out totalRecords);
            }
            else {
                string pathToMatch = null;
                DateTime inactiveSinceDate = PersonalizationAdministration.DefaultInactiveSinceDate;
                string usernameToMatch = null;
                if (query != null) {
                    pathToMatch = StringUtil.CheckAndTrimString(query.PathToMatch, "query.PathToMatch", false, maxStringLength);
                    inactiveSinceDate = query.UserInactiveSinceDate;
                    usernameToMatch = StringUtil.CheckAndTrimString(query.UsernameToMatch, "query.UsernameToMatch", false, maxStringLength);
                }

                return FindUserState(pathToMatch, inactiveSinceDate, usernameToMatch,
                                     pageIndex, pageSize, out totalRecords);
            }
        }

        private PersonalizationStateInfoCollection FindUserState(string path,
                                                                 DateTime inactiveSinceDate,
                                                                 string username,
                                                                 int pageIndex,
                                                                 int pageSize,
                                                                 out int totalRecords) {
            SqlConnectionHolder connectionHolder = null;
            SqlConnection connection = null;
            SqlDataReader reader = null;
            totalRecords = 0;

            // Extra try-catch block to prevent elevation of privilege attack via exception filter
            try {
                try {
                    connectionHolder = GetConnectionHolder();
                    connection = connectionHolder.Connection;
                    Debug.Assert(connection != null);

                    CheckSchemaVersion( connection );

                    SqlCommand command = new SqlCommand("dbo.aspnet_PersonalizationAdministration_FindState", connection);
                    SetCommandTypeAndTimeout(command);
                    SqlParameterCollection parameters = command.Parameters;

                    SqlParameter parameter = parameters.Add(new SqlParameter("AllUsersScope", SqlDbType.Bit));
                    parameter.Value = false;

                    parameters.AddWithValue("ApplicationName", ApplicationName);
                    parameters.AddWithValue("PageIndex", pageIndex);
                    parameters.AddWithValue("PageSize", pageSize);

                    SqlParameter returnValue = new SqlParameter("@ReturnValue", SqlDbType.Int);
                    returnValue.Direction = ParameterDirection.ReturnValue;
                    parameters.Add(returnValue);

                    parameter = parameters.Add("Path", SqlDbType.NVarChar);
                    if (path != null) {
                        parameter.Value = path;
                    }

                    parameter = parameters.Add("UserName", SqlDbType.NVarChar);
                    if (username != null) {
                        parameter.Value = username;
                    }

                    parameter = parameters.Add("InactiveSinceDate", SqlDbType.DateTime);
                    if (inactiveSinceDate != PersonalizationAdministration.DefaultInactiveSinceDate) {
                        parameter.Value = inactiveSinceDate.ToUniversalTime();
                    }

                    reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
                    PersonalizationStateInfoCollection stateInfoCollection = new PersonalizationStateInfoCollection();

                    if (reader != null) {
                        if (reader.HasRows) {
                            while(reader.Read()) {
                                string returnedPath = reader.GetString(0);
                                DateTime lastUpdatedDate = DateTime.SpecifyKind(reader.GetDateTime(1), DateTimeKind.Utc);
                                int size = reader.GetInt32(2);
                                string returnedUsername = reader.GetString(3);
                                DateTime lastActivityDate = DateTime.SpecifyKind(reader.GetDateTime(4), DateTimeKind.Utc);
                                stateInfoCollection.Add(new UserPersonalizationStateInfo(
                                                                returnedPath, lastUpdatedDate,
                                                                size, returnedUsername, lastActivityDate));
                            }
                        }

                        // The reader needs to be closed so return value can be accessed
                        // See MSDN doc for SqlParameter.Direction for details.
                        reader.Close();
                        reader = null;                        
                    }

                    // Set the total count at the end after all operations pass
                    if (returnValue.Value != null && returnValue.Value is int) {
                        totalRecords = (int)returnValue.Value;
                    }

                    return stateInfoCollection;
                }
                finally {
                    if (reader != null) {
                        reader.Close();
                    }

                    if (connectionHolder != null) {
                        connectionHolder.Close();
                        connectionHolder = null;
                    }
                }
            }
            catch {
                throw;
            }
        }

        /// <devdoc>
        /// </devdoc>
        private SqlConnectionHolder GetConnectionHolder() {
            SqlConnection connection = null;
            SqlConnectionHolder connectionHolder = SqlConnectionHelper.GetConnection(_connectionString, true);

            if (connectionHolder != null) {
                connection = connectionHolder.Connection;
            }
            if (connection == null) {
                throw new ProviderException(SR.GetString(SR.PersonalizationProvider_CantAccess, Name));
            }

            return connectionHolder;
        }

        private int GetCountOfSharedState(string path) {
            SqlConnectionHolder connectionHolder = null;
            SqlConnection connection = null;
            int count = 0;

            // Extra try-catch block to prevent elevation of privilege attack via exception filter
            try {
                try {
                    connectionHolder = GetConnectionHolder();
                    connection = connectionHolder.Connection;
                    Debug.Assert(connection != null);

                    CheckSchemaVersion( connection );

                    SqlCommand command = new SqlCommand("dbo.aspnet_PersonalizationAdministration_GetCountOfState", connection);
                    SetCommandTypeAndTimeout(command);
                    SqlParameterCollection parameters = command.Parameters;

                    SqlParameter parameter = parameters.Add(new SqlParameter("Count", SqlDbType.Int));
                    parameter.Direction = ParameterDirection.Output;

                    parameter = parameters.Add(new SqlParameter("AllUsersScope", SqlDbType.Bit));
                    parameter.Value = true;

                    parameters.AddWithValue("ApplicationName", ApplicationName);

                    parameter = parameters.Add("Path", SqlDbType.NVarChar);
                    if (path != null) {
                        parameter.Value = path;
                    }

                    parameter = parameters.Add("UserName", SqlDbType.NVarChar);
                    parameter = parameters.Add("InactiveSinceDate", SqlDbType.DateTime);

                    command.ExecuteNonQuery();
                    parameter = command.Parameters[0];
                    if (parameter != null && parameter.Value != null && parameter.Value is Int32) {
                        count = (Int32) parameter.Value;
                    }
                }
                finally {
                    if (connectionHolder != null) {
                        connectionHolder.Close();
                        connectionHolder = null;
                    }
                }
            }
            catch {
                throw;
            }

            return count;
        }

        public override int GetCountOfState(PersonalizationScope scope, PersonalizationStateQuery query) {
            PersonalizationProviderHelper.CheckPersonalizationScope(scope);
            if (scope == PersonalizationScope.Shared) {
                string pathToMatch = null;
                if (query != null) {
                    pathToMatch = StringUtil.CheckAndTrimString(query.PathToMatch, "query.PathToMatch", false, maxStringLength);
                }
                return GetCountOfSharedState(pathToMatch);
            }
            else {
                string pathToMatch = null;
                DateTime userInactiveSinceDate = PersonalizationAdministration.DefaultInactiveSinceDate;
                string usernameToMatch = null;
                if (query != null) {
                    pathToMatch = StringUtil.CheckAndTrimString(query.PathToMatch, "query.PathToMatch", false, maxStringLength);
                    userInactiveSinceDate = query.UserInactiveSinceDate;
                    usernameToMatch = StringUtil.CheckAndTrimString(query.UsernameToMatch, "query.UsernameToMatch", false, maxStringLength);
                }
                return GetCountOfUserState(pathToMatch, userInactiveSinceDate, usernameToMatch);
            }
        }

        private int GetCountOfUserState(string path, DateTime inactiveSinceDate, string username) {
            SqlConnectionHolder connectionHolder = null;
            SqlConnection connection = null;
            int count = 0;

            // Extra try-catch block to prevent elevation of privilege attack via exception filter
            try {
                try {
                    connectionHolder = GetConnectionHolder();
                    connection = connectionHolder.Connection;
                    Debug.Assert(connection != null);

                    CheckSchemaVersion( connection );

                    SqlCommand command = new SqlCommand("dbo.aspnet_PersonalizationAdministration_GetCountOfState", connection);
                    SetCommandTypeAndTimeout(command);
                    SqlParameterCollection parameters = command.Parameters;

                    SqlParameter parameter = parameters.Add(new SqlParameter("Count", SqlDbType.Int));
                    parameter.Direction = ParameterDirection.Output;

                    parameter = parameters.Add(new SqlParameter("AllUsersScope", SqlDbType.Bit));
                    parameter.Value = false;

                    parameters.AddWithValue("ApplicationName", ApplicationName);

                    parameter = parameters.Add("Path", SqlDbType.NVarChar);
                    if (path != null) {
                        parameter.Value = path;
                    }

                    parameter = parameters.Add("UserName", SqlDbType.NVarChar);
                    if (username != null) {
                        parameter.Value = username;
                    }

                    parameter = parameters.Add("InactiveSinceDate", SqlDbType.DateTime);
                    if (inactiveSinceDate != PersonalizationAdministration.DefaultInactiveSinceDate) {
                        parameter.Value = inactiveSinceDate.ToUniversalTime();
                    }

                    command.ExecuteNonQuery();
                    parameter = command.Parameters[0];
                    if (parameter != null && parameter.Value != null && parameter.Value is Int32) {
                        count = (Int32) parameter.Value;
                    }
                }
                finally {
                    if (connectionHolder != null) {
                        connectionHolder.Close();
                        connectionHolder = null;
                    }
                }
            }
            catch {
                throw;
            }

            return count;
        }

        public override void Initialize(string name, NameValueCollection configSettings) {
            HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Low, SR.Feature_not_supported_at_this_level);

            // configSettings cannot be null because there are required settings needed below
            if (configSettings == null) {
                throw new ArgumentNullException("configSettings");
            }

            if (String.IsNullOrEmpty(name)) {
                name = "SqlPersonalizationProvider";
            }

            // description will be set from the base class' Initialize method
            if (string.IsNullOrEmpty(configSettings["description"])) {
                configSettings.Remove("description");
                configSettings.Add("description", SR.GetString(SR.SqlPersonalizationProvider_Description));
            }
            base.Initialize(name, configSettings);

            _SchemaVersionCheck = 0;

            // If not available, the default value is set in the get accessor of ApplicationName
            _applicationName = configSettings["applicationName"];
            if (_applicationName != null) {
                configSettings.Remove("applicationName");

                if (_applicationName.Length > maxStringLength) {
                    throw new ProviderException(SR.GetString(SR.PersonalizationProvider_ApplicationNameExceedMaxLength, maxStringLength.ToString(CultureInfo.CurrentCulture)));
                }
            }

            string connectionStringName = configSettings["connectionStringName"];
            if (String.IsNullOrEmpty(connectionStringName)) {
                throw new ProviderException(SR.GetString(SR.PersonalizationProvider_NoConnection));
            }
            configSettings.Remove("connectionStringName");

            string connectionString = SqlConnectionHelper.GetConnectionString(connectionStringName, true, true);
            if (String.IsNullOrEmpty(connectionString)) {
                throw new ProviderException(SR.GetString(SR.PersonalizationProvider_BadConnection, connectionStringName));
            }
            _connectionString = connectionString;

            _commandTimeout = SecUtility.GetIntValue(configSettings, "commandTimeout", -1, true, 0);
            configSettings.Remove("commandTimeout");

            if (configSettings.Count > 0) {
                string invalidAttributeName = configSettings.GetKey(0);
                throw new ProviderException(SR.GetString(SR.PersonalizationProvider_UnknownProp, invalidAttributeName, name));
            }
        }

        private void CheckSchemaVersion( SqlConnection connection )
        {
            string[] features = { "Personalization" };
            string   version  = "1";

            SecUtility.CheckSchemaVersion( this,
                                           connection,
                                           features,
                                           version,
                                           ref _SchemaVersionCheck );
        }

        /// <devdoc>
        /// </devdoc>
        private byte[] LoadPersonalizationBlob(SqlConnection connection, string path, string userName) {
            Debug.Assert(connection != null);
            Debug.Assert(!String.IsNullOrEmpty(path));

            SqlCommand command;

            if (userName != null) {
                command = new SqlCommand("dbo.aspnet_PersonalizationPerUser_GetPageSettings", connection);
            }
            else {
                command = new SqlCommand("dbo.aspnet_PersonalizationAllUsers_GetPageSettings", connection);
            }

            SetCommandTypeAndTimeout(command);
            command.Parameters.Add(CreateParameter("@ApplicationName", SqlDbType.NVarChar, this.ApplicationName));
            command.Parameters.Add(CreateParameter("@Path", SqlDbType.NVarChar, path));
            if (userName != null) {
                command.Parameters.Add(CreateParameter("@UserName", SqlDbType.NVarChar, userName));
                command.Parameters.Add(CreateParameter("@CurrentTimeUtc", SqlDbType.DateTime, DateTime.UtcNow));
            }

            SqlDataReader reader = null;
            try {
                reader = command.ExecuteReader(CommandBehavior.SingleRow);
                if (reader.Read()) {
                    int length = (int)reader.GetBytes(0, 0, null, 0, 0);
                    byte[] state = new byte[length];

                    reader.GetBytes(0, 0, state, 0, length);
                    return state;
                }
            }
            finally {
                if (reader != null) {
                    reader.Close();
                }
            }

            return null;
        }

        /// <internalonly />
        protected override void LoadPersonalizationBlobs(WebPartManager webPartManager, string path, string userName, ref byte[] sharedDataBlob, ref byte[] userDataBlob) {
            sharedDataBlob = null;
            userDataBlob = null;

            SqlConnectionHolder connectionHolder = null;
            SqlConnection connection = null;

            // Extra try-catch block to prevent elevation of privilege attack via exception filter
            try {
                try {
                    connectionHolder = GetConnectionHolder();
                    connection = connectionHolder.Connection;

                    CheckSchemaVersion( connection );

                    sharedDataBlob = LoadPersonalizationBlob(connection, path, null);
                    if (!String.IsNullOrEmpty(userName)) {
                        userDataBlob = LoadPersonalizationBlob(connection, path, userName);
                    }
                }
                finally {
                    if (connectionHolder != null) {
                        connectionHolder.Close();
                        connectionHolder = null;
                    }
                }
            }
            catch {
                throw;
            }
        }

        /// <devdoc>
        /// </devdoc>
        private void ResetPersonalizationState(SqlConnection connection, string path, string userName) {
            Debug.Assert(connection != null);
            Debug.Assert(!String.IsNullOrEmpty(path));

            SqlCommand command;

            if (userName != null) {
                command = new SqlCommand("dbo.aspnet_PersonalizationPerUser_ResetPageSettings", connection);
            }
            else {
                command = new SqlCommand("dbo.aspnet_PersonalizationAllUsers_ResetPageSettings", connection);
            }

            SetCommandTypeAndTimeout(command);
            command.Parameters.Add(CreateParameter("@ApplicationName", SqlDbType.NVarChar, ApplicationName));
            command.Parameters.Add(CreateParameter("@Path", SqlDbType.NVarChar, path));
            if (userName != null) {
                command.Parameters.Add(CreateParameter("@UserName", SqlDbType.NVarChar, userName));
                command.Parameters.Add(CreateParameter("@CurrentTimeUtc", SqlDbType.DateTime, DateTime.UtcNow));
            }

            command.ExecuteNonQuery();
        }

        /// <internalonly />
        protected override void ResetPersonalizationBlob(WebPartManager webPartManager, string path, string userName) {
            SqlConnectionHolder connectionHolder = null;
            SqlConnection connection = null;

            // Extra try-catch block to prevent elevation of privilege attack via exception filter
            try {
                try {
                    connectionHolder = GetConnectionHolder();
                    connection = connectionHolder.Connection;

                    CheckSchemaVersion( connection );

                    ResetPersonalizationState(connection, path, userName);
                }
                finally {
                    if (connectionHolder != null) {
                        connectionHolder.Close();
                        connectionHolder = null;
                    }
                }
            }
            catch {
                throw;
            }
        }

        private int ResetAllState(PersonalizationScope scope) {
            SqlConnectionHolder connectionHolder = null;
            SqlConnection connection = null;
            int count = 0;

            // Extra try-catch block to prevent elevation of privilege attack via exception filter
            try {
                try {
                    connectionHolder = GetConnectionHolder();
                    connection = connectionHolder.Connection;
                    Debug.Assert(connection != null);

                    CheckSchemaVersion( connection );

                    SqlCommand command = new SqlCommand("dbo.aspnet_PersonalizationAdministration_DeleteAllState", connection);
                    SetCommandTypeAndTimeout(command);
                    SqlParameterCollection parameters = command.Parameters;

                    SqlParameter parameter = parameters.Add(new SqlParameter("AllUsersScope", SqlDbType.Bit));
                    parameter.Value = (scope == PersonalizationScope.Shared);

                    parameters.AddWithValue("ApplicationName", ApplicationName);

                    parameter = parameters.Add(new SqlParameter("Count", SqlDbType.Int));
                    parameter.Direction = ParameterDirection.Output;

                    command.ExecuteNonQuery();
                    parameter = command.Parameters[2];
                    if (parameter != null && parameter.Value != null && parameter.Value is Int32) {
                        count = (Int32) parameter.Value;
                    }
                }
                finally {
                    if (connectionHolder != null) {
                        connectionHolder.Close();
                        connectionHolder = null;
                    }
                }
            }
            catch {
                throw;
            }

            return count;
        }

        private int ResetSharedState(string[] paths) {
            int resultCount = 0;

            if (paths == null) {
                resultCount = ResetAllState(PersonalizationScope.Shared);
            }
            else {
                SqlConnectionHolder connectionHolder = null;
                SqlConnection connection = null;

                // Extra try-catch block to prevent elevation of privilege attack via exception filter
                try {
                    bool beginTranCalled = false;
                    try {
                        connectionHolder = GetConnectionHolder();
                        connection = connectionHolder.Connection;
                        Debug.Assert(connection != null);

                        CheckSchemaVersion( connection );

                        SqlCommand command = new SqlCommand("dbo.aspnet_PersonalizationAdministration_ResetSharedState", connection);
                        SetCommandTypeAndTimeout(command);
                        SqlParameterCollection parameters = command.Parameters;

                        SqlParameter parameter = parameters.Add(new SqlParameter("Count", SqlDbType.Int));
                        parameter.Direction = ParameterDirection.Output;

                        parameters.AddWithValue("ApplicationName", ApplicationName);

                        parameter = parameters.Add("Path", SqlDbType.NVarChar);
                        foreach (string path in paths) {
                            if (!beginTranCalled && paths.Length > 1) {
                                (new SqlCommand("BEGIN TRANSACTION", connection)).ExecuteNonQuery();
                                beginTranCalled = true;
                            }

                            parameter.Value = path;
                            command.ExecuteNonQuery();
                            SqlParameter countParam = command.Parameters[0];
                            if (countParam != null && countParam.Value != null && countParam.Value is Int32) {
                                resultCount += (Int32) countParam.Value;
                            }
                        }

                        if (beginTranCalled) {
                            (new SqlCommand("COMMIT TRANSACTION", connection)).ExecuteNonQuery();
                            beginTranCalled = false;
                        }
                    }
                    catch {
                        if (beginTranCalled) {
                            (new SqlCommand("ROLLBACK TRANSACTION", connection)).ExecuteNonQuery();
                            beginTranCalled = false;
                        }
                        throw;
                    }
                    finally {
                       if (connectionHolder != null) {
                            connectionHolder.Close();
                            connectionHolder = null;
                        }
                    }
                }
                catch {
                    throw;
                }
            }

            return resultCount;
        }

        public override int ResetUserState(string path, DateTime userInactiveSinceDate) {
            path = StringUtil.CheckAndTrimString(path, "path", false, maxStringLength);
            string [] paths = (path == null) ? null : new string [] {path};
            return ResetUserState(ResetUserStateMode.PerInactiveDate,
                                  userInactiveSinceDate, paths, null);
        }

        public override int ResetState(PersonalizationScope scope, string[] paths, string[] usernames) {
            PersonalizationProviderHelper.CheckPersonalizationScope(scope);
            paths = PersonalizationProviderHelper.CheckAndTrimNonEmptyStringEntries(paths, "paths", false, false, maxStringLength);
            usernames = PersonalizationProviderHelper.CheckAndTrimNonEmptyStringEntries(usernames, "usernames", false, true, maxStringLength);

            if (scope == PersonalizationScope.Shared) {
                PersonalizationProviderHelper.CheckUsernamesInSharedScope(usernames);
                return ResetSharedState(paths);
            }
            else {
                PersonalizationProviderHelper.CheckOnlyOnePathWithUsers(paths, usernames);
                return ResetUserState(paths, usernames);
            }
        }

        private int ResetUserState(string[] paths, string[] usernames) {
            int count = 0;
            bool hasPaths = !(paths == null || paths.Length == 0);
            bool hasUsernames = !(usernames == null || usernames.Length == 0);

            if (!hasPaths && !hasUsernames) {
                count = ResetAllState(PersonalizationScope.User);
            }
            else if (!hasUsernames) {
                count = ResetUserState(ResetUserStateMode.PerPaths,
                                       PersonalizationAdministration.DefaultInactiveSinceDate,
                                       paths, usernames);
            }
            else {
                count = ResetUserState(ResetUserStateMode.PerUsers,
                                       PersonalizationAdministration.DefaultInactiveSinceDate,
                                       paths, usernames);
            }

            return count;
        }

        private int ResetUserState(ResetUserStateMode mode,
                                   DateTime userInactiveSinceDate,
                                   string[] paths,
                                   string[] usernames) {
            SqlConnectionHolder connectionHolder = null;
            SqlConnection connection = null;
            int count = 0;

            // Extra try-catch block to prevent elevation of privilege attack via exception filter
            try {
                bool beginTranCalled = false;
                try {
                    connectionHolder = GetConnectionHolder();
                    connection = connectionHolder.Connection;
                    Debug.Assert(connection != null);

                    CheckSchemaVersion( connection );

                    SqlCommand command = new SqlCommand("dbo.aspnet_PersonalizationAdministration_ResetUserState", connection);
                    SetCommandTypeAndTimeout(command);
                    SqlParameterCollection parameters = command.Parameters;

                    SqlParameter parameter = parameters.Add(new SqlParameter("Count", SqlDbType.Int));
                    parameter.Direction = ParameterDirection.Output;

                    parameters.AddWithValue("ApplicationName", ApplicationName);

                    string firstPath = (paths != null && paths.Length > 0) ? paths[0] : null;

                    if (mode == ResetUserStateMode.PerInactiveDate) {
                        if (userInactiveSinceDate != PersonalizationAdministration.DefaultInactiveSinceDate) {
                            // Special note: DateTime object cannot be added to collection
                            // via AddWithValue for some reason.
                            parameter = parameters.Add("InactiveSinceDate", SqlDbType.DateTime);
                            parameter.Value = userInactiveSinceDate.ToUniversalTime();
                        }

                        if (firstPath != null) {
                            parameters.AddWithValue("Path", firstPath);
                        }

                        command.ExecuteNonQuery();
                        SqlParameter countParam = command.Parameters[0];
                        if (countParam != null && countParam.Value != null && countParam.Value is Int32) {
                            count = (Int32) countParam.Value;
                        }
                    }
                    else if (mode == ResetUserStateMode.PerPaths) {
                        Debug.Assert(paths != null);
                        parameter = parameters.Add("Path", SqlDbType.NVarChar);
                        foreach (string path in paths) {
                            if (!beginTranCalled && paths.Length > 1) {
                                (new SqlCommand("BEGIN TRANSACTION", connection)).ExecuteNonQuery();
                                beginTranCalled = true;
                            }

                            parameter.Value = path;
                            command.ExecuteNonQuery();
                            SqlParameter countParam = command.Parameters[0];
                            if (countParam != null && countParam.Value != null && countParam.Value is Int32) {
                                count += (Int32) countParam.Value;
                            }
                        }
                    }
                    else {
                        Debug.Assert(mode == ResetUserStateMode.PerUsers);
                        if (firstPath != null) {
                            parameters.AddWithValue("Path", firstPath);
                        }

                        parameter = parameters.Add("UserName", SqlDbType.NVarChar);
                        foreach (string user in usernames) {
                            if (!beginTranCalled && usernames.Length > 1) {
                                (new SqlCommand("BEGIN TRANSACTION", connection)).ExecuteNonQuery();
                                beginTranCalled = true;
                            }

                            parameter.Value = user;
                            command.ExecuteNonQuery();
                            SqlParameter countParam = command.Parameters[0];
                            if (countParam != null && countParam.Value != null && countParam.Value is Int32) {
                                count += (Int32) countParam.Value;
                            }
                        }
                    }

                    if (beginTranCalled) {
                        (new SqlCommand("COMMIT TRANSACTION", connection)).ExecuteNonQuery();
                        beginTranCalled = false;
                    }
                }
                catch {
                    if (beginTranCalled) {
                        (new SqlCommand("ROLLBACK TRANSACTION", connection)).ExecuteNonQuery();
                        beginTranCalled = false;
                    }
                    throw;
                }
                finally {
                    if (connectionHolder != null) {
                        connectionHolder.Close();
                        connectionHolder = null;
                    }
                }
            }
            catch {
                throw;
            }

            return count;
        }

        /// <devdoc>
        /// </devdoc>
        private void SavePersonalizationState(SqlConnection connection, string path, string userName, byte[] state) {
            Debug.Assert(connection != null);
            Debug.Assert(!String.IsNullOrEmpty(path));
            Debug.Assert((state != null) && (state.Length != 0));
            SqlCommand command;

            if (userName != null) {
                command = new SqlCommand("dbo.aspnet_PersonalizationPerUser_SetPageSettings", connection);
            }
            else {
                command = new SqlCommand("dbo.aspnet_PersonalizationAllUsers_SetPageSettings", connection);
            }

            SetCommandTypeAndTimeout(command);
            command.Parameters.Add(CreateParameter("@ApplicationName", SqlDbType.NVarChar, ApplicationName));
            command.Parameters.Add(CreateParameter("@Path", SqlDbType.NVarChar, path));
            command.Parameters.Add(CreateParameter("@PageSettings", SqlDbType.Image, state));
            command.Parameters.Add(CreateParameter("@CurrentTimeUtc", SqlDbType.DateTime, DateTime.UtcNow));
            if (userName != null) {
                command.Parameters.Add(CreateParameter("@UserName", SqlDbType.NVarChar, userName));
            }

            command.ExecuteNonQuery();
        }

        /// <internalonly />
        protected override void SavePersonalizationBlob(WebPartManager webPartManager, string path, string userName, byte[] dataBlob) {
            SqlConnectionHolder connectionHolder = null;
            SqlConnection connection = null;

            // Extra try-catch block to prevent elevation of privilege attack via exception filter
            try {
                try {
                    connectionHolder = GetConnectionHolder();
                    connection = connectionHolder.Connection;

                    CheckSchemaVersion( connection );

                    SavePersonalizationState(connection, path, userName, dataBlob);
                } catch(SqlException  sqlEx) {
                    // Check if it failed due to duplicate user name
                    if (userName != null && (sqlEx.Number == 2627 || sqlEx.Number == 2601 || sqlEx.Number == 2512)) {
                        // Try again, because it failed first time with duplicate user name
                        SavePersonalizationState(connection, path, userName, dataBlob);
                    } else {
                        throw;
                    }
                } 
                finally {
                    if (connectionHolder != null) {
                        connectionHolder.Close();
                        connectionHolder = null;
                    }
                }
            }
            catch {
                throw;
            }
        }

        private void SetCommandTypeAndTimeout(SqlCommand command) {
            command.CommandType = CommandType.StoredProcedure;
            if (_commandTimeout != -1) {
                command.CommandTimeout = _commandTimeout;
            }
        }
    }
}
