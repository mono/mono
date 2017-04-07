// Copyright (c) Microsoft Corporation. All rights reserved. 
//  
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// WHETHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
// WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// THE ENTIRE RISK OF USE OR RESULTS IN CONNECTION WITH THE USE OF THIS CODE 
// AND INFORMATION REMAINS WITH THE USER. 


/*********************************************************************
 * NOTE: A copy of this file exists at: WF\Activities\Common
 * The two files must be kept in sync.  Any change made here must also
 * be made to WF\Activities\Common\DelegateTypeInfo.cs
*********************************************************************/
namespace System.Workflow.ComponentModel
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Globalization;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Reflection;
    using System.Diagnostics.CodeAnalysis;

    internal class DelegateTypeInfo
    {
        private CodeParameterDeclarationExpression[] parameters;
        private Type[] parameterTypes;
        private CodeTypeReference returnType;

        internal CodeParameterDeclarationExpression[] Parameters
        {
            get
            {
                return parameters;
            }
        }

        internal Type[] ParameterTypes
        {
            get
            {
                return parameterTypes;
            }
        }

        internal CodeTypeReference ReturnType
        {
            get
            {
                return returnType;
            }
        }

        internal DelegateTypeInfo(Type delegateClass)
        {
            Resolve(delegateClass);
        }

        [SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", Justification = "EndsWith(\"&\") not a security issue.")]
        private void Resolve(Type delegateClass)
        {
            MethodInfo invokeMethod = delegateClass.GetMethod("Invoke");
            if (invokeMethod == null)
                throw new ArgumentException("delegateClass");
            Resolve(invokeMethod);
        }

        [SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", Justification = "EndsWith(\"&\") not a security issue.")]
        private void Resolve(MethodInfo method)
        {
            // Here we build up an array of argument types, separated
            // by commas.
            ParameterInfo[] argTypes = method.GetParameters();

            parameters = new CodeParameterDeclarationExpression[argTypes.Length];
            parameterTypes = new Type[argTypes.Length];
            for (int index = 0; index < argTypes.Length; index++)
            {
                string paramName = argTypes[index].Name;
                Type paramType = argTypes[index].ParameterType;

                if (paramName == null || paramName.Length == 0)
                    paramName = "param" + index.ToString(CultureInfo.InvariantCulture);

                FieldDirection fieldDir = FieldDirection.In;

                // check for the '&' that means ref (gotta love it!) 
                // and we need to strip that & before we continue.  Ouch.
                if (paramType.IsByRef)
                {
                    if (paramType.FullName.EndsWith("&"))
                    {
                        // strip the & and reload the type without it.
                        paramType = paramType.Assembly.GetType(paramType.FullName.Substring(0, paramType.FullName.Length - 1), true);
                    }
                    fieldDir = FieldDirection.Ref;
                }
                if (argTypes[index].IsOut)
                {
                    if (argTypes[index].IsIn)
                        fieldDir = FieldDirection.Ref;
                    else
                        fieldDir = FieldDirection.Out;
                }
                parameters[index] = new CodeParameterDeclarationExpression(new CodeTypeReference(paramType), paramName);
                parameters[index].Direction = fieldDir;
                parameterTypes[index] = paramType;
            }
            this.returnType = new CodeTypeReference(method.ReturnType);
        }
        public override bool Equals(object other)
        {
            if (other == null)
                return false;

            DelegateTypeInfo dtiOther = other as DelegateTypeInfo;

            if (dtiOther == null)
                return false;

            if (ReturnType.BaseType != dtiOther.ReturnType.BaseType || Parameters.Length != dtiOther.Parameters.Length)
                return false;

            for (int parameter = 0; parameter < Parameters.Length; parameter++)
            {
                CodeParameterDeclarationExpression otherParam = dtiOther.Parameters[parameter];
                if (otherParam.Type.BaseType != Parameters[parameter].Type.BaseType)
                    return false;
            }
            return true;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
