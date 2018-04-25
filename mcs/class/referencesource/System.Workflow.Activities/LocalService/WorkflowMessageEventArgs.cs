//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

#region Using directives

using System;
using System.Diagnostics;
using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime;
using System.Workflow.Runtime.Hosting;
using System.Security.Principal;

#endregion

namespace System.Workflow.Activities
{
    [AttributeUsageAttribute(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class ExternalDataExchangeAttribute : Attribute
    {
    }


    [AttributeUsageAttribute(AttributeTargets.Event | AttributeTargets.Method, AllowMultiple = false)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class CorrelationInitializerAttribute : Attribute
    {
    }

    [AttributeUsageAttribute(AttributeTargets.Interface, AllowMultiple = true)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class CorrelationParameterAttribute : Attribute
    {
        private string name = string.Empty;
        public CorrelationParameterAttribute(string name)
        {
            this.name = name;
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }
    }

    [AttributeUsageAttribute(AttributeTargets.Delegate | AttributeTargets.Event | AttributeTargets.Method, AllowMultiple = true)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class CorrelationAliasAttribute : Attribute
    {
        private string path;
        private string name;

        public CorrelationAliasAttribute(String name, String path)
        {
            this.path = path;
            this.name = name;
        }

        public String Name
        {
            get
            {
                return this.name;
            }
        }

        public String Path
        {
            get
            {
                return path;
            }
        }
    }

    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class ExternalDataEventArgs : EventArgs
    {
        Guid instanceId;
        object batchworkItem;
        IPendingWork batchworkHandler;
        String identity;
        bool waitForIdle;

        public ExternalDataEventArgs()
            : this(Guid.Empty, null, null, false)
        {
        }

        public ExternalDataEventArgs(Guid instanceId)
            : this(instanceId, null, null, false)
        {
        }

        public ExternalDataEventArgs(Guid instanceId, IPendingWork workHandler, object workItem, bool waitForIdle)
        {
            this.instanceId = instanceId;
            this.batchworkHandler = workHandler;
            this.batchworkItem = workItem;
            this.waitForIdle = waitForIdle;
        }

        public ExternalDataEventArgs(Guid instanceId, IPendingWork workHandler, object workItem)
            : this(instanceId, workHandler, workItem, false)
        {
        }

        public Guid InstanceId
        {
            get
            {
                return this.instanceId;
            }
            set
            {
                this.instanceId = value;
            }
        }

        public object WorkItem
        {
            get
            {
                return this.batchworkItem;
            }
            set
            {
                this.batchworkItem = value;
            }
        }

        public IPendingWork WorkHandler
        {
            get
            {
                return this.batchworkHandler;
            }
            set
            {
                this.batchworkHandler = value;
            }
        }

        public String Identity
        {
            get
            {
                return this.identity;
            }
            set
            {
                this.identity = value;
            }
        }

        public bool WaitForIdle
        {
            get
            {
                return this.waitForIdle;
            }
            set
            {
                this.waitForIdle = value;
            }
        }
    }
}
