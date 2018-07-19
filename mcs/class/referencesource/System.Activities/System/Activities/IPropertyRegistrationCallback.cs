//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Runtime;

    public interface IPropertyRegistrationCallback
    {
        [Fx.Tag.Throws(typeof(Exception), "Extensibility point.")]
        void Register(RegistrationContext context);
        [Fx.Tag.Throws(typeof(Exception), "Extensibility point.")]
        void Unregister(RegistrationContext context);
    }
}


