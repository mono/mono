//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Activities
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Security;
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.Activities.Design;

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class TypedOperationInfo : OperationInfoBase
    {
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        internal static readonly DependencyProperty ContractTypeProperty =
            DependencyProperty.Register("ContractType",
            typeof(Type), typeof(TypedOperationInfo),
            new PropertyMetadata(null, DependencyPropertyOptions.Metadata));

        public TypedOperationInfo()
        {
        }

        public TypedOperationInfo(Type contractType, string operationName)
        {
            if (contractType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contractType");
            }
            if (string.IsNullOrEmpty(operationName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("operationName",
                    SR2.GetString(SR2.Error_ArgumentValueNullOrEmptyString));
            }

            this.ContractType = contractType;
            this.Name = operationName;
        }

        public Type ContractType
        {
            get { return (Type) this.GetValue(TypedOperationInfo.ContractTypeProperty); }
            set { this.SetValue(TypedOperationInfo.ContractTypeProperty, value); }
        }

        public override OperationInfoBase Clone()
        {
            TypedOperationInfo clonedOperation = (TypedOperationInfo) base.Clone();
            clonedOperation.ContractType = this.ContractType;

            return clonedOperation;
        }

        public override bool Equals(object obj)
        {
            if (!base.Equals(obj))
            {
                return false;
            }

            TypedOperationInfo operationInfo = obj as TypedOperationInfo;
            if (operationInfo == null)
            {
                return false;
            }
            if (this.ContractType != operationInfo.ContractType)
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            string returnValue = string.Empty;
            if (!string.IsNullOrEmpty(this.Name))
            {
                returnValue = this.Name;

                if (this.ContractType != null)
                {
                    returnValue = this.ContractType.FullName + "." + returnValue;
                }
            }

            return returnValue;
        }

        protected internal override string GetContractFullName(IServiceProvider provider)
        {
            if (this.ContractType != null)
            {
                return this.ContractType.FullName;
            }
            return string.Empty;
        }

        internal protected override Type GetContractType(IServiceProvider provider)
        {
            if (this.ContractType == null)
            {
                return null;
            }

            ITypeProvider typeProvider = null;

            if (provider != null)
            {
                typeProvider = provider.GetService(typeof(ITypeProvider)) as ITypeProvider;
            }

            Type contractType = this.ContractType;
            if (!this.IsReadOnly && contractType != null && typeProvider != null)
            {
                //Get the type from TypeProvider in case the type definition has changed in the local assembly.
                // the refresh is needed if contractType is a designtime type or is in the runtime type from the built assembly
                if (contractType is DesignTimeType ||
                    (typeProvider.LocalAssembly != null && typeProvider.LocalAssembly.Equals(contractType.Assembly)))
                {
                    Type currentDesignTimeType = typeProvider.GetType(contractType.AssemblyQualifiedName);
                    if (currentDesignTimeType != null)
                    {
                        this.ContractType = currentDesignTimeType;
                        this.RemoveProperty(OperationInfoBase.MethodInfoProperty);
                    }
                }
            }

            return this.ContractType;
        }

        internal protected override bool GetIsOneWay(IServiceProvider provider)
        {
            MethodInfo methodInfo = this.GetMethodInfo(provider);
            if (methodInfo != null)
            {
                object[] operationContractAttribs =
                    methodInfo.GetCustomAttributes(typeof(OperationContractAttribute), true);

                if (operationContractAttribs != null && operationContractAttribs.Length > 0)
                {
                    if (operationContractAttribs[0] is OperationContractAttribute)
                    {
                        return ((OperationContractAttribute) operationContractAttribs[0]).IsOneWay;
                    }
                    if (operationContractAttribs[0] is AttributeInfoAttribute)
                    {
                        AttributeInfoAttribute attribInfoAttrib = operationContractAttribs[0] as AttributeInfoAttribute;
                        return GetAttributePropertyValue<bool>(provider,
                            attribInfoAttrib.AttributeInfo,
                            "IsOneWay");
                    }
                }
            }

            return false;
        }

        internal protected override MethodInfo GetMethodInfo(IServiceProvider provider)
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                return null;
            }

            MethodInfo methodInfo = null;
            if (this.IsReadOnly)
            {
                if (this.UserData.Contains(OperationInfoBase.MethodInfoProperty))
                {
                    methodInfo = this.UserData[OperationInfoBase.MethodInfoProperty] as MethodInfo;
                }

                if (methodInfo != null)
                {
                    return methodInfo;
                }
            }

            Type type = this.GetContractType(provider);
            if (type != null && ServiceOperationHelpers.IsValidServiceContract(type))
            {
                methodInfo = this.InternalGetMethodInfo(provider, type);
            }

            if (this.IsReadOnly)
            {
                this.UserData[OperationInfoBase.MethodInfoProperty] = methodInfo;
            }

            return methodInfo;
        }

        internal protected override OperationParameterInfoCollection GetParameters(IServiceProvider provider)
        {
            OperationParameterInfoCollection parameters = new OperationParameterInfoCollection();

            MethodInfo methodInfo = this.GetMethodInfo(provider);
            if (methodInfo != null)
            {
                foreach (ParameterInfo parameter in methodInfo.GetParameters())
                {
                    if (parameters[parameter.Name] == null)
                    {
                        parameters.Add(new OperationParameterInfo(parameter));
                    }
                }

                if (methodInfo.ReturnParameter != null && methodInfo.ReturnParameter.ParameterType != typeof(void))
                {
                    if (parameters["(ReturnValue)"] == null)
                    {
                        OperationParameterInfo parameterInfo = new OperationParameterInfo(methodInfo.ReturnParameter);
                        parameterInfo.Name = "(ReturnValue)";
                        parameters.Add(parameterInfo);
                    }
                }
            }

            return parameters;
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

        T GetAttributePropertyValue<T>(IServiceProvider provider, AttributeInfo attribInfo, string propertyName)
        {
            string[] argumentNames = GetAttributePropertyNames(attribInfo);
            int argumentIndex = -1;
            for (int index = 0; index < argumentNames.Length; index++)
            {
                // skip unnamed arguments these are constructor arguments
                if ((argumentNames[index] == null) || (argumentNames[index].Length == 0))
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
            if (argumentIndex != -1)
            {
                return (T) attribInfo.GetArgumentValueAs(provider, argumentIndex, typeof(T));
            }
            else
            {
                return default(T);
            }
        }

        MethodInfo InternalGetMethodInfo(IServiceProvider provider, Type contractType)
        {
            MethodInfo methodInfo = null;

            if (contractType != null && ServiceOperationHelpers.IsValidServiceContract(contractType))
            {
                foreach (MethodInfo currentMethodInfo in contractType.GetMethods())
                {
                    object[] operationContractAttribs =
                        currentMethodInfo.GetCustomAttributes(typeof(OperationContractAttribute), true);

                    if (operationContractAttribs != null && operationContractAttribs.Length > 0)
                    {
                        string operationName = null;
                        if (operationContractAttribs[0] is OperationContractAttribute)
                        {
                            OperationContractAttribute operationContractAttribute =
                                operationContractAttribs[0] as OperationContractAttribute;

                            operationName = operationContractAttribute.Name;
                        }
                        if (operationContractAttribs[0] is AttributeInfoAttribute)
                        {
                            AttributeInfoAttribute attribInfoAttrib =
                                operationContractAttribs[0] as AttributeInfoAttribute;

                            operationName = GetAttributePropertyValue<string>(provider,
                                attribInfoAttrib.AttributeInfo,
                                "Name");
                        }

                        if (string.IsNullOrEmpty(operationName) &&
                            string.Compare(currentMethodInfo.Name, this.Name, StringComparison.Ordinal) == 0)
                        {
                            methodInfo = currentMethodInfo;
                            break;
                        }
                        else if (string.Compare(operationName, this.Name, StringComparison.Ordinal) == 0)
                        {
                            methodInfo = currentMethodInfo;
                            break;
                        }
                    }
                }
            }

            if (methodInfo == null)
            {
                foreach (Type parentContract in contractType.GetInterfaces())
                {
                    methodInfo = this.InternalGetMethodInfo(provider, parentContract);
                    if (methodInfo != null)
                    {
                        break;
                    }
                }
            }

            return methodInfo;
        }
    }
}
