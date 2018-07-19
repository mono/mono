//------------------------------------------------------------------------------
// <copyright file="FieldDirection.cs" company="Microsoft">
// 
// <OWNER>petes</OWNER>
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.CodeDom {

    using System.Diagnostics;
    using System.Runtime.InteropServices;

    /// <devdoc>
    ///    <para>
    ///       Specifies values used to indicate field and parameter directions.
    ///    </para>
    /// </devdoc>
    [
        ComVisible(true),
        Serializable,
    ]
    public enum FieldDirection {
        /// <devdoc>
        ///    <para>
        ///       Incoming field.
        ///    </para>
        /// </devdoc>
        In,
        /// <devdoc>
        ///    <para>
        ///       Outgoing field.
        ///    </para>
        /// </devdoc>
        Out,
        /// <devdoc>
        ///    <para>
        ///       Field by reference.
        ///    </para>
        /// </devdoc>
        Ref,
    }
}
