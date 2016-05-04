#pragma warning disable 1634, 1691
namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;

    #region DesignTimePropertyInfo
    internal sealed class DesignTimePropertyInfo : PropertyInfo
    {
        #region Members and Constructors
        private Attribute[] attributes = null;
        private CodeMemberProperty property = null;
        private DesignTimeType declaringType = null;

        private MethodInfo getMethod = null;
        private MethodInfo setMethod = null;

        internal DesignTimePropertyInfo(DesignTimeType declaringType, CodeMemberProperty property)
        {
            this.property = property;
            this.declaringType = declaringType;
        }

        #endregion

        internal CodeMemberProperty CodeMemberProperty
        {
            get
            {
                return this.property;
            }
        }

        #region Property Info overrides

        public override Type PropertyType
        {
            get
            {
                return declaringType.ResolveType(DesignTimeType.GetTypeNameFromCodeTypeReference(this.property.Type, declaringType));
            }
        }
        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            throw new NotImplementedException(TypeSystemSR.GetString("Error_RuntimeNotSupported"));
        }

        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            ArrayList accessorList = new ArrayList();

            if (Helper.IncludeAccessor(this.GetGetMethod(nonPublic), nonPublic))
                accessorList.Add(this.getMethod);

            if (Helper.IncludeAccessor(this.GetSetMethod(nonPublic), nonPublic))
                accessorList.Add(this.setMethod);

            return accessorList.ToArray(typeof(MethodInfo)) as MethodInfo[];
        }

        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            if (this.CanRead && this.getMethod == null)
            {
                String accessor = "get_" + this.Name;
                this.getMethod = new PropertyMethodInfo(true, accessor, this);
            }
            // now check to see if getMethod is public
            if (nonPublic || ((this.getMethod != null) && ((this.getMethod.Attributes & MethodAttributes.Public) == MethodAttributes.Public)))
                return this.getMethod;

            return null;
        }
        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            if (this.CanWrite && this.setMethod == null)
            {
                String accessor = "set_" + this.Name;
                this.setMethod = new PropertyMethodInfo(false, accessor, this);
            }

            // now check to see if getMethod is public
            if (nonPublic || ((this.setMethod != null) && ((this.setMethod.Attributes & MethodAttributes.Public) == MethodAttributes.Public)))
                return this.setMethod;

            return null;
        }
        public override ParameterInfo[] GetIndexParameters()
        {
            int numParams = 0;
            ParameterInfo[] methParams = null;

            // First try to get the Get method.
            MethodInfo methodInfo = this.GetGetMethod(true);

            if (methodInfo != null)
            {
                // There is a Get method so use it.
                methParams = methodInfo.GetParameters();
                numParams = methParams.Length;
            }
            else
            {
                // If there is no Get method then use the Set method.
                methodInfo = GetSetMethod(true);
                if (methodInfo != null)
                {
                    methParams = methodInfo.GetParameters();
                    // Exclude value parameter
                    numParams = methParams.Length - 1;
                }
            }

            // Now copy over the parameter info's and change their 
            // owning member info to the current property info.
            ParameterInfo[] propParams = new ParameterInfo[numParams];

            for (int i = 0; i < numParams; i++)
                propParams[i] = methParams[i];

            return propParams; // 
        }
        public override PropertyAttributes Attributes
        {
            get
            {
                return PropertyAttributes.None;
            }
        }
        public override bool CanRead
        {
            get
            {
                return this.property.HasGet;
            }
        }
        public override bool CanWrite
        {
            get
            {
                return this.property.HasSet;
            }
        }
        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            throw new NotImplementedException(TypeSystemSR.GetString("Error_RuntimeNotSupported"));
        }
        public override string Name
        {
            get
            {
                return Helper.EnsureTypeName(this.property.Name);
            }
        }
        public override Type DeclaringType
        {
            get
            {
                return this.declaringType;
            }
        }
        public override Type ReflectedType
        {
            get
            {
                return this.declaringType;
            }
        }

        #endregion

        #region MemberInfo Overrides

        public override object[] GetCustomAttributes(bool inherit)
        {
            return GetCustomAttributes(typeof(object), inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (attributeType == null)
                throw new ArgumentNullException("attributeType");

            if (this.attributes == null)
                this.attributes = Helper.LoadCustomAttributes(this.property.CustomAttributes, this.DeclaringType as DesignTimeType);

            return Helper.GetCustomAttributes(attributeType, inherit, this.attributes, this);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            if (attributeType == null)
                throw new ArgumentNullException("attributeType");

            if (this.attributes == null)
                this.attributes = Helper.LoadCustomAttributes(this.property.CustomAttributes, this.DeclaringType as DesignTimeType);

            if (Helper.IsDefined(attributeType, inherit, attributes, this))
                return true;

            return false;
        }

        #endregion

        #region PropertyInfo MethodInfo classes
        private sealed class PropertyMethodInfo : MethodInfo
        {
            private string name = String.Empty;
            private DesignTimePropertyInfo property = null;
            private ParameterInfo[] parameters = null;
            private bool isGetter = false;

            internal PropertyMethodInfo(bool isGetter, string name, DesignTimePropertyInfo property)
            {
                this.isGetter = isGetter;
                this.name = name;
                this.property = property;
            }
            public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
            {
                throw new NotImplementedException(TypeSystemSR.GetString("Error_RuntimeNotSupported"));
            }

            internal bool IsGetter
            {
                get
                {
                    return this.isGetter;
                }
            }

            #region MemberInfo Overrides
            public override string Name
            {
                get
                {
                    return Helper.EnsureTypeName(this.name);
                }
            }
            public override Type DeclaringType
            {
                get
                {
                    return this.property.declaringType;
                }
            }
            public override Type ReflectedType
            {
                get
                {
                    return this.property.declaringType;
                }
            }
            public override object[] GetCustomAttributes(bool inherit)
            {
                return GetCustomAttributes(typeof(object), inherit);
            }
            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                return new Object[0];
            }
            public override bool IsDefined(Type attributeType, bool inherit)
            {
                return false;
            }
            #endregion

            #region Method Info overrides
            public override ParameterInfo ReturnParameter
            {
                get
                {
#pragma warning suppress 56503
                    throw new NotImplementedException();
                }
            }

            public override Type ReturnType
            {
                get
                {
                    if (this.isGetter)
                        return ((DesignTimeType)this.DeclaringType).ResolveType(DesignTimeType.GetTypeNameFromCodeTypeReference(this.property.CodeMemberProperty.Type, ((DesignTimeType)this.DeclaringType)));
                    return typeof(void);
                }
            }
            public override ICustomAttributeProvider ReturnTypeCustomAttributes
            {
                get
                {
#pragma warning suppress 56503
                    throw new NotImplementedException();
                }
            }
            public override MethodInfo GetBaseDefinition()
            {
                throw new NotImplementedException();
            }
            #endregion

            #region MethodBase Overrides
            public override ParameterInfo[] GetParameters()
            {
                if (this.parameters == null)
                {
                    // Get the parameters
                    CodeParameterDeclarationExpressionCollection parameters = this.property.CodeMemberProperty.Parameters;
                    ParameterInfo[] paramArray = new ParameterInfo[this.IsGetter ? parameters.Count : parameters.Count + 1];

                    for (int index = 0; index < parameters.Count; index++)
                    {
                        paramArray[index] = new DesignTimeParameterInfo(parameters[index], index, this.property);
                    }
                    if (!this.IsGetter)
                    {
                        CodeParameterDeclarationExpression valueParameter = new CodeParameterDeclarationExpression(this.property.CodeMemberProperty.Type.BaseType, "value");
                        valueParameter.Direction = FieldDirection.In;
                        paramArray[parameters.Count] = new DesignTimeParameterInfo(valueParameter, 0, this.property);
                    }
                    this.parameters = paramArray;
                }

                return this.parameters; // 
            }
            public override MethodImplAttributes GetMethodImplementationFlags()
            {
                return MethodImplAttributes.IL;
            }
            public override RuntimeMethodHandle MethodHandle
            {
                get
                {
                    // not interested in Runtime information
#pragma warning suppress 56503
                    throw new NotImplementedException(TypeSystemSR.GetString("Error_RuntimeNotSupported"));
                }
            }
            public override MethodAttributes Attributes
            {
                get
                {
                    return (Helper.ConvertToMethodAttributes(this.property.CodeMemberProperty.Attributes) |
                            MethodAttributes.SpecialName);
                }
            }
            #endregion
        }
        #endregion
    }
    #endregion
}
