//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.ServiceModel.Activities.Dispatcher;
    using System.Xml.Linq;

    [Fx.Tag.XamlVisible(false)]
    public sealed class DurableInstancingOptions : IDurableInstancingOptions
    {
        DurableInstanceManager instanceManager;

        internal DurableInstancingOptions(DurableInstanceManager instanceManager)
        {
            this.instanceManager = instanceManager;
        }

        public InstanceStore InstanceStore
        {
            get
            {
                return this.instanceManager.InstanceStore;
            }

            set
            {
                this.instanceManager.InstanceStore = value;
            }
        }

        internal XName ScopeName
        {
            set;
            get;
        }

        void IDurableInstancingOptions.SetScopeName(XName scopeName)
        {
            this.ScopeName = scopeName;
        }

        public void AddInstanceOwnerValues(IDictionary<XName, object> readWriteValues, IDictionary<XName, object> writeOnlyValues)
        {
            this.instanceManager.AddInstanceOwnerValues(readWriteValues, writeOnlyValues);
        }

        public void AddInitialInstanceValues(IDictionary<XName, object> writeOnlyValues)
        {
            this.instanceManager.AddInitialInstanceValues(writeOnlyValues);
        }
    }
}
