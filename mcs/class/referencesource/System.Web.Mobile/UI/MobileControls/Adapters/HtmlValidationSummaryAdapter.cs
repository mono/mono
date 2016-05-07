//------------------------------------------------------------------------------
// <copyright file="HtmlValidationSummaryAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.MobileControls;
using System.Diagnostics;
using System.Collections;
using System.Security.Permissions;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource
#else
namespace System.Web.UI.MobileControls.Adapters
#endif    

{

    /*
     * HtmlValidationSummaryAdapter provides the html device functionality for
     * ValidationSummary control.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\HtmlValidationSummaryAdapter.uex' path='docs/doc[@for="HtmlValidationSummaryAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class HtmlValidationSummaryAdapter : HtmlControlAdapter
    {
        private List _list;  // to paginate error messages
        private Link _link;  // to go back to the form validated by this control

        /// <include file='doc\HtmlValidationSummaryAdapter.uex' path='docs/doc[@for="HtmlValidationSummaryAdapter.Control"]/*' />
        protected new ValidationSummary Control
        {
            get
            {
                return (ValidationSummary)base.Control;
            }
        }

        /// <include file='doc\HtmlValidationSummaryAdapter.uex' path='docs/doc[@for="HtmlValidationSummaryAdapter.OnInit"]/*' />
        public override void OnInit(EventArgs e)
        {
            // Create child controls to help on rendering
            _list = new List();
            Control.Controls.Add(_list);
            _link = new Link();
            Control.Controls.Add(_link);
        }

        /// <include file='doc\HtmlValidationSummaryAdapter.uex' path='docs/doc[@for="HtmlValidationSummaryAdapter.Render"]/*' />
        public override void Render(HtmlMobileTextWriter writer)
        {
            String[] errorMessages = null;

            if (Control.Visible)
            {
                errorMessages = Control.GetErrorMessages();
            }

            if (errorMessages != null)
            {
                writer.EnterStyle(Style);
                if (Control.HeaderText.Length > 0)
                {
                    writer.WriteText(Control.HeaderText, true);
                }

                ArrayList arr = new ArrayList();
                foreach (String errorMessage in errorMessages)
                {
                    Debug.Assert(errorMessage != null && errorMessage.Length > 0, "Bad Error Messages");
                    arr.Add(errorMessage);
                }

                _list.Decoration = ListDecoration.Bulleted;
                _list.DataSource = arr;
                _list.DataBind();

                if (String.Compare(Control.FormToValidate, Control.Form.UniqueID, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    _link.NavigateUrl = Constants.FormIDPrefix + Control.FormToValidate;
                    _link.Text = String.IsNullOrEmpty(Control.BackLabel) ? GetDefaultLabel(BackLabel) : Control.BackLabel;
                    // Summary writes its own break so last control should write one.
                    _link.BreakAfter = false;
                }
                else
                {
                    _link.Visible = false;
                    // Summary writes its own break so last control should write one.
                    _list.BreakAfter = false;
                }

                // Render the child controls to display error message list and a
                // link for going back to the Form that is having error
                RenderChildren(writer);
                writer.ExitStyle(Style, Control.BreakAfter);
            }
        }
    }
}
