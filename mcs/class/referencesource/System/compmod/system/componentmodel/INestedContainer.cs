//------------------------------------------------------------------------------
// <copyright file="INestedContainer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {

    /// <devdoc>
    ///     A "nested container" is an object that logically contains zero or more child
    ///     components and is controlled (owned) by some parent component.
    ///    
    ///     In this context, "containment" refers to logical containment, not visual
    ///     containment.  Components and containers can be used in a variety of
    ///     scenarios, including both visual and non-visual scenarios.
    /// </devdoc>
    public interface INestedContainer : IContainer {

        /// <devdoc>
        ///     The component that owns this nested container.
        /// </devdoc>
        IComponent Owner { get; }
    }
}

