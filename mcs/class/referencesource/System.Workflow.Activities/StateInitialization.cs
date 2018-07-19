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

    [SRDescription(SR.StateInitializationActivityDescription)]
    [Designer(typeof(StateInitializationDesigner), typeof(IDesigner))]
    [ToolboxItem(typeof(ActivityToolboxItem))]
    [ToolboxBitmap(typeof(StateInitializationActivity), "Resources.StateInitializationActivity.png")]
    [ActivityValidator(typeof(StateInitializationValidator))]
    [SRCategory(SR.Standard)]
    [System.Runtime.InteropServices.ComVisible(false)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class StateInitializationActivity : SequenceActivity
    {
        public StateInitializationActivity()
        {
        }

        public StateInitializationActivity(string name)
            : base(name)
        {
        }
    }

    [System.Runtime.InteropServices.ComVisible(false)]
    internal sealed class StateInitializationValidator : CompositeActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = base.Validate(manager, obj);

            StateInitializationActivity stateInitialization = obj as StateInitializationActivity;
            if (stateInitialization == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(StateInitializationActivity).FullName), "obj");

            StateActivity state = stateInitialization.Parent as StateActivity;
            if (state == null)
            {
                validationErrors.Add(new ValidationError(SR.GetError_StateInitializationParentNotState(), ErrorNumbers.Error_StateHandlerParentNotState));
                return validationErrors;
            }

            foreach (Activity activity in state.EnabledActivities)
            {
                StateInitializationActivity childStateInitialization = activity as StateInitializationActivity;
                if (childStateInitialization != null)
                {
                    if (childStateInitialization == stateInitialization)
                        continue;

                    validationErrors.Add(new ValidationError(
                        SR.GetError_MultipleStateInitializationActivities(), ErrorNumbers.Error_MultipleStateInitializationActivities));
                    break;
                }
            }

            ValidateSetStateInsideStateInitialization(stateInitialization, state, validationErrors);

            if (StateMachineHelpers.ContainsEventActivity(stateInitialization))
            {
                validationErrors.Add(new ValidationError(SR.GetError_EventActivityNotValidInStateInitialization(), ErrorNumbers.Error_EventActivityNotValidInStateHandler));
            }

            return validationErrors;
        }

        private void ValidateSetStateInsideStateInitialization(StateInitializationActivity stateInitialization, StateActivity state, ValidationErrorCollection validationErrors)
        {
            ValidateSetStateInsideStateInitializationCore(stateInitialization, state, validationErrors);
        }

        private void ValidateSetStateInsideStateInitializationCore(CompositeActivity compositeActivity, StateActivity state, ValidationErrorCollection validationErrors)
        {
            foreach (Activity activity in compositeActivity.EnabledActivities)
            {
                CompositeActivity childCompositeActivity = activity as CompositeActivity;
                if (childCompositeActivity != null)
                {
                    ValidateSetStateInsideStateInitializationCore(childCompositeActivity, state, validationErrors);
                }
                else
                {
                    SetStateActivity setState = activity as SetStateActivity;
                    if (setState != null)
                    {
                        if (!String.IsNullOrEmpty(setState.TargetStateName))
                        {
                            if (setState.TargetStateName.Equals(state.QualifiedName))
                            {
                                validationErrors.Add(new ValidationError(
                                    SR.GetError_InvalidTargetStateInStateInitialization(), ErrorNumbers.Error_InvalidTargetStateInStateInitialization));
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}
