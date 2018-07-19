//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.Xml.Linq;

    [Fx.Tag.XamlVisible(false)]
    public sealed class DeleteWorkflowOwnerCommand : InstancePersistenceCommand
    {
        public DeleteWorkflowOwnerCommand()
            : base(InstancePersistence.ActivitiesCommandNamespace.GetName("DeleteWorkflowOwner"))
        {
        }

        protected internal override bool IsTransactionEnlistmentOptional
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
        }
    }
}
