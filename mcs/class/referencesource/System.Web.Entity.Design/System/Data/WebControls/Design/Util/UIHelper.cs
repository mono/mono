//------------------------------------------------------------------------------
// <copyright file="UIHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//
// Helper methods for UI functionality like displaying dialogs
//------------------------------------------------------------------------------

using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace System.Web.UI.Design.WebControls.Util
{
    internal static class UIHelper
    {
        internal static Font GetDialogFont(IServiceProvider serviceProvider)
        {
            if (serviceProvider != null)
            {
                IUIService uiService = (IUIService)serviceProvider.GetService(typeof(IUIService));
                if (uiService != null)
                {
                    IDictionary uiStyles = uiService.Styles;
                    if (uiStyles != null)
                    {
                        return (Font)uiStyles["DialogFont"];
                    }
                }
            }
            return null;
        }

        internal static DialogResult ShowDialog(IServiceProvider serviceProvider, Form form)
        {
            if (serviceProvider != null)
            {
                IUIService uiService = (IUIService)serviceProvider.GetService(typeof(IUIService));
                if (uiService != null)
                {
                    return uiService.ShowDialog(form);
                }
            }

            return form.ShowDialog();
        }

        public static void ShowError(IServiceProvider serviceProvider, string message)
        {
            if (serviceProvider != null)
            {
                IUIService uiService = (IUIService)serviceProvider.GetService(typeof(IUIService));
                if (uiService != null)
                {
                    uiService.ShowError(message);
                    return;
                }
            }

            RTLAwareMessageBox.Show(null, message, Strings.UIHelper_ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);
        }

        public static void ShowWarning(IServiceProvider serviceProvider, string message)
        {
            if (serviceProvider != null)
            {
                IUIService uiService = (IUIService)serviceProvider.GetService(typeof(IUIService));
                if (uiService != null)
                {
                    uiService.ShowError(message);
                    return;
                }
            }

            RTLAwareMessageBox.Show(null, message, Strings.UIHelper_WarningCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, 0);
        }
    }
}
