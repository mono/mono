//------------------------------------------------------------------------------
// <copyright file="ISubscriptionToken.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web {

    // Interface that is returned by event subscription methods and can be used to unsubscribe listeners.

    public interface ISubscriptionToken {

        // Returns a value stating whether the subscription is currently active
        bool IsActive { get; }

        // Unsubscribes from the event
        void Unsubscribe();

    }
}
