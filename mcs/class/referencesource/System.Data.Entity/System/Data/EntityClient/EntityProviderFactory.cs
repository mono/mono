//---------------------------------------------------------------------
// <copyright file="EntityProviderFactory.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Security;
using System.Security.Permissions;

namespace System.Data.EntityClient
{
    /// <summary>
    /// Class representing a provider factory for the entity client provider
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2302", Justification="We don't expect serviceType to be an Embedded Interop Types.")]
    public sealed class EntityProviderFactory : DbProviderFactory, IServiceProvider
    {
        /// <summary>
        /// A singleton object for the entity client provider factory object
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "EntityProviderFactory implements the singleton pattern and it's stateless.  This is needed in order to work with DbProviderFactories.")]
        public static readonly EntityProviderFactory Instance = new EntityProviderFactory();

        /// <summary>
        /// Constructs the EntityProviderFactory object, this is private as users shouldn't create it directly
        /// </summary>
        private EntityProviderFactory()
        {
        }

        /// <summary>
        /// Creates a EntityCommand object and returns it
        /// </summary>
        /// <returns>A EntityCommand object</returns>
        public override DbCommand CreateCommand()
        {
            return new EntityCommand();
        }

        /// <summary>
        /// Creates a EntityCommandBuilder object and returns it
        /// </summary>
        /// <returns>A EntityCommandBuilder object</returns>
        /// <exception cref="NotSupportedException"></exception>
        public override DbCommandBuilder CreateCommandBuilder()
        {
            throw EntityUtil.NotSupported();
        }

        /// <summary>
        /// Creates a EntityConnection object and returns it
        /// </summary>
        /// <returns>A EntityConnection object</returns>
        public override DbConnection CreateConnection()
        {
            return new EntityConnection();
        }

        /// <summary>
        /// Creates a EntityConnectionStringBuilder object and returns it
        /// </summary>
        /// <returns>A EntityConnectionStringBuilder object</returns>
        public override DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return new EntityConnectionStringBuilder();
        }

        /// <summary>
        /// Creates a DbDataAdapter object and returns it, this method is currently not supported
        /// </summary>
        /// <returns>A DbDataAdapter object</returns>
        /// <exception cref="NotSupportedException"></exception>
        public override DbDataAdapter CreateDataAdapter()
        {
            throw EntityUtil.NotSupported();
        }

        /// <summary>
        /// Creates a EntityParameter object and returns it
        /// </summary>
        /// <returns>A EntityParameter object</returns>
        public override DbParameter CreateParameter()
        {
            return new EntityParameter();
        }

        /// <summary>
        /// Creates a CodeAccessPermission object and returns it
        /// </summary>
        /// <param name="state">The permission state level for the code access</param>
        /// <returns>A CodeAccessPermission object</returns>
        public override CodeAccessPermission CreatePermission(PermissionState state)
        {
            throw EntityUtil.NotSupported();
        }

        /// <summary>
        /// Extension mechanism for additional services;  
        /// </summary>
        /// <returns>requested service provider or null.</returns>
        object IServiceProvider.GetService(Type serviceType) {
            object result = null;
            if (serviceType == typeof(DbProviderServices)) {
                result = EntityProviderServices.Instance;
            }
            else if (serviceType == typeof(IEntityAdapter)) {
                result = new EntityAdapter();
            }
            return result;
        }    
    }
}
