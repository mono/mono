//------------------------------------------------------------------------------
// <copyright file="IDesigner.cs" company="Microsoft">
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
    ///    <para> Provides the basic framework for building a custom designer.
    ///       This interface stores the verbs available to the designer, as well as basic
    ///       services for the designer.</para>
    /// </devdoc>
    [System.Runtime.InteropServices.ComVisible(true)]
    public interface IDesigner : IDisposable {

        /// <devdoc>
        ///    <para>Gets or sets the base component this designer is designing.</para>
        /// </devdoc>
        IComponent Component {get;}
        
        /// <devdoc>
        ///    <para> Gets or sets the design-time verbs supported by the designer.</para>
        /// </devdoc>
        DesignerVerbCollection Verbs {get;}

        /// <devdoc>
        ///    <para>
        ///       Performs the default action for this designer.
        ///    </para>
        /// </devdoc>
        void DoDefaultAction();
        
        /// <devdoc>
        ///    <para>
        ///       Initializes the designer with the given component.
        ///    </para>
        /// </devdoc>
        void Initialize(IComponent component);
    }
}

