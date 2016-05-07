//------------------------------------------------------------------------------
// <copyright file="WebPartChrome.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Web.Handlers;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.Util;

    public class WebPartChrome {

        private const string titleSeparator = " - ";
        private const string descriptionSeparator = " - ";

        private WebPartManager _manager;
        private WebPartConnectionCollection _connections;
        private WebPartZoneBase _zone;

        // PERF: Cache these, since they are used on every call to FilterVerbs
        private Page _page;
        private bool _designMode;
        private bool _personalizationEnabled;
        private PersonalizationScope _personalizationScope;

        // PERF: Cache these, since they are needed for every WebPart in the zone
        private Style _chromeStyleWithBorder;
        private Style _chromeStyleNoBorder;
        private Style _titleTextStyle;
        private Style _titleStyleWithoutFontOrAlign;

        private int _cssStyleIndex;

        public WebPartChrome(WebPartZoneBase zone, WebPartManager manager) {
            if (zone == null) {
                throw new ArgumentNullException("zone");
            }
            _zone = zone;
            _page = zone.Page;
            _designMode = zone.DesignMode;
            _manager = manager;

            if (_designMode) {
                // Consider personalization to be enabled at design-time
                _personalizationEnabled = true;
            }
            else {
                _personalizationEnabled = (manager != null && manager.Personalization.IsModifiable);
            }

            if (manager != null) {
                _personalizationScope = manager.Personalization.Scope;
            }
            else {
                // Consider scope to be shared at design-time
                _personalizationScope = PersonalizationScope.Shared;
            }
        }

        // PERF: Cache the Connections collection on demand
        private WebPartConnectionCollection Connections {
            get {
                if (_connections == null) {
                    _connections = _manager.Connections;
                }
                return _connections;
            }
        }

        protected bool DragDropEnabled {
            get {
                return Zone.DragDropEnabled;
            }
        }

        protected WebPartManager WebPartManager {
            get {
                return _manager;
            }
        }

        protected WebPartZoneBase Zone {
            get {
                return _zone;
            }
        }

        private Style CreateChromeStyleNoBorder(Style partChromeStyle) {
            Style style = new Style();
            style.CopyFrom(Zone.PartChromeStyle);
            if (style.BorderStyle != BorderStyle.NotSet) {
                style.BorderStyle = BorderStyle.NotSet;
            }
            if (style.BorderWidth != Unit.Empty) {
                style.BorderWidth = Unit.Empty;
            }
            if (style.BorderColor != Color.Empty) {
                style.BorderColor = Color.Empty;
            }
            return style;
        }

        private Style CreateChromeStyleWithBorder(Style partChromeStyle) {
            Style style = new Style();
            style.CopyFrom(partChromeStyle);
            if (style.BorderStyle == BorderStyle.NotSet) {
                style.BorderStyle = BorderStyle.Solid;
            }
            if (style.BorderWidth == Unit.Empty) {
                style.BorderWidth = Unit.Pixel(1);
            }
            if (style.BorderColor == Color.Empty) {
                style.BorderColor = Color.Black;
            }
            return style;
        }

        private Style CreateTitleTextStyle(Style partTitleStyle) {
            Style style = new Style();
            if (partTitleStyle.ForeColor != Color.Empty) {
                style.ForeColor = partTitleStyle.ForeColor;
            }
            style.Font.CopyFrom(partTitleStyle.Font);
            return style;
        }

        private Style CreateTitleStyleWithoutFontOrAlign(Style partTitleStyle) {
            // Need to remove font info from TitleStyle.  We only want the font
            // info to apply to the title text, not the whole title bar table.
            // (NDPWhidbey 27755)
            // Use plain style so we don't copy alignment or wrap from TableItemStyle
            Style style = new Style();
            style.CopyFrom(partTitleStyle);
            style.Font.Reset();
            if (style.ForeColor != Color.Empty) {
                style.ForeColor = Color.Empty;
            }
            return style;
        }

        protected virtual Style CreateWebPartChromeStyle(WebPart webPart, PartChromeType chromeType) {
            if (webPart == null) {
                throw new ArgumentNullException("webPart");
            }
            if ((chromeType < PartChromeType.Default) || (chromeType > PartChromeType.BorderOnly)) {
                throw new ArgumentOutOfRangeException("chromeType");
            }

            // PERF: Cache these, since they are needed for every WebPart in the zone, and only vary
            // if one of the WebParts is selected
            Style webPartChromeStyle;
            if (chromeType == PartChromeType.BorderOnly || chromeType == PartChromeType.TitleAndBorder) {
                if (_chromeStyleWithBorder == null) {
                    _chromeStyleWithBorder = CreateChromeStyleWithBorder(Zone.PartChromeStyle);
                }
                webPartChromeStyle = _chromeStyleWithBorder;
            }
            else {
                if (_chromeStyleNoBorder == null) {
                    _chromeStyleNoBorder = CreateChromeStyleNoBorder(Zone.PartChromeStyle);
                }
                webPartChromeStyle = _chromeStyleNoBorder;
            }

            // add SelectedPartChromeStyle
            if (WebPartManager != null && webPart == WebPartManager.SelectedWebPart) {
                Style style = new Style();
                style.CopyFrom(webPartChromeStyle);
                style.CopyFrom(Zone.SelectedPartChromeStyle);
                return style;
            }
            else {
                return webPartChromeStyle;
            }
        }

        private string GenerateDescriptionText(WebPart webPart) {
            string descriptionText = webPart.DisplayTitle;

            string description = webPart.Description;
            if (!String.IsNullOrEmpty(description)) {
                descriptionText += descriptionSeparator + description;
            }

            return descriptionText;
        }

        private string GenerateTitleText(WebPart webPart) {
            string titleText = webPart.DisplayTitle;

            string subtitle = webPart.Subtitle;
            if (!String.IsNullOrEmpty(subtitle)) {
                titleText += titleSeparator + subtitle;
            }

            return titleText;
        }

        protected string GetWebPartChromeClientID(WebPart webPart) {
            if (webPart == null) {
                throw new ArgumentNullException("webPart");
            }
            return webPart.WholePartID;
        }

        protected string GetWebPartTitleClientID(WebPart webPart) {
            if (webPart == null) {
                throw new ArgumentNullException("webPart");
            }
            return webPart.TitleBarID;
        }

        protected virtual WebPartVerbCollection GetWebPartVerbs(WebPart webPart) {
            if (webPart == null) {
                throw new ArgumentNullException("webPart");
            }
            return Zone.VerbsForWebPart(webPart);
        }

        protected virtual WebPartVerbCollection FilterWebPartVerbs(WebPartVerbCollection verbs, WebPart webPart) {
            if (verbs == null) {
                throw new ArgumentNullException("verbs");
            }
            if (webPart == null) {
                throw new ArgumentNullException("webPart");
            }

            WebPartVerbCollection filteredVerbs = new WebPartVerbCollection();

            foreach (WebPartVerb verb in verbs) {
                if (ShouldRenderVerb(verb, webPart)) {
                    filteredVerbs.Add(verb);
                }
            }

            return filteredVerbs;
        }

        private void RegisterStyle(Style style) {
            Debug.Assert(_page.SupportsStyleSheets);
            // The style should not have already been registered
            Debug.Assert(style.RegisteredCssClass.Length == 0);

            if (!style.IsEmpty) {
                string name = Zone.ClientID + "_" + _cssStyleIndex++.ToString(NumberFormatInfo.InvariantInfo);
                _page.Header.StyleSheet.CreateStyleRule(style, Zone, "." + name);
                style.SetRegisteredCssClass(name);
            }
        }

        public virtual void PerformPreRender() {
            if (_page != null && _page.SupportsStyleSheets) {
                Style partChromeStyle = Zone.PartChromeStyle;
                Style partTitleStyle = Zone.PartTitleStyle;

                _chromeStyleWithBorder = CreateChromeStyleWithBorder(partChromeStyle);
                RegisterStyle(_chromeStyleWithBorder);

                _chromeStyleNoBorder = CreateChromeStyleNoBorder(partChromeStyle);
                RegisterStyle(_chromeStyleNoBorder);

                _titleTextStyle = CreateTitleTextStyle(partTitleStyle);
                RegisterStyle(_titleTextStyle);

                _titleStyleWithoutFontOrAlign = CreateTitleStyleWithoutFontOrAlign(partTitleStyle);
                RegisterStyle(_titleStyleWithoutFontOrAlign);

                if (Zone.RenderClientScript && (Zone.WebPartVerbRenderMode == WebPartVerbRenderMode.Menu) && Zone.Menu != null) {
                    Zone.Menu.RegisterStyles();
                }
            }
        }

        protected virtual void RenderPartContents(HtmlTextWriter writer, WebPart webPart) {
            if (!String.IsNullOrEmpty(webPart.ConnectErrorMessage)) {
                if (!Zone.ErrorStyle.IsEmpty) {
                    Zone.ErrorStyle.AddAttributesToRender(writer, Zone);
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                writer.WriteEncodedText(webPart.ConnectErrorMessage);
                writer.RenderEndTag();  // Div
            }
            else {
                webPart.RenderControl(writer);
            }
        }

        // Made non-virtual, since it may be confusing to override this method when it's style
        // is rendered by RenderWebPart.
        private void RenderTitleBar(HtmlTextWriter writer, WebPart webPart) {
            // Can't apply title style here, since the border would be inside the cell padding
            // of the parent td.
            // titleStyle.AddAttributesToRender(writer, this);
            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");

            // Want table to span full width of part for drag and drop
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");

            writer.RenderBeginTag(HtmlTextWriterTag.Table);
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            int colspan = 1;
            bool showTitleIcons = Zone.ShowTitleIcons;
            string titleIconImageUrl = null;
            if (showTitleIcons) {
                titleIconImageUrl = webPart.TitleIconImageUrl;
                if (!String.IsNullOrEmpty(titleIconImageUrl)) {
                    colspan++;
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    RenderTitleIcon(writer, webPart);
                    writer.RenderEndTag();  // Td
                }
            }

            // title text
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");

            TableItemStyle titleStyle = Zone.PartTitleStyle;

            // Render align and wrap from the TableItemStyle (copied from TableItemStyle.cs)
            if (titleStyle.Wrap == false) {
                writer.AddStyleAttribute(HtmlTextWriterStyle.WhiteSpace, "nowrap");
            }
            HorizontalAlign hAlign = titleStyle.HorizontalAlign;
            if (hAlign != HorizontalAlign.NotSet) {
                TypeConverter hac = TypeDescriptor.GetConverter(typeof(HorizontalAlign));
                writer.AddAttribute(HtmlTextWriterAttribute.Align, hac.ConvertToString(hAlign).ToLower(CultureInfo.InvariantCulture));
            }
            VerticalAlign vAlign = titleStyle.VerticalAlign;
            if (vAlign != VerticalAlign.NotSet) {
                TypeConverter vac = TypeDescriptor.GetConverter(typeof(VerticalAlign));
                writer.AddAttribute(HtmlTextWriterAttribute.Valign, vac.ConvertToString(vAlign).ToLower(CultureInfo.InvariantCulture));
            }

            if (Zone.RenderClientScript) {
                writer.AddAttribute(HtmlTextWriterAttribute.Id, GetWebPartTitleClientID(webPart));
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Td);

            if (showTitleIcons) {
                if (!String.IsNullOrEmpty(titleIconImageUrl)) {
                    // Render &nbsp; so there is a space between the icon and the title text
                    // Can't be rendered in RenderTitleIcon(), since we want the space to be a valid drag target
                    writer.Write("&nbsp;");
                }
            }

            RenderTitleText(writer, webPart);

            writer.RenderEndTag();  // Td

            RenderVerbsInTitleBar(writer, webPart, colspan);

            writer.RenderEndTag();  // Tr
            writer.RenderEndTag();  // Table
        }

        private void RenderTitleIcon(HtmlTextWriter writer, WebPart webPart) {
            // 
            writer.AddAttribute(HtmlTextWriterAttribute.Src, Zone.ResolveClientUrl(webPart.TitleIconImageUrl) );
            // Use "DisplayTitle - Description" as the alt tag (VSWhidbey 376241)
            writer.AddAttribute(HtmlTextWriterAttribute.Alt, GenerateDescriptionText(webPart));
            writer.RenderBeginTag(HtmlTextWriterTag.Img);
            writer.RenderEndTag();  // Img
        }

        // PERF: Implement RenderTitleText() without using server controls
        private void RenderTitleText(HtmlTextWriter writer, WebPart webPart) {
            // PERF: Cache this, since it is needed for every WebPart in the zone
            if (_titleTextStyle == null) {
                _titleTextStyle = CreateTitleTextStyle(Zone.PartTitleStyle);
            }

            if (!_titleTextStyle.IsEmpty) {
                _titleTextStyle.AddAttributesToRender(writer, Zone);
            }

            // Render "DisplayTitle - Description" as tooltip (VSWhidbey 367041)
            writer.AddAttribute(HtmlTextWriterAttribute.Title, GenerateDescriptionText(webPart), true);

            // 
            string url = webPart.TitleUrl;
            string text = GenerateTitleText(webPart);
            if (!String.IsNullOrEmpty(url) && !DragDropEnabled) {
                writer.AddAttribute(HtmlTextWriterAttribute.Href, Zone.ResolveClientUrl(url));
                writer.RenderBeginTag(HtmlTextWriterTag.A);
            }
            else {
                writer.RenderBeginTag(HtmlTextWriterTag.Span);
            }

            writer.WriteEncodedText(text);
            writer.RenderEndTag(); // A || Span

            // PERF: Always render &nbsp; even if no verbs will be rendered
            writer.Write("&nbsp;");
        }

        private void RenderVerb(HtmlTextWriter writer, WebPart webPart, WebPartVerb verb) {
            WebControl verbControl;
            bool isEnabled = Zone.IsEnabled && verb.Enabled;

            ButtonType verbButtonType = Zone.TitleBarVerbButtonType;

            if (verb == Zone.HelpVerb) {
                // 
                string resolvedHelpUrl = Zone.ResolveClientUrl(webPart.HelpUrl);

                // 
                if (verbButtonType == ButtonType.Button) {
                    ZoneButton button = new ZoneButton(Zone, null);

                    if (isEnabled) {
                        if (Zone.RenderClientScript) {
                            button.OnClientClick = "__wpm.ShowHelp('" +
                                Util.QuoteJScriptString(resolvedHelpUrl) +
                                "', " +
                                ((int)webPart.HelpMode).ToString(CultureInfo.InvariantCulture) +
                                ");return;";
                        }
                        else {
                            if (webPart.HelpMode != WebPartHelpMode.Navigate) {
                                button.OnClientClick = "window.open('" +
                                    Util.QuoteJScriptString(resolvedHelpUrl) +
                                    "', '_blank', 'scrollbars=yes,resizable=yes,status=no,toolbar=no,menubar=no,location=no');return;";
                            }
                            else {
                                button.OnClientClick = "window.location.href='" +
                                    Util.QuoteJScriptString(resolvedHelpUrl) +
                                    "';return;";
                            }
                        }
                    }
                    button.Text = verb.Text;
                    verbControl = button;
                }
                else {
                    HyperLink hyperLink = new HyperLink();

                    switch (webPart.HelpMode) {
                        case WebPartHelpMode.Modal:
                            if (!Zone.RenderClientScript) {
                                goto case WebPartHelpMode.Modeless;
                            }
                            hyperLink.NavigateUrl = "javascript:__wpm.ShowHelp('" +
                                Util.QuoteJScriptString(resolvedHelpUrl) +
                                "', 0)";
                            break;
                        case WebPartHelpMode.Modeless:
                            hyperLink.NavigateUrl = resolvedHelpUrl;
                            hyperLink.Target = "_blank";
                            break;
                        case WebPartHelpMode.Navigate:
                            hyperLink.NavigateUrl = resolvedHelpUrl;
                            break;
                    }

                    hyperLink.Text = verb.Text;
                    if (verbButtonType == ButtonType.Image) {
                        hyperLink.ImageUrl = verb.ImageUrl;
                    }
                    verbControl = hyperLink;
                }
            }
            else if (verb == Zone.ExportVerb) {
                string exportUrl = _manager.GetExportUrl(webPart);
                if (verbButtonType == ButtonType.Button) {
                    ZoneButton button = new ZoneButton(Zone, String.Empty);
                    button.Text = verb.Text;

                    if (isEnabled) {
                        if ((webPart.ExportMode == WebPartExportMode.All) &&
                            (_personalizationScope == PersonalizationScope.User)) {
                            if (Zone.RenderClientScript) {
                                button.OnClientClick = "__wpm.ExportWebPart('" +
                                    Util.QuoteJScriptString(exportUrl) +
                                    "', true, false);return false;";
                            }
                            else {
                                button.OnClientClick = "if(__wpmExportWarning.length == 0 || "
                                    + "confirm(__wpmExportWarning)){window.location='" +
                                    Util.QuoteJScriptString(exportUrl) +
                                    "';}return false;";
                            }
                        }
                        else {
                            button.OnClientClick = "window.location='" +
                                Util.QuoteJScriptString(exportUrl) +
                                "';return false;";
                        }
                    }

                    verbControl = button;
                }
                else {
                    // Special case for export which must be a plain HyperLink
                    // (href=javascript:void(0) would ruin any redirecting script)
                    HyperLink link = new HyperLink();
                    link.Text = verb.Text;
                    if (verbButtonType == ButtonType.Image) {
                        link.ImageUrl = verb.ImageUrl;
                    }
                    link.NavigateUrl = exportUrl;
                    if (webPart.ExportMode == WebPartExportMode.All) {
                        // Confirm before exporting
                        if (Zone.RenderClientScript) {
                            link.Attributes.Add("onclick", "return __wpm.ExportWebPart('', true, true)");
                        }
                        else {
                            string onclick = "return (__wpmExportWarning.length == 0 || confirm(__wpmExportWarning))";
                            link.Attributes.Add("onclick", onclick);
                        }
                    }
                    verbControl = link;
                }
            }
            else {
                string eventArgument = verb.GetEventArgument(webPart.ID);
                string clientClickHandler = verb.ClientClickHandler;

                if (verbButtonType == ButtonType.Button) {
                    ZoneButton button = new ZoneButton(Zone, eventArgument);
                    button.Text = verb.Text;
                    if (!String.IsNullOrEmpty(clientClickHandler) && isEnabled) {
                        button.OnClientClick = clientClickHandler;
                    }
                    verbControl = button;
                }
                else {
                    ZoneLinkButton linkButton = new ZoneLinkButton(Zone, eventArgument);
                    linkButton.Text = verb.Text;
                    if (verbButtonType == ButtonType.Image) {
                        linkButton.ImageUrl = verb.ImageUrl;
                    }
                    if (!String.IsNullOrEmpty(clientClickHandler) && isEnabled) {
                        linkButton.OnClientClick = clientClickHandler;
                    }
                    verbControl = linkButton;
                }

                if (_manager != null && isEnabled) {
                    if (verb == Zone.CloseVerb) {
                        // PERF: First check if this WebPart even has provider connection points
                        ProviderConnectionPointCollection connectionPoints = _manager.GetProviderConnectionPoints(webPart);
                        if (connectionPoints != null && connectionPoints.Count > 0 &&
                            Connections.ContainsProvider(webPart)) {
                            string onclick = "if (__wpmCloseProviderWarning.length >= 0 && " +
                                "!confirm(__wpmCloseProviderWarning)) { return false; }";
                            verbControl.Attributes.Add("onclick", onclick);
                        }
                    }
                    else if (verb == Zone.DeleteVerb) {
                        string onclick = "if (__wpmDeleteWarning.length >= 0 && !confirm(__wpmDeleteWarning)) { return false; }";
                        verbControl.Attributes.Add("onclick", onclick);
                    }
                }
            }

            verbControl.ApplyStyle(Zone.TitleBarVerbStyle);
            verbControl.ToolTip = String.Format(CultureInfo.CurrentCulture, verb.Description, webPart.DisplayTitle);
            verbControl.Enabled = verb.Enabled;
            verbControl.Page = _page;
            verbControl.RenderControl(writer);
        }

        private void RenderVerbs(HtmlTextWriter writer, WebPart webPart, WebPartVerbCollection verbs) {
            if (verbs == null) {
                throw new ArgumentNullException("verbs");
            }

            WebPartVerb priorVerb = null;
            foreach (WebPartVerb verb in verbs) {
                // If you are rendering as a linkbutton, OR the prior verb rendered as a linkbutton,
                // render an "&nbsp;" prior to yourself.  This ensures that all linkbuttons are preceeded
                // and followed by a space.
                if (priorVerb != null && (VerbRenderedAsLinkButton(verb) || VerbRenderedAsLinkButton(priorVerb))) {
                    writer.Write("&nbsp;");
                }
                RenderVerb(writer, webPart, verb);
                priorVerb = verb;
            }
        }

        private void RenderVerbsInTitleBar(HtmlTextWriter writer, WebPart webPart, int colspan) {
            WebPartVerbCollection verbs = GetWebPartVerbs(webPart);
            verbs = FilterWebPartVerbs(verbs, webPart);

            if (verbs != null && verbs.Count > 0) {
                writer.AddStyleAttribute(HtmlTextWriterStyle.WhiteSpace, "nowrap");
                colspan++;

                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                if (Zone.RenderClientScript && (Zone.WebPartVerbRenderMode == WebPartVerbRenderMode.Menu) && Zone.Menu != null) {
                    if (_designMode) {
                        Zone.Menu.Render(writer, webPart.WholePartID + "Verbs");
                    }
                    else {
                        // If Zone.RenderClientScript, then WebPartManager must not be null
                        Debug.Assert(WebPartManager != null);
                        Zone.Menu.Render(writer, verbs, webPart.WholePartID + "Verbs", webPart, WebPartManager);
                    }
                }
                else {
                    RenderVerbs(writer, webPart, verbs);
                }

                writer.RenderEndTag();  // Td
            }
        }

        public virtual void RenderWebPart(HtmlTextWriter writer, WebPart webPart) {
            if (webPart == null) {
                throw new ArgumentNullException("webPart");
            }

            bool vertical = (Zone.LayoutOrientation == Orientation.Vertical);

            PartChromeType chromeType = Zone.GetEffectiveChromeType(webPart);
            Style partChromeStyle = CreateWebPartChromeStyle(webPart, chromeType);

            // 
            if (!partChromeStyle.IsEmpty) {
                partChromeStyle.AddAttributesToRender(writer, Zone);
            }

            // Render CellPadding=2 so there is a 2 pixel gap between the border and the title/body
            // of the WebPart.  Can't render CellSpacing=2, since we want the backcolor of the title
            // bar to fill the title bar, and backcolor is not rendered in the CellSpacing.
            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "2");
            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");

            if (vertical) {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
            }
            else if (webPart.ChromeState != PartChromeState.Minimized) {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "100%");
            }

            if (Zone.RenderClientScript) {
                writer.AddAttribute(HtmlTextWriterAttribute.Id, GetWebPartChromeClientID(webPart));
            }

            if (!_designMode && webPart.Hidden && WebPartManager != null &&
                !WebPartManager.DisplayMode.ShowHiddenWebParts) {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Table);

            if (chromeType == PartChromeType.TitleOnly || chromeType == PartChromeType.TitleAndBorder) {
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                // PERF: Cache this, since it is needed for every WebPart in the zone
                if (_titleStyleWithoutFontOrAlign == null) {
                    _titleStyleWithoutFontOrAlign = CreateTitleStyleWithoutFontOrAlign(Zone.PartTitleStyle);
                }

                // Need to apply title style here (at least backcolor and border) so the backcolor
                // and border include the cell padding on the td.
                // Should not apply font style here, since we don't want verbs to use this
                // font style.  In IE compat mode, the font style would not be inherited anyway,
                // But in IE strict mode the font style would be inherited.
                if (!_titleStyleWithoutFontOrAlign.IsEmpty) {
                    _titleStyleWithoutFontOrAlign.AddAttributesToRender(writer, Zone);
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Td);

                RenderTitleBar(writer, webPart);

                writer.RenderEndTag();  // Td
                writer.RenderEndTag();  // Tr
            }

            // Render the contents of minimized WebParts with display:none, instead of not rendering
            // the contents at all.  The contents may need to be rendered for client-side connections
            // or other client-side features.  Also allows child controls to maintain their postback
            // values between requests while the WebPart is minimized.
            if (webPart.ChromeState == PartChromeState.Minimized) {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            if (!vertical) {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "100%");
                writer.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
            }

            Style partStyle = Zone.PartStyle;
            if (!partStyle.IsEmpty) {
                partStyle.AddAttributesToRender(writer, Zone);
            }

            // Add some extra padding around the WebPart contents (VSWhidbey 324397)
            writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, Zone.PartChromePadding.ToString());
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            RenderPartContents(writer, webPart);
            writer.RenderEndTag();  // Td
            writer.RenderEndTag();  // Tr

            writer.RenderEndTag();  // Table
        }

        private bool ShouldRenderVerb(WebPartVerb verb, WebPart webPart) {
            // PERF: Consider caching the Zone.*Verb properties

            // Can have null verbs in the CreateVerbs or WebPart.Verbs collections
            if (verb == null) {
                return false;
            }

            if (!verb.Visible) {
                return false;
            }

            if (verb == Zone.CloseVerb) {
                if (!_personalizationEnabled || !webPart.AllowClose || !Zone.AllowLayoutChange) {
                    return false;
                }
            }

            if (verb == Zone.ConnectVerb) {
                if (WebPartManager != null) {
                    if ((WebPartManager.DisplayMode != WebPartManager.ConnectDisplayMode) ||
                        (webPart == WebPartManager.SelectedWebPart) ||
                        !webPart.AllowConnect) {
                        return false;
                    }

                    // Don't render Connect verb if web part has no connection points
                    ConsumerConnectionPointCollection consumerConnectionPoints =
                        WebPartManager.GetEnabledConsumerConnectionPoints(webPart);
                    ProviderConnectionPointCollection providerConnectionPoints =
                        WebPartManager.GetEnabledProviderConnectionPoints(webPart);
                    if ((consumerConnectionPoints == null || consumerConnectionPoints.Count == 0) &&
                        (providerConnectionPoints == null || providerConnectionPoints.Count == 0)) {
                        return false;
                    }
                }

            }

            if (verb == Zone.DeleteVerb) {
                if (!_personalizationEnabled ||
                    !Zone.AllowLayoutChange ||
                    webPart.IsStatic ||
                    (webPart.IsShared && _personalizationScope == PersonalizationScope.User) ||
                    (WebPartManager != null && !WebPartManager.DisplayMode.AllowPageDesign)) {
                    return false;
                }
            }

            if (verb == Zone.EditVerb) {
                if (WebPartManager != null &&
                    ((WebPartManager.DisplayMode != WebPartManager.EditDisplayMode) ||
                     (webPart == WebPartManager.SelectedWebPart))) {
                    return false;
                }
            }

            if (verb == Zone.HelpVerb) {
                if (String.IsNullOrEmpty(webPart.HelpUrl)) {
                    return false;
                }
            }

            if (verb == Zone.MinimizeVerb) {
                if (!_personalizationEnabled ||
                    webPart.ChromeState == PartChromeState.Minimized ||
                    !webPart.AllowMinimize ||
                    !Zone.AllowLayoutChange) {
                    return false;
                }
            }

            if (verb == Zone.RestoreVerb) {
                if (!_personalizationEnabled ||
                    webPart.ChromeState == PartChromeState.Normal ||
                    !Zone.AllowLayoutChange) {
                    return false;
                }
            }

            if (verb == Zone.ExportVerb) {
                if (!_personalizationEnabled ||
                    webPart.ExportMode == WebPartExportMode.None) {
                    return false;
                }
            }

            return true;
        }

        private bool VerbRenderedAsLinkButton(WebPartVerb verb) {
            if (Zone.TitleBarVerbButtonType == ButtonType.Link) {
                return true;
            }

            if (String.IsNullOrEmpty(verb.ImageUrl)) {
                return true;
            }

            return false;
        }

    }
}
