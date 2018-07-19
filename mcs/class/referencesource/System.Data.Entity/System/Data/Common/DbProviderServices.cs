//------------------------------------------------------------------------------
// <copyright file="DbProviderServices.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//------------------------------------------------------------------------------

using System.Data.SqlClient;

namespace System.Data.Common
{
    using System.Data.Common.CommandTrees;
    using System.Data.Entity;
    using System.Data.Metadata.Edm;
    using System.Data.Spatial;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Xml;

    /// <summary>
    /// The factory for building command definitions; use the type of this object
    /// as the argument to the IServiceProvider.GetService method on the provider
    /// factory;
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    [CLSCompliant(false)]
    abstract public class DbProviderServices
    {
        /// <summary>
        /// Create a Command Definition object given a command tree.
        /// </summary>
        /// <param name="commandTree">command tree for the statement</param>
        /// <returns>an exectable command definition object</returns>
        /// <remarks>
        /// This method simply delegates to the provider's implementation of CreateDbCommandDefinition.
        /// </remarks>
        public DbCommandDefinition CreateCommandDefinition(DbCommandTree commandTree)
        {
            EntityUtil.CheckArgumentNull(commandTree, "commandTree");
            ValidateDataSpace(commandTree);
            StoreItemCollection storeMetadata = (StoreItemCollection)commandTree.MetadataWorkspace.GetItemCollection(DataSpace.SSpace);
            Debug.Assert(storeMetadata.StoreProviderManifest != null, "StoreItemCollection has null StoreProviderManifest?");
            
            return CreateDbCommandDefinition(storeMetadata.StoreProviderManifest, commandTree);
        }

        /// <summary>
        /// Create a Command Definition object given a command tree.
        /// </summary>
        /// <param name="commandTree">command tree for the statement</param>
        /// <returns>an exectable command definition object</returns>
        /// <remarks>
        /// This method simply delegates to the provider's implementation of CreateDbCommandDefinition.
        /// </remarks>
        public DbCommandDefinition CreateCommandDefinition(DbProviderManifest providerManifest, DbCommandTree commandTree)
        {
            try
            {
                return CreateDbCommandDefinition(providerManifest, commandTree);
            }
            catch (ProviderIncompatibleException)
            {
                throw;
            }
            catch (Exception e)
            {
                if (EntityUtil.IsCatchableExceptionType(e))
                {
                    throw EntityUtil.ProviderIncompatible(Strings.ProviderDidNotCreateACommandDefinition, e);
                }
                throw;
            }
        }

        /// <summary>
        /// Create a Command Definition object, given the provider manifest and command tree
        /// </summary>
        /// <param name="connection">provider manifest previously retrieved from the store provider</param>
        /// <param name="commandTree">command tree for the statement</param>
        /// <returns>an exectable command definition object</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
        protected abstract DbCommandDefinition CreateDbCommandDefinition(DbProviderManifest providerManifest, DbCommandTree commandTree);

        /// <summary>
        /// Ensures that the data space of the specified command tree is the target (S-) space
        /// </summary>
        /// <param name="commandTree">The command tree for which the data space should be validated</param>
        internal virtual void ValidateDataSpace(DbCommandTree commandTree)
        {
            Debug.Assert(commandTree != null, "Ensure command tree is non-null before calling ValidateDataSpace");

            if (commandTree.DataSpace != DataSpace.SSpace)
            {
                throw EntityUtil.ProviderIncompatible(Strings.ProviderRequiresStoreCommandTree);
            }
        }

        /// <summary>
        /// Create a DbCommand object given a command tree.
        /// </summary>
        /// <param name="commandTree">command tree for the statement</param>
        /// <returns>a command object</returns>
        internal virtual DbCommand CreateCommand(DbCommandTree commandTree) {
            DbCommandDefinition commandDefinition = CreateCommandDefinition(commandTree);
            DbCommand command = commandDefinition.CreateCommand();
            return command;
        }

