//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Administration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    class ContractInstanceProvider : ProviderBase, IWmiProvider
    {
        static Dictionary<string, ContractDescription> knownContracts = new Dictionary<string, ContractDescription>();

        internal static string ContractReference(string contractName)
        {
            return String.Format(CultureInfo.InvariantCulture,
                                 AdministrationStrings.Contract +
                                        "." +
                                        AdministrationStrings.Name +
                                        "='{0}'," +
                                        AdministrationStrings.ProcessId +
                                        "={1}," +
                                        AdministrationStrings.AppDomainId +
                                        "={2}",
                                contractName,
                                AppDomainInfo.Current.ProcessId,
                                AppDomainInfo.Current.Id);
        }

        internal static void RegisterContract(ContractDescription contract)
        {
            lock (ContractInstanceProvider.knownContracts)
            {
                if (!ContractInstanceProvider.knownContracts.ContainsKey(contract.Name))
                {
                    ContractInstanceProvider.knownContracts.Add(contract.Name, contract);
                }
            }
        }

        static void FillContract(IWmiInstance contract, ContractDescription contractDescription)
        {
            Fx.Assert(null != contractDescription, "contractDescription cannot be null");
            contract.SetProperty(AdministrationStrings.Type, contractDescription.ContractType.Name);
            if (null != contractDescription.CallbackContractType)
            {
                contract.SetProperty(AdministrationStrings.CallbackContract, ContractReference(contractDescription.CallbackContractType.Name));
            }

            contract.SetProperty(AdministrationStrings.Name, contractDescription.Name);
            contract.SetProperty(AdministrationStrings.Namespace, contractDescription.Namespace);
            contract.SetProperty(AdministrationStrings.SessionMode, contractDescription.SessionMode.ToString());

            IWmiInstance[] operations = new IWmiInstance[contractDescription.Operations.Count];
            for (int j = 0; j < operations.Length; ++j)
            {
                OperationDescription operationDescription = contractDescription.Operations[j];
                Fx.Assert(operationDescription.Messages.Count > 0, "");
                IWmiInstance operation = contract.NewInstance(AdministrationStrings.Operation);
                FillOperation(operation, operationDescription);
                operations[j] = operation;

            }
            contract.SetProperty(AdministrationStrings.Operations, operations);
            FillBehaviorsInfo(contract, contractDescription.Behaviors);
        }

        static void FillOperation(IWmiInstance operation, OperationDescription operationDescription)
        {
            operation.SetProperty(AdministrationStrings.Name, operationDescription.Name);
            operation.SetProperty(AdministrationStrings.Action, FixWildcardAction(operationDescription.Messages[0].Action));
            if (operationDescription.Messages.Count > 1)
            {
                operation.SetProperty(AdministrationStrings.ReplyAction, FixWildcardAction(operationDescription.Messages[1].Action));
            }
            operation.SetProperty(AdministrationStrings.IsOneWay, operationDescription.IsOneWay);
            operation.SetProperty(AdministrationStrings.IsInitiating, operationDescription.IsInitiating);
            operation.SetProperty(AdministrationStrings.IsTerminating, operationDescription.IsTerminating);
            operation.SetProperty(AdministrationStrings.AsyncPattern, null != operationDescription.BeginMethod);
            if (null != operationDescription.SyncMethod)
            {
                if (null != operationDescription.SyncMethod.ReturnType)
                {
                    operation.SetProperty(AdministrationStrings.ReturnType, operationDescription.SyncMethod.ReturnType.Name);
                }
                operation.SetProperty(AdministrationStrings.MethodSignature, operationDescription.SyncMethod.ToString());
                ParameterInfo[] parameterInfo = operationDescription.SyncMethod.GetParameters();
                string[] parameterTypes = new string[parameterInfo.Length];
                for (int i = 0; i < parameterInfo.Length; i++)
                {
                    parameterTypes[i] = parameterInfo[i].ParameterType.ToString();
                }
                operation.SetProperty(AdministrationStrings.ParameterTypes, parameterTypes);
            }
            operation.SetProperty(AdministrationStrings.IsCallback, operationDescription.Messages[0].Direction == MessageDirection.Output);

            FillBehaviorsInfo(operation, operationDescription.Behaviors);

        }

        static void FillBehaviorsInfo(IWmiInstance operation, KeyedByTypeCollection<IOperationBehavior> behaviors)
        {
            List<IWmiInstance> behaviorInstances = new List<IWmiInstance>(behaviors.Count);
            foreach (IOperationBehavior behavior in behaviors)
            {
                IWmiInstance behaviorInstance;
                FillBehaviorInfo(behavior, operation, out behaviorInstance);
                if (null != behaviorInstance)
                {
                    behaviorInstances.Add(behaviorInstance);
                }
            }
            operation.SetProperty(AdministrationStrings.Behaviors, behaviorInstances.ToArray());
        }

        static void FillBehaviorsInfo(IWmiInstance operation, KeyedByTypeCollection<IContractBehavior> behaviors)
        {
            List<IWmiInstance> behaviorInstances = new List<IWmiInstance>(behaviors.Count);
            foreach (IContractBehavior behavior in behaviors)
            {
                IWmiInstance behaviorInstance;
                FillBehaviorInfo(behavior, operation, out behaviorInstance);
                if (null != behaviorInstance)
                {
                    behaviorInstances.Add(behaviorInstance);
                }
            }
            operation.SetProperty(AdministrationStrings.Behaviors, behaviorInstances.ToArray());
        }

        static void FillBehaviorInfo(IContractBehavior behavior, IWmiInstance existingInstance, out IWmiInstance instance)
        {
            Fx.Assert(null != existingInstance, "");
            Fx.Assert(null != behavior, "");
            instance = null;
            if (behavior is DeliveryRequirementsAttribute)
            {
                instance = existingInstance.NewInstance("DeliveryRequirementsAttribute");
                DeliveryRequirementsAttribute specificBehavior = (DeliveryRequirementsAttribute)behavior;
                instance.SetProperty(AdministrationStrings.QueuedDeliveryRequirements, specificBehavior.QueuedDeliveryRequirements.ToString());
                instance.SetProperty(AdministrationStrings.RequireOrderedDelivery, specificBehavior.RequireOrderedDelivery);
                if (null != specificBehavior.TargetContract)
                {
                    instance.SetProperty(AdministrationStrings.TargetContract, specificBehavior.TargetContract.ToString());
                }
            }
            else if (behavior is IWmiInstanceProvider)
            {
                IWmiInstanceProvider instanceProvider = (IWmiInstanceProvider)behavior;
                instance = existingInstance.NewInstance(instanceProvider.GetInstanceType());
                instanceProvider.FillInstance(instance);
            }
            else
            {
                instance = existingInstance.NewInstance("Behavior");
            }
            if (null != instance)
            {
                instance.SetProperty(AdministrationStrings.Type, behavior.GetType().FullName);
            }
        }

        static void FillBehaviorInfo(IOperationBehavior behavior, IWmiInstance existingInstance, out IWmiInstance instance)
        {
            Fx.Assert(null != existingInstance, "");
            Fx.Assert(null != behavior, "");
            instance = null;
            if (behavior is DataContractSerializerOperationBehavior)
            {
                instance = existingInstance.NewInstance("DataContractSerializerOperationBehavior");
                DataContractSerializerOperationBehavior specificBehavior = (DataContractSerializerOperationBehavior)behavior;
                instance.SetProperty(AdministrationStrings.IgnoreExtensionDataObject, specificBehavior.IgnoreExtensionDataObject);
                instance.SetProperty(AdministrationStrings.MaxItemsInObjectGraph, specificBehavior.MaxItemsInObjectGraph);
                if (null != specificBehavior.DataContractFormatAttribute)
                {
                    instance.SetProperty(AdministrationStrings.Style, specificBehavior.DataContractFormatAttribute.Style.ToString());
                }
            }
            else if (behavior is OperationBehaviorAttribute)
            {
                instance = existingInstance.NewInstance("OperationBehaviorAttribute");
                OperationBehaviorAttribute specificBehavior = (OperationBehaviorAttribute)behavior;
                instance.SetProperty(AdministrationStrings.AutoDisposeParameters, specificBehavior.AutoDisposeParameters);
                instance.SetProperty(AdministrationStrings.Impersonation, specificBehavior.Impersonation.ToString());
                instance.SetProperty(AdministrationStrings.ReleaseInstanceMode, specificBehavior.ReleaseInstanceMode.ToString());
                instance.SetProperty(AdministrationStrings.TransactionAutoComplete, specificBehavior.TransactionAutoComplete);
                instance.SetProperty(AdministrationStrings.TransactionScopeRequired, specificBehavior.TransactionScopeRequired);
            }
            else if (behavior is TransactionFlowAttribute)
            {
                instance = existingInstance.NewInstance("TransactionFlowAttribute");
                TransactionFlowAttribute specificBehavior = (TransactionFlowAttribute)behavior;
                instance.SetProperty(AdministrationStrings.TransactionFlowOption, specificBehavior.Transactions.ToString());
            }
            else if (behavior is XmlSerializerOperationBehavior)
            {
                instance = existingInstance.NewInstance("XmlSerializerOperationBehavior");
                XmlSerializerOperationBehavior specificBehavior = (XmlSerializerOperationBehavior)behavior;
                if (null != specificBehavior.XmlSerializerFormatAttribute)
                {
                    instance.SetProperty(AdministrationStrings.Style, specificBehavior.XmlSerializerFormatAttribute.Style.ToString());
                    instance.SetProperty(AdministrationStrings.Use, specificBehavior.XmlSerializerFormatAttribute.Use.ToString());
                    instance.SetProperty(AdministrationStrings.SupportFaults, specificBehavior.XmlSerializerFormatAttribute.SupportFaults.ToString());
                }
            }
            else if (behavior is IWmiInstanceProvider)
            {
                IWmiInstanceProvider instanceProvider = (IWmiInstanceProvider)behavior;
                instance = existingInstance.NewInstance(instanceProvider.GetInstanceType());
                instanceProvider.FillInstance(instance);
            }
            else
            {
                instance = existingInstance.NewInstance("Behavior");
            }
            if (null != instance)
            {
                instance.SetProperty(AdministrationStrings.Type, behavior.GetType().FullName);
            }
        }

        static string FixWildcardAction(string action)
        {
#pragma warning suppress 56507
            return null != action ? action : MessageHeaders.WildcardAction;
        }

        static void UpdateContracts()
        {
            foreach (ServiceInfo info in new ServiceInfoCollection(ManagementExtension.Services))
            {
                foreach (EndpointInfo endpointInfo in info.Endpoints)
                {
                    ContractInstanceProvider.RegisterContract(endpointInfo.Contract);
                }
            }
        }

        void IWmiProvider.EnumInstances(IWmiInstances instances)
        {
            Fx.Assert(null != instances, "");
            int processId = AppDomainInfo.Current.ProcessId;
            int appDomainId = AppDomainInfo.Current.Id;
            lock (ContractInstanceProvider.knownContracts)
            {
                UpdateContracts();
                foreach (ContractDescription contract in ContractInstanceProvider.knownContracts.Values)
                {
                    IWmiInstance instance = instances.NewInstance(null);

                    instance.SetProperty(AdministrationStrings.ProcessId, processId);
                    instance.SetProperty(AdministrationStrings.AppDomainId, appDomainId);

                    FillContract(instance, contract);

                    instances.AddInstance(instance);
                }
            }

        }

        bool IWmiProvider.GetInstance(IWmiInstance contract)
        {
            Fx.Assert(null != contract, "");
            bool bFound = false;
            if ((int)contract.GetProperty(AdministrationStrings.ProcessId) == AppDomainInfo.Current.ProcessId
                && (int)contract.GetProperty(AdministrationStrings.AppDomainId) == AppDomainInfo.Current.Id)
            {
                string contractName = (string)contract.GetProperty(AdministrationStrings.Name);

                ContractDescription contractDescription;
                UpdateContracts();
                if (ContractInstanceProvider.knownContracts.TryGetValue(contractName, out contractDescription))
                {
                    bFound = true;
                    FillContract(contract, contractDescription);
                }
            }

            return bFound;
        }
    }
}
