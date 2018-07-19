//------------------------------------------------------------------------------
// <copyright file="RepeaterCommandEventHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */

namespace System.Web.UI.WebControls {

    using System;


    /// <devdoc>
    ///    <para>Represents the method that will handle the
    ///    <see langword='ItemCommand'/> event of a <see cref='System.Web.UI.WebControls.Repeater'/>.</para>
    /// </devdoc>
    public delegate void RepeaterCommandEventHandler(object source, RepeaterCommandEventArgs e);
}
