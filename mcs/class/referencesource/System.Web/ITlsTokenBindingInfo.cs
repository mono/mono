//------------------------------------------------------------------------------
// <copyright file="ITlsTokenBindingInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web {
    using System;

    // Represents token binding information for a request.
    // TLS token bindings help mitigate the risk of impersonation by an
    // attacker in the event an authenticated client's bearer tokens are
    // somehow exfiltrated from the client's machine.
    // More info: https://datatracker.ietf.org/doc/draft-popov-token-binding/
    public interface ITlsTokenBindingInfo {
        // Gets the 'provided' token binding identifier associated with
        // the request. This method could return null if the client did
        // not supply a 'provided' token binding or if the client did
        // not supply a valid proof of possession for the associated
        // private key.
        //
        // The caller should treat this token binding id as an opaque blob
        // and should not try to parse it.
        byte[] GetProvidedTokenBindingId();

        // Gets the 'referred' token binding identifier associated with
        // the request. This method could return null if the client did
        // not supply a 'referred' token binding or if the client did
        // not supply a valid proof of possession for the associated
        // private key.
        //
        // The caller should treat this token binding id as an opaque blob
        // and should not try to parse it.
        byte[] GetReferredTokenBindingId();
    }
}
