namespace System.Workflow.ComponentModel
{
    #region Using directives

    using System;
    using System.IO;
    using System.Xml;
    using System.Text;
    using System.CodeDom;
    using System.Reflection;
    using System.Xml.XPath;
    using System.Collections;
    using System.Xml.Serialization;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Collections.Specialized;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Diagnostics;
    using System.Globalization;
    using System.Drawing.Design;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Serialization;
    using System.Workflow.ComponentModel.Design;
    using System.Configuration;
    #endregion

    #region Bind
    [Browsable(false)]
    internal abstract class BindBase
    {
        [NonSerialized]
        protected bool designMode = true;
        [NonSerialized]
        private object syncRoot = new object();

        public abstract object GetRuntimeValue(Activity activity);
        public abstract object GetRuntimeValue(Activity activity, Type targetType);
        public abstract void SetRuntimeValue(Activity activity, object value);

        protected virtual void OnRuntimeInitialized(Activity activity)
        {
        }
    }

    #endregion

    #region Redundant Binds

    #region MemberBind
    // 

    internal abstract class MemberBind : BindBase
    {
        private string name = string.Empty;

        protected MemberBind()
        {
        }

        protected MemberBind(string name)
        {
            this.name = name;
        }

        [DefaultValue("")]
        public string Name
        {
            get
            {
                return this.name;
            }
        }

        internal static object GetValue(MemberInfo memberInfo, object dataContext, string path)
        {
            if (memberInfo == null)
                throw new ArgumentNullException("memberInfo");
            if (dataContext == null)
                throw new ArgumentNullException("dataContext");
            if (path == null)
                path = string.Empty;

            if (string.IsNullOrEmpty(path))
                return null;

            object targetObject = dataContext;
            System.Type memberType = dataContext.GetType();

            PathWalker pathWalker = new PathWalker();
            pathWalker.MemberFound += delegate(object sender, PathMemberInfoEventArgs eventArgs)
            {
                if (targetObject == null)
                {
                    eventArgs.Action = PathWalkAction.Cancel;
                    return;
                }
                switch (eventArgs.MemberKind)
                {
                    case PathMemberKind.Field:
                        memberType = (eventArgs.MemberInfo as FieldInfo).FieldType;
                        targetObject = (eventArgs.MemberInfo as FieldInfo).GetValue(targetObject);
                        break;

                    case PathMemberKind.Event:
                        EventInfo evt = eventArgs.MemberInfo as EventInfo;
                        memberType = evt.EventHandlerType;

                        // GetValue() returns the actual value of the property.  We need the Bind object here.
                        // Find out if there is a matching dependency property and get the value throw the DP.
                        DependencyObject dependencyObject = targetObject as DependencyObject;
                        DependencyProperty dependencyProperty = DependencyProperty.FromName(evt.Name, dependencyObject.GetType());
                        if (dependencyProperty != null && dependencyObject != null)
                        {
                            if (dependencyObject.IsBindingSet(dependencyProperty))
                                targetObject = dependencyObject.GetBinding(dependencyProperty);
                            else
                                targetObject = dependencyObject.GetHandler(dependencyProperty);
                        }
                        else
                            targetObject = null;

                        //
                        eventArgs.Action = PathWalkAction.Stop;
                        break;

                    case PathMemberKind.Property:
                        memberType = (eventArgs.MemberInfo as PropertyInfo).PropertyType;
                        if (!(eventArgs.MemberInfo as PropertyInfo).CanRead)
                        {
                            eventArgs.Action = PathWalkAction.Cancel;
                            return;
                        }

                        targetObject = (eventArgs.MemberInfo as PropertyInfo).GetValue(targetObject, null);
                        break;

                    case PathMemberKind.IndexedProperty:
                        memberType = (eventArgs.MemberInfo as PropertyInfo).PropertyType;
                        if (!(eventArgs.MemberInfo as PropertyInfo).CanRead)
                        {
                            eventArgs.Action = PathWalkAction.Cancel;
                            return;
                        }

                        targetObject = (eventArgs.MemberInfo as PropertyInfo).GetValue(targetObject, eventArgs.IndexParameters);
                        break;

                    case PathMemberKind.Index://
                        memberType = (eventArgs.MemberInfo as PropertyInfo).PropertyType;
                        targetObject = (eventArgs.MemberInfo as PropertyInfo).GetValue(targetObject, BindingFlags.GetProperty, null, eventArgs.IndexParameters, CultureInfo.InvariantCulture);
                        break;
                }
                if (targetObject == null)
                {
                    if (eventArgs.LastMemberInThePath)
                    {
                        eventArgs.Action = PathWalkAction.Cancel;
                        return;
                    }
                    else
                    {
                        throw new InvalidOperationException(SR.GetString(SR.Error_BindPathNullValue, eventArgs.Path));
                    }
                }
            };

            if (pathWalker.TryWalkPropertyPath(memberType, path))
            {
                //success
                return ((targetObject != dataContext) ? targetObject : null);
            }
            else
            {
                //failure
                return null;
            }
        }

        internal static void SetValue(object dataContext, string path, object value)
        {
            if (dataContext == null)
                throw new ArgumentNullException("dataContext");
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            object parentObj = null;
            object obj = dataContext;

            object[] args = null;
            MemberInfo memberInfo = null;

            PathWalker pathWalker = new PathWalker();
            pathWalker.MemberFound += delegate(object sender, PathMemberInfoEventArgs eventArgs)
            {
                //
                if (obj == null)
                {
                    eventArgs.Action = PathWalkAction.Cancel;
                    return;
                }

                parentObj = obj;
                memberInfo = eventArgs.MemberInfo;

                switch (eventArgs.MemberKind)
                {
                    case PathMemberKind.Field:
                        obj = (eventArgs.MemberInfo as FieldInfo).GetValue(parentObj);
                        args = null;
                        break;

                    case PathMemberKind.Event:
                        //
                        eventArgs.Action = PathWalkAction.Cancel; //set value is not supported on events
                        return;

                    case PathMemberKind.Property:
                        obj = (eventArgs.MemberInfo as PropertyInfo).GetValue(parentObj, null);
                        args = null;
                        break;

                    case PathMemberKind.IndexedProperty:
                    case PathMemberKind.Index:
                        obj = (eventArgs.MemberInfo as PropertyInfo).GetValue(parentObj, eventArgs.IndexParameters);
                        args = eventArgs.IndexParameters;
                        break;
                }
            };

            if (pathWalker.TryWalkPropertyPath(dataContext.GetType(), path))
            {
                //at this point the 'obj' holds the old value, we will be changing it to 'value'
                //success
                if (memberInfo is FieldInfo)
                {
                    (memberInfo as FieldInfo).SetValue(parentObj, value);
                }
                else if (memberInfo is PropertyInfo)
                {
                    if ((memberInfo as PropertyInfo).CanWrite)
                        (memberInfo as PropertyInfo).SetValue(parentObj, value, args);
                    else
                        throw new InvalidOperationException(SR.GetString(SR.Error_ReadOnlyField, memberInfo.Name));
                }
            }
        }

