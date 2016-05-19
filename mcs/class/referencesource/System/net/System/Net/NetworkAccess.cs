//------------------------------------------------------------------------------
// <copyright file="NetworkAccess.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Net {
    /// <devdoc>
    ///    <para>
    ///       Defines network access permissions.
    ///    </para>
    /// </devdoc>
    [FlagsAttribute]
    public  enum    NetworkAccess {
        /// <devdoc>
        ///    <para>
        ///       An application is allowed to accept connections from the Internet.
        ///    </para>
        /// </devdoc>
        Accept  = 0x80,
        /// <devdoc>
        ///    <para>
        ///       An application is allowed to connect to Internet resources.
        ///    </para>
        /// </devdoc>
        Connect = 0x40

    } // enum NetworkAccess

} // namespace System.Net
