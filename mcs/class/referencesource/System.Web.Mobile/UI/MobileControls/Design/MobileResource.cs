//------------------------------------------------------------------------------
// <copyright file="MobileResource.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls
{
    using System;
    using System.Web.UI.Design;
    using System.Security.Permissions;

    /// <include file='doc\MobileResource.uex' path='docs/doc[@for="MobileResource"]/*' />
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public sealed class MobileResource
    {
        private MobileResource()
        {
            // classes with only static methods shouldn't have public
            // constructors.
        }
        
        /// <include file='doc\MobileResource.uex' path='docs/doc[@for="MobileResource.GetString"]/*' />
        public static String GetString(String name)
        {
            return SR.GetString(name);
        }
    }
}
