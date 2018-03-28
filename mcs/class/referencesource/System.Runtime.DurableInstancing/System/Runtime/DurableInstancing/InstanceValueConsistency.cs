//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Runtime.DurableInstancing
{
    using System;
    using System.Runtime;
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage(FxCop.Category.Naming, FxCop.Rule.FlagsEnumsShouldHavePluralNames, Justification = "Consistency is an adjective.")]
    [Flags]
    public enum InstanceValueConsistency
    {
        None = 0,
        InDoubt = 1,
        Partial = 2,
    }
}
