//------------------------------------------------------------------------------
// <copyright file="RTLAwareMessageBox.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//------------------------------------------------------------------------------

using System;
using System.Windows.Forms;
using System.Design;

namespace System.Web.UI.Design.WebControls
{
    /// <devdoc>
    ///    <para>
    ///       The Show method displays a message box that can contain text, buttons, and symbols that
    ///       inform and instruct the user. This MessageBox will be RTL, if the resources
    ///       for this dll have been localized to a RTL language.
    ///    </para>
    /// </devdoc>
    internal static class RTLAwareMessageBox
    {
        /// <devdoc>
        ///    <para>
        ///       Displays a message box with specified text, caption, and style.
        ///       Makes the dialog RTL if the resources for this dll have been localized to a RTL language.
        ///    </para>
        /// </devdoc>
        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon,
                                        MessageBoxDefaultButton defaultButton, MessageBoxOptions options)
        {
            if (RTLAwareMessageBox.IsRTLResources)
            {
                options |= (MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            }
            return MessageBox.Show(owner, text, caption, buttons, icon, defaultButton, options);
        }

        /// <devdoc>
        ///     Tells whether the current resources for this dll have been
        ///     localized for a RTL language.
        /// </devdoc>
        public static bool IsRTLResources
        {
            get
            {
                return Strings.RTL != "RTL_False";
            }
        }
    }
}


