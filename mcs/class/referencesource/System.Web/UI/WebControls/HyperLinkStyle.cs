//------------------------------------------------------------------------------
// <copyright file="TreeNode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Text;
    using System.Web.UI;
    using System.Web.Util;

    // This class renders selectively attributes from the owner class, filtering out anything not font or forecolor
    internal sealed class HyperLinkStyle : Style {
        private bool _doNotRenderDefaults = false;
        private Style _owner; // This is a style because of the newed IsSet (we need the Style.IsSet, not the new one)

        public HyperLinkStyle(Style owner) {
            _owner = owner;
        }

        public bool DoNotRenderDefaults {
            get {
                return _doNotRenderDefaults;
            }
            set {
                _doNotRenderDefaults = value;
            }
        }

        public sealed override bool IsEmpty {
            get {
                return  (RegisteredCssClass.Length == 0) && 
                    !(_owner.IsSet(PROP_CSSCLASS) ||
                    _owner.IsSet(PROP_FORECOLOR) ||
                    _owner.IsSet(PROP_FONT_NAMES) ||
                    _owner.IsSet(PROP_FONT_SIZE) ||
                    _owner.IsSet(PROP_FONT_BOLD) ||
                    _owner.IsSet(PROP_FONT_ITALIC) ||
                    _owner.IsSet(PROP_FONT_UNDERLINE) ||
                    _owner.IsSet(PROP_FONT_OVERLINE) ||
                    _owner.IsSet(PROP_FONT_STRIKEOUT));
            }
        }

        public sealed override void AddAttributesToRender(HtmlTextWriter writer, WebControl owner) {
            string cssClass = String.Empty;
            bool renderInlineStyle = true;

            if (_owner.IsSet(PROP_CSSCLASS)) {
                cssClass = _owner.CssClass;
            }
            if (RegisteredCssClass.Length != 0) {
                renderInlineStyle = false;
                if (cssClass.Length != 0) {
                    cssClass = cssClass + " " + RegisteredCssClass;
                }
                else {
                    cssClass = RegisteredCssClass;
                }
            }

            if (cssClass.Length > 0) {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, cssClass);
            }

            if (renderInlineStyle) {
                CssStyleCollection styleAttributes = GetStyleAttributes(owner);
                styleAttributes.Render(writer);
            }
        }

        protected override sealed void FillStyleAttributes(CssStyleCollection attributes, IUrlResolutionService urlResolver) {
            Debug.Assert(_owner != null);

            StateBag viewState = ViewState;

            Color c;

            // ForeColor
            if (_owner.IsSet(PROP_FORECOLOR)) {
                c = _owner.ForeColor;
                if (!c.IsEmpty) {
                    attributes.Add(HtmlTextWriterStyle.Color, ColorTranslator.ToHtml(c));
                }
            }
            // Not defaulting to black anymore for not entirely satisfying but reasonable reasons (VSWhidbey 356729)

            // need to call the property get in case we have font properties from view state and have not
            // created the font object
            FontInfo font = _owner.Font;

            // Font.Names
            string[] names = font.Names;
            if (names.Length > 0) {
                attributes.Add(HtmlTextWriterStyle.FontFamily, String.Join(",", names));
            }

            // Font.Size
            FontUnit fu = font.Size;
            if (fu.IsEmpty == false) {
                attributes.Add(HtmlTextWriterStyle.FontSize, fu.ToString(CultureInfo.InvariantCulture));
            }

            // Font.Bold
            if (_owner.IsSet(PROP_FONT_BOLD)) {
                if (font.Bold) {
                    attributes.Add(HtmlTextWriterStyle.FontWeight, "bold");
                }
                else {
                    attributes.Add(HtmlTextWriterStyle.FontWeight, "normal");
                }
            }

            // Font.Italic
            if (_owner.IsSet(PROP_FONT_ITALIC)) {
                if (font.Italic == true) {
                    attributes.Add(HtmlTextWriterStyle.FontStyle, "italic");
                }
                else {
                    attributes.Add(HtmlTextWriterStyle.FontStyle, "normal");
                }
            }

            string textDecoration = String.Empty;
            if (font.Underline) {
                textDecoration = "underline";
            }
            if (font.Overline) {
                textDecoration += " overline";
            }
            if (font.Strikeout) {
                textDecoration += " line-through";
            }
            if (textDecoration.Length > 0) {
                attributes.Add(HtmlTextWriterStyle.TextDecoration, textDecoration);
            }
            else {
                if (!DoNotRenderDefaults) {
                    attributes.Add(HtmlTextWriterStyle.TextDecoration, "none");
                }
            }
            // Removing the border with an inline style if the class name was set
            if (_owner.IsSet(PROP_CSSCLASS)) {
                attributes.Add(HtmlTextWriterStyle.BorderStyle, "none");
            }
        }
    }
}
