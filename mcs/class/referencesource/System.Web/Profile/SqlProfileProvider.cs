//------------------------------------------------------------------------------
// <copyright file="SqlProfileProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Profile {
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
    using  System.Runtime.Serialization.Formatters.Binary;
    using  System.IO;
    using  System.Reflection;
    using  System.Xml.Serialization;
    using  System.Text;
    using  System.Configuration.Provider;
    using  System.Configuration;
    using  System.Web.Hosting;
    using  System.Web.DataAccess;
    using  System.Web.Util;


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class SqlProfileProvider : ProfileProvider
    {
        private string  _AppName;
        private string  _sqlConnectionString;
        private int     _SchemaVersionCheck;
        private int     _CommandTimeout;


        public override void Initialize(string name, NameValueCollection config)
        {
            HttpRuntime.CheckAspNetHostingPermission (AspNetHostingPermissionLevel.Low, SR.Feature_not_supported_at_this_level);
            if (config == null)
               throw new ArgumentNullException("config");
            if (name == null || name.Length < 1)
                name = "SqlProfileProvider";
            if (string.IsNullOrEmpty(config["description"])) {
                config.Remove("description");
                config.Add("description", SR.GetString(SR.ProfileSqlProvider_description));
            }
            base.Initialize(name, config);

            _SchemaVersionCheck = 0;

            _sqlConnectionString = SecUtility.GetConnectionString(config);

            _AppName = config["applicationName"];
            if (string.IsNullOrEmpty(_AppName))
                _AppName = SecUtility.GetDefaultAppName();

            if( _AppName.Length > 256 )
            {
                throw new ProviderException(SR.GetString(SR.Provider_application_name_too_long));
            }

            _CommandTimeout = SecUtility.GetIntValue( config, "commandTimeout", 30, true, 0 );

            config.Remove("commandTimeout");
            config.Remove("connectionStringName");
            config.Remove("connectionString");
            config.Remove("applicationName");
            if (config.Count > 0)
            {
                string attribUnrecognized = config.GetKey(0);
                if (!String.IsNullOrEmpty(attribUnrecognized))
                    throw new ProviderException(SR.GetString(SR.Provider_unrecognized_attribute, attribUnrecognized));
            }
        }

        private void CheckSchemaVersion( SqlConnection connection )
        {
            string[] features = { "Profile" };
            string   version  = "1";

            SecUtility.CheckSchemaVersion( this,
                                           connection,
                                           features,
                                           version,
                                           ref _SchemaVersionCheck );
        }


        public override string ApplicationName
        {
            get { return _AppName;  }
            set {
                if ( value.Length > 256 )
                {
                    throw new ProviderException( SR.GetString(SR.Provider_application_name_too_long)  );
                }
                _AppName = value;

            }
        }

        private int CommandTimeout
        {
            get{ return _CommandTimeout; }
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////

        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext sc, SettingsPropertyCollection properties)
        {
            SettingsPropertyValueCollection svc = new SettingsPropertyValueCollection();

            if (properties.Count < 1)
                return svc;

            string username = (string)sc["UserName"];

            foreach (SettingsProperty prop in properties)
            {
                if (prop.SerializeAs == SettingsSerializeAs.ProviderSpecific)
                    if (prop.PropertyType.IsPrimitive || prop.PropertyType == typeof(string))
                        prop.SerializeAs = SettingsSerializeAs.String;
                    else
                        prop.SerializeAs = SettingsSerializeAs.Xml;

                svc.Add(new SettingsPropertyValue(prop));
            }
            if (!String.IsNullOrEmpty(username))
                GetPropertyValuesFromDatabase (username, svc);
            return svc;
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        private void GetPropertyValuesFromDatabase(string userName, SettingsPropertyValueCollection svc) {
            HttpContext context = HttpContext.Current;

            if (context != null && HostingEnvironment.IsHosted && EtwTrace.IsTraceEnabled(EtwTraceLevel.Information, EtwTraceFlags.AppSvc)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PROFILE_BEGIN, HttpContext.Current.WorkerRequest);

            string[] names = null;
            string values = null;
            byte[] buf = null;
            string sName = null;

            if (context != null)
                sName = (context.Request.IsAuthenticated ? context.User.Identity.Name : context.Request.AnonymousID);

            try {
                SqlConnectionHolder holder = null;
                SqlDataReader reader = null;
                try
                {
                    holder = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);
                    CheckSchemaVersion(holder.Connection);
                    SqlCommand cmd = new SqlCommand("dbo.aspnet_Profile_GetProperties", holder.Connection);

                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(CreateInputParam("@ApplicationName", SqlDbType.NVarChar, ApplicationName));
                    cmd.Parameters.Add(CreateInputParam("@UserName", SqlDbType.NVarChar, userName));
                    cmd.Parameters.Add(CreateInputParam("@CurrentTimeUtc", SqlDbType.DateTime, DateTime.UtcNow));
                    reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                    if (reader.Read())
                    {
                        names = reader.GetString(0).Split(':');
                        values = reader.GetString(1);

                        int size = (int)reader.GetBytes(2, 0, null, 0, 0);

                        buf = new byte[size];
                        reader.GetBytes(2, 0, buf, 0, size);
                    }
                } finally {
                    if( holder != null )
                    {
                        holder.Close();
                        holder = null;
                    }

                    if (reader != null)
                        reader.Close();
                }

                ProfileModule.ParseDataFromDB(names, values, buf, svc);

                if (context != null && HostingEnvironment.IsHosted && EtwTrace.IsTraceEnabled(EtwTraceLevel.Information, EtwTraceFlags.AppSvc)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_PROFILE_END, HttpContext.Current.WorkerRequest, userName);
            } catch {
                throw;
            }
        }


        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////

        public override void SetPropertyValues(SettingsContext sc, SettingsPropertyValueCollection properties)
        {
            string username = (string)sc["UserName"];
            bool userIsAuthenticated = (bool)sc["IsAuthenticated"];

            if (username == null || username.Length < 1 || properties.Count < 1)
                return;

            string        names   = String.Empty;
            string        values  = String.Empty;
            byte []       buf     = null;

            ProfileModule.PrepareDataForSaving(ref names, ref values, ref buf, true, properties, userIsAuthenticated);
            if (names.Length == 0)
                return;

            try {
                SqlConnectionHolder holder = null;
                try
                {
                    holder = SqlConnectionHelper.GetConnection (_sqlConnectionString, true);
                    CheckSchemaVersion( holder.Connection );

                    SqlCommand    cmd     = new SqlCommand("dbo.aspnet_Profile_SetProperties", holder.Connection);

                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(CreateInputParam("@ApplicationName", SqlDbType.NVarChar, ApplicationName));
                    cmd.Parameters.Add(CreateInputParam("@UserName", SqlDbType.NVarChar, username));
                    cmd.Parameters.Add(CreateInputParam("@PropertyNames", SqlDbType.NText, names));
                    cmd.Parameters.Add(CreateInputParam("@PropertyValuesString", SqlDbType.NText, values));
                    cmd.Parameters.Add(CreateInputParam("@PropertyValuesBinary", SqlDbType.Image, buf));
                    cmd.Parameters.Add(CreateInputParam("@IsUserAnonymous", SqlDbType.Bit, !userIsAuthenticated));
                    cmd.Parameters.Add(CreateInputParam("@CurrentTimeUtc", SqlDbType.DateTime, DateTime.UtcNow));
                    cmd.ExecuteNonQuery();
                }
                finally {
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

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////

        private SqlParameter CreateInputParam(string paramName, SqlDbType dbType, object objValue){
            SqlParameter param = new SqlParameter(paramName, dbType);
            if (objValue == null)
                objValue = String.Empty;
            param.Value = objValue;
            return param;
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        // Mangement APIs from ProfileProvider class

        public override int DeleteProfiles(ProfileInfoCollection profiles)
        {
            if( profiles == null )
            {
                throw new ArgumentNullException( "profiles" );
            }

            if ( profiles.Count < 1 )
            {
                throw new ArgumentException(
                    SR.GetString(SR.Parameter_collection_empty,
                        "profiles" ),
                    "profiles" );
            }

            string[] usernames = new string[ profiles.Count ];

            int iter = 0;
            foreach ( ProfileInfo profile in profiles )
            {
                usernames[ iter++ ] = profile.UserName;
            }

            return DeleteProfiles( usernames );
        }
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        public override int DeleteProfiles(string[] usernames)
        {
            SecUtility.CheckArrayParameter( ref usernames,
                                            true,
                                            true,
                                            true,
                                            256,
                                            "usernames");

            int numProfilesDeleted = 0;
            bool beginTranCalled = false;
            try {
                SqlConnectionHolder holder = null;
                try
                {
                    holder = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);
                    CheckSchemaVersion( holder.Connection );

                    SqlCommand cmd;

                    int numUsersRemaing = usernames.Length;
                    while (numUsersRemaing > 0)
                    {
                        string allUsers = usernames[usernames.Length - numUsersRemaing];
                        numUsersRemaing--;
                        for (int iter = usernames.Length - numUsersRemaing; iter < usernames.Length; iter++)
                        {
                            if (allUsers.Length + usernames[iter].Length + 1 >= 4000)
                                break;
                            allUsers += "," + usernames[iter];
                            numUsersRemaing--;
                        }

                        // We don't need to start a transaction if we can finish this in one sql command
                        if (!beginTranCalled && numUsersRemaing > 0) {
                            cmd = new SqlCommand("BEGIN TRANSACTION", holder.Connection);
                            cmd.ExecuteNonQuery();
                            beginTranCalled = true;
                        }

                        cmd = new SqlCommand("dbo.aspnet_Profile_DeleteProfiles", holder.Connection);

                        cmd.CommandTimeout = CommandTimeout;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(CreateInputParam("@ApplicationName", SqlDbType.NVarChar, ApplicationName));
                        cmd.Parameters.Add(CreateInputParam("@UserNames", SqlDbType.NVarChar, allUsers));
                        object o = cmd.ExecuteScalar();
                        if (o != null && o is int)
                            numProfilesDeleted += (int)o;

                    }

                    if (beginTranCalled) {
                        cmd = new SqlCommand("COMMIT TRANSACTION", holder.Connection);
                        cmd.ExecuteNonQuery();
                        beginTranCalled = false;
                    }
                } catch  {
                    if (beginTranCalled) {
                        SqlCommand cmd = new SqlCommand("ROLLBACK TRANSACTION", holder.Connection);
                        cmd.ExecuteNonQuery();
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
            return numProfilesDeleted;
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        public override int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            try {
                SqlConnectionHolder holder = null;
                try
                {
                    holder = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);
                    CheckSchemaVersion( holder.Connection );

                    SqlCommand cmd = new SqlCommand("dbo.aspnet_Profile_DeleteInactiveProfiles", holder.Connection);

                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(CreateInputParam("@ApplicationName", SqlDbType.NVarChar, ApplicationName));
                    cmd.Parameters.Add(CreateInputParam("@ProfileAuthOptions", SqlDbType.Int, (int) authenticationOption));
                    cmd.Parameters.Add(CreateInputParam("@InactiveSinceDate", SqlDbType.DateTime, userInactiveSinceDate.ToUniversalTime()));
                    object o = cmd.ExecuteScalar();
                    if (o == null || !(o is int))
                        return 0;
                    return (int) o;
                }
                finally {
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
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        public override int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            try {
                SqlConnectionHolder holder = null;
                try
                {
                    holder = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);
                    CheckSchemaVersion( holder.Connection );

                    SqlCommand cmd = new SqlCommand("dbo.aspnet_Profile_GetNumberOfInactiveProfiles", holder.Connection);
                    
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(CreateInputParam("@ApplicationName", SqlDbType.NVarChar, ApplicationName));
                    cmd.Parameters.Add(CreateInputParam("@ProfileAuthOptions", SqlDbType.Int, (int) authenticationOption));
                    cmd.Parameters.Add(CreateInputParam("@InactiveSinceDate", SqlDbType.DateTime, userInactiveSinceDate.ToUniversalTime()));
                    object o = cmd.ExecuteScalar();
                    if (o == null || !(o is int))
                        return 0;
                    return (int) o;
                }
                finally {
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

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        public override ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords)
        {
            return GetProfilesForQuery(new SqlParameter[0], authenticationOption, pageIndex, pageSize, out totalRecords);
        }


        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        public override ProfileInfoCollection GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            SqlParameter [] args = new SqlParameter[1];
            args[0] = CreateInputParam("@InactiveSinceDate", SqlDbType.DateTime, userInactiveSinceDate.ToUniversalTime());
            return GetProfilesForQuery(args, authenticationOption, pageIndex, pageSize, out totalRecords);
        }
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        public override ProfileInfoCollection FindProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            SecUtility.CheckParameter(ref usernameToMatch, true, true, false, 256, "username");
            SqlParameter[] args = new SqlParameter[1];
            args[0] = CreateInputParam("@UserNameToMatch", SqlDbType.NVarChar, usernameToMatch);
            return GetProfilesForQuery(args, authenticationOption, pageIndex, pageSize, out totalRecords);
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        public override ProfileInfoCollection FindInactiveProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            SecUtility.CheckParameter(ref usernameToMatch, true, true, false, 256, "username");
            SqlParameter[] args = new SqlParameter[2];
            args[0] = CreateInputParam("@UserNameToMatch", SqlDbType.NVarChar, usernameToMatch);
            args[1] = CreateInputParam("@InactiveSinceDate", SqlDbType.DateTime, userInactiveSinceDate.ToUniversalTime());
            return GetProfilesForQuery(args, authenticationOption, pageIndex, pageSize, out totalRecords);
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        // Private methods

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        private ProfileInfoCollection GetProfilesForQuery(SqlParameter [] args, ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords)
        {
            if ( pageIndex < 0 )
                throw new ArgumentException(SR.GetString(SR.PageIndex_bad), "pageIndex");
            if ( pageSize < 1 )
                throw new ArgumentException(SR.GetString(SR.PageSize_bad), "pageSize");

            long upperBound = (long)pageIndex * pageSize + pageSize - 1;
            if ( upperBound > Int32.MaxValue )
            {
                throw new ArgumentException(SR.GetString(SR.PageIndex_PageSize_bad), "pageIndex and pageSize");
            }

            try {
                SqlConnectionHolder holder = null;
                SqlDataReader reader = null;
                try {
                    holder = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);
                    CheckSchemaVersion( holder.Connection );

                    SqlCommand cmd = new SqlCommand("dbo.aspnet_Profile_GetProfiles", holder.Connection);

                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(CreateInputParam("@ApplicationName", SqlDbType.NVarChar, ApplicationName));
                    cmd.Parameters.Add(CreateInputParam("@ProfileAuthOptions", SqlDbType.Int, (int) authenticationOption));
                    cmd.Parameters.Add(CreateInputParam("@PageIndex", SqlDbType.Int, pageIndex));
                    cmd.Parameters.Add(CreateInputParam("@PageSize", SqlDbType.Int, pageSize));
                    foreach (SqlParameter arg in args)
                        cmd.Parameters.Add(arg);
                    reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
                    ProfileInfoCollection profiles = new ProfileInfoCollection();
                    while (reader.Read())
                    {
                        string username;
                        DateTime dtLastActivity, dtLastUpdated;
                        bool isAnon;

                        username = reader.GetString(0);
                        isAnon = reader.GetBoolean(1);
                        dtLastActivity = DateTime.SpecifyKind(reader.GetDateTime(2), DateTimeKind.Utc);
                        dtLastUpdated = DateTime.SpecifyKind(reader.GetDateTime(3), DateTimeKind.Utc);
                        int size = reader.GetInt32(4);
                        profiles.Add(new ProfileInfo(username, isAnon, dtLastActivity, dtLastUpdated, size));
                    }
                    totalRecords = profiles.Count;
                    if (reader.NextResult())
                        if (reader.Read())
                            totalRecords = reader.GetInt32(0);
                    return profiles;
                } finally {
                    if (reader != null)
                        reader.Close();
                    
                    if( holder != null )
                    {
                        holder.Close();
                        holder = null;
                    }
                }
            }
            catch {
                throw;
            }
        }
    }
}