        internal static ValidationError ValidateTypesInPath(Type srcType, string path)
        {
            ValidationError error = null;

            if (srcType == null)
                throw new ArgumentNullException("srcType");
            if (path == null)
                throw new ArgumentNullException("path");
            if (path.Length == 0)
                throw new ArgumentException(SR.GetString(SR.Error_EmptyPathValue), "path");

            Debug.Assert(WorkflowCompilationContext.Current != null, "Can't have checkTypes set to true without a context in scope");
            IList<AuthorizedType> authorizedTypes = WorkflowCompilationContext.Current.GetAuthorizedTypes();
            if (authorizedTypes == null)
            {
                return new ValidationError(SR.GetString(SR.Error_ConfigFileMissingOrInvalid), ErrorNumbers.Error_ConfigFileMissingOrInvalid);
            }

            Type propertyType = srcType;
            MemberInfo memberInfo = null;

            PathWalker pathWalker = new PathWalker();
            pathWalker.MemberFound += delegate(object sender, PathMemberInfoEventArgs eventArgs)
            {
                Type memberType = null;
                memberInfo = eventArgs.MemberInfo;

                if (memberInfo is FieldInfo)
                    memberType = ((FieldInfo)memberInfo).FieldType;

                if (memberInfo is PropertyInfo)
                    memberType = ((PropertyInfo)memberInfo).PropertyType;

                if (memberType != null && !SafeType(authorizedTypes, memberType))
                {
                    error = new ValidationError(SR.GetString(SR.Error_TypeNotAuthorized, memberType), ErrorNumbers.Error_TypeNotAuthorized);
                    eventArgs.Action = PathWalkAction.Stop;
                    return;
                }
            };
            pathWalker.TryWalkPropertyPath(propertyType, path);
            return error;
        }

        private static bool SafeType(IList<AuthorizedType> authorizedTypes, Type referenceType)
        {
            bool authorized = false;
            foreach (AuthorizedType authorizedType in authorizedTypes)
            {
                if (authorizedType.RegularExpression.IsMatch(referenceType.AssemblyQualifiedName))
                {
                    authorized = (String.Compare(bool.TrueString, authorizedType.Authorized, StringComparison.OrdinalIgnoreCase) == 0);
                    if (!authorized)
                        return false;
                }
            }
            return authorized;
        }


        internal static MemberInfo GetMemberInfo(Type srcType, string path)
        {
            if (srcType == null)
                throw new ArgumentNullException("srcType");
            if (path == null)
                throw new ArgumentNullException("path");
            if (path.Length == 0)
                throw new ArgumentException(SR.GetString(SR.Error_EmptyPathValue), "path");

            Type propertyType = srcType;
            MemberInfo memberInfo = null;

            PathWalker pathWalker = new PathWalker();
            pathWalker.MemberFound += delegate(object sender, PathMemberInfoEventArgs eventArgs)
            {
                memberInfo = eventArgs.MemberInfo;
                if (eventArgs.MemberKind == PathMemberKind.Event)
                {
                    //need to exit!!!
                    eventArgs.Action = PathWalkAction.Stop;
                    return;
                }
            };

            if (pathWalker.TryWalkPropertyPath(propertyType, path))
                return memberInfo;
            else
                return null;
        }
    }
    #endregion

    #region FieldBind
    [ActivityValidator(typeof(FieldBindValidator))]
    internal sealed class FieldBind : MemberBind
    {
        private string path = string.Empty;

        public FieldBind()
        {
        }

        public FieldBind(string name)
            : base(name)
        {
        }

        public FieldBind(string name, string path)
            : base(name)
        {
            this.path = path;
        }

        public string Path
        {
            get
            {
                return this.path;
            }
            set
            {
                if (!this.designMode)
                    throw new InvalidOperationException(SR.GetString(SR.Error_CanNotChangeAtRuntime));

                this.path = value;
            }
        }

        public override object GetRuntimeValue(Activity activity, Type targetType)
        {
            throw new NotImplementedException();
        }

        public override object GetRuntimeValue(Activity activity)
        {
            throw new NotImplementedException();
        }

        public override void SetRuntimeValue(Activity activity, object value)
        {
            throw new NotImplementedException();
        }


        protected override void OnRuntimeInitialized(Activity activity)
        {
            throw new NotImplementedException();
        }
    }

    #endregion

    #region PropertyBind
    [ActivityValidator(typeof(PropertyBindValidator))]
    internal sealed class PropertyBind : MemberBind
    {
        private string path = string.Empty;

        public PropertyBind()
        {
        }

        public PropertyBind(string name)
            : base(name)
        {
        }

        public PropertyBind(string name, string path)
            : base(name)
        {
            this.path = path;
        }

        public string Path
        {
            get
            {
                return this.path;
            }
            set
            {
                if (!this.designMode)
                    throw new InvalidOperationException(SR.GetString(SR.Error_CanNotChangeAtRuntime));

                this.path = value;
            }
        }

        public override object GetRuntimeValue(Activity activity, Type targetType)
        {
            throw new NotImplementedException();
        }

        public override object GetRuntimeValue(Activity activity)
        {
            throw new NotImplementedException();
        }

        public override void SetRuntimeValue(Activity activity, object value)
        {
            throw new NotImplementedException();
        }
    }

    #endregion

    #region MethodBind
    [ActivityValidator(typeof(MethodBindValidator))]
    internal sealed class MethodBind : MemberBind
    {
        public MethodBind()
        {
        }

        public MethodBind(string name)
            : base(name)
        {
        }

        public override object GetRuntimeValue(Activity activity, Type targetType)
        {
            throw new NotImplementedException();
        }

        public override object GetRuntimeValue(Activity activity)
        {
            throw new Exception(SR.GetString(SR.Error_NoTargetTypeForMethod));
        }

        public override void SetRuntimeValue(Activity activity, object value)
        {
            throw new Exception(SR.GetString(SR.Error_MethodDataSourceIsReadOnly));
        }
    }
    #endregion

    #endregion

    #region ActivityBind
    internal enum ActivityBindTypes { Field = 1, Property = 2, Method = 3 };

