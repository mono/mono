using System;
using System.Reflection;
using System.Drawing;
using System.Drawing.Design;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.Runtime;
using System.CodeDom;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Workflow.Runtime.Hosting;
using System.Runtime.Remoting.Messaging;
using System.Diagnostics.CodeAnalysis;
using System.Workflow.Activities.Common;

namespace System.Workflow.Activities
{
    [SRDescription(SR.WebServiceReceiveActivityDescription)]
    [SRCategory(SR.Standard)]
    [ToolboxBitmap(typeof(WebServiceInputActivity), "Resources.WebServiceIn.png")]
    [Designer(typeof(WebServiceReceiveDesigner), typeof(IDesigner))]
    [ActivityValidator(typeof(WebServiceReceiveValidator))]
    [ActivityCodeGeneratorAttribute(typeof(WebServiceCodeGenerator))]
    [DefaultEvent("InputReceived")]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class WebServiceInputActivity : Activity, IEventActivity, IPropertyValueProvider, IActivityEventListener<QueueEventArgs>, IDynamicPropertyTypeProvider
    {
        //metadata properties
        public static readonly DependencyProperty IsActivatingProperty = DependencyProperty.Register("IsActivating", typeof(bool), typeof(WebServiceInputActivity), new PropertyMetadata(false, DependencyPropertyOptions.Metadata));
        public static readonly DependencyProperty InterfaceTypeProperty = DependencyProperty.Register("InterfaceType", typeof(Type), typeof(WebServiceInputActivity), new PropertyMetadata(null, DependencyPropertyOptions.Metadata));
        public static readonly DependencyProperty MethodNameProperty = DependencyProperty.Register("MethodName", typeof(string), typeof(WebServiceInputActivity), new PropertyMetadata("", DependencyPropertyOptions.Metadata));
        public static readonly DependencyProperty RolesProperty = DependencyProperty.Register("Roles", typeof(WorkflowRoleCollection), typeof(WebServiceInputActivity));

        //instance properties
        public static readonly DependencyProperty ParameterBindingsProperty = DependencyProperty.Register("ParameterBindings", typeof(WorkflowParameterBindingCollection), typeof(WebServiceInputActivity), new PropertyMetadata(DependencyPropertyOptions.Metadata | DependencyPropertyOptions.ReadOnly, new Attribute[] { new BrowsableAttribute(false), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Content) }));
        public static readonly DependencyProperty ActivitySubscribedProperty = DependencyProperty.Register("ActivitySubscribed", typeof(bool), typeof(WebServiceInputActivity), new PropertyMetadata(false));
        private static readonly DependencyProperty QueueNameProperty = DependencyProperty.Register("QueueName", typeof(IComparable), typeof(WebServiceInputActivity));

        //event
        public static readonly DependencyProperty InputReceivedEvent = DependencyProperty.Register("InputReceived", typeof(EventHandler), typeof(WebServiceInputActivity));

        #region Constructors

        public WebServiceInputActivity()
        {
            //
            base.SetReadOnlyPropertyValue(ParameterBindingsProperty, new WorkflowParameterBindingCollection(this));
        }

        public WebServiceInputActivity(string name)
            : base(name)
        {
            //
            base.SetReadOnlyPropertyValue(ParameterBindingsProperty, new WorkflowParameterBindingCollection(this));
        }

        #endregion

        [SRCategory(SR.Activity)]
        [SRDescription(SR.InterfaceTypeDescription)]
        [RefreshProperties(RefreshProperties.All)]
        [Editor(typeof(TypeBrowserEditor), typeof(UITypeEditor))]
        [TypeFilterProviderAttribute(typeof(InterfaceTypeFilterProvider))]
        [DefaultValue(null)]
        public Type InterfaceType
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

