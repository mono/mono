//------------------------------------------------------------------------------
// <copyright file="IDesignerLoaderService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel.Design.Serialization {

    using System.Collections;

    /// <devdoc>
    ///     This interface may be optionally implemented by the designer loader to provide
    ///     load services to outside components.  It provides support for asynchronous loading
    ///     of the designer and allows other objects to initiate a reload of othe
    ///     design surface.  Designer loaders do not need to implement this but it is 
    ///     recommended.  We do not directly put this on DesignerLoader so we can prevent
    ///     outside objects from interacting with the main methods of a designer loader.
    ///     These should only be called by the designer host.
    /// </devdoc>
    public interface IDesignerLoaderService {
    
        /// <devdoc>
        ///     Adds a load dependency to this loader.  This indicates that some other
        ///     object is also participating in the load, and that the designer loader
        ///     should not call EndLoad on the loader host until all load dependencies
        ///     have called DependentLoadComplete on the designer loader.
        /// </devdoc>
        void AddLoadDependency();
    
        /// <devdoc>
        ///     This is called by any object that has previously called
        ///     AddLoadDependency to signal that the dependent load has completed.
        ///     The caller should pass either an empty collection or null to indicate
        ///     a successful load, or a collection of exceptions that indicate the
        ///     reason(s) for failure.
        /// </devdoc>
        void DependentLoadComplete(bool successful, ICollection errorCollection);
    
        /// <devdoc>
        ///     This can be called by an outside object to request that the loader
        ///     reload the design document.  If it supports reloading and wants to
        ///     comply with the reload, the designer loader should return true.  Otherwise
        ///     it should return false, indicating that the reload will not occur.
        ///     Callers should not rely on the reload happening immediately; the
        ///     designer loader may schedule this for some other time, or it may
        ///     try to reload at once.
        /// </devdoc>
        bool Reload();
    }
}

