//------------------------------------------------------------------------------
// <copyright file="HtmlPhoneCallAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Globalization;
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
     * HtmlPhoneCallAdapter class.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\HtmlPhoneCallAdapter.uex' path='docs/doc[@for="HtmlPhoneCallAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class HtmlPhoneCallAdapter : HtmlControlAdapter
    {
        /// <include file='doc\HtmlPhoneCallAdapter.uex' path='docs/doc[@for="HtmlPhoneCallAdapter.Control"]/*' />
        protected new PhoneCall Control
        {
            get
            {
                return (PhoneCall)base.Control;
            }
        }

        /// <include file='doc\HtmlPhoneCallAdapter.uex' path='docs/doc[@for="HtmlPhoneCallAdapter.Render"]/*' />
        public override void Render(HtmlMobileTextWriter writer)
        {
            writer.EnterStyle(Style);
            if (Device.CanInitiateVoiceCall)
            {
                String text = Control.Text;
                String phoneNumber = Control.PhoneNumber;

                if (String.IsNullOrEmpty(text))
                {
                    text = phoneNumber;
                }

                writer.WriteBeginTag("a");
                writer.Write(" href=\"tel:");

                foreach (char ch in phoneNumber)
                {
                    if (ch >= '0' && ch <= '9' || ch == '#' || ch=='+')
                    {
                        writer.Write(ch);
                    }
                }
                writer.Write("\"");
                AddAttributes(writer);
                writer.Write(">");
                writer.WriteText(text, true);
                writer.WriteEndTag("a");
            }
            else
            {
                // Format the text string based on properties
                String text = String.Format(CultureInfo.CurrentCulture, Control.AlternateFormat, Control.Text,
                                            Control.PhoneNumber);
                String url = Control.AlternateUrl;

                // If URI specified, create a link.  Otherwise, only text is displayed.
                if (!String.IsNullOrEmpty(url))
                {
                    RenderBeginLink(writer, url);
                    writer.WriteText(text, true);
                    RenderEndLink(writer);
                }
                else
                {
                    writer.WriteText(text, true);
                }
            }
            writer.ExitStyle(Style, Control.BreakAfter);
        }
    }

}

