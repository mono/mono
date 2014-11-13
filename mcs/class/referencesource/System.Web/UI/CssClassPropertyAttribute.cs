//------------------------------------------------------------------------------
// <copyright file="CssClassPropertyAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web.Util;
    

    /// <devdoc>
    ///     CssClassPropertyAttribute 
    ///     The CssClassPropertyAttribute is applied to properties that contain CssClass names.
    ///     The designer uses this attribute to add a design-time CssClass editor experience
    ///     to the property in the property grid.
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CssClassPropertyAttribute : Attribute {

        public CssClassPropertyAttribute() {
        }
    }
}
