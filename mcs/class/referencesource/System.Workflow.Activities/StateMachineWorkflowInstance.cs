#pragma warning disable 1634, 1691
namespace System.Workflow.Activities
{
    using System;
    using System.Xml.Serialization;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Reflection;
    using System.Workflow.Activities;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Workflow.ComponentModel.Compiler;
    using System.Security.Principal;
    using System.Workflow.Runtime.Tracking;
    using System.Diagnostics;
    using System.Workflow.Runtime;

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class StateMachineWorkflowInstance
    {
        private Guid _instanceId;
        private WorkflowInstance _workflowInstance;
        private SqlTrackingQuery _sqlTrackingQuery;
        private SqlTrackingService _sqlTrackingService;
        private SqlTrackingWorkflowInstance _sqlTrackingWorkflowInstance;
        private StateMachineWorkflowActivity _stateMachineWorkflow;
        private WorkflowRuntime _runtime;

        internal const string StateHistoryPropertyName = "StateHistory";

        public StateMachineWorkflowInstance(WorkflowRuntime runtime, Guid instanceId)
        {
            if (runtime == null)
                throw new ArgumentNullException("runtime");
            if (instanceId == Guid.Empty)
                throw new ArgumentNullException("instanceId");
            _runtime = runtime;
            _instanceId = instanceId;
            _workflowInstance = runtime.GetWorkflow(instanceId);
            _stateMachineWorkflow = _workflowInstance.GetWorkflowDefinition() as StateMachineWorkflowActivity;
            if (_stateMachineWorkflow == null)
                throw new ArgumentException(SR.GetStateMachineWorkflowRequired(), "instanceId");
        }

        public StateMachineWorkflowActivity StateMachineWorkflow
        {
            get
            {
                // we always get a new definition, in case a 
                // dynamic updated happened. The exception handling here
                // is because after the workflow completes, we can no longer
                // retrieve the workflow definition. In this case, we 
                // return the last retrieved definition
                try
                {
                    _stateMachineWorkflow = (StateMachineWorkflowActivity)this.WorkflowInstance.GetWorkflowDefinition();
                }
                catch (InvalidOperationException)
                {
                }

                return _stateMachineWorkflow;
            }
        }

        public Guid InstanceId
        {
            get
            {
                return _instanceId;
            }
        }

        public WorkflowInstance WorkflowInstance
        {
            get
            {
                return _workflowInstance;
            }
        }

        public StateActivity CurrentState
        {
            get
            {
                return GetCurrentState();
            }
        }

        public string CurrentStateName
        {
            get
            {
                StateActivity currentState = this.CurrentState;
                if (currentState == null)
                    return null;
                return currentState.QualifiedName;
            }
        }

        private static ReadOnlyCollection<StateActivity> GetLeafStates(StateActivity parentState)
        {
            if (parentState == null)
                throw new ArgumentNullException("parentState");

            List<StateActivity> leafStates = new List<StateActivity>();
            Queue<StateActivity> states = new Queue<StateActivity>();
            states.Enqueue(parentState);
            while (states.Count > 0)
            {
                StateActivity parent = states.Dequeue();
                foreach (Activity childActivity in parent.EnabledActivities)
                {
                    StateActivity childState = childActivity as StateActivity;
                    if (childState != null)
                    {
                        if (StateMachineHelpers.IsLeafState(childState))
                            leafStates.Add(childState);
                        else
                            states.Enqueue(childState);
                    }
                }
            }
            return leafStates.AsReadOnly();
        }


        public ReadOnlyCollection<StateActivity> States
        {
            get
            {
                StateMachineWorkflowActivity stateMachineWorkflow = this.StateMachineWorkflow;

#pragma warning disable 56503

                if (stateMachineWorkflow == null)
                    throw new InvalidOperationException();

                return GetLeafStates(stateMachineWorkflow);

#pragma warning restore 56503
            }
        }

        public ReadOnlyCollection<string> PossibleStateTransitions
        {
            get
            {
                return GetPossibleStateTransitions();
            }
        }

        public ReadOnlyCollection<string> StateHistory
        {
            get
            {
                return GetStateHistory();
            }
        }

        public void EnqueueItem(IComparable queueName, object item)
        {
            EnqueueItem(queueName, item, null, null);
        }

        public void EnqueueItem(IComparable queueName, object item, IPendingWork pendingWork, object workItem)
        {
            this.WorkflowInstance.EnqueueItemOnIdle(queueName, item, pendingWork, workItem);
        }

        public void SetState(StateActivity targetState)
        {
            if (targetState == null)
                throw new ArgumentNullException("targetState");
            SetState(targetState.QualifiedName);
        }

        public void SetState(string targetStateName)
        {
            if (targetStateName == null)
                throw new ArgumentNullException("targetStateName");
            StateActivity targetState = FindActivityByQualifiedName(targetStateName) as StateActivity;
            if (targetState == null)
                throw new ArgumentOutOfRangeException("targetStateName");
            SetStateEventArgs eventArgs = new SetStateEventArgs(targetStateName);
            this.WorkflowInstance.EnqueueItemOnIdle(System.Workflow.Activities.StateMachineWorkflowActivity.SetStateQueueName, eventArgs, null, null);
        }

        internal Activity FindActivityByQualifiedName(string id)
        {
            return StateMachineHelpers.FindActivityByName(this.StateMachineWorkflow, id);
        }

        private StateActivity GetCurrentState()
        {
            ReadOnlyCollection<WorkflowQueueInfo> workflowQueuedInfos = this.WorkflowInstance.GetWorkflowQueueData();
            foreach (WorkflowQueueInfo queueInfo in workflowQueuedInfos)
            {
                if (queueInfo.QueueName.Equals(StateMachineWorkflowActivity.SetStateQueueName))
                {
                    if (queueInfo.SubscribedActivityNames.Count == 0)
                        return null;
                    Debug.Assert(queueInfo.SubscribedActivityNames.Count == 1);
                    StateMachineWorkflowActivity stateMachineWorkflow = this.StateMachineWorkflow;
                    StateActivity currentState = StateMachineHelpers.FindStateByName(stateMachineWorkflow, queueInfo.SubscribedActivityNames[0]);
                    return currentState;
                }
            }
            return null;
        }


        private ReadOnlyCollection<string> GetPossibleStateTransitions()
        {
            List<string> targetStates = new List<string>();
            ReadOnlyCollection<WorkflowQueueInfo> workflowQueuedInfos = this.WorkflowInstance.GetWorkflowQueueData();
            StateMachineWorkflowActivity stateMachineWorkflow = this.StateMachineWorkflow;
            foreach (WorkflowQueueInfo queueInfo in workflowQueuedInfos)
            {
                foreach (string subscribedActivityName in queueInfo.SubscribedActivityNames)
                {
                    Activity subscribedActivity = StateMachineHelpers.FindActivityByName(stateMachineWorkflow, subscribedActivityName);
                    IEventActivity eventActivity = subscribedActivity as IEventActivity;
                    if (eventActivity == null)
                        continue;

                    EventDrivenActivity eventDriven = StateMachineHelpers.GetParentEventDriven(eventActivity);
                    Debug.Assert(eventDriven != null);
                    Queue<Activity> activities = new Queue<Activity>();
                    activities.Enqueue(eventDriven);
                    while (activities.Count > 0)
                    {
                        Activity activity = activities.Dequeue();
                        SetStateActivity setState = activity as SetStateActivity;
                        if (setState != null)
                        {
                            targetStates.Add(setState.TargetStateName);
                        }
                        else
                        {
                            CompositeActivity compositeActivity = activity as CompositeActivity;
                            if (compositeActivity != null)
                            {
                                foreach (Activity childActivity in compositeActivity.EnabledActivities)
                                {
                                    activities.Enqueue(childActivity);
                                }
                            }
                        }
                    }
                }
            }
            return targetStates.AsReadOnly();
        }

        private ReadOnlyCollection<string> GetStateHistory()
        {
            if (_sqlTrackingService == null)
            {
                _sqlTrackingService = _runtime.GetService<SqlTrackingService>();
                if (_sqlTrackingService == null)
                    throw new InvalidOperationException(SR.GetSqlTrackingServiceRequired());
            }

            if (_sqlTrackingQuery == null)
                _sqlTrackingQuery = new SqlTrackingQuery(_sqlTrackingService.ConnectionString);

            StateMachineWorkflowActivity stateMachineWorkflow;
            Stack<string> stateHistory = new Stack<string>();

            try
            {
                stateMachineWorkflow = this.StateMachineWorkflow;
            }
            catch (InvalidOperationException)
            {
                return new ReadOnlyCollection<string>(stateHistory.ToArray());
            }

            if (_sqlTrackingWorkflowInstance == null)
            {
                bool result = _sqlTrackingQuery.TryGetWorkflow(_instanceId, out _sqlTrackingWorkflowInstance);
                if (!result)
                {
                    // Workflow has not started yet, so we just return an
                    // empty collection
                    return new ReadOnlyCollection<string>(stateHistory.ToArray());
                }
            }

            _sqlTrackingWorkflowInstance.Refresh();
            IList<UserTrackingRecord> events = _sqlTrackingWorkflowInstance.UserEvents;
            foreach (UserTrackingRecord record in events)
            {
                if (record.UserDataKey != StateActivity.StateChangeTrackingDataKey)
                    continue;

                string stateQualifiedName = record.UserData as string;
                if (stateQualifiedName == null)
                    throw new InvalidOperationException(SR.GetInvalidUserDataInStateChangeTrackingRecord());

                StateActivity state = StateMachineHelpers.FindStateByName(stateMachineWorkflow, record.QualifiedName);
                if (state == null)
                    throw new InvalidOperationException(SR.GetInvalidUserDataInStateChangeTrackingRecord());

                if (StateMachineHelpers.IsLeafState(state))
                    stateHistory.Push(stateQualifiedName);
            }

            ReadOnlyCollection<string> history = new ReadOnlyCollection<string>(stateHistory.ToArray());
            return history;
        }
    }
}
