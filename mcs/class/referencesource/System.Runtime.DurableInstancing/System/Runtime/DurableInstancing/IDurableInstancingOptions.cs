//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Runtime.DurableInstancing
{
    using System;
    using System.Xml.Linq;

    //This sole purpose of this interface is to avoid adding S.SM.Activation as a friend of S.SM.Activities
    interface IDurableInstancingOptions
    {
        void SetScopeName(XName scopeName);
    }
}
