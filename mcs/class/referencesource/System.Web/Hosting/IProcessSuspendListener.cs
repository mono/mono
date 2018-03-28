//------------------------------------------------------------------------------
// <copyright file="IProcessSuspendListener.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Hosting {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    /*
     * !! USAGE NOTE !!
     * This interface is not exposed publicly because it is expected that Helios developers will consume the
     * no-PIA interfaces that will be released OOB. This interface only exists so that ASP.NET can interface
     * with the Helios layer if necessary. These interfaces are subject to change.
     */

    /// <summary>
    /// If an ICustomRuntime also implements this interface, it will be notified when
    /// IIS sends a notification to suspend / resume the current process. Implementing
    /// this interface is optional.
    /// </summary>
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("406E6C4C-1C5D-4357-9DFE-EF4BE00D654B")]
    internal interface IProcessSuspendListener {
        /// <summary>
        /// Called upon IIS notification of process suspension. This method *must not*
        /// throw, otherwise the behavior is undefined (we will probably terminate the
        /// process). This method may block for short periods of time if it needs to
        /// delay process suspension.
        /// </summary>
        /// <returns>A callback to be invoked when the process resumes. The return
        /// value may be null if the listener does not care about resume notifications.</returns>
        /// <remarks>
        /// This method can be called at any time, including potentially simultaneously
        /// with other calls to ICustomRuntime methods, or even after ICustomRuntime.Stop
        /// has been called.
        /// </remarks>
        [return: MarshalAs(UnmanagedType.Interface)]
        IProcessResumeCallback Suspend();
    }

    /// <summary>
    /// Receives notifications that the process is resuming from suspension.
    /// </summary>
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("BB1AEEC0-E4EC-47BA-8724-D26AC4F16604")]
    internal interface IProcessResumeCallback {
        /// <summary>
        /// Called when the process resumes from suspension. This method *must not*
        /// throw, otherwise the behavior is undefined (we will probably terminate
        /// the process).
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Resume", Justification = "Per spec.")]
        void Resume();
    }
}
