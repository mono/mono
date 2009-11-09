// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;

#if SILVERLIGHT

namespace System.ComponentModel.Composition.Primitives
{
    public interface IPartCreatorImportDefinition
    {
        ContractBasedImportDefinition ProductImportDefinition { get; }
    }
}

#endif