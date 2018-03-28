//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Runtime.DurableInstancing
{
    using System;
    using System.Runtime;
    using System.Xml.Linq;
    using System.Collections.Generic;

    public abstract class InstancePersistenceCommand
    {
        protected InstancePersistenceCommand(XName name)
        {
            if (name == null)
            {
                throw Fx.Exception.ArgumentNull("name");
            }
            Name = name;
        }

        public XName Name { get; private set; }

        protected internal virtual bool IsTransactionEnlistmentOptional
        {
            get
            {
                return false;
            }
        }

        // For now, only support registering to bind once the owner is established.  (Can't create an owner and take a lock in one command.)
        protected internal virtual bool AutomaticallyAcquiringLock
        {
            get
            {
                return false;
            }
        }

        protected internal virtual void Validate(InstanceView view)
        {
        }

        internal virtual IEnumerable<InstancePersistenceCommand> Reduce(InstanceView view)
        {
            return null;
        }
    }
}
