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
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Reflection;
using DbLinq.Util;
using DbLinq.Vendor;
using DbMetal.Configuration;

namespace DbMetal.Generator.Implementation
{
#if !MONO_STRICT
    public
#endif
    class SchemaLoaderFactory : ISchemaLoaderFactory
    {
        private TextWriter log;
        /// <summary>
        /// Log output
        /// </summary>
        public TextWriter Log
        {
            get { return log ?? Console.Out; }
            set { log = value; }
        }

        /// <summary>
        /// the 'main entry point' into this class
        /// </summary>
        public ISchemaLoader Load(Parameters parameters)
        {
            string dbLinqSchemaLoaderType;
            string databaseConnectionType;
            string sqlDialectType;
            GetLoaderAndConnection(parameters, out dbLinqSchemaLoaderType, out databaseConnectionType, out sqlDialectType);
            return Load(parameters, dbLinqSchemaLoaderType, databaseConnectionType, sqlDialectType);
        }

        /// <summary>
        /// given a schemaLoaderType and dbConnType 
        /// (e.g. DbLinq.Oracle.OracleSchemaLoader and System.Data.OracleClient.OracleConnection),
        /// return an instance of the OracleSchemaLoader.
        /// </summary>
        public ISchemaLoader Load(Parameters parameters, Type dbLinqSchemaLoaderType, Type databaseConnectionType, Type sqlDialectType)
        {
            if (dbLinqSchemaLoaderType == null)
                throw new ArgumentNullException("dbLinqSchemaLoaderType");
            if (databaseConnectionType == null)
                throw new ArgumentNullException("databaseConnectionType");

            string errorMsg = "";
            try
            {
                errorMsg = "Failed on Activator.CreateInstance(" + dbLinqSchemaLoaderType.Name + ")";
                var loader = (ISchemaLoader)Activator.CreateInstance(dbLinqSchemaLoaderType);
                // set log output
                loader.Log = Log;

                errorMsg = "Failed on Activator.CreateInstance(" + databaseConnectionType.Name + ")";
                var connection = (IDbConnection)Activator.CreateInstance(databaseConnectionType);

                IVendor vendor = null;
                if (sqlDialectType != null)
                {
                    errorMsg = "Failed on Activator.CreateInstance(" + sqlDialectType.Name + ")";
                    vendor = (IVendor)Activator.CreateInstance(sqlDialectType);
                    loader.Vendor = vendor;
                }

                if (parameters != null)
                {
                    string connectionString = parameters.Conn;
                    if (string.IsNullOrEmpty(connectionString))
                        connectionString = loader.Vendor.BuildConnectionString(parameters.Server, parameters.Database,
                                                                               parameters.User, parameters.Password);
                    errorMsg = "Failed on setting ConnectionString=" + connectionString;
                    connection.ConnectionString = connectionString;
                }

                errorMsg = "";
                loader.Connection = connection;
                return loader;
            }
            catch (Exception ex)
            {
                //see Pascal's comment on this failure:
                //http://groups.google.com/group/dblinq/browse_thread/thread/b7a29138435b0678
                Output.WriteErrorLine(Log, "LoaderFactory.Load(schemaType=" + dbLinqSchemaLoaderType.Name + ", dbConnType=" + databaseConnectionType.Name + ")");
                if (errorMsg != "")
                    Output.WriteErrorLine(Log, errorMsg);
                Output.WriteErrorLine(Log, "LoaderFactory.Load() failed: " + ex.Message);
                throw;
            }
        }

        public ISchemaLoader Load(Parameters parameters, string dbLinqSchemaLoaderTypeName, string databaseConnectionTypeName, string sqlDialectTypeName)
        {
            Type dbLinqSchemaLoaderType = Type.GetType(dbLinqSchemaLoaderTypeName);
            Type databaseConnectionType = Type.GetType(databaseConnectionTypeName);
            Type sqlDialectType         = string.IsNullOrEmpty(sqlDialectTypeName) ? null : Type.GetType(sqlDialectTypeName);

            if (dbLinqSchemaLoaderType == null)
                throw new ArgumentException("Could not load dbLinqSchemaLoaderType type '" + dbLinqSchemaLoaderTypeName + "'.  Try using the --with-schema-loader=TYPE option.");
            if (databaseConnectionType == null)
                throw new ArgumentException("Could not load databaseConnectionType type '" + databaseConnectionTypeName + "'.  Try using the --with-dbconnection=TYPE option.");

            ISchemaLoader loader = Load(parameters, dbLinqSchemaLoaderType, databaseConnectionType, sqlDialectType);
            return loader;
        }

        private void GetLoaderAndConnection(string provider, out string dbLinqSchemaLoaderType, out string databaseConnectionType, out string sqlDialectType)
        {
            var configuration = (ProvidersSection)ConfigurationManager.GetSection("providers");

            ProvidersSection.ProviderElement element;
            string errorMsg = null;
            if (configuration == null || !configuration.Providers.TryGetProvider(provider, out element, out errorMsg))
            {
                throw new ApplicationException(string.Format("Failed to load provider {0}: {1}", provider, errorMsg));
            }

            //var element = configuration.Providers.GetProvider(provider);
            //databaseConnectionType = types[1].Trim();
            dbLinqSchemaLoaderType = element.DbLinqSchemaLoader;
            databaseConnectionType = element.DatabaseConnection;
            sqlDialectType         = element.SqlDialectType;
        }

        protected void GetLoaderAndConnection(Parameters parameters, out string dbLinqSchemaLoaderType, out string databaseConnectionType, out string sqlDialectType)
        {
            dbLinqSchemaLoaderType = databaseConnectionType = sqlDialectType = null;

            if (!string.IsNullOrEmpty(parameters.Provider))
            {
                GetLoaderAndConnection(parameters.Provider, out dbLinqSchemaLoaderType, out databaseConnectionType, out sqlDialectType);
            }
            else if (!string.IsNullOrEmpty(parameters.DbLinqSchemaLoaderProvider) &&
                !string.IsNullOrEmpty(parameters.DatabaseConnectionProvider) &&
                !string.IsNullOrEmpty(parameters.SqlDialectType))
            {
                // User manually specified everything we need; don't bother with the
                // default .exe.config provider lookup
            }
            else
            {
                // No provider specified, not explicitly provided either
                // Default to using SQL Server for sqlmetal.exe compatibility
                GetLoaderAndConnection("SqlServer", out dbLinqSchemaLoaderType, out databaseConnectionType, out sqlDialectType);
            }

            // Allow command-line options to override the .exe.config file.
            if (!string.IsNullOrEmpty(parameters.DbLinqSchemaLoaderProvider))
                dbLinqSchemaLoaderType = parameters.DbLinqSchemaLoaderProvider;
            if (!string.IsNullOrEmpty(parameters.DatabaseConnectionProvider))
                databaseConnectionType = parameters.DatabaseConnectionProvider;
            if (!string.IsNullOrEmpty(parameters.SqlDialectType))
                sqlDialectType = parameters.SqlDialectType;
        }

    }
}
