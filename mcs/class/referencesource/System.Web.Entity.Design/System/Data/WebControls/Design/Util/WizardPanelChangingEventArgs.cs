//------------------------------------------------------------------------------
// <copyright file="WizardPanelChangingEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;

namespace System.Web.UI.Design.WebControls.Util
{
    /// <devdoc>
    /// </devdoc>
    internal class WizardPanelChangingEventArgs : EventArgs
    {

        private WizardPanel _currentPanel;

        /// <devdoc>
        /// </devdoc>
        public WizardPanelChangingEventArgs(WizardPanel currentPanel)
        {
            _currentPanel = currentPanel;
        }
    }
}

