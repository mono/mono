//------------------------------------------------------------------------------
// <copyright file="CatalogPartChrome.cs" company="Microsoft">
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
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public class CatalogPartChrome {

        private CatalogZoneBase _zone;

        // PERF: Cache these, since they are needed for every CatalogPart in the zone
        private Page _page;
        private Style _chromeStyleWithBorder;
        private Style _chromeStyleNoBorder;

        public CatalogPartChrome(CatalogZoneBase zone) {
            if (zone == null) {
                throw new ArgumentNullException("zone");
            }
            _zone = zone;
            _page = zone.Page;
        }

        protected CatalogZoneBase Zone {
            get {
                return _zone;
            }
        }

        protected virtual Style CreateCatalogPartChromeStyle(CatalogPart catalogPart, PartChromeType chromeType) {
            if (catalogPart == null) {
                throw new ArgumentNullException("catalogPart");
            }
            if ((chromeType < PartChromeType.Default) || (chromeType > PartChromeType.BorderOnly)) {
                throw new ArgumentOutOfRangeException("chromeType");
            }

            if (chromeType == PartChromeType.BorderOnly || chromeType == PartChromeType.TitleAndBorder) {
                if (_chromeStyleWithBorder == null) {
                    Style style = new Style();
                    style.CopyFrom(Zone.PartChromeStyle);

                    if (style.BorderStyle == BorderStyle.NotSet) {
                        style.BorderStyle = BorderStyle.Solid;
                    }
                    if (style.BorderWidth == Unit.Empty) {
                        style.BorderWidth = Unit.Pixel(1);
                    }
                    if (style.BorderColor == Color.Empty) {
                        style.BorderColor = Color.Black;
                    }

                    _chromeStyleWithBorder = style;
                }
                return _chromeStyleWithBorder;
            }
            else {
                if (_chromeStyleNoBorder == null) {
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

                    _chromeStyleNoBorder = style;
                }
                return _chromeStyleNoBorder;
            }
        }

        public virtual void PerformPreRender() {
        }

        public virtual void RenderCatalogPart(HtmlTextWriter writer, CatalogPart catalogPart) {
            if (catalogPart == null) {
                throw new ArgumentNullException("catalogPart");
            }

            PartChromeType chromeType = Zone.GetEffectiveChromeType(catalogPart);
            Style partChromeStyle = CreateCatalogPartChromeStyle(catalogPart, chromeType);

            // 
            if (!partChromeStyle.IsEmpty) {
                partChromeStyle.AddAttributesToRender(writer, Zone);
            }
            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            // Use CellPadding=2 to match WebPartChrome (VSWhidbey 324397)
            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "2");
            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");

            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");

            writer.RenderBeginTag(HtmlTextWriterTag.Table);

            if (chromeType == PartChromeType.TitleOnly || chromeType == PartChromeType.TitleAndBorder) {
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                // Can apply PartTitleStyle directly, since the title bar doesn't contain a nested table
                Style partTitleStyle = Zone.PartTitleStyle;
                if (!partTitleStyle.IsEmpty) {
                    partTitleStyle.AddAttributesToRender(writer, Zone);
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Td);

                RenderTitle(writer, catalogPart);

                writer.RenderEndTag();  // Td
                writer.RenderEndTag();  // Tr
            }

            if (catalogPart.ChromeState != PartChromeState.Minimized) {
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                Style partStyle = Zone.PartStyle;
                if (!partStyle.IsEmpty) {
                    partStyle.AddAttributesToRender(writer, Zone);
                }

                // For now, I don't think we should render extra padding here.  People writing custom
                // CatalogParts can add a margin to their contents if they want it.  This is not the
                // same as the WebPartChrome case, since for WebParts we allow people to use ServerControls,
                // that will likely not have a margin.  (VSWhidbey 324397)
                // writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, "5px");

                writer.RenderBeginTag(HtmlTextWriterTag.Td);

                // 
                RenderPartContents(writer, catalogPart);

                RenderItems(writer, catalogPart);

                writer.RenderEndTag();  // Td
                writer.RenderEndTag();  // Tr
            }

            writer.RenderEndTag();  // Table
        }

        private void RenderItem(HtmlTextWriter writer, WebPartDescription webPartDescription) {
            string description = webPartDescription.Description;
            if (String.IsNullOrEmpty(description)) {
                description = webPartDescription.Title;
            }

            RenderItemCheckBox(writer, webPartDescription.ID);

            writer.Write("&nbsp;");

            if (Zone.ShowCatalogIcons) {
                string icon = webPartDescription.CatalogIconImageUrl;
                if (!String.IsNullOrEmpty(icon)) {
                    RenderItemIcon(writer, icon, description);
                    writer.Write("&nbsp;");
                }
            }

            RenderItemText(writer, webPartDescription.ID, webPartDescription.Title, description);

            writer.WriteBreak();
        }

        private void RenderItemCheckBox(HtmlTextWriter writer, string value) {
            Zone.EditUIStyle.AddAttributesToRender(writer, Zone);
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "checkbox");
            writer.AddAttribute(HtmlTextWriterAttribute.Id, Zone.GetCheckBoxID(value));
            writer.AddAttribute(HtmlTextWriterAttribute.Name, Zone.CheckBoxName);
            writer.AddAttribute(HtmlTextWriterAttribute.Value, value);
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();  // Input

            if (_page != null) {
                _page.ClientScript.RegisterForEventValidation(Zone.CheckBoxName);
            }
        }

        private void RenderItemIcon(HtmlTextWriter writer, string iconUrl, string description) {
            System.Web.UI.WebControls.Image img = new System.Web.UI.WebControls.Image();
            img.AlternateText = description;

            // 
            img.ImageUrl = iconUrl;
            img.BorderStyle = BorderStyle.None;
            img.Page = _page;
            img.RenderControl(writer);
        }

        private void RenderItemText(HtmlTextWriter writer, string value, string text, string description) {
            Zone.LabelStyle.AddAttributesToRender(writer, Zone);
            writer.AddAttribute(HtmlTextWriterAttribute.For, Zone.GetCheckBoxID(value));
            writer.AddAttribute(HtmlTextWriterAttribute.Title, description, true /* fEncode */);
            writer.RenderBeginTag(HtmlTextWriterTag.Label);
            writer.WriteEncodedText(text);
            writer.RenderEndTag();
        }

        private void RenderItems(HtmlTextWriter writer, CatalogPart catalogPart) {
            WebPartDescriptionCollection availableWebParts = catalogPart.GetAvailableWebPartDescriptions();

            if (availableWebParts != null) {
                foreach (WebPartDescription webPartDescription in availableWebParts) {
                    RenderItem(writer, webPartDescription);
                }
            }
        }

        protected virtual void RenderPartContents(HtmlTextWriter writer, CatalogPart catalogPart) {
            if (catalogPart == null) {
                throw new ArgumentNullException("catalogPart");
            }
            catalogPart.RenderControl(writer);
        }

        private void RenderTitle(HtmlTextWriter writer, CatalogPart catalogPart) {
            Label label = new Label();
            label.Text = catalogPart.DisplayTitle;
            label.ToolTip = catalogPart.Description;
            label.Page = _page;
            label.RenderControl(writer);
        }
    }
}

