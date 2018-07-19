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

    #region DesignTimeFieldInfo
    internal sealed class DesignTimeFieldInfo : FieldInfo
    {
        #region Members and Constructors

        private Attribute[] attributes = null;
        private FieldAttributes fieldAttributes;
        private DesignTimeType declaringType;
        private CodeMemberField codeDomField;

        internal DesignTimeFieldInfo(DesignTimeType declaringType, CodeMemberField codeDomField)
        {
            if (declaringType == null)
            {
                throw new ArgumentNullException("Declaring Type");
            }

            if (codeDomField == null)
            {
                throw new ArgumentNullException("codeDomEvent");
            }

            this.declaringType = declaringType;
            this.codeDomField = codeDomField;
            fieldAttributes = Helper.ConvertToFieldAttributes(codeDomField.Attributes);
        }

        #endregion

        #region FieldInfo overrides
        public override RuntimeFieldHandle FieldHandle
        {
            get
            {
                // not interested in Runtime information
#pragma warning suppress 56503
                throw new NotImplementedException(TypeSystemSR.GetString("Error_RuntimeNotSupported"));
            }
        }
        public override Type FieldType
        {
            get
            {
                return declaringType.ResolveType(DesignTimeType.GetTypeNameFromCodeTypeReference(this.codeDomField.Type, declaringType));
            }
        }
        public override Object GetValue(object obj)
        {
            // We don't need to get into instance probing
            throw new NotImplementedException(TypeSystemSR.GetString("Error_RuntimeNotSupported"));
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
        {
            // We don't need to get into instance probing
            throw new NotImplementedException(TypeSystemSR.GetString("Error_RuntimeNotSupported"));
        }

        public override FieldAttributes Attributes
        {
            get { return this.fieldAttributes; }
        }

        #endregion

        #region MemberInfo Overrides
        public override string Name
        {
            get
            {
                return Helper.EnsureTypeName(this.codeDomField.Name);
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
                this.attributes = Helper.LoadCustomAttributes(this.codeDomField.CustomAttributes, this.DeclaringType as DesignTimeType);

            return Helper.GetCustomAttributes(attributeType, inherit, this.attributes, this);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            if (attributeType == null)
                throw new ArgumentNullException("attributeType");

            if (this.attributes == null)
                this.attributes = Helper.LoadCustomAttributes(this.codeDomField.CustomAttributes, this.DeclaringType as DesignTimeType);

            if (Helper.IsDefined(attributeType, inherit, attributes, this))
                return true;

            return false;
        }

        #endregion
    }
    #endregion
}
