//------------------------------------------------------------------------------
// <copyright file="DesignerForm.cs" company="Microsoft">
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
using System.Design;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace System.Web.UI.Design.WebControls.Util
{
    /// <devdoc>
    /// Represents a form used by a designer.
    /// </devdoc>
    internal abstract class DesignerForm : Form
    {
        private const int SC_CONTEXTHELP = 0xF180;
        private const int WM_SYSCOMMAND = 0x0112;

        private IServiceProvider _serviceProvider;
        private bool _firstActivate;
#if DEBUG
        private bool _formInitialized;
#endif

        /// <devdoc>
        /// Creates a new DesignerForm with a given service provider.
        /// </devdoc>
        protected DesignerForm(IServiceProvider serviceProvider)
        {
            Debug.Assert(serviceProvider != null);
            _serviceProvider = serviceProvider;

            _firstActivate = true;
        }

        /// <devdoc>
        /// The service provider for the form.
        /// </devdoc>
        protected internal IServiceProvider ServiceProvider
        {
            get
            {
                return _serviceProvider;
            }
        }

        /// <devdoc>
        /// Frees up resources.
        /// </devdoc>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _serviceProvider = null;
            }
            base.Dispose(disposing);
        }

        protected void InitializeForm()
        {
            Font dialogFont = UIHelper.GetDialogFont(ServiceProvider);
            if (dialogFont != null)
            {
                Font = dialogFont;
            }

            // Set RightToLeft mode based on resource file
            string rtlText = Strings.RTL;
            if (!String.Equals(rtlText, "RTL_False", StringComparison.Ordinal))
            {
                RightToLeft = RightToLeft.Yes;
                RightToLeftLayout = true;
            }

#pragma warning disable 618
            AutoScaleBaseSize = new System.Drawing.Size(5, 14);
#pragma warning restore 618
            HelpButton = true;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.Sizable;
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            MinimumSize = Size;
#if DEBUG
            _formInitialized = true;
#endif
        }

        /// <devdoc>
        /// Gets a service of the desired entitySetNameItem. Returns null if the service does not exist or there is no service provider.
        /// </devdoc>
        protected override object GetService(Type serviceType)
        {
            if (_serviceProvider != null)
            {
                return _serviceProvider.GetService(serviceType);
            }
            return null;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            if (_firstActivate)
            {
                _firstActivate = false;
                OnInitialActivated(e);
            }
        }

        /// <devdoc>
        /// Returns the help topic for the form. Consult with your UE contact on
        /// what the appropriate help topic is for your dialog.
        /// </devdoc>
        protected abstract string HelpTopic
        {
            get;
        }

        protected sealed override void OnHelpRequested(HelpEventArgs hevent)
        {
            ShowHelp();
            hevent.Handled = true;
        }

        /// <summary>
        /// Raised upon first activation of the form.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnInitialActivated(EventArgs e)
        {
#if DEBUG
            Debug.Assert(_formInitialized, "All classes deriving from DesignerForm must call InitializeForm() from within InitializeComponent().");
#endif
        }

        /// <devdoc>
        /// Launches the help for this form.
        /// </devdoc>
        private void ShowHelp()
        {
            if (ServiceProvider != null)
            {
                IHelpService helpService = (IHelpService)ServiceProvider.GetService(typeof(IHelpService));
                if (helpService != null)
                {
                    helpService.ShowHelpFromKeyword(HelpTopic);
                }
            }
        }

        /// <devdoc>
        /// Overridden to reroute the context-help button to our own handler.
        /// </devdoc>
        protected override void WndProc(ref Message m)
        {
            if ((m.Msg == WM_SYSCOMMAND) && ((int)m.WParam == SC_CONTEXTHELP))
            {
                ShowHelp();
                return;
            }
            else
            {
                base.WndProc(ref m);
            }
        }
    }
}

