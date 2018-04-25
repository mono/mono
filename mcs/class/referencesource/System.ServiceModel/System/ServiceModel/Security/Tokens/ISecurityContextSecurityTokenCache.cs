//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.Xml;
    using System.Collections.ObjectModel;

    public interface ISecurityContextSecurityTokenCache 
    {
        void AddContext(SecurityContextSecurityToken token);
        bool TryAddContext(SecurityContextSecurityToken token);
        void ClearContexts();
        void RemoveContext(UniqueId contextId, UniqueId generation);
        void RemoveAllContexts(UniqueId contextId);
        SecurityContextSecurityToken GetContext(UniqueId contextId, UniqueId generation);
        Collection<SecurityContextSecurityToken> GetAllContexts(UniqueId contextId);
        void UpdateContextCachingTime(SecurityContextSecurityToken context, DateTime expirationTime);
    }
}
