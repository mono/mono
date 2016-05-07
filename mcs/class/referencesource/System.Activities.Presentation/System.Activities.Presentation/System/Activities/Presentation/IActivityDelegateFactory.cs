//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    internal interface IActivityDelegateFactory
    {
        Type DelegateType { get; }

        ActivityDelegate Create();
    }
}
