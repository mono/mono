namespace System.Workflow.ComponentModel.Design
{
    #region Imports

    using System;
    using System.IO;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.CodeDom;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Serialization;

    using System.Drawing.Design;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Globalization;

    #endregion

    #region Class IDPropertyDescriptor
    internal sealed class IDPropertyDescriptor : DynamicPropertyDescriptor
    {
        internal IDPropertyDescriptor(IServiceProvider serviceProvider, PropertyDescriptor actualPropDesc)
            : base(serviceProvider, actualPropDesc)
        {
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override void SetValue(object component, object value)
        {
            Activity activity = component as Activity;
            if (activity != null)
            {
                ISite site = PropertyDescriptorUtils.GetSite(ServiceProvider, component);
                if (site == null)
                    throw new Exception(SR.GetString(SR.General_MissingService, typeof(ISite).FullName));

                IIdentifierCreationService identifierCreationService = site.GetService(typeof(IIdentifierCreationService)) as IIdentifierCreationService;
                if (identifierCreationService == null)
                    throw new Exception(SR.GetString(SR.General_MissingService, typeof(IIdentifierCreationService).FullName));

                string newID = value as string;
                identifierCreationService.ValidateIdentifier(activity, newID);

                DesignerHelpers.UpdateSiteName(activity, newID);
                base.SetValue(component, value);
            }
        }
    }
    #endregion

    #region Class NamePropertyDescriptor
    internal sealed class NamePropertyDescriptor : DynamicPropertyDescriptor
    {
        internal NamePropertyDescriptor(IServiceProvider serviceProvider, PropertyDescriptor actualPropDesc)
            : base(serviceProvider, actualPropDesc)
        {
        }

        public override string Category
        {
            get
            {
                return SR.GetString(SR.Activity);
            }
        }

        public override string Description
        {
            get
            {
                return SR.GetString(SR.RootActivityNameDesc);
            }
        }

        public override void SetValue(object component, object value)
        {
            Activity activity = component as Activity;
            if (activity != null)
            {
                // validate the identifier
                IIdentifierCreationService identifierCreationService = activity.Site.GetService(typeof(IIdentifierCreationService)) as IIdentifierCreationService;
                if (identifierCreationService == null)
                    throw new Exception(SR.GetString(SR.General_MissingService, typeof(IIdentifierCreationService).FullName));

                string name = value as string;
                identifierCreationService.ValidateIdentifier(activity, name);

                bool isVB = (CompilerHelpers.GetSupportedLanguage(activity.Site) == SupportedLanguages.VB);
                Type designedType = Helpers.GetDataSourceClass(Helpers.GetRootActivity(activity), activity.Site);
                if (designedType != null)
                {
                    MemberInfo matchingMember = ActivityBindPropertyDescriptor.FindMatchingMember(name, designedType, isVB);
                    if (matchingMember != null)
                        throw new ArgumentException(SR.GetString(SR.Error_ActivityNameExist, name));
                }
                IMemberCreationService memberCreationService = activity.Site.GetService(typeof(IMemberCreationService)) as IMemberCreationService;
                if (memberCreationService == null)
                    throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(IMemberCreationService).FullName));

                IDesignerHost host = activity.Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (host == null)
                    throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(IDesignerHost).FullName));

                // We need to update the activityType's name before trying to update the type because
                // updating the type causes a flush, which access the custom activity's properties, and 
                // doing so requires the new type name
                string newClassName = name;
                int indexOfDot = host.RootComponentClassName.LastIndexOf('.');
                if (indexOfDot > 0)
                    newClassName = host.RootComponentClassName.Substring(0, indexOfDot + 1) + name;

                // IMPORTANT: You must update the class name in code before renaming the site, since
                // VS's OnComponentRename updates the RootComponentClassName, so the flush code called
                // in our OnComponentRename tries to access the new class for information.
                memberCreationService.UpdateTypeName(((Activity)host.RootComponent).GetValue(WorkflowMarkupSerializer.XClassProperty) as string, newClassName);

                //((Activity)host.RootComponent).Name = name;
                ((Activity)host.RootComponent).SetValue(WorkflowMarkupSerializer.XClassProperty, newClassName);
                base.SetValue(component, value);

                // Update the site name so the component name shows up correctly in the designer
                DesignerHelpers.UpdateSiteName((Activity)host.RootComponent, name);
            }
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }
    }
    #endregion

    #region Class PropertyDescriptorUtils
    internal static class PropertyDescriptorFilter
    {
        internal static PropertyDescriptorCollection FilterProperties(IServiceProvider serviceProvider, object propertyOwner, PropertyDescriptorCollection props)
        {
            Hashtable properties = new Hashtable();
            foreach (PropertyDescriptor prop in props)
            {
                if (!properties.ContainsKey(prop.Name))
                    properties.Add(prop.Name, prop);
            }

            FilterProperties(serviceProvider, propertyOwner, properties);

            PropertyDescriptor[] returnProps = new PropertyDescriptor[properties.Count];
            properties.Values.CopyTo(returnProps, 0);
            return new PropertyDescriptorCollection(returnProps);
        }

        internal static void FilterProperties(IServiceProvider serviceProvider, object propertyOwner, IDictionary props)
        {
            InternalFilterProperties(serviceProvider, propertyOwner, props);

            if (propertyOwner != null)
            {
                foreach (PropertyDescriptor property in GetPropertiesForEvents(serviceProvider, propertyOwner))
                {
                    if (!props.Contains(property.Name))
                        props.Add(property.Name, property);
                }
            }
        }

        private static void InternalFilterProperties(IServiceProvider serviceProvider, object propertyOwner, IDictionary properties)
        {
            // change property descriptors
            Hashtable newProperties = new Hashtable();
            foreach (object key in properties.Keys)
            {
                PropertyDescriptor propDesc = properties[key] as PropertyDescriptor;
                if (string.Equals(propDesc.Name, "Name", StringComparison.Ordinal) && typeof(Activity).IsAssignableFrom(propDesc.ComponentType))
                {
                    //Activity id
                    Activity activity = propertyOwner as Activity;
                    if (activity != null && activity == Helpers.GetRootActivity(activity))
                        newProperties[key] = new NamePropertyDescriptor(serviceProvider, propDesc);
                    else
                        newProperties[key] = new IDPropertyDescriptor(serviceProvider, propDesc);
                }
                else if (!(propDesc is ActivityBindPropertyDescriptor) && ActivityBindPropertyDescriptor.IsBindableProperty(propDesc))
                {
                    if (typeof(Type).IsAssignableFrom(propDesc.PropertyType) && !(propDesc is ParameterInfoBasedPropertyDescriptor))
                        propDesc = new TypePropertyDescriptor(serviceProvider, propDesc);
                    newProperties[key] = new ActivityBindPropertyDescriptor(serviceProvider, propDesc, propertyOwner);
                }
                else if (typeof(Type).IsAssignableFrom(propDesc.PropertyType))
                {
                    newProperties[key] = new TypePropertyDescriptor(serviceProvider, propDesc);
                }
                else
                {
                    newProperties[key] = new DynamicPropertyDescriptor(serviceProvider, propDesc);
                }
            }

            foreach (object key in newProperties.Keys)
            {
                properties[key] = newProperties[key];
            }
        }

        internal static PropertyDescriptorCollection GetPropertiesForEvents(IServiceProvider serviceProvider, object eventOwner)
        {
            //Now for each event we need to add properties
            List<PropertyDescriptor> properties = new List<PropertyDescriptor>();

            // Find out if there is a data context.
            IEventBindingService eventBindingService = serviceProvider.GetService(typeof(IEventBindingService)) as IEventBindingService;
            if (eventBindingService != null)
            {
                foreach (EventDescriptor eventDesc in TypeDescriptor.GetEvents(eventOwner))
                {
                    if (eventDesc.IsBrowsable)
                    {
                        PropertyDescriptor propertyDescriptor = eventBindingService.GetEventProperty(eventDesc);
                        if (!(propertyDescriptor is ActivityBindPropertyDescriptor) && ActivityBindPropertyDescriptor.IsBindableProperty(propertyDescriptor))
                            properties.Add(new ActivityBindPropertyDescriptor(serviceProvider, propertyDescriptor, eventOwner));
                        else
                            properties.Add(propertyDescriptor);
                    }
                }
            }

            return new PropertyDescriptorCollection(properties.ToArray());
        }
    }
    #endregion

    #region ActivityBind PropertyBrowser Integration

    #region Class ActivityBindPropertyDescriptor
    /// Please note that ActivityBindPropertyDescriptor is now changed so that it can support MetaProperty binds too.
    /// Although this feature is not yet enabled. Please change code in the PropertyDescritorUtils.InternalFilterProperties to support it

    /// We need this property descriptor for following reason:
    /// When we are not in data context we support emission of events directly by using EventBindingService
    /// When we do this the events dont get stored in the DependencyObject rather than we store them in userdata
    /// Only the event property descriptor knows how to get it back.
    /// At the same time we also support Promotion of the events (ActivityBind) when using this the information gets
    /// stored directly in the DependencyObject which we fetch from the ActivityBindPropertyDescriptor base class

    /// Whenever the code in this class is changed please run the following test cases
    /// 1. In ActivityDesigner Drop Code and Promote Properties. (ExecuteCode should get promoted)
    /// 2. In ActivityDesigner, change the promoted property to event handler (Event needs to be emitted with += syntax)
    /// 3. In ActivityDesigner, Set the data context to true and try to generate the event for code. (Event should be emitted in DataContext)
    /// 4. In ActivityDesigner, make sure that when within data context the serialization works through the bind, when outside works through +=
    /// Try all of the above cases in WorkflowDesigner where root supports datacontext, roots that dont support data context
    internal class ActivityBindPropertyDescriptor : DynamicPropertyDescriptor
    {
        private object propertyOwner = null;

        internal ActivityBindPropertyDescriptor(IServiceProvider serviceProvider, PropertyDescriptor realPropertyDescriptor, object propertyOwner)
            : base(serviceProvider, realPropertyDescriptor)
        {
            this.propertyOwner = propertyOwner;
        }

        public override bool IsReadOnly
        {
            get
            {
                return RealPropertyDescriptor.IsReadOnly;
            }
        }

        public override TypeConverter Converter
        {
            get
            {
                TypeConverter baseTypeConverter = base.Converter;
                if (typeof(ActivityBindTypeConverter).IsAssignableFrom(baseTypeConverter.GetType()))
                    return baseTypeConverter;
                else
                    return new ActivityBindTypeConverter();
            }
        }

        public override AttributeCollection Attributes
        {
            get
            {
                List<Attribute> attributes = new List<Attribute>();
                foreach (Attribute attribute in base.Attributes)
                    attributes.Add(attribute);

                object uiTypeEditor = RealPropertyDescriptor.GetEditor(typeof(UITypeEditor));
                object value = (PropertyOwner != null) ? GetValue(PropertyOwner) : null;
                bool propertiesSupported = RealPropertyDescriptor.Converter.GetPropertiesSupported((PropertyOwner != null) ? new TypeDescriptorContext(ServiceProvider, RealPropertyDescriptor, PropertyOwner) : null);
                if (((uiTypeEditor == null && !propertiesSupported) || value is ActivityBind) && !IsReadOnly)
                    attributes.Add(new EditorAttribute(typeof(BindUITypeEditor), typeof(UITypeEditor)));

                return new AttributeCollection(attributes.ToArray());
            }
        }

        public override object GetEditor(Type editorBaseType)
        {
            //If the converter is simple type converter and there is a simple UITypeEditor
            object editor = base.GetEditor(editorBaseType);
            if (editorBaseType == typeof(UITypeEditor) && !IsReadOnly)
            {
                object value = (PropertyOwner != null) ? GetValue(PropertyOwner) : null;
                bool propertiesSupported = RealPropertyDescriptor.Converter.GetPropertiesSupported((PropertyOwner != null) ? new TypeDescriptorContext(ServiceProvider, RealPropertyDescriptor, PropertyOwner) : null);
                if (value is ActivityBind || (editor == null && !propertiesSupported))
                    editor = new BindUITypeEditor();
            }
            return editor;
        }

        public override object GetValue(object component)
        {
            object value = null;

            DependencyObject dependencyObj = component as DependencyObject;
            DependencyProperty dependencyProperty = DependencyProperty.FromName(Name, ComponentType);
            if (dependencyObj != null && dependencyProperty != null)
            {
                if (dependencyObj.IsBindingSet(dependencyProperty))
                    value = dependencyObj.GetBinding(dependencyProperty);
            }

            //We have to also call base's getvalue as Bindings are stored in MetaProperties collection of DependencyObject but
            //actual values are stored in DependencyValueProperties
            if (!(value is ActivityBind))
                value = base.GetValue(component);

            return value;
        }

        public override void SetValue(object component, object value)
        {
            object oldValue = GetValue(component);

            ActivityBind activityBind = value as ActivityBind;

            DependencyObject dependencyObj = component as DependencyObject;
            DependencyProperty dependencyProperty = DependencyProperty.FromName(Name, ComponentType);
            if (dependencyObj != null && dependencyProperty != null && activityBind != null)
            {
                ComponentChangeDispatcher componentChangeDispatcher = new ComponentChangeDispatcher(ServiceProvider, dependencyObj, this);
                try
                {
                    if (dependencyProperty.IsEvent && ServiceProvider != null)
                    {
                        IEventBindingService eventBindingService = ServiceProvider.GetService(typeof(IEventBindingService)) as IEventBindingService;
                        if (eventBindingService != null)
                        {
                            EventDescriptor eventDescriptor = eventBindingService.GetEvent(RealPropertyDescriptor);
                            if (eventDescriptor != null)
                                RealPropertyDescriptor.SetValue(component, null);
                        }
                    }

                    dependencyObj.SetBinding(dependencyProperty, activityBind);
                    base.OnValueChanged(dependencyObj, EventArgs.Empty);
                }
                finally
                {
                    componentChangeDispatcher.Dispose();
                }
            }
            else
            {
                if (dependencyObj != null && dependencyProperty != null && dependencyObj.IsBindingSet(dependencyProperty))
                {
                    ComponentChangeDispatcher componentChangeDispatcher = new ComponentChangeDispatcher(ServiceProvider, dependencyObj, this);
                    try
                    {
                        dependencyObj.RemoveProperty(dependencyProperty);
                        // Need to fire component changed event because this means we're clearing
                        // out a previously set Bind value.  If the new value matches the old value stored in the user data, 
                        // base.SetValue will do nothing but return.  When that happens, if we don't fire a change
                        // event here, we'll still have the activity bind in the code or xoml file.
                        base.OnValueChanged(dependencyObj, EventArgs.Empty);
                    }
                    finally
                    {
                        componentChangeDispatcher.Dispose();
                    }
                }

                base.SetValue(component, value);
            }

            //Following code is for making sure that when we change the value from activity bind to actual value
            //and from actual value to activity bind; we need to change the UITypeEditor associated with property
            //from data binding editor to the editor specified by the user
            if (oldValue != value &&
                ((oldValue is ActivityBind && !(value is ActivityBind)) ||
                 (!(oldValue is ActivityBind) && value is ActivityBind)))
            {
                TypeDescriptor.Refresh(component);
            }
        }

        #region Helpers
        internal object PropertyOwner
        {
            get
            {
                return this.propertyOwner;
            }
        }
        #endregion

        #region Static Helpers
        internal static IList<MemberInfo> GetBindableMembers(object obj, ITypeDescriptorContext context)
        {
            List<MemberInfo> memberInfos = new List<MemberInfo>();

            IDesignerHost designerHost = context.GetService(typeof(IDesignerHost)) as IDesignerHost;
            Activity rootActivity = (designerHost != null) ? designerHost.RootComponent as Activity : null;
            Type objectType = (obj == rootActivity) ? Helpers.GetDataSourceClass(rootActivity, context) : obj.GetType();

            Type memberType = PropertyDescriptorUtils.GetBaseType(context.PropertyDescriptor, context.Instance, context);

            if (objectType != null && memberType != null)
            {
                DependencyProperty dependencyProperty = DependencyProperty.FromName(context.PropertyDescriptor.Name, context.PropertyDescriptor.ComponentType);
                bool includeEvents = (dependencyProperty != null && dependencyProperty.IsEvent);

                BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy;
                if (obj == rootActivity)
                    bindingFlags |= BindingFlags.NonPublic;

                foreach (MemberInfo memberInfo in objectType.GetMembers(bindingFlags))
                {
                    //filter our methods with System.Diagnostics.DebuggerNonUserCodeAttribute
                    object[] nonUserCodeAttributes = memberInfo.GetCustomAttributes(typeof(System.Diagnostics.DebuggerNonUserCodeAttribute), false);
                    if (nonUserCodeAttributes != null && nonUserCodeAttributes.Length > 0 && nonUserCodeAttributes[0] is System.Diagnostics.DebuggerNonUserCodeAttribute)
                        continue;

                    object[] browsableAttributes = memberInfo.GetCustomAttributes(typeof(BrowsableAttribute), false);
                    if (browsableAttributes.Length > 0)
                    {
                        bool browsable = false;

                        BrowsableAttribute browsableAttribute = browsableAttributes[0] as BrowsableAttribute;
                        if (browsableAttribute != null)
                        {
                            browsable = browsableAttribute.Browsable;
                        }
                        else
                        {
                            try
                            {
                                AttributeInfoAttribute attributeInfoAttribute = browsableAttributes[0] as AttributeInfoAttribute;
                                if (attributeInfoAttribute != null && attributeInfoAttribute.AttributeInfo.ArgumentValues.Count > 0)
                                    browsable = (bool)attributeInfoAttribute.AttributeInfo.GetArgumentValueAs(context, 0, typeof(bool));
                            }
                            catch
                            {
                            }
                        }

                        if (!browsable)
                            continue;
                    }

                    if (memberInfo.DeclaringType == typeof(System.Object) && (string.Equals(memberInfo.Name, "Equals", StringComparison.Ordinal) || string.Equals(memberInfo.Name, "ReferenceEquals", StringComparison.Ordinal)))
                        continue;

                    bool addMember = false;
                    bool isProtectedOrPublicMember = false;
                    bool isInternalMember = false;
                    if (includeEvents && memberInfo is EventInfo)
                    {
                        EventInfo eventInfo = memberInfo as EventInfo;

                        MethodInfo addAccessor = eventInfo.GetAddMethod();
                        MethodInfo removeAccessor = eventInfo.GetRemoveMethod();

                        isProtectedOrPublicMember = ((addAccessor != null && addAccessor.IsFamily) || (removeAccessor != null && removeAccessor.IsFamily) ||
                                                    (addAccessor != null && addAccessor.IsPublic) || (removeAccessor != null && removeAccessor.IsPublic));
                        isInternalMember = ((addAccessor != null && addAccessor.IsAssembly) || (removeAccessor != null && removeAccessor.IsAssembly));

                        addMember = TypeProvider.IsAssignable(memberType, eventInfo.EventHandlerType);
                    }
                    else if (memberInfo is FieldInfo)
                    {
                        FieldInfo fieldInfo = memberInfo as FieldInfo;
                        isProtectedOrPublicMember = (fieldInfo.IsFamily || fieldInfo.IsPublic);
                        isInternalMember = fieldInfo.IsAssembly;
                        addMember = TypeProvider.IsAssignable(memberType, fieldInfo.FieldType);
                    }
                    else if (memberInfo is PropertyInfo)
                    {
                        PropertyInfo propertyInfo = memberInfo as PropertyInfo;

                        MethodInfo getAccessor = propertyInfo.GetGetMethod();
                        MethodInfo setAccessor = propertyInfo.GetSetMethod();

                        isProtectedOrPublicMember = ((getAccessor != null && getAccessor.IsFamily) || (setAccessor != null && setAccessor.IsFamily) ||
                                                    (getAccessor != null && getAccessor.IsPublic) || (setAccessor != null && setAccessor.IsPublic));
                        isInternalMember = ((getAccessor != null && getAccessor.IsAssembly) || (setAccessor != null && setAccessor.IsAssembly));
                        addMember = (getAccessor != null && TypeProvider.IsAssignable(memberType, propertyInfo.PropertyType));
                    }

                    //We only want to allow binding to protected, public and internal members of baseType
                    if (memberInfo.DeclaringType != objectType && !isProtectedOrPublicMember && !(memberInfo.DeclaringType.Assembly == null && isInternalMember))
                        addMember = false;

                    if (addMember)
                        memberInfos.Add(memberInfo);
                }
            }

            return memberInfos.AsReadOnly();
        }

        internal static bool CreateField(ITypeDescriptorContext context, ActivityBind activityBind, bool throwOnError)
        {
            //Check if the activity is root activity and has valid design time type
            if (!String.IsNullOrEmpty(activityBind.Path))
            {
                Type boundType = PropertyDescriptorUtils.GetBaseType(context.PropertyDescriptor, context.Instance, context);
                Activity activity = PropertyDescriptorUtils.GetComponent(context) as Activity;
                if (activity != null && boundType != null)
                {
                    activity = Helpers.ParseActivityForBind(activity, activityBind.Name);
                    if (activity == Helpers.GetRootActivity(activity))
                    {
                        bool isVB = (CompilerHelpers.GetSupportedLanguage(context) == SupportedLanguages.VB);
                        Type designedType = Helpers.GetDataSourceClass(activity, context);
                        if (designedType != null)
                        {
                            //field path could be nested too.
                            //need to find field only with the name up to the first dot (CimplexTypeField in the example below)
                            //and the right type (that would be tricky if the field doesnt exist yet)
                            //example: CimplexTypeField.myIDictionary_int_string[10].someOtherGood2

                            string fieldName = activityBind.Path;
                            int indexOfDot = fieldName.IndexOfAny(new char[] { '.', '/', '[' });
                            if (indexOfDot != -1)
                                fieldName = fieldName.Substring(0, indexOfDot); //path is a nested field access

                            MemberInfo matchingMember = ActivityBindPropertyDescriptor.FindMatchingMember(fieldName, designedType, isVB);
                            if (matchingMember != null)
                            {
                                Type memberType = null;
                                bool isPrivate = false;
                                if (matchingMember is FieldInfo)
                                {
                                    isPrivate = ((FieldInfo)matchingMember).IsPrivate;
                                    memberType = ((FieldInfo)matchingMember).FieldType;
                                }
                                else if (matchingMember is PropertyInfo)
                                {
                                    MethodInfo getMethod = ((PropertyInfo)matchingMember).GetGetMethod();
                                    MethodInfo setMethod = ((PropertyInfo)matchingMember).GetSetMethod();
                                    isPrivate = ((getMethod != null && getMethod.IsPrivate) || (setMethod != null && setMethod.IsPrivate));
                                }
                                else if (matchingMember is MethodInfo)
                                {
                                    isPrivate = ((MethodInfo)matchingMember).IsPrivate;
                                }

                                if (indexOfDot != -1)
                                { //need to find the type of the member the path references (and if the path is valid at all)
                                    PathWalker pathWalker = new PathWalker();
                                    PathMemberInfoEventArgs finalEventArgs = null;
                                    pathWalker.MemberFound += delegate(object sender, PathMemberInfoEventArgs eventArgs)
                                    { finalEventArgs = eventArgs; };

                                    if (pathWalker.TryWalkPropertyPath(designedType, activityBind.Path))
                                    {
                                        //successfully walked the entire path
                                        memberType = BindHelpers.GetMemberType(finalEventArgs.MemberInfo);
                                    }
                                    else
                                    {
                                        //the path is invalid
                                        if (throwOnError)
                                            throw new InvalidOperationException(SR.GetString(SR.Error_MemberWithSameNameExists, activityBind.Path, designedType.FullName));

                                        return false;
                                    }
                                }

                                if ((matchingMember.DeclaringType == designedType || !isPrivate) &&
                                    matchingMember is FieldInfo &&
                                    TypeProvider.IsAssignable(boundType, memberType))
                                {
                                    return true;
                                }
                                else
                                {
                                    if (throwOnError)
                                        throw new InvalidOperationException(SR.GetString(SR.Error_MemberWithSameNameExists, activityBind.Path, designedType.FullName));
                                    return false;
                                }
                            }
                            else
                            {
                                // Find out if the name conflicts with an existing activity that has not be flushed in to the 
                                // code beside.  An activity bind can bind to this field only if the type of the property
                                // is the assignable from the activity type.
                                Activity matchingActivity = null;
                                if (string.Compare(activity.Name, fieldName, isVB ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) == 0)
                                    matchingActivity = activity;
                                else if (activity is CompositeActivity)
                                {
                                    if (activity is CompositeActivity)
                                    {
                                        foreach (Activity existingActivity in Helpers.GetAllNestedActivities(activity as CompositeActivity))
                                        {
                                            if (string.Compare(existingActivity.Name, fieldName, isVB ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) == 0)
                                                matchingActivity = existingActivity;
                                        }
                                    }
                                }

                                if (matchingActivity != null)
                                {
                                    if (TypeProvider.IsAssignable(boundType, matchingActivity.GetType()))
                                        return true;
                                    else
                                    {
                                        if (throwOnError)
                                            throw new InvalidOperationException(SR.GetString(SR.Error_MemberWithSameNameExists, activityBind.Path, designedType.FullName));
                                        return false;
                                    }
                                }
                            }

                            IMemberCreationService memberCreationService = context.GetService(typeof(IMemberCreationService)) as IMemberCreationService;
                            if (memberCreationService == null)
                            {
                                if (throwOnError)
                                    throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(IMemberCreationService).FullName));
                            }
                            else
                            {
                                IDesignerHost designerHost = context.GetService(typeof(IDesignerHost)) as IDesignerHost;
                                if (designerHost == null)
                                {
                                    if (throwOnError)
                                        throw new InvalidOperationException(SR.GetString("General_MissingService", typeof(IDesignerHost).FullName));
                                }
                                else
                                {
                                    memberCreationService.CreateField(designerHost.RootComponentClassName, activityBind.Path, boundType, null, MemberAttributes.Public, null, false);
                                    return true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (activity == null && throwOnError)
                        throw new InvalidOperationException(SR.GetString(SR.Error_InvalidActivityIdentifier, activityBind.Name));

                    if (boundType == null && throwOnError)
                        throw new InvalidOperationException(SR.GetString(SR.Error_PropertyTypeNotDefined, context.PropertyDescriptor.Name, typeof(ActivityBind).Name, typeof(IDynamicPropertyTypeProvider).Name));
                }
            }

            return false;
        }

        internal static bool IsBindableProperty(PropertyDescriptor propertyDescriptor)
        {
            //The property type itself is ActivityBind; we dont support such cases very well in componentmodel
            //but still we need to handle such cases as a fallback in ui
            if (propertyDescriptor.PropertyType == typeof(ActivityBind))
                return true;

            //We check this condition so that we will make sure that ActivityBind UI infrastructure
            //kicks into action in cases of parameter properties. User might sometimes do this on their properties
            //but in such cases they need to write their custom property descriptors just as we have written
            //ParameterInfoBasedPropertyDescriptor
            if (propertyDescriptor.Converter is ActivityBindTypeConverter)
                return true;

            //For all the other cases 
            DependencyProperty dependencyProperty = DependencyProperty.FromName(propertyDescriptor.Name, propertyDescriptor.ComponentType);
            if (typeof(DependencyObject).IsAssignableFrom(propertyDescriptor.ComponentType) && dependencyProperty != null && !dependencyProperty.DefaultMetadata.IsMetaProperty)
                return true;

            return false;
        }

        internal static MemberInfo FindMatchingMember(string name, Type ownerType, bool ignoreCase)
        {
            MemberInfo matchingMember = null;
            foreach (MemberInfo memberInfo in ownerType.GetMembers(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (memberInfo.Name.Equals(name, ((ignoreCase) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)))
                {
                    matchingMember = memberInfo;
                    break;
                }
            }
            return matchingMember;
        }
        #endregion
    }
    #endregion

    #region Class ActivityBindNamePropertyDescriptor
    internal sealed class ActivityBindNamePropertyDescriptor : DynamicPropertyDescriptor
    {
        private ITypeDescriptorContext context;

        public ActivityBindNamePropertyDescriptor(ITypeDescriptorContext context, PropertyDescriptor realPropertyDescriptor)
            : base(context, realPropertyDescriptor)
        {
            this.context = context;
        }

        public override object GetValue(object component)
        {
            object value = base.GetValue(component);
            string id = value as string;
            if (!String.IsNullOrEmpty(id))
            {
                Activity activity = PropertyDescriptorUtils.GetComponent(this.context) as Activity;
                activity = (activity != null) ? Helpers.ParseActivityForBind(activity, id) : null;
                value = (activity != null) ? activity.QualifiedName : id;
            }

            return value;
        }

        public override void SetValue(object component, object value)
        {
            string id = value as string;
            if (String.IsNullOrEmpty(id))
                throw new InvalidOperationException(SR.GetString(SR.Error_ActivityIdentifierCanNotBeEmpty));

            Activity activity = PropertyDescriptorUtils.GetComponent(this.context) as Activity;
            if (activity != null)
            {
                if (Helpers.ParseActivityForBind(activity, id) == null)
                    throw new InvalidOperationException(SR.GetString(SR.Error_InvalidActivityIdentifier, id));
            }

            base.SetValue(component, value);
        }
    }
    #endregion

    #region Class ActivityBindPathPropertyDescriptor
    internal sealed class ActivityBindPathPropertyDescriptor : DynamicPropertyDescriptor
    {
        private ITypeDescriptorContext context;

        public ActivityBindPathPropertyDescriptor(ITypeDescriptorContext context, PropertyDescriptor realPropertyDescriptor)
            : base(context, realPropertyDescriptor)
        {
            this.context = context;
        }

        internal ITypeDescriptorContext OuterPropertyContext
        {
            get
            {
                return this.context;
            }
        }
    }
    #endregion

    #endregion

    #region ReadOnly Properties Integration

    #region Class ReadonlyTypeDescriptorProvider
    internal sealed class ReadonlyTypeDescriptonProvider : TypeDescriptionProvider
    {
        internal ReadonlyTypeDescriptonProvider(TypeDescriptionProvider realProvider)
            : base(realProvider)
        {
        }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type type, object instance)
        {
            ICustomTypeDescriptor realTypeDescriptor = base.GetTypeDescriptor(type, instance);
            ICustomTypeDescriptor readonlyTypeDescriptor = new ReadonlyTypeDescriptor(realTypeDescriptor);
            return readonlyTypeDescriptor;
        }
    }
    #endregion

    #region Class ReadonlyTypeDescriptor
    internal sealed class ReadonlyTypeDescriptor : CustomTypeDescriptor
    {
        internal ReadonlyTypeDescriptor(ICustomTypeDescriptor realTypeDescriptor)
            : base(realTypeDescriptor)
        {
        }

        public override AttributeCollection GetAttributes()
        {
            ArrayList collection = new ArrayList();
            foreach (Attribute attribute in base.GetAttributes())
            {
                //should not have any editor attribute and only one readonly attribute
                if (!(attribute is EditorAttribute || attribute is ReadOnlyAttribute))
                    collection.Add(attribute);
            }
            collection.Add(new ReadOnlyAttribute(true));
            AttributeCollection newCollection = new AttributeCollection((Attribute[])collection.ToArray(typeof(Attribute)));
            return newCollection;
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            PropertyDescriptorCollection properties = base.GetProperties();

            ArrayList readonlyProperties = new ArrayList();
            foreach (PropertyDescriptor property in properties)
            {
                BrowsableAttribute browsable = property.Attributes[typeof(BrowsableAttribute)] as BrowsableAttribute;
                if (browsable != null && browsable.Browsable && !(property is ReadonlyPropertyDescriptor))
                    readonlyProperties.Add(new ReadonlyPropertyDescriptor(property));
                else
                    readonlyProperties.Add(property);
            }

            return new PropertyDescriptorCollection((PropertyDescriptor[])readonlyProperties.ToArray(typeof(PropertyDescriptor)));
        }

        public override EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            EventDescriptorCollection events = base.GetEvents(attributes);

            ArrayList readonlyEvents = new ArrayList();
            foreach (EventDescriptor e in events)
            {
                BrowsableAttribute browsable = e.Attributes[typeof(BrowsableAttribute)] as BrowsableAttribute;
                if (browsable != null && browsable.Browsable)
                    readonlyEvents.Add(new ReadonlyEventDescriptor(e));
                else
                    readonlyEvents.Add(e);
            }

            return new EventDescriptorCollection((EventDescriptor[])readonlyEvents.ToArray(typeof(EventDescriptor)));
        }
    }
    #endregion

    #region Class ReadonlyPropertyDescriptor
    internal sealed class ReadonlyPropertyDescriptor : PropertyDescriptor
    {
        private PropertyDescriptor realPropertyDescriptor;

        internal ReadonlyPropertyDescriptor(PropertyDescriptor descriptor)
            : base(descriptor, null)
        {
            this.realPropertyDescriptor = descriptor;
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
                ArrayList collection = new ArrayList();
                foreach (Attribute attribute in this.realPropertyDescriptor.Attributes)
                {
                    //should not have any editor attribute and only one readonly attribute
                    if (!(attribute is EditorAttribute || attribute is ReadOnlyAttribute))
                        collection.Add(attribute);
                }
                collection.Add(new ReadOnlyAttribute(true));
                AttributeCollection newCollection = new AttributeCollection((Attribute[])collection.ToArray(typeof(Attribute)));
                return newCollection;
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
                return true;
            }
        }
        public override void ResetValue(object component)
        {
            this.realPropertyDescriptor.ResetValue(component);
        }
        public override bool CanResetValue(object component)
        {
            return false;
        }
        public override bool ShouldSerializeValue(object component)
        {
            return this.realPropertyDescriptor.ShouldSerializeValue(component);
        }
        public override object GetValue(object component)
        {
            return this.realPropertyDescriptor.GetValue(component);
        }
        public override void SetValue(object component, object value)
        {
            //This is readonly property descriptor
            Debug.Assert(false, "SetValue should not be called on readonly property!");
        }
    }
    #endregion

    #region Class ReadonlyEventDescriptor
    internal sealed class ReadonlyEventDescriptor : EventDescriptor
    {
        private EventDescriptor realEventDescriptor;

        internal ReadonlyEventDescriptor(EventDescriptor e)
            : base(e, null)
        {
            this.realEventDescriptor = e;
        }

        public override string Category
        {
            get
            {
                return this.realEventDescriptor.Category;
            }
        }
        public override AttributeCollection Attributes
        {
            get
            {
                ArrayList collection = new ArrayList();
                foreach (Attribute attribute in this.realEventDescriptor.Attributes)
                {
                    //should not have any editor attribute and only one readonly attribute
                    if (!(attribute is EditorAttribute || attribute is ReadOnlyAttribute))
                        collection.Add(attribute);
                }
                collection.Add(new ReadOnlyAttribute(true));
                AttributeCollection newCollection = new AttributeCollection((Attribute[])collection.ToArray(typeof(Attribute)));
                return newCollection;
            }
        }
        public override string Description
        {
            get
            {
                return this.realEventDescriptor.Description;
            }
        }
        public override Type ComponentType
        {
            get
            {
                return this.realEventDescriptor.ComponentType;
            }
        }
        public override Type EventType
        {
            get
            {
                return this.realEventDescriptor.EventType;
            }
        }
        public override bool IsMulticast
        {
            get
            {
                return this.realEventDescriptor.IsMulticast;
            }
        }

        public override void AddEventHandler(object component, Delegate value)
        {
            //This is readonly event descriptor
        }
        public override void RemoveEventHandler(object component, Delegate value)
        {
            //This is readonly event descriptor
        }
    }
    #endregion

    #endregion
}

