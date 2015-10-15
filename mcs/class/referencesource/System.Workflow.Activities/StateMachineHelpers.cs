#region Using directives

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.Runtime;
using System.Workflow.Runtime.Hosting;

#endregion Using directives

namespace System.Workflow.Activities
{
    internal static class StateMachineHelpers
    {
        internal static bool IsStateMachine(StateActivity state)
        {
            if (state == null)
                throw new ArgumentNullException("state");

            return (state is StateMachineWorkflowActivity);
        }

        internal static bool IsRootState(StateActivity state)
        {
            if (state == null)
                throw new ArgumentNullException("state");

            StateActivity parent = state.Parent as StateActivity;
            return parent == null;
        }

        internal static bool IsLeafState(StateActivity state)
        {
            if (state == null)
                throw new ArgumentNullException("state");

            if (IsRootState(state))
                return false;

            foreach (Activity child in state.EnabledActivities)
            {
                if (child is StateActivity)
                    return false;
            }
            return true;
        }

        internal static bool IsRootExecutionContext(ActivityExecutionContext context)
        {
            return (context.Activity.Parent == null);
        }

        /// <summary>
        /// Finds the enclosing state for this activity
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        internal static StateActivity FindEnclosingState(Activity activity)
        {
            Debug.Assert(activity != null);
            Debug.Assert(activity.Parent != activity);

            StateActivity state = activity as StateActivity;
            if (state != null)
                return state;

            if (activity.Parent == null)
                return null;

            return FindEnclosingState(activity.Parent);
        }

        /// <summary>
        /// Returns the root State activity
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        internal static StateActivity GetRootState(StateActivity state)
        {
            Debug.Assert(state != null);
            Debug.Assert(state.Parent != state);

            if (state.Parent == null)
                return state;

            // this handles the case when the StateMachineWorkflow
            // is called using an Invoke activity
            if (!(state.Parent is StateActivity))
                return state;

            return GetRootState((StateActivity)state.Parent);
        }

        internal static bool IsInitialState(StateActivity state)
        {
            Debug.Assert(state != null);

            string initialStateName = GetInitialStateName(state);
            if (initialStateName == null)
                return false;

            return state.QualifiedName.Equals(initialStateName);
        }

        internal static bool IsCompletedState(StateActivity state)
        {
            Debug.Assert(state != null);

            string completedStateName = GetCompletedStateName(state);
            if (completedStateName == null)
                return false;

            return state.QualifiedName.Equals(completedStateName);
        }

        internal static string GetInitialStateName(StateActivity state)
        {
            StateActivity rootState = GetRootState(state);
            return (string)rootState.GetValue(StateMachineWorkflowActivity.InitialStateNameProperty);
        }

        internal static string GetCompletedStateName(StateActivity state)
        {
            Debug.Assert(state != null);
            StateActivity rootState = GetRootState(state);
            return (string)rootState.GetValue(StateMachineWorkflowActivity.CompletedStateNameProperty);
        }

        /*
        internal static bool IsInInitialStatePath(StateActivity state)
        {
            StateActivity rootState = GetRootState(state);
            string initialStateName = GetInitialStateName(rootState);

            StateActivity initialState = FindStateByName(rootState, initialStateName);
            CompositeActivity current = initialState;
            while (current != null)
            {
                if (current.QualifiedName == state.QualifiedName)
                    return true;
                current = current.Parent;
            }
            return false;
        }
         */

        /// <summary>
        /// Returns the State activity that is currently executing
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        static internal StateActivity GetCurrentState(ActivityExecutionContext context)
        {
            StateActivity state = context.Activity as StateActivity;
            if (state == null)
                state = FindEnclosingState(context.Activity);
            Debug.Assert(state != null, "StateMachineHelpers.GetCurrentState: only valid to call this method from a State executor or a contained EventDriven");
            StateActivity rootState = GetRootState(state);

            StateMachineExecutionState executionState = StateMachineExecutionState.Get(rootState);
            string currentStateName = executionState.CurrentStateName;
            if (currentStateName == null)
                return null;

            StateActivity currentState = FindDynamicStateByName(rootState, currentStateName);
            Debug.Assert(currentState == null || IsLeafState(currentState));
            return currentState;
        }