        [SRCategory(SR.Activity)]
        [SRDescription(SR.WebServiceMethodDescription)]
        [TypeConverter(typeof(PropertyValueProviderTypeConverter))]
        [RefreshProperties(RefreshProperties.All)]
        [MergablePropertyAttribute(false)]
        [DefaultValue("")]
        public string MethodName
        {
            get
            {
                return (string)base.GetValue(MethodNameProperty);
            }

            set
            {
                base.SetValue(MethodNameProperty, value);
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
        [SRDescription(SR.ActivationDescr)]
        [DefaultValue(false)]
        [MergableProperty(false)]
        public bool IsActivating
        {
            get
            {
                return (bool)base.GetValue(IsActivatingProperty);
            }

            set
            {
                base.SetValue(IsActivatingProperty, value);
            }
        }

        [SRDescription(SR.OnAfterReceiveDescr)]
        [SRCategory(SR.Handlers)]
        [MergableProperty(false)]
        public event EventHandler InputReceived
        {
            add
            {
                base.AddHandler(InputReceivedEvent, value);
            }
            remove
            {
                base.RemoveHandler(InputReceivedEvent, value);
            }
        }

        internal bool ActivitySubscribed
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
            ITypeProvider typeProvider = (ITypeProvider)context.GetService(typeof(ITypeProvider));
            if (typeProvider == null)
                throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(ITypeProvider).FullName));

