//------------------------------------------------------------------------------
// <copyright file="DeviceOverridableATtribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.MobileControls
{
    using System;
    using System.Security.Permissions;

    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class DeviceOverridableAttribute : Attribute
    {
        bool _overridable = false;
    
        public DeviceOverridableAttribute()
        {
        }

        public DeviceOverridableAttribute(bool overridable)
        {
            _overridable = overridable;
        }

        public bool Overridable
        {
            get
            {
                return _overridable;
            }
        }
    }
}
