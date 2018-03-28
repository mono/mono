//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;

    public enum WorkflowIdentityFilter
    {
        Exact = 0,
        Any = 1,
        AnyRevision = 2
    }

    internal static class WorkflowIdentityFilterExtensions
    {
        public static bool IsValid(this WorkflowIdentityFilter value)
        {
            return (int)value >= (int)WorkflowIdentityFilter.Exact && (int)value <= (int)WorkflowIdentityFilter.AnyRevision;
        }
    }
}
