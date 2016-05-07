//------------------------------------------------------------------------------
// <copyright file="DataSourceControlBuilder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System;
    using System.Collections;
    using System.Reflection;
    using System.Web.Util;


    /// <devdoc>
    /// </devdoc>
    public sealed class DataSourceControlBuilder : ControlBuilder {


        /// <devdoc>
        /// </devdoc>
        public override bool AllowWhitespaceLiterals() {
            return false;
        }
    }
}
