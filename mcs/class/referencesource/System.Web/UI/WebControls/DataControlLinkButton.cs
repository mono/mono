//------------------------------------------------------------------------------
// <copyright file="DataControlLinkButton.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Drawing;
    using System.Web.Util;


    /// <devdoc>
    ///  Derived version of LinkButton used within a DataControl.
    /// </devdoc>
    [SupportsEventValidation]
    internal class DataControlLinkButton : LinkButton {
        
        IPostBackContainer _container;
        string _callbackArgument;
        bool _enableCallback;

        internal DataControlLinkButton(IPostBackContainer container) {
            _container = container;
        }

        public override bool CausesValidation {
            get {
                if (_container != null) {
                    return false;
                }
                return base.CausesValidation;
            }
            set {
                if (_container != null) {
                    throw new NotSupportedException(SR.GetString(SR.CannotSetValidationOnDataControlButtons));
                }
                base.CausesValidation = value;
            }
        }
        
        internal void EnableCallback(string argument) {
            _enableCallback = true;
            _callbackArgument = argument;
        }

        protected override PostBackOptions GetPostBackOptions() {
            if (_container != null) {
                return _container.GetPostBackOptions(this);
            }

            return base.GetPostBackOptions();
        }

        protected internal override void Render(HtmlTextWriter writer) {
            SetCallbackProperties();
            SetForeColor();
            base.Render(writer);
        }

        private void SetCallbackProperties() {
            if (_enableCallback) {
                ICallbackContainer _callbackContainer = _container as ICallbackContainer;
                if (_callbackContainer != null) {
                    string callbackScript = _callbackContainer.GetCallbackScript(this, _callbackArgument);
                    if (!String.IsNullOrEmpty(callbackScript)) {
                        this.OnClientClick = callbackScript;
                    }
                }
            }
        }


        /// <devdoc>
        ///  In HTML hyperlinks always use the browser's link color.
        ///  For the DataControl, we want all LinkButtons to honor the ForeColor setting.
        ///  This requires looking up into the control hierarchy to see if either the cell
        ///  or the containing row or table define a ForeColor.
        /// </devdoc>
        protected virtual void SetForeColor() {
            if (ControlStyle.IsSet(System.Web.UI.WebControls.Style.PROP_FORECOLOR) == false) {
                Color hyperLinkForeColor;
                Control control = this;

                for (int i = 0; i < 3; i++) {
                    control = control.Parent;

                    Debug.Assert(((i == 0) && (control is TableCell)) ||
                                 ((i == 1) && (control is TableRow)) ||
                                 ((i == 2) && (control is Table)));
                    hyperLinkForeColor = ((WebControl)control).ForeColor;
                    if (hyperLinkForeColor != Color.Empty) {
                        ForeColor = hyperLinkForeColor;
                        break;
                    }
                }
            }
        }
    }
}

