//------------------------------------------------------------------------------
// <copyright file="DesignerCapabilities.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls
{
    using System.Collections;
    using System.Diagnostics;
    using System.Web.Mobile;
    using System.Web.UI;
    using System.Globalization;

    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class DesignerCapabilities : MobileCapabilities
    {
        private static IDictionary _items = null;
        private readonly static DesignerCapabilities _staticInstance = new DesignerCapabilities();

        static DesignerCapabilities()
        {
            _items = new Hashtable();

            // HTTPBrowserCapabilities Properties
            _items["type"] = "IE5";
            _items["browser"] = "IE";
            _items["version"] = "5.5";
            _items["majorversion"] = "5";
            _items["minorversion"] = "5";
            _items["platform"] = "Win32";
            _items["tagwriter"] = "System.Web.UI.HtmlTextWriter";
            _items["beta"] = "false";
            _items["crawler"] = "false";
            _items["aol"] = "false";
            _items["win16"] = "false";
            _items["win32"] = "true";
            _items["frames"] = "true";
            _items["tables"] = "true";
            _items["cookies"] = "true";
            _items["vbscript"] = "false";
            _items["javascript"] = "true";
            _items["javaapplets"] = "false";
            _items["activeXControls"] = "false";
            _items["backgroundSounds"] = "false";
            _items["cdf"] = "false";

            // Designer Capabilities Properties
            _items["defaultCharacterWidth"] = "8";
            _items["defaultCharacterHeight"] = "12";
            _items["mobileDeviceManufacturer"] = "Unknown";
            _items["mobileDeviceModel"] = "Unknown";
            _items["gatewayVersion"] = "None";
            _items["gatewayMajorVersion"] = "0";
            _items["gatewayMinorVersion"] = "0";
            _items["preferredRenderingType"] = "html32";
            _items["preferredRenderingMime"] = "text/html";
            _items["preferredImageMime"] = "image/gif";
            _items["preferredLanguage"] = "";
            _items["screenCharactersWidth"] = "80";
            _items["screenCharactersHeight"] = "40";
            _items["screenPixelsWidth"] = "640";
            _items["screenPixelsHeight"] = "480";
            _items["screenBitDepth"] = "8";
            _items["isColor"] = "true";
            _items["inputType"] = "";
            _items["numberOfSoftkeys"] = "0";
            _items["maximumSoftkeyLabelLength"] = "8";
            _items["canInitiateVoiceCall"] = "false";
            _items["canSendMail"] = "true";
            _items["hasBackButton"] = "true";
            _items["rendersWmlDoAcceptsInline"] = "true";
            _items["rendersWmlSelectsAsMenuCards"] = "false";
            _items["rendersBreaksAfterWmlAnchor"] = "false";
            _items["rendersBreaksAfterWmlInput"] = "false";
            _items["rendersBreakBeforeWmlSelectAndInput"] = "false";
            _items["requiresPhoneNumbersAsPlainText"] = "false";
            _items["requiresUrlEncodedPostfieldValues"] = "false";
            _items["requiredMetaTagNameValue"] = "";
            _items["rendersBreaksAfterHtmlLists"] = "true";
            _items["requiresUniqueHtmlCheckboxNames"] = "false";
            _items["requiresUniqueHtmlInputNames"] = "false";
            _items["requiresAttributeColonSubstitution"] = "false";
            _items["requiresHtmlAdaptiveErrorReporting"] = "false";
            _items["requiresContentTypeMetaTag"] = "false";
            _items["requiresDBCSCharacter"] = "false";
            _items["supportsCss"] = "true";
            _items["hidesRightAlignedMultiselectScrollbars"]="false";
            _items["isMobileDevice"] = "false";
            _items["canRenderInputAndSelectElementsTogether"] = "true";
            _items["canRenderAfterInputOrSelectElement"] = "true";
            _items["canRenderOneventAndPrevElementsTogether"] = "true";
            _items["canRenderSetvarZeroWithMultiSelectionList"] = "true";
            _items["canRenderPostBackCards"] = "true";
            _items["canRenderMixedSelects"] = "true";
            _items["canCombineFormsInDeck"] = "true";
            _items["supportsImageSubmit"] = "true";
            _items["requiresUniqueFilePathSuffix"] = "false";
            _items["supportsSelectMultiple"] = "true";
            _items["supportsBold"] = "true";
            _items["supportsItalic"] = "true";
            _items["supportsFontSize"] = "true";
            _items["supportsFontName"] = "true";
            _items["supportsFontColor"] = "true";
            _items["supportsBodyColor"] = "true";
            _items["supportsDivAlign"] = "true";
            _items["supportsDivNoWrap"] = "true";
            _items["requiresOutputOptimization"] = "false";
            _items["supportsAccesskeyAttribute"] = "false";
            _items["supportsInputIStyle"] = "false";
            _items["supportsInputMode"] = "false";
            _items["supportsIModeSymbols"] = "false";
            _items["supportsJPhoneSymbols"] = "false";
            _items["supportsJPhoneMultiMediaAttributes"] = "false";
            _items["maximumRenderedPageSize"] = "2000";
            _items["requiresSpecialViewStateEncoding"] = "false";
            _items["requiresNoBreakInFormatting"] = "false";
            _items["supportsQueryStringInFormAction"] = "true";
            _items["supportsCacheControlMetaTag"] = "true";
            _items["supportsUncheck"] = "true";
            _items["canRenderEmptySelects"] = "true";
            _items["supportsRedirectWithCookie"] = "true";
            _items["supportsEmptyStringInCookieValue"] = "true";
            _items["requiresNoSoftkeyLabels"] = "false";
            _items["defaultSubmitButtonLimit"] = "1";
            _items["supportsCharacterEntityEncoding"] = "true";
        }

        public override String this[String key]
        {
            get
            {
                Object obj = _items[key];
                Debug.Assert(obj != null, 
                    String.Format(CultureInfo.CurrentCulture, "property {0} not defined in DesignerCapabilities", key));
                Debug.Assert(obj is String,
                    String.Format(CultureInfo.CurrentCulture, "property {0} invalid type defined", key));

                return obj as String;
            }
        }

        internal static DesignerCapabilities Instance
        {
            get
            {
                return _staticInstance;
            }
        }
    }
}
