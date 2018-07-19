//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

#pragma warning disable 1634, 1691
namespace System.Workflow.Activities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net.Security;
    using System.ServiceModel;
    using System.Reflection;

    internal sealed class ContractMethodInfo : MethodInfo
    {
        private Attribute[] attributes = null;
        private ContractType declaringType;
        private MethodAttributes methodAttributes;
        private string name;
        private ParameterInfo[] parameters;
        private ParameterInfo returnParam = null;

        internal ContractMethodInfo(ContractType declaringType, OperationInfo operationInfo)
        {
            if (declaringType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("declaringType");
            }
            if (operationInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("operationInfo");
            }
            if (string.IsNullOrEmpty(operationInfo.Name))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("operationInfo",
                    SR2.GetString(SR2.Error_OperationNameNotSpecified));
            }

            this.declaringType = declaringType;
            this.name = operationInfo.Name;
            this.methodAttributes = MethodAttributes.Public |
                MethodAttributes.Abstract |
                MethodAttributes.Virtual;

            SortedList<int, ContractMethodParameterInfo> localParameters =
                new SortedList<int, ContractMethodParameterInfo>();

            foreach (OperationParameterInfo operationParameterInfo in operationInfo.Parameters)
            {
                ContractMethodParameterInfo parameterInfo =
                    new ContractMethodParameterInfo(this, operationParameterInfo);
                if (parameterInfo.Position == -1)
                {
                    this.returnParam = parameterInfo;
                }
                else
                {
                    localParameters.Add(parameterInfo.Position, parameterInfo);
                }
            }

            this.parameters = new ParameterInfo[localParameters.Count];
            foreach (ContractMethodParameterInfo paramInfo in localParameters.Values)
            {
                this.parameters[paramInfo.Position] = paramInfo;
            }

            if (this.returnParam == null)
            {
                OperationParameterInfo returnParameterInfo = new OperationParameterInfo();
                returnParameterInfo.Position = -1;
                returnParameterInfo.ParameterType = typeof(void);

                this.returnParam = new ContractMethodParameterInfo(this, returnParameterInfo);
            }

            OperationContractAttribute operationContract = new OperationContractAttribute();
            if (operationInfo.HasProtectionLevel && operationInfo.ProtectionLevel != null)
            {
                operationContract.ProtectionLevel = (ProtectionLevel) operationInfo.ProtectionLevel;
            }
            operationContract.IsOneWay = operationInfo.IsOneWay;

            this.attributes = new Attribute[] { operationContract };

            declaringType.AddMethod(this);
        }

        public override MethodAttributes Attributes
        {
            get
            {
                return this.methodAttributes;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                return this.declaringType;
            }
        }
        public override RuntimeMethodHandle MethodHandle
        {
            get
            {
#pragma warning suppress 56503
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new NotImplementedException(SR2.GetString(SR2.Error_RuntimeNotSupported)));
            }
        }

        public override string Name
        {
            get
            {
                return this.name;
            }
        }

        public override Type ReflectedType
        {
            get
            {
                return this.declaringType;
            }
        }

        public override ParameterInfo ReturnParameter
        {
            get
            {
                return this.returnParam;
            }
        }

        public override Type ReturnType
        {
            get
            {
                if (this.returnParam == null)
                {
                    return null;
                }
                return this.returnParam.ParameterType;
            }
        }

        public override ICustomAttributeProvider ReturnTypeCustomAttributes
        {
            get
            {
                return this.ReturnType;
            }
        }

        public override MethodInfo GetBaseDefinition()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                new NotImplementedException());
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return GetCustomAttributes(typeof(object), inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (attributeType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("attributeType");
            }
            return ServiceOperationHelpers.GetCustomAttributes(attributeType, this.attributes);
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            return MethodImplAttributes.IL;
        }

        public override ParameterInfo[] GetParameters()
        {
            return this.parameters;
        }

        public override object Invoke(object obj,
            BindingFlags invokeAttr,
            Binder binder,
            object[] parameters,
            CultureInfo culture)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                new NotImplementedException(SR2.GetString(SR2.Error_RuntimeNotSupported)));
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            if (attributeType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("attributeType");
            }

            return ServiceOperationHelpers.IsDefined(attributeType, attributes);
        }

        public override string ToString()
        {
            System.Workflow.Activities.ContractType.MemberSignature signature =
                new ContractType.MemberSignature(this);
            return signature.ToString();
        }
    }
}
