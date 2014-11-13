//------------------------------------------------------------------------------
// <copyright file="MobileUserControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Web.UI;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{
    /// <include file='doc\MobileUserControl.uex' path='docs/doc[@for="MobileUserControl"]/*' />
    [
        Designer("Microsoft.VisualStudio.Web.WebForms.MobileWebFormDesigner, " + AssemblyRef.MicrosoftVisualStudioWeb, typeof(IRootDesigner)),
        Designer(typeof(System.Web.UI.Design.MobileControls.MobileUserControlDesigner))
    ]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class MobileUserControl : UserControl
    {
        /// <include file='doc\MobileUserControl.uex' path='docs/doc[@for="MobileUserControl.AddParsedSubObject"]/*' />
        protected override void AddParsedSubObject(Object o)
        {
            // Note : AddParsedSubObject is never called at DesignTime
            if (o is StyleSheet)
            {
                if (_styleSheet != null)
                {
                    throw new
                        Exception(SR.GetString(SR.StyleSheet_DuplicateWarningMessage));
                }
                else
                {
                    _styleSheet = (StyleSheet)o;
                }
            }

            base.AddParsedSubObject(o);
        }

        private StyleSheet _styleSheet = null;
        internal StyleSheet StyleSheet
        {
            get
            {
                return (_styleSheet != null) ? _styleSheet : StyleSheet.Default;
            }
        }
    }
}
