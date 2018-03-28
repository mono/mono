//------------------------------------------------------------------------------
// <copyright file="WmlPhoneLinkAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#if WMLSUPPORT

namespace System.Web.UI.WebControls.Adapters {
    using System.Web.UI.WebControls;

    public class WmlPhoneLinkAdapter : PhoneLinkAdapter {

        // UNDONE: Add style.
        protected internal override void Render(HtmlTextWriter markupWriter) {
            WmlTextWriter writer = (WmlTextWriter)markupWriter;
            String text, url, phoneNumber;
            String controlText = Control.Text;

            // Always strip off optional separators for PhoneNumber before it
            // is added in markup.

            String originalNumber = Control.PhoneNumber;
            char[] plainNumber = new char[originalNumber.Length];  // allocate enough buffer size

            // Loop to strip out optional separators
            int sizeOfPlainNumber = 0;
            for (int i = 0; i < originalNumber.Length; ++i) {
                char ch = originalNumber[i];
                if ((ch >= '0' && ch <= '9') || ch == '+') {
                    plainNumber[sizeOfPlainNumber] = ch;
                    sizeOfPlainNumber++;
                }
            }

            // Assign the number string with the right size
            phoneNumber = new String(plainNumber, 0, sizeOfPlainNumber);

            // Construct text and url based on device capabilities
            //
            HttpBrowserCapabilities browser = null;
            if (Page != null && Page.Request != null) {
                browser = Page.Request.Browser;
            }
            // TODO: Replace hard coded string key.
            if (browser != null && (String)browser["canInitiateVoiceCall"] != "true") {
                text = String.Format(controlText,
                                     originalNumber);
                url = Control.ResolveClientUrl(Control.NavigateUrl);
                url = Control.GetCountClickUrl(url);
            }
            else {
                // Some WML browsers require the phone number
                // showing as text so it can be selected.  If it is not
                // formatted in the text yet, append it to the end of the
                // text.
                // TODO: Replace hard coded string key.
                if (browser != null && browser["requiresPhoneNumbersAsPlainText"] == "true") {
                    text = controlText + " " + phoneNumber;
                    url = String.Empty;
                }
                else {
                    text = (!String.IsNullOrEmpty(controlText)) ?
                           controlText : originalNumber;
                    url = "wtai://wp/mc;" + phoneNumber;
                }
            }

            // Write out plain text or corresponding link/softkey command
            // accordingly
            //
            writer.EnterStyle(Control.ControlStyle);
            if (url.Length == 0) {
                writer.WriteEncodedText(text);
            }
            else {
                String softkeyLabel = Control.SoftkeyLabel;
                if (String.IsNullOrEmpty(softkeyLabel)) {
                    softkeyLabel = text;
                }
                PageAdapter.RenderBeginHyperlink(writer, url, false /* encode, Whidbey 28731 */, softkeyLabel, Control.AccessKey);
                writer.Write(text);
                PageAdapter.RenderEndHyperlink(writer);
            }
            writer.ExitStyle(Control.ControlStyle);
        }
    }
}

#endif
