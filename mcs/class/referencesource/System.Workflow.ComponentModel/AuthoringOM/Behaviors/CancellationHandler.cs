namespace System.Workflow.ComponentModel
{
    using System;
    using System.Drawing;
    using System.ComponentModel;
    using System.Collections;
    using System.ComponentModel.Design;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Compiler;


    [ToolboxItem(false)]
    [Designer(typeof(CancellationHandlerActivityDesigner), typeof(IDesigner))]
    [ToolboxBitmap(typeof(CancellationHandlerActivity), "Resources.CancellationHandler.bmp")]
    [ActivityValidator(typeof(CancellationHandlerValidator))]
    [AlternateFlowActivity]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class CancellationHandlerActivity : CompositeActivity, IActivityEventListener<ActivityExecutionStatusChangedEventArgs>
    {
        public CancellationHandlerActivity()
        {
        }

        public CancellationHandlerActivity(string name)
            : base(name)
        {
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
    }

    internal sealed class CancellationHandlerValidator : CompositeActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = base.Validate(manager, obj);

            CancellationHandlerActivity cancellationHandlerActivity = obj as CancellationHandlerActivity;
            if (cancellationHandlerActivity == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(CancellationHandlerActivity).FullName), "obj");

            if (cancellationHandlerActivity.EnabledActivities.Count == 0)
                validationErrors.Add(new ValidationError(SR.GetString(SR.Warning_EmptyBehaviourActivity, typeof(CancellationHandlerActivity).FullName, cancellationHandlerActivity.QualifiedName), ErrorNumbers.Warning_EmptyBehaviourActivity, true));

            // cancellation handlers can not contain fault handlers, compensation handler and cancellation handler
            if (((ISupportAlternateFlow)cancellationHandlerActivity).AlternateFlowActivities.Count > 0)
                validationErrors.Add(new ValidationError(SR.GetString(SR.Error_ModelingConstructsCanNotContainModelingConstructs), ErrorNumbers.Error_ModelingConstructsCanNotContainModelingConstructs));

            return validationErrors;
        }
    }
}
