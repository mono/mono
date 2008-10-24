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
        public virtual bool Ping(DataContext dataContext)
        {
            return dataContext.ExecuteCommand("SELECT 11") == 11;
        }

        public abstract ISqlProvider SqlProvider { get; }

        public virtual void BulkInsert<T>(Table<T> table, List<T> rows, int pageSize, IDbTransaction transaction) where T : class
        {
            throw new NotImplementedException();
        }

        public abstract string VendorName { get; }

        public abstract IExecuteResult ExecuteMethodCall(DataContext context, MethodInfo method, params object[] sqlParams);

        protected virtual IDbDataAdapter CreateDataAdapter(DataContext dataContext)
        {
            return dataContext.CreateDataAdapter();
        }

        protected virtual string ConnectionStringServer { get { return "server"; } }
        protected virtual string ConnectionStringUser { get { return "user id"; } }
        protected virtual string ConnectionStringPassword { get { return "password"; } }
        protected virtual string ConnectionStringDatabase { get { return "database"; } }

        protected virtual void AddConnectionStringPart(IList<string> parts, string name, string value)
        {
            if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(name))
                parts.Add(string.Format("{0}={1}", name, value));
        }

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
            IDbConnection iDbConn = (IDbConnection)iDbConnObject;
            return iDbConn;
        }

        public class TypeToLoadData
        {
            public string assemblyName;
            public string className;
        }


    }
}
