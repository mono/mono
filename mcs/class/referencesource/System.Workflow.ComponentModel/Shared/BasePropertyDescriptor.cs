// Copyright (c) 1999-2002 Microsoft Corporation. All rights reserved. 
//  
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// WHETHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
// WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// THE ENTIRE RISK OF USE OR RESULTS IN CONNECTION WITH THE USE OF THIS CODE 
// AND INFORMATION REMAINS WITH THE USER. 
//  
using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using System.Reflection;
using System.Globalization;
using System.Security;
using System.Security.Permissions;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Compiler;

/*********************************************************************
 * NOTE: A copy of this file exists at: WF\Activities\Common
 * The two files must be kept in sync.  Any change made here must also
 * be made to WF\Activities\Common\BasePropertyDescriptor.cs
*********************************************************************/

namespace System.Workflow.ComponentModel.Design
{
    #region Class PropertyDescriptorUtils
    internal static class PropertyDescriptorUtils
    {
        internal static ISite GetSite(IServiceProvider serviceProvider, object component)
        {
            ISite site = null;

            if (component != null)
            {
                if ((component is IComponent) && ((IComponent)component).Site != null)
                    site = ((IComponent)component).Site;

                if (site == null && component.GetType().IsArray && (component as object[]).Length > 0 && (component as object[])[0] is IComponent)
                    site = ((IComponent)(component as object[])[0]).Site;

                if (site == null && serviceProvider != null)
                {
                    IReferenceService referenceService = serviceProvider.GetService(typeof(IReferenceService)) as IReferenceService;
                    if (referenceService != null)
                    {
                        IComponent baseComponent = referenceService.GetComponent(component);
                        if (baseComponent != null)
                            site = baseComponent.Site;
                    }
                }
            }

            if (site == null)
                site = serviceProvider as ISite;

            return site;
        }

        internal static IComponent GetComponent(ITypeDescriptorContext context)
        {
            ISite site = (context != null) ? GetSite(context, context.Instance) : null;
            return (site != null) ? site.Component : null;
        }

        internal static Type GetBaseType(PropertyDescriptor property, object owner, IServiceProvider serviceProvider)
        {
            Type baseType = null;

            Type ownerType = owner.GetType();
            if (owner != null)
            {
                IDynamicPropertyTypeProvider basetypeProvider = owner as IDynamicPropertyTypeProvider;
                if (basetypeProvider != null)
                    baseType = basetypeProvider.GetPropertyType(serviceProvider, property.Name);
            }

            if (baseType == null)
                baseType = property.PropertyType;

            return baseType;
        }

        internal static void SetPropertyValue(IServiceProvider serviceProvider, PropertyDescriptor propertyDescriptor, object component, object value)
        {
            ComponentChangeDispatcher componentChange = new ComponentChangeDispatcher(serviceProvider, component, propertyDescriptor);
            try
            {
                propertyDescriptor.SetValue(component, value);
            }
            catch (Exception t)
            {
                // If there was a problem setting the controls property then we get:
                // ArgumentException (from properties set method)
                // ==> Becomes inner exception of TargetInvocationException
                // ==> caught here
                // Propagate the original exception up
                if (t is TargetInvocationException && t.InnerException != null)
                    throw t.InnerException;
                else
                    throw t;
            }
            finally
            {
                componentChange.Dispose();
            }
        }
    }
    #endregion

    #region Class ComponentChangeDispatcher
    internal sealed class ComponentChangeDispatcher : IDisposable
    {
        private IServiceProvider serviceProvider;
        private object component;
        private PropertyDescriptor property;
        private object oldValue;
        private object newValue;

        public ComponentChangeDispatcher(IServiceProvider serviceProvider, object component, PropertyDescriptor propertyDescriptor)
        {
            this.serviceProvider = serviceProvider;
            this.component = component;
            this.property = propertyDescriptor;

            IComponentChangeService changeService = serviceProvider.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            if (changeService != null)
            {
                try
                {
                    newValue = oldValue = propertyDescriptor.GetValue(component);
                    propertyDescriptor.AddValueChanged(component, new EventHandler(OnValueChanged));
                    changeService.OnComponentChanging(component, propertyDescriptor);
                }
                catch (CheckoutException coEx)
                {
                    if (coEx == CheckoutException.Canceled)
                        return;
                    throw coEx;
                }
            }
        }

