//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System.Activities.Persistence;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.Security.Cryptography;
    using System.Text;
    using System.Transactions;
    using System.Xml.Linq;

    sealed class CreateWorkflowOwnerAsyncResult : WorkflowOwnerAsyncResult
    {
        static readonly string commandText = string.Format(CultureInfo.InvariantCulture, "{0}.[CreateLockOwner]", SqlWorkflowInstanceStoreConstants.DefaultSchema);
        bool fireActivatableInstancesEvent;
        bool fireRunnableInstancesEvent;
        Guid lockOwnerId;

        public CreateWorkflowOwnerAsyncResult
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
        }

        protected override string ConnectionString
        {
            get
            {
                // by making CreateWorkflowOwnerAsyncResult to use the same Connection Pool as the PersistenceTasks(LockRenewalTask, etc),
                // we can prevent unbound bloating of new threads created for task dispatching blocked on the busy Connection Pool.
                // If the Connection Pool is too busy to handle pending tasks, then any incoming CreateWorkflowOwnerAsyncResult will also block on the Connection Pool too.
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(base.Store.CachedConnectionString);
                builder.ApplicationName = SqlWorkflowInstanceStore.CommonConnectionPoolName;
                return builder.ToString();
            }
        }

        protected override void GenerateSqlCommand(SqlCommand sqlCommand)
        {
            base.GenerateSqlCommand(sqlCommand);

            if (base.StoreLock.IsValid)
            {
                throw FxTrace.Exception.AsError(new InstancePersistenceCommandException(SR.MultipleLockOwnersNotSupported));
            }

            bool withIdentity;
            IDictionary<XName, InstanceValue> commandMetadata = GetCommandMetadata(out withIdentity);
            SqlParameterCollection parameters = sqlCommand.Parameters;
            double lockTimeout = base.Store.BufferedHostLockRenewalPeriod.TotalSeconds;
            this.lockOwnerId = Guid.NewGuid();
            ExtractWorkflowHostType(commandMetadata);

            InstanceValue instanceValue;
            if (commandMetadata.TryGetValue(PersistenceMetadataNamespace.ActivationType, out instanceValue))
            {
                if (withIdentity)
                {
                    throw FxTrace.Exception.AsError(new InstancePersistenceCommandException(SR.IdentityNotSupportedWithActivation));
                }
                if (!PersistenceMetadataNamespace.ActivationTypes.WAS.Equals(instanceValue.Value))
                {
                    throw FxTrace.Exception.AsError(new InstancePersistenceCommandException(SR.NonWASActivationNotSupported));
                }
                this.fireActivatableInstancesEvent = true;
            }

            ArraySegment<byte>[] properties = SerializationUtilities.SerializePropertyBag(commandMetadata, base.Store.InstanceEncodingOption);

            parameters.Add(new SqlParameter { ParameterName = "@lockTimeout", SqlDbType = SqlDbType.Int, Value = lockTimeout });
            parameters.Add(new SqlParameter { ParameterName = "@lockOwnerId", SqlDbType = SqlDbType.UniqueIdentifier, Value = this.lockOwnerId });
            parameters.Add(new SqlParameter { ParameterName = "@workflowHostType", SqlDbType = SqlDbType.UniqueIdentifier, Value = (base.Store.WorkflowHostType != Guid.Empty) ? base.Store.WorkflowHostType : (object) DBNull.Value });
            parameters.Add(new SqlParameter { ParameterName = "@enqueueCommand", SqlDbType = SqlDbType.Bit, Value = base.Store.EnqueueRunCommands });
            parameters.Add(new SqlParameter { ParameterName = "@deleteInstanceOnCompletion", SqlDbType = SqlDbType.Bit, Value = (base.Store.InstanceCompletionAction == InstanceCompletionAction.DeleteAll) });
            parameters.Add(new SqlParameter { ParameterName = "@primitiveLockOwnerData", SqlDbType = SqlDbType.VarBinary, Size = properties[0].Count, Value = (object)(properties[0].Array) ?? DBNull.Value });
            parameters.Add(new SqlParameter { ParameterName = "@complexLockOwnerData", SqlDbType = SqlDbType.VarBinary, Size = properties[1].Count, Value = (object)(properties[1].Array) ?? DBNull.Value });
            parameters.Add(new SqlParameter { ParameterName = "@writeOnlyPrimitiveLockOwnerData", SqlDbType = SqlDbType.VarBinary, Size = properties[2].Count, Value = (object)(properties[2].Array) ?? DBNull.Value });
            parameters.Add(new SqlParameter { ParameterName = "@writeOnlyComplexLockOwnerData", SqlDbType = SqlDbType.VarBinary, Size = properties[3].Count, Value = (object)(properties[3].Array) ?? DBNull.Value });
            parameters.Add(new SqlParameter { ParameterName = "@encodingOption", SqlDbType = SqlDbType.TinyInt, Value = base.Store.InstanceEncodingOption });
            parameters.Add(new SqlParameter { ParameterName = "@machineName", SqlDbType = SqlDbType.NVarChar, Value = SqlWorkflowInstanceStoreConstants.MachineName });

            if (withIdentity)
            {
                Fx.Assert(base.Store.DatabaseVersion >= StoreUtilities.Version45, "Should never get here if the db version isn't 4.5 or higher");

                string identityMetadataXml = SerializationUtilities.GetIdentityMetadataXml(base.InstancePersistenceCommand);
                parameters.Add(new SqlParameter { ParameterName = "@identityMetadata", SqlDbType = SqlDbType.Xml, Value = identityMetadataXml });
            }
        }

        protected override string GetSqlCommandText()
        {
            return CreateWorkflowOwnerAsyncResult.commandText;
        }

        protected override CommandType GetSqlCommandType()
        {
            return CommandType.StoredProcedure;
        }

        protected override Exception ProcessSqlResult(SqlDataReader reader)
        {
            Exception exception = StoreUtilities.GetNextResultSet(this.InstancePersistenceCommand.Name, reader);

            if (exception == null)
            {
                base.InstancePersistenceContext.BindInstanceOwner(this.lockOwnerId, this.lockOwnerId);
                long surrogateLockOwnerId = reader.GetInt64(1);

                // Activatable takes precendence over Runnable.  (Activation owners cannot run instances.)
                if (this.fireActivatableInstancesEvent)
                {
                    base.InstancePersistenceContext.BindEvent(HasActivatableWorkflowEvent.Value);
                }
                else if (this.fireRunnableInstancesEvent)
                {
                    base.InstancePersistenceContext.BindEvent(HasRunnableWorkflowEvent.Value);
                }

                base.StoreLock.MarkInstanceOwnerCreated(this.lockOwnerId, surrogateLockOwnerId, base.InstancePersistenceContext.InstanceHandle, this.fireRunnableInstancesEvent, this.fireActivatableInstancesEvent);
            }

            return exception;
        }

        void ExtractWorkflowHostType(IDictionary<XName, InstanceValue> commandMetadata)
        {
            InstanceValue instanceValue;
            if (commandMetadata.TryGetValue(WorkflowNamespace.WorkflowHostType, out instanceValue))
            {
                XName workflowHostType = instanceValue.Value as XName;

                if (workflowHostType == null)
                {
                    throw FxTrace.Exception.AsError(new InstancePersistenceCommandException(SR.InvalidMetadataValue(WorkflowNamespace.WorkflowHostType, typeof(XName).Name)));
                }

                byte[] workflowHostTypeBuffer = Encoding.Unicode.GetBytes(workflowHostType.ToString());
                base.Store.WorkflowHostType = new Guid(HashHelper.ComputeHash(workflowHostTypeBuffer));
                this.fireRunnableInstancesEvent = true;
            }
        }

        IDictionary<XName, InstanceValue> GetCommandMetadata(out bool withIdentity)
        {
            CreateWorkflowOwnerWithIdentityCommand createOwnerWithIdentityCommand = base.InstancePersistenceCommand as CreateWorkflowOwnerWithIdentityCommand;
            if (createOwnerWithIdentityCommand != null)
            {
                withIdentity = true;
                return createOwnerWithIdentityCommand.InstanceOwnerMetadata;
            }
            else
            {
                CreateWorkflowOwnerCommand createOwnerCommand = (CreateWorkflowOwnerCommand)base.InstancePersistenceCommand;
                withIdentity = false;
                return createOwnerCommand.InstanceOwnerMetadata;
            }
        }
    }
}
