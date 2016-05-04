//------------------------------------------------------------------------------
// <copyright file="MobileUITypeEditor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls 
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using System.Diagnostics;

    /// <summary>
    ///    <para>
    ///       The editor for column collections.
    ///    </para>
    /// </summary>
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class MobileUITypeEditor : UITypeEditor 
    {
        protected ControlDesigner GetDesigner(ITypeDescriptorContext context)
        {
            IDesignerHost designerHost = (IDesignerHost)context.GetService(typeof(IDesignerHost));
            Debug.Assert(designerHost != null, "Did not get DesignerHost service.");

            Debug.Assert(context.Instance is Control, "Expected Control");
            Control control = (Control)context.Instance;

            ControlDesigner designer = (ControlDesigner)designerHost.GetDesigner(control);
            Debug.Assert(designer != null, "Did not get designer for component");

            return designer;
        }

        /// <summary>
        ///    <para>
        ///       Gets the edit style.
        ///    </para>
        /// </summary>
        /// <param name='context'>
        ///    An <see cref='System.ComponentModel.ITypeDescriptorContext'/> that specifies the associated context.
        /// </param>
        /// <returns>
        ///    <para>
        ///       A <see cref='System.Drawing.Design.UITypeEditorEditStyle'/> that represents the edit style.
        ///    </para>
        /// </returns>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) 
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}

