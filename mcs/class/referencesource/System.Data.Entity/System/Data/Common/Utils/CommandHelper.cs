//---------------------------------------------------------------------
// <copyright file="CommandHelper.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Common.Utils
{
    using System.Data.EntityClient;
    using System.Data.Metadata.Edm;
    using System.Data.Spatial;
    using System.Diagnostics;

    /// <summary>
    /// Contains utility methods for construction of DB commands through generic
    /// provider interfaces.
    /// </summary>
    internal static class CommandHelper
    {
        /// <summary>
        /// Consumes all rows and result sets from the reader. This allows client to retrieve
        /// parameter values and intercept any store exceptions.
        /// </summary>
        /// <param name="reader">reader to consume</param>
        internal static void ConsumeReader(DbDataReader reader)
        {
            if (null != reader && !reader.IsClosed)
            {
                while (reader.NextResult())
                {
                    // Note that we only walk through the result sets. We don't need
                    // to walk through individual rows (though underlying provider
                    // implementation may do so)
                }
            }
        }

        /// <summary>
        /// requires: commandText must not be null
        /// The command text must be in the form Container.FunctionImportName.
        /// </summary>
        internal static void ParseFunctionImportCommandText(string commandText, string defaultContainerName, out string containerName, out string functionImportName)
        {
            Debug.Assert(null != commandText);

            // Split the string
            string[] nameParts = commandText.Split('.');
            containerName = null;
            functionImportName = null;
            if (2 == nameParts.Length)
            {
                containerName = nameParts[0].Trim();
                functionImportName = nameParts[1].Trim();
            }
            else if (1 == nameParts.Length && null != defaultContainerName)
            {
                containerName = defaultContainerName;
                functionImportName = nameParts[0].Trim();
            }
            if (string.IsNullOrEmpty(containerName) || string.IsNullOrEmpty(functionImportName))
            {
                throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.EntityClient_InvalidStoredProcedureCommandText);
            }
        }

        /// <summary>
        /// Given an entity command, returns the associated entity transaction and performs validation
        /// to ensure the transaction is consistent.
        /// </summary>
        /// <param name="entityCommand">Entity command instance. Must not be null.</param>
        /// <returns>Entity transaction</returns>
        internal static EntityTransaction GetEntityTransaction(EntityCommand entityCommand)
        {
            Debug.Assert(null != entityCommand);
            EntityTransaction entityTransaction = (EntityTransaction)entityCommand.Transaction;

            // Check to make sure that either the command has no transaction associated with it, or it
            // matches the one used by the connection
            if (entityTransaction != null && entityTransaction != entityCommand.Connection.CurrentTransaction)
            {
                throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.EntityClient_InvalidTransactionForCommand);
            }
            // Now we have asserted that EntityCommand either has no transaction or has one that matches the
            // one used in the connection, we can simply use the connection's transaction object
            entityTransaction = entityCommand.Connection.CurrentTransaction;
            return entityTransaction;
        }


        /// <summary>
        /// Given an entity command and entity transaction, passes through relevant state to store provider
        /// command.
        /// </summary>
        /// <param name="entityCommand">Entity command. Must not be null.</param>
        /// <param name="entityTransaction">Entity transaction. Must not be null.</param>
        /// <param name="storeProviderCommand">Store provider command that is being setup. Must not be null.</param>
        internal static void SetStoreProviderCommandState(EntityCommand entityCommand, EntityTransaction entityTransaction, DbCommand storeProviderCommand)
        {
            Debug.Assert(null != entityCommand);
            Debug.Assert(null != storeProviderCommand);

            storeProviderCommand.CommandTimeout = entityCommand.CommandTimeout;
            storeProviderCommand.Connection = ((EntityConnection)entityCommand.Connection).StoreConnection;
            storeProviderCommand.Transaction = (null != entityTransaction) ? entityTransaction.StoreTransaction : null;
            storeProviderCommand.UpdatedRowSource = entityCommand.UpdatedRowSource;
        }


        /// <summary>
        /// Given an entity command, store provider command and a connection, sets all output parameter values on the entity command.
        /// The connection is used to determine how to map spatial values.
        /// </summary>
        /// <param name="entityCommand">Entity command on which to set parameter values. Must not be null.</param>
        /// <param name="storeProviderCommand">Store provider command from which to retrieve parameter values. Must not
        /// be null.</param>
        /// <param name="connection">The connection on which the command was run.  Must not be null</param>
        internal static void SetEntityParameterValues(EntityCommand entityCommand, DbCommand storeProviderCommand, EntityConnection connection)
        {
            Debug.Assert(null != entityCommand);
            Debug.Assert(null != storeProviderCommand);
            Debug.Assert(null != connection);

            foreach (DbParameter storeParameter in storeProviderCommand.Parameters)
            {
                ParameterDirection direction = storeParameter.Direction;
                if (0 != (direction & ParameterDirection.Output))
                {
                    // if the entity command also defines the parameter, propagate store parameter value
                    // to entity parameter
                    int parameterOrdinal = entityCommand.Parameters.IndexOf(storeParameter.ParameterName);
                    if (0 <= parameterOrdinal)
                    {
                        EntityParameter entityParameter = entityCommand.Parameters[parameterOrdinal];
                        object parameterValue = storeParameter.Value;
                        TypeUsage parameterType = entityParameter.GetTypeUsage();
                        if (Helper.IsSpatialType(parameterType))
                        {
                            parameterValue = GetSpatialValueFromProviderValue(parameterValue, (PrimitiveType)parameterType.EdmType, connection);
                        }
                        entityParameter.Value = parameterValue;
                    }
                }
            }
        }

        private static object GetSpatialValueFromProviderValue(object spatialValue, PrimitiveType parameterType, EntityConnection connection)
        {
            DbProviderServices providerServices = DbProviderServices.GetProviderServices(connection.StoreConnection);
            StoreItemCollection storeItemCollection = (StoreItemCollection)connection.GetMetadataWorkspace().GetItemCollection(DataSpace.SSpace);
            DbSpatialServices spatialServices = providerServices.GetSpatialServices(storeItemCollection.StoreProviderManifestToken);
            if (Helper.IsGeographicType(parameterType))
            {
                return spatialServices.GeographyFromProviderValue(spatialValue);
            }
            else
            {
                Debug.Assert(Helper.IsGeometricType(parameterType));
                return spatialServices.GeometryFromProviderValue(spatialValue);
            }
        }

        // requires: all arguments must be given
        internal static EdmFunction FindFunctionImport(MetadataWorkspace workspace, string containerName, string functionImportName)
        {
            Debug.Assert(null != workspace && null != containerName && null != functionImportName);
            // find entity container
            EntityContainer entityContainer;
            if (!workspace.TryGetEntityContainer(containerName, DataSpace.CSpace, out entityContainer))
            {
                throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.EntityClient_UnableToFindFunctionImportContainer(
                    containerName));
            }

            // find function import
            EdmFunction functionImport = null;
            foreach (EdmFunction candidate in entityContainer.FunctionImports)
            {
                if (candidate.Name == functionImportName)
                {
                    functionImport = candidate;
                    break;
                }
            }
            if (null == functionImport)
            {
                throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.EntityClient_UnableToFindFunctionImport(
                    containerName, functionImportName));
            }
            if (functionImport.IsComposableAttribute)
            {
                throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.EntityClient_FunctionImportMustBeNonComposable(containerName + "." + functionImportName));
            }
            return functionImport;
        }
    }
}
