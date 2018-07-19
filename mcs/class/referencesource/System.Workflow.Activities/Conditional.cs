namespace System.Workflow.Activities
{
    #region Imports

    using System;
    using System.Text;
    using System.Reflection;
    using System.Collections;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Design;
    using System.Workflow.ComponentModel;
    using System.Runtime.Serialization;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.Activities.Common;

    #endregion


    [SRDescription(SR.ConditionalActivityDescription)]
    [ToolboxItem(typeof(IfElseToolboxItem))]
    [Designer(typeof(IfElseDesigner), typeof(IDesigner))]
    [SRCategory(SR.Standard)]
    [ToolboxBitmap(typeof(IfElseActivity), "Resources.Decision.png")]
    [ActivityValidator(typeof(IfElseValidator))]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class IfElseActivity : CompositeActivity, IActivityEventListener<ActivityExecutionStatusChangedEventArgs>
    {

        #region Constructors

        public IfElseActivity()
        {
        }

        public IfElseActivity(string name)
            : base(name)
        {
        }

        #endregion

        public IfElseBranchActivity AddBranch(ICollection<Activity> activities)
        {
            if (activities == null)
                throw new ArgumentNullException("activities");

            return AddBranch(activities, null);
        }

        public IfElseBranchActivity AddBranch(ICollection<Activity> activities, ActivityCondition branchCondition)
        {
            if (activities == null)
                throw new ArgumentNullException("activities");

            if (!this.DesignMode)
                throw new InvalidOperationException(SR.GetString(SR.Error_ConditionalBranchUpdateAtRuntime));

            IfElseBranchActivity branchActivity = new IfElseBranchActivity();

            foreach (Activity activity in activities)
                branchActivity.Activities.Add(activity);

            branchActivity.Condition = branchCondition;
            this.Activities.Add(branchActivity);

            return branchActivity;
        }

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            bool noneTrue = true;

            for (int i = 0; i < this.EnabledActivities.Count; ++i)
            {
                IfElseBranchActivity branch = this.EnabledActivities[i] as IfElseBranchActivity;

                // Else case dont have enable condition so find them and enable it
                if (branch.Condition == null || branch.Condition.Evaluate(branch, executionContext))
                {
                    noneTrue = false;
                    branch.RegisterForStatusChange(Activity.ClosedEvent, this);
                    executionContext.ExecuteActivity(branch);
                    break;
                }
            }

            return noneTrue ? ActivityExecutionStatus.Closed : ActivityExecutionStatus.Executing;
        }

        protected override ActivityExecutionStatus Cancel(ActivityExecutionContext executionContext)
        {
            bool canCloseNow = true;

            for (int i = 0; i < this.EnabledActivities.Count; ++i)
            {
                Activity childBranch = this.EnabledActivities[i];

                if (childBranch.ExecutionStatus == ActivityExecutionStatus.Executing)
                {
                    canCloseNow = false;
                    executionContext.CancelActivity(childBranch);
                    break;
                }
                else if (childBranch.ExecutionStatus == ActivityExecutionStatus.Canceling || childBranch.ExecutionStatus == ActivityExecutionStatus.Faulting)
                {
                    canCloseNow = false;
                    break;
                }
            }
            return canCloseNow ? ActivityExecutionStatus.Closed : ActivityExecutionStatus.Canceling;
        }

        void IActivityEventListener<ActivityExecutionStatusChangedEventArgs>.OnEvent(object sender, ActivityExecutionStatusChangedEventArgs e)
        {
            if (sender == null)
                throw new ArgumentNullException("sender");
            if (e == null)
                throw new ArgumentNullException("e");

            ActivityExecutionContext context = sender as ActivityExecutionContext;

            if (context == null)
                throw new ArgumentException(SR.Error_SenderMustBeActivityExecutionContext, "sender");

            context.CloseActivity();
        }
    }

    internal sealed class IfElseValidator : CompositeActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = base.Validate(manager, obj);

            IfElseActivity ifElse = obj as IfElseActivity;
            if (ifElse == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(IfElseActivity).FullName), "obj");

            // Validate number of children
            if (ifElse.EnabledActivities.Count < 1)
                validationErrors.Add(new ValidationError(SR.GetString(SR.Error_ConditionalLessThanOneChildren), ErrorNumbers.Error_IfElseLessThanOneChildren));

            // all child activities must be IfElse branch
            foreach (Activity activity in ifElse.EnabledActivities)
            {
                if (!(activity is IfElseBranchActivity))
                {
                    validationErrors.Add(new ValidationError(SR.GetString(SR.Error_ConditionalDeclNotAllConditionalBranchDecl), ErrorNumbers.Error_IfElseNotAllIfElseBranchDecl));
                    break;
                }
            }
            return validationErrors;
        }

        public override ValidationError ValidateActivityChange(Activity activity, ActivityChangeAction action)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");
            if (action == null)
                throw new ArgumentNullException("action");

            if (activity.ExecutionStatus != ActivityExecutionStatus.Initialized &&
                 activity.ExecutionStatus != ActivityExecutionStatus.Closed)
            {
                return new ValidationError(SR.GetString(SR.Error_DynamicActivity, activity.QualifiedName, Enum.GetName(typeof(ActivityExecutionStatus), activity.ExecutionStatus)), ErrorNumbers.Error_DynamicActivity);
            }
            return null;
        }
    }
}
