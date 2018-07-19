//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Activities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    internal static class ServiceOperationHelpers
    {
        public static string GetOperationName(IServiceProvider serviceProvider, MethodInfo methodInfo)
        {
            Fx.Assert((methodInfo != null), " MethoInfo cannot be null");

            string operationName = methodInfo.Name;
            object[] operationContractAttribs = methodInfo.GetCustomAttributes(typeof(OperationContractAttribute), true);
            if (operationContractAttribs != null && operationContractAttribs.Length > 0)
            {
                if (operationContractAttribs[0] is OperationContractAttribute)
                {
                    OperationContractAttribute operationContractAttribute = operationContractAttribs[0] as OperationContractAttribute;
                    if (!String.IsNullOrEmpty(operationContractAttribute.Name))
                    {
                        operationName = operationContractAttribute.Name;
                    }
                }
                if (operationContractAttribs[0] is AttributeInfoAttribute)
                {
                    AttributeInfoAttribute attribInfoAttrib = operationContractAttribs[0] as AttributeInfoAttribute;
                    string propertyName = "Name";
                    string namePropertyValue;
                    if (TryGetArgumentValueAs<string>(serviceProvider, attribInfoAttrib.AttributeInfo, propertyName, out namePropertyValue))
                    {
                        operationName = namePropertyValue;
                    }
                }
            }
            return operationName;
        }

        public static PropertyDescriptor GetServiceOperationInfoPropertyDescriptor(Activity activity)
        {
            if (activity is ReceiveActivity)
            {
                return TypeDescriptor.GetProperties(activity)[ReceiveActivity.ServiceOperationInfoProperty.Name];
            }
            else
            {
                Fx.Assert(activity is SendActivity, " only Receive and Send activities are valid inputs to this method");
                return TypeDescriptor.GetProperties(activity)[SendActivity.ServiceOperationInfoProperty.Name];
            }
        }

        public static bool IsAsyncOperation(IServiceProvider serviceProvider, MethodInfo methodInfo)
        {
            if (serviceProvider == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceProvider");
            }

            if (methodInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("methodInfo");
            }

            bool isAsync = false;
            object[] operationContractAttribs = methodInfo.GetCustomAttributes(typeof(OperationContractAttribute), true);
            Fx.Assert(operationContractAttribs != null, "returned attribs list cannot be null");
            Fx.Assert(operationContractAttribs.Length > 0, "operation doesnt seem to be a valid operationcontract");

            if (operationContractAttribs[0] is OperationContractAttribute)
            {
                OperationContractAttribute operationContractAttribute = operationContractAttribs[0] as OperationContractAttribute;
                isAsync = operationContractAttribute.AsyncPattern;
            }
            if (operationContractAttribs[0] is AttributeInfoAttribute)
            {
                AttributeInfoAttribute attribInfoAttrib = operationContractAttribs[0] as AttributeInfoAttribute;
                isAsync = GetOperationAsyncPattern(serviceProvider, attribInfoAttrib.AttributeInfo);
            }

            return isAsync;
        }

        public static bool IsInitiatingOperation(IServiceProvider serviceProvider, MethodInfo methodInfo)
        {
            if (serviceProvider == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceProvider");
            }

            if (methodInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("methodInfo");
            }

            bool isInitiating = true;
            object[] operationContractAttribs = methodInfo.GetCustomAttributes(typeof(OperationContractAttribute), true);
            Fx.Assert(operationContractAttribs != null, "returned attribs list cannot be null");
            Fx.Assert(operationContractAttribs.Length > 0, "operation doesnt seem to be a valid operationcontract");

            if (operationContractAttribs[0] is OperationContractAttribute)
            {
                OperationContractAttribute operationContractAttribute = operationContractAttribs[0] as OperationContractAttribute;
                isInitiating = operationContractAttribute.IsInitiating;
            }
            if (operationContractAttribs[0] is AttributeInfoAttribute)
            {
                AttributeInfoAttribute attribInfoAttrib = operationContractAttribs[0] as AttributeInfoAttribute;
                isInitiating = IsInitiatingOperationContract(serviceProvider, attribInfoAttrib.AttributeInfo);
            }

            return isInitiating;
        }

        public static bool IsNullableType(Type type)
        {
            return (Nullable.GetUnderlyingType(type.IsByRef ? type.GetElementType() : type) != null);
        }

        public static bool IsValidServiceContract(Type contractType)
        {
            if (contractType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contractType");
            }

            object[] contractAttribs = contractType.GetCustomAttributes(typeof(ServiceContractAttribute), false);
            if (contractAttribs != null && contractAttribs.Length > 0)
            {
                if (contractAttribs[0] is ServiceContractAttribute)
                {
                    return true;
                }
                if (contractAttribs[0] is AttributeInfoAttribute)
                {
                    AttributeInfoAttribute attribInfoAttrib = contractAttribs[0] as AttributeInfoAttribute;
                    if (typeof(ServiceContractAttribute).IsAssignableFrom(attribInfoAttrib.AttributeInfo.AttributeType))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsValidServiceOperation(MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("methodInfo");
            }

            object[] operationContractAttribs = methodInfo.GetCustomAttributes(typeof(OperationContractAttribute), true);
            if (operationContractAttribs != null && operationContractAttribs.Length > 0)
            {
                if (operationContractAttribs[0] is OperationContractAttribute)
                {
                    return true;
                }
                if (operationContractAttribs[0] is AttributeInfoAttribute)
                {
                    AttributeInfoAttribute attribInfoAttrib = operationContractAttribs[0] as AttributeInfoAttribute;
                    if (typeof(OperationContractAttribute).IsAssignableFrom(attribInfoAttrib.AttributeInfo.AttributeType))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal static List<Type> GetContracts(Type contractType)
        {
            List<Type> types = new List<Type>();
            if (contractType.IsDefined(typeof(ServiceContractAttribute), false))
            {
                types.Add(contractType);
            }
            foreach (Type contract in contractType.GetInterfaces())
            {
                if (contract.IsDefined(typeof(ServiceContractAttribute), false))
                {
                    types.Add(contract);
                }
            }

            return types;
        }

        internal static SessionMode GetContractSessionMode(IServiceProvider serviceProvider, AttributeInfo attribInfo)
        {
            string propertyName = "SessionMode";
            SessionMode sessionMode = SessionMode.Allowed;
            if (!TryGetArgumentValueAs<SessionMode>(serviceProvider, attribInfo, propertyName, out sessionMode))
            {
                sessionMode = SessionMode.Allowed;
            }
            return sessionMode;
        }

        internal static object[] GetCustomAttributes(Type attributeType, Attribute[] attributes)
        {
            if (attributeType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("attributeType");
            }

            ArrayList attributeList = new ArrayList();

            foreach (Attribute currentAttribute in attributes)
            {
                if (attributeType.IsAssignableFrom(currentAttribute.GetType()))
                {
                    attributeList.Add(currentAttribute);
                }
            }

            return attributeList.ToArray();
        }

        internal static bool GetOperationAsyncPattern(IServiceProvider serviceProvider, AttributeInfo attribInfo)
        {
            string propertyName = "AsyncPattern";
            bool isAsync = false;
            if (!TryGetArgumentValueAs<bool>(serviceProvider, attribInfo, propertyName, out isAsync))
            {
                isAsync = false;
            }
            return isAsync;
        }

        internal static bool IsDefined(Type attributeType,
            Attribute[] attributes)
        {
            if (attributeType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("attributeType");
            }

            foreach (Attribute attribute in attributes)
            {
                if (attributeType.IsAssignableFrom(attribute.GetType()))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool IsInitiatingOperationContract(IServiceProvider serviceProvider, AttributeInfo attribInfo)
        {
            string propertyName = "IsInitiating";
            bool isInitiating = true;
            if (!TryGetArgumentValueAs<bool>(serviceProvider, attribInfo, propertyName, out isInitiating))
            {
                isInitiating = true;
            }
            return isInitiating;
        }

        internal static void SetWorkflowOperationBehavior(ContractDescription contractDescription, ServiceDescriptionContext context)
        {
            if (contractDescription == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contractDescription");
            }

            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            foreach (OperationDescription opDescription in contractDescription.Operations)
            {
                WorkflowOperationBehavior behavior = null;
                KeyValuePair<Type, string> operationKey =
                    new KeyValuePair<Type, string>(opDescription.DeclaringContract.ContractType, opDescription.Name);

                if (!context.WorkflowOperationBehaviors.TryGetValue(operationKey, out behavior))
                {
                    behavior = new WorkflowOperationBehavior();
                    context.WorkflowOperationBehaviors.Add(operationKey, behavior);
                    behavior.CanCreateInstance = false;
                }

                if (opDescription.Behaviors.Find<WorkflowOperationBehavior>() != behavior)
                {
                    opDescription.Behaviors.Remove(typeof(WorkflowOperationBehavior));
                    opDescription.Behaviors.Add(behavior);
                }
            }
        }

        private static string[] GetAttributePropertyNames(AttributeInfo attributeInfo)
        {
            if (attributeInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("attributeInfo");
            }

            string[] argumentNames = null;
            BindingFlags bindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic;
            FieldInfo argumentNamesField = typeof(AttributeInfo).GetField("argumentNames", bindingFlags);
            if (argumentNamesField != null)
            {
                argumentNames = argumentNamesField.GetValue(attributeInfo) as string[];
            }

            return argumentNames;
        }


        private static bool TryGetArgumentValueAs<T>(IServiceProvider serviceProvider, AttributeInfo attribInfo, string propertyName, out T propertyValue)
        {
            string[] argumentNames = GetAttributePropertyNames(attribInfo);
            int argumentIndex = -1;
            for (int index = 0; index < argumentNames.Length; index++)
            {
                // skip unnamed arguments these are constructor arguments
                if (string.IsNullOrEmpty((argumentNames[index])))
                {
                    continue;
                }
                else
                {
                    if (argumentNames[index].Equals(propertyName))
                    {
                        argumentIndex = index;
                        break;
                    }
                }
            }
            if (argumentIndex == -1)
            {
                propertyValue = default(T);
                return false;
            }
            propertyValue = (T) attribInfo.GetArgumentValueAs(serviceProvider, argumentIndex, typeof(T));
            return true;
        }
    }
}
