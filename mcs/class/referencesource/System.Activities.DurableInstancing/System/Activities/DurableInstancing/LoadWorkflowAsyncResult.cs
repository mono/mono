//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.DurableInstancing;
    using System.Transactions;
    using System.Xml.Linq;

    class LoadWorkflowAsyncResult : SqlWorkflowInstanceStoreAsyncResult
    {
        static readonly string commandText = string.Format(CultureInfo.InvariantCulture, "{0}.[LoadInstance]", SqlWorkflowInstanceStoreConstants.DefaultSchema);
        Dictionary<Guid, IDictionary<XName, InstanceValue>> associatedInstanceKeys;
        Dictionary<Guid, IDictionary<XName, InstanceValue>> completedInstanceKeys;

        Dictionary<XName, InstanceValue> instanceData;
        Dictionary<XName, InstanceValue> instanceMetadata;
        IObjectSerializer objectSerializer;

        public LoadWorkflowAsyncResult
            (
            InstancePersistenceContext context, 
            InstancePersistenceCommand command, 
            SqlWorkflowInstanceStore store,
            SqlWorkflowInstanceStoreLock storeLock,
            Transaction currentTransaction,
            TimeSpan timeout, 
            AsyncCallback callback, 
            object state
            ) :
            base(context, command, store, storeLock, currentTransaction, timeout, callback, state)
        {
            this.associatedInstanceKeys = new Dictionary<Guid, IDictionary<XName, InstanceValue>>();
            this.completedInstanceKeys = new Dictionary<Guid, IDictionary<XName, InstanceValue>>();
            this.objectSerializer = ObjectSerializerFactory.GetDefaultObjectSerializer();
        }

        protected void GenerateLoadSqlCommand
            (
            SqlCommand command, 
            LoadType loadType, 
            Guid keyToLoadBy, 
            Guid instanceId, 
            List<CorrelationKey> keysToAssociate
            )
        {
            long surrogateLockOwnerId = base.StoreLock.SurrogateLockOwnerId;
            byte[] concatenatedKeyProperties = null;
            bool singleKeyToAssociate = (keysToAssociate != null && keysToAssociate.Count == 1);

            if (keysToAssociate != null)
            {
                concatenatedKeyProperties = SerializationUtilities.CreateKeyBinaryBlob(keysToAssociate);
            }

            double operationTimeout = this.TimeoutHelper.RemainingTime().TotalMilliseconds;

            SqlParameterCollection parameters = command.Parameters;
            parameters.Add(new SqlParameter { ParameterName = "@surrogateLockOwnerId", SqlDbType = SqlDbType.BigInt, Value = surrogateLockOwnerId });
            parameters.Add(new SqlParameter { ParameterName = "@operationType", SqlDbType = SqlDbType.TinyInt, Value = loadType });
            parameters.Add(new SqlParameter { ParameterName = "@keyToLoadBy", SqlDbType = SqlDbType.UniqueIdentifier, Value = keyToLoadBy });
            parameters.Add(new SqlParameter { ParameterName = "@instanceId", SqlDbType = SqlDbType.UniqueIdentifier, Value = instanceId });
            parameters.Add(new SqlParameter { ParameterName = "@handleInstanceVersion", SqlDbType = SqlDbType.BigInt, Value = base.InstancePersistenceContext.InstanceVersion });
            parameters.Add(new SqlParameter { ParameterName = "@handleIsBoundToLock", SqlDbType = SqlDbType.Bit, Value = base.InstancePersistenceContext.InstanceView.IsBoundToLock });
            parameters.Add(new SqlParameter { ParameterName = "@keysToAssociate", SqlDbType = SqlDbType.Xml, Value = singleKeyToAssociate ? DBNull.Value : SerializationUtilities.CreateCorrelationKeyXmlBlob(keysToAssociate) });
            parameters.Add(new SqlParameter { ParameterName = "@encodingOption", SqlDbType = SqlDbType.TinyInt, Value = base.Store.InstanceEncodingOption });
            parameters.Add(new SqlParameter { ParameterName = "@concatenatedKeyProperties", SqlDbType = SqlDbType.VarBinary, Value = (object) concatenatedKeyProperties ?? DBNull.Value });
            parameters.Add(new SqlParameter { ParameterName = "@operationTimeout", SqlDbType = SqlDbType.Int, Value = (operationTimeout < Int32.MaxValue) ? Convert.ToInt32(operationTimeout) : Int32.MaxValue });
            parameters.Add(new SqlParameter { ParameterName = "@singleKeyId", SqlDbType = SqlDbType.UniqueIdentifier, Value = singleKeyToAssociate ? keysToAssociate[0].KeyId : (object) DBNull.Value });
        }

        protected override void GenerateSqlCommand(SqlCommand command)
        {
            LoadWorkflowCommand loadWorkflowCommand = base.InstancePersistenceCommand as LoadWorkflowCommand;
            LoadType loadType = loadWorkflowCommand.AcceptUninitializedInstance ? LoadType.LoadOrCreateByInstance : LoadType.LoadByInstance;
            Guid instanceId = base.InstancePersistenceContext.InstanceView.InstanceId;

            GenerateLoadSqlCommand(command, loadType, Guid.Empty, instanceId, null);
        }

        protected override string GetSqlCommandText()
        {
            return LoadWorkflowAsyncResult.commandText;
        }

        protected override CommandType GetSqlCommandType()
        {
            return CommandType.StoredProcedure;
        }

        protected override Exception ProcessSqlResult(SqlDataReader reader)
        {
            Exception exception = StoreUtilities.GetNextResultSet(base.InstancePersistenceCommand.Name, reader);

            if (exception == null)
            {
                Guid instanceId = reader.GetGuid(1);
                long surrogateInstanceId = reader.GetInt64(2);
                byte[] primitiveProperties = reader.IsDBNull(3) ? null : (byte[])(reader.GetValue(3));
                byte[] complexProperties = reader.IsDBNull(4) ? null : (byte[])(reader.GetValue(4));
                byte[] metadataProperties = reader.IsDBNull(5) ? null : (byte[])(reader.GetValue(5));
                InstanceEncodingOption dataEncodingOption = (InstanceEncodingOption)(reader.GetByte(6));
                InstanceEncodingOption metadataEncodingOption = (InstanceEncodingOption)(reader.GetByte(7));
                long version = reader.GetInt64(8);
                bool isInitialized = reader.GetBoolean(9);
                bool createdInstance = reader.GetBoolean(10);

                LoadWorkflowCommand loadWorkflowCommand = base.InstancePersistenceCommand as LoadWorkflowCommand;
                LoadWorkflowByInstanceKeyCommand loadByKeycommand = base.InstancePersistenceCommand as LoadWorkflowByInstanceKeyCommand;

                if (!base.InstancePersistenceContext.InstanceView.IsBoundToInstance)
                {
                    base.InstancePersistenceContext.BindInstance(instanceId);
                }
                if (!base.InstancePersistenceContext.InstanceView.IsBoundToInstanceOwner)
                {
                    base.InstancePersistenceContext.BindInstanceOwner(base.StoreLock.LockOwnerId, base.StoreLock.LockOwnerId);
                }
                if (!base.InstancePersistenceContext.InstanceView.IsBoundToLock)
                {
                    InstanceLockTracking instanceLockTracking = (InstanceLockTracking)(base.InstancePersistenceContext.UserContext);
                    instanceLockTracking.TrackStoreLock(instanceId, version, this.DependentTransaction);
                    base.InstancePersistenceContext.BindAcquiredLock(version);
                }

                this.instanceData = SerializationUtilities.DeserializePropertyBag(primitiveProperties, complexProperties, dataEncodingOption);
                this.instanceMetadata = SerializationUtilities.DeserializeMetadataPropertyBag(metadataProperties, metadataEncodingOption);

                if (!createdInstance)
                {
                    ReadInstanceMetadataChanges(reader, this.instanceMetadata);
                    ReadKeyData(reader, this.associatedInstanceKeys, this.completedInstanceKeys);
                }
                else if (loadByKeycommand != null)
                {
                    foreach (KeyValuePair<Guid, IDictionary<XName, InstanceValue>> keyEntry in loadByKeycommand.InstanceKeysToAssociate)
                    {
                        this.associatedInstanceKeys.Add(keyEntry.Key, keyEntry.Value);
                    }

                    if (!this.associatedInstanceKeys.ContainsKey(loadByKeycommand.LookupInstanceKey))
                    {
                        base.InstancePersistenceContext.AssociatedInstanceKey(loadByKeycommand.LookupInstanceKey);
                        this.associatedInstanceKeys.Add(loadByKeycommand.LookupInstanceKey, new Dictionary<XName, InstanceValue>());
                    }
                }

                if (loadByKeycommand != null)
                {
                    foreach (KeyValuePair<Guid, IDictionary<XName, InstanceValue>> keyEntry in loadByKeycommand.InstanceKeysToAssociate)
                    {
                        base.InstancePersistenceContext.AssociatedInstanceKey(keyEntry.Key);

                        if (keyEntry.Value != null)
                        {
                            foreach (KeyValuePair<XName, InstanceValue> property in keyEntry.Value)
                            {
                                base.InstancePersistenceContext.WroteInstanceKeyMetadataValue(keyEntry.Key, property.Key, property.Value);
                            }
                        }
                    }
                }

                base.InstancePersistenceContext.LoadedInstance
                    (
                    isInitialized ? InstanceState.Initialized : InstanceState.Uninitialized,
                    this.instanceData,
                    this.instanceMetadata,
                    this.associatedInstanceKeys,
                    this.completedInstanceKeys
                    );
            }
            else if (exception is InstanceLockLostException)
            {
                base.InstancePersistenceContext.InstanceHandle.Free();
            }

            return exception;
        }

        void ReadInstanceMetadataChanges(SqlDataReader reader, Dictionary<XName, InstanceValue> instanceMetadata)
        {
            Exception exception = StoreUtilities.GetNextResultSet(base.InstancePersistenceCommand.Name, reader);

            if (exception == null)
            {
                if (reader.IsDBNull(1))
                {
                    return;
                }
            }

            do
            {
                InstanceEncodingOption encodingOption = (InstanceEncodingOption) reader.GetByte(1);
                byte[] serializedMetadataChanges = (byte[]) reader.GetValue(2);

                Dictionary<XName, InstanceValue> metadataChangeSet = SerializationUtilities.DeserializeMetadataPropertyBag(serializedMetadataChanges, encodingOption);

                foreach (KeyValuePair<XName, InstanceValue> metadataChange in metadataChangeSet)
                {
                    XName xname = metadataChange.Key;
                    InstanceValue propertyValue = metadataChange.Value;

                    if (propertyValue.Value is DeletedMetadataValue)
                    {
                        instanceMetadata.Remove(xname);
                    }
                    else
                    {
                        instanceMetadata[xname] = propertyValue;
                    }
                }
            } 
            while (reader.Read());
        }

        void ReadKeyData(SqlDataReader reader, Dictionary<Guid, IDictionary<XName, InstanceValue>> associatedInstanceKeys,
            Dictionary<Guid, IDictionary<XName, InstanceValue>> completedInstanceKeys)
        {
            Exception exception = StoreUtilities.GetNextResultSet(base.InstancePersistenceCommand.Name, reader);

            if (exception == null)
            {
                if (reader.IsDBNull(1))
                {
                    return;
                }

                do
                {
                    Guid key = reader.GetGuid(1);
                    bool isAssociated = reader.GetBoolean(2);
                    InstanceEncodingOption encodingOption = (InstanceEncodingOption) reader.GetByte(3);
                    Dictionary<Guid, IDictionary<XName, InstanceValue>> destination = isAssociated ? associatedInstanceKeys : completedInstanceKeys;

                    if (!reader.IsDBNull(4))
                    {
                        destination[key] = SerializationUtilities.DeserializeKeyMetadata((byte[]) reader.GetValue(4), encodingOption);
                    }
                    else
                    {
                        destination[key] = new Dictionary<XName, InstanceValue>();
                    }

                } 
                while (reader.Read());
            } 
        }
    }
}
