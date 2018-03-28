//------------------------------------------------------------------------------
// <copyright file="ITemplate.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * A factory for populating a control with children and setting attributes on the
 * control.
 *
 * Copyright (c) 1999 Microsoft Corporation
 */
namespace System.Web.UI {

using System;

/// <devdoc>
///    <para>
///       Provides a factory definition for populating a control with child
///       controls from an inline template within a page file.
///    </para>
/// </devdoc>
public interface ITemplate {

    /// <devdoc>
    ///    <para>
    ///       Iteratively populates a provided <see cref='System.Web.UI.ControlCollection'/>
    ///       Control with a sub-hierarchy of child controls represented by the template.
    ///    </para>
    /// </devdoc>
    void InstantiateIn(Control container);
}
}
