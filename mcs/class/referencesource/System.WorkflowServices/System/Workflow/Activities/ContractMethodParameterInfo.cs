//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Activities
{
    using System;
    using System.Reflection;
    using System.ServiceModel;
    using System.Workflow.ComponentModel.Compiler;

    internal sealed class ContractMethodParameterInfo : ParameterInfo
    {
        internal ContractMethodParameterInfo(ContractMethodInfo member,
            OperationParameterInfo parameterInfo)
        {
            if (member == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("member");
            }
            if (parameterInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameterInfo");
            }

            this.AttrsImpl = parameterInfo.Attributes;

            this.MemberImpl = member;
            this.PositionImpl = parameterInfo.Position;
            if (parameterInfo.Position >= 0)
            {
                this.NameImpl = parameterInfo.Name;

                string typeName = parameterInfo.ParameterType.FullName;
                if ((this.AttrsImpl & ParameterAttributes.Out) > 0)
                {
                    typeName += '&'; // Append with & for (ref & out) parameter types

                    if (this.Member.DeclaringType is DesignTimeType)
                    {
                        this.ClassImpl = (this.Member.DeclaringType as DesignTimeType).ResolveType(typeName);
                    }
                    else if (parameterInfo.ParameterType is DesignTimeType)
                    {
                        this.ClassImpl = (parameterInfo.ParameterType as DesignTimeType).ResolveType(typeName);
                    }
                    else
                    {
                        typeName += ", " + parameterInfo.ParameterType.Assembly.FullName;
                        this.ClassImpl = Type.GetType(typeName);
                    }
                }
                else
                {
                    this.ClassImpl = parameterInfo.ParameterType;
                }
            }
            else
            {
                this.ClassImpl = parameterInfo.ParameterType;
            }
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

            if (this.ParameterType == null)
            {
                return new object[0];
            }

            return this.ParameterType.GetCustomAttributes(attributeType, inherit);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            if (attributeType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("attributeType");
            }

            if (this.ParameterType == null)
            {
                return false;
            }

            return this.ParameterType.IsDefined(attributeType, inherit);
        }
    }
}
