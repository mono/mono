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
    using System.Diagnostics.CodeAnalysis;
    using System.ServiceModel;
    using System.Xml;
    using System.ServiceModel.Dispatcher;

    [Serializable]
    internal sealed class ReceiveContext
    {
        static string emptyGuid = Guid.Empty.ToString();
        Guid contextId = Guid.Empty;
        bool initialized = false;
        bool isRootContext = false;
        string name = null;
        SerializableReadOnlyDictionary<string, string> properties = null;
        string workflowId = emptyGuid;

        public ReceiveContext(string name, Guid workflowId, bool isRootContext)
        {
            this.name = name;
            this.workflowId = workflowId.ToString();
            this.isRootContext = isRootContext;
        }

        [Browsable(false)]
        public bool Initialized
        {
            get
            {
                return this.initialized;
            }
        }

        [Browsable(false)]
        public string Name
        {
            get
            {
                return this.name;
            }
        }

        [Browsable(false)]
        internal SerializableReadOnlyDictionary<string, string> Properties
        {
            get
            {
                return this.properties;
            }
        }

        public void EnsureInitialized(Guid contextId)
        {
            if (this.contextId != contextId)
            {
                this.initialized = false;
                this.contextId = contextId;
            }

            if (this.Initialized)
            {
                return;
            }

            if (!isRootContext)
            {
                this.properties =
                    new SerializableReadOnlyDictionary<string, string>(
                    new KeyValuePair<string, string>(WellKnownContextProperties.InstanceId, workflowId),
                    new KeyValuePair<string, string>(WellKnownContextProperties.ConversationId, Guid.NewGuid().ToString()));
            }
            else
            {
                this.properties = new SerializableReadOnlyDictionary<string, string>(
                    new KeyValuePair<string, string>(WellKnownContextProperties.InstanceId, workflowId));
            }

            this.initialized = true;
        }
    }
}
