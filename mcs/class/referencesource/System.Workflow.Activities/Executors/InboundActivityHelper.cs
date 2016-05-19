#region Using directives

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Workflow.Runtime.Hosting;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime;

#endregion

namespace System.Workflow.Activities
{
    internal static class InboundActivityHelper
    {
        internal static ActivityExecutionStatus ExecuteForActivity(HandleExternalEventActivity activity, ActivityExecutionContext context, Type interfaceType, string operation, out object[] args)
        {
            WorkflowQueuingService queueSvcs = (WorkflowQueuingService)context.GetService(typeof(WorkflowQueuingService));
            args = null;
            IComparable queueName = CorrelationService.ResolveQueueName(activity, interfaceType, operation);
            if (queueName != null)
            {
                WorkflowQueue queue;
                object message = DequeueMessage(queueName, queueSvcs, activity, out queue);
                CorrelationService.UninitializeFollowers(interfaceType, operation, queue);
                if (message != null)
                {
                    args = ProcessEvent(activity, context, message, interfaceType, operation);
                    return ActivityExecutionStatus.Closed;
                }
            }

            return ActivityExecutionStatus.Executing;
        }

        internal static object DequeueMessage(IComparable queueId, WorkflowQueuingService queueSvcs, Activity activity, out WorkflowQueue queue)
        {
            object message = null;
            queue = queueSvcs.GetWorkflowQueue(queueId);

            if (queue.Count != 0)
            {
                message = queue.Dequeue();
                if (message == null)
                    throw new ArgumentException(SR.GetString(SR.Error_InvalidEventMessage, activity.QualifiedName));
            }
            return message;
        }

        private static object[] ProcessEvent(HandleExternalEventActivity activity, ActivityExecutionContext context, object msg, Type interfaceType, string operation)
        {
            IMethodMessage message = msg as IMethodMessage;
            if (message == null)
            {
                Exception excp = msg as Exception;
                if (excp != null)
                    throw excp;
                throw new InvalidOperationException(SR.GetString(SR.Error_InvalidLocalServiceMessage));
            }

            CorrelationService.InvalidateCorrelationToken(activity, interfaceType, operation, message.Args);

            IdentityContextData identityData =
                (IdentityContextData)message.LogicalCallContext.GetData(IdentityContextData.IdentityContext);
            ValidateRoles(activity, identityData.Identity);

            if (ProcessEventParameters(activity.ParameterBindings, message, interfaceType, operation))
                return message.Args;

            return null;
        }

        private static bool ProcessEventParameters(WorkflowParameterBindingCollection parameters, IMethodMessage message, Type interfaceType, string operation)
        {
            bool isKnownSignature = false;
            if (parameters == null)
                return isKnownSignature;

            EventInfo eventInfo = interfaceType.GetEvent(operation);
            MethodInfo methodInfo = eventInfo.EventHandlerType.GetMethod("Invoke");
            int index = 0;

            foreach (ParameterInfo formalParameter in methodInfo.GetParameters())
            {
                if ((typeof(ExternalDataEventArgs).IsAssignableFrom(formalParameter.ParameterType)))
                {
                    if (index == 1)
                        isKnownSignature = true;
                }

                if (parameters.Contains(formalParameter.Name))
                {
                    WorkflowParameterBinding binding = parameters[formalParameter.Name];
                    binding.Value = message.Args[index];
                }
                index++;
            }
            return isKnownSignature;
        }

        internal static void ValidateRoles(Activity activity, string identity)
        {
            DependencyProperty dependencyProperty = DependencyProperty.FromName("Roles", activity.GetType().BaseType);
            if (dependencyProperty == null)
                dependencyProperty = DependencyProperty.FromName("Roles", activity.GetType());

            if (dependencyProperty == null)
                return;

            ActivityBind rolesBind = activity.GetBinding(dependencyProperty) as ActivityBind;
            if (rolesBind == null)
                return;

            WorkflowRoleCollection roles = rolesBind.GetRuntimeValue(activity) as WorkflowRoleCollection;
            if (roles == null)
                return;

            if (!roles.IncludesIdentity(identity))
                throw new WorkflowAuthorizationException(activity.Name, identity);
        }
    }

}
