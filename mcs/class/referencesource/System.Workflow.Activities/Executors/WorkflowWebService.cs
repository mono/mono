/*******************************************************************************
// Copyright (C) 2000-2006 Microsoft Corporation.  All rights reserved.
 * ****************************************************************************/
#region Using directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Web;
using System.Collections.Specialized;
using System.Threading;
using System.Web.Services;
using System.Workflow.Runtime;
using System.Workflow.Runtime.Hosting;
using System.Security.Permissions;
using System.Security.Principal;
using System.Reflection;
#endregion

namespace System.Workflow.Activities
{
    /// <summary>
    /// Abstract WorkflowWebService Base class for all the Workflow's Web Service.
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public abstract class WorkflowWebService : WebService
    {
        Type workflowType;
        /// <summary>
        /// Protected Constructor for the Workflow Web Service.
        /// </summary>
        /// <param name="workflowType"></param>        
        protected WorkflowWebService(Type workflowType)
        {
            this.workflowType = workflowType;
        }
        protected Object[] Invoke(Type interfaceType, String methodName, bool isActivation, Object[] parameters)
        {
            Guid workflowInstanceId = GetWorkflowInstanceId(ref isActivation);
            WorkflowInstance wfInstance;

            EventQueueName key = new EventQueueName(interfaceType, methodName);

            MethodInfo mInfo = interfaceType.GetMethod(methodName);

            bool responseRequired = (mInfo.ReturnType != typeof(void));

            if (!responseRequired)
            {
                foreach (ParameterInfo parameter in mInfo.GetParameters())
                {
                    if (parameter.ParameterType.IsByRef || parameter.IsOut)
                    {
                        responseRequired = true;
                        break;
                    }
                }
            }

            MethodMessage methodMessage = PrepareMessage(interfaceType, methodName, parameters, responseRequired);

            EventHandler<WorkflowTerminatedEventArgs> workflowTerminationHandler = null;
            EventHandler<WorkflowCompletedEventArgs> workflowCompletedHandler = null;

            try
            {
                if (isActivation)
                {
                    wfInstance = WorkflowRuntime.CreateWorkflow(this.workflowType, null, workflowInstanceId);
                    SafeEnqueueItem(wfInstance, key, methodMessage);
                    wfInstance.Start();
                }
                else
                {
                    wfInstance = WorkflowRuntime.GetWorkflow(workflowInstanceId);
                    SafeEnqueueItem(wfInstance, key, methodMessage);
                }

                bool workflowTerminated = false;

                //Handler for workflow termination in b/w outstanding req-response.
                workflowTerminationHandler = delegate(Object sender, WorkflowTerminatedEventArgs e)
                {
                    if (e.WorkflowInstance.InstanceId.Equals(workflowInstanceId))
                    {
                        methodMessage.SendException(e.Exception);
                        workflowTerminated = true;
                    }
                };

                workflowCompletedHandler = delegate(Object sender, WorkflowCompletedEventArgs e)
                {
                    if (e.WorkflowInstance.InstanceId.Equals(workflowInstanceId))
                    {
                        methodMessage.SendException(new ApplicationException(SR.GetString(System.Globalization.CultureInfo.CurrentCulture, SR.Error_WorkflowCompleted)));
                    }
                };

                WorkflowRuntime.WorkflowTerminated += workflowTerminationHandler;
                WorkflowRuntime.WorkflowCompleted += workflowCompletedHandler;

                ManualWorkflowSchedulerService scheduler = WorkflowRuntime.GetService<ManualWorkflowSchedulerService>();

                if (scheduler != null)
                {
                    scheduler.RunWorkflow(wfInstance.InstanceId);
                }

                if (!responseRequired)
                {
                    // no ret, out or ref
                    return new Object[] { };
                }

                IMethodResponseMessage response = methodMessage.WaitForResponseMessage();

                if (response.Exception != null)
                {
                    if (!workflowTerminated)
                        throw response.Exception;
                    else
                        throw new ApplicationException(SR.GetString(System.Globalization.CultureInfo.CurrentCulture, SR.Error_WorkflowTerminated), response.Exception);
                }

                if (response.OutArgs != null)
                    return ((ArrayList)response.OutArgs).ToArray();
                else
                    return new Object[] { };
            }
            finally
            {
                if (workflowTerminationHandler != null)
                    WorkflowRuntime.WorkflowTerminated -= workflowTerminationHandler;

                if (workflowCompletedHandler != null)
                    WorkflowRuntime.WorkflowCompleted -= workflowCompletedHandler;
            }
        }
        protected WorkflowRuntime WorkflowRuntime
        {
            get
            {
                if (HttpContext.Current != null)
                    return WorkflowWebService.CurrentWorkflowRuntime;
                return null;
            }
        }

        #region Static Helpers
        private static Guid GetWorkflowInstanceId(ref bool isActivation)
        {
            Guid workflowInstanceId = Guid.Empty;

            Object instanceId = HttpContext.Current.Items["__WorkflowInstanceId__"];

            if (instanceId == null && !isActivation)
                throw new InvalidOperationException(SR.GetString(SR.Error_NoInstanceInSession));

            if (instanceId != null)
            {
                workflowInstanceId = (Guid)instanceId;

                Object isActivationContext = HttpContext.Current.Items["__IsActivationContext__"];

                if (isActivationContext != null)
                    isActivation = (bool)isActivationContext;
                else
                    isActivation = false;
            }
            else if (isActivation)
            {
                workflowInstanceId = Guid.NewGuid();
                HttpContext.Current.Items["__WorkflowInstanceId__"] = workflowInstanceId;
            }
            return workflowInstanceId;
        }
        private static MethodMessage PrepareMessage(Type interfaceType, String operation, object[] parameters, bool responseRequired)
        {
            // construct IMethodMessage object
            String securityIdentifier = null;
            IIdentity identity = System.Threading.Thread.CurrentPrincipal.Identity;
            WindowsIdentity windowsIdentity = identity as WindowsIdentity;
            if (windowsIdentity != null && windowsIdentity.User != null)
                securityIdentifier = windowsIdentity.User.Translate(typeof(NTAccount)).ToString();
            else if (identity != null)
                securityIdentifier = identity.Name;

            MethodMessage msg = new MethodMessage(interfaceType, operation, parameters, securityIdentifier, responseRequired);
            return msg;
        }
        //Back - off logic for conflicting workflow load across workflow runtime boundaries.
        static void SafeEnqueueItem(WorkflowInstance instance, EventQueueName key, MethodMessage message)
        {
            while (true) //When Execution times out ASP.NET going to forcefully plung this request.
            {
                try
                {
                    instance.EnqueueItem(key, message, null, null);
                    return;
                }
                catch (WorkflowOwnershipException)
                {
                    WorkflowActivityTrace.Activity.TraceEvent(TraceEventType.Warning, 0, String.Format(System.Globalization.CultureInfo.InvariantCulture, "Workflow Web Host Encountered Workflow Instance Ownership conflict for instanceid {0}.", instance.InstanceId));
                    //Back-off for 1/2 sec. Should we make this configurable?
                    System.Threading.Thread.Sleep(500);
                    continue;
                }
            }
        }
        #endregion

        #region Singleton WorkflowRuntime Accessor
        internal const string ConfigSectionName = "WorkflowRuntime";

        // Double-checked locking pattern requires volatile for read/write synchronization
        static volatile WorkflowRuntime wRuntime;
        static Object wRuntimeSync = new Object();

        internal static WorkflowRuntime CurrentWorkflowRuntime
        {
            get
            {
                if (wRuntime == null)
                {
                    lock (wRuntimeSync)
                    {
                        if (wRuntime == null)
                        {
                            WorkflowRuntime workflowRuntimeTemp = new WorkflowRuntime(ConfigSectionName);
                            try
                            {
                                workflowRuntimeTemp.StartRuntime();
                            }
                            catch
                            {
                                workflowRuntimeTemp.Dispose();
                                throw;
                            }

                            wRuntime = workflowRuntimeTemp;
                        }
                    }
                }
                return wRuntime;
            }
        }
        #endregion
    }
}
