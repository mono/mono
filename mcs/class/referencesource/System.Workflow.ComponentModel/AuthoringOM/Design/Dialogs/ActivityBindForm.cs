using System;
using System.Collections.Generic;
using System.Globalization;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Workflow.ComponentModel;
using System.ComponentModel.Design;
using System.Workflow.ComponentModel.Compiler;
using System.Reflection;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Windows.Forms.Design;
using System.Diagnostics.CodeAnalysis;

namespace System.Workflow.ComponentModel.Design
{
    internal sealed partial class ActivityBindForm : Form
    {
        //ui control
        private ActivityBindFormWorkflowOutline workflowOutline = null;

        private IServiceProvider serviceProvider = null;
        private ITypeDescriptorContext context = null;
        private Type boundType = null;

        //returns
        private ActivityBind binding = null;
        private bool createNew = false; //create new field or use existing
        private bool createNewProperty = false; //create property or field
        private string newMemberName = string.Empty;

        private const string MemberTypeFormat = "MemberType#{0}"; //non-localiazable, used for image list keys

        private string ActivityBindDialogTitleFormat; // = "Bind '{0}' to Activity Property";
        private string PropertyAssignableFormat; // = "Selected property of type '{0}' is assignable to the target property type '{1}'.";
        private string DescriptionFormat; // = " It has description '{0}'.";
        private string EditIndex; // = " You may change index(es) on the selected tree item either by clicking on a node with the left mouse button or by pressing Alt-I.";

        private string PleaseSelectCorrectActivityProperty; // = "Select a property of type '{0}'. Currently selected property of type \"{1}\" isn't assignable to the target type.";
        private string PleaseSelectActivityProperty; // = "Select a property of type \"{0}\" on an activity from the workflow activity tree.";
        private string IncorrectIndexChange; // = "New index expression \"{0}\" is incorrect.";

        private string CreateNewMemberHelpFormat; // = "Enter new member name you want to be created on the root activity for property promotion, then choose the kind of member between either a field or a property.\nNew member will be of type '{0}'.";

        System.Windows.Forms.ImageList memberTypes = null;
        List<CustomProperty> properties;

        public ActivityBindForm(IServiceProvider serviceProvider, ITypeDescriptorContext context)
        {
            this.context = context;
            this.serviceProvider = serviceProvider;

            InitializeComponent();
            this.createProperty.Checked = true; //make the property to be the default emitted entity

            this.helpTextBox.Multiline = true;

            //Set dialog fonts
            IUIService uisvc = (IUIService)this.serviceProvider.GetService(typeof(IUIService));
            if (uisvc != null)
                this.Font = (Font)uisvc.Styles["DialogFont"];

            //add images to the tree-view's imagelist
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ActivityBindForm));

            ActivityBindDialogTitleFormat = resources.GetString("ActivityBindDialogTitleFormat");
            PropertyAssignableFormat = resources.GetString("PropertyAssignableFormat");
            DescriptionFormat = resources.GetString("DescriptionFormat");
            EditIndex = resources.GetString("EditIndex");
            PleaseSelectCorrectActivityProperty = resources.GetString("PleaseSelectCorrectActivityProperty");
            PleaseSelectActivityProperty = resources.GetString("PleaseSelectActivityProperty");
            IncorrectIndexChange = resources.GetString("IncorrectIndexChange");
            CreateNewMemberHelpFormat = resources.GetString("CreateNewMemberHelpFormat");

