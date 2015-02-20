//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.Xml.Linq;

    [Fx.Tag.XamlVisible(false)]
    public sealed class LoadWorkflowByInstanceKeyCommand : InstancePersistenceCommand
    {
        Dictionary<Guid, IDictionary<XName, InstanceValue>> keysToAssociate;

        public LoadWorkflowByInstanceKeyCommand()
            : base(InstancePersistence.ActivitiesCommandNamespace.GetName("LoadWorkflowByInstanceKey"))
        {
        }

        public bool AcceptUninitializedInstance { get; set; }

        public Guid LookupInstanceKey { get; set; }
        public Guid AssociateInstanceKeyToInstanceId { get; set; }

        public IDictionary<Guid, IDictionary<XName, InstanceValue>> InstanceKeysToAssociate
        {
            get
            {
                if (this.keysToAssociate == null)
                {
                    this.keysToAssociate = new Dictionary<Guid, IDictionary<XName, InstanceValue>>();
                }
                return this.keysToAssociate;
            }
        }

        protected internal override bool IsTransactionEnlistmentOptional
        {
            get
            {
                return (this.keysToAssociate == null || this.keysToAssociate.Count == 0) && AssociateInstanceKeyToInstanceId == Guid.Empty;
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

            if (LookupInstanceKey == Guid.Empty)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SRCore.LoadOpKeyMustBeValid));
            }

            if (AssociateInstanceKeyToInstanceId == Guid.Empty)
            {
                if (InstanceKeysToAssociate.ContainsKey(LookupInstanceKey))
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SRCore.LoadOpAssociateKeysCannotContainLookupKey));
                }
            }
            else
            {
                if (!AcceptUninitializedInstance)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SRCore.LoadOpFreeKeyRequiresAcceptUninitialized));
                }
            }

            if (this.keysToAssociate != null)
            {
                foreach (KeyValuePair<Guid, IDictionary<XName, InstanceValue>> key in this.keysToAssociate)
                {
                    InstancePersistence.ValidatePropertyBag(key.Value);
                }
            }
        }
    }
}