        public void Dispose()
        {
            IComponentChangeService changeService = this.serviceProvider.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            if (changeService != null)
                changeService.OnComponentChanged(this.component, this.property, this.oldValue, this.newValue);
        }

        private void OnValueChanged(object sender, EventArgs e)
        {
            this.newValue = this.property.GetValue(this.component);
            this.property.RemoveValueChanged(this.component, new EventHandler(OnValueChanged));
        }
    }
    #endregion

    #region Class DynamicPropertyDescriptor
    internal class DynamicPropertyDescriptor : PropertyDescriptor
    {
        private IServiceProvider serviceProvider;
        private PropertyDescriptor realPropertyDescriptor;

        public DynamicPropertyDescriptor(IServiceProvider serviceProvider, PropertyDescriptor descriptor)
            : base(descriptor, null)
        {
            this.serviceProvider = serviceProvider;
            this.realPropertyDescriptor = descriptor;
        }

        public IServiceProvider ServiceProvider
        {
            get
            {
                return this.serviceProvider;
            }
        }

        public PropertyDescriptor RealPropertyDescriptor
        {
            get
            {
                return this.realPropertyDescriptor;
            }
        }

        public override string Category
        {
            get
            {
                return this.realPropertyDescriptor.Category;
            }
        }

        public override AttributeCollection Attributes
        {
            get
            {
                ArrayList attributes = new ArrayList();
                attributes.AddRange(this.realPropertyDescriptor.Attributes);
                attributes.Add(new MergablePropertyAttribute(false));
                return new AttributeCollection((Attribute[])attributes.ToArray(typeof(Attribute)));
            }
        }

        public override TypeConverter Converter
        {
            get
            {
                return this.realPropertyDescriptor.Converter;
            }
        }

        public override string Description
        {
            get
            {
                return this.realPropertyDescriptor.Description;
            }
        }

        public override string DisplayName
        {
            get
            {
                return this.realPropertyDescriptor.DisplayName;
            }
        }

        public override Type ComponentType
        {
            get
            {
                return this.realPropertyDescriptor.ComponentType;
            }
        }

        public override Type PropertyType
        {
            get
            {
                return this.realPropertyDescriptor.PropertyType;
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return this.realPropertyDescriptor.IsReadOnly;
            }
        }

        public override void ResetValue(object component)
        {
            this.realPropertyDescriptor.ResetValue(component);
        }

        public override bool CanResetValue(object component)
        {
            return this.realPropertyDescriptor.CanResetValue(component);
        }

        public override bool ShouldSerializeValue(object component)
        {
            // work around: The real property descriptor is returning false for all the
            // SequentialWorkflow class's properties because they all get replaced with 
            // InheritedPropertyDescriptors on Initialization (in the ComponentDesigner
            // base class), which is causing problems in the code-only serialization.
            if (string.Equals(this.realPropertyDescriptor.GetType().FullName, "System.ComponentModel.Design.InheritedPropertyDescriptor", StringComparison.Ordinal))
                return true;

            return this.realPropertyDescriptor.ShouldSerializeValue(component);
        }

        public override object GetValue(object component)
        {
            // When a child property is of type event, component could be null.
            if (component == null)
                return null;

            return this.realPropertyDescriptor.GetValue(component);
        }

        public override void SetValue(object component, object value)
        {
            if (component is IComponent)
                this.realPropertyDescriptor.SetValue(component, value);
            else
                PropertyDescriptorUtils.SetPropertyValue(ServiceProvider, this.realPropertyDescriptor, component, value);
        }
    }
    #endregion

    #region Class ParameterInfoBasedPropertyDescriptor

    internal class ParameterInfoBasedPropertyDescriptor : PropertyDescriptor
    {
        private Type componentType;
        private string desc = string.Empty;
        private bool avoidDuplication = false;
        private object parameter = null; // Could be either ParameterInfo or PropertyInfo
        private Type parameterType = null;
        private const string parameterPrefix = "(Parameter) ";

