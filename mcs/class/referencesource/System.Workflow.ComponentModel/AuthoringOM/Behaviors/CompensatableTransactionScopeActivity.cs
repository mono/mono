namespace System.Workflow.ComponentModel
{
    #region Imports

    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Compiler;

    #endregion

    [SRDescription(SR.CompensatableTransactionalContextActivityDescription)]
    [ToolboxItem(typeof(ActivityToolboxItem))]
    [ToolboxBitmap(typeof(CompensatableTransactionScopeActivity), "Resources.Sequence.png")]
    [Designer(typeof(CompensatableTransactionScopeActivityDesigner), typeof(IDesigner))]
    [PersistOnClose]
    [SupportsTransaction]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class CompensatableTransactionScopeActivity : CompositeActivity, IActivityEventListener<ActivityExecutionStatusChangedEventArgs>, ICompensatableActivity
    {
        internal static readonly DependencyProperty TransactionOptionsProperty = DependencyProperty.Register("TransactionOptions", typeof(WorkflowTransactionOptions), typeof(CompensatableTransactionScopeActivity), new PropertyMetadata(DependencyPropertyOptions.Metadata, new Attribute[] { new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Content) }));
        public CompensatableTransactionScopeActivity()
        {
            this.SetValueBase(TransactionOptionsProperty, new WorkflowTransactionOptions());
        }

        public CompensatableTransactionScopeActivity(string name)
            : base(name)
        {
            this.SetValueBase(TransactionOptionsProperty, new WorkflowTransactionOptions());
        }

        //[SRDisplayName(SR.Transaction)]
        [SRDescription(SR.TransactionDesc)]
        [MergableProperty(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [ReadOnly(true)]
        public WorkflowTransactionOptions TransactionOptions
        {
            get
            {
                return (WorkflowTransactionOptions)this.GetValue(TransactionOptionsProperty);
            }

            set
            {
                SetValue(TransactionOptionsProperty, value);
            }
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

        ActivityExecutionStatus ICompensatableActivity.Compensate(ActivityExecutionContext executionContext)
        {
            return ActivityExecutionStatus.Closed;
        }
    }
}
