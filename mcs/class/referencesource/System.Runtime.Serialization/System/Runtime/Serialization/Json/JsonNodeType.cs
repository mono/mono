//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Serialization.Json
{
    enum JsonNodeType
    {
        None,
        Object,
        Element,
        EndElement,
        QuotedText,
        StandaloneText,
        Collection
    }
}