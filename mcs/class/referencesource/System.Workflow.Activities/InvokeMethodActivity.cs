namespace System.Workflow.Activities
{
    using System;
    using System.Reflection;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Compiler;
    using System.Runtime.Serialization;
    using System.ComponentModel.Design.Serialization;
    using System.Collections.Specialized;
    using System.Collections.Generic;
    using System.Workflow.Runtime;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Workflow.Activities.Common;

    [SRDescription(SR.CallExternalMethodActivityDescription)]
    [Designer(typeof(CallExternalMethodActivityDesigner), typeof(IDesigner))]
    [DefaultEvent("MethodInvoking")]
    [ActivityValidator(typeof(CallExternalMethodActivityValidator))]
    [SRCategory(SR.Base)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class CallExternalMethodActivity : Activity, IPropertyValueProvider, IDynamicPropertyTypeProvider
    {
        //instance properties
        public static readonly DependencyProperty CorrelationTokenProperty = DependencyProperty.Register("CorrelationToken", typeof(CorrelationToken), typeof(CallExternalMethodActivity), new PropertyMetadata(DependencyPropertyOptions.Metadata));
        public static readonly DependencyProperty ParameterBindingsProperty = DependencyProperty.Register("ParameterBindings", typeof(WorkflowParameterBindingCollection), typeof(CallExternalMethodActivity), new PropertyMetadata(DependencyPropertyOptions.Metadata | DependencyPropertyOptions.ReadOnly, new Attribute[] { new BrowsableAttribute(false), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Content) }));

        //metadata properties
        public static readonly DependencyProperty InterfaceTypeProperty = DependencyProperty.Register("InterfaceType", typeof(System.Type), typeof(CallExternalMethodActivity), new PropertyMetadata(null, DependencyPropertyOptions.Metadata, new Attribute[] { new ValidationOptionAttribute(ValidationOption.Required) }));
        public static readonly DependencyProperty MethodNameProperty = DependencyProperty.Register("MethodName", typeof(string), typeof(CallExternalMethodActivity), new PropertyMetadata("", DependencyPropertyOptions.Metadata, new Attribute[] { new ValidationOptionAttribute(ValidationOption.Required) }));

        //event
        public static readonly DependencyProperty MethodInvokingEvent = DependencyProperty.Register("MethodInvoking", typeof(EventHandler), typeof(CallExternalMethodActivity));

        internal static readonly ArrayList ReservedParameterNames = new ArrayList(new string[] { "Name", "Enabled", "Description", "MethodName", "MethodInvoking", "InterfaceType" });

        #region Constructors

        public CallExternalMethodActivity()
        {
            base.SetReadOnlyPropertyValue(ParameterBindingsProperty, new WorkflowParameterBindingCollection(this));
        }

        public CallExternalMethodActivity(string name)
            : base(name)
        {
            base.SetReadOnlyPropertyValue(ParameterBindingsProperty, new WorkflowParameterBindingCollection(this));
        }

        #endregion

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

        [RefreshProperties(RefreshProperties.All)]
        [TypeConverter(typeof(PropertyValueProviderTypeConverter))]
        [SRCategory(SR.Activity)]
        [SRDescription(SR.ExternalMethodNameDescr)]
        [MergablePropertyAttribute(false)]
        [DefaultValue("")]
        public virtual string MethodName
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

        [SRDescription(SR.OnBeforeMethodInvokeDescr)]
        [SRCategory(SR.Handlers)]
        [MergableProperty(false)]
        public event EventHandler MethodInvoking
        {
            add
            {
                base.AddHandler(MethodInvokingEvent, value);
            }
            remove
            {
                base.RemoveHandler(MethodInvokingEvent, value);
            }
        }

        ICollection IPropertyValueProvider.GetPropertyValues(ITypeDescriptorContext context)
        {
            StringCollection names = new StringCollection();
            if (this.InterfaceType == null)
                return names;

            if (context.PropertyDescriptor.Name == "MethodName")
            {
                foreach (MethodInfo method in this.InterfaceType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public))
                {
                    if (method.IsSpecialName)
                        continue;
                    names.Add(method.Name);
                }
            }
            return names;
        }

        protected override void InitializeProperties()
        {
            ActivityHelpers.InitializeCorrelationTokenCollection(this, this.CorrelationToken);

            Type type = this.InterfaceType;
            if (type == null)
                throw new InvalidOperationException(SR.GetString(SR.InterfaceTypeMissing, this.Name));

            string methodName = this.MethodName;
            if (methodName == null)
                throw new InvalidOperationException(SR.GetString(SR.MethodNameMissing, this.Name));

            MethodInfo methodInfo = type.GetMethod(methodName);
            if (methodInfo != null)
                InvokeHelper.InitializeParameters(methodInfo, this.ParameterBindings);
            else
                throw new InvalidOperationException(SR.GetString(SR.MethodInfoMissing, this.MethodName, this.InterfaceType.Name));

            base.InitializeProperties();
        }

        protected virtual void OnMethodInvoking(EventArgs e)
        {
        }

        protected virtual void OnMethodInvoked(EventArgs e)
        {
        }

        protected sealed override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");
            if (this.InterfaceType == null)
                throw new ArgumentException(
                    SR.GetString(SR.Error_MissingInterfaceType), "executionContext");

            Type type = this.InterfaceType;
            string methodName = this.MethodName;

            object serviceValue = executionContext.GetService(type);
            if (serviceValue == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_ServiceNotFound, this.InterfaceType));

            this.RaiseEvent(MethodInvokingEvent, this, EventArgs.Empty);
            OnMethodInvoking(EventArgs.Empty);

            MethodInfo methodInfo = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
            ParameterModifier[] parameterModifiers = null;
            object[] actualParameters = InvokeHelper.GetParameters(methodInfo, this.ParameterBindings, out parameterModifiers);

            WorkflowParameterBinding resultBinding = null;
            if (this.ParameterBindings.Contains("(ReturnValue)"))
                resultBinding = this.ParameterBindings["(ReturnValue)"];

            CorrelationService.InvalidateCorrelationToken(this, type, methodName, actualParameters);

            object result = type.InvokeMember(this.MethodName, BindingFlags.InvokeMethod, new ExternalDataExchangeBinder(), serviceValue, actualParameters, parameterModifiers, null, null);
            if (resultBinding != null)
                resultBinding.Value = InvokeHelper.CloneOutboundValue(result, new BinaryFormatter(), "(ReturnValue)");

            InvokeHelper.SaveOutRefParameters(actualParameters, methodInfo, this.ParameterBindings);
            OnMethodInvoked(EventArgs.Empty);
            return ActivityExecutionStatus.Closed;
        }

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

            if (this.GetType() != typeof(CallExternalMethodActivity))
                return; // if custom activity do not add parameter binding

            Type type = this.InterfaceType;
            if (type == null)
                return;

            MethodInfo method = type.GetMethod(this.MethodName);
            if (method != null)
            {
                ArrayList paramInfo = new ArrayList(method.GetParameters());
                if (!(method.ReturnType == typeof(void)))
                    paramInfo.Add(method.ReturnParameter);

                foreach (ParameterInfo param in paramInfo)
                {
                    if (param.ParameterType != null)
                    {
                        PropertyDescriptor prop = new ParameterInfoBasedPropertyDescriptor(typeof(CallExternalMethodActivity), param, true, DesignOnlyAttribute.Yes);
                        properties[prop.Name] = prop;
                    }
                }
            }
        }
        #endregion
    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class CallExternalMethodActivityValidator : ActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            CallExternalMethodActivity methodInvoke = obj as CallExternalMethodActivity;
            if (methodInvoke == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(CallExternalMethodActivity).FullName), "obj");

            ValidationErrorCollection validationErrors = base.Validate(manager, obj);
            validationErrors.AddRange(CorrelationSetsValidator.Validate(manager, obj));
            validationErrors.AddRange(ParameterBindingValidator.Validate(manager, obj));
            return validationErrors;
        }
    }
}
