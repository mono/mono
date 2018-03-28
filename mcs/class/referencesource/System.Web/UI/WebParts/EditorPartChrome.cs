//------------------------------------------------------------------------------
// <copyright file="EditorPartChrome.cs" company="Microsoft">
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

    public class EditorPartChrome {

        private EditorZoneBase _zone;

        // PERF: Cache these, since they are needed for every EditorPart in the zone
        private Style _chromeStyleNoBorder;
        private Style _titleTextStyle;

        public EditorPartChrome(EditorZoneBase zone) {
            if (zone == null) {
                throw new ArgumentNullException("zone");
            }
            _zone = zone;
        }

        protected EditorZoneBase Zone {
            get {
                return _zone;
            }
        }

        protected virtual Style CreateEditorPartChromeStyle(EditorPart editorPart, PartChromeType chromeType) {
            if (editorPart == null) {
                throw new ArgumentNullException("editorPart");
            }
            if ((chromeType < PartChromeType.Default) || (chromeType > PartChromeType.BorderOnly)) {
                throw new ArgumentOutOfRangeException("chromeType");
            }

            // PERF: Cache these, since they are needed for every EditorPart in the zone.
            if (chromeType == PartChromeType.BorderOnly || chromeType == PartChromeType.TitleAndBorder) {
                // We don't want to set any border styles for ChromeType of TitleAndBorder or BorderOnly,
                // since the FrameSet has a default border, and it will use XP themes as long as no border styles
                // are set.
                // PERF: Just return the Zone.PartChromeStyle directly without making a copy
                return Zone.PartChromeStyle;
            }
            else {
                if (_chromeStyleNoBorder == null) {
                    Style style = new Style();

                    // create copy of PartChromeStyle so we can modify it
                    style.CopyFrom(Zone.PartChromeStyle);

                    if (style.BorderStyle != BorderStyle.None) {
                        style.BorderStyle = BorderStyle.None;
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

        public virtual void RenderEditorPart(HtmlTextWriter writer, EditorPart editorPart) {
            if (editorPart == null) {
                throw new ArgumentNullException("editorPart");
            }

            PartChromeType chromeType = Zone.GetEffectiveChromeType(editorPart);
            Style partChromeStyle = CreateEditorPartChromeStyle(editorPart, chromeType);

            // Apply ChromeStyle to the Fieldset
            if (!partChromeStyle.IsEmpty) {
                partChromeStyle.AddAttributesToRender(writer, Zone);
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Fieldset);

            // Use ChromeType to determine whether to render the legend
            if (chromeType == PartChromeType.TitleAndBorder || chromeType == PartChromeType.TitleOnly) {
                RenderTitle(writer, editorPart);
            }

            if (editorPart.ChromeState != PartChromeState.Minimized) {
                // Apply PartStyle to a <div> around the part rendering
                Style partStyle = Zone.PartStyle;
                if (!partStyle.IsEmpty) {
                    partStyle.AddAttributesToRender(writer, Zone);
                }

                // We want to have 5 pixels of spacing aroung the EditorPart contents.  There are
                // 3 ways to accomplish this:
                // 1. <fieldset style="padding:5px"> - This is bad because it adds 5px of space above
                //    the legend.  It also makes the fieldset too wide.
                // 2. <fieldset><div style="padding:5px"> - This is bad because the PartStyle-BackColor
                //    will now span the whole width of the legend.  For consistency with WebPartChrome,
                //    we want the PartChromeStyle-BackColor to show in the 5px of space around the contents.
                // 3. <fieldset><div style="margin:5px"> - This is the best option.
                //
                // For now, I don't think we should render a margin here.  People writing custom
                // EditorParts can add a margin to their contents if they want it.  This is not the
                // same as the WebPartChrome case, since for WebParts we allow people to use ServerControls,
                // that will likely not have a margin.  (VSWhidbey 324397)
                // writer.AddStyleAttribute(HtmlTextWriterStyle.Margin, "5px");

                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                RenderPartContents(writer, editorPart);
                writer.RenderEndTag();  // Div
            }

            writer.RenderEndTag();  // Fieldset
        }

        protected virtual void RenderPartContents(HtmlTextWriter writer, EditorPart editorPart) {
            // The AccessKey is rendered by the chrome on the <legend> tag, so we don't want
            // the EditorPart to render it on its own tags.
            string accessKey = editorPart.AccessKey;
            if (!String.IsNullOrEmpty(accessKey)) {
                editorPart.AccessKey = String.Empty;
            }
            editorPart.RenderControl(writer);
            if (!String.IsNullOrEmpty(accessKey)) {
                editorPart.AccessKey = accessKey;
            }
        }

        private void RenderTitle(HtmlTextWriter writer, EditorPart editorPart) {
            string displayTitle = editorPart.DisplayTitle;

            if (String.IsNullOrEmpty(displayTitle)) {
                return;
            }

            // Apply TitleStyle to the Legend
            TableItemStyle titleTableItemStyle = Zone.PartTitleStyle;

            // PERF: Cache this, since it is needed for every EditorPart in the zone
            if (_titleTextStyle == null) {
                // Need to copy the TableItemStyle to a plain Style, since we are going to apply it to
                // the <legend> tag, which is not a table item.  We ignore the horizontal align,
                // vertical align, and nowrap properties.
                Style style = new Style();
                style.CopyFrom(titleTableItemStyle);
                _titleTextStyle = style;
            }

            if (!_titleTextStyle.IsEmpty) {
                _titleTextStyle.AddAttributesToRender(writer, Zone);
            }

            string description = editorPart.Description;
            if (!String.IsNullOrEmpty(description)) {
                writer.AddAttribute(HtmlTextWriterAttribute.Title, description);
            }

            string accessKey = editorPart.AccessKey;
            if (!String.IsNullOrEmpty(accessKey)) {
                writer.AddAttribute(HtmlTextWriterAttribute.Accesskey, accessKey);
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Legend);
            writer.Write(displayTitle);
            writer.RenderEndTag();  // Legend
        }
    }
}
