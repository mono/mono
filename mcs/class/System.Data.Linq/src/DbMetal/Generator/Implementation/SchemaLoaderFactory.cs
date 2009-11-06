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
            GetLoaderAndConnection(out dbLinqSchemaLoaderType, out databaseConnectionType, parameters);
            if (dbLinqSchemaLoaderType == null)
                throw new ApplicationException("Please provide -Provider=MySql (or Oracle, OracleODP, PostgreSql, Sqlite - see app.config for provider listing)");
            return Load(parameters, dbLinqSchemaLoaderType, databaseConnectionType);
        }

        /// <summary>
        /// loads a ISchemaLoader from a provider id string (used by schema loader)
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public ISchemaLoader Load(string provider)
        {
            string dbLinqSchemaLoaderType;
            string databaseConnectionType;
            GetLoaderAndConnection(out dbLinqSchemaLoaderType, out databaseConnectionType, provider);
            if (dbLinqSchemaLoaderType == null)
                return null;
            return Load(null, dbLinqSchemaLoaderType, databaseConnectionType);
        }

        /// <summary>
        /// given a schemaLoaderType and dbConnType 
        /// (e.g. DbLinq.Oracle.OracleSchemaLoader and System.Data.OracleClient.OracleConnection),
        /// return an instance of the OracleSchemaLoader.
        /// </summary>
        public ISchemaLoader Load(Parameters parameters, Type dbLinqSchemaLoaderType, Type databaseConnectionType)
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

        public ISchemaLoader Load(Parameters parameters, string dbLinqSchemaLoaderTypeName, string databaseConnectionTypeName)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            Type dbLinqSchemaLoaderType = Type.GetType(dbLinqSchemaLoaderTypeName);
            Type databaseConnectionType = Type.GetType(databaseConnectionTypeName);

            if (dbLinqSchemaLoaderType == null)
                throw new ArgumentException("Unable to resolve dbLinqSchemaLoaderType: " + dbLinqSchemaLoaderTypeName);
            if (databaseConnectionType == null)
                throw new ArgumentException("Unable to resolve databaseConnectionType: " + databaseConnectionTypeName);

            ISchemaLoader loader = Load(parameters, dbLinqSchemaLoaderType, databaseConnectionType);
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
            return loader;
        }

        private Assembly LoadAssembly(string path)
        {
            if (File.Exists(path))
                return Assembly.LoadFile(path);
            return null;
        }

        private Assembly LoadAssembly(string baseName, string path)
        {
            string basePath = Path.Combine(path, baseName);
            Assembly assembly = LoadAssembly(basePath + ".dll");
            if (assembly == null)
                assembly = LoadAssembly(basePath + ".exe");
            return assembly;
        }

        private Assembly LocalLoadAssembly(string baseName)
        {
            Assembly assembly = LoadAssembly(baseName, Directory.GetCurrentDirectory());
            if (assembly == null) {
                var path = Path.GetDirectoryName(Assembly.GetEntryAssembly().CodeBase);
                assembly = LoadAssembly(baseName, path);
            }
            return assembly;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // try to load from within the current AppDomain
            IList<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly loadedAssembly in assemblies)
            {
                if (loadedAssembly.GetName().Name == args.Name)
                    return loadedAssembly;
            }
            var assembly = LocalLoadAssembly(args.Name);
            if (assembly == null)
                assembly = GacLoadAssembly(args.Name);
            return assembly;
        }

        /// <summary>
        /// This is dirty, and must not be used in production environment
        /// </summary>
        /// <param name="shortName"></param>
        /// <returns></returns>
        private Assembly GacLoadAssembly(string shortName)
        {
            var systemRoot = Environment.GetEnvironmentVariable("SystemRoot");
            string assemblyDirectory = Path.Combine(systemRoot, "Assembly");
            var assembly = GacLoadAssembly(shortName, Path.Combine(assemblyDirectory, "GAC_MSIL"));
            if (assembly == null)
                assembly = GacLoadAssembly(shortName, Path.Combine(assemblyDirectory, "GAC_32"));
            if (assembly == null)
                assembly = GacLoadAssembly(shortName, Path.Combine(assemblyDirectory, "GAC"));
            return assembly;
        }

        private Assembly GacLoadAssembly(string shortName, string directory)
        {
            string assemblyDirectory = Path.Combine(directory, shortName);
            if (Directory.Exists(assemblyDirectory))
            {
                Version latestVersion = null;
                string latestVersionDirectory = null;
                foreach (string versionDirectory in Directory.GetDirectories(assemblyDirectory))
                {
                    var testVersion = new Version(Path.GetFileName(versionDirectory).Split('_')[0]);
                    if (latestVersion == null || testVersion.CompareTo(latestVersion) > 0)
                    {
                        latestVersion = testVersion;
                        latestVersionDirectory = versionDirectory;
                    }
                }
                if (latestVersionDirectory != null)
                {
                    string assemblyPath = Path.Combine(latestVersionDirectory, shortName + ".dll");
                    if (File.Exists(assemblyPath))
                        return Assembly.LoadFile(assemblyPath);
                }
            }
            return null;
        }

        protected void GetLoaderAndConnection(out string dbLinqSchemaLoaderType, out string databaseConnectionType, string provider)
        {
            var configuration = (ProvidersSection)ConfigurationManager.GetSection("providers");

            ProvidersSection.ProviderElement element;
            string errorMsg = null;
            if (configuration == null || !configuration.Providers.TryGetProvider(provider, out element, out errorMsg))
            {
                Output.WriteErrorLine(Log, "Failed to load provider {0} : {1}", provider, errorMsg);
                throw new ApplicationException("Failed to load provider " + provider);
            }

            //var element = configuration.Providers.GetProvider(provider);
            //databaseConnectionType = types[1].Trim();
            dbLinqSchemaLoaderType = element.DbLinqSchemaLoader;
            databaseConnectionType = element.DatabaseConnection;
        }

        protected void GetLoaderAndConnection(out string dbLinqSchemaLoaderType, out string databaseConnectionType, Parameters parameters)
        {
            if (!string.IsNullOrEmpty(parameters.Provider))
            {
                GetLoaderAndConnection(out dbLinqSchemaLoaderType, out databaseConnectionType, parameters.Provider);
            }
            else
            {
                dbLinqSchemaLoaderType = parameters.DbLinqSchemaLoaderProvider;
                databaseConnectionType = parameters.DatabaseConnectionProvider;
            }
        }

    }
}
