namespace System.Workflow.Activities
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Text;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Collections.Generic;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Compiler;
    using System.Drawing.Design;

    #region Class SequentialWorkflow
    [Designer(typeof(SequentialWorkflowDesigner), typeof(IRootDesigner))]
    [Designer(typeof(SequentialWorkflowDesigner), typeof(IDesigner))]
    [ToolboxBitmap(typeof(SequentialWorkflowActivity), "Resources.SequentialWorkflow.bmp")]
    [SRCategory(SR.Standard)]
    [SRDisplayName(SR.SequentialWorkflow)]
    [ToolboxItem(false)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class SequentialWorkflowActivity : SequenceActivity
    {
        #region Dependency Properties
        public static readonly DependencyProperty InitializedEvent = DependencyProperty.Register("Initialized", typeof(EventHandler), typeof(SequentialWorkflowActivity));
        public static readonly DependencyProperty CompletedEvent = DependencyProperty.Register("Completed", typeof(EventHandler), typeof(SequentialWorkflowActivity));
        #endregion

        #region Constructors

        public SequentialWorkflowActivity()
        {
        }

        public SequentialWorkflowActivity(string name)
            : base(name)
        {
        }

        [SRDescription(SR.DynamicUpdateConditionDescr)]
        [SRCategory(SR.Conditions)]
        [DefaultValue(null)]
        public ActivityCondition DynamicUpdateCondition
        {
            get
            {
                return WorkflowChanges.GetCondition(this) as ActivityCondition;
            }
            set
            {
                WorkflowChanges.SetCondition(this, value);
            }
        }
        #endregion

        #region Handlers
        [SRDescription(SR.OnInitializedDescr)]
        [SRCategory(SR.Handlers)]
        [MergableProperty(false)]
        public event EventHandler Initialized
        {
            add
            {
                base.AddHandler(InitializedEvent, value);
            }
            remove
            {
                base.RemoveHandler(InitializedEvent, value);
            }
        }

        [SRDescription(SR.OnCompletedDescr)]
        [SRCategory(SR.Handlers)]
        [MergableProperty(false)]
        public event EventHandler Completed
        {
            add
            {
                base.AddHandler(CompletedEvent, value);
            }
            remove
            {
                base.RemoveHandler(CompletedEvent, value);
            }
        }
        #endregion

        #region Protected Implementations

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            base.RaiseEvent(SequentialWorkflowActivity.InitializedEvent, this, EventArgs.Empty);
            return base.Execute(executionContext);
        }

        protected override sealed void OnSequenceComplete(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            base.RaiseEvent(SequentialWorkflowActivity.CompletedEvent, this, EventArgs.Empty);
        }
        #endregion
    }
    #endregion
}
