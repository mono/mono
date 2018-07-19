//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;

    public enum ServiceDiscoveryMode
    {
        [SuppressMessage(FxCop.Category.Naming, FxCop.Rule.IdentifiersShouldBeSpelledCorrectly, Justification = "Adhoc is a valid name.")]
        Adhoc,
        Managed
    };
}