        static internal StateActivity FindDynamicStateByName(StateActivity state, string stateQualifiedName)
        {
            while (!state.QualifiedName.Equals(stateQualifiedName) && ContainsState(state, stateQualifiedName))
            {
                foreach (Activity activity in state.EnabledActivities)
                {
                    StateActivity childState = activity as StateActivity;
                    if (childState == null)
                        continue;

                    if (ContainsState(childState, stateQualifiedName))
                    {
                        StateActivity dynamicChildState = (StateActivity)state.GetDynamicActivity(childState);
                        if (dynamicChildState == null)
                            return null;
                        state = dynamicChildState;
                        break;
                    }
                }
            }
            if (state.QualifiedName.Equals(stateQualifiedName))
                return state;
            else
                return null;
        }

        static internal StateActivity FindStateByName(StateActivity state, string qualifiedName)
        {
            Debug.Assert(state != null);
            Debug.Assert(qualifiedName != null);
            StateActivity found = FindActivityByName(state, qualifiedName) as StateActivity;
            return found;
        }

        static internal Activity FindActivityByName(CompositeActivity parentActivity, string qualifiedName)
        {
            return parentActivity.GetActivityByName(qualifiedName, true);
        }

        static internal bool ContainsEventActivity(CompositeActivity compositeActivity)
        {
            Debug.Assert(compositeActivity != null);

            Queue<Activity> activities = new Queue<Activity>();
            activities.Enqueue(compositeActivity);
            while (activities.Count > 0)
            {
                Activity activity = activities.Dequeue();
                if (activity is IEventActivity)
                    return true;

                compositeActivity = activity as CompositeActivity;
                if (compositeActivity != null)
                {
                    foreach (Activity child in compositeActivity.Activities)
                    {
                        if (child.Enabled)
                            activities.Enqueue(child);
                    }
                }
            }
            return false;
        }

        static internal IEventActivity GetEventActivity(EventDrivenActivity eventDriven)
        {
            CompositeActivity sequenceActivity = eventDriven as CompositeActivity;
            Debug.Assert(eventDriven.EnabledActivities.Count > 0);
            IEventActivity eventActivity = sequenceActivity.EnabledActivities[0] as IEventActivity;
            Debug.Assert(eventActivity != null);
            return eventActivity;
        }

        static internal EventDrivenActivity GetParentEventDriven(IEventActivity eventActivity)
        {
            Activity activity = ((Activity)eventActivity).Parent;
            while (activity != null)
            {
                EventDrivenActivity eventDriven = activity as EventDrivenActivity;
                if (eventDriven != null)
                    return eventDriven;

                activity = activity.Parent;
            }
            return null;
        }

        static internal bool ContainsState(StateActivity state, string stateName)
        {
            if (state == null)
                throw new ArgumentNullException("state");
            if (String.IsNullOrEmpty(stateName))
                throw new ArgumentNullException("stateName");

            Queue<StateActivity> states = new Queue<StateActivity>();
            states.Enqueue(state);
            while (states.Count > 0)
            {
                state = states.Dequeue();
                if (state.QualifiedName.Equals(stateName))
                    return true;

                foreach (Activity childActivity in state.EnabledActivities)
                {
                    StateActivity childState = childActivity as StateActivity;
                    if (childState != null)
                    {
                        states.Enqueue(childState);
                    }
                }
            }
            return false;
        }
    }