    [Browsable(true)]
    [TypeConverter(typeof(ActivityBindTypeConverter))]
    [ActivityValidator(typeof(ActivityBindValidator))]
    [DesignerSerializer(typeof(BindMarkupExtensionSerializer), typeof(WorkflowMarkupSerializer))]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class ActivityBind : MarkupExtension, IPropertyValueProvider
    {
        #region stuff from the former Bind
        [NonSerialized]
        private bool designMode = true;
        [NonSerialized]
        private bool dynamicUpdateMode = false;
        [NonSerialized]
        private IDictionary userData = null;
        [NonSerialized]
        private object syncRoot = new object();

        internal void SetContext(Activity activity)
        {
            this.designMode = false;
            OnRuntimeInitialized(activity);
        }

        internal bool DynamicUpdateMode
        {
            get
            {
                return this.dynamicUpdateMode;
            }
            set
            {
                this.dynamicUpdateMode = false;
            }
        }


        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private bool DesignMode
        {
            get
            {
                return this.designMode && !this.dynamicUpdateMode;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IDictionary UserData
        {
            get
            {
                if (this.userData == null)
                {
                    lock (this.syncRoot)
                    {
                        if (this.userData == null)
                            this.userData = Hashtable.Synchronized(new Hashtable());
                    }
                }
                return this.userData;
            }
        }

        internal static object GetDataSourceObject(Activity activity, string inputName, out string name)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");
            if (string.IsNullOrEmpty(inputName))
                throw new ArgumentNullException("inputName");

            Activity contextActivity = Helpers.GetDataSourceActivity(activity, inputName, out name);
            return contextActivity;
        }
        #endregion

        private string id = string.Empty;
        private string path = string.Empty;

        public ActivityBind()
        {
        }

        public ActivityBind(string name)
        {
            this.id = name;
        }

        public ActivityBind(string name, string path)
        {
            this.id = name;
            this.path = path;
        }

        [DefaultValue("")]
        [SRDescription(SR.ActivityBindIDDescription)]
        [ConstructorArgument("name")]
        public string Name
        {
            get
            {
                return this.id;
            }
            set
            {
                if (!this.DesignMode)
                    throw new InvalidOperationException(SR.GetString(SR.Error_CanNotChangeAtRuntime));

                this.id = value;
            }
        }

        [DefaultValue("")]
        [SRDescription(SR.ActivityBindPathDescription)]
        [TypeConverter(typeof(ActivityBindPathTypeConverter))]
        public string Path
        {
            get
            {
                return this.path;
            }
            set
            {
                if (!this.DesignMode)
                    throw new InvalidOperationException(SR.GetString(SR.Error_CanNotChangeAtRuntime));

                this.path = value;
            }
        }

        public override object ProvideValue(IServiceProvider provider)
        {
            return this;
        }

        public object GetRuntimeValue(Activity activity, Type targetType)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");
            if (targetType == null)
                throw new ArgumentNullException("targetType");
            return this.InternalGetRuntimeValue(activity, targetType);
        }

        public object GetRuntimeValue(Activity activity)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");
            return this.InternalGetRuntimeValue(activity, null);
        }

        private object InternalGetRuntimeValue(Activity activity, Type targetType)
        {
            object runtimeValue = null;
            Activity referencedActivity = Helpers.ParseActivityForBind(activity, this.Name);
            if (referencedActivity != null)
            {
                //Now lets get the MemberInfo
                MemberInfo memberInfo = ActivityBind.GetMemberInfo(referencedActivity.GetType(), Path, targetType);
                if (memberInfo != null)
                {
                    runtimeValue = ActivityBind.GetMemberValue(referencedActivity, memberInfo, Path, targetType);
                    if (runtimeValue is ActivityBind && BindHelpers.GetMemberType(memberInfo) != typeof(ActivityBind))
                        runtimeValue = ((ActivityBind)runtimeValue).GetRuntimeValue(referencedActivity, targetType);
                }
                else
                {
                    // The value of this ActivityBind is bound to properties or events defined on the referenced activity
                    // Note that we don't have corresponding logic for SetRuntimeValue because value should be only set
                    // at the end of the activity reference chain.
                    Activity rootActivity = Helpers.GetRootActivity(activity);
                    DependencyProperty dependencyProperty = DependencyProperty.FromName(this.Path, rootActivity.GetType());
                }
            }
            return runtimeValue;
        }

        public void SetRuntimeValue(Activity activity, object value)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");