            this.memberTypes = new System.Windows.Forms.ImageList();
            this.memberTypes.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("memberTypes.ImageStream")));
            this.memberTypes.TransparentColor = AmbientTheme.TransparentColor;
            //this.memberTypes.Images.SetKeyName(0, "Field_Public");
            //this.memberTypes.Images.SetKeyName(1, "Field_Internal");
            //this.memberTypes.Images.SetKeyName(2, "Field_Protected");
            //this.memberTypes.Images.SetKeyName(3, "Field_Private");
            //this.memberTypes.Images.SetKeyName(4, "Property_Public");
            //this.memberTypes.Images.SetKeyName(5, "Property_Internal");
            //this.memberTypes.Images.SetKeyName(6, "Property_Protected");
            //this.memberTypes.Images.SetKeyName(7, "Property_Private");
            //this.memberTypes.Images.SetKeyName(8, "Constant_Public");
            //this.memberTypes.Images.SetKeyName(9, "Constant_Internal");
            //this.memberTypes.Images.SetKeyName(10, "Constant_Protected");
            //this.memberTypes.Images.SetKeyName(11, "Constant_Private");
            //this.memberTypes.Images.SetKeyName(12, "Event_Public");
            //this.memberTypes.Images.SetKeyName(13, "Event_Internal");
            //this.memberTypes.Images.SetKeyName(14, "Event_Protected");
            //this.memberTypes.Images.SetKeyName(15, "Event_Private");
            //this.memberTypes.Images.SetKeyName(16, "Delegate_Public");
            //this.memberTypes.Images.SetKeyName(17, "Delegate_Internal");
            //this.memberTypes.Images.SetKeyName(18, "Delegate_Protected");
            //this.memberTypes.Images.SetKeyName(19, "Delegate_Private");
            //this.memberTypes.Images.SetKeyName(20, "Index_Public");
            //this.memberTypes.Images.SetKeyName(21, "Index_Internal");
            //this.memberTypes.Images.SetKeyName(22, "Index_Protected");
            //this.memberTypes.Images.SetKeyName(23, "Index_Private");

            //preload custom properties before getting type from the type provider (as it would refresh the types)
            this.properties = CustomActivityDesignerHelper.GetCustomProperties(context);

        }

        #region return properties
        public ActivityBind Binding
        {
            get
            {
                return this.binding;
            }
        }
        public bool CreateNew
        {
            get
            {
                return this.createNew;
            }
        }
        public bool CreateNewProperty
        {
            get
            {
                return this.createNewProperty;
            }
        }
        public string NewMemberName
        {
            get
            {
                return this.newMemberName;
            }
        }
        #endregion

        private void ActivityBindForm_Load(object sender, EventArgs e)
        {
            this.Text = string.Format(CultureInfo.CurrentCulture, ActivityBindDialogTitleFormat, context.PropertyDescriptor.Name);


            if (this.context.PropertyDescriptor is DynamicPropertyDescriptor)
                this.boundType = PropertyDescriptorUtils.GetBaseType(this.context.PropertyDescriptor, PropertyDescriptorUtils.GetComponent(context), serviceProvider);

            if (this.boundType != null)
            {
                //lets get the same type through the type provider (otherwise this type may mismatch with the one obtained from the design time types)
                ITypeProvider typeProvider = this.serviceProvider.GetService(typeof(ITypeProvider)) as ITypeProvider;
                if (typeProvider != null)
                {
                    Type designTimeType = typeProvider.GetType(this.boundType.FullName, false);
                    this.boundType = (designTimeType != null) ? designTimeType : this.boundType;
                }
            }

            //create outline control
            this.workflowOutline = new ActivityBindFormWorkflowOutline(this.serviceProvider, this);
            this.dummyPanel.BorderStyle = BorderStyle.None;
            this.dummyPanel.SuspendLayout();
            this.dummyPanel.Controls.Add(this.workflowOutline);
            this.workflowOutline.Location = new Point(3, 3);
            this.workflowOutline.Size = new Size(199, 351);
            this.workflowOutline.Dock = DockStyle.Fill;
            this.dummyPanel.ResumeLayout(false);

            this.workflowOutline.AddMemberKindImages(this.memberTypes);

            //make the outline view load initial state
            this.workflowOutline.ReloadWorkflowOutline();

            //expand just the root node
            this.workflowOutline.ExpandRootNode();

            //now we need to select the activity/path which was previously set
            //NOTE: we would have to expand all nodes on the way to make doc outline control populate their children
            Activity activity = PropertyDescriptorUtils.GetComponent(context) as Activity;
            if (activity == null)
            {
                IReferenceService rs = this.context.GetService(typeof(IReferenceService)) as IReferenceService;
                if (rs != null)
                    activity = rs.GetComponent(this.context.Instance) as Activity;
            }

            ActivityBind previousBinding = context.PropertyDescriptor.GetValue(context.Instance) as ActivityBind;
            if (activity != null && previousBinding != null)
            {
                Activity previousBindActivity = Helpers.ParseActivity(Helpers.GetRootActivity(activity), previousBinding.Name);
                if (previousBindActivity != null)
                    this.workflowOutline.SelectActivity(previousBindActivity, ParseStringPath(GetActivityType(previousBindActivity), previousBinding.Path));
            }

            if (this.properties != null)
            {
                List<String> customPropertyNames = new List<String>();
                foreach (CustomProperty customProperty in this.properties)
                    customPropertyNames.Add(customProperty.Name);

                // set default name
                this.memberNameTextBox.Text = DesignerHelpers.GenerateUniqueIdentifier(this.serviceProvider, activity.Name + "_" + context.PropertyDescriptor.Name, customPropertyNames.ToArray());
            }

            this.newMemberHelpTextBox.Lines = string.Format(CultureInfo.CurrentCulture, CreateNewMemberHelpFormat, GetSimpleTypeFullName(this.boundType)).Split(new char[] { '\n' });
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.None;

            this.createNew = (this.bindTabControl.SelectedIndex != this.bindTabControl.TabPages.IndexOf(this.existingMemberPage));
            if (this.createNew)
            {
                //
                this.createNewProperty = this.createProperty.Checked;
                this.newMemberName = this.memberNameTextBox.Text;
                //validate name based on the prop promotion dialog
                this.DialogResult = ValidateNewMemberBind(this.newMemberName);
            }
            else
            {
                this.DialogResult = ValidateExistingPropertyBind();
            }
        }

        private DialogResult ValidateExistingPropertyBind()
        {
            Activity activity = this.workflowOutline.SelectedActivity;
            PathInfo member = this.workflowOutline.SelectedMember;
            string propertyPath = this.workflowOutline.PropertyPath; //the path on the PathInfo will be incorrect if user had changed indexes

            if (activity == null || member == null)
            {
                string message = SR.GetString(SR.Error_BindDialogNoValidPropertySelected, GetSimpleTypeFullName(this.boundType));
                DesignerHelpers.ShowError(this.serviceProvider, message);
                return DialogResult.None;
            }

            Type parsedPropertyType = member.PropertyType;
            //lets get the same type through the type provider (otherwise this type may mismatch with the one obtained from the design time types)
            ITypeProvider typeProvider = this.serviceProvider.GetService(typeof(ITypeProvider)) as ITypeProvider;
            if (typeProvider != null && parsedPropertyType != null)
            {
                Type designTimeParsedType = typeProvider.GetType(parsedPropertyType.FullName, false);
                parsedPropertyType = (designTimeParsedType != null) ? designTimeParsedType : parsedPropertyType;
            }

            if (this.boundType != parsedPropertyType && !TypeProvider.IsAssignable(this.boundType, parsedPropertyType))
            {
                string message = SR.GetString(SR.Error_BindDialogWrongPropertyType, GetSimpleTypeFullName(parsedPropertyType), GetSimpleTypeFullName(this.boundType));
                DesignerHelpers.ShowError(this.serviceProvider, message);
                return DialogResult.None;
            }

            //this is the selected activity which property is being bound
            Activity bindingActivity = PropertyDescriptorUtils.GetComponent(this.context) as Activity;

            //this is the name of the property we are binding
            string propertyName = context.PropertyDescriptor.Name;
            if (bindingActivity == activity && member != null && member.Path.Equals(propertyName, StringComparison.Ordinal))
            {
                DesignerHelpers.ShowError(this.serviceProvider, SR.GetString(SR.Error_BindDialogCanNotBindToItself));
                return DialogResult.None;
            }

            if (activity != null && member != null)
            {
                //
                ActivityBind bind = new ActivityBind(activity.QualifiedName, propertyPath);

                ValidationManager manager = new ValidationManager(this.serviceProvider);
                PropertyValidationContext propertyValidationContext = new PropertyValidationContext(this.context.Instance, DependencyProperty.FromName(this.context.PropertyDescriptor.Name, this.context.Instance.GetType()));
                manager.Context.Append(this.context.Instance); //

                ValidationErrorCollection errors;
                using (WorkflowCompilationContext.CreateScope(manager))
                {
                    errors = ValidationHelpers.ValidateProperty(manager, bindingActivity, bind, propertyValidationContext);
                }
                if (errors != null && errors.Count > 0 && errors.HasErrors)
                {
                    string message = string.Empty;
                    for (int i = 0; i < errors.Count; i++)
                    {
                        ValidationError error = errors[i];
                        message += error.ErrorText + ((i == errors.Count - 1) ? string.Empty : "; ");
                    }

                    message = SR.GetString(SR.Error_BindDialogBindNotValid) + message;
                    DesignerHelpers.ShowError(this.serviceProvider, message);
                    return DialogResult.None;
                }
                else
                {
                    this.binding = bind;
                    return DialogResult.OK;
                }
            }

            return DialogResult.None;
        }


        [SuppressMessage("Microsoft.Globalization", "CA130:UseOrdinalStringComparison", MessageId = "System.String.Compare(System.String,System.String,System.Boolean,System.Globalization.CultureInfo)", Justification = "This is a design time method and so there is no security issue")]
        private DialogResult ValidateNewMemberBind(string newMemberName)
        {
            Activity activity = PropertyDescriptorUtils.GetComponent(context) as Activity;
            if (activity == null)
            {
                IReferenceService rs = this.context.GetService(typeof(IReferenceService)) as IReferenceService;
                if (rs != null)
                    activity = rs.GetComponent(this.context.Instance) as Activity;
            }

            string errorMsg = null;
            try
            {
                ValidationHelpers.ValidateIdentifier(context, newMemberName);
            }
            catch
            {
                errorMsg = SR.GetString(SR.Error_InvalidLanguageIdentifier, newMemberName);
            }

            // get all the members of the custom activity to ensure uniqueness
            Type customActivityType = CustomActivityDesignerHelper.GetCustomActivityType(context);
            SupportedLanguages language = CompilerHelpers.GetSupportedLanguage(context);
            foreach (MemberInfo memberInfo in customActivityType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                if (string.Compare(memberInfo.Name, newMemberName, language == SupportedLanguages.VB, CultureInfo.InvariantCulture) == 0)
                {
                    errorMsg = SR.GetString(SR.Failure_FieldAlreadyExist);
                    break;
                }
            }

            // ctor name should be checked separately
            if (errorMsg == null && string.Compare(customActivityType.Name, newMemberName, language == SupportedLanguages.VB, CultureInfo.InvariantCulture) == 0)
                errorMsg = SR.GetString(SR.Failure_FieldAlreadyExist);

            if (errorMsg == null)
            {
                ActivityBind newBind = new ActivityBind(ActivityBind.GetRelativePathExpression(Helpers.GetRootActivity(activity), activity), newMemberName);
                IDesignerHost host = this.context.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (host == null)
                    throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(IDesignerHost).FullName));
                this.binding = newBind;
            }
            else
            {
                DesignerHelpers.ShowError(context, errorMsg);
            }

            return ((errorMsg == null) ? DialogResult.OK : DialogResult.None);
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void SelectedActivityChanged(Activity activity, PathInfo memberPathInfo, string path)
        {
            string helpMessage = string.Empty;
            string desiredType = GetSimpleTypeFullName(this.boundType);

            if (memberPathInfo != null)
            {
                if (path == null || path.Length == 0)
                {
                    helpMessage = string.Format(CultureInfo.CurrentCulture, PleaseSelectActivityProperty, desiredType);
                }
                else
                {
                    string memberName = MemberActivityBindTreeNode.MemberName(memberPathInfo.Path);
                    string memberType = GetSimpleTypeFullName(memberPathInfo.PropertyType);
                    string memberDescription = GetMemberDescription(memberPathInfo.MemberInfo);

                    if (TypeProvider.IsAssignable(this.boundType, memberPathInfo.PropertyType))
                        helpMessage = string.Format(CultureInfo.CurrentCulture, PropertyAssignableFormat, memberType, desiredType) + ((memberDescription.Length > 0) ? string.Format(CultureInfo.CurrentCulture, DescriptionFormat, memberDescription) : string.Empty);
                    else
                        helpMessage = string.Format(CultureInfo.CurrentCulture, PleaseSelectCorrectActivityProperty, desiredType, memberType);

                    helpMessage += ((MemberActivityBindTreeNode.MemberName(path).IndexOfAny(new char[] { '[', ']' }) != -1) ? EditIndex : string.Empty);
                }
            }
            else
            {
                helpMessage = string.Format(CultureInfo.CurrentCulture, PleaseSelectActivityProperty, desiredType);
            }

            this.helpTextBox.Lines = helpMessage.Split(new char[] { '\n' });
        }

        private List<PathInfo> PopulateAutoCompleteList(Activity activity, PathInfo path)
        {
            List<PathInfo> currentPropertyList = new List<PathInfo>();
            Type activityType = GetActivityType(activity);
            PathInfo[] subProps = (activityType != null) ? ProcessPaths(activityType, path) : null;
            if (subProps != null)
                currentPropertyList.AddRange(subProps);

            return currentPropertyList;
        }

        private Type GetActivityType(Activity activity)
        {
            Type activityType = null;

            IDesignerHost designerHost = this.serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            WorkflowDesignerLoader loader = this.serviceProvider.GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
            if (designerHost != null && loader != null && activity.Parent == null)
            {
                ITypeProvider typeProvider = this.serviceProvider.GetService(typeof(ITypeProvider)) as ITypeProvider;
                if (typeProvider != null)
                    activityType = typeProvider.GetType(designerHost.RootComponentClassName, false);
            }

            if (activityType == null)
                activityType = activity.GetType();

            return activityType;
        }

        #region help - related
        private void ActivityBindForm_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            GetHelp();
        }

        protected override void OnHelpRequested(HelpEventArgs e)
        {
            e.Handled = true;
            GetHelp();
        }

        private void GetHelp()
        {
            DesignerHelpers.ShowHelpFromKeyword(this.serviceProvider, typeof(ActivityBindForm).FullName + ".UI");
        }
        #endregion

        //given activity and current path, process all immediate children properties of the selected property
        private PathInfo[] ProcessPaths(Type activityType, PathInfo topProperty)
        {
            List<PathInfo> paths = new List<PathInfo>();

            if (topProperty == null)
            {
                paths.AddRange(GetSubPropertiesOnType(activityType, string.Empty));
            }
            else //topProperty != null
            {
                //sub properties on activity properties
                paths.AddRange(GetSubPropertiesOnType(topProperty.PropertyType, topProperty.Path));
            }

            return paths.ToArray();
        }

        private PathInfo[] GetArraySubProperties(Type propertyType, string currentPath)//(PathInfo pathInfo)
        {
            List<PathInfo> paths = new List<PathInfo>();

            if (propertyType != typeof(string))//ignore char item[int] on the string
            {
                List<MethodInfo> getterMethodInfos = new List<MethodInfo>();
                MemberInfo[] arrayMembers = null;
                try
                {
                    arrayMembers = propertyType.GetDefaultMembers();
                }
                catch (NotImplementedException)
                {
                    //Even if we encounted a RTTTypeWrapper that doesnt implement GetDefaultMemebers dont crash.
                    //we should atleast be able to continue to bind to other members of the type.
                }
                catch (ArgumentException)
                {
                    // This is a work-around for DevDiv Bugs 109401.  Type.GetDefaultMembers() can throw 
                    // ArgumentException in certain circumstances.  In order to avoid crashing the designer host 
                    // (typically VS), we must handle the exception and ignore the offending type.
                }
                if (arrayMembers != null && arrayMembers.Length > 0)
                {
                    foreach (MemberInfo member in arrayMembers)
                    {
                        if (member is PropertyInfo)
                            getterMethodInfos.Add((member as PropertyInfo).GetGetMethod());
                    }
                }

                if (propertyType.IsArray)
                {
                    MemberInfo[] getMembers = propertyType.GetMember("Get"); //arrays will always implement that
                    if (getMembers != null && getMembers.Length > 0)
                    {
                        foreach (MemberInfo member in getMembers)
                            if (member is MethodInfo)
                                getterMethodInfos.Add(member as MethodInfo);
                    }
                }

                foreach (MethodInfo info in getterMethodInfos)
                {
                    string indexString = ConstructIndexString(info);
                    if (indexString != null)
                    {
                        //add array accessor
                        paths.Add(new PathInfo(currentPath + indexString, info, info.ReturnType));
                    }
                }
            }

            return paths.ToArray();
        }

        private string ConstructIndexString(MethodInfo getterMethod)
        {
            StringBuilder indexString = new StringBuilder();

            ParameterInfo[] parameters = getterMethod.GetParameters();
            if (parameters != null && parameters.Length > 0)
            {
                indexString.Append("[");

                for (int i = 0; i < parameters.Length; i++)
                {
                    ParameterInfo parameter = parameters[i];
                    string subIndex = GetIndexerString(parameter.ParameterType);
                    if (subIndex == null)
                        return null;

                    indexString.Append(subIndex);
                    if (i < parameters.Length - 1)
                        indexString.Append(",");
                }

                indexString.Append("]");
            }

            return indexString.ToString();
        }

        private string GetIndexerString(Type indexType)
        {
            //The primitive types are Boolean, Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, Char, Double, and Single.
            object defaultIndexerInstance = null;
            if (IsTypePrimitive(indexType))
            {
                try
                {
                    //we'll just new the instance and get the string value out of the default one
                    defaultIndexerInstance = Activator.CreateInstance(indexType);
                }
                catch
                {
                    defaultIndexerInstance = null;
                }
            }
            else if (indexType == typeof(string))
            {
                defaultIndexerInstance = "\"<name>\"";
            }

            return (defaultIndexerInstance != null) ? defaultIndexerInstance.ToString() : null;
        }

        PropertyInfo[] GetProperties(Type type)
        {
            List<PropertyInfo> members = new List<PropertyInfo>();
            members.AddRange(type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy));
            if (type.IsInterface)
            {
                Type[] interfaces = type.GetInterfaces();
                foreach (Type implementedInterface in interfaces)
                {
                    members.AddRange(implementedInterface.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy));
                }
            }

            return members.ToArray();
        }

        //quite expensive - uses reflection to go over all public properties/field/events
        private PathInfo[] GetSubPropertiesOnType(Type typeToGetPropertiesOn, string currentPath)
        {
            List<PathInfo> paths = new List<PathInfo>();

            if (typeToGetPropertiesOn == typeof(string) || (TypeProvider.IsAssignable(typeof(System.Delegate), typeToGetPropertiesOn) && !this.boundType.IsSubclassOf(typeof(Delegate))))//ignore char item[int] on the string
                return paths.ToArray();

            currentPath = (string.IsNullOrEmpty(currentPath)) ? string.Empty : currentPath + ".";
            ITypeProvider typeProvider = this.serviceProvider.GetService(typeof(ITypeProvider)) as ITypeProvider;

            foreach (PropertyInfo property in GetProperties(typeToGetPropertiesOn))
            {
                MethodInfo getterMethod = property.GetGetMethod();
                Type memberType = BindHelpers.GetMemberType(property);
                if (memberType == null)
                    continue;

                if (typeProvider != null)
                {
                    Type designTimeMemberType = typeProvider.GetType(memberType.FullName, false);
                    memberType = (designTimeMemberType != null) ? designTimeMemberType : memberType;
                }

                //if (memberType == typeof(WorkflowParameterBindingCollection) && string.IsNullOrEmpty(currentPath))
                //{
                //    //special case for the parameters collection on an activity itself (when path is empty)
                //    Activity activity = this.workflowOutline.SelectedActivity;
                //    if(getterMethod != null && typeToGetPropertiesOn == activity.GetType())
                //    {
                //        WorkflowParameterBindingCollection collection = getterMethod.Invoke(activity, null) as WorkflowParameterBindingCollection;
                //        if (collection != null)
                //        {
                //            foreach (WorkflowParameterBinding parameterBinding in collection)
                //            {
                //                //note that the currentPath is always empty
                //                paths.Add(new PathInfo(property.Name + "[\"" + parameterBinding.ParameterName + "\"].Value", typeof(object)));
                //            }
                //        }
                //    }
                //}

                //if it's a primitive and not equal to the desired type, skip it. 
                //skip properties of type object if the target property is not object
                if (IsPropertyBrowsable(property) &&
                    getterMethod != null && memberType != null &&
                    (!IsTypePrimitive(memberType) || TypeProvider.IsAssignable(this.boundType, memberType)) &&
                    !((this.boundType != typeof(object) && memberType == typeof(object))))
                {
                    //some properties are indexers... analyze the parameters on the getter method
                    // C#: at design time indexer property is called "this" while at runtime it gets renamed to "Item"
                    // VB: indexer is called Item at design and runtime .
                    string propertyName = property.Name;
                    propertyName = currentPath + propertyName + ConstructIndexString(getterMethod);
                    paths.Add(new PathInfo(propertyName, property, memberType));
                    paths.AddRange(GetArraySubProperties(memberType, propertyName));
                }
            }

            //

            foreach (FieldInfo field in typeToGetPropertiesOn.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy))//BindingFlags.Static is needed for the const fields
            {
                Type fieldType = BindHelpers.GetMemberType(field);
                if (fieldType == null)
                    continue;

                if (TypeProvider.IsAssignable(typeof(DependencyProperty), fieldType))
                    continue; //dont want to show all static public dependency properties fields

                if (typeProvider != null)
                {
                    Type designTimeFieldType = typeProvider.GetType(fieldType.FullName, false);
                    fieldType = (designTimeFieldType != null) ? designTimeFieldType : fieldType;
                }

                //if it's a primitive and not equal to the desired type, skip it.
                //
                if (IsPropertyBrowsable(field) && fieldType != null &&
                    (!IsTypePrimitive(fieldType) || TypeProvider.IsAssignable(this.boundType, fieldType)) && //primitive fields should only be shown for primitive properties
                    !(this.boundType != typeof(object) && fieldType == typeof(object)) && //fields of type object should only be shown for properties of type object
                    !(!TypeProvider.IsAssignable(typeof(Delegate), this.boundType) && TypeProvider.IsAssignable(typeof(Delegate), fieldType)))//fields of type delegate should only be shown for delegate properties
                {
                    string fieldName = currentPath + field.Name;
                    paths.Add(new PathInfo(fieldName, field, BindHelpers.GetMemberType(field)));
                    paths.AddRange(GetArraySubProperties(fieldType, fieldName));
                }
            }

            //we will populate events only if the target type is event (since it is always going to be the last valid entry in the path)
            if (this.boundType.IsSubclassOf(typeof(Delegate)))//System.MulticastDelegate ???
            {
                foreach (EventInfo eventInfo in typeToGetPropertiesOn.GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
                {
                    Type eventType = BindHelpers.GetMemberType(eventInfo);
                    if (eventType == null)
                        continue;

                    if (typeProvider != null)
                    {
                        Type designTimeEventType = typeProvider.GetType(eventType.FullName, false);
                        eventType = (designTimeEventType != null) ? designTimeEventType : eventType;
                    }

                    if (IsPropertyBrowsable(eventInfo) && eventType != null && TypeProvider.IsAssignable(this.boundType, eventType))
                        paths.Add(new PathInfo(currentPath + eventInfo.Name, eventInfo, eventType));
                }
            }

            return paths.ToArray();
        }

        private string GetMemberDescription(MemberInfo member)
        {
            object[] descriptions = member.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (descriptions != null && descriptions.Length > 0)
            {
                DescriptionAttribute description = descriptions[0] as DescriptionAttribute;
                return (description != null) ? description.Description : string.Empty;
            }

            return string.Empty;
        }

        //given user typed path, find all properties along it and return them in the list
        private List<PathInfo> ParseStringPath(Type activityType, string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            List<PathInfo> pathInfoList = new List<PathInfo>();

            PathWalker pathWalker = new PathWalker();
            PathMemberInfoEventArgs finalEventArgs = null;
            PathErrorInfoEventArgs errorEventArgs = null;
            pathWalker.MemberFound += delegate(object sender, PathMemberInfoEventArgs eventArgs)
            {
                finalEventArgs = eventArgs; //store the latest args
                pathInfoList.Add(new PathInfo(eventArgs.Path, eventArgs.MemberInfo, BindHelpers.GetMemberType(eventArgs.MemberInfo)));
            };
            pathWalker.PathErrorFound += delegate(object sender, PathErrorInfoEventArgs eventArgs)
            {
                errorEventArgs = eventArgs; //store the error args
            };

            pathWalker.TryWalkPropertyPath(activityType, path);
            return pathInfoList;
        }

        private bool IsPropertyBrowsable(MemberInfo property)
        {
            object[] attributes = property.GetCustomAttributes(typeof(BrowsableAttribute), false);
            if (attributes.Length > 0)
            {
                BrowsableAttribute attribute = attributes[0] as BrowsableAttribute;
                if (attribute != null)
                    return attribute.Browsable;
                else
                {
                    AttributeInfoAttribute attributeInfoAttribute = attributes[0] as AttributeInfoAttribute;
                    if (attributeInfoAttribute != null)
                    {
                        ReadOnlyCollection<object> argumentValues = attributeInfoAttribute.AttributeInfo.ArgumentValues;
                        if (argumentValues.Count > 0)
                            return Convert.ToBoolean(argumentValues[0], CultureInfo.InvariantCulture);
                    }
                }
            }

            return true;
        }

        //primitive types - we dont expand them
        private static bool IsTypePrimitive(Type type)
        {
            return type.IsPrimitive || type.IsEnum || type == typeof(Guid) || type == typeof(IntPtr) || type == typeof(string) || type == typeof(DateTime) || type == typeof(TimeSpan);
        }

        //
        private string GetSimpleTypeFullName(Type type)
        {
            if (type == null)
                return string.Empty;

            StringBuilder typeName = new StringBuilder(type.FullName);
            Stack<Type> types = new Stack<Type>();
            types.Push(type);

            while (types.Count > 0)
            {
                type = types.Pop();

                while (type.IsArray)
                    type = type.GetElementType();

                if (type.IsGenericType && !type.IsGenericTypeDefinition)
                {
                    foreach (Type parameterType in type.GetGenericArguments())
                    {
                        typeName.Replace("[" + parameterType.AssemblyQualifiedName + "]", GetSimpleTypeFullName(parameterType));
                        types.Push(parameterType);
                    }
                }
            }

            return typeName.ToString();
        }

        #region Class ActivityBindFormWorkflowOutline
        internal enum BindMemberAccessKind
        {
            Public = 0, Internal = 1, Protected = 2, Private = 3,
        }

        internal enum BindMemberKind
        {
            Field = 0, Property = 4, Constant = 8, Event = 12, Delegate = 16, Index = 20 //Index doesnt have any image
        };

        //tree node to be used in the activity bind form for activity nodes
        private class ActivityBindTreeNode : WorkflowOutlineNode
        {
            public ActivityBindTreeNode(Activity activity)
                : base(activity)
            { }
        }

        private class DummyActivityBindTreeNode : WorkflowOutlineNode
        {
            public DummyActivityBindTreeNode(Activity activity)
                : base(activity)
            { }
        }

        //used for members of activities or their nested members
        private class MemberActivityBindTreeNode : ActivityBindTreeNode
        {
            //all member nodes have activity property set to the closest activity in the tree
            private PathInfo pathInfo = null;
            private BindMemberKind kind = BindMemberKind.Property;
            private BindMemberAccessKind accessKind = BindMemberAccessKind.Public;

            public MemberActivityBindTreeNode(Activity activity, PathInfo pathInfo)
                : base(activity)
            {
                this.pathInfo = pathInfo;

                string memberName = MemberName(this.PathInfo.Path);

                // Field Property Constant Event Delegate Index
                if (this.pathInfo.MemberInfo is EventInfo)
                {
                    this.kind = BindMemberKind.Event;
                    this.accessKind = BindMemberAccessKind.Public; //this.accessKind = (this.pathInfo.MemberInfo as EventInfo).Attributes 
                }
                else if (this.pathInfo.MemberInfo is FieldInfo)
                {
                    FieldInfo fieldInfo = this.pathInfo.MemberInfo as FieldInfo;
                    if ((fieldInfo.Attributes & FieldAttributes.Static) != 0 && (fieldInfo.Attributes & FieldAttributes.Literal) != 0)
                    {
                        this.kind = BindMemberKind.Constant;
                    }
                    else
                    {
                        if (TypeProvider.IsAssignable(typeof(Delegate), fieldInfo.FieldType))
                            this.kind = BindMemberKind.Delegate;
                        else
                            this.kind = BindMemberKind.Field;
                    }
                    this.accessKind = (fieldInfo.IsPublic) ? BindMemberAccessKind.Public : ((fieldInfo.IsFamily) ? BindMemberAccessKind.Internal : (fieldInfo.IsPrivate) ? BindMemberAccessKind.Private : BindMemberAccessKind.Protected);
                }
                else if (this.pathInfo.MemberInfo is PropertyInfo)
                {
                    this.kind = BindMemberKind.Property;
                    PropertyInfo propertyInfo = this.pathInfo.MemberInfo as PropertyInfo;
                    this.accessKind = BindMemberAccessKind.Public; //
                }
                else if (memberName.IndexOfAny("[]".ToCharArray()) != -1)
                {
                    this.kind = BindMemberKind.Index;
                    this.accessKind = BindMemberAccessKind.Public; //
                }
                else
                {
                    this.kind = BindMemberKind.Property;
                    this.accessKind = BindMemberAccessKind.Public; //
                }
            }

            public override void RefreshNode()
            {
                base.RefreshNode();
                this.Text = MemberName(this.PathInfo.Path);
                this.ForeColor = Color.DarkBlue;
            }

            public PathInfo PathInfo
            {
                get
                {
                    return this.pathInfo;
                }
                set
                {
                    this.pathInfo = value;
                }
            }

            public bool MayHaveChildNodes
            {
                get
                {
                    Type memberType = (this.pathInfo != null) ? this.pathInfo.PropertyType : null;
                    if (memberType == null)
                        return false;

                    if (IsTypePrimitive(memberType) || (TypeProvider.IsAssignable(typeof(System.Delegate), memberType)) || (memberType == typeof(object)))
                        return false;

                    return true;
                }
            }

            public BindMemberKind MemberKind
            {
                get
                {
                    return this.kind;
                }
            }

            public BindMemberAccessKind MemberAccessKind
            {
                get
                {
                    return this.accessKind;
                }
            }

            internal static string MemberName(string path)
            {
                string memberName = path;
                //need to show just the latest portion of the path
                int index = memberName.LastIndexOf('.');
                memberName = (index != -1 && (index + 1) < memberName.Length) ? memberName.Substring(index + 1) : memberName;
                return memberName;
            }
        }

        private class ActivityBindFormWorkflowOutline : WorkflowOutline
        {
            private ActivityBindForm parent = null;
            private Activity selectedActivity = null;
            private PathInfo selectedPathInfo = null;

            public ActivityBindFormWorkflowOutline(IServiceProvider serviceProvider, ActivityBindForm parent)
                : base(serviceProvider)
            {
                this.parent = parent;
                base.NeedsExpandAll = false;

                this.Expanding += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeView1_BeforeExpand);

                base.TreeView.BeforeLabelEdit += new NodeLabelEditEventHandler(TreeView_BeforeLabelEdit);
                base.TreeView.AfterLabelEdit += new NodeLabelEditEventHandler(TreeView_AfterLabelEdit);
                base.TreeView.LabelEdit = true;
                base.TreeView.KeyDown += new KeyEventHandler(TreeView_KeyDown);
            }

            void TreeView_KeyDown(object sender, KeyEventArgs e)
            {
                //F2 -> start editing index
                if (e.KeyCode == Keys.F2 && base.TreeView.SelectedNode != null)
                {
                    base.TreeView.SelectedNode.BeginEdit();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            }

            void TreeView_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
            {
                //allow editing the label only if it's an array member
                MemberActivityBindTreeNode memberNode = e.Node as MemberActivityBindTreeNode;
                e.CancelEdit = (memberNode == null) || !memberNode.Text.Contains("[") || !memberNode.Text.Contains("]");
            }

            void TreeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
            {
                //
                string oldLabel = e.Node.Text;
                string newLabel = e.Label;
                if (oldLabel == null || newLabel == null)
                {
                    e.CancelEdit = true;
                    return;
                }

                MemberActivityBindTreeNode memberNode = e.Node as MemberActivityBindTreeNode;

                bool incorrectChange = false;
                //sanity check (member name has not been changed, still have opening/closing square brackets, same number of commas)
                if (newLabel.IndexOf("[", StringComparison.Ordinal) == -1 || !newLabel.EndsWith("]", StringComparison.Ordinal))
                {
                    incorrectChange = true;
                }
                else
                {
                    string oldMemberName = oldLabel.Substring(0, oldLabel.IndexOf("[", StringComparison.Ordinal));
                    string newMemberName = newLabel.Substring(0, newLabel.IndexOf("[", StringComparison.Ordinal));
                    incorrectChange = !oldMemberName.Equals(newMemberName, StringComparison.Ordinal);
                }

                //re-parse, update pathinfo member
                if (!incorrectChange)
                {
                    ActivityBindTreeNode parentNode = memberNode.Parent as ActivityBindTreeNode;
                    MemberActivityBindTreeNode memberParentNode = parentNode as MemberActivityBindTreeNode;
                    Type memberType = (memberParentNode != null) ? memberParentNode.PathInfo.PropertyType : this.parent.GetActivityType(parentNode.Activity);
                    //we will try to parse just the latest member path since the previous is assumed to be valid
                    List<PathInfo> reparsedPathInfoList = this.parent.ParseStringPath(memberType, newLabel);
                    if (reparsedPathInfoList == null || reparsedPathInfoList.Count == 0)
                    {
                        incorrectChange = true;
                    }
                    else
                    {
                        PathInfo newPathInfo = reparsedPathInfoList[reparsedPathInfoList.Count - 1]; //get the last item in the list
                        if (newPathInfo.Path.Equals(newLabel, StringComparison.Ordinal))
                            memberNode.PathInfo = newPathInfo;
                        else
                            incorrectChange = true;
                    }
                }

                if (incorrectChange)
                {
                    DesignerHelpers.ShowError(this.parent.serviceProvider, string.Format(CultureInfo.CurrentCulture, this.parent.IncorrectIndexChange, newLabel));
                    e.CancelEdit = true;
                }
            }

            private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
            {
                //see if the first child is the dummy node, replace it with members...
                ActivityBindTreeNode node = e.Node as ActivityBindTreeNode;
                if (node != null)
                {
                    if (node.Nodes.Count > 0 && (node.Nodes[0] is DummyActivityBindTreeNode))
                    {
                        //Poluate child members on this node...
                        MemberActivityBindTreeNode memberNode = node as MemberActivityBindTreeNode;
                        List<PathInfo> members = this.parent.PopulateAutoCompleteList(node.Activity, (memberNode != null) ? memberNode.PathInfo : null);
                        List<TreeNode> nodes = new List<TreeNode>();
                        foreach (PathInfo mamberPathInfo in members)
                        {
                            MemberActivityBindTreeNode childMemberNode = CreateMemberNode(node.Activity, mamberPathInfo);
                            if (childMemberNode != null)
                            {
                                RefreshNode(childMemberNode, false);
                                nodes.Add(childMemberNode);
                            }
                        }

                        base.TreeView.BeginUpdate();
                        try
                        {
                            node.Nodes.RemoveAt(0);
                            e.Node.Nodes.AddRange(nodes.ToArray());
                        }
                        finally
                        {
                            base.TreeView.EndUpdate();
                        }
                    }
                }
            }

            public void AddMemberKindImages(ImageList memberTypes)
            {
                for (int i = 0; i < memberTypes.Images.Count; i++)
                {
                    Image image = memberTypes.Images[i];
                    this.TreeView.ImageList.Images.Add(string.Format(CultureInfo.InvariantCulture, MemberTypeFormat, i), image); //member type key is non-localizable
                }
            }

            protected override WorkflowOutlineNode CreateNewNode(Activity activity)
            {
                //can't bind to commented or locked activities...
                if (!activity.Enabled || Helpers.IsActivityLocked(activity))
                    return null;

                ActivityBindTreeNode treeNode = new ActivityBindTreeNode(activity);
                treeNode.Nodes.Add(new DummyActivityBindTreeNode(activity));
                return treeNode;
            }

            private MemberActivityBindTreeNode CreateMemberNode(Activity activity, PathInfo pathInfo)
            {
                MemberActivityBindTreeNode memberNode = new MemberActivityBindTreeNode(activity, pathInfo);
                if (memberNode.MayHaveChildNodes)
                    memberNode.Nodes.Add(new DummyActivityBindTreeNode(activity));
                return memberNode;
            }

            protected override void OnRefreshNode(WorkflowOutlineNode node)
            {
                Activity activity = node.Activity;
                if (activity == null)
                    return;

                MemberActivityBindTreeNode memberNode = node as MemberActivityBindTreeNode;
                if (memberNode != null)
                {
                    node.RefreshNode();
                    int imageNumber = (int)memberNode.MemberKind + (int)memberNode.MemberAccessKind;
                    node.ImageIndex = node.SelectedImageIndex = this.TreeView.ImageList.Images.IndexOfKey(string.Format(CultureInfo.InvariantCulture, MemberTypeFormat, imageNumber)); //it's non-localizable
                }
                else
                {
                    ActivityBindTreeNode activityNode = node as ActivityBindTreeNode;
                    //if (this.activityNodeFont == null)
                    //    this.activityNodeFont = new Font(Font, FontStyle.Bold);
                    //activityNode.NodeFont = this.activityNodeFont;

                    base.OnRefreshNode(node);
                }
            }

            protected override void OnNodeSelected(WorkflowOutlineNode node)
            {
                //notify the form that activity selection has been changed
                this.selectedActivity = (node != null) ? node.Activity : null;
                MemberActivityBindTreeNode memberNode = node as MemberActivityBindTreeNode;
                this.selectedPathInfo = (memberNode != null) ? memberNode.PathInfo : null;
                string path = PropertyPath;
                this.parent.SelectedActivityChanged(this.selectedActivity, this.selectedPathInfo, path);
            }

            public void SelectActivity(Activity activity, List<PathInfo> pathInfoList)
            {
                WorkflowOutlineNode node = base.GetNode(activity);

                //now walk all activity children along the property path list
                if (node != null)
                {
                    //expand the node to make sure dummy node is populated with the members
                    node.Expand();

                    if (pathInfoList != null && pathInfoList.Count > 0)
                    {
                        for (int i = 0; i < pathInfoList.Count; i++)
                        {
                            //there are several options here - it could be a regular property, an array or an indexer property
                            //we'd do couple attempts at trying to find the right node (taking into account the fact that indexes could be changed by the user)

                            PathInfo currentPathInfo = pathInfoList[i];
                            MemberActivityBindTreeNode matchingChildNode = null;

                            //this is an indexer, there could have been a non-indexer before
                            int indexOfOpenBracket = currentPathInfo.Path.IndexOf('[');
                            if (indexOfOpenBracket != -1)
                            {
                                string indexPropertyName = currentPathInfo.Path.Substring(0, indexOfOpenBracket);
                                if (node.Text.Equals(indexPropertyName, StringComparison.Ordinal))
                                {
                                    //need to get back to the parent and select a different child with an index
                                    //see if we need to get the properties on the parent node again...
                                    if (i > 0 && pathInfoList[i - 1].Path.Equals(indexPropertyName, StringComparison.Ordinal))
                                        node = node.Parent as WorkflowOutlineNode;
                                }
                            }

                            //find a child node with the same PathInfo member as the given one
                            foreach (TreeNode childNode in node.Nodes)
                            {
                                MemberActivityBindTreeNode memberTreeNode = childNode as MemberActivityBindTreeNode;
                                if (memberTreeNode != null && memberTreeNode.PathInfo.Equals(currentPathInfo))
                                {
                                    matchingChildNode = memberTreeNode;
                                    break;
                                }

                                //actual indexes may be different from the default ones...
                                //if it's a indexer property (this[]), indexes might mismatch
                                if (memberTreeNode != null && memberTreeNode.Text.Contains("[") && currentPathInfo.Path.Contains("["))
                                {
                                    //need to compare parameter type index collections...
                                    string currentPropertyName = GetMemberNameFromIndexerName(currentPathInfo.Path);
                                    string treeNodePropertyName = GetMemberNameFromIndexerName(memberTreeNode.Text);
                                    if (string.Equals(currentPropertyName, treeNodePropertyName, StringComparison.Ordinal) && IsSamePropertyIndexer(currentPathInfo.MemberInfo, memberTreeNode.PathInfo.MemberInfo))
                                    {
                                        matchingChildNode = memberTreeNode;
                                        memberTreeNode.PathInfo = currentPathInfo;
                                        memberTreeNode.Text = MemberActivityBindTreeNode.MemberName(currentPathInfo.Path);
                                        break;
                                    }
                                }
                            }

                            //havent found matching tree node in the list of children - will exit
                            if (matchingChildNode == null)
                                break;

                            node = matchingChildNode;
                            node.Expand();
                        }
                    }

                    base.TreeView.SelectedNode = node;

                    System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
                    timer.Tick += new EventHandler(timer_Tick);
                    timer.Interval = 50;
                    timer.Start();
                }
            }

            private string GetMemberNameFromIndexerName(string fullName)
            {
                int indexOfSeparator = fullName.IndexOf('[');
                if (indexOfSeparator != -1)
                    fullName = fullName.Substring(0, indexOfSeparator);
                return fullName;
            }

            private void timer_Tick(object sender, EventArgs e)
            {
                System.Windows.Forms.Timer timer = sender as System.Windows.Forms.Timer;
                if (timer != null)
                {
                    timer.Stop();
                    timer.Tick -= new EventHandler(timer_Tick);
                }

                if (base.TreeView.SelectedNode != null)
                    base.TreeView.SelectedNode.EnsureVisible();

                this.Focus();
            }

            private bool IsSamePropertyIndexer(MemberInfo member1, MemberInfo member2)
            {
                if (member1 == null || member2 == null)
                    return false;

                PropertyInfo property1 = member1 as PropertyInfo;
                PropertyInfo property2 = member2 as PropertyInfo;

                MethodInfo methodInfo1 = member1 as MethodInfo;
                MethodInfo methodInfo2 = member2 as MethodInfo;

                ParameterInfo[] parameters1 = (property1 != null) ? property1.GetIndexParameters() : (methodInfo1 != null) ? methodInfo1.GetParameters() : null;
                ParameterInfo[] parameters2 = (property2 != null) ? property2.GetIndexParameters() : (methodInfo2 != null) ? methodInfo2.GetParameters() : null;

                if (parameters1 == null || parameters1.Length == 0 || parameters2 == null || parameters2.Length == 0 || parameters1.Length != parameters2.Length)
                    return false;

                for (int i = 0; i < parameters1.Length; i++)
                {
                    if (parameters1[i].ParameterType != parameters2[i].ParameterType)
                        return false;
                }

                return true;
            }

            public void ExpandRootNode()
            {
                TreeNode node = base.RootNode;
                if (node != null)
                {
                    node.Collapse();
                    node.Expand();
                }
            }

            public Activity SelectedActivity
            {
                get
                {
                    return this.selectedActivity;
                }
            }
            public PathInfo SelectedMember
            {
                get
                {
                    return this.selectedPathInfo;
                }
            }
            public string PropertyPath
            {
                get
                {
                    MemberActivityBindTreeNode memberNode = base.TreeView.SelectedNode as MemberActivityBindTreeNode;
                    string path = string.Empty;
                    while (memberNode != null)
                    {
                        path = (path.Length == 0) ? memberNode.Text : memberNode.Text + "." + path;
                        memberNode = memberNode.Parent as MemberActivityBindTreeNode;
                    }
                    return path;
                }
            }
        }
        #endregion

        #region Class PathInfo
        private class PathInfo
        {
            private string path;
            private MemberInfo memberInfo;
            private Type propertyType;

            public PathInfo(string path, MemberInfo memberInfo, Type propertyType)
            {
                if (string.IsNullOrEmpty(path))
                    throw new ArgumentNullException("path");
                if (propertyType == null)
                    throw new ArgumentNullException("propertyType");
                if (memberInfo == null)
                    throw new ArgumentNullException("memberInfo");

                this.path = path;
                this.propertyType = propertyType;
                this.memberInfo = memberInfo;
            }

            public string Path
            {
                get { return this.path; }
            }

            public MemberInfo MemberInfo
            {
                get { return this.memberInfo; }
            }

            public Type PropertyType
            {
                get { return this.propertyType; }
            }

            //show bind's path information only (no activity id here)
            public override string ToString()
            {
                return this.path;
            }

            public override bool Equals(object obj)
            {
                PathInfo otherInfo = obj as PathInfo;
                if (otherInfo == null)
                    return false;

                return this.path.Equals(otherInfo.path, StringComparison.Ordinal);
            }

            public override int GetHashCode()
            {
                return this.path.GetHashCode();
            }
        }
        #endregion
    }
}
