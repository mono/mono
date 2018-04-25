//------------------------------------------------------------------------------
// <copyright file="WorkflowRuntimeServiceSettings.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Configuration;
using System.Collections.Specialized;


namespace System.Workflow.Runtime.Configuration
{
    /// <summary> Configuration element for a WorkflowRuntime service </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class WorkflowRuntimeServiceElement : ConfigurationElement
    {

        /// <summary> Collection of service-specific name-value pairs </summary>
        private NameValueCollection _parameters = new NameValueCollection();

        private const string _type = "type";

        public NameValueCollection Parameters
        {
            get { return _parameters; }
        }

        public WorkflowRuntimeServiceElement()
        {
        }

        /// <summary> The assembly-qualified type name of the service </summary>
        /// <remarks> Type is also used as the collection key in WorkflowRuntimeServiceSettingsCollections </remarks>
        [ConfigurationProperty(_type, DefaultValue = null)]
        public string Type
        {
            get
            {
                return (string)base[_type];
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                base[_type] = value;
            }
        }


        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            _parameters.Add(name, value);
            return true;
        }
    }
}
