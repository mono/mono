//------------------------------------------------------------------------------
// <copyright file="TlsTokenBindingInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Hosting {
    using System;

    internal sealed class TlsTokenBindingInfo : ITlsTokenBindingInfo {
        private readonly byte[] _providedTokenBindingId;
        private readonly byte[] _referredTokenBindingId;

        internal TlsTokenBindingInfo(byte[] providedTokenBindingId, byte[] referredTokenBindingId) {
            _providedTokenBindingId = providedTokenBindingId;
            _referredTokenBindingId = referredTokenBindingId;
        }

        public byte[] GetProvidedTokenBindingId() {
            return (_providedTokenBindingId != null)
                ? (byte[])_providedTokenBindingId.Clone()
                : null;
        }

        public byte[] GetReferredTokenBindingId() {
            return (_referredTokenBindingId != null)
                ? (byte[])_referredTokenBindingId.Clone()
                : null;
        }
    }
}
