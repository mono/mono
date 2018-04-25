//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System.Runtime;
    using System.Runtime.DurableInstancing;

    [Fx.Tag.XamlVisible(false)]
    public sealed class TryLoadRunnableWorkflowCommand : InstancePersistenceCommand
    {
        public TryLoadRunnableWorkflowCommand()
            : base(InstancePersistence.ActivitiesCommandNamespace.GetName("TryLoadRunnableWorkflow"))
        {
        }

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
            if (!view.IsBoundToInstanceOwner)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SRCore.OwnerRequired));
            }
            if (view.IsBoundToInstance)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SRCore.AlreadyBoundToInstance));
            }
        }
    }
}
