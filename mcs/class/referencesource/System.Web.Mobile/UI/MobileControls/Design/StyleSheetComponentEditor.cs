//------------------------------------------------------------------------------
// <copyright file="StyleSheetComponentEditor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Web.UI.MobileControls;
    using System.Web.UI.Design.MobileControls;
    using System.Web.UI.Design.MobileControls.Adapters;
    using System.Windows.Forms;

    /// <summary>
    ///    <para>
    ///       Provides a component editor for a StyleSheet <see cref='System.Web.UI.MobileControls.StyleSheet'/>
    ///       control.
    ///    </para>
    /// </summary>
    /// <seealso cref='System.Web.UI.MobileControls.StyleSheet'/>
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class StyleSheetComponentEditor : ComponentEditor 
    {
        public override bool EditComponent(ITypeDescriptorContext context, Object component)  
        {
            Debug.Assert(component is StyleSheet);

            StyleSheet styleSheet = (StyleSheet)component;
            StyleSheetDesigner designer = 
                (StyleSheetDesigner)DesignerAdapterUtil.ControlDesigner(styleSheet);
            //String currentStyle = designer.TemplateStyle;

            if (designer.InTemplateMode)
            {
                MessageBox.Show(SR.GetString(SR.BaseTemplatedMobileComponentEditor_TemplateModeErrorMessage), 
                    SR.GetString(SR.BaseTemplatedMobileComponentEditor_TemplateModeErrorTitle),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                return false;
            }

            StylesEditorDialog dialog;
            
            try
            {
                dialog = new StylesEditorDialog(
                    styleSheet,
                    designer,
                    null /*currentStyle*/
                );
            }
            catch//(ArgumentException e)
            {
                // Debug.Fail(e.ToString());
                // Block user from entering StylesEditorDialog until they fix
                // duplicate style declarations.
                return false;
            }

            return (dialog.ShowDialog() == DialogResult.OK);
        }
    }
}
