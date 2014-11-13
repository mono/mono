//------------------------------------------------------------------------------
// <copyright file="HtmlTextBoxAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Security.Permissions;
using System.Globalization;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource
#else
namespace System.Web.UI.MobileControls.Adapters
#endif    

{
    /*
     * HtmlTextBoxAdapter class.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\HtmlTextBoxAdapter.uex' path='docs/doc[@for="HtmlTextBoxAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class HtmlTextBoxAdapter : HtmlControlAdapter
    {
        private String _staticValue;

        /// <include file='doc\HtmlTextBoxAdapter.uex' path='docs/doc[@for="HtmlTextBoxAdapter.Control"]/*' />
        protected new TextBox Control
        {
            get
            {
                return (TextBox)base.Control;
            }
        }

        /// <include file='doc\HtmlTextBoxAdapter.uex' path='docs/doc[@for="HtmlTextBoxAdapter.OnInit"]/*' />
        public override void OnInit(EventArgs e)
        {
            _staticValue = Control.Text;
            base.OnInit(e);
        }

        /// <include file='doc\HtmlTextBoxAdapter.uex' path='docs/doc[@for="HtmlTextBoxAdapter.Render"]/*' />
        public override void Render(HtmlMobileTextWriter writer)
        {
            writer.EnterLayout(Style); 

            writer.WriteBeginTag("input");

            writer.WriteAttribute("name", GetRenderName());

            if ((Control.Text == null || Control.Text.Length > 0) && !Control.Password)
            {
                writer.Write(" value=\"");
                writer.WriteText(Control.Text, true);
                writer.Write("\"");
            }
            if (Control.Size > 0)
            {
                writer.WriteAttribute("size", Control.Size.ToString(CultureInfo.InvariantCulture));
            }
            if (Control.MaxLength > 0)
            {
                writer.WriteAttribute("maxlength", Control.MaxLength.ToString(CultureInfo.InvariantCulture));
            }
            if (Control.Password)
            {
                writer.WriteAttribute("type", "password");
            }
            AddAttributes(writer);
            writer.Write("/>");

            writer.ExitLayout(Style, Control.BreakAfter);
            writer.InputWritten = true;
        }

        internal virtual String GetRenderName()
        {
            String renderName;
            if(Device.RequiresAttributeColonSubstitution)
            {
                renderName = Control.UniqueID.Replace(':', ',');
            }
            else
            {
                renderName = Control.UniqueID;
            }

            return renderName;
        }

        /// <include file='doc\HtmlTextBoxAdapter.uex' path='docs/doc[@for="HtmlTextBoxAdapter.RenderAsHiddenInputField"]/*' />
        protected override void RenderAsHiddenInputField(HtmlMobileTextWriter writer)
        {
            // Optimization - if viewstate is enabled for this control, and the
            // postback returns to this page, we just let it do the trick.

            if (Control.Form.Action.Length > 0 || (!IsViewStateEnabled() && Control.Text != _staticValue))
            {
                writer.WriteHiddenField(Control.UniqueID, Control.Text);
            }
        }

        private bool IsViewStateEnabled()
        {
            Control ctl = Control;
            while (ctl != null)
            {
                if (!ctl.EnableViewState)
                {
                    return false;
                }
                ctl = ctl.Parent;
            }
            return true;
        }
    }
}
