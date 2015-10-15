//------------------------------------------------------------------------------
// <copyright file="IVType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration
{
    // DevDiv Bugs 137864
    internal enum IVType
    {
        // This switch doesn't prepend any IV at all. DO NOT use it unless you
        // really, really need to for back-compat (e.g. Membership) and the
        // end user does not control the input or output path.
        None = 0,

        // Prepends a random IV to the ciphertext. Almost all callers should
        // use this switch.
        Random = 1,

        // Prepends H(plaintext) [where H is an unkeyed hash function] to the
        // ciphertext. Only use if you need the ciphertext to be stable for
        // a given plaintext, e.g. WebResource.axd. MS AJAX will cache the
        // ciphertext values client-side, so if the ciphertext changes you may
        // end up with Javascript errors upon loading the same resource
        // multiple times.
        Hash = 2,
    }
}