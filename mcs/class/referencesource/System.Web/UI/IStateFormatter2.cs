//------------------------------------------------------------------------------
// <copyright file="IStateFormatter2.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System;
    using System.Web.Security.Cryptography;

    // Similar to IStateFormatter, but also accepts a Purpose object to control cryptographic
    // operations during serialization / deserialization. Internal since we don't publicly
    // expose Purpose objects.

    internal interface IStateFormatter2 : IStateFormatter {

        object Deserialize(string serializedState, Purpose purpose);

        string Serialize(object state, Purpose purpose);

    }
}
