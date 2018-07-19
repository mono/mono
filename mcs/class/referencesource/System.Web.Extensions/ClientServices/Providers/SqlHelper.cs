//------------------------------------------------------------------------------
// <copyright file="SqlHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Web.ClientServices.Providers
{
    using System;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.AccessControl;
    using System.Globalization;
    using System.Data;
    using System.Data.Common;
    using System.Data.OleDb;
    using System.IO;
    using System.Windows.Forms;
    using System.Data.SqlClient;
    using System.Reflection;
    using System.Diagnostics.CodeAnalysis;
    using System.Web.Resources;

    internal static class SqlHelper
    {
        private const string _SQL_CE_Tag           = "|SQL/CE|";
        private const string _SQL_FILES_Tag        = "|FILES|";
        private const string _SQL_CE_CONN_STRING   = "Data Source = |SQL/CE|";
        private const string _Isolated_Storage_Tag = "|Isolated_Storage|";

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        internal static string GetDefaultConnectionString()
        {
            return _SQL_FILES_Tag;
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        internal static int IsSpecialConnectionString(string connectionString) {
            if (string.IsNullOrEmpty(connectionString))
                return 1; // Default to FILES
            if (string.Compare(connectionString, _SQL_FILES_Tag, StringComparison.OrdinalIgnoreCase) == 0)
                return 1;
            // if (string.Compare(connectionString, _Isolated_Storage_Tag, StringComparison.OrdinalIgnoreCase) == 0)
            //     return 2; -- disable isolated storage
            return 3;
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        internal static DbConnection GetConnection(string username, string connectionString, string sqlProvider)
        {
            if (connectionString.Contains(_SQL_CE_Tag) || (sqlProvider != null && sqlProvider.Contains(".SqlServerCe")))  {
                try {
                    return GetSqlCeConnection(username, connectionString);
                } catch (TypeLoadException e) {
                    throw new ArgumentException(AtlasWeb.SqlHelper_SqlEverywhereNotInstalled, e);
                }
            }

            DbConnection connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        internal static void AddParameter(DbConnection conn, DbCommand cmd, string paramName, object paramValue)
        {
            if (!(conn is SqlConnection))
                AddSqlCeParameter(cmd, paramName, paramValue);
            else
                cmd.Parameters.Add(new SqlParameter(paramName, paramValue));
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        internal static string GetCookieFromDB(string name, string username, string connectionString, string sqlProvider)
        {
            if (connectionString == _SQL_FILES_Tag)
                return ClientDataManager.GetCookie(username, name, false);
            if (connectionString == _Isolated_Storage_Tag)
                return ClientDataManager.GetCookie(username, name, true);

            using (DbConnection connection = GetConnection(username, connectionString, sqlProvider)) {
                DbCommand cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT PropertyValue FROM UserProperties WHERE PropertyName = @PropName";
                AddParameter(connection, cmd, "@PropName", "CookieName_" + name);
                return (cmd.ExecuteScalar() as string);
            }
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        internal static string StoreCookieInDB(string cookieName, string cookieValue, string username, string connectionString, string sqlProvider)
        {
            if (connectionString == _SQL_FILES_Tag)
                return ClientDataManager.StoreCookie(username, cookieName, cookieValue, false);
            if (connectionString == _Isolated_Storage_Tag)
                return ClientDataManager.StoreCookie(username, cookieName, cookieValue, true);

            string name = Guid.NewGuid().ToString("N");
            using (DbConnection connection = GetConnection(username, connectionString, sqlProvider)) {
                DbTransaction trans         = null;
                try {
                    trans = connection.BeginTransaction();
                    DbCommand cmd = connection.CreateCommand();

                    // delete old cookie
                    cmd.CommandText = "DELETE FROM UserProperties WHERE PropertyName LIKE N'CookieName_%' AND PropertyValue LIKE @PropValue";
                    cmd.Transaction = trans;
                    AddParameter(connection, cmd, "@PropValue", cookieName + "=%");
                    cmd.ExecuteNonQuery();
                    if (!string.IsNullOrEmpty(cookieValue)) {
                        cmd = connection.CreateCommand();
                        cmd.Transaction = trans;
                        cmd.CommandText = "INSERT INTO UserProperties (PropertyName, PropertyValue) VALUES (@PropName, @PropValue)";
                        AddParameter(connection, cmd, "@PropName", "CookieName_" + name);
                        AddParameter(connection, cmd, "@PropValue", cookieName + "=" + cookieValue);
                        cmd.ExecuteNonQuery();
                        return name;
                    }
                    return cookieName;
                } catch {
                    if (trans != null) {
                        trans.Rollback();
                        trans = null;
                    }
                    throw;
                } finally {
                    if (trans != null)
                        trans.Commit();
                }
            }
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        internal static void DeleteAllCookies(string username, string connectionString, string sqlProvider)
        {
            if (connectionString == _SQL_FILES_Tag || connectionString == _Isolated_Storage_Tag) {
                ClientDataManager.DeleteAllCookies(username, connectionString == _Isolated_Storage_Tag);
                return;
            }

            using (DbConnection connection = GetConnection(username, connectionString, sqlProvider)) {
                DbTransaction trans         = null;
                try {
                    trans = connection.BeginTransaction();
                    DbCommand cmd = connection.CreateCommand();
                    cmd.CommandText = "DELETE FROM UserProperties WHERE PropertyName LIKE N'CookieName_%'";
                    cmd.Transaction = trans;
                    cmd.ExecuteNonQuery();
                } catch {
                    if (trans != null) {
                        trans.Rollback();
                        trans = null;
                    }
                    throw;
                } finally {
                    if (trans != null)
                        trans.Commit();
                }
            }
        }


        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////

        private static Type _SqlCeConnectionType = null;
        private static Type _SqlCeParamType = null;

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        private static DbConnection GetSqlCeConnection(string username, string connectionString)
        {
            DbConnection conn = CreateDBIfRequired(username, connectionString);
            if (conn == null)
                conn = CreateNewSqlCeConnection(connectionString, true); //new SqlCeConnection(connectionString);
            return conn;
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        // [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        private static DbConnection CreateDBIfRequired(string username, string connectionString) {

            if (!connectionString.Contains(_SQL_CE_Tag))
                return null;

             DbConnection conn = null;
             try {
                 conn = CreateNewSqlCeConnection(connectionString, false);
                 if (string.Compare(conn.Database.Trim(), _SQL_CE_Tag, StringComparison.OrdinalIgnoreCase) != 0) {
                     conn.Open();
                     return conn;
                 }
                 conn.Dispose();
                 conn = null;
             } catch (TypeLoadException e) {
                 throw new ArgumentException(AtlasWeb.SqlHelper_SqlEverywhereNotInstalled, e);
             }


             string   fileName         = GetFullDBFileName(username, "_DB.spf");
             bool     needToCreateDB   = (File.Exists(fileName) == false);

             connectionString = connectionString.Replace(_SQL_CE_Tag, fileName);

            if (needToCreateDB) {
                 using (IDisposable engine = (IDisposable)Activator.CreateInstance(GetSqlCeType("SqlCeEngine"), new object[] { connectionString })) {
                     engine.GetType().InvokeMember("CreateDatabase", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance,
                                                   null, engine, null, CultureInfo.InvariantCulture);
                 }

                 using (conn = CreateNewSqlCeConnection(connectionString, true)) {
                     DbCommand cmd = conn.CreateCommand();
                     if (username == null) {
                         cmd.CommandText = "CREATE TABLE ApplicationProperties (PropertyName nvarchar(256), PropertyValue nvarchar(256))";
                         cmd.ExecuteNonQuery();
                     } else {
                         cmd.CommandText = "CREATE TABLE UserProperties (PropertyName nvarchar(256), PropertyValue nvarchar(256))";
                         cmd.ExecuteNonQuery();
                         cmd = conn.CreateCommand();
                         cmd.CommandText = "CREATE TABLE Roles (UserName nvarchar(256), RoleName nvarchar(256))";
                         cmd.ExecuteNonQuery();
                         cmd = conn.CreateCommand();
                         cmd.CommandText = "CREATE TABLE Settings (PropertyName nvarchar(256), PropertyStoredAs nvarchar(1), PropertyValue nvarchar(2048))";
                         cmd.ExecuteNonQuery();
                     }
                 }
            }

            return CreateNewSqlCeConnection(connectionString, true);
        }


        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        private static Type GetSqlCeType(string typeName)
        {
            // Try versionless
            Type t = Type.GetType("System.Data.SqlServerCe." + typeName + ", System.Data.SqlServerCe",
                                  false, true);
            if (t != null)
                return t;

            // Try version 3.5
            t = Type.GetType("System.Data.SqlServerCe." + typeName +
                                  ", System.Data.SqlServerCe, Version=3.5.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91",
                                  false, true);
            if (t != null)
                return t;


            // Try version 3.0
            t = Type.GetType("System.Data.SqlServerCe." + typeName +
                             ", System.Data.SqlServerCe, Version=3.0.3600.0, Culture=neutral, PublicKeyToken=3be235df1c8d2ad3",
                             false, true);
            if (t != null)
                return t;


            // Call 3.5 again to throw error
            return Type.GetType("System.Data.SqlServerCe." + typeName +
                                ", System.Data.SqlServerCe, Version=3.5.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91",
                                true, true);
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        private static DbConnection CreateNewSqlCeConnection(string connectionString, bool openConn)
        {
            if (_SqlCeConnectionType == null)
                _SqlCeConnectionType = GetSqlCeType("SqlCeConnection");
            DbConnection conn = (DbConnection) Activator.CreateInstance(_SqlCeConnectionType, new object[] {connectionString});
            if (openConn)
                conn.Open();
            return conn;
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        private static void AddSqlCeParameter(DbCommand cmd, string paramName, object paramValue)
        {
            if (_SqlCeParamType == null)
                _SqlCeParamType = GetSqlCeType("SqlCeParameter");
            cmd.Parameters.Add((DbParameter) Activator.CreateInstance(_SqlCeParamType, new object[]{paramName, paramValue}));
        }


        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        internal static string GetFullDBFileName(string username, string extension)
        {
            return Path.Combine(Application.UserAppDataPath, GetPartialDBFileName(username, extension));
        }
        internal static string GetPartialDBFileName(string username, string extension)
        {
            if (string.IsNullOrEmpty(username)) {
                return "Application" + extension;
            }

            char[] usernameChars = username.ToCharArray();
            for (int iter = 0; iter < usernameChars.Length; iter++)
                if (!char.IsLetterOrDigit(usernameChars[iter]))
                    usernameChars[iter] = '_';
            return "User_" + new string(usernameChars) + extension;
        }
    }
}
