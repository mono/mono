// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.DynamicUpdate
{
    internal interface IInstanceUpdatable
    {
        void InternalUpdateInstance(NativeActivityUpdateContext updateContext);
    }
}
