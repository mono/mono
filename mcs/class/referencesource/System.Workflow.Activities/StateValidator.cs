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
    using System.Diagnostics;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Runtime.Serialization;
    using System.Collections.Generic;
    using System.ComponentModel.Design.Serialization;
    using System.Xml.Serialization;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.Activities.Common;

    #endregion

    #region StateValidator class

    [System.Runtime.InteropServices.ComVisible(false)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class StateActivityValidator : CompositeActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = new ValidationErrorCollection(base.Validate(manager, obj));

            StateActivity state = obj as StateActivity;
            if (state == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(StateActivity).FullName), "obj");

            // First we validate contaiment
            if (state.Parent != null)
            {
                if (StateMachineHelpers.IsStateMachine(state))
                {
                    validationErrors.Add(new ValidationError(SR.GetError_StateMachineWorkflowMustBeARootActivity(), ErrorNumbers.Error_StateMachineWorkflowMustBeARootActivity));
                    return validationErrors;
                }
                else
                {
                    // Make sure that a State is always contained in
                    // another State or StateMachineWorkflow. State machine
                    // within a sequential workflow is not supported
                    if (!(state.Parent is StateActivity))
                    {
                        validationErrors.Add(new ValidationError(SR.GetError_InvalidStateActivityParent(), ErrorNumbers.Error_InvalidStateActivityParent));
                        return validationErrors;
                    }
                }
            }

            if (state.Parent == null && !StateMachineHelpers.IsStateMachine(state))
            {
                ValidateCustomStateActivity(state, validationErrors);
            }

            if (StateMachineHelpers.IsLeafState(state))
            {
                ValidateLeafState(state, validationErrors);
            }
            else if (StateMachineHelpers.IsRootState(state))
            {
                ValidateRootState(state, validationErrors);
            }
            else
            {
                ValidateState(state, validationErrors);
            }

            ValidateEventDrivenActivities(state, validationErrors);

            return validationErrors;
        }

        private static void ValidateCustomStateActivity(StateActivity state, ValidationErrorCollection validationErrors)
        {
            if (state.Activities.Count > 0)
            {
                validationErrors.Add(new ValidationError(SR.GetError_BlackBoxCustomStateNotSupported(), ErrorNumbers.Error_BlackBoxCustomStateNotSupported));
            }
        }

        public override ValidationError ValidateActivityChange(Activity activity, ActivityChangeAction action)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");
            if (action == null)
                throw new ArgumentNullException("action");

            if (activity.ExecutionStatus != ActivityExecutionStatus.Initialized &&
                activity.ExecutionStatus != ActivityExecutionStatus.Executing &&
                activity.ExecutionStatus != ActivityExecutionStatus.Closed)
            {
                return new ValidationError(SR.GetString(SR.Error_DynamicActivity2, activity.QualifiedName, activity.ExecutionStatus, activity.GetType().FullName), ErrorNumbers.Error_DynamicActivity2);
            }

            RemovedActivityAction remove = action as RemovedActivityAction;
            if (remove != null)
            {
                StateActivity removedState = remove.OriginalRemovedActivity as StateActivity;
                if (removedState != null)
                {
                    // we don't have a way to check if the removed
                    // activity is executing or not, so if the user is trying to 
                    // remove a StateActivity, we simply disallow it.
                    // 

                    return new ValidationError(
                        SR.GetError_CantRemoveState(removedState.QualifiedName),
                        ErrorNumbers.Error_CantRemoveState);
                }

                if (activity.ExecutionStatus == ActivityExecutionStatus.Executing)
                {
                    EventDrivenActivity removedEventDriven = remove.OriginalRemovedActivity as EventDrivenActivity;
                    if (removedEventDriven != null)
                    {
                        return new ValidationError(
                            SR.GetError_CantRemoveEventDrivenFromExecutingState(removedEventDriven.QualifiedName, activity.QualifiedName),
                            ErrorNumbers.Error_CantRemoveEventDrivenFromExecutingState);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        private static void ValidateLeafState(StateActivity state, ValidationErrorCollection validationErrors)
        {
            ValidateLeafStateChildren(state, validationErrors);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        private static void ValidateRootState(StateActivity state, ValidationErrorCollection validationErrors)
        {
            ValidateCompositeStateChildren(state, validationErrors);
            if (StateMachineHelpers.IsStateMachine(state))
            {
                ValidateInitialState(state, validationErrors);
                ValidateCompletedState(state, validationErrors);
            }
        }

        private static void ValidateState(StateActivity state, ValidationErrorCollection validationErrors)
        {
            ValidateCompositeStateChildren(state, validationErrors);
        }

        #region Helpers

        private static void ValidateLeafStateChildren(StateActivity state, ValidationErrorCollection validationErrors)
        {
            bool invalidChild = false;

            foreach (Activity activity in state.Activities)
            {
                if (!activity.Enabled)
                    continue;

                if (activity is EventDrivenActivity ||
                    activity is StateInitializationActivity ||
                    activity is StateFinalizationActivity)
                {
                    continue;
                }
                else
                {
                    invalidChild = true;
                    break;
                }
            }

            // validate that all child activities are event driven activities.
            if (invalidChild)
                validationErrors.Add(new ValidationError(
                    SR.GetError_InvalidLeafStateChild(),
                    ErrorNumbers.Error_InvalidLeafStateChild));
        }

        private static void ValidateCompositeStateChildren(StateActivity state, ValidationErrorCollection validationErrors)
        {
            bool invalidChild = false;

            foreach (Activity activity in state.Activities)
            {
                if (!activity.Enabled)
                    continue;

                if (activity is EventDrivenActivity ||
                    activity is StateActivity)
                {
                    continue;
                }
                else
                {
                    invalidChild = true;
                    break;
                }
            }

            if (invalidChild)
                validationErrors.Add(new ValidationError(
                    SR.GetError_InvalidCompositeStateChild(),
                    ErrorNumbers.Error_InvalidCompositeStateChild));
        }

        private static void ValidateInitialState(StateActivity state, ValidationErrorCollection validationErrors)
        {
            string initialStateName = StateMachineHelpers.GetInitialStateName(state);
            if (String.IsNullOrEmpty(initialStateName))
            {
                if (state.Activities.Count > 0)
                {
                    // we only require an initial state if the state machine is 
                    // not empty
                    validationErrors.Add(new ValidationError(
                        SR.GetString(SR.Error_PropertyNotSet, StateMachineWorkflowActivity.InitialStateNamePropertyName),
                        ErrorNumbers.Error_PropertyNotSet, false,
                        StateMachineWorkflowActivity.InitialStateNamePropertyName));
                }
            }
            else
            {
                StateActivity initialState = StateMachineHelpers.FindStateByName(
                    state,
                    initialStateName);
                if (initialState == null)
                {
                    validationErrors.Add(new ValidationError(
                        SR.GetError_InitialStateMustPointToAState(),
                        ErrorNumbers.Error_InitialStateMustPointToAState,
                        false,
                        StateMachineWorkflowActivity.InitialStateNamePropertyName));
                }
                else
                {
                    if (!StateMachineHelpers.IsLeafState(initialState))
                    {
                        validationErrors.Add(new ValidationError(
                            SR.GetError_InitialStateMustPointToALeafNodeState(),
                            ErrorNumbers.Error_InitialStateMustPointToALeafNodeState,
                            false,
                            StateMachineWorkflowActivity.InitialStateNamePropertyName));
                    }

                    // InitialState cannot be the completed state
                    string completedStateName = StateMachineHelpers.GetCompletedStateName(state);
                    if (initialStateName == completedStateName)
                    {
                        validationErrors.Add(new ValidationError(
                            SR.GetError_InitialStateMustBeDifferentThanCompletedState(),
                            ErrorNumbers.Error_InitialStateMustBeDifferentThanCompletedState,
                            false,
                            StateMachineWorkflowActivity.InitialStateNamePropertyName));
                    }
                }
            }
        }

        private static void ValidateCompletedState(StateActivity state, ValidationErrorCollection validationErrors)
        {
            string completedStateName = StateMachineHelpers.GetCompletedStateName(state);
            if (!String.IsNullOrEmpty(completedStateName))
            {
                StateActivity completedState = StateMachineHelpers.FindStateByName(
                    state,
                    completedStateName);
                if (completedState == null)
                {
                    validationErrors.Add(new ValidationError(
                        SR.GetError_CompletedStateMustPointToAState(),
                        ErrorNumbers.Error_CompletedStateMustPointToAState,
                        false,
                        StateMachineWorkflowActivity.CompletedStateNamePropertyName));
                }
                else
                {
                    if (StateMachineHelpers.IsLeafState(completedState))
                    {
                        if (completedState.EnabledActivities.Count > 0)
                            validationErrors.Add(new ValidationError(
                                SR.GetString(SR.Error_CompletedStateCannotContainActivities),
                                ErrorNumbers.Error_CompletedStateCannotContainActivities,
                                false,
                                StateMachineWorkflowActivity.CompletedStateNamePropertyName));
                    }
                    else
                    {
                        validationErrors.Add(new ValidationError(
                            SR.GetError_CompletedStateMustPointToALeafNodeState(),
                            ErrorNumbers.Error_CompletedStateMustPointToALeafNodeState,
                            false,
                            StateMachineWorkflowActivity.CompletedStateNamePropertyName));
                    }
                }
            }
        }

        private static void ValidateEventDrivenActivities(StateActivity state, ValidationErrorCollection validationErrors)
        {
            List<EventDrivenActivity> eventDrivenList = new List<EventDrivenActivity>();

            foreach (Activity activity in state.EnabledActivities)
            {
                EventDrivenActivity eventDriven = activity as EventDrivenActivity;
                if (eventDriven != null)
                {
                    eventDrivenList.Add(eventDriven);
                }
            }
            foreach (EventDrivenActivity eventDriven in eventDrivenList)
            {
                bool result = ValidateMultipleIEventActivity(eventDriven, validationErrors);
                if (!result)
                    break;
            }
        }

        private static bool ValidateMultipleIEventActivity(EventDrivenActivity eventDriven, ValidationErrorCollection validationErrors)
        {
            IEventActivity firstEventActivity = null;
            if (eventDriven.EnabledActivities.Count > 0)
            {
                firstEventActivity = eventDriven.EnabledActivities[0] as IEventActivity;
            }

            return ValidateMultipleIEventActivityInCompositeActivity(eventDriven, firstEventActivity, eventDriven, validationErrors);
        }

        private static bool ValidateMultipleIEventActivityInCompositeActivity(EventDrivenActivity eventDriven, IEventActivity firstEventActivity, CompositeActivity parent, ValidationErrorCollection validationErrors)
        {
            foreach (Activity activity in parent.Activities)
            {
                // Skip disabled activities or the first IEventActivity
                // Note that we don't use EnabledActivities because we want to 
                // enforce this rule inside Cancellation and Exception Handlers.
                if (!activity.Enabled || activity == firstEventActivity)
                    continue;

                if (activity is IEventActivity)
                {
                    validationErrors.Add(new ValidationError(
                        SR.GetString(SR.Error_EventDrivenMultipleEventActivity, eventDriven.Name, typeof(IEventActivity).FullName, typeof(EventDrivenActivity).Name),
                        ErrorNumbers.Error_EventDrivenMultipleEventActivity));
                    return false;
                }
                else
                {
                    CompositeActivity compositeActivity = activity as CompositeActivity;
                    if (compositeActivity != null)
                    {
                        bool result = ValidateMultipleIEventActivityInCompositeActivity(eventDriven, firstEventActivity, compositeActivity, validationErrors);
                        if (!result)
                            return false;
                    }
                }
            }
            return true;
        }

        #endregion
    }

    #endregion StateValidator class
}
