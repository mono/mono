//------------------------------------------------------------------------------
// <copyright file="ICustomLoader.cs" company="Microsoft">
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
    /// Defines the entry point where the Helios hoster calls into the developer-provided bootstrapper.
    /// The developer's bin-deployed AspNet.Loader assembly is expected to have an assembly-level
    /// CustomLoaderAttribute whose ctor parameter is a type which implements this interface.
    /// </summary>
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("50A3CE65-2F9F-44E9-9094-32C6C928F966")]
    internal interface ICustomLoader {

        /// <summary>
        /// Loads a custom runtime for the current application.
        /// </summary>
        /// <param name="appId">The ID of the current application (e.g., IHttpApplication::GetApplicationId).</param>
        /// <param name="appConfigPath">The configuration path of the current application (e.g., IHttpApplication::GetAppConfigPath).</param>
        /// <param name="supportFunctions">Support functions for the current host.</param>
        /// <param name="pLoadAppData">Additional data that may be useful to a custom loader for integrating with the IIS pipeline.
        /// This pointer is only valid within the call to LoadApplication.</param>
        /// <param name="loadAppDataSize">The size (in bytes) of the structure pointed to by pLoadAppData.</param>
        /// <returns>An ICustomRuntime instance wrapped inside an ObjectHandle.</returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        IObjectHandle LoadApplication(
            [In, MarshalAs(UnmanagedType.LPWStr)] string appId,
            [In, MarshalAs(UnmanagedType.LPWStr)] string appConfigPath,
            [In, MarshalAs(UnmanagedType.Interface)] IProcessHostSupportFunctions supportFunctions,
            [In] IntPtr pLoadAppData,
            [In] int loadAppDataSize);

    }
}
