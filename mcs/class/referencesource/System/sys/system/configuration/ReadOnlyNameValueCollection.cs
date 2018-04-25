//------------------------------------------------------------------------------
// <copyright file="ReadOnlyNameValueCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using System.Collections;
    using System.Collections.Specialized;

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    internal class ReadOnlyNameValueCollection : NameValueCollection {

        internal ReadOnlyNameValueCollection(IEqualityComparer equalityComparer) : base(equalityComparer) {
        }

        internal ReadOnlyNameValueCollection(ReadOnlyNameValueCollection value) : base(value) {
        }

        internal void SetReadOnly() {
            IsReadOnly = true;
        }
    }
}
