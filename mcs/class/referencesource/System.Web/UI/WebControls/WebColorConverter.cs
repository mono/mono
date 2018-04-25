//------------------------------------------------------------------------------
// <copyright file="WebColorConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;    
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Text;
    using System.Web.Util;
    using System.Web.UI;
    using System.Globalization;

    /// <devdoc>
    /// </devdoc>
    public class WebColorConverter : ColorConverter {

        private static Hashtable htmlSysColorTable;


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value is string) {
                string colorText = ((string)value).Trim();
                Color c = Color.Empty;

                // empty color
                if (String.IsNullOrEmpty(colorText))
                    return c;

                // #RRGGBB notation is handled by ColorConverter
                if (colorText[0] == '#') {
                    return base.ConvertFrom(context, culture, value);
                }

                // special case. HTML requires LightGrey, but System.Drawing.KnownColor has LightGray
                if (StringUtil.EqualsIgnoreCase(colorText, "LightGrey")) {
                    return Color.LightGray;
                }

                // System color
                if (htmlSysColorTable == null) {
                    InitializeHTMLSysColorTable();
                }
                object o = htmlSysColorTable[colorText];
                if (o != null) {
                    return (Color)o;
                }
            }

            // ColorConverter handles all named and KnownColors
            return base.ConvertFrom(context, culture, value);
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if (destinationType == null) {
                throw new ArgumentNullException("destinationType");
            }

            if (destinationType == typeof(string)) {
                if (value != null) {
                    Color c = (Color)value;

                    if (c == Color.Empty) {
                        return String.Empty;
                    }

                    /*
                    if (c.IsKnownColor) {
                        // Handle Web system colors and any 'special' named colors

                        string colorString = null;
                        switch (c.ToKnownColor()) {
                            case KnownColor.ActiveBorder: colorString = "ActiveBorder"; break;
                            case KnownColor.ActiveCaption: colorString = "ActiveCaption"; break;
                            case KnownColor.AppWorkspace: colorString = "AppWorkspace"; break;
                            case KnownColor.Desktop: colorString = "Background"; break;
                            case KnownColor.Control: colorString = "ButtonFace"; break;
                            case KnownColor.ControlLight: colorString = "ButtonHighlight"; break;
                            case KnownColor.ControlDark: colorString = "ButtonShadow"; break;
                            case KnownColor.ControlText: colorString = "ButtonText"; break;
                            case KnownColor.ActiveCaptionText: colorString = "CaptionText"; break;
                            case KnownColor.GrayText: colorString = "GrayText"; break;
                            case KnownColor.HotTrack:
                            case KnownColor.Highlight: colorString = "Highlight"; break;
                            case KnownColor.HighlightText: colorString = "HighlightText"; break;
                            case KnownColor.InactiveBorder: colorString = "InactiveBorder"; break;
                            case KnownColor.InactiveCaption: colorString = "InactiveCaption"; break;
                            case KnownColor.InactiveCaptionText: colorString = "InactiveCaptionText"; break;
                            case KnownColor.Info: colorString = "InfoBackground"; break;
                            case KnownColor.InfoText: colorString = "InfoText"; break;
                            case KnownColor.Menu: colorString = "Menu"; break;
                            case KnownColor.MenuText: colorString = "MenuText"; break;
                            case KnownColor.ScrollBar: colorString = "Scrollbar"; break;
                            case KnownColor.ControlDarkDark: colorString = "ThreeDDarkShadow"; break;
                            case KnownColor.ControlLightLight: colorString = "ButtonHighlight"; break;
                            case KnownColor.Window: colorString = "Window"; break;
                            case KnownColor.WindowFrame: colorString = "WindowFrame"; break;
                            case KnownColor.WindowText: colorString = "WindowText"; break;
                            
                            case KnownColor.LightGray: colorString = "LightGrey"; break;
                        }

                        if (colorString != null) {
                            return colorString;
                        }
                    }
                    */

                    if (c.IsKnownColor == false) {
                        // in the Web scenario, colors should be formatted in #RRGGBB notation
                        StringBuilder sb = new StringBuilder("#", 7);
                        sb.Append((c.R).ToString("X2", CultureInfo.InvariantCulture));
                        sb.Append((c.G).ToString("X2", CultureInfo.InvariantCulture));
                        sb.Append((c.B).ToString("X2", CultureInfo.InvariantCulture));
                        return sb.ToString();
                    }
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        private static void InitializeHTMLSysColorTable() {
            Hashtable t = new Hashtable(StringComparer.OrdinalIgnoreCase);
            t["activeborder"] = Color.FromKnownColor(KnownColor.ActiveBorder);
            t["activecaption"] = Color.FromKnownColor(KnownColor.ActiveCaption);
            t["appworkspace"] = Color.FromKnownColor(KnownColor.AppWorkspace);
            t["background"] = Color.FromKnownColor(KnownColor.Desktop);
            t["buttonface"] = Color.FromKnownColor(KnownColor.Control);
            t["buttonhighlight"] = Color.FromKnownColor(KnownColor.ControlLightLight);
            t["buttonshadow"] = Color.FromKnownColor(KnownColor.ControlDark);
            t["buttontext"] = Color.FromKnownColor(KnownColor.ControlText);
            t["captiontext"] = Color.FromKnownColor(KnownColor.ActiveCaptionText);
            t["graytext"] = Color.FromKnownColor(KnownColor.GrayText);
            t["highlight"] = Color.FromKnownColor(KnownColor.Highlight);
            t["highlighttext"] = Color.FromKnownColor(KnownColor.HighlightText);
            t["inactiveborder"] = Color.FromKnownColor(KnownColor.InactiveBorder);
            t["inactivecaption"] = Color.FromKnownColor(KnownColor.InactiveCaption);
            t["inactivecaptiontext"] = Color.FromKnownColor(KnownColor.InactiveCaptionText);
            t["infobackground"] = Color.FromKnownColor(KnownColor.Info);
            t["infotext"] = Color.FromKnownColor(KnownColor.InfoText);
            t["menu"] = Color.FromKnownColor(KnownColor.Menu);
            t["menutext"] = Color.FromKnownColor(KnownColor.MenuText);
            t["scrollbar"] = Color.FromKnownColor(KnownColor.ScrollBar);
            t["threeddarkshadow"] = Color.FromKnownColor(KnownColor.ControlDarkDark);
            t["threedface"] = Color.FromKnownColor(KnownColor.Control);
            t["threedhighlight"] = Color.FromKnownColor(KnownColor.ControlLight);
            t["threedlightshadow"] = Color.FromKnownColor(KnownColor.ControlLightLight);
            t["window"] = Color.FromKnownColor(KnownColor.Window);
            t["windowframe"] = Color.FromKnownColor(KnownColor.WindowFrame);
            t["windowtext"] = Color.FromKnownColor(KnownColor.WindowText);
            htmlSysColorTable = t;
        }
    }
}

