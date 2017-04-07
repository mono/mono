//------------------------------------------------------------------------------
// <copyright file="SecurityUtil.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * SecurityUtil class
 * 
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web.Util {
    using System.Globalization;
    using System.Web.Hosting;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Data;
    using System.Data.SqlClient;
    using System.Data.SqlTypes;
    using System.Configuration.Provider;
    using System.Configuration;
    using System.Text.RegularExpressions;
    using System.Web.DataAccess;

    internal static class SecUtility {

        internal static string GetDefaultAppName() {
            try {
                string appName = HostingEnvironment.ApplicationVirtualPath;
                if (String.IsNullOrEmpty(appName)) {
#if !FEATURE_PAL
                    // ROTORTODO: enable Process.MainModule or support an alternative 
                    // naming scheme for (HttpRuntime.AppDomainAppVirtualPath == null)

                    appName = System.Diagnostics.Process.GetCurrentProcess().
                                     MainModule.ModuleName;

                    int indexOfDot = appName.IndexOf('.');
                    if (indexOfDot != -1) {
                        appName = appName.Remove(indexOfDot);
                    }
#endif // !FEATURE_PAL
                }

                if (String.IsNullOrEmpty(appName)) {
                    return "/";
                }
                else {
                    return appName;
                }
            }
            catch {
                return "/";
            }
        }

        internal static string GetConnectionString(NameValueCollection config) {
            Debug.Assert(config != null);

            string connectionString = config["connectionString"];
            if (!String.IsNullOrEmpty(connectionString)) {
                return connectionString;
            }
            else {
                string connectionStringName = config["connectionStringName"];
                if (String.IsNullOrEmpty(connectionStringName))
                    throw new ProviderException(SR.GetString(SR.Connection_name_not_specified));

                connectionString = SqlConnectionHelper.GetConnectionString(connectionStringName, lookupConnectionString: true, appLevel: true);
                if (String.IsNullOrEmpty(connectionString)) {
                    throw new ProviderException(SR.GetString(SR.Connection_string_not_found, connectionStringName));
                }
                else {
                    return connectionString;
                }
            }
        }

        // We don't trim the param before checking with password parameters
        internal static bool ValidatePasswordParameter(ref string param, int maxSize) {
            if (param == null) {
                return false;
            }

            if (param.Length < 1) {
                return false;
            }

            if (maxSize > 0 && (param.Length > maxSize) ) {
                return false;
            }

            return true;
        }

        internal static bool ValidateParameter(ref string param, bool checkForNull, bool checkIfEmpty, bool checkForCommas, int maxSize) {
            if (param == null) {
                return !checkForNull;
            }

            param = param.Trim();
            if ((checkIfEmpty && param.Length < 1) ||
                 (maxSize > 0 && param.Length > maxSize) ||
                 (checkForCommas && param.Contains(","))) {
                return false;
            }

            return true;
        }

        // We don't trim the param before checking with password parameters
        internal static void CheckPasswordParameter(ref string param, int maxSize, string paramName) {
            if (param == null) {
                throw new ArgumentNullException(paramName);
            }

            if (param.Length < 1) {
                throw new ArgumentException(SR.GetString(SR.Parameter_can_not_be_empty, paramName), paramName);
            }

            if (maxSize > 0 && param.Length > maxSize) {
                throw new ArgumentException(SR.GetString(SR.Parameter_too_long, paramName, maxSize.ToString(CultureInfo.InvariantCulture)), paramName);
            }
        }

        internal static void CheckParameter(ref string param, bool checkForNull, bool checkIfEmpty, bool checkForCommas, int maxSize, string paramName) {
            if (param == null) {
                if (checkForNull) {
                    throw new ArgumentNullException(paramName);
                }

                return;
            }

            param = param.Trim();
            if (checkIfEmpty && param.Length < 1) {
                throw new ArgumentException(SR.GetString(SR.Parameter_can_not_be_empty, paramName), paramName);
            }

            if (maxSize > 0 && param.Length > maxSize) {
                throw new ArgumentException(SR.GetString(SR.Parameter_too_long, paramName, maxSize.ToString(CultureInfo.InvariantCulture)), paramName);
            }

            if (checkForCommas && param.Contains(",")) {
                throw new ArgumentException(SR.GetString(SR.Parameter_can_not_contain_comma, paramName), paramName);
            }
        }

        internal static void CheckArrayParameter(ref string[] param, bool checkForNull, bool checkIfEmpty, bool checkForCommas, int maxSize, string paramName) {
            if (param == null) {
                throw new ArgumentNullException(paramName);
            }

            if (param.Length < 1) {
                throw new ArgumentException(SR.GetString(SR.Parameter_array_empty, paramName), paramName);
            }

            Hashtable values = new Hashtable(param.Length);
            for (int i = param.Length - 1; i >= 0; i--) {
                SecUtility.CheckParameter(ref param[i], checkForNull, checkIfEmpty, checkForCommas, maxSize,
                    paramName + "[ " + i.ToString(CultureInfo.InvariantCulture) + " ]");
                if (values.Contains(param[i])) {
                    throw new ArgumentException(SR.GetString(SR.Parameter_duplicate_array_element, paramName), paramName);
                }
                else {
                    values.Add(param[i], param[i]);
                }
            }
        }

        internal static bool GetBooleanValue(NameValueCollection config, string valueName, bool defaultValue) {
            string sValue = config[valueName];
            if (sValue == null) {
                return defaultValue;
            }

            bool result;
            if (bool.TryParse(sValue, out result)) {
                return result;
            }
            else {
                throw new ProviderException(SR.GetString(SR.Value_must_be_boolean, valueName));
            }
        }

        internal static int GetIntValue(NameValueCollection config, string valueName, int defaultValue, bool zeroAllowed, int maxValueAllowed) {
            string sValue = config[valueName];

            if (sValue == null) {
                return defaultValue;
            }

            int iValue;
            if (!Int32.TryParse(sValue, out iValue)) {
                if (zeroAllowed) {
                    throw new ProviderException(SR.GetString(SR.Value_must_be_non_negative_integer, valueName));
                }

                throw new ProviderException(SR.GetString(SR.Value_must_be_positive_integer, valueName));
            }

            if (zeroAllowed && iValue < 0) {
                throw new ProviderException(SR.GetString(SR.Value_must_be_non_negative_integer, valueName));
            }

            if (!zeroAllowed && iValue <= 0) {
                throw new ProviderException(SR.GetString(SR.Value_must_be_positive_integer, valueName));
            }

            if (maxValueAllowed > 0 && iValue > maxValueAllowed) {
                throw new ProviderException(SR.GetString(SR.Value_too_big, valueName, maxValueAllowed.ToString(CultureInfo.InvariantCulture)));
            }

            return iValue;
        }

        internal static TimeUnit GetTimeoutUnit(NameValueCollection config, string valueName, TimeUnit defaultValue) {
            TimeUnit unit;
            string sValue = config[valueName];

            if (sValue == null || !Enum.TryParse(sValue, out unit)) {
                return defaultValue;
            }

            return unit;
        }

        internal static int? GetNullableIntValue(NameValueCollection config, string valueName) {
            int iValue;
            string sValue = config[valueName];

            if (sValue == null || !Int32.TryParse(sValue, out iValue)) {
                return null;
            }

            return iValue;
        }

#if !FEATURE_PAL //
        internal static void CheckSchemaVersion(ProviderBase provider, SqlConnection connection, string[] features, string version, ref int schemaVersionCheck) {
            if (connection == null) {
                throw new ArgumentNullException("connection");
            }

            if (features == null) {
                throw new ArgumentNullException("features");
            }

            if (version == null) {
                throw new ArgumentNullException("version");
            }

            if (schemaVersionCheck == -1) {
                throw new ProviderException(SR.GetString(SR.Provider_Schema_Version_Not_Match, provider.ToString(), version));
            }
            else if (schemaVersionCheck == 0) {
                lock (provider) {
                    if (schemaVersionCheck == -1) {
                        throw new ProviderException(SR.GetString(SR.Provider_Schema_Version_Not_Match, provider.ToString(), version));
                    }
                    else if (schemaVersionCheck == 0) {
                        int iStatus = 0;
                        SqlCommand cmd = null;
                        SqlParameter p = null;

                        foreach (string feature in features) {
                            cmd = new SqlCommand("dbo.aspnet_CheckSchemaVersion", connection);

                            cmd.CommandType = CommandType.StoredProcedure;

                            p = new SqlParameter("@Feature", feature);
                            cmd.Parameters.Add(p);

                            p = new SqlParameter("@CompatibleSchemaVersion", version);
                            cmd.Parameters.Add(p);

                            p = new SqlParameter("@ReturnValue", SqlDbType.Int);
                            p.Direction = ParameterDirection.ReturnValue;
                            cmd.Parameters.Add(p);

                            cmd.ExecuteNonQuery();

                            iStatus = ((p.Value != null) ? ((int)p.Value) : -1);
                            if (iStatus != 0) {
                                schemaVersionCheck = -1;

                                throw new ProviderException(SR.GetString(SR.Provider_Schema_Version_Not_Match, provider.ToString(), version));
                            }
                        }

                        schemaVersionCheck = 1;
                    }
                }
            }
        }
#endif // !FEATURE_PAL
    }
}
