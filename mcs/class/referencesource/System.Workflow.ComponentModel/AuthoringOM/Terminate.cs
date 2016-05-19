namespace System.Workflow.ComponentModel
{
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

    [SRDescription(SR.TerminateActivityDescription)]
    [ToolboxItem(typeof(ActivityToolboxItem))]
    [Designer(typeof(TerminateDesigner), typeof(IDesigner))]
    [ToolboxBitmap(typeof(TerminateActivity), "Resources.Terminate.png")]
    [SRCategory(SR.Standard)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class TerminateActivity : Activity
    {
        public static readonly DependencyProperty ErrorProperty = DependencyProperty.Register("Error", typeof(string), typeof(TerminateActivity));

        #region Constructors

        public TerminateActivity()
        {
        }

        public TerminateActivity(string name)
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

            string terminateReason = this.Error;
            executionContext.TerminateWorkflowInstance(new WorkflowTerminatedException(terminateReason));
            return ActivityExecutionStatus.Closed;
        }
        [Browsable(true)]
        [SRCategory(SR.Activity)]
        [SRDescription(SR.TerminateErrorMessageDescr)]
        [MergableProperty(false)]
        [DefaultValue((string)null)]
        public string Error
        {
            get
            {
                return (string)base.GetValue(ErrorProperty);
            }
            set
            {
                base.SetValue(ErrorProperty, value);
            }
        }

    }
}
