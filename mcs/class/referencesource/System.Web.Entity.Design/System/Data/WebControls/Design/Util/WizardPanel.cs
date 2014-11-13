//------------------------------------------------------------------------------
// <copyright file="WizardPanel.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace System.Web.UI.Design.WebControls.Util
{
    /// <devdoc>
    /// Represents a single step in a wizard.
    /// WizardPanels are contained within a single WizardForm.
    /// </devdoc>
    internal class WizardPanel : System.Windows.Forms.UserControl
    {

        private WizardForm _parentWizard;
        private string _caption;
        private WizardPanel _nextPanel;
        private bool _needsToInvalidate;

        /// <devdoc>
        /// Creates a new WizardPanel.
        /// </devdoc>
        public WizardPanel()
        {
        }

        /// <devdoc>
        /// The caption to be shown on the WizardForm
        /// </devdoc>
        public string Caption
        {
            get
            {
                if (_caption == null)
                {
                    return String.Empty;
                }
                return _caption;
            }
            set
            {
                _caption = value;
                if (_parentWizard != null)
                {
                    _parentWizard.Invalidate();
                }
                else
                {
                    _needsToInvalidate = true;
                }
            }
        }

        /// <devdoc>
        /// The panel to go to when the Next button is clicked. This can be set dynamically in
        /// the OnNext() event to customize the order in which panels are used.
        /// </devdoc>
        public WizardPanel NextPanel
        {
            get
            {
                return _nextPanel;
            }
            set
            {
                _nextPanel = value;
                Debug.Assert(_parentWizard != null);
                if (_parentWizard != null)
                {
                    _parentWizard.RegisterPanel(_nextPanel);
                }
            }
        }

        /// <devdoc>
        /// This method is called when the wizard's Finish button is clicked.
        /// It is called once for each wizard panel on the panel stack, in the order from the first panel to the last (current) panel.
        /// </devdoc>
        protected internal virtual void OnComplete()
        {
        }

        /// <devdoc>
        /// Runs when the next button is clicked while this panel is showing.
        /// Returns true if the wizard should proceed to the next panel.
        /// </devdoc>
        public virtual bool OnNext()
        {
            return true;
        }

        /// <devdoc>
        /// Runs when the previous button of the parent wizard form is clicked while this panel is active
        /// </devdoc>
        public virtual void OnPrevious()
        {
        }

        /// <devdoc>
        /// </devdoc>
        internal void SetParentWizard(WizardForm parent)
        {
            _parentWizard = parent;
            if ((_parentWizard != null) && _needsToInvalidate)
            {
                _parentWizard.Invalidate();
                _needsToInvalidate = false;
            }
        }
    }
}
