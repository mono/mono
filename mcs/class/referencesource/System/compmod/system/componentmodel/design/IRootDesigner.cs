//------------------------------------------------------------------------------
// <copyright file="IRootDesigner.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel.Design {
    using System.Diagnostics;
    using System;
    using System.ComponentModel;
    using Microsoft.Win32;

    /// <devdoc>
    ///     Defines the root designer.  A root designer is the designer that sits
    ///     at the top, or root, of the object hierarchy.  The root designer's job
    ///     is to provide the design-time user interface for the design surface.
    ///     It does this through the View property.
    /// </devdoc>
    [System.Runtime.InteropServices.ComVisible(true)]
    public interface IRootDesigner : IDesigner {
    
        /// <devdoc>
        ///     The list of technologies that this designer can support
        ///     for its view.  Examples of different technologies are
        ///     Windows Forms and Web Forms.  Other object models can be
        ///     supported at design time, but they most be able to
        ///     provide a view in one of the supported technologies.
        /// </devdoc>
        ViewTechnology[] SupportedTechnologies { get; }

        /// <devdoc>
        ///     The user interface to present to the user.  The returning
        ///     data type is an object because there can be a variety
        ///     of different user interface technologies.  Development
        ///     environments typically support more than one technology.
        /// </devdoc>
        object GetView(ViewTechnology technology);
    }
}

