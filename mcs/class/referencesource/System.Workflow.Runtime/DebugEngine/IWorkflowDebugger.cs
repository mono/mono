// Copyright (c) Microsoft Corp., 2004. All rights reserved.
#region Using directives

using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Workflow.Runtime;
using System.Workflow.Runtime.Hosting;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Design;
using System.Workflow.ComponentModel.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Configuration;
using System.Security.Permissions;
using System.Globalization;
using Microsoft.Win32;
using System.Security.AccessControl;
using System.Security.Principal;
#endregion

namespace System.Workflow.Runtime.DebugEngine
{
    #region Interface IWorkflowDebugger

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public interface IWorkflowDebugger
    {
        void InstanceCreated(Guid programId, Guid instanceId, Guid scheduleTypeId);
        void InstanceDynamicallyUpdated(Guid programId, Guid instanceId, Guid scheduleTypeId);
        void InstanceCompleted(Guid programId, Guid instanceId);
        void BeforeActivityStatusChanged(Guid programId, Guid scheduleTypeId, Guid instanceId, string activityQualifiedName, string hierarchicalActivityId, ActivityExecutionStatus status, int stateReaderId);
        void ActivityStatusChanged(Guid programId, Guid scheduleTypeId, Guid instanceId, string activityQualifiedName, string hierarchicalActivityId, ActivityExecutionStatus status, int stateReaderId);
        void SetInitialActivityStatus(Guid programId, Guid scheduleTypeId, Guid instanceId, string activityQualifiedName, string hierarchicalActivityId, ActivityExecutionStatus status, int stateReaderId);
        void ScheduleTypeLoaded(Guid programId, Guid scheduleTypeId, string assemblyFullName, string fileName, string md5Digest, bool isDynamic, string scheduleNamespace, string scheduleName, string workflowMarkup);
        void UpdateHandlerMethodsForActivity(Guid programId, Guid scheduleTypeId, string activityQualifiedName, List<ActivityHandlerDescriptor> handlerMethods);
        void AssemblyLoaded(Guid programId, string assemblyPath, bool fromGlobalAssemblyCache);
        void HandlerInvoked(Guid programId, Guid instanceId, int threadId, string activityQualifiedName);
        void BeforeHandlerInvoked(Guid programId, Guid scheduleTypeId, string activityQualifiedName, ActivityHandlerDescriptor handlerMethod);
    }

    #endregion



}
