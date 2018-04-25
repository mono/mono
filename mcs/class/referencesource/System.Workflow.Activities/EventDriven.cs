namespace System.Workflow.Activities
{
    #region Imports

    using System;
    using System.Text;
    using System.Reflection;
    using System.Collections;
    using System.Collections.Generic;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Compiler;
    using System.Collections.ObjectModel;
    using System.Workflow.Activities.Common;

    #endregion

    [SRDescription(SR.EventDrivenActivityDescription)]
    [Designer(typeof(EventDrivenDesigner), typeof(IDesigner))]
    [ToolboxItem(typeof(ActivityToolboxItem))]
    [ToolboxBitmap(typeof(EventDrivenActivity), "Resources.EventDriven.png")]
    [ActivityValidator(typeof(EventDrivenValidator))]
    [SRCategory(SR.Standard)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class EventDrivenActivity : SequenceActivity
    {
        public EventDrivenActivity()
        {
        }

        public EventDrivenActivity(string name)
            : base(name)
        {
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IEventActivity EventActivity
        {
            get
            {
                ReadOnlyCollection<Activity> enabledActivities = this.EnabledActivities;
                if (enabledActivities.Count == 0)
                    return null;
                else
                    return enabledActivities[0] as IEventActivity;
            }
        }
    }

    internal sealed class EventDrivenValidator : CompositeActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = base.Validate(manager, obj);

            EventDrivenActivity eventDriven = obj as EventDrivenActivity;
            if (eventDriven == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(EventDrivenActivity).FullName), "obj");

            // check parent 
            if (!(eventDriven.Parent is ListenActivity) &&
                !(eventDriven.Parent is EventHandlersActivity) &&
                !(eventDriven.Parent is StateActivity)
                )
                validationErrors.Add(new ValidationError(SR.GetError_EventDrivenParentNotListen(), ErrorNumbers.Error_EventDrivenParentNotListen));

            // validate Event property
            string message = string.Empty;
            int errorNumber = -1;
            Activity firstActivity = (eventDriven.EnabledActivities.Count > 0) ? eventDriven.EnabledActivities[0] : null;
            if (firstActivity == null)
            {
                message = SR.GetString(SR.Error_EventDrivenNoFirstActivity);
                errorNumber = ErrorNumbers.Error_EventDrivenNoFirstActivity;
            }
            else if (!(firstActivity is IEventActivity))
            {
                message = SR.GetError_EventDrivenInvalidFirstActivity();
                errorNumber = ErrorNumbers.Error_EventDrivenInvalidFirstActivity;
            }
            if (message.Length > 0)
                validationErrors.Add(new ValidationError(message, errorNumber));

            return validationErrors;
        }

        public override ValidationError ValidateActivityChange(Activity activity, ActivityChangeAction action)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");
            if (action == null)
                throw new ArgumentNullException("action");

            RemovedActivityAction removedAction = action as RemovedActivityAction;
            if (removedAction != null && removedAction.RemovedActivityIndex == 0)
            {
                return new ValidationError(SR.GetString(SR.Error_EventActivityIsImmutable), ErrorNumbers.Error_DynamicActivity, false);
            }
            else
            {
                AddedActivityAction addedAction = action as AddedActivityAction;

                if (addedAction != null && addedAction.Index == 0)
                    return new ValidationError(SR.GetString(SR.Error_EventActivityIsImmutable), ErrorNumbers.Error_DynamicActivity, false);
            }
            return base.ValidateActivityChange(activity, action);
        }
    }
}
