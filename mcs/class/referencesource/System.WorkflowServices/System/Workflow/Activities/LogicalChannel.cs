//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Activities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Diagnostics.CodeAnalysis;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Serialization;
    using System.Xml;

    [Serializable]
    internal sealed class LogicalChannel
    {
        string configurationName = string.Empty;
        IDictionary<string, string> context = SerializableReadOnlyDictionary<string, string>.Empty;
        Type contractType = null;
        string customAddress = null;
        bool initialized = false;

        Guid instanceId;
        string name = null;

        public LogicalChannel()
        {
            this.instanceId = Guid.NewGuid();
        }

        public LogicalChannel(string name, Type contractType)
        {
            this.instanceId = Guid.NewGuid();
            this.name = name;
            this.contractType = contractType;
        }

        [DefaultValue(null)]
        [Browsable(false)]
        public string ConfigurationName
        {
            get
            {
                return this.configurationName;
            }
        }

        [Browsable(false)]
        public IDictionary<string, string> Context
        {
            get
            {
                return this.context;
            }
            set
            {
                if (value != null)
                {
                    this.context = new ContextDictionary(value);
                }
                else
                {
                    this.context = SerializableReadOnlyDictionary<string, string>.Empty;
                }
            }
        }

        [DefaultValue(null)]
        [Browsable(false)]
        public Type ContractType
        {
            get
            {
                return this.contractType;
            }
        }

        [DefaultValue(null)]
        [Browsable(false)]
        public string CustomAddress
        {
            get
            {
                return this.customAddress;
            }
        }

        [DefaultValue(false)]
        [Browsable(false)]
        public bool Initialized
        {
            get
            {
                return this.initialized;
            }
        }

        [DefaultValue(null)]
        [Browsable(false)]
        public string Name
        {
            get
            {
                return this.name;
            }
        }

        internal Guid InstanceId
        {
            get
            {
                return this.instanceId;
            }
        }

        public void Initialize(string configurationName, string customAddress)
        {
            if (this.Initialized)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR2.GetString(SR2.Error_LogicalChannelAlreadyInitialized, this.Name)));
            }
            this.configurationName = configurationName ?? string.Empty;
            this.customAddress = customAddress;
            this.initialized = true;
        }
    }
}
