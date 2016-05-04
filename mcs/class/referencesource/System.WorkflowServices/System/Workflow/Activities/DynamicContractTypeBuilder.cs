//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Activities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing.Design;
    using System.Net.Security;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.ServiceModel;
    using System.Threading;
    using System.Workflow.Activities.Design;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    internal static class DynamicContractTypeBuilder
    {
        internal static readonly DependencyProperty DynamicContractTypesProperty =
            DependencyProperty.RegisterAttached("DynamicContractTypes",
            typeof(Dictionary<string, ContractType>), typeof(DynamicContractTypeBuilder),
            new PropertyMetadata(null, DependencyPropertyOptions.NonSerialized));

        public static Type GetContractType(OperationInfo operationInfo, ReceiveActivity contextActivity)
        {
            if (operationInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("operationInfo");
            }

            if (contextActivity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contextActivity");
            }

            if (string.IsNullOrEmpty(operationInfo.ContractName))
            {
                return null;
            }

            Activity rootActivity = contextActivity.RootActivity;
            Dictionary<string, ContractType> contractTypes =
                rootActivity.GetValue(DynamicContractTypeBuilder.DynamicContractTypesProperty) as Dictionary<string, ContractType>;

            if (contractTypes == null)
            {
                Activity definition = rootActivity.GetValue(Activity.WorkflowDefinitionProperty) as Activity;
                if (definition != null)
                {
                    contractTypes = definition.GetValue(DynamicContractTypeBuilder.DynamicContractTypesProperty) as Dictionary<string, ContractType>;
                }

                if (contractTypes != null)
                {
                    rootActivity.SetValue(DynamicContractTypeBuilder.DynamicContractTypesProperty, contractTypes);
                }
            }

            if (contractTypes == null)
            {
                contractTypes = BuildContractTypes(rootActivity);
                rootActivity.SetValue(DynamicContractTypeBuilder.DynamicContractTypesProperty, contractTypes);
            }

            if (contractTypes.ContainsKey(operationInfo.ContractName))
            {
                return contractTypes[operationInfo.ContractName];
            }

            return null;
        }

        static Dictionary<string, ContractType> BuildContractTypes(Activity contextActivity)
        {
            if (contextActivity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contextActivity");
            }

            Dictionary<string, ContractType> types = new Dictionary<string, ContractType>();

            Walker walker = new Walker(true);
            walker.FoundActivity += delegate(Walker w, WalkerEventArgs args)
            {
                ReceiveActivity currentActivity = args.CurrentActivity as ReceiveActivity;
                if (currentActivity == null)
                {
                    return;
                }
                OperationInfo operationInfo = currentActivity.ServiceOperationInfo as OperationInfo;
                if (operationInfo == null)
                {
                    return;
                }

                if (string.IsNullOrEmpty(operationInfo.ContractName) ||
                    string.IsNullOrEmpty(operationInfo.Name))
                {
                    return;
                }

                if (!types.ContainsKey(operationInfo.ContractName))
                {
                    types.Add(operationInfo.ContractName,
                        new ContractType(operationInfo.ContractName));
                }

                bool hasReturnValue = false;
                bool duplicatedPositions = false;
                int maxPosition = -1;
                List<int> parameterIndexs = new List<int>();

                foreach (OperationParameterInfo operationParameterInfo in operationInfo.Parameters)
                {
                    if (operationParameterInfo.Position == -1)
                    {
                        hasReturnValue = true;
                    }
                    else
                    {
                        maxPosition = (maxPosition < operationParameterInfo.Position) ? operationParameterInfo.Position : maxPosition;
                    }

                    if (parameterIndexs.Contains(operationParameterInfo.Position))
                    {
                        duplicatedPositions = true;
                        break;
                    }
                    else
                    {
                        parameterIndexs.Add(operationParameterInfo.Position);
                    }
                }

                if (duplicatedPositions ||
                    maxPosition > (operationInfo.Parameters.Count - (hasReturnValue ? 2 : 1)))
                {
                    return;
                }

                ContractType contract = types[operationInfo.ContractName];
                ContractMethodInfo methodInfo = new ContractMethodInfo(contract, operationInfo);
            };

            walker.Walk(contextActivity);

            return types;
        }
    }
}
