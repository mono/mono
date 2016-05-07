//------------------------------------------------------------------------------
// <copyright file="MobileUserControlDesigner.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Web.UI;
    using System.Web.UI.Design.MobileControls;
    using System.Web.UI.MobileControls;

    [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand, Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class MobileUserControlDesigner : ControlDesigner {

        internal MobileUserControlDesigner() {
            ShouldCodeSerialize = false;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets a value indicating whether all user controls are resizeable.
        ///    </para>
        /// </devdoc>
        public override bool AllowResize {
            get {
                return false;
            }
        }

        // Displays the userControl using a simple placeholder like V1.
        public override string GetDesignTimeHtml() {
            return CreatePlaceHolderDesignTimeHtml();
        }

        public override string GetPersistenceContent() {
            return null;
        }
    }
}
