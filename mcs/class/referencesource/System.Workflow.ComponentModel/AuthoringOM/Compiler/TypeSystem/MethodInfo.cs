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

    #region DesignTimeConstructorInfo
    /// <summary>
    /// This class takes care of static and non-static constructors
    /// </summary>
    internal sealed class DesignTimeConstructorInfo : ConstructorInfo
    {
        #region Members and Constructors

        private CodeMemberMethod codeConstructor = null;
        // Data associated with a bound ctor
        private DesignTimeType declaringType = null;
        // Data associated with this ctor
        private ParameterInfo[] parameters = null;
        private Attribute[] attributes = null;

        internal DesignTimeConstructorInfo(DesignTimeType declaringType, CodeMemberMethod codeConstructor)
        {
            this.declaringType = declaringType;
            this.codeConstructor = codeConstructor;
        }

        #endregion

        #region ConstructorInfo overrides
        public override Object Invoke(BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture)
        {
            throw new NotImplementedException(TypeSystemSR.GetString("Error_RuntimeNotSupported"));
        }

        #endregion

        #region MethodBase Overrides
        public override ParameterInfo[] GetParameters()
        {
            if (this.parameters == null)
            {
                // Get the parameters
                CodeParameterDeclarationExpressionCollection parameters = codeConstructor.Parameters;
                ParameterInfo[] paramArray = new ParameterInfo[parameters.Count];

                for (int index = 0; index < parameters.Count; index++)
                {
                    paramArray[index] = new DesignTimeParameterInfo(parameters[index], index, this);
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
                return Helper.ConvertToMethodAttributes(this.codeConstructor.Attributes);
            }
        }
        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            throw new NotImplementedException(TypeSystemSR.GetString("Error_RuntimeNotSupported"));
        }

        #endregion

        #region MemberInfo Overrides
        public override string Name
        {
            get
            {
                return ".ctor";
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
        public override object[] GetCustomAttributes(bool inherit)
        {
            return GetCustomAttributes(typeof(object), inherit);
        }
        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (attributeType == null)
                throw new ArgumentNullException("attributeType");

            if (this.attributes == null)
                this.attributes = Helper.LoadCustomAttributes(this.codeConstructor.CustomAttributes, this.DeclaringType as DesignTimeType);

            return Helper.GetCustomAttributes(attributeType, inherit, this.attributes, this);
        }
        public override bool IsDefined(Type attributeType, bool inherit)
        {
            if (attributeType == null)
                throw new ArgumentNullException("attributeType");

            if (this.attributes == null)
                this.attributes = Helper.LoadCustomAttributes(this.codeConstructor.CustomAttributes, this.DeclaringType as DesignTimeType);

            if (Helper.IsDefined(attributeType, inherit, attributes, this))
                return true;

            return false;
        }

        #endregion
    }

    #endregion

    #region DesignTimeMethodInfo
    internal class DesignTimeMethodInfo : MethodInfo
    {
        #region Members and Constructors

        private CodeMemberMethod methodInfo;
        private ParameterInfo[] parameters;
        // Data assocaited with a bound object
        private DesignTimeType declaringType;
        private Attribute[] attributes = null;
        private ParameterInfo returnParam = null;
        private bool isSpecialName = false;

        internal DesignTimeMethodInfo(DesignTimeType declaringType, CodeMemberMethod methodInfo, bool isSpecialName)
        {
            this.declaringType = declaringType;
            this.methodInfo = methodInfo;
            this.isSpecialName = isSpecialName;
        }

        internal DesignTimeMethodInfo(DesignTimeType declaringType, CodeMemberMethod methodInfo)
        {
            this.declaringType = declaringType;
            this.methodInfo = methodInfo;
        }

        #endregion

        #region Method Info overrides
        public override Type ReturnType
        {
            get
            {
                return declaringType.ResolveType(DesignTimeType.GetTypeNameFromCodeTypeReference(this.methodInfo.ReturnType, declaringType));
            }
        }
        public override ICustomAttributeProvider ReturnTypeCustomAttributes
        {
            get
            {
                return null;
            }
        }
        public override MethodInfo GetBaseDefinition()
        {
            throw new NotImplementedException();
        }

        public override ParameterInfo ReturnParameter
        {
            get
            {
                if (this.returnParam == null)
                    this.returnParam = new DesignTimeParameterInfo(this.methodInfo.ReturnType, this);
                return this.returnParam;
            }
        }

        #endregion

        #region MethodBase Overrides
        public override ParameterInfo[] GetParameters()
        {
            if (this.parameters == null)
            {
                // Get the parameters
                CodeParameterDeclarationExpressionCollection parameters = this.methodInfo.Parameters;
                ParameterInfo[] paramArray = new ParameterInfo[parameters.Count];

                for (int index = 0; index < parameters.Count; index++)
                {
                    paramArray[index] = new DesignTimeParameterInfo(parameters[index], index, this);
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
                return Helper.ConvertToMethodAttributes(this.methodInfo.Attributes) | (this.isSpecialName ? MethodAttributes.SpecialName : 0);
            }
        }
        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            throw new NotImplementedException(TypeSystemSR.GetString("Error_RuntimeNotSupported"));
        }

        #endregion

        #region MemberInfo Overrides
        public override string Name
        {
            get
            {
                return Helper.EnsureTypeName(this.methodInfo.Name);
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
        public override object[] GetCustomAttributes(bool inherit)
        {
            return GetCustomAttributes(typeof(object), inherit);
        }
        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (attributeType == null)
                throw new ArgumentNullException("attributeType");

            if (this.attributes == null)
                if (this.methodInfo == null)
                    this.attributes = new Attribute[0];
                else
                    this.attributes = Helper.LoadCustomAttributes(this.methodInfo.CustomAttributes, this.DeclaringType as DesignTimeType);

            return Helper.GetCustomAttributes(attributeType, inherit, this.attributes, this);
        }
        public override bool IsDefined(Type attributeType, bool inherit)
        {
            if (attributeType == null)
                throw new ArgumentNullException("attributeType");

            if (this.attributes == null)
                this.attributes = Helper.LoadCustomAttributes(this.methodInfo.CustomAttributes, this.DeclaringType as DesignTimeType);

            if (Helper.IsDefined(attributeType, inherit, attributes, this))
                return true;

            return false;
        }

        #endregion
    }
    #endregion

}
