namespace System.Workflow.Activities
{
    using System;
    using System.Text;
    using System.Reflection;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Diagnostics;
    using System.IO;
    using System.Windows.Forms;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Runtime.Serialization;

    internal partial class StateDesigner : FreeformActivityDesigner
    {
        internal class TransitionInfo
        {
            private SetStateActivity _setState;
            private CompositeActivity _eventHandler;
            private StateActivity _targetState;
            private StateDesignerConnector _connector;

            internal TransitionInfo(SetStateActivity setState, CompositeActivity eventHandler)
            {
                if (setState == null)
                    throw new ArgumentNullException("setState");
                if (eventHandler == null)
                    throw new ArgumentNullException("eventHandler");
                _setState = setState;
                _eventHandler = eventHandler;
            }

            internal SetStateActivity SetState
            {
                get
                {
                    return _setState;
                }
            }

            internal CompositeActivity EventHandler
            {
                get
                {
                    return _eventHandler;
                }
            }

            internal StateActivity SourceState
            {
                get
                {
                    if (_eventHandler == null)
                        return null;

                    StateActivity sourceState = _eventHandler.Parent as StateActivity;
                    return sourceState;
                }
            }

            internal StateActivity TargetState
            {
                get
                {
                    return _targetState;
                }
                set
                {
                    _targetState = value;
                }
            }

            internal StateDesignerConnector Connector
            {
                get
                {
                    return _connector;
                }
                set
                {
                    _connector = value;
                }
            }

            internal bool Matches(StateDesignerConnector stateDesignerConnector)
            {
                if (stateDesignerConnector == null)
                    throw new ArgumentNullException("stateDesignerConnector");

                if (this.Connector != null &&
                    this.Connector == stateDesignerConnector)
                    return true;

                // this transitioninfo is incomplete,
                // therefore, it cannot match an existing connector
                if (this.SetState == null ||
                    this.SourceState == null ||
                    this.TargetState == null ||
                    this.EventHandler == null)
                    return false;

                if (this.SetState.QualifiedName != stateDesignerConnector.SetStateName)
                    return false;

                if (this.SourceState.QualifiedName != stateDesignerConnector.SourceStateName)
                    return false;

                if (this.TargetState.QualifiedName != stateDesignerConnector.TargetStateName ||
                    stateDesignerConnector.Target.AssociatedDesigner.Activity.QualifiedName != stateDesignerConnector.TargetStateName)
                    return false;

                if (this.EventHandler.QualifiedName != stateDesignerConnector.EventHandlerName)
                    return false;

                return true;
            }

            internal static ReadOnlyCollection<TransitionInfo> ParseStateMachine(StateActivity rootState)
            {
                List<TransitionInfo> transitions = new List<TransitionInfo>();
                Dictionary<string, StateActivity> states = new Dictionary<string, StateActivity>();
                Queue<StateActivity> processingQueue = new Queue<StateActivity>();
                processingQueue.Enqueue(rootState);
                while (processingQueue.Count > 0)
                {
                    StateActivity state = processingQueue.Dequeue();
                    states[state.QualifiedName] = state;
                    foreach (Activity childActivity in state.Activities)
                    {
                        StateActivity childState = childActivity as StateActivity;
                        if (childState == null)
                        {
                            CompositeActivity compositeChild = childActivity as CompositeActivity;
                            if (compositeChild != null)
                                ParseEventHandler(compositeChild, transitions);
                        }
                        else
                        {
                            processingQueue.Enqueue(childState);
                        }
                    }
                }

                foreach (TransitionInfo transitionInfo in transitions)
                {
                    StateActivity targetState;
                    string targetStateName = transitionInfo.SetState.TargetStateName;
                    if (!String.IsNullOrEmpty(targetStateName))
                    {
                        states.TryGetValue(transitionInfo.SetState.TargetStateName, out targetState);
                        transitionInfo.TargetState = targetState;
                    }
                }

                return transitions.AsReadOnly();
            }

            private static void ParseEventHandler(CompositeActivity eventHandler, List<TransitionInfo> transitions)
            {
                Queue<Activity> processingQueue = new Queue<Activity>();
                processingQueue.Enqueue(eventHandler);
                while (processingQueue.Count > 0)
                {
                    Activity activity = processingQueue.Dequeue();
                    SetStateActivity setState = activity as SetStateActivity;
                    if (setState != null)
                    {
                        TransitionInfo transitionInfo = new TransitionInfo(setState, eventHandler);
                        transitions.Add(transitionInfo);
                    }
                    else
                    {
                        CompositeActivity compositeActivity = activity as CompositeActivity;
                        if (compositeActivity != null)
                        {
                            foreach (Activity childActivity in compositeActivity.Activities)
                            {
                                processingQueue.Enqueue(childActivity);
                            }
                        }
                    }
                }
            }
        }
    }
}
