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
    using System.Drawing;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Collections.Generic;
    using System.Workflow.ComponentModel.Compiler;

    #endregion

    [SRDescription(SR.SequenceActivityDescription)]
    [ToolboxItem(typeof(ActivityToolboxItem))]
    [Designer(typeof(SequenceDesigner), typeof(IDesigner))]
    [ToolboxBitmap(typeof(SequenceActivity), "Resources.Sequence.png")]
    [SRCategory(SR.Standard)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class SequenceActivity : CompositeActivity, IActivityEventListener<ActivityExecutionStatusChangedEventArgs>
    {
        #region Constructors

        public SequenceActivity()
        {
        }

        public SequenceActivity(string name)
            : base(name)
        {
        }

        #endregion
        private static readonly DependencyProperty SequenceFaultingProperty = DependencyProperty.Register("SequenceFaulting", typeof(bool), typeof(SequenceActivity));
        private static readonly DependencyProperty ActiveChildQualifiedNameProperty = DependencyProperty.Register("ActiveChildQualifiedName", typeof(string), typeof(SequenceActivity));

        [NonSerialized]
        private bool activeChildRemovedInDynamicUpdate = false;

        #region Protected Methods
        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            if (this.EnabledActivities.Count == 0)
            {
                OnSequenceComplete(executionContext);
                return ActivityExecutionStatus.Closed;
            }
            else
            {
                this.EnabledActivities[0].RegisterForStatusChange(Activity.ClosedEvent, this);
                executionContext.ExecuteActivity(this.EnabledActivities[0]);
                this.SetValue(ActiveChildQualifiedNameProperty, this.EnabledActivities[0].QualifiedName);
                return ActivityExecutionStatus.Executing;
            }
        }
        protected override ActivityExecutionStatus Cancel(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            for (int i = (this.EnabledActivities.Count - 1); i >= 0; i--)
            {
                Activity childActivity = this.EnabledActivities[i];

                if (childActivity.ExecutionStatus == ActivityExecutionStatus.Executing)
                {
                    executionContext.CancelActivity(childActivity);
                    return ActivityExecutionStatus.Canceling;
                }

                if (childActivity.ExecutionStatus == ActivityExecutionStatus.Canceling || childActivity.ExecutionStatus == ActivityExecutionStatus.Faulting)
                    return ActivityExecutionStatus.Canceling;

                if (childActivity.ExecutionStatus == ActivityExecutionStatus.Closed)
                {
                    this.RemoveProperty(ActiveChildQualifiedNameProperty);
                    return ActivityExecutionStatus.Closed;
                }
            }

            return ActivityExecutionStatus.Closed;
        }

        protected override ActivityExecutionStatus HandleFault(ActivityExecutionContext executionContext, Exception exception)
        {
            this.SetValue(SequenceFaultingProperty, true);

            ActivityExecutionStatus executionStatus = base.HandleFault(executionContext, exception);

            if (executionStatus == ActivityExecutionStatus.Closed)
                this.RemoveProperty(SequenceFaultingProperty);

            return executionStatus;
        }
        #endregion

        #region IActivityEventListener<ActivityStatusChangeEventArgs> Implementation
        void IActivityEventListener<ActivityExecutionStatusChangedEventArgs>.OnEvent(Object sender, ActivityExecutionStatusChangedEventArgs e)
        {
            if (sender == null)
                throw new ArgumentNullException("sender");
            if (e == null)
                throw new ArgumentNullException("e");

            ActivityExecutionContext context = sender as ActivityExecutionContext;
            if (context == null)
                throw new ArgumentException(SR.Error_SenderMustBeActivityExecutionContext, "sender");

            e.Activity.UnregisterForStatusChange(Activity.ClosedEvent, this);

            SequenceActivity sequenceActivity = context.Activity as SequenceActivity;

            if (sequenceActivity == null)
                throw new ArgumentException("sender");

            if (sequenceActivity.ExecutionStatus == ActivityExecutionStatus.Canceling ||
                (sequenceActivity.ExecutionStatus == ActivityExecutionStatus.Faulting) &&
                (bool)this.GetValue(SequenceFaultingProperty))
            {
                if (sequenceActivity.ExecutionStatus == ActivityExecutionStatus.Faulting)
                    this.RemoveProperty(SequenceFaultingProperty);

                this.RemoveProperty(ActiveChildQualifiedNameProperty);
                context.CloseActivity();
            }
            else if (sequenceActivity.ExecutionStatus == ActivityExecutionStatus.Executing)
            {
                if (!TryScheduleNextChild(context))
                {
                    OnSequenceComplete(context);
                    context.CloseActivity();
                }
            }
        }
        #endregion

        #region Private Implementation
        private bool TryScheduleNextChild(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            IList<Activity> seqList = this.EnabledActivities;

            if (seqList.Count == 0)
                return false;

            //Find index of next activity to run.
            int indexOfNextActivity = 0;
            for (int i = (seqList.Count - 1); i >= 0; i--)
            {
                if (seqList[i].ExecutionStatus == ActivityExecutionStatus.Closed)
                {
                    //Check whether this is last child?
                    if (i == (seqList.Count - 1))
                        return false;

                    indexOfNextActivity = i + 1;
                    break;
                }
            }

            seqList[indexOfNextActivity].RegisterForStatusChange(Activity.ClosedEvent, this);
            executionContext.ExecuteActivity(seqList[indexOfNextActivity]);
            this.SetValue(ActiveChildQualifiedNameProperty, seqList[indexOfNextActivity].QualifiedName);
            return true;
        }


        protected virtual void OnSequenceComplete(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            this.RemoveProperty(ActiveChildQualifiedNameProperty);
        }
        #endregion

        #region Dynamic Update Handler
        protected override void OnActivityChangeRemove(ActivityExecutionContext executionContext, Activity removedActivity)
        {
            String activeChildQualifiedName = this.GetValue(ActiveChildQualifiedNameProperty) as String;

            if (removedActivity.QualifiedName.Equals(activeChildQualifiedName))
                activeChildRemovedInDynamicUpdate = true;

            base.OnActivityChangeRemove(executionContext, removedActivity);
        }
        protected override void OnWorkflowChangesCompleted(ActivityExecutionContext executionContext)
        {
            String activeChildQualifiedName = this.GetValue(ActiveChildQualifiedNameProperty) as String;

            if (activeChildQualifiedName != null && activeChildRemovedInDynamicUpdate)
            {   //We have our active child removed.    
                if (this.ExecutionStatus == ActivityExecutionStatus.Canceling ||
                   (this.ExecutionStatus == ActivityExecutionStatus.Faulting) &&
                   (bool)this.GetValue(SequenceFaultingProperty))
                {
                    if (this.ExecutionStatus == ActivityExecutionStatus.Faulting)
                        this.RemoveProperty(SequenceFaultingProperty);

                    this.RemoveProperty(ActiveChildQualifiedNameProperty);
                    executionContext.CloseActivity();
                }
                else if (this.ExecutionStatus == ActivityExecutionStatus.Executing)
                {
                    if (!TryScheduleNextChild(executionContext))
                    {
                        OnSequenceComplete(executionContext);
                        executionContext.CloseActivity();
                    }
                }
            }
            activeChildRemovedInDynamicUpdate = false;
            base.OnWorkflowChangesCompleted(executionContext);
        }
        #endregion
    }
}
