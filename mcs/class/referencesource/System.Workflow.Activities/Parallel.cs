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
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Runtime.Serialization;
    using System.Collections.Generic;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.Runtime.DebugEngine;
    using System.Workflow.Activities.Common;

    #endregion

    [SRDescription(SR.ParallelActivityDescription)]
    [ToolboxItem(typeof(ParallelToolboxItem))]
    [ToolboxBitmap(typeof(ParallelActivity), "Resources.Parallel.png")]
    [Designer(typeof(ParallelDesigner), typeof(IDesigner))]
    [ActivityValidator(typeof(ParallelValidator))]
    [SRCategory(SR.Standard)]
    [WorkflowDebuggerSteppingAttribute(WorkflowDebuggerSteppingOption.Concurrent)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class ParallelActivity : CompositeActivity, IActivityEventListener<ActivityExecutionStatusChangedEventArgs>
    {
        #region Constructors

        public ParallelActivity()
        {
        }

        public ParallelActivity(string name)
            : base(name)
        {
        }

        #endregion

        #region Protected Methods
        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            //Tomark that execute method is called for this activity.
            this.IsExecuting = true;

            for (int i = 0; i < this.EnabledActivities.Count; ++i)
            {
                Activity childActivity = this.EnabledActivities[i];
                childActivity.RegisterForStatusChange(Activity.ClosedEvent, this);
                executionContext.ExecuteActivity(childActivity);
            }

            return (this.EnabledActivities.Count == 0) ? ActivityExecutionStatus.Closed : ActivityExecutionStatus.Executing;
        }
        protected override ActivityExecutionStatus Cancel(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            bool canCloseNow = true;

            for (int i = 0; i < this.EnabledActivities.Count; ++i)
            {
                Activity childActivity = this.EnabledActivities[i];

                if (childActivity.ExecutionStatus == ActivityExecutionStatus.Executing)
                {
                    executionContext.CancelActivity(childActivity);
                    canCloseNow = false;
                }
                else if (childActivity.ExecutionStatus == ActivityExecutionStatus.Canceling || childActivity.ExecutionStatus == ActivityExecutionStatus.Faulting)
                {
                    canCloseNow = false;
                }
            }

            return canCloseNow ? ActivityExecutionStatus.Closed : ActivityExecutionStatus.Canceling;
        }
        protected override void OnClosed(IServiceProvider provider)
        {
            base.RemoveProperty(ParallelActivity.IsExecutingProperty);
        }



        protected override void OnActivityChangeAdd(ActivityExecutionContext executionContext, Activity addedActivity)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");
            if (addedActivity == null)
                throw new ArgumentNullException("addedActivity");

            ParallelActivity parallel = executionContext.Activity as ParallelActivity;

            if (parallel.ExecutionStatus == ActivityExecutionStatus.Executing && parallel.IsExecuting)
            {
                addedActivity.RegisterForStatusChange(Activity.ClosedEvent, this);
                executionContext.ExecuteActivity(addedActivity);
            }
        }

        protected override void OnActivityChangeRemove(ActivityExecutionContext rootExecutionContext, Activity removedActivity)
        {

        }

        protected override void OnWorkflowChangesCompleted(ActivityExecutionContext executionContext)
        {
            base.OnWorkflowChangesCompleted(executionContext);

            if (this.IsExecuting)
            {
                bool canCloseNow = true;

                for (int i = 0; i < this.EnabledActivities.Count; ++i)
                {
                    Activity childActivity = this.EnabledActivities[i];
                    if (childActivity.ExecutionStatus != ActivityExecutionStatus.Closed)
                    {
                        canCloseNow = false;
                        break;
                    }
                }

                if (canCloseNow)
                    executionContext.CloseActivity();
            }
        }
        #endregion

        #region IActivityEventListener<ActivityExecutionStatusChangedEventArgs> Members
        void IActivityEventListener<ActivityExecutionStatusChangedEventArgs>.OnEvent(object sender, ActivityExecutionStatusChangedEventArgs e)
        {
            if (sender == null)
                throw new ArgumentNullException("sender");
            if (e == null)
                throw new ArgumentNullException("e");

            ActivityExecutionContext context = sender as ActivityExecutionContext;

            if (context == null)
                throw new ArgumentException(SR.Error_SenderMustBeActivityExecutionContext, "sender");

            ParallelActivity parallel = context.Activity as ParallelActivity;

            e.Activity.UnregisterForStatusChange(Activity.ClosedEvent, this);

            bool canCloseNow = true;

            for (int i = 0; i < parallel.EnabledActivities.Count; ++i)
            {
                Activity childActivity = parallel.EnabledActivities[i];
                if (!(childActivity.ExecutionStatus == ActivityExecutionStatus.Initialized || childActivity.ExecutionStatus == ActivityExecutionStatus.Closed))
                {
                    canCloseNow = false;
                    break;
                }
            }

            if (canCloseNow)
                context.CloseActivity();
        }
        #endregion

        #region Runtime Specific Data
        //Runtime Properties
        static DependencyProperty IsExecutingProperty = DependencyProperty.Register("IsExecuting", typeof(bool), typeof(ParallelActivity), new PropertyMetadata(false));

        private bool IsExecuting
        {
            get
            {
                return (bool)base.GetValue(IsExecutingProperty);
            }
            set
            {
                base.SetValue(IsExecutingProperty, value);
            }
        }
        #endregion
    }

    #region Validator
    internal sealed class ParallelValidator : CompositeActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = base.Validate(manager, obj);

            ParallelActivity parallel = obj as ParallelActivity;
            if (parallel == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(ParallelActivity).FullName), "obj");

            // Validate number of children
            if (parallel.EnabledActivities.Count < 2)
                validationErrors.Add(new ValidationError(SR.GetString(SR.Error_ParallelLessThanTwoChildren), ErrorNumbers.Error_ParallelLessThanTwoChildren));

            bool notAllSequence = false;

            foreach (Activity activity in parallel.EnabledActivities)
            {
                if (activity.GetType() != typeof(SequenceActivity))
                {
                    notAllSequence = true;
                }
            }

            // Validate that all child activities are sequence activities.
            if (notAllSequence)
                validationErrors.Add(new ValidationError(SR.GetString(SR.Error_ParallelNotAllSequence), ErrorNumbers.Error_ParallelNotAllSequence));

            return validationErrors;
        }
    }
    #endregion
}
