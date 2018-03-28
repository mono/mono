//------------------------------------------------------------------------------
// <copyright file="GenericUI.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls.Util
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Diagnostics;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.Text;

    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class GenericUI
    {
        // Internal type should not be instantiated.
        private GenericUI() {
        }

        internal static readonly Bitmap SortDownIcon =
            new Icon(typeof(MobileControlDesigner), "SortDown.ico").ToBitmap();

        internal static readonly Bitmap SortUpIcon =
            new Icon(typeof(MobileControlDesigner), "SortUp.ico").ToBitmap();
        
        internal static readonly Bitmap DeleteIcon =
            new Icon(typeof(MobileControlDesigner), "Delete.ico").ToBitmap();
        
        internal static readonly Bitmap ErrorIcon = 
            new Icon(typeof(MobileContainerDesigner), "Error.ico").ToBitmap();

        internal static readonly Bitmap InfoIcon = 
            new Icon(typeof(MobileContainerDesigner), "Info.ico").ToBitmap();

        internal static void InitDialog(
            Form dialog,
            ISite site
        ) {
            dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
            dialog.Icon = null;
            dialog.MaximizeBox = false;
            dialog.MinimizeBox = false;
            dialog.ShowInTaskbar = false;
            dialog.StartPosition = FormStartPosition.CenterParent;
            dialog.AutoScaleBaseSize = new Size(5, 14);
            dialog.Font = GetVS7Font(site);
        }

        internal static Font GetVS7Font(ISite site)
        {
            System.Drawing.Font vsfont = Control.DefaultFont;
            if (site != null)
            {
                IUIService uiService = (IUIService) site.GetService(
                    typeof(IUIService)
                    );
                if (uiService != null)
                {
                    vsfont = (Font) uiService.Styles["DialogFont"];
                }
            }
            return vsfont;
        }    
        
        // This version of InitDialog() handles merged UIs
        internal static int InitDialog(
            Form dialog,
            IDeviceSpecificDesigner designer,
            int mergingContext
        ) {
            InitDialog(dialog, designer.UnderlyingControl.Site);
            int tabOffset = 0;
            designer.InitHeader(mergingContext);
            Control header = designer.Header;
            if (header != null)
            {
                // (6, 5) = Windows standard positioning
                header.Location = new Point(6, 5);
                dialog.Controls.Add(header);
                // +6 = 6px space between header and first control
                dialog.Height += header.Height + 6;
                tabOffset = GenericUI.GetMaxContainedTabIndex(header);
                // Changing the header width is going to magically
                // cause everything to be repositioned vertically
                // -10 = 5px padding on each side of the client area
                header.Width = dialog.ClientSize.Width - 10;
            }
            return tabOffset;
        }

        internal static int GetMaxContainedTabIndex(Control control)
        {
            int maxTabIndex = control.TabIndex;
            
            foreach(Control child in control.Controls)
            {
                int maxChildTabIndex = GetMaxContainedTabIndex(child);
                if(maxChildTabIndex > maxTabIndex)
                {
                    maxTabIndex = maxChildTabIndex;
                }
            }
            return maxTabIndex;
        }

        internal static void ShowErrorMessage(String title, String message)
        {
            RTLAwareMessageBox.Show(
                null,
                message,
                title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                0
            );
        }

        internal static String BuildCommaDelimitedList(ICollection stringList)
        {
            StringBuilder delimitedString = new StringBuilder();

            foreach (String str in stringList)
            {
                if(delimitedString.Length > 0)
                {
                    delimitedString.Append(", ");
                }
                delimitedString.Append(str);
            }
            return delimitedString.ToString();
        }

        internal static void ShowWarningMessage(String title, String message)
        {
            RTLAwareMessageBox.Show(
                null,
                message,
                title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation,
                MessageBoxDefaultButton.Button1,
                0
            );
        }
        
        internal static bool ConfirmYesNo(String title, String message)
        {
            DialogResult result = RTLAwareMessageBox.Show(
                null,
                message,
                title,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Exclamation,
                MessageBoxDefaultButton.Button1,
                0
            );
            return result == DialogResult.Yes;
        }
    }

    // Copied from ndp\fx\src\Designer\Microsoft\System\Microsoft\Design\RTLAwareMessageBox.cs
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal sealed class RTLAwareMessageBox {

        // Private helper class shouldn't be instantiated.
        private RTLAwareMessageBox() {
        }

        /// <include file='doc\MessageBox.uex' path='docs/doc[@for="MessageBox.Show6"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Displays a message box with specified text, caption, and style.
        ///       Makes the dialog RTL if the resources for this dll have been localized to a RTL language.
        ///    </para>
        /// </devdoc>
        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon,
                                        MessageBoxDefaultButton defaultButton, MessageBoxOptions options) {
            if (RTLAwareMessageBox.IsRTLResources) {
                options |= (MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            }
            return MessageBox.Show(owner, text, caption, buttons, icon, defaultButton, options);
        }

        /// <devdoc>
        ///     Tells whether the current resources for this dll have been
        ///     localized for a RTL language.
        /// </devdoc>
        public static bool IsRTLResources {
            get {
                // Set RightToLeft mode based on resource file
                string rtlText = SR.GetString(SR.RTL);

                return !String.Equals(rtlText, "RTL_False", StringComparison.Ordinal);
            }
        }
    }
}
