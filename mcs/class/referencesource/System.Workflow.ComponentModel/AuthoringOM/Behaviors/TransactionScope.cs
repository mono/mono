namespace System.Workflow.ComponentModel
{
    #region Imports

    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Compiler;
    using System.ComponentModel.Design;

    #endregion

    [SRDescription(SR.TransactionalContextActivityDescription)]
    [ToolboxItem(typeof(ActivityToolboxItem))]
    [ToolboxBitmap(typeof(TransactionScopeActivity), "Resources.Sequence.png")]
    [Designer(typeof(TransactionScopeActivityDesigner), typeof(IDesigner))]
    [PersistOnClose]
    [SupportsTransaction]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class TransactionScopeActivity : CompositeActivity, IActivityEventListener<ActivityExecutionStatusChangedEventArgs>
    {
        internal static readonly DependencyProperty TransactionOptionsProperty = DependencyProperty.Register("TransactionOptions", typeof(WorkflowTransactionOptions), typeof(TransactionScopeActivity), new PropertyMetadata(DependencyPropertyOptions.Metadata, new Attribute[] { new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Content) }));
        internal static readonly string TransactionScopeActivityIsolationHandle = "A1DAF1E7-E9E7-4df2-B88F-3A92E1D744F2";

        public TransactionScopeActivity()
        {
            this.SetValueBase(TransactionOptionsProperty, new WorkflowTransactionOptions());
        }

        public TransactionScopeActivity(string name)
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
    }
}
