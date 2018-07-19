//------------------------------------------------------------------------------
// <copyright file="StandardToolWindows.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel.Design {
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Security.Permissions;
    
    /// <devdoc>
    ///    <para> Defines GUID specifiers that contain GUIDs which reference the standard set of tool windows that are available in
    ///       the design environment.</para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.InheritanceDemand, Name = "FullTrust")]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Name="FullTrust")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2112:SecuredTypesShouldNotExposeFields")] // ReadOnly fields - already shipped.
    // SECREVIEW: The demands here have no effects, the class is all made up of static fields.
    public class StandardToolWindows {
        /// <devdoc>
        ///    <para>Gets the GUID for the object browser.</para>
        /// </devdoc>
        public static readonly Guid ObjectBrowser   = new Guid("{970d9861-ee83-11d0-a778-00a0c91110c3}");
        /// <devdoc>
        ///    <para>Gets the GUID for the output window.</para>
        /// </devdoc>
        public static readonly Guid OutputWindow    = new Guid("{34e76e81-ee4a-11d0-ae2e-00a0c90fffc3}");
        /// <devdoc>
        ///    <para>Gets the GUID for the project explorer.</para>
        /// </devdoc>
        public static readonly Guid ProjectExplorer = new Guid("{3ae79031-e1bc-11d0-8f78-00a0c9110057}");
        /// <devdoc>
        ///    <para>Gets the GUID for the properties window.</para>
        /// </devdoc>
        public static readonly Guid PropertyBrowser = new Guid("{eefa5220-e298-11d0-8f78-00a0c9110057}");
        /// <devdoc>
        ///    <para>Gets the GUID for the related links frame.</para>
        /// </devdoc>
        public static readonly Guid RelatedLinks    = new Guid("{66dba47c-61df-11d2-aa79-00c04f990343}");
        /// <devdoc>
        ///    <para>Gets the GUID for the server explorer.</para>
        /// </devdoc>
        public static readonly Guid ServerExplorer  = new Guid("{74946827-37a0-11d2-a273-00c04f8ef4ff}");
        /// <devdoc>
        ///    <para>Gets the GUID for the task list.</para>
        /// </devdoc>
        public static readonly Guid TaskList        = new Guid("{4a9b7e51-aa16-11d0-a8c5-00a0c921a4d2}");
        /// <devdoc>
        ///    <para>Gets the GUID for the toolbox.</para>
        /// </devdoc>
        public static readonly Guid Toolbox         = new Guid("{b1e99781-ab81-11d0-b683-00aa00a3ee26}");
    }
}
