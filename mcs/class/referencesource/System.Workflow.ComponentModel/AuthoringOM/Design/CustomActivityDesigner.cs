namespace System.Workflow.ComponentModel.Design
{
    #region Using directives

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.ComponentModel.Design;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Globalization;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.ComponentModel.Design.Serialization;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.CodeDom;

    #endregion

    #region Class CustomActivityDesignerAdapter

    internal sealed class CustomActivityDesignerAdapter : IDisposable
    {
        private IServiceProvider serviceProvider = null;
        private EventHandler ensureChildHierarchyHandler = null;

        public CustomActivityDesignerAdapter(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;

            IComponentChangeService componentChangeService = this.serviceProvider.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            if (componentChangeService != null)
            {
                componentChangeService.ComponentAdding += new ComponentEventHandler(OnComponentAdding);
                componentChangeService.ComponentAdded += new ComponentEventHandler(OnComponentAdded);
            }
        }

        void IDisposable.Dispose()
        {
            if (this.ensureChildHierarchyHandler != null)
            {
                Application.Idle -= this.ensureChildHierarchyHandler;
                this.ensureChildHierarchyHandler = null;
            }

            IComponentChangeService componentChangeService = this.serviceProvider.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            if (componentChangeService != null)
            {
                componentChangeService.ComponentAdding -= new ComponentEventHandler(OnComponentAdding);
                componentChangeService.ComponentAdded -= new ComponentEventHandler(OnComponentAdded);
            }
        }

        #region Helpers
        private void OnComponentAdding(object sender, ComponentEventArgs eventArgs)
        {
            //We are adding root component, while doing this make sure that we provide the root designer attribute
            IDesignerHost designerHost = (IDesignerHost)this.serviceProvider.GetService(typeof(IDesignerHost));
            if (designerHost != null)
            {
                if (designerHost.RootComponent == null)
                {
                    Activity rootActivity = eventArgs.Component as Activity;
                    if (rootActivity != null)
                    {
                        //Add root designer attribute
                        DesignerAttribute rootDesignerAttrib = GetDesignerAttribute(rootActivity, typeof(IRootDesigner));
                        if (rootDesignerAttrib.DesignerTypeName == typeof(ActivityDesigner).AssemblyQualifiedName)
                        {
                            DesignerAttribute designerAttrib = GetDesignerAttribute(rootActivity, typeof(IDesigner));
                            if (designerAttrib != null)
                                TypeDescriptor.AddAttributes(rootActivity, new Attribute[] { new DesignerAttribute(designerAttrib.DesignerTypeName, typeof(IRootDesigner)) });
                        }
                    }
                }
            }
        }

        private void OnComponentAdded(object sender, ComponentEventArgs eventArgs)
        {
            IDesignerHost designerHost = (IDesignerHost)this.serviceProvider.GetService(typeof(IDesignerHost));
            if (designerHost != null)
            {
                if (designerHost.RootComponent == eventArgs.Component)
                {
                    Activity rootActivity = designerHost.RootComponent as Activity;
                    if (rootActivity != null)
                    {
                        CompositeActivity compositeActivity = rootActivity as CompositeActivity;
                        if (compositeActivity != null)
                        {
                            if (this.ensureChildHierarchyHandler == null)
                            {
                                this.ensureChildHierarchyHandler = new EventHandler(OnEnsureChildHierarchy);
                                Application.Idle += this.ensureChildHierarchyHandler;
                            }
                        }
                        rootActivity.UserData[UserDataKeys.CustomActivity] = false;
                    }
                }
                else if (eventArgs.Component is Activity)
                {
                    if ((eventArgs.Component is CompositeActivity) && Helpers.IsCustomActivity(eventArgs.Component as CompositeActivity))
                        (eventArgs.Component as Activity).UserData[UserDataKeys.CustomActivity] = true;
                    else
                        (eventArgs.Component as Activity).UserData[UserDataKeys.CustomActivity] = false;
                }
            }
        }

        /// PLEASE NOTE: We have added this handler for a reason. When the user changes the base type of the Activity
        /// we reload the designer after flushing the changes to the buffer, we do that in the IDesignerHost.TransactionClosed event
        /// The reload of the designer is then done on Idle event; but during reload we add extra components in the composite activity
        /// which in short modifies the composite activity,but during loading process the BasicDesignerLoader ignores such modification and
        /// does not tag the buffer as modified. The problem which happens due to this is that we wont flush the changes caused by
        /// adding of additional components to the buffer there by not saving the child designers. Hence instead of adding components during reload
        /// we add them in the first idle event after reload.
        private void OnEnsureChildHierarchy(object sender, EventArgs e)
        {
            if (this.ensureChildHierarchyHandler != null)
            {
                Application.Idle -= this.ensureChildHierarchyHandler;
                this.ensureChildHierarchyHandler = null;

                IDesignerHost designerHost = (IDesignerHost)this.serviceProvider.GetService(typeof(IDesignerHost));
                if (designerHost != null)
                    EnsureDefaultChildHierarchy(designerHost);
            }
        }

        private static DesignerAttribute GetDesignerAttribute(object component, Type designerBaseType)
        {
            AttributeCollection attribs = TypeDescriptor.GetAttributes(component);
            foreach (Attribute attribute in attribs)
            {
                DesignerAttribute designerAttribute = attribute as DesignerAttribute;
                if (designerAttribute != null && designerAttribute.DesignerBaseTypeName == designerBaseType.AssemblyQualifiedName)
                    return designerAttribute;
            }

            return null;
        }

        private static void EnsureDefaultChildHierarchy(IDesignerHost designerHost)
        {
            //When we are adding the root activity we need to make sure that all the child activities which are required by the parent
            //activity are looked up in the toolboxitem and added appropriately
            //If the composite activity already has a some child activities but not all then it 
            //means that user has changed the InitializeComponent and hence we do nothing
            //This is the simple check to get the designer working in case of selecting composite
            //root activities
            CompositeActivity rootActivity = designerHost.RootComponent as CompositeActivity;
            if (rootActivity != null && rootActivity.Activities.Count == 0)
            {
                object[] attribs = rootActivity.GetType().GetCustomAttributes(typeof(ToolboxItemAttribute), false);
                ToolboxItemAttribute toolboxItemAttrib = (attribs != null && attribs.GetLength(0) > 0) ? attribs[0] as ToolboxItemAttribute : null;
                if (toolboxItemAttrib != null && toolboxItemAttrib.ToolboxItemType != null)
                {
                    ToolboxItem item = Activator.CreateInstance(toolboxItemAttrib.ToolboxItemType, new object[] { rootActivity.GetType() }) as ToolboxItem;
                    IComponent[] components = item.CreateComponents();

                    //I am assuming here that there will be always one top level component created.
                    //If there are multiple then there is a bigger problem as we dont know how
                    //to use those
                    CompositeActivity compositeActivity = null;
                    foreach (IComponent component in components)
                    {
                        if (component.GetType() == rootActivity.GetType())
                        {
                            compositeActivity = component as CompositeActivity;
                            break;
                        }
                    }

                    //Add the children
                    if (compositeActivity != null && compositeActivity.Activities.Count > 0)
                    {
                        IIdentifierCreationService identifierCreationService = designerHost.GetService(typeof(IIdentifierCreationService)) as IIdentifierCreationService;
                        if (identifierCreationService != null)
                        {
                            //We do not go thru the composite designer here as composite activity
                            //might have simple designer
                            Activity[] activities = compositeActivity.Activities.ToArray();
                            compositeActivity.Activities.Clear();

                            identifierCreationService.EnsureUniqueIdentifiers(rootActivity, activities);
                            // Work around : Don't called AddRange because it doesn't send the ListChange notifications
                            // to the activity collection.  Use multiple Add calls instaead
                            foreach (Activity newActivity in activities)
                                rootActivity.Activities.Add(newActivity);

                            foreach (Activity childActivity in activities)
                                WorkflowDesignerLoader.AddActivityToDesigner(designerHost, childActivity);
                        }
                    }
                }
            }
        }
        #endregion
    }
    #endregion

    #region Extenders

    #region Class CustomActivityPropertyExtender
    [ProvideProperty("BaseActivityType", typeof(Activity))]
    internal sealed class CustomActivityPropertyExtender : IExtenderProvider
    {
        public CustomActivityPropertyExtender()
        {
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [SRDisplayName(SR.BaseActivityType)]
        [SRCategory(SR.ActivityDesc)]
        [SRDescription(SR.CustomActivityBaseTypeDesc)]
        [Editor(typeof(TypeBrowserEditor), typeof(UITypeEditor))]
        [DesignOnly(true)]
        [TypeFilterProvider(typeof(BaseClassTypeFilterProvider))]
        [DefaultValue("System.Workflow.ComponentModel.Sequence")]
        public string GetBaseActivityType(Activity activity)
        {
            // The activity type will be the base type.  No need for more complicated logic here.
            return activity.GetType().FullName;
        }

        //Do not remove this function, it is being called indirectly
        public void SetBaseActivityType(Activity activity, string baseActivityTypeName)
        {
            CustomActivityDesignerHelper.SetBaseTypeName(baseActivityTypeName, activity.Site);

            // Once the base type is changed, cause each of the companion class properties
            // to set their value again, updating their base types appropriatly
        }

        bool IExtenderProvider.CanExtend(object extendee)
        {
            bool canExtend = false;

            Activity activity = extendee as Activity;
            if (activity != null && activity.Site != null && activity == Helpers.GetRootActivity(activity))
            {
                ActivityDesigner rootDesigner = ActivityDesigner.GetDesigner(activity);
                if (rootDesigner != null && rootDesigner.ParentDesigner == null)
                    canExtend = true;
            }

            return canExtend;
        }
    }
    #endregion

    #endregion

    #region Class CustomActivityDesignerHelper
    internal static class CustomActivityDesignerHelper
    {
        #region Base Type Helper Methods
        public static Type GetCustomActivityType(IServiceProvider serviceProvider)
        {
            IDesignerHost host = serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (host == null)
                throw new Exception(SR.GetString(SR.General_MissingService, typeof(IDesignerHost).FullName));

            string className = host.RootComponentClassName;
            if (string.IsNullOrEmpty(className))
                return null;

            ITypeProvider typeProvider = serviceProvider.GetService(typeof(ITypeProvider)) as ITypeProvider;
            if (typeProvider == null)
                throw new Exception(SR.GetString(SR.General_MissingService, typeof(ITypeProvider).FullName));

            return typeProvider.GetType(className, false);
        }

        public static void SetBaseTypeName(string typeName, IServiceProvider serviceProvider)
        {
            if (string.IsNullOrEmpty(typeName))
                throw new ArgumentNullException("typeName");

            IDesignerHost host = serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (host == null)
                throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(IDesignerHost).FullName));

            IMemberCreationService memberCreationService = serviceProvider.GetService(typeof(IMemberCreationService)) as IMemberCreationService;
            if (memberCreationService == null)
                throw new InvalidOperationException(SR.GetString("General_MissingService", typeof(IMemberCreationService).FullName));

            // Validate the base type (this will throw an exception if the type name isn't valid
            Type newBaseType = ValidateBaseType(typeName, serviceProvider);

            //Warn the user of the change
            Type oldBaseType = host.RootComponent.GetType();
            if (oldBaseType == newBaseType)
                return;

            // If we're switch to a base type that is not derived from CompositeActivity, make sure
            // we dont's support events or exceptions
            if (!TypeProvider.IsAssignable(typeof(CompositeActivity), newBaseType))
            {
                PropertyDescriptor supportsEventsPropDesc = TypeDescriptor.GetProperties(host.RootComponent)["SupportsEvents"];
                if (supportsEventsPropDesc != null && ((bool)supportsEventsPropDesc.GetValue(host.RootComponent)) == true)
                    supportsEventsPropDesc.SetValue(host.RootComponent, false);

                PropertyDescriptor supportsExceptionsPropDesc = TypeDescriptor.GetProperties(host.RootComponent)["SupportsExceptions"];
                if (supportsExceptionsPropDesc != null && ((bool)supportsExceptionsPropDesc.GetValue(host.RootComponent)) == true)
                    supportsExceptionsPropDesc.SetValue(host.RootComponent, false);
            }

            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(host.RootComponent);
            if (host.RootComponent is CompositeActivity && ((CompositeActivity)host.RootComponent).Activities.Count > 0)
            {
                // Warn user first if there are any children that can not be re-parented to the new root.
                IUIService uiService = serviceProvider.GetService(typeof(IUIService)) as IUIService;
                if (uiService != null)
                {
                    if (DialogResult.OK != uiService.ShowMessage(SR.GetString(SR.NoChildActivities_Message),
                        SR.GetString(SR.NoChildActivities_Caption), MessageBoxButtons.OKCancel))
                        return;
                }

                // Remove the children first. This would cause the component removed event to be fired,
                // thus remove the generated field from the designer.cs file.
                List<Activity> activitiesToRemove = new List<Activity>(((CompositeActivity)host.RootComponent).Activities);
                CompositeActivityDesigner rootDesigner = host.GetDesigner(host.RootComponent) as CompositeActivityDesigner;
                if (rootDesigner != null)
                    rootDesigner.RemoveActivities(activitiesToRemove.AsReadOnly());
            }

            //Also, clear all properties of original base. That will allow undo to set old values back.
            foreach (PropertyDescriptor propertyDescriptor in properties)
            {
                if (!propertyDescriptor.Name.Equals("BaseActivityType", StringComparison.Ordinal) &&
                    !propertyDescriptor.Name.Equals("Name", StringComparison.Ordinal) &&
                    propertyDescriptor.CanResetValue(host.RootComponent))
                {
                    propertyDescriptor.ResetValue(host.RootComponent);
                }
            }

            PropertyDescriptor realBaseActivityTypePropertyDescriptor = properties["BaseActivityType"];
            PropertyDescriptor baseActivityTypePropertyDescriptor = TypeDescriptor.CreateProperty(realBaseActivityTypePropertyDescriptor.ComponentType, realBaseActivityTypePropertyDescriptor, DesignerSerializationVisibilityAttribute.Visible);

            IComponentChangeService changeService = serviceProvider.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            if (changeService != null)
                changeService.OnComponentChanging(host.RootComponent, baseActivityTypePropertyDescriptor);

            ((Activity)host.RootComponent).UserData[UserDataKeys.NewBaseType] = newBaseType;

            memberCreationService.UpdateBaseType(host.RootComponentClassName, newBaseType);

            if (changeService != null)
                changeService.OnComponentChanged(host.RootComponent, baseActivityTypePropertyDescriptor, baseActivityTypePropertyDescriptor.GetValue(host.RootComponent), typeName);

            //Work around: Force update of the host by raising idle.This is to ensure undo events work on updated host.
            Application.RaiseIdle(new EventArgs());
        }

        private static Type ValidateBaseType(string typeName, IServiceProvider serviceProvider)
        {
            if (typeName != null && typeName.Length > 0)
            {
                ITypeProvider typeProvider = (ITypeProvider)serviceProvider.GetService(typeof(ITypeProvider));
                if (typeProvider == null)
                    throw new Exception(SR.GetString(SR.General_MissingService, typeof(ITypeProvider).FullName));

                Type type = typeProvider.GetType(typeName);
                if (type == null)
                    throw new Exception(SR.GetString(SR.Error_TypeNotResolved, typeName));

                IDesignerHost host = serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (host == null)
                    throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(IDesignerHost).FullName));

                Type rootComponentType = typeProvider.GetType(host.RootComponentClassName);

                if (type is DesignTimeType && rootComponentType != null && rootComponentType.Assembly == type.Assembly)
                    throw new InvalidOperationException(SR.GetString(SR.Error_CantUseCurrentProjectTypeAsBase));

                if (!TypeProvider.IsAssignable(typeof(Activity), type))
                    throw new InvalidOperationException(SR.GetString(SR.Error_BaseTypeMustBeActivity));

                return type;
            }

            return null;
        }

        #endregion

        #region Custom Properties Helper Methods
        internal static List<CustomProperty> GetCustomProperties(IServiceProvider serviceProvider)
        {
            // We need to perform a flush just before getting the custom properties so that we are sure that type system is updated
            // and we always get the updated properties
            WorkflowDesignerLoader loader = serviceProvider.GetService(typeof(IDesignerLoaderService)) as WorkflowDesignerLoader;
            if (loader != null)
                loader.Flush();

            Type customActivityType = GetCustomActivityType(serviceProvider);
            if (customActivityType == null)
                return null;

            List<CustomProperty> cpc = new List<CustomProperty>();
            PropertyInfo[] properties = customActivityType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in properties)
            {
                if (property.PropertyType != null)
                    cpc.Add(CreateCustomProperty(serviceProvider, customActivityType, property, property.PropertyType));
            }

            EventInfo[] events = customActivityType.GetEvents(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            foreach (EventInfo evt in events)
            {
                if (evt.EventHandlerType == null)
                    continue;
                CustomProperty eventProperty = CreateCustomProperty(serviceProvider, customActivityType, evt, evt.EventHandlerType);
                eventProperty.IsEvent = true;
                cpc.Add(eventProperty);
            }

            return cpc;
        }

        private static CustomProperty CreateCustomProperty(IServiceProvider serviceProvider, Type customActivityType, MemberInfo member, Type propertyType)
        {
            CustomProperty customProperty = new CustomProperty(serviceProvider);
            customProperty.Name = member.Name;
            customProperty.IsEvent = (member is EventInfo);

            if (propertyType == typeof(ActivityBind))
            {
                customProperty.GenerateDependencyProperty = false;
                customProperty.Type = typeof(ActivityBind).FullName;
            }
            else
            {
                string fieldSuffix = (customProperty.IsEvent) ? "Event" : "Property";
                FieldInfo fieldInfo = customActivityType.GetField(member.Name + fieldSuffix, BindingFlags.Public | BindingFlags.Static);
                if ((fieldInfo != null && fieldInfo.FieldType == typeof(DependencyProperty)))
                    customProperty.GenerateDependencyProperty = true;
                else
                    customProperty.GenerateDependencyProperty = false;

                customProperty.Type = propertyType.FullName;
            }

            customProperty.oldPropertyName = member.Name;
            customProperty.oldPropertyType = propertyType.FullName;

            object[] hiddenCodeAttributes = member.GetCustomAttributes(typeof(FlagsAttribute), true);
            if (hiddenCodeAttributes != null && hiddenCodeAttributes.Length > 0)
                customProperty.Hidden = true;

            foreach (object attributeObj in member.GetCustomAttributes(false))
            {
                AttributeInfoAttribute attribute = attributeObj as AttributeInfoAttribute;
                AttributeInfo attributeInfo = (attribute != null) ? attribute.AttributeInfo : null;
                if (attributeInfo != null)
                {
                    try
                    {
                        if (attributeInfo.AttributeType == typeof(BrowsableAttribute) && attributeInfo.ArgumentValues.Count > 0)
                        {
                            customProperty.Browseable = (bool)attributeInfo.GetArgumentValueAs(serviceProvider, 0, typeof(bool));
                        }
                        else if (attributeInfo.AttributeType == typeof(CategoryAttribute) && attributeInfo.ArgumentValues.Count > 0)
                        {
                            customProperty.Category = attributeInfo.GetArgumentValueAs(serviceProvider, 0, typeof(string)) as string;
                        }
                        else if (attributeInfo.AttributeType == typeof(DescriptionAttribute) && attributeInfo.ArgumentValues.Count > 0)
                        {
                            customProperty.Description = attributeInfo.GetArgumentValueAs(serviceProvider, 0, typeof(string)) as string;
                        }
                        else if (attributeInfo.AttributeType == typeof(DesignerSerializationVisibilityAttribute) && attributeInfo.ArgumentValues.Count > 0)
                        {
                            customProperty.DesignerSerializationVisibility = (DesignerSerializationVisibility)attributeInfo.GetArgumentValueAs(serviceProvider, 0, typeof(DesignerSerializationVisibility));
                        }
                        else if (attributeInfo.AttributeType == typeof(EditorAttribute) && attributeInfo.ArgumentValues.Count > 1)
                        {
                            Type editorType = attributeInfo.GetArgumentValueAs(serviceProvider, 1, typeof(Type)) as Type;
                            if (editorType == typeof(UITypeEditor))
                            {
                                Type uiTypeEditorType = attributeInfo.GetArgumentValueAs(serviceProvider, 0, typeof(Type)) as Type;
                                if (uiTypeEditorType != null)
                                    customProperty.UITypeEditor = uiTypeEditorType.FullName;

                                if (String.IsNullOrEmpty(customProperty.UITypeEditor))
                                    customProperty.UITypeEditor = attributeInfo.GetArgumentValueAs(serviceProvider, 0, typeof(string)) as string;
                            }
                        }
                    }
                    catch
                    {
                        // Catch and ignore all attribute value conversion errors
                    }
                }
            }

            return customProperty;
        }


        internal static void SetCustomProperties(List<CustomProperty> customProperties, IServiceProvider serviceProvider)
        {
            if (customProperties == null)
                throw new ArgumentNullException("customProperties");

            Type customActivityType = GetCustomActivityType(serviceProvider);
            if (customActivityType == null)
                return;

            List<CustomProperty> existingCustomProperties = GetCustomProperties(serviceProvider);

            // Remove any deleted properties
            RemoveDeletedProperties(customProperties, customActivityType, serviceProvider);

            // Add any new properties
            AddNewProperties(customProperties, customActivityType, serviceProvider, existingCustomProperties);
        }

        private static void RemoveDeletedProperties(List<CustomProperty> propCollection, Type customActivityType, IServiceProvider serviceProvider)
        {
            // 
            IMemberCreationService memberCreationService = serviceProvider.GetService(typeof(IMemberCreationService)) as IMemberCreationService;
            if (memberCreationService == null)
                throw new Exception(SR.GetString(SR.General_MissingService, typeof(IMemberCreationService).FullName));

            PropertyInfo[] properties = customActivityType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in properties)
            {
                bool found = false;
                foreach (CustomProperty customProperty in propCollection)
                {
                    if (property.Name == customProperty.oldPropertyName &&
                        property.PropertyType.FullName == customProperty.oldPropertyType)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    memberCreationService.RemoveProperty(customActivityType.FullName, property.Name, property.PropertyType);
            }

            EventInfo[] events = customActivityType.GetEvents(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            foreach (EventInfo evtInfo in events)
            {
                bool found = false;
                foreach (CustomProperty customProperty in propCollection)
                {
                    if (evtInfo.Name == customProperty.oldPropertyName &&
                        evtInfo.EventHandlerType.FullName == customProperty.oldPropertyType)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found && evtInfo.Name != null && evtInfo.EventHandlerType != null)
                    memberCreationService.RemoveEvent(customActivityType.FullName, evtInfo.Name, evtInfo.EventHandlerType);
            }
        }

        private static void AddNewProperties(List<CustomProperty> propCollection, Type customActivityType, IServiceProvider serviceProvider, List<CustomProperty> existingProps)
        {
            IMemberCreationService memberCreationService = serviceProvider.GetService(typeof(IMemberCreationService)) as IMemberCreationService;
            if (memberCreationService == null)
                throw new Exception(SR.GetString(SR.General_MissingService, typeof(IMemberCreationService).FullName));

            ITypeProvider typeProvider = serviceProvider.GetService(typeof(ITypeProvider)) as ITypeProvider;
            if (typeProvider == null)
                throw new Exception(SR.GetString(SR.General_MissingService, typeof(ITypeProvider).FullName));

            try
            {
                //



                foreach (CustomProperty property in propCollection)
                {
                    bool createNew = (property.oldPropertyName == null || property.oldPropertyType == null);
                    if (!createNew)
                    {
                        if (!property.IsEvent)
                            createNew = (customActivityType.GetProperty(property.oldPropertyName, typeProvider.GetType(property.oldPropertyType)) == null);
                        else
                            createNew = (customActivityType.GetEvent(property.oldPropertyName) == null);
                    }

                    if (createNew)
                    {
                        AttributeInfo[] attributes = CreateCustomPropertyAttributeArray(property, serviceProvider);
                        if (property.IsEvent)
                            memberCreationService.CreateEvent(customActivityType.FullName, property.Name, typeProvider.GetType(property.Type), attributes, property.GenerateDependencyProperty);
                        else
                            memberCreationService.CreateProperty(customActivityType.FullName, property.Name, typeProvider.GetType(property.Type), attributes, property.GenerateDependencyProperty, false, false, null, false);
                    }
                    else
                    {
                        // 

                        CustomProperty oldProperty = null;
                        foreach (CustomProperty existingProperty in existingProps)
                        {
                            if (existingProperty.Name == property.oldPropertyName && existingProperty.Type == property.oldPropertyType)
                                oldProperty = existingProperty;
                        }

                        if (oldProperty == null || ArePropertiesDifferent(property, oldProperty))
                        {
                            AttributeInfo[] attributes = CreateCustomPropertyAttributeArray(property, serviceProvider);
                            AttributeInfo[] oldAttributes = CreateCustomPropertyAttributeArray(oldProperty, serviceProvider);

                            Type propertyType = typeProvider.GetType(property.Type, false);
                            Type oldPropertyType = typeProvider.GetType(property.oldPropertyType, false);
                            if (propertyType != null)
                            {
                                if (property.IsEvent)
                                    memberCreationService.UpdateEvent(customActivityType.FullName, property.oldPropertyName, oldPropertyType, property.Name, propertyType, attributes, property.GenerateDependencyProperty, false);
                                else
                                    memberCreationService.UpdateProperty(customActivityType.FullName, property.oldPropertyName, oldPropertyType, property.Name, propertyType, attributes, property.GenerateDependencyProperty, false);
                            }
                        }
                    }
                }
            }
            finally
            {
                //


            }
        }

        private static AttributeInfo[] CreateCustomPropertyAttributeArray(CustomProperty property, IServiceProvider serviceProvider)
        {
            // Don't generate these attributes for hidden properties, just let the
            // attributes that already exist on the property stay
            if (property == null || property.Hidden)
                return new AttributeInfo[0];

            List<AttributeInfo> attributeList = new List<AttributeInfo>();

            if (property.Category != null)
                attributeList.Add(new AttributeInfo(typeof(CategoryAttribute), new string[] { }, new object[] { new CodePrimitiveExpression(property.Category) }));

            if (property.Description != null)
                attributeList.Add(new AttributeInfo(typeof(DescriptionAttribute), new string[] { }, new object[] { new CodePrimitiveExpression(property.Description) }));

            if (!string.IsNullOrEmpty(property.UITypeEditor))
                attributeList.Add(new AttributeInfo(typeof(EditorAttribute), new string[] { }, new object[] { new CodeTypeOfExpression(property.UITypeEditor), new CodeTypeOfExpression(typeof(UITypeEditor)) }));

            attributeList.Add(new AttributeInfo(typeof(BrowsableAttribute), new string[] { }, new object[] { new CodePrimitiveExpression(property.Browseable) }));
            attributeList.Add(new AttributeInfo(typeof(DesignerSerializationVisibilityAttribute), new string[] { }, new object[] { new CodeSnippetExpression(typeof(DesignerSerializationVisibility).Name + "." + property.DesignerSerializationVisibility.ToString()) }));

            return attributeList.ToArray();
        }

        private static bool ArePropertiesDifferent(CustomProperty property, CustomProperty oldProperty)
        {
            if (property.Name == oldProperty.Name &&
                property.Type == oldProperty.Type &&
                property.Browseable == oldProperty.Browseable &&
                property.Category == oldProperty.Category &&
                property.Description == oldProperty.Description &&
                property.DesignerSerializationVisibility == oldProperty.DesignerSerializationVisibility &&
                property.Hidden == oldProperty.Hidden &&
                property.UITypeEditor == oldProperty.UITypeEditor)
            {
                return false;
            }

            return true;
        }
        #endregion
    }
    #endregion

    #region Class BaseClassTypeFilterProvider
    internal sealed class BaseClassTypeFilterProvider : ITypeFilterProvider
    {
        private IServiceProvider serviceProvider;

        public BaseClassTypeFilterProvider(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        #region ITypeFilterProvider Members
        bool ITypeFilterProvider.CanFilterType(Type type, bool throwOnError)
        {
            bool filterType = false;
            if (TypeProvider.IsAssignable(typeof(Activity), type) && type.IsPublic && !type.IsSealed && !type.IsAbstract && !(type is DesignTimeType))
            {
                filterType = true;
            }

            return filterType;
        }

        string ITypeFilterProvider.FilterDescription
        {
            get
            {
                return SR.GetString(SR.CustomActivityBaseClassTypeFilterProviderDesc);
            }
        }
        #endregion
    }
    #endregion

    #region CustomProperties
    internal sealed class CustomProperty
    {
        public string oldPropertyName;
        public string oldPropertyType;
        private string name;
        private string type;
        private string category;
        private string description;
        private DesignerSerializationVisibility designerSerializationVisibility = DesignerSerializationVisibility.Visible;
        // NOTE: we don't write the ValidationOption attribute anymore (WinOE Bug 17398). We have removed our property creation
        // dialog in beta1.  Now this code is only used for property promotion.  If the promoted property is a meta property,
        // it can not be bindable so no promotion is not allowed.  If the property is an instance property, this attribute is ignored.  
        // There is no reason for writing out this attribute anymore.We just remove it from property promotion all together.
        // NOTE II: for the same reason that this code is only used for promotion, we don't write out meta properties anymore.
        // We had customized the CodeDomSerializer.Deserialize to recognize meta properties by inspecting the field init expression,
        // which is no long needed.  If we were to bring this functionality back in the future, sample code can be found
        // from the file history in Source Depot.
        private bool isEvent = false;
        private bool browseable = true;
        private bool hidden = false;
        private string uiTypeEditor;
        private IServiceProvider serviceProvider = null;
        private bool generateDependencyProperty = true;

        public CustomProperty(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        public string Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }

        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = value;
            }
        }

        public string Category
        {
            get
            {
                return this.category;
            }
            set
            {
                this.category = value;
            }
        }

        public bool Browseable
        {
            get
            {
                return this.browseable;
            }
            set
            {
                this.browseable = value;
            }
        }

        public DesignerSerializationVisibility DesignerSerializationVisibility
        {
            get
            {
                return this.designerSerializationVisibility;
            }
            set
            {
                this.designerSerializationVisibility = value;
            }
        }

        public string UITypeEditor
        {
            get
            {
                return this.uiTypeEditor;
            }
            set
            {
                string typeName = value;

                // Try to make sure the type is specified witht he fullName;
                if (this.serviceProvider != null)
                {
                    ITypeProvider typeProvider = this.serviceProvider.GetService(typeof(ITypeProvider)) as ITypeProvider;
                    if (typeProvider == null)
                        throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(ITypeProvider).FullName));

                    Type type = typeProvider.GetType(typeName);
                    if (type != null)
                        typeName = type.FullName;
                }

                this.uiTypeEditor = typeName;
            }
        }

        public bool IsEvent
        {
            get
            {
                return this.isEvent;
            }
            set
            {
                this.isEvent = value;
            }
        }

        public bool Hidden
        {
            get
            {
                return this.hidden;
            }
            set
            {
                this.hidden = value;
            }
        }

        public bool GenerateDependencyProperty
        {
            get
            {
                return this.generateDependencyProperty;
            }

            set
            {
                this.generateDependencyProperty = value;
            }
        }

        #region Private Helpers
        public static CustomProperty CreateCustomProperty(IServiceProvider serviceProvider, string customPropertyName, PropertyDescriptor propertyDescriptor, object propertyOwner)
        {
            CustomProperty newCustomProperty = new CustomProperty(serviceProvider);
            newCustomProperty.Name = customPropertyName;
            if (TypeProvider.IsAssignable(typeof(ActivityBind), propertyDescriptor.PropertyType))
            {
                Type baseType = PropertyDescriptorUtils.GetBaseType(propertyDescriptor, propertyOwner, serviceProvider);
                if (baseType == null)
                    throw new InvalidOperationException(SR.GetString(SR.Error_CantDeterminePropertyBaseType, propertyDescriptor.Name));
                newCustomProperty.Type = baseType.FullName;
            }
            else
            {
                newCustomProperty.Type = propertyDescriptor.PropertyType.FullName;
            }

            if (propertyDescriptor is ActivityBindPropertyDescriptor)
            {
                DependencyProperty dependencyProperty = DependencyProperty.FromName(propertyDescriptor.Name, propertyDescriptor.ComponentType);
                newCustomProperty.IsEvent = (dependencyProperty != null && dependencyProperty.IsEvent);
            }

            newCustomProperty.Category = propertyDescriptor.Category;
            return newCustomProperty;
        }
        #endregion
    }
    #endregion

}
