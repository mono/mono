//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System.Runtime.DurableInstancing;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Xml.Linq;

    [Serializable]
    class InstanceAlreadyLockedToOwnerException : InstancePersistenceCommandException
    {
        public InstanceAlreadyLockedToOwnerException(XName commandName, Guid instanceId, long instanceVersion)
            : base(commandName, instanceId)
        {
            this.InstanceVersion = instanceVersion;
        }

        public long InstanceVersion
        {
            get;
            private set;
        }
    }
}
