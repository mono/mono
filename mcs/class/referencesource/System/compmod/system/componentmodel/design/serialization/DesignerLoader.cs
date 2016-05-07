//------------------------------------------------------------------------------
// <copyright file="DesignerLoader.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel.Design.Serialization {

    using System;
    using System.Reflection;
    using System.Security.Permissions;

    /// <devdoc>
    ///     DesignerLoader.  This class is responsible for loading a designer document.  
    ///     Where and how this load occurs is a private matter for the designer loader.
    ///     The designer loader will be handed to an IDesignerHost instance.  This instance, 
    ///     when it is ready to load the document, will call BeginLoad, passing an instance
    ///     of IDesignerLoaderHost.  The designer loader will load up the design surface
    ///     using the host interface, and call EndLoad on the interface when it is done.
    ///     The error collection passed into EndLoad should be empty or null to indicate a
    ///     successful load, or it should contain a collection of exceptions that 
    ///     describe the error.
    ///
    ///     Once a document is loaded, the designer loader is also responsible for
    ///     writing any changes made to the document back whatever storage the
    ///     loader used when loading the document.  
    /// </devdoc>
    [HostProtection(SharedState = true)]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.InheritanceDemand, Name = "FullTrust")]
    [System.Runtime.InteropServices.ComVisible(true)]
    public abstract class DesignerLoader {

        /// <devdoc>
        ///     Returns true when the designer is in the process of loading.  Clients that are
        ///     sinking notifications from the designer often want to ignore them while the desingner is loading
        ///     and only respond to them if they result from user interatcions.
        /// </devdoc>
        public virtual bool Loading {
            get {
                return false;
            }
        }
    
        /// <devdoc>
        ///     Called by the designer host to begin the loading process.  The designer
        ///     host passes in an instance of a designer loader host (which is typically
        ///     the same object as the designer host.  This loader host allows
        ///     the designer loader to reload the design document and also allows
        ///     the designer loader to indicate that it has finished loading the
        ///     design document.
        /// </devdoc>
        public abstract void BeginLoad(IDesignerLoaderHost host);
        
        /// <devdoc>
        ///     Disposes this designer loader.  The designer host will call this method
        ///     when the design document itself is being destroyed.  Once called, the
        ///     designer loader will never be called again.
        /// </devdoc>
        public abstract void Dispose();
        
        /// <devdoc>
        ///     The designer host will call this periodically when it wants to
        ///     ensure that any changes that have been made to the document
        ///     have been saved by the designer loader.  This method allows
        ///     designer loaders to implement a lazy-write scheme to improve
        ///     performance.  The default implementation does nothing.
        /// </devdoc>
        public virtual void Flush() {}
    }
}

