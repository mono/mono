//------------------------------------------------------------------------------
// <copyright file="HtmlCommandAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.MobileControls;
using System.Drawing;
using System.Security.Permissions;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource
#else
namespace System.Web.UI.MobileControls.Adapters
#endif    

{

    /*
     * HtmlCommandAdapter class.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\HtmlCommandAdapter.uex' path='docs/doc[@for="HtmlCommandAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class HtmlCommandAdapter : HtmlControlAdapter
    {
        /// <include file='doc\HtmlCommandAdapter.uex' path='docs/doc[@for="HtmlCommandAdapter.Control"]/*' />
        protected new Command Control
        {
            get
            {
                return (Command)base.Control;
            }
        }

        /// <include file='doc\HtmlCommandAdapter.uex' path='docs/doc[@for="HtmlCommandAdapter.Render"]/*' />
        public override void Render(HtmlMobileTextWriter writer)
        {
            bool renderLink = false;
            bool renderImage = false;

            // If image is defined and renderable, just do it.  Otherwise,
            // render as a link if specified.
            if (!String.IsNullOrEmpty(Control.ImageUrl) &&
                Device.SupportsImageSubmit)
            {
                renderImage = true;
            }
            else if (Control.Format == CommandFormat.Link &&
                     Device.JavaScript)
            {
                renderLink = true;
            }


            if (renderLink)
            {
                writer.EnterStyle(Style);
                Form form = Control.Form;
                if (form.Action.Length > 0)
                {
                    writer.Write("<a href=\"javascript:document.");
                    writer.Write(form.ClientID);
                    writer.Write(".submit()\"");
                    AddAttributes(writer);
                    writer.Write(">");
                    writer.WriteText(Control.Text, true);
                    writer.WriteEndTag("a");
                }
                else
                {
                    RenderBeginLink(writer, Constants.FormIDPrefix + form.UniqueID);
                    writer.WriteText(Control.Text, true);
                    RenderEndLink(writer);
                }
                writer.ExitStyle(Style, Control.BreakAfter);
            }
            else
            {
                writer.EnterLayout(Style);
                writer.WriteBeginTag("input");
                writer.WriteAttribute("name", Control.UniqueID);

                if (renderImage)
                {
                    writer.WriteAttribute("type", "image");
                    writer.WriteAttribute("src", Control.ResolveUrl(Control.ImageUrl), true);
                    writer.WriteAttribute("alt", Control.Text, true);
                }
                else
                {
                    writer.WriteAttribute("type", "submit");
                    writer.Write(" value=\"");
                    writer.WriteText(Control.Text, true);
                    writer.Write("\"");
                }

                AddAttributes(writer);
                writer.Write("/>");
                writer.ExitLayout(Style, Control.BreakAfter);
            }

        }

        /// <include file='doc\HtmlCommandAdapter.uex' path='docs/doc[@for="HtmlCommandAdapter.LoadPostData"]/*' />
        public override bool LoadPostData(String key,
                                          NameValueCollection data,
                                          Object controlPrivateData,
                                          out bool dataChanged)
        {
            dataChanged = false;
            
            // HTML input tags of type image postback with the coordinates
            // of the click rather than the name of the control.
            String name = Control.UniqueID;
            String postX = data[name + ".x"];
            String postY = data[name + ".y"];
            if (postX != null && postY != null
                && postX.Length > 0 && postY.Length > 0)
            {
                // set dataChannged to cause RaisePostDataChangedEvent()
                dataChanged = true;
                return true;
            }
            // For other command control, defer to default logic in control.
            return false;     
        }
    }
}
