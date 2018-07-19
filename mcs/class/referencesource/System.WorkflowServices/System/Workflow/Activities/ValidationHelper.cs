//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Activities
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;
    using System.Globalization;

    static class ValidationHelper
    {
        internal static bool IsValidTypeNameOrIdentifier(string value, bool isTypeName)
        {
            bool nextMustBeStartChar = true;
            bool previousWasNamespaceSeparatorChar = false;

            if (value.Length == 0)
            {
                return false;
            }

            // each char must be Lu, Ll, Lt, Lm, Lo, Nd, Mn, Mc, Pc
            // 
            for (int i = 0; i < value.Length; i++)
            {
                char ch = value[i];
                UnicodeCategory uc = Char.GetUnicodeCategory(ch);
                switch (uc)
                {
                    case UnicodeCategory.UppercaseLetter: // Lu
                    case UnicodeCategory.LowercaseLetter: // Ll
                    case UnicodeCategory.TitlecaseLetter: // Lt
                    case UnicodeCategory.ModifierLetter: // Lm
                    case UnicodeCategory.LetterNumber: // Lm
                    case UnicodeCategory.OtherLetter: // Lo
                        {
                            nextMustBeStartChar = false;
                            previousWasNamespaceSeparatorChar = false;
                            break;
                        }

                    case UnicodeCategory.NonSpacingMark: // Mn
                    case UnicodeCategory.SpacingCombiningMark: // Mc
                    case UnicodeCategory.ConnectorPunctuation: // Pc
                    case UnicodeCategory.DecimalDigitNumber: // Nd
                        {
                            // Underscore is a valid starting character, even though it is a ConnectorPunctuation.
                            if (nextMustBeStartChar && ch != '_')
                            {
                                return false;
                            }

                            nextMustBeStartChar = false;
                            previousWasNamespaceSeparatorChar = false;
                            break;
                        }

                    default:
                        {
                            // We only check the special Type chars for type names. 
                            if (isTypeName && !nextMustBeStartChar && !previousWasNamespaceSeparatorChar && IsNamespaceSeparatorChar(ch, ref nextMustBeStartChar))
                            {
                                previousWasNamespaceSeparatorChar = true;
                                break;
                            }

                            return false;
                        }
                }
            }

            if (isTypeName && previousWasNamespaceSeparatorChar && nextMustBeStartChar)
            {
                return false;
            }

            return true;
        }

        internal static ValidationErrorCollection ValidateAllServiceOperationsImplemented(
            ValidationManager manager,
            Activity rootActivity)
        {
            if (manager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("manager");
            }

            if (rootActivity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rootActivity");
            }

            ValidationErrorCollection validationErrors = new ValidationErrorCollection();

            WorkflowServiceAttributes serviceAttributes = (WorkflowServiceAttributes) ReceiveActivity.GetWorkflowServiceAttributes(rootActivity);
            if (serviceAttributes != null)
            {
                if (serviceAttributes.MaxItemsInObjectGraph < 0)
                {
                    validationErrors.Add(new ValidationError(
                        SR2.GetString(
                        SR2.Error_Validation_InvalidMaxItemsInObjectGraph),
                        WorkflowServicesErrorNumbers.Error_InvalidMaxItemsInObjectGraph,
                        false,
                        "WorkflowServiceAttributes"));
                }
            }

            // verifying that we do not have two contract with the same full name
            // one contract first
            // one workflow first
            //

            Dictionary<string, string> implementedTypedContracts = new Dictionary<string, string>();
            Dictionary<string, string> implementedInferredContracts = new Dictionary<string, string>();

            foreach (ReceiveActivity receiveActivity in GetActivities<ReceiveActivity>(rootActivity))
            {
                if (receiveActivity.ServiceOperationInfo == null)
                {
                    continue;
                }

                TypedOperationInfo typedServiceOperation =
                    receiveActivity.ServiceOperationInfo as TypedOperationInfo;
                OperationInfo inferredServiceOperation =
                    receiveActivity.ServiceOperationInfo as OperationInfo;

                string typeName = string.Empty;

                Dictionary<string, string> toAddTo = implementedInferredContracts;
                Dictionary<string, string> toVerify = implementedTypedContracts;

                if (typedServiceOperation != null)
                {
                    if (typedServiceOperation.ContractType != null)
                    {
                        typeName = typedServiceOperation.ContractType.FullName;

                        toAddTo = implementedTypedContracts;
                        toVerify = implementedInferredContracts;
                    }
                }
                else
                {
                    typeName = inferredServiceOperation.ContractName;
                }

                if (string.IsNullOrEmpty(typeName))
                {
                    continue;
                }

                if (toVerify.ContainsKey(typeName) && !toAddTo.ContainsKey(typeName))
                {
                    validationErrors.Add(new ValidationError(
                        SR2.GetString(SR2.Error_Validation_ContractNameDuplicate, typeName),
                        WorkflowServicesErrorNumbers.Error_ContractNameDuplicate, false));
                }

                if (!toAddTo.ContainsKey(typeName))
                {
                    toAddTo.Add(typeName, typeName);
                }
            }

            // collect operations that are implemented
            //

            Dictionary<Type, Hashtable> implementedServiceOperations = new Dictionary<Type, Hashtable>();
            foreach (ReceiveActivity receiveActivity in GetActivities<ReceiveActivity>(rootActivity))
            {
                OperationInfoBase serviceOperation = receiveActivity.ServiceOperationInfo;
                if (serviceOperation != null)
                {
                    Type contractType = serviceOperation.GetContractType(manager);

                    if (contractType == null)
                    {
                        continue;
                    }

                    if (!implementedServiceOperations.ContainsKey(contractType))
                    {
                        Hashtable serviceOperationHashTable = new Hashtable();
                        implementedServiceOperations.Add(contractType, serviceOperationHashTable);
                    }

                    MethodInfo methodInfo = serviceOperation.GetMethodInfo(manager);
                    if (methodInfo == null)
                    {
                        continue;
                    }

                    contractType = methodInfo.DeclaringType;

                    if (!implementedServiceOperations.ContainsKey(contractType))
                    {
                        Hashtable serviceOperationHashTable = new Hashtable();
                        serviceOperationHashTable.Add(serviceOperation.Name, methodInfo);
                        implementedServiceOperations.Add(contractType, serviceOperationHashTable);
                    }
                    else
                    {
                        if (!implementedServiceOperations[contractType].ContainsKey(serviceOperation.Name))
                        {
                            implementedServiceOperations[contractType].Add(
                                serviceOperation.Name,
                                methodInfo);
                        }
                    }
                }
            }

            // verifying that we do not have one two methods defining the same operation
            // verify which operatinos are not implemented and give warnings for each
            //

            Dictionary<Type, bool> checkedContracts = new Dictionary<Type, bool>();
            Dictionary<Type, Hashtable> notImplementedServiceOperations = new Dictionary<Type, Hashtable>();

            foreach (Type contractType in implementedServiceOperations.Keys)
            {
                Queue<Type> interfacesQueue = new Queue<Type>();
                List<Type> contractList = new List<Type>();

                interfacesQueue.Enqueue(contractType);

                while (interfacesQueue.Count > 0)
                {
                    Type currentInterfaceType = interfacesQueue.Dequeue();
                    if (!contractList.Contains(currentInterfaceType) &&
                        currentInterfaceType.IsDefined(typeof(ServiceContractAttribute), false))
                    {
                        contractList.Add(currentInterfaceType);
                    }

                    foreach (Type baseInteface in currentInterfaceType.GetInterfaces())
                    {
                        if (!contractList.Contains(baseInteface))
                        {
                            interfacesQueue.Enqueue(baseInteface);
                        }
                    }
                }

                foreach (Type currentContractType in contractList)
                {
                    if (checkedContracts.ContainsKey(currentContractType))
                    {
                        continue;
                    }

                    foreach (MethodInfo methodInfo in currentContractType.GetMethods())
                    {
                        if (methodInfo.DeclaringType != currentContractType)
                        {
                            continue;
                        }
                        if (!ServiceOperationHelpers.IsValidServiceOperation(methodInfo))
                        {
                            continue;
                        }

                        string operationName = ServiceOperationHelpers.GetOperationName(manager, methodInfo);

                        if (!implementedServiceOperations.ContainsKey(currentContractType))
                        {
                            validationErrors.Add(new ValidationError(
                                SR2.GetString(
                                SR2.Error_OperationNotImplemented,
                                operationName,
                                currentContractType.FullName),
                                WorkflowServicesErrorNumbers.Error_OperationNotImplemented,
                                true));

                            if (!notImplementedServiceOperations.ContainsKey(currentContractType))
                            {
                                Hashtable serviceOperationHashTable = new Hashtable();
                                serviceOperationHashTable.Add(operationName, methodInfo);
                                notImplementedServiceOperations.Add(currentContractType, serviceOperationHashTable);
                            }
                            else if (notImplementedServiceOperations[currentContractType].ContainsKey(operationName))
                            {
                                validationErrors.Add(new ValidationError(
                                    SR2.GetString(
                                    SR2.Error_DuplicatedOperationName,
                                    methodInfo.Name,
                                    ((MethodInfo) notImplementedServiceOperations[currentContractType][operationName]).Name,
                                    currentContractType.FullName),
                                    WorkflowServicesErrorNumbers.Error_DuplicatedOperationName,
                                    false));
                            }
                            else
                            {
                                notImplementedServiceOperations[currentContractType].Add(
                                    operationName,
                                    methodInfo);
                            }
                        }
                        else if (!implementedServiceOperations[currentContractType].ContainsKey(operationName))
                        {
                            validationErrors.Add(new ValidationError(
                                SR2.GetString(
                                SR2.Error_OperationNotImplemented,
                                operationName,
                                currentContractType.FullName),
                                WorkflowServicesErrorNumbers.Error_OperationNotImplemented,
                                true));

                            if (!notImplementedServiceOperations.ContainsKey(currentContractType))
                            {
                                Hashtable serviceOperationHashTable = new Hashtable();
                                serviceOperationHashTable.Add(operationName, methodInfo);
                                notImplementedServiceOperations.Add(currentContractType, serviceOperationHashTable);
                            }
                            else if (notImplementedServiceOperations[currentContractType].ContainsKey(operationName))
                            {
                                validationErrors.Add(new ValidationError(
                                    SR2.GetString(
                                    SR2.Error_DuplicatedOperationName,
                                    methodInfo.Name,
                                    ((MethodInfo) notImplementedServiceOperations[currentContractType][operationName]).Name,
                                    currentContractType.FullName),
                                    WorkflowServicesErrorNumbers.Error_DuplicatedOperationName,
                                    false));
                            }
                            else
                            {
                                notImplementedServiceOperations[currentContractType].Add(
                                    operationName,
                                    methodInfo);
                            }
                        }
                        else if (implementedServiceOperations[currentContractType][operationName] != (object)methodInfo)
                        {
                            validationErrors.Add(new ValidationError(
                                SR2.GetString(
                                SR2.Error_DuplicatedOperationName,
                                methodInfo.Name,
                                ((MethodInfo) implementedServiceOperations[currentContractType][operationName]).Name,
                                currentContractType.FullName),
                                WorkflowServicesErrorNumbers.Error_DuplicatedOperationName,
                                false));
                        }
                    }

                    checkedContracts.Add(currentContractType, true);
                }

                if (validationErrors.Count != 0)
                {
                    break;
                }
            }

            if (manager.Context[typeof(ServiceOperationsImplementedValidationMarker)] == null)
            {
                manager.Context.Append(new ServiceOperationsImplementedValidationMarker());
            }

            return validationErrors;
        }

        internal static ValidationErrorCollection ValidateChannelToken(
            SendActivity activity,
            ValidationManager manager)
        {
            if (activity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("activity");
            }
            if (manager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("manager");
            }

            ValidationErrorCollection validationErrors = new ValidationErrorCollection();

            // validation rules:
            // 
            // endpoint cannot be null
            //
            // Name: !(null ||empty)
            // OwnerActivityName: any
            // ConfigurationName: !(null ||empty) || bound
            //

            ChannelToken endpoint = activity.ChannelToken;

            if (endpoint == null)
            {
                validationErrors.Add(
                    new ValidationError(SR2.GetString(SR2.Error_Validation_ChannelTokenNotSpecified, activity.Name),
                    WorkflowServicesErrorNumbers.Error_ChannelTokenNotSpecified,
                    false,
                    "ChannelToken"));
            }
            else
            {
                if (string.IsNullOrEmpty(endpoint.Name))
                {
                    validationErrors.Add(
                        new ValidationError(SR2.GetString(SR2.Error_Validation_ChannelTokenNameNotSpecified, activity.Name),
                        WorkflowServicesErrorNumbers.Error_ChannelTokenNameNotSpecified,
                        false,
                        "ChannelToken"));
                }

                if (string.IsNullOrEmpty(endpoint.EndpointName) &&
                    !endpoint.IsBindingSet(ChannelToken.EndpointNameProperty))
                {
                    validationErrors.Add(
                        new ValidationError(SR2.GetString(SR2.Error_Validation_ChannelTokenConfigurationNameNotSpecified, activity.Name),
                        WorkflowServicesErrorNumbers.Error_ChannelTokenConfigurationNameNotSpecified,
                        false,
                        "ChannelToken"));
                }

                if (!string.IsNullOrEmpty(endpoint.OwnerActivityName))
                {
                    string qualifiedOwnerName = null;
                    Activity sourceActivity = activity.GetActivityByName(endpoint.OwnerActivityName);
                    if (sourceActivity == null)
                    {
                        sourceActivity = Helpers.ParseActivityForBind(activity, endpoint.OwnerActivityName);
                    }
                    if (sourceActivity != null)
                    {
                        qualifiedOwnerName = sourceActivity.QualifiedName;
                    }

                    Activity replicatorParent = null;
                    Activity parent = activity;
                    bool ownerIsParentOrItself = false;
                    while (parent != null)
                    {
                        // We hardcode Replicator here, not MultiInstance | Concurrent.
                        if (parent is ReplicatorActivity && replicatorParent == null)
                        {
                            replicatorParent = parent;
                        }

                        if (qualifiedOwnerName == parent.QualifiedName)
                        {
                            ownerIsParentOrItself = true;
                        }

                        parent = parent.Parent;

                    }

                    if (!ownerIsParentOrItself)
                    {
                        validationErrors.Add(
                            new ValidationError(SR2.GetString(SR2.Error_Validation_OwnerActivityNameNotFound, endpoint.OwnerActivityName, activity.Name),
                            WorkflowServicesErrorNumbers.Error_OwnerActivityNameNotFound,
                            false,
                            "ChannelToken"));
                    }
                }
            }

            return validationErrors;
        }

        internal static ValidationErrorCollection ValidateContextToken(
            Activity activity,
            ContextToken contextToken,
            ValidationManager manager)
        {
            if (activity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("activity");
            }
            if (manager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("manager");
            }

            ValidationErrorCollection validationErrors = new ValidationErrorCollection();

            // having no context token is valid
            // this means "use RootContext" which is the implicit one.
            //
            if (contextToken == null)
            {
                return validationErrors;
            }

            // validation rules:
            // 
            // root context:
            //  Name: (RootContext)
            //  OwnerActivityName: null || emtpy
            //
            // non root context:
            //  Name: !(RootContext) && !(null ||empty)
            //  OwnerActivityName: any
            //

            if (string.IsNullOrEmpty(contextToken.Name))
            {
                validationErrors.Add(
                    new ValidationError(SR2.GetString(SR2.Error_Validation_ContextTokenNameNotSpecified, activity.Name),
                    WorkflowServicesErrorNumbers.Error_ContextTokenNameNotSpecified,
                    false,
                    "ContextToken"));
            }

            if (string.Compare(contextToken.Name, ContextToken.RootContextName, StringComparison.Ordinal) == 0)
            {
                if (!string.IsNullOrEmpty(contextToken.OwnerActivityName))
                {
                    validationErrors.Add(
                        new ValidationError(SR2.GetString(SR2.Error_Validation_RootContextScope, activity.Name),
                        WorkflowServicesErrorNumbers.Error_RootContextScope,
                        false,
                        "ContextToken"));
                }
            }
            else if (!string.IsNullOrEmpty(contextToken.OwnerActivityName))
            {
                string qualifiedOwnerName = null;
                Activity sourceActivity = activity.GetActivityByName(contextToken.OwnerActivityName);
                if (sourceActivity == null)
                {
                    sourceActivity = Helpers.ParseActivityForBind(activity, contextToken.OwnerActivityName);
                }
                if (sourceActivity != null)
                {
                    qualifiedOwnerName = sourceActivity.QualifiedName;
                }

                Activity replicatorParent = null;
                Activity parent = activity;
                bool ownerIsParent = false;
                while (parent != null)
                {
                    // We hardcode Replicator here, not MultiInstance | Concurrent.
                    if (parent is ReplicatorActivity && replicatorParent == null)
                    {
                        replicatorParent = parent;
                    }

                    if (qualifiedOwnerName == parent.QualifiedName)
                    {
                        ownerIsParent = true;
                    }

                    parent = parent.Parent;

                }

                if (!ownerIsParent)
                {
                    validationErrors.Add(
                        new ValidationError(SR2.GetString(SR2.Error_Validation_OwnerActivityNameNotFound, contextToken.OwnerActivityName, activity.Name),
                        WorkflowServicesErrorNumbers.Error_OwnerActivityNameNotFound,
                        false,
                        "ContextToken"));
                }
            }

            return validationErrors;
        }

        internal static ValidationErrorCollection ValidateOperationInfo(
            Activity activity,
            OperationInfoBase operationInfo,
            ValidationManager manager)
        {
            if (activity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("activity");
            }
            if (operationInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("operationInfo");
            }
            if (manager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("manager");
            }

            ITypeProvider typeProvider = manager.GetService(typeof(ITypeProvider)) as ITypeProvider;
            if (typeProvider == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR2.GetString(SR2.General_MissingService, typeof(ITypeProvider).Name)));
            }

            ValidationErrorCollection validationErrors = new ValidationErrorCollection();

            if (string.IsNullOrEmpty(operationInfo.Name))
            {
                validationErrors.Add(
                    new ValidationError(SR2.GetString(SR2.Error_Validation_OperationNameNotSpecified, activity.Name),
                    WorkflowServicesErrorNumbers.Error_OperationNameNotSpecified, false, "ServiceOperationInfo"));
            }

            if (operationInfo is OperationInfo)
            {
                OperationInfo currentOperationInfo = operationInfo as OperationInfo;

                if (!string.IsNullOrEmpty(currentOperationInfo.Name) &&
                    !IsValidTypeNameOrIdentifier(currentOperationInfo.Name, false))
                {
                    validationErrors.Add(
                        new ValidationError(SR2.GetString(SR2.Error_Validation_OperationNameInvalid, activity.Name),
                        WorkflowServicesErrorNumbers.Error_OperationNameInvalid, false, "ServiceOperationInfo"));
                }

                if (string.IsNullOrEmpty(currentOperationInfo.ContractName))
                {
                    validationErrors.Add(
                        new ValidationError(SR2.GetString(SR2.Error_Validation_ContractNameNotSpecified, activity.Name),
                        WorkflowServicesErrorNumbers.Error_ContractNameNotSpecified,
                        false,
                        "ServiceOperationInfo"));
                }
                else if (!IsValidTypeNameOrIdentifier(currentOperationInfo.ContractName, true))
                {
                    validationErrors.Add(
                        new ValidationError(SR2.GetString(SR2.Error_Validation_ContractNameInvalid, activity.Name),
                        WorkflowServicesErrorNumbers.Error_ContractNameInvalid,
                        false,
                        "ServiceOperationInfo"));
                }

                bool hasReturnValue = false;
                foreach (OperationParameterInfo operationParameterInfo in currentOperationInfo.Parameters)
                {
                    if (operationParameterInfo.Position == -1)
                    {
                        hasReturnValue = true;
                        break;
                    }
                }

                int maxPosition = currentOperationInfo.Parameters.Count - (hasReturnValue ? 2 : 1);
                List<int> parameterIndexs = new List<int>();
                List<string> parameterNames = new List<string>();

                foreach (OperationParameterInfo operationParameterInfo in currentOperationInfo.Parameters)
                {
                    if (operationParameterInfo.Position != -1 && operationParameterInfo.Position > maxPosition)
                    {
                        validationErrors.Add(
                            new ValidationError(SR2.GetString(SR2.Error_Validation_OperationParameterPosition,
                            operationParameterInfo.Name, currentOperationInfo.Name, currentOperationInfo.ContractName),
                            WorkflowServicesErrorNumbers.Error_OperationParameterPosition,
                            false,
                            "ServiceOperationInfo"));
                    }

                    if (parameterIndexs.Contains(operationParameterInfo.Position))
                    {
                        validationErrors.Add(
                            new ValidationError(SR2.GetString(SR2.Error_Validation_OperationParameterPositionDuplicate,
                            operationParameterInfo.Name, currentOperationInfo.Name, currentOperationInfo.ContractName),
                            WorkflowServicesErrorNumbers.Error_OperationParameterPositionDuplicate,
                            false,
                            "ServiceOperationInfo"));
                    }
                    else
                    {
                        parameterIndexs.Add(operationParameterInfo.Position);
                    }

                    if (operationParameterInfo.Position != -1 && !IsValidTypeNameOrIdentifier(operationParameterInfo.Name, false))
                    {
                        validationErrors.Add(
                            new ValidationError(SR2.GetString(SR2.Error_Validation_OperationParameterNameInvalid,
                            operationParameterInfo.Name, currentOperationInfo.Name, currentOperationInfo.ContractName),
                            WorkflowServicesErrorNumbers.Error_OperationParameterNameInvalid,
                            false,
                            "ServiceOperationInfo"));
                    }

                    if (parameterNames.Contains(operationParameterInfo.Name))
                    {
                        validationErrors.Add(
                            new ValidationError(SR2.GetString(SR2.Error_Validation_OperationParameterNameDuplicate,
                            operationParameterInfo.Name, currentOperationInfo.Name, currentOperationInfo.ContractName),
                            WorkflowServicesErrorNumbers.Error_OperationParameterNameDuplicate,
                            false,
                            "ServiceOperationInfo"));
                    }
                    else
                    {
                        parameterNames.Add(operationParameterInfo.Name);
                    }

                    if (operationParameterInfo.Position != -1 && operationParameterInfo.ParameterType == typeof(void))
                    {
                        validationErrors.Add(
                            new ValidationError(SR2.GetString(SR2.Error_Validation_OperationParameterType,
                            operationParameterInfo.Name, currentOperationInfo.Name, currentOperationInfo.ContractName),
                            WorkflowServicesErrorNumbers.Error_OperationParameterType,
                            false,
                            "ServiceOperationInfo"));
                    }
                }
            }
            else
            {
                TypedOperationInfo currentOperationInfo = operationInfo as TypedOperationInfo;
                if (currentOperationInfo.ContractType == null)
                {
                    validationErrors.Add(
                        new ValidationError(SR2.GetString(SR2.Error_Validation_ContractTypeNotSpecified, activity.Name),
                        WorkflowServicesErrorNumbers.Error_ContractTypeNotSpecified,
                        false,
                        "ServiceOperationInfo"));
                }
            }

            // no point validating further as we will not be able to get to the contract type or operation
            //
            if (validationErrors.Count > 0)
            {
                return validationErrors;
            }

            Type contractType = operationInfo.GetContractType(manager);
            if (contractType == null)
            {
                validationErrors.Add(
                    new ValidationError(SR2.GetString(SR2.Error_Validation_ContractTypeNotFound, activity.Name),
                    WorkflowServicesErrorNumbers.Error_ContractTypeNotFound,
                    false,
                    "ServiceOperationInfo"));
            }
            else if (!contractType.IsInterface)
            {
                validationErrors.Add(
                    new ValidationError(SR2.GetString(SR2.Error_Validation_ContractTypeNotInterface, contractType.FullName, activity.Name),
                    WorkflowServicesErrorNumbers.Error_ContractTypeNotInterface,
                    false,
                    "ServiceOperationInfo"));
            }
            else if (!ServiceOperationHelpers.IsValidServiceContract(contractType))
            {
                validationErrors.Add(
                    new ValidationError(SR2.GetString(SR2.Error_ServiceContractAttributeMissing, contractType.FullName),
                    WorkflowServicesErrorNumbers.Error_ServiceContractAttributeMissing,
                    false,
                    "ServiceOperationInfo"));
            }
            else
            {
                MethodInfo methodInfo = operationInfo.GetMethodInfo(manager);
                if (methodInfo == null)
                {
                    validationErrors.Add(
                        new ValidationError(SR2.GetString(SR2.Error_Validation_OperationNotInContract, operationInfo.Name, contractType.FullName),
                        WorkflowServicesErrorNumbers.Error_OperationNotInContract,
                        false,
                        "ServiceOperationInfo"));
                }
                else if (ServiceOperationHelpers.IsAsyncOperation(manager, methodInfo))
                {
                    validationErrors.Add(
                        new ValidationError(SR2.GetString(SR2.Error_Validation_AsyncPatternOperationNotSupported, operationInfo.Name),
                        WorkflowServicesErrorNumbers.Error_AsyncPatternOperationNotSupported,
                        false,
                        "ServiceOperationInfo"));
                }
                else
                {
                    List<int> parameterIndexs = new List<int>();
                    List<string> parameterNames = new List<string>();

                    bool isOneWay = operationInfo.GetIsOneWay(manager);

                    if (isOneWay && methodInfo.ReturnType != typeof(void))
                    {
                        validationErrors.Add(
                            new ValidationError(SR2.GetString(SR2.Error_Validation_ReturnTypeInOneWayOperation,
                            operationInfo.Name, contractType.FullName),
                            WorkflowServicesErrorNumbers.Error_ReturnTypeInOneWayOperation,
                            false,
                            "ServiceOperationInfo"));
                    }

                    foreach (ParameterInfo parameter in methodInfo.GetParameters())
                    {
                        if (parameter.Position >= methodInfo.GetParameters().Length ||
                            parameter.Position < 0)
                        {
                            validationErrors.Add(
                                new ValidationError(SR2.GetString(SR2.Error_Validation_OperationParameterPosition,
                                parameter.Name, operationInfo.Name, contractType.FullName),
                                WorkflowServicesErrorNumbers.Error_OperationParameterPosition,
                                false,
                                "ServiceOperationInfo"));
                        }

                        if (parameterIndexs.Contains(parameter.Position))
                        {
                            validationErrors.Add(
                                new ValidationError(SR2.GetString(SR2.Error_Validation_OperationParameterPositionDuplicate,
                                parameter.Name, operationInfo.Name, contractType.FullName),
                                WorkflowServicesErrorNumbers.Error_OperationParameterPositionDuplicate,
                                false,
                                "ServiceOperationInfo"));
                        }

                        if (!IsValidTypeNameOrIdentifier(parameter.Name, false))
                        {
                            validationErrors.Add(
                                new ValidationError(SR2.GetString(SR2.Error_Validation_OperationParameterNameInvalid,
                                parameter.Name, operationInfo.Name, contractType.FullName),
                                WorkflowServicesErrorNumbers.Error_OperationParameterNameInvalid,
                                false,
                                "ServiceOperationInfo"));
                        }

                        if (parameterNames.Contains(parameter.Name))
                        {
                            validationErrors.Add(
                                new ValidationError(SR2.GetString(SR2.Error_Validation_OperationParameterNameDuplicate,
                                parameter.Name, operationInfo.Name, contractType.FullName),
                                WorkflowServicesErrorNumbers.Error_OperationParameterNameDuplicate,
                                false,
                                "ServiceOperationInfo"));
                        }

                        if (isOneWay && ((parameter.Attributes & ParameterAttributes.Out) > 0 || parameter.ParameterType.IsByRef))
                        {
                            validationErrors.Add(
                                new ValidationError(SR2.GetString(SR2.Error_Validation_OperationParameterDirectionInOneWayOperation,
                                parameter.Name, operationInfo.Name, contractType.FullName),
                                WorkflowServicesErrorNumbers.Error_OperationParameterDirectionInOneWayOperation,
                                false,
                                "ServiceOperationInfo"));
                        }

                        parameterIndexs.Add(parameter.Position);
                        parameterNames.Add(parameter.Name);
                    }

                    validationErrors.AddRange(
                        ValidationHelper.ValidateServiceModelAttributes(activity, contractType, methodInfo, manager));
                }


            }

            return validationErrors;
        }

        internal static IEnumerable<ValidationError> ValidateParameterBindings(
            Activity ownerActivity,
            OperationInfoBase operationInfo,
            WorkflowParameterBindingCollection parameterBindings,
            ValidationManager manager)
        {
            ValidationErrorCollection validationErrors = new ValidationErrorCollection();

            if (ownerActivity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("ownerActivity");
            }
            if (operationInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("operationInfo");
            }
            if (parameterBindings == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameterBindings");
            }
            if (manager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("manager");
            }

            MethodInfo methodInfo = operationInfo.GetMethodInfo(manager);
            if (methodInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("operationInfo",
                    SR2.GetString(SR2.Error_MethodInfoNotAvailable, ownerActivity.Name));
            }

            bool isOneWayOperation = operationInfo.GetIsOneWay(manager);

            foreach (ParameterInfo parameter in GetParameterInfo(methodInfo))
            {
                if (!(parameter.IsOut || parameter.Position == -1) || !isOneWayOperation)
                {
                    string parameterName = (parameter.Position == -1) ? "(ReturnValue)" : parameter.Name;
                    object parameterValue = null;

                    if (parameterBindings.Contains(parameterName))
                    {
                        if (parameterBindings[parameterName].IsBindingSet(WorkflowParameterBinding.ValueProperty))
                        {
                            parameterValue = parameterBindings[parameterName].GetBinding(
                                WorkflowParameterBinding.ValueProperty);
                        }
                        else
                        {
                            parameterValue = parameterBindings[parameterName].GetValue(
                                WorkflowParameterBinding.ValueProperty);
                        }

                        if (parameterValue != null)
                        {
                            // Check for access type of the binding.
                            AccessTypes requiredAccess = AccessTypes.Read;
                            if (parameter.IsOut ||
                                parameter.ParameterType.IsByRef ||
                                parameter.Position == -1)
                            {
                                requiredAccess |= AccessTypes.Write;
                            }

                            PropertyValidationContext propertyValidationContext =
                                new PropertyValidationContext(parameterBindings[parameterName],
                                null,
                                parameterName);
                            BindValidationContext bindValidationContext = new BindValidationContext(
                                parameter.ParameterType.IsByRef ? parameter.ParameterType.GetElementType() : parameter.ParameterType,
                                requiredAccess);

                            validationErrors.AddRange(ValidationHelpers.ValidateProperty(
                                manager,
                                ownerActivity,
                                parameterValue,
                                propertyValidationContext,
                                bindValidationContext));
                        }
                    }

                    if (!parameterBindings.Contains(parameterName) ||
                        parameterValue == null)
                    {
                        if (ownerActivity is SendActivity)
                        {
                            if (parameter.Position != -1)
                            {
                                validationErrors.Add(new ValidationError(SR2.GetString(
                                    SR2.Warning_SendActivityParameterBindingMissing,
                                    parameterName),
                                    WorkflowServicesErrorNumbers.Warning_SendActivityParameterBindingMissing,
                                    true,
                                    parameterName));
                            }
                        }
                        else if (ownerActivity is ReceiveActivity)
                        {
                            if (parameter.Position == -1)
                            {
                                validationErrors.Add(new ValidationError(SR2.GetString(
                                    SR2.Warning_ReceiveActivityReturnValueBindingMissing,
                                    parameterName),
                                    WorkflowServicesErrorNumbers.Warning_ReceiveActivityReturnValueBindingMissing,
                                    true,
                                    parameterName));
                            }
                            else
                            {
                                validationErrors.Add(new ValidationError(SR2.GetString(
                                    SR2.Warning_ReceiveActivityParameterBindingMissing,
                                    parameterName),
                                    WorkflowServicesErrorNumbers.Warning_ReceiveActivityParameterBindingMissing,
                                    true,
                                    parameterName));
                            }
                        }
                    }
                }
            }
            return validationErrors;

        }

        internal static IEnumerable<ValidationError> ValidateServiceModelAttributes(
            Activity activity,
            Type contractType,
            MethodInfo methodInfo,
            ValidationManager manager)
        {
            if (activity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("activity");
            }

            if (contractType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contractType");
            }

            if (methodInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("methodInfo");
            }

            if (manager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("manager");
            }

            ValidationErrorCollection validationErrors = new ValidationErrorCollection();
            object[] serviceContractAttributes = contractType.GetCustomAttributes(typeof(ServiceContractAttribute), true);
            if (serviceContractAttributes == null || serviceContractAttributes.Length == 0)
            {
                validationErrors.Add(new ValidationError(SR2.GetString(SR2.Error_ServiceContractAttributeMissing, contractType.FullName), WorkflowServicesErrorNumbers.Error_ServiceContractAttributeMissing, false, "ServiceOperationInfo"));
            }
            else
            {
                if (!methodInfo.ReflectedType.IsAssignableFrom(contractType))
                {
                    validationErrors.Add(new ValidationError(SR2.GetString(SR2.Error_OperationNotInContract, methodInfo.Name, contractType.FullName), WorkflowServicesErrorNumbers.Error_OperationNotInContract, false, "ServiceOperationInfo"));
                }
                else
                {
                    object[] operationContractAttrributes = methodInfo.GetCustomAttributes(typeof(OperationContractAttribute), true);
                    if (operationContractAttrributes == null || operationContractAttrributes.Length == 0)
                    {
                        validationErrors.Add(new ValidationError(SR2.GetString(SR2.Error_OperationContractAttributeMissing, methodInfo.Name), WorkflowServicesErrorNumbers.Error_OperationContractAttributeMissing, false, "ServiceOperationInfo"));
                    }
                    else if (activity is ReceiveActivity)
                    {
                        ReceiveActivity receiveActivity = activity as ReceiveActivity;

                        SessionMode contractSessionMode = SessionMode.Allowed;
                        if (serviceContractAttributes[0] is ServiceContractAttribute)
                        {
                            contractSessionMode = ((ServiceContractAttribute) serviceContractAttributes[0]).SessionMode;
                        }
                        else if (serviceContractAttributes[0] is AttributeInfoAttribute)
                        {
                            AttributeInfoAttribute attribInfoAttrib = serviceContractAttributes[0] as AttributeInfoAttribute;
                            if (typeof(ServiceContractAttribute).IsAssignableFrom(attribInfoAttrib.AttributeInfo.AttributeType))
                            {
                                contractSessionMode = ServiceOperationHelpers.GetContractSessionMode(manager, attribInfoAttrib.AttributeInfo);
                            }
                        }

                        if (receiveActivity.CanCreateInstance == true &&
                            receiveActivity.ServiceOperationInfo.GetIsOneWay(manager) &&
                            contractSessionMode != SessionMode.NotAllowed)
                        {
                            validationErrors.Add(new ValidationError(SR2.GetString(SR2.Error_Validation_OperationIsOneWay, methodInfo.Name), WorkflowServicesErrorNumbers.Error_OperationIsOneWay, false, "CanCreateInstance"));
                        }
                        if (receiveActivity.CanCreateInstance == true && !ServiceOperationHelpers.IsInitiatingOperation(manager, methodInfo))
                        {
                            validationErrors.Add(new ValidationError(SR2.GetString(SR2.Error_OperationNotInitiating, methodInfo.Name), WorkflowServicesErrorNumbers.Error_OperationNotInitiating, false, "CanCreateInstance"));
                        }
                    }
                }
            }
            return validationErrors;
        }

        private static IEnumerable GetActivities<T>(Activity rootActivity)
        {
            if (rootActivity == null || !rootActivity.Enabled)
            {
                yield break;
            }

            if (rootActivity is CompositeActivity)
            {
                foreach (Activity activity in ((CompositeActivity) rootActivity).Activities)
                {
                    if (!activity.Enabled)
                    {
                        continue;
                    }

                    if (activity.GetType() == typeof(T))
                    {
                        yield return activity;
                    }

                    if (activity is CompositeActivity)
                    {
                        foreach (T requestedType in GetActivities<T>(activity))
                        {
                            yield return requestedType;
                        }
                    }
                }
            }
            else
            {
                if (rootActivity.GetType() == typeof(T))
                {
                    yield return rootActivity;
                }
            }
            yield break;
        }

        private static List<ParameterInfo> GetParameterInfo(MethodInfo methodInfo)
        {
            List<ParameterInfo> parametersInfo = new List<ParameterInfo>();
            parametersInfo.AddRange(methodInfo.GetParameters());

            if (methodInfo.ReturnParameter != null && methodInfo.ReturnType != typeof(void))
            {
                parametersInfo.Add(methodInfo.ReturnParameter);
            }

            return parametersInfo;
        }

        private static bool IsNamespaceSeparatorChar(char ch, ref bool nextMustBeStartChar)
        {
            switch (ch)
            {
                case '.':
                    nextMustBeStartChar = true;
                    return true;
            }
            return false;
        }
    }
}
