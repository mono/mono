#pragma warning disable 1634, 1691
using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using System.Transactions;
using SES = System.EnterpriseServices;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime.Hosting;

namespace System.Workflow.Runtime
{
    #region ContextActivityUtils Class

    internal static class ContextActivityUtils
    {
        internal static int ContextId(Activity activity)
        {
            return ((ActivityExecutionContextInfo)ContextActivity(activity).GetValue(Activity.ActivityExecutionContextInfoProperty)).ContextId;
        }

        internal static Activity ContextActivity(Activity activity)
        {
            Activity contextActivity = activity;
            while (contextActivity != null && contextActivity.GetValue(Activity.ActivityExecutionContextInfoProperty) == null)
                contextActivity = contextActivity.Parent;
            return contextActivity;
        }
        internal static Activity ParentContextActivity(Activity activity)
        {
            Activity contextActivity = ContextActivity(activity);
            ActivityExecutionContextInfo executionContextInfo = (ActivityExecutionContextInfo)contextActivity.GetValue(Activity.ActivityExecutionContextInfoProperty);
            if (executionContextInfo.ParentContextId == -1)
                return null;

            return RetrieveWorkflowExecutor(activity).GetContextActivityForId(executionContextInfo.ParentContextId);
        }
        internal static IWorkflowCoreRuntime RetrieveWorkflowExecutor(Activity activity)
        {
            // fetch workflow executor
            IWorkflowCoreRuntime workflowExecutor = null;
            Activity rootActivity = activity;
            while (rootActivity != null && rootActivity.Parent != null)
                rootActivity = rootActivity.Parent;
            if (rootActivity != null)
                workflowExecutor = (IWorkflowCoreRuntime)rootActivity.GetValue(WorkflowExecutor.WorkflowExecutorProperty);

            return workflowExecutor;
        }
        internal static Activity RootContextActivity(Activity activity)
        {
            return RetrieveWorkflowExecutor(activity).RootActivity;
        }
    }
    #endregion
}