        /// <summary>
        /// Create the default DbCommandDefinition object based on the prototype command
        /// This method is intended for provider writers to build a default command definition
        /// from a command. 
        /// Note: This will clone the prototype
        /// </summary>
        /// <param name="prototype">the prototype command</param>
        /// <returns>an executable command definition object</returns>
        public virtual DbCommandDefinition CreateCommandDefinition(DbCommand prototype) {
            return DbCommandDefinition.CreateCommandDefinition(prototype);
        }
                
        /// <summary>
        /// Retrieve the provider manifest token based on the specified connection.
        /// </summary>
        /// <param name="connection">The connection for which the provider manifest token should be retrieved.</param>
        /// <returns>
        /// The provider manifest token that describes the specified connection, as determined by the provider.
        /// </returns>
        /// <remarks>
        /// This method simply delegates to the provider's implementation of GetDbProviderManifestToken.
        /// </remarks>
        public string GetProviderManifestToken(DbConnection connection) {
            try
            {
                string providerManifestToken = GetDbProviderManifestToken(connection);
                if (providerManifestToken == null)
                {
                    throw EntityUtil.ProviderIncompatible(Strings.ProviderDidNotReturnAProviderManifestToken);
                }
                return providerManifestToken;
            }
            catch (ProviderIncompatibleException)
            {
                throw;
            }
            catch (Exception e)
            {
                if (EntityUtil.IsCatchableExceptionType(e))
                {
                    throw EntityUtil.ProviderIncompatible(Strings.ProviderDidNotReturnAProviderManifestToken, e);
                }
                throw;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
        protected abstract string GetDbProviderManifestToken(DbConnection connection);

        public DbProviderManifest GetProviderManifest(string manifestToken) {
            try
            {
                DbProviderManifest providerManifest = GetDbProviderManifest(manifestToken);
                if (providerManifest == null)
                {
                    throw EntityUtil.ProviderIncompatible(Strings.ProviderDidNotReturnAProviderManifest);
                }

                return providerManifest;
            }
            catch (ProviderIncompatibleException)
            {
                throw;
            }
            catch (Exception e)
            {
                if (EntityUtil.IsCatchableExceptionType(e)) {
                    throw EntityUtil.ProviderIncompatible(System.Data.Entity.Strings.ProviderDidNotReturnAProviderManifest, e);
                }
                throw;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
        protected abstract DbProviderManifest GetDbProviderManifest(string manifestToken);

        public DbSpatialDataReader GetSpatialDataReader(DbDataReader fromReader, string manifestToken)
        {
            try
            {
                DbSpatialDataReader spatialReader = GetDbSpatialDataReader(fromReader, manifestToken);
                if (spatialReader == null)
                {
                    throw EntityUtil.ProviderIncompatible(Strings.ProviderDidNotReturnSpatialServices);
                }

                return spatialReader;
            }
            catch (ProviderIncompatibleException)
            {
                throw;
            }
            catch (Exception e)
            {
                if (EntityUtil.IsCatchableExceptionType(e))
                {
                    throw EntityUtil.ProviderIncompatible(System.Data.Entity.Strings.ProviderDidNotReturnSpatialServices, e);
                }
                throw;
            }
        }

        public DbSpatialServices GetSpatialServices(string manifestToken)
        {
            try
            {
                DbSpatialServices spatialServices = DbGetSpatialServices(manifestToken);
                if (spatialServices == null)
                {
                    throw EntityUtil.ProviderIncompatible(Strings.ProviderDidNotReturnSpatialServices);
                }

                return spatialServices;
            }
            catch (ProviderIncompatibleException)
            {
                throw;
            }
            catch (Exception e)
            {
                if (EntityUtil.IsCatchableExceptionType(e))
                {
                    throw EntityUtil.ProviderIncompatible(System.Data.Entity.Strings.ProviderDidNotReturnSpatialServices, e);
                }
                throw;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
        protected virtual DbSpatialDataReader GetDbSpatialDataReader(DbDataReader fromReader, string manifestToken)
        {
            // Must be a virtual method; abstract would break previous implementors of DbProviderServices
            throw EntityUtil.ProviderIncompatible(Strings.ProviderDidNotReturnSpatialServices);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
        protected virtual DbSpatialServices DbGetSpatialServices(string manifestToken)
        {
            // Must be a virtual method; abstract would break previous implementors of DbProviderServices
            throw EntityUtil.ProviderIncompatible(Strings.ProviderDidNotReturnSpatialServices);
        }

        internal void SetParameterValue(DbParameter parameter, TypeUsage parameterType, object value)
        {
            Debug.Assert(parameter != null, "Validate parameter before calling SetParameterValue");
            Debug.Assert(parameterType != null, "Validate parameterType before calling SetParameterValue");

            this.SetDbParameterValue(parameter, parameterType, value);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
        protected virtual void SetDbParameterValue(DbParameter parameter, TypeUsage parameterType, object value)
        {
            EntityUtil.CheckArgumentNull(parameter, "parameter");
            EntityUtil.CheckArgumentNull(parameterType, "parameterType");

            parameter.Value = value;
        }

        /// <summary>
        /// Create an instance of DbProviderServices based on the supplied DbConnection
        /// </summary>
        /// <param name="connection">The DbConnection to use</param>
        /// <returns>An instance of DbProviderServices</returns>
        public static DbProviderServices GetProviderServices(DbConnection connection) {
            return GetProviderServices(GetProviderFactory(connection));
        }

        internal static DbProviderFactory GetProviderFactory(string providerInvariantName)
        {
            EntityUtil.CheckArgumentNull(providerInvariantName, "providerInvariantName");
            DbProviderFactory factory;
            try
            {
                factory = DbProviderFactories.GetFactory(providerInvariantName);
            }
            catch (ArgumentException e)
            {
                throw EntityUtil.Argument(Strings.EntityClient_InvalidStoreProvider, e);
            }
            return factory;
        }

        /// <summary>
        /// Retrieve the DbProviderFactory based on the specified DbConnection
        /// </summary>
        /// <param name="connection">The DbConnection to use</param>
        /// <returns>An instance of DbProviderFactory</returns>
        public static DbProviderFactory GetProviderFactory(DbConnection connection)
        {
            EntityUtil.CheckArgumentNull(connection, "connection");
            DbProviderFactory factory = DbProviderFactories.GetFactory(connection);
            if (factory == null)
            {
                throw EntityUtil.ProviderIncompatible(System.Data.Entity.Strings.EntityClient_ReturnedNullOnProviderMethod(
                        "get_ProviderFactory",
                        connection.GetType().ToString()));
            }
            Debug.Assert(factory != null, "Should have thrown on null"); 
            return factory;
        }

        internal static DbProviderServices GetProviderServices(DbProviderFactory factory) {
            EntityUtil.CheckArgumentNull(factory, "factory");

            // Special case SQL client so that it will work with System.Data from .NET 4.0 even without
            // a binding redirect.
            if (factory is SqlClientFactory)
            {
                return SqlProviderServices.Instance;
            }

            IServiceProvider serviceProvider = factory as IServiceProvider;
            if (serviceProvider == null) {
                throw EntityUtil.ProviderIncompatible(System.Data.Entity.Strings.EntityClient_DoesNotImplementIServiceProvider(
                    factory.GetType().ToString()));
            }

            DbProviderServices providerServices = serviceProvider.GetService(typeof(DbProviderServices)) as DbProviderServices;
            if (providerServices == null) {
                throw EntityUtil.ProviderIncompatible(
                    System.Data.Entity.Strings.EntityClient_ReturnedNullOnProviderMethod(
                        "GetService",
                        factory.GetType().ToString()));
            }

            return providerServices;
        }

        /// <summary>
        /// Return an XML reader which represents the CSDL description
        /// </summary>
        /// <returns>An XmlReader that represents the CSDL description</returns>
        internal static XmlReader GetConceptualSchemaDefinition(string csdlName) {
            return DbProviderServices.GetXmlResource("System.Data.Resources.DbProviderServices." + csdlName + ".csdl");
        }

        internal static XmlReader GetXmlResource(string resourceName) {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            Stream stream = executingAssembly.GetManifestResourceStream(resourceName);
            return XmlReader.Create(stream, null, resourceName);
        }

        /// <summary>
        /// Generates a DDL script which creates schema objects (tables, primary keys, foreign keys) 
        /// based on the contents of the storeItemCollection and targeted for the version of the backend corresponding to 
        /// the providerManifestToken.
        /// Individual statements should be separated using database-specific DDL command separator. 
        /// It is expected that the generated script would be executed in the context of existing database with 
        /// sufficient permissions, and it should not include commands to create the database, but it may include 
        /// commands to create schemas and other auxiliary objects such as sequences, etc.
        /// </summary>
        /// <param name="providerManifestToken">The provider manifest token identifying the target version</param>
        /// <param name="storeItemCollection">The collection of all store items based on which the script should be created</param>
        /// <returns>
        /// A DDL script which creates schema objects based on contents of storeItemCollection 
        /// and targeted for the version of the backend corresponding to the providerManifestToken.
        /// </returns>
        public string CreateDatabaseScript(string providerManifestToken, StoreItemCollection storeItemCollection)
        {
           return DbCreateDatabaseScript(providerManifestToken, storeItemCollection);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
        protected virtual string DbCreateDatabaseScript(string providerManifestToken, StoreItemCollection storeItemCollection)
        {
            throw EntityUtil.ProviderIncompatible(Strings.ProviderDoesNotSupportCreateDatabaseScript);
        }

        /// <summary>
        /// Creates a database indicated by connection and creates schema objects 
        /// (tables, primary keys, foreign keys) based on the contents of storeItemCollection. 
        /// </summary>
        /// <param name="connection">Connection to a non-existent database that needs to be created 
        /// and be populated with the store objects indicated by the storeItemCollection</param>
        /// <param name="commandTimeout">Execution timeout for any commands needed to create the database.</param>
        /// <param name="storeItemCollection">The collection of all store items based on which the script should be created<</param>
        public void CreateDatabase(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            DbCreateDatabase(connection, commandTimeout, storeItemCollection);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
        protected virtual void DbCreateDatabase(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            throw EntityUtil.ProviderIncompatible(Strings.ProviderDoesNotSupportCreateDatabase);
        }

        /// <summary>
        /// Returns a value indicating whether given database exists on the server 
        /// and/or whether schema objects contained in teh storeItemCollection have been created.
        /// If the provider can deduct the database only based on the connection, they do not need
        /// to additionally verify all elements of the storeItemCollection.
        /// </summary>
        /// <param name="connection">Connection to a database whose existence is checked by this method</param>
        /// <param name="commandTimeout">Execution timeout for any commands needed to determine the existence of the database</param>
        /// <param name="storeItemCollection">The collection of all store items contained in the database 
        /// whose existence is determined by this method<</param>
        /// <returns>Whether the database indicated by the connection and the storeItemCollection exist</returns>
        public bool DatabaseExists(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            return DbDatabaseExists(connection, commandTimeout, storeItemCollection);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
        protected virtual bool DbDatabaseExists(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            throw EntityUtil.ProviderIncompatible(Strings.ProviderDoesNotSupportDatabaseExists);
        }


        /// <summary>
        /// Deletes all store objects specified in the store item collection from the database and the database itself.
        /// </summary>
        /// <param name="connection">Connection to an existing database that needs to be deleted</param>
        /// <param name="commandTimeout">Execution timeout for any commands needed to delete the database</param>
        /// <param name="storeItemCollection">The collection of all store items contained in the database that should be deleted<</param>
        public void DeleteDatabase(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            DbDeleteDatabase(connection, commandTimeout, storeItemCollection);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
        protected virtual void DbDeleteDatabase(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            throw EntityUtil.ProviderIncompatible(Strings.ProviderDoesNotSupportDeleteDatabase);
        }
    }

}
