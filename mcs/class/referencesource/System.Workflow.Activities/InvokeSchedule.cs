namespace System.Workflow.Activities
{
    #region Imports

    using System;
    using System.IO;
    using System.Xml;
    using System.Text;
    using System.Reflection;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Diagnostics;
    using System.Drawing.Design;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.CodeDom;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Collections.Generic;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.Runtime;
    using System.Workflow.Runtime.Hosting;
    using System.Collections.Specialized;
    using System.Workflow.Activities.Common;

    #endregion

    // Note that the designer is associated dynamically through the type description provider
    [SRDescription(SR.InvokeWorkflowActivityDescription)]
    [ToolboxItem(typeof(ActivityToolboxItem))]
    [ToolboxBitmap(typeof(InvokeWorkflowActivity), "Resources.Service.bmp")]
    [ActivityValidator(typeof(InvokeWorkflowValidator))]
    [DefaultEvent("Invoking")]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class InvokeWorkflowActivity : Activity, ITypeFilterProvider
    {
        public static readonly DependencyProperty TargetWorkflowProperty = DependencyProperty.Register("TargetWorkflow", typeof(Type), typeof(InvokeWorkflowActivity), new PropertyMetadata(null, DependencyPropertyOptions.Metadata));
        public static readonly DependencyProperty ParameterBindingsProperty = DependencyProperty.Register("ParameterBindings", typeof(WorkflowParameterBindingCollection), typeof(InvokeWorkflowActivity), new PropertyMetadata(DependencyPropertyOptions.Metadata | DependencyPropertyOptions.ReadOnly, new Attribute[] { new BrowsableAttribute(false), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Content) }));

        public static readonly DependencyProperty InvokingEvent = DependencyProperty.Register("Invoking", typeof(EventHandler), typeof(InvokeWorkflowActivity));

        public static readonly DependencyProperty InstanceIdProperty = DependencyProperty.Register("InstanceId", typeof(Guid), typeof(InvokeWorkflowActivity), new PropertyMetadata(Guid.Empty));

        internal static readonly ArrayList ReservedParameterNames = new ArrayList(new string[] { "Name", "Enabled", "Description", "TargetWorkflow", "Invoking", "ParameterBindings" });

        #region Constructors

        public InvokeWorkflowActivity()
        {
            //
            base.SetReadOnlyPropertyValue(ParameterBindingsProperty, new WorkflowParameterBindingCollection(this));
        }

        public InvokeWorkflowActivity(string name)
            : base(name)
        {
            //
            base.SetReadOnlyPropertyValue(ParameterBindingsProperty, new WorkflowParameterBindingCollection(this));
        }

        #endregion

        [SRCategory(SR.Activity)]
        [SRDescription(SR.TargetWorkflowDescr)]
        [Editor(typeof(TypeBrowserEditor), typeof(UITypeEditor))]
        [DefaultValue(null)]
        public Type TargetWorkflow
        {
            get
            {
                return base.GetValue(TargetWorkflowProperty) as Type;
            }
            set
            {
                base.SetValue(TargetWorkflowProperty, value);
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Guid InstanceId
        {
            get
            {
                return (Guid)base.GetValue(InstanceIdProperty);
            }
        }

        internal void SetInstanceId(Guid value)
        {
            //
            base.SetValue(InstanceIdProperty, value);
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

        [SRCategory(SR.Handlers)]
        [SRDescription(SR.InitializeCaleeDescr)]
        [MergableProperty(false)]
        public event EventHandler Invoking
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

        #region ITypeFilterProvider Members

        bool ITypeFilterProvider.CanFilterType(Type type, bool throwOnError)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            bool canFilterType = TypeProvider.IsAssignable(typeof(Activity), type) && type != typeof(Activity) && !type.IsAbstract;
            if (canFilterType)
            {
                //This means that the type is DesignTimeType
                IDesignerHost designerHost = ((IComponent)this).Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (designerHost != null && String.Compare(designerHost.RootComponentClassName, type.FullName, StringComparison.Ordinal) == 0)
                {
                    if (throwOnError)
                        throw new InvalidOperationException(SR.GetString(SR.Error_CantInvokeSelf));
                    else
                        canFilterType = false;
                }
            }

            if (throwOnError && !canFilterType)
                throw new Exception(SR.GetString(SR.Error_TypeIsNotRootActivity, "TargetWorkflow"));

            return canFilterType;
        }

        string ITypeFilterProvider.FilterDescription
        {
            get
            {
                return SR.GetString(SR.FilterDescription_InvokeWorkflow);
            }
        }

        #endregion

        #region Execute
        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            // raise event
            base.RaiseEvent(InvokeWorkflowActivity.InvokingEvent, this, EventArgs.Empty);

            // collect the [in] parameters
            Dictionary<string, object> namedArgumentValues = new Dictionary<string, object>();
            foreach (WorkflowParameterBinding paramBinding in this.ParameterBindings)
                namedArgumentValues.Add(paramBinding.ParameterName, paramBinding.Value);

            IStartWorkflow workflowInvoker = executionContext.GetService(typeof(IStartWorkflow)) as IStartWorkflow;
            if (workflowInvoker == null)
                throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(IStartWorkflow).FullName));

            Guid instanceId = workflowInvoker.StartWorkflow(this.TargetWorkflow, namedArgumentValues);
            if (instanceId == Guid.Empty)
                throw new InvalidOperationException(SR.GetString(SR.Error_FailedToStartTheWorkflow));

            this.SetInstanceId(instanceId);

            return ActivityExecutionStatus.Closed;
        }
        #endregion

    }

    #region validator
    internal sealed class InvokeWorkflowValidator : ActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = base.Validate(manager, obj);

            InvokeWorkflowActivity invokeWorkflow = obj as InvokeWorkflowActivity;
            if (invokeWorkflow == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(InvokeWorkflowActivity).FullName), "obj");

            if (invokeWorkflow.TargetWorkflow == null)
            {
                ValidationError error = new ValidationError(SR.GetString(SR.Error_TypePropertyInvalid, "TargetWorkflow"), ErrorNumbers.Error_PropertyNotSet);
                error.PropertyName = "TargetWorkflow";
                validationErrors.Add(error);
            }
            else
            {
                ITypeProvider typeProvider = (ITypeProvider)manager.GetService(typeof(ITypeProvider));
                if (typeProvider == null)
                    throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(ITypeProvider).FullName));

                Type targetWorkflowType = invokeWorkflow.TargetWorkflow;
                if (targetWorkflowType.Assembly == null && typeProvider.LocalAssembly != null)
                {
                    Type workflowType = typeProvider.LocalAssembly.GetType(targetWorkflowType.FullName);
                    if (workflowType != null)
                        targetWorkflowType = workflowType;
                }

                if (!TypeProvider.IsAssignable(typeof(Activity), targetWorkflowType))
                {
                    ValidationError error = new ValidationError(SR.GetString(SR.Error_TypeIsNotRootActivity, "TargetWorkflow"), ErrorNumbers.Error_TypeIsNotRootActivity);
                    error.PropertyName = "TargetWorkflow";
                    validationErrors.Add(error);
                }
                else
                {
                    Activity rootActivity = null;
                    try
                    {
                        rootActivity = Activator.CreateInstance(targetWorkflowType) as Activity;
                    }
                    catch (Exception)
                    {
                        //
                    }

                    if (rootActivity == null)
                    {
                        ValidationError error = new ValidationError(SR.GetString(SR.Error_GetCalleeWorkflow, invokeWorkflow.TargetWorkflow), ErrorNumbers.Error_GetCalleeWorkflow);
                        error.PropertyName = "TargetWorkflow";
                        validationErrors.Add(error);
                    }
                    else
                    {
                        // Exec can't have activate receive.
                        Walker walker = new Walker();
                        walker.FoundActivity += delegate(Walker w, WalkerEventArgs args)
                        {
                            if ((args.CurrentActivity is WebServiceInputActivity && ((WebServiceInputActivity)args.CurrentActivity).IsActivating))
                            {
                                ValidationError validationError = new ValidationError(SR.GetString(SR.Error_ExecWithActivationReceive), ErrorNumbers.Error_ExecWithActivationReceive);
                                validationError.PropertyName = "Name";
                                validationErrors.Add(validationError);

                                args.Action = WalkerAction.Abort;
                            }
                        };

                        walker.Walk((Activity)rootActivity);

                        bool inAtomicScope = false;
                        Activity parentScope = invokeWorkflow.Parent;
                        while (parentScope != null)
                        {
                            if (parentScope is CompensatableTransactionScopeActivity || parentScope is TransactionScopeActivity)
                            {
                                inAtomicScope = true;
                                break;
                            }
                            parentScope = parentScope.Parent;
                        }

                        // Validate that if the workflow is transactional or being exec'd then it is not enclosed in an atomic scope.
                        if (inAtomicScope)
                        {
                            ValidationError validationError = new ValidationError(SR.GetString(SR.Error_ExecInAtomicScope), ErrorNumbers.Error_ExecInAtomicScope);
                            validationErrors.Add(validationError);
                        }

                        foreach (WorkflowParameterBinding paramBinding in invokeWorkflow.ParameterBindings)
                        {
                            PropertyInfo propertyInfo = null;

                            propertyInfo = targetWorkflowType.GetProperty(paramBinding.ParameterName);
                            if (propertyInfo == null)
                            {
                                ValidationError validationError = new ValidationError(SR.GetString(SR.Error_ParameterNotFound, paramBinding.ParameterName), ErrorNumbers.Error_ParameterNotFound);
                                if (InvokeWorkflowActivity.ReservedParameterNames.Contains(paramBinding.ParameterName))
                                    validationError.PropertyName = ParameterInfoBasedPropertyDescriptor.GetParameterPropertyName(invokeWorkflow.GetType(), paramBinding.ParameterName);

                                validationErrors.Add(validationError);
                                continue;
                            }

                            Type parameterType = propertyInfo.PropertyType;
                            if (paramBinding.GetBinding(WorkflowParameterBinding.ValueProperty) != null)
                            {
                                ValidationErrorCollection memberErrors = ValidationHelpers.ValidateProperty(manager, invokeWorkflow, paramBinding.GetBinding(WorkflowParameterBinding.ValueProperty), new PropertyValidationContext(paramBinding, null, paramBinding.ParameterName), new BindValidationContext(parameterType, AccessTypes.Read));

                                if (memberErrors.Count != 0)
                                {
                                    validationErrors.AddRange(memberErrors);
                                    continue;
                                }
                            }
                        }
                    }
                }
            }

            return validationErrors;
        }
    }
    #endregion
}
