namespace System.Workflow.ComponentModel
{
    #region Imports

    using System;
    using System.Text;
    using System.Reflection;
    using System.Collections;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Collections.Generic;
    using System.Drawing.Design;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.Runtime;
    #endregion

    [SRDescription(SR.SuspendActivityDescription)]
    [ToolboxItem(typeof(ActivityToolboxItem))]
    [Designer(typeof(SuspendDesigner), typeof(IDesigner))]
    [ToolboxBitmap(typeof(SuspendActivity), "Resources.Suspend.png")]
    [ActivityValidator(typeof(SuspendValidator))]
    [SRCategory(SR.Standard)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class SuspendActivity : Activity
    {
        public static readonly DependencyProperty ErrorProperty = DependencyProperty.Register("Error", typeof(string), typeof(SuspendActivity));

        #region Constructors

        public SuspendActivity()
        {
        }

        public SuspendActivity(string name)
            : base(name)
        {
        }

        #endregion

        protected internal override void Initialize(IServiceProvider provider)
        {
            if (this.Parent == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_MustHaveParent));

            base.Initialize(provider);
        }

        protected internal override sealed ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            executionContext.CloseActivity();

            string suspendReason = this.Error;
            executionContext.SuspendWorkflowInstance(suspendReason);
            return ActivityExecutionStatus.Closed;
        }

        [Browsable(true)]
        [SRCategory(SR.Activity)]
        [SRDescription(SR.SuspendErrorMessageDescr)]
        [MergableProperty(false)]
        [DefaultValue((string)null)]
        public string Error
        {
            get
            {
                return base.GetValue(ErrorProperty) as string;
            }
            set
            {
                base.SetValue(ErrorProperty, value);
            }
        }
    }

    internal sealed class SuspendValidator : ActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = base.Validate(manager, obj);

            SuspendActivity suspend = obj as SuspendActivity;
            if (suspend == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(SuspendActivity).FullName), "obj");

            CompositeActivity parent = suspend.Parent;
            while (parent != null)
            {
                if (parent.SupportsTransaction)
                {
                    validationErrors.Add(new ValidationError(SR.GetString(SR.Error_SuspendInAtomicScope), ErrorNumbers.Error_SuspendInAtomicScope));
                    break;
                }
                parent = parent.Parent;
            }
            return validationErrors;
        }
    }
}