            Activity referencedActivity = Helpers.ParseActivityForBind(activity, this.Name);
            if (referencedActivity != null)
            {
                MemberInfo memberInfo = ActivityBind.GetMemberInfo(referencedActivity.GetType(), Path, null);
                if (memberInfo != null)
                {
                    ActivityBind bind = ActivityBind.GetMemberValue(referencedActivity, memberInfo, Path, null) as ActivityBind;
                    if (bind != null)
                        bind.SetRuntimeValue(referencedActivity, value);
                    else
                        MemberBind.SetValue(referencedActivity, this.Path, value);
                }
                // Dependency property
                /*else
                {
                    Activity rootActivity = Helpers.GetRootActivity(activity);
                    DependencyProperty dependencyProperty = DependencyProperty.FromName(this.Path, rootActivity.GetType());
                    if (dependencyProperty != null)
                    {
                        referencedActivity.SetValue(dependencyProperty, value);
                    }
                }*/
            }
        }

        private void OnRuntimeInitialized(Activity activity)
        {
            Activity dataSourceActivity = null;
            ActivityBind activityBind = ActivityBind.GetContextBind(this, activity, out dataSourceActivity);
            if (activityBind != null && dataSourceActivity != null)
            {
                Type companionType = dataSourceActivity.GetType();
                if (companionType != null)
                {
                    MemberInfo memberInfo = ActivityBind.GetMemberInfo(companionType, activityBind.Path, null);
                    if (memberInfo != null)
                    {
                        if (memberInfo is FieldInfo || memberInfo is PropertyInfo || memberInfo is EventInfo)
                        {
                            if (activityBind.UserData[UserDataKeys.BindDataSource] == null)
                                activityBind.UserData[UserDataKeys.BindDataSource] = new Hashtable();

                            ((Hashtable)activityBind.UserData[UserDataKeys.BindDataSource])[activity.QualifiedName] = memberInfo;
                            if (dataSourceActivity != null)
                            {
                                if (activityBind.UserData[UserDataKeys.BindDataContextActivity] == null)
                                    activityBind.UserData[UserDataKeys.BindDataContextActivity] = new Hashtable();
                                ((Hashtable)activityBind.UserData[UserDataKeys.BindDataContextActivity])[activity.QualifiedName] = dataSourceActivity.QualifiedName;
                            }
                        }
                    }
                    /*else
                    {
                        Activity rootActivity = Helpers.GetRootActivity(activity);
                        DependencyProperty dependencyProperty = DependencyProperty.FromName(activityBind.Path, rootActivity.GetType());
                        if (dependencyProperty != null)
                        {
                            if (activityBind.UserData[UserDataKeys.BindDataSource] == null)
                                activityBind.UserData[UserDataKeys.BindDataSource] = new Hashtable();

                            ((Hashtable)activityBind.UserData[UserDataKeys.BindDataSource])[activity.QualifiedName] = dependencyProperty;

                            if (dataSourceActivity != null)
                            {
                                if (activityBind.UserData[UserDataKeys.BindDataContextActivity] == null)
                                    activityBind.UserData[UserDataKeys.BindDataContextActivity] = new Hashtable();
                                ((Hashtable)activityBind.UserData[UserDataKeys.BindDataContextActivity])[activity.QualifiedName] = dataSourceActivity.QualifiedName;
                            }
                        }
                    }*/
                }
            }
        }

        public override string ToString()
        {
            Activity activity = UserData[UserDataKeys.BindDataContextActivity] as Activity;
            if (activity != null)
            {
                string bindString = String.Empty;
                if (!string.IsNullOrEmpty(Name))
                    bindString = Helpers.ParseActivityForBind(activity, Name).QualifiedName;

                if (!string.IsNullOrEmpty(Path))
                {
                    string path = Path;
                    int indexOfSeparator = path.IndexOfAny(new char[] { '.', '/', '[' });
                    path = ((indexOfSeparator != -1)) ? path.Substring(0, indexOfSeparator) : path;
                    bindString += (!String.IsNullOrEmpty(bindString)) ? "." + path : path;
                }

                return bindString;
            }
            else
            {
                return base.ToString();
            }
        }

        #region Runtime / Validation Time Helpers
        internal static MemberInfo GetMemberInfo(Type dataSourceType, string path, Type targetType)
        {
            MemberInfo memberInfo = MemberBind.GetMemberInfo(dataSourceType, path);

            //The events can be either bound to properties or can be bound to methods, 
            //There are cases where fields and methods can be of same name so in that case we either make sure for
            //in the case of event handlers we either find Property or a Method
            if (targetType != null && typeof(Delegate).IsAssignableFrom(targetType) && (memberInfo == null || !(memberInfo is EventInfo)))
            {
                MethodInfo delegateMethod = targetType.GetMethod("Invoke");
                List<Type> paramTypes = new List<Type>();
                foreach (ParameterInfo paramInfo in delegateMethod.GetParameters())
                    paramTypes.Add(paramInfo.ParameterType);

                memberInfo = dataSourceType.GetMethod(path, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy, null, paramTypes.ToArray(), null);
            }

            return memberInfo;
        }

        private static object GetMemberValue(object dataSourceObject, MemberInfo memberInfo, string path, Type targetType)
        {
            object memberValue = null;
            if (memberInfo is FieldInfo || memberInfo is PropertyInfo || memberInfo is EventInfo)
            {
                memberValue = MemberBind.GetValue(memberInfo, dataSourceObject, path);

                /*if (memberValue != null && targetType != null &&
                    (memberValue.GetType().IsPrimitive || memberValue.GetType().IsEnum || memberValue.GetType() == typeof(string))
                    && !targetType.IsAssignableFrom(memberValue.GetType()))
                {
                    try
                    {
                        memberValue = Convert.ChangeType(memberValue, targetType, CultureInfo.InvariantCulture);
                    }
                    catch (Exception e)
                    {
                        throw new Exception(SR.GetString(SR.Error_DataSourceTypeConversionFailed, memberInfo.Name, memberValue.ToString(), targetType.FullName), e);
                    }
                }*/
            }
            else if (targetType != null && memberInfo is MethodInfo)
            {
                memberValue = Delegate.CreateDelegate(targetType, dataSourceObject, (MethodInfo)memberInfo); //the wrapper method will never be static (even if the original one is)
            }
            else
            {
                throw new InvalidOperationException(SR.GetString(SR.Error_MemberNotFound));
            }

            return memberValue;
        }

        //This function is used to get the outermost activity bind which is bound to actual field/property
        //The function is called in OnRuntimeInitialized and makes sure that we get outer target bind and cache
        //it so that at runtime we do not have to walk the bind chain to find the bound member
        private static ActivityBind GetContextBind(ActivityBind activityBind, Activity activity, out Activity contextActivity)
        {
            if (activityBind == null)
                throw new ArgumentNullException("activityBind");
            if (activity == null)
                throw new ArgumentNullException("activity");

            BindRecursionContext recursionContext = new BindRecursionContext();
            ActivityBind contextBind = activityBind;
            contextActivity = activity;

            while (contextBind != null)
            {
                Activity resolvedActivity = Helpers.ParseActivityForBind(contextActivity, contextBind.Name);
                if (resolvedActivity == null)
                    return null;

                object dataSourceObject = resolvedActivity;
                MemberInfo memberInfo = ActivityBind.GetMemberInfo(dataSourceObject.GetType(), contextBind.Path, null);
                if (memberInfo == null)
                {
                    contextActivity = resolvedActivity;
                    return contextBind;
                }
                else if (memberInfo is FieldInfo)
                {
                    contextActivity = resolvedActivity;
                    return contextBind;
                }
                else if (memberInfo is PropertyInfo && (memberInfo as PropertyInfo).PropertyType == typeof(ActivityBind) && dataSourceObject != null)
                {
                    object value = MemberBind.GetValue(memberInfo, dataSourceObject, contextBind.Path);
                    if (value is ActivityBind)
                    {
                        if (recursionContext.Contains(contextActivity, contextBind))
                            return null;

                        recursionContext.Add(contextActivity, contextBind);
                        contextActivity = resolvedActivity;
                        contextBind = value as ActivityBind;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            return contextBind;
        }
        #endregion

        #region DesignTime Integration (DO NOT CALL THESE AT RUNTIME)

        #region Helper Functions
        // This function replaces Activity1.code1 with /ParentContext.code1.  This must be done for Binds that refer to
        // to fields, properties and methods in the top level custom activity data context.
        internal static string GetRelativePathExpression(Activity parentActivity, Activity childActivity)
        {
            string relativeBindExpression = String.Empty;

            Activity rootActivity = Helpers.GetRootActivity(childActivity);
            if (rootActivity == childActivity)
                relativeBindExpression = "/Self";
            else
                relativeBindExpression = parentActivity.QualifiedName;

            return relativeBindExpression;
        }

        #region IPropertyValueProvider Implementation
        ICollection IPropertyValueProvider.GetPropertyValues(ITypeDescriptorContext context)
        {
            ArrayList values = new ArrayList();

            if (string.Equals(context.PropertyDescriptor.Name, "Path", StringComparison.Ordinal) && !String.IsNullOrEmpty(Name) && context.PropertyDescriptor is ActivityBindPathPropertyDescriptor)
            {
                ITypeDescriptorContext outerPropertyContext = ((ActivityBindPathPropertyDescriptor)context.PropertyDescriptor).OuterPropertyContext;
                if (outerPropertyContext != null)
                {
                    Activity activity = PropertyDescriptorUtils.GetComponent(outerPropertyContext) as Activity;
                    if (activity != null)
                    {
                        Activity targetActivity = Helpers.ParseActivityForBind(activity, Name);
                        if (targetActivity != null)
                        {
                            foreach (MemberInfo memberInfo in ActivityBindPropertyDescriptor.GetBindableMembers(targetActivity, outerPropertyContext))
                                values.Add(memberInfo.Name);
                        }
                    }
                }
            }

            return values;
        }
        #endregion

        #endregion

        #endregion
    }
    #endregion

    #region BindRecursionContext

    internal sealed class BindRecursionContext
    {
        private Hashtable activityBinds = new Hashtable();

        public bool Contains(Activity activity, ActivityBind bind)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");
            if (bind == null)
                throw new ArgumentNullException("bind");

            if (this.activityBinds[activity] != null)
            {
                List<ActivityBind> binds = this.activityBinds[activity] as List<ActivityBind>;
                foreach (ActivityBind prevBind in binds)
                {
                    if (prevBind.Path == bind.Path)
                        return true;
                }
            }
            return false;
        }

        public void Add(Activity activity, ActivityBind bind)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");
            if (bind == null)
                throw new ArgumentNullException("bind");
            if (this.activityBinds[activity] == null)
                this.activityBinds[activity] = new List<ActivityBind>();

            ((List<ActivityBind>)this.activityBinds[activity]).Add(bind);
        }
    }

    #endregion

    #region BindHelpers
    internal static class BindHelpers
    {
        internal static Type GetBaseType(IServiceProvider serviceProvider, PropertyValidationContext validationContext)
        {
            Type type = null;
            if (validationContext.Property is PropertyInfo)
            {
                type = Helpers.GetBaseType(validationContext.Property as PropertyInfo, validationContext.PropertyOwner, serviceProvider);
            }
            else if (validationContext.Property is DependencyProperty)
            {
                //
                DependencyProperty dependencyProperty = validationContext.Property as DependencyProperty;
                if (dependencyProperty != null)
                {
                    if (type == null)
                    {
                        IDynamicPropertyTypeProvider basetypeProvider = validationContext.PropertyOwner as IDynamicPropertyTypeProvider;
                        if (basetypeProvider != null)
                            type = basetypeProvider.GetPropertyType(serviceProvider, dependencyProperty.Name);
                    }

                    if (type == null)
                        type = dependencyProperty.PropertyType;
                }
            }

            return type;
        }

        internal static AccessTypes GetAccessType(IServiceProvider serviceProvider, PropertyValidationContext validationContext)
        {
            AccessTypes accessType = AccessTypes.Read;
            if (validationContext.Property is PropertyInfo)
            {
                accessType = Helpers.GetAccessType(validationContext.Property as PropertyInfo, validationContext.PropertyOwner, serviceProvider);
            }
            else if (validationContext.Property is DependencyProperty)
            {
                IDynamicPropertyTypeProvider basetypeProvider = validationContext.PropertyOwner as IDynamicPropertyTypeProvider;
                if (basetypeProvider != null)
                    accessType = basetypeProvider.GetAccessType(serviceProvider, ((DependencyProperty)validationContext.Property).Name);
            }

            return accessType;
        }

        internal static object ResolveActivityPath(Activity refActivity, string path)
        {
            if (refActivity == null)
                throw new ArgumentNullException("refActivity");
            if (path == null)
                throw new ArgumentNullException("path");
            if (path.Length == 0)
                throw new ArgumentException(SR.GetString(SR.Error_EmptyPathValue), "path");

            object value = refActivity;
            BindRecursionContext recursionContext = new BindRecursionContext();

            PathWalker pathWalker = new PathWalker();
            pathWalker.MemberFound += delegate(object sender, PathMemberInfoEventArgs eventArgs)
            {
                // If value is null, we don't want to use GetValue on the MemberInfo
                if (value == null)
                {
                    eventArgs.Action = PathWalkAction.Cancel; //need to cancel the walk with the failure return result
                    return;
                }

                switch (eventArgs.MemberKind)
                {
                    case PathMemberKind.Field:
                        try
                        {
                            value = (eventArgs.MemberInfo as FieldInfo).GetValue(value);
                        }
                        catch (Exception exception)
                        {
                            //in some cases the value might not be there yet (e.g. validation vs. runtime)
                            value = null;
                            eventArgs.Action = PathWalkAction.Cancel;

                            //we should throw only if we are at the runtime
                            if (!refActivity.DesignMode)
                            {
                                TargetInvocationException targetInvocationException = exception as TargetInvocationException;
                                throw (targetInvocationException != null) ? targetInvocationException.InnerException : exception;
                            }
                        }
                        break;

                    case PathMemberKind.Event:
                        EventInfo evt = eventArgs.MemberInfo as EventInfo;

                        // GetValue() returns the actual value of the property.  We need the Bind object here.
                        // Find out if there is a matching dependency property and get the value throw the DP.
                        DependencyProperty eventDependencyProperty = DependencyProperty.FromName(evt.Name, value.GetType());
                        if (eventDependencyProperty != null && value is DependencyObject)
                        {
                            if ((value as DependencyObject).IsBindingSet(eventDependencyProperty))
                                value = (value as DependencyObject).GetBinding(eventDependencyProperty);
                            else
                                value = (value as DependencyObject).GetHandler(eventDependencyProperty);
                        }
                        break;

                    case PathMemberKind.Property:
                        if (!(eventArgs.MemberInfo as PropertyInfo).CanRead)
                        {
                            eventArgs.Action = PathWalkAction.Cancel;
                            return;
                        }

                        // GetValue() returns the actual value of the property.  We need the Bind object here.
                        // Find out if there is a matching dependency property and get the value throw the DP.
                        DependencyProperty dependencyProperty = DependencyProperty.FromName(eventArgs.MemberInfo.Name, value.GetType());
                        if (dependencyProperty != null && value is DependencyObject && (value as DependencyObject).IsBindingSet(dependencyProperty))
                            value = (value as DependencyObject).GetBinding(dependencyProperty);
                        else
                            try
                            {
                                value = (eventArgs.MemberInfo as PropertyInfo).GetValue(value, null);
                            }
                            catch (Exception exception)
                            {
                                //property getter function might throw at design time, validation should not fail bacause of that
                                value = null;
                                eventArgs.Action = PathWalkAction.Cancel;

                                //we should throw only if we are at the runtime
                                if (!refActivity.DesignMode)
                                {
                                    TargetInvocationException targetInvocationException = exception as TargetInvocationException;
                                    throw (targetInvocationException != null) ? targetInvocationException.InnerException : exception;
                                }
                            }
                        break;

                    case PathMemberKind.IndexedProperty:
                    case PathMemberKind.Index:
                        try
                        {
                            value = (eventArgs.MemberInfo as PropertyInfo).GetValue(value, BindingFlags.GetProperty, null, eventArgs.IndexParameters, CultureInfo.InvariantCulture);
                        }
                        catch (Exception exception)
                        {
                            //in some cases the value might not be there yet - e.g. array or dictionary is populated at runtime only (validation vs. runtime)
                            value = null;
                            eventArgs.Action = PathWalkAction.Cancel;

                            //we should throw only if we are at the runtime
                            if (!refActivity.DesignMode)
                            {
                                TargetInvocationException targetInvocationException = exception as TargetInvocationException;
                                throw (targetInvocationException != null) ? targetInvocationException.InnerException : exception;
                            }
                        }
                        break;
                }

                //need to unwrap the activity bind if we get one - to proceed with the actual field/property/delegate
                //do not unwrap if the property/field is itself of type ActivityBind
                //we should not unwrap the latest ActivityBind though - only intermediate ones
                //avoid circular reference problems with the BindRecursionContext
                if (value is ActivityBind && !eventArgs.LastMemberInThePath && GetMemberType(eventArgs.MemberInfo) != typeof(ActivityBind))
                {
                    while (value is ActivityBind)
                    {
                        ActivityBind activityBind = value as ActivityBind;
                        if (recursionContext.Contains(refActivity, activityBind))
                            throw new InvalidOperationException(SR.GetString(SR.Bind_ActivityDataSourceRecursionDetected));

                        recursionContext.Add(refActivity, activityBind);
                        value = activityBind.GetRuntimeValue(refActivity);
                    }
                }
            };

            if (pathWalker.TryWalkPropertyPath(refActivity.GetType(), path))
                return value;
            else
                return null;
        }

        internal static PropertyInfo GetMatchedPropertyInfo(Type memberType, string[] aryArgName, object[] args)
        {
            if (memberType == null)
                throw new ArgumentNullException("memberType");
            if (aryArgName == null)
                throw new ArgumentNullException("aryArgName");
            if (args == null)
                throw new ArgumentNullException("args");

            MemberInfo[][] aryMembers = new MemberInfo[][] { memberType.GetDefaultMembers(), null };

            if (memberType.IsArray)
            {
                MemberInfo[] getMember = memberType.GetMember("Get"); //arrays will always implement that
                MemberInfo[] setMember = memberType.GetMember("Set"); //arrays will always implement that
                PropertyInfo getProperty = new ActivityBindPropertyInfo(memberType, getMember[0] as MethodInfo, setMember[0] as MethodInfo, string.Empty, null);
                aryMembers[1] = new MemberInfo[] { getProperty };
            }

            for (int index = 0; index < aryMembers.Length; ++index)
            {
                if (aryMembers[index] == null)
                    continue;
                MemberInfo[] defaultMembers = aryMembers[index];
                foreach (MemberInfo memberInfo in defaultMembers)
                {
                    PropertyInfo propertyInfo = memberInfo as PropertyInfo;
                    if (propertyInfo != null)
                    {
                        if (MatchIndexerParameters(propertyInfo, aryArgName, args))
                            return propertyInfo;
                    }
                }
            }
            return null;

        }

        internal static bool MatchIndexerParameters(PropertyInfo propertyInfo, string[] argNames, object[] args)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException("propertyInfo");
            if (argNames == null)
                throw new ArgumentNullException("argNames");
            if (args == null)
                throw new ArgumentNullException("args");

            ParameterInfo[] aryPI = propertyInfo.GetIndexParameters();
            if (aryPI.Length != argNames.Length)
                return false;

            for (int index = 0; index < args.Length; ++index)
            {
                Type paramType = aryPI[index].ParameterType;
                if (paramType != typeof(String) && paramType != typeof(System.Int32))
                    return false;
                try
                {
                    object arg = null;
                    string argName = argNames[index].Trim();
                    if (paramType == typeof(String) && argName.StartsWith("\"", StringComparison.Ordinal) && argName.EndsWith("\"", StringComparison.Ordinal))
                        arg = argName.Substring(1, argName.Length - 2).Trim();
                    else if (paramType == typeof(System.Int32))
                        arg = Convert.ChangeType(argName, typeof(System.Int32), CultureInfo.InvariantCulture);

                    if (arg != null)
                        args.SetValue(arg, index);
                    else
                        return false;
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        internal static Type GetMemberType(MemberInfo memberInfo)
        {
            FieldInfo fieldInfo = memberInfo as FieldInfo;
            if (fieldInfo != null)
                return fieldInfo.FieldType;

            PropertyInfo propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != null)
            {
                if (propertyInfo.PropertyType != null)
                    return propertyInfo.PropertyType;

                //sometimes need to get the property type off the getter method
                MethodInfo getter = propertyInfo.GetGetMethod();
                return getter.ReturnType;
            }

            EventInfo eventInfo = memberInfo as EventInfo;
            if (eventInfo != null)
                return eventInfo.EventHandlerType;

            return null;
        }
    }

    #endregion

    #region Class PathWalker
    internal enum PathMemberKind { Field, Event, Property, IndexedProperty, Index }
    internal enum PathWalkAction { Continue, Stop, Cancel }; //stop returns true, while cancel returns false

    internal class PathMemberInfoEventArgs : EventArgs
    {
        private string path;
        private Type parentType;
        private PathMemberKind memberKind;
        private MemberInfo memberInfo;
        private object[] indexParameters = new object[0]; //not empty for IndexedProperty and Index types
        private PathWalkAction action = PathWalkAction.Continue;
        private bool lastMemberInThePath = false;

        public PathMemberInfoEventArgs(string path, Type parentType, MemberInfo memberInfo, PathMemberKind memberKind, bool lastMemberInThePath)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");
            if (parentType == null)
                throw new ArgumentNullException("parentType");
            if (memberInfo == null)
                throw new ArgumentNullException("memberInfo");

            this.path = path;
            this.parentType = parentType;
            this.memberInfo = memberInfo;
            this.memberKind = memberKind;
            this.lastMemberInThePath = lastMemberInThePath;
        }

        public PathMemberInfoEventArgs(string path, Type parentType, MemberInfo memberInfo, PathMemberKind memberKind, bool lastMemberInThePath, object[] indexParameters)
            : this(path, parentType, memberInfo, memberKind, lastMemberInThePath)
        {
            this.indexParameters = indexParameters;
        }

        public string Path
        {
            get { return this.path; }
        }
        //public Type ParentType
        //{
        //    get { return this.parentType; }
        //}
        public MemberInfo MemberInfo
        {
            get { return this.memberInfo; }
        }
        public PathMemberKind MemberKind
        {
            get { return this.memberKind; }
        }
        public object[] IndexParameters
        {
            get { return this.indexParameters; }
        }
        public bool LastMemberInThePath
        {
            get { return this.lastMemberInThePath; }
        }
        public PathWalkAction Action
        {
            get { return this.action; }
            set { this.action = value; }
        }
    }

    internal class PathErrorInfoEventArgs : EventArgs
    {
        private SourceValueInfo info;
        private string currentPath;

        public PathErrorInfoEventArgs(SourceValueInfo info, string currentPath)
        {
            if (currentPath == null)
                throw new ArgumentNullException("currentPath");

            this.info = info;
            this.currentPath = currentPath;
        }

        //public SourceValueInfo Info
        //{
        //    get { return this.info; }
        //}
        //public string CurrentPath
        //{
        //    get { return this.currentPath; }
        //}
    }

    //common path walker
    //it is based off the property types and the PathParser results
    //caller might keep a ref to the actual object and call Get/Set value on the members returned in the PathMemberInfoEventArgs
    internal class PathWalker
    {
        public EventHandler<PathMemberInfoEventArgs> MemberFound; //on every member along the path
        public EventHandler<PathErrorInfoEventArgs> PathErrorFound; //if there was an error parsing or walking the path

        private static MemberInfo[] PopulateMembers(Type type, string memberName)
        {
            List<MemberInfo> members = new List<MemberInfo>();
            members.AddRange(type.GetMember(memberName, MemberTypes.Field | MemberTypes.Property | MemberTypes.Event | MemberTypes.Method, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy));

            if (type.IsInterface)
            {
                Type[] interfaces = type.GetInterfaces();
                foreach (Type implementedInterface in interfaces)
                {
                    members.AddRange(implementedInterface.GetMember(memberName, MemberTypes.Field | MemberTypes.Property | MemberTypes.Event | MemberTypes.Method, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy));
                }
            }

            return members.ToArray();
        }

        public bool TryWalkPropertyPath(Type rootType, string path)
        {
            if (rootType == null)
                throw new ArgumentNullException("rootType");
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            Type propertyType = rootType;
            string currentPath = string.Empty;

            PathParser parser = new PathParser();
            List<SourceValueInfo> pathInfo = parser.Parse(path, true);
            string parsingError = parser.Error;

            for (int i = 0; i < pathInfo.Count; i++)
            {
                SourceValueInfo info = pathInfo[i];

                if (string.IsNullOrEmpty(info.name))
                {
                    if (PathErrorFound != null)
                        PathErrorFound(this, new PathErrorInfoEventArgs(info, currentPath));

                    return false;
                }

                string additionalPath = (info.type == SourceValueType.Property) ? info.name : "[" + info.name + "]";
                string newPath = (string.IsNullOrEmpty(currentPath)) ? additionalPath : currentPath + ((info.type == SourceValueType.Property) ? "." : string.Empty) + additionalPath;

                Type newPropertyType = null;
                MemberInfo newMemberInfo = null;

                switch (info.type)
                {
                    case SourceValueType.Property:
                        MemberInfo[] members = PopulateMembers(propertyType, info.name);
                        if (members == null || members.Length == 0 || members[0] == null)
                        {
                            if (PathErrorFound != null)
                                PathErrorFound(this, new PathErrorInfoEventArgs(info, currentPath));

                            return false;
                        }

                        newMemberInfo = members[0];
                        if (newMemberInfo is EventInfo || newMemberInfo is MethodInfo)
                        {
                            if (MemberFound != null)
                            {
                                PathMemberInfoEventArgs args = new PathMemberInfoEventArgs(newPath, propertyType, newMemberInfo, PathMemberKind.Event, i == pathInfo.Count - 1);
                                MemberFound(this, args);

                                if (args.Action == PathWalkAction.Cancel)
                                    return false;
                                else if (args.Action == PathWalkAction.Stop)
                                    return true;
                            }

                            //
                            return string.IsNullOrEmpty(parsingError);
                        }
                        else if (newMemberInfo is PropertyInfo)
                        {
                            //property getter could be an indexer
                            PropertyInfo memberPropertyInfo = newMemberInfo as PropertyInfo;
                            MethodInfo getterMethod = memberPropertyInfo.GetGetMethod();
                            MethodInfo setterMethod = memberPropertyInfo.GetSetMethod();
                            ActivityBindPropertyInfo properyInfo = new ActivityBindPropertyInfo(propertyType, getterMethod, setterMethod, memberPropertyInfo.Name, memberPropertyInfo);

                            newPropertyType = properyInfo.PropertyType;
                            ParameterInfo[] parameters = properyInfo.GetIndexParameters();
                            if (parameters.Length > 0)
                            {
                                //need to check that the next parsed element is an indexer
                                if (i < pathInfo.Count - 1 && pathInfo[i + 1].type == SourceValueType.Indexer && !string.IsNullOrEmpty(pathInfo[i + 1].name))
                                {
                                    string[] arrayArgName = pathInfo[i + 1].name.Split(',');
                                    object[] arguments = new object[arrayArgName.Length];

                                    //match the number/type of parameters from the following indexer item
                                    if (BindHelpers.MatchIndexerParameters(properyInfo, arrayArgName, arguments))
                                    {
                                        newPath += "[" + pathInfo[i + 1].name + "]";

                                        //indexer property, uses two of the parsed array elements
                                        if (MemberFound != null)
                                        {
                                            PathMemberInfoEventArgs args = new PathMemberInfoEventArgs(newPath, propertyType, properyInfo, PathMemberKind.IndexedProperty, i == pathInfo.Count - 2, arguments);
                                            MemberFound(this, args);

                                            if (args.Action == PathWalkAction.Cancel)
                                                return false;
                                            else if (args.Action == PathWalkAction.Stop)
                                                return true;
                                        }

                                        //skip the next indexer item too
                                        i++;
                                    }
                                    else
                                    {
                                        //parameters didn't match
                                        if (PathErrorFound != null)
                                            PathErrorFound(this, new PathErrorInfoEventArgs(info, currentPath));

                                        return false;
                                    }
                                }
                                else
                                {
                                    //trailing index is missing or empty for the indexed property
                                    if (PathErrorFound != null)
                                        PathErrorFound(this, new PathErrorInfoEventArgs(info, currentPath));

                                    return false;
                                }
                            }
                            else // parameters.Length == 0
                            {
                                //a regular property
                                if (MemberFound != null)
                                {
                                    PathMemberInfoEventArgs args = new PathMemberInfoEventArgs(newPath, propertyType, properyInfo, PathMemberKind.Property, i == pathInfo.Count - 1);
                                    MemberFound(this, args);

                                    if (args.Action == PathWalkAction.Cancel)
                                        return false;
                                    else if (args.Action == PathWalkAction.Stop)
                                        return true;
                                }
                            }
                        }
                        else
                        {
                            //that would be a field
                            if (MemberFound != null)
                            {
                                PathMemberInfoEventArgs args = new PathMemberInfoEventArgs(newPath, propertyType, newMemberInfo, PathMemberKind.Field, i == pathInfo.Count - 1);
                                MemberFound(this, args);

                                if (args.Action == PathWalkAction.Cancel)
                                    return false;
                                else if (args.Action == PathWalkAction.Stop)
                                    return true;
                            }

                            newPropertyType = (newMemberInfo as FieldInfo).FieldType;
                        }

                        break;

                    case SourceValueType.Indexer:
                        if (!string.IsNullOrEmpty(info.name))
                        {
                            string[] arrayArgName = info.name.Split(',');
                            object[] arguments = new object[arrayArgName.Length];

                            PropertyInfo arrayPropertyInfo = BindHelpers.GetMatchedPropertyInfo(propertyType, arrayArgName, arguments);
                            if (arrayPropertyInfo != null)
                            {
                                if (MemberFound != null)
                                {
                                    PathMemberInfoEventArgs args = new PathMemberInfoEventArgs(newPath, propertyType, arrayPropertyInfo, PathMemberKind.Index, i == pathInfo.Count - 1, arguments);
                                    MemberFound(this, args);

                                    if (args.Action == PathWalkAction.Cancel)
                                        return false;
                                    else if (args.Action == PathWalkAction.Stop)
                                        return true;
                                }

                                newPropertyType = arrayPropertyInfo.PropertyType;
                                if (newPropertyType == null)
                                    newPropertyType = arrayPropertyInfo.GetGetMethod().ReturnType;
                            }
                            else
                            {
                                //did not mach number/type of arguments
                                if (PathErrorFound != null)
                                    PathErrorFound(this, new PathErrorInfoEventArgs(info, currentPath));

                                return false;
                            }
                        }
                        else
                        {
                            //empty indexer
                            if (PathErrorFound != null)
                                PathErrorFound(this, new PathErrorInfoEventArgs(info, currentPath));

                            return false;
                        }

                        break;
                }

                propertyType = newPropertyType;
                currentPath = newPath;
            }

            return string.IsNullOrEmpty(parsingError);
        }
    }

    //multi-dimentional arrays dont implement default property
    //this fake property will help us to handle that
    internal class ActivityBindPropertyInfo : PropertyInfo
    {
        private MethodInfo getMethod;
        private MethodInfo setMethod;
        private Type declaringType;
        private string propertyName;
        private PropertyInfo originalPropertyInfo; //in vb fields get returned as properties

        public ActivityBindPropertyInfo(Type declaringType, MethodInfo getMethod, MethodInfo setMethod, string propertyName, PropertyInfo originalPropertyInfo)
        {
            if (declaringType == null)
                throw new ArgumentNullException("declaringType");
            if (propertyName == null)
                throw new ArgumentNullException("propertyName");

            this.declaringType = declaringType;
            this.getMethod = getMethod;
            this.setMethod = setMethod;
            this.propertyName = propertyName;
            this.originalPropertyInfo = originalPropertyInfo; //could be null for array indexers.
        }

        public override string Name
        {
            get { return this.propertyName; }
        }

        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            return this.getMethod;
        }

        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            return this.setMethod;
        }

        public override Type PropertyType
        {
            get
            {
                if (this.getMethod != null)
                    return this.getMethod.ReturnType;
                else if (this.originalPropertyInfo != null)
                    return this.originalPropertyInfo.PropertyType;
                else
                    return typeof(object);
            }
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            if (this.getMethod != null)
                return this.getMethod.GetParameters();
            else if (this.originalPropertyInfo != null)
                return this.originalPropertyInfo.GetIndexParameters();
            else
                return new ParameterInfo[0];
        }

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            if (this.getMethod == null && (this.originalPropertyInfo == null || !this.originalPropertyInfo.CanRead))
                throw new InvalidOperationException(SR.GetString(SR.Error_PropertyHasNoGetterDefined, this.propertyName));

            if (this.getMethod != null)
                return this.getMethod.Invoke(obj, invokeAttr, binder, index, culture);
            else
                return this.originalPropertyInfo.GetValue(obj, invokeAttr, binder, index, culture);
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            if (this.setMethod == null && (this.originalPropertyInfo == null || !this.originalPropertyInfo.CanWrite))
                throw new InvalidOperationException(SR.GetString(SR.Error_PropertyHasNoSetterDefined, this.propertyName));

            if (this.setMethod != null)
            {
                object[] parameters = new object[((index != null) ? index.Length : 0) + 1];
                parameters[((index != null) ? index.Length : 0)] = value;

                if (index != null)
                    index.CopyTo(parameters, 0);

                this.setMethod.Invoke(obj, invokeAttr, binder, parameters, culture);
            }
            else
            {
                this.originalPropertyInfo.SetValue(obj, value, invokeAttr, binder, index, culture);
            }
        }

        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            return new MethodInfo[] { this.getMethod, this.setMethod };
        }

        public override PropertyAttributes Attributes
        {
            get { return PropertyAttributes.None; }
        }
        public override bool CanRead
        {
            get
            {
                if (this.getMethod != null)
                    return true;
                else if (this.originalPropertyInfo != null)
                    return this.originalPropertyInfo.CanRead;
                else
                    return false;
            }
        }
        public override bool CanWrite
        {
            get
            {
                if (this.setMethod != null)
                    return true;
                else if (this.originalPropertyInfo != null)
                    return this.originalPropertyInfo.CanWrite;
                else
                    return false;
            }
        }
        public override Type DeclaringType
        {
            get { return this.declaringType; }
        }
        public override Type ReflectedType
        {
            get { return this.declaringType; }
        }
        public override object[] GetCustomAttributes(bool inherit)
        {
            return new object[0];
        }
        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return new object[0];
        }
        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotSupportedException();
        }
    }
    #endregion

    #region PathParser

    internal enum SourceValueType
    {
        Property,
        Indexer
    };

    internal enum DrillIn
    {
        Never,
        IfNeeded
    };

    internal struct SourceValueInfo
    {
        internal SourceValueType type;
        internal DrillIn drillIn;
        internal string name;

        internal SourceValueInfo(SourceValueType t, DrillIn d, string n)
        {
            type = t;
            drillIn = d;
            name = n;
        }
    }

    internal sealed class PathParser
    {
        private string error = string.Empty;
        private State state;
        private string pathValue;
        private int index;
        private int pathLength;
        private DrillIn drillIn;
        private List<SourceValueInfo> al = new List<SourceValueInfo>();
        private const char NullChar = Char.MinValue;
        private static List<SourceValueInfo> EmptyInfo = new List<SourceValueInfo>(1);
        private static string SpecialChars = @".[]";

        private enum State
        {
            Init,
            Prop,
            Done
        };

        internal String Error
        {
            get
            {
                return error;
            }
        }

        internal List<SourceValueInfo> Parse(string path, bool returnResultBeforeError)
        {
            this.pathValue = (path != null) ? path.Trim() : String.Empty;
            this.pathLength = this.pathValue.Length;
            this.index = 0;
            this.drillIn = DrillIn.IfNeeded;

            this.al.Clear();
            this.error = null;
            this.state = State.Init;

            if (this.pathLength > 0 && this.pathValue[0] == '.')
            {
                //empty first prop - > input path was like ".bar"; need to add first empty property
                SourceValueInfo info = new SourceValueInfo(SourceValueType.Property, this.drillIn, string.Empty);
                this.al.Add(info);
            }

            while (this.state != State.Done)
            {
                char c = (this.index < this.pathLength) ? this.pathValue[this.index] : NullChar;
                switch (this.state)
                {
                    case State.Init:
                        switch (c)
                        {
                            case '/':
                            case '.':
                            case '[':
                            case NullChar:
                                this.state = State.Prop;
                                break;
                            case ']'://unexpected close indexer, report error
                                this.error = "path[" + this.index + "] = " + c;
                                return returnResultBeforeError ? this.al : EmptyInfo;

                            default:
                                AddProperty();
                                break;
                        }
                        break;

                    case State.Prop:
                        bool isIndexer = false;
                        switch (c)
                        {
                            case '.':
                                this.drillIn = DrillIn.Never;
                                break;
                            case '[':
                                isIndexer = true;
                                break;
                            case NullChar:
                                --this.index;
                                break;
                            default:
                                this.error = "path[" + this.index + "] = " + c;
                                return returnResultBeforeError ? this.al : EmptyInfo;
                        }
                        ++this.index;      // skip over special character
                        if (isIndexer)
                            AddIndexer();
                        else
                            AddProperty();
                        break;
                }
            }

            return (this.error == null || returnResultBeforeError) ? this.al : EmptyInfo;
        }

        private void AddProperty()
        {
            int start = this.index;
            while (this.index < this.pathLength && SpecialChars.IndexOf(this.pathValue[this.index]) < 0)
                ++this.index;

            string name = this.pathValue.Substring(start, this.index - start).Trim();
            SourceValueInfo info = new SourceValueInfo(
                                        SourceValueType.Property,
                                        this.drillIn, name);
            this.al.Add(info);
            StartNewLevel();
        }

        private void AddIndexer()
        {
            int start = this.index;
            int level = 1;
            while (level > 0)
            {
                if (this.index >= this.pathLength)
                {
                    return;
                }
                if (this.pathValue[this.index] == '[')
                    ++level;
                else if (this.pathValue[this.index] == ']')
                    --level;
                ++this.index;
            }
            string name = this.pathValue.Substring(start, this.index - start - 1).Trim();
            SourceValueInfo info = new SourceValueInfo(
                                        SourceValueType.Indexer,
                                        this.drillIn, name);
            this.al.Add(info);
            StartNewLevel();
        }

        private void StartNewLevel()
        {
            if (this.index >= this.pathLength)
                this.state = State.Done;
            this.drillIn = DrillIn.Never;
        }
    }

    #endregion
}

