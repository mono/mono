//------------------------------------------------------------------------------
// <copyright file="ICustomRuntime.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Hosting {
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;

    /*
     * !! USAGE NOTE !!
     * This interface is not exposed publicly because it is expected that Helios developers will consume the
     * no-PIA interfaces that will be released OOB. This interface only exists so that ASP.NET can interface
     * with the Helios layer if necessary. These interfaces are subject to change.
     */

    /// <summary>
    /// Defines the mechanism via which IIS will interact with the application.
    /// </summary>
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("692D0723-C338-4D09-9057-C71F0F47DA87")]
    internal interface ICustomRuntime {
        /// <summary>
        /// Called at some point between GL_APPLICATION_START and
        /// GL_RESOLVE_MODULES and signals managed application start.
        /// </summary>
        /// <param name="reserved0">Do not use this parameter.</param>
        /// <param name="reserved1">Do not use this parameter.</param>
        void Start(
            [In] IntPtr reserved0,
            [In] int reserved1);

        /// <summary>
        /// Called during GL_RESOLVE_MODULES to give managed runtime a
        /// chance to register CHttpModule instances with the IIS pipeline.
        /// </summary>
        /// <param name="pResolveModuleData">Additional data that may be useful to the custom runtime for integrating with the IIS pipeline.
        /// This pointer is only valid within the call to ResolveModules.</param>
        /// <param name="resolveModuleDataSize">The size (in bytes) of the structure pointed to by pResolveModuleData.</param>
        void ResolveModules(
            [In] IntPtr pResolveModuleData,
            [In] int resolveModuleDataSize);

        /// <summary>
        /// Called during GL_APPLICATION_STOP and signals managed
        /// application end.
        /// </summary>
        /// <param name="reserved0">Do not use this parameter.</param>
        /// <param name="reserved1">Do not use this parameter.</param>
        /// <remarks>
        /// It is acceptable for this method to unload the current AppDomain
        /// and return COR_E_APPDOMAINUNLOADED.
        /// </remarks>
        void Stop(
            [In] IntPtr reserved0,
            [In] int reserved1);
    }
}
