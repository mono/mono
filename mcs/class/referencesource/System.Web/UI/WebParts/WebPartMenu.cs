//------------------------------------------------------------------------------
// <copyright file="WebPartMenu.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Web;
    using System.Web.Handlers;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.Util;

    internal sealed class WebPartMenu {

        private static string _defaultCheckImageUrl;

        private int _cssStyleIndex;

        private IWebPartMenuUser _menuUser;

        public WebPartMenu(IWebPartMenuUser menuUser) {
            _menuUser = menuUser;
        }

        private static string DefaultCheckImageUrl {
            get {
                if (_defaultCheckImageUrl == null) {
                    _defaultCheckImageUrl = AssemblyResourceLoader.GetWebResourceUrl(typeof(WebPartMenu), "WebPartMenu_Check.gif");
                }
                return _defaultCheckImageUrl;
            }
        }

        private void RegisterStartupScript(string clientID) {
            string menuItemStyleCss = String.Empty;
            string menuItemHoverStyleCss = String.Empty;

            Style itemStyle = _menuUser.ItemStyle;
            if (itemStyle != null) {
                menuItemStyleCss = itemStyle.GetStyleAttributes(_menuUser.UrlResolver).Value;
            }

            Style itemHoverStyle = _menuUser.ItemHoverStyle;
            if (itemHoverStyle != null) {
                menuItemHoverStyleCss = itemHoverStyle.GetStyleAttributes(_menuUser.UrlResolver).Value;
            }

            string labelHoverColor = String.Empty;
            string labelHoverClass = String.Empty;

            Style labelHoverStyle = _menuUser.LabelHoverStyle;
            if (labelHoverStyle != null) {
                Color foreColor = labelHoverStyle.ForeColor;
                if (foreColor.IsEmpty == false) {
                    labelHoverColor = ColorTranslator.ToHtml(foreColor);
                }
                labelHoverClass = labelHoverStyle.RegisteredCssClass;
            }

            // Using concatenation instead of String.Format for perf
            // (here, the compiler will build an object[] and call String.Concat only once).
            string script = @"
<script type=""text/javascript"">
var menu" + clientID + " = new WebPartMenu(document.getElementById('" + clientID + "'), document.getElementById('" + clientID + "Popup'), document.getElementById('" + clientID + @"Menu'));
menu" + clientID + ".itemStyle = '" + Util.QuoteJScriptString(menuItemStyleCss) + @"';
menu" + clientID + ".itemHoverStyle = '" + Util.QuoteJScriptString(menuItemHoverStyleCss) + @"';
menu" + clientID + ".labelHoverColor = '" + labelHoverColor + @"';
menu" + clientID + ".labelHoverClassName = '" + labelHoverClass + @"';
</script>
";

            if (_menuUser.Page != null) {
                _menuUser.Page.ClientScript.RegisterStartupScript((Control)_menuUser, typeof(WebPartMenu), clientID, script, false);
                
                IScriptManager scriptManager = _menuUser.Page.ScriptManager;
                if ((scriptManager != null) && scriptManager.SupportsPartialRendering) {
                    scriptManager.RegisterDispose((Control)_menuUser,
                        "document.getElementById('" + clientID + "').__menu.Dispose();");
                }

            }
        }

        private void RegisterStyle(Style style) {
            Debug.Assert(_menuUser.Page != null && _menuUser.Page.SupportsStyleSheets);

            if (style != null && !style.IsEmpty) {
                // The style should not have already been registered
                Debug.Assert(style.RegisteredCssClass.Length == 0);

                string name = _menuUser.ClientID + "__Menu_" + _cssStyleIndex++.ToString(NumberFormatInfo.InvariantInfo);
                _menuUser.Page.Header.StyleSheet.CreateStyleRule(style, _menuUser.UrlResolver, "." + name);
                style.SetRegisteredCssClass(name);
            }

        }

        public void RegisterStyles() {
            // Assert is fine here as the class is internal
            Debug.Assert(_menuUser.Page != null && _menuUser.Page.SupportsStyleSheets);

            // Registering the static label style before hover so hover takes precedence
            RegisterStyle(_menuUser.LabelStyle);
            RegisterStyle(_menuUser.LabelHoverStyle);
        }

        public void Render(HtmlTextWriter writer, string clientID) {
            RenderLabel(writer, clientID, null);
        }

        public void Render(HtmlTextWriter writer, ICollection verbs, string clientID, WebPart associatedWebPart,
                           WebPartManager webPartManager) {
            // This method should only be called when Zone.RenderClientScript is true, which means
            // WebPartManager is not null.
            Debug.Assert(webPartManager != null);

            RegisterStartupScript(clientID);
            RenderLabel(writer, clientID, associatedWebPart);
            RenderMenuPopup(writer, verbs, clientID, associatedWebPart, webPartManager);
        }

        private void RenderLabel(HtmlTextWriter writer, string clientID, WebPart associatedWebPart) {
            _menuUser.OnBeginRender(writer);

            if (associatedWebPart != null) {
                writer.AddAttribute(HtmlTextWriterAttribute.Id, clientID);

                Style labelStyle = _menuUser.LabelStyle;
                if (labelStyle != null) {
                    labelStyle.AddAttributesToRender(writer, _menuUser as WebControl);
                }
            }
            writer.AddStyleAttribute(HtmlTextWriterStyle.Cursor, "hand");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "inline-block");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, "1px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.TextDecoration, "none");
            writer.RenderBeginTag(HtmlTextWriterTag.Span);

            string labelImageUrl = _menuUser.LabelImageUrl;
            string text = _menuUser.LabelText;
            if (!String.IsNullOrEmpty(labelImageUrl)) {
                writer.AddAttribute(HtmlTextWriterAttribute.Src, labelImageUrl);
                writer.AddAttribute(HtmlTextWriterAttribute.Alt,
                                    (!String.IsNullOrEmpty(text) ?
                                     text :
                                     SR.GetString(SR.WebPartMenu_DefaultDropDownAlternateText)),
                                    true);
                writer.AddStyleAttribute("vertical-align", "middle");
                writer.AddStyleAttribute(HtmlTextWriterStyle.BorderStyle, "none");
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();
                writer.Write("&nbsp;");
            }

            if (!String.IsNullOrEmpty(text)) {
                writer.Write(text);
                writer.Write("&nbsp;");
            }

            writer.AddAttribute(HtmlTextWriterAttribute.Id, clientID + "Popup");

            string popupImageUrl = _menuUser.PopupImageUrl;
            if (!String.IsNullOrEmpty(popupImageUrl)) {
                writer.AddAttribute(HtmlTextWriterAttribute.Src, popupImageUrl);
                writer.AddAttribute(HtmlTextWriterAttribute.Alt,
                                    (!String.IsNullOrEmpty(text) ?
                                     text :
                                     SR.GetString(SR.WebPartMenu_DefaultDropDownAlternateText)),
                                    true);
                writer.AddStyleAttribute("vertical-align", "middle");
                writer.AddStyleAttribute(HtmlTextWriterStyle.BorderStyle, "none");
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();
            }
            else {
                // Render down arrow using windows font
                writer.AddStyleAttribute(HtmlTextWriterStyle.FontFamily, "Marlett");
                writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "8pt");
                writer.RenderBeginTag(HtmlTextWriterTag.Span);
                writer.Write("u");
                writer.RenderEndTag();
            }

            writer.RenderEndTag();  // Span

            _menuUser.OnEndRender(writer);
        }

        private void RenderMenuPopup(HtmlTextWriter writer, ICollection verbs, string clientID, WebPart associatedWebPart,
                                     WebPartManager webPartManager) {
            writer.AddAttribute(HtmlTextWriterAttribute.Id, clientID + "Menu");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            bool popupSpansFullExtent = true;
            WebPartMenuStyle menuStyle = _menuUser.MenuPopupStyle;
            if (menuStyle != null) {
                menuStyle.AddAttributesToRender(writer, _menuUser as WebControl);
                popupSpansFullExtent = menuStyle.Width.IsEmpty;
            }
            else {
                // generate attributes corresponding to defaults on WebPartMenuStyle
                writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
                writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "1");
                writer.AddStyleAttribute(HtmlTextWriterStyle.BorderCollapse, "collapse");
            }
            if (popupSpansFullExtent) {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Table);
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            writer.AddStyleAttribute(HtmlTextWriterStyle.WhiteSpace, "nowrap");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);

            bool isParentEnabled = associatedWebPart.Zone.IsEnabled;
            foreach (WebPartVerb verb in verbs) {
                Debug.Assert(verb != null);
                writer.RenderBeginTag(HtmlTextWriterTag.Div);

                string alt;
                if (associatedWebPart != null) {
                    alt = String.Format(CultureInfo.CurrentCulture, verb.Description, associatedWebPart.DisplayTitle);
                }
                else {
                    alt = verb.Description;
                }
                if (alt.Length != 0) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, alt);
                }
                bool isEnabled = isParentEnabled && verb.Enabled;

                // Special case help, export, etc.
                if (verb is WebPartHelpVerb) {
                    Debug.Assert(associatedWebPart != null);

                    string resolvedHelpUrl =
                        ((IUrlResolutionService)associatedWebPart).ResolveClientUrl(associatedWebPart.HelpUrl);

                    writer.AddAttribute(HtmlTextWriterAttribute.Href, "javascript:void(0)");
                    if (isEnabled) {
                        writer.AddAttribute(HtmlTextWriterAttribute.Onclick,
                                            "document.body.__wpm.ShowHelp('" +
                                            Util.QuoteJScriptString(resolvedHelpUrl) +
                                            "', " +
                                            ((int)associatedWebPart.HelpMode).ToString(CultureInfo.InvariantCulture) + ")");
                    }
                }
                else if (verb is WebPartExportVerb) {
                    Debug.Assert(associatedWebPart != null);

                    string exportUrl = webPartManager.GetExportUrl(associatedWebPart);

                    writer.AddAttribute(HtmlTextWriterAttribute.Href, "javascript:void(0)");
                    if (isEnabled) {
                        writer.AddAttribute(HtmlTextWriterAttribute.Onclick,
                                            "document.body.__wpm.ExportWebPart('" +
                                            Util.QuoteJScriptString(exportUrl) +
                                            ((associatedWebPart.ExportMode == WebPartExportMode.All) ?
                                                "', true, false)" :
                                                "', false, false)"));
                    }
                }
                else {
                    string target = _menuUser.PostBackTarget;
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, "javascript:void(0)");
                    if (isEnabled) {
                        string eventArgument = verb.EventArgument;
                        if (associatedWebPart != null) {
                            eventArgument = verb.GetEventArgument(associatedWebPart.ID);
                        }

                        string submitScript = null;
                        if (!String.IsNullOrEmpty(eventArgument)) {
                            submitScript = "document.body.__wpm.SubmitPage('" +
                                Util.QuoteJScriptString(target) +
                                "', '" +
                                Util.QuoteJScriptString(eventArgument) +
                                "');";

                            _menuUser.Page.ClientScript.RegisterForEventValidation(target, eventArgument);
                        }

                        string clientClickScript = null;
                        if (!String.IsNullOrEmpty(verb.ClientClickHandler)) {
                            clientClickScript = "document.body.__wpm.Execute('" +
                                Util.QuoteJScriptString(Util.EnsureEndWithSemiColon(verb.ClientClickHandler)) +
                                "')";
                        }

                        // There must be either an EventArgument or a ClientClickHandler
                        Debug.Assert(submitScript != null || clientClickScript != null);

                        string onclick = String.Empty;
                        if (submitScript != null && clientClickScript != null) {
                            onclick = "if(" + clientClickScript + "){" + submitScript + "}";
                        }
                        else if (submitScript != null) {
                            onclick = submitScript;
                        }
                        else if (clientClickScript != null) {
                            onclick = clientClickScript;
                        }

                        if (verb is WebPartCloseVerb) {
                            Debug.Assert(associatedWebPart != null);

                            // PERF: First check if this WebPart even has provider connection points
                            ProviderConnectionPointCollection connectionPoints =
                                webPartManager.GetProviderConnectionPoints(associatedWebPart);
                            if (connectionPoints != null && connectionPoints.Count > 0 &&
                                webPartManager.Connections.ContainsProvider(associatedWebPart)) {
                                onclick = "if(document.body.__wpmCloseProviderWarning.length == 0 || " +
                                    "confirm(document.body.__wpmCloseProviderWarning)){" + onclick + "}";
                            }
                        }
                        else if (verb is WebPartDeleteVerb) {
                            onclick = "if(document.body.__wpmDeleteWarning.length == 0 || " +
                                "confirm(document.body.__wpmDeleteWarning)){" + onclick + "}";
                        }
                        writer.AddAttribute(HtmlTextWriterAttribute.Onclick, onclick);
                    }
                }

                string disabledClass = "menuItem";
                if (!verb.Enabled) {
                    if (associatedWebPart.Zone.RenderingCompatibility < VersionUtil.Framework40) {
                        writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
                    }
                    else if (!String.IsNullOrEmpty(WebControl.DisabledCssClass)) {
                        disabledClass = WebControl.DisabledCssClass + " " + disabledClass;
                    }
                }
                writer.AddAttribute(HtmlTextWriterAttribute.Class, disabledClass);
                writer.RenderBeginTag(HtmlTextWriterTag.A);

                string img = verb.ImageUrl;
                if (img.Length != 0) {
                    img = _menuUser.UrlResolver.ResolveClientUrl(img);
                }
                else {
                    if (verb.Checked) {
                        img = _menuUser.CheckImageUrl;
                        if (img.Length == 0) {
                            img = DefaultCheckImageUrl;
                        }
                    }
                    else {
                        img = webPartManager.SpacerImageUrl;
                    }
                }
                writer.AddAttribute(HtmlTextWriterAttribute.Src, img);
                writer.AddAttribute(HtmlTextWriterAttribute.Alt, alt, true);
                writer.AddAttribute(HtmlTextWriterAttribute.Width, "16");
                writer.AddAttribute(HtmlTextWriterAttribute.Height, "16");
                writer.AddStyleAttribute(HtmlTextWriterStyle.BorderStyle, "none");
                writer.AddStyleAttribute("vertical-align", "middle");
                if (verb.Checked) {
                    Style checkImageStyle = _menuUser.CheckImageStyle;
                    if (checkImageStyle != null) {
                        checkImageStyle.AddAttributesToRender(writer, _menuUser as WebControl);
                    }
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();  // Img

                writer.Write("&nbsp;");
                writer.Write(verb.Text);
                writer.Write("&nbsp;");

                writer.RenderEndTag();  // A

                writer.RenderEndTag();  // Div
            }

            writer.RenderEndTag();  // Td
            writer.RenderEndTag();  // Tr
            writer.RenderEndTag();  // Table

            writer.RenderEndTag();  // Div
        }
    }
}
