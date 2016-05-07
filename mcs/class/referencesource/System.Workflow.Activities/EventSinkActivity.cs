namespace System.Workflow.Activities
{
    using System;
    using System.Reflection;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Workflow.Runtime;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.Runtime.Hosting;
    using System.Workflow.ComponentModel.Compiler;
    using System.ComponentModel.Design.Serialization;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Runtime.Serialization;
    using System.Workflow.Activities.Common;

    [SRDescription(SR.HandleExternalEventActivityDescription)]
    [DefaultEvent("Invoked")]
    [Designer(typeof(HandleExternalEventActivityDesigner), typeof(IDesigner))]
    [ActivityValidator(typeof(HandleExternalEventActivityValidator))]
    [SRCategory(SR.Base)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class HandleExternalEventActivity : Activity, IEventActivity, IPropertyValueProvider, IActivityEventListener<QueueEventArgs>, IDynamicPropertyTypeProvider
    {
        //instance properties
        public static readonly DependencyProperty CorrelationTokenProperty = DependencyProperty.Register("CorrelationToken", typeof(CorrelationToken), typeof(HandleExternalEventActivity), new PropertyMetadata(DependencyPropertyOptions.Metadata));
        public static readonly DependencyProperty RolesProperty = DependencyProperty.Register("Roles", typeof(WorkflowRoleCollection), typeof(HandleExternalEventActivity));
        public static readonly DependencyProperty ParameterBindingsProperty = DependencyProperty.Register("ParameterBindings", typeof(WorkflowParameterBindingCollection), typeof(HandleExternalEventActivity), new PropertyMetadata(DependencyPropertyOptions.Metadata | DependencyPropertyOptions.ReadOnly, new Attribute[] { new BrowsableAttribute(false), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Content) }));
        private static DependencyProperty ActivitySubscribedProperty = DependencyProperty.Register("ActivitySubscribed", typeof(bool), typeof(HandleExternalEventActivity), new PropertyMetadata(false));
        private static DependencyProperty QueueNameProperty = DependencyProperty.Register("QueueName", typeof(IComparable), typeof(HandleExternalEventActivity));

        //metadata properties
        public static readonly DependencyProperty InterfaceTypeProperty = DependencyProperty.Register("InterfaceType", typeof(System.Type), typeof(HandleExternalEventActivity), new PropertyMetadata(null, DependencyPropertyOptions.Metadata, new Attribute[] { new ValidationOptionAttribute(ValidationOption.Required) }));
        public static readonly DependencyProperty EventNameProperty = DependencyProperty.Register("EventName", typeof(string), typeof(HandleExternalEventActivity), new PropertyMetadata("", DependencyPropertyOptions.Metadata, new Attribute[] { new ValidationOptionAttribute(ValidationOption.Required) }));

        //event
        public static readonly DependencyProperty InvokedEvent = DependencyProperty.Register("Invoked", typeof(EventHandler<ExternalDataEventArgs>), typeof(HandleExternalEventActivity));

        internal static readonly ArrayList ReservedParameterNames = new ArrayList(new string[] { "Name", "Enabled", "Description", "EventName", "InterfaceType", "Invoked", "Roles" });

        #region Constructors

        public HandleExternalEventActivity()
        {
            base.SetReadOnlyPropertyValue(ParameterBindingsProperty, new WorkflowParameterBindingCollection(this));
        }

        public HandleExternalEventActivity(string name)
            : base(name)
        {
            base.SetReadOnlyPropertyValue(ParameterBindingsProperty, new WorkflowParameterBindingCollection(this));
        }

        #endregion

        [RefreshProperties(RefreshProperties.All)]
        [TypeConverter(typeof(PropertyValueProviderTypeConverter))]
        [SRCategory(SR.Activity)]
        [SRDescription(SR.ExternalEventNameDescr)]
        [MergablePropertyAttribute(false)]
        [DefaultValue("")]
        public virtual string EventName
        {
            get
            {
                return base.GetValue(EventNameProperty) as string;
            }

            set
            {
                base.SetValue(EventNameProperty, value);
            }
        }

        [SRCategory(SR.Activity)]
        [SRDescription(SR.HelperExternalDataExchangeDesc)]
        [RefreshProperties(RefreshProperties.All)]
        [Editor(typeof(TypeBrowserEditor), typeof(UITypeEditor))]
        [TypeFilterProviderAttribute(typeof(ExternalDataExchangeInterfaceTypeFilterProvider))]
        [DefaultValue(null)]
        public virtual Type InterfaceType
        {
            get
            {
                return base.GetValue(InterfaceTypeProperty) as Type;
            }

            set
            {
                base.SetValue(InterfaceTypeProperty, value);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Browsable(false)]
        public WorkflowParameterBindingCollection ParameterBindings
        {
            get
            {
                return base.GetValue(ParameterBindingsProperty) as WorkflowParameterBindingCollection;
            }
        }

        [SRCategory(SR.Activity)]
        [SRDescription(SR.RoleDescr)]
        [Editor(typeof(BindUITypeEditor), typeof(UITypeEditor))]
        [DefaultValue(null)]
        public WorkflowRoleCollection Roles
        {
            get
            {
                return base.GetValue(RolesProperty) as WorkflowRoleCollection;
            }
            set
            {
                base.SetValue(RolesProperty, value);
            }
        }

        [SRCategory(SR.Activity)]
        [RefreshProperties(RefreshProperties.All)]
        [SRDescription(SR.CorrelationSetDescr)]
        [MergableProperty(false)]
        [TypeConverter(typeof(CorrelationTokenTypeConverter))]
        [DefaultValue(null)]
        public virtual CorrelationToken CorrelationToken
        {
            get
            {
                return base.GetValue(CorrelationTokenProperty) as CorrelationToken;
            }
            set
            {
                base.SetValue(CorrelationTokenProperty, value);
            }
        }

        [SRCategory(SR.Handlers)]
        [SRDescription(SR.OnAfterMethodInvokeDescr)]
        [MergableProperty(false)]
        public event EventHandler<ExternalDataEventArgs> Invoked
        {
            add
            {
                base.AddHandler(InvokedEvent, value);
            }
            remove
            {
                base.RemoveHandler(InvokedEvent, value);
            }
        }

        private bool ActivitySubscribed
        {
            get
            {
                return (bool)base.GetValue(ActivitySubscribedProperty);
            }
            set
            {
                base.SetValue(ActivitySubscribedProperty, value);
            }
        }

        ICollection IPropertyValueProvider.GetPropertyValues(ITypeDescriptorContext context)
        {
            StringCollection names = new StringCollection();
            if (this.InterfaceType == null)
                return names;

            if (context.PropertyDescriptor.Name == "EventName")
            {
                foreach (EventInfo eventInfo in this.InterfaceType.GetEvents(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public))
                    names.Add(eventInfo.Name);
            }
            return names;
        }

        protected override void InitializeProperties()
        {
            ActivityHelpers.InitializeCorrelationTokenCollection(this, this.CorrelationToken);

            Type type = this.InterfaceType;
            if (type == null)
                throw new InvalidOperationException(SR.GetString(SR.InterfaceTypeMissing, this.Name));

            string eventName = this.EventName;
            if (eventName == null)
                throw new InvalidOperationException(SR.GetString(SR.EventNameMissing, this.Name));

            EventInfo eventInfo = type.GetEvent(eventName);
            if (eventInfo != null)
            {
                MethodInfo methodInfo = eventInfo.EventHandlerType.GetMethod("Invoke");
                InvokeHelper.InitializeParameters(methodInfo, this.ParameterBindings);
            }
            else
            {
                throw new InvalidOperationException(SR.GetString(SR.MethodInfoMissing, this.EventName, this.InterfaceType.Name));
            }

            base.InitializeProperties();
        }

        protected sealed override void Initialize(IServiceProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            //When activity is dropped inside multi instance container(replicator)
            //We delay CorrelationService initialization to template initialization time.
            if ((!this.IsDynamicActivity && !IsNestedUnderMultiInstanceContainer) || IsInitializingUnderMultiInstanceContainer)
            {
                Type type = this.InterfaceType;
                string eventName = this.EventName;
                IComparable queueName = null;
                if (CorrelationResolver.IsInitializingMember(type, eventName, null))
                    queueName = new EventQueueName(type, eventName);
                this.SetValue(QueueNameProperty, queueName);

                CorrelationService.Initialize(provider, this, type, eventName, this.WorkflowInstanceId);
            }
        }

        //Only MultiInstance container we have today is Replicator
        bool IsInitializingUnderMultiInstanceContainer
        {
            get
            {
                CompositeActivity parent = this.Parent;
                Activity templateChild = this;

                while (parent != null)
                {
                    //Need to look at attribute/interface 
                    if (parent is ReplicatorActivity)
                        break;

                    //If we cross execution context then return false.
                    if (!parent.GetActivityByName(templateChild.QualifiedName).Equals(templateChild))
                        return false;

                    templateChild = parent;
                    parent = parent.Parent;
                }

                if (parent != null)
                    return !parent.GetActivityByName(templateChild.QualifiedName).Equals(templateChild);

                return false;
            }
        }

        bool IsNestedUnderMultiInstanceContainer
        {
            get
            {
                CompositeActivity parent = this.Parent;

                while (parent != null)
                {
                    //Need to look at attribute/interface 
                    if (parent is ReplicatorActivity)
                        return true;

                    parent = parent.Parent;
                }

                return false;
            }
        }

        protected virtual void OnInvoked(EventArgs e)
        {
        }

        #region Execute/Cancel

        protected sealed override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            object[] args = null;
            ActivityExecutionStatus status = InboundActivityHelper.ExecuteForActivity(this, executionContext, this.InterfaceType, this.EventName, out args);
            if (status == ActivityExecutionStatus.Closed)
            {
                RaiseEvent(args);
                UnsubscribeForActivity(executionContext);
                executionContext.CloseActivity();
                return status;
            }

            // cannot resolve queue name or message not available
            // hence subscribe for message arrival
            if (!this.ActivitySubscribed)
            {
                this.ActivitySubscribed = CorrelationService.Subscribe(executionContext, this, this.InterfaceType, this.EventName, this, this.WorkflowInstanceId);
            }

            return ActivityExecutionStatus.Executing;
        }

        protected sealed override ActivityExecutionStatus HandleFault(ActivityExecutionContext executionContext, Exception exception)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            if (exception == null)
                throw new ArgumentNullException("exception");

            ActivityExecutionStatus newStatus = this.Cancel(executionContext);
            if (newStatus == ActivityExecutionStatus.Canceling)
                return ActivityExecutionStatus.Faulting;

            return newStatus;
        }

        protected sealed override ActivityExecutionStatus Cancel(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            UnsubscribeForActivity(executionContext);

            return ActivityExecutionStatus.Closed;
        }

        protected override void OnClosed(IServiceProvider provider)
        {
            base.RemoveProperty(HandleExternalEventActivity.ActivitySubscribedProperty);
        }

        private void UnsubscribeForActivity(ActivityExecutionContext context)
        {
            if (this.ActivitySubscribed)
            {
                CorrelationService.Unsubscribe(context, this, this.InterfaceType, this.EventName, this);
                this.ActivitySubscribed = false;
            }
        }

        void IActivityEventListener<QueueEventArgs>.OnEvent(object sender, QueueEventArgs e)
        {
            ActivityExecutionContext context = (ActivityExecutionContext)sender;
            HandleExternalEventActivity activity = context.Activity as HandleExternalEventActivity;

            // if activity is not scheduled for execution do not dequeue the message
            if (activity.ExecutionStatus != ActivityExecutionStatus.Executing) return;

            object[] args = null;
            ActivityExecutionStatus status = InboundActivityHelper.ExecuteForActivity(this, context, this.InterfaceType, this.EventName, out args);
            if (status == ActivityExecutionStatus.Closed)
            {
                RaiseEvent(args);
                UnsubscribeForActivity(context);
                context.CloseActivity();
            }
        }

        private void RaiseEvent(object[] args)
        {
            if (args != null)
            {
                ExternalDataEventArgs extArgs = args[1] as ExternalDataEventArgs;
                OnInvoked(extArgs);
                this.RaiseGenericEvent(InvokedEvent, args[0], extArgs);
            }
            else
            {
                OnInvoked(EventArgs.Empty);
                this.RaiseGenericEvent(InvokedEvent, this, EventArgs.Empty);
            }

        }
        #endregion

        #region IEventActivity members
        void IEventActivity.Subscribe(ActivityExecutionContext parentContext, IActivityEventListener<QueueEventArgs> parentEventHandler)
        {
            if (parentContext == null)
                throw new ArgumentNullException("parentContext");
            if (parentEventHandler == null)
                throw new ArgumentNullException("parentEventHandler");

            CorrelationService.Subscribe(parentContext, this, InterfaceType, EventName, parentEventHandler, this.WorkflowInstanceId);
        }

        void IEventActivity.Unsubscribe(ActivityExecutionContext parentContext, IActivityEventListener<QueueEventArgs> parentEventHandler)
        {
            if (parentContext == null)
                throw new ArgumentNullException("parentContext");
            if (parentEventHandler == null)
                throw new ArgumentNullException("parentEventHandler");

            CorrelationService.Unsubscribe(parentContext, this, InterfaceType, EventName, parentEventHandler);
        }

        IComparable IEventActivity.QueueName
        {
            get
            {
                IComparable queueName = (IComparable)this.GetValue(QueueNameProperty);
                if (queueName != null)
                    return queueName;
                else
                    return CorrelationService.ResolveQueueName(this, InterfaceType, EventName);
            }
        }

        #endregion

        #region IDynamicPropertyTypeProvider

        Type IDynamicPropertyTypeProvider.GetPropertyType(IServiceProvider serviceProvider, string propertyName)
        {
            if (propertyName == null)
                throw new ArgumentNullException("propertyName");

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            this.GetParameterPropertyDescriptors(parameters);
            if (parameters.ContainsKey(propertyName))
            {
                ParameterInfoBasedPropertyDescriptor descriptor = parameters[propertyName] as ParameterInfoBasedPropertyDescriptor;
                if (descriptor != null)
                    return descriptor.ParameterType;
            }

            return null;
        }

        AccessTypes IDynamicPropertyTypeProvider.GetAccessType(IServiceProvider serviceProvider, string propertyName)
        {
            if (propertyName == null)
                throw new ArgumentNullException("propertyName");

            return AccessTypes.Read;
        }

        internal void GetParameterPropertyDescriptors(IDictionary properties)
        {
            if (((IComponent)this).Site == null)
                return;

            ITypeProvider typeProvider = (ITypeProvider)((IComponent)this).Site.GetService(typeof(ITypeProvider));
            if (typeProvider == null)
                throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(ITypeProvider).FullName));

            Type type = this.InterfaceType;
            if (type == null)
                return;

            if (this.GetType() != typeof(HandleExternalEventActivity))
                return; // if custom activity do not add parameter binding

            EventInfo eventInfo = type.GetEvent(this.EventName);
            if (eventInfo != null)
            {
                Type delegateType = TypeProvider.GetEventHandlerType(eventInfo);
                if (delegateType != null)
                {
                    MethodInfo method = delegateType.GetMethod("Invoke");
                    ArrayList paramInfo = new ArrayList();
                    if (method != null)
                    {
                        paramInfo.AddRange(method.GetParameters());
                        if (!(method.ReturnType == typeof(void)))
                            paramInfo.Add(method.ReturnParameter);
                    }

                    foreach (ParameterInfo param in paramInfo)
                    {
                        PropertyDescriptor prop = new ParameterInfoBasedPropertyDescriptor(typeof(HandleExternalEventActivity), param, true, DesignOnlyAttribute.Yes);
                        properties[prop.Name] = prop;
                    }
                }
            }
        }
        #endregion
    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class HandleExternalEventActivityValidator : ActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            HandleExternalEventActivity eventSink = obj as HandleExternalEventActivity;
            if (eventSink == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(HandleExternalEventActivity).FullName), "obj");

            ValidationErrorCollection validationErrors = base.Validate(manager, obj);
            validationErrors.AddRange(CorrelationSetsValidator.Validate(manager, obj));
            validationErrors.AddRange(ParameterBindingValidator.Validate(manager, obj));
            return validationErrors;
        }
    }

    internal class ExternalDataExchangeInterfaceTypeFilterProvider : ITypeFilterProvider
    {
        private IServiceProvider serviceProvider;
        public ExternalDataExchangeInterfaceTypeFilterProvider(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public bool CanFilterType(Type type, bool throwOnError)
        {
            if (type.IsInterface)
            {
                object[] dsAttribs = type.GetCustomAttributes(typeof(ExternalDataExchangeAttribute), false);
                if (dsAttribs.Length != 0)
                    return true;
                else if (throwOnError)
                    throw new Exception(SR.GetString(SR.Error_InterfaceTypeNeedsExternalDataExchangeAttribute, "InterfaceType"));
            }
            /*  else
              {
                  Type[] types = type.GetNestedTypes();
                  foreach (Type nestedType in types)
                  {
                      object[] dsAttribs = nestedType.GetCustomAttributes(typeof(ExternalDataExchangeAttribute), false);
                      if (dsAttribs.Length != 0)
                          return true;
                  }
              }*/

            if (throwOnError)
                throw new Exception(SR.GetString(SR.Error_InterfaceTypeNotInterface, "InterfaceType"));

            return false;
        }

        public string FilterDescription
        {
            get
            {
                return SR.GetString(SR.ShowingExternalDataExchangeService);
            }
        }
    }
}
