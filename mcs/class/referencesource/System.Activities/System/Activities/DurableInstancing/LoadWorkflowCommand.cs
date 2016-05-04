//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.Xml.Linq;

    [Fx.Tag.XamlVisible(false)]
    public sealed class LoadWorkflowCommand : InstancePersistenceCommand
    {
        public LoadWorkflowCommand()
            : base(InstancePersistence.ActivitiesCommandNamespace.GetName("LoadWorkflow"))
        {
        }

        public bool AcceptUninitializedInstance { get; set; }

        protected internal override bool IsTransactionEnlistmentOptional
        {
            get
            {
                return true;
            }
        }

        protected internal override bool AutomaticallyAcquiringLock
        {
            get
            {
                return true;
            }
        }

        protected internal override void Validate(InstanceView view)
        {
            if (!view.IsBoundToInstance)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SRCore.InstanceRequired));
            }

            if (!view.IsBoundToInstanceOwner)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SRCore.OwnerRequired));
            }
        }
    }
}
