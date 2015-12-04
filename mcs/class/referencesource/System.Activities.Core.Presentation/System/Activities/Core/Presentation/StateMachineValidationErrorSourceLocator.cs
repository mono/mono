// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation.Validation;
    using System.Activities.Statements;
    using System.Collections.Generic;
    using System.Runtime;

    internal class StateMachineValidationErrorSourceLocator : IValidationErrorSourceLocator
    {
        public List<object> FindSourceDetailFromActivity(Activity errorSource, object errorSourceDetail)
        {
            if (errorSourceDetail == null)
            {
                return new List<object> { errorSource };
            }
            else
            {
                return FindRelativePath((StateMachine)errorSource, errorSourceDetail);
            }
        }

        // case 1: StateMachine -> Default expression of StateMachine's variable -> ...
        // case 2: StateMachine -> InternalState -> ...
        public void ReplaceParentChainWithSource(Activity parentActivity, List<object> parentChain)
        {
            Activity lastActivity = parentChain[parentChain.Count - 1] as Activity;
            StateMachine stateMachine = (StateMachine)parentActivity;

            foreach (Variable variable in stateMachine.Variables)
            {
                if (variable != null && variable.Default == lastActivity)
                {
                    parentChain.Add(stateMachine);
                    return;
                }
            }

            if (parentChain.Count > 1)
            {
                // assume lastActivity is InternalState

                // remove InternalState
                parentChain.RemoveAt(parentChain.Count - 1);

                Activity targetActivity = (Activity)parentChain[parentChain.Count - 1];

                // the targetActivity will be available in the path
                parentChain.RemoveAt(parentChain.Count - 1);

                List<object> path = FindRelativePath(stateMachine, targetActivity);

                foreach (object pathObject in path)
                {
                    parentChain.Add(pathObject);
                }
            }
        }

        private static List<object> FindRelativePath(StateMachine machine, object descendent)
        {
            List<object> path = FindDescendentFromStateMachine(machine, descendent);
            path.Reverse();
            return path;
        }

        private static List<object> FindDescendentFromStateMachine(StateMachine machine, object descendent)
        {
            List<object> path = new List<object>();
            path.Add(machine);
            foreach (State state in machine.States)
            {
                if (state == descendent)
                {
                    break;
                }
                else if (state.Entry == descendent)
                {
                    path.Add(state);
                    break;
                }
                else if (state.Exit == descendent)
                {
                    path.Add(state);
                    break;
                }
                else
                {
                    Transition foundTransition = null;
                    bool transitionAlone = false;
                    foreach (Transition transition in state.Transitions)
                    {
                        foundTransition = transition;
                        if (transition == descendent)
                        {
                            transitionAlone = true;
                            break;
                        }
                        else if (transition.Trigger == descendent)
                        {
                            break;
                        }
                        else if (transition.Action == descendent)
                        {
                            break;
                        }
                        else if (transition.Condition == descendent)
                        {
                            break;
                        }
                        else
                        {
                            foundTransition = null;
                        }
                    }

                    if (foundTransition != null)
                    {
                        path.Add(state);
                        if (!transitionAlone)
                        {
                            path.Add(foundTransition);
                        }

                        break;
                    }

                    bool isVariableError = false;
                    foreach (Variable variable in state.Variables)
                    {
                        if (variable.Default == descendent)
                        {
                            isVariableError = true;
                        }
                    }

                    if (isVariableError)
                    {
                        path.Add(state);
                        break;
                    }
                }
            }

            path.Add(descendent);
            return path;
        }       
    }
}
