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
    using System.Workflow.Activities.Common;

    #endregion

    [SRDescription(SR.StateFinalizationActivityDescription)]
    [Designer(typeof(StateFinalizationDesigner), typeof(IDesigner))]
    [ToolboxItem(typeof(ActivityToolboxItem))]
    [ToolboxBitmap(typeof(StateFinalizationActivity), "Resources.StateFinalizationActivity.png")]
    [ActivityValidator(typeof(StateFinalizationValidator))]
    [SRCategory(SR.Standard)]
    [System.Runtime.InteropServices.ComVisible(false)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class StateFinalizationActivity : SequenceActivity
    {
        public StateFinalizationActivity()
        {
        }

        public StateFinalizationActivity(string name)
            : base(name)
        {
        }
    }

    [System.Runtime.InteropServices.ComVisible(false)]
    internal sealed class StateFinalizationValidator : CompositeActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = base.Validate(manager, obj);

            StateFinalizationActivity stateFinalization = obj as StateFinalizationActivity;
            if (stateFinalization == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(StateFinalizationActivity).FullName), "obj");

            StateActivity state = stateFinalization.Parent as StateActivity;
            if (state == null)
            {
                validationErrors.Add(new ValidationError(SR.GetError_StateFinalizationParentNotState(), ErrorNumbers.Error_StateHandlerParentNotState));
                return validationErrors;
            }

            foreach (Activity activity in state.EnabledActivities)
            {
                StateFinalizationActivity childStateFinalization = activity as StateFinalizationActivity;
                if (childStateFinalization != null)
                {
                    if (childStateFinalization == stateFinalization)
                        continue;

                    validationErrors.Add(new ValidationError(
                        SR.GetError_MultipleStateFinalizationActivities(), ErrorNumbers.Error_MultipleStateFinalizationActivities));
                    break;
                }
            }

            if (StateMachineHelpers.ContainsEventActivity(stateFinalization))
            {
                validationErrors.Add(new ValidationError(SR.GetError_EventActivityNotValidInStateFinalization(), ErrorNumbers.Error_EventActivityNotValidInStateHandler));
            }

            return validationErrors;
        }
    }
}
