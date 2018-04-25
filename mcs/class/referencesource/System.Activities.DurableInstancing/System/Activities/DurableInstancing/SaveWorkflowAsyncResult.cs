//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System.Activities.Hosting;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.Text;
    using System.Threading;
    using System.Transactions;
    using System.Xml;
    using System.Xml.Linq;

    sealed class SaveWorkflowAsyncResult : SqlWorkflowInstanceStoreAsyncResult
    {
        const string createServiceDeploymentStoredProcedureParameters = @"@serviceDeploymentHash, @siteName, @relativeServicePath, @relativeApplicationPath,
                @serviceName, @serviceNamespace, @serviceDeploymentId output";

        const string storedProcedureParameters40 = @"@instanceId, @surrogateLockOwnerId, @handleInstanceVersion, @handleIsBoundToLock,
@primitiveDataProperties, @complexDataProperties, @writeOnlyPrimitiveDataProperties, @writeOnlyComplexDataProperties, @metadataProperties,
@metadataIsConsistent, @encodingOption, @timerDurationMilliseconds, @suspensionStateChange, @suspensionReason, @suspensionExceptionName, @keysToAssociate,
@keysToComplete, @keysToFree, @concatenatedKeyProperties, @unlockInstance, @isReadyToRun, @isCompleted, @singleKeyId,
@lastMachineRunOn, @executionStatus, @blockingBookmarks, @workflowHostType, @serviceDeploymentId, @operationTimeout";

        const string storedProcedureParameters = @"@instanceId, @surrogateLockOwnerId, @handleInstanceVersion, @handleIsBoundToLock,
@primitiveDataProperties, @complexDataProperties, @writeOnlyPrimitiveDataProperties, @writeOnlyComplexDataProperties, @metadataProperties,
@metadataIsConsistent, @encodingOption, @timerDurationMilliseconds, @suspensionStateChange, @suspensionReason, @suspensionExceptionName, @keysToAssociate,
@keysToComplete, @keysToFree, @concatenatedKeyProperties, @unlockInstance, @isReadyToRun, @isCompleted, @singleKeyId,
@lastMachineRunOn, @executionStatus, @blockingBookmarks, @workflowHostType, @serviceDeploymentId, @operationTimeout, @identityMetadata";

        static Dictionary<Guid, long> serviceDeploymentIdsCache = new Dictionary<Guid, long>();
        static ReaderWriterLockSlim serviceDeploymentIdsCacheLock = new ReaderWriterLockSlim();
        string commandText;

        Guid serviceDeploymentHash;
        long serviceDeploymentId;

        public SaveWorkflowAsyncResult
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
            if (((SaveWorkflowCommand)command).InstanceKeyMetadataChanges.Count > 0)
            {
                throw FxTrace.Exception.AsError(new InstancePersistenceCommandException(SR.InstanceKeyMetadataChangesNotSupported));
            }
        }

        protected override void GenerateSqlCommand(SqlCommand command)
        {
            SaveWorkflowCommand saveWorkflowCommand = base.InstancePersistenceCommand as SaveWorkflowCommand;
            StringBuilder commandTextBuilder = new StringBuilder(SqlWorkflowInstanceStoreConstants.DefaultStringBuilderCapacity);
            double operationTimeout = this.TimeoutHelper.RemainingTime().TotalMilliseconds;
            SqlParameterCollection parameters = command.Parameters;
            string suspensionReason;
            string suspensionExceptionName;
                         
            parameters.Add(new SqlParameter { ParameterName = "@instanceId", SqlDbType = SqlDbType.UniqueIdentifier, Value = base.InstancePersistenceContext.InstanceView.InstanceId });
            parameters.Add(new SqlParameter { ParameterName = "@surrogateLockOwnerId", SqlDbType = SqlDbType.BigInt, Value = base.StoreLock.SurrogateLockOwnerId });
            parameters.Add(new SqlParameter { ParameterName = "@handleInstanceVersion", SqlDbType = SqlDbType.BigInt, Value = base.InstancePersistenceContext.InstanceVersion });
            parameters.Add(new SqlParameter { ParameterName = "@handleIsBoundToLock", SqlDbType = SqlDbType.Bit, Value = base.InstancePersistenceContext.InstanceView.IsBoundToLock });
            parameters.Add(new SqlParameter { ParameterName = "@timerDurationMilliseconds", SqlDbType = SqlDbType.BigInt, Value = (object)GetPendingTimerExpiration(saveWorkflowCommand) ?? DBNull.Value });
            parameters.Add(new SqlParameter { ParameterName = "@unlockInstance", SqlDbType = SqlDbType.Bit, Value = saveWorkflowCommand.UnlockInstance });
            parameters.Add(new SqlParameter { ParameterName = "@suspensionStateChange", SqlDbType = SqlDbType.TinyInt, Value = GetSuspensionReason(saveWorkflowCommand, out suspensionReason, out suspensionExceptionName) });
            parameters.Add(new SqlParameter { ParameterName = "@suspensionReason", SqlDbType = SqlDbType.NVarChar, Value = (object)suspensionReason ?? DBNull.Value });
            parameters.Add(new SqlParameter { ParameterName = "@suspensionExceptionName", SqlDbType = SqlDbType.NVarChar, Size = 450, Value = (object)suspensionExceptionName ?? DBNull.Value });
            parameters.Add(new SqlParameter { ParameterName = "@isCompleted", SqlDbType = SqlDbType.Bit, Value = saveWorkflowCommand.CompleteInstance });
            parameters.Add(new SqlParameter { ParameterName = "@isReadyToRun", SqlDbType = SqlDbType.Bit, Value = IsReadyToRun(saveWorkflowCommand) });
            parameters.Add(new SqlParameter { ParameterName = "@workflowHostType", SqlDbType = SqlDbType.UniqueIdentifier, Value = (object)GetWorkflowHostType(saveWorkflowCommand) ?? DBNull.Value });
            parameters.Add(new SqlParameter { ParameterName = "@operationTimeout", SqlDbType = SqlDbType.Int, Value = (operationTimeout < Int32.MaxValue) ? Convert.ToInt32(operationTimeout) : Int32.MaxValue });

            string parameterNames = null;
            if (base.Store.DatabaseVersion >= StoreUtilities.Version45)
            {
                string identityMetadataXml = SerializationUtilities.GetIdentityMetadataXml(saveWorkflowCommand);
                parameters.Add(new SqlParameter { ParameterName = "@identityMetadata", SqlDbType = SqlDbType.Xml, Value = (object)identityMetadataXml ?? DBNull.Value });

                parameterNames = SaveWorkflowAsyncResult.storedProcedureParameters;
            }
            else
            {
                parameterNames = SaveWorkflowAsyncResult.storedProcedureParameters40;
            }

            commandTextBuilder.AppendLine(@"set nocount on
                                            set transaction isolation level read committed		
                                            set xact_abort on
                                            begin transaction");

            ExtractServiceDeploymentInformation(saveWorkflowCommand, commandTextBuilder, parameters);

            commandTextBuilder.AppendLine("declare @result int");
            commandTextBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "exec @result = {0}.[SaveInstance] {1} ;",
                SqlWorkflowInstanceStoreConstants.DefaultSchema, parameterNames));
            commandTextBuilder.AppendLine("if (@result = 0)");
            commandTextBuilder.AppendLine("begin");

            SerializeAssociatedData(parameters, saveWorkflowCommand, commandTextBuilder);

            commandTextBuilder.AppendLine("commit transaction");
            commandTextBuilder.AppendLine("end");
            commandTextBuilder.AppendLine("else");
            commandTextBuilder.AppendLine("rollback transaction");

            this.commandText = commandTextBuilder.ToString();
        }

        protected override string GetSqlCommandText()
        {
            return this.commandText;
        }

        protected override CommandType GetSqlCommandType()
        {
            return CommandType.Text;
        }

        protected override Exception ProcessSqlResult(SqlDataReader reader)
        {
            Exception exception = StoreUtilities.GetNextResultSet(base.InstancePersistenceCommand.Name, reader);

            if (exception == null)
            {
                SaveWorkflowCommand saveWorkflowCommand = base.InstancePersistenceCommand as SaveWorkflowCommand;
                InstanceLockTracking instanceLockTracking = (InstanceLockTracking)(base.InstancePersistenceContext.UserContext);
                if ((this.serviceDeploymentHash != Guid.Empty) && (this.serviceDeploymentId == 0))
                {
                    this.serviceDeploymentId = reader.GetInt64(1);
                    PutServiceDeploymentId();
                    exception = StoreUtilities.GetNextResultSet(base.InstancePersistenceCommand.Name, reader);
                }

                if (exception == null)
                {
                    if (!base.InstancePersistenceContext.InstanceView.IsBoundToLock)
                    {
                        long instanceVersion = reader.GetInt64(1);
                        instanceLockTracking.TrackStoreLock(base.InstancePersistenceContext.InstanceView.InstanceId, instanceVersion, this.DependentTransaction);
                        base.InstancePersistenceContext.BindAcquiredLock(instanceVersion);
                    }

                    if (saveWorkflowCommand.InstanceData.Count > 0)
                    {
                        base.InstancePersistenceContext.PersistedInstance(saveWorkflowCommand.InstanceData);
                    }

                    SaveWorkflowAsyncResult.UpdateKeyData(base.InstancePersistenceContext, saveWorkflowCommand);

                    foreach (KeyValuePair<XName, InstanceValue> property in saveWorkflowCommand.InstanceMetadataChanges)
                    {
                        base.InstancePersistenceContext.WroteInstanceMetadataValue(property.Key, property.Value);
                    }

                    if (saveWorkflowCommand.CompleteInstance)
                    {
                        base.InstancePersistenceContext.CompletedInstance();
                    }

                    if (saveWorkflowCommand.UnlockInstance || saveWorkflowCommand.CompleteInstance)
                    {
                        instanceLockTracking.TrackStoreUnlock(this.DependentTransaction);
                        base.InstancePersistenceContext.InstanceHandle.Free();
                    }
                }
                else if (exception is InstanceLockLostException)
                {
                    base.InstancePersistenceContext.InstanceHandle.Free();
                }
            }

            return exception;
        }

        static void AddSerializedProperty(ArraySegment<byte> source, SqlParameterCollection parameters, string parameterName)
        {
            int parameterSize = source.Count > 8000 ? source.Count : -1;
            object parameterValue = (parameterSize == -1 ? SaveWorkflowAsyncResult.GenerateByteArray(source) : source.Array) ?? (object)DBNull.Value;
            parameters.Add(new SqlParameter { ParameterName = parameterName, SqlDbType = SqlDbType.VarBinary, Size = parameterSize, Value = parameterValue });
        }

        static byte[] GenerateByteArray(ArraySegment<byte> source)
        {
            if (source.Array != null)
            {
                byte[] destination = new byte[source.Count];
                Buffer.BlockCopy(source.Array, 0, destination, 0, source.Count);
                return destination;
            }

            return null;
        }

        static string GetBlockingBookmarks(SaveWorkflowCommand saveWorkflowCommand)
        {
            string blockingBookmarks = null;
            InstanceValue binaryBlockingBookmarks;

            if (saveWorkflowCommand.InstanceData.TryGetValue(SqlWorkflowInstanceStoreConstants.BinaryBlockingBookmarksPropertyName, out binaryBlockingBookmarks))
            {
                StringBuilder bookmarkListBuilder = new StringBuilder(SqlWorkflowInstanceStoreConstants.DefaultStringBuilderCapacity);
                IEnumerable<BookmarkInfo> activeBookmarks = binaryBlockingBookmarks.Value as IEnumerable<BookmarkInfo>;

                foreach (BookmarkInfo bookmarkInfo in activeBookmarks)
                {
                    bookmarkListBuilder.AppendFormat(CultureInfo.InvariantCulture, "[{0}: {1}]{2}", bookmarkInfo.BookmarkName, bookmarkInfo.OwnerDisplayName, Environment.NewLine);
                }

                blockingBookmarks = bookmarkListBuilder.ToString();
            }

            return blockingBookmarks;
        }

        static string GetExecutionStatus(SaveWorkflowCommand saveWorkflowCommand)
        {
            string executionStatus = null;
            InstanceValue executionStatusProperty;

            if (saveWorkflowCommand.InstanceData.TryGetValue(SqlWorkflowInstanceStoreConstants.StatusPropertyName, out executionStatusProperty))
            {
                executionStatus = (string)executionStatusProperty.Value;
            }

            return executionStatus;
        }

        static Int64? GetPendingTimerExpiration(SaveWorkflowCommand saveWorkflowCommand)
        {
            InstanceValue pendingTimerExpirationPropertyValue;

            if (saveWorkflowCommand.InstanceData.TryGetValue(SqlWorkflowInstanceStoreConstants.PendingTimerExpirationPropertyName, out pendingTimerExpirationPropertyValue))
            {
                DateTime pendingTimerExpiration = ((DateTime)pendingTimerExpirationPropertyValue.Value).ToUniversalTime();
                TimeSpan datetimeOffset = pendingTimerExpiration - DateTime.UtcNow;

                return (Int64)datetimeOffset.TotalMilliseconds;
            }

            return null;
        }

        static SuspensionStateChange GetSuspensionReason(SaveWorkflowCommand saveWorkflowCommand, out string suspensionReason, out string suspensionExceptionName)
        {
            IDictionary<XName, InstanceValue> instanceMetadataChanges = saveWorkflowCommand.InstanceMetadataChanges;
            SuspensionStateChange suspensionStateChange = SuspensionStateChange.NoChange;
            InstanceValue propertyValue;
            suspensionReason = null;
            suspensionExceptionName = null;

            if (instanceMetadataChanges.TryGetValue(WorkflowServiceNamespace.SuspendReason, out propertyValue))
            {
                if (!propertyValue.IsDeletedValue)
                {
                    suspensionStateChange = SuspensionStateChange.SuspendInstance;
                    suspensionReason = (string)propertyValue.Value;

                    if (instanceMetadataChanges.TryGetValue(WorkflowServiceNamespace.SuspendException, out propertyValue) && !propertyValue.IsDeletedValue)
                    {
                        suspensionExceptionName = ((Exception)propertyValue.Value).GetType().ToString();
                    }
                }
                else
                {
                    suspensionStateChange = SuspensionStateChange.UnsuspendInstance;
                }
            }

            return suspensionStateChange;
        }

        static Guid? GetWorkflowHostType(SaveWorkflowCommand saveWorkflowCommand)
        {
            InstanceValue instanceValue;
            if (saveWorkflowCommand.InstanceMetadataChanges.TryGetValue(WorkflowNamespace.WorkflowHostType, out instanceValue))
            {
                XName workflowHostType = instanceValue.Value as XName;

                if (workflowHostType == null)
                {
                    throw FxTrace.Exception.AsError(new InstancePersistenceCommandException(SR.InvalidMetadataValue(WorkflowNamespace.WorkflowHostType, typeof(XName).Name)));
                }
                byte[] workflowHostTypeBuffer = Encoding.Unicode.GetBytes(((XName)instanceValue.Value).ToString());
                return new Guid(HashHelper.ComputeHash(workflowHostTypeBuffer));
            }
            return null;
        }

        static bool IsReadyToRun(SaveWorkflowCommand saveWorkflowCommand)
        {
            InstanceValue statusPropertyValue;

            if (saveWorkflowCommand.InstanceData.TryGetValue(SqlWorkflowInstanceStoreConstants.StatusPropertyName, out statusPropertyValue) &&
                ((string)statusPropertyValue.Value) == SqlWorkflowInstanceStoreConstants.ExecutingStatusPropertyValue)
            {
                return true;
            }

            return false;
        }

        static void UpdateKeyData(InstancePersistenceContext context, SaveWorkflowCommand saveWorkflowCommand)
        {
            InstanceView instanceView = context.InstanceView;

            foreach (KeyValuePair<Guid, IDictionary<XName, InstanceValue>> keyEntry in saveWorkflowCommand.InstanceKeysToAssociate)
            {
                if (!instanceView.InstanceKeys.ContainsKey(keyEntry.Key))
                {
                    context.AssociatedInstanceKey(keyEntry.Key);

                    if (keyEntry.Value != null)
                    {
                        foreach (KeyValuePair<XName, InstanceValue> property in keyEntry.Value)
                        {
                            context.WroteInstanceKeyMetadataValue(keyEntry.Key, property.Key, property.Value);
                        }
                    }
                }
            }

            foreach (Guid key in saveWorkflowCommand.InstanceKeysToComplete)
            {
                InstanceKeyView existingKeyView;
                if (instanceView.InstanceKeys.TryGetValue(key, out existingKeyView))
                {
                    if (existingKeyView.InstanceKeyState != InstanceKeyState.Completed)
                    {
                        context.CompletedInstanceKey(key);
                    }
                }
            }

            foreach (Guid key in saveWorkflowCommand.InstanceKeysToFree)
            {
                InstanceKeyView existingKeyView;
                if (instanceView.InstanceKeys.TryGetValue(key, out existingKeyView))
                {
                    context.UnassociatedInstanceKey(key);
                }
            }

            foreach (KeyValuePair<Guid, IDictionary<XName, InstanceValue>> keyEntry in saveWorkflowCommand.InstanceKeyMetadataChanges)
            {
                if (keyEntry.Value != null)
                {
                    foreach (KeyValuePair<XName, InstanceValue> property in keyEntry.Value)
                    {
                        context.WroteInstanceKeyMetadataValue(keyEntry.Key, property.Key, property.Value);
                    }
                }
            }

            if (saveWorkflowCommand.CompleteInstance)
            {
                foreach (KeyValuePair<Guid, InstanceKeyView> instanceKeys in instanceView.InstanceKeys)
                {
                    if (instanceKeys.Value != null)
                    {
                        if (instanceKeys.Value.InstanceKeyState == InstanceKeyState.Associated)
                        {
                            context.CompletedInstanceKey(instanceKeys.Key);
                        }
                    }
                }
            }
        }

        void ExtractServiceDeploymentInformation(SaveWorkflowCommand saveWorkflowCommand, StringBuilder commandTextBuilder, SqlParameterCollection parameters)
        {
            InstanceValue instanceValue;
            //Extract the activation parameters
            string serviceName = null;
            string serviceNamespace = null;
            string site = null;
            string relativeApplicationPath = null;
            string relativeServicePath = null;

            if (saveWorkflowCommand.InstanceMetadataChanges.TryGetValue(PersistenceMetadataNamespace.ActivationType, out instanceValue))
            {
                if (PersistenceMetadataNamespace.ActivationTypes.WAS.Equals(instanceValue.Value))
                {
                    if (saveWorkflowCommand.InstanceMetadataChanges.TryGetValue(WorkflowServiceNamespace.Service, out instanceValue))
                    {
                        serviceName = ((XName)instanceValue.Value).LocalName;
                        serviceNamespace = ((XName)instanceValue.Value).Namespace.NamespaceName;
                    }
                    if (saveWorkflowCommand.InstanceMetadataChanges.TryGetValue(WorkflowServiceNamespace.SiteName, out instanceValue))
                    {
                        site = (string)instanceValue.Value;
                    }
                    if (saveWorkflowCommand.InstanceMetadataChanges.TryGetValue(WorkflowServiceNamespace.RelativeApplicationPath, out instanceValue))
                    {
                        relativeApplicationPath = (string)instanceValue.Value;
                    }
                    if (saveWorkflowCommand.InstanceMetadataChanges.TryGetValue(WorkflowServiceNamespace.RelativeServicePath, out instanceValue))
                    {
                        relativeServicePath = (string)instanceValue.Value;
                    }

                    byte[] serviceDeploymentHashBuffer = Encoding.Unicode.GetBytes(string.Format(CultureInfo.InvariantCulture, "{0}#{1}#{2}#{3}#{4}",
                        serviceName ?? string.Empty, serviceNamespace ?? string.Empty, site ?? string.Empty, relativeApplicationPath ?? string.Empty, relativeServicePath ?? string.Empty));
                    this.serviceDeploymentHash = new Guid(HashHelper.ComputeHash(serviceDeploymentHashBuffer));

                    //Get the service id has been seen before, get it from the cache
                    GetServiceDeploymentId();
                }
                else
                {
                    throw FxTrace.Exception.AsError(new InstancePersistenceCommandException(SR.NonWASActivationNotSupported));
                }
            }

            if ((this.serviceDeploymentHash != Guid.Empty) && (this.serviceDeploymentId == 0))
            {
                //This is the first time we see this service deployment so we need to create a new entry for it before creating the instance
                commandTextBuilder.AppendLine("declare @serviceDeploymentId bigint");
                commandTextBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "exec {0}.[CreateServiceDeployment] {1} ;",
                    SqlWorkflowInstanceStoreConstants.DefaultSchema, SaveWorkflowAsyncResult.createServiceDeploymentStoredProcedureParameters));

                parameters.Add(new SqlParameter { ParameterName = "@serviceDeploymentHash", SqlDbType = SqlDbType.UniqueIdentifier, Value = this.serviceDeploymentHash });
                parameters.Add(new SqlParameter { ParameterName = "@serviceName", Size = -1, SqlDbType = SqlDbType.NVarChar, Value = serviceName ?? (object)DBNull.Value });
                parameters.Add(new SqlParameter { ParameterName = "@serviceNamespace", Size = -1, SqlDbType = SqlDbType.NVarChar, Value = serviceNamespace ?? (object)DBNull.Value });
                parameters.Add(new SqlParameter { ParameterName = "@siteName", Size = -1, SqlDbType = SqlDbType.NVarChar, Value = site ?? (object)DBNull.Value });
                parameters.Add(new SqlParameter { ParameterName = "@relativeServicePath", Size = -1, SqlDbType = SqlDbType.NVarChar, Value = relativeServicePath ?? (object)DBNull.Value });
                parameters.Add(new SqlParameter { ParameterName = "@relativeApplicationPath", Size = -1, SqlDbType = SqlDbType.NVarChar, Value = relativeApplicationPath ?? (object)DBNull.Value });
            }
            else
            {
                parameters.Add(new SqlParameter { ParameterName = "@serviceDeploymentId", SqlDbType = SqlDbType.BigInt, Value = (this.serviceDeploymentId != 0) ? (object)this.serviceDeploymentId : (object)DBNull.Value });
            }
        }

        void GetServiceDeploymentId()
        {
            try
            {
                SaveWorkflowAsyncResult.serviceDeploymentIdsCacheLock.EnterReadLock();
                SaveWorkflowAsyncResult.serviceDeploymentIdsCache.TryGetValue(this.serviceDeploymentHash, out this.serviceDeploymentId);
            }
            finally
            {
                SaveWorkflowAsyncResult.serviceDeploymentIdsCacheLock.ExitReadLock();
            }
        }

        void PutServiceDeploymentId()
        {
            try
            {
                serviceDeploymentIdsCacheLock.EnterWriteLock();
                serviceDeploymentIdsCache[this.serviceDeploymentHash] = this.serviceDeploymentId;
            }
            finally
            {
                serviceDeploymentIdsCacheLock.ExitWriteLock();
            }
        }

        void SerializeAssociatedData(SqlParameterCollection parameters, SaveWorkflowCommand saveWorkflowCommand, StringBuilder commandTextBuilder)
        {
            if (saveWorkflowCommand.CompleteInstance && base.Store.InstanceCompletionAction == InstanceCompletionAction.DeleteAll)
            {
                parameters.Add(new SqlParameter { ParameterName = "@keysToAssociate", SqlDbType = SqlDbType.Xml, Value = DBNull.Value });
                parameters.Add(new SqlParameter { ParameterName = "@singleKeyId", SqlDbType = SqlDbType.UniqueIdentifier, Value = DBNull.Value });
                parameters.Add(new SqlParameter { ParameterName = "@keysToComplete", SqlDbType = SqlDbType.Xml, Value = DBNull.Value });
                parameters.Add(new SqlParameter { ParameterName = "@keysToFree", SqlDbType = SqlDbType.Xml, Value = DBNull.Value });
                parameters.Add(new SqlParameter { ParameterName = "@concatenatedKeyProperties", SqlDbType = SqlDbType.VarBinary, Value = DBNull.Value });
                parameters.Add(new SqlParameter { ParameterName = "@primitiveDataProperties", SqlDbType = SqlDbType.VarBinary, Value = DBNull.Value });
                parameters.Add(new SqlParameter { ParameterName = "@complexDataProperties", SqlDbType = SqlDbType.VarBinary, Value = DBNull.Value });
                parameters.Add(new SqlParameter { ParameterName = "@writeOnlyPrimitiveDataProperties", SqlDbType = SqlDbType.VarBinary, Value = DBNull.Value });
                parameters.Add(new SqlParameter { ParameterName = "@writeOnlyComplexDataProperties", SqlDbType = SqlDbType.VarBinary, Value = DBNull.Value });
                parameters.Add(new SqlParameter { ParameterName = "@metadataProperties", SqlDbType = SqlDbType.VarBinary, Value = DBNull.Value });
                parameters.Add(new SqlParameter { ParameterName = "@metadataIsConsistent", SqlDbType = SqlDbType.Bit, Value = DBNull.Value });
                parameters.Add(new SqlParameter { ParameterName = "@encodingOption", SqlDbType = SqlDbType.TinyInt, Value = DBNull.Value });
                parameters.Add(new SqlParameter { ParameterName = "@lastMachineRunOn", SqlDbType = SqlDbType.NVarChar, Value = DBNull.Value });
                parameters.Add(new SqlParameter { ParameterName = "@executionStatus", SqlDbType = SqlDbType.NVarChar, Value = DBNull.Value });
                parameters.Add(new SqlParameter { ParameterName = "@blockingBookmarks", SqlDbType = SqlDbType.NVarChar, Value = DBNull.Value });

                return;
            }

            List<CorrelationKey> keysToAssociate = CorrelationKey.BuildKeyList(saveWorkflowCommand.InstanceKeysToAssociate, base.Store.InstanceEncodingOption);
            List<CorrelationKey> keysToComplete = CorrelationKey.BuildKeyList(saveWorkflowCommand.InstanceKeysToComplete);
            List<CorrelationKey> keysToFree = CorrelationKey.BuildKeyList(saveWorkflowCommand.InstanceKeysToFree);
            ArraySegment<byte>[] dataProperties = SerializationUtilities.SerializePropertyBag(saveWorkflowCommand.InstanceData, base.Store.InstanceEncodingOption);
            ArraySegment<byte> metadataProperties = SerializationUtilities.SerializeMetadataPropertyBag(saveWorkflowCommand, base.InstancePersistenceContext, base.Store.InstanceEncodingOption);
            byte[] concatenatedKeyProperties = SerializationUtilities.CreateKeyBinaryBlob(keysToAssociate);
            bool metadataConsistency = (base.InstancePersistenceContext.InstanceView.InstanceMetadataConsistency == InstanceValueConsistency.None);
            bool singleKeyToAssociate = (keysToAssociate != null && keysToAssociate.Count == 1);

            parameters.Add(new SqlParameter { ParameterName = "@keysToAssociate", SqlDbType = SqlDbType.Xml, Value = singleKeyToAssociate ? DBNull.Value : SerializationUtilities.CreateCorrelationKeyXmlBlob(keysToAssociate) });
            parameters.Add(new SqlParameter { ParameterName = "@singleKeyId", SqlDbType = SqlDbType.UniqueIdentifier, Value = singleKeyToAssociate ? keysToAssociate[0].KeyId : (object)DBNull.Value });
            parameters.Add(new SqlParameter { ParameterName = "@keysToComplete", SqlDbType = SqlDbType.Xml, Value = SerializationUtilities.CreateCorrelationKeyXmlBlob(keysToComplete) });
            parameters.Add(new SqlParameter { ParameterName = "@keysToFree", SqlDbType = SqlDbType.Xml, Value = SerializationUtilities.CreateCorrelationKeyXmlBlob(keysToFree) });
            parameters.Add(new SqlParameter { ParameterName = "@concatenatedKeyProperties", SqlDbType = SqlDbType.VarBinary, Size = -1, Value = (object)concatenatedKeyProperties ?? DBNull.Value });
            parameters.Add(new SqlParameter { ParameterName = "@metadataIsConsistent", SqlDbType = SqlDbType.Bit, Value = metadataConsistency });
            parameters.Add(new SqlParameter { ParameterName = "@encodingOption", SqlDbType = SqlDbType.TinyInt, Value = base.Store.InstanceEncodingOption });
            parameters.Add(new SqlParameter { ParameterName = "@lastMachineRunOn", SqlDbType = SqlDbType.NVarChar, Size = 450, Value = SqlWorkflowInstanceStoreConstants.MachineName });
            parameters.Add(new SqlParameter { ParameterName = "@executionStatus", SqlDbType = SqlDbType.NVarChar, Size = 450, Value = GetExecutionStatus(saveWorkflowCommand) ?? (object)DBNull.Value });
            parameters.Add(new SqlParameter { ParameterName = "@blockingBookmarks", SqlDbType = SqlDbType.NVarChar, Size = -1, Value = GetBlockingBookmarks(saveWorkflowCommand) ?? (object)DBNull.Value });

            ArraySegment<byte>[] properties = { dataProperties[0], dataProperties[1], dataProperties[2], dataProperties[3], metadataProperties };
            string[] dataPropertyParameters = { "@primitiveDataProperties", "@complexDataProperties", "@writeOnlyPrimitiveDataProperties", @"writeOnlyComplexDataProperties", "@metadataProperties" };

            for (int i = 0; i < 5; i++)
            {
                SaveWorkflowAsyncResult.AddSerializedProperty(properties[i], parameters, dataPropertyParameters[i]);
            }

            this.SerializePromotedProperties(parameters, commandTextBuilder, saveWorkflowCommand);
        }

        void SerializePromotedProperties(SqlParameterCollection parameters, StringBuilder commandTextBuilder, SaveWorkflowCommand saveWorkflowCommand)
        {
            const int SqlVariantStartColumn = 1;
            const string promotionNameParameter = "@promotionName=";
            const string instanceIdParameter = "@instanceId=";
            int promotionNumber = 0;

            foreach (KeyValuePair<string, Tuple<List<XName>, List<XName>>> promotion in base.Store.Promotions)
            {
                StringBuilder storedProcInvocationBuilder = new StringBuilder(SqlWorkflowInstanceStoreConstants.DefaultStringBuilderCapacity);
                int column = SqlVariantStartColumn;
                bool addPromotion = false;
                string promotionNameArgument = string.Format(CultureInfo.InvariantCulture, "@promotionName{0}", promotionNumber);
                string instanceIdArgument = string.Format(CultureInfo.InvariantCulture, "@instanceId{0}", promotionNumber);

                storedProcInvocationBuilder.Append(string.Format(CultureInfo.InvariantCulture, "exec {0}.[InsertPromotedProperties] ", SqlWorkflowInstanceStoreConstants.DefaultSchema));
                storedProcInvocationBuilder.Append(promotionNameParameter);
                storedProcInvocationBuilder.Append(promotionNameArgument);
                storedProcInvocationBuilder.Append(",");
                storedProcInvocationBuilder.Append(instanceIdParameter);
                storedProcInvocationBuilder.Append(instanceIdArgument);

                foreach (XName name in promotion.Value.Item1)
                {
                    InstanceValue propertyValue;

                    if (saveWorkflowCommand.InstanceData.TryGetValue(name, out propertyValue))
                    {
                        if (!SerializationUtilities.IsPropertyTypeSqlVariantCompatible(propertyValue))
                        {
                            throw FxTrace.Exception.AsError(new InstancePersistenceException(SR.CannotPromoteAsSqlVariant(propertyValue.Value.GetType().ToString(), name.ToString())));
                        }

                        string parameterName = string.Format(CultureInfo.InvariantCulture, "@value{0}=", column);
                        string argumentName = string.Format(CultureInfo.InvariantCulture, "@value{0}_promotion{1}", column, promotionNumber);
                        parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Variant, ParameterName = argumentName, Value = propertyValue.Value ?? DBNull.Value });

                        storedProcInvocationBuilder.Append(", ");
                        storedProcInvocationBuilder.Append(parameterName);
                        storedProcInvocationBuilder.Append(argumentName);
                        addPromotion = true;
                    }
                    column++;
                }

                column = SqlVariantStartColumn + SqlWorkflowInstanceStoreConstants.MaximumPropertiesPerPromotion;

                foreach (XName name in promotion.Value.Item2)
                {
                    InstanceValue propertyValue;
                    IObjectSerializer serializer = ObjectSerializerFactory.GetObjectSerializer(base.Store.InstanceEncodingOption);

                    if (saveWorkflowCommand.InstanceData.TryGetValue(name, out propertyValue))
                    {
                        string parameterName = string.Format(CultureInfo.InvariantCulture, "@value{0}=", column);
                        string argumentName = string.Format(CultureInfo.InvariantCulture, "@value{0}_promotion{1}", column, promotionNumber);

                        SaveWorkflowAsyncResult.AddSerializedProperty(serializer.SerializeValue(propertyValue.Value), parameters, argumentName);
                        storedProcInvocationBuilder.Append(", ");
                        storedProcInvocationBuilder.Append(parameterName);
                        storedProcInvocationBuilder.Append(argumentName);
                        addPromotion = true;
                    }
                    column++;
                }

                if (addPromotion)
                {
                    parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.NVarChar, Size = 400, ParameterName = promotionNameArgument, Value = promotion.Key });
                    parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.UniqueIdentifier, ParameterName = instanceIdArgument, Value = base.InstancePersistenceContext.InstanceView.InstanceId });
                    storedProcInvocationBuilder.Append(";");
                    commandTextBuilder.AppendLine(storedProcInvocationBuilder.ToString());
                    promotionNumber++;
                }
            }
        }
    }
}
