//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Reflection;
    using System.Collections;

    public interface IClientOperationSelector
    {
        bool AreParametersRequiredForSelection { get; }
        string SelectOperation(MethodBase method, object[] parameters);
    }
}
