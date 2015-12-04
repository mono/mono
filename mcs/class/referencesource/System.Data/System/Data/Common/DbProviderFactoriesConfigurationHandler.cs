//------------------------------------------------------------------------------
// <copyright file="DbProviderFactoriesConfigurationHandler.cs" company="Microsoft">
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

    // VSTFDevDiv # 624213: System.Data.Common.DbProviderFactories.GetFactoryClasses() still gets OracleClient provider in ClientSku environment.
    // NOTES: As part of this bug fix, the decision was taken to make it consistent and to remove all the framework 
    //      providers from the list in the machine.config file. The DbProviderFactories section of the machine.config will contain only
    //      custom providers names and details.
    internal enum DbProvidersIndex : int
    {
        Odbc = 0,
        OleDb,
        OracleClient,
        SqlClient,
        DbProvidersIndexCount // As enums are 0-based index, the DbProvidersIndexCount will hold the maximum count of the enum objects;
    }

    internal class DbProviderFactoryConfigSection
    {
        Type factType;
        string name;
        string invariantName;
        string description;
        string assemblyQualifiedName;

        public DbProviderFactoryConfigSection(Type FactoryType, string FactoryName, string FactoryDescription)
        {
            try
            {
                factType = FactoryType;
                name = FactoryName;
                invariantName = factType.Namespace.ToString();
                description = FactoryDescription;
                assemblyQualifiedName = factType.AssemblyQualifiedName.ToString();
            }
            catch 
            {
                factType = null;
                name = string.Empty;
                invariantName = string.Empty;
                description = string.Empty;
                assemblyQualifiedName = string.Empty;
            }
        }

        public DbProviderFactoryConfigSection(string FactoryName, string FactoryInvariantName, string FactoryDescription, string FactoryAssemblyQualifiedName)
        {
            factType = null;
            name = FactoryName;
            invariantName = FactoryInvariantName;
            description = FactoryDescription;
            assemblyQualifiedName = FactoryAssemblyQualifiedName;
        }

        public bool IsNull()
        {
            if ((factType == null) && (invariantName == string.Empty))
                return true;
            else
                return false;
        }

        public string Name
        {
            get { return name; }
        }

        public string InvariantName
        {
            get { return invariantName; }
        }

        public string Description
        {
            get { return description; }
        }

        public string AssemblyQualifiedName
        {
            get { return assemblyQualifiedName; }
        }
    }

    // <configSections>
    //     <section name="system.data" type="System.Data.Common.DbProviderFactoriesConfigurationHandler, System.Data, Version=%ASSEMBLY_VERSION%, Culture=neutral, PublicKeyToken=%ECMA_PUBLICKEY%" />
    // </configSections>
    // <system.data>
    //     <DbProviderFactories>
    //         <add name="Odbc Data Provider"         invariant="System.Data.Odbc"         support="1BF" description=".Net Framework Data Provider for Odbc"      type="System.Data.Odbc.OdbcFactory, System.Data, Version=%ASSEMBLY_VERSION%, Culture=neutral, PublicKeyToken=%ECMA_PUBLICKEY%"/>
    //         <add name="OleDb Data Provider"        invariant="System.Data.OleDb"        support="1BF" description=".Net Framework Data Provider for OleDb"     type="System.Data.OleDb.OleDbFactory, System.Data, Version=%ASSEMBLY_VERSION%, Culture=neutral, PublicKeyToken=%ECMA_PUBLICKEY%"/>
    //         <add name="OracleClient Data Provider" invariant="System.Data.OracleClient" support="1AF" description=".Net Framework Data Provider for Oracle"    type="System.Data.OracleClient.OracleFactory, System.Data.OracleClient, Version=%ASSEMBLY_VERSION%, Culture=neutral, PublicKeyToken=%ECMA_PUBLICKEY%"/>
    //         <add name="SqlClient Data Provider"    invariant="System.Data.SqlClient"    support="1FF" description=".Net Framework Data Provider for SqlServer" type="System.Data.SqlClient.SqlClientFactory, System.Data, Version=%ASSEMBLY_VERSION%, Culture=neutral, PublicKeyToken=%ECMA_PUBLICKEY%"/>
    //     </DbProviderFactories>
    // </system.data>
    // this class is delayed created, use ConfigurationSettings.GetSection("system.data") to obtain
    public class DbProviderFactoriesConfigurationHandler : IConfigurationSectionHandler { // V1.2.3300
        internal const string sectionName = "system.data";
        internal const string providerGroup = "DbProviderFactories";

        // NOTES: Framework-Based DbProviderFactories Details
        internal const string odbcProviderName = "Odbc Data Provider";
        internal const string odbcProviderDescription = ".Net Framework Data Provider for Odbc";

        internal const string oledbProviderName = "OleDb Data Provider";
        internal const string oledbProviderDescription = ".Net Framework Data Provider for OleDb";

        internal const string oracleclientProviderName = "OracleClient Data Provider";
        internal const string oracleclientProviderNamespace = "System.Data.OracleClient";
        internal const string oracleclientProviderDescription = ".Net Framework Data Provider for Oracle";

        internal const string sqlclientProviderName = "SqlClient Data Provider";
        internal const string sqlclientProviderDescription = ".Net Framework Data Provider for SqlServer";

        internal const string sqlclientPartialAssemblyQualifiedName = "System.Data.SqlClient.SqlClientFactory, System.Data,";
        internal const string oracleclientPartialAssemblyQualifiedName = "System.Data.OracleClient.OracleClientFactory, System.Data.OracleClient,";

        public DbProviderFactoriesConfigurationHandler() { // V1.2.3300
        }

        virtual public object Create(object parent, object configContext, XmlNode section) { // V1.2.3300
#if DEBUG
            try {
#endif
                return CreateStatic(parent, configContext, section);
#if DEBUG
            }
            catch(Exception e) {
                ADP.TraceExceptionWithoutRethrow(e); // it will be rethrown
                throw;
            }
#endif
        }

        static internal object CreateStatic(object parent, object configContext, XmlNode section) {            
            object config = parent;
            if (null != section) {
                config = HandlerBase.CloneParent(parent as DataSet, false);
                bool foundFactories = false;

                HandlerBase.CheckForUnrecognizedAttributes(section);
                foreach (XmlNode child in section.ChildNodes) {
                    if (HandlerBase.IsIgnorableAlsoCheckForNonElement(child)) {
                        continue;
                    }
                    string sectionGroup = child.Name;
                    switch(sectionGroup) {
                    case DbProviderFactoriesConfigurationHandler.providerGroup:
                        if (foundFactories) {
                            throw ADP.ConfigSectionsUnique(DbProviderFactoriesConfigurationHandler.providerGroup);
                        }
                        foundFactories = true;
                        HandleProviders(config as DataSet, configContext, child, sectionGroup);
                        break;
                    default:
                        throw ADP.ConfigUnrecognizedElement(child);
                    }
                }
            }
            return config;
        }

        // sectionName - i.e. "providerconfiguration"
        private static void HandleProviders(DataSet config, object configContext, XmlNode section, string sectionName) {
            DataTableCollection tables = config.Tables;
            DataTable dataTable = tables[sectionName];
            bool tableExisted = (null != dataTable);
            dataTable = DbProviderDictionarySectionHandler.CreateStatic(dataTable, configContext, section);
            if (!tableExisted) {
                tables.Add(dataTable);
            }
        }

        // based off of DictionarySectionHandler
        private static class DbProviderDictionarySectionHandler/* : IConfigurationSectionHandler*/ {
            /*
            internal DbProviderDictionarySectionHandler() {
            }

            public object Create(Object parent, Object context, XmlNode section) {
                return CreateStatic(parent, context, section);
            }
            */

            static internal DataTable CreateStatic(DataTable config, Object context, XmlNode section) {
                if (null != section) {
                    HandlerBase.CheckForUnrecognizedAttributes(section);
                    
                    if (null == config) {
                        config = DbProviderFactoriesConfigurationHandler.CreateProviderDataTable();
                    }
                    // else already copied via DataSet.Copy

                    foreach (XmlNode child in section.ChildNodes) {
                        if (HandlerBase.IsIgnorableAlsoCheckForNonElement(child)) {
                            continue;
                        }
                        switch(child.Name) {
                        case "add":
                            HandleAdd(child, config);
                            break;
                        case "remove":
                            HandleRemove(child, config);
                            break;
                        case "clear":
                            HandleClear(child, config);
                            break;
                        default:
                            throw ADP.ConfigUnrecognizedElement(child);
                        }
                    }
                    config.AcceptChanges();
                }
                return config;
            }
            static private void HandleAdd(XmlNode child, DataTable config) {
                HandlerBase.CheckForChildNodes(child);
                DataRow values = config.NewRow();
                values[0] = HandlerBase.RemoveAttribute(child, "name", true, false);
                values[1] = HandlerBase.RemoveAttribute(child, "description", true, false);
                values[2] = HandlerBase.RemoveAttribute(child, "invariant", true, false);
                values[3] = HandlerBase.RemoveAttribute(child, "type", true, false);

                // because beta shipped recognizing "support=hex#", need to give
                // more time for other providers to remove it from the .config files
                HandlerBase.RemoveAttribute(child, "support", false, false);
                
                HandlerBase.CheckForUnrecognizedAttributes(child);
                config.Rows.Add(values);
            }
            static private void HandleRemove(XmlNode child, DataTable config) {
                HandlerBase.CheckForChildNodes(child);                
                String invr = HandlerBase.RemoveAttribute(child, "invariant", true, false);                
                HandlerBase.CheckForUnrecognizedAttributes(child);
                DataRow row = config.Rows.Find(invr);
                if (null != row) { // ignore invariants that don't exist
                    row.Delete();
                }
            }
            static private void HandleClear(XmlNode child, DataTable config) {
                HandlerBase.CheckForChildNodes(child);
                HandlerBase.CheckForUnrecognizedAttributes(child);
                config.Clear();
            }
        }
        
        internal static DataTable CreateProviderDataTable() {
            DataColumn frme = new DataColumn("Name", typeof(string));
            frme.ReadOnly = true;
            DataColumn desc = new DataColumn("Description", typeof(string));
            desc.ReadOnly = true;
            DataColumn invr = new DataColumn("InvariantName", typeof(string));
            invr.ReadOnly = true;
            DataColumn qual = new DataColumn("AssemblyQualifiedName", typeof(string));
            qual.ReadOnly = true;

            DataColumn[] primaryKey = new DataColumn[] { invr };
            DataColumn[] columns = new DataColumn[] {frme, desc, invr, qual };
            DataTable table = new DataTable(DbProviderFactoriesConfigurationHandler.providerGroup);
            table.Locale = CultureInfo.InvariantCulture;
            table.Columns.AddRange(columns);
            table.PrimaryKey = primaryKey;
            return table;
        }
    }
}

