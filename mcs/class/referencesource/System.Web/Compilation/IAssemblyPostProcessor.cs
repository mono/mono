//------------------------------------------------------------------------------
// <copyright file="IAssemblyPostProcessor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Compilation {

    using System.Security.Permissions;
    
    /*
     * Interface to post-process an assembly after it has been built (DevDiv 32575)
     */
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.High)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.High)]
    public interface IAssemblyPostProcessor: IDisposable {

        /*
         * Give the implementor a chance to modify the assembly before it gets loaded.
         * This can be used for example by a profiling tool which needs to insert some probes.
         */
        void PostProcessAssembly(string path);
    }

}

