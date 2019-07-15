#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using System.Linq;

#if MONO_STRICT
using System.Data.Linq;
#else
using DbLinq.Data.Linq;
#endif

using Data = DbLinq.Data;

using IExecuteResult = System.Data.Linq.IExecuteResult;
using System.Text;

namespace DbLinq.Vendor.Implementation
{
    /// <summary>
    /// some IVendor functionality is the same for many vendors,
    /// implemented here as virtual functions.
    /// </summary>
#if !MONO_STRICT
    public
#endif
    abstract partial class Vendor : IVendor
    {
        /// <summary>
        /// Pings requested DB, true is result is OK.
        /// May throw a connection exception (DataContext.DatabaseExists() handles this)
        /// </summary>
        /// <param name="dataContext"></param>
        /// <returns></returns>
        public virtual bool Ping(DataContext dataContext)
        {
            return dataContext.ExecuteCommand("SELECT 11") == 11;
        }

        /// <summary>
        /// Component used to provide SQL Parts
        /// </summary>
        /// <value></value>
        public abstract ISqlProvider SqlProvider { get; }

        /// <summary>
        /// Performs bulk insert.
        /// Please notice that PKs may not be updated
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="rows"></param>
        /// <param name="pageSize"></param>
        /// <param name="transaction"></param>
        public virtual void BulkInsert<T>(Table<T> table, List<T> rows, int pageSize, IDbTransaction transaction) where T : class
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// VendorName represents the database being handled by this vendor 'Oracle' or 'MySql'
        /// </summary>
        /// <value></value>
        public abstract string VendorName { get; }

        /// <summary>
        /// Executes a stored procedure/function call
        /// </summary>
        /// <param name="context"></param>
        /// <param name="method"></param>
        /// <param name="sqlParams"></param>
        /// <returns></returns>
        public abstract IExecuteResult ExecuteMethodCall(DataContext context, MethodInfo method, params object[] sqlParams);

        /// <summary>
        /// Creates the data adapter.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        /// <returns></returns>
        protected virtual IDbDataAdapter CreateDataAdapter(DataContext dataContext)
        {
            return dataContext.CreateDataAdapter();
        }

        /// <summary>
        /// Gets the connection string server part name.
        /// </summary>
        /// <value>The connection string server.</value>
        protected virtual string ConnectionStringServer { get { return "server"; } }
        /// <summary>
        /// Gets the connection string user part name.
        /// </summary>
        /// <value>The connection string user.</value>
        protected virtual string ConnectionStringUser { get { return "user id"; } }
        /// <summary>
        /// Gets the connection string password part name.
        /// </summary>
        /// <value>The connection string password.</value>
        protected virtual string ConnectionStringPassword { get { return "password"; } }
        /// <summary>
        /// Gets the connection string database part name.
        /// </summary>
        /// <value>The connection string database.</value>
        protected virtual string ConnectionStringDatabase { get { return "database"; } }

        /// <summary>
        /// Adds the connection string part.
        /// </summary>
        /// <param name="parts">The parts.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        protected void AppendConnectionString(StringBuilder connectionString, string name, string value)
        {
            if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(name))
                connectionString.AppendFormat("{0}={1};", name, value);
        }

        protected virtual void AppendDatabase(StringBuilder connectionString, string databaseName)
        {
            AppendConnectionString(connectionString, ConnectionStringDatabase, databaseName);
        }

        protected virtual void AppendPassword(StringBuilder connectionString, string password)
        {
            AppendConnectionString(connectionString, ConnectionStringPassword, password);
        }

        protected virtual void AppendServer(StringBuilder connectionString, string host)
        {
            // A majority of databases want a server/host port number as a separate key:
            //      http://www.connectionstrings.com/postgre-sql
            //      http://www.connectionstrings.com/firebird
            //      http://www.connectionstrings.com/mysql
            // So make this the default.
            if (host == null)
                return;
            var colonIdx = host.IndexOf(':');
            string port = colonIdx < 0 || host.Length == (colonIdx + 1) ? null : host.Substring(colonIdx + 1);
            if (colonIdx >= 0)
                host = host.Substring(0, colonIdx);
            AppendConnectionString(connectionString, ConnectionStringServer, host);
            if (port != null)
                AppendConnectionString(connectionString, "Port", port);
        }

        protected virtual void AppendUser(StringBuilder connectionString, string userName)
        {
            AppendConnectionString(connectionString, ConnectionStringUser, userName);
        }

        /// <summary>
        /// Builds a connection string given the input parameters
        /// </summary>
        /// <param name="host">Server host</param>
        /// <param name="databaseName">Database (or schema) name</param>
        /// <param name="userName">Login user name</param>
        /// <param name="password">Login password</param>
        /// <returns></returns>
        public virtual string BuildConnectionString(string host, string databaseName, string userName, string password)
        {
            var connectionString = new StringBuilder();
            AppendServer(connectionString, host);
            AppendDatabase(connectionString, databaseName);
            AppendUser(connectionString, userName);
            AppendPassword(connectionString, password);
            return connectionString.ToString();
        }

        /// <summary>
        /// called from DataContext ctor, which needs to create an IDbConnection, given an IVendor
        /// </summary>
        public IDbConnection CreateDbConnection(string connectionString)
        {
            var reConnectionType = new System.Text.RegularExpressions.Regex(@"DbLinqConnectionType=([^;]*);?");
            string connTypeVal = null;
            if (!reConnectionType.IsMatch(connectionString))
            {
                connTypeVal = "System.Data.SqlClient.SqlConnection, System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
            }
            else
            {
                var    match        = reConnectionType.Match(connectionString);
                connTypeVal         = match.Groups[1].Value;
                connectionString    = reConnectionType.Replace(connectionString, "");
            }

            var    connType     = Type.GetType(connTypeVal);
            if (connType == null)
                throw new ArgumentException(string.Format(
                        "Could not load the specified DbLinqConnectionType `{0}'.",
                        connTypeVal),
                    "connectionString");
            return (IDbConnection)Activator.CreateInstance(connType, connectionString);
        }
    }
}
