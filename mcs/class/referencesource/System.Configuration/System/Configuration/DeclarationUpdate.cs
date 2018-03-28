//------------------------------------------------------------------------------
// <copyright file="DeclarationUpdate.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {

    //
    // Trivially derived class of Update to represent an update
    // to the declaration of a section.
    //
    internal class DeclarationUpdate : Update {
        internal DeclarationUpdate(string configKey, bool moved, string updatedXml) : base(configKey, moved, updatedXml) {
        }
    }
}