        internal ParameterInfoBasedPropertyDescriptor(Type componentType, ParameterInfo paramInfo, bool avoidDuplication, params Attribute[] attributes)
            : base((paramInfo.Position == -1) ? "(ReturnValue)" : paramInfo.Name, attributes)
        {
            if (componentType == null)
                throw new ArgumentNullException("componentType");

            if (paramInfo == null)
                throw new ArgumentNullException("paramInfo");

            if (paramInfo.ParameterType == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_ParameterTypeResolution, paramInfo.Name));

            this.componentType = componentType;
            this.parameter = paramInfo;
            this.avoidDuplication = avoidDuplication;
            this.parameterType = paramInfo.ParameterType;

            //Build and cache description
            string qualifier = String.Empty;
            if ((paramInfo.ParameterType != null) && (paramInfo.ParameterType.IsByRef || (paramInfo.IsIn && paramInfo.IsOut)))
                qualifier = SR.GetString(SR.Ref);
            else if (paramInfo.IsOut || paramInfo.Name == null)
                qualifier = SR.GetString(SR.Out);
            else
                qualifier = SR.GetString(SR.In);
            this.desc = SR.GetString(SR.ParameterDescription, paramInfo.ParameterType.FullName);
        }


        internal ParameterInfoBasedPropertyDescriptor(Type componentType, string propertyName, Type propertyType, bool avoidDuplication, params Attribute[] attributes)
            : base(propertyName, attributes)
        {
            if (componentType == null)
                throw new ArgumentNullException("componentType");

            if (propertyType == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_ParameterTypeResolution, propertyName));

