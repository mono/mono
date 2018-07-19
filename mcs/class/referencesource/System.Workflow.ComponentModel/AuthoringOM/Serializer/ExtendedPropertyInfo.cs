namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.IO;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Collections;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Globalization;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Collections.ObjectModel;
    using System.Drawing;



    #region X:Key Support
    internal delegate object GetValueHandler(ExtendedPropertyInfo extendedProperty, object extendee);
    internal delegate void SetValueHandler(ExtendedPropertyInfo extendedProperty, object extendee, object value);
    internal delegate XmlQualifiedName GetQualifiedNameHandler(ExtendedPropertyInfo extendedProperty, WorkflowMarkupSerializationManager manager, out string prefix);

    #region Class ExtendedPropertyInfo
    internal sealed class ExtendedPropertyInfo : PropertyInfo
    {
        #region Members and Constructors
        private PropertyInfo realPropertyInfo = null;
        private GetValueHandler OnGetValue;
        private SetValueHandler OnSetValue;
        private GetQualifiedNameHandler OnGetXmlQualifiedName;
        private WorkflowMarkupSerializationManager manager = null;

        internal ExtendedPropertyInfo(PropertyInfo propertyInfo, GetValueHandler getValueHandler, SetValueHandler setValueHandler, GetQualifiedNameHandler qualifiedNameHandler)
        {
            this.realPropertyInfo = propertyInfo;
            this.OnGetValue = getValueHandler;
            this.OnSetValue = setValueHandler;
            this.OnGetXmlQualifiedName = qualifiedNameHandler;
        }

        internal ExtendedPropertyInfo(PropertyInfo propertyInfo, GetValueHandler getValueHandler, SetValueHandler setValueHandler, GetQualifiedNameHandler qualifiedNameHandler, WorkflowMarkupSerializationManager manager)
            : this(propertyInfo, getValueHandler, setValueHandler, qualifiedNameHandler)
        {
            this.manager = manager;
        }

        internal PropertyInfo RealPropertyInfo
        {
            get
            {
                return this.realPropertyInfo;
            }
        }

        internal WorkflowMarkupSerializationManager SerializationManager
        {
            get
            {
                return this.manager;
            }
        }
        #endregion

        #region Property Info overrides
        public override string Name
        {
            get
            {
                return this.realPropertyInfo.Name;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                return this.realPropertyInfo.DeclaringType;
            }
        }

        public override Type ReflectedType
        {
            get
            {
                return this.realPropertyInfo.ReflectedType;
            }
        }

        public override Type PropertyType
        {
            get
            {
                return this.realPropertyInfo.PropertyType;
            }
        }

        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            return this.realPropertyInfo.GetAccessors(nonPublic);
        }

        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            return this.realPropertyInfo.GetGetMethod(nonPublic);
        }

        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            return this.realPropertyInfo.GetSetMethod(nonPublic);
        }

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            if (OnGetValue != null)
                return OnGetValue(this, obj);
            else
                return null;
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            if (OnSetValue != null)
                OnSetValue(this, obj, value);
        }

        public XmlQualifiedName GetXmlQualifiedName(WorkflowMarkupSerializationManager manager, out string prefix)
        {
            prefix = String.Empty;
            if (OnGetXmlQualifiedName != null)
                return OnGetXmlQualifiedName(this, manager, out prefix);
            else
                return null;
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            return this.realPropertyInfo.GetIndexParameters();
        }

        public override PropertyAttributes Attributes
        {
            get
            {
                return this.realPropertyInfo.Attributes;
            }
        }

        public override bool CanRead
        {
            get
            {
                return this.realPropertyInfo.CanRead;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return this.realPropertyInfo.CanWrite;
            }
        }
        #endregion

        #region MemberInfo Overrides
        public override object[] GetCustomAttributes(bool inherit)
        {
            return this.realPropertyInfo.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return this.realPropertyInfo.GetCustomAttributes(attributeType, inherit);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return this.realPropertyInfo.IsDefined(attributeType, inherit);
        }
        #endregion

        #region  Helpers
        internal static bool IsExtendedProperty(WorkflowMarkupSerializationManager manager, XmlQualifiedName xmlQualifiedName)
        {
            bool isExtendedProperty = false;
            object extendee = manager.Context.Current;
            if (extendee != null)
            {
                foreach (ExtendedPropertyInfo extendedProperty in manager.GetExtendedProperties(extendee))
                {
                    string prefix = String.Empty;
                    XmlQualifiedName qualifiedPropertyName = extendedProperty.GetXmlQualifiedName(manager, out prefix);
                    if (qualifiedPropertyName.Name.Equals(xmlQualifiedName.Name, StringComparison.Ordinal)
                        && qualifiedPropertyName.Namespace.Equals(xmlQualifiedName.Namespace, StringComparison.Ordinal))
                    {
                        isExtendedProperty = true;
                        break;
                    }
                }
            }

            return isExtendedProperty;
        }
        internal static bool IsExtendedProperty(WorkflowMarkupSerializationManager manager, IList<PropertyInfo> propInfos, XmlQualifiedName xmlQualifiedName)
        {
            foreach (PropertyInfo propInfo in propInfos)
            {
                ExtendedPropertyInfo extendedProperty = propInfo as ExtendedPropertyInfo;
                if (extendedProperty == null)
                    continue;

                string prefix = String.Empty;
                XmlQualifiedName qualifiedPropertyName = extendedProperty.GetXmlQualifiedName(manager, out prefix);
                if (qualifiedPropertyName.Name.Equals(xmlQualifiedName.Name, StringComparison.Ordinal)
                    && qualifiedPropertyName.Namespace.Equals(xmlQualifiedName.Namespace, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
        #endregion
    }
    #endregion

    #endregion
}

