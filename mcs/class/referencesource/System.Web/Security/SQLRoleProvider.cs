//------------------------------------------------------------------------------
// <copyright file="SqlRoleProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Security {
    using  System.Web;
    using  System.Web.Configuration;
    using  System.Security.Principal;
    using  System.Security.Permissions;
    using  System.Globalization;
    using  System.Runtime.Serialization;
    using  System.Collections;
    using  System.Collections.Specialized;
    using  System.Data;
    using  System.Data.SqlClient;
    using  System.Data.SqlTypes;
    using  System.Text;
    using  System.Configuration.Provider;
    using  System.Configuration;
    using  System.Web.DataAccess;
    using  System.Web.Hosting;
    using  System.Web.Util;


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class SqlRoleProvider : RoleProvider
    {
        private string  _AppName;
        private int     _SchemaVersionCheck;
        private string  _sqlConnectionString;
        private int     _CommandTimeout;

        ////////////////////////////////////////////////////////////
        // Public properties
        private int CommandTimeout
        {
            get{ return _CommandTimeout; }
        }


        public override  void Initialize(string name, NameValueCollection config){
            HttpRuntime.CheckAspNetHostingPermission (AspNetHostingPermissionLevel.Low, SR.Feature_not_supported_at_this_level);
            if (config == null)
               throw new ArgumentNullException("config");

            if (String.IsNullOrEmpty(name))
                name = "SqlRoleProvider";
            if (string.IsNullOrEmpty(config["description"])) {
                config.Remove("description");
                config.Add("description", SR.GetString(SR.RoleSqlProvider_description));
            }
            base.Initialize(name, config);

            _SchemaVersionCheck = 0;

            _CommandTimeout = SecUtility.GetIntValue( config, "commandTimeout", 30, true, 0 );

            _sqlConnectionString = SecUtility.GetConnectionString(config);

            _AppName = config["applicationName"];
            if (string.IsNullOrEmpty(_AppName))
                _AppName = SecUtility.GetDefaultAppName();

            if( _AppName.Length > 256 )
            {
                throw new ProviderException(SR.GetString(SR.Provider_application_name_too_long));
            }

            config.Remove("connectionString");
            config.Remove("connectionStringName");
            config.Remove("applicationName");
            config.Remove("commandTimeout");
            if (config.Count > 0)
            {
                string attribUnrecognized = config.GetKey(0);
                if (!String.IsNullOrEmpty(attribUnrecognized))
                    throw new ProviderException(SR.GetString(SR.Provider_unrecognized_attribute, attribUnrecognized));
            }
        }

        private void CheckSchemaVersion( SqlConnection connection )
        {
            string[] features = { "Role Manager" };
            string   version  = "1";

            SecUtility.CheckSchemaVersion( this,
                                           connection,
                                           features,
                                           version,
                                           ref _SchemaVersionCheck );
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////

        public override bool IsUserInRole(string username, string roleName)
        {
            SecUtility.CheckParameter(ref roleName, true, true, true, 256, "roleName");
            SecUtility.CheckParameter(ref username, true, false, true, 256, "username");
            if (username.Length < 1)
                return false;

            try {
                SqlConnectionHolder holder = null;
                try {
                    holder = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);
                    CheckSchemaVersion( holder.Connection );

                    SqlCommand    cmd     = new SqlCommand("dbo.aspnet_UsersInRoles_IsUserInRole", holder.Connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = CommandTimeout;

                    SqlParameter p = new SqlParameter("@ReturnValue", SqlDbType.Int);
                    p.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(p);
                    cmd.Parameters.Add(CreateInputParam("@ApplicationName", SqlDbType.NVarChar, ApplicationName));
                    cmd.Parameters.Add(CreateInputParam("@UserName", SqlDbType.NVarChar, username));
                    cmd.Parameters.Add(CreateInputParam("@RoleName", SqlDbType.NVarChar, roleName));
                    cmd.ExecuteNonQuery();
                    int iStatus = GetReturnValue(cmd);

                    switch(iStatus)
                    {
                    case 0:
                        return false;
                    case 1:
                        return true;
                    case 2:
                        return false;
                        // throw new ProviderException(SR.GetString(SR.Provider_user_not_found));
                    case 3:
                        return false; // throw new ProviderException(SR.GetString(SR.Provider_role_not_found, roleName));
                    }
                    throw new ProviderException(SR.GetString(SR.Provider_unknown_failure));
                }
                finally
                {
                    if( holder != null )
                    {
                        holder.Close();
                        holder = null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////

        public override  string [] GetRolesForUser(string username)
        {
            SecUtility.CheckParameter(ref username, true, false, true, 256, "username");
            if (username.Length < 1)
                return new string[0];
            try {
                SqlConnectionHolder holder = null;

                try {
                    holder = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);
                    CheckSchemaVersion( holder.Connection );

                    SqlCommand      cmd     = new SqlCommand("dbo.aspnet_UsersInRoles_GetRolesForUser", holder.Connection);
                    SqlParameter    p       = new SqlParameter("@ReturnValue", SqlDbType.Int);
                    SqlDataReader   reader  = null;
                    StringCollection       sc      = new StringCollection();

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = CommandTimeout;

                    p.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(p);
                    cmd.Parameters.Add(CreateInputParam("@ApplicationName", SqlDbType.NVarChar, ApplicationName));
                    cmd.Parameters.Add(CreateInputParam("@UserName", SqlDbType.NVarChar, username));
                    try {
                        reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
                        while (reader.Read())
                            sc.Add(reader.GetString(0));
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        if (reader != null)
                            reader.Close();
                    }
                    if (sc.Count > 0)
                    {
                        String [] strReturn = new String[sc.Count];
                        sc.CopyTo(strReturn, 0);
                        return strReturn;
                    }

                    switch(GetReturnValue(cmd))
                    {
                    case 0:
                        return new string[0];
                    case 1:
                        return new string[0];
                        //throw new ProviderException(SR.GetString(SR.Provider_user_not_found));
                    default:
                        throw new ProviderException(SR.GetString(SR.Provider_unknown_failure));
                    }
                }
                finally
                {
                    if( holder != null )
                    {
                        holder.Close();
                        holder = null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////

        public override  void CreateRole(string roleName)
        {
            SecUtility.CheckParameter(ref roleName, true, true, true, 256, "roleName");
            try {
                SqlConnectionHolder holder = null;

                try {
                    holder = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);
                    CheckSchemaVersion(holder.Connection);
                    SqlCommand cmd = new SqlCommand("dbo.aspnet_Roles_CreateRole", holder.Connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = CommandTimeout;

                    SqlParameter p = new SqlParameter("@ReturnValue", SqlDbType.Int);

                    p.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(p);
                    cmd.Parameters.Add(CreateInputParam("@ApplicationName", SqlDbType.NVarChar, ApplicationName));
                    cmd.Parameters.Add(CreateInputParam("@RoleName", SqlDbType.NVarChar, roleName));
                    cmd.ExecuteNonQuery();

                    int returnValue = GetReturnValue(cmd);

                    switch (returnValue) {
                    case 0 :
                        return;

                    case 1 :
                        throw new ProviderException(SR.GetString(SR.Provider_role_already_exists, roleName));

                    default :
                        throw new ProviderException(SR.GetString(SR.Provider_unknown_failure));
                    }
                }
                finally
                {
                    if( holder != null )
                    {
                        holder.Close();
                        holder = null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            SecUtility.CheckParameter(ref roleName, true, true, true, 256, "roleName");
            try {
                SqlConnectionHolder holder = null;

                try {
                    holder = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);
                    CheckSchemaVersion( holder.Connection );

                    SqlCommand    cmd     = new SqlCommand("dbo.aspnet_Roles_DeleteRole", holder.Connection);

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = CommandTimeout;

                    SqlParameter p = new SqlParameter("@ReturnValue", SqlDbType.Int);
                    p.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(p);
                    cmd.Parameters.Add(CreateInputParam("@ApplicationName", SqlDbType.NVarChar, ApplicationName));
                    cmd.Parameters.Add(CreateInputParam("@RoleName", SqlDbType.NVarChar, roleName));
                    cmd.Parameters.Add(CreateInputParam("@DeleteOnlyIfRoleIsEmpty", SqlDbType.Bit, throwOnPopulatedRole ? 1 : 0));
                    cmd.ExecuteNonQuery();
                    int returnValue = GetReturnValue(cmd);

                    if( returnValue == 2 )
                    {
                        throw new ProviderException(SR.GetString(SR.Role_is_not_empty));
                    }

                    return ( returnValue == 0 );
                }
                finally
                {
                    if( holder != null )
                    {
                        holder.Close();
                        holder = null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////

        public override  bool RoleExists(string roleName)
        {
            SecUtility.CheckParameter( ref roleName, true, true, true, 256, "roleName" );

            try {
                SqlConnectionHolder holder = null;

                try {
                    holder = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);
                    CheckSchemaVersion( holder.Connection );

                    SqlCommand    cmd     = new SqlCommand("dbo.aspnet_Roles_RoleExists", holder.Connection);

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = CommandTimeout;

                    SqlParameter p = new SqlParameter("@ReturnValue", SqlDbType.Int);
                    p.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(p);
                    cmd.Parameters.Add(CreateInputParam("@ApplicationName", SqlDbType.NVarChar, ApplicationName));
                    cmd.Parameters.Add(CreateInputParam("@RoleName", SqlDbType.NVarChar, roleName));
                    cmd.ExecuteNonQuery();
                    int returnValue = GetReturnValue(cmd);

                    switch(returnValue)
                    {
                    case 0:
                        return false;
                    case 1:
                        return true;
                    }
                    throw new ProviderException(SR.GetString(SR.Provider_unknown_failure));
                }
                finally
                {
                    if( holder != null )
                    {
                        holder.Close();
                        holder = null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            SecUtility.CheckArrayParameter(ref roleNames, true, true, true, 256, "roleNames");
            SecUtility.CheckArrayParameter(ref usernames, true, true, true, 256, "usernames");

            bool beginTranCalled = false;
            try {
                SqlConnectionHolder holder = null;
                try
                {
                    holder = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);
                    CheckSchemaVersion(holder.Connection);
                    int numUsersRemaing = usernames.Length;
                    while (numUsersRemaing > 0)
                    {
                        int iter;
                        string allUsers = usernames[usernames.Length - numUsersRemaing];
                        numUsersRemaing--;
                        for (iter = usernames.Length - numUsersRemaing; iter < usernames.Length; iter++)
                        {
                            if (allUsers.Length + usernames[iter].Length + 1 >= 4000)
                                break;
                            allUsers += "," + usernames[iter];
                            numUsersRemaing--;
                        }

                        int numRolesRemaining = roleNames.Length;
                        while (numRolesRemaining > 0)
                        {
                            string allRoles = roleNames[roleNames.Length - numRolesRemaining];
                            numRolesRemaining--;
                            for (iter = roleNames.Length - numRolesRemaining; iter < roleNames.Length; iter++)
                            {
                                if (allRoles.Length + roleNames[iter].Length + 1 >= 4000)
                                    break;
                                allRoles += "," + roleNames[iter];
                                numRolesRemaining--;
                            }
                            if (!beginTranCalled && (numUsersRemaing > 0 || numRolesRemaining > 0)) {
                                (new SqlCommand("BEGIN TRANSACTION", holder.Connection)).ExecuteNonQuery();
                                beginTranCalled = true;
                            }
                            AddUsersToRolesCore(holder.Connection, allUsers, allRoles);
                        }
                    }
                    if (beginTranCalled) {
                        (new SqlCommand("COMMIT TRANSACTION", holder.Connection)).ExecuteNonQuery();
                        beginTranCalled = false;
                    }
                } catch  {
                    if (beginTranCalled) {
                        try {
                            (new SqlCommand("ROLLBACK TRANSACTION", holder.Connection)).ExecuteNonQuery();
                        } catch {
                        }
                        beginTranCalled = false;
                    }
                    throw;
                } finally {
                    if( holder != null )
                    {
                        holder.Close();
                        holder = null;
                    }
                }
            } catch {
                throw;
            }
        }

        private void  AddUsersToRolesCore(SqlConnection conn, string usernames, string roleNames)
        {
            SqlCommand      cmd     = new SqlCommand("dbo.aspnet_UsersInRoles_AddUsersToRoles", conn);
            SqlDataReader   reader  = null;
            SqlParameter    p       = new SqlParameter("@ReturnValue", SqlDbType.Int);
            string          s1      = String.Empty, s2 = String.Empty;

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = CommandTimeout;

            p.Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(p);
            cmd.Parameters.Add(CreateInputParam("@ApplicationName", SqlDbType.NVarChar, ApplicationName));
            cmd.Parameters.Add(CreateInputParam("@RoleNames", SqlDbType.NVarChar, roleNames));
            cmd.Parameters.Add(CreateInputParam("@UserNames", SqlDbType.NVarChar, usernames));
            cmd.Parameters.Add(CreateInputParam("@CurrentTimeUtc", SqlDbType.DateTime, DateTime.UtcNow));
            try {
                reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                if (reader.Read()) {
                    if (reader.FieldCount > 0)
                        s1 = reader.GetString(0);
                    if (reader.FieldCount > 1)
                        s2 = reader.GetString(1);
                }
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
            switch(GetReturnValue(cmd))
            {
            case 0:
                return;
            case 1:
                throw new ProviderException(SR.GetString(SR.Provider_this_user_not_found, s1));
            case 2:
                throw new ProviderException(SR.GetString(SR.Provider_role_not_found, s1));
            case 3:
                throw new ProviderException(SR.GetString(SR.Provider_this_user_already_in_role, s1, s2));
            }
            throw new ProviderException(SR.GetString(SR.Provider_unknown_failure));
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            SecUtility.CheckArrayParameter(ref roleNames, true, true, true, 256, "roleNames");
            SecUtility.CheckArrayParameter(ref usernames, true, true, true, 256, "usernames");

            bool beginTranCalled = false;
            try {
                SqlConnectionHolder holder = null;
                try
                {
                    holder = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);
                    CheckSchemaVersion( holder.Connection );
                    int numUsersRemaing = usernames.Length;
                    while (numUsersRemaing > 0) {
                        int iter;
                        string allUsers = usernames[usernames.Length - numUsersRemaing];
                        numUsersRemaing--;
                        for (iter = usernames.Length - numUsersRemaing; iter < usernames.Length; iter++) {
                            if (allUsers.Length + usernames[iter].Length + 1 >= 4000)
                                break;
                            allUsers += "," + usernames[iter];
                            numUsersRemaing--;
                        }

                        int numRolesRemaining = roleNames.Length;
                        while (numRolesRemaining > 0) {
                            string allRoles = roleNames[roleNames.Length - numRolesRemaining];
                            numRolesRemaining--;
                            for (iter = roleNames.Length - numRolesRemaining; iter < roleNames.Length; iter++) {
                                if (allRoles.Length + roleNames[iter].Length + 1 >= 4000)
                                    break;
                                allRoles += "," + roleNames[iter];
                                numRolesRemaining--;
                            }

                            if (!beginTranCalled && (numUsersRemaing > 0 || numRolesRemaining > 0)) {
                                (new SqlCommand("BEGIN TRANSACTION", holder.Connection)).ExecuteNonQuery();
                                beginTranCalled = true;
                            }
                            RemoveUsersFromRolesCore(holder.Connection, allUsers, allRoles);
                        }
                    }
                    if (beginTranCalled) {
                        (new SqlCommand("COMMIT TRANSACTION", holder.Connection)).ExecuteNonQuery();
                        beginTranCalled = false;
                    }
                } catch  {
                    if (beginTranCalled) {
                        (new SqlCommand("ROLLBACK TRANSACTION", holder.Connection)).ExecuteNonQuery();
                        beginTranCalled = false;
                    }
                    throw;
                } finally {
                    if( holder != null )
                    {
                        holder.Close();
                        holder = null;
                    }
                }
            } catch {
                throw;
            }
        }

        private void RemoveUsersFromRolesCore(SqlConnection conn, string usernames, string roleNames)
        {
            SqlCommand      cmd     = new SqlCommand("dbo.aspnet_UsersInRoles_RemoveUsersFromRoles", conn);
            SqlDataReader   reader  = null;
            SqlParameter    p       = new SqlParameter("@ReturnValue", SqlDbType.Int);
            string          s1      = String.Empty, s2 = String.Empty;

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = CommandTimeout;

            p.Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(p);
            cmd.Parameters.Add(CreateInputParam("@ApplicationName", SqlDbType.NVarChar, ApplicationName));
            cmd.Parameters.Add(CreateInputParam("@UserNames", SqlDbType.NVarChar, usernames));
            cmd.Parameters.Add(CreateInputParam("@RoleNames", SqlDbType.NVarChar, roleNames));
            try {
                reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                if (reader.Read()) {
                    if (reader.FieldCount > 0)
                        s1 = reader.GetString(0);
                    if (reader.FieldCount > 1)
                        s2 = reader.GetString(1);
                }
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
            switch (GetReturnValue(cmd))
            {
                case 0:
                    return;
                case 1:
                    throw new ProviderException(SR.GetString(SR.Provider_this_user_not_found, s1));
                case 2:
                    throw new ProviderException(SR.GetString(SR.Provider_role_not_found, s2));
                case 3:
                    throw new ProviderException(SR.GetString(SR.Provider_this_user_already_not_in_role, s1, s2));
            }
            throw new ProviderException(SR.GetString(SR.Provider_unknown_failure));
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////

        public override  string [] GetUsersInRole(string roleName)
        {
            SecUtility.CheckParameter(ref roleName, true, true, true, 256, "roleName");

            try {
                SqlConnectionHolder holder = null;
                try {
                    holder = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);
                    CheckSchemaVersion( holder.Connection );

                    SqlCommand      cmd     = new SqlCommand("dbo.aspnet_UsersInRoles_GetUsersInRoles", holder.Connection);
                    SqlDataReader   reader  = null;
                    SqlParameter    p       = new SqlParameter("@ReturnValue", SqlDbType.Int);
                    StringCollection       sc      = new StringCollection();

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = CommandTimeout;

                    p.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(p);
                    cmd.Parameters.Add(CreateInputParam("@ApplicationName", SqlDbType.NVarChar, ApplicationName));
                    cmd.Parameters.Add(CreateInputParam("@RoleName", SqlDbType.NVarChar, roleName));
                    try {
                        reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
                        while (reader.Read())
                            sc.Add(reader.GetString(0));
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        if (reader != null)
                            reader.Close();
                    }
                    if (sc.Count < 1)
                    {
                        switch(GetReturnValue(cmd))
                        {
                        case 0:
                            return new string[0];
                        case 1:
                            throw new ProviderException(SR.GetString(SR.Provider_role_not_found, roleName));
                        }
                        throw new ProviderException(SR.GetString(SR.Provider_unknown_failure));
                    }

                    String [] strReturn = new String[sc.Count];
                    sc.CopyTo(strReturn, 0);
                    return strReturn;
                }
                finally
                {
                    if( holder != null )
                    {
                        holder.Close();
                        holder = null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////

        public override  string [] GetAllRoles(){
            try {
                SqlConnectionHolder holder = null;

                try {
                    holder = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);
                    CheckSchemaVersion( holder.Connection );

                    SqlCommand      cmd     = new SqlCommand("dbo.aspnet_Roles_GetAllRoles", holder.Connection);
                    StringCollection       sc      = new StringCollection();
                    SqlParameter    p       = new SqlParameter("@ReturnValue", SqlDbType.Int);
                    SqlDataReader   reader  = null;

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = CommandTimeout;

                    p.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(p);
                    cmd.Parameters.Add(CreateInputParam("@ApplicationName", SqlDbType.NVarChar, ApplicationName));
                    try {
                        reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
                        while (reader.Read())
                            sc.Add(reader.GetString(0));
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        if (reader != null)
                            reader.Close();
                    }

                    String [] strReturn = new String [sc.Count];
                    sc.CopyTo(strReturn, 0);
                    return strReturn;
                }
                finally
                {
                    if( holder != null )
                    {
                        holder.Close();
                        holder = null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            SecUtility.CheckParameter(ref roleName, true, true, true, 256, "roleName");
            SecUtility.CheckParameter(ref usernameToMatch, true, true, false, 256, "usernameToMatch");

            try {
                SqlConnectionHolder holder = null;

                try {
                    holder = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);
                    CheckSchemaVersion( holder.Connection );

                    SqlCommand cmd = new SqlCommand("dbo.aspnet_UsersInRoles_FindUsersInRole", holder.Connection);
                    SqlDataReader reader = null;
                    SqlParameter p = new SqlParameter("@ReturnValue", SqlDbType.Int);
                    StringCollection sc = new StringCollection();

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = CommandTimeout;

                    p.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(p);
                    cmd.Parameters.Add(CreateInputParam("@ApplicationName", SqlDbType.NVarChar, ApplicationName));
                    cmd.Parameters.Add(CreateInputParam("@RoleName", SqlDbType.NVarChar, roleName));
                    cmd.Parameters.Add(CreateInputParam("@UserNameToMatch", SqlDbType.NVarChar, usernameToMatch));
                    try {
                        reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
                        while (reader.Read())
                            sc.Add(reader.GetString(0));
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        if (reader != null)
                            reader.Close();
                    }
                    if (sc.Count < 1)
                    {
                        switch (GetReturnValue(cmd))
                        {
                        case 0:
                            return new string[0];

                        case 1:
                            throw new ProviderException(SR.GetString(SR.Provider_role_not_found, roleName));

                        default:
                            throw new ProviderException(SR.GetString(SR.Provider_unknown_failure));
                        }
                    }
                    String[] strReturn = new String[sc.Count];
                    sc.CopyTo(strReturn, 0);
                    return strReturn;
                }
                finally
                {
                    if( holder != null )
                    {
                        holder.Close();
                        holder = null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////

        public override  string ApplicationName
        {
            get { return _AppName; }
            set {
                _AppName = value;

                if ( _AppName.Length > 256 )
                {
                    throw new ProviderException( SR.GetString(SR.Provider_application_name_too_long)  );
                }
            }
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        private SqlParameter CreateInputParam(string paramName, SqlDbType dbType, object objValue){
            SqlParameter param = new SqlParameter(paramName, dbType);
            if (objValue == null)
                objValue = String.Empty;
            param.Value = objValue;
            return param;
        }

        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        private int GetReturnValue(SqlCommand cmd) {
            foreach(SqlParameter param in cmd.Parameters){
                if (param.Direction == ParameterDirection.ReturnValue && param.Value != null && param.Value is int)
                    return (int) param.Value;
            }
            return -1;
        }
    }
}