            this.componentType = componentType;
            this.parameterType = propertyType;
            this.avoidDuplication = avoidDuplication;
            this.desc = SR.GetString(SR.InvokeParameterDescription, propertyType.FullName.ToString());
        }

        internal Type ParameterType
        {
            get
            {
                Type type = this.parameterType;
                if (type.IsByRef)
                    type = type.GetElementType();
                return type;
            }
        }

        public override string Description
        {
            get
            {
                return this.desc;
            }
        }

        public override string Category
        {
            get
            {
                return SR.GetString(SR.Parameters);
            }
        }

        public override object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(PropertyType, editorBaseType);
        }

        public override string DisplayName
        {
            get
            {
                return this.Name;
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public override Type ComponentType
        {
            get
            {
                return this.componentType;
            }
        }

        public override string Name
        {
            get
            {
                if (this.avoidDuplication)
                {
                    // WinOE Bug 10442: should only prefix with "(Parameter)" if there is and existing
                    // member of the same name.
                    return GetParameterPropertyName(this.componentType, base.Name);
                }
                else
                    return base.Name;
            }
        }

        internal static MemberInfo FindMatchingMember(string name, Type ownerType, bool ignoreCase)
        {
            MemberInfo matchingMember = null;
            foreach (MemberInfo memberInfo in ownerType.GetMembers(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (memberInfo.Name.Equals(name, ((ignoreCase) ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture)))
                {
                    matchingMember = memberInfo;
                    break;
                }
            }
            return matchingMember;
        }

        public override Type PropertyType
        {
            get
            {
                Type propertyType = ParameterType;
                if (propertyType == null)
                    propertyType = typeof(ActivityBind);
                return propertyType;
            }
        }

        public override TypeConverter Converter
        {
            get
            {
                //We return this to make sure that we can bind parameters through UI
                return new ActivityBindTypeConverter();
            }
        }

        public override AttributeCollection Attributes
        {
            get
            {
                ArrayList attributes = new ArrayList();
                attributes.AddRange(base.Attributes);
                attributes.AddRange(TypeDescriptor.GetAttributes(PropertyType));
                return new AttributeCollection((Attribute[])attributes.ToArray(typeof(Attribute)));
            }
        }

        public override void ResetValue(object component)
        {
            if (PropertyType != null && !PropertyType.IsValueType)
                SetValue(component, null);
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        public override void SetValue(object component, object value)
        {
            // the logic for notifications is borrowed from ReflectPropertyDescritpor
            if (component == null)
                return;

            IServiceProvider serviceProvider = GetSite(component);
            ComponentChangeDispatcher componentChange = (serviceProvider != null) ? new ComponentChangeDispatcher(serviceProvider, component, this) : null;

            try
            {
                WorkflowParameterBindingCollection parameters = GetParameterBindings(component);
                if (parameters != null)
                {
                    string propertyName = String.Empty;
                    if (this.Name.StartsWith(parameterPrefix, StringComparison.Ordinal))
                        propertyName = this.Name.Substring(parameterPrefix.Length);
                    else
                        propertyName = this.Name;

                    WorkflowParameterBinding binding = null;
                    if (parameters.Contains(propertyName))
                        binding = parameters[propertyName];
                    else
                    {
                        binding = new WorkflowParameterBinding(propertyName);
                        parameters.Add(binding);
                    }

                    if (value is ActivityBind)
                        binding.SetBinding(WorkflowParameterBinding.ValueProperty, value as ActivityBind);
                    else
                        binding.SetValue(WorkflowParameterBinding.ValueProperty, value);

                    OnValueChanged(component, EventArgs.Empty);
                }
            }
            catch (Exception t)
            {
                // If there was a problem setting the controls property then we get:
                // ArgumentException (from properties set method)
                // ==> Becomes inner exception of TargetInvocationException
                // ==> caught here
                // Propagate the original exception up
                if (t is TargetInvocationException && t.InnerException != null)
                    throw t.InnerException;
                else
                    throw t;
            }
            finally
            {
                // Now notify the change service that the change was successful.
                if (componentChange != null)
                    componentChange.Dispose();
            }
        }

        public override object GetValue(object component)
        {
            WorkflowParameterBindingCollection parameters = GetParameterBindings(component);
            string displayName = this.Name;
            string propertyName = (displayName.StartsWith(parameterPrefix, StringComparison.Ordinal)) ? displayName.Substring(parameterPrefix.Length) : displayName;
            if (parameters != null && parameters.Contains(propertyName))
            {
                if (parameters[propertyName].IsBindingSet(WorkflowParameterBinding.ValueProperty))
                    return parameters[propertyName].GetBinding(WorkflowParameterBinding.ValueProperty);
                else
                    return parameters[propertyName].GetValue(WorkflowParameterBinding.ValueProperty);
            }

            return null;
        }

        private WorkflowParameterBindingCollection GetParameterBindings(object component)
        {
            WorkflowParameterBindingCollection retVal = null;
            MemberInfo memberInfo = component.GetType().GetProperty("ParameterBindings", BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.ExactBinding, null, typeof(WorkflowParameterBindingCollection), new Type[] { }, new ParameterModifier[] { });
            if (memberInfo != null)
                retVal = component.GetType().InvokeMember("ParameterBindings", BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.ExactBinding, null, component, new object[] { }, CultureInfo.InvariantCulture) as WorkflowParameterBindingCollection;
            return retVal;
        }

        public static string GetParameterPropertyName(Type componentType, string paramName)
        {
            string paramPropertyName = paramName;
            if (FindMatchingMember(paramName, componentType, false) != null)
                paramPropertyName = parameterPrefix + paramName;

            return paramPropertyName;
        }
    }
    #endregion

    #region IPropertyValueProvider Interface
    internal interface IPropertyValueProvider
    {
        ICollection GetPropertyValues(ITypeDescriptorContext typeDescriptorContext);
    }
    #endregion

    #region Class PropertyValueProviderTypeConverter
    internal class PropertyValueProviderTypeConverter : TypeConverter
    {
        public PropertyValueProviderTypeConverter()
        {
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            // grab all property values
            IPropertyValueProvider valuesProvider = null;
            object[] instances = context.Instance as object[];
            if (instances != null && instances.Length > 0)
                valuesProvider = instances[0] as IPropertyValueProvider;
            else
                valuesProvider = context.Instance as IPropertyValueProvider;

            ICollection values = new object[] { };
            if (valuesProvider != null)
                values = valuesProvider.GetPropertyValues(context);

            return new StandardValuesCollection(values);
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }
    }
    #endregion

    #region Class TypePropertyDescriptor
    internal class TypePropertyDescriptor : DynamicPropertyDescriptor
    {
        public TypePropertyDescriptor(IServiceProvider serviceProvider, PropertyDescriptor actualPropDesc)
            : base(serviceProvider, actualPropDesc)
        {
        }

        public override TypeConverter Converter
        {
            get
            {
                // work around: a direct type comparison would not work because the types in this file are compiled
                // into the component model and the activities dll separately.  The assembly name differs thus
                // the comparison fails.  Work around by getting the type from the same assembly before doing
                // the comparison.
                TypeConverter baseTypeConverter = base.Converter;
                string baseConverterTypeName = baseTypeConverter.GetType().FullName;
                Type baseConverterType = Assembly.GetExecutingAssembly().GetType(baseConverterTypeName);
                if (baseConverterType != null && typeof(TypePropertyTypeConverter).IsAssignableFrom(baseConverterType))
                    return baseTypeConverter;
                else
                    return new TypePropertyTypeConverter();
            }
        }

        public override object GetValue(object component)
        {
            if (component == null)
                throw new ArgumentNullException("component");

            object value = base.GetValue(component);

            if (value == null)
            {
                // See if there is a value in the user data DesignTimeTypeNames hashtable. 
                // If yes, it's probably a wrong type name.  Show the name anyway.
                DependencyObject dependencyObject = component as DependencyObject;
                if (dependencyObject != null)
                {
                    object key = DependencyProperty.FromName(this.RealPropertyDescriptor.Name, this.RealPropertyDescriptor.ComponentType);
                    value = Helpers.GetDesignTimeTypeName(dependencyObject, key);
                    if (string.IsNullOrEmpty(value as string))
                    {
                        key = this.RealPropertyDescriptor.ComponentType.FullName + "." + this.RealPropertyDescriptor.Name;
                        value = Helpers.GetDesignTimeTypeName(dependencyObject, key);
                    }
                }
            }

            return value;
        }

        public override void SetValue(object component, object value)
        {
            if (component == null)
                throw new ArgumentNullException("component");

            if (value != null)
            {
                Type type = value as Type;
                ITypeFilterProvider filterProvider = PropertyDescriptorUtils.GetComponent(new TypeDescriptorContext(ServiceProvider, RealPropertyDescriptor, component)) as ITypeFilterProvider;
                if (filterProvider != null)
                    filterProvider.CanFilterType(type, true); //this will throw an exception if the type is not correctly filterable
            }

            base.SetValue(component, value);
        }
    }
    #endregion

    #region Class TypePropertyValueProviderTypeConverter
    internal class TypePropertyValueProviderTypeConverter : TypePropertyTypeConverter
    {
        // NOTE: Copied from PropertyValueProviderTypeConverter.
        // The purpose of this type converter is so that we can both provide standard values and
        // convert System.Type to string.
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            // grab all property values
            IPropertyValueProvider valuesProvider = null;
            object[] instances = context.Instance as object[];
            if (instances != null && instances.Length > 0)
                valuesProvider = instances[0] as IPropertyValueProvider;
            else
                valuesProvider = context.Instance as IPropertyValueProvider;

            ICollection values = new object[] { };
            if (valuesProvider != null && context != null)
                values = valuesProvider.GetPropertyValues(context);

            return new StandardValuesCollection(values);
        }
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }
    }
    #endregion

    #region Class TypePropertyTypeConverter
    internal class TypePropertyTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == null)
                throw new ArgumentNullException("sourceType");

            if (TypeDescriptor.Equals(sourceType, typeof(string)))
                return true;

            return base.CanConvertFrom(context, sourceType);

        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == null)
                throw new ArgumentNullException("destinationType");

            if (TypeDescriptor.Equals(destinationType, typeof(Type)))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object valueToConvert)
        {
            string typeName = valueToConvert as string;
            if (String.IsNullOrEmpty(typeName))
                return null;

            if (context != null)
            {
                ITypeProvider typeProvider = context.GetService(typeof(ITypeProvider)) as ITypeProvider;
                if (typeProvider != null)
                    return typeProvider.GetType(typeName, true);
            }

            return base.ConvertFrom(context, culture, valueToConvert);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is Type && TypeDescriptor.Equals(destinationType, typeof(string)))
                return ((Type)value).FullName;

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
    #endregion
}
