//------------------------------------------------------------------------------
// <copyright file="IAdviseSink.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", Scope="member", Target="System.Runtime.InteropServices.ComTypes.IAdviseSink.OnDataChange(System.Runtime.InteropServices.ComTypes.FORMATETC&,System.Runtime.InteropServices.ComTypes.STGMEDIUM&):System.Void")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", Scope="member", Target="System.Runtime.InteropServices.ComTypes.IAdviseSink.OnDataChange(System.Runtime.InteropServices.ComTypes.FORMATETC&,System.Runtime.InteropServices.ComTypes.STGMEDIUM&):System.Void")]

namespace System.Runtime.InteropServices.ComTypes {

    using System.Runtime.InteropServices;

    /// <devdoc>
    ///     The IAdviseSink interface enables containers and other objects to 
    ///     receive notifications of data changes, view changes, and compound-document 
    ///     changes occurring in objects of interest. Container applications, for 
    ///     example, require such notifications to keep cached presentations of their 
    ///     linked and embedded objects up-to-date. Calls to IAdviseSink methods are 
    ///     asynchronous, so the call is sent and then the next instruction is executed 
    ///     without waiting for the call's return.
    /// </devdoc>
    [ComImport()]
    [Guid("0000010F-0000-0000-C000-000000000046")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAdviseSink {

        /// <devdoc>
        ///     Called by the server to notify a data object's currently registered 
        ///     advise sinks that data in the object has changed.
        /// </devdoc>
        [PreserveSig]
        void OnDataChange([In] ref FORMATETC format, [In] ref STGMEDIUM stgmedium);

        /// <devdoc>
        ///     Notifies an object's registered advise sinks that its view has changed.
        /// </devdoc>
        [PreserveSig]
        void OnViewChange(int aspect, int index);

        /// <devdoc>
        ///     Called by the server to notify all registered advisory sinks that 
        ///     the object has been renamed.
        /// </devdoc>
        [PreserveSig]
        void OnRename(IMoniker moniker);

        /// <devdoc>
        ///     Called by the server to notify all registered advisory sinks that 
        ///     the object has been saved.
        /// </devdoc>
        [PreserveSig]
        void OnSave();

        /// <devdoc>
        ///     Called by the server to notify all registered advisory sinks that the 
        ///     object has changed from the running to the loaded state.
        /// </devdoc>
        [PreserveSig]
        void OnClose();
    }
}


