//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.Xml.Linq;
    using System.Diagnostics.CodeAnalysis;

    [Fx.Tag.XamlVisible(false)]   
    public sealed class QueryActivatableWorkflowsCommand : InstancePersistenceCommand
    {
        public QueryActivatableWorkflowsCommand()
            : base(InstancePersistence.ActivitiesCommandNamespace.GetName("QueryActivatableWorkflows"))
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

            if (view.IsBoundToInstance)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SRCore.AlreadyBoundToInstance));
            }
        }
    }
}
