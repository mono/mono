//------------------------------------------------------------------------------
// <copyright file="EntityProviderServices.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//------------------------------------------------------------------------------

namespace System.Data.EntityClient {

    using System.Data.Common;
    using System.Data.Common.CommandTrees;
    using System.Data.Entity;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// The class for provider services of the entity client
    /// </summary>
    internal sealed class EntityProviderServices : DbProviderServices {

        /// <summary>
        /// Singleton object;
        /// </summary>
        internal static readonly EntityProviderServices Instance = new EntityProviderServices();

        /// <summary>
        /// Create a Command Definition object, given the connection and command tree
        /// </summary>
        /// <param name="connection">connection to the underlying provider</param>
        /// <param name="commandTree">command tree for the statement</param>
        /// <returns>an exectable command definition object</returns>
        /// <exception cref="ArgumentNullException">connection and commandTree arguments must not be null</exception>
        protected override DbCommandDefinition CreateDbCommandDefinition(DbProviderManifest providerManifest, DbCommandTree commandTree) {
            EntityUtil.CheckArgumentNull(providerManifest, "providerManifest");
            EntityUtil.CheckArgumentNull(commandTree, "commandTree");

            StoreItemCollection storeMetadata = (StoreItemCollection)commandTree.MetadataWorkspace.GetItemCollection(DataSpace.SSpace);
            return this.CreateCommandDefinition(storeMetadata.StoreProviderFactory, commandTree);
        }

        internal EntityCommandDefinition CreateCommandDefinition(DbProviderFactory storeProviderFactory, DbCommandTree commandTree) {
            EntityUtil.CheckArgumentNull(storeProviderFactory, "storeProviderFactory");
            Debug.Assert(commandTree != null, "Command Tree cannot be null");

            return new EntityCommandDefinition(storeProviderFactory, commandTree);
        }

        /// <summary>
        /// Ensures that the data space of the specified command tree is the model (C-) space
        /// </summary>
        /// <param name="commandTree">The command tree for which the data space should be validated</param>
        internal override void ValidateDataSpace(DbCommandTree commandTree)
        {
            Debug.Assert(commandTree != null, "Ensure command tree is non-null before calling ValidateDataSpace");

            if (commandTree.DataSpace != DataSpace.CSpace)
            {
                throw EntityUtil.ProviderIncompatible(Strings.EntityClient_RequiresNonStoreCommandTree);
            }
        }
        
        /// <summary>
        /// Create a EntityCommandDefinition object based on the prototype command
        /// This method is intended for provider writers to build a default command definition
        /// from a command. 
        /// </summary>
        /// <param name="prototype"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">prototype argument must not be null</exception>
        /// <exception cref="InvalidCastException">prototype argument must be a EntityCommand</exception>
        public override DbCommandDefinition CreateCommandDefinition(DbCommand prototype) {
            EntityUtil.CheckArgumentNull(prototype, "prototype");
            return ((EntityCommand)prototype).GetCommandDefinition();
        }

        protected override string GetDbProviderManifestToken(DbConnection connection)
        {
            EntityUtil.CheckArgumentNull(connection, "connection");
            if (connection.GetType() != typeof(EntityConnection))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Mapping_Provider_WrongConnectionType(typeof(EntityConnection)));
            }

            return MetadataItem.EdmProviderManifest.Token;
        }

        protected override DbProviderManifest GetDbProviderManifest(string versionHint)
        {
            EntityUtil.CheckArgumentNull(versionHint, "versionHint");
            return MetadataItem.EdmProviderManifest;
        }
    }
}
