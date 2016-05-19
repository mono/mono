//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.Workflow.Runtime.Configuration;
    using System.Diagnostics.CodeAnalysis;

    // Legacy WF V1 configuration extension
    [SuppressMessage("Configuration", "Configuration100")]
    [SuppressMessage("Configuration", "Configuration101")]
    [ConfigurationCollection(typeof(WorkflowRuntimeServiceElement))]
    [Obsolete("The WF3 types are deprecated.  Instead, please use the new WF4 types from System.Activities.*")]
    public class ExtendedWorkflowRuntimeServiceElementCollection : WorkflowRuntimeServiceElementCollection
    {
        public ExtendedWorkflowRuntimeServiceElementCollection()
            : base()
        {
            // empty
        }

        public void Remove(WorkflowRuntimeServiceElement serviceSettings)
        {
            base.BaseRemove(base.GetElementKey(serviceSettings));
        }

        public void Remove(string key)
        {
            base.BaseRemove(key);
        }
    }
}
