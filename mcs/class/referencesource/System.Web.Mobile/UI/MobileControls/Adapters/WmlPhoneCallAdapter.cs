//------------------------------------------------------------------------------
// <copyright file="WmlPhoneCallAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.MobileControls;
using System.Security.Permissions;
using System.Globalization;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource
#else
namespace System.Web.UI.MobileControls.Adapters
#endif    

{

    /*
     * WmlPhoneCallAdapter class.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\WmlPhoneCallAdapter.uex' path='docs/doc[@for="WmlPhoneCallAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class WmlPhoneCallAdapter : WmlControlAdapter
    {
        /// <include file='doc\WmlPhoneCallAdapter.uex' path='docs/doc[@for="WmlPhoneCallAdapter.Control"]/*' />
        protected new PhoneCall Control
        {
            get
            {
                return (PhoneCall)base.Control;
            }
        }

        /// <include file='doc\WmlPhoneCallAdapter.uex' path='docs/doc[@for="WmlPhoneCallAdapter.Render"]/*' />
        public override void Render(WmlMobileTextWriter writer)
        {
            String text, url, phoneNumber;
            String controlText = Control.Text;

            // Always strip off optional separators for PhoneNumber before it
            // is added in markup.

            String originalNumber = Control.PhoneNumber;
            char[] plainNumber = new char[originalNumber.Length];  // allocate enough buffer size

            // Loop to strip out optional separators
            int sizeOfPlainNumber = 0;
            foreach (char ch in originalNumber)
            {
                if ((ch >= '0' && ch <= '9') || ch == '+')
                {
                    plainNumber[sizeOfPlainNumber] = ch;
                    sizeOfPlainNumber++;
                }
            }

            // Assign the number string with the right size
            phoneNumber = new String(plainNumber, 0, sizeOfPlainNumber);

            // Construct text and url based on device capabilities
            //
            if (!Device.CanInitiateVoiceCall)
            {
                text = String.Format(CultureInfo.InvariantCulture, Control.AlternateFormat,
                                     controlText,
                                     originalNumber);
                url = Control.AlternateUrl;
            }
            else
            {
                // Some WML browsers require the phone number
                // showing as text so it can be selected.  If it is not
                // formatted in the text yet, append it to the end of the
                // text.
                if(Device.RequiresPhoneNumbersAsPlainText)
                {
                    text = controlText + " " + phoneNumber;
                    url = String.Empty;
                }
                else
                {
                    text = (controlText == null || controlText.Length > 0) ?
                                controlText : originalNumber;
                    url = "wtai://wp/mc;" + phoneNumber;
                }
            }

            // Write out plain text or corresponding link/softkey command
            // accordingly
            //
            writer.EnterStyle(Style);
            if (url.Length == 0)
            {
                writer.RenderText(text, Control.BreakAfter);
            }
            else
            {
                String softkeyLabel = Control.SoftkeyLabel;
                bool implicitSoftkeyLabel = false;
                if (softkeyLabel.Length == 0)
                {
                    implicitSoftkeyLabel = true;
                    softkeyLabel = text;
                }
                if (!writer.IsValidSoftkeyLabel(softkeyLabel))
                {
                    if (!implicitSoftkeyLabel && softkeyLabel.Length > 0)
                    {
                        softkeyLabel = softkeyLabel.Substring(0, Device.MaximumSoftkeyLabelLength);
                    }
                    else
                    {
                        implicitSoftkeyLabel = true;
                        softkeyLabel = GetDefaultLabel(CallLabel);
                    }
                }
                RenderBeginLink(writer, url, softkeyLabel, implicitSoftkeyLabel, true);
                writer.RenderText(text);
                RenderEndLink(writer, url, Control.BreakAfter);
            }
            writer.ExitStyle(Style);
        }

    }

}