    #region StateMachineMessages
#if DEBUG
    /*
     * this is only used for testing the State Machine related resource messages
     *
    internal class StateMachineMessages
    {
        internal static void PrintMessages()
        {
            Console.WriteLine("GetInvalidUserDataInStateChangeTrackingRecord: {0}\n", SR.GetInvalidUserDataInStateChangeTrackingRecord());
            Console.WriteLine("GetError_EventDrivenInvalidFirstActivity: {0}\n", SR.GetError_EventDrivenInvalidFirstActivity());
            Console.WriteLine("GetError_InvalidLeafStateChild: {0}\n", SR.GetError_InvalidLeafStateChild());
            Console.WriteLine("GetError_InvalidCompositeStateChild: {0}\n", SR.GetError_InvalidCompositeStateChild());
            Console.WriteLine("GetError_SetStateOnlyWorksOnStateMachineWorkflow: {0}\n", SR.GetError_SetStateOnlyWorksOnStateMachineWorkflow());
            Console.WriteLine("GetError_SetStateMustPointToAState: {0}\n", SR.GetError_SetStateMustPointToAState());
            Console.WriteLine("GetError_InitialStateMustPointToAState: {0}\n", SR.GetError_InitialStateMustPointToAState());
            Console.WriteLine("GetError_CompletedStateMustPointToAState: {0}\n", SR.GetError_CompletedStateMustPointToAState());

            Console.WriteLine("GetError_SetStateMustPointToALeafNodeState: {0}\n", SR.GetError_SetStateMustPointToALeafNodeState());
            Console.WriteLine("GetError_InitialStateMustPointToALeafNodeState: {0}\n", SR.GetError_InitialStateMustPointToALeafNodeState());
            Console.WriteLine("GetError_CompletedStateMustPointToALeafNodeState: {0}\n", SR.GetError_CompletedStateMustPointToALeafNodeState());

            Console.WriteLine("GetError_StateInitializationParentNotState: {0}\n", SR.GetError_StateInitializationParentNotState());
            Console.WriteLine("GetError_StateFinalizationParentNotState: {0}\n", SR.GetError_StateFinalizationParentNotState());

            Console.WriteLine("GetError_EventActivityNotValidInStateInitialization: {0}\n", SR.GetError_EventActivityNotValidInStateInitialization());
            Console.WriteLine("GetError_EventActivityNotValidInStateFinalization: {0}\n", SR.GetError_EventActivityNotValidInStateFinalization());

            Console.WriteLine("GetError_MultipleStateInitializationActivities: {0}\n", SR.GetError_MultipleStateInitializationActivities());
            Console.WriteLine("GetError_MultipleStateFinalizationActivities: {0}\n", SR.GetError_MultipleStateFinalizationActivities());

            Console.WriteLine("GetError_InvalidTargetStateInStateInitialization: {0}\n", SR.GetError_InvalidTargetStateInStateInitialization());
            Console.WriteLine("GetError_CantRemoveState: {0}\n", SR.GetError_CantRemoveState());
            Console.WriteLine("GetSqlTrackingServiceRequired: {0}\n", SR.GetSqlTrackingServiceRequired());

            Console.WriteLine("GetStateMachineWorkflowMustHaveACurrentState: {0}\n", SR.GetStateMachineWorkflowMustHaveACurrentState());

            Console.WriteLine("GetInvalidActivityStatus: {0}\n", SR.GetInvalidActivityStatus(new Activity("Hello")));
            Console.WriteLine("GetStateMachineWorkflowRequired: {0}\n", SR.GetStateMachineWorkflowRequired());
            Console.WriteLine("GetError_EventDrivenParentNotListen: {0}\n", SR.GetError_EventDrivenParentNotListen());

            Console.WriteLine("GetGetUnableToTransitionToState: {0}\n", SR.GetUnableToTransitionToState("StateName"));
            Console.WriteLine("GetInvalidStateTransitionPath: {0}\n", SR.GetInvalidStateTransitionPath());
            Console.WriteLine("GetInvalidSetStateInStateInitialization: {0}\n", SR.GetInvalidSetStateInStateInitialization());
            Console.WriteLine("GetStateAlreadySubscribesToThisEvent: {0}\n", SR.GetStateAlreadySubscribesToThisEvent("StateName", "QueueName"));
        }
    }
    */

#endif
    #endregion
}
