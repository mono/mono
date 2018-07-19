//------------------------------------------------------------------------------
// <copyright file="IStateFormatter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System;

    public interface IStateFormatter {

        object Deserialize(string serializedState);

        string Serialize(object state);
    }
}
