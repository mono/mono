#region Imports

using System;
using System.Reflection;
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ComponentModel.Design;
using System.Collections.Specialized;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.ComponentModel.Compiler;
using System.ComponentModel.Design.Serialization;
using System.CodeDom;
using System.Globalization;
using System.Workflow.Runtime;
using System.Workflow.Activities.Common;

#endregion

namespace System.Workflow.Activities
{
    internal static class CorrelationSetsValidator
    {
        internal static ValidationErrorCollection Validate(ValidationManager manager, Object obj)
        {
            ValidationErrorCollection validationErrors = new ValidationErrorCollection();

            Activity activity = obj as Activity;
            if (!(activity is CallExternalMethodActivity) && !(activity is HandleExternalEventActivity))
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(Activity).FullName), "obj");

            Type interfaceType = (activity is CallExternalMethodActivity) ? ((CallExternalMethodActivity)activity).InterfaceType : ((HandleExternalEventActivity)activity).InterfaceType;
            if (interfaceType == null)
                return validationErrors;

            if (interfaceType.ContainsGenericParameters)
            {
                ValidationError error = new ValidationError(string.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Error_GenericMethodsNotSupported), interfaceType.FullName), ErrorNumbers.Error_GenericMethodsNotSupported);
                error.PropertyName = "InterfaceType";
                validationErrors.Add(error);
                return validationErrors;
            }

            object[] attributes = interfaceType.GetCustomAttributes(typeof(ExternalDataExchangeAttribute), false);
            if (attributes.Length == 0)
            {
                ValidationError error = new ValidationError(string.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Error_ExternalDataExchangeException), interfaceType.FullName), ErrorNumbers.Error_TypeNotExist);
                error.PropertyName = "InterfaceType";
                validationErrors.Add(error);
                return validationErrors;
            }

            if (activity.Site == null)
            {
                ValidationErrorCollection interfaceErrors = ValidateHostInterface(manager, interfaceType, activity);
                if (interfaceErrors.Count != 0)
                {
                    validationErrors.AddRange(interfaceErrors);
                    return validationErrors;
                }
            }

            MemberInfo targetMember = null;
            if (activity is CallExternalMethodActivity)
            {
                if (((CallExternalMethodActivity)activity).MethodName == null || ((CallExternalMethodActivity)activity).MethodName.Length == 0)
                    return validationErrors;

                MethodInfo methodInfo = interfaceType.GetMethod(((CallExternalMethodActivity)activity).MethodName, BindingFlags.Instance | BindingFlags.Public);
                if (methodInfo == null || methodInfo.IsSpecialName)
                {
                    validationErrors.Add(new ValidationError(SR.GetString(SR.Error_MissingMethodName, activity.Name, ((CallExternalMethodActivity)activity).MethodName), ErrorNumbers.Error_MissingMethodName));
                    return validationErrors;
                }
                if (methodInfo.ContainsGenericParameters)
                {
                    ValidationError error = new ValidationError(string.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Error_GenericMethodsNotSupported), methodInfo.Name), ErrorNumbers.Error_GenericMethodsNotSupported);
                    error.PropertyName = "MethodName";
                    validationErrors.Add(error);
                    return validationErrors;
                }
                targetMember = methodInfo;
            }
            else
            {
                if (((HandleExternalEventActivity)activity).EventName == null || ((HandleExternalEventActivity)activity).EventName.Length == 0)
                    return validationErrors;

                EventInfo eventInfo = interfaceType.GetEvent(((HandleExternalEventActivity)activity).EventName, BindingFlags.Instance | BindingFlags.Public);
                if (eventInfo == null)
                {
                    validationErrors.Add(new ValidationError(SR.GetString(SR.Error_MissingEventName, activity.Name, ((HandleExternalEventActivity)activity).EventName), ErrorNumbers.Error_MissingMethodName));
                    return validationErrors;
                }
                targetMember = eventInfo;
            }

            attributes = interfaceType.GetCustomAttributes(typeof(CorrelationProviderAttribute), false);
            if (attributes.Length != 0)
                return validationErrors;

            CorrelationToken correlator = activity.GetValue((activity is CallExternalMethodActivity) ? CallExternalMethodActivity.CorrelationTokenProperty : HandleExternalEventActivity.CorrelationTokenProperty) as CorrelationToken;

            object[] correlationParameterAttributes = interfaceType.GetCustomAttributes(typeof(CorrelationParameterAttribute), false);
            if (correlationParameterAttributes.Length == 0)
            {
                if (correlator != null)
                    validationErrors.Add(new ValidationError(string.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Error_CorrelationTokenSpecifiedForUncorrelatedInterface), activity.QualifiedName, interfaceType), ErrorNumbers.Error_InvalidIdentifier, false, "CorrelationToken"));

                return validationErrors;
            }

            // Someone derived from the activity and compiled, don't generate errors (P || C) validation.
            if (activity.Parent == null)
                return validationErrors;

            if (correlator == null || String.IsNullOrEmpty(correlator.Name))
            {
                validationErrors.Add(new ValidationError(string.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Error_MissingCorrelationTokenProperty), activity.QualifiedName), ErrorNumbers.Error_ParameterPropertyNotSet, false, "CorrelationToken"));
                return validationErrors;
            }

            if (String.IsNullOrEmpty(correlator.OwnerActivityName))
            {
                validationErrors.Add(new ValidationError(string.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Error_MissingCorrelationTokenOwnerNameProperty), activity.QualifiedName), ErrorNumbers.Error_ParameterPropertyNotSet, false, "CorrelationToken"));
                return validationErrors;
            }

            string qualifiedCorrelationToken = null;
            Activity sourceActivity = activity.GetActivityByName(correlator.OwnerActivityName);
            if (sourceActivity == null)
                sourceActivity = Helpers.ParseActivityForBind(activity, correlator.OwnerActivityName);
            if (sourceActivity != null)
                qualifiedCorrelationToken = sourceActivity.QualifiedName;

            Activity replicatorParent = null;
            CompositeActivity parent = activity.Parent;
            Activity rootActivity = parent;
            bool ownerIsParent = false;
            while (parent != null)
            {
                // We hardcode Replicator here, not MultiInstance | Concurrent.
                if (parent is ReplicatorActivity && replicatorParent == null)
                    replicatorParent = parent;

                if (qualifiedCorrelationToken == parent.QualifiedName)
                    ownerIsParent = true;

                rootActivity = parent;
                parent = parent.Parent;

            }

            if (!ownerIsParent)
            {
                ValidationError error = new ValidationError(string.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Error_OwnerActivityIsNotParent), activity.QualifiedName), ErrorNumbers.Error_ParameterPropertyNotSet);
                error.PropertyName = "CorrelationToken";
                validationErrors.Add(error);
            }

            bool initializer = false;
            attributes = targetMember.GetCustomAttributes(typeof(CorrelationInitializerAttribute), false) as object[];
            if (attributes.Length > 0)
                initializer = true;

            if (initializer)
            {
                if (replicatorParent != null && activity is HandleExternalEventActivity)
                {
                    ValidationError error = new ValidationError(string.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Error_InitializerInReplicator), replicatorParent.QualifiedName), ErrorNumbers.Error_InitializerInReplicator, false);
                    error.PropertyName = "CorrelationToken";
                    validationErrors.Add(error);
                }
            }

            if (!string.IsNullOrEmpty(qualifiedCorrelationToken))
            {
                if (replicatorParent != null)
                {
                    bool isValid = false;
                    Walker walker = new Walker();
                    walker.FoundActivity += delegate(Walker w, WalkerEventArgs args)
                    {
                        if (!args.CurrentActivity.Enabled)
                            return;

                        if (args.CurrentActivity.QualifiedName == qualifiedCorrelationToken)
                        {
                            isValid = true;
                            args.Action = WalkerAction.Abort;
                            return;
                        }
                    };

                    walker.Walk(replicatorParent);
                    if (!isValid)
                    {
                        ValidationError error = new ValidationError(string.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Error_CorrelationTokenInReplicator, correlator.Name, replicatorParent.QualifiedName)), ErrorNumbers.Error_CorrelationTokenInReplicator, true);
                        error.PropertyName = "CorrelationToken";
                        validationErrors.Add(error);
                    }
                }

                //if (rootActivity is RootActivity)
                {
                    if (!initializer)
                    {
                        bool isValid = false;
                        bool ownerNameValidated = false;
                        bool initFollowerInTxnlScope = false;
                        Walker walker = new Walker();
                        walker.FoundActivity += delegate(Walker w, WalkerEventArgs args)
                        {
                            Activity currentActivity = args.CurrentActivity;
                            if (!currentActivity.Enabled)
                                return;

                            if (!(currentActivity is CallExternalMethodActivity) && !(currentActivity is HandleExternalEventActivity))
                                return;

                            CorrelationToken existingCorrelationTokenValue = currentActivity.GetValue((currentActivity is CallExternalMethodActivity) ? CallExternalMethodActivity.CorrelationTokenProperty : HandleExternalEventActivity.CorrelationTokenProperty) as CorrelationToken;
                            if (existingCorrelationTokenValue == null)
                                return;

                            if (currentActivity is CallExternalMethodActivity && !interfaceType.Equals(((CallExternalMethodActivity)currentActivity).InterfaceType))
                                return;
                            else if (currentActivity is HandleExternalEventActivity && !interfaceType.Equals(((HandleExternalEventActivity)currentActivity).InterfaceType))
                                return;

                            MemberInfo existingTargetMember = null;
                            if (currentActivity is CallExternalMethodActivity)
                            {
                                if (((CallExternalMethodActivity)currentActivity).MethodName == null || ((CallExternalMethodActivity)currentActivity).MethodName.Length == 0)
                                    return;

                                MethodInfo methodInfo = interfaceType.GetMethod(((CallExternalMethodActivity)currentActivity).MethodName, BindingFlags.Instance | BindingFlags.Public);
                                if (methodInfo == null || methodInfo.IsSpecialName)
                                    return;

                                existingTargetMember = methodInfo;
                            }
                            else
                            {
                                if (((HandleExternalEventActivity)currentActivity).EventName == null || ((HandleExternalEventActivity)currentActivity).EventName.Length == 0)
                                    return;

                                EventInfo eventInfo = interfaceType.GetEvent(((HandleExternalEventActivity)currentActivity).EventName, BindingFlags.Instance | BindingFlags.Public);
                                if (eventInfo == null)
                                    return;

                                existingTargetMember = eventInfo;
                            }

                            attributes = existingTargetMember.GetCustomAttributes(typeof(CorrelationInitializerAttribute), false) as object[];
                            if (attributes.Length == 0)
                                return;

                            if (activity is HandleExternalEventActivity)
                            {
                                Activity txnlParent = GetTransactionalScopeParent(currentActivity);
                                if (txnlParent != null && IsFollowerInTxnlScope(txnlParent, activity))
                                    initFollowerInTxnlScope = true;
                            }

                            string existingQualifiedCorrelationToken = null;
                            sourceActivity = activity.GetActivityByName(existingCorrelationTokenValue.OwnerActivityName);
                            if (sourceActivity == null)
                                sourceActivity = Helpers.ParseActivityForBind(activity, existingCorrelationTokenValue.OwnerActivityName);
                            if (sourceActivity != null)
                                existingQualifiedCorrelationToken = sourceActivity.QualifiedName;

                            if ((correlator.Name == existingCorrelationTokenValue.Name) &&
                                IsOwnerActivitySame(correlator.OwnerActivityName, existingCorrelationTokenValue.OwnerActivityName, activity, currentActivity))
                            {
                                isValid = true;
                                ownerNameValidated = true;
                                args.Action = WalkerAction.Abort;
                                return;
                            }
                        };

                        walker.Walk(rootActivity);

                        if (!isValid)
                        {
                            ValidationError error = new ValidationError(string.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Error_UninitializedCorrelation)), ErrorNumbers.Error_UninitializedCorrelation, true);
                            error.PropertyName = "CorrelationToken";
                            validationErrors.Add(error);
                            if (ownerNameValidated)
                            {
                                error = new ValidationError(string.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Error_MisMatchCorrelationTokenOwnerNameProperty), correlator.Name), ErrorNumbers.Error_UninitializedCorrelation, false);
                                error.PropertyName = "CorrelationToken";
                                validationErrors.Add(error);
                            }
                        }

                        if (initFollowerInTxnlScope)
                        {
                            ValidationError error = new ValidationError(string.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Error_InitializerFollowerInTxnlScope)), ErrorNumbers.Error_InitializerFollowerInTxnlScope, false);
                            error.PropertyName = "CorrelationToken";
                            validationErrors.Add(error);
                        }
                    }
                }
            }

            return validationErrors;
        }

        private static Activity GetTransactionalScopeParent(Activity activity)
        {
            Activity parent = activity;
            while (parent != null)
            {
                if (parent is CompensatableTransactionScopeActivity || parent is TransactionScopeActivity)
                {
                    return parent;
                }
                parent = parent.Parent;
            }
            return parent;
        }

        private static bool IsFollowerInTxnlScope(Activity parent, Activity activity)
        {
            Activity currentParent = activity;
            while (currentParent != null)
            {
                if (currentParent == parent)
                {
                    return true;
                }
                currentParent = currentParent.Parent;
            }
            return false;
        }

        private static bool IsOwnerActivitySame(string ownerActivityName, string existingOwnerActivityName, Activity currentActivity, Activity existingActivity)
        {
            if (ownerActivityName.Equals(existingOwnerActivityName))
                return true;

            Activity owner = currentActivity.GetActivityByName(ownerActivityName);
            if (owner == null)
                owner = Helpers.ParseActivityForBind(currentActivity, ownerActivityName);

            Activity existingowner = currentActivity.GetActivityByName(existingOwnerActivityName);
            if (existingowner == null)
                existingowner = Helpers.ParseActivityForBind(existingActivity, existingOwnerActivityName);

            if (owner != null && existingowner != null && owner.QualifiedName.Equals(existingowner.QualifiedName))
                return true;

            return false;
        }

        private static ValidationErrorCollection ValidateHostInterface(IServiceProvider serviceProvider, Type interfaceType, Activity activity)
        {
            Dictionary<Type, ValidationErrorCollection> typesValidated = serviceProvider.GetService(typeof(Dictionary<Type, ValidationErrorCollection>)) as Dictionary<Type, ValidationErrorCollection>;

            if (typesValidated == null)
            {
                typesValidated = new Dictionary<Type, ValidationErrorCollection>();
                IServiceContainer serviceContainer = serviceProvider.GetService(typeof(IServiceContainer)) as IServiceContainer;
                if (serviceContainer != null)
                    serviceContainer.AddService(typeof(Dictionary<Type, ValidationErrorCollection>), typesValidated);
            }

            if (typesValidated.ContainsKey(interfaceType))
                return new ValidationErrorCollection();

            typesValidated.Add(interfaceType, new ValidationErrorCollection());

            object[] attributes = interfaceType.GetCustomAttributes(typeof(CorrelationProviderAttribute), false);
            if (attributes.Length == 0)
            {
                object[] dsAttribs = interfaceType.GetCustomAttributes(typeof(ExternalDataExchangeAttribute), false);
                object[] corrParamAttribs = interfaceType.GetCustomAttributes(typeof(CorrelationParameterAttribute), false);

                if (dsAttribs.Length != 0 && corrParamAttribs.Length != 0)
                {
                    typesValidated[interfaceType].AddRange(ValidateHostInterfaceMembers(interfaceType, activity));
                    typesValidated[interfaceType].AddRange(ValidateHostInterfaceAttributes(interfaceType));
                }
                else
                {
                    typesValidated[interfaceType].AddRange(ValidateInvalidHostInterfaceAttributes(interfaceType));
                }
            }

            return typesValidated[interfaceType];
        }

        private static ValidationErrorCollection ValidateHostInterfaceMembers(Type interfaceType, Activity activity)
        {
            ValidationErrorCollection validationErrors = new ValidationErrorCollection();

            foreach (MemberInfo memberInfo in interfaceType.GetMembers())
            {
                if (!(memberInfo is MethodInfo) && !(memberInfo is EventInfo))
                    continue;

                if ((memberInfo is MethodInfo) && ((MethodInfo)memberInfo).IsSpecialName)
                    continue;

                MethodInfo methodInfo = null;
                Type delegateType = null;
                if (memberInfo is EventInfo)
                {
                    EventInfo eventInfo = (EventInfo)memberInfo;
                    delegateType = eventInfo.EventHandlerType;
                    if (delegateType == null)
                        delegateType = TypeProvider.GetEventHandlerType(eventInfo);

                    if (delegateType == null)
                        throw new InvalidOperationException();

                    methodInfo = delegateType.GetMethod("Invoke");
                }
                else
                    methodInfo = (MethodInfo)memberInfo;

                if (methodInfo.IsGenericMethod)
                {
                    ValidationError error = new ValidationError(
                        string.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Error_GenericMethodsNotSupported), (memberInfo is EventInfo) ? delegateType.Name : methodInfo.Name), ErrorNumbers.Error_GenericMethodsNotSupported);
                    if (memberInfo is EventInfo)
                        error.UserData.Add(typeof(EventInfo), ((EventInfo)memberInfo).Name);
                    else
                        error.UserData.Add(typeof(MethodInfo), methodInfo.Name);
                    validationErrors.Add(error);
                }

                if (methodInfo.ReturnType != typeof(void) && (memberInfo is EventInfo))
                {
                    ValidationError error = new ValidationError(
                        string.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Error_ReturnTypeNotVoid), (memberInfo is EventInfo) ? delegateType.Name : methodInfo.Name), ErrorNumbers.Error_ReturnTypeNotVoid);
                    if (memberInfo is EventInfo)
                        error.UserData.Add(typeof(EventInfo), ((EventInfo)memberInfo).Name);
                    else
                        error.UserData.Add(typeof(MethodInfo), methodInfo.Name);
                    validationErrors.Add(error);
                }

                foreach (ParameterInfo param in methodInfo.GetParameters())
                {
                    if (param.IsOut || param.IsRetval)
                    {
                        ValidationError error = new ValidationError(
                            string.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Error_OutRefParameterNotSupported), (memberInfo is EventInfo) ? delegateType.Name : methodInfo.Name, param.Name), ErrorNumbers.Error_OutRefParameterNotSupported);
                        if (memberInfo is EventInfo)
                            error.UserData.Add(typeof(EventInfo), ((EventInfo)memberInfo).Name);
                        else
                            error.UserData.Add(typeof(MethodInfo), methodInfo.Name);
                        error.UserData.Add(typeof(ParameterInfo), param.Name);
                        validationErrors.Add(error);
                    }
                }
            }

            return validationErrors;
        }

        private static ValidationErrorCollection ValidateHostInterfaceAttributes(Type interfaceType)
        {
            if (interfaceType == null)
                throw new ArgumentNullException("interfaceType");

            ValidationErrorCollection validationErrors = new ValidationErrorCollection();
            ArrayList parameterAttrs = new ArrayList();
            foreach (object interfaceAttribute in interfaceType.GetCustomAttributes(typeof(CorrelationParameterAttribute), false))
            {
                CorrelationParameterAttribute parameterAttribute = Helpers.GetAttributeFromObject<CorrelationParameterAttribute>(interfaceAttribute);
                if (String.IsNullOrEmpty(parameterAttribute.Name) || parameterAttribute.Name.Trim().Length == 0)
                {
                    ValidationError error = new ValidationError(SR.GetString(CultureInfo.CurrentCulture, SR.Error_CorrelationAttributeInvalid, typeof(CorrelationParameterAttribute).Name, "Name", interfaceType.Name), ErrorNumbers.Error_CorrelationAttributeInvalid);
                    error.UserData.Add(typeof(CorrelationParameterAttribute), interfaceType.Name);
                    validationErrors.Add(error);
                    continue;
                }
                if (parameterAttrs.Contains(parameterAttribute.Name))
                {
                    ValidationError error = new ValidationError(SR.GetString(CultureInfo.CurrentCulture, SR.Error_DuplicateCorrelationAttribute, typeof(CorrelationParameterAttribute).Name, parameterAttribute.Name, interfaceType.Name), ErrorNumbers.Error_DuplicateCorrelationAttribute);
                    error.UserData.Add(typeof(CorrelationParameterAttribute), interfaceType.Name);
                    validationErrors.Add(error);
                    continue;
                }
                parameterAttrs.Add(parameterAttribute.Name);
            }

            Hashtable paramTypes = new Hashtable();
            Hashtable memberInfoCorrelationAliasAttrs = new Hashtable(); // MemberInfo -> CorrelationAliases
            Hashtable delegateTypeCorrelationAliasAttrs = new Hashtable(); // Delegate type -> CorrelationAliases
            int initializerCount = 0;
            foreach (MemberInfo memberInfo in interfaceType.GetMembers())
            {
                if (memberInfo is MethodInfo && !((MethodInfo)memberInfo).IsSpecialName)
                {
                    Hashtable correlationAliasAttrs = new Hashtable();
                    memberInfoCorrelationAliasAttrs.Add(memberInfo, correlationAliasAttrs);
                    FillCorrelationAliasAttrs(memberInfo, correlationAliasAttrs, validationErrors);
                    int aliasLength = memberInfo.GetCustomAttributes(typeof(CorrelationInitializerAttribute), false).Length;
                    initializerCount += aliasLength;
                    if (aliasLength > 0)
                    {
                        foreach (string paramName in parameterAttrs)
                        {
                            string paramPath = paramName;
                            if (correlationAliasAttrs.Contains(paramName))
                                paramPath = ((CorrelationAliasAttribute)correlationAliasAttrs[paramName]).Path;

                            Type paramType = FetchParameterType(memberInfo, paramPath);
                            if (paramType != null)
                            {
                                if (!paramTypes.ContainsKey(paramName))
                                    paramTypes[paramName] = paramType;
                            }
                        }
                    }
                }
                else if (memberInfo is EventInfo)
                {
                    int aliasLength = memberInfo.GetCustomAttributes(typeof(CorrelationInitializerAttribute), false).Length;
                    initializerCount += aliasLength;

                    // Add event info it's delegate's CorrelationAliasAttributes 
                    // to memberInfoCorrelationAliasAttrs against the EventInfo.
                    Hashtable correlationAliasAttrs = new Hashtable();
                    memberInfoCorrelationAliasAttrs.Add(memberInfo, correlationAliasAttrs);
                    FillCorrelationAliasAttrs(memberInfo, correlationAliasAttrs, validationErrors);
                    Type delegateType = Helpers.GetDelegateFromEvent((EventInfo)memberInfo);
                    MethodInfo delegateMethod = delegateType.GetMethod("Invoke");
                    FillCorrelationAliasAttrs(delegateType, correlationAliasAttrs, validationErrors);

                    // Add event delegate's CorrelationAliasAttributes 
                    // to delegateTypeCorrelationAliasAttrs against the deletgate type.
                    Hashtable delegateCorrelationAliasAttrs = new Hashtable();
                    FillCorrelationAliasAttrs(delegateType, delegateCorrelationAliasAttrs, validationErrors);
                    if (delegateTypeCorrelationAliasAttrs[delegateType] == null)
                        delegateTypeCorrelationAliasAttrs.Add(delegateType, delegateCorrelationAliasAttrs);

                    if (aliasLength > 0)
                    {
                        foreach (string paramName in parameterAttrs)
                        {
                            string paramPath = paramName;
                            if (correlationAliasAttrs.Contains(paramName))
                                paramPath = ((CorrelationAliasAttribute)correlationAliasAttrs[paramName]).Path;

                            Type paramType = FetchParameterType(memberInfo, paramPath);
                            if (paramType != null)
                            {
                                if (!paramTypes.ContainsKey(paramName))
                                    paramTypes[paramName] = paramType;
                            }
                        }
                    }
                }
            }

            // validate : correlaion alias has corresponding parameter
            foreach (DictionaryEntry memberEntry in memberInfoCorrelationAliasAttrs)
            {
                MemberInfo memberInfo = memberEntry.Key as MemberInfo;
                Hashtable correlationAliasAttrs = (Hashtable)memberEntry.Value;
                foreach (string paramName in correlationAliasAttrs.Keys)
                {
                    if (!parameterAttrs.Contains(paramName))
                    {
                        // Ignore the error if the alias attribute is from the event delegate.
                        if (memberInfo is EventInfo && ((Hashtable)delegateTypeCorrelationAliasAttrs[Helpers.GetDelegateFromEvent((EventInfo)memberInfo)])[paramName] != null)
                            continue;

                        ValidationError error = new ValidationError(SR.GetString(CultureInfo.CurrentCulture, SR.Error_CorrelationParameterNotFound, typeof(CorrelationAliasAttribute).Name, paramName, memberInfo.Name, typeof(CorrelationParameterAttribute).Name, interfaceType.Name), ErrorNumbers.Error_CorrelationParameterNotFound);
                        error.UserData.Add(typeof(CorrelationAliasAttribute), memberInfo.Name);
                        validationErrors.Add(error);
                    }
                }
            }

            // validate : correlation parameters has valid entries in all members
            foreach (string paramName in parameterAttrs)
            {
                foreach (DictionaryEntry memberEntry in memberInfoCorrelationAliasAttrs)
                {
                    string paramPath = paramName;
                    MemberInfo memberInfo = (MemberInfo)memberEntry.Key;
                    Hashtable correlationAliasAttrs = (Hashtable)memberEntry.Value;
                    if (correlationAliasAttrs.Contains(paramName))
                        paramPath = ((CorrelationAliasAttribute)correlationAliasAttrs[paramName]).Path;

                    Type paramType = FetchParameterType((MemberInfo)memberEntry.Key, paramPath);
                    if (paramType == null)
                    {
                        // Ignore the error if the alias attribute is from the event delegate.
                        if (memberInfo is EventInfo && ((Hashtable)delegateTypeCorrelationAliasAttrs[Helpers.GetDelegateFromEvent((EventInfo)memberInfo)])[paramName] != null)
                            continue;

                        //error path not resolved
                        ValidationError error = new ValidationError(SR.GetString(CultureInfo.CurrentCulture, SR.Error_CorrelationInvalid, (memberInfo.DeclaringType == interfaceType) ? memberInfo.Name : memberInfo.DeclaringType.Name, paramName), ErrorNumbers.Error_CorrelationInvalid);
                        error.UserData.Add(typeof(CorrelationParameterAttribute), (memberInfo.DeclaringType == interfaceType) ? memberInfo.Name : memberInfo.DeclaringType.Name);
                        validationErrors.Add(error);
                    }
                    else if (paramTypes.ContainsKey(paramName) && (Type)paramTypes[paramName] != paramType)
                    {
                        // error parameter type mismatch
                        ValidationError error = new ValidationError(SR.GetString(CultureInfo.CurrentCulture, SR.Error_CorrelationTypeNotConsistent, paramPath, typeof(CorrelationAliasAttribute).Name, (memberInfo.DeclaringType == interfaceType) ? memberInfo.Name : memberInfo.DeclaringType.Name, paramType.Name, ((Type)paramTypes[paramName]).Name, paramName, interfaceType.Name), ErrorNumbers.Error_CorrelationTypeNotConsistent);
                        error.UserData.Add(typeof(CorrelationAliasAttribute), (memberInfo.DeclaringType == interfaceType) ? memberInfo.Name : memberInfo.DeclaringType.Name);
                        validationErrors.Add(error);
                    }
                }
            }

            if (initializerCount == 0)
            {
                ValidationError error = new ValidationError(SR.GetString(CultureInfo.CurrentCulture, SR.Error_CorrelationInitializerNotDefinied, interfaceType.Name), ErrorNumbers.Error_CorrelationInitializerNotDefinied);
                error.UserData.Add(typeof(CorrelationInitializerAttribute), interfaceType.Name);
                validationErrors.Add(error);
            }

            return validationErrors;
        }

        private static ValidationErrorCollection ValidateInvalidHostInterfaceAttributes(Type interfaceType)
        {
            if (interfaceType == null)
                throw new ArgumentNullException("interfaceType");

            ValidationErrorCollection validationErrors = new ValidationErrorCollection();
            bool corrAttrsFound = false;
            foreach (MemberInfo memberInfo in interfaceType.GetMembers())
            {
                if (memberInfo.GetCustomAttributes(typeof(CorrelationInitializerAttribute), false).Length != 0 ||
                    memberInfo.GetCustomAttributes(typeof(CorrelationAliasAttribute), false).Length != 0)
                {
                    corrAttrsFound = true;
                    break;
                }
            }

            if (corrAttrsFound)
            {
                ValidationError error = new ValidationError(SR.GetString(CultureInfo.CurrentCulture, SR.Error_MissingCorrelationParameterAttribute, interfaceType.Name), ErrorNumbers.Error_MissingCorrelationParameterAttribute);
                error.UserData.Add(typeof(CorrelationParameterAttribute), interfaceType.Name);
                validationErrors.Add(error);
            }

            return validationErrors;
        }

        private static Type FetchParameterType(MemberInfo memberInfo, string paramPath)
        {
            MethodInfo method = null;
            if (memberInfo is EventInfo)
            {
                Type delegateType = Helpers.GetDelegateFromEvent((EventInfo)memberInfo);
                method = delegateType.GetMethod("Invoke");
            }
            else
                method = (MethodInfo)memberInfo;

            return GetCorrelationParameterType(paramPath, method.GetParameters());
        }

        private static void FillCorrelationAliasAttrs(MemberInfo memberInfo, Hashtable correlationAliasAttrs, ValidationErrorCollection validationErrors)
        {
            foreach (object memberAttribute in memberInfo.GetCustomAttributes(typeof(CorrelationAliasAttribute), false))
            {
                CorrelationAliasAttribute aliasAttribute = Helpers.GetAttributeFromObject<CorrelationAliasAttribute>(memberAttribute);
                // fill validation errors, name, path, duplicate check
                if (String.IsNullOrEmpty(aliasAttribute.Name) || aliasAttribute.Name.Trim().Length == 0)
                {
                    ValidationError error = new ValidationError(SR.GetString(CultureInfo.CurrentCulture, SR.Error_CorrelationAttributeInvalid, typeof(CorrelationAliasAttribute).Name, "Name", memberInfo.Name), ErrorNumbers.Error_CorrelationAttributeInvalid);
                    error.UserData.Add(typeof(CorrelationAliasAttribute), memberInfo.Name);
                    validationErrors.Add(error);
                    continue;
                }

                if (String.IsNullOrEmpty(aliasAttribute.Path) || aliasAttribute.Path.Trim().Length == 0)
                {
                    ValidationError error = new ValidationError(SR.GetString(CultureInfo.CurrentCulture, SR.Error_CorrelationAttributeInvalid, typeof(CorrelationAliasAttribute).Name, "Path", memberInfo.Name), ErrorNumbers.Error_CorrelationAttributeInvalid);
                    error.UserData.Add(typeof(CorrelationAliasAttribute), memberInfo.Name);
                    validationErrors.Add(error);
                    continue;
                }

                if (correlationAliasAttrs.Contains(aliasAttribute.Name))
                {
                    ValidationError error = new ValidationError(SR.GetString(CultureInfo.CurrentCulture, SR.Error_DuplicateCorrelationAttribute, typeof(CorrelationAliasAttribute).Name, aliasAttribute.Name, memberInfo.Name), ErrorNumbers.Error_DuplicateCorrelationAttribute);
                    error.UserData.Add(typeof(CorrelationAliasAttribute), memberInfo.Name);
                    validationErrors.Add(error);
                    continue;
                }
                correlationAliasAttrs.Add(aliasAttribute.Name, aliasAttribute);
            }

            return;
        }

        private static Type GetCorrelationParameterType(string parameterPropertyName, object parametersCollection)
        {
            string[] parsedPropertyName = parameterPropertyName.Split('.');
            Type correlationParameterType = null;
            int index = 0;

            if (parsedPropertyName.Length == 1)
            {
                Type evntHandlerType = null;
                if (parametersCollection is CodeParameterDeclarationExpressionCollection)
                {
                    foreach (CodeParameterDeclarationExpression parameterDeclaration in (CodeParameterDeclarationExpressionCollection)parametersCollection)
                    {
                        if (String.Compare("e", parameterDeclaration.Name, StringComparison.Ordinal) == 0)
                            evntHandlerType = parameterDeclaration.UserData[typeof(Type)] as Type;
                    }
                }
                else if (parametersCollection is ParameterInfo[])
                {
                    foreach (ParameterInfo parameterInfo in (ParameterInfo[])parametersCollection)
                    {
                        if (String.Compare("e", parameterInfo.Name, StringComparison.Ordinal) == 0)
                            evntHandlerType = parameterInfo.ParameterType;
                    }
                }
                if (evntHandlerType != null)
                {
                    string paramName = parsedPropertyName[0];
                    parsedPropertyName = new string[] { "e", paramName };
                }
            }

            if (parametersCollection is CodeParameterDeclarationExpressionCollection)
            {
                foreach (CodeParameterDeclarationExpression parameterDeclaration in (CodeParameterDeclarationExpressionCollection)parametersCollection)
                {
                    if (String.Compare(parsedPropertyName[0], parameterDeclaration.Name, StringComparison.Ordinal) == 0)
                        correlationParameterType = parameterDeclaration.UserData[typeof(Type)] as Type;
                }
            }
            else if (parametersCollection is ParameterInfo[])
            {
                foreach (ParameterInfo parameterInfo in (ParameterInfo[])parametersCollection)
                {
                    if (String.Compare(parsedPropertyName[0], parameterInfo.Name, StringComparison.Ordinal) == 0)
                        correlationParameterType = parameterInfo.ParameterType;
                }
            }
            else
                return null;

            if (parsedPropertyName.Length == 1)
                return correlationParameterType;

            //Search each part of the parsed name in it's predecessor's public properties/fields
            for (index = 1; index < parsedPropertyName.Length && correlationParameterType != null; index++)
            {
                Type tempParameterType = null;

                //Search though the public properties for a matching name
                PropertyInfo[] publicProperties = correlationParameterType.GetProperties();
                foreach (PropertyInfo propertyInfo in publicProperties)
                {
                    tempParameterType = null;
                    if (String.Compare(propertyInfo.Name, parsedPropertyName[index], StringComparison.Ordinal) == 0)
                    {
                        tempParameterType = propertyInfo.PropertyType;
                        break;
                    }
                }

                if (tempParameterType != null)
                {
                    correlationParameterType = tempParameterType;
                    continue;
                }

                //Search though the public fields for a matching name
                FieldInfo[] publicFields = correlationParameterType.GetFields();
                foreach (FieldInfo fieldInfo in publicFields)
                {
                    tempParameterType = null;
                    if (String.Compare(fieldInfo.Name, parsedPropertyName[index], StringComparison.Ordinal) == 0)
                    {
                        tempParameterType = fieldInfo.FieldType;
                        break;
                    }
                }

                if (tempParameterType != null)
                {
                    correlationParameterType = tempParameterType;
                    continue;
                }

                // If a matching public field or property was not found, return null.
                if (tempParameterType == null)
                    return null;
            }

            if (index == parsedPropertyName.Length)
                return correlationParameterType;

            return null;
        }
    }

    internal static class ParameterBindingValidator
    {
        internal static ValidationErrorCollection Validate(ValidationManager manager, Object obj)
        {
            ValidationErrorCollection validationErrors = new ValidationErrorCollection();

            Activity activity = obj as Activity;
            if (!(activity is CallExternalMethodActivity) && !(activity is HandleExternalEventActivity))
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(Activity).FullName), "obj");

            Type interfaceType = (activity is CallExternalMethodActivity) ? ((CallExternalMethodActivity)activity).InterfaceType : ((HandleExternalEventActivity)activity).InterfaceType;
            if (interfaceType == null)
                return validationErrors;

            string operation = (activity is CallExternalMethodActivity) ? ((CallExternalMethodActivity)activity).MethodName : ((HandleExternalEventActivity)activity).EventName;
            if (String.IsNullOrEmpty(operation))
                return validationErrors;

            WorkflowParameterBindingCollection parameterBinding = (activity is CallExternalMethodActivity) ? ((CallExternalMethodActivity)activity).ParameterBindings : ((HandleExternalEventActivity)activity).ParameterBindings;

            MethodInfo mInfo = interfaceType.GetMethod(operation);
            if (mInfo == null)
            {
                if (activity is CallExternalMethodActivity)
                {
                    return validationErrors;
                }
            }
            bool isEvent = false;
            if (mInfo == null)
            {
                //This is a work around for delgates of unbounded generic type. There is no support
                //in file code model for these so we dont support it for now. The only way
                //to detect if the DesignTimeEventInfo has EventHandler of unbounded generic type
                //is to check if we get the methods correctly here Ref Bug#17783
                EventInfo eventInfo = interfaceType.GetEvent(operation);
                if (eventInfo == null || eventInfo.GetAddMethod(true) == null)
                {
                    return validationErrors;
                }

                Type delegateType = eventInfo.EventHandlerType;
                if (delegateType == null)
                    delegateType = TypeProvider.GetEventHandlerType(eventInfo);

                mInfo = delegateType.GetMethod("Invoke");
                isEvent = true;
            }

            ValidateParameterBinding(manager, activity, isEvent, operation, mInfo, parameterBinding, validationErrors);
            return validationErrors;
        }

        private static void ValidateParameterBinding(ValidationManager manager, Activity activity, bool isEvent, string operation, MethodInfo mInfo, WorkflowParameterBindingCollection parameterBindings, ValidationErrorCollection validationErrors)
        {
            Hashtable parameterCollection = new Hashtable();
            ParameterInfo[] parameters = mInfo.GetParameters();
            bool canBeIntercepted = false;

            foreach (ParameterInfo parameter in parameters)
            {
                if (TypeProvider.IsAssignable(typeof(ExternalDataEventArgs), parameter.ParameterType))
                {
                    if (parameter.Position == 1)
                        canBeIntercepted = true;
                    ValidateParameterSerializabiltiy(validationErrors, parameter.ParameterType);
                }
                parameterCollection.Add(parameter.Name, parameter);
            }

            if (isEvent && (!canBeIntercepted || parameters.Length != 2))
                validationErrors.Add(new ValidationError(string.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Error_InvalidEventArgsSignature, operation)), ErrorNumbers.Error_FieldNotExists, false, "EventName"));

            if (mInfo.ReturnType != typeof(void))
                parameterCollection.Add("(ReturnValue)", mInfo.ReturnParameter);

            foreach (WorkflowParameterBinding parameterBinding in parameterBindings)
            {
                string paramName = parameterBinding.ParameterName;
                if (!parameterCollection.ContainsKey(paramName))
                {
                    if (isEvent)
                        validationErrors.Add(new ValidationError(string.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Error_InvalidEventPropertyName, paramName)), ErrorNumbers.Error_FieldNotExists, false, "ParameterBindings"));
                    else
                        validationErrors.Add(new ValidationError(string.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Error_InvalidMethodPropertyName, paramName)), ErrorNumbers.Error_FieldNotExists, false, "ParameterBindings"));
                    continue;
                }

                object paramValue = null;
                if (parameterBinding.IsBindingSet(WorkflowParameterBinding.ValueProperty))
                    paramValue = parameterBinding.GetBinding(WorkflowParameterBinding.ValueProperty);
                else
                    paramValue = parameterBinding.GetValue(WorkflowParameterBinding.ValueProperty);

                if (paramValue == null)
                    continue;

                ParameterInfo paramInfo = parameterCollection[paramName] as ParameterInfo;
                if (paramInfo != null)
                {
                    AccessTypes access = AccessTypes.Read;
                    if (paramInfo.IsOut || paramInfo.IsRetval)
                        access = AccessTypes.Write;
                    else if (paramInfo.ParameterType.IsByRef)
                        access |= AccessTypes.Write;

                    ValidationErrorCollection variableErrors = ValidationHelpers.ValidateProperty(manager, activity, paramValue,
                                                                                                  new PropertyValidationContext(parameterBinding, null, paramName),
                                                                                                  new BindValidationContext(paramInfo.ParameterType.IsByRef ? paramInfo.ParameterType.GetElementType() : paramInfo.ParameterType, access));
                    validationErrors.AddRange(variableErrors);
                }
            }
        }

        private static void ValidateParameterSerializabiltiy(ValidationErrorCollection validationErrors, Type type)
        {
            object[] attrs = type.GetCustomAttributes(typeof(SerializableAttribute), false);
            Type serializableType = type.GetInterface(typeof(ISerializable).FullName);
            if (attrs.Length == 0 && serializableType == null)
            {
                validationErrors.Add(new ValidationError(string.Format(CultureInfo.CurrentCulture,
                                     SR.GetString(SR.Error_EventArgumentValidationException), type.FullName),
                                     ErrorNumbers.Error_FieldNotExists, false, "EventName"));
            }


        }
    }

}
