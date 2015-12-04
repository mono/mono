//------------------------------------------------------------------------------
// <copyright file="DbProviderFactories.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.Common {

    using System;
    using System.Collections;
    using System.Configuration;
    using System.Data;
    using System.Diagnostics;
    using System.Globalization;
    using System.Xml;

    public static class DbProviderFactories {

        private const string AssemblyQualifiedName = "AssemblyQualifiedName";
        private const string Instance = "Instance";
        private const string InvariantName = "InvariantName";
        private const string Name = "Name";
        private const string Description = "Description";


        private static ConnectionState _initState; // closed, connecting, open
        private static DataTable _providerTable;
        private static object _lockobj = new object();

        static public DbProviderFactory GetFactory(string providerInvariantName) {
            ADP.CheckArgumentLength(providerInvariantName, "providerInvariantName");

            // NOTES: Include the Framework Providers and any other Providers listed in the config file.
            DataTable providerTable = GetProviderTable();
            if (null != providerTable) {
                // we don't need to copy the DataTable because its used in a controlled fashion
                // also we don't need to create a blank datatable because we know the information won't exist

#if DEBUG
                DataColumn[] pkey = providerTable.PrimaryKey;
                Debug.Assert(null != providerTable.Columns[InvariantName], "missing primary key column");
                Debug.Assert((null != pkey) && (1 == pkey.Length) && (InvariantName == pkey[0].ColumnName), "bad primary key");
#endif
                DataRow providerRow = providerTable.Rows.Find(providerInvariantName);
                if (null != providerRow) {
                    return DbProviderFactories.GetFactory(providerRow);
                }
            }
            throw ADP.ConfigProviderNotFound();
        }

        static public DbProviderFactory GetFactory(DataRow providerRow) {
            ADP.CheckArgumentNull(providerRow, "providerRow");
            
            // fail with ConfigProviderMissing rather than ColumnNotInTheTable exception
            DataColumn column = providerRow.Table.Columns[AssemblyQualifiedName];
            if (null != column) { 
                // column value may not be a string
                string assemblyQualifiedName = providerRow[column] as string;
                if (!ADP.IsEmpty(assemblyQualifiedName)) {

    // FXCop is concerned about the following line call to Get Type,
    // If this code is deemed safe during our security review we should add this warning to our exclusion list.
    // FXCop Message, pertaining to the call to GetType.
    //
    // Secure late-binding methods,System.Data.dll!System.Data.Common.DbProviderFactories.GetFactory(System.Data.DataRow):System.Data.Common.DbProviderFactory,
                    Type providerType = Type.GetType(assemblyQualifiedName);
                    if (null != providerType) {

                        System.Reflection.FieldInfo providerInstance = providerType.GetField(Instance, System.Reflection.BindingFlags.DeclaredOnly|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Static);
                        if (null != providerInstance) {
                            Debug.Assert(providerInstance.IsPublic, "field not public");
                            Debug.Assert(providerInstance.IsStatic, "field not static");

                             if (providerInstance.FieldType.IsSubclassOf(typeof(DbProviderFactory))) {

                                object factory = providerInstance.GetValue(null);
                                if (null != factory) {
                                    return (DbProviderFactory)factory;
                                }
                                // else throw ConfigProviderInvalid
                            }
                            // else throw ConfigProviderInvalid
                        }
                        throw ADP.ConfigProviderInvalid();
                    }
                    throw ADP.ConfigProviderNotInstalled();
                }
                // else throw ConfigProviderMissing
            }
            throw ADP.ConfigProviderMissing();
        }

        static public DbProviderFactory GetFactory(DbConnection connection) {
            ADP.CheckArgumentNull(connection, "connection");

            return connection.ProviderFactory;
        }
            
        static public DataTable GetFactoryClasses() { // V1.2.3300
            // NOTES: Include the Framework Providers and any other Providers listed in the config file.
            DataTable dataTable = GetProviderTable();
            if (null != dataTable) {
                dataTable = dataTable.Copy();
            }
            else {
                dataTable = DbProviderFactoriesConfigurationHandler.CreateProviderDataTable();
            }
            return dataTable;
        }

        // VSTFDevDiv # 624213: System.Data.Common.DbProviderFactories.GetFactoryClasses() still gets OracleClient provider in ClientSku environment.
        static private DataTable IncludeFrameworkFactoryClasses(DataTable configDataTable)
        {
            DataTable dataTable = DbProviderFactoriesConfigurationHandler.CreateProviderDataTable();

            // NOTES: Adding the following Framework DbProviderFactories
            //  <add name="Odbc Data Provider" invariant="System.Data.Odbc" description=".Net Framework Data Provider for Odbc" type="System.Data.Odbc.OdbcFactory, System.Data, Version=%ASSEMBLY_VERSION%, Culture=neutral, PublicKeyToken=%ECMA_PUBLICKEY%"/>
            //  <add name="OleDb Data Provider" invariant="System.Data.OleDb" description=".Net Framework Data Provider for OleDb" type="System.Data.OleDb.OleDbFactory, System.Data, Version=%ASSEMBLY_VERSION%, Culture=neutral, PublicKeyToken=%ECMA_PUBLICKEY%"/>
            //  <add name="OracleClient Data Provider" invariant="System.Data.OracleClient" description=".Net Framework Data Provider for Oracle" type="System.Data.OracleClient.OracleClientFactory, System.Data.OracleClient, Version=%ASSEMBLY_VERSION%, Culture=neutral, PublicKeyToken=%ECMA_PUBLICKEY%"/>
            //  <add name="SqlClient Data Provider" invariant="System.Data.SqlClient" description=".Net Framework Data Provider for SqlServer" type="System.Data.SqlClient.SqlClientFactory, System.Data, Version=%ASSEMBLY_VERSION%, Culture=neutral, PublicKeyToken=%ECMA_PUBLICKEY%"/>
            Type sysDataType = typeof(System.Data.SqlClient.SqlClientFactory);
            string asmQualName = sysDataType.AssemblyQualifiedName.ToString().Replace(DbProviderFactoriesConfigurationHandler.sqlclientPartialAssemblyQualifiedName, DbProviderFactoriesConfigurationHandler.oracleclientPartialAssemblyQualifiedName);
            DbProviderFactoryConfigSection[] dbFactoriesConfigSection = new DbProviderFactoryConfigSection[(int)DbProvidersIndex.DbProvidersIndexCount];
            dbFactoriesConfigSection[(int)DbProvidersIndex.Odbc] = new DbProviderFactoryConfigSection(typeof(System.Data.Odbc.OdbcFactory), DbProviderFactoriesConfigurationHandler.odbcProviderName, DbProviderFactoriesConfigurationHandler.odbcProviderDescription);
            dbFactoriesConfigSection[(int)DbProvidersIndex.OleDb] = new DbProviderFactoryConfigSection(typeof(System.Data.OleDb.OleDbFactory), DbProviderFactoriesConfigurationHandler.oledbProviderName, DbProviderFactoriesConfigurationHandler.oledbProviderDescription);
            dbFactoriesConfigSection[(int)DbProvidersIndex.OracleClient] = new DbProviderFactoryConfigSection(DbProviderFactoriesConfigurationHandler.oracleclientProviderName, DbProviderFactoriesConfigurationHandler.oracleclientProviderNamespace, DbProviderFactoriesConfigurationHandler.oracleclientProviderDescription, asmQualName);
            dbFactoriesConfigSection[(int)DbProvidersIndex.SqlClient] = new DbProviderFactoryConfigSection(typeof(System.Data.SqlClient.SqlClientFactory), DbProviderFactoriesConfigurationHandler.sqlclientProviderName, DbProviderFactoriesConfigurationHandler.sqlclientProviderDescription);

            for (int i = 0; i < dbFactoriesConfigSection.Length; i++)
            {
                if (dbFactoriesConfigSection[i].IsNull() == false)
                {
                    bool flagIncludeToTable = false;

                    if (i == ((int)DbProvidersIndex.OracleClient)) // OracleClient Provider: Include only if it installed
                    {
                        Type providerType = Type.GetType(dbFactoriesConfigSection[i].AssemblyQualifiedName);
                        if (providerType != null)
                        {
                            // NOTES: Try and create a instance; If it fails, it will throw a System.NullReferenceException exception;
                            System.Reflection.FieldInfo providerInstance = providerType.GetField(Instance, System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                            if ((null != providerInstance) && (providerInstance.FieldType.IsSubclassOf(typeof(DbProviderFactory))))
                            {
                                Debug.Assert(providerInstance.IsPublic, "field not public");
                                Debug.Assert(providerInstance.IsStatic, "field not static");

                                object factory = providerInstance.GetValue(null);
                                if (null != factory)
                                {
                                    flagIncludeToTable = true;
                                } // Else ignore and don't add to table
                            } // Else ignore and don't add to table
                        }
                    }
                    else
                    {
                        flagIncludeToTable = true;
                    }

                    if (flagIncludeToTable == true)
                    {
                        DataRow row = dataTable.NewRow();
                        row[Name] = dbFactoriesConfigSection[i].Name;
                        row[InvariantName] = dbFactoriesConfigSection[i].InvariantName;
                        row[Description] = dbFactoriesConfigSection[i].Description;
                        row[AssemblyQualifiedName] = dbFactoriesConfigSection[i].AssemblyQualifiedName;
                        dataTable.Rows.Add(row);
                    } // Else Ignore and do not include to table;
                }
            }

            // NOTES: Additional step added here to maintain the sequence order of the providers listed.
            // The Framework Providers get listed first and is followed the custom providers.
            for (int i = 0; (configDataTable != null) && (i < configDataTable.Rows.Count); i++)
            {
                try
                {
                    bool flagIncludeToTable = false;

                    // OracleClient Provider: Include only if it installed
                    if (configDataTable.Rows[i][AssemblyQualifiedName].ToString().ToLowerInvariant().Contains(DbProviderFactoriesConfigurationHandler.oracleclientProviderNamespace.ToString().ToLowerInvariant()))
                    {
                        Type providerType = Type.GetType(configDataTable.Rows[i][AssemblyQualifiedName].ToString());
                        if (providerType != null)
                        {
                            // NOTES: Try and create a instance; If it fails, it will throw a System.NullReferenceException exception;
                            System.Reflection.FieldInfo providerInstance = providerType.GetField(Instance, System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                            if ((null != providerInstance) && (providerInstance.FieldType.IsSubclassOf(typeof(DbProviderFactory))))
                            {
                                Debug.Assert(providerInstance.IsPublic, "field not public");
                                Debug.Assert(providerInstance.IsStatic, "field not static");

                                object factory = providerInstance.GetValue(null);
                                if (null != factory)
                                {
                                    flagIncludeToTable = true;
                                } // Else ignore and don't add to table
                            } // Else ignore and don't add to table
                        }
                    }
                    else
                    {
                        flagIncludeToTable = true;
                    }

                    if(flagIncludeToTable == true)
                    {
                        // NOTES: If it already exist in the configTable, it raises a ConstraintException;
                        dataTable.Rows.Add(configDataTable.Rows[i].ItemArray);
                    }
                }
                catch (System.Data.ConstraintException) 
                {
                    // NOTES: Ignore item; Already exist in the configTable, hence the ConstraintException; Move to the next item;
                }
            }

            return dataTable;
        }

        static private DataTable GetProviderTable() {
            Initialize();
            return _providerTable;
        }

        static private void Initialize() {
            if (ConnectionState.Open != _initState) {
                lock (_lockobj) {
                    switch(_initState) {
                    case ConnectionState.Closed:
                        _initState = ConnectionState.Connecting; // used for preventing recursion
                        try {
                            DataSet configTable = PrivilegedConfigurationManager.GetSection(DbProviderFactoriesConfigurationHandler.sectionName) as DataSet;
                            _providerTable = (null != configTable) ? IncludeFrameworkFactoryClasses(configTable.Tables[DbProviderFactoriesConfigurationHandler.providerGroup]) : IncludeFrameworkFactoryClasses(null);
                        }
                        finally {
                            _initState = ConnectionState.Open;
                        }
                        break;
                    case ConnectionState.Connecting:
                    case ConnectionState.Open:
                        break;
                    default:
                        Debug.Assert(false, "unexpected state");
                        break;
                    }
                }
            }
        }
    }
}
