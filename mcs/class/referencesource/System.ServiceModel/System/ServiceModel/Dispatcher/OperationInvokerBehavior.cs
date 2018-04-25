//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System.Collections.Generic;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    class OperationInvokerBehavior : IOperationBehavior
    {
        public OperationInvokerBehavior()
        {
        }

        void IOperationBehavior.Validate(OperationDescription description)
        {
        }

        void IOperationBehavior.AddBindingParameters(OperationDescription description, BindingParameterCollection parameters)
        {
        }

        void IOperationBehavior.ApplyDispatchBehavior(OperationDescription description, DispatchOperation dispatch)
        {
            if (dispatch == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dispatch");
            }
            if (description == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("description");
            }

            if (description.TaskMethod != null)
            {
                dispatch.Invoker = new TaskMethodInvoker(description.TaskMethod, description.TaskTResult);
            }
            else if (description.SyncMethod != null)
            {
                if (description.BeginMethod != null)
                {
                    // both sync and async methods are present on the contract, check the preference
                    OperationBehaviorAttribute operationBehaviorAttribue = description.Behaviors.Find<OperationBehaviorAttribute>();
                    if ((operationBehaviorAttribue != null) && operationBehaviorAttribue.PreferAsyncInvocation)
                    {
                        dispatch.Invoker = new AsyncMethodInvoker(description.BeginMethod, description.EndMethod);
                    }
                    else
                    {
                        dispatch.Invoker = new SyncMethodInvoker(description.SyncMethod);
                    }
                }
                else
                {
                    // only sync method is present on the contract
                    dispatch.Invoker = new SyncMethodInvoker(description.SyncMethod);
                }
            }
            else
            {
                if (description.BeginMethod != null)
                {
                    // only async method is present on the contract
                    dispatch.Invoker = new AsyncMethodInvoker(description.BeginMethod, description.EndMethod);
                }
            }
        }

        void IOperationBehavior.ApplyClientBehavior(OperationDescription description, ClientOperation proxy)
        {
        }
    }
}
