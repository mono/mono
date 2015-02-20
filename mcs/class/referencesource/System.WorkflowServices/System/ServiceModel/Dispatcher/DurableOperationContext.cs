//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel.Description;

    [Obsolete("The WF3 types are deprecated.  Instead, please use the new WF4 types from System.Activities.*")]
    public static class DurableOperationContext
    {
        public static Guid InstanceId
        {
            get
            {
                ServiceDurableInstance durableInstance = GetInstanceContextExtension();

                return durableInstance.InstanceId;
            }
        }

        public static void AbortInstance()
        {
            ServiceDurableInstance durableInstance = GetInstanceContextExtension();

            durableInstance.AbortInstance();
        }

        public static void CompleteInstance()
        {
            ServiceDurableInstance durableInstance = GetInstanceContextExtension();

            durableInstance.MarkForCompletion();
        }

        internal static void BeginOperation()
        {
            OperationContext operationContext = OperationContext.Current;

            if (operationContext != null)
            {
                operationContext.Extensions.Add(new DurableOperationContext.IsInOperation());
            }
        }

        internal static void EndOperation()
        {
            OperationContext operationContext = OperationContext.Current;

            if (operationContext != null)
            {
                DurableOperationContext.IsInOperation isInOperation = operationContext.Extensions.Find<DurableOperationContext.IsInOperation>();

                if (isInOperation != null)
                {
                    operationContext.Extensions.Remove(isInOperation);
                }
            }
        }

        static ServiceDurableInstance GetInstanceContextExtension()
        {
            OperationContext operationContext = OperationContext.Current;

            if (operationContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(
                    SR2.GetString(
                    SR2.OnlyCallableFromServiceOperation,
                    typeof(DurableOperationContext).Name)));
            }

            IsInOperation isInOperation = operationContext.Extensions.Find<IsInOperation>();

            if (isInOperation == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(
                    SR2.GetString(
                    SR2.OnlyCallableWhileInOperation,
                    typeof(DurableOperationContext).Name)));
            }

            InstanceContext currentInstanceContext = operationContext.InstanceContext;

            if (currentInstanceContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(
                    SR2.GetString(
                    SR2.OnlyCallableFromServiceOperation,
                    typeof(DurableOperationContext).Name)));
            }

            ServiceDurableInstance durableInstance =
                currentInstanceContext.Extensions.Find<ServiceDurableInstance>();

            if (durableInstance == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(
                    SR2.GetString(
                    SR2.OnlyCallableFromDurableService,
                    typeof(DurableOperationContext).Name,
                    typeof(DurableServiceAttribute).Name)));
            }

            return durableInstance;
        }

        class IsInOperation : IExtension<OperationContext>
        {
            public void Attach(OperationContext owner)
            {
            }

            public void Detach(OperationContext owner)
            {
            }
        }
    }
}
