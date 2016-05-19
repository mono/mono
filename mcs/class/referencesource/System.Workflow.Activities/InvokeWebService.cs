namespace System.Workflow.Activities
{
    #region Imports

    using System;
    using System.Text;
    using System.Reflection;
    using System.Collections;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Runtime.Serialization;
    using System.Workflow.ComponentModel.Compiler;
    using System.ComponentModel.Design.Serialization;
    using System.Collections.Specialized;
    using System.Xml;
    using System.Web.Services.Protocols;
    using System.Collections.Generic;
    using System.Workflow.Runtime;
    using System.Workflow.Activities.Common;
    #endregion

    #region Class InvokeWebService

    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class InvokeWebServiceEventArgs : EventArgs
    {
        [NonSerialized]
        Object proxyInstance;

        public InvokeWebServiceEventArgs(Object proxyInstance)
        {
            this.proxyInstance = proxyInstance;
        }

        public Object WebServiceProxy
        {
            get
            {
                return this.proxyInstance;
            }
        }
    }

    [SRDescription(SR.InvokeWebServiceActivityDescription)]
    [ToolboxItem(typeof(InvokeWebServiceToolboxItem))]
    [Designer(typeof(InvokeWebServiceDesigner), typeof(IDesigner))]
    [ToolboxBitmap(typeof(InvokeWebServiceActivity), "Resources.WebServiceInOut.png")]
    [ActivityValidator(typeof(InvokeWebServiceValidator))]
    [SRCategory(SR.Standard)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class InvokeWebServiceActivity : Activity, IPropertyValueProvider, IDynamicPropertyTypeProvider
    {
        private static readonly Guid WebServiceInvoker = new Guid("C3FE5ABC-7D41-4064-810E-42BEF0A855EC");

        public static readonly DependencyProperty ProxyClassProperty = DependencyProperty.Register("ProxyClass", typeof(Type), typeof(InvokeWebServiceActivity), new PropertyMetadata(null, DependencyPropertyOptions.Metadata));
        public static readonly DependencyProperty MethodNameProperty = DependencyProperty.Register("MethodName", typeof(string), typeof(InvokeWebServiceActivity), new PropertyMetadata("", DependencyPropertyOptions.Metadata));
        public static readonly DependencyProperty SessionIdProperty = DependencyProperty.Register("SessionId", typeof(string), typeof(InvokeWebServiceActivity), new PropertyMetadata("", DependencyPropertyOptions.Metadata));
        public static readonly DependencyProperty ParameterBindingsProperty = DependencyProperty.Register("ParameterBindings", typeof(WorkflowParameterBindingCollection), typeof(InvokeWebServiceActivity), new PropertyMetadata(DependencyPropertyOptions.Metadata | DependencyPropertyOptions.ReadOnly, new Attribute[] { new BrowsableAttribute(false), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Content) }));
        public static readonly DependencyProperty InvokingEvent = DependencyProperty.Register("Invoking", typeof(EventHandler<InvokeWebServiceEventArgs>), typeof(InvokeWebServiceActivity));
        public static readonly DependencyProperty InvokedEvent = DependencyProperty.Register("Invoked", typeof(EventHandler<InvokeWebServiceEventArgs>), typeof(InvokeWebServiceActivity));

        static DependencyProperty SessionCookieContainerProperty = DependencyProperty.Register("SessionCookieContainer", typeof(System.Net.CookieContainer), typeof(InvokeWebServiceActivity));
        static DependencyProperty SessionCookieMapProperty = DependencyProperty.RegisterAttached("SessionCookieMap", typeof(Dictionary<String, System.Net.CookieContainer>), typeof(InvokeWebServiceActivity));

        #region Constructors

        public InvokeWebServiceActivity()
        {
            base.SetReadOnlyPropertyValue(ParameterBindingsProperty, new WorkflowParameterBindingCollection(this));
        }

        public InvokeWebServiceActivity(string name)
            : base(name)
        {
            base.SetReadOnlyPropertyValue(ParameterBindingsProperty, new WorkflowParameterBindingCollection(this));
        }

        #endregion

        internal System.Net.CookieContainer SessionCookieContainer
        {
            get
            {
                return (System.Net.CookieContainer)base.GetValue(SessionCookieContainerProperty);
            }
            set
            {
                base.SetValue(SessionCookieContainerProperty, value);
            }
        }

        internal static readonly ArrayList ReservedParameterNames = new ArrayList(new string[] { "Name", "Enabled", "Description", "MethodName", "ProxyClass", "SessionId", "Invoked", "Invoking" });

        #region Execute()

        protected override sealed ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            if (SessionId != "" && SessionId != null)
                PopulateSessionCookie();


            // Create instance of Proxy.
            object proxyInstance = Activator.CreateInstance(this.ProxyClass);

            //Set Session Cookie
            System.Web.Services.Protocols.HttpWebClientProtocol proxy = proxyInstance as System.Web.Services.Protocols.HttpWebClientProtocol;
            System.Diagnostics.Debug.Assert(proxy != null);
            proxy.CookieContainer = this.SessionCookieContainer;

            //Invoke OnBefore Invoke
            this.RaiseGenericEvent(InvokeWebServiceActivity.InvokingEvent, this, new InvokeWebServiceEventArgs(proxyInstance));

            // Get the parameters.
            MethodInfo methodInfo = this.ProxyClass.GetMethod(this.MethodName, BindingFlags.Instance | BindingFlags.Public);

            object[] actualParameters = InvokeHelper.GetParameters(methodInfo, this.ParameterBindings);

            WorkflowParameterBinding resultBinding = null;

            if (this.ParameterBindings.Contains("(ReturnValue)"))
                resultBinding = this.ParameterBindings["(ReturnValue)"];

            Object result = null;

            try
            {
                //Invoke the Web Service.
                result = this.ProxyClass.InvokeMember(this.MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod, null, proxyInstance, actualParameters, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException != null)
                    throw e.InnerException;
                else
                    throw;
            }

            // Save the result.
            if (resultBinding != null)
                resultBinding.Value = result;

            // Save ref parameters.
            InvokeHelper.SaveOutRefParameters(actualParameters, methodInfo, this.ParameterBindings);

            //Invoke OnAfter Invoke
            this.RaiseGenericEvent(InvokeWebServiceActivity.InvokedEvent, this, new InvokeWebServiceEventArgs(proxyInstance));

            return ActivityExecutionStatus.Closed;
        }

        void PopulateSessionCookie()
        {
            //Already session cookie is populated.
            if (this.SessionCookieContainer != null)
                return;

            //Map of Session - CookieContainer is populated in RootActvity.
            Activity rootActivity = (Activity)GetRootActivity();
            Dictionary<String, System.Net.CookieContainer> sessionCookieContainers;
            System.Net.CookieContainer cookieContainer;

            sessionCookieContainers = (Dictionary<String, System.Net.CookieContainer>)rootActivity.GetValue(SessionCookieMapProperty);

            if (sessionCookieContainers == null)
            {
                sessionCookieContainers = new Dictionary<String, System.Net.CookieContainer>();
                rootActivity.SetValue(SessionCookieMapProperty, sessionCookieContainers);
                cookieContainer = new System.Net.CookieContainer();
                sessionCookieContainers.Add(SessionId, cookieContainer);
            }
            else
            {
                if (!sessionCookieContainers.TryGetValue(SessionId, out cookieContainer))
                {
                    cookieContainer = new System.Net.CookieContainer();
                    sessionCookieContainers.Add(SessionId, cookieContainer);
                }
            }
            this.SessionCookieContainer = cookieContainer;
        }

        Activity GetRootActivity()
        {
            Activity parent = this;

            while (true)
            {
                if (parent.Parent == null)
                    return parent;

                parent = parent.Parent;
            }
        }

        protected override void OnClosed(IServiceProvider provider)
        {
            base.RemoveProperty(InvokeWebServiceActivity.SessionCookieContainerProperty);
            base.RemoveProperty(InvokeWebServiceActivity.SessionCookieMapProperty);
        }

        #endregion

        [SRCategory(SR.Activity)]
        [SRDescription(SR.ProxyClassDescr)]
        [TypeConverter(typeof(TypePropertyValueProviderTypeConverter))]
        [RefreshProperties(RefreshProperties.All)]
        [MergablePropertyAttribute(false)]
        [DefaultValue(null)]
        public Type ProxyClass
        {
            get
            {
                return base.GetValue(ProxyClassProperty) as Type;
            }
            set
            {
                base.SetValue(ProxyClassProperty, value);
            }
        }
        [RefreshProperties(RefreshProperties.All)]
        [TypeConverter(typeof(PropertyValueProviderTypeConverter))]
        [SRCategory(SR.Activity)]
        [SRDescription(SR.MethodNameDescr)]
        [MergablePropertyAttribute(false)]
        [DefaultValue("")]
        public string MethodName
        {
            get
            {
                return base.GetValue(MethodNameProperty) as string;
            }
            set
            {
                base.SetValue(MethodNameProperty, value);
            }
        }



        [SRCategory(SR.Handlers)]
        [SRDescription(SR.OnBeforeMethodInvokeDescr)]
        [MergableProperty(false)]
        public event EventHandler<InvokeWebServiceEventArgs> Invoking
        {
            add
            {
                base.AddHandler(InvokingEvent, value);
            }
            remove
            {
                base.RemoveHandler(InvokingEvent, value);
            }
        }

        [SRCategory(SR.Handlers)]
        [SRDescription(SR.OnAfterMethodInvokeDescr)]
        [MergableProperty(false)]
        public event EventHandler<InvokeWebServiceEventArgs> Invoked
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


        [SRCategory(SR.Activity)]
        [SRDescription(SR.WebServiceSessionIDDescr)]
        [DefaultValue("")]
        [MergablePropertyAttribute(false)]
        public string SessionId
        {
            get
            {
                return base.GetValue(SessionIdProperty) as string;
            }
            set
            {
                base.SetValue(SessionIdProperty, value);
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

        #region IPropertyValueProvider Members

        ICollection IPropertyValueProvider.GetPropertyValues(ITypeDescriptorContext context)
        {
            ITypeProvider typeProvider = (ITypeProvider)context.GetService(typeof(ITypeProvider));
            if (typeProvider == null)
                throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(ITypeProvider).FullName));

            ICollection values = null;
            if (context.PropertyDescriptor.Name == "ProxyClass")
            {
                List<Type> typeList = new List<Type>();
                Type webServiceBaseType = typeProvider.GetType(typeof(System.Web.Services.Protocols.SoapHttpClientProtocol).FullName);
                if (webServiceBaseType != null)
                {
                    Type[] types = typeProvider.GetTypes();
                    foreach (Type type in types)
                    {
                        if (!(type.Equals(webServiceBaseType)) && TypeProvider.IsAssignable(webServiceBaseType, type))
                            typeList.Add(type);
                    }
                }
                values = typeList.ToArray();
            }
            else if (context.PropertyDescriptor.Name == "MethodName")
            {
                StringCollection names = new StringCollection();
                Type type = this.ProxyClass;
                if (type != null)
                {
                    foreach (MethodInfo method in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public))
                    {
                        object[] attrs = method.GetCustomAttributes(typeof(SoapDocumentMethodAttribute), false);
                        if (attrs == null || attrs.Length == 0)
                            attrs = method.GetCustomAttributes(typeof(SoapRpcMethodAttribute), false);
                        if (attrs != null && attrs.Length > 0)
                            names.Add(method.Name);
                    }
                }
                values = names;
            }
            return values;
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

            Type type = this.ProxyClass;
            if (type != null)
            {
                MethodInfo method = type.GetMethod(this.MethodName);
                if (method != null)
                {
                    ArrayList paramInfos = new ArrayList(method.GetParameters());
                    if (!(method.ReturnType == typeof(void)))
                        paramInfos.Add(method.ReturnParameter);

                    foreach (ParameterInfo paramInfo in paramInfos)
                    {
                        if (paramInfo.ParameterType != null)
                        {
                            PropertyDescriptor prop = null;
                            if (paramInfo.Position == -1)
                                prop = new ParameterInfoBasedPropertyDescriptor(typeof(InvokeWebServiceActivity), paramInfo, false, DesignOnlyAttribute.Yes);
                            else
                                prop = new ParameterInfoBasedPropertyDescriptor(typeof(InvokeWebServiceActivity), paramInfo, InvokeWebServiceActivity.ReservedParameterNames.Contains(paramInfo.Name), DesignOnlyAttribute.Yes);

                            // return this parameter
                            properties[prop.Name] = prop;
                        }
                    }
                }
            }
        }
        #endregion

    }
    #endregion

    #region class InvokeWebServiceValidator

    internal sealed class InvokeWebServiceValidator : ActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = base.Validate(manager, obj);

            InvokeWebServiceActivity invokeWebService = obj as InvokeWebServiceActivity;
            if (invokeWebService == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(InvokeWebServiceActivity).FullName), "obj");

            if (invokeWebService.ProxyClass == null)
            {
                ValidationError error = new ValidationError(SR.GetString(SR.Error_TypePropertyInvalid, "ProxyClass"), ErrorNumbers.Error_PropertyNotSet);
                error.PropertyName = "ProxyClass";
                validationErrors.Add(error);
            }
            else
            {
                ITypeProvider typeProvider = (ITypeProvider)manager.GetService(typeof(ITypeProvider));
                if (typeProvider == null)
                    throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(ITypeProvider).FullName));

                Type proxyClassType = invokeWebService.ProxyClass;

                // Validate method
                if (invokeWebService.MethodName == null || invokeWebService.MethodName.Length == 0)
                    validationErrors.Add(ValidationError.GetNotSetValidationError("MethodName"));
                else
                {
                    MethodInfo methodInfo = proxyClassType.GetMethod(invokeWebService.MethodName);
                    if (methodInfo == null)
                    {
                        ValidationError error = new ValidationError(SR.GetString(SR.Error_MethodNotExists, "MethodName", invokeWebService.MethodName), ErrorNumbers.Error_MethodNotExists);
                        error.PropertyName = "MethodName";
                        validationErrors.Add(error);
                    }
                    else
                    {
                        ArrayList paramInfos = new ArrayList(methodInfo.GetParameters());
                        if (methodInfo.ReturnType != typeof(void))
                            paramInfos.Add(methodInfo.ReturnParameter);

                        foreach (ParameterInfo paramInfo in paramInfos)
                        {
                            string paramName = paramInfo.Name;
                            if (paramInfo.Position == -1)
                                paramName = "(ReturnValue)";

                            object paramValue = null;
                            if (invokeWebService.ParameterBindings.Contains(paramName))
                            {
                                if (invokeWebService.ParameterBindings[paramName].IsBindingSet(WorkflowParameterBinding.ValueProperty))
                                    paramValue = invokeWebService.ParameterBindings[paramName].GetBinding(WorkflowParameterBinding.ValueProperty);
                                else
                                    paramValue = invokeWebService.ParameterBindings[paramName].GetValue(WorkflowParameterBinding.ValueProperty);
                            }
                            if (!invokeWebService.ParameterBindings.Contains(paramName) || paramValue == null)
                            {
                                ValidationError validationError = ValidationError.GetNotSetValidationError(paramName);
                                if (InvokeWebServiceActivity.ReservedParameterNames.Contains(paramName))
                                    validationError.PropertyName = ParameterInfoBasedPropertyDescriptor.GetParameterPropertyName(invokeWebService.GetType(), paramName);
                                validationError.PropertyName = paramName;
                                validationErrors.Add(validationError);
                            }
                            else
                            {
                                AccessTypes access = AccessTypes.Read;
                                if (paramInfo.IsOut || paramInfo.IsRetval)
                                    access = AccessTypes.Write;
                                else if (paramInfo.ParameterType.IsByRef)
                                    access |= AccessTypes.Write;

                                ValidationErrorCollection variableErrors = ValidationHelpers.ValidateProperty(manager, invokeWebService, paramValue,
                                                                                new PropertyValidationContext(invokeWebService.ParameterBindings[paramName], null, paramName), new BindValidationContext(paramInfo.ParameterType.IsByRef ? paramInfo.ParameterType.GetElementType() : paramInfo.ParameterType, access));
                                if (InvokeWebServiceActivity.ReservedParameterNames.Contains(paramName))
                                {
                                    foreach (ValidationError validationError in variableErrors)
                                        validationError.PropertyName = ParameterInfoBasedPropertyDescriptor.GetParameterPropertyName(invokeWebService.GetType(), paramName);
                                }
                                validationErrors.AddRange(variableErrors);
                            }
                        }

                        if (invokeWebService.ParameterBindings.Count > paramInfos.Count)
                            validationErrors.Add(new ValidationError(SR.GetString(SR.Warning_AdditionalBindingsFound), ErrorNumbers.Warning_AdditionalBindingsFound, true));
                    }
                }
            }
            return validationErrors;
        }
    }

    #endregion
}
