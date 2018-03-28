//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Collections.Generic;
    using System.Collections;
    using System.Reflection;

    class OperationSelectorBehavior : IContractBehavior
    {
        void IContractBehavior.Validate(ContractDescription description, ServiceEndpoint endpoint)
        {
        }

        void IContractBehavior.AddBindingParameters(ContractDescription description, ServiceEndpoint endpoint, BindingParameterCollection parameters)
        {
        }

        void IContractBehavior.ApplyDispatchBehavior(ContractDescription description, ServiceEndpoint endpoint, DispatchRuntime dispatch)
        {
            if (dispatch.ClientRuntime != null)
                dispatch.ClientRuntime.OperationSelector = new MethodInfoOperationSelector(description, MessageDirection.Output); 
        }

        void IContractBehavior.ApplyClientBehavior(ContractDescription description, ServiceEndpoint endpoint, ClientRuntime proxy)
        {
            proxy.OperationSelector = new MethodInfoOperationSelector(description, MessageDirection.Input);
        }

        internal class MethodInfoOperationSelector : IClientOperationSelector
        {
            Dictionary<object, string> operationMap;

            internal MethodInfoOperationSelector(ContractDescription description, MessageDirection directionThatRequiresClientOpSelection)
            {
                operationMap = new Dictionary<object, string>();

                for (int i = 0; i < description.Operations.Count; i++)
                {
                    OperationDescription operation = description.Operations[i];
                    if (operation.Messages[0].Direction == directionThatRequiresClientOpSelection)
                    {
                        if (operation.SyncMethod != null)
                        {
                            if (!operationMap.ContainsKey(operation.SyncMethod.MethodHandle))
                                operationMap.Add(operation.SyncMethod.MethodHandle, operation.Name);
                        }
    
                        if (operation.BeginMethod != null)
                        {
                            if (!operationMap.ContainsKey(operation.BeginMethod.MethodHandle))
                            {
                                operationMap.Add(operation.BeginMethod.MethodHandle, operation.Name);
                                operationMap.Add(operation.EndMethod.MethodHandle, operation.Name);                    
                            }
                        }

                        if (operation.TaskMethod != null)
                        {
                            if (!operationMap.ContainsKey(operation.TaskMethod.MethodHandle))
                            {
                                operationMap.Add(operation.TaskMethod.MethodHandle, operation.Name);
                            }
                        }
                    }
                }
            }

            public bool AreParametersRequiredForSelection
            {
                get { return false; }
            }

            public string SelectOperation(MethodBase method, object[] parameters)
            {
                if (this.operationMap.ContainsKey(method.MethodHandle))
                    return operationMap[method.MethodHandle];
                else
                    return null;
            }
        }
    }
}
