namespace System.Workflow.ComponentModel
{
    #region Imports

    using System;
    using System.Drawing;
    using System.CodeDom;
    using System.Collections;
    using System.Reflection;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Compiler;

    #endregion

    [SRDescription(SR.FaultHandlerActivityDescription)]
    [ToolboxItem(typeof(ActivityToolboxItem))]
    [ToolboxBitmap(typeof(FaultHandlerActivity), "Resources.Exception.png")]
    [SRCategory(SR.Standard)]
    [Designer(typeof(FaultHandlerActivityDesigner), typeof(IDesigner))]
    [ActivityValidator(typeof(FaultHandlerActivityValidator))]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class FaultHandlerActivity : CompositeActivity, IActivityEventListener<ActivityExecutionStatusChangedEventArgs>, ITypeFilterProvider, IDynamicPropertyTypeProvider
    {
        public static readonly DependencyProperty FaultTypeProperty = DependencyProperty.Register("FaultType", typeof(Type), typeof(FaultHandlerActivity), new PropertyMetadata(DependencyPropertyOptions.Metadata));
        internal static readonly DependencyProperty FaultProperty = DependencyProperty.Register("Fault", typeof(Exception), typeof(FaultHandlerActivity));

        public FaultHandlerActivity()
        {
        }

        public FaultHandlerActivity(string name)
            : base(name)
        {
        }

        [Editor(typeof(TypeBrowserEditor), typeof(UITypeEditor))]
        [SRDescription(SR.ExceptionTypeDescr)]
        [MergableProperty(false)]
        public Type FaultType
        {
            get
            {
                return (Type)base.GetValue(FaultTypeProperty);
            }
            set
            {
                base.SetValue(FaultTypeProperty, value);
            }
        }

        [SRDescription(SR.FaultDescription)]
        [MergableProperty(false)]
        [ReadOnly(true)]
        public Exception Fault
        {
            get
            {
                return base.GetValue(FaultProperty) as Exception;
            }
        }

        internal void SetException(Exception e)
        {
            this.SetValue(FaultProperty, e);
        }

        protected internal override void Initialize(IServiceProvider provider)
        {
            if (this.Parent == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_MustHaveParent));

            base.Initialize(provider);
        }

        protected internal override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            return SequenceHelper.Execute(this, executionContext);
        }

        protected internal override ActivityExecutionStatus Cancel(ActivityExecutionContext executionContext)
        {
            return SequenceHelper.Cancel(this, executionContext);
        }

        void IActivityEventListener<ActivityExecutionStatusChangedEventArgs>.OnEvent(Object sender, ActivityExecutionStatusChangedEventArgs e)
        {
            SequenceHelper.OnEvent(this, sender, e);
        }

        protected internal override void OnActivityChangeRemove(ActivityExecutionContext executionContext, Activity removedActivity)
        {
            SequenceHelper.OnActivityChangeRemove(this, executionContext, removedActivity);
        }

        protected internal override void OnWorkflowChangesCompleted(ActivityExecutionContext executionContext)
        {
            SequenceHelper.OnWorkflowChangesCompleted(this, executionContext);
        }

        #region ITypeFilterProvider Members

        bool ITypeFilterProvider.CanFilterType(Type type, bool throwOnError)
        {
            bool isAssignable = TypeProvider.IsAssignable(typeof(Exception), type);

            if (throwOnError && !isAssignable)
                throw new Exception(SR.GetString(SR.Error_ExceptionTypeNotException, type, "Type"));

            return isAssignable;
        }

        string ITypeFilterProvider.FilterDescription
        {
            get
            {
                return SR.GetString(SR.FilterDescription_FaultHandlerActivity);
            }
        }

        #endregion

        #region IDynamicPropertyTypeProvider Members
        Type IDynamicPropertyTypeProvider.GetPropertyType(IServiceProvider serviceProvider, string propertyName)
        {
            if (propertyName == null)
                throw new ArgumentNullException("propertyName");

            Type returnType = null;
            if (string.Equals(propertyName, "Fault", StringComparison.Ordinal))
            {
                returnType = this.FaultType;
                if (returnType == null)
                    returnType = typeof(Exception);
            }

            return returnType;
        }

        AccessTypes IDynamicPropertyTypeProvider.GetAccessType(IServiceProvider serviceProvider, string propertyName)
        {
            if (propertyName == null)
                throw new ArgumentNullException("propertyName");

            if (propertyName.Equals("Fault", StringComparison.Ordinal))
                return AccessTypes.Write;
            else
                return AccessTypes.Read;
        }
        #endregion
    }

    internal sealed class FaultHandlerActivityValidator : CompositeActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = base.Validate(manager, obj);

            FaultHandlerActivity exceptionHandler = obj as FaultHandlerActivity;
            if (exceptionHandler == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(FaultHandlerActivity).FullName), "obj");

            // check parent must be exception handler
            if (!(exceptionHandler.Parent is FaultHandlersActivity))
                validationErrors.Add(new ValidationError(SR.GetString(SR.Error_FaultHandlerActivityParentNotFaultHandlersActivity), ErrorNumbers.Error_FaultHandlerActivityParentNotFaultHandlersActivity));

            // validate exception property
            ITypeProvider typeProvider = manager.GetService(typeof(ITypeProvider)) as ITypeProvider;
            if (typeProvider == null)
                throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(ITypeProvider).FullName));

            // Validate the required Type property
            ValidationError error = null;
            if (exceptionHandler.FaultType == null)
            {
                error = new ValidationError(SR.GetString(SR.Error_TypePropertyInvalid, "FaultType"), ErrorNumbers.Error_PropertyNotSet);
                error.PropertyName = "FaultType";
                validationErrors.Add(error);
            }
            else if (!TypeProvider.IsAssignable(typeof(Exception), exceptionHandler.FaultType))
            {
                error = new ValidationError(SR.GetString(SR.Error_TypeTypeMismatch, new object[] { "FaultType", typeof(Exception).FullName }), ErrorNumbers.Error_TypeTypeMismatch);
                error.PropertyName = "FaultType";
                validationErrors.Add(error);
            }

            // Generate a warning for unrechable code, if the catch type is all and this is not the last exception handler.
            /*if (exceptionHandler.FaultType == typeof(System.Exception) && exceptionHandler.Parent is FaultHandlersActivity && ((FaultHandlersActivity)exceptionHandler.Parent).Activities.IndexOf(exceptionHandler) != ((FaultHandlersActivity)exceptionHandler.Parent).Activities.Count - 1)
            {
                error = new ValidationError(SR.GetString(SR.Error_FaultHandlerActivityAllMustBeLast), ErrorNumbers.Error_FaultHandlerActivityAllMustBeLast, true);
                error.PropertyName = "FaultType";
                validationErrors.Add(error);
            }*/

            if (exceptionHandler.EnabledActivities.Count == 0)
                validationErrors.Add(new ValidationError(SR.GetString(SR.Warning_EmptyBehaviourActivity, typeof(FaultHandlerActivity).FullName, exceptionHandler.QualifiedName), ErrorNumbers.Warning_EmptyBehaviourActivity, true));

            // fault handler can not contain fault handlers, compensation handler and cancellation handler
            if (((ISupportAlternateFlow)exceptionHandler).AlternateFlowActivities.Count > 0)
                validationErrors.Add(new ValidationError(SR.GetString(SR.Error_ModelingConstructsCanNotContainModelingConstructs), ErrorNumbers.Error_ModelingConstructsCanNotContainModelingConstructs));

            return validationErrors;
        }
    }
}
