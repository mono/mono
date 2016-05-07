//------------------------------------------------------------------------------
// <copyright file="SqlConnectionHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.DataAccess {

    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Configuration.Provider;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Security.Permissions;
    using System.Threading;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Management;
    using System.Web.Util;

    internal static class SqlConnectionHelper {
        internal const string s_strDataDir = "DataDirectory";
        internal const string s_strUpperDataDirWithToken = "|DATADIRECTORY|";
        internal const string s_strSqlExprFileExt = ".MDF";
        internal const string s_strUpperUserInstance = "USER INSTANCE";
        private const string s_localDbName = "(LOCALDB)";
        private static object s_lock = new object();

        internal static void EnsureNoUserInstance(string connectionString) {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
            if (builder.UserInstance) {
                throw new ProviderException(SR.GetString(SR.LocalDB_cannot_have_userinstance_flag));
            }
        }

        internal static SqlConnectionHolder GetConnection(string connectionString, bool revertImpersonation) {
            string strTempConnection = connectionString.ToUpperInvariant();
            if (strTempConnection.Contains(s_strUpperDataDirWithToken)) {
                EnsureDBFile(connectionString);
            }

            // Only block UserInstance for LocalDB connections
            if (strTempConnection.Contains(s_localDbName)) {
                EnsureNoUserInstance(connectionString);
            }

            SqlConnectionHolder holder = new SqlConnectionHolder(connectionString);
            bool closeConn = true;
            try {
                try {
                    holder.Open(null, revertImpersonation);
                    closeConn = false;
                }
                finally {
                    if (closeConn) {
                        holder.Close();
                        holder = null;
                    }
                }
            }
            catch {
                throw;
            }
            return holder;
        }

        internal static string GetConnectionString(string specifiedConnectionString, bool lookupConnectionString, bool appLevel) {
            System.Web.Util.Debug.Assert((specifiedConnectionString != null) && (specifiedConnectionString.Length != 0));
            if (specifiedConnectionString == null || specifiedConnectionString.Length < 1)
                return null;

            string connectionString = null;

            // Step 1: Check <connectionStrings> config section for this connection string
            if (lookupConnectionString) {
                RuntimeConfig config = (appLevel) ? RuntimeConfig.GetAppConfig() : RuntimeConfig.GetConfig();
                ConnectionStringSettings connObj = config.ConnectionStrings.ConnectionStrings[specifiedConnectionString];
                if (connObj != null)
                    connectionString = connObj.ConnectionString;

                if (connectionString == null)
                    return null;

                //HandlerBase.CheckAndReadRegistryValue (ref connectionString, true);
            }
            else {
                connectionString = specifiedConnectionString;
            }

            return connectionString;
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        internal static string GetDataDirectory() {
            if (HostingEnvironment.IsHosted)
                return Path.Combine(HttpRuntime.AppDomainAppPath, HttpRuntime.DataDirectoryName);

            string dataDir = AppDomain.CurrentDomain.GetData(s_strDataDir) as string;
            if (string.IsNullOrEmpty(dataDir)) {
                string appPath = null;

#if !FEATURE_PAL // FEATURE_PAL does not support ProcessModule
                Process p = Process.GetCurrentProcess();
                ProcessModule pm = (p != null ? p.MainModule : null);
                string exeName = (pm != null ? pm.FileName : null);

                if (!string.IsNullOrEmpty(exeName))
                    appPath = Path.GetDirectoryName(exeName);
#endif // !FEATURE_PAL

                if (string.IsNullOrEmpty(appPath))
                    appPath = Environment.CurrentDirectory;

                dataDir = Path.Combine(appPath, HttpRuntime.DataDirectoryName);
                AppDomain.CurrentDomain.SetData(s_strDataDir, dataDir, new FileIOPermission(FileIOPermissionAccess.PathDiscovery, dataDir));
            }

            return dataDir;
        }

        private static void EnsureDBFile(string connectionString) {
            string partialFileName = null;
            string fullFileName = null;
            string dataDir = GetDataDirectory();
            bool lookingForDataDir = true;
            bool lookingForDB = true;
            string[] splitedConnStr = connectionString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            bool lookingForUserInstance = !connectionString.ToUpperInvariant().Contains(s_localDbName); // We don't require UserInstance=True for LocalDb
            bool lookingForTimeout = true;

            foreach (string str in splitedConnStr) {
                string strUpper = str.ToUpper(CultureInfo.InvariantCulture).Trim();

                if (lookingForDataDir && strUpper.Contains(s_strUpperDataDirWithToken)) {
                    lookingForDataDir = false;

                    // Replace the AttachDBFilename part with "Pooling=false"
                    connectionString = connectionString.Replace(str, "Pooling=false");

                    // Extract the filenames
                    int startPos = strUpper.IndexOf(s_strUpperDataDirWithToken, StringComparison.Ordinal) + s_strUpperDataDirWithToken.Length;
                    partialFileName = strUpper.Substring(startPos).Trim();
                    while (partialFileName.StartsWith("\\", StringComparison.Ordinal))
                        partialFileName = partialFileName.Substring(1);
                    if (partialFileName.Contains("..")) // don't allow it to traverse-up
                        partialFileName = null;
                    else
                        fullFileName = Path.Combine(dataDir, partialFileName);
                    if (!lookingForDB)
                        break; // done
                }
                else if (lookingForDB && (strUpper.StartsWith("INITIAL CATALOG", StringComparison.Ordinal) || strUpper.StartsWith("DATABASE", StringComparison.Ordinal))) {
                    lookingForDB = false;
                    connectionString = connectionString.Replace(str, "Database=master");
                    if (!lookingForDataDir)
                        break; // done
                }
                else if (lookingForUserInstance && strUpper.StartsWith(s_strUpperUserInstance, StringComparison.Ordinal)) {
                    lookingForUserInstance = false;
                    int pos = strUpper.IndexOf('=');
                    if (pos < 0)
                        return;
                    string strTemp = strUpper.Substring(pos + 1).Trim();
                    if (strTemp != "TRUE")
                        return;
                }
                else if (lookingForTimeout && strUpper.StartsWith("CONNECT TIMEOUT", StringComparison.Ordinal)) {
                    lookingForTimeout = false;
                }
            }
            if (lookingForUserInstance)
                return;

            if (fullFileName == null)
                throw new ProviderException(SR.GetString(SR.SqlExpress_file_not_found_in_connection_string));

            if (File.Exists(fullFileName))
                return;

            if (!HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.High))
                throw new ProviderException(SR.GetString(SR.Provider_can_not_create_file_in_this_trust_level));

            if (!connectionString.Contains("Database=master"))
                connectionString += ";Database=master";
            if (lookingForTimeout)
                connectionString += ";Connect Timeout=45";
            using (new ApplicationImpersonationContext())
                lock (s_lock)
                    if (!File.Exists(fullFileName))
                        CreateMdfFile(fullFileName, dataDir, connectionString);
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        private static void CreateMdfFile(string fullFileName, string dataDir, string connectionString) {
            bool creatingDir = false;
            string databaseName = null;
            HttpContext context = HttpContext.Current;
            string tempFileName = null;

            try {
                if (!Directory.Exists(dataDir)) {
                    creatingDir = true;
                    Directory.CreateDirectory(dataDir);
                    creatingDir = false;
                    try {
                        if (context != null)
                            HttpRuntime.RestrictIISFolders(context);
                    }
                    catch { }
                }

                fullFileName = fullFileName.ToUpper(CultureInfo.InvariantCulture);
                char[] strippedFileNameChars = Path.GetFileNameWithoutExtension(fullFileName).ToCharArray();
                for (int iter = 0; iter < strippedFileNameChars.Length; iter++)
                    if (!char.IsLetterOrDigit(strippedFileNameChars[iter]))
                        strippedFileNameChars[iter] = '_';
                string strippedFileName = new string(strippedFileNameChars);
                if (strippedFileName.Length > 30)
                    databaseName = strippedFileName.Substring(0, 30) + "_" + Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
                else
                    databaseName = strippedFileName + "_" + Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);

                tempFileName = Path.Combine(Path.GetDirectoryName(fullFileName), strippedFileName + "_TMP" + s_strSqlExprFileExt);

                // Auto create the temporary database
                SqlServices.Install(databaseName, tempFileName, connectionString);
                DetachDB(databaseName, connectionString);
                try {
                    File.Move(tempFileName, fullFileName);
                }
                catch {
                    if (!File.Exists(fullFileName)) {
                        File.Copy(tempFileName, fullFileName);
                        try {
                            File.Delete(tempFileName);
                        }
                        catch { }
                    }
                }
                try {
                    File.Delete(tempFileName.Replace("_TMP.MDF", "_TMP_log.LDF"));
                }
                catch { }
            }
            catch (Exception e) {
                if (context == null || context.IsCustomErrorEnabled)
                    throw;
                HttpException httpExec = new HttpException(e.Message, e);
                if (e is UnauthorizedAccessException)
                    httpExec.SetFormatter(new SqlExpressConnectionErrorFormatter(creatingDir ? DataConnectionErrorEnum.CanNotCreateDataDir : DataConnectionErrorEnum.CanNotWriteToDataDir));
                else
                    httpExec.SetFormatter(new SqlExpressDBFileAutoCreationErrorFormatter(e));
                throw httpExec;
            }
        }

        private static void DetachDB(string databaseName, string connectionString) {
            SqlConnection connection = new SqlConnection(connectionString);
            try {
                connection.Open();
                SqlCommand command = new SqlCommand("USE master", connection);
                command.ExecuteNonQuery();
                command = new SqlCommand("sp_detach_db", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@dbname", databaseName);
                command.Parameters.AddWithValue("@skipchecks", "true");
                command.ExecuteNonQuery();
            }
            catch {
            }
            finally {
                connection.Close();
            }
        }
    }

    internal sealed class SqlConnectionHolder {
        internal SqlConnection _Connection;
        private bool _Opened;

        internal SqlConnection Connection {
            get { return _Connection; }
        }

        internal SqlConnectionHolder(string connectionString) {
            try {
                _Connection = new SqlConnection(connectionString);
                System.Web.Util.Debug.Assert(_Connection != null);
            }
            catch (ArgumentException e) {
                throw new ArgumentException(SR.GetString(SR.SqlError_Connection_String), "connectionString", e);
            }
        }

        internal void Open(HttpContext context, bool revertImpersonate) {
            if (_Opened)
                return; // Already opened

            if (revertImpersonate) {
                using (new ApplicationImpersonationContext()) {
                    Connection.Open();
                }
            }
            else {
                Connection.Open();
            }

            _Opened = true; // Open worked!
        }

        internal void Close() {
            if (!_Opened) // Not open!
                return;
            // Close connection
            Connection.Close();
            _Opened = false;
        }
    }
}

