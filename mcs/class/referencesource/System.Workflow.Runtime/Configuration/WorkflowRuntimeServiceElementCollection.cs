//------------------------------------------------------------------------------
// <copyright file="WorkflowRuntimeServiceSettingsCollection.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Configuration;


namespace System.Workflow.Runtime.Configuration
{
    /// <summary> Collection of WorkflowRuntimeServiceSettings used by WorkflowRuntimeSection </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class WorkflowRuntimeServiceElementCollection : ConfigurationElementCollection
    {
        /// <summary> Creates a new WorkflowRuntimeServiceSettings object </summary>
        /// <returns> An empty WorkflowRuntimeServiceSettings </returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new WorkflowRuntimeServiceElement();
        }

        /// <summary> Returns the Type of the WorkflowRuntimeServiceSettings object </summary>
        /// <param name="settings"> The WorkflowRuntimeServiceSettings </param>
        /// <returns> The Type name of the WorkflowRuntimeServiceSettings </returns>
        protected override object GetElementKey(ConfigurationElement settings)
        {
            return ((WorkflowRuntimeServiceElement)settings).Type;
        }

        /// <summary> Adds a WorkflowRuntimeServiceSettings object to this collection </summary>
        /// <param name="settings"> The settings object to add </param>
        public void Add(WorkflowRuntimeServiceElement serviceSettings)
        {
            if (serviceSettings == null)
                throw new ArgumentNullException("serviceSettings");

            base.BaseAdd(serviceSettings);
        }
    }
}

