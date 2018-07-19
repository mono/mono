//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Persistence
{
    using System;
    using System.ServiceModel.Channels;

    [Obsolete("The WF3 types are deprecated.  Instead, please use the new WF4 types from System.Activities.*")]
    public abstract class PersistenceProviderFactory : CommunicationObject
    {
        protected PersistenceProviderFactory()
        {
        }

        public abstract PersistenceProvider CreateProvider(Guid id);
    }
}
