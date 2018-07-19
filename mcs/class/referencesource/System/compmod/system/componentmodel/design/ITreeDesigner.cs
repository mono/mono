//------------------------------------------------------------------------------
// <copyright file="ITreeDesigner.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel.Design {

    using System;
    using System.Collections;

    /// <devdoc>
    ///     ITreeDesigner is a variation of IDesigner that provides support for
    ///     generically indicating parent / child relationships within a designer.
    /// </devdoc>
    public interface ITreeDesigner : IDesigner {
        
        /// <devdoc>
        ///     Retrieves the children of this designer.  This will return an empty collection
        ///     if this designer has no children.
        /// </devdoc>
        ICollection Children { get; }

        /// <devdoc>
        ///     Retrieves the parent designer for this designer. This may return null if
        ///     there is no parent.
        /// </devdoc>
        IDesigner Parent { get; }
    }
}

