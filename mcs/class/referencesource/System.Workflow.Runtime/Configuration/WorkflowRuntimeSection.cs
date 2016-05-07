//------------------------------------------------------------------------------
// <copyright file="WorkflowRuntimeSection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Configuration;
using System.Collections.Specialized;

using System.Workflow.Runtime;

namespace System.Workflow.Runtime.Configuration
{
    /// <summary> Configuration settings for the WorkflowRuntime </summary>
    /// <remarks><para>
    /// Services that are automatically instantiated must implement one of the 
    /// following constructors:
    /// <code>
    /// public MyService();
    /// public MyService(NameValueCollection);
    /// public MyService(WorkflowRuntime);
    /// public MyService(WorkflowRuntime, NameValueCollection);
    /// </code>
    /// </para></remarks>
    /// <see cref="System.Workflow.Runtime.Hosting.WorkflowRuntime"/>
    /// <see cref="System.Workflow.Runtime.Hosting.WorkflowRuntimeServiceSettings"/>
    /// <see cref="System.Workflow.Runtime.Hosting.WorkflowRuntimeServiceSettingsCollection"/>
    /// <see cref="System.Configuration.ConfigurationSection"/>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class WorkflowRuntimeSection : ConfigurationSection
    {
        private const string _services = "Services";
        private const string commonParametersSectionName = "CommonParameters";
        private const string _name = "Name";
        private const string _validateOnCreate = "ValidateOnCreate";
        private const string _enablePerfCounters = "EnablePerformanceCounters";
        private const string _definitionCacheCapacity = "WorkflowDefinitionCacheCapacity";

        internal const string DefaultSectionName = "WorkflowRuntime";

        /// <summary> The capacity of WorkflowDefinition cache </summary>
        [ConfigurationProperty(_definitionCacheCapacity, DefaultValue = 0)]
        public int WorkflowDefinitionCacheCapacity
        {
            get
            {
                return (int)base[_definitionCacheCapacity];
            }
            set
            {
                base[_definitionCacheCapacity] = value;
            }
        }

        /// <summary> The name of the service container </summary>
        [ConfigurationProperty(_name, DefaultValue = "")]
        public string Name
        {
            get
            {
                return (string)base[_name];
            }
            set
            {
                base[_name] = value;
            }
        }

        [ConfigurationProperty(_validateOnCreate, DefaultValue = true)]
        public bool ValidateOnCreate
        {
            get
            {
                return (bool)base[_validateOnCreate];
            }
            set
            {
                base[_validateOnCreate] = value;
            }
        }

        [ConfigurationProperty(_enablePerfCounters, DefaultValue = true)]
        public bool EnablePerformanceCounters
        {
            get
            {
                return (bool)base[_enablePerfCounters];
            }
            set
            {
                base[_enablePerfCounters] = value;
            }
        }


        /// <summary> The providers to be instantiated by the service container. </summary>
        [ConfigurationProperty(_services, DefaultValue = null)]
        public WorkflowRuntimeServiceElementCollection Services
        {
            get
            {
                return (WorkflowRuntimeServiceElementCollection)base[_services];
            }
        }

        /// <summary> The resources to be shared by the services. </summary>
        [ConfigurationProperty(WorkflowRuntimeSection.commonParametersSectionName, DefaultValue = null)]
        public NameValueConfigurationCollection CommonParameters
        {
            get
            {
                return (NameValueConfigurationCollection)base[WorkflowRuntimeSection.commonParametersSectionName];
            }
        }
    }
}
