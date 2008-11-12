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
using DataContext = System.Data.Linq.DataContext;
using Data = System.Data;
using System.Data.Linq;
#else
using DataContext = DbLinq.Data.Linq.DataContext;
using Data = DbLinq.Data;
using DbLinq.Data.Linq;
#endif
using IExecuteResult = System.Data.Linq.IExecuteResult;

namespace DbLinq.Vendor.Implementation
{
    /// <summary>
    /// some IVendor functionality is the same for many vendors,
    /// implemented here as virtual functions.
    /// </summary>
#if MONO_STRICT
    internal
#else
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
        protected virtual void AddConnectionStringPart(IList<string> parts, string name, string value)
        {
            if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(name))
                parts.Add(string.Format("{0}={1}", name, value));
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
            var connectionStringParts = new List<string>();
            AddConnectionStringPart(connectionStringParts, ConnectionStringServer, host);
            AddConnectionStringPart(connectionStringParts, ConnectionStringDatabase, databaseName);
            AddConnectionStringPart(connectionStringParts, ConnectionStringUser, userName);
            AddConnectionStringPart(connectionStringParts, ConnectionStringPassword, password);
            return string.Join(";", connectionStringParts.ToArray());
        }

        /// <summary>
        /// used during DataContext ctor -
        /// - to ask specific DLL and class to load an IDbConnection object from
        /// </summary>
        /// <returns></returns>
        protected abstract TypeToLoadData GetProviderTypeName();

        /// <summary>
        /// called from DataContext ctor, which needs to create an IDbConnection, given an IVendor
        /// </summary>
        public IDbConnection CreateDbConnection(string connectionString)
        {
            TypeToLoadData typeToLoad = GetProviderTypeName();
            string assemblyToLoad = typeToLoad.assemblyName; //e.g. "System.Data.SQLite.DLL",
            Assembly assy;
            try
            {
                //TODO: check if DLL is already loaded?
                assy = Assembly.LoadFrom(assemblyToLoad);
            }
            catch (Exception ex)
            {
                //TODO: add proper logging here
                Console.WriteLine("DataContext ctor: Assembly load failed for " + assemblyToLoad + ": " + ex);
                throw ex;
            }
            Type[] STRING_PARAM = new Type[] { typeof(string) };

            //find IDbProvider class in this assembly:
            var ctors = (from mod in assy.GetModules()
                         from cls in mod.GetTypes()
                         where cls.GetInterfaces().Contains(typeof(IDbConnection))
                         let ctorInfo = cls.GetConstructor(STRING_PARAM)
                         where ctorInfo != null
                         select ctorInfo).ToList();
            if (ctors.Count == 0)
            {
                string msg = "Found no IVendor class in assembly " + assemblyToLoad + " having a string ctor";
                throw new ArgumentException(msg);
            }
            else if (ctors.Count > 1)
            {
                string msg = "Found more than one IVendor class in assembly " + assemblyToLoad + " having a string ctor";
                throw new ArgumentException(msg);
            }
            ConstructorInfo ctorInfo2 = ctors[0];

            object iDbConnObject;
            try
            {
                iDbConnObject = ctorInfo2.Invoke(new object[] { connectionString });
            }
            catch (Exception ex)
            {
                //TODO: add proper logging here
                Console.WriteLine("DataContext/Vendor: Failed to invoke IDbConnection ctor " + ctorInfo2.Name + ": " + ex);
                throw ex;
            }
            var connection = (IDbConnection)iDbConnObject;
            return connection;
        }

        // TODO: update, this is obsolete
        public class TypeToLoadData
        {
            public string assemblyName;
            public string className;
        }
    }
}
