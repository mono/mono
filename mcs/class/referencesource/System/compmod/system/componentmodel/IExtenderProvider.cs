//------------------------------------------------------------------------------
// <copyright file="IExtenderProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {
    

    using System.Diagnostics;

    using System;

    /// <devdoc>
    ///    <para>
    ///       Defines the interface
    ///       for extending properties to other components in a container.
    ///    </para>
    /// </devdoc>
    public interface IExtenderProvider {

        /// <devdoc>
        ///    <para>
        ///       Specifies
        ///       whether this object can provide its extender properties to
        ///       the specified object.
        ///    </para>
        /// </devdoc>
        bool CanExtend(object extendee);
    }
}
