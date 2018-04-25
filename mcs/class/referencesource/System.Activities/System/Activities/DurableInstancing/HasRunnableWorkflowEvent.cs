//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System.Runtime;
    using System.Runtime.DurableInstancing;

    [Fx.Tag.XamlVisible(false)]
    public sealed class HasRunnableWorkflowEvent : InstancePersistenceEvent<HasRunnableWorkflowEvent>
    {
        public HasRunnableWorkflowEvent()
            : base(InstancePersistence.ActivitiesEventNamespace.GetName("HasRunnableWorkflow"))
        {
        }
    }
}
