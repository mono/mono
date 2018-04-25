namespace System.Workflow.Activities
{
    using System;
    using System.Text;
    using System.Reflection;
    using System.Collections;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Security;
    using System.Security.Permissions;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Collections.Generic;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.Activities.Common;

    [SRDescription(SR.SetStateActivityDescription)]
    [ToolboxItem(typeof(ActivityToolboxItem))]
    [Designer(typeof(SetStateDesigner), typeof(IDesigner))]
    [ToolboxBitmap(typeof(SetStateActivity), "Resources.SetStateActivity.png")]
    [ActivityValidator(typeof(SetStateValidator))]
    [SRCategory(SR.Standard)]
    [System.Runtime.InteropServices.ComVisible(false)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class SetStateActivity : Activity
    {
        internal const string TargetStateNamePropertyName = "TargetStateName";

        //metadata property
        public static readonly DependencyProperty TargetStateNameProperty = DependencyProperty.Register(TargetStateNamePropertyName, typeof(string), typeof(SetStateActivity), new PropertyMetadata("", DependencyPropertyOptions.Metadata, new ValidationOptionAttribute(ValidationOption.Optional)));

        #region Constructors

        public SetStateActivity()
        {
        }

        public SetStateActivity(string name)
            : base(name)
        {
        }

        #endregion
        [SRDescription(SR.TargetStateDescription)]
        [Editor(typeof(StateDropDownEditor), typeof(UITypeEditor))]
        [DefaultValue((string)null)]
        public string TargetStateName
        {
            get
            {
                return base.GetValue(TargetStateNameProperty) as string;
            }
            set
            {
                base.SetValue(TargetStateNameProperty, value);
            }
        }

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            StateActivity parentState = StateMachineHelpers.FindEnclosingState(executionContext.Activity);
            StateActivity rootState = StateMachineHelpers.GetRootState(parentState);
            StateMachineExecutionState executionState = StateMachineExecutionState.Get(rootState);
            executionState.NextStateName = this.TargetStateName;
            return ActivityExecutionStatus.Closed;
        }
    }

    [System.Runtime.InteropServices.ComVisible(false)]
    internal sealed class SetStateValidator : ActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = new ValidationErrorCollection(base.Validate(manager, obj));

            SetStateActivity setState = obj as SetStateActivity;
            if (setState == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(StateActivity).FullName), "obj");

            if (!SetStateContainment.Validate(setState, validationErrors))
                return validationErrors; // could not find a valid parent

            if (String.IsNullOrEmpty(setState.TargetStateName))
            {
                validationErrors.Add(new ValidationError(
                    SR.GetString(SR.Error_PropertyNotSet, SetStateActivity.TargetStateNamePropertyName),
                    ErrorNumbers.Error_PropertyNotSet, false,
                    SetStateActivity.TargetStateNamePropertyName));
            }
            else
            {
                StateActivity enclosingState = StateMachineHelpers.FindEnclosingState(setState);
                Debug.Assert(enclosingState != null); // this should be caught by the SetStateContainment.Validate call above

                StateActivity rootState = StateMachineHelpers.GetRootState(enclosingState);

                StateActivity targetActivity = StateMachineHelpers.FindStateByName(
                    rootState,
                    setState.TargetStateName);
                StateActivity targetState = targetActivity as StateActivity;
                if (targetState == null)
                {
                    validationErrors.Add(new ValidationError(SR.GetError_SetStateMustPointToAState(), ErrorNumbers.Error_SetStateMustPointToAState, false, SetStateActivity.TargetStateNamePropertyName));
                }
                else
                {
                    if (!StateMachineHelpers.IsLeafState(targetState))
                    {
                        validationErrors.Add(new ValidationError(SR.GetError_SetStateMustPointToALeafNodeState(), ErrorNumbers.Error_SetStateMustPointToALeafNodeState, false, SetStateActivity.TargetStateNamePropertyName));
                    }
                }
            }

            return validationErrors;
        }

        #region SetStateContainement

        private class SetStateContainment
        {
            private bool validParentFound = true;
            private bool validParentStateFound;

            private SetStateContainment()
            {
            }

            public static bool Validate(SetStateActivity setState, ValidationErrorCollection validationErrors)
            {
                SetStateContainment containment = new SetStateContainment();
                ValidateContainment(containment, setState);

                if (!containment.validParentFound ||
                    !containment.validParentStateFound)
                {
                    validationErrors.Add(new ValidationError(SR.GetError_SetStateOnlyWorksOnStateMachineWorkflow(), ErrorNumbers.Error_SetStateOnlyWorksOnStateMachineWorkflow));
                    return false;
                }
                return true;
            }

            private static void ValidateContainment(SetStateContainment containment, Activity activity)
            {
                Debug.Assert(activity != null);
                if (activity.Parent == null || activity.Parent == activity)
                {
                    containment.validParentFound = false;
                    return;
                }

                if (SetStateValidator.IsValidContainer(activity.Parent))
                {
                    ValidateParentState(containment, activity.Parent);
                    return;
                }

                ValidateContainment(containment, activity.Parent);
            }

            private static void ValidateParentState(SetStateContainment containment, CompositeActivity activity)
            {
                Debug.Assert(activity != null);
                if (activity.Parent == null)
                    return;

                StateActivity state = activity.Parent as StateActivity;
                if (state != null)
                {
                    containment.validParentStateFound = true;
                    return;
                }

                ValidateParentState(containment, activity.Parent);
            }
        }

        #endregion

        #region Helper methods
        static internal bool IsValidContainer(CompositeActivity activity)
        {
            return (activity is EventDrivenActivity || activity is StateInitializationActivity);
        }
        #endregion
    }
}
