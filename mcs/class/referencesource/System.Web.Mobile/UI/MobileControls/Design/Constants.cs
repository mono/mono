//------------------------------------------------------------------------------
// <copyright file="Constants.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Globalization;

namespace System.Web.UI.Design.MobileControls
{
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class Constants : System.Web.UI.MobileControls.Constants
    {
        // You don't want to instantiate this class.
        private Constants() 
        {
        }

        static Constants()
        {
            ControlSizeAtToplevelInNonErrorMode = ControlMaxsizeAtToplevel.ToString(CultureInfo.InvariantCulture) + "px";
        }

        internal static readonly int ControlMaxsizeAtToplevel               = 297;
        internal static readonly String ControlSizeAtToplevelInNonErrorMode;
        internal static readonly String ControlSizeAtToplevelInErrormode    = "300px";
        internal static readonly String ControlSizeInContainer              = "100%";

        internal static readonly String ReflectPropertyDescriptorTypeFullName = 
            "System.ComponentModel.ReflectPropertyDescriptor";
    }
}