            StringCollection names = new StringCollection();
            if (context.PropertyDescriptor.Name == "MethodName")
            {
                if (this.InterfaceType != null)
                {
                    Type type = typeProvider.GetType(this.InterfaceType.AssemblyQualifiedName);
                    if (type != null)
                    {
                        foreach (MethodInfo method in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public))
                        {
                            if (!method.IsSpecialName)
                                names.Add(method.Name);
                        }

                        foreach (Type interfaceType in type.GetInterfaces())
                        {
                            foreach (MethodInfo method in interfaceType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public))
                            {
                                names.Add(interfaceType.FullName + "." + method.Name);
                            }
                        }
                    }
                }
            }
            return names;
        }

        protected sealed override void Initialize(IServiceProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            if (this.Parent == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_MustHaveParent));

            WorkflowQueuingService queueService = (WorkflowQueuingService)provider.GetService(typeof(WorkflowQueuingService));

            //create a static q entry 
            EventQueueName key = new EventQueueName(this.InterfaceType, this.MethodName);
            this.SetValue(QueueNameProperty, key);

            if (!queueService.Exists(key))
                queueService.CreateWorkflowQueue(key, true);
        }

        #region Execute/Cancel

        protected sealed override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            ActivityExecutionStatus status = ExecuteForActivity(executionContext, this.InterfaceType, this.MethodName);
            if (status == ActivityExecutionStatus.Closed)
            {
                UnsubscribeForActivity(executionContext);
                executionContext.CloseActivity();
                return status;
            }

            // cannot resolve queue name or message not available
            // hence subscribe for message arrival
            if (!this.ActivitySubscribed)
            {
                this.ActivitySubscribed = CorrelationService.Subscribe(executionContext, this, this.InterfaceType, this.MethodName, this, this.WorkflowInstanceId);
            }

            return ActivityExecutionStatus.Executing;
        }

        private ActivityExecutionStatus ExecuteForActivity(ActivityExecutionContext context, Type interfaceType, string operation)
        {
            WorkflowQueuingService queueSvcs = (WorkflowQueuingService)context.GetService(typeof(WorkflowQueuingService));

            IComparable queueName = new EventQueueName(interfaceType, operation);
            if (queueName != null)
            {
                WorkflowQueue queue;
                object message = InboundActivityHelper.DequeueMessage(queueName, queueSvcs, this, out queue);
                if (message != null)
                {
                    ProcessMessage(context, message, interfaceType, operation);
                    return ActivityExecutionStatus.Closed;
                }
            }

            return ActivityExecutionStatus.Executing;
        }

        private void ProcessMessage(ActivityExecutionContext context, object msg, Type interfaceType, string operation)
        {
            IMethodMessage message = msg as IMethodMessage;
            if (message == null)
            {
                Exception excp = msg as Exception;
                if (excp != null)
                    throw excp;
                throw new ArgumentNullException("msg");
            }

            CorrelationService.InvalidateCorrelationToken(this, interfaceType, operation, message.Args);

            IdentityContextData identityData =
                (IdentityContextData)message.LogicalCallContext.GetData(IdentityContextData.IdentityContext);
            InboundActivityHelper.ValidateRoles(this, identityData.Identity);

            ProcessParameters(context, message, interfaceType, operation);
            RaiseEvent(WebServiceInputActivity.InputReceivedEvent, this, EventArgs.Empty);
        }

        private void ProcessParameters(ActivityExecutionContext context, IMethodMessage message, Type interfaceType, string operation)
        {
            WorkflowParameterBindingCollection parameters = ParameterBindings;
            if (parameters == null)
                return;

            //cache mInfo todo
            MethodInfo mInfo = interfaceType.GetMethod(operation);
            if (mInfo == null)
                return;

            int index = 0;
            bool responseRequired = false;
            foreach (ParameterInfo formalParameter in mInfo.GetParameters())
            {
                // populate in params, checking on IsIn alone is not sufficient
                if (!(formalParameter.ParameterType.IsByRef || (formalParameter.IsIn && formalParameter.IsOut)))
                {
                    if (parameters.Contains(formalParameter.Name))
                    {
                        WorkflowParameterBinding binding = parameters[formalParameter.Name];
                        binding.Value = message.Args[index++];
                    }
                }
                else
                {
                    responseRequired = true;
                }
            }

            if (mInfo.ReturnType != typeof(void) || responseRequired)
            {
                // create queue entry {interface, operation and receive activity Id}
                IComparable queueId = new EventQueueName(interfaceType, operation, QualifiedName);
                // enqueue the message for sendresponse reply context
                WorkflowQueuingService queuingService = (WorkflowQueuingService)context.GetService(typeof(WorkflowQueuingService));
                if (!queuingService.Exists(queueId))
                    queuingService.CreateWorkflowQueue(queueId, true);

                queuingService.GetWorkflowQueue(queueId).Enqueue(message);
            }
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

        private void UnsubscribeForActivity(ActivityExecutionContext context)
        {
            if (this.ActivitySubscribed)
            {
                CorrelationService.Unsubscribe(context, this, this.InterfaceType, this.MethodName, this);
                this.ActivitySubscribed = false;
            }
        }

        void IActivityEventListener<QueueEventArgs>.OnEvent(object sender, QueueEventArgs e)
        {
            ActivityExecutionContext context = (ActivityExecutionContext)sender;
            WebServiceInputActivity activity = context.Activity as WebServiceInputActivity;

            // if activity is not scheduled for execution do not dequeue the message
            if (activity.ExecutionStatus != ActivityExecutionStatus.Executing) return;

            ActivityExecutionStatus status = ExecuteForActivity(context, activity.InterfaceType, activity.MethodName);
            if (status == ActivityExecutionStatus.Closed)
            {
                UnsubscribeForActivity(context);
                context.CloseActivity();
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

            CorrelationService.Subscribe(parentContext, this, this.InterfaceType, this.MethodName, parentEventHandler, this.WorkflowInstanceId);
        }

        void IEventActivity.Unsubscribe(ActivityExecutionContext parentContext, IActivityEventListener<QueueEventArgs> parentEventHandler)
        {
            if (parentContext == null)
                throw new ArgumentNullException("parentContext");
            if (parentEventHandler == null)
                throw new ArgumentNullException("parentEventHandler");

            CorrelationService.Unsubscribe(parentContext, this, this.InterfaceType, this.MethodName, parentEventHandler);
        }

        IComparable IEventActivity.QueueName
        {
            get
            {
                return (IComparable)this.GetValue(QueueNameProperty);
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

            Type type = null;
            if (this.InterfaceType != null)
                type = typeProvider.GetType(this.InterfaceType.AssemblyQualifiedName);

            if (type != null && type.IsInterface)
            {
                MethodInfo method = Helpers.GetInterfaceMethod(type, this.MethodName);
                if (method != null && WebServiceActivityHelpers.ValidateParameterTypes(method).Count == 0)
                {
                    List<ParameterInfo> inputParameters, outParameters;
                    WebServiceActivityHelpers.GetParameterInfo(method, out inputParameters, out outParameters);

                    foreach (ParameterInfo paramInfo in inputParameters)
                    {
                        PropertyDescriptor prop = new ParameterInfoBasedPropertyDescriptor(typeof(WebServiceInputActivity), paramInfo, true, DesignOnlyAttribute.Yes);
                        properties[prop.Name] = prop;
                    }
                }
            }

        }
        #endregion
    }

    internal sealed class WebServiceReceiveValidator : ActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = base.Validate(manager, obj);

            WebServiceInputActivity webServiceReceive = obj as WebServiceInputActivity;
            if (webServiceReceive == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(WebServiceInputActivity).FullName), "obj");

            if (Helpers.IsActivityLocked(webServiceReceive))
            {
                return validationErrors;
            }

            if (webServiceReceive.IsActivating)
            {
                if (WebServiceActivityHelpers.GetPreceedingActivities(webServiceReceive).GetEnumerator().MoveNext() == true)
                {
                    ValidationError error = new ValidationError(SR.GetString(SR.Error_ActivationActivityNotFirst), ErrorNumbers.Error_ActivationActivityNotFirst);
                    error.PropertyName = "IsActivating";
                    validationErrors.Add(error);
                    return validationErrors;
                }

                if (WebServiceActivityHelpers.IsInsideLoop(webServiceReceive, null))
                {
                    ValidationError error = new ValidationError(SR.GetString(SR.Error_ActivationActivityInsideLoop), ErrorNumbers.Error_ActivationActivityInsideLoop);
                    error.PropertyName = "IsActivating";
                    validationErrors.Add(error);
                    return validationErrors;
                }
            }
            else
            {
                if (WebServiceActivityHelpers.GetPreceedingActivities(webServiceReceive, true).GetEnumerator().MoveNext() == false)
                {
                    ValidationError error = new ValidationError(SR.GetString(SR.Error_WebServiceReceiveNotMarkedActivate), ErrorNumbers.Error_WebServiceReceiveNotMarkedActivate);
                    error.PropertyName = "IsActivating";
                    validationErrors.Add(error);
                    return validationErrors;
                }
            }

            ITypeProvider typeProvider = (ITypeProvider)manager.GetService(typeof(ITypeProvider));
            if (typeProvider == null)
                throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(ITypeProvider).FullName));

            Type interfaceType = null;
            if (webServiceReceive.InterfaceType != null)
                interfaceType = typeProvider.GetType(webServiceReceive.InterfaceType.AssemblyQualifiedName);

            if (interfaceType == null)
            {
                ValidationError error = new ValidationError(SR.GetString(SR.Error_TypePropertyInvalid, "InterfaceType"), ErrorNumbers.Error_PropertyNotSet);
                error.PropertyName = "InterfaceType";
                validationErrors.Add(error);
            }
            else if (!interfaceType.IsInterface)
            {
                ValidationError error = new ValidationError(SR.GetString(SR.Error_InterfaceTypeNotInterface, "InterfaceType"), ErrorNumbers.Error_InterfaceTypeNotInterface);
                error.PropertyName = "InterfaceType";
                validationErrors.Add(error);
            }
            else
            {
                // Validate method
                if (String.IsNullOrEmpty(webServiceReceive.MethodName))
                    validationErrors.Add(ValidationError.GetNotSetValidationError("MethodName"));
                else
                {
                    MethodInfo methodInfo = Helpers.GetInterfaceMethod(interfaceType, webServiceReceive.MethodName);

                    if (methodInfo == null)
                    {
                        ValidationError error = new ValidationError(SR.GetString(SR.Error_MethodNotExists, "MethodName", webServiceReceive.MethodName), ErrorNumbers.Error_MethodNotExists);
                        error.PropertyName = "MethodName";
                        validationErrors.Add(error);
                    }
                    else
                    {
                        ValidationErrorCollection parameterTypeErrors = WebServiceActivityHelpers.ValidateParameterTypes(methodInfo);
                        if (parameterTypeErrors.Count > 0)
                        {
                            foreach (ValidationError parameterTypeError in parameterTypeErrors)
                            {
                                parameterTypeError.PropertyName = "MethodName";
                            }
                            validationErrors.AddRange(parameterTypeErrors);
                        }
                        else
                        {
                            List<ParameterInfo> inputParameters, outParameters;
                            WebServiceActivityHelpers.GetParameterInfo(methodInfo, out inputParameters, out outParameters);

                            // Check to see if all input parameters have a valid binding.
                            foreach (ParameterInfo paramInfo in inputParameters)
                            {
                                string paramName = paramInfo.Name;
                                string parameterPropertyName = ParameterInfoBasedPropertyDescriptor.GetParameterPropertyName(webServiceReceive.GetType(), paramName);

                                Type paramType = paramInfo.ParameterType.IsByRef ? paramInfo.ParameterType.GetElementType() : paramInfo.ParameterType;
                                object paramValue = null;
                                if (webServiceReceive.ParameterBindings.Contains(paramName))
                                {
                                    if (webServiceReceive.ParameterBindings[paramName].IsBindingSet(WorkflowParameterBinding.ValueProperty))
                                        paramValue = webServiceReceive.ParameterBindings[paramName].GetBinding(WorkflowParameterBinding.ValueProperty);
                                    else
                                        paramValue = webServiceReceive.ParameterBindings[paramName].GetValue(WorkflowParameterBinding.ValueProperty);
                                }

                                if (!paramType.IsPublic || !paramType.IsSerializable)
                                {
                                    ValidationError validationError = new ValidationError(SR.GetString(SR.Error_TypeNotPublicSerializable, paramName, paramType.FullName), ErrorNumbers.Error_TypeNotPublicSerializable);
                                    validationError.PropertyName = parameterPropertyName;
                                    validationErrors.Add(validationError);
                                }
                                else if (!webServiceReceive.ParameterBindings.Contains(paramName) || paramValue == null)
                                {
                                    ValidationError validationError = ValidationError.GetNotSetValidationError(paramName);
                                    validationError.PropertyName = parameterPropertyName;
                                    validationErrors.Add(validationError);
                                }
                                else
                                {
                                    AccessTypes access = AccessTypes.Read;
                                    if (paramInfo.ParameterType.IsByRef)
                                        access |= AccessTypes.Write;

                                    ValidationErrorCollection variableErrors = ValidationHelpers.ValidateProperty(manager, webServiceReceive, paramValue,
                                                                                    new PropertyValidationContext(webServiceReceive.ParameterBindings[paramName], null, paramName), new BindValidationContext(paramInfo.ParameterType.IsByRef ? paramInfo.ParameterType.GetElementType() : paramInfo.ParameterType, access));
                                    foreach (ValidationError validationError in variableErrors)
                                        validationError.PropertyName = parameterPropertyName;
                                    validationErrors.AddRange(variableErrors);
                                }
                            }

                            if (webServiceReceive.ParameterBindings.Count > inputParameters.Count)
                                validationErrors.Add(new ValidationError(SR.GetString(SR.Warning_AdditionalBindingsFound), ErrorNumbers.Warning_AdditionalBindingsFound, true));

                            bool foundMatchingResponse = false;
                            foreach (Activity succeedingActivity in WebServiceActivityHelpers.GetSucceedingActivities(webServiceReceive))
                            {
                                if ((succeedingActivity is WebServiceOutputActivity && ((WebServiceOutputActivity)succeedingActivity).InputActivityName == webServiceReceive.Name) ||
                                     (succeedingActivity is WebServiceFaultActivity && ((WebServiceFaultActivity)succeedingActivity).InputActivityName == webServiceReceive.Name))
                                {
                                    foundMatchingResponse = true;
                                    break;
                                }
                            }

                            // If the method has out parameters or is the method has a return value,
                            // check to see if there are any corresponding WebServiceResponse activities.
                            if ((outParameters.Count > 0 || methodInfo.ReturnType != typeof(void)) && !foundMatchingResponse)
                            {
                                validationErrors.Add(new ValidationError(SR.GetString(SR.Error_WebServiceResponseNotFound), ErrorNumbers.Error_WebServiceResponseNotFound));
                            }
                        }
                    }
                }
            }
            return validationErrors;
        }
    }

    internal sealed class WebServiceCodeGenerator : ActivityCodeGenerator
    {
        public override void GenerateCode(CodeGenerationManager manager, object obj)
        {
            WebServiceInputActivity webserviceInput = obj as WebServiceInputActivity;

            if (manager == null)
                throw new ArgumentNullException("manager");

            if (obj == null)
                throw new ArgumentNullException("obj");

            if (webserviceInput == null)
                return;

            ITypeProvider typeProvider = manager.GetService(typeof(ITypeProvider)) as ITypeProvider;

            if (typeProvider == null)
                throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(ITypeProvider).FullName));

            CodeNamespaceCollection codeNamespaces = manager.Context[typeof(CodeNamespaceCollection)] as CodeNamespaceCollection;

            if (codeNamespaces == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_ContextStackItemMissing, typeof(CodeNamespaceCollection).Name));

            CodeTypeDeclaration webServiceClass = CreateOrGetServiceDeclaration(Helpers.GetRootActivity(webserviceInput), codeNamespaces);

            Debug.Assert(webserviceInput.InterfaceType != null, "Interface type should not be null");

            if (webserviceInput.InterfaceType != null)
            {
                bool memberExists = false;
                MethodInfo methodInfo = Helpers.GetInterfaceMethod(webserviceInput.InterfaceType, webserviceInput.MethodName);

                //Check to see if a method with the same name already exists (the same activity may be used 
                // in multiple places).
                SupportedLanguages language = CompilerHelpers.GetSupportedLanguage(manager);
                foreach (CodeTypeMember member in webServiceClass.Members)
                {
                    //
                    if (member is CodeMemberMethod && String.Compare(member.Name, methodInfo.Name, language == SupportedLanguages.CSharp ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase) == 0)
                        memberExists = true;
                }

                if (!memberExists)
                    webServiceClass.Members.Add(this.GetWebServiceMethodDeclaraion(methodInfo, webserviceInput.IsActivating, language));
            }
            base.GenerateCode(manager, obj);
            return;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", Justification = "IndexOf(\".\") not a security issue.")]
        private CodeTypeDeclaration CreateOrGetServiceDeclaration(Activity rootActivity, CodeNamespaceCollection codeNamespaceCollection)
        {
            String namespaceName = "";
            CodeNamespace webServiceCodeNamespace = null;
            string className = rootActivity.GetType().FullName;
            CodeTypeDeclaration webServiceClass = null;

            if (rootActivity.GetType().FullName.IndexOf(".") != -1)
                namespaceName = rootActivity.GetType().FullName.Substring(0, rootActivity.GetType().FullName.LastIndexOf('.'));


            foreach (CodeNamespace codeNamespace in codeNamespaceCollection)
            {
                if (codeNamespace.Name == namespaceName)
                {
                    webServiceCodeNamespace = codeNamespace;
                    break;
                }
            }

            if (webServiceCodeNamespace == null)
            {
                webServiceCodeNamespace = this.GetWebServiceCodeNamespace(namespaceName);
                codeNamespaceCollection.Add(webServiceCodeNamespace);
            }
            String webServiceTypeName = className.Substring(className.LastIndexOf('.') + 1) + "_WebService";

            foreach (CodeTypeDeclaration codeTypeDeclaration in webServiceCodeNamespace.Types)
            {
                if (codeTypeDeclaration.Name == webServiceTypeName)
                {
                    webServiceClass = codeTypeDeclaration;
                    break;
                }
            }

            if (webServiceClass == null)
            {
                webServiceClass = this.GetWebserviceCodeTypeDeclaration(className.Substring(className.LastIndexOf('.') + 1));
                webServiceCodeNamespace.Types.Add(webServiceClass);
                //Ensure namespaces are imported.
                webServiceCodeNamespace.Imports.Add(new CodeNamespaceImport("System"));
                webServiceCodeNamespace.Imports.Add(new CodeNamespaceImport("System.Web"));
                webServiceCodeNamespace.Imports.Add(new CodeNamespaceImport("System.Web.Services"));
                webServiceCodeNamespace.Imports.Add(new CodeNamespaceImport("System.Web.Services.Protocols"));
                webServiceCodeNamespace.Imports.Add(new CodeNamespaceImport("System.Workflow.Runtime.Hosting"));
                webServiceCodeNamespace.Imports.Add(new CodeNamespaceImport("System.Workflow.Activities"));
            }

            return webServiceClass;
        }


        private CodeNamespace GetWebServiceCodeNamespace(string namespaceName)
        {
            CodeNamespace nameSpace = new CodeNamespace(namespaceName);
            return nameSpace;
        }

        //[WebServiceBinding(ConformsTo=WsiClaims.BP10,EmitConformanceClaims = true)]
        //public class <WorkflowTypeName>_WebService : System.Workflow.Runtime.Hosting.WorkflowWebService
        //{
        //  public <WorkflowTypeName>_WebService():base(typeof(<WorkflowTypeName>)) {}
        //}
        private CodeTypeDeclaration GetWebserviceCodeTypeDeclaration(string workflowTypeName)
        {
            CodeTypeDeclaration typeDeclaration = new CodeTypeDeclaration(workflowTypeName + "_WebService");
            typeDeclaration.BaseTypes.Add(new CodeTypeReference("WorkflowWebService"));

            CodeAttributeDeclaration attributeDeclaration = new CodeAttributeDeclaration("WebServiceBinding", new CodeAttributeArgument("ConformsTo", new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("WsiProfiles"), "BasicProfile1_1")), new CodeAttributeArgument("EmitConformanceClaims", new CodePrimitiveExpression(true)));
            typeDeclaration.CustomAttributes.Add(attributeDeclaration);

            //Emit Constructor
            CodeConstructor constructor = new CodeConstructor();
            constructor.Attributes = MemberAttributes.Public;
            constructor.BaseConstructorArgs.Add(new CodeTypeOfExpression(workflowTypeName));
            typeDeclaration.Members.Add(constructor);

            return typeDeclaration;
        }

        //[WebMethod(Description=<OperationName>, EnableSession=true)]
        //public virtual <ReturnType> <OperationName>(<Arguments>)
        //{
        //    For No out ref case.
        //    return (<ReturnType>)base.Invoke(<InterfaceType>, <MethodName>, true | false(based on activation), arguments)[0];
        //    
        //    For out & ref case.
        //    Object[] results = base.Invoke(<InterfaceType>, <MethodName>, true | false(based on activation), arguments);
        //    refParam1 = (RPType)results[1];  
        //    refParam2 = (RPType)results[2];
        //    outParam3 = (RPType)results[3];    
        //    return (<ReturnType>)results[0];
        //}
        private CodeMemberMethod GetWebServiceMethodDeclaraion(MethodInfo methodInfo, bool isActivation, SupportedLanguages language)
        {
            CodeMemberMethod webMethod = new CodeMemberMethod();
            webMethod.Attributes = MemberAttributes.Public;
            webMethod.ReturnType = new CodeTypeReference(methodInfo.ReturnType);
            webMethod.Name = methodInfo.Name;

            CodeAttributeDeclaration attrDecl = new CodeAttributeDeclaration("WebMethodAttribute");
            attrDecl.Arguments.Add(new CodeAttributeArgument("Description", new CodePrimitiveExpression(methodInfo.Name)));
            attrDecl.Arguments.Add(new CodeAttributeArgument("EnableSession", new CodePrimitiveExpression(false)));

            webMethod.CustomAttributes.Add(attrDecl);

            List<ParameterInfo> outRefParams = new List<ParameterInfo>();

            CodeArrayCreateExpression paramArrayCreationExpression = new CodeArrayCreateExpression();
            paramArrayCreationExpression.CreateType = new CodeTypeReference(typeof(Object));

            foreach (ParameterInfo paramInfo in methodInfo.GetParameters())
            {
                CodeParameterDeclarationExpression paramDecl = new CodeParameterDeclarationExpression();

                if (paramInfo.IsOut || paramInfo.ParameterType.IsByRef)
                {
                    paramDecl.Type = new CodeTypeReference(paramInfo.ParameterType.GetElementType().FullName);
                    paramDecl.Direction = paramInfo.IsOut ? FieldDirection.Out : FieldDirection.Ref;

                    if (paramDecl.Direction == FieldDirection.Out && language == SupportedLanguages.VB)
                        paramDecl.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(System.Runtime.InteropServices.OutAttribute))));


                    outRefParams.Add(paramInfo);
                }
                else
                {
                    paramDecl.Type = new CodeTypeReference(paramInfo.ParameterType.FullName);
                }
                paramDecl.Name = paramInfo.Name;
                webMethod.Parameters.Add(paramDecl);

                if (!paramInfo.IsOut)
                    paramArrayCreationExpression.Initializers.Add(new CodeArgumentReferenceExpression(paramInfo.Name));
            }

            //Emit method body
            CodeMethodInvokeExpression baseInvokeExpression = new CodeMethodInvokeExpression();
            baseInvokeExpression.Method = new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "Invoke");

            baseInvokeExpression.Parameters.Add(new CodeTypeOfExpression(methodInfo.DeclaringType));
            baseInvokeExpression.Parameters.Add(new CodePrimitiveExpression(methodInfo.Name));
            baseInvokeExpression.Parameters.Add(new CodePrimitiveExpression(isActivation));
            baseInvokeExpression.Parameters.Add(paramArrayCreationExpression);

            int iStartIndex = (methodInfo.ReturnType == typeof(void)) ? 0 : 1;
            if (outRefParams.Count != 0)
            {
                //Results variable declaration
                CodeVariableDeclarationStatement resultsDeclaration = new CodeVariableDeclarationStatement(new CodeTypeReference(new CodeTypeReference(typeof(Object)), 1), "results");
                resultsDeclaration.InitExpression = baseInvokeExpression;
                webMethod.Statements.Add(resultsDeclaration);

                for (int i = 0; i < outRefParams.Count; ++i)
                {
                    ParameterInfo pinfo = outRefParams[i];
                    CodeAssignStatement assignStatement = new CodeAssignStatement();
                    CodeExpression leftExpression = new CodeArgumentReferenceExpression(pinfo.Name);
                    CodeExpression rightExpression = new CodeCastExpression(new CodeTypeReference(pinfo.ParameterType.GetElementType().FullName), new CodeIndexerExpression(new CodeVariableReferenceExpression("results"), new CodePrimitiveExpression(i + iStartIndex)));
                    assignStatement.Left = leftExpression;
                    assignStatement.Right = rightExpression;
                    webMethod.Statements.Add(assignStatement);
                }
            }
            if (methodInfo.ReturnType != typeof(void))
            {
                CodeExpression returnTargetExpression;

                if (outRefParams.Count != 0)
                    returnTargetExpression = new CodeVariableReferenceExpression("results");
                else
                    returnTargetExpression = baseInvokeExpression;

                CodeMethodReturnStatement methodReturnStatement = new CodeMethodReturnStatement(new CodeCastExpression(methodInfo.ReturnType, new CodeIndexerExpression(returnTargetExpression, new CodePrimitiveExpression(0))));
                webMethod.Statements.Add(methodReturnStatement);
            }
            else if (outRefParams.Count == 0 && methodInfo.ReturnType == typeof(void))
            {
                webMethod.Statements.Add(baseInvokeExpression);
            }

            return webMethod;
        }
    }

    internal sealed class InterfaceTypeFilterProvider : ITypeFilterProvider
    {
        private IServiceProvider serviceProvider;
        public InterfaceTypeFilterProvider(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public bool CanFilterType(Type type, bool throwOnError)
        {
            if (throwOnError && !type.IsInterface)
                throw new Exception(SR.GetString(SR.Error_InterfaceTypeNotInterface, "InterfaceType"));

            return type.IsInterface;
        }
        public string FilterDescription
        {
            get
            {
                return SR.GetString(SR.InterfaceTypeFilterDescription);
            }
        }
    }
}
