//------------------------------------------------------------------------------
// <copyright file="WsiProfiles.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services {

    using System;

    /// <include file='doc\WsiProfiles.uex' path='docs/doc[@for="WsiProfiles"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [Flags]
    public enum WsiProfiles {
        /// <include file='doc\WsiProfiles.uex' path='docs/doc[@for="WsiProfiles.None"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        None = 0x00,

        /// <include file='doc\WsiProfiles.uex' path='docs/doc[@for="WsiProfiles.BasicProfile1_1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        BasicProfile1_1 = 0x01,
    }
}
