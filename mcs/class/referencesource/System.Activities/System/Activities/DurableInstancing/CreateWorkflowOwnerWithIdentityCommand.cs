// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.Xml.Linq;

    [Fx.Tag.XamlVisible(false)]
    public sealed class CreateWorkflowOwnerWithIdentityCommand : InstancePersistenceCommand
    {
        Dictionary<XName, InstanceValue> instanceOwnerMetadata;

        public CreateWorkflowOwnerWithIdentityCommand()
            : base(InstancePersistence.ActivitiesCommandNamespace.GetName("CreateWorkflowOwnerWithIdentity"))
        {
        }

        public IDictionary<XName, InstanceValue> InstanceOwnerMetadata
        {
            get
            {
                if (this.instanceOwnerMetadata == null)
                {
                    this.instanceOwnerMetadata = new Dictionary<XName, InstanceValue>();
                }

                return this.instanceOwnerMetadata;
            }
        }

        protected internal override bool IsTransactionEnlistmentOptional
        {
            get
            {
                return this.instanceOwnerMetadata == null || this.instanceOwnerMetadata.Count == 0;
            }
        }

        protected internal override void Validate(InstanceView view)
        {
            if (view.IsBoundToInstanceOwner)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SRCore.AlreadyBoundToOwner));
            }

            InstancePersistence.ValidatePropertyBag(this.instanceOwnerMetadata);
        }
    }
}
