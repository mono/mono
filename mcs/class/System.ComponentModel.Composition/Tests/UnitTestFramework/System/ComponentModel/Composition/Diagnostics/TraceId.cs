// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;

namespace System.ComponentModel.Composition.Diagnostics
{
    // We need a public version of CompositionTraceId, so that the QA tests can access and verify the trace.
    [CLSCompliant(false)]
    public enum TraceId : ushort
    {
        Rejection_DefinitionRejected = CompositionTraceId.Rejection_DefinitionRejected,
        Rejection_DefinitionResurrected = CompositionTraceId.Rejection_DefinitionResurrected,

        Discovery_AssemblyLoadFailed = CompositionTraceId.Discovery_AssemblyLoadFailed,
        Discovery_DefinitionMarkedWithPartNotDiscoverableAttribute = CompositionTraceId.Discovery_DefinitionMarkedWithPartNotDiscoverableAttribute,
        Discovery_DefinitionContainsGenericParameters = CompositionTraceId.Discovery_DefinitionContainsGenericParameters,
        Discovery_DefinitionContainsNoExports = CompositionTraceId.Discovery_DefinitionContainsNoExports,
        Discovery_MemberMarkedWithMultipleImportAndImportMany = CompositionTraceId.Discovery_MemberMarkedWithMultipleImportAndImportMany,
    }
}
